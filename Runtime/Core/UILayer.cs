using System;

namespace Feif.UIFramework
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class UILayer : Attribute, IComparable<UILayer>
    {
        public int CompareTo(UILayer other)
        {
            return GetOrder().CompareTo(other.GetOrder());
        }

        public abstract string GetName();
        public abstract int GetOrder();
    }
}