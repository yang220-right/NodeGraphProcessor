using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	/// <summary>
	/// 复制粘贴助手类
	/// 用于管理图形元素的复制和粘贴操作
	/// 存储复制的节点、组和边的数据
	/// </summary>
	[System.Serializable]
	public class CopyPasteHelper
	{
		/// <summary>
		/// 复制的节点列表
		/// 存储被复制节点的JSON序列化数据
		/// </summary>
		public List< JsonElement >	copiedNodes = new List< JsonElement >();

		/// <summary>
		/// 复制的组列表
		/// 存储被复制组的JSON序列化数据
		/// </summary>
		public List< JsonElement >	copiedGroups = new List< JsonElement >();
	
		/// <summary>
		/// 复制的边列表
		/// 存储被复制边的JSON序列化数据
		/// </summary>
		public List< JsonElement >	copiedEdges = new List< JsonElement >();
	}
}