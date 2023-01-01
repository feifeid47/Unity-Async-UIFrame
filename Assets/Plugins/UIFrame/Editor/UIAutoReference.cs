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

            var changedFields = SetReference(uibase);
            if (changedFields.Count > 0)
            {
                foreach (var field in changedFields)
                {
                    Debug.Log($"自动引用: {field}");
                }
            }
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
                    if (!(list == null || list.Count == 0)) continue;

                    Undo.RecordObject(script, field.Name);

                    // 初始化空数组
                    field.SetValue(script, Activator.CreateInstance(fieldType));
                    // 获得数组
                    list = field.GetValue(script) as IList;
                    list.Clear();
                    // 获得元素组的父节点
                    var content = (type.GetMethod("GetComponentsInChildren", new Type[] { })
                        ?.MakeGenericMethod(typeof(Transform))
                        .Invoke(script, new object[] { }) as Component[])
                        .FirstOrDefault(item => item.name.Trim('@').ToUpper() == field.Name.ToUpper()) as Transform;
                    if (content == null) continue;
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
                var method = type.GetMethod("GetComponentsInChildren", new Type[] { }).MakeGenericMethod(fieldType);
                var components = method.Invoke(script, new object[] { }) as Component[];
                var target = components.FirstOrDefault(item =>
                {
                    if (!item.name.StartsWith('@')) return false;
                    return field.Name.ToUpper() == item.name.Trim('@').ToUpper();
                });
                if (target == null) continue;

                Undo.RecordObject(script, field.Name);

                var sourceValue = field.GetValue(script);
                if (!(sourceValue == null || (sourceValue as UnityObject) == null)) continue;
                result.Add(field.Name);
                field.SetValue(script, isGameObject ? target.gameObject as UnityObject : target);
            }
            EditorUtility.SetDirty(script);
            return result;
        }
    }
}