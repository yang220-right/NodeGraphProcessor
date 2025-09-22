using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

namespace GraphProcessor{
  /// <summary>
  /// 自定义端口行为委托
  /// 用于定义端口的动态行为，根据连接的边来生成端口数据
  /// </summary>
  /// <param name="edges">与端口相连的边列表</param>
  /// <returns>端口数据集合</returns>
  public delegate IEnumerable<PortData> CustomPortBehaviorDelegate(List<SerializableEdge> edges);

  /// <summary>
  /// 自定义端口类型行为委托
  /// 用于根据字段名、显示名和值来生成端口数据
  /// </summary>
  /// <param name="fieldName">字段名</param>
  /// <param name="displayName">显示名称</param>
  /// <param name="value">字段值</param>
  /// <returns>端口数据集合</returns>
  public delegate IEnumerable<PortData> CustomPortTypeBehaviorDelegate(string fieldName, string displayName,
    object value);

  /// <summary>
  /// 节点基类
  /// 所有图形节点都应该继承此类，提供节点的基本功能和属性
  /// 包括端口管理、事件处理、生命周期管理等核心功能
  /// </summary>
  [Serializable]
  public abstract class BaseNode{
    /// <summary>
    /// 节点的自定义名称
    /// 当用户重命名节点时存储的自定义名称
    /// </summary>
    [SerializeField] internal string nodeCustomName = null;

    /// <summary>
    /// 节点的名称，将显示在标题部分
    /// 默认返回类型名称，子类可以重写此属性
    /// </summary>
    /// <returns>节点显示名称</returns>
    public virtual string name => GetType().Name;

    /// <summary>
    /// 节点的强调色
    /// 用于在图形视图中突出显示节点
    /// </summary>
    public virtual Color color => Color.clear;

    /// <summary>
    /// 为节点设置自定义USS文件路径
    /// 使用Resources.Load来获取样式表，请确保放置正确的资源路径
    /// https://docs.unity3d.com/ScriptReference/Resources.Load.html
    /// </summary>
    public virtual string layoutStyle => string.Empty;

    /// <summary>
    /// 节点是否可以锁定
    /// 控制节点是否可以被锁定以防止移动
    /// </summary>
    public virtual bool unlockable => true;

    /// <summary>
    /// 节点是否被锁定（如果锁定则无法移动）
    /// </summary>
    public virtual bool isLocked => nodeLock;

    /// <summary>
    /// 节点的唯一标识符
    /// </summary>
    public string GUID;

    /// <summary>
    /// 节点的计算顺序
    /// 用于确定节点在图形中的执行顺序，-1表示未设置
    /// </summary>
    public int computeOrder = -1;

    /// <summary>
    /// 告诉节点是否可以处理
    /// 不要检查输入的任何内容，因为此步骤发生在输入发送到节点之前
    /// </summary>
    public virtual bool canProcess => true;

    /// <summary>
    /// 仅当鼠标悬停在节点上时显示节点控制容器
    /// 用于控制节点UI的显示行为
    /// </summary>
    public virtual bool showControlsOnHover => false;

    /// <summary>
    /// 如果节点可以删除则为true，否则为false
    /// 控制节点是否可以被用户删除
    /// </summary>
    public virtual bool deletable => true;

    /// <summary>
    /// 输入端口容器
    /// 管理节点的所有输入端口
    /// </summary>
    [NonSerialized] public readonly NodeInputPortContainer inputPorts;

    /// <summary>
    /// 输出端口容器
    /// 管理节点的所有输出端口
    /// </summary>
    [NonSerialized] public readonly NodeOutputPortContainer outputPorts;

    /// <summary>
    /// 节点在图形中的位置
    /// </summary>
    public Rect position;

    /// <summary>
    /// 节点是否展开
    /// 控制节点在UI中的展开/折叠状态
    /// </summary>
    public bool expanded;

    /// <summary>
    /// 调试信息是否可见
    /// 控制是否显示节点的调试信息
    /// </summary>
    public bool debug;

