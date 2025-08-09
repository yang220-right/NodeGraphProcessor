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
    /// 堆栈节点视图实现，可用于在上下文中堆叠多个节点，就像VFX图形所做的那样。
    /// </summary>
    public class BaseStackNodeView : StackNode
    {
        public delegate void ReorderNodeAction(BaseNodeView nodeView, int oldIndex, int newIndex);
    
        /// <summary>
        /// 来自图形的StackNode数据
        /// </summary>
        protected internal BaseStackNode    stackNode;
        protected BaseGraphView             owner;
        readonly string                     styleSheet = "GraphProcessorStyles/BaseStackNodeView";

        /// <summary>当节点在堆栈中重新排序时触发。</summary>
        public event ReorderNodeAction      onNodeReordered;

        public BaseStackNodeView(BaseStackNode stackNode)
        {
            this.stackNode = stackNode;
            styleSheets.Add(Resources.Load<StyleSheet>(styleSheet));
        }

        /// <inheritdoc />
        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            // TODO: 为堆栈节点编写上下文菜单
        }

        /// <summary>
        /// 在StackNode添加到图形视图后调用
        /// </summary>
        public virtual void Initialize(BaseGraphView graphView)
        {
            owner = graphView;
            headerContainer.Add(new Label(stackNode.title));

            SetPosition(new Rect(stackNode.position, Vector2.one));

            InitializeInnerNodes();
        }

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

        /// <inheritdoc />
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            stackNode.position = newPos.position;
        }

        /// <inheritdoc />
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