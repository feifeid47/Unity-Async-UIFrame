using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Feif.UIFramework.Editor
{
    public static class NewBehaviourScript
    {

        [MenuItem("Assets/创建UIComponent", false, 0)]
        public static void CreateScript()
        {
            var guid = AssetDatabase.FindAssets("t:UIFrameSetting").FirstOrDefault();
            if (guid == null)
            {
                var asset = ScriptableObject.CreateInstance<UIFrameSetting>();
                AssetDatabase.CreateAsset(asset, "Assets/UIFrameSetting.asset");
                AssetDatabase.Refresh();
                guid = AssetDatabase.FindAssets("t:UIFrameSetting").FirstOrDefault();
            }
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var setting = AssetDatabase.LoadAssetAtPath<UIFrameSetting>(path);
            if (setting.Template == null)
            {
                Debug.LogError("请设置模板文件，模板文件中 #SCRIPTNAME# 将被文件名替换", setting);
                return;
            }
            var templatePath = AssetDatabase.GetAssetPath(setting.Template.GetInstanceID());
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path.Combine(Application.dataPath, "..", templatePath), "NewUIComponent.cs");
        }
    }
}