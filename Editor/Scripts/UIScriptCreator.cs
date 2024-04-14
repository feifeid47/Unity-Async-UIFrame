using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System;

namespace Feif.UIFramework.Editor
{
    public class TestEndAction : UnityEditor.ProjectWindowCallback.EndNameEditAction
    {
        public override void Action(int instanceId, string path, string content)
        {
            var script = content;
            var fileName = Path.GetFileName(path);
            script = script.Replace("#SCRIPTNAME#", Path.GetFileNameWithoutExtension(fileName));
            script = Regex.Replace(script, " *#FIELDS#", "");
            script = Regex.Replace(script, " *#FUNCTIONS#", "");
            script = Regex.Replace(script, @"^ +\n", "\n", RegexOptions.Multiline);
            script = Regex.Replace(script, @"^ +\r\n", "\r\n", RegexOptions.Multiline);
            var currentDir = ReflactionUtils.RunClassFunc<string>(typeof(ProjectWindowUtil), "GetActiveFolderPath");
            File.WriteAllText(Path.Combine(Application.dataPath, "..", currentDir, fileName), script);
            AssetDatabase.Refresh();
        }
    }

    public static class UIScriptCreator
    {
        public static Dictionary<string, List<string>> GetCodeSnippets(GameObject prefab)
        {
            var assembly = Assembly.GetAssembly(typeof(CodeSnippetGenerator));
            var generatorList = assembly.GetTypes()
                        .Where(item => item.IsSubclassOf(typeof(CodeSnippetGenerator)))
                        .Select(item => Activator.CreateInstance(item) as CodeSnippetGenerator)
                        .OrderByDescending(item => item.GetPriority());

            var finishedSet = new HashSet<GameObject>();

            var functionCodes = new List<string>();
            var fieldCodes = new List<string>();

            foreach (var generator in generatorList)
            {
                var objects = generator.GetGameObjects(prefab);
                foreach (var gameObject in objects)
                {
                    if (finishedSet.Contains(gameObject)) continue;

                    var fieldCode = generator.GenerateField(gameObject);
                    var functionCode = generator.GenerateFunction(gameObject);
                    if (fieldCode != null)
                    {
                        fieldCodes.AddRange(fieldCode);
                    }
                    if (functionCode != null)
                    {
                        functionCodes.AddRange(functionCode);
                    }
                    finishedSet.Add(gameObject);
                }
            }
            return new Dictionary<string, List<string>>()
            {
                {"Functions", functionCodes },
                {"Fields", fieldCodes }
            };
        }

        public static string AddCodeSnippetToTemplate(string template, string fileName, Dictionary<string, List<string>> codeSnippets)
        {
            var fieldIndent = Regex.Match(template, " *#FIELDS#").Value.Replace("#FIELDS#", "").Length;
            var functionIndent = Regex.Match(template, " *#FUNCTIONS#").Value.Replace("#FUNCTIONS#", "").Length;
            var fieldBuilder = new StringBuilder();
            var functionBuilder = new StringBuilder();
            foreach (var item in codeSnippets["Fields"])
            {
                fieldBuilder.AppendLine(new string(' ', fieldIndent) + item);
            }
            foreach (var item in codeSnippets["Functions"])
            {
                functionBuilder.AppendLine(new string(' ', functionIndent) + item);
            }
            var script = template;
            script = script.Replace("#SCRIPTNAME#", fileName);
            script = Regex.Replace(script, " *#FIELDS#", fieldBuilder.ToString());
            script = Regex.Replace(script, " *#FUNCTIONS#", functionBuilder.ToString().TrimEnd('\r', '\n'));
            script = Regex.Replace(script, @"^ +\n", "\n", RegexOptions.Multiline);
            script = Regex.Replace(script, @"^ +\r\n", "\r\n", RegexOptions.Multiline);
            return script;
        }

        public static void DoCreate(string fileName, string template)
        {
            if (Selection.objects.Length > 0)
            {
                var prefab = Selection.objects[0] as GameObject;
                if (prefab != null)
                {
                    var currentDir = ReflactionUtils.RunClassFunc<string>(typeof(ProjectWindowUtil), "GetActiveFolderPath");
                    var codeSnippets = GetCodeSnippets(prefab);
                    var result = AddCodeSnippetToTemplate(template, prefab.name, codeSnippets);
                    File.WriteAllText(Path.Combine(Application.dataPath, "..", currentDir, $"{prefab.name}.cs"), result);
                    AssetDatabase.Refresh();
                    return;
                }
            }
            var projectBrowserIfExists = ReflactionUtils.RunClassFunc(typeof(ProjectWindowUtil), "GetProjectBrowserIfExists");
            if (projectBrowserIfExists != null)
            {
                TestEndAction endAction = ScriptableObject.CreateInstance<TestEndAction>();
                ReflactionUtils.RunInstanceFunc(projectBrowserIfExists, "Focus");
                ReflactionUtils.RunInstanceFunc(projectBrowserIfExists, "BeginPreimportedNameEditing", 0, endAction, fileName, EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D, template, true);
                ReflactionUtils.RunInstanceFunc(projectBrowserIfExists, "Repaint");
            }
        }

        [MenuItem("Assets/Create/UIFrame/UIBase", false, 0)]
        public static void CreateUIBase()
        {
            DoCreate("NewUIBase.cs", UIFrameSetting.Instance.UIBaseTemplate.text);
        }

        [MenuItem("Assets/Create/UIFrame/UIComponent", false, 0)]
        public static void CreateUIComponent()
        {
            DoCreate("NewUIComponent.cs", UIFrameSetting.Instance.UIComponentTemplate.text);
        }

        [MenuItem("Assets/Create/UIFrame/UIPanel", false, 0)]
        public static void CreateUIPanel()
        {
            DoCreate("NewUIPanel.cs", UIFrameSetting.Instance.UIPanelTemplate.text);
        }

        [MenuItem("Assets/Create/UIFrame/UIWindow", false, 0)]
        public static void CreateUIWindow()
        {
            DoCreate("NewUIWindow.cs", UIFrameSetting.Instance.UIWindowTemplate.text);
        }
    }
}