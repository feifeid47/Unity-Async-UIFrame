using System;
using System.Collections.Generic;
using UnityEngine;
#if USING_UNITASK
using Task = Cysharp.Threading.Tasks.UniTask;
#else
using Task = System.Threading.Tasks.Task;
#endif

namespace Feif.UIFramework
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public abstract class UIBase : MonoBehaviour
    {
        /// <summary>
        /// 是否自动销毁
        /// </summary>
        public bool AutoDestroy = true;

        /// <summary>
        /// 一级父节点
        /// </summary>
        public UIBase Parent;

        /// <summary>
        /// 一级子节点
        /// </summary>
        public List<UIBase> Children = new List<UIBase>();

        protected internal Task InnerOnCreate() => OnCreate();
        protected internal Task InnerOnRefresh() => OnRefresh();
        protected internal void InnerOnBind() => OnBind();
        protected internal void InnerOnUnbind() => OnUnbind();
        protected internal void InnerOnShow() => OnShow();
        protected internal void InnerOnHide() => OnHide();
        protected internal void InnerOnDied() => OnDied();

        private HashSet<UITimer> timers = new HashSet<UITimer>();

        /// <summary>
        /// 创建时调用，生命周期内只执行一次
        /// </summary>
        protected virtual Task OnCreate() => Task.CompletedTask;

        /// <summary>
        /// 刷新时调用
        /// </summary>
        protected virtual Task OnRefresh() => Task.CompletedTask;

        /// <summary>
        /// 绑定事件
        /// </summary>
        protected virtual void OnBind() { }

        /// <summary>
        /// 解绑事件
        /// </summary>
        protected virtual void OnUnbind() { }

        /// <summary>
        /// 显示时调用
        /// </summary>
        protected virtual void OnShow() { }

        /// <summary>
        /// 隐藏时调用
        /// </summary>
        protected virtual void OnHide() { }

        /// <summary>
        /// 销毁时调用，生命周期内只执行一次
        /// </summary>
        protected virtual void OnDied() { }

        /// <summary>
        /// 创建定时器，gameObject被销毁时会自动Cancel定时器
        /// </summary>
        /// <param name="delay">延迟多少秒后执行callback</param>
        /// <param name="callback">延迟执行的方法</param>
        /// <param name="isLoop">是否是循环定时器</param>
        protected UITimer CreateTimer(float delay, Action callback, bool isLoop = false)
        {
            var timer = UIFrame.CreateTimer(delay, callback, isLoop);
            timers.Add(timer);
            return timer;
        }

        /// <summary>
        /// 取消所有定时器
        /// </summary>
        public void CancelAllTimer()
        {
            foreach (var item in timers)
            {
                item.Cancel();
            }
            timers.Clear();
        }
    }
}