using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

/// <summary>
/// 中继节点类
/// 用于在图形中传递和转换数据，支持数据打包和解包
/// 可以动态创建端口来处理不同类型的数据
/// </summary>
[System.Serializable, NodeMenuItem("Utils/Relay")]
public class RelayNode : BaseNode
{
	/// <summary>
	/// 打包标识符
	/// 用于标识打包端口的特殊标识符
	/// </summary>
	const string packIdentifier = "_Pack";

	/// <summary>
	/// 打包的中继数据结构
	/// 用于存储多个数据值及其相关信息
	/// </summary>
	[HideInInspector]
	public struct PackedRelayData
	{
		/// <summary>
		/// 数据值列表
		/// 存储所有传递的数据值
		/// </summary>
		public List<object>	values;
		
		/// <summary>
		/// 数据名称列表
		/// 存储每个数据值的显示名称
		/// </summary>
		public List<string>	names;
		
		/// <summary>
		/// 数据类型列表
		/// 存储每个数据值的类型
		/// </summary>
		public List<Type>	types;
	}

	/// <summary>
	/// 输入数据
	/// 接收来自其他节点的数据
	/// </summary>
	[Input(name = "In")]
    public PackedRelayData	input;

	/// <summary>
	/// 输出数据
	/// 向其他节点传递数据
	/// </summary>
	[Output(name = "Out")]
	public PackedRelayData	output;

	/// <summary>
	/// 是否解包输出
	/// 控制是否将打包的数据解包为多个单独的输出
	/// </summary>
	public bool		unpackOutput = false;
	
	/// <summary>
	/// 是否打包输入
	/// 控制是否将多个输入打包为单个数据
	/// </summary>
	public bool		packInput = false;
	
	/// <summary>
	/// 输入边数量
	/// 记录连接到输入端口的边数量
	/// </summary>
	public int		inputEdgeCount = 0;
	
	/// <summary>
	/// 输出索引
	/// 用于跟踪当前输出的数据索引
	/// </summary>
	[System.NonSerialized]
	int				outputIndex = 0;

	/// <summary>
	/// 输入类型
	/// 存储输入数据的类型信息
	/// </summary>
	SerializableType inputType = new SerializableType(typeof(object));

	/// <summary>
	/// 最大端口大小
	/// 限制动态创建的端口数量
	/// </summary>
	const int		k_MaxPortSize = 14;

	/// <summary>
	/// 处理节点逻辑
	/// 将输入数据传递给输出
	/// </summary>
	protected override void Process()
	{
		outputIndex = 0;
		output = input;
	}

	/// <summary>
	/// 节点布局样式
	/// 指定节点在UI中的显示样式
	/// </summary>
	public override string layoutStyle => "GraphProcessorStyles/RelayNode";

	/// <summary>
	/// 获取输入数据
	/// 自定义端口输入处理，从连接的边中获取数据
	/// </summary>
	/// <param name="edges">连接的边列表</param>
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

	/// <summary>
	/// 推送输出数据
	/// 自定义端口输出处理，将数据推送到连接的边
	/// </summary>
	/// <param name="edges">连接的边列表</param>
	/// <param name="outputPort">输出端口</param>
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

	/// <summary>
	/// 输入端口行为
	/// 自定义输入端口的动态行为
	/// </summary>
	/// <param name="edges">连接的边列表</param>
	/// <returns>端口数据集合</returns>
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

	/// <summary>
	/// 输出端口行为
	/// 自定义输出端口的动态行为
	/// </summary>
	/// <param name="edges">连接的边列表</param>
	/// <returns>端口数据集合</returns>
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