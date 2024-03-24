using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using System.Collections;
using UnityObject = UnityEngine.Object;
using Feif.Extensions;

# if UNITY_2020_1_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Feif.UIFramework.Editor
{
    public static class UIAutoReference
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

            var uibases = prefab.transform.BreadthTraversal()
                 .Where(item => item.GetComponent<UIBase>() != null)
                 .Select(item => item.GetComponent<UIBase>());
            foreach (var item in uibases)
            {
                item.Parent = null;
                item.Children.Clear();
            }
            foreach (var item in uibases)
            {
                var parent = item.GetComponentsInParent<UIBase>().FirstOrDefault(p => p != item);
                if (parent == null) continue;
                item.Parent = parent;
                parent.Children.Add(item);
            }

            foreach (var item in uibase.transform.BreadthTraversal().Where(item => item.GetComponent<UIBase>() != null))
            {
                SetReference(item.GetComponent<UIBase>());
            }
        }

        /// <summary>
        /// 自动引用，并维护UI关系树
        /// </summary>
        /// <returns>被自动引用赋值的的字段</returns>
        public static List<string> SetReference(UnityObject script)
        {
            var result = new List<string>();
            var type = script.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                  .Where(item => Attribute.IsDefined(item, typeof(SerializeField)));
            Transform scriptTransform;
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
                    scriptTransform = (script as Component).transform;
                    var content = scriptTransform.BreadthTraversal(except: t => t != scriptTransform && t.GetComponent<UIBase>() != null)
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

                scriptTransform = (script as Component).transform;
                var target = scriptTransform.BreadthTraversal(except: t => t != scriptTransform && t.GetComponent<UIBase>() != null)
                    .Where(item => item.GetComponent(fieldType) != null)
                    .Select(item => item.GetComponent(fieldType))
                    .FirstOrDefault(item =>
                    {
                        if (!item.name.StartsWith("@")) return false;
                        return field.Name.ToUpper() == item.name.Trim('@').ToUpper();
                    });

                if (target == null) continue;

                result.Add(field.Name);
                field.SetValue(script, isGameObject ? target.gameObject as UnityObject : target);
            }
            EditorUtility.SetDirty(script);
            return result;
        }

        /// <summary>
        /// 扫描目录下所有Prefab，并自动引用UIBase组件中的值
        /// </summary>
        /// <param name="path">要扫描的目录，例如：Assets/Prefabs</param>
        /// <returns>被自动引用的资源(资源路径)</returns>
        public static List<string> SetReference(string path)
        {
            var result = new List<string>();
            var paths = AssetDatabase.GetAllAssetPaths().Where(item => item.StartsWith(path) && item.EndsWith(".prefab"));
            foreach (var item in paths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(item);
                if (asset == null) continue;
                var uibase = asset.GetComponent<UIBase>();
                if (uibase == null) continue;
                SetReference(uibase);
                result.Add(item);
            }
            return result;
        }
    }
}