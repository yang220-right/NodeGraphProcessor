# SO Inspector Wrapper 使用指南

## 概述

`SOInspectorWrapper` 是一个强大的工具，可以将任何 `ScriptableObject` 的所有属性完整地显示在 `BaseNodeView` 上。这使得你可以在节点图中直接编辑复杂的配置数据，而无需切换到 Inspector 窗口。

## 核心特性

### ✅ 完整属性显示
- 显示 SO 对象的所有公共字段
- 支持所有 Unity 内置类型
- 支持自定义类型和结构
- 保持原有的属性标签和限制

### ✅ 实时编辑
- 所有属性都可以直接在 NodeView 中编辑
- 实时保存修改
- 支持撤销/重做操作

### ✅ 类型支持
- **基础类型**: string, int, float, bool
- **Unity类型**: Vector3, Color, GameObject, Material, Texture
- **数组类型**: 支持所有类型的数组
- **枚举类型**: 下拉选择器
- **自定义结构**: 可展开编辑
- **引用类型**: 拖拽赋值

### ✅ 属性标签支持
- `[Header]` - 分组标题
- `[Tooltip]` - 工具提示
- `[Range]` - 数值范围限制
- `[Min]` - 最小值限制
- `[TextArea]` - 多行文本
- 所有其他 Unity 属性标签

## 使用方法

### 1. 创建 ScriptableObject 类

```csharp
[CreateAssetMenu(fileName = "MySO", menuName = "Examples/MySO")]
public class MySO : ScriptableObject
{
    [Header("基础设置")]
    [Tooltip("名称")]
    public string name = "默认名称";
    
    [Header("数值设置")]
    [Range(0f, 100f)]
    public float value = 50f;
    
    [Header("枚举设置")]
    public MyEnum myEnum = MyEnum.Option1;
    
    public enum MyEnum
    {
        Option1,
        Option2,
        Option3
    }
}
```

### 2. 创建 NodeView 类

```csharp
public class MySOView : SOInspectorWrapper
{
    protected override ScriptableObject CreateTargetSO()
    {
        var so = ScriptableObject.CreateInstance<MySO>();
        so.name = "MySO_Node";
        
        // 设置初始值
        so.name = "节点中的MySO";
        
        return so;
    }
}
```

### 3. 在节点中使用

```csharp
[System.Serializable, NodeMenuItem("Examples/MySO")]
public class MySONode : BaseNode
{
    [Input(name = "Input")]
    public int input;
    
    [Output(name = "Output")]
    public int output;
    
    public override string name => "MySO";
    
    protected override void Process()
    {
        // 处理逻辑
        output = input + 1;
    }
}
```

## 文件结构

```
Tests/Scripts/Node/Editor/
├── SOInspectorWrapper.cs      # 核心包装器类
├── ExampleSO.cs              # 示例SO类
├── ExampleSOView.cs          # 示例NodeView
├── SOInspectorTest.cs        # 测试脚本
└── README_SOInspector.md     # 本文档
```

## 核心类说明

### SOInspectorWrapper

这是核心的抽象基类，提供了以下功能：

- **自动属性显示**: 自动检测并显示所有公共字段
- **实时编辑**: 支持所有属性的实时编辑
- **错误处理**: 完善的错误处理和用户提示
- **资源管理**: 自动管理 SO 对象的生命周期

#### 主要方法

```csharp
// 子类必须实现的方法
protected abstract ScriptableObject CreateTargetSO();

// 内部方法（自动调用）
private void SetupInspector();           // 设置界面
private void InitializeInspector();      // 初始化Inspector
private void RefreshInspector();         // 刷新显示
private void RecreateSO();              // 重新创建SO
```

### ExampleSO

这是一个完整的示例 SO 类，展示了各种属性的使用：

- **基础设置**: 字符串、描述文本
- **数值设置**: 范围限制、最小值限制
- **布尔设置**: 开关选项
- **枚举设置**: 多选项枚举
- **向量设置**: 位置、旋转、缩放
- **颜色设置**: 主色、辅助色
- **数组设置**: 字符串数组、数值数组
- **引用设置**: GameObject、Material、Texture
- **自定义结构**: 可序列化的自定义数据

