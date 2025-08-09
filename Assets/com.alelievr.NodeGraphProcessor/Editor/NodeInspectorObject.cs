using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace GraphProcessor
{
    /// <summary>
    /// 节点检查器对象编辑器
    /// 节点检查器的自定义编辑器，您可以继承此类来自定义节点检查器
    /// 负责在检查器中显示选中节点的属性和设置
    /// </summary>
    [CustomEditor(typeof(NodeInspectorObject))]
    public class NodeInspectorObjectEditor : Editor
    {
        /// <summary>
        /// 节点检查器对象
        /// 被编辑的目标对象
        /// </summary>
        NodeInspectorObject inspector;
        
        /// <summary>
        /// 根UI元素
        /// 检查器界面的根容器
        /// </summary>
        protected VisualElement root;
        
        /// <summary>
        /// 选中节点列表容器
        /// 用于显示所有选中节点的UI元素
        /// </summary>
        protected VisualElement selectedNodeList;
        
        /// <summary>
        /// 占位符元素
        /// 当没有选中节点时显示的提示信息
        /// </summary>
        protected VisualElement placeholder;

        /// <summary>
        /// 启用时的初始化
        /// 设置事件监听和UI元素
        /// </summary>
        protected virtual void OnEnable()
        {
            inspector = target as NodeInspectorObject;
            inspector.nodeSelectionUpdated += UpdateNodeInspectorList;
            root = new VisualElement();
            selectedNodeList = new VisualElement();
            selectedNodeList.styleSheets.Add(Resources.Load<StyleSheet>("GraphProcessorStyles/InspectorView"));
            root.Add(selectedNodeList);
            placeholder = new Label("Select a node to show it's settings in the inspector");
            placeholder.AddToClassList("PlaceHolder");
            UpdateNodeInspectorList();
        }

        /// <summary>
        /// 禁用时的清理
        /// 移除事件监听
        /// </summary>
        protected virtual void OnDisable()
        {
            inspector.nodeSelectionUpdated -= UpdateNodeInspectorList;
        }

        /// <summary>
        /// 创建检查器GUI
        /// 返回检查器的根UI元素
        /// </summary>
        /// <returns>检查器的根UI元素</returns>
        public override VisualElement CreateInspectorGUI() => root;

        /// <summary>
        /// 更新节点检查器列表
        /// 根据选中的节点更新检查器界面
        /// </summary>
        protected virtual void UpdateNodeInspectorList()
        {
            selectedNodeList.Clear();

            if (inspector.selectedNodes.Count == 0)
                selectedNodeList.Add(placeholder);

            foreach (var nodeView in inspector.selectedNodes)
                selectedNodeList.Add(CreateNodeBlock(nodeView));
        }

        /// <summary>
        /// 创建节点块
        /// 为单个节点创建检查器UI块
        /// </summary>
        /// <param name="nodeView">节点视图</param>
        /// <returns>节点的UI块</returns>
        protected VisualElement CreateNodeBlock(BaseNodeView nodeView)
        {
            var view = new VisualElement();

            view.Add(new Label(nodeView.nodeTarget.name));

            var tmp = nodeView.controlsContainer;
            nodeView.controlsContainer = view;
            nodeView.Enable(true);
            nodeView.controlsContainer.AddToClassList("NodeControls");
            var block = nodeView.controlsContainer;
            nodeView.controlsContainer = tmp;
            
            return block;
        }
    }

    /// <summary>
    /// 节点检查器对象
    /// 您可以继承此类来自定义节点检查器
    /// 管理选中节点的状态和检查器界面
    /// </summary>
    public class NodeInspectorObject : ScriptableObject
    {
        /// <summary>
        /// 检查器之前选择的对象
        /// 用于跟踪选择状态的变化
        /// </summary>
        public Object previouslySelectedObject;
        
        /// <summary>
        /// 当前选择的节点列表
        /// 存储所有当前选中的节点视图
        /// </summary>
        public HashSet<BaseNodeView> selectedNodes { get; private set; } = new HashSet<BaseNodeView>();

        /// <summary>
        /// 节点选择更新事件
        /// 当选择更新时触发
        /// </summary>
        public event Action nodeSelectionUpdated;

        /// <summary>
        /// 从图形更新选择
        /// 更新当前选中的节点列表
        /// </summary>
        /// <param name="views">新的选中节点视图集合</param>
        public virtual void UpdateSelectedNodes(HashSet<BaseNodeView> views)
        {
            selectedNodes = views;
            nodeSelectionUpdated?.Invoke();
        }

        /// <summary>
        /// 刷新节点
        /// 触发节点选择更新事件以刷新界面
        /// </summary>
        public virtual void RefreshNodes() => nodeSelectionUpdated?.Invoke();

        /// <summary>
        /// 节点视图移除
        /// 当节点视图被移除时从选中列表中移除
        /// </summary>
        /// <param name="view">被移除的节点视图</param>
        public virtual void NodeViewRemoved(BaseNodeView view)
        {
            selectedNodes.Remove(view);
            nodeSelectionUpdated?.Invoke();
        }
    }
}