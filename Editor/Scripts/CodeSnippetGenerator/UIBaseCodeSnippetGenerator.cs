using System.Collections.Generic;
using System.Linq;
using Feif.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Feif.UIFramework.Editor
{
    public class UIBaseCodeSnippetGenerator : CodeSnippetGenerator
    {
        public override int GetPriority()
        {
            return 160;
        }

        public override List<GameObject> GetGameObjects(GameObject prefab)
        {
            return prefab.transform.BreadthTraversal(t => t != prefab.transform && t.GetComponent<UIBase>() != null)
                .Where(item => item.GetComponent<UIBase>() != null && item.name.StartsWith("@"))
                .Select(item => item.gameObject)
                .ToList();
        }

        public override List<string> GenerateField(GameObject gameObject)
        {
            var name = gameObject.name;
            var fieldName = name.Replace("@", "");
            fieldName = fieldName.Substring(0, 1).ToLower() + fieldName.Substring(1);
            var uibaseTypeName = gameObject.GetComponent<UIBase>().GetType().Name;
            return new List<string>()
            {
                $"[SerializeField] private {uibaseTypeName} {fieldName};"
            };
        }
    }
}