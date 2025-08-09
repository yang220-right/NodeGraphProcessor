using UnityEngine;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.Experimental.GraphView;

/// <summary>
/// 中继节点视图类
/// 继承自BaseNodeView，为RelayNode提供自定义的UI界面
/// 用于显示和编辑中继节点的打包/解包功能
/// </summary>
[NodeCustomEditor(typeof(RelayNode))]
public class RelayNodeView : BaseNodeView
{
	/// <summary>
	/// 中继节点
	/// 对应的中继节点对象
	/// </summary>
	RelayNode	relay => nodeTarget as RelayNode;
	
	/// <summary>
	/// 输入元素
	/// 输入端的UI元素
	/// </summary>
	VisualElement input => this.Q("input");
	
	/// <summary>
	/// 输出元素
	/// 输出端的UI元素
	/// </summary>
	VisualElement output => this.Q("output");

	/// <summary>
	/// 启用节点视图
	/// 初始化中继节点视图的UI元素和事件监听
	/// </summary>
	public override void Enable()
	{
		// 移除无用元素
		this.Q("title").RemoveFromHierarchy();
		this.Q("divider").RemoveFromHierarchy();

		// 订阅端口更新事件
		relay.onPortsUpdated += _ => UpdateSize();
	}

	/// <summary>
	/// 构建上下文菜单
	/// 添加中继节点特有的菜单选项
	/// </summary>
	/// <param name="evt">上下文菜单事件</param>
	public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
	{
		// TODO: 检查输入中是否有具有打包选项的中继节点，并切换这些选项的可见性：
		evt.menu.AppendAction("Pack Input", TogglePackInput, PackInputStatus);
		evt.menu.AppendAction("Unpack Output", ToggleUnpackOutput, UnpackOutputStatus);
		base.BuildContextualMenu(evt);
	}

	/// <summary>
	/// 切换打包输入
	/// 切换中继节点的输入打包状态
	/// </summary>
	/// <param name="action">下拉菜单动作</param>
	void TogglePackInput(DropdownMenuAction action)
	{
		relay.packInput = !relay.packInput;

		ForceUpdatePorts();
		UpdateSize();
		MarkDirtyRepaint();
	}

	/// <summary>
	/// 切换解包输出
	/// 切换中继节点的输出解包状态
	/// </summary>
	/// <param name="action">下拉菜单动作</param>
	void ToggleUnpackOutput(DropdownMenuAction action)
	{
		relay.unpackOutput = !relay.unpackOutput;

		ForceUpdatePorts();
		UpdateSize();
		MarkDirtyRepaint();
	}

	/// <summary>
	/// 打包输入状态
	/// 确定打包输入菜单项的状态
	/// </summary>
	/// <param name="action">下拉菜单动作</param>
	/// <returns>菜单项状态</returns>
	DropdownMenuAction.Status PackInputStatus(DropdownMenuAction action)
	{
		if (relay.GetNonRelayEdges().Count != 1)
			return DropdownMenuAction.Status.Disabled;

		if (relay.packInput)
			return DropdownMenuAction.Status.Checked;
		else
			return DropdownMenuAction.Status.Normal;
	}

	/// <summary>
	/// 解包输出状态
	/// 确定解包输出菜单项的状态
	/// </summary>
	/// <param name="action">下拉菜单动作</param>
	/// <returns>菜单项状态</returns>
	DropdownMenuAction.Status UnpackOutputStatus(DropdownMenuAction action)
	{
		if (relay.GetNonRelayEdges().Count == 0)
			return DropdownMenuAction.Status.Disabled;

		if (relay.unpackOutput)
			return DropdownMenuAction.Status.Checked;
		else
			return DropdownMenuAction.Status.Normal;
	}

	/// <summary>
	/// 设置位置
	/// 设置中继节点视图的位置并更新大小
	/// </summary>
	/// <param name="newPos">新的位置</param>
	public override void SetPosition(Rect newPos)
	{
		base.SetPosition(new Rect(newPos.position, new Vector2(200, 200)));
		UpdateSize();
	}

	/// <summary>
	/// 更新大小
	/// 根据中继节点的解包状态动态调整节点大小
	/// </summary>
	void UpdateSize()
	{
		if (relay.unpackOutput)
		{
			// 解包模式：根据输入边数量调整高度
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
			// 打包模式：使用固定的小尺寸
			style.height = 20;
			style.width = 50;
			if (input != null)
				input.style.height = 16;
			if (output != null)
				output.style.height = 16;
			AddToClassList("hideLabels");
		}
	}

	/// <summary>
	/// 节点移除时处理
	/// 当节点被移除时，自动连接相关的边
	/// </summary>
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

				// 自动连接输入边到所有输出边
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