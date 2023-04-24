using System.Collections.Generic;
using UnityEngine;

namespace Feif.Extensions
{
    public static class TransformExtensions
    {
        /// <summary>
        /// 深度优先遍历
        /// </summary>
        public static IEnumerable<Transform> DepthTraversal(this Transform root)
        {
            if (root == null) yield break;

            var stack = new Stack<Transform>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                for (int i = node.childCount - 1; i >= 0; --i)
                {
                    stack.Push(node.GetChild(i));
                }
            }
        }

        /// <summary>
        /// 广度优先遍历
        /// </summary>
        public static IEnumerable<Transform> BreadthTraversal(this Transform root)
        {
            if (root == null) yield break;

            var queue = new Queue<Transform>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                yield return node;
                for (int i = 0; i < node.childCount; ++i)
                {
                    queue.Enqueue(node.GetChild(i));
                }
            }
        }
    }
}