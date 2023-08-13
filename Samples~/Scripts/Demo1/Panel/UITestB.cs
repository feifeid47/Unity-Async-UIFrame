using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Feif.UIFramework;

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
        [SerializeField] private Button btnStarred;
        [SerializeField] private Button btnRefresh;
        [SerializeField] private Button btnBack;
        [SerializeField] private Button btnShow1;
        [SerializeField] private Button btnShow2;
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

        protected override void OnBind()
        {
            btnBack.onClick.AddListener(OnBtnBack);
            btnStarred.onClick.AddListener(OnBtnStarred);
            btnRefresh.onClick.AddListener(OnBtnRefresh);
            btnShow1.onClick.AddListener(OnBtnShow1);
            btnShow2.onClick.AddListener(OnBtnShow2);
        }

        protected override void OnUnbind()
        {
            btnBack.onClick.RemoveListener(OnBtnBack);
            btnStarred.onClick.RemoveListener(OnBtnStarred);
            btnRefresh.onClick.RemoveListener(OnBtnRefresh);
            btnShow1.onClick.RemoveListener(OnBtnShow1);
            btnShow2.onClick.RemoveListener(OnBtnShow2);
        }

        private void OnBtnShow1()
        {
            // 显示子UI
            UIFrame.Show(subuia);
        }

        private void OnBtnShow2()
        {
            var data = new SubUIBData()
            {
                Content = $"来自Show按钮的刷新调用 {Random.Range(0, 100)}"
            };
            // 显示子UI
            UIFrame.Show(subuib, data);
        }

        private void OnBtnBack()
        {
            // 隐藏当前Panel，使用UIFrame.Hide(this)或使用UIFrame.Hide()都可以
            UIFrame.Hide();
        }

        private async void OnBtnStarred()
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

        private void OnBtnRefresh()
        {
            // 刷新，使用已有的UIData刷新
            UIFrame.Refresh(this);
        }
    }
}