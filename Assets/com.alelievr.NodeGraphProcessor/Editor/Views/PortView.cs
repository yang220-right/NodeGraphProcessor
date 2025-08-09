using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace GraphProcessor
{
	/// <summary>
	/// 端口视图类
	/// 继承自Unity的Port类，提供图形中端口的可视化表示
	/// 负责端口的显示、连接管理和用户交互
	/// </summary>
	public class PortView : Port
	{
		/// <summary>
		/// 字段名称
		/// 端口对应的字段名
		/// </summary>
		public string				fieldName => fieldInfo.Name;
		
		/// <summary>
		/// 字段类型
		/// 端口对应的字段类型
		/// </summary>
		public Type					fieldType => fieldInfo.FieldType;
		
		/// <summary>
		/// 端口类型
		/// 端口的显示类型，可能与字段类型不同
		/// </summary>
		public new Type				portType;
        
		/// <summary>
		/// 端口所有者
		/// 拥有此端口的节点视图
		/// </summary>
        public BaseNodeView     	owner { get; private set; }
		
		/// <summary>
		/// 端口数据
		/// 包含端口的配置和显示信息
		/// </summary>
		public PortData				portData;

		/// <summary>
		/// 端口连接事件
		/// 当端口被连接时触发
		/// </summary>
		public event Action< PortView, Edge >	OnConnected;
		
		/// <summary>
		/// 端口断开连接事件
		/// 当端口断开连接时触发
		/// </summary>
		public event Action< PortView, Edge >	OnDisconnected;

		/// <summary>
		/// 字段信息
		/// 通过反射获取的字段信息
		/// </summary>
		protected FieldInfo		fieldInfo;
		
		/// <summary>
		/// 边连接监听器
		/// 处理边的连接和断开逻辑
		/// </summary>
		protected BaseEdgeConnectorListener	listener;

		/// <summary>
		/// 用户端口样式文件
		/// 自定义端口样式的文件名
		/// </summary>
		string userPortStyleFile = "PortViewTypes";

		/// <summary>
		/// 连接的边列表
		/// 存储与此端口连接的所有边
		/// </summary>
		List< EdgeView >		edges = new List< EdgeView >();

		/// <summary>
		/// 连接数量
		/// 当前端口连接的边数量
		/// </summary>
		public int connectionCount => edges.Count;

		/// <summary>
		/// 端口样式表路径
		/// 默认端口样式的路径
		/// </summary>
		readonly string portStyle = "GraphProcessorStyles/PortView";

        /// <summary>
        /// 端口视图构造函数
        /// 创建新的端口视图实例
        /// </summary>
        /// <param name="direction">端口方向（输入/输出）</param>
        /// <param name="fieldInfo">字段信息</param>
        /// <param name="portData">端口数据</param>
        /// <param name="edgeConnectorListener">边连接监听器</param>
        protected PortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
            : base(portData.vertical ? Orientation.Vertical : Orientation.Horizontal, direction, Capacity.Multi, portData.displayType ?? fieldInfo.FieldType)
		{
			this.fieldInfo = fieldInfo;
			this.listener = edgeConnectorListener;
			this.portType = portData.displayType ?? fieldInfo.FieldType;
			this.portData = portData;
			this.portName = fieldName;

			// 添加默认样式表
			styleSheets.Add(Resources.Load<StyleSheet>(portStyle));

			UpdatePortSize();

			// 添加用户自定义样式表
			var userPortStyle = Resources.Load<StyleSheet>(userPortStyleFile);
			if (userPortStyle != null)
				styleSheets.Add(userPortStyle);
			
			// 如果是垂直端口，添加垂直样式类
			if (portData.vertical)
				AddToClassList("Vertical");
			
			this.tooltip = portData.tooltip;
		}

		/// <summary>
		/// 创建端口视图
		/// 静态工厂方法，创建并配置端口视图
		/// </summary>
		/// <param name="direction">端口方向</param>
		/// <param name="fieldInfo">字段信息</param>
		/// <param name="portData">端口数据</param>
		/// <param name="edgeConnectorListener">边连接监听器</param>
		/// <returns>配置好的端口视图</returns>
		public static PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
		{
			var pv = new PortView(direction, fieldInfo, portData, edgeConnectorListener);
			pv.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
			pv.AddManipulator(pv.m_EdgeConnector);

			// 强制在端口标签中选择以扩大边创建区域
			var portLabel = pv.Q("type");
			if (portLabel != null)
			{
				portLabel.pickingMode = PickingMode.Position;
				portLabel.style.flexGrow = 1;
			}

			// 当端口垂直时隐藏标签
			if (portData.vertical && portLabel != null)
				portLabel.style.display = DisplayStyle.None;
			
			// 修复垂直顶部端口的拾取模式
			if (portData.vertical)
				pv.Q("connector").pickingMode = PickingMode.Position;

			return pv;
		}

		/// <summary>
		/// 更新端口视图的大小
		/// 使用portData.sizeInPixel属性设置端口大小
		/// </summary>
		public void UpdatePortSize()
		{
			int size = portData.sizeInPixel == 0 ? 8 : portData.sizeInPixel;
			var connector = this.Q("connector");
			var cap = connector.Q("cap");
			connector.style.width = size;
			connector.style.height = size;
			cap.style.width = size - 4;
			cap.style.height = size - 4;

			// 更新连接的边大小
			edges.ForEach(e => e.UpdateEdgeSize());
		}

		/// <summary>
		/// 初始化端口视图
		/// 设置端口的所有者节点视图
		/// </summary>
		/// <param name="nodeView">节点视图</param>
		/// <param name="name">端口名称</param>
		public virtual void Initialize(BaseNodeView nodeView, string name)
		{
			this.owner = nodeView;
			AddToClassList(fieldName);

			// 如果端口接受多个值（因此是容器），则更正端口类型
			if (direction == Direction.Input && portData.acceptMultipleEdges && portType == fieldType) // 如果用户没有设置自定义字段类型
			{
				if (fieldType.GetGenericArguments().Length > 0)
					portType = fieldType.GetGenericArguments()[0];
			}

			if (name != null)
				portName = name;
			visualClass = "Port_" + portType.Name;
			tooltip = portData.tooltip;
		}

		public override void Connect(Edge edge)
		{
			OnConnected?.Invoke(this, edge);

			base.Connect(edge);

			var inputNode = (edge.input as PortView).owner;
			var outputNode = (edge.output as PortView).owner;

			edges.Add(edge as EdgeView);

			inputNode.OnPortConnected(edge.input as PortView);
			outputNode.OnPortConnected(edge.output as PortView);
		}

		public override void Disconnect(Edge edge)
		{
			OnDisconnected?.Invoke(this, edge);

			base.Disconnect(edge);

			if (!(edge as EdgeView).isConnected)
				return ;

			var inputNode = (edge.input as PortView)?.owner;
			var outputNode = (edge.output as PortView)?.owner;

			inputNode?.OnPortDisconnected(edge.input as PortView);
			outputNode?.OnPortDisconnected(edge.output as PortView);

			edges.Remove(edge as EdgeView);
		}

		public void UpdatePortView(PortData data)
		{
			if (data.displayType != null)
			{
				base.portType = data.displayType;
				portType = data.displayType;
				visualClass = "Port_" + portType.Name;
			}
			if (!String.IsNullOrEmpty(data.displayName))
				base.portName = data.displayName;

			portData = data;

			// 如果端口颜色已更改，则更新边
			schedule.Execute(() => {
				foreach (var edge in edges)
				{
					edge.UpdateEdgeControl();
					edge.MarkDirtyRepaint();
				}
			}).ExecuteLater(50); // 嗯

			UpdatePortSize();
		}

		public List< EdgeView >	GetEdges()
		{
			return edges;
		}
	}
}