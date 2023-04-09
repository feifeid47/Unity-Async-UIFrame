using Feif.UIFramework;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Feif.UI
{
    public class SubUIBData : UIData
    {
        public string Content;
    }

    public class SubUIB : UIComponent<SubUIBData>
    {
        [SerializeField] private Text txtContent;
        [SerializeField] private Button btnRefresh;
        [SerializeField] private Button btnHide;

        private int refreshCount = 0;

        protected override void OnBind()
        {
            btnRefresh.onClick.AddListener(OnBtnRefresh);
            btnHide.onClick.AddListener(OnBtnHide);
        }

        protected override void OnUnbind()
        {
            btnRefresh.onClick.RemoveListener(OnBtnRefresh);
            btnHide.onClick.RemoveListener(OnBtnHide);
        }

        protected override Task OnRefresh()
        {
            Debug.Log("Refresh SubUIB");
            txtContent.text = $"刷新次数 = {++refreshCount}\n {this.Data.Content}";
            return Task.CompletedTask;
        }

        private void OnBtnHide()
        {
            UIFrame.Hide(this);
        }

        private void OnBtnRefresh()
        {
            this.Data = new SubUIBData()
            {
                Content = $"来自自身的刷新调用 {Random.Range(0, 100)}"
            };
            // 刷新当前UI
            UIFrame.Refresh(this);
        }
    }
}