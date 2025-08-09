using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using NodeGraphProcessor.Examples;

[System.Serializable, NodeMenuItem("Conditional/ForLoop")]
public class ForLoopNode : ConditionalNode
{
	[Output(name = "Loop Body")]
	public ConditionalLink		loopBody;
	
	[Output(name = "Loop Completed")]
	public ConditionalLink		loopCompleted;

	public int					start = 0;
	public int					end = 10;

	[Output]
	public int					index;

	public override string		name => "ForLoop";

	protected override void Process() => index++; // 实现影响循环内部字段的所有逻辑

	public override IEnumerable< ConditionalNode >	GetExecutedNodes() => throw new System.Exception("Do not use GetExecutedNoes in for loop to get it's dependencies");

	public IEnumerable< ConditionalNode >	GetExecutedNodesLoopBody()
	{
		// 返回连接到executes端口的所有节点
		return outputPorts.FirstOrDefault(n => n.fieldName == nameof(loopBody))
			.GetEdges().Select(e => e.inputNode as ConditionalNode);
	}

	public IEnumerable< ConditionalNode >	GetExecutedNodesLoopCompleted()
	{
		// 返回连接到executes端口的所有节点
		return outputPorts.FirstOrDefault(n => n.fieldName == nameof(loopCompleted))
			.GetEdges().Select(e => e.inputNode as ConditionalNode);
	}
}
