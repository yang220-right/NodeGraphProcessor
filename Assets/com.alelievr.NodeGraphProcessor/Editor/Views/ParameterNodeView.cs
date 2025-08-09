using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;

/// <summary>
/// 参数节点视图类
/// 继承自BaseNodeView，为ParameterNode提供自定义的UI界面
/// 用于显示和编辑参数节点的访问模式和参数信息
/// </summary>
[NodeCustomEditor(typeof(ParameterNode))]
public class ParameterNodeView : BaseNodeView
{
    /// <summary>
    /// 参数节点
    /// 对应的参数节点对象
    /// </summary>
    ParameterNode parameterNode;

    /// <summary>
    /// 启用节点视图
    /// 初始化参数节点视图的UI元素和事件监听
    /// </summary>
    /// <param name="fromInspector">是否来自检查器</param>
    public override void Enable(bool fromInspector = false)
    {
        parameterNode = nodeTarget as ParameterNode;

        // 创建访问模式选择器
        EnumField accessorSelector = new EnumField(parameterNode.accessor);
        accessorSelector.SetValueWithoutNotify(parameterNode.accessor);
        accessorSelector.RegisterValueChangedCallback(evt =>
        {
            parameterNode.accessor = (ParameterAccessor)evt.newValue;
            UpdatePort();
            controlsContainer.MarkDirtyRepaint();
            ForceUpdatePorts();
        });
        
        UpdatePort();
        controlsContainer.Add(accessorSelector);
        
        // 移除展开/折叠按钮
        titleContainer.Remove(titleContainer.Q("title-button-container"));
        // 从#content中移除端口
        topContainer.parent.Remove(topContainer);
        // 将端口添加到#title
        titleContainer.Add(topContainer);

        // 订阅参数变化事件
        parameterNode.onParameterChanged += UpdateView;
        UpdateView();
    }

    /// <summary>
    /// 更新视图
    /// 根据参数节点的当前状态更新视图显示
    /// </summary>
    void UpdateView()
    {
        title = parameterNode.parameter?.name;
    }
    
    /// <summary>
    /// 更新端口
    /// 根据访问模式更新端口的显示样式
    /// </summary>
    void UpdatePort()
    {
        if(parameterNode.accessor == ParameterAccessor.Set)
        {
            // 设置为输入模式时添加输入样式类
            titleContainer.AddToClassList("input");
        }
        else
        {
            // 设置为输出模式时移除输入样式类
            titleContainer.RemoveFromClassList("input");
        }
    }
}
