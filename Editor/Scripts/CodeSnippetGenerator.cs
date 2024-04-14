using System.Collections.Generic;
using UnityEngine;

namespace Feif.UIFramework.Editor
{
    public abstract class CodeSnippetGenerator
    {
        /// <summary>
        /// 代码生成的优先级
        /// 优先级越高越先被生成，已经生成过代码的gameObject将不再使用其他代码生成器生成
        /// </summary>
        public abstract int GetPriority();

        /// <summary>
        /// 获得这个prefab下所有需要生成代码的gameObject
        /// </summary>
        public abstract List<GameObject> GetGameObjects(GameObject prefab);

        /// <summary>
        /// 这个gameObject所生成的自定义函数代码
        /// </summary>
        public virtual List<string> GenerateFunction(GameObject gameObject)
        {
            return null;
        }

        /// <summary>
        /// 这个gameObject所生成的字段代码
        /// </summary>
        public virtual List<string> GenerateField(GameObject gameObject)
        {
            return null;
        }
    }
}