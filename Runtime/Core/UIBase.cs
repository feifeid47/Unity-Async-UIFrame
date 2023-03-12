using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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
    }
}