using System.Collections.Generic;
using System.Reflection;

namespace NodeGraphProcessor.Examples
{
	interface IConditionalNode
	{
		IEnumerable< ConditionalNode >	GetExecutedNodes();

		FieldInfo[] GetNodeFields(); // 为字段提供自定义顺序（使条件链接始终位于节点顶部）
	}
}