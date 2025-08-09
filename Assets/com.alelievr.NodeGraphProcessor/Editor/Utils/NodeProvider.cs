using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEditor.Experimental.GraphView;

namespace GraphProcessor
{
	/// <summary>
	/// 节点提供者类
	/// 静态工具类，负责管理节点的注册、发现和创建
	/// 提供节点菜单、脚本缓存和端口描述等功能
	/// </summary>
	public static class NodeProvider
	{
		/// <summary>
		/// 端口描述结构体
		/// 描述节点的端口信息，用于边创建和端口连接
		/// </summary>
		public struct PortDescription
		{
			/// <summary>
			/// 节点类型
			/// </summary>
			public Type nodeType;
			
			/// <summary>
			/// 端口类型
			/// </summary>
			public Type portType;
			
			/// <summary>
			/// 是否为输入端口
			/// </summary>
			public bool isInput;
			
			/// <summary>
			/// 端口字段名
			/// </summary>
			public string portFieldName;
			
			/// <summary>
			/// 端口标识符
			/// </summary>
			public string portIdentifier;
			
			/// <summary>
			/// 端口显示名称
			/// </summary>
			public string portDisplayName;
		}

		/// <summary>
		/// 节点视图脚本缓存
		/// 存储节点视图类型到MonoScript的映射
		/// </summary>
		static Dictionary< Type, MonoScript >	nodeViewScripts = new Dictionary< Type, MonoScript >();
		
		/// <summary>
		/// 节点脚本缓存
		/// 存储节点类型到MonoScript的映射
		/// </summary>
		static Dictionary< Type, MonoScript >	nodeScripts = new Dictionary< Type, MonoScript >();
		
		/// <summary>
		/// 节点视图类型映射
		/// 存储节点类型到节点视图类型的映射
		/// </summary>
		static Dictionary< Type, Type >			nodeViewPerType = new Dictionary< Type, Type >();

		/// <summary>
		/// 节点描述类
		/// 包含特定图形的节点描述信息
		/// </summary>
		public class NodeDescriptions
		{
			/// <summary>
			/// 菜单标题到节点类型的映射
			/// </summary>
			public Dictionary< string, Type >		nodePerMenuTitle = new Dictionary< string, Type >();
			
			/// <summary>
			/// 槽位类型列表
			/// </summary>
			public List< Type >						slotTypes = new List< Type >();
			
			/// <summary>
			/// 节点创建端口描述列表
			/// </summary>
			public List< PortDescription >			nodeCreatePortDescription = new List<PortDescription>();
		}

		/// <summary>
		/// 特定图形节点结构体
		/// 描述特定于某个图形类型的节点信息
		/// </summary>
		public struct NodeSpecificToGraph
		{
			/// <summary>
			/// 节点类型
			/// </summary>
			public Type				nodeType;
			
			/// <summary>
			/// 图形兼容性检查方法列表
			/// </summary>
			public List<MethodInfo>	isCompatibleWithGraph;
			
			/// <summary>
			/// 兼容的图形类型
			/// </summary>
			public Type				compatibleWithGraphType;
		} 

		/// <summary>
		/// 特定图形节点描述缓存
		/// 存储每个图形的特定节点描述
		/// </summary>
		static Dictionary<BaseGraph, NodeDescriptions>	specificNodeDescriptions = new Dictionary<BaseGraph, NodeDescriptions>();
		
		/// <summary>
		/// 特定节点列表
		/// 存储所有特定于图形的节点信息
		/// </summary>
		static List<NodeSpecificToGraph>				specificNodes = new List<NodeSpecificToGraph>();

		/// <summary>
		/// 通用节点描述
		/// 存储不特定于任何图形的通用节点信息
		/// </summary>
		static NodeDescriptions							genericNodes = new NodeDescriptions();

		/// <summary>
		/// 静态构造函数
		/// 初始化节点提供者，构建脚本缓存和通用节点缓存
		/// </summary>
		static NodeProvider()
		{
			BuildScriptCache();
			BuildGenericNodeCache();
		}

		/// <summary>
		/// 加载图形
		/// 为指定图形构建节点描述缓存
		/// </summary>
		/// <param name="graph">要加载的图形</param>
		public static void LoadGraph(BaseGraph graph)
		{
			// 清除旧的图形数据
			specificNodeDescriptions.Remove(graph);
			var descriptions = new NodeDescriptions();
			specificNodeDescriptions.Add(graph, descriptions);

			var graphType = graph.GetType();
			foreach (var nodeInfo in specificNodes)
			{
				bool compatible = nodeInfo.compatibleWithGraphType == null || nodeInfo.compatibleWithGraphType == graphType;

				if (nodeInfo.isCompatibleWithGraph != null)
				{
					foreach (var method in nodeInfo.isCompatibleWithGraph)
						compatible &= (bool)method?.Invoke(null, new object[]{ graph });
				}

				if (compatible)
					BuildCacheForNode(nodeInfo.nodeType, descriptions, graph);
			}
		}