    /// <summary>
    /// 节点锁定状态
    /// 控制节点是否被锁定
    /// </summary>
    public bool nodeLock;

    /// <summary>
    /// 处理委托类型
    /// </summary>
    public delegate void ProcessDelegate();

    /// <summary>
    /// 当节点被处理时触发的事件
    /// </summary>
    public event ProcessDelegate onProcessed;

    /// <summary>
    /// 当添加消息时触发的事件
    /// </summary>
    public event Action<string, NodeMessageType> onMessageAdded;

    /// <summary>
    /// 当移除消息时触发的事件
    /// </summary>
    public event Action<string> onMessageRemoved;

    /// <summary>
    /// 在节点上连接边后触发的事件
    /// </summary>
    public event Action<SerializableEdge> onAfterEdgeConnected;

    /// <summary>
    /// 在节点上断开边后触发的事件
    /// </summary>
    public event Action<SerializableEdge> onAfterEdgeDisconnected;

    /// <summary>
    /// 当端口更新时触发的事件
    /// </summary>
    public event Action<string> onPortsUpdated;

    [NonSerialized] bool _needsInspector = false;

    /// <summary>
    /// 节点是否需要在检查器中可见（当被选中时）。
    /// </summary>
    public virtual bool needsInspector => _needsInspector;

    /// <summary>
    /// 节点是否可以在UI中重命名。默认情况下，节点可以通过双击其名称来重命名。
    /// </summary>
    public virtual bool isRenamable => false;

    /// <summary>
    /// 节点是否是从重复操作创建的（ctrl-D 或复制/粘贴）。
    /// </summary>
    public bool createdFromDuplication{ get; internal set; } = false;

    /// <summary>
    /// 仅当节点是从重复操作创建的，并且位于同时被复制的组内时为true。
    /// </summary>
    public bool createdWithinGroup{ get; internal set; } = false;

    [NonSerialized]
    internal Dictionary<string, NodeFieldInformation> nodeFields = new Dictionary<string, NodeFieldInformation>();

    [NonSerialized] internal Dictionary<Type, CustomPortTypeBehaviorDelegate> customPortTypeBehaviorMap =
      new Dictionary<Type, CustomPortTypeBehaviorDelegate>();

    [NonSerialized] List<string> messages = new List<string>();

    [NonSerialized] protected BaseGraph graph;

    internal class NodeFieldInformation{
      public string name;
      public string fieldName;
      public FieldInfo info;
      public bool input;
      public bool isMultiple;
      public string tooltip;
      public CustomPortBehaviorDelegate behavior;
      public bool vertical;

      public NodeFieldInformation(FieldInfo info, string name, bool input, bool isMultiple, string tooltip,
        bool vertical, CustomPortBehaviorDelegate behavior){
        this.input = input;
        this.isMultiple = isMultiple;
        this.info = info;
        this.name = name;
        this.fieldName = info.Name;
        this.behavior = behavior;
        this.tooltip = tooltip;
        this.vertical = vertical;
      }
    }

    struct PortUpdate{
      public List<string> fieldNames;
      public BaseNode node;

      public void Deconstruct(out List<string> fieldNames, out BaseNode node){
        fieldNames = this.fieldNames;
        node = this.node;
      }
    }

    // 用于端口更新算法
    Stack<PortUpdate> fieldsToUpdate = new Stack<PortUpdate>();
    HashSet<PortUpdate> updatedFields = new HashSet<PortUpdate>();

    /// <summary>
    /// 在指定位置创建类型为T的节点
    /// </summary>
    /// <param name="position">图形中的位置（像素）</param>
    /// <typeparam name="T">节点类型</typeparam>
    /// <returns>节点实例</returns>
    public static T CreateFromType<T>(Vector2 position) where T : BaseNode{
      return CreateFromType(typeof(T), position) as T;
    }

