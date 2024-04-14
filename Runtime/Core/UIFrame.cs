using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Feif.Extensions;

#if USING_UNITASK
using GameObjectTask = Cysharp.Threading.Tasks.UniTask<UnityEngine.GameObject>;
using UIBaseTask = Cysharp.Threading.Tasks.UniTask<Feif.UIFramework.UIBase>;
using Task = Cysharp.Threading.Tasks.UniTask;
using Cysharp.Threading.Tasks;
#else
using GameObjectTask = System.Threading.Tasks.Task<UnityEngine.GameObject>;
using UIBaseTask = System.Threading.Tasks.Task<Feif.UIFramework.UIBase>;
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
#endif

namespace Feif.UIFramework
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class UIFrame : MonoBehaviour
    {
        private static readonly Dictionary<Type, GameObject> instances = new Dictionary<Type, GameObject>();
        private static readonly Stack<(Type type, UIData data)> panelStack = new Stack<(Type, UIData)>();
        private static readonly Dictionary<UILayer, RectTransform> uiLayers = new Dictionary<UILayer, RectTransform>();
        private static HashSet<UITimer> timers = new HashSet<UITimer>();
        private static HashSet<UITimer> timerRemoveSet = new HashSet<UITimer>();
        private static RectTransform layerTransform;

        [SerializeField] private RectTransform layers;
        [SerializeField] private Canvas canvas;

        /// <summary>
        /// UI画布
        /// </summary>
        public static Canvas Canvas { get; private set; }

        /// <summary>
        /// UI相机
        /// </summary>
        public static Camera Camera { get; private set; }

        /// <summary>
        /// 当加载UI超过这个时间（单位：秒）时，检测为卡住
        /// </summary>
        public static float StuckTime = 1;

        /// <summary>
        /// 当前显示的Panel
        /// </summary>
        public static UIBase CurrentPanel
        {
            get
            {
                if (panelStack.Count <= 0) return null;

                if (panelStack.Peek().type == null) return null;

                if (instances.TryGetValue(panelStack.Peek().type, out var instance))
                {
                    return instance.GetComponent<UIBase>();
                }
                return null;
            }
        }

        private void Awake()
        {
            if (canvas == null) throw new Exception("UIFrame初始化失败，请设置Canvas");
            if (canvas.worldCamera == null) throw new Exception("UIFrame初始化失败，请给Canvas设置worldCamera");
            if (layers == null) throw new Exception("UIFrame初始化失败，请设置layers");
            Canvas = canvas;
            Camera = canvas.worldCamera;
            layerTransform = layers;
            layerTransform.anchorMin = Vector2.zero;
            layerTransform.anchorMax = Vector2.one;
            layerTransform.offsetMin = Vector2.zero;
            layerTransform.offsetMax = Vector2.zero;
            DontDestroyOnLoad(gameObject);
            AutoBindUITimer.Enable();
            AutoBindUGUIButtonEvent.Enable();
        }

        #region 事件
        /// <summary>
        /// 卡住开始时触发的事件
        /// </summary>
        public static event Action OnStuckStart;

        /// <summary>
        /// 卡住结束时触发的事件
        /// </summary>
        public static event Action OnStuckEnd;

        /// <summary>
        /// 资源请求
        /// </summary>
        public static event Func<Type, GameObjectTask> OnAssetRequest;

        /// <summary>
        /// 资源释放
        /// </summary>
        public static event Action<Type> OnAssetRelease;

        /// <summary>
        /// UI创建时调用
        /// </summary>
        public static event Action<UIBase> OnCreate;

        /// <summary>
        /// UI刷新时调用
        /// </summary>
        public static event Action<UIBase> OnRefresh;

        /// <summary>
        /// UI绑定事件时调用
        /// </summary>
        public static event Action<UIBase> OnBind;

        /// <summary>
        /// UI解绑事件时调用
        /// </summary>
        public static event Action<UIBase> OnUnbind;

        /// <summary>
        /// UI显示时调用
        /// </summary>
        public static event Action<UIBase> OnShow;

        /// <summary>
        /// UI隐藏时调用
        /// </summary>
        public static event Action<UIBase> OnHide;

        /// <summary>
        /// UI销毁时调用
        /// </summary>
        public static event Action<UIBase> OnDied;
        #endregion

        #region 显示
        /// <summary>
        /// 显示UI
        /// </summary>
        public static UIBaseTask Show(UIBase ui, UIData data = null)
        {
            if (GetLayer(ui) != null && ui.Parent != null) throw new Exception("子UI不能使用UILayer属性");

            return ShowAsync(ui, data);
        }

        /// <summary>
        /// 显示Panel或Window
        /// </summary>
#if USING_UNITASK
        public static UniTask<T> Show<T>(UIData data = null) where T : UIBase
#else
        public static Task<T> Show<T>(UIData data = null) where T : UIBase
#endif
        {
            return ShowAsync<T>(data);
        }

        /// <summary>
        /// 显示Panel或Window
        /// </summary>
        public static UIBaseTask Show(Type type, UIData data = null)
        {
            if (GetLayer(type) == null) throw new Exception("请使用[UILayer]子类标记类，显示子UI请使用Show(UIBase ui)");

            return ShowAsync(type, data);
        }
        #endregion

        #region 隐藏
        /// <summary>
        /// 隐藏Panel
        /// </summary>
        public static Task Hide(bool forceDestroy = false)
        {
            return HideAsync(forceDestroy);
        }

        /// <summary>
        /// 隐藏Panel或Window
        /// </summary>
        public static Task Hide<T>(bool forceDestroy = false)
        {
            return Hide(typeof(T), forceDestroy);
        }

        /// <summary>
        /// 隐藏Panel或Window
        /// </summary>
        public static Task Hide(Type type, bool forceDestroy = false)
        {
            if (GetLayer(type) is PanelLayer)
            {
                if (CurrentPanel != null && CurrentPanel.GetType() == type) return Hide();

                throw new Exception(type.ToString() + "不是当前正在显示的Panel，请使用UIFrame.Hide()来隐藏当前Panel");
            }
            else if (GetLayer(type) != null)
            {
                if (instances.TryGetValue(type, out var instance))
                {
                    var uibase = instance.GetComponent<UIBase>();
                    var uibases = uibase.BreadthTraversal().ToArray();
                    DoUnbind(uibases);
                    DoHide(uibases);
                    instance.SetActive(false);
                    if (uibase.AutoDestroy || forceDestroy) ReleaseInstance(type);
                }
                return Task.CompletedTask;
            }
            throw new Exception("隐藏UI失败，请使用[UILayer]子类标记类，隐藏子UI请使用UIFrame.Hide(UIBase ui)");
        }

        /// <summary>
        /// 隐藏UI，forceDestroy对子UI无效。
        /// </summary>
        public static Task Hide(UIBase ui, bool forceDestroy = false)
        {
            if (GetLayer(ui) == null)
            {
                if (!ui.gameObject.activeSelf) return Task.CompletedTask;

                var uibases = ui.BreadthTraversal().ToArray();
                DoUnbind(uibases);
                DoHide(uibases);
                ui.gameObject.SetActive(false);
                return Task.CompletedTask;
            }
            return Hide(ui.GetType(), forceDestroy);
        }
        #endregion

        #region 获得
        /// <summary>
        /// 获得已经实例化的UI
        /// </summary>
        public static UIBase Get(Type type)
        {
            if (instances.TryGetValue(type, out var instance))
            {
                return instance.GetComponent<UIBase>();
            }
            return null;
        }

        /// <summary>
        /// 获得已经实例化的UI
        /// </summary>
        public static UIBase Get<T>()
        {
            return Get(typeof(T));
        }

        /// <summary>
        /// 获得已经实例化的UI
        /// </summary>
        public static bool TryGet<T>(out UIBase ui)
        {
            ui = Get<T>();
            return ui != null;
        }

        /// <summary>
        /// 获得已经实例化的UI
        /// </summary>
        public static bool TryGet(Type type, out UIBase ui)
        {
            ui = Get(type);
            return ui != null;
        }

        /// <summary>
        /// 获得所有已经实例化的UI
        /// </summary>
        public static IEnumerable<UIBase> GetAll(Func<Type, bool> predicate = null)
        {
            foreach (var item in instances)
            {
                if (predicate != null && !predicate.Invoke(item.Key)) continue;

                yield return item.Value.GetComponent<UIBase>();
            }
        }

        /// <summary>
        /// 获得UILayer
        /// </summary>
        public static UILayer GetLayer(Type type)
        {
            if (type == null) return null;

            var layer = type.GetCustomAttributes(typeof(UILayer), true).FirstOrDefault() as UILayer;
            return layer;
        }

        /// <summary>
        /// 获得UILayer
        /// </summary>
        public static UILayer GetLayer(UIBase ui)
        {
            return GetLayer(ui.GetType());
        }

        /// <summary>
        /// 获得UI层RectTransform
        /// </summary>
        public static RectTransform GetLayerTransform(Type type)
        {
            var layer = GetLayer(type);
            uiLayers.TryGetValue(layer, out var result);
            return result;
        }
        #endregion

        #region 刷新
        /// <summary>
        /// 刷新UI。data为null时，将用之前的data刷新
        /// </summary>
        public static Task Refresh<T>(UIData data = null)
        {
            return Refresh(typeof(T), data);
        }

        /// <summary>
        /// 刷新UI。data为null时将用之前的data刷新
        /// </summary>
        public static Task Refresh(Type type, UIData data = null)
        {
            if (type != null && instances.TryGetValue(type, out var instance))
            {
                return Refresh(instance.GetComponent<UIBase>(), data);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 刷新UI。data为null时将用之前的data刷新
        /// </summary>
        public static Task Refresh(UIBase ui, UIData data = null)
        {
            if (!ui.gameObject.activeInHierarchy) return Task.CompletedTask;

            var uibases = ui.BreadthTraversal().ToArray();
            if (data != null) TrySetData(ui, data);
            if (panelStack.Count > 0 && GetLayer(ui) is PanelLayer)
            {
                var (type, _) = panelStack.Pop();
                panelStack.Push((type, data));
            }
            return DoRefresh(uibases);
        }

        /// <summary>
        /// 刷新所有UI
        /// </summary>
        public static async Task RefreshAll(Func<Type, bool> predicate = null)
        {
            foreach (var item in instances)
            {
                if (predicate != null && !predicate.Invoke(item.Key)) continue;

                await Refresh(item.Value.GetComponent<UIBase>());
            }
        }
        #endregion

        /// <summary>
        /// 创建UI GameObject
        /// </summary>
        public static GameObjectTask Instantiate(GameObject prefab, Transform parent = null, UIData data = null)
        {
            return InstantiateAsync(prefab, parent, data);
        }

        /// <summary>
        /// 销毁UI GameObject
        /// </summary>
        public static void Destroy(GameObject instance)
        {
            var uibases = instance.transform.BreadthTraversal()
                .Where(item => item.GetComponent<UIBase>() != null)
                .Select(item => item.GetComponent<UIBase>())
                .ToArray();
            var parentUI = GetParent(uibases.FirstOrDefault());
            foreach (var item in uibases)
            {
                if (parentUI == null) break;

                if (GetParent(item) != parentUI) break;

                parentUI.Children.Remove(item);
            }
            foreach (var item in uibases)
            {
                try
                {
                    OnDied?.Invoke(item);
                    item.InnerOnDied();
                    item.CancelAllTimer();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            GameObject.Destroy(instance);
        }

        /// <summary>
        /// 立即销毁UI GameObject
        /// </summary>
        public static void DestroyImmediate(GameObject instance)
        {
            var uibases = instance.transform.BreadthTraversal()
               .Where(item => item.GetComponent<UIBase>() != null)
               .Select(item => item.GetComponent<UIBase>())
               .ToArray();
            var parentUI = GetParent(uibases.FirstOrDefault());
            foreach (var item in uibases)
            {
                if (parentUI == null) break;

                if (GetParent(item) != parentUI) break;

                parentUI.Children.Remove(item);
            }
            foreach (var item in uibases)
            {
                try
                {
                    OnDied?.Invoke(item);
                    item.InnerOnDied();
                    item.CancelAllTimer();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            GameObject.DestroyImmediate(instance);
        }

        /// <summary>
        /// 强制释放已经关闭的UI，即使UI的AutoDestroy为false，仍然释放该资源
        /// </summary>
        public static void Release()
        {
            var keys = new List<Type>();
            foreach (var item in instances)
            {
                if (item.Value != null && !item.Value.activeInHierarchy)
                {
                    UIFrame.Destroy(item.Value);
                    OnAssetRelease?.Invoke(item.Key);
                    keys.Add(item.Key);
                }
            }
            foreach (var item in keys)
            {
                instances.Remove(item);
            }
        }

        /// <summary>
        /// 创建定时器
        /// </summary>
        /// <param name="delay">延迟多少秒后执行callback</param>
        /// <param name="callback">延迟执行的方法</param>
        /// <param name="isLoop">是否是循环定时器</param>
        public static UITimer CreateTimer(float delay, Action callback, bool isLoop = false)
        {
            if (delay <= 0) throw new Exception("delay必须大于0");
            var timer = new UITimer(delay, callback, isLoop);
            timers.Add(timer);
            return timer;
        }

        private static async GameObjectTask RequestInstance(Type type, UIData data)
        {
            if (type == null) throw new NullReferenceException();

            if (instances.TryGetValue(type, out var instance))
            {
                TrySetData(instance.GetComponent<UIBase>(), data);
                return instance;
            }
            GameObject refInstance = null;
            if (OnAssetRelease != null)
            {
                refInstance = await OnAssetRequest.Invoke(type);
            }
            var uibase = refInstance.GetComponent<UIBase>();
            if (uibase == null) throw new Exception("预制体没有挂载继承自UIBase的脚本");
            var parent = GetOrCreateLayerTransform(type);
            instance = await UIFrame.Instantiate(refInstance, parent, data);
            instances[type] = instance;
            return instance;
        }

        private static void ReleaseInstance(Type type)
        {
            if (type == null) return;

            if (instances.TryGetValue(type, out var instance))
            {
                var root = instance.GetComponent<UIBase>();
                UIFrame.Destroy(instance);
                OnAssetRelease?.Invoke(type);
                instances.Remove(type);
            }
        }

        private static async Task DoRefresh(IList<UIBase> uibases)
        {
            if (uibases == null) return;

            for (int i = 0; i < uibases.Count; ++i)
            {
                if (uibases[i] == null) continue;

                if (i == 0 || uibases[i].gameObject.activeSelf)
                {
                    try
                    {
                        OnRefresh?.Invoke(uibases[i]);
                        await uibases[i].InnerOnRefresh();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        private static void DoBind(IList<UIBase> uibases)
        {
            if (uibases == null) return;

            for (int i = 0; i < uibases.Count; ++i)
            {
                if (uibases[i] == null) continue;

                if (i == 0 || uibases[i].gameObject.activeSelf)
                {
                    try
                    {
                        OnBind?.Invoke(uibases[i]);
                        uibases[i].InnerOnBind();

                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        private static void DoUnbind(IList<UIBase> uibases)
        {
            if (uibases == null) return;

            for (int i = uibases.Count - 1; i >= 0; --i)
            {
                if (uibases[i] == null) continue;

                if (i == 0 || uibases[i].gameObject.activeSelf)
                {
                    try
                    {
                        OnUnbind?.Invoke(uibases[i]);
                        uibases[i].InnerOnUnbind();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        private static void DoShow(IList<UIBase> uibases)
        {
            if (uibases == null) return;

            for (int i = 0; i < uibases.Count; ++i)
            {
                if (uibases[i] == null) continue;

                if (i == 0 || uibases[i].gameObject.activeSelf)
                {
                    try
                    {
                        OnShow?.Invoke(uibases[i]);
                        uibases[i].InnerOnShow();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        private static void DoHide(IList<UIBase> uibases)
        {
            if (uibases == null) return;

            for (int i = uibases.Count - 1; i >= 0; --i)
            {
                if (uibases[i] == null) continue;

                if (i == 0 || uibases[i].gameObject.activeSelf)
                {
                    try
                    {
                        OnHide?.Invoke(uibases[i]);
                        uibases[i].InnerOnHide();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        private static async GameObjectTask InstantiateAsync(GameObject prefab, Transform parent, UIData data)
        {
            bool refActiveSelf = prefab.activeSelf;
            prefab.SetActive(false);
            var instance = GameObject.Instantiate(prefab, parent);
            prefab.SetActive(refActiveSelf);
            var uibase = instance.GetComponent<UIBase>();
            var uibases = instance.transform.BreadthTraversal()
                .Where(item => item.GetComponent<UIBase>() != null)
                .Select(item => item.GetComponent<UIBase>())
                .ToArray();
            TrySetData(instance.GetComponent<UIBase>(), data);
            foreach (var item in uibases)
            {
                item.Children.Clear();
            }
            foreach (var item in uibases)
            {
                var parentUI = GetParent(item);
                if (parentUI == null) continue;
                parentUI.Children.Add(item);
                item.Parent = parentUI;
            }
            foreach (var item in uibases)
            {
                try
                {
                    OnCreate?.Invoke(item);
                    await item.InnerOnCreate();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            if (GetLayer(uibase) == null)
            {
                await DoRefresh(uibases);
                instance.SetActive(true);
                DoBind(uibases);
                DoShow(uibases);
            }
            return instance;
        }

        private static async UIBaseTask ShowAsync(UIBase ui, UIData data = null)
        {
            try
            {
                if (GetLayer(ui) == null)
                {
                    if (ui.gameObject.activeSelf) return ui;

                    TrySetData(ui, data);
                    var timeout = new CancellationTokenSource();
                    bool isStuck = false;
                    Task.Delay(TimeSpan.FromSeconds(StuckTime)).GetAwaiter().OnCompleted(() =>
                    {
                        if (timeout.IsCancellationRequested) return;

                        OnStuckStart?.Invoke();
                        isStuck = true;
                    });
                    var parentUIBases = ui.Parent.BreadthTraversal().ToArray();
                    DoUnbind(parentUIBases);
                    var uibases = ui.BreadthTraversal().ToArray();
                    await DoRefresh(uibases);
                    ui.gameObject.SetActive(true);
                    if (ui.Parent != null)
                    {
                        DoBind(parentUIBases);
                    }
                    else
                    {
                        DoBind(uibases);
                    }
                    DoShow(uibases);
                    timeout.Cancel();

                    if (isStuck) OnStuckEnd?.Invoke();

                    return ui;
                }
                return await Show(ui.GetType(), data);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

#if USING_UNITASK
        private static async UniTask<T> ShowAsync<T>(UIData data = null) where T : UIBase
#else
        private static async Task<T> ShowAsync<T>(UIData data = null) where T : UIBase
#endif
        {
            var result = await Show(typeof(T), data);
            return result as T;
        }

        private static async UIBaseTask ShowAsync(Type type, UIData data = null)
        {
            try
            {
                var timeout = new CancellationTokenSource();
                UIBase result = null;
                bool isStuck = false;
                Task.Delay(TimeSpan.FromSeconds(StuckTime)).GetAwaiter().OnCompleted(() =>
                {
                    if (timeout.IsCancellationRequested) return;
                    OnStuckStart?.Invoke();
                    isStuck = true;
                });
                if (GetLayer(type) is PanelLayer)
                {
                    if (CurrentPanel != null && type == CurrentPanel.GetType()) return CurrentPanel;

                    UIBase[] currentUIBases = null;
                    if (CurrentPanel != null)
                    {
                        currentUIBases = CurrentPanel.BreadthTraversal().ToArray();
                        DoUnbind(currentUIBases);
                    }
                    var instance = await RequestInstance(type, data);
                    var uibases = instance.GetComponent<UIBase>().BreadthTraversal().ToArray();
                    if (data != null && CurrentPanel != null)
                    {
                        data.Sender = CurrentPanel.GetType();
                    }
                    await DoRefresh(uibases);
                    if (CurrentPanel != null)
                    {
                        DoHide(currentUIBases);
                        CurrentPanel.gameObject.SetActive(false);
                        if (CurrentPanel.AutoDestroy) ReleaseInstance(CurrentPanel.GetType());
                    }
                    instance.SetActive(true);
                    panelStack.Push((type, data));
                    DoBind(uibases);
                    DoShow(uibases);
                    result = instance.GetComponent<UIBase>();
                }
                else if (GetLayer(type) != null)
                {
                    var instance = await RequestInstance(type, data);
                    var uibases = instance.GetComponent<UIBase>().BreadthTraversal().ToArray();

                    if (data != null && CurrentPanel != null)
                    {
                        data.Sender = CurrentPanel.GetType();
                    }
                    await DoRefresh(uibases);
                    instance.SetActive(true);
                    instance.transform.SetAsLastSibling();
                    DoBind(uibases);
                    DoShow(uibases);
                    result = instance.GetComponent<UIBase>();
                }
                timeout.Cancel();
                if (isStuck) OnStuckEnd?.Invoke();
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        private static async Task HideAsync(bool forceDestroy)
        {
            try
            {
                var timeout = new CancellationTokenSource();

                bool isStuck = false;
                Task.Delay(TimeSpan.FromSeconds(StuckTime)).GetAwaiter().OnCompleted(() =>
                {
                    if (timeout.IsCancellationRequested) return;
                    OnStuckStart?.Invoke();
                    isStuck = true;
                });

                if (CurrentPanel == null)
                {
                    timeout.Cancel();
                    return;
                }

                var currentPanel = CurrentPanel;
                var currentUIBases = currentPanel.BreadthTraversal().ToArray();
                panelStack.Pop();
                DoUnbind(currentUIBases);
                if (panelStack.Count > 0)
                {
                    var data = panelStack.Peek().data;
                    if (data != null && currentPanel != null)
                    {
                        data.Sender = currentPanel.GetType();
                    }
                    var instance = await RequestInstance(panelStack.Peek().type, data);
                    var uibases = instance.GetComponent<UIBase>().BreadthTraversal().ToArray();
                    await DoRefresh(uibases);
                    currentPanel.gameObject.SetActive(false);
                    DoHide(currentUIBases);
                    if (currentPanel.AutoDestroy || forceDestroy) ReleaseInstance(currentPanel.GetType());
                    instance.SetActive(true);
                    instance.transform.SetAsLastSibling();
                    DoBind(uibases);
                    DoShow(uibases);
                }
                else
                {
                    currentPanel.gameObject.SetActive(false);
                    DoHide(currentUIBases);
                    if (currentPanel.AutoDestroy || forceDestroy) ReleaseInstance(currentPanel.GetType());
                }
                timeout.Cancel();
                if (isStuck) OnStuckEnd?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static bool TrySetData(UIBase ui, UIData data)
        {
            if (ui == null) return false;
            var property = ui.GetType().GetProperty("Data", BindingFlags.Public | BindingFlags.Instance);
            if (property == null) return false;
            property.SetValue(ui, data);
            return true;
        }

        private static UIBase GetParent(UIBase ui)
        {
            if (ui == null) return null;

            var parent = ui.transform.parent;
            while (parent != null)
            {
                var uibase = parent.GetComponent<UIBase>();
                if (uibase != null) return uibase;
                parent = parent.parent;
            }
            return null;
        }

        private static RectTransform GetOrCreateLayerTransform(Type type)
        {
            var layer = GetLayer(type);
            if (!uiLayers.TryGetValue(layer, out var result))
            {
                var layerObject = new GameObject(layer.GetName());
                layerObject.transform.SetParent(layerTransform);
                result = layerObject.AddComponent<RectTransform>();
                result.anchorMin = Vector2.zero;
                result.anchorMax = Vector2.one;
                result.offsetMin = Vector2.zero;
                result.offsetMax = Vector2.zero;
                result.localScale = Vector3.one;
                uiLayers[layer] = result;
                int index = 0;
                foreach (var item in uiLayers.OrderBy(i => i.Key.GetOrder()))
                {
                    item.Value.SetSiblingIndex(++index);
                }
            }
            return result;
        }

        private void Update()
        {
            foreach (var item in timers)
            {
                item.Update();
                if (item.IsCancel) timerRemoveSet.Add(item);
            }
            foreach (var item in timerRemoveSet)
            {
                timers.Remove(item);
            }
        }

        private void OnDestroy()
        {
            AutoBindUITimer.Disable();
            AutoBindUGUIButtonEvent.Disable();
        }
    }
}