## 高级用法

### 1. 自定义初始值

```csharp
protected override ScriptableObject CreateTargetSO()
{
    var so = ScriptableObject.CreateInstance<MySO>();
    
    // 设置自定义初始值
    so.name = "自定义名称";
    so.value = 75f;
    so.myEnum = MySO.MyEnum.Option2;
    
    return so;
}
```

### 2. 动态属性更新

```csharp
public class DynamicSOView : SOInspectorWrapper
{
    private ScriptableObject dynamicSO;
    
    protected override ScriptableObject CreateTargetSO()
    {
        // 根据条件创建不同的SO
        if (someCondition)
        {
            dynamicSO = ScriptableObject.CreateInstance<TypeA>();
        }
        else
        {
            dynamicSO = ScriptableObject.CreateInstance<TypeB>();
        }
        
        return dynamicSO;
    }
}
```

### 3. 属性验证

```csharp
public class ValidatedSOView : SOInspectorWrapper
{
    protected override ScriptableObject CreateTargetSO()
    {
        var so = ScriptableObject.CreateInstance<MySO>();
        
        // 验证和修正属性值
        if (so.value < 0) so.value = 0;
        if (so.value > 100) so.value = 100;
        
        return so;
    }
}
```

## 测试和调试

### 运行测试

1. 在Unity菜单栏选择 `Tools > SO Inspector测试`
2. 依次点击各个测试按钮
3. 查看测试结果和调试信息

### 常见问题

#### Q: 属性不显示？
A: 确保字段是 `public` 的，并且没有 `[HideInInspector]` 属性

#### Q: 编辑后没有保存？
A: 检查 `SerializedObject.ApplyModifiedProperties()` 是否被调用

#### Q: 性能问题？
A: 避免在 `OnInspectorGUI` 中进行复杂计算，使用缓存机制

## 扩展功能

### 1. 添加自定义控件

```csharp
private void SetupInspector()
{
    // ... 现有代码 ...
    
    // 添加自定义控件
    var customButton = new Button(() => {
        // 自定义操作
        CustomAction();
    })
    {
        text = "自定义操作"
    };
    
    controlsContainer.Add(customButton);
}
```

### 2. 属性过滤

```csharp
private void OnInspectorGUI()
{
    // ... 现有代码 ...
    
    // 过滤特定属性
    var iterator = serializedObject.GetIterator();
    bool enterChildren = true;
    
    while (iterator.NextVisible(enterChildren))
    {
        enterChildren = false;
        
        // 跳过不需要的属性
        if (ShouldSkipProperty(iterator.propertyPath))
            continue;
        
        EditorGUILayout.PropertyField(iterator, true);
    }
}

private bool ShouldSkipProperty(string propertyPath)
{
    // 自定义过滤逻辑
    return propertyPath.Contains("Internal") || 
           propertyPath.Contains("Debug");
}
```

### 3. 条件显示

```csharp
private void OnInspectorGUI()
{
    // ... 现有代码 ...
    
    // 根据条件显示不同内容
    if (showAdvanced)
    {
        EditorGUILayout.LabelField("高级设置", EditorStyles.boldLabel);
        // 显示高级属性
    }
    else
    {
        EditorGUILayout.LabelField("基础设置", EditorStyles.boldLabel);
        // 显示基础属性
    }
}
```

## 总结

`SOInspectorWrapper` 提供了一个强大而灵活的解决方案，让你可以在 NodeView 中完整地显示和编辑 ScriptableObject 的所有属性。通过继承这个基类，你可以轻松地为任何 SO 类型创建对应的节点视图，大大提升了节点图编辑器的功能性和易用性。

## 技术支持

如果在使用过程中遇到问题，可以：

1. 运行测试脚本查看详细错误信息
2. 检查 Console 窗口的错误日志
3. 验证 SO 类的字段访问权限
4. 确认属性标签的正确使用

这个系统设计得非常灵活，可以适应各种复杂的使用场景，是构建强大节点图编辑器的理想选择。
