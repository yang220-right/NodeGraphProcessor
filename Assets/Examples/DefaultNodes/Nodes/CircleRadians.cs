using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/CircleRadians")]
public class CircleRadians : BaseNode
{
	[Output(name = "In")]
    public List< float >		outputRadians;

	public override string		name => "CircleRadians";

	[CustomPortOutput(nameof(outputRadians), typeof(float))]
	public void PushOutputRadians(List< SerializableEdge > connectedEdges)
	{
		int i = 0;

		// outputRadians应该匹配connectedEdges长度，列表由编辑器函数生成

		foreach (var edge in connectedEdges)
		{
			edge.passThroughBuffer = outputRadians[i];
			i++;
		}
	}
}
