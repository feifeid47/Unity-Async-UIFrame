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

        private int refreshCount = 0;

        protected override Task OnRefresh()
        {
            Debug.Log("Refresh SubUIB");
            txtContent.text = $"刷新次数 = {++refreshCount}\n {this.Data.Content}";
            return Task.CompletedTask;
        }

        [UGUIButtonEvent("@BtnHide")]
        protected void OnClickBtnHide()
        {
            UIFrame.Hide(this);
        }

        [UGUIButtonEvent("@BtnRefresh")]
        protected void OnClickBtnRefresh()
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