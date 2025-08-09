using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace GraphProcessor
{
	/// <summary>
	/// 堆栈节点视图提供者类
	/// 静态工具类，负责管理堆栈节点的自定义视图
	/// 提供堆栈节点类型到自定义视图类型的映射
	/// </summary>
	public static class StackNodeViewProvider
	{
		/// <summary>
		/// 堆栈节点视图类型映射
		/// 存储堆栈节点类型到对应自定义视图类型的映射
		/// </summary>
		static Dictionary< Type, Type >		stackNodeViewPerType = new Dictionary< Type, Type >();

        /// <summary>
        /// 静态构造函数
        /// 扫描所有带有CustomStackNodeView属性的类型并构建映射
        /// </summary>
        static StackNodeViewProvider()
        {
            // 获取所有带有CustomStackNodeView属性的类型
            foreach (var t in TypeCache.GetTypesWithAttribute<CustomStackNodeView>())
            {
                var attr = t.GetCustomAttributes(false).Select(a => a as CustomStackNodeView).FirstOrDefault();

                // 将堆栈节点类型和对应的视图类型添加到映射中
                stackNodeViewPerType.Add(attr.stackNodeType, t);
                // Debug.Log("Add " + attr.stackNodeType);
            }
        }

        /// <summary>
        /// 获取堆栈节点的自定义视图类型
        /// 根据堆栈节点类型查找对应的自定义视图类型
        /// </summary>
        /// <param name="stackNodeType">堆栈节点类型</param>
        /// <returns>对应的自定义视图类型，如果不存在则返回null</returns>
        public static Type GetStackNodeCustomViewType(Type stackNodeType)
        {
            // Debug.Log(stackNodeType);
            foreach (var t in stackNodeViewPerType)
            {
                // Debug.Log(t.Key + " -> " + t.Value);
            }
            stackNodeViewPerType.TryGetValue(stackNodeType, out var view);
            return view;
        }
    }
}