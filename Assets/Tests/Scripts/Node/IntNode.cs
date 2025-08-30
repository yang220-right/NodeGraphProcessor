using GraphProcessor;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable, NodeMenuItem("Action/Int")]
public class IntNode : BaseNode
{
  [Output] public int output;
  public int input;
  
  public override string name => "IntNode";

  protected override void Process(){
    output = input;
  }
}
