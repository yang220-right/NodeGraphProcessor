using UnityEditor;
using GraphProcessor;

public class GraphProcessorMenuItems : NodeGraphProcessorMenuItems
{
	[MenuItem("Assets/Create/Node C# Script", false, MenuItemPosition.afterCreateScript)]
	private static void CreateNodeCSharpScritpt() => CreateDefaultNodeCSharpScritpt();
	
	[MenuItem("Assets/Create/Node View C# Script", false, MenuItemPosition.afterCreateScript + 1)]
	private static void CreateNodeViewCSharpScritpt() => CreateDefaultNodeViewCSharpScritpt();

	// 要使用您自己的模板添加C#脚本创建，请使用ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, defaultFileName)
}