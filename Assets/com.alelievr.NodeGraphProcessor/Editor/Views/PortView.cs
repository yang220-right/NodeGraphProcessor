using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace GraphProcessor
{
	public class PortView : Port
	{
		public string				fieldName => fieldInfo.Name;
		public Type					fieldType => fieldInfo.FieldType;
		public new Type				portType;
        public BaseNodeView     	owner { get; private set; }
		public PortData				portData;

		public event Action< PortView, Edge >	OnConnected;
		public event Action< PortView, Edge >	OnDisconnected;

		protected FieldInfo		fieldInfo;
		protected BaseEdgeConnectorListener	listener;

		string userPortStyleFile = "PortViewTypes";

		List< EdgeView >		edges = new List< EdgeView >();

		public int connectionCount => edges.Count;

		readonly string portStyle = "GraphProcessorStyles/PortView";

        protected PortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
            : base(portData.vertical ? Orientation.Vertical : Orientation.Horizontal, direction, Capacity.Multi, portData.displayType ?? fieldInfo.FieldType)
		{
			this.fieldInfo = fieldInfo;
			this.listener = edgeConnectorListener;
			this.portType = portData.displayType ?? fieldInfo.FieldType;
			this.portData = portData;
			this.portName = fieldName;

			styleSheets.Add(Resources.Load<StyleSheet>(portStyle));

			UpdatePortSize();

			var userPortStyle = Resources.Load<StyleSheet>(userPortStyleFile);
			if (userPortStyle != null)
				styleSheets.Add(userPortStyle);
			
			if (portData.vertical)
				AddToClassList("Vertical");
			
			this.tooltip = portData.tooltip;
		}

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
		/// 更新端口视图的大小（使用portData.sizeInPixel属性）
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

			// 更新连接的边大小：
			edges.ForEach(e => e.UpdateEdgeSize());
		}

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