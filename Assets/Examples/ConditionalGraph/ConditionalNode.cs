using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using UnityEngine;

namespace NodeGraphProcessor.Examples
{
	[System.Serializable]
	/// <summary>
	/// 这是由条件处理器执行的每个节点的基类，它接受一个executed布尔值作为输入
	/// </summary>
	public abstract class ConditionalNode : BaseNode, IConditionalNode
	{
		// 这些布尔值将控制后续节点的执行是否完成或被丢弃。
		[Input(name = "Executed", allowMultiple = true)]
		public ConditionalLink	executed;

		public abstract IEnumerable< ConditionalNode >	GetExecutedNodes();

		// 确保executed字段始终位于节点端口部分的顶部
		public override FieldInfo[] GetNodeFields()
		{
			var fields = base.GetNodeFields();
			Array.Sort(fields, (f1, f2) => f1.Name == nameof(executed) ? -1 : 1);
			return fields;
		}
	}

	[System.Serializable]
	/// <summary>
	/// 这个类表示一个简单的节点，它接受一个事件作为参数并将其传递给下一个节点
	/// </summary>
	public abstract class LinearConditionalNode : ConditionalNode, IConditionalNode
	{
		[Output(name = "Executes")]
		public ConditionalLink	executes;

		public override IEnumerable< ConditionalNode >	GetExecutedNodes()
		{
			// 返回连接到executes端口的所有节点
			return outputPorts.FirstOrDefault(n => n.fieldName == nameof(executes))
				.GetEdges().Select(e => e.inputNode as ConditionalNode);
		}
	}
	
	[System.Serializable]
	/// <summary>
	/// 这个类表示一个可等待节点，它在时间/帧后调用另一个节点
	/// </summary>
	public abstract class WaitableNode : LinearConditionalNode
	{
		[Output(name = "Execute After")]
		public ConditionalLink executeAfter;

		protected void ProcessFinished()
		{
			onProcessFinished.Invoke(this);
		}

		[HideInInspector]
		public Action<WaitableNode> onProcessFinished;

		public IEnumerable< ConditionalNode > GetExecuteAfterNodes()
		{
			return outputPorts.FirstOrDefault(n => n.fieldName == nameof(executeAfter))
			                  .GetEdges().Select(e => e.inputNode as ConditionalNode);
		}
	}
}