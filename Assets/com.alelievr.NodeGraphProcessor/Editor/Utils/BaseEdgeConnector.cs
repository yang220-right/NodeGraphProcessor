using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;

namespace GraphProcessor
{
	/// <summary>
	/// 基础边连接器类
	/// 继承自Unity的EdgeConnector，提供边的连接和拖拽功能
	/// 负责处理端口的鼠标交互和边的创建
	/// </summary>
	public class BaseEdgeConnector : EdgeConnector
	{
		/// <summary>
		/// 边拖拽助手
		/// 处理边的拖拽操作
		/// </summary>
		protected BaseEdgeDragHelper dragHelper;
        
		/// <summary>
		/// 候选边
		/// 正在创建的边对象
		/// </summary>
        Edge edgeCandidate;
        
		/// <summary>
		/// 激活状态
		/// 标识连接器是否处于激活状态
		/// </summary>
        protected bool active;
        
		/// <summary>
		/// 鼠标按下位置
		/// 记录鼠标按下的位置
		/// </summary>
        Vector2 mouseDownPosition;
        
		/// <summary>
		/// 图形视图
		/// 连接器所属的图形视图
		/// </summary>
        protected BaseGraphView graphView;

        /// <summary>
        /// 连接距离阈值
        /// 判断是否可以建立连接的距离阈值
        /// </summary>
        internal const float k_ConnectionDistanceTreshold = 10f;

		/// <summary>
		/// 构造函数
		/// 初始化边连接器
		/// </summary>
		/// <param name="listener">边连接监听器</param>
		public BaseEdgeConnector(IEdgeConnectorListener listener) : base()
		{
            graphView = (listener as BaseEdgeConnectorListener)?.graphView;
            active = false;
            InitEdgeConnector(listener);
        }

        /// <summary>
        /// 初始化边连接器
        /// 设置拖拽助手和激活器
        /// </summary>
        /// <param name="listener">边连接监听器</param>
        protected virtual void InitEdgeConnector(IEdgeConnectorListener listener)
        {
            dragHelper = new BaseEdgeDragHelper(listener);
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

		/// <summary>
		/// 边拖拽助手属性
		/// 返回当前使用的边拖拽助手
		/// </summary>
		public override EdgeDragHelper edgeDragHelper => dragHelper;

        /// <summary>
        /// 注册目标回调
        /// 在目标元素上注册鼠标和键盘事件回调
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<MouseCaptureOutEvent>(OnCaptureOut);
        }

        /// <summary>
        /// 注销目标回调
        /// 从目标元素上注销事件回调
        /// </summary>
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        /// <summary>
        /// 鼠标按下事件处理
        /// 处理鼠标按下事件，开始边的创建过程
        /// </summary>
        /// <param name="e">鼠标事件</param>
        protected virtual void OnMouseDown(MouseDownEvent e)
        {
            if (active)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (!CanStartManipulation(e))
            {
                return;
            }

            var graphElement = target as Port;
            if (graphElement == null)
            {
                return;
            }

            mouseDownPosition = e.localMousePosition;

            // 创建候选边
            edgeCandidate = graphView != null ? graphView.CreateEdgeView() : new EdgeView();
            edgeDragHelper.draggedPort = graphElement;
            edgeDragHelper.edgeCandidate = edgeCandidate;

            if (edgeDragHelper.HandleMouseDown(e))
            {
                active = true;
                target.CaptureMouse();

                e.StopPropagation();
            }
            else
            {
                edgeDragHelper.Reset();
                edgeCandidate = null;
            }
        }

        /// <summary>
        /// 鼠标捕获丢失事件处理
        /// 当鼠标捕获丢失时中止连接操作
        /// </summary>
        /// <param name="e">鼠标捕获事件</param>
        void OnCaptureOut(MouseCaptureOutEvent e)
        {
            active = false;
            if (edgeCandidate != null)
                Abort();
        }

        /// <summary>
        /// 鼠标移动事件处理
        /// 处理鼠标移动事件，更新边的拖拽状态
        /// </summary>
        /// <param name="e">鼠标事件</param>
        protected virtual void OnMouseMove(MouseMoveEvent e)
        {
            if (!active) return;

            edgeDragHelper.HandleMouseMove(e);
            edgeCandidate.candidatePosition = e.mousePosition;
            edgeCandidate.UpdateEdgeControl();
            e.StopPropagation();
        }

        /// <summary>
        /// 鼠标释放事件处理
        /// 处理鼠标释放事件，结束边的创建过程
        /// </summary>
        /// <param name="e">鼠标事件</param>
        protected virtual void OnMouseUp(MouseUpEvent e)
        {
            if (!active || !CanStopManipulation(e))
                return;

            if (CanPerformConnection(e.localMousePosition))
                edgeDragHelper.HandleMouseUp(e);
            else
                Abort();

            active = false;
            edgeCandidate = null;
            target.ReleaseMouse();
            e.StopPropagation();
        }

        /// <summary>
        /// 键盘按下事件处理
        /// 处理键盘按下事件，中止连接操作
        /// </summary>
        /// <param name="e">键盘事件</param>
        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode != KeyCode.Escape || !active)
                return;

            Abort();

            active = false;
            target.ReleaseMouse();
            e.StopPropagation();
        }

        /// <summary>
        /// 中止连接操作
        /// 移除候选边并重置拖拽助手
        /// </summary>
        void Abort()
        {
            var graphView = target?.GetFirstAncestorOfType<GraphView>();
            graphView?.RemoveElement(edgeCandidate);

            edgeCandidate.input = null;
            edgeCandidate.output = null;
            edgeCandidate = null;

            edgeDragHelper.Reset();
        }

        /// <summary>
        /// 判断是否可以建立连接
        /// 通过比较鼠标按下位置和当前鼠标位置的距离来判断
        /// </summary>
        /// <param name="mousePosition">当前鼠标位置</param>
        /// <returns>是否可以建立连接</returns>
        bool CanPerformConnection(Vector2 mousePosition)
        {
            return Vector2.Distance(mouseDownPosition, mousePosition) > k_ConnectionDistanceTreshold;
        }
    }
}