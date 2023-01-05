using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using System.Collections;
using UnityObject = UnityEngine.Object;
using UnityEditor.SceneManagement;

namespace Feif.UIFramework.Editor
{
    public class UIAutoReference
    {
        [InitializeOnLoadMethod]
        public static void AddListeners()
        {
            PrefabStage.prefabSaving -= OnPrefabSaving;
            PrefabStage.prefabSaving += OnPrefabSaving;
        }

        private static void OnPrefabSaving(GameObject prefab)
        {
            var uibase = prefab.GetComponent<UIBase>();
            if (uibase == null) return;

            var guid = AssetDatabase.FindAssets("t:UIFrameSetting").FirstOrDefault();
            if (guid == null) return;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var setting = AssetDatabase.LoadAssetAtPath<UIFrameSetting>(path);
            if (setting == null || setting.AutoReference == false) return;

            _ = SetReference(uibase);
        }

        public static List<string> SetReference(UnityObject script)
        {
            List<string> result = new List<string>();
            var type = script.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                  .Where(item => Attribute.IsDefined(item, typeof(SerializeField)));
            foreach (var field in fields)
            {
                bool isGameObject = field.FieldType.IsEquivalentTo(typeof(GameObject));
                // 如果是GameObject类型，获得Transform类型
                var fieldType = isGameObject ? typeof(Transform) : field.FieldType;
                // 如果是数组类型
                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var list = field.GetValue(script) as IList;

                    // 获得元素组的父节点
                    var content = GetComponentsInChildrenIgnoreActive<Transform>((script as Component).transform)
                         .FirstOrDefault(item => item.name.Trim('@').ToUpper() == field.Name.ToUpper());

                    if (content == null) continue;
                    if (!content.name.StartsWith("@")) continue;

                    // 初始化空数组
                    field.SetValue(script, Activator.CreateInstance(fieldType));
                    // 获得数组
                    list = field.GetValue(script) as IList;
                    list.Clear();

                    // 泛型类型
                    var genericType = fieldType.GetGenericArguments().First();
                    result.Add(field.Name);
                    for (int i = 0; i < content.childCount; i++)
                    {
                        if (genericType.IsEquivalentTo(typeof(GameObject)))
                        {
                            list.Add(content.GetChild(i).gameObject);
                        }
                        else
                        {
                            // 给数组添加元素
                            list.Add(content.GetChild(i).GetComponent(genericType.Name));
                        }
                    }
                    continue;
                }
                if (!fieldType.IsSubclassOf(typeof(Component))) continue;
                var components = GetComponentsInChildrenIgnoreActive<Component>((script as Component).transform);
                var target = components.FirstOrDefault(item =>
                {
                    if (item.GetType() != fieldType) return false;
                    if (!item.name.StartsWith('@')) return false;

                    return field.Name.ToUpper() == item.name.Trim('@').ToUpper();
                });

                if (target == null) continue;

                result.Add(field.Name);
                field.SetValue(script, isGameObject ? target.gameObject as UnityObject : target);
            }
            EditorUtility.SetDirty(script);
            return result;
        }

        public static List<T> GetComponentsInChildrenIgnoreActive<T>(Transform root) where T : Component
        {
            List<T> components = new List<T>();
            if (root.GetComponents<T>() != null) components.AddRange(root.GetComponents<T>());
            for (int i = 0; i < root.childCount; i++)
            {
                var t = GetComponentsInChildrenIgnoreActive<T>(root.GetChild(i));
                components.AddRange(t);
            }
            return components;
        }
    }
}