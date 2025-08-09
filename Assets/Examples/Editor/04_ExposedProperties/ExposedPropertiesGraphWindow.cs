using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class ExposedPropertiesGraphWindow : BaseGraphWindow
{
	BaseGraph	tmpGraph;

	[MenuItem("Window/04 Exposed Properties")]
	public static BaseGraphWindow OpenWithTmpGraph()
	{
		var graphWindow = CreateWindow< ExposedPropertiesGraphWindow >();

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
		titleContent = new GUIContent("Properties Graph");

		if (graphView == null)
			graphView = new ExposedPropertiesGraphView(this);

		rootView.Add(graphView);
	}

	protected override void InitializeGraphView(BaseGraphView view)
	{
		view.OpenPinned< ExposedParameterView >();
	}
}
