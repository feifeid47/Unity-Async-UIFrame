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
    // 这个UI所需要的参数（数据）
    public class UIConfirmBoxData : UIData
    {
        public string Content;
        public Action ConfirmAction;
        public Action CancelAction;
    }

    // 这是一个Window，不需要UIData则继承UIBase，需要UIData则继承UIComponent
    [WindowLayer]
    public class UIConfirmBox : UIComponent<UIConfirmBoxData>
    {
        [SerializeField] private Text txtContent;

        private UIWindowEffect effect;

        protected override Task OnCreate()
        {
            effect = GetComponent<UIWindowEffect>();
            return Task.CompletedTask;
        }

        protected override Task OnRefresh()
        {
            txtContent.text = this.Data.Content;
            return Task.CompletedTask;
        }

        protected override void OnShow()
        {
            // 播放打开Window动效
            effect.PlayOpen();
        }

        [UGUIButtonEvent("@BtnConfirm")]
        protected async void OnClickBtnConfirm()
        {
            this.Data.ConfirmAction?.Invoke();
            // 播放关闭Window动效
            await effect.PlayClose();
            await UIFrame.Hide(this);
        }

        [UGUIButtonEvent("@BtnCancel")]
        protected async void OnClickBtnCancel()
        {
            this.Data.CancelAction?.Invoke();
            // 播放关闭Window动效
            await effect.PlayClose();
            await UIFrame.Hide(this);
        }
    }
}