		/// <summary>
		/// 卸载图形
		/// 从缓存中移除指定图形的节点描述
		/// </summary>
		/// <param name="graph">要卸载的图形</param>
		public static void UnloadGraph(BaseGraph graph)
		{
			specificNodeDescriptions.Remove(graph);
		}

		/// <summary>
		/// 构建通用节点缓存
		/// 扫描所有继承自BaseNode的类型并构建缓存
		/// </summary>
		static void BuildGenericNodeCache()
		{
			foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())
			{
				if (!IsNodeAccessibleFromMenu(nodeType))
					continue;

				if (IsNodeSpecificToGraph(nodeType))
					continue;

				BuildCacheForNode(nodeType, genericNodes);
			}
		}

		/// <summary>
		/// 为节点构建缓存
		/// 处理单个节点的缓存构建逻辑
		/// </summary>
		/// <param name="nodeType">节点类型</param>
		/// <param name="targetDescription">目标描述对象</param>
		/// <param name="graph">图形对象（可选）</param>
		static void BuildCacheForNode(Type nodeType, NodeDescriptions targetDescription, BaseGraph graph = null)
		{
			var attrs = nodeType.GetCustomAttributes(typeof(NodeMenuItemAttribute), false) as NodeMenuItemAttribute[];

			if (attrs != null && attrs.Length > 0)
			{
				foreach (var attr in attrs)
					targetDescription.nodePerMenuTitle[attr.menuTitle] = nodeType;
			}

			foreach (var field in nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (field.GetCustomAttribute<HideInInspector>() == null && field.GetCustomAttributes().Any(c => c is InputAttribute || c is OutputAttribute))
					targetDescription.slotTypes.Add(field.FieldType);
			}

			ProvideNodePortCreationDescription(nodeType, targetDescription, graph);
		}

		static bool IsNodeAccessibleFromMenu(Type nodeType)
		{
			if (nodeType.IsAbstract)
				return false;

			return nodeType.GetCustomAttributes<NodeMenuItemAttribute>().Count() > 0;
		}

		// Check if node has anything that depends on the graph type or settings
		static bool IsNodeSpecificToGraph(Type nodeType)
		{
			var isCompatibleWithGraphMethods = nodeType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(m => m.GetCustomAttribute<IsCompatibleWithGraph>() != null);
			var nodeMenuAttributes = nodeType.GetCustomAttributes<NodeMenuItemAttribute>();

			List<Type> compatibleGraphTypes = nodeMenuAttributes.Where(n => n.onlyCompatibleWithGraph != null).Select(a => a.onlyCompatibleWithGraph).ToList();

			List<MethodInfo> compatibleMethods = new List<MethodInfo>();
			foreach (var method in isCompatibleWithGraphMethods)
			{
				// Check if the method is static and have the correct prototype
				var p = method.GetParameters();
				if (method.ReturnType != typeof(bool) || p.Count() != 1 || p[0].ParameterType != typeof(BaseGraph))
					Debug.LogError($"The function '{method.Name}' marked with the IsCompatibleWithGraph attribute either doesn't return a boolean or doesn't take one parameter of BaseGraph type.");
				else
					compatibleMethods.Add(method);
			}

			if (compatibleMethods.Count > 0 || compatibleGraphTypes.Count > 0)
			{
				// We still need to add the element in specificNode even without specific graph
				if (compatibleGraphTypes.Count == 0)
					compatibleGraphTypes.Add(null);

				foreach (var graphType in compatibleGraphTypes)
				{
					specificNodes.Add(new NodeSpecificToGraph{
						nodeType = nodeType,
						isCompatibleWithGraph = compatibleMethods,
						compatibleWithGraphType = graphType
					});
				}
				return true;
			}
			return false;
		}
	
		static void BuildScriptCache()
		{
			foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())
			{
				if (!IsNodeAccessibleFromMenu(nodeType))
					continue;

				AddNodeScriptAsset(nodeType);
			}

