using UnityEngine;
using System;
using System.Collections.Generic;

namespace GraphProcessor
{
    /// <summary>
    /// 暴露参数临时解决方案类
    /// 继承自ScriptableObject，用于解决暴露参数序列化的问题
    /// 这是一个临时的工作解决方案，用于处理暴露参数的序列化和反序列化
    /// </summary>
    [Serializable]
    public class ExposedParameterWorkaround : ScriptableObject
    {
        /// <summary>
        /// 参数列表
        /// 存储暴露参数的列表，使用SerializeReference特性确保正确的序列化
        /// </summary>
        [SerializeReference]
        public List<ExposedParameter>   parameters = new List<ExposedParameter>();
        
        /// <summary>
        /// 图形引用
        /// 关联的基础图形对象
        /// </summary>
        public BaseGraph                graph;
    }
}