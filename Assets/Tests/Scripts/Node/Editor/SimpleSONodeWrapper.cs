using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// SimpleSONode的SO包装器
/// 自动在NodeView中创建和显示
/// </summary>
[CreateAssetMenu(fileName = "SimpleSONodeWrapper", menuName = "Examples/SimpleSONodeWrapper")]
public class SimpleSONodeWrapper : SerializedScriptableObject
{
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
    
    /// <summary>
    /// 重置为默认值
    /// </summary>
    [ContextMenu("重置为默认值")]
    public void ResetToDefaults()
    {
        nodeName = "Simple SO Node";
        isEnabled = true;
        multiplier = 2f;
        offset = 5;
        processCount = 0;
        
        Debug.Log("SimpleSONodeWrapper 已重置为默认值");
    }
    
    /// <summary>
    /// 随机化数值
    /// </summary>
    [ContextMenu("随机化数值")]
    public void RandomizeValues()
    {
        multiplier = Random.Range(1f, 10f);
        offset = Random.Range(-10, 11);
        
        Debug.Log($"SimpleSONodeWrapper 数值已随机化: 倍数={multiplier}, 偏移={offset}");
    }
    
    /// <summary>
    /// 打印当前状态
    /// </summary>
    [ContextMenu("打印状态")]
    public void PrintStatus()
    {
        Debug.Log($"SimpleSONodeWrapper 状态:\n" +
                  $"节点名称: {nodeName}\n" +
                  $"启用状态: {isEnabled}\n" +
                  $"倍数: {multiplier}\n" +
                  $"偏移: {offset}\n" +
                  $"处理次数: {processCount}");
    }
    
    /// <summary>
    /// 应用设置到实际的SimpleSONode
    /// </summary>
    /// <param name="targetNode">目标节点</param>
    public void ApplyToNode(SimpleSONode targetNode)
    {
        if (targetNode == null) return;
        
        targetNode.nodeName = nodeName;
        targetNode.isEnabled = isEnabled;
        targetNode.multiplier = multiplier;
        targetNode.offset = offset;
        targetNode.processCount = processCount;
        
        Debug.Log("设置已应用到目标节点");
    }
    
    /// <summary>
    /// 从实际的SimpleSONode同步设置
    /// </summary>
    /// <param name="targetNode">目标节点</param>
    public void SyncFromNode(SimpleSONode targetNode)
    {
        if (targetNode == null) return;
        
        nodeName = targetNode.nodeName;
        isEnabled = targetNode.isEnabled;
        multiplier = targetNode.multiplier;
        offset = targetNode.offset;
        processCount = targetNode.processCount;
        
        Debug.Log("设置已从目标节点同步");
    }
}
