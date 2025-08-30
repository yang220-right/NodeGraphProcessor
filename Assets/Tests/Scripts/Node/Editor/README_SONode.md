# SO NodeView 系统使用指南

## 概述

SO NodeView 系统是一个完整的解决方案，可以将任何节点的所有属性完整地显示在 NodeView 中，使用 ScriptableObject 包装器技术。这个系统基于 `SOInspectorWrapper`，提供了强大的属性显示和编辑功能。

## 系统架构

### 核心组件

```
SONode (节点类)
    ↓
SONodeView (NodeView类)
    ↓
SOInspectorWrapper (基础包装器)
    ↓
SONodeWrapper (SO包装器)
```

### 文件结构

```
Tests/Scripts/Node/
├── SONode.cs                    # 主节点类
└── Editor/
    ├── SONodeView.cs            # NodeView实现
    ├── SOInspectorWrapper.cs    # 基础包装器
    ├── SONodeTest.cs            # 测试脚本
    └── README_SONode.md         # 本文档
```

## 核心特性

### ✅ 完整属性显示
- 显示节点的所有公共字段
- 支持所有 Unity 内置类型
- 支持自定义类型和结构
- 保持原有的属性标签和限制

### ✅ 实时编辑
- 所有属性都可以直接在 NodeView 中编辑
- 实时保存修改
- 支持撤销/重做操作

### ✅ 类型支持
- **基础类型**: string, int, float, bool
- **Unity类型**: Vector3, Color, GameObject
- **数组类型**: 支持所有类型的数组
- **枚举类型**: 下拉选择器
- **自定义结构**: 可展开编辑

### ✅ 属性标签支持
- `[Header]` - 分组标题
- `[Tooltip]` - 工具提示
- `[Range]` - 数值范围限制
- `[Min]` - 最小值限制
- 所有其他 Unity 属性标签

## 使用方法

### 1. 创建节点类

```csharp
[System.Serializable, NodeMenuItem("Action/SO Inspector")]
public class SONode : BaseNode
{
    [Input(name = "Input")]
    public int input;
    
    [Output(name = "Output")]
    public int output;
    
    [Header("节点设置")]
    [Tooltip("节点名称")]
    public string nodeName = "SO Inspector Node";
    
    [Header("数据处理")]
    [Range(1f, 10f)]
    public float multiplier = 2f;
    
    public override string name => "SO Inspector";
    
    protected override void Process()
    {
        output = Mathf.RoundToInt(input * multiplier);
    }
}
```

### 2. 创建 NodeView 类

```csharp
[NodeCustomEditor(typeof(SONode))]
public class SONodeView : SOInspectorWrapper
{
    protected override ScriptableObject CreateTargetSO()
    {
        var so = ScriptableObject.CreateInstance<SONodeWrapper>();
        so.name = "SONode_Wrapper";
        
        // 设置初始值
        so.nodeName = "SO Inspector Node";
        so.multiplier = 2f;
        
        return so;
    }
}
```

### 3. 创建 SO 包装器

```csharp
[CreateAssetMenu(fileName = "SONodeWrapper", menuName = "Examples/SONodeWrapper")]
public class SONodeWrapper : ScriptableObject
{
    [Header("节点设置")]
    [Tooltip("节点名称")]
    public string nodeName = "SO Inspector Node";
    
    [Header("数据处理")]
    [Range(1f, 10f)]
    public float multiplier = 2f;
    
    // 添加实用方法
    [ContextMenu("重置为默认值")]
    public void ResetToDefaults()
    {
        nodeName = "SO Inspector Node";
        multiplier = 2f;
    }
}
```

## 实际使用示例

### SONode 完整实现

```csharp
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
        
        processCount++;
        lastProcessTime = Time.realtimeSinceStartup - startTime;
    }
    
    private int ProcessNormal(int inputValue)
    {
        return Mathf.RoundToInt(inputValue * multiplier) + offset;
    }
    
    private int ProcessAdvanced(int inputValue)
    {
        if (useAdvancedSettings)
        {
            return Mathf.RoundToInt(inputValue * advancedMultiplier) + advancedOffset;
        }
        return ProcessNormal(inputValue);
    }
    
    private int ProcessExpert(int inputValue)
    {
        var result = inputValue;
        result = Mathf.RoundToInt(result * multiplier);
        result += offset;
        result = Mathf.RoundToInt(result * 1.5f);
        return result;
    }
    
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
        
        result += Random.Range(-2, 3);
        return result;
    }
    
    [ContextMenu("重置节点状态")]
    public void ResetNodeState()
    {
        processCount = 0;
        lastProcessTime = 0f;
    }
    
    [ContextMenu("随机化配置")]
    public void RandomizeConfig()
    {
        multiplier = Random.Range(1f, 10f);
        offset = Random.Range(-20, 21);
        advancedMultiplier = Random.Range(0.1f, 5f);
        advancedOffset = Random.Range(-10, 11);
        configMode = (ConfigMode)Random.Range(0, 4);
        useAdvancedSettings = Random.value > 0.5f;
    }
    
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
```

