using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class DefaultGraphWindow : BaseGraphWindow
{
	BaseGraph	tmpGraph;

	[MenuItem("Window/01 DefaultGraph")]
	public static BaseGraphWindow OpenWithTmpGraph()
	{
		var graphWindow = CreateWindow< DefaultGraphWindow >();

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
		titleContent = new GUIContent("Default Graph");

		if (graphView == null)
			graphView = new BaseGraphView(this);

		rootView.Add(graphView);
	}
}
