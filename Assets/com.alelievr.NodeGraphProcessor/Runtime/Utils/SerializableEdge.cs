using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	[System.Serializable]
	public class SerializableEdge : ISerializationCallbackReceiver
	{
		public string	GUID;

		[SerializeField]
		BaseGraph		owner;

		[SerializeField]
		string			inputNodeGUID;
		[SerializeField]
		string			outputNodeGUID;

		[System.NonSerialized]
		public BaseNode	inputNode;

		[System.NonSerialized]
		public NodePort	inputPort;
		[System.NonSerialized]
		public NodePort outputPort;

		//当使用自定义输入/输出函数时，用于在端口之间发送数据的临时对象。
		[System.NonSerialized]
		public object	passThroughBuffer;

		[System.NonSerialized]
		public BaseNode	outputNode;

		public string	inputFieldName;
		public string	outputFieldName;

		// 用于存储生成多个端口的字段的id
		public string	inputPortIdentifier;
		public string	outputPortIdentifier;

		public SerializableEdge() {}

		public static SerializableEdge CreateNewEdge(BaseGraph graph, NodePort inputPort, NodePort outputPort)
		{
			SerializableEdge	edge = new SerializableEdge();

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

		public void OnBeforeSerialize()
		{
			if (outputNode == null || inputNode == null)
				return;

			outputNodeGUID = outputNode.GUID;
			inputNodeGUID = inputNode.GUID;
		}

		public void OnAfterDeserialize() {}

		//这里我们的所有者已经被反序列化
		public void Deserialize()
		{
			if (!owner.nodesPerGUID.ContainsKey(outputNodeGUID) || !owner.nodesPerGUID.ContainsKey(inputNodeGUID))
				return ;

			outputNode = owner.nodesPerGUID[outputNodeGUID];
			inputNode = owner.nodesPerGUID[inputNodeGUID];
			inputPort = inputNode.GetPort(inputFieldName, inputPortIdentifier);
			outputPort = outputNode.GetPort(outputFieldName, outputPortIdentifier);
		}

		public override string ToString() => $"{outputNode.name}:{outputPort.fieldName} -> {inputNode.name}:{inputPort.fieldName}";
	}
}
