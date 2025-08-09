using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class CustomContextMenuGraphWindow : BaseGraphWindow
{
	BaseGraph	tmpGraph;

	[MenuItem("Window/02 Custom Context Menu")]
	public static BaseGraphWindow OpenWithTmpGraph()
	{
		var graphWindow = CreateWindow< CustomContextMenuGraphWindow >();

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
		titleContent = new GUIContent("Context Menu Graph");

		if (graphView == null)
		{
			graphView = new CustomContextMenuGraphView(this);
			graphView.Add(new MiniMapView(graphView));
		}

		rootView.Add(graphView);
	}
}
