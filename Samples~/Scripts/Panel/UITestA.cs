using UnityEngine;
using UnityEngine.UI;
using Feif.UIFramework;
using System;
#if USING_UNITASK
using Task = Cysharp.Threading.Tasks.UniTask;
#else
using Task = System.Threading.Tasks.Task;
#endif

namespace Feif.UI
{
    public class UITestAData : UIData
    {
        public string Title;
    }

    [PanelLayer]
    public class UITestA : UIComponent<UITestAData>
    {
        [SerializeField] private Text txtTitle;

        protected override Task OnRefresh()
        {
            txtTitle.text = this.Data.Title;
            Debug.Log("Refresh UITestA");
            return Task.CompletedTask;
        }

        [UGUIButtonEvent("@BtnNext")]
        protected void OnBtnNext()
        {
            var data = new UITestBData()
            {
                Title = "This is UITestB"
            };
            // 显示下一个Panel
            UIFrame.Show<UITestB>(data);
        }

        [UGUIButtonEvent("@BtnRefresh")]
        protected void OnBtnRefresh()
        {
            // 刷新，使用新的UIData刷新
            UIFrame.Refresh(this, new UITestAData()
            {
                Title = DateTime.Now.ToString()
            });
            Debug.Log("OnBtnRefresh");
        }

        [UITimer(delay: 1f, isLoop: true)]
        protected void UpdateEverySecond()
        {
            UIFrame.Refresh(this, new UITestAData()
            {
                Title = DateTime.Now.ToString()
            });
        }
    }
}