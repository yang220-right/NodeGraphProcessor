using GraphProcessor;
using UnityEngine;

/// <summary>
/// 简单SO节点
/// 用于演示在NodeView中自动创建和显示SO对象
/// </summary>
[System.Serializable, NodeMenuItem("Examples/Simple SO")]
public class SimpleSONode : BaseNode
{
    [Input(name = "Input")]
    public int input;
    
    [Output(name = "Output")]
    public int output;
    
    [Header("基础设置")]
    [Tooltip("节点名称")]
    public string nodeName = "Simple SO Node";
    
    [Tooltip("是否启用")]
    public bool isEnabled = true;
    
    [Header("数值设置")]
    [Tooltip("倍数")]
    [Range(1f, 10f)]
    public float multiplier = 2f;
    
    [Tooltip("偏移")]
    public int offset = 5;
    
    [Header("状态")]
    [Tooltip("处理次数")]
    public int processCount = 0;
    
    public override string name => "Simple SO";
    
    protected override void Process()
    {
        if (!isEnabled) return;
        
        output = Mathf.RoundToInt(input * multiplier) + offset;
        processCount++;
        
        Debug.Log($"SimpleSONode处理: {input} -> {output}, 处理次数: {processCount}");
    }
    
    [ContextMenu("重置状态")]
    public void ResetState()
    {
        processCount = 0;
        Debug.Log("SimpleSONode状态已重置");
    }
    
    [ContextMenu("随机化设置")]
    public void RandomizeSettings()
    {
        multiplier = Random.Range(1f, 10f);
        offset = Random.Range(-10, 11);
        Debug.Log($"SimpleSONode设置已随机化: 倍数={multiplier}, 偏移={offset}");
    }
}
