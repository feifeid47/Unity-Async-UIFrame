using UnityEngine;
using UnityEngine.UI;
using Feif.UIFramework;
#if USING_UNITASK
using Task = Cysharp.Threading.Tasks.UniTask;
#else
using Task = System.Threading.Tasks.Task;
#endif

namespace Feif.UI
{
    public class UITestBData : UIData
    {
        public string Title;
    }

    [PanelLayer]
    public class UITestB : UIComponent<UITestBData>
    {
        [SerializeField] private Text txtTitle;
        [SerializeField] private SubUIA subuia;
        [SerializeField] private SubUIB subuib;

        protected override Task OnRefresh()
        {
            txtTitle.text = this.Data.Title;
            Debug.Log("Refresh UITestB");
            subuib.Data = new SubUIBData()
            {
                Content = $"来自父物体的刷新调用 {Random.Range(0, 100)}"
            };
            return Task.CompletedTask;
        }

        [UGUIButtonEvent("@BtnShow1")]
        protected void OnBtnShow1()
        {
            // 显示子UI
            UIFrame.Show(subuia);
        }

        [UGUIButtonEvent("@BtnShow2")]
        protected void OnBtnShow2()
        {
            var data = new SubUIBData()
            {
                Content = $"来自Show按钮的刷新调用 {Random.Range(0, 100)}"
            };
            // 显示子UI
            UIFrame.Show(subuib, data);
        }

        [UGUIButtonEvent("@BtnBack")]
        protected void OnBtnBack()
        {
            // 隐藏当前Panel，使用UIFrame.Hide(this)或使用UIFrame.Hide()都可以
            UIFrame.Hide();
        }

        [UGUIButtonEvent("@BtnStarred")]
        protected async void OnBtnStarred()
        {
            var data = new UIConfirmBoxData()
            {
                Content = "打开这个UI需要较长的加载时间，是否继续？\n（请确保网络通畅）",
                ConfirmAction = () =>
                {
                    UIFrame.Show<UIStarred>();
                },
                CancelAction = null
            };
            await UIFrame.Show<UIConfirmBox>(data);
        }

        [UGUIButtonEvent("@BtnRefresh")]
        protected void OnBtnRefresh()
        {
            // 刷新，使用已有的UIData刷新
            UIFrame.Refresh(this);
        }
    }
}