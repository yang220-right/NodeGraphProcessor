using UnityEngine.UIElements;
using UnityEngine;
using System;

namespace GraphProcessor
{
	/// <summary>
	/// 固定元素类
	/// 覆盖图形的元素，如黑板、面板等
	/// 提供在图形上显示额外UI元素的功能
	/// </summary>
	[System.Serializable]
	public class PinnedElement
	{
		/// <summary>
		/// 默认大小
		/// 固定元素的默认尺寸
		/// </summary>
		public static readonly Vector2	defaultSize = new Vector2(150, 200);

		/// <summary>
		/// 元素位置和大小
		/// 固定元素在图形中的位置和尺寸
		/// </summary>
		public Rect				position = new Rect(Vector2.zero, defaultSize);
		
		/// <summary>
		/// 是否打开
		/// 控制固定元素的显示状态
		/// </summary>
		public bool				opened = true;
		
		/// <summary>
		/// 编辑器类型
		/// 固定元素对应的编辑器类型
		/// </summary>
		public SerializableType	editorType;

		/// <summary>
		/// 构造函数
		/// 创建新的固定元素
		/// </summary>
		/// <param name="editorType">编辑器类型</param>
		public PinnedElement(Type editorType)
		{
			this.editorType = new SerializableType(editorType);
		}
	}
}