using GraphProcessor;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable, NodeMenuItem("Action/OdinFeature")]
public class OdinFeatureNode : BaseNode
{
    [Input(name = "Input")]
    public int input;
    
    [Output(name = "Output")]
    public int output;

    [Title("Odin特性演示")]
    [InfoBox("这是一个使用Odin Inspector特性的节点")]
    
    [FoldoutGroup("基础设置", expanded: true)]
    [Range(0, 100)]
    [LabelText("数值范围")]
    public float rangeValue = 50f;
    
    [FoldoutGroup("基础设置")]
    [Toggle("启用功能")]
    public bool isEnabled = true;
    
    public TestEnum testEnum = TestEnum.Option1;
    
    public float advancedValue = 100f;
    
    public bool isAdvancedEnabled = false;
    
    public AdvancedEnum advancedEnum = AdvancedEnum.Advanced1;
    
    [FoldoutGroup("按钮操作", expanded: true)]
    [Button("执行操作", ButtonStyle.Box)]
    [ShowIf("isEnabled")]
    public void ExecuteAction()
    {
        Debug.Log($"OdinFeatureNode执行操作: {input} -> {output}");
        // 这里可以添加具体的操作逻辑
    }
    
    [FoldoutGroup("按钮操作")]
    [Button("重置数值",ButtonStyle.FoldoutButton)]
    public void ResetValues()
    {
        rangeValue = 50f;
        isEnabled = true;
        testEnum = TestEnum.Option1;
        advancedValue = 100f;
        isAdvancedEnabled = false;
        advancedEnum = AdvancedEnum.Advanced1;
        Debug.Log("数值已重置");
    }
    
    [FoldoutGroup("按钮操作")]
    [Button("随机数值",ButtonStyle.Box)]
    [ShowIf("isEnabled")]
    public void RandomizeValues()
    {
        rangeValue = Random.Range(0f, 100f);
        testEnum = (TestEnum)Random.Range(0, 3);
        advancedValue = Random.Range(0f, 200f);
        advancedEnum = (AdvancedEnum)Random.Range(0, 3);
        Debug.Log($"随机化完成: {rangeValue}, {testEnum}, {advancedValue}, {advancedEnum}");
    }
    
    [FoldoutGroup("高级操作", expanded: false)]
    [Button("高级操作", ButtonStyle.Box)]
    [ShowIf("isAdvancedEnabled")]
    public void AdvancedAction()
    {
        Debug.Log($"执行高级操作: {advancedValue} -> {advancedEnum}");
    }
    
    [FoldoutGroup("高级操作")]
    [Button("切换高级状态")]
    public void ToggleAdvanced()
    {
        isAdvancedEnabled = !isAdvancedEnabled;
        Debug.Log($"高级功能状态: {isAdvancedEnabled}");
    }

    public override string name => "OdinFeature";

    protected override void Process()
    {
        if (isEnabled)
        {
            output = input + (int)rangeValue;
        }
        else
        {
            output = input;
        }
    }
    
    public enum TestEnum
    {
        Option1,
        Option2,
        Option3
    }
    
    public enum AdvancedEnum
    {
        Advanced1,
        Advanced2,
        Advanced3
    }
}
