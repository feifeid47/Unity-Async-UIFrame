using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Feif.UIFramework
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class UIFrame : MonoBehaviour
    {
        private static readonly Dictionary<string, GameObject> instances = new Dictionary<string, GameObject>();
        private static readonly Stack<(string name, UIProperties properties)> panelStack = new Stack<(string, UIProperties)>();

        [SerializeField] private RectTransform panelLayer;
        [SerializeField] private RectTransform windowLayer;
        [SerializeField] private Canvas canvas;

        public static RectTransform PanelLayer { get; private set; }
        public static RectTransform WindowLayer { get; private set; }
        public static Canvas Canvas { get; private set; }
        public static Camera Camera { get; private set; }

        /// <summary>当加载UI超过这个时间（单位：秒）时，检测为卡住</summary>
        public static float StuckTime = 1;
        /// <summary>卡住开始时触发的事件</summary>
        public static event Action<string> OnStuckStart;
        /// <summary>卡住结束时触发的事件</summary>
        public static event Action<string> OnStuckEnd;
        /// <summary>资源请求</summary>
        public static event Action<string, Action<GameObject>> OnAssetRequest;
        /// <summary>资源释放</summary>
        public static event Action<string> OnAssetRelease;
        /// <summary>当前显示的Panel</summary>
        public static GameObject CurrentPanel
        {
            get
            {
                if (panelStack.Count <= 0) return null;

                if (panelStack.Peek().name == null) return null;

                if (instances.TryGetValue(panelStack.Peek().name, out var instance))
                {
                    return instance;
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
        }

        private static async Task<GameObject> RequestAsset(string name, Transform parent, Action<GameObject> beforInit)
        {
            if (name != null && instances.TryGetValue(name, out var instance))
            {
                beforInit?.Invoke(instance);
                return instance;
            }
            var completionSource = new TaskCompletionSource<GameObject>();
            OnAssetRequest?.Invoke(name, asset =>
            {
                completionSource.SetResult(asset);
            });
            var refInstance = await completionSource.Task;
            instance = GameObject.Instantiate(refInstance, parent);
            beforInit?.Invoke(instance);
            await InvokeInitialize(instance);
            instances[name] = instance;
            return instance;
        }

        private static void ReleaseAsset(string name)
        {
            if (name != null && instances.TryGetValue(name, out var instance))
            {
                var component = instance.GetComponent<UIBase>();

                if (!component.CanDestroy) return;

                Destroy(instance);
                OnAssetRelease?.Invoke(name);
                instances.Remove(name);
            }
        }

        private static async Task InvokeInitialize(GameObject instance)
        {
            if (instance == null) return;

            var self = instance.GetComponent<UIBase>();
            var children = instance.GetComponentsInChildren<UIBase>().Where(item => item != self);
            try
            {
                await self.Initialize();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            foreach (var item in children)
            {
                try
                {
                    await item.Initialize();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static void InvokeAddListeners(GameObject instance)
        {
            if (instance == null) return;

            var self = instance.GetComponent<UIBase>();
            var children = instance.GetComponentsInChildren<UIBase>().Where(item => item != self);
            try
            {
                self.AddListeners();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            foreach (var item in children)
            {
                try
                {
                    item.AddListeners();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static void SetProperties(GameObject instance, UIProperties properties)
        {
            if (instance == null) return;

            var self = instance.GetComponent<UIBase>();
            try
            {
                self.GetType().GetProperty("Properties", BindingFlags.Public | BindingFlags.Instance).SetValue(self, properties);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static void InvokeRefresh(GameObject instance)
        {
            if (instance == null) return;

            var self = instance.GetComponent<UIBase>();
            var children = instance.GetComponentsInChildren<UIBase>().Where(item => item != self);
            try
            {
                self.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            foreach (var item in children)
            {
                try
                {
                    item.Refresh();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static void InvokeRemoveListener(GameObject instance)
        {
            if (instance == null) return;

            var self = instance.GetComponent<UIBase>();
            var children = instance.GetComponentsInChildren<UIBase>().Where(item => item != self);
            foreach (var item in children)
            {
                try
                {
                    item.RemoveListeners();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            try
            {
                self.RemoveListeners();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static async void ShowPanel(string name, UIProperties properties, Action callback)
        {
            if (panelStack.Count > 0 && name == panelStack.Peek().name) return;

            properties ??= new UIProperties();
            string currentPanelName = panelStack.Count > 0 ? panelStack.Peek().name : null; ;
            InvokeRemoveListener(CurrentPanel);
            properties.Sender = currentPanelName;
            var timeout = new CancellationTokenSource();
            bool isStuck = false;
            Task.Delay(TimeSpan.FromSeconds(StuckTime)).GetAwaiter().OnCompleted(() =>
            {
                if (timeout.IsCancellationRequested) return;
                OnStuckStart?.Invoke(name);
                isStuck = true;
            });
            var instance = await RequestAsset(name, PanelLayer, instance =>
            {
                CurrentPanel?.SetActive(false);
                ReleaseAsset(currentPanelName);
            });
            timeout.Cancel();

            if (isStuck) OnStuckEnd?.Invoke(name);

            instance.SetActive(true);
            instance.transform.SetAsLastSibling();
            panelStack.Push((name, properties));
            InvokeAddListeners(instance);
            SetProperties(instance, properties);
            InvokeRefresh(instance);
            callback?.Invoke();
        }

        public static void ShowPanel<T>(UIProperties properties)
        {
            ShowPanel(typeof(T).Name, properties, null);
        }

        public static void ShowPanel<T>()
        {
            ShowPanel(typeof(T).Name, null, null);
        }

        public static void ShowPanel<T>(UIProperties properties, Action callback)
        {
            ShowPanel(typeof(T).Name, properties, callback);
        }

        public static void ShowPanel<T>(Action callback)
        {
            ShowPanel(typeof(T).Name, null, callback);
        }

        public static async void HidePanel(Action callback)
        {
            var currentPanelName = panelStack.Count > 0 ? panelStack.Peek().name : null;
            if (currentPanelName == null)
            {
                callback?.Invoke();
                return;
            }
            var currentPanel = CurrentPanel;
            panelStack.Pop();
            if (panelStack.Count > 0)
            {
                InvokeRemoveListener(currentPanel);
                var source = new CancellationTokenSource();
                bool isStuck = false;
                Task.Delay(TimeSpan.FromSeconds(StuckTime)).GetAwaiter().OnCompleted(() =>
                {
                    if (source.IsCancellationRequested) return;
                    OnStuckStart?.Invoke(panelStack.Peek().name);
                    isStuck = true;
                });
                var instance = await RequestAsset(panelStack.Peek().name, PanelLayer, instance =>
                {
                    instance.SetActive(true);
                    instance.transform.SetAsLastSibling();
                    currentPanel?.SetActive(false);
                    ReleaseAsset(currentPanelName);
                });
                source.Cancel();
                if (isStuck)
                {
                    OnStuckEnd?.Invoke(panelStack.Peek().name);
                }
                InvokeAddListeners(instance);
                panelStack.Peek().properties.Sender = currentPanelName;
                SetProperties(instance, panelStack.Peek().properties);
                InvokeRefresh(instance);
            }
            else
            {
                InvokeRemoveListener(currentPanel);
                currentPanel?.SetActive(false);
                ReleaseAsset(currentPanelName);
            }
            callback?.Invoke();
        }

        public static void HidePanel()
        {
            HidePanel(null);
        }

        public static async void OpenWindow(string name, UIProperties properties, Action callback)
        {
            var instance = await RequestAsset(name, WindowLayer, null);
            instance.SetActive(true);
            instance.transform.SetAsLastSibling();
            InvokeAddListeners(instance);
            SetProperties(instance, properties);
            InvokeRefresh(instance);
            callback?.Invoke();
        }

        public static void OpenWindow<T>(UIProperties properties)
        {
            OpenWindow(typeof(T).Name, properties, null);
        }

        public static void OpenWindow<T>()
        {
            OpenWindow(typeof(T).Name, null, null);
        }

        public static void OpenWindow<T>(UIProperties properties, Action callback)
        {
            OpenWindow(typeof(T).Name, properties, callback);
        }

        public static void OpenWindow<T>(Action callback)
        {
            OpenWindow(typeof(T).Name, null, callback);
        }

        public static void CloseWindow<T>()
        {
            var name = typeof(T).Name;
            if (name != null && instances.TryGetValue(name, out var instance))
            {
                InvokeRemoveListener(instance);
                instance.SetActive(false);
                ReleaseAsset(name);
            }
        }

        public static void Refresh(string name)
        {
            if (name != null && instances.TryGetValue(name, out var instance))
            {
                InvokeRefresh(instance);
            }
        }

        public static void Refresh<T>()
        {
            Refresh(typeof(T).Name);
        }

        public void Release()
        {
            foreach (var item in instances)
            {
                if (item.Value != null && !item.Value.activeInHierarchy)
                {
                    Destroy(item.Value);
                    OnAssetRelease(item.Key);
                    instances.Remove(item.Key);
                }
            }
        }
    }
}