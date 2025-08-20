// #define DEBUG_LAMBDA

using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq.Expressions;
using System;

namespace GraphProcessor{
  /// <summary>
  /// 端口数据类
  /// 描述端口创建属性的类，包含端口的显示、行为和连接信息
  /// </summary>
  public class PortData : IEquatable<PortData>{
    /// <summary>
    /// 端口的唯一标识符
    /// 用于在代码中唯一标识端口
    /// </summary>
    public string identifier;

    /// <summary>
    /// 节点上的显示名称
    /// 在图形界面中显示的端口名称
    /// </summary>
    public string displayName;

    /// <summary>
    /// 将用于类型样式表着色的类型
    /// 决定端口的颜色和样式
    /// </summary>
    public Type displayType;

    /// <summary>
    /// 端口是否接受多个连接
    /// 控制端口是否可以连接多条边
    /// </summary>
    public bool acceptMultipleEdges;

    /// <summary>
    /// 端口大小，也会影响连接边的大小
    /// 以像素为单位的端口显示大小
    /// </summary>
    public int sizeInPixel;

    /// <summary>
    /// 端口的工具提示
    /// 鼠标悬停时显示的提示信息
    /// </summary>
    public string tooltip;

    /// <summary>
    /// 端口是否垂直
    /// 控制端口的显示方向
    /// </summary>
    public bool vertical;

    /// <summary>
    /// 比较两个PortData是否相等
    /// </summary>
    /// <param name="other">要比较的PortData</param>
    /// <returns>如果相等则返回true</returns>
    public bool Equals(PortData other){
      return identifier == other.identifier
             && displayName == other.displayName
             && displayType == other.displayType
             && acceptMultipleEdges == other.acceptMultipleEdges
             && sizeInPixel == other.sizeInPixel
             && tooltip == other.tooltip
             && vertical == other.vertical;
    }

    /// <summary>
    /// 从另一个PortData复制数据
    /// </summary>
    /// <param name="other">要复制的PortData</param>
    public void CopyFrom(PortData other){
      identifier = other.identifier;
      displayName = other.displayName;
      displayType = other.displayType;
      acceptMultipleEdges = other.acceptMultipleEdges;
      sizeInPixel = other.sizeInPixel;
      tooltip = other.tooltip;
      vertical = other.vertical;
    }
  }

  /// <summary>
  /// 节点端口类
  /// 运行时类，存储处理所需的一个端口的所有信息
  /// 负责管理端口的数据传输、连接状态和自定义IO操作
  /// </summary>
  public class NodePort{
    /// <summary>
    /// 端口后面的属性的实际名称（必须准确，用于反射）
    /// 对应节点类中的字段名
    /// </summary>
    public string fieldName;

    /// <summary>
    /// 端口所在的节点
    /// 引用拥有此端口的节点对象
    /// </summary>
    public BaseNode owner;

    /// <summary>
    /// 来自fieldName的fieldInfo
    /// 通过反射获取的字段信息，用于数据访问
    /// </summary>
    public FieldInfo fieldInfo;

    /// <summary>
    /// 端口的数据
    /// 包含端口的显示和行为配置
    /// </summary>
    public PortData portData;

    /// <summary>
    /// 与此端口相连的边列表
    /// </summary>
    List<SerializableEdge> edges = new List<SerializableEdge>();

    /// <summary>
    /// 推送数据委托字典
    /// 存储每条边对应的数据推送委托
    /// </summary>
    Dictionary<SerializableEdge, PushDataDelegate> pushDataDelegates =
      new Dictionary<SerializableEdge, PushDataDelegate>();

    /// <summary>
    /// 具有远程自定义IO的边列表
    /// 存储使用自定义IO方法的边
    /// </summary>
    List<SerializableEdge> edgeWithRemoteCustomIO = new List<SerializableEdge>();

    /// <summary>
    /// FieldInfo的所有者，在Get/SetValue情况下使用
    /// 通常是节点对象，但也可以是其他对象
    /// </summary>
    public object fieldOwner;

    /// <summary>
    /// 自定义端口IO方法
    /// 用于处理自定义的输入输出逻辑
    /// </summary>
    CustomPortIODelegate customPortIOMethod;

