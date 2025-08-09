using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace GraphProcessor
{
	/// <summary>
	/// 节点自定义编辑器属性
	/// 用于为特定类型的节点指定自定义编辑器
	/// 允许为节点类型创建专门的编辑器界面
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class NodeCustomEditor : Attribute
	{
		/// <summary>
		/// 节点类型
		/// 指定此自定义编辑器适用的节点类型
		/// </summary>
		public Type nodeType;

		/// <summary>
		/// 构造函数
		/// 创建节点自定义编辑器属性
		/// </summary>
		/// <param name="nodeType">节点类型</param>
		public NodeCustomEditor(Type nodeType)
		{
			this.nodeType = nodeType;
		}
	}
}