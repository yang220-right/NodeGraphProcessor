using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(SwitchNode))]
public class SwitchNodeView : BaseNodeView
{
	public override void Enable()
	{
		var node = nodeTarget as SwitchNode;

        // 使用节点的变量创建字段并将它们添加到controlsContainer

		controlsContainer.Add(new Label("Hello World !"));
	}
}