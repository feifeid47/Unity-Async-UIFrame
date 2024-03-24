
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using Feif.Extensions;

namespace Feif.UIFramework
{
    public class UGUIButtonEventAttribute : Attribute
    {
        public string Name { get; private set; }

        public UGUIButtonEventAttribute(string name)
        {
            Name = name;
        }
    }

    internal class AutoBindUGUIButtonEvent
    {
        private static Dictionary<UIBase, List<(Button btn, UnityAction action)>> binds = new Dictionary<UIBase, List<(Button, UnityAction)>>();

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
            binds[uibase] = new List<(Button, UnityAction)>();
            var bind = binds[uibase];

            var buttons = uibase.transform.BreadthTraversal(t => t.transform != uibase.transform && t.GetComponent<UIBase>() != null)
            .Where(item => item.GetComponent<Button>() != null)
            .Select(item => item.GetComponent<Button>())
            .ToArray();

            foreach (var method in methods)
            {
                if (method.GetParameters().Length == 0)
                {
                    var attribute = method.GetCustomAttribute<UGUIButtonEventAttribute>();
                    var callback = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), uibase, method);
                    var btn = buttons.FirstOrDefault(item => item.name == attribute.Name);
                    bind.Add((btn, callback));
                }
            }

        }

        private static void OnBind(UIBase uibase)
        {
            if (binds.TryGetValue(uibase, out var bind))
            {
                foreach (var (btn, action) in bind)
                {
                    btn.onClick.AddListener(action);
                }
            }
        }

        private static void OnUnbind(UIBase uibase)
        {
            if (binds.TryGetValue(uibase, out var bind))
            {
                foreach (var (btn, action) in bind)
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