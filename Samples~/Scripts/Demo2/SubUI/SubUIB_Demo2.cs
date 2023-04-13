using Feif.UIFramework;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Feif.UI
{
    public class SubUIBData_Demo2 : UIData
    {
        public string Content;
    }

    public class SubUIB_Demo2 : UIComponent<SubUIBData_Demo2>
    {
        [SerializeField] private Text txtContent;

        private int refreshCount = 0;

        protected override Task OnRefresh()
        {
            Debug.Log("Refresh SubUIB");
            txtContent.text = $"刷新次数 = {++refreshCount}\n {Data.Content}";
            return Task.CompletedTask;
        }

        [UGUIButtonEvent]
        protected void OnBtnHide()
        {
            UIFrame.Hide(this);
        }

        [UGUIButtonEvent]
        protected void OnBtnRefresh()
        {
            Data = new SubUIBData_Demo2()
            {
                Content = $"来自自身的刷新调用 {Random.Range(0, 100)}"
            };
            // 刷新当前UI
            UIFrame.Refresh(this);
        }
    }
}