			foreach (var nodeViewType in TypeCache.GetTypesDerivedFrom<BaseNodeView>())
			{
				if (!nodeViewType.IsAbstract)
					AddNodeViewScriptAsset(nodeViewType);
			}
		}

		static FieldInfo SetGraph = typeof(BaseNode).GetField("graph", BindingFlags.NonPublic | BindingFlags.Instance);
		static void ProvideNodePortCreationDescription(Type nodeType, NodeDescriptions targetDescription, BaseGraph graph = null)
		{
			var node = Activator.CreateInstance(nodeType) as BaseNode;
			try {
				SetGraph.SetValue(node, graph);
				node.InitializePorts();
				node.UpdateAllPorts();
			} catch (Exception) { }

			foreach (var p in node.inputPorts)
				AddPort(p, true);
			foreach (var p in node.outputPorts)
				AddPort(p, false);

			void AddPort(NodePort p, bool input)
			{
				targetDescription.nodeCreatePortDescription.Add(new PortDescription{
					nodeType = nodeType,
					portType = p.portData.displayType ?? p.fieldInfo.FieldType,
					isInput = input,
					portFieldName = p.fieldName,
					portDisplayName = p.portData.displayName ?? p.fieldName,
					portIdentifier = p.portData.identifier,
				});
			}
		}

		static void AddNodeScriptAsset(Type type)
		{
			var nodeScriptAsset = FindScriptFromClassName(type.Name);

			// Try find the class name with Node name at the end
			if (nodeScriptAsset == null)
				nodeScriptAsset = FindScriptFromClassName(type.Name + "Node");
			if (nodeScriptAsset != null)
				nodeScripts[type] = nodeScriptAsset;
		}

		static void	AddNodeViewScriptAsset(Type type)
		{
			var attrs = type.GetCustomAttributes(typeof(NodeCustomEditor), false) as NodeCustomEditor[];

			if (attrs != null && attrs.Length > 0)
			{
				Type nodeType = attrs.First().nodeType;
				nodeViewPerType[nodeType] = type;

				var nodeViewScriptAsset = FindScriptFromClassName(type.Name);
				if (nodeViewScriptAsset == null)
					nodeViewScriptAsset = FindScriptFromClassName(type.Name + "View");
				if (nodeViewScriptAsset == null)
					nodeViewScriptAsset = FindScriptFromClassName(type.Name + "NodeView");

				if (nodeViewScriptAsset != null)
					nodeViewScripts[type] = nodeViewScriptAsset;
			}
		}

		static MonoScript FindScriptFromClassName(string className)
		{
			var scriptGUIDs = AssetDatabase.FindAssets($"t:script {className}");

			if (scriptGUIDs.Length == 0)
				return null;

			foreach (var scriptGUID in scriptGUIDs)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(scriptGUID);
				var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

				if (script != null && String.Equals(className, Path.GetFileNameWithoutExtension(assetPath), StringComparison.OrdinalIgnoreCase))
					return script;
			}

			return null;
		}

		public static Type GetNodeViewTypeFromType(Type nodeType)
		{
			Type view;

            if (nodeViewPerType.TryGetValue(nodeType, out view))
                return view;

            Type baseType = null;

            // Allow for inheritance in node views: multiple C# node using the same view
            foreach (var type in nodeViewPerType)
            {
                // Find a view (not first fitted view) of nodeType
                if (nodeType.IsSubclassOf(type.Key) && (baseType == null || type.Value.IsSubclassOf(baseType)))
                    baseType = type.Value;
            }

            if (baseType != null)
                return baseType;

            return view;
        }

        public static IEnumerable<(string path, Type type)>	GetNodeMenuEntries(BaseGraph graph = null)
		{
			foreach (var node in genericNodes.nodePerMenuTitle)
				yield return (node.Key, node.Value);

			if (graph != null && specificNodeDescriptions.TryGetValue(graph, out var specificNodes))
			{
				foreach (var node in specificNodes.nodePerMenuTitle)
					yield return (node.Key, node.Value);
			}
		}

		public static MonoScript GetNodeViewScript(Type type)
		{
			nodeViewScripts.TryGetValue(type, out var script);

			return script;
		}

		public static MonoScript GetNodeScript(Type type)
		{
			nodeScripts.TryGetValue(type, out var script);

			return script;
		}

		public static IEnumerable<Type> GetSlotTypes(BaseGraph graph = null) 
		{
			foreach (var type in genericNodes.slotTypes)
				yield return type;

			if (graph != null && specificNodeDescriptions.TryGetValue(graph, out var specificNodes))
			{
				foreach (var type in specificNodes.slotTypes)
					yield return type;
			}
		}

		public static IEnumerable<PortDescription> GetEdgeCreationNodeMenuEntry(PortView portView, BaseGraph graph = null)
		{
			foreach (var description in genericNodes.nodeCreatePortDescription)
			{
				if (!IsPortCompatible(description))
					continue;

				yield return description;
			}

			if (graph != null && specificNodeDescriptions.TryGetValue(graph, out var specificNodes))
			{
				foreach (var description in specificNodes.nodeCreatePortDescription)
				{
					if (!IsPortCompatible(description))
						continue;
					yield return description;
				}
			}

			bool IsPortCompatible(PortDescription description)
			{
				if ((portView.direction == Direction.Input && description.isInput) || (portView.direction == Direction.Output && !description.isInput))
					return false;
	
				if (!BaseGraph.TypesAreConnectable(description.portType, portView.portType))
					return false;
					
				return true;
			}
		}
	}
}
