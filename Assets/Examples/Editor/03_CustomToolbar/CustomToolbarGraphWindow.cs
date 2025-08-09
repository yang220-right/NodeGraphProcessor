using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class CustomToolbarGraphWindow : BaseGraphWindow
{
	BaseGraph	tmpGraph;

	[MenuItem("Window/03 Custom Toolbar")]
	public static BaseGraphWindow OpenWithTmpGraph()
	{
		var graphWindow = CreateWindow< CustomToolbarGraphWindow >();

		// 当从窗口打开图形时，我们不将图形保存到磁盘
		graphWindow.tmpGraph = ScriptableObject.CreateInstance<BaseGraph>();
		graphWindow.tmpGraph.hideFlags = HideFlags.HideAndDontSave;
		graphWindow.InitializeGraph(graphWindow.tmpGraph);

		graphWindow.Show();

		return graphWindow;
	}

	protected override void OnDestroy()
	{
		graphView?.Dispose();
		DestroyImmediate(tmpGraph);
	}

	protected override void InitializeWindow(BaseGraph graph)
	{
		titleContent = new GUIContent("Custom Toolbar Graph");

		if (graphView == null)
		{
			graphView = new CustomToolbarGraphView(this);
			graphView.Add(new CustomToolbarView(graphView));
		}

		rootView.Add(graphView);
	}
}
