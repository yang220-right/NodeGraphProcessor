using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace GraphProcessor
{
	/// <summary>
	/// 边视图类
	/// 继承自Unity的Edge类，提供图形中边的可视化表示
	/// 负责边的显示、交互和样式管理
	/// </summary>
	public class EdgeView : Edge
	{
		/// <summary>
		/// 连接状态
		/// 标识边是否已连接到端口
		/// </summary>
		public bool					isConnected = false;

		/// <summary>
		/// 序列化边数据
		/// 从userData中获取的序列化边对象
		/// </summary>
		public SerializableEdge		serializedEdge { get { return userData as SerializableEdge; } }

		/// <summary>
		/// 边样式表路径
		/// 边视图的样式定义文件路径
		/// </summary>
		readonly string				edgeStyle = "GraphProcessorStyles/EdgeView";

		/// <summary>
		/// 边所有者
		/// 拥有此边的图形视图
		/// </summary>
		protected BaseGraphView		owner => ((input ?? output) as PortView).owner.owner;

		/// <summary>
		/// 边视图构造函数
		/// 初始化边视图并设置样式和事件监听
		/// </summary>
		public EdgeView() : base()
		{
			styleSheets.Add(Resources.Load<StyleSheet>(edgeStyle));
			RegisterCallback<MouseDownEvent>(OnMouseDown);
		}

        /// <summary>
        /// 端口变化处理
        /// 当边的输入或输出端口发生变化时调用
        /// </summary>
        /// <param name="isInput">是否为输入端口变化</param>
        public override void OnPortChanged(bool isInput)
		{
			base.OnPortChanged(isInput);
			UpdateEdgeSize();
		}

		/// <summary>
		/// 更新边大小
		/// 根据连接的端口大小调整边的显示大小
		/// </summary>
		public void UpdateEdgeSize()
		{
			if (input == null && output == null)
				return;

			PortData inputPortData = (input as PortView)?.portData;
			PortData outputPortData = (output as PortView)?.portData;

			// 移除所有现有的边大小样式类
			for (int i = 1; i < 20; i++)
				RemoveFromClassList($"edge_{i}");
			
			// 根据端口大小设置边的大小样式
			int maxPortSize = Mathf.Max(inputPortData?.sizeInPixel ?? 0, outputPortData?.sizeInPixel ?? 0);
			if (maxPortSize > 0)
				AddToClassList($"edge_{Mathf.Max(1, maxPortSize - 6)}");
		}

		/// <summary>
		/// 自定义样式解析完成
		/// 当自定义样式解析完成时更新边控制
		/// </summary>
		/// <param name="styles">自定义样式</param>
		protected override void OnCustomStyleResolved(ICustomStyle styles)
		{
			base.OnCustomStyleResolved(styles);

			UpdateEdgeControl();
		}

		/// <summary>
		/// 鼠标按下事件处理
		/// 处理边的鼠标交互，支持双击创建中继节点
		/// </summary>
		/// <param name="e">鼠标事件</param>
		void OnMouseDown(MouseDownEvent e)
		{
			if (e.clickCount == 2)
			{
				// 经验偏移：调整鼠标位置以正确定位中继节点
				var position = e.mousePosition;
                position += new Vector2(-10f, -28);
                Vector2 mousePos = owner.ChangeCoordinatesTo(owner.contentViewContainer, position);

				// 在鼠标位置创建中继节点
				owner.AddRelayNode(input as PortView, output as PortView, mousePos);
			}
		}
	}
}