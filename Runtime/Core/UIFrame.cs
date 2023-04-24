using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Feif.Extensions;

namespace Feif.UIFramework
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class UIFrame : MonoBehaviour
    {
        private static readonly Dictionary<Type, GameObject> instances = new Dictionary<Type, GameObject>();
        private static readonly Stack<(Type type, UIData data)> panelStack = new Stack<(Type, UIData)>();

        [SerializeField] private RectTransform panelLayer;
        [SerializeField] private RectTransform windowLayer;
        [SerializeField] private Canvas canvas;

        /// <summary>
        /// Panel层级
        /// </summary>
        public static RectTransform PanelLayer { get; private set; }

        /// <summary>
        /// Window层级
        /// </summary>
        public static RectTransform WindowLayer { get; private set; }

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
            Canvas = canvas;
            Camera = canvas.worldCamera;
            PanelLayer = panelLayer;
            WindowLayer = windowLayer;
            DontDestroyOnLoad(gameObject);
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
        public static event Func<Type, Task<GameObject>> OnAssetRequest;

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
        public static Task Show(UIBase ui, UIData data = null)
        {
            return ShowAsync(ui, data);
        }

        /// <summary>
        /// 显示Panel或Window
        /// </summary>
        public static Task Show<T>(UIData data = null)
        {
            return Show(typeof(T), data);
        }

        /// <summary>
        /// 显示Panel或Window
        /// </summary>
        public static Task Show(Type type, UIData data = null)
        {
            if (!IsPanel(type) && !IsWindow(type)) throw new InvalidOperationException("显示Panel或Window失败，请使用[UIPanel]或[UIWindow]标记类");

            return ShowAsync(type, data);
        }

        #endregion

        #region 隐藏

        /// <summary>
        /// 隐藏Panel
        /// </summary>
        public static Task Hide()
        {
            return HideAsync();
        }

        /// <summary>
        /// 隐藏Panel或Window
        /// </summary>
        public static Task Hide<T>()
        {
            return Hide(typeof(T));
        }

        /// <summary>
        /// 隐藏Panel或Window
        /// </summary>
        public static Task Hide(Type type)
        {
            if (IsWindow(type))
            {
                if (instances.TryGetValue(type, out var instance))
                {
                    var uibase = instance.GetComponent<UIBase>();
                    var uibases = uibase.BreadthTraversal().ToArray();
                    DoUnbind(uibases);
                    DoHide(uibases);
                    instance.SetActive(false);
                    ReleaseInstance(type);
                }
                return Task.CompletedTask;
            }
            if (IsPanel(type))
            {
                if (CurrentPanel != null && CurrentPanel.GetType() == type) return Hide();

                throw new InvalidOperationException(type.ToString() + "不是当前正在显示的Panel，请使用UIFrame.Hide()来隐藏当前Panel");
            }
            throw new InvalidOperationException("隐藏Panel或Window失败，请使用[UIPanel]或[UIWindow]标记类");
        }

        /// <summary>
        /// 隐藏UI
        /// </summary>
        public static Task Hide(UIBase ui)
        {
            if (!IsPanel(ui) && !IsWindow(ui))
            {
                if (!ui.gameObject.activeSelf) return Task.CompletedTask;

                var uibases = ui.BreadthTraversal().ToArray();
                DoUnbind(uibases);
                DoHide(uibases);
                ui.gameObject.SetActive(false);
                return Task.CompletedTask;
            }
            return Hide(ui.GetType());
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
            if (panelStack.Count > 0 && IsPanel(ui))
            {
                var (type, _) = panelStack.Pop();
                panelStack.Push((type, data));
            }
            return DoRefresh(uibases);
        }
        #endregion

        /// <summary>
        /// 创建UI GameObject
        /// </summary>
        public static Task<GameObject> Instantiate(GameObject prefab, Transform parent = null, UIData data = null)
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
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            GameObject.DestroyImmediate(instance);
        }

        /// <summary>
        /// 判断UI是否是Panel
        /// </summary>
        public static bool IsPanel(UIBase ui)
        {
            return IsPanel(ui.GetType());
        }

        /// <summary>
        /// 判断UI是否是Window
        /// </summary>
        public static bool IsWindow(UIBase ui)
        {
            return IsWindow(ui.GetType());
        }

        /// <summary>
        /// 判断UI是否是Panel
        /// </summary>
        public static bool IsPanel(Type type)
        {
            if (Attribute.IsDefined(type, typeof(UIPanelAttribute)) && Attribute.IsDefined(type, typeof(UIWindowAttribute)))
            {
                throw new InvalidOperationException("不能同时定义[UIPanel]属性和[UIWindow]属性");
            }
            return Attribute.IsDefined(type, typeof(UIPanelAttribute));
        }

        /// <summary>
        /// 判断UI是否是Window
        /// </summary>
        public static bool IsWindow(Type type)
        {
            if (Attribute.IsDefined(type, typeof(UIPanelAttribute)) && Attribute.IsDefined(type, typeof(UIWindowAttribute)))
            {
                throw new InvalidOperationException("不能同时定义[UIPanel]属性和[UIWindow]属性");
            }
            return Attribute.IsDefined(type, typeof(UIWindowAttribute));
        }

        /// <summary>
        /// 强制释放已经关闭的UI，即使UI的AutoDestroy为false，仍然释放该资源
        /// </summary>
        public static void Release()
        {
            foreach (var item in instances)
            {
                if (item.Value != null && !item.Value.activeInHierarchy)
                {
                    UIFrame.Destroy(item.Value);
                    OnAssetRelease?.Invoke(item.Key);
                    instances.Remove(item.Key);
                }
            }
        }

        private static async Task<GameObject> RequestInstance(Type type, UIData data)
        {
            if (type == null) throw new NullReferenceException();

            if (instances.TryGetValue(type, out var instance)) return instance;

            var refInstance = await OnAssetRequest?.Invoke(type);
            var parent = IsPanel(refInstance.GetComponent<UIBase>()) ? PanelLayer : WindowLayer;
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

                if (!root.AutoDestroy) return;

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

        private static async Task<GameObject> InstantiateAsync(GameObject prefab, Transform parent, UIData data)
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
            if (!IsPanel(uibase) && !IsWindow(uibase))
            {
                await DoRefresh(uibases);
                instance.SetActive(true);
                DoBind(uibases);
                DoShow(uibases);
            }
            return instance;
        }

        private static async Task ShowAsync(UIBase ui, UIData data = null)
        {
            try
            {
                if (!IsPanel(ui) && !IsWindow(ui))
                {
                    if (ui.gameObject.activeSelf) return;

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

                    return;
                }
                await Show(ui.GetType(), data);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static async Task ShowAsync(Type type, UIData data = null)
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
                if (IsPanel(type))
                {
                    if (CurrentPanel != null && type == CurrentPanel.GetType()) return;
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
                        ReleaseInstance(CurrentPanel.GetType());
                    }
                    instance.SetActive(true);
                    panelStack.Push((type, data));
                    DoBind(uibases);
                    DoShow(uibases);
                }
                if (IsWindow(type))
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
                }
                timeout.Cancel();
                if (isStuck) OnStuckEnd?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static async Task HideAsync()
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

                if (CurrentPanel == null) return;

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
                    ReleaseInstance(currentPanel.GetType());
                    instance.SetActive(true);
                    instance.transform.SetAsLastSibling();
                    DoBind(uibases);
                    DoShow(uibases);
                }
                else
                {
                    currentPanel.gameObject.SetActive(false);
                    DoHide(currentUIBases);
                    ReleaseInstance(currentPanel.GetType());
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
    }
}