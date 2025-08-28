using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(IntNode))]
public class IntNodeView : BaseNodeView
{
    public override void Enable()
    {
        var intNode = nodeTarget as IntNode;

        // 创建类似Odin的Range滑动条（不显示输入框）
        var slider = new SliderInt
        {
            value = intNode.input,
            lowValue = 0,      // 最小值
            highValue = 30,    // 最大值
            showInputField = false,  // 不显示输入框，我们单独创建
        };
        // 设置滑动条的长度和高度
        slider.style.width = 200;
        slider.style.height = 20;
        
        // 创建滑动条和标签的水平容器
        var sliderContainer = new VisualElement();
        sliderContainer.style.flexDirection = FlexDirection.Row;
        sliderContainer.style.alignItems = Align.Center;
        sliderContainer.style.width = 250;  // 增加宽度以容纳标签
       
        // 创建最小值标签
        var minLabel = new Label(slider.lowValue.ToString());
        minLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        minLabel.style.fontSize = 10;
        minLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        minLabel.style.width = 20;
        // minLabel.style.marginRight = 5;

        

        // 创建最大值标签
        var maxLabel = new Label(slider.highValue.ToString());
        maxLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        maxLabel.style.fontSize = 10;
        maxLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        maxLabel.style.width = 20;
        maxLabel.style.marginLeft = 5;

        // 将标签和滑动条添加到水平容器
        sliderContainer.Add(minLabel);
        sliderContainer.Add(slider);
        sliderContainer.Add(maxLabel);

        intNode.onProcessed += () => {
            slider.value = intNode.input;
        };

        slider.RegisterValueChangedCallback((v) => {
            owner.RegisterCompleteObjectUndo("Updated intNode input");
            intNode.input = v.newValue;
            // 触发连接的节点立即处理
            TriggerConnectedNodesProcess(intNode);
        });

        // 添加控件到界面：滑动条容器（包含标签和滑动条）-> 输入框
        controlsContainer.Add(sliderContainer);
    }
    
    /// <summary>
    /// 触发连接到指定节点输出端口的节点立即处理
    /// </summary>
    /// <param name="node">要触发连接的节点</param>
    private void TriggerConnectedNodesProcess(BaseNode node)
    {
        //触发自己
        node.OnProcess();
        // 获取节点的所有输出端口
        foreach (var outputPort in node.outputPorts)
        {
            // 获取连接到每个输出端口的所有边
            var edges = outputPort.GetEdges();
            foreach (var edge in edges)
            {
                // 获取输入节点（连接到输出端口的节点）
                var inputNode = edge.inputNode;
                if (inputNode != null)
                {
                    // 立即触发输入节点的Process方法
                    inputNode.OnProcess();
                }
            }
        }
    }
}
