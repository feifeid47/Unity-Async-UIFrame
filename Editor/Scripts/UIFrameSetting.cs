using System.IO;
using UnityEditor;
using UnityEngine;

namespace Feif.UIFramework.Editor
{
    [CreateAssetMenu(fileName = "UIFrameSetting", menuName = "UIFrame/UIFrameSetting", order = 0)]
    public class UIFrameSetting : ScriptableObject
    {
        public TextAsset UIBaseTemplate;
        public TextAsset UIComponentTemplate;
        public TextAsset UIPanelTemplate;
        public TextAsset UIWindowTemplate;
        public bool AutoReference = true;

        private void Reset()
        {
            var ms = MonoScript.FromScriptableObject(this);
            var path = AssetDatabase.GetAssetPath(ms);
            var resPath = Path.GetDirectoryName(path).Replace("Scripts", "Resources");
            var fields = GetType().GetFields();
            foreach (var field in fields)
            {
                if (field.Name.EndsWith("Template"))
                {
                    var file = Path.Combine(resPath, $"{field.Name}.txt");
                    var res = AssetDatabase.LoadAssetAtPath<TextAsset>(file);
                    field.SetValue(this, res);
                }
            }
            EditorUtility.SetDirty(this);
        }
    }
}