using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Feif.UIFramework;

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

        protected override Task OnRefresh()
        {
            txtTitle.text = this.Data.Title;
            Debug.Log("Refresh UITestA");
            return Task.CompletedTask;
        }

        protected override void OnBind()
        {
            btnNext.onClick.AddListener(OnBtnNext);
        }

        protected override void OnUnbind()
        {
            btnNext.onClick.RemoveListener(OnBtnNext);
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
    }
}