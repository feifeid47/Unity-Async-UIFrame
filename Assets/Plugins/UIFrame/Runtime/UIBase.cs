using System.Threading.Tasks;
using UnityEngine;

namespace Feif.UIFramework
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public abstract class UIBase : MonoBehaviour
    {
        public bool CanDestroy = true;

        public virtual Task Initialize() => Task.CompletedTask;

        public virtual void AddListeners() { }

        public virtual void RemoveListeners() { }

        public virtual void Refresh() { }

        public virtual void Show()
        {
            if (gameObject.activeInHierarchy) return;

            gameObject.SetActive(true);
            AddListeners();
        }

        public virtual void Hide()
        {
            if (!gameObject.activeInHierarchy) return;

            RemoveListeners();
            gameObject.SetActive(false);
        }
    }
}