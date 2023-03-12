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
    }
}