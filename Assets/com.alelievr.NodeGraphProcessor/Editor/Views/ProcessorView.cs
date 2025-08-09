using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GraphProcessor
{
	/// <summary>
	/// 处理器视图类
	/// 继承自PinnedElementView，提供图形处理器的UI界面
	/// 用于显示图形处理器的控制面板和执行功能
	/// </summary>
	public class ProcessorView : PinnedElementView
	{
		/// <summary>
		/// 图形处理器
		/// 用于执行图形处理的处理器实例
		/// </summary>
		BaseGraphProcessor	processor;

		/// <summary>
		/// 构造函数
		/// 初始化处理器视图
		/// </summary>
		public ProcessorView()
		{
			title = "Process panel";
		}

		/// <summary>
		/// 初始化处理器视图
		/// 设置处理器和UI元素
		/// </summary>
		/// <param name="graphView">图形视图</param>
		protected override void Initialize(BaseGraphView graphView)
		{
			// 创建图形处理器实例
			processor = new ProcessGraphProcessor(graphView.graph);

			// 订阅计算顺序更新事件
			graphView.computeOrderUpdated += processor.UpdateComputeOrder;

			// 创建播放按钮
			Button	b = new Button(OnPlay) { name = "ActionButton", text = "Play !" };

			content.Add(b);
		}

		/// <summary>
		/// 播放按钮点击处理
		/// 执行图形处理
		/// </summary>
		void OnPlay()
		{
			processor.Run();
		}
	}
}