    /// <summary>
    /// 用于将数据从此端口发送到通过边连接的另一端口的委托
    /// 与使用反射动态设置值相比，这是一种优化（反射真的很慢）
    /// 更多信息：https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/
    /// </summary>
    public delegate void PushDataDelegate();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="owner">所有者节点</param>
    /// <param name="fieldName">C#属性名称</param>
    /// <param name="portData">端口数据</param>
    public NodePort(BaseNode owner, string fieldName, PortData portData) : this(owner, owner, fieldName, portData){
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="owner">所有者节点</param>
    /// <param name="fieldOwner"></param>
    /// <param name="fieldName">C#属性名称</param>
    /// <param name="portData">端口数据</param>
    public NodePort(BaseNode owner, object fieldOwner, string fieldName, PortData portData){
      this.fieldName = fieldName;
      this.owner = owner;
      this.portData = portData;
      this.fieldOwner = fieldOwner;

      fieldInfo = fieldOwner.GetType().GetField(
        fieldName,
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      customPortIOMethod = CustomPortIO.GetCustomPortMethod(owner.GetType(), fieldName);
    }

    /// <summary>
    /// 将边连接到此端口
    /// </summary>
    /// <param name="edge"></param>
    public void Add(SerializableEdge edge){
      if (!edges.Contains(edge))
        edges.Add(edge);

      if (edge.inputNode == owner){
        if (edge.outputPort.customPortIOMethod != null)
          edgeWithRemoteCustomIO.Add(edge);
      }
      else{
        if (edge.inputPort.customPortIOMethod != null)
          edgeWithRemoteCustomIO.Add(edge);
      }

      //如果我们有自定义io实现，我们不需要生成默认的
      if (edge.inputPort.customPortIOMethod != null || edge.outputPort.customPortIOMethod != null)
        return;

      PushDataDelegate edgeDelegate = CreatePushDataDelegateForEdge(edge);

      if (edgeDelegate != null)
        pushDataDelegates[edge] = edgeDelegate;
    }

    PushDataDelegate CreatePushDataDelegateForEdge(SerializableEdge edge){
      try{
        //创建委托以将数据从输入节点移动到输出节点：
        FieldInfo inputField = edge.inputNode.GetType().GetField(edge.inputFieldName,
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo outputField = edge.outputNode.GetType().GetField(edge.outputFieldName,
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Type inType, outType;

        #if DEBUG_LAMBDA
				return new PushDataDelegate(() => {
					var outValue = outputField.GetValue(edge.outputNode);
					inType = edge.inputPort.portData.displayType ?? inputField.FieldType;
					outType = edge.outputPort.portData.displayType ?? outputField.FieldType;
					Debug.Log($"Push: {inType}({outValue}) -> {outType} | {owner.name}");

					object convertedValue = outValue;
					if (TypeAdapter.AreAssignable(outType, inType))
					{
						var convertionMethod = TypeAdapter.GetConvertionMethod(outType, inType);
						Debug.Log("Convertion method: " + convertionMethod.Name);
						convertedValue = convertionMethod.Invoke(null, new object[]{ outValue });
					}

					inputField.SetValue(edge.inputNode, convertedValue);
				});
        #endif

        // 我们在编辑器内保持慢速检查
        #if UNITY_EDITOR
        if (!BaseGraph.TypesAreConnectable(inputField.FieldType, outputField.FieldType)){
          Debug.LogError("Can't convert from " + inputField.FieldType + " to " + outputField.FieldType +
                         ", you must specify a custom port function (i.e CustomPortInput or CustomPortOutput) for non-implicit convertions");
          return null;
        }
        #endif

        Expression inputParamField = Expression.Field(Expression.Constant(edge.inputNode), inputField);
        Expression outputParamField = Expression.Field(Expression.Constant(edge.outputNode), outputField);

        inType = edge.inputPort.portData.displayType ?? inputField.FieldType;
        outType = edge.outputPort.portData.displayType ?? outputField.FieldType;

        // 如果有用户定义的转换函数，则我们调用它
        if (TypeAdapter.AreAssignable(outType, inType)){
          // 我们添加一个转换，以防我们使用基类参数（如object）调用转换方法
          var convertedParam = Expression.Convert(outputParamField, outType);
          outputParamField = Expression.Call(TypeAdapter.GetConvertionMethod(outType, inType), convertedParam);
          // 如果输出中有自定义端口行为，那么我们需要重新转换为基类型，因为
          // 转换方法的返回类型并不总是可以直接分配的：
          outputParamField = Expression.Convert(outputParamField, inputField.FieldType);
        }
        else // 否则我们进行转换
          outputParamField = Expression.Convert(outputParamField, inputField.FieldType);

        BinaryExpression assign = Expression.Assign(inputParamField, outputParamField);
        return Expression.Lambda<PushDataDelegate>(assign).Compile();
      }
      catch (Exception e){
        Debug.LogError(e);
        return null;
      }
    }

    /// <summary>
    /// 从此端口断开边
    /// </summary>
    /// <param name="edge"></param>
    public void Remove(SerializableEdge edge){
      if (!edges.Contains(edge))
        return;

      pushDataDelegates.Remove(edge);
      edgeWithRemoteCustomIO.Remove(edge);
      edges.Remove(edge);
    }

    /// <summary>
    /// 获取连接到此端口的所有边
    /// </summary>
    /// <returns></returns>
    public List<SerializableEdge> GetEdges() => edges;

    /// <summary>
    /// 通过边推送端口的值
    /// 此方法只能在输出端口上调用
    /// </summary>
    public void PushData(){
      if (customPortIOMethod != null){
        customPortIOMethod(owner, edges, this);
        return;
      }

      foreach (var pushDataDelegate in pushDataDelegates)
        pushDataDelegate.Value();

      if (edgeWithRemoteCustomIO.Count == 0)
        return;

      //如果其他端口有自定义IO实现，它们将需要我们在passThrough缓冲区中的值
      object ourValue = fieldInfo.GetValue(fieldOwner);
      foreach (var edge in edgeWithRemoteCustomIO)
        edge.passThroughBuffer = ourValue;
    }

    /// <summary>
    /// 如果可能，将字段的值重置为默认值
    /// </summary>
    public void ResetToDefault(){
      // 清除列表，将类设置为null，结构体设置为默认值。
      if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
        (fieldInfo.GetValue(fieldOwner) as IList)?.Clear();
      else if (fieldInfo.FieldType.GetTypeInfo().IsClass)
        fieldInfo.SetValue(fieldOwner, null);
      else{
        try{
          fieldInfo.SetValue(fieldOwner, Activator.CreateInstance(fieldInfo.FieldType));
        }
        catch{
        } // 捕获没有任何构造函数的类型
      }
    }

    /// <summary>
    /// 从边拉取值（在自定义转换方法的情况下）
    /// 此方法只能在输入端口上调用
    /// </summary>
    public void PullData(){
      if (customPortIOMethod != null){
        customPortIOMethod(owner, edges, this);
        return;
      }

      // 检查此端口是否连接到具有自定义输出函数的端口
      if (edgeWithRemoteCustomIO.Count == 0)
        return;

      // 此代码只处理一个输入连接，如果您想要
      // 接受多个输入，您必须创建一个自定义输入函数，请参阅CustomPortsNode.cs
      if (edges.Count > 0){
        var passThroughObject = edges.First().passThroughBuffer;

        // 我们执行额外的转换步骤，以防缓冲区输出与输入端口不兼容
        if (passThroughObject != null)
          if (TypeAdapter.AreAssignable(fieldInfo.FieldType, passThroughObject.GetType()))
            passThroughObject = TypeAdapter.Convert(passThroughObject, fieldInfo.FieldType);

        fieldInfo.SetValue(fieldOwner, passThroughObject);
      }
    }
  }

  /// <summary>
  /// 端口和连接到这些端口的边的容器
  /// </summary>
  public abstract class NodePortContainer : List<NodePort>{
    protected BaseNode node;

    public NodePortContainer(BaseNode node){
      this.node = node;
    }

    /// <summary>
    /// 移除连接到容器中某个节点的边
    /// </summary>
    /// <param name="edge"></param>
    public void Remove(SerializableEdge edge){
      ForEach(p => p.Remove(edge));
    }

    /// <summary>
    /// 添加连接到容器中某个节点的边
    /// </summary>
    /// <param name="edge"></param>
    public void Add(SerializableEdge edge){
      string portFieldName = (edge.inputNode == node) ? edge.inputFieldName : edge.outputFieldName;
      string portIdentifier = (edge.inputNode == node) ? edge.inputPortIdentifier : edge.outputPortIdentifier;

      // 强制空字符串为null，因为portIdentifier是序列化值
      if (String.IsNullOrEmpty(portIdentifier))
        portIdentifier = null;

      var port = this.FirstOrDefault(p => {
        return p.fieldName == portFieldName && p.portData.identifier == portIdentifier;
      });

      if (port == null){
        Debug.LogError("The edge can't be properly connected because it's ports can't be found");
        return;
      }

      port.Add(edge);
    }
  }

  /// <inheritdoc/>
  public class NodeInputPortContainer : NodePortContainer{
    public NodeInputPortContainer(BaseNode node) : base(node){
    }

    public void PullDatas(){
      ForEach(p => p.PullData());
    }
  }

  /// <inheritdoc/>
  public class NodeOutputPortContainer : NodePortContainer{
    public NodeOutputPortContainer(BaseNode node) : base(node){
    }

    public void PushDatas(){
      ForEach(p => p.PushData());
    }
  }
}