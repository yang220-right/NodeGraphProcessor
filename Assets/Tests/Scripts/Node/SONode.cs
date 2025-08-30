using GraphProcessor;
using UnityEngine;

/// <summary>
/// SO Inspector节点
/// 用于演示在NodeView中显示ScriptableObject的所有属性
/// </summary>
[System.Serializable, NodeMenuItem("Action/SO Inspector")]
public class SONode : BaseNode
{
    [Input(name = "Input")]
    public int input;
    
    [Output(name = "Output")]
    public int output;
    
    [Output(name = "SO Data")]
    public ScriptableObject soData;
    
    [Header("节点设置")]
    [Tooltip("节点名称")]
    public string nodeName = "SO Inspector Node";
    
    [Tooltip("是否启用节点")]
    public bool isEnabled = true;
    
    [Header("数据处理")]
    [Tooltip("处理倍数")]
    [Range(1f, 10f)]
    public float multiplier = 2f;
    
    [Tooltip("偏移值")]
    public int offset = 10;
    
    [Header("状态信息")]
    [Tooltip("处理次数")]
    public int processCount = 0;
    
    [Tooltip("最后处理时间")]
    public float lastProcessTime = 0f;
    
    [Header("配置选项")]
    [Tooltip("配置模式")]
    public ConfigMode configMode = ConfigMode.Normal;
    
    [Tooltip("高级设置")]
    public bool useAdvancedSettings = false;
    
    [Header("高级设置")]
    [Tooltip("高级倍数")]
    [Range(0.1f, 5f)]
    public float advancedMultiplier = 1.5f;
    
    [Tooltip("高级偏移")]
    public int advancedOffset = 5;
    
    /// <summary>
    /// 配置模式枚举
    /// </summary>
    public enum ConfigMode
    {
        Normal,
        Advanced,
        Expert,
        Custom
    }
    
    public override string name => "SO Inspector";
    
    protected override void Process()
    {
        if (!isEnabled) return;
        
        var startTime = Time.realtimeSinceStartup;
        
        // 根据配置模式选择处理方式
        switch (configMode)
        {
            case ConfigMode.Normal:
                output = ProcessNormal(input);
                break;
            case ConfigMode.Advanced:
                output = ProcessAdvanced(input);
                break;
            case ConfigMode.Expert:
                output = ProcessExpert(input);
                break;
            case ConfigMode.Custom:
                output = ProcessCustom(input);
                break;
        }
        
        // 更新状态信息
        processCount++;
        lastProcessTime = Time.realtimeSinceStartup - startTime;
        
        Debug.Log($"SONode处理完成: {input} -> {output}, 模式: {configMode}, 处理次数: {processCount}");
    }
    
    /// <summary>
    /// 普通模式处理
    /// </summary>
    private int ProcessNormal(int inputValue)
    {
        return Mathf.RoundToInt(inputValue * multiplier) + offset;
    }
    
    /// <summary>
    /// 高级模式处理
    /// </summary>
    private int ProcessAdvanced(int inputValue)
    {
        if (useAdvancedSettings)
        {
            return Mathf.RoundToInt(inputValue * advancedMultiplier) + advancedOffset;
        }
        return ProcessNormal(inputValue);
    }
    
    /// <summary>
    /// 专家模式处理
    /// </summary>
    private int ProcessExpert(int inputValue)
    {
        var result = inputValue;
        result = Mathf.RoundToInt(result * multiplier);
        result += offset;
        result = Mathf.RoundToInt(result * 1.5f);
        return result;
    }
    
    /// <summary>
    /// 自定义模式处理
    /// </summary>
    private int ProcessCustom(int inputValue)
    {
        var result = inputValue;
        
        if (useAdvancedSettings)
        {
            result = Mathf.RoundToInt(result * advancedMultiplier);
            result += advancedOffset;
        }
        else
        {
            result = Mathf.RoundToInt(result * multiplier);
            result += offset;
        }
        
        // 添加一些随机性
        result += Random.Range(-2, 3);
        
        return result;
    }
    
    /// <summary>
    /// 重置节点状态
    /// </summary>
    [ContextMenu("重置节点状态")]
    public void ResetNodeState()
    {
        processCount = 0;
        lastProcessTime = 0f;
        Debug.Log("节点状态已重置");
    }
    
    /// <summary>
    /// 随机化配置
    /// </summary>
    [ContextMenu("随机化配置")]
    public void RandomizeConfig()
    {
        multiplier = Random.Range(1f, 10f);
        offset = Random.Range(-20, 21);
        advancedMultiplier = Random.Range(0.1f, 5f);
        advancedOffset = Random.Range(-10, 11);
        configMode = (ConfigMode)Random.Range(0, 4);
        useAdvancedSettings = Random.value > 0.5f;
        
        Debug.Log("节点配置已随机化");
    }
    
    /// <summary>
    /// 打印当前配置
    /// </summary>
    [ContextMenu("打印配置")]
    public void PrintConfig()
    {
        Debug.Log($"SONode当前配置:\n" +
                  $"节点名称: {nodeName}\n" +
                  $"启用状态: {isEnabled}\n" +
                  $"配置模式: {configMode}\n" +
                  $"倍数: {multiplier}\n" +
                  $"偏移: {offset}\n" +
                  $"使用高级设置: {useAdvancedSettings}\n" +
                  $"高级倍数: {advancedMultiplier}\n" +
                  $"高级偏移: {advancedOffset}\n" +
                  $"处理次数: {processCount}\n" +
                  $"最后处理时间: {lastProcessTime:F3}s");
    }
}
