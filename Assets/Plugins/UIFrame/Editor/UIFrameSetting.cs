using UnityEngine;

namespace Feif.UIFramework.Editor
{
    [CreateAssetMenu(fileName = "UIFrameSetting", menuName = "UIFrame/UIFrameSetting", order = 0)]
    public class UIFrameSetting : ScriptableObject
    {
        public TextAsset Template;
        public bool AutoReference = true;
    }
}