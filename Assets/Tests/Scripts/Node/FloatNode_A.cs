using GraphProcessor;

[System.Serializable, NodeMenuItem("Action/Float_A")]
public class FloatNode_A : BaseNode
{
    [Output("Out")] public float output;
    [Input("IN")] public float input;
    public override string name => "Float_A";
    protected override void Process() => output = input;
}
