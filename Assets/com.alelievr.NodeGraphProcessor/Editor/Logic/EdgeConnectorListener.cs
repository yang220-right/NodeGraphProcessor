using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace GraphProcessor
{
    /// <summary>
    /// 基础边连接监听器类
    /// 编写您自己的边处理连接系统的基类
    /// 实现IEdgeConnectorListener接口，处理边的连接、断开和节点创建
    /// </summary>
    public class BaseEdgeConnectorListener : IEdgeConnectorListener
    {
        /// <summary>
        /// 图形视图
        /// 监听器所属的图形视图
        /// </summary>
        public readonly BaseGraphView graphView;

        /// <summary>
        /// 边输入端口映射
        /// 存储边到其输入端口的映射关系
        /// </summary>
        Dictionary< Edge, PortView >    edgeInputPorts = new Dictionary< Edge, PortView >();
        
        /// <summary>
        /// 边输出端口映射
        /// 存储边到其输出端口的映射关系
        /// </summary>
        Dictionary< Edge, PortView >    edgeOutputPorts = new Dictionary< Edge, PortView >();

        /// <summary>
        /// 边节点创建菜单窗口
        /// 静态实例，用于显示节点创建菜单
        /// </summary>
        static CreateNodeMenuWindow     edgeNodeCreateMenuWindow;

        /// <summary>
        /// 构造函数
        /// 初始化边连接监听器
        /// </summary>
        /// <param name="graphView">图形视图</param>
        public BaseEdgeConnectorListener(BaseGraphView graphView)
        {
            this.graphView = graphView;
        }

        /// <summary>
        /// 端口外拖放处理
        /// 当边被拖放到端口外部时调用
        /// </summary>
        /// <param name="edge">被拖放的边</param>
        /// <param name="position">拖放位置</param>
        public virtual void OnDropOutsidePort(Edge edge, Vector2 position)
        {
			this.graphView.RegisterCompleteObjectUndo("Disconnect edge");

			// 如果边已经存在，则移除它
			if (!edge.isGhostEdge)
				graphView.Disconnect(edge as EdgeView);

            // 当其中一个端口为空时，说明边被创建并拖放到了端口外部
            if (edge.input == null || edge.output == null)
                ShowNodeCreationMenuFromEdge(edge as EdgeView, position);
        }

        /// <summary>
        /// 拖放处理
        /// 当边被拖放到图形视图上时调用
        /// </summary>
        /// <param name="graphView">图形视图</param>
        /// <param name="edge">被拖放的边</param>
        public virtual void OnDrop(GraphView graphView, Edge edge)
        {
			var edgeView = edge as EdgeView;
            bool wasOnTheSamePort = false;

			if (edgeView?.input == null || edgeView?.output == null)
				return ;

			// 如果边被移动到另一个端口
			if (edgeView.isConnected)
			{
                if (edgeInputPorts.ContainsKey(edge) && edgeOutputPorts.ContainsKey(edge))
                    if (edgeInputPorts[edge] == edge.input && edgeOutputPorts[edge] == edge.output)
                        wasOnTheSamePort = true;

                if (!wasOnTheSamePort)
                    this.graphView.Disconnect(edgeView);
			}

            if (edgeView.input.node == null || edgeView.output.node == null)
                return;

            // 更新边的端口映射
            edgeInputPorts[edge] = edge.input as PortView;
            edgeOutputPorts[edge] = edge.output as PortView;
            
            try
            {
                this.graphView.RegisterCompleteObjectUndo("Connected " + edgeView.input.node.name + " and " + edgeView.output.node.name);
                if (!this.graphView.Connect(edge as EdgeView, autoDisconnectInputs: !wasOnTheSamePort))
                    this.graphView.Disconnect(edge as EdgeView);
            } catch (System.Exception)
            {
                this.graphView.Disconnect(edge as EdgeView);
            }
        }

        /// <summary>
        /// 从边显示节点创建菜单
        /// 当边被拖放到端口外部时显示节点创建菜单
        /// </summary>
        /// <param name="edgeView">边视图</param>
        /// <param name="position">菜单显示位置</param>
        void ShowNodeCreationMenuFromEdge(EdgeView edgeView, Vector2 position)
        {
            if (edgeNodeCreateMenuWindow == null)
                edgeNodeCreateMenuWindow = ScriptableObject.CreateInstance< CreateNodeMenuWindow >();

            edgeNodeCreateMenuWindow.Initialize(graphView, EditorWindow.focusedWindow, edgeView);
			SearchWindow.Open(new SearchWindowContext(position + EditorWindow.focusedWindow.position.position), edgeNodeCreateMenuWindow);
        }
    }
}