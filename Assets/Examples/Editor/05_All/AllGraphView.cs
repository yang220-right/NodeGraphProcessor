using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using GraphProcessor;
using System;
using UnityEditor;

public class AllGraphView : BaseGraphView
{
	// 目前没有特殊内容需要添加
	public AllGraphView(EditorWindow window) : base(window) {}

	public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
	{
		BuildStackNodeContextualMenu(evt);
		base.BuildContextualMenu(evt);
	}

	/// <summary>
	/// 将New Stack条目添加到上下文菜单
	/// </summary>
	/// <param name="evt"></param>
	protected void BuildStackNodeContextualMenu(ContextualMenuPopulateEvent evt)
	{
		Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
		evt.menu.AppendAction("New Stack", (e) => AddStackNode(new BaseStackNode(position)), DropdownMenuAction.AlwaysEnabled);
	}
}