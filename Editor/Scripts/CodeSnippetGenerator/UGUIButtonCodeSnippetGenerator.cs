using System.Collections.Generic;
using System.Linq;
using Feif.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Feif.UIFramework.Editor
{
    public class UGUIButtonCodeSnippetGenerator : CodeSnippetGenerator
    {
        public override int GetPriority()
        {
            return 150;
        }

        public override List<GameObject> GetGameObjects(GameObject prefab)
        {
            return prefab.transform.BreadthTraversal(t => t != prefab.transform && t.GetComponent<UIBase>() != null)
                .Where(item => item.GetComponent<Button>() != null && item.name.StartsWith("@"))
                .Select(item => item.gameObject)
                .ToList();
        }

        public override List<string> GenerateField(GameObject gameObject)
        {
            var name = gameObject.name;
            var fieldName = name.Replace("@", "");
            fieldName = fieldName.Substring(0, 1).ToLower() + fieldName.Substring(1);
            return new List<string>()
            {
                $"[SerializeField] private Button {fieldName};"
            };
        }

        public override List<string> GenerateFunction(GameObject gameObject)
        {
            var name = gameObject.name;
            var functionName = name.Replace("@", "");
            functionName = functionName.Substring(0, 1).ToUpper() + functionName.Substring(1);
            return new List<string>()
            {
                $"[UGUIButtonEvent(\"{name}\")]",
                $"protected void OnClick{functionName}()",
                "{",
                "}",
                "",
            };
        }
    }
}