using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Action/SceneGameObjectNode")]
public class SceneGameObjectNode : BaseNode
{
    public GameObject input;
    [Output(name = "GameObject")]
    public GameObject output;

    public override string name => "SceneGameObjectNode";

    protected override void Process()
    {
        output = input;
    }
}
