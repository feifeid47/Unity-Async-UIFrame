using Feif.UI;
using Feif.UIFramework;
using System;
using UnityEngine;
#if USING_UNITASK
using Task = Cysharp.Threading.Tasks.UniTask;
using Cysharp.Threading.Tasks;
#else
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
#endif

namespace Feif
{
    public class DemoLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject stuckPanel;

        // 使用UIFrame时要先确保UIFrame的Awake已经执行过了
        private void Start()
        {
            // 注册资源请求释放事件
            UIFrame.OnAssetRequest += OnAssetRequest;
            UIFrame.OnAssetRelease += OnAssetRelease;
            // 注册UI卡住事件
            // 加载时间超过0.5s后触发UI卡住事件
            UIFrame.StuckTime = 0.5f;
            UIFrame.OnStuckStart += OnStuckStart;
            UIFrame.OnStuckEnd += OnStuckEnd;

            var data = new UITestAData();
            data.Title = "This is UITestA";
            UIFrame.Show<UITestA>(data);
        }

        // 资源请求事件，type为UI脚本的类型
        // 可以使用Addressables，YooAssets等第三方资源管理系统
#if USING_UNITASK
        private UniTask<GameObject> OnAssetRequest(Type type)
#else
        private Task<GameObject> OnAssetRequest(Type type)
#endif
        {
            var layer = UIFrame.GetLayer(type);
            return Task.FromResult(Resources.Load<GameObject>($"{layer.GetName()}/{type.Name}"));
        }

        // 资源释放事件
        private void OnAssetRelease(Type type)
        {
            // TODO
        }

        private void OnStuckStart()
        {
            stuckPanel.SetActive(true);
        }

        private void OnStuckEnd()
        {
            stuckPanel.SetActive(false);
        }
    }
}