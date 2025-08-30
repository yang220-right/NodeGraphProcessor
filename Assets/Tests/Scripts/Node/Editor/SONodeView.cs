using System;
using System.Collections.Generic;
using GraphProcessor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Examples;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// SONode的NodeView实现
/// 继承自SOInspectorWrapper，专门用于显示SONode的所有属性
/// </summary>
[NodeCustomEditor(typeof(SONode))]
public class SONodeView : SOInspectorWrapper
{
    /// <summary>
    /// 创建目标ScriptableObject
    /// 这里我们创建一个包装了SONode数据的SO对象
    /// </summary>
    /// <returns>包装了SONode数据的SO对象</returns>
    protected override SerializedScriptableObject CreateTargetSO()
    {
        var so = SerializedScriptableObject.CreateInstance<SONodeWrapper>();
        
        // 设置初始值
        so.nodeName = "SO Inspector Node";
        so.isEnabled = true;
        so.multiplier = 2f;
        so.offset = 10;
        so.processCount = 0;
        so.lastProcessTime = 0f;
        so.configMode = SONodeWrapper.ConfigMode.Normal;
        so.useAdvancedSettings = false;
        so.advancedMultiplier = 1.5f;
        so.advancedOffset = 5;
        
        Debug.Log("SONodeWrapper 已创建并初始化");
        return so;
    }
    
    /// <summary>
    /// 重写自动创建方法，添加更多日志信息
    /// </summary>
    protected override void AutoCreateAndDisplaySO()
    {
        Debug.Log("SONodeView 开始自动创建SO对象...");
        base.AutoCreateAndDisplaySO();
        
        if (targetSO != null)
        {
            Debug.Log($"SONodeView SO对象创建成功: {targetSO.name}");
        }
        else
        {
            Debug.LogError("SONodeView SO对象创建失败");
        }
    }
}

/// <summary>
/// SONode数据包装器
/// 将SONode的数据包装成ScriptableObject，以便在Inspector中显示
/// </summary>
[CreateAssetMenu(fileName = "SONodeWrapper", menuName = "Examples/SONodeWrapper")]
public class SONodeWrapper : SerializedScriptableObject
{
    #region list

    [TableList(AlwaysExpanded = true, DrawScrollView = false)]//一直可拓展 不可折叠
    public List<SomeCustomClass> AlwaysExpandedTable = new List<SomeCustomClass>()
    {
        new SomeCustomClass(),
        new SomeCustomClass(),
    };
    [Serializable]
    public class SomeCustomClass
    {
        [TableColumnWidth(57, Resizable = false)]
        [PreviewField(Alignment = ObjectFieldAlignment.Center)]
        public Texture Icon;

        [TextArea]
        public string Description;

        [VerticalGroup("Combined Column"), LabelWidth(22)]
        public string A, B, C;

        [TableColumnWidth(60)]
        [Button, VerticalGroup("Actions")]
        public void Test1() { }

        [TableColumnWidth(60)]
        [Button, VerticalGroup("Actions")]
        public void Test2() { }

        [OnInspectorInit]
        private void CreateData()
        {
            Description = ExampleHelper.GetString();
            Icon = ExampleHelper.GetTexture();
        }
    }
    
    
    [AssetSelector]
    public Material ScriptableObjectsFromMultipleFolders;

    #endregion
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
    
    /// <summary>
    /// 重置所有值为默认值
    /// </summary>
    [ContextMenu("重置为默认值")]
    public void ResetToDefaults()
    {
        nodeName = "SO Inspector Node";
        isEnabled = true;
        multiplier = 2f;
        offset = 10;
        processCount = 0;
        lastProcessTime = 0f;
        configMode = ConfigMode.Normal;
        useAdvancedSettings = false;
        advancedMultiplier = 1.5f;
        advancedOffset = 5;
        
        Debug.Log("SONodeWrapper 已重置为默认值");
    }
    
    /// <summary>
    /// 随机化所有数值
    /// </summary>
    [ContextMenu("随机化数值")]
    public void RandomizeValues()
    {
        multiplier = Random.Range(1f, 10f);
        offset = Random.Range(-20, 21);
        advancedMultiplier = Random.Range(0.1f, 5f);
        advancedOffset = Random.Range(-10, 11);
        configMode = (ConfigMode)Random.Range(0, 4);
        useAdvancedSettings = Random.value > 0.5f;
        
        Debug.Log("SONodeWrapper 数值已随机化");
    }
    
    /// <summary>
    /// 打印当前状态
    /// </summary>
    [ContextMenu("打印状态")]
    public void PrintStatus()
    {
        Debug.Log($"SONodeWrapper 状态:\n" +
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
    
    /// <summary>
    /// 应用设置到实际的SONode
    /// </summary>
    /// <param name="targetNode">目标SONode</param>
    public void ApplyToNode(SONode targetNode)
    {
        if (targetNode == null) return;
        
        targetNode.nodeName = nodeName;
        targetNode.isEnabled = isEnabled;
        targetNode.multiplier = multiplier;
        targetNode.offset = offset;
        targetNode.processCount = processCount;
        targetNode.lastProcessTime = lastProcessTime;
        targetNode.configMode = (SONode.ConfigMode)configMode;
        targetNode.useAdvancedSettings = useAdvancedSettings;
        targetNode.advancedMultiplier = advancedMultiplier;
        targetNode.advancedOffset = advancedOffset;
        
        Debug.Log("设置已应用到目标节点");
    }
    
    /// <summary>
    /// 从实际的SONode同步设置
    /// </summary>
    /// <param name="targetNode">目标SONode</param>
    public void SyncFromNode(SONode targetNode)
    {
        if (targetNode == null) return;
        
        nodeName = targetNode.nodeName;
        isEnabled = targetNode.isEnabled;
        multiplier = targetNode.multiplier;
        offset = targetNode.offset;
        processCount = targetNode.processCount;
        lastProcessTime = targetNode.lastProcessTime;
        configMode = (ConfigMode)targetNode.configMode;
        useAdvancedSettings = targetNode.useAdvancedSettings;
        advancedMultiplier = targetNode.advancedMultiplier;
        advancedOffset = targetNode.advancedOffset;
        
        Debug.Log("设置已从目标节点同步");
    }
}
