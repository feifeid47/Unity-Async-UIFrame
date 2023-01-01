using UnityEngine;

namespace Feif.UIFramework
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public abstract class UIComponent<T> : UIBase where T : UIProperties
    {
        public T Properties { get; set; }
    }
}