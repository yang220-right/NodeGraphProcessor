using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GraphProcessor
{
	/// <summary>
	/// 小地图视图类
	/// 继承自Unity的MiniMap，提供图形的小地图显示功能
	/// 用于显示图形的整体视图和导航
	/// </summary>
	public class MiniMapView : MiniMap
	{
		/// <summary>
		/// 图形视图
		/// 小地图所属的图形视图
		/// </summary>
		new BaseGraphView	graphView;
		
		/// <summary>
		/// 小地图大小
		/// 小地图的尺寸
		/// </summary>
		Vector2				size;

		/// <summary>
		/// 构造函数
		/// 初始化小地图视图
		/// </summary>
		/// <param name="baseGraphView">基础图形视图</param>
		public MiniMapView(BaseGraphView baseGraphView)
		{
			this.graphView = baseGraphView;
			SetPosition(new Rect(0, 0, 100, 100));
			size = new Vector2(100, 100);
		}
	}
}