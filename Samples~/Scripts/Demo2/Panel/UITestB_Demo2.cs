using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Feif.UIFramework;

namespace Feif.UI
{
    public class UITestBData_Demo2 : UIData
    {
        public string Title;
    }

    [UIPanel]
    public class UITestB_Demo2 : UIComponent<UITestBData_Demo2>
    {
        [SerializeField] private Text txtTitle;
        [SerializeField] private SubUIA_Demo2 subuia_demo2;
        [SerializeField] private SubUIB_Demo2 subuib_demo2;

        protected override Task OnRefresh()
        {
            txtTitle.text = Data.Title;
            Debug.Log("Refresh UITestB");
            subuib_demo2.Data = new SubUIBData_Demo2()
            {
                Content = $"来自父物体的刷新调用 {Random.Range(0, 100)}"
            };
            return Task.CompletedTask;
        }

        [UGUIButtonEvent]
        protected void OnBtnShow1()
        {
            // 显示子UI
            UIFrame.Show(subuia_demo2);
        }

        [UGUIButtonEvent]
        protected void OnBtnShow2()
        {
            var data = new SubUIBData_Demo2()
            {
                Content = $"来自Show按钮的刷新调用 {Random.Range(0, 100)}"
            };
            // 显示子UI
            UIFrame.Show(subuib_demo2, data);
        }

        [UGUIButtonEvent]
        protected void OnBtnBack()
        {
            // 隐藏当前Panel，使用UIFrame.Hide(this)或使用UIFrame.Hide()都可以
            UIFrame.Hide();
        }

        [UGUIButtonEvent]
        protected async void OnBtnStarred()
        {
            var data = new UIConfirmBoxData_Demo2()
            {
                Content = "打开这个UI需要较长的加载时间，是否继续？\n（请确保网络通畅）",
                ConfirmAction = () =>
                {
                    UIFrame.Show<UIStarred_Demo2>();
                },
                CancelAction = null
            };
            await UIFrame.Show<UIConfirmBox_Demo2>(data);
        }

        [UGUIButtonEvent]
        protected void OnBtnRefresh()
        {
            UIFrame.Refresh(this);
        }
    }
}