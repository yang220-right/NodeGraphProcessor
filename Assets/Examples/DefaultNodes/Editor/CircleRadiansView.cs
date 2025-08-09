using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(CircleRadians))]
public class CircleRadiansView : BaseNodeView
{
	CircleRadians	node;
	VisualElement	listContainer;

	public override void Enable()
	{
		node = nodeTarget as CircleRadians;

		listContainer = new VisualElement();
        // 使用节点的变量创建字段并将它们添加到controlsContainer

		controlsContainer.Add(listContainer);
		onPortConnected += OnPortUpdate;
		onPortDisconnected += OnPortUpdate;

		UpdateOutputRadians(GetFirstPortViewFromFieldName("outputRadians").connectionCount);
	}

	void UpdateOutputRadians(int count)
	{
		node.outputRadians = new List<float>();

		listContainer.Clear();

		for (int i = 0; i < count; i++)
		{
			float r = (Mathf.PI * 2 / count) * i;
			node.outputRadians.Add(r);
			listContainer.Add(new Label(r.ToString("F3")));
		}
	}

	public void OnPortUpdate(PortView port)
	{
		// 这个节点只有一个端口，所以只能是输出
		UpdateOutputRadians(port.connectionCount);
	}
}