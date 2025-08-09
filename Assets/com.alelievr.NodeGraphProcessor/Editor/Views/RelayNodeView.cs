using UnityEngine;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.Experimental.GraphView;

[NodeCustomEditor(typeof(RelayNode))]
public class RelayNodeView : BaseNodeView
{
	RelayNode	relay => nodeTarget as RelayNode;
	VisualElement input => this.Q("input");
	VisualElement output => this.Q("output");

	public override void Enable()
	{
		// 移除无用元素
		this.Q("title").RemoveFromHierarchy();
		this.Q("divider").RemoveFromHierarchy();

		relay.onPortsUpdated += _ => UpdateSize();
	}

	public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
	{
		// TODO: 检查输入中是否有具有打包选项的中继节点，并切换这些选项的可见性：
		evt.menu.AppendAction("Pack Input", TogglePackInput, PackInputStatus);
		evt.menu.AppendAction("Unpack Output", ToggleUnpackOutput, UnpackOutputStatus);
		base.BuildContextualMenu(evt);
	}

	void TogglePackInput(DropdownMenuAction action)
	{
		relay.packInput = !relay.packInput;

		ForceUpdatePorts();
		UpdateSize();
		MarkDirtyRepaint();
	}

	void ToggleUnpackOutput(DropdownMenuAction action)
	{
		relay.unpackOutput = !relay.unpackOutput;

		ForceUpdatePorts();
		UpdateSize();
		MarkDirtyRepaint();
	}

	DropdownMenuAction.Status PackInputStatus(DropdownMenuAction action)
	{
		if (relay.GetNonRelayEdges().Count != 1)
			return DropdownMenuAction.Status.Disabled;

		if (relay.packInput)
			return DropdownMenuAction.Status.Checked;
		else
			return DropdownMenuAction.Status.Normal;
	}

	DropdownMenuAction.Status UnpackOutputStatus(DropdownMenuAction action)
	{
		if (relay.GetNonRelayEdges().Count == 0)
			return DropdownMenuAction.Status.Disabled;

		if (relay.unpackOutput)
			return DropdownMenuAction.Status.Checked;
		else
			return DropdownMenuAction.Status.Normal;
	}

	public override void SetPosition(Rect newPos)
	{
		base.SetPosition(new Rect(newPos.position, new Vector2(200, 200)));
		UpdateSize();
	}

	void UpdateSize()
	{
		if (relay.unpackOutput)
		{
			int inputEdgeCount = relay.GetNonRelayEdges().Count + 1;
			style.height = Mathf.Max(30, 24 * inputEdgeCount + 5);
			style.width = -1;
			if (input != null)
				input.style.height = -1;
			if (output != null)
				output.style.height = -1;
			RemoveFromClassList("hideLabels");
		}
		else
		{
			style.height = 20;
			style.width = 50;
			if (input != null)
				input.style.height = 16;
			if (output != null)
				output.style.height = 16;
			AddToClassList("hideLabels");
		}
	}

	public override void OnRemoved()
	{
		// 我们延迟边的连接，以防我们要连接的节点发生什么事情
		// 即多个中继节点删除
		schedule.Execute(() => {
			if (!relay.unpackOutput)
			{
				var inputEdges = inputPortViews[0].GetEdges();
				var outputEdges = outputPortViews[0].GetEdges();

				if (inputEdges.Count == 0 || outputEdges.Count == 0)
					return;

				var inputEdge = inputEdges.First();

				foreach (var outputEdge in outputEdges.ToList())
				{
					var input = outputEdge.input as PortView;
					var output = inputEdge.output as PortView;

					owner.Connect(input, output);
				}
			}
		}).ExecuteLater(1);
	}
}