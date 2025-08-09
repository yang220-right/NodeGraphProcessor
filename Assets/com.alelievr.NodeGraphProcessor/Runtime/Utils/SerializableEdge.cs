using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	/// <summary>
	/// 可序列化边类
	/// 表示图形中两个节点之间的连接，支持序列化和反序列化
	/// 负责管理节点间的数据传输和连接关系
	/// </summary>
	[System.Serializable]
	public class SerializableEdge : ISerializationCallbackReceiver
	{
		/// <summary>
		/// 边的唯一标识符
		/// 用于唯一标识图形中的每条边
		/// </summary>
		public string	GUID;

		/// <summary>
		/// 边所属的图形
		/// 引用包含此边的图形对象
		/// </summary>
		[SerializeField]
		BaseGraph		owner;

		/// <summary>
		/// 输入节点的GUID
		/// 序列化时存储的输入节点标识符
		/// </summary>
		[SerializeField]
		string			inputNodeGUID;
		
		/// <summary>
		/// 输出节点的GUID
		/// 序列化时存储的输出节点标识符
		/// </summary>
		[SerializeField]
		string			outputNodeGUID;

		/// <summary>
		/// 输入节点对象
		/// 边指向的目标节点
		/// </summary>
		[System.NonSerialized]
		public BaseNode	inputNode;

		/// <summary>
		/// 输入端口对象
		/// 边连接的输入端口
		/// </summary>
		[System.NonSerialized]
		public NodePort	inputPort;
		
		/// <summary>
		/// 输出端口对象
		/// 边连接的输出端口
		/// </summary>
		[System.NonSerialized]
		public NodePort outputPort;

		/// <summary>
		/// 数据传递缓冲区
		/// 当使用自定义输入/输出函数时，用于在端口之间发送数据的临时对象
		/// </summary>
		[System.NonSerialized]
		public object	passThroughBuffer;

		/// <summary>
		/// 输出节点对象
		/// 边的起始节点
		/// </summary>
		[System.NonSerialized]
		public BaseNode	outputNode;

		/// <summary>
		/// 输入字段名称
		/// 输入端口对应的字段名
		/// </summary>
		public string	inputFieldName;
		
		/// <summary>
		/// 输出字段名称
		/// 输出端口对应的字段名
		/// </summary>
		public string	outputFieldName;

		/// <summary>
		/// 输入端口标识符
		/// 用于存储生成多个端口的字段的ID
		/// </summary>
		public string	inputPortIdentifier;
		
		/// <summary>
		/// 输出端口标识符
		/// 用于存储生成多个端口的字段的ID
		/// </summary>
		public string	outputPortIdentifier;

		/// <summary>
		/// 默认构造函数
		/// </summary>
		public SerializableEdge() {}

		/// <summary>
		/// 创建新的边
		/// 根据输入端口和输出端口创建一条新的边
		/// </summary>
		/// <param name="graph">边所属的图形</param>
		/// <param name="inputPort">输入端口</param>
		/// <param name="outputPort">输出端口</param>
		/// <returns>新创建的边对象</returns>
		public static SerializableEdge CreateNewEdge(BaseGraph graph, NodePort inputPort, NodePort outputPort)
		{
			SerializableEdge	edge = new SerializableEdge();

			// 设置边的基本属性
			edge.owner = graph;
			edge.GUID = System.Guid.NewGuid().ToString();
			edge.inputNode = inputPort.owner;
			edge.inputFieldName = inputPort.fieldName;
			edge.outputNode = outputPort.owner;
			edge.outputFieldName = outputPort.fieldName;
			edge.inputPort = inputPort;
			edge.outputPort = outputPort;
			edge.inputPortIdentifier = inputPort.portData.identifier;
			edge.outputPortIdentifier = outputPort.portData.identifier;

			return edge;
		}

		/// <summary>
		/// 序列化前的回调
		/// 在序列化前保存节点的GUID引用
		/// </summary>
		public void OnBeforeSerialize()
		{
			// 如果节点为空，跳过序列化
			if (outputNode == null || inputNode == null)
				return;

			// 保存节点的GUID引用
			outputNodeGUID = outputNode.GUID;
			inputNodeGUID = inputNode.GUID;
		}

		/// <summary>
		/// 反序列化后的回调
		/// 当前为空实现，反序列化逻辑在Deserialize方法中处理
		/// </summary>
		public void OnAfterDeserialize() {}

		/// <summary>
		/// 反序列化边
		/// 这里我们的所有者已经被反序列化，可以恢复节点和端口的引用
		/// </summary>
		public void Deserialize()
		{
			// 检查节点GUID是否存在于图形中
			if (!owner.nodesPerGUID.ContainsKey(outputNodeGUID) || !owner.nodesPerGUID.ContainsKey(inputNodeGUID))
				return ;

			// 恢复节点引用
			outputNode = owner.nodesPerGUID[outputNodeGUID];
			inputNode = owner.nodesPerGUID[inputNodeGUID];
			
			// 恢复端口引用
			inputPort = inputNode.GetPort(inputFieldName, inputPortIdentifier);
			outputPort = outputNode.GetPort(outputFieldName, outputPortIdentifier);
		}

		/// <summary>
		/// 转换为字符串表示
		/// 用于调试和日志输出
		/// </summary>
		/// <returns>边的字符串表示</returns>
		public override string ToString() => $"{outputNode.name}:{outputPort.fieldName} -> {inputNode.name}:{inputPort.fieldName}";
	}
}
