using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

[System.Serializable, NodeMenuItem("Utils/Relay")]
public class RelayNode : BaseNode
{
	const string packIdentifier = "_Pack";

	[HideInInspector]
	public struct PackedRelayData
	{
		public List<object>	values;
		public List<string>	names;
		public List<Type>	types;
	}

	[Input(name = "In")]
    public PackedRelayData	input;

	[Output(name = "Out")]
	public PackedRelayData	output;

	public bool		unpackOutput = false;
	public bool		packInput = false;
	public int		inputEdgeCount = 0;
	[System.NonSerialized]
	int				outputIndex = 0;

	SerializableType inputType = new SerializableType(typeof(object));

	const int		k_MaxPortSize = 14;

	protected override void Process()
	{
		outputIndex = 0;
		output = input;
	}

	public override string layoutStyle => "GraphProcessorStyles/RelayNode";

	[CustomPortInput(nameof(input), typeof(object), true)]
	public void GetInputs(List< SerializableEdge > edges)
	{
		inputEdgeCount = edges.Count;

		// 如果中继节点仅连接到另一个中继节点：
		if (edges.Count == 1 && edges.First().outputNode.GetType() == typeof(RelayNode))
		{
			if (edges.First().passThroughBuffer != null)
				input = (PackedRelayData)edges.First().passThroughBuffer;
		}
		else
		{
			input.values = edges.Select(e => e.passThroughBuffer).ToList();
			input.names = edges.Select(e => e.outputPort.portData.displayName).ToList();
			input.types = edges.Select(e => e.outputPort.portData.displayType ?? e.outputPort.fieldInfo.FieldType).ToList();
		}
	}

	[CustomPortOutput(nameof(output), typeof(object), true)]
	public void PushOutputs(List< SerializableEdge > edges, NodePort outputPort)
	{
		if (inputPorts.Count == 0)
			return;

		var inputPortEdges = inputPorts[0].GetEdges();

		if (outputPort.portData.identifier != packIdentifier && outputIndex >= 0 && (unpackOutput || inputPortEdges.Count == 1))
		{
			if (output.values == null)
				return;

			// 当我们解包输出时，输出中每种数据类型都有一个端口
			// 这意味着此函数将被调用的次数与输出端口数量相同
			// 因此我们使用类字段来保持索引。
			object data = output.values[outputIndex++];

			foreach (var edge in edges)
			{
				var inputRelay = edge.inputNode as RelayNode;
				edge.passThroughBuffer = inputRelay != null && !inputRelay.packInput ? output : data;
			}
		}
		else
		{
			foreach (var edge in edges)
				edge.passThroughBuffer = output;
		}
	}

	[CustomPortBehavior(nameof(input))]
	IEnumerable< PortData > InputPortBehavior(List< SerializableEdge > edges)
	{
		// 当节点初始化时，输入端口为空，因为生成端口的是这个函数
		int sizeInPixel = 0;
		if (inputPorts.Count != 0)
		{
			// 添加所有输入边的大小：
			var inputEdges = inputPorts[0]?.GetEdges();
			sizeInPixel = inputEdges.Sum(e => Mathf.Max(0, e.outputPort.portData.sizeInPixel - 8));
		}
		
		if (edges.Count == 1 && !packInput)
			inputType.type = edges[0].outputPort.portData.displayType;
		else
			inputType.type = typeof(object);

		yield return new PortData {
			displayName = "",
			displayType = inputType.type,
			identifier = "0",
			acceptMultipleEdges = true,
			sizeInPixel = Mathf.Min(k_MaxPortSize, sizeInPixel + 8),
		};
	}

	[CustomPortBehavior(nameof(output))]
	IEnumerable< PortData > OutputPortBehavior(List< SerializableEdge > edges)
	{
		if (inputPorts.Count == 0)
		{
			// 默认虚拟端口以避免中继节点没有任何输出：
			yield return new PortData {
				displayName = "",
				displayType = typeof(object),
				identifier = "0",
				acceptMultipleEdges = true,
			};
			yield break;
		}

		var inputPortEdges = inputPorts[0].GetEdges();
		var underlyingPortData = GetUnderlyingPortDataList();
		if (unpackOutput && inputPortEdges.Count == 1)
		{
			yield return new PortData
			{
				displayName = "Pack",
				identifier = packIdentifier,
				displayType = inputType.type,
				acceptMultipleEdges = true,
				sizeInPixel = Mathf.Min(k_MaxPortSize, Mathf.Max(underlyingPortData.Count, 1) + 7), // TODO: function
			};

			// 解包时我们仍然保留打包数据作为输出，以防解包后想要继续中继
			for (int i = 0; i < underlyingPortData.Count; i++)
			{
				yield return new PortData {
					displayName = underlyingPortData?[i].name ?? "",
					displayType = underlyingPortData?[i].type ?? typeof(object),
					identifier = i.ToString(),
					acceptMultipleEdges = true,
					sizeInPixel = 0,
				};
			}
		}
		else
		{
			yield return new PortData {
				displayName = "",
				displayType = inputType.type,
				identifier = "0",
				acceptMultipleEdges = true,
				sizeInPixel = Mathf.Min(k_MaxPortSize, Mathf.Max(underlyingPortData.Count, 1) + 7),
			};
		}
	}

	static List<(Type, string)> s_empty = new List<(Type, string)>();
	public List<(Type type, string name)> GetUnderlyingPortDataList()
	{
		// 获取输入边：
		if (inputPorts.Count == 0)
			return s_empty;

		var inputEdges = GetNonRelayEdges();

		if (inputEdges != null)
			return inputEdges.Select(e => (e.outputPort.portData.displayType ?? e.outputPort.fieldInfo.FieldType, e.outputPort.portData.displayName)).ToList();

		return s_empty;
	}

	public List<SerializableEdge> GetNonRelayEdges()
	{
		var inputEdges = inputPorts?[0]?.GetEdges();

		// 迭代直到输入中没有中继节点
		while (inputEdges.Count == 1 && inputEdges.First().outputNode.GetType() == typeof(RelayNode))
			inputEdges = inputEdges.First().outputNode.inputPorts[0]?.GetEdges();

		return inputEdges;
	}
}