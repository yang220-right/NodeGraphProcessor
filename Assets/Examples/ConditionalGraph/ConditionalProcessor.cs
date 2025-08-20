using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using Debug = UnityEngine.Debug;

namespace NodeGraphProcessor.Examples{
  public class ConditionalProcessor : BaseGraphProcessor{
    List<BaseNode> processList;
    List<StartNode> startNodeList;

    Dictionary<BaseNode, List<BaseNode>> nonConditionalDependenciesCache = new Dictionary<BaseNode, List<BaseNode>>();

    public bool pause;

    public IEnumerator<BaseNode> currentGraphExecution{ get; private set; } = null;

    // static readonly float   maxExecutionTimeMS = 100; // 100毫秒最大执行时间以避免无限循环

    /// <summary>
    /// 管理图形调度和处理
    /// </summary>
    /// <param name="graph">要处理的图形</param>
    public ConditionalProcessor(BaseGraph graph) : base(graph){
    }

    public override void UpdateComputeOrder(){
      // 收集起始节点：
      startNodeList = graph.nodes.Where(n => n is StartNode).Select(n => n as StartNode).ToList();

      // 如果没有起始节点，我们像往常一样处理图形
      if (startNodeList.Count == 0){
        processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
      }
      else{
        nonConditionalDependenciesCache.Clear();
        // 准备非条件节点执行的缓存
      }
    }

    public override void Run(){
      IEnumerator<BaseNode> enumerator;

      if (startNodeList.Count == 0){
        enumerator = RunTheGraph();
      }
      else{
        Stack<BaseNode> nodeToExecute = new Stack<BaseNode>();
        // 将所有起始节点添加到执行堆栈
        startNodeList.ForEach(s => nodeToExecute.Push(s));
        // 执行整个图形：
        enumerator = RunTheGraph(nodeToExecute);
      }

      while (enumerator.MoveNext()) ;
    }

    private void WaitedRun(Stack<BaseNode> nodesToRun){
      // 执行可等待节点：
      var enumerator = RunTheGraph(nodesToRun);

      while (enumerator.MoveNext()) ;
    }

    IEnumerable<BaseNode> GatherNonConditionalDependencies(BaseNode node){
      Stack<BaseNode> dependencies = new Stack<BaseNode>();

      dependencies.Push(node);

      while (dependencies.Count > 0){
        var dependency = dependencies.Pop();

        foreach (var d in dependency.GetInputNodes().Where(n => !(n is IConditionalNode)))
          dependencies.Push(d);

        if (dependency != node)
          yield return dependency;
      }
    }

    private IEnumerator<BaseNode> RunTheGraph(){
      int count = processList.Count;

      for (int i = 0; i < count; i++){
        processList[i].OnProcess();
        yield return processList[i];
      }
    }

    private IEnumerator<BaseNode> RunTheGraph(Stack<BaseNode> nodeToExecute){
      HashSet<BaseNode> nodeDependenciesGathered = new HashSet<BaseNode>();
      HashSet<BaseNode> skipConditionalHandling = new HashSet<BaseNode>();

      while (nodeToExecute.Count > 0){
        var node = nodeToExecute.Pop();
        // TODO: maxExecutionTimeMS

        // 如果节点是条件性的，那么我们需要先执行它的非条件依赖项
        if (node is IConditionalNode && !skipConditionalHandling.Contains(node)){
          // 收集非条件依赖项：TODO，移动到缓存：
          if (nodeDependenciesGathered.Contains(node)){
            // 执行条件节点：
            node.OnProcess();
            yield return node;

            // 并选择要执行的下一个节点：
            switch (node){
              // 循环节点的特殊代码路径，因为它将多次执行相同的节点
              case ForLoopNode forLoopNode:
                forLoopNode.index = forLoopNode.start - 1; // 初始化起始索引
                foreach (var n in forLoopNode.GetExecutedNodesLoopCompleted())
                  nodeToExecute.Push(n);
                for (int i = forLoopNode.start; i < forLoopNode.end; i++){
                  foreach (var n in forLoopNode.GetExecutedNodesLoopBody())
                    nodeToExecute.Push(n);

                  nodeToExecute.Push(node); // 递增计数器
                }

                skipConditionalHandling.Add(node);
                break;
              // 可等待节点的另一个特殊情况，如"等待协程"、"等待x秒"等
              case WaitableNode waitableNode:
                foreach (var n in waitableNode.GetExecutedNodes())
                  nodeToExecute.Push(n);

                waitableNode.onProcessFinished += (waitedNode) => {
                  Stack<BaseNode> waitedNodes = new Stack<BaseNode>();
                  foreach (var n in waitedNode.GetExecuteAfterNodes())
                    waitedNodes.Push(n);
                  WaitedRun(waitedNodes);
                  waitableNode.onProcessFinished = null;
                };
                break;
              case IConditionalNode cNode:
                foreach (var n in cNode.GetExecutedNodes())
                  nodeToExecute.Push(n);
                break;
              default:
                Debug.LogError($"Conditional node {node} not handled");
                break;
            }

            nodeDependenciesGathered.Remove(node);
          }
          else{
            nodeToExecute.Push(node);
            nodeDependenciesGathered.Add(node);
            foreach (var nonConditionalNode in GatherNonConditionalDependencies(node)){
              nodeToExecute.Push(nonConditionalNode);
            }
          }
        }
        else{
          node.OnProcess();
          yield return node;
        }
      }
    }

    // 将图形的执行推进一个节点，主要用于调试。不适用于WaitableNode的executeAfter端口。
    public void Step(){
      if (currentGraphExecution == null){
        Stack<BaseNode> nodeToExecute = new Stack<BaseNode>();
        if (startNodeList.Count > 0)
          startNodeList.ForEach(s => nodeToExecute.Push(s));

        currentGraphExecution = startNodeList.Count == 0 ? RunTheGraph() : RunTheGraph(nodeToExecute);
        currentGraphExecution.MoveNext(); // 推进到第一个节点
      }
      else if (!currentGraphExecution.MoveNext())
        currentGraphExecution = null;
    }
  }
}