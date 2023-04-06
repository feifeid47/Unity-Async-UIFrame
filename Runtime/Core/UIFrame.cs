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
        public static Action OnStuckStart;

        /// <summary>
        /// 卡住结束时触发的事件
        /// </summary>
        public static Action OnStuckEnd;

        /// <summary>
        /// 资源请求
        /// </summary>
        public static Func<Type, Task<GameObject>> OnAssetRequest;

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
        public static Task Show<T>(UIData data = null)
        {
            return Show(typeof(T), data);
        }

        /// <summary>
        /// 显示UI
        /// </summary>
        public static Task Show(UIBase ui, UIData data = null)
        {
            return ShowAsync(ui, data);
        }

        /// <summary>
        /// 显示UI
        /// </summary>
        public static Task Show(Type type, UIData data = null)
        {
            return ShowAsync(type, data);
        }

        #endregion

        #region 隐藏

        /// <summary>
        /// 隐藏UI
        /// </summary>
        public static Task Hide()
        {
            return HideAsync();
        }

        /// <summary>
        /// 隐藏UI
        /// </summary>
        public static Task Hide<T>()
        {
            return Hide(typeof(T));
        }

        /// <summary>
        /// 隐藏UI
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
                if (CurrentPanel != null && CurrentPanel.GetType() == type)
                {
                    return Hide();
                }
                throw new InvalidOperationException(type.ToString() + "不是当前正在显示的Panel，请使用UIFrame.Hide()来隐藏当前Panel");
            }
            throw new InvalidOperationException("请使用UIPanel或UIWindow标记类");
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
        /// 刷新UI
        /// </summary>
        public static Task Refresh<T>()
        {
            return Refresh(typeof(T));
        }

        /// <summary>
        /// 刷新UI
        /// </summary>
        public static Task Refresh(Type type)
        {
            if (type != null && instances.TryGetValue(type, out var instance))
            {
                return Refresh(instance.GetComponent<UIBase>());
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 刷新UI
        /// </summary>
        public static Task Refresh(UIBase ui)
        {
            var uibases = ui.BreadthTraversal().ToArray();
            return DoRefresh(uibases);
        }
        #endregion

        /// <summary>
        /// 创建UI GameObject
        /// </summary>
        public static Task<GameObject> Instantiate(GameObject prefab, Transform parent = null)
        {
            return InstantiateAsync(prefab, parent);
        }

        /// <summary>
        /// 为UI设置数据
        /// </summary>
        public static bool TrySetData(UIBase ui, UIData data)
        {
            if (ui == null) return false;
            var property = ui.GetType().GetProperty("Data", BindingFlags.Public | BindingFlags.Instance);
            if (property == null) return false;
            property.SetValue(ui, data);
            return true;
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
            foreach (var item in uibases)
            {
                var parentUI = item.GetComponentsInParent<UIBase>().FirstOrDefault(p => p != item);
                if (parentUI == null) continue;
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
            foreach (var item in uibases)
            {
                var parentUI = item.GetComponentsInParent<UIBase>().FirstOrDefault(p => p != item);
                if (parentUI == null) continue;
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
                throw new InvalidOperationException("不能同时定义UIPanel属性和UIWindow属性");

            return Attribute.IsDefined(type, typeof(UIPanelAttribute));
        }

        /// <summary>
        /// 判断UI是否是Window
        /// </summary>
        public static bool IsWindow(Type type)
        {
            if (Attribute.IsDefined(type, typeof(UIPanelAttribute)) && Attribute.IsDefined(type, typeof(UIWindowAttribute)))
                throw new InvalidOperationException("不能同时定义UIPanel属性和UIWindow属性");

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

        private static async Task<GameObject> RequestInstance(Type type)
        {
            if (type == null) throw new NullReferenceException();
            if (instances.TryGetValue(type, out var instance)) return instance;

            var refInstance = await OnAssetRequest?.Invoke(type);
            var parent = IsPanel(refInstance.GetComponent<UIBase>()) ? PanelLayer : WindowLayer;
            bool refActiveSelf = refInstance.activeSelf;

            refInstance.SetActive(false);
            instance = await UIFrame.Instantiate(refInstance, parent);
            refInstance.SetActive(refActiveSelf);
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

        private static async Task<GameObject> InstantiateAsync(GameObject prefab, Transform parent)
        {
            var instance = GameObject.Instantiate(prefab, parent);
            var uibases = instance.transform.BreadthTraversal()
                .Where(item => item.GetComponent<UIBase>() != null)
                .Select(item => item.GetComponent<UIBase>())
                .ToArray();
            foreach (var item in uibases)
            {
                var parentUI = item.GetComponentsInParent<UIBase>().FirstOrDefault(p => p != item);
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
            return instance;
        }

        private static async Task ShowAsync(UIBase ui, UIData data = null)
        {
            try
            {
                if (!IsPanel(ui) && !IsWindow(ui))
                {
                    if (ui.gameObject.activeSelf) return;

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
                    TrySetData(ui, data);
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

                    var currentUIBases = CurrentPanel.BreadthTraversal().ToArray();
                    DoUnbind(currentUIBases);
                    var instance = await RequestInstance(type);
                    var uibases = instance.GetComponent<UIBase>().BreadthTraversal().ToArray();
                    if (data != null && CurrentPanel != null)
                    {
                        data.Sender = CurrentPanel.GetType();
                    }
                    TrySetData(instance.GetComponent<UIBase>(), data);
                    await DoRefresh(uibases);
                    DoHide(currentUIBases);
                    if (CurrentPanel != null)
                    {
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
                    var instance = await RequestInstance(type);
                    var uibases = instance.GetComponent<UIBase>().BreadthTraversal().ToArray();
                    if (data != null && CurrentPanel != null)
                    {
                        data.Sender = CurrentPanel.GetType();
                    }
                    TrySetData(instance.GetComponent<UIBase>(), data);
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
                    var instance = await RequestInstance(panelStack.Peek().type);
                    var uibases = instance.GetComponent<UIBase>().BreadthTraversal().ToArray();
                    if (panelStack.Peek().data != null && currentPanel != null)
                    {
                        panelStack.Peek().data.Sender = currentPanel.GetType();
                    }
                    TrySetData(instance.GetComponent<UIBase>(), panelStack.Peek().data);
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
    }
}