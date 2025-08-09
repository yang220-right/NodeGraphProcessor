using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace GraphProcessor
{
    /// <summary>
    /// 创建节点菜单窗口类
    /// 实现ISearchWindowProvider接口，提供节点创建菜单
    /// TODO: 用新的UnityEditor.Searcher包替换这个
    /// </summary>
    class CreateNodeMenuWindow : ScriptableObject, ISearchWindowProvider
    {
        /// <summary>
        /// 图形视图
        /// 菜单窗口所属的图形视图
        /// </summary>
        BaseGraphView   graphView;
        
        /// <summary>
        /// 编辑器窗口
        /// 菜单窗口的父窗口
        /// </summary>
        EditorWindow    window;
        
        /// <summary>
        /// 图标
        /// 用于搜索窗口项目缩进的透明图标
        /// </summary>
        Texture2D       icon;
        
        /// <summary>
        /// 边过滤器
        /// 用于过滤节点创建的边视图
        /// </summary>
        EdgeView        edgeFilter;
        
        /// <summary>
        /// 输入端口视图
        /// 边的输入端口视图
        /// </summary>
        PortView        inputPortView;
        
        /// <summary>
        /// 输出端口视图
        /// 边的输出端口视图
        /// </summary>
        PortView        outputPortView;

        /// <summary>
        /// 初始化菜单窗口
        /// 设置菜单窗口的基本参数和图标
        /// </summary>
        /// <param name="graphView">图形视图</param>
        /// <param name="window">编辑器窗口</param>
        /// <param name="edgeFilter">边过滤器（可选）</param>
        public void Initialize(BaseGraphView graphView, EditorWindow window, EdgeView edgeFilter = null)
        {
            this.graphView = graphView;
            this.window = window;
            this.edgeFilter = edgeFilter;
            this.inputPortView = edgeFilter?.input as PortView;
            this.outputPortView = edgeFilter?.output as PortView;

            // 创建透明图标，用于欺骗搜索窗口缩进项目
            if (icon == null)
                icon = new Texture2D(1, 1);
            icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            icon.Apply();
        }

        /// <summary>
        /// 销毁时清理资源
        /// 销毁创建的图标资源
        /// </summary>
        void OnDestroy()
        {
            if (icon != null)
            {
                DestroyImmediate(icon);
                icon = null;
            }
        }

        /// <summary>
        /// 创建搜索树
        /// 实现ISearchWindowProvider接口，创建节点创建菜单的搜索树
        /// </summary>
        /// <param name="context">搜索窗口上下文</param>
        /// <returns>搜索树条目列表</returns>
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            };

            // 根据是否有边过滤器决定创建标准菜单还是边节点菜单
            if (edgeFilter == null)
                CreateStandardNodeMenu(tree);
            else
                CreateEdgeNodeMenu(tree);

            return tree;
        }

        /// <summary>
        /// 创建标准节点菜单
        /// 创建常规的节点创建菜单，按字母顺序和子菜单排序
        /// </summary>
        /// <param name="tree">搜索树条目列表</param>
        void CreateStandardNodeMenu(List<SearchTreeEntry> tree)
        {
            // 按字母顺序和子菜单排序菜单
            var nodeEntries = graphView.FilterCreateNodeMenuEntries().OrderBy(k => k.path);
            var titlePaths = new HashSet< string >();
            
			foreach (var nodeMenuItem in nodeEntries)
			{
                var nodePath = nodeMenuItem.path;
                var nodeName = nodePath;
                var level    = 0;
                var parts    = nodePath.Split('/');

                if(parts.Length > 1)
                {
                    level++;
                    nodeName = parts[parts.Length - 1];
                    var fullTitleAsPath = "";
                    
                    for(var i = 0; i < parts.Length - 1; i++)
                    {
                        var title = parts[i];
                        fullTitleAsPath += title;
                        level = i + 1;
                        
                        // 如果节点在子类别中，添加节标题
                        if (!titlePaths.Contains(fullTitleAsPath))
                        {
                            tree.Add(new SearchTreeGroupEntry(new GUIContent(title)){
                                level = level
                            });
                            titlePaths.Add(fullTitleAsPath);
                        }
                    }
                }
                
                tree.Add(new SearchTreeEntry(new GUIContent(nodeName, icon))
                {
                    level    = level + 1,
                    userData = nodeMenuItem.type
                });
			}
        }

        /// <summary>
        /// 创建边节点菜单
        /// 创建基于边的节点创建菜单，根据边过滤器和图形中的节点排序
        /// </summary>
        /// <param name="tree">搜索树条目列表</param>
        void CreateEdgeNodeMenu(List<SearchTreeEntry> tree)
        {
            var entries = NodeProvider.GetEdgeCreationNodeMenuEntry((edgeFilter.input ?? edgeFilter.output) as PortView, graphView.graph);

            var titlePaths = new HashSet< string >();

            var nodePaths = NodeProvider.GetNodeMenuEntries(graphView.graph);

            tree.Add(new SearchTreeEntry(new GUIContent($"Relay", icon))
            {
                level = 1,
                userData = new NodeProvider.PortDescription{
			        nodeType = typeof(RelayNode),
			        portType = typeof(System.Object),
			        isInput = inputPortView != null,
			        portFieldName = inputPortView != null ? nameof(RelayNode.output) : nameof(RelayNode.input),
			        portIdentifier = "0",
			        portDisplayName = inputPortView != null ? "Out" : "In",
                }
            });

            var sortedMenuItems = entries.Select(port => (port, nodePaths.FirstOrDefault(kp => kp.type == port.nodeType).path)).OrderBy(e => e.path);

            // 按字母顺序和子菜单排序菜单
			foreach (var nodeMenuItem in sortedMenuItems)
			{
                var nodePath = nodePaths.FirstOrDefault(kp => kp.type == nodeMenuItem.port.nodeType).path;

                // 如果节点不在创建菜单中，则忽略它
                if (String.IsNullOrEmpty(nodePath))
                    continue;

                var nodeName = nodePath;
                var level    = 0;
                var parts    = nodePath.Split('/');

                if (parts.Length > 1)
                {
                    level++;
                    nodeName = parts[parts.Length - 1];
                    var fullTitleAsPath = "";
                    
                    for (var i = 0; i < parts.Length - 1; i++)
                    {
                        var title = parts[i];
                        fullTitleAsPath += title;
                        level = i + 1;

                        // 如果节点在子类别中，添加节标题
                        if (!titlePaths.Contains(fullTitleAsPath))
                        {
                            tree.Add(new SearchTreeGroupEntry(new GUIContent(title)){
                                level = level
                            });
                            titlePaths.Add(fullTitleAsPath);
                        }
                    }
                }

                tree.Add(new SearchTreeEntry(new GUIContent($"{nodeName}:  {nodeMenuItem.port.portDisplayName}", icon))
                {
                    level    = level + 1,
                    userData = nodeMenuItem.port
                });
			}
        }

        // 验证选择时创建节点
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            // 窗口到图形位置
            var windowRoot = window.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, context.screenMousePosition - window.position.position);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            var nodeType = searchTreeEntry.userData is Type ? (Type)searchTreeEntry.userData : ((NodeProvider.PortDescription)searchTreeEntry.userData).nodeType;
            
            graphView.RegisterCompleteObjectUndo("Added " + nodeType);
            var view = graphView.AddNode(BaseNode.CreateFromType(nodeType, graphMousePosition));

            if (searchTreeEntry.userData is NodeProvider.PortDescription desc)
            {
                var targetPort = view.GetPortViewFromFieldName(desc.portFieldName, desc.portIdentifier);
                if (inputPortView == null)
                    graphView.Connect(targetPort, outputPortView);
                else
                    graphView.Connect(inputPortView, targetPort);
            }

            return true;
        }
    }
}