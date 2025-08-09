using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace GraphProcessor
{
    /// <summary>
    /// 基础堆栈节点视图类
    /// 堆栈节点视图实现，可用于在上下文中堆叠多个节点，就像VFX图形所做的那样
    /// 继承自Unity的StackNode，提供节点的堆叠和组织功能
    /// </summary>
    public class BaseStackNodeView : StackNode
    {
        /// <summary>
        /// 节点重新排序委托
        /// 当节点在堆栈中重新排序时触发的回调
        /// </summary>
        /// <param name="nodeView">被重新排序的节点视图</param>
        /// <param name="oldIndex">旧的索引位置</param>
        /// <param name="newIndex">新的索引位置</param>
        public delegate void ReorderNodeAction(BaseNodeView nodeView, int oldIndex, int newIndex);
    
        /// <summary>
        /// 来自图形的StackNode数据
        /// 对应的堆栈节点数据对象
        /// </summary>
        protected internal BaseStackNode    stackNode;
        
        /// <summary>
        /// 所有者图形视图
        /// 堆栈节点视图所属的图形视图
        /// </summary>
        protected BaseGraphView             owner;
        
        /// <summary>
        /// 样式表路径
        /// 堆栈节点视图的样式定义文件路径
        /// </summary>
        readonly string                     styleSheet = "GraphProcessorStyles/BaseStackNodeView";

        /// <summary>
        /// 节点重新排序事件
        /// 当节点在堆栈中重新排序时触发
        /// </summary>
        public event ReorderNodeAction      onNodeReordered;

        /// <summary>
        /// 构造函数
        /// 创建新的堆栈节点视图
        /// </summary>
        /// <param name="stackNode">堆栈节点数据</param>
        public BaseStackNodeView(BaseStackNode stackNode)
        {
            this.stackNode = stackNode;
            styleSheets.Add(Resources.Load<StyleSheet>(styleSheet));
        }

        /// <summary>
        /// 分隔符上下文菜单事件处理
        /// 当分隔符的上下文菜单被触发时调用
        /// </summary>
        /// <param name="evt">上下文菜单事件</param>
        /// <param name="separatorIndex">分隔符索引</param>
        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            // TODO: 为堆栈节点编写上下文菜单
        }

        /// <summary>
        /// 初始化堆栈节点视图
        /// 在StackNode添加到图形视图后调用
        /// </summary>
        /// <param name="graphView">图形视图</param>
        public virtual void Initialize(BaseGraphView graphView)
        {
            owner = graphView;
            headerContainer.Add(new Label(stackNode.title));

            SetPosition(new Rect(stackNode.position, Vector2.one));

            InitializeInnerNodes();
        }

        /// <summary>
        /// 初始化内部节点
        /// 根据堆栈节点数据中的GUID列表添加节点到堆栈中
        /// </summary>
        void InitializeInnerNodes()
        {
            int i = 0;
            // 清理GUID列表，以防某些节点被移除
            stackNode.nodeGUIDs.RemoveAll(nodeGUID =>
            {
                if (owner.graph.nodesPerGUID.ContainsKey(nodeGUID))
                {
                    var node = owner.graph.nodesPerGUID[nodeGUID];
                    var view = owner.nodeViewsPerNode[node];
                    view.AddToClassList("stack-child__" + i);
                    i++;
                    AddElement(view);
                    return false;
                }
                else
                {
                    return true; // 移除条目，因为GUID不再存在
                }
            });
        }

        /// <summary>
        /// 设置位置
        /// 设置堆栈节点视图的位置并更新数据
        /// </summary>
        /// <param name="newPos">新的位置</param>
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            stackNode.position = newPos.position;
        }

        /// <summary>
        /// 接受元素
        /// 判断是否接受元素添加到堆栈中，并处理节点的重新排序
        /// </summary>
        /// <param name="element">要添加的元素</param>
        /// <param name="proposedIndex">建议的索引位置</param>
        /// <param name="maxIndex">最大索引</param>
        /// <returns>是否接受元素</returns>
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            bool accept = base.AcceptsElement(element, ref proposedIndex, maxIndex);

            if (accept && element is BaseNodeView nodeView)
            {
                var index = Mathf.Clamp(proposedIndex, 0, stackNode.nodeGUIDs.Count - 1);

                int oldIndex = stackNode.nodeGUIDs.FindIndex(g => g == nodeView.nodeTarget.GUID);
                if (oldIndex != -1)
                {
                    stackNode.nodeGUIDs.Remove(nodeView.nodeTarget.GUID);
                    if (oldIndex != index)
                        onNodeReordered?.Invoke(nodeView, oldIndex, index);
                }

                stackNode.nodeGUIDs.Insert(index, nodeView.nodeTarget.GUID);
            }

            return accept;
        }

        public override bool DragLeave(DragLeaveEvent evt, IEnumerable<ISelectable> selection, IDropTarget leftTarget, ISelection dragSource)
        {
            foreach (var elem in selection)
            {
                if (elem is BaseNodeView nodeView)
                    stackNode.nodeGUIDs.Remove(nodeView.nodeTarget.GUID);
            }
            return base.DragLeave(evt, selection, leftTarget, dragSource);
        }
    }
}