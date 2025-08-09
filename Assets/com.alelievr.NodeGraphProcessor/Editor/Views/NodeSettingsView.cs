using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace GraphProcessor
{
	/// <summary>
	/// 节点设置视图类
	/// 继承自VisualElement，提供节点设置的UI界面
	/// 用于显示和编辑节点的各种设置选项
	/// </summary>
	class NodeSettingsView : VisualElement
    {
        /// <summary>
        /// 内容容器
        /// 用于容纳节点设置内容的UI元素
        /// </summary>
        VisualElement m_ContentContainer;

        /// <summary>
        /// 构造函数
        /// 初始化节点设置视图
        /// </summary>
        public NodeSettingsView()
        {
            pickingMode = PickingMode.Ignore;
            styleSheets.Add(Resources.Load<StyleSheet>("GraphProcessorStyles/NodeSettings"));
            var uxml = Resources.Load<VisualTreeAsset>("UXML/NodeSettings");
            uxml.CloneTree(this);

            // 获取要用作内容容器的元素
            m_ContentContainer = this.Q("contentContainer");
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        /// <summary>
        /// 鼠标释放事件处理
        /// 阻止事件传播
        /// </summary>
        /// <param name="evt">鼠标事件</param>
        void OnMouseUp(MouseUpEvent evt)
        {
            evt.StopPropagation();
        }

        /// <summary>
        /// 鼠标按下事件处理
        /// 阻止事件传播
        /// </summary>
        /// <param name="evt">鼠标事件</param>
        void OnMouseDown(MouseDownEvent evt)
        {
            evt.StopPropagation();
        }

        /// <summary>
        /// 内容容器属性
        /// 重写contentContainer属性，返回自定义的内容容器
        /// </summary>
        public override VisualElement contentContainer
        {
            get { return m_ContentContainer; }
        }
    }
}