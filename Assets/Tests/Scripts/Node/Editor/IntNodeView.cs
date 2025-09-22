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
        VisualTreeAsset tpl = null;
        var assetPath = "Assets/Tests/CSS/CustomSlider.uxml";
        tpl = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
        if (tpl != null)
        {
            var customSliderElement = tpl.Instantiate();
            // 查找模板中的滑动条控件
            var customSlider = customSliderElement.Q<SliderInt>();
            var min = customSliderElement.Q<Label>("min");
            var max = customSliderElement.Q<Label>("max");
            min.text = "0";
            max.text = "30";
            customSlider.value = intNode.input;
            customSlider.lowValue = 0;
            customSlider.highValue = 30;
            customSlider.RegisterValueChangedCallback((v) => {
                owner.RegisterCompleteObjectUndo("Updated intNode input");
                intNode.input = v.newValue;
                TriggerConnectedNodesProcess(intNode);
            });
            intNode.onProcessed += () => {
                customSlider.value = intNode.input;
            };
            controlsContainer.Add(customSliderElement);
        }
    }
    private void TriggerConnectedNodesProcess(BaseNode node)
    {
        node.OnProcess();
        foreach (var outputPort in node.outputPorts)
        {
            var edges = outputPort.GetEdges();
            foreach (var edge in edges)
            {
                var inputNode = edge.inputNode;
                if (inputNode != null)
                    inputNode.OnProcess();
            }
        }
    }
}