### SONodeWrapper 完整实现

```csharp
[CreateAssetMenu(fileName = "SONodeWrapper", menuName = "Examples/SONodeWrapper")]
public class SONodeWrapper : ScriptableObject
{
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
    
    public enum ConfigMode
    {
        Normal,
        Advanced,
        Expert,
        Custom
    }
    
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
    }
    
    [ContextMenu("随机化数值")]
    public void RandomizeValues()
    {
        multiplier = Random.Range(1f, 10f);
        offset = Random.Range(-20, 21);
        advancedMultiplier = Random.Range(0.1f, 5f);
        advancedOffset = Random.Range(-10, 11);
        configMode = (ConfigMode)Random.Range(0, 4);
        useAdvancedSettings = Random.value > 0.5f;
    }
    
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
    }
    
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
    }
}
```

## 测试和验证

### 运行测试

1. **运行测试脚本**:
   - 在Unity菜单栏选择 `Tools > SO Node测试`
   - 依次点击各个测试按钮

2. **创建示例节点**:
   - 在 NodeGraphProcessor 中创建新节点
   - 在节点菜单中找到 `Action/SO Inspector`
   - 创建 SONode 节点

3. **验证功能**:
   - 确认所有属性都正确显示
   - 测试各种属性的编辑功能
   - 验证数据保存和加载

### 测试内容

- **SONode创建测试**: 验证节点类的基本功能
- **SONodeWrapper创建测试**: 验证SO包装器的功能
- **SONodeView测试**: 验证NodeView的实现
- **节点处理测试**: 验证不同配置模式的处理逻辑

## 高级用法

### 1. 动态属性更新

```csharp
public class DynamicSONodeView : SOInspectorWrapper
{
    private ScriptableObject dynamicWrapper;
    
    protected override ScriptableObject CreateTargetSO()
    {
        // 根据条件创建不同的包装器
        if (someCondition)
        {
            dynamicWrapper = ScriptableObject.CreateInstance<TypeAWrapper>();
        }
        else
        {
            dynamicWrapper = ScriptableObject.CreateInstance<TypeBWrapper>();
        }
        
        return dynamicWrapper;
    }
}
```

### 2. 属性验证

```csharp
public class ValidatedSONodeView : SOInspectorWrapper
{
    protected override ScriptableObject CreateTargetSO()
    {
        var so = ScriptableObject.CreateInstance<SONodeWrapper>();
        
        // 验证和修正属性值
        if (so.multiplier < 0) so.multiplier = 0;
        if (so.multiplier > 100) so.multiplier = 100;
        
        return so;
    }
}
```

### 3. 自定义控件

```csharp
private void SetupInspector()
{
    // ... 现有代码 ...
    
    // 添加自定义控件
    var customButton = new Button(() => {
        CustomAction();
    })
    {
        text = "自定义操作"
    };
    
    controlsContainer.Add(customButton);
}
```

## 常见问题

### Q: 属性不显示？
A: 确保字段是 `public` 的，并且没有 `[HideInInspector]` 属性

### Q: 编辑后没有保存？
A: 检查 `SerializedObject.ApplyModifiedProperties()` 是否被调用

### Q: 性能问题？
A: 避免在 `OnInspectorGUI` 中进行复杂计算，使用缓存机制

### Q: 如何添加新的属性类型？
A: 在 SONodeWrapper 中添加对应字段，并添加相应的属性标签

## 扩展功能

### 1. 添加新的属性类型

```csharp
[Header("新功能")]
[Tooltip("新属性")]
public Color newColor = Color.white;

[Tooltip("新数组")]
public string[] newArray = new string[3];
```

### 2. 添加新的处理方法

```csharp
[ContextMenu("新操作")]
public void NewAction()
{
    Debug.Log("执行新操作");
}
```

### 3. 添加新的配置模式

```csharp
public enum ConfigMode
{
    Normal,
    Advanced,
    Expert,
    Custom,
    NewMode  // 新增模式
}
```

## 总结

SO NodeView 系统提供了一个强大而灵活的解决方案，让你可以在 NodeView 中完整地显示和编辑节点的所有属性。通过使用 ScriptableObject 包装器技术，这个系统可以：

- **完整显示**: 显示节点的所有公共字段
- **实时编辑**: 支持所有属性的实时编辑
- **类型安全**: 保持类型安全和属性验证
- **易于扩展**: 可以轻松添加新的属性和功能
- **性能优化**: 高效的属性显示和编辑

这个系统是构建强大节点图编辑器的理想选择，特别适合需要复杂配置和属性管理的节点类型。
