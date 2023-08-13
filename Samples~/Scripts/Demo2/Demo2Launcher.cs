using Feif.UI;
using Feif.UIFramework;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Feif
{
    public class Demo2Launcher : MonoBehaviour
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
            // <------------------------------>
            // 自动绑定事件
            AutoBindUGUIButtonEvent.Enable();
            // <------------------------------>
            var data = new UITestAData_Demo2();
            data.Title = "This is UITestA";
            UIFrame.Show<UITestA_Demo2>(data);
        }

        // 资源请求事件，type为UI脚本的类型
        // 可以使用Addressables，YooAssets等第三方资源管理系统
        private Task<GameObject> OnAssetRequest(Type type)
        {
            var layer = UIFrame.GetLayer(type);
            return Task.FromResult(Resources.Load<GameObject>($"Demo2/{layer.GetName()}/{type.Name}"));
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