using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Feif.UIFramework.Editor
{
    public static class UIScriptCreator
    {

        [MenuItem("Assets/Create/UIFrame/UIBase", false, 0)]
        public static void CreateUIBase()
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
            if (setting.UIBaseTemplate == null)
            {
                Debug.LogError("请设置模板文件，模板文件中 #SCRIPTNAME# 将被文件名替换", setting);
                return;
            }
            var templatePath = AssetDatabase.GetAssetPath(setting.UIBaseTemplate.GetInstanceID());
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path.Combine(Application.dataPath, "..", templatePath), "NewUIBase.cs");
        }

        [MenuItem("Assets/Create/UIFrame/UIComponent", false, 0)]
        public static void CreateUIComponent()
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
            if (setting.UIComponentTemplate == null)
            {
                Debug.LogError("请设置模板文件，模板文件中 #SCRIPTNAME# 将被文件名替换", setting);
                return;
            }
            var templatePath = AssetDatabase.GetAssetPath(setting.UIComponentTemplate.GetInstanceID());
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path.Combine(Application.dataPath, "..", templatePath), "NewUIComponent.cs");
        }

        [MenuItem("Assets/Create/UIFrame/UIPanel", false, 0)]
        public static void CreateUIPanel()
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
            if (setting.UIPanelTemplate == null)
            {
                Debug.LogError("请设置模板文件，模板文件中 #SCRIPTNAME# 将被文件名替换", setting);
                return;
            }
            var templatePath = AssetDatabase.GetAssetPath(setting.UIPanelTemplate.GetInstanceID());
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path.Combine(Application.dataPath, "..", templatePath), "NewUIPanel.cs");
        }

        [MenuItem("Assets/Create/UIFrame/UIWindow", false, 0)]
        public static void CreateUIWindow()
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
            if (setting.UIWindowTemplate == null)
            {
                Debug.LogError("请设置模板文件，模板文件中 #SCRIPTNAME# 将被文件名替换", setting);
                return;
            }
            var templatePath = AssetDatabase.GetAssetPath(setting.UIWindowTemplate.GetInstanceID());
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path.Combine(Application.dataPath, "..", templatePath), "NewUIWindow.cs");
        }
    }
}