    /// <summary>
    /// 在指定位置创建指定类型的节点
    /// </summary>
    /// <param name="position">图形中的位置（像素）</param>
    /// <typeparam name="nodeType">节点类型</typeparam>
    /// <returns>节点实例</returns>
    public static BaseNode CreateFromType(Type nodeType, Vector2 position){
      if (!nodeType.IsSubclassOf(typeof(BaseNode)))
        return null;

      var node = Activator.CreateInstance(nodeType) as BaseNode;

      node.position = new Rect(position, new Vector2(100, 100));

      ExceptionToLog.Call(node.OnNodeCreated);

      return node;
    }

    #region Initialization

    // 当节点添加到图形时由BaseGraph调用
    public void Initialize(BaseGraph graph){
      this.graph = graph;

      ExceptionToLog.Call(Enable);

      InitializePorts();
    }

    void InitializeCustomPortTypeMethods(){
      MethodInfo[] methods = new MethodInfo[0];
      Type baseType = GetType();
      while (true){
        methods = baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var method in methods){
          var typeBehaviors = method.GetCustomAttributes<CustomPortTypeBehavior>().ToArray();

          if (typeBehaviors.Length == 0)
            continue;

          CustomPortTypeBehaviorDelegate deleg = null;
          try{
            deleg =
              Delegate.CreateDelegate(typeof(CustomPortTypeBehaviorDelegate), this, method) as
                CustomPortTypeBehaviorDelegate;
          }
          catch (Exception e){
            Debug.LogError(e);
            Debug.LogError(
              $"Cannot convert method {method} to a delegate of type {typeof(CustomPortTypeBehaviorDelegate)}");
          }

          foreach (var typeBehavior in typeBehaviors)
            customPortTypeBehaviorMap[typeBehavior.type] = deleg;
        }

        // 也尝试在基类中查找私有方法
        baseType = baseType.BaseType;
        if (baseType == null)
          break;
      }
    }

