using GraphProcessor;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Odin Inspector集成节点
/// 此节点将Odin Inspector的所有功能完全集成到NodeView中
/// </summary>
[System.Serializable, NodeMenuItem("Action/OdinInspector")]
public class OdinInspectorNode : BaseNode
{
    [Input(name = "Input")]
    public int input;
    
    [Output(name = "Output")]
    public int output;

    [Title("Odin Inspector 完全集成演示")]
    [InfoBox("这是一个完全集成Odin Inspector功能的节点，所有Odin特性都在NodeView中正常工作")]
    
    [FoldoutGroup("基础功能", expanded: true)]
    [Range(0, 100)]
    [LabelText("基础数值")]
    public float baseValue = 50f;
    
    [FoldoutGroup("基础功能")]
    [Toggle("启用基础功能")]
    public bool isBaseEnabled = true;
    
    [FoldoutGroup("基础功能")]
    [EnumToggleButtons]
    public BaseEnum baseEnum = BaseEnum.Option1;
    
    [FoldoutGroup("高级功能", expanded: false)]
    [InfoBox("高级功能设置区域")]
    [Range(0, 200)]
    [LabelText("高级数值")]
    public float advancedValue = 100f;
    
    [FoldoutGroup("高级功能")]
    [Toggle("启用高级功能")]
    public bool isAdvancedEnabled = false;
    
    [FoldoutGroup("高级功能")]
    [EnumToggleButtons]
    public AdvancedEnum advancedEnum = AdvancedEnum.Advanced1;
    
    [FoldoutGroup("按钮操作", expanded: true)]
    [Button("执行基础操作", ButtonStyle.Box)]
    [ShowIf("isBaseEnabled")]
    public void ExecuteBaseAction()
    {
        Debug.Log($"OdinInspectorNode执行基础操作: {input} -> {baseValue}");
        // 这里可以添加具体的操作逻辑
    }
    
    [FoldoutGroup("按钮操作")]
    [Button("执行高级操作", ButtonStyle.Box)]
    [ShowIf("isAdvancedEnabled")]
    public void ExecuteAdvancedAction()
    {
        Debug.Log($"OdinInspectorNode执行高级操作: {advancedValue} -> {advancedEnum}");
        // 这里可以添加具体的操作逻辑
    }
    
    [FoldoutGroup("按钮操作")]
    [Button("重置所有数值", ButtonStyle.FoldoutButton)]
    public void ResetAllValues()
    {
        baseValue = 50f;
        isBaseEnabled = true;
        baseEnum = BaseEnum.Option1;
        advancedValue = 100f;
        isAdvancedEnabled = false;
        advancedEnum = AdvancedEnum.Advanced1;
        Debug.Log("所有数值已重置");
    }
    
    [FoldoutGroup("按钮操作")]
    [Button("随机化数值", ButtonStyle.Box)]
    [ShowIf("isBaseEnabled")]
    public void RandomizeValues()
    {
        baseValue = Random.Range(0f, 100f);
        baseEnum = (BaseEnum)Random.Range(0, 3);
        advancedValue = Random.Range(0f, 200f);
        advancedEnum = (AdvancedEnum)Random.Range(0, 3);
        Debug.Log($"随机化完成: {baseValue}, {baseEnum}, {advancedValue}, {advancedEnum}");
    }
    
    [FoldoutGroup("高级操作", expanded: false)]
    [Button("切换高级状态")]
    public void ToggleAdvanced()
    {
        isAdvancedEnabled = !isAdvancedEnabled;
        Debug.Log($"高级功能状态: {isAdvancedEnabled}");
    }
    
    [FoldoutGroup("高级操作")]
    [Button("特殊操作", ButtonStyle.Box)]
    [ShowIf("isAdvancedEnabled")]
    public void SpecialAction()
    {
        Debug.Log($"执行特殊操作: {baseValue + advancedValue}");
        // 这里可以添加特殊的操作逻辑
    }

    public override string name => "OdinInspector";

    protected override void Process()
    {
        if (isBaseEnabled)
        {
            output = input + (int)baseValue;
            if (isAdvancedEnabled)
            {
                output += (int)advancedValue;
            }
        }
        else
        {
            output = input;
        }
    }
    
    public enum BaseEnum
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
