using GraphProcessor;
using UnityEngine;
using System.Linq;

[System.Serializable, NodeMenuItem("Action/AnimationControllerByInt")]
public class AnimationControllerByInt : BaseNode
{
  [Input("frame")] public int input;
  [Input("obj")] public GameObject obj;
  public override string name => "AnimationControllerByInt";

  protected override void Process(){
    // 如果obj为null，尝试执行连接到obj端口的前置节点
    if (obj == null)
    {
        // 获取连接到obj端口的输入节点
        var objPort = GetPort("obj", "");
        if (objPort != null)
        {
            // 获取连接到该端口的所有边
            var edges = objPort.GetEdges();
            foreach (var edge in edges)
            {
                var outputNode = edge.outputNode;
                if (outputNode != null)
                {
                    Debug.Log($"执行前置节点: {outputNode.GetType().Name}");
                    outputNode.OnProcess();
                }
            }
        }
        // 如果执行前置节点后obj仍然为null，则返回
        if (obj == null)
        {
            Debug.LogWarning("执行前置节点后，obj仍然为null");
            return;
        }
    }
    
    // 执行动画控制逻辑
    if (obj != null)
    {
        var anim = obj.GetComponentInChildren<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            var clip = anim.runtimeAnimatorController.animationClips[0];
            clip.SampleAnimation(anim.gameObject, input * 0.033f);
        }
        else Debug.LogWarning($"对象 {obj.name} 没有Animator组件或AnimationController");
    }
  }
}