    /// <summary>
    /// 使用此函数初始化与节点端口生成相关的任何内容
    /// 这将允许节点创建菜单正确识别可以在节点之间连接的端口
    /// </summary>
    public virtual void InitializePorts(){
      InitializeCustomPortTypeMethods();

      foreach (var key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info))){
        var nodeField = nodeFields[key.Name];

        if (HasCustomBehavior(nodeField)){
          UpdatePortsForField(nodeField.fieldName, sendPortUpdatedEvent: false);
        }
        else{
          // 如果节点上没有自定义行为，我们只需要创建一个简单的端口
          AddPort(nodeField.input, nodeField.fieldName,
            new PortData{
              acceptMultipleEdges = nodeField.isMultiple, displayName = nodeField.name, tooltip = nodeField.tooltip,
              vertical = nodeField.vertical
            });
        }
      }
    }

    /// <summary>
    /// 重写节点内字段的顺序。它允许重新排序UI中的所有端口和字段。
    /// </summary>
    /// <param name="fields">要排序的字段列表</param>
    /// <returns>排序后的字段列表</returns>
    public virtual IEnumerable<FieldInfo> OverrideFieldOrder(IEnumerable<FieldInfo> fields){
      long GetFieldInheritanceLevel(FieldInfo f){
        int level = 0;
        var t = f.DeclaringType;
        while (t != null){
          t = t.BaseType;
          level++;
        }

        return level;
      }

      // 按MetadataToken和继承级别排序，以与端口顺序同步（确保FieldDrawers在正确端口旁边）
      return fields.OrderByDescending(f => (long)(((GetFieldInheritanceLevel(f) << 32)) | (long)f.MetadataToken));
    }

    protected BaseNode(){
      inputPorts = new NodeInputPortContainer(this);
      outputPorts = new NodeOutputPortContainer(this);

      InitializeInOutDatas();
    }

    /// <summary>
    /// 更新节点的所有端口
    /// </summary>
    public bool UpdateAllPorts(){
      bool changed = false;

      foreach (var key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info))){
        var field = nodeFields[key.Name];
        changed |= UpdatePortsForField(field.fieldName);
      }

      return changed;
    }

    /// <summary>
    /// 更新节点的所有端口，但不更新连接的端口。仅当您需要在图形中更新所有节点端口时使用此方法。
    /// </summary>
    public bool UpdateAllPortsLocal(){
      bool changed = false;

      foreach (var key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info))){
        var field = nodeFields[key.Name];
        changed |= UpdatePortsForFieldLocal(field.fieldName);
      }

      return changed;
    }


    /// <summary>
    /// 更新与一个C#属性字段相关的端口（仅限此节点）
    /// </summary>
    /// <param name="fieldName"></param>
    public bool UpdatePortsForFieldLocal(string fieldName, bool sendPortUpdatedEvent = true){
      bool changed = false;

      if (!nodeFields.ContainsKey(fieldName))
        return false;

      var fieldInfo = nodeFields[fieldName];

      if (!HasCustomBehavior(fieldInfo))
        return false;

      List<string> finalPorts = new List<string>();

      var portCollection = fieldInfo.input ? (NodePortContainer)inputPorts : outputPorts;

      // 收集此端口的所有字段（在修改之前）
      var nodePorts = portCollection.Where(p => p.fieldName == fieldName);
      // 收集连接到这些字段的所有边：
      var edges = nodePorts.SelectMany(n => n.GetEdges()).ToList();

      if (fieldInfo.behavior != null){
        foreach (var portData in fieldInfo.behavior(edges))
          AddPortData(portData);
      }
      else{
        var customPortTypeBehavior = customPortTypeBehaviorMap[fieldInfo.info.FieldType];

        foreach (var portData in customPortTypeBehavior(fieldName, fieldInfo.name, fieldInfo.info.GetValue(this)))
          AddPortData(portData);
      }

      void AddPortData(PortData portData){
        var port = nodePorts.FirstOrDefault(n => n.portData.identifier == portData.identifier);
        // 使用端口标识符进行保护，这样我们就不会重复标识符
        if (port == null){
          AddPort(fieldInfo.input, fieldName, portData);
          changed = true;
        }
        else{
          // 如果端口类型已更改为不兼容类型，我们断开连接到此端口的所有边
          if (!BaseGraph.TypesAreConnectable(port.portData.displayType, portData.displayType)){
            foreach (var edge in port.GetEdges().ToList())
              graph.Disconnect(edge.GUID);
          }

          // 修补端口数据
          if (port.portData != portData){
            port.portData.CopyFrom(portData);
            changed = true;
          }
        }

        finalPorts.Add(portData.identifier);
      }

      // 待办事项
      // 仅移除不再在列表中的端口
      if (nodePorts != null){
        var currentPortsCopy = nodePorts.ToList();
        foreach (var currentPort in currentPortsCopy){
          // 如果当前端口没有出现在最终端口列表中，我们将其移除
          if (!finalPorts.Any(id => id == currentPort.portData.identifier)){
            RemovePort(fieldInfo.input, currentPort);
            changed = true;
          }
        }
      }

      // 确保端口顺序正确：
      portCollection.Sort((p1, p2) => {
        int p1Index = finalPorts.FindIndex(id => p1.portData.identifier == id);
        int p2Index = finalPorts.FindIndex(id => p2.portData.identifier == id);

        if (p1Index == -1 || p2Index == -1)
          return 0;

        return p1Index.CompareTo(p2Index);
      });

      if (sendPortUpdatedEvent)
        onPortsUpdated?.Invoke(fieldName);

      return changed;
    }

    bool HasCustomBehavior(NodeFieldInformation info){
      if (info.behavior != null)
        return true;

      if (customPortTypeBehaviorMap.ContainsKey(info.info.FieldType))
        return true;

      return false;
    }

    /// <summary>
    /// 更新与一个C#属性字段和所有连接节点相关的端口（在图形中）
    /// </summary>
    /// <param name="fieldName"></param>
    public bool UpdatePortsForField(string fieldName, bool sendPortUpdatedEvent = true){
      bool changed = false;

      fieldsToUpdate.Clear();
      updatedFields.Clear();

      fieldsToUpdate.Push(new PortUpdate{ fieldNames = new List<string>(){ fieldName }, node = this });

      // 遍历需要更新的所有端口，当端口更新时跟随图形连接。
      // 这对于具有类型传播的多个节点是必需的，这些节点更改端口类型并相互连接（即中继节点）
      while (fieldsToUpdate.Count != 0){
        var (fields, node) = fieldsToUpdate.Pop();

        // 避免更新两次端口
        if (updatedFields.Any((t) => t.node == node && fields.SequenceEqual(t.fieldNames)))
          continue;
        updatedFields.Add(new PortUpdate{ fieldNames = fields, node = node });

        foreach (var field in fields){
          if (node.UpdatePortsForFieldLocal(field, sendPortUpdatedEvent)){
            foreach (var port in node.IsFieldInput(field) ? (NodePortContainer)node.inputPorts : node.outputPorts){
              if (port.fieldName != field)
                continue;

              foreach (var edge in port.GetEdges()){
                var edgeNode = (node.IsFieldInput(field)) ? edge.outputNode : edge.inputNode;
                var fieldsWithBehavior = edgeNode.nodeFields.Values.Where(f => HasCustomBehavior(f))
                  .Select(f => f.fieldName).ToList();
                fieldsToUpdate.Push(new PortUpdate{ fieldNames = fieldsWithBehavior, node = edgeNode });
              }
            }

            changed = true;
          }
        }
      }

      return changed;
    }

    HashSet<BaseNode> portUpdateHashSet = new HashSet<BaseNode>();

    internal void DisableInternal(){
      // 端口容器在OnEnable中初始化
      inputPorts.Clear();
      outputPorts.Clear();

      ExceptionToLog.Call(Disable);
    }

    internal void DestroyInternal() => ExceptionToLog.Call(Destroy);

    /// <summary>
    /// 仅当节点被创建时调用，而不是实例化时
    /// </summary>
    public virtual void OnNodeCreated() => GUID = Guid.NewGuid().ToString();

    public virtual FieldInfo[] GetNodeFields()
      => GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    void InitializeInOutDatas(){
      var fields = GetNodeFields();
      var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

      foreach (var field in fields){
        var inputAttribute = field.GetCustomAttribute<InputAttribute>();
        var outputAttribute = field.GetCustomAttribute<OutputAttribute>();
        var tooltipAttribute = field.GetCustomAttribute<TooltipAttribute>();
        var showInInspector = field.GetCustomAttribute<ShowInInspector>();
        var vertical = field.GetCustomAttribute<VerticalAttribute>();
        bool isMultiple = false;
        bool input = false;
        string name = field.Name;
        string tooltip = null;

        if (showInInspector != null)
          _needsInspector = true;

        if (inputAttribute == null && outputAttribute == null)
          continue;

        // 检查字段是否为集合类型
        isMultiple = (inputAttribute != null) ? inputAttribute.allowMultiple : (outputAttribute.allowMultiple);
        input = inputAttribute != null;
        tooltip = tooltipAttribute?.tooltip;

        if (!String.IsNullOrEmpty(inputAttribute?.name))
          name = inputAttribute.name;
        if (!String.IsNullOrEmpty(outputAttribute?.name))
          name = outputAttribute.name;

        // 默认情况下我们将行为设置为null，如果字段有自定义行为，它将在下面的循环中设置
        nodeFields[field.Name] =
          new NodeFieldInformation(field, name, input, isMultiple, tooltip, vertical != null, null);
      }

      foreach (var method in methods){
        var customPortBehaviorAttribute = method.GetCustomAttribute<CustomPortBehaviorAttribute>();
        CustomPortBehaviorDelegate behavior = null;

        if (customPortBehaviorAttribute == null)
          continue;

        // 检查自定义端口行为函数是否有效
        try{
          var referenceType = typeof(CustomPortBehaviorDelegate);
          behavior = (CustomPortBehaviorDelegate)Delegate.CreateDelegate(referenceType, this, method, true);
        }
        catch{
          Debug.LogError("The function " + method + " cannot be converted to the required delegate format: " +
                         typeof(CustomPortBehaviorDelegate));
        }

        if (nodeFields.ContainsKey(customPortBehaviorAttribute.fieldName))
          nodeFields[customPortBehaviorAttribute.fieldName].behavior = behavior;
        else
          Debug.LogError("Invalid field name for custom port behavior: " + method + ", " +
                         customPortBehaviorAttribute.fieldName);
      }
    }

    #endregion

    #region Events and Processing

    public void OnEdgeConnected(SerializableEdge edge){
      bool input = edge.inputNode == this;
      NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;

      portCollection.Add(edge);

      UpdateAllPorts();

      onAfterEdgeConnected?.Invoke(edge);
    }

    protected virtual bool CanResetPort(NodePort port) => true;

    public void OnEdgeDisconnected(SerializableEdge edge){
      if (edge == null)
        return;

      bool input = edge.inputNode == this;
      NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;

      portCollection.Remove(edge);

      // 重置输入端口的默认值：
      bool haveConnectedEdges = edge.inputNode.inputPorts.Where(p => p.fieldName == edge.inputFieldName)
        .Any(p => p.GetEdges().Count != 0);
      if (edge.inputNode == this && !haveConnectedEdges && CanResetPort(edge.inputPort))
        edge.inputPort?.ResetToDefault();

      UpdateAllPorts();

      onAfterEdgeDisconnected?.Invoke(edge);
    }

    public void OnProcess(){
      inputPorts.PullDatas();

      ExceptionToLog.Call(Process);

      InvokeOnProcessed();

      outputPorts.PushDatas();
    }

    public void InvokeOnProcessed() => onProcessed?.Invoke();

    /// <summary>
    /// 当节点启用时调用
    /// </summary>
    protected virtual void Enable(){
    }

    /// <summary>
    /// 当节点禁用时调用
    /// </summary>
    protected virtual void Disable(){
    }

    /// <summary>
    /// 当节点移除时调用
    /// </summary>
    protected virtual void Destroy(){
    }

    /// <summary>
    /// 重写此方法以实现自定义处理
    /// </summary>
    protected virtual void Process(){
    }

    #endregion

    #region API and utils

    /// <summary>
    /// 添加端口
    /// </summary>
    /// <param name="input">是否为输入端口</param>
    /// <param name="fieldName">C#字段名</param>
    /// <param name="portData">端口数据</param>
    public void AddPort(bool input, string fieldName, PortData portData)
    {
      // 如果需要，修复端口数据信息：
      portData.displayType ??= nodeFields[fieldName].info.FieldType;

      if (input)
        inputPorts.Add(new NodePort(this, fieldName, portData));
      else
        outputPorts.Add(new NodePort(this, fieldName, portData));
    }

    /// <summary>
    /// 移除端口
    /// </summary>
    /// <param name="input">是否为输入端口</param>
    /// <param name="port">要删除的端口</param>
    public void RemovePort(bool input, NodePort port){
      if (input)
        inputPorts.Remove(port);
      else
        outputPorts.Remove(port);
    }

    /// <summary>
    /// 从字段名移除端口
    /// </summary>
    /// <param name="input">是否为输入</param>
    /// <param name="fieldName">C#字段名</param>
    public void RemovePort(bool input, string fieldName){
      if (input)
        inputPorts.RemoveAll(p => p.fieldName == fieldName);
      else
        outputPorts.RemoveAll(p => p.fieldName == fieldName);
    }

    /// <summary>
    /// 获取此节点所有输入端口连接到的节点
    /// </summary>
    /// <returns>一个节点枚举</returns>
    public IEnumerable<BaseNode> GetInputNodes(){
      foreach (var port in inputPorts)
        foreach (var edge in port.GetEdges())
          yield return edge.outputNode;
    }

    /// <summary>
    /// 获取此节点所有输出端口连接到的节点
    /// </summary>
    /// <returns>一个节点枚举</returns>
    public IEnumerable<BaseNode> GetOutputNodes(){
      foreach (var port in outputPorts)
        foreach (var edge in port.GetEdges())
          yield return edge.inputNode;
    }

    /// <summary>
    /// 根据节点的依赖关系查找符合条件的节点
    /// </summary>
    /// <param name="condition">选择节点的条件</param>
    /// <returns>匹配的节点或null</returns>
    public BaseNode FindInDependencies(Func<BaseNode, bool> condition){
      Stack<BaseNode> dependencies = new Stack<BaseNode>();

      dependencies.Push(this);

      int depth = 0;
      while (dependencies.Count > 0){
        var node = dependencies.Pop();

        // 防止无限循环的保护（比基于HashSet的解决方案更快）
        depth++;
        if (depth > 2000)
          break;

        if (condition(node))
          return node;

        foreach (var dep in node.GetInputNodes())
          dependencies.Push(dep);
      }

      return null;
    }

    /// <summary>
    /// 根据字段名和标识符获取端口
    /// </summary>
    /// <param name="fieldName">C#字段名</param>
    /// <param name="identifier">唯一端口标识符</param>
    /// <returns></returns>
    public NodePort GetPort(string fieldName, string identifier){
      return inputPorts.Concat(outputPorts).FirstOrDefault(p => {
        var bothNull = String.IsNullOrEmpty(identifier) && String.IsNullOrEmpty(p.portData.identifier);
        return p.fieldName == fieldName && (bothNull || identifier == p.portData.identifier);
      });
    }

    /// <summary>
    /// 返回节点的所有端口
    /// </summary>
    /// <returns></returns>
    public IEnumerable<NodePort> GetAllPorts(){
      foreach (var port in inputPorts)
        yield return port;
      foreach (var port in outputPorts)
        yield return port;
    }

    /// <summary>
    /// 返回节点的所有连接边
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SerializableEdge> GetAllEdges(){
      foreach (var port in GetAllPorts())
        foreach (var edge in port.GetEdges())
          yield return edge;
    }

    /// <summary>
    /// 是否为输入端口
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public bool IsFieldInput(string fieldName) => nodeFields[fieldName].input;

    /// <summary>
    /// 在节点上添加消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    public void AddMessage(string message, NodeMessageType messageType){
      if (messages.Contains(message))
        return;

      onMessageAdded?.Invoke(message, messageType);
      messages.Add(message);
    }

    /// <summary>
    /// 从节点移除消息
    /// </summary>
    /// <param name="message"></param>
    public void RemoveMessage(string message){
      onMessageRemoved?.Invoke(message);
      messages.Remove(message);
    }

    /// <summary>
    /// 移除包含的消息
    /// </summary>
    /// <param name="subMessage"></param>
    public void RemoveMessageContains(string subMessage){
      string toRemove = messages.Find(m => m.Contains(subMessage));
      messages.Remove(toRemove);
      onMessageRemoved?.Invoke(toRemove);
    }

    /// <summary>
    /// 清除节点的所有消息
    /// </summary>
    public void ClearMessages(){
      foreach (var message in messages)
        onMessageRemoved?.Invoke(message);
      messages.Clear();
    }

    /// <summary>
    /// 设置节点的自定义名称。此自定义名称将序列化到节点中。
    /// </summary>
    /// <param name="customNodeName">节点的自定义名称。</param>
    public void SetCustomName(string customName) => nodeCustomName = customName;

    /// <summary>
    /// 获取节点的名称。如果节点具有自定义名称（通过双击节点标题在UI中设置），则它将首先返回此名称，否则返回名称字段值。
    /// </summary>
    /// <returns>节点标题中写入的名称</returns>
    public string GetCustomName() => String.IsNullOrEmpty(nodeCustomName) ? name : nodeCustomName;

    #endregion
  }
}