using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(IntNode))]
public class IntNodeView : BaseNodeView
{
    public override void Enable()
    {
        var intNode = nodeTarget as IntNode;

        // 设置节点视图的整体宽度
        style.width = 150f;
        
        // 尝试多种方式加载CustomSlider.uxml模板
        VisualTreeAsset tpl = null;
        // 方法3: 如果方法2失败，尝试从AssetDatabase加载
        #if UNITY_EDITOR
        var assetPath = "Assets/Tests/CSS/CustomSlider.uxml";
        tpl = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
        #endif
        
        if (tpl != null)
        {
            // 实例化UXML模板
            var customSliderElement = tpl.Instantiate();
            // 查找模板中的滑动条控件
            var customSlider = customSliderElement.Q<SliderInt>();
            var min = customSliderElement.Q<Label>("min");
            var max = customSliderElement.Q<Label>("max");
            min.text = "0";
            max.text = "30";
            // 配置滑动条
            customSlider.value = intNode.input;
            customSlider.lowValue = 0;
            customSlider.highValue = 30;
            // 注册值变化回调
            customSlider.RegisterValueChangedCallback((v) => {
                owner.RegisterCompleteObjectUndo("Updated intNode input");
                intNode.input = v.newValue;
                // 触发连接的节点立即处理
                TriggerConnectedNodesProcess(intNode);
            });
            // 注册处理完成回调
            intNode.onProcessed += () => {
                customSlider.value = intNode.input;
            };
            // 将自定义滑动条添加到控制容器
            controlsContainer.Add(customSliderElement);
        }
    }
    
    /// <summary>
    /// 创建默认滑动条作为回退方案
    /// </summary>
    /// <param name="intNode">IntNode实例</param>
    private void CreateDefaultSlider(IntNode intNode)
    {
        var slider = new SliderInt
        {
            value = intNode.input,
            lowValue = 0,
            highValue = 30,
            showInputField = false,
            label = ""
        };
        
        slider.style.width = 200;
        slider.style.height = 20;
        
        slider.RegisterValueChangedCallback((v) => {
            owner.RegisterCompleteObjectUndo("Updated intNode input");
            intNode.input = v.newValue;
            TriggerConnectedNodesProcess(intNode);
        });
        
        intNode.onProcessed += () => {
            slider.value = intNode.input;
        };
        
        controlsContainer.Add(slider);
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
