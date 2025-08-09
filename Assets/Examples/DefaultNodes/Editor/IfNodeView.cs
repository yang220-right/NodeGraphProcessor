using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(IfNode))]
public class IfNodeView : BaseNodeView
{
	public override void Enable()
	{
		hasSettings = true;	// 或 base.Enable();
		var node = nodeTarget as IfNode;

        // 使用节点的变量创建字段并将它们添加到controlsContainer

		controlsContainer.Add(new Label($"Last Evaluation: {node.condition}"));
	}
}