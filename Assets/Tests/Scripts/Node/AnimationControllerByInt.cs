using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Action/AnimationControllerByInt")]
public class AnimationControllerByInt : BaseNode
{
  [Input("frame")] public int input;
  [Input("obj")] public GameObject obj;
  public override string name => "AnimationControllerByInt";

  protected override void Process(){
    var anim = obj.GetComponentInChildren<Animator>();
    var clip = anim.runtimeAnimatorController.animationClips[0];
    clip.SampleAnimation(anim.gameObject, input * 0.033f);
  }
}
