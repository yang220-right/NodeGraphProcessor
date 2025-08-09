using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;

namespace NodeGraphProcessor.Examples
{
	[System.Serializable, NodeMenuItem("Conditional/Start")]
	public class StartNode : BaseNode, IConditionalNode
	{
		[Output(name = "Executes")]
		public ConditionalLink		executes;

		public override string		name => "Start";

		public IEnumerable< ConditionalNode >	GetExecutedNodes()
		{
			// 返回连接到executes端口的所有节点
			return GetOutputNodes().Where(n => n is ConditionalNode).Select(n => n as ConditionalNode);
		}

		public override FieldInfo[] GetNodeFields() => base.GetNodeFields();
	}
}
