using Feif.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Feif.UIFramework
{
    // 物体名称 => [属性] 方法名称
    // @BtnClick => [UGUIButtonEvent] void OnBtnClick();
    public class AutoBindUGUIButtonEvent
    {
        private static Dictionary<UIBase, Dictionary<string, (Button btn, UnityAction action)>> binds = new Dictionary<UIBase, Dictionary<string, (Button, UnityAction)>>();

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
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .Where(item => Attribute.IsDefined(item, typeof(UGUIButtonEventAttribute)));
            binds[uibase] = new Dictionary<string, (Button, UnityAction)>();
            var bind = binds[uibase];
            var buttons = new Dictionary<string, Button>();

            foreach (var item in uibase.transform.BreadthTraversal()
                .Where(item => item.GetComponent<Button>() != null
                && item.name.StartsWith("@")))
            {
                var key = $"On{item.name.Trim('@')}".ToUpper();
                if (!buttons.ContainsKey(key))
                {
                    buttons[key] = item.GetComponent<Button>();
                }
            }

            foreach (var method in methods)
            {
                var key = method.Name.ToUpper();
                if (buttons.TryGetValue(key, out var btn))
                {
                    bind[key] = (btn, (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), uibase, method));
                }
            }
        }

        private static void OnBind(UIBase uibase)
        {
            if (binds.TryGetValue(uibase, out var bind))
            {
                foreach (var (btn, action) in bind.Values)
                {
                    btn.onClick.AddListener(action);
                }
            }
        }

        private static void OnUnbind(UIBase uibase)
        {
            if (binds.TryGetValue(uibase, out var bind))
            {
                foreach (var (btn, action) in bind.Values)
                {
                    btn.onClick.RemoveListener(action);
                }
            }
        }

        private static void OnDied(UIBase uibase)
        {
            binds.Remove(uibase);
        }
    }
}