using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Feif.UIFramework;
using System;

namespace Feif.UI
{
    public class UITestAData : UIData
    {
        public string Title;
    }

    [UIPanel]
    public class UITestA : UIComponent<UITestAData>
    {
        [SerializeField] private Text txtTitle;
        [SerializeField] private Button btnNext;
        [SerializeField] private Button btnRefresh;

        protected override Task OnRefresh()
        {
            txtTitle.text = this.Data.Title;
            Debug.Log("Refresh UITestA");
            return Task.CompletedTask;
        }

        protected override void OnBind()
        {
            btnNext.onClick.AddListener(OnBtnNext);
            btnRefresh.onClick.AddListener(OnBtnRefresh);
        }

        protected override void OnUnbind()
        {
            btnNext.onClick.RemoveListener(OnBtnNext);
            btnRefresh.onClick.RemoveListener(OnBtnRefresh);
        }

        private void OnBtnNext()
        {
            var data = new UITestBData()
            {
                Title = "This is UITestB"
            };
            // 显示下一个Panel
            UIFrame.Show<UITestB>(data);
        }

        private void OnBtnRefresh()
        {
            // 刷新，使用新的UIData刷新
            UIFrame.Refresh(this, new UITestAData()
            {
                Title = DateTime.Now.ToString()
            });
        }
    }
}