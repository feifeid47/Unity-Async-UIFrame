using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

namespace Feif.UIFramework
{
    public class UITimer
    {
        private float progress = 0;
        private Action callback = null;

        public float Delay { get; private set; }
        public bool IsCancel { get; private set; }
        public bool IsLoop { get; private set; }

        protected internal UITimer(float delay, Action callback, bool isLoop = false)
        {
            Delay = delay;
            IsLoop = isLoop;
            this.callback = callback;
        }

        protected internal void Update()
        {
            progress += Time.deltaTime;
            if (!IsCancel && progress >= Delay)
            {
                callback?.Invoke();
                progress = 0;
                if (!IsLoop) Cancel();
            }
        }

        public void Cancel()
        {
            IsCancel = true;
        }
    }

    public class UITimerAttribute : Attribute
    {
        public float Delay { get; private set; }
        public bool IsLoop { get; private set; }

        public UITimerAttribute(float delay, bool isLoop = false)
        {
            Delay = delay;
            IsLoop = isLoop;
        }
    }

    internal class AutoBindUITimer
    {
        private static Dictionary<UIBase, List<(float, Action, bool)>> binds = new Dictionary<UIBase, List<(float, Action, bool)>>();
        private static Dictionary<UIBase, List<UITimer>> timers = new Dictionary<UIBase, List<UITimer>>();

        public static void Enable()
        {
            UIFrame.OnCreate += OnCreate;
            UIFrame.OnBind += OnBind;
            UIFrame.OnUnbind += OnUnbind;
            UIFrame.OnDied += OnDied;
        }

        public static void Disable()
        {
            UIFrame.OnCreate -= OnCreate;
            UIFrame.OnBind -= OnBind;
            UIFrame.OnUnbind -= OnUnbind;
            UIFrame.OnDied -= OnDied;
        }

        private static void OnCreate(UIBase uibase)
        {
            var methods = uibase.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(item => Attribute.IsDefined(item, typeof(UITimerAttribute)));

            binds[uibase] = new List<(float, Action, bool)>();
            timers[uibase] = new List<UITimer>();

            var bind = binds[uibase];

            foreach (var method in methods)
            {
                if (method.GetParameters().Length == 0)
                {
                    var attribute = method.GetCustomAttribute<UITimerAttribute>();
                    Action callback = (Action)Delegate.CreateDelegate(typeof(Action), uibase, method);
                    bind.Add((attribute.Delay, callback, attribute.IsLoop));
                }
            }
        }

        private static void OnBind(UIBase uibase)
        {
            if (binds.TryGetValue(uibase, out var callbacks))
            {
                foreach (var (delay, callback, isLoop) in callbacks)
                {
                    timers[uibase].Add(UIFrame.CreateTimer(delay, callback, isLoop));
                }
            }
        }

        private static void OnUnbind(UIBase uibase)
        {
            if (timers.TryGetValue(uibase, out var timerList))
            {
                timerList.ForEach(item => item.Cancel());
            }
        }

        private static void OnDied(UIBase uibase)
        {
            binds.Remove(uibase);
            timers.Remove(uibase);
        }
    }
}