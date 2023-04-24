using Feif.UIFramework;
using System.Collections.Generic;

namespace Feif.Extensions
{
    public static class UIBaseExtensions
    {
        /// <summary>
        /// 广度优先遍历UIBase
        /// </summary>
        public static IEnumerable<UIBase> BreadthTraversal(this UIBase root)
        {
            if (root == null) yield break;

            var queue = new Queue<UIBase>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                yield return node;
                for (int i = 0; i < node.Children.Count; ++i)
                {
                    queue.Enqueue(node.Children[i]);
                }
            }
        }

        /// <summary>
        /// 深度优先遍历UIBase
        /// </summary>
        public static IEnumerable<UIBase> DepthTraversal(this UIBase root)
        {
            if (root == null) yield break;

            var stack = new Stack<UIBase>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                for (int i = node.Children.Count - 1; i >= 0; --i)
                {
                    stack.Push(node.Children[i]);
                }
            }
        }
    }
}