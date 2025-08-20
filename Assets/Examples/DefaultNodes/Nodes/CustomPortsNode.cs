using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Custom/MultiPorts")]
public class CustomPortsNode : BaseNode{
  [Input] public List<float> inputs;

  [Output] public List<float> outputs; // TODO: 为这个自定义函数

  List<object> values = new List<object>();

  public override string name => "CustomPorts";

  public override string layoutStyle => "TestType";

  // 我们保持最大端口计数以避免绑定问题
  [SerializeField, HideInInspector] int portCount = 1;

  protected override void Process(){
    // 对值进行处理
  }

  [CustomPortBehavior(nameof(inputs))]
  IEnumerable<PortData> ListPortBehavior(List<SerializableEdge> edges){
    portCount = Mathf.Max(portCount, edges.Count + 1);

    for (int i = 0; i < portCount; i++){
      yield return new PortData{
        displayName = "In " + i,
        displayType = typeof(float),
        identifier = i.ToString(), // 必须唯一
      };
    }
  }

  // 这个函数将从`inputs`自定义端口函数创建的每个端口调用一次
  // 参数将是连接到此端口的边的列表
  [CustomPortInput(nameof(inputs), typeof(float))]
  void PullInputs(List<SerializableEdge> inputEdges){
    values.AddRange(inputEdges.Select(e => e.passThroughBuffer).ToList());
  }

  [CustomPortOutput(nameof(outputs), typeof(float))]
  void PushOutputs(List<SerializableEdge> connectedEdges){
    // 值的长度应该匹配连接的边的长度
    for (int i = 0; i < connectedEdges.Count; i++)
      connectedEdges[i].passThroughBuffer = values[Mathf.Min(i, values.Count - 1)];

    // 一旦输出被推送，我们就不再需要输入数据了
    values.Clear();
  }
}