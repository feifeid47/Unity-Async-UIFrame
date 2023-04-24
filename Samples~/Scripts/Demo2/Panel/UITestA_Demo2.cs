using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Feif.UIFramework;
using System;

namespace Feif.UI
{
    public class UITestAData_Demo2 : UIData
    {
        public string Title;
    }

    [UIPanel]
    public class UITestA_Demo2 : UIComponent<UITestAData_Demo2>
    {
        [SerializeField] private Text txtTitle;

        protected override Task OnRefresh()
        {
            txtTitle.text = Data.Title;
            Debug.Log("Refresh UITestA");
            return Task.CompletedTask;
        }

        [UGUIButtonEvent]
        protected void OnBtnNext()
        {
            var data = new UITestBData_Demo2()
            {
                Title = "This is UITestB"
            };
            // 显示下一个Panel
            UIFrame.Show<UITestB_Demo2>(data);
        }

        [UGUIButtonEvent]
        protected void OnBtnRefresh()
        {
            // 刷新，使用新的UIData刷新
            UIFrame.Refresh(this, new UITestAData_Demo2()
            {
                Title = DateTime.Now.ToString()
            });
        }
    }
}