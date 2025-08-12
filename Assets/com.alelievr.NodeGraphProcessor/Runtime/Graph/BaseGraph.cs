using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace GraphProcessor{
  /// <summary>
  /// 图形变化记录类
  /// 用于记录图形中发生的各种变化，如节点和边的添加、删除等
  /// </summary>
  public class GraphChanges{
    /// <summary>
    /// 被移除的边
    /// </summary>
    public SerializableEdge removedEdge;

    /// <summary>
    /// 被添加的边
    /// </summary>
    public SerializableEdge addedEdge;

    /// <summary>
    /// 被移除的节点
    /// </summary>
    public BaseNode removedNode;

    /// <summary>
    /// 被添加的节点
    /// </summary>
    public BaseNode addedNode;

    /// <summary>
    /// 发生变化的节点
    /// </summary>
    public BaseNode nodeChanged;

    /// <summary>
    /// 被添加的组
    /// </summary>
    public Group addedGroups;

    /// <summary>
    /// 被移除的组
    /// </summary>
    public Group removedGroups;

    /// <summary>
    /// 被添加的堆栈节点
    /// </summary>
    public BaseStackNode addedStackNode;

    /// <summary>
    /// 被移除的堆栈节点
    /// </summary>
    public BaseStackNode removedStackNode;

    /// <summary>
    /// 被添加的便签
    /// </summary>
    public StickyNote addedStickyNotes;

    /// <summary>
    /// 被移除的便签
    /// </summary>
    public StickyNote removedStickyNotes;
  }

  /// <summary>
  /// 计算顺序类型枚举
  /// 用于确定节点计算顺序整数的计算顺序类型
  /// </summary>
  public enum ComputeOrderType{
    /// <summary>
    /// 深度优先搜索
    /// 优先处理深层依赖的节点
    /// </summary>
    DepthFirst,

    /// <summary>
    /// 广度优先搜索
    /// 优先处理同层级的节点
    /// </summary>
    BreadthFirst,
  }

  /// <summary>
  /// 图形基类
  /// 所有图形都应该继承此类，提供图形的核心功能
  /// 包括节点管理、边管理、计算顺序更新、暴露参数管理等
  /// </summary>
  [System.Serializable]
  public class BaseGraph : ScriptableObject, ISerializationCallbackReceiver{
    /// <summary>
    /// 最大计算顺序深度
    /// 防止无限递归
    /// </summary>
    static readonly int maxComputeOrderDepth = 1000;

    /// <summary>
    /// 节点在循环内时的无效计算顺序号
    /// </summary>
    public static readonly int loopComputeOrder = -2;

    /// <summary>
    /// 无法处理的节点的无效计算顺序号
    /// </summary>
    public static readonly int invalidComputeOrder = -1;

    /// <summary>
    /// 仅用于编辑器中复制粘贴的序列化节点的Json列表
    /// 注意此字段不会被序列化，已过时
    /// </summary>
    [SerializeField, Obsolete("Use BaseGraph.nodes instead")]
    public List<JsonElement> serializedNodes = new List<JsonElement>();

    /// <summary>
    /// 图形中所有节点的列表
    /// 存储图形中的所有节点对象
    /// </summary>
    [SerializeReference] public List<BaseNode> nodes = new List<BaseNode>();

    /// <summary>
    /// 通过GUID访问节点的字典，比在列表中搜索更快
    /// 提供O(1)的节点查找性能
    /// </summary>
    [System.NonSerialized] public Dictionary<string, BaseNode> nodesPerGUID = new Dictionary<string, BaseNode>();

    /// <summary>
    /// 边的列表
    /// 存储图形中所有的边对象
    /// </summary>
    [SerializeField] public List<SerializableEdge> edges = new List<SerializableEdge>();

    /// <summary>
    /// 通过GUID访问边的字典，比在列表中搜索更快
    /// 提供O(1)的边查找性能
    /// </summary>
    [System.NonSerialized]
    public Dictionary<string, SerializableEdge> edgesPerGUID = new Dictionary<string, SerializableEdge>();

    /// <summary>
    /// 图形中的所有组
    /// 用于组织和分组节点
    /// </summary>
    [SerializeField, FormerlySerializedAs("commentBlocks")]
    public List<Group> groups = new List<Group>();

    /// <summary>
    /// 图形中的所有堆栈节点
    /// 用于创建可折叠的节点组
    /// </summary>
    [SerializeField, SerializeReference] // 多态序列化
    public List<BaseStackNode> stackNodes = new List<BaseStackNode>();

    /// <summary>
    /// 图形中的所有固定元素
    /// </summary>
    /// <typeparam name="PinnedElement"></typeparam>
    /// <returns></returns>
    [SerializeField] public List<PinnedElement> pinnedElements = new List<PinnedElement>();

    /// <summary>
    /// 图形中的所有暴露参数
    /// </summary>
    /// <typeparam name="ExposedParameter"></typeparam>
    /// <returns></returns>
    [SerializeField, SerializeReference] public List<ExposedParameter> exposedParameters = new List<ExposedParameter>();

    [SerializeField, FormerlySerializedAs("exposedParameters")] // 我们保留这个用于升级
    List<ExposedParameter> serializedParameterList = new List<ExposedParameter>();

    [SerializeField] public List<StickyNote> stickyNotes = new List<StickyNote>();

    [System.NonSerialized] Dictionary<BaseNode, int> computeOrderDictionary = new Dictionary<BaseNode, int>();

    [NonSerialized] Scene linkedScene;

    // 在编辑器会话期间保持节点检查器活跃的技巧
    [SerializeField] internal UnityEngine.Object nodeInspectorReference;

    //图形视觉属性
    public Vector3 position = Vector3.zero;
    public Vector3 scale = Vector3.one;

    /// <summary>
    /// 当暴露参数列表中的某些内容发生变化时触发
    /// </summary>
    public event Action onExposedParameterListChanged;

    public event Action<ExposedParameter> onExposedParameterModified;
    public event Action<ExposedParameter> onExposedParameterValueChanged;

    /// <summary>
    /// 当图形链接到活动场景时触发。
    /// </summary>
    public event Action<Scene> onSceneLinked;

    /// <summary>
    /// 当图形启用时触发
    /// </summary>
    public event Action onEnabled;

    /// <summary>
    /// 当图形发生变化时触发
    /// </summary>
    public event Action<GraphChanges> onGraphChanges;

    [System.NonSerialized] bool _isEnabled = false;

    public bool isEnabled{
      get => _isEnabled;
      private set => _isEnabled = value;
    }

    public HashSet<BaseNode> graphOutputs{ get; private set; } = new HashSet<BaseNode>();

    protected virtual void OnEnable(){
      if (isEnabled)
        OnDisable();

      MigrateGraphIfNeeded();
      InitializeGraphElements();
      DestroyBrokenGraphElements();
      UpdateComputeOrder();
      isEnabled = true;
      onEnabled?.Invoke();
    }

    void InitializeGraphElements(){
      // 清理元素列表（如果节点的完整类名已更改，节点可能为null）
      // 如果您重命名/更改节点或参数的程序集，请使用MovedFrom()属性以避免破坏图形。
      nodes.RemoveAll(n => n == null);
      exposedParameters.RemoveAll(e => e == null);

      foreach (var node in nodes.ToList()){
        nodesPerGUID[node.GUID] = node;
        node.Initialize(this);
      }

      foreach (var edge in edges.ToList()){
        edge.Deserialize();
        edgesPerGUID[edge.GUID] = edge;

        // 边的完整性检查：
        if (edge.inputPort == null || edge.outputPort == null){
          Disconnect(edge.GUID);
          continue;
        }

        // 将边添加到非序列化的端口数据
        edge.inputPort.owner.OnEdgeConnected(edge);
        edge.outputPort.owner.OnEdgeConnected(edge);
      }
    }

    protected virtual void OnDisable(){
      isEnabled = false;
      foreach (var node in nodes)
        node.DisableInternal();
    }

    public virtual void OnAssetDeleted(){
    }

    /// <summary>
    /// 向图形添加节点
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public BaseNode AddNode(BaseNode node){
      nodesPerGUID[node.GUID] = node;

      nodes.Add(node);
      node.Initialize(this);

      onGraphChanges?.Invoke(new GraphChanges{ addedNode = node });

      return node;
    }

    /// <summary>
    /// 从图形中移除节点
    /// </summary>
    /// <param name="node"></param>
    public void RemoveNode(BaseNode node){
      node.DisableInternal();
      node.DestroyInternal();

      nodesPerGUID.Remove(node.GUID);

      nodes.Remove(node);

      onGraphChanges?.Invoke(new GraphChanges{ removedNode = node });
    }

    /// <summary>
    /// 用边连接两个端口
    /// </summary>
    /// <param name="inputPort">输入端口</param>
    /// <param name="outputPort">输出端口</param>
    /// <param name="DisconnectInputs">边是否允许断开另一个边</param>
    /// <returns>连接的边</returns>
    public SerializableEdge Connect(NodePort inputPort, NodePort outputPort, bool autoDisconnectInputs = true){
      var edge = SerializableEdge.CreateNewEdge(this, inputPort, outputPort);

      //如果输入端口不支持多连接，我们移除它们
      if (autoDisconnectInputs && !inputPort.portData.acceptMultipleEdges){
        foreach (var e in inputPort.GetEdges().ToList()){
          // TODO: 如果连接的端口与旧连接的端口相同，则不要断开它们
          Disconnect(e);
        }
      }

      // 对输出端口也是如此：
      if (autoDisconnectInputs && !outputPort.portData.acceptMultipleEdges){
        foreach (var e in outputPort.GetEdges().ToList()){
          // TODO: 如果连接的端口与旧连接的端口相同，则不要断开它们
          Disconnect(e);
        }
      }

      edges.Add(edge);

      // 将边添加到节点的连接边列表中
      inputPort.owner.OnEdgeConnected(edge);
      outputPort.owner.OnEdgeConnected(edge);

      onGraphChanges?.Invoke(new GraphChanges{ addedEdge = edge });

      return edge;
    }

    /// <summary>
    /// 断开两个端口
    /// </summary>
    /// <param name="inputNode">输入节点</param>
    /// <param name="inputFieldName">输入字段名</param>
    /// <param name="outputNode">输出节点</param>
    /// <param name="outputFieldName">输出字段名</param>
    public void Disconnect(BaseNode inputNode, string inputFieldName, BaseNode outputNode, string outputFieldName){
      edges.RemoveAll(r => {
        bool remove = r.inputNode == inputNode
                      && r.outputNode == outputNode
                      && r.outputFieldName == outputFieldName
                      && r.inputFieldName == inputFieldName;

        if (remove){
          r.inputNode?.OnEdgeDisconnected(r);
          r.outputNode?.OnEdgeDisconnected(r);
          onGraphChanges?.Invoke(new GraphChanges{ removedEdge = r });
        }

        return remove;
      });
    }

    /// <summary>
    /// 断开边
    /// </summary>
    /// <param name="edge"></param>
    public void Disconnect(SerializableEdge edge) => Disconnect(edge.GUID);

    /// <summary>
    /// 断开边
    /// </summary>
    /// <param name="edgeGUID"></param>
    public void Disconnect(string edgeGUID){
      List<(BaseNode, SerializableEdge)> disconnectEvents = new List<(BaseNode, SerializableEdge)>();

      edges.RemoveAll(r => {
        if (r.GUID == edgeGUID){
          disconnectEvents.Add((r.inputNode, r));
          disconnectEvents.Add((r.outputNode, r));
          onGraphChanges?.Invoke(new GraphChanges{ removedEdge = r });
        }

        return r.GUID == edgeGUID;
      });

      // 延迟边断开事件以避免递归
      foreach (var (node, edge) in disconnectEvents)
        node?.OnEdgeDisconnected(edge);
    }

    /// <summary>
    /// 添加组
    /// </summary>
    /// <param name="block"></param>
    public void AddGroup(Group block){
      groups.Add(block);
      onGraphChanges?.Invoke(new GraphChanges{ addedGroups = block });
    }

    /// <summary>
    /// 移除组
    /// </summary>
    /// <param name="block"></param>
    public void RemoveGroup(Group block){
      groups.Remove(block);
      onGraphChanges?.Invoke(new GraphChanges{ removedGroups = block });
    }

    /// <summary>
    /// 添加堆栈节点
    /// </summary>
    /// <param name="stackNode"></param>
    public void AddStackNode(BaseStackNode stackNode){
      stackNodes.Add(stackNode);
      onGraphChanges?.Invoke(new GraphChanges{ addedStackNode = stackNode });
    }

    /// <summary>
    /// 移除堆栈节点
    /// </summary>
    /// <param name="stackNode"></param>
    public void RemoveStackNode(BaseStackNode stackNode){
      stackNodes.Remove(stackNode);
      onGraphChanges?.Invoke(new GraphChanges{ removedStackNode = stackNode });
    }

    /// <summary>
    /// 添加便签
    /// </summary>
    /// <param name="note"></param>
    public void AddStickyNote(StickyNote note){
      stickyNotes.Add(note);
      onGraphChanges?.Invoke(new GraphChanges{ addedStickyNotes = note });
    }

    /// <summary>
    /// 移除便签
    /// </summary>
    /// <param name="note"></param>
    public void RemoveStickyNote(StickyNote note){
      stickyNotes.Remove(note);
      onGraphChanges?.Invoke(new GraphChanges{ removedStickyNotes = note });
    }

    /// <summary>
    /// 调用onGraphChanges事件，当节点内容发生变化时可用作执行图形的触发器
    /// </summary>
    /// <param name="node"></param>
    public void NotifyNodeChanged(BaseNode node) => onGraphChanges?.Invoke(new GraphChanges{ nodeChanged = node });

    /// <summary>
    /// 打开类型为viewType的固定元素
    /// </summary>
    /// <param name="viewType">固定元素的类型</param>
    /// <returns>固定元素</returns>
    public PinnedElement OpenPinned(Type viewType){
      var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

      if (pinned == null){
        pinned = new PinnedElement(viewType);
        pinnedElements.Add(pinned);
      }
      else
        pinned.opened = true;

      return pinned;
    }

    /// <summary>
    /// 关闭类型为viewType的固定元素
    /// </summary>
    /// <param name="viewType">固定元素的类型</param>
    public void ClosePinned(Type viewType){
      var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

      pinned.opened = false;
    }

    public void OnBeforeSerialize(){
      // 清理损坏的元素
      stackNodes.RemoveAll(s => s == null);
      nodes.RemoveAll(n => n == null);
    }

    // 我们可以在这里反序列化数据，因为它是在unity上下文中调用的
    // 所以我们可以加载对象引用
    public void Deserialize(){
      // 在移除节点之前正确禁用它们：
      if (nodes != null){
        foreach (var node in nodes)
          node.DisableInternal();
      }

      MigrateGraphIfNeeded();

      InitializeGraphElements();
    }

    public void MigrateGraphIfNeeded(){
#pragma warning disable CS0618
      // 从JSON序列化节点到[SerializeReference]的迁移步骤
      if (serializedNodes.Count > 0){
        nodes.Clear();
        foreach (var serializedNode in serializedNodes.ToList()){
          var node = JsonSerializer.DeserializeNode(serializedNode) as BaseNode;
          if (node != null)
            nodes.Add(node);
        }

        serializedNodes.Clear();

        // 我们在这里也迁移参数：
        var paramsToMigrate = serializedParameterList.ToList();
        exposedParameters.Clear();
        foreach (var param in paramsToMigrate){
          if (param == null)
            continue;

          var newParam = param.Migrate();

          if (newParam == null){
            Debug.LogError(
              $"Can't migrate parameter of type {param.type}, please create an Exposed Parameter class that implements this type.");
            continue;
          }
          else
            exposedParameters.Add(newParam);
        }
      }
#pragma warning restore CS0618
    }

    public void OnAfterDeserialize(){
    }

    /// <summary>
    /// 更新图形中节点的计算顺序
    /// </summary>
    /// <param name="type">计算顺序类型</param>
    public void UpdateComputeOrder(ComputeOrderType type = ComputeOrderType.DepthFirst){
      if (nodes.Count == 0)
        return;

      // 查找图形输出（结束节点）并重置计算顺序
      graphOutputs.Clear();
      foreach (var node in nodes){
        if (node.GetOutputNodes().Count() == 0)
          graphOutputs.Add(node);
        node.computeOrder = 0;
      }

      computeOrderDictionary.Clear();
      infiniteLoopTracker.Clear();

      switch (type){
        default:
        case ComputeOrderType.DepthFirst:
          UpdateComputeOrderDepthFirst();
          break;
        case ComputeOrderType.BreadthFirst:
          foreach (var node in nodes)
            UpdateComputeOrderBreadthFirst(0, node);
          break;
      }
    }

    /// <summary>
    /// 添加暴露参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="type">参数类型（必须是ExposedParameter的子类）</param>
    /// <param name="value">默认值</param>
    /// <returns>参数的唯一id</returns>
    public string AddExposedParameter(string name, Type type, object value = null){
      if (!type.IsSubclassOf(typeof(ExposedParameter))){
        Debug.LogError($"Can't add parameter of type {type}, the type doesn't inherit from ExposedParameter.");
      }

      var param = Activator.CreateInstance(type) as ExposedParameter;

      // 用正确的类型修补值：
      if (param.GetValueType().IsValueType)
        value = Activator.CreateInstance(param.GetValueType());

      param.Initialize(name, value);
      exposedParameters.Add(param);

      onExposedParameterListChanged?.Invoke();

      return param.guid;
    }

    /// <summary>
    /// 向图形添加已分配/初始化的参数
    /// </summary>
    /// <param name="parameter">要添加的参数</param>
    /// <returns>参数的唯一id</returns>
    public string AddExposedParameter(ExposedParameter parameter){
      string guid = Guid.NewGuid().ToString(); // 每个参数生成一次且唯一

      parameter.guid = guid;
      exposedParameters.Add(parameter);

      onExposedParameterListChanged?.Invoke();

      return guid;
    }

    /// <summary>
    /// 移除暴露参数
    /// </summary>
    /// <param name="ep">要移除的参数</param>
    public void RemoveExposedParameter(ExposedParameter ep){
      exposedParameters.Remove(ep);

      onExposedParameterListChanged?.Invoke();
    }

    /// <summary>
    /// 移除暴露参数
    /// </summary>
    /// <param name="guid">参数的GUID</param>
    public void RemoveExposedParameter(string guid){
      if (exposedParameters.RemoveAll(e => e.guid == guid) != 0)
        onExposedParameterListChanged?.Invoke();
    }

    internal void NotifyExposedParameterListChanged()
      => onExposedParameterListChanged?.Invoke();

    /// <summary>
    /// 更新暴露参数值
    /// </summary>
    /// <param name="guid">参数的GUID</param>
    /// <param name="value">新值</param>
    public void UpdateExposedParameter(string guid, object value){
      var param = exposedParameters.Find(e => e.guid == guid);
      if (param == null)
        return;

      if (value != null && !param.GetValueType().IsAssignableFrom(value.GetType()))
        throw new Exception("Type mismatch when updating parameter " + param.name + ": from " + param.GetValueType() +
                            " to " + value.GetType().AssemblyQualifiedName);

      param.value = value;
      onExposedParameterModified?.Invoke(param);
    }

    /// <summary>
    /// 更新暴露参数名称
    /// </summary>
    /// <param name="parameter">参数</param>
    /// <param name="name">新名称</param>
    public void UpdateExposedParameterName(ExposedParameter parameter, string name){
      parameter.name = name;
      onExposedParameterModified?.Invoke(parameter);
    }

    /// <summary>
    /// 更新参数可见性
    /// </summary>
    /// <param name="parameter">参数</param>
    /// <param name="isHidden">是否隐藏</param>
    public void NotifyExposedParameterChanged(ExposedParameter parameter){
      onExposedParameterModified?.Invoke(parameter);
    }

    public void NotifyExposedParameterValueChanged(ExposedParameter parameter){
      onExposedParameterValueChanged?.Invoke(parameter);
    }

    /// <summary>
    /// 从名称获取暴露参数
    /// </summary>
    /// <param name="name">名称</param>
    /// <returns>参数或null</returns>
    public ExposedParameter GetExposedParameter(string name){
      return exposedParameters.FirstOrDefault(e => e.name == name);
    }

    /// <summary>
    /// 从GUID获取暴露参数
    /// </summary>
    /// <param name="guid">参数的GUID</param>
    /// <returns>参数</returns>
    public ExposedParameter GetExposedParameterFromGUID(string guid){
      return exposedParameters.FirstOrDefault(e => e?.guid == guid);
    }

    /// <summary>
    /// 从名称设置参数值。（警告：参数名称可以被用户更改）
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">新值</param>
    /// <returns>如果值已被分配则为true</returns>
    public bool SetParameterValue(string name, object value){
      var e = exposedParameters.FirstOrDefault(p => p.name == name);

      if (e == null)
        return false;

      e.value = value;

      return true;
    }

    /// <summary>
    /// 获取参数值
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <returns>值</returns>
    public object GetParameterValue(string name) => exposedParameters.FirstOrDefault(p => p.name == name)?.value;

    /// <summary>
    /// 获取参数值模板
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <typeparam name="T">参数类型</typeparam>
    /// <returns>值</returns>
    public T GetParameterValue<T>(string name) => (T)GetParameterValue(name);

    /// <summary>
    /// 将当前图形链接到参数中的场景，允许图形从场景中选择和序列化对象。
    /// </summary>
    /// <param name="scene">要链接的目标场景</param>
    public void LinkToScene(Scene scene){
      linkedScene = scene;
      onSceneLinked?.Invoke(scene);
    }

    /// <summary>
    /// 当图形链接到场景时返回true，否则返回false。
    /// </summary>
    public bool IsLinkedToScene() => linkedScene.IsValid();

    /// <summary>
    /// 获取链接的场景。如果没有链接的场景，则返回无效场景
    /// </summary>
    public Scene GetLinkedScene() => linkedScene;

    HashSet<BaseNode> infiniteLoopTracker = new HashSet<BaseNode>();

    int UpdateComputeOrderBreadthFirst(int depth, BaseNode node){
      int computeOrder = 0;

      if (depth > maxComputeOrderDepth){
        Debug.LogError("Recursion error while updating compute order");
        return -1;
      }

      if (computeOrderDictionary.ContainsKey(node))
        return node.computeOrder;

      if (!infiniteLoopTracker.Add(node))
        return -1;

      if (!node.canProcess){
        node.computeOrder = -1;
        computeOrderDictionary[node] = -1;
        return -1;
      }

      foreach (var dep in node.GetInputNodes()){
        int c = UpdateComputeOrderBreadthFirst(depth + 1, dep);

        if (c == -1){
          computeOrder = -1;
          break;
        }

        computeOrder += c;
      }

      if (computeOrder != -1)
        computeOrder++;

      node.computeOrder = computeOrder;
      computeOrderDictionary[node] = computeOrder;

      return computeOrder;
    }

    void UpdateComputeOrderDepthFirst(){
      Stack<BaseNode> dfs = new Stack<BaseNode>();

      GraphUtils.FindCyclesInGraph(this, (n) => { PropagateComputeOrder(n, loopComputeOrder); });

      int computeOrder = 0;
      foreach (var node in GraphUtils.DepthFirstSort(this)){
        if (node.computeOrder == loopComputeOrder)
          continue;
        if (!node.canProcess)
          node.computeOrder = -1;
        else
          node.computeOrder = computeOrder++;
      }
    }

    void PropagateComputeOrder(BaseNode node, int computeOrder){
      Stack<BaseNode> deps = new Stack<BaseNode>();
      HashSet<BaseNode> loop = new HashSet<BaseNode>();

      deps.Push(node);
      while (deps.Count > 0){
        var n = deps.Pop();
        n.computeOrder = computeOrder;

        if (!loop.Add(n))
          continue;

        foreach (var dep in n.GetOutputNodes())
          deps.Push(dep);
      }
    }

    void DestroyBrokenGraphElements(){
      edges.RemoveAll(e => e.inputNode == null
                           || e.outputNode == null
                           || string.IsNullOrEmpty(e.outputFieldName)
                           || string.IsNullOrEmpty(e.inputFieldName)
      );
      nodes.RemoveAll(n => n == null);
    }

    /// <summary>
    /// 判断在图形上下文中两个类型是否可以连接
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static bool TypesAreConnectable(Type t1, Type t2){
      if (t1 == null || t2 == null)
        return false;

      if (TypeAdapter.AreIncompatible(t1, t2))
        return false;

      //检查是否有用于此分配的自定义适配器
      if (CustomPortIO.IsAssignable(t1, t2))
        return true;

      //检查类型可分配性
      if (t2.IsReallyAssignableFrom(t1))
        return true;

      // 用户定义的类型转换
      if (TypeAdapter.AreAssignable(t1, t2))
        return true;

      return false;
    }
  }
}