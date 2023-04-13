using UnityEngine;

namespace Feif
{
    public class StuckPanel : MonoBehaviour
    {
        [SerializeField] RectTransform img;

        private void Update()
        {
            img.Rotate(0, 0, -500 * Time.deltaTime);
        }
    }
}