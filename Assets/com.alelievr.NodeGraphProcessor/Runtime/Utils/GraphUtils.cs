using System;
using System.Linq;
using System.Collections.Generic;

namespace GraphProcessor
{
    /// <summary>
    /// 图形工具类
    /// 提供图形遍历、排序和循环检测等实用功能
    /// 包含深度优先搜索、拓扑排序和循环检测算法
    /// </summary>
    public static class GraphUtils
    {
        /// <summary>
        /// 节点状态枚举
        /// 用于深度优先搜索中的节点状态标记
        /// </summary>
        enum State
        {
            /// <summary>
            /// 未访问状态
            /// 节点尚未被访问
            /// </summary>
            White,
            
            /// <summary>
            /// 正在访问状态
            /// 节点正在被访问，用于检测循环
            /// </summary>
            Grey,
            
            /// <summary>
            /// 已访问状态
            /// 节点及其所有依赖都已访问完成
            /// </summary>
            Black,
        }

        /// <summary>
        /// 遍历节点类
        /// 用于图形遍历的内部数据结构
        /// </summary>
        class TarversalNode
        {
            /// <summary>
            /// 原始节点对象
            /// </summary>
            public BaseNode node;
            
            /// <summary>
            /// 输入节点列表
            /// 当前节点的所有输入依赖
            /// </summary>
            public List<TarversalNode> inputs = new List<TarversalNode>();
            
            /// <summary>
            /// 输出节点列表
            /// 当前节点的所有输出依赖
            /// </summary>
            public List<TarversalNode> outputs = new List<TarversalNode>();
            
            /// <summary>
            /// 节点状态
            /// 用于深度优先搜索的状态标记
            /// </summary>
            public State    state = State.White;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="node">原始节点</param>
            public TarversalNode(BaseNode node) { this.node = node; }
        }

        /// <summary>
        /// 遍历图形类
        /// 为便于图形遍历而创建的结构
        /// 包含所有节点和输出节点的引用
        /// </summary>
        class TraversalGraph
        {
            /// <summary>
            /// 所有节点列表
            /// </summary>
            public List<TarversalNode> nodes = new List<TarversalNode>();
            
            /// <summary>
            /// 输出节点列表
            /// 图形的最终输出节点
            /// </summary>
            public List<TarversalNode> outputs = new List<TarversalNode>();
        }

        /// <summary>
        /// 将BaseGraph转换为TraversalGraph
        /// 创建用于遍历的内部数据结构
        /// </summary>
        /// <param name="graph">原始图形</param>
        /// <returns>遍历图形对象</returns>
        static TraversalGraph ConvertGraphToTraversalGraph(BaseGraph graph)
        {
            TraversalGraph g = new TraversalGraph();
            Dictionary<BaseNode, TarversalNode> nodeMap = new Dictionary<BaseNode, TarversalNode>();

            // 创建所有节点的遍历节点对象
            foreach (var node in graph.nodes)
            {
                var tn = new TarversalNode(node);
                g.nodes.Add(tn);
                nodeMap[node] = tn;

                // 标记输出节点
                if (graph.graphOutputs.Contains(node))
                    g.outputs.Add(tn);
            }

            // 建立节点间的连接关系
            foreach (var tn in g.nodes)
            {
                tn.inputs = tn.node.GetInputNodes().Where(n => nodeMap.ContainsKey(n)).Select(n => nodeMap[n]).ToList();
                tn.outputs = tn.node.GetOutputNodes().Where(n => nodeMap.ContainsKey(n)).Select(n => nodeMap[n]).ToList();
            }

            return g;
        }

        /// <summary>
        /// 深度优先排序
        /// 对图形进行深度优先搜索排序，确保依赖关系正确
        /// 处理参数节点的特殊逻辑，确保getter在setter之前执行
        /// </summary>
        /// <param name="g">要排序的图形</param>
        /// <returns>按依赖顺序排序的节点列表</returns>
        public static List<BaseNode> DepthFirstSort(BaseGraph g)
        {
            var graph = ConvertGraphToTraversalGraph(g);
            List<BaseNode> depthFirstNodes = new List<BaseNode>();

            // 对所有节点进行深度优先搜索
            foreach (var n in graph.nodes)
                DFS(n);

            /// <summary>
            /// 深度优先搜索的递归实现
            /// </summary>
            /// <param name="n">当前遍历的节点</param>
            void DFS(TarversalNode n)
            {
                // 如果节点已访问完成，直接返回
                if (n.state == State.Black)
                    return;
                
                // 标记节点为正在访问状态
                n.state = State.Grey;

                // 特殊处理参数节点
                if (n.node is ParameterNode parameterNode && parameterNode.accessor == ParameterAccessor.Get)
                {
                    // 对于参数getter，确保对应的setter先执行
                    foreach (var setter in graph.nodes.Where(x=> 
                        x.node is ParameterNode p &&
                        p.parameterGUID == parameterNode.parameterGUID &&
                        p.accessor == ParameterAccessor.Set))
                    {
                        if (setter.state == State.White)
                            DFS(setter);
                    }
                }
                else
                {
                    // 普通节点：先访问所有输入节点
                    foreach (var input in n.inputs)
                    {
                        if (input.state == State.White)
                            DFS(input);
                    }
                }

                // 标记节点为已访问完成
                n.state = State.Black;

                // 只有当其子节点完全访问后才添加节点
                depthFirstNodes.Add(n.node);
            }

            return depthFirstNodes;
        }

        /// <summary>
        /// 在图形中查找循环
        /// 使用深度优先搜索检测图形中的循环依赖
        /// </summary>
        /// <param name="g">要检查的图形</param>
        /// <param name="cyclicNode">发现循环节点时的回调函数</param>
        public static void FindCyclesInGraph(BaseGraph g, Action<BaseNode> cyclicNode)
        {
            var graph = ConvertGraphToTraversalGraph(g);
            List<TarversalNode> cyclicNodes = new List<TarversalNode>();

            // 对所有节点进行深度优先搜索
            foreach (var n in graph.nodes)
                DFS(n);

            /// <summary>
            /// 深度优先搜索的递归实现（循环检测版本）
            /// </summary>
            /// <param name="n">当前遍历的节点</param>
            void DFS(TarversalNode n)
            {
                // 如果节点已访问完成，直接返回
                if (n.state == State.Black)
                    return;
                
                // 标记节点为正在访问状态
                n.state = State.Grey;

                // 检查所有输入节点
                foreach (var input in n.inputs)
                {
                    if (input.state == State.White)
                        DFS(input);
                    else if (input.state == State.Grey)
                        // 发现循环：当前节点指向正在访问的节点
                        cyclicNodes.Add(n);
                }
                
                // 标记节点为已访问完成
                n.state = State.Black;
            }

            // 调用回调函数通知发现的循环节点
            cyclicNodes.ForEach((tn) => cyclicNode?.Invoke(tn.node));
        }
    }
}