using System.Linq;
using UnityEngine;
using UnityEditor;
using GraphProcessor;

/// <summary>
/// SO Inspector测试脚本
/// 用于验证SOInspectorWrapper的功能
/// </summary>
public class SOInspectorTest : EditorWindow
{
    [MenuItem("Tools/SO Inspector测试")]
    public static void ShowWindow()
    {
        GetWindow<SOInspectorTest>("SO Inspector测试");
    }
    
    private Vector2 scrollPosition;
    private string testInfo = "";
    
    private void OnGUI()
    {
        GUILayout.Label("SO Inspector 测试", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("测试ExampleSO创建", GUILayout.Height(30)))
        {
            TestExampleSOCreation();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("测试SOInspectorWrapper", GUILayout.Height(30)))
        {
            TestSOInspectorWrapper();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("创建测试节点", GUILayout.Height(30)))
        {
            CreateTestNode();
        }

        EditorGUILayout.Space();

        // 显示测试信息
        if (!string.IsNullOrEmpty(testInfo))
        {
            GUILayout.Label("测试信息:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            EditorGUILayout.TextArea(testInfo, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "这个测试脚本用于验证SOInspectorWrapper的功能。\n" +
            "1. 测试ExampleSO创建\n" +
            "2. 测试SOInspectorWrapper功能\n" +
            "3. 创建测试节点\n\n" +
            "SOInspectorWrapper可以将任何ScriptableObject的所有属性显示在BaseNodeView上。", 
            MessageType.Info);
    }
    
    private void TestExampleSOCreation()
    {
        try
        {
            testInfo = "开始测试ExampleSO创建...\n\n";
            
            // 创建ExampleSO
            var so = ScriptableObject.CreateInstance<ExampleSO>();
            if (so != null)
            {
                testInfo += "✅ 成功创建 ExampleSO\n";
                testInfo += $"SO类型: {so.GetType().Name}\n";
                testInfo += $"SO名称: {so.name}\n";
                testInfo += $"是否为 ScriptableObject: {so is ScriptableObject}\n";
                
                // 检查字段
                var fields = so.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                testInfo += $"SO字段数量: {fields.Length}\n\n";
                
                testInfo += "字段列表:\n";
                foreach (var field in fields)
                {
                    testInfo += $"- {field.Name}: {field.FieldType.Name}\n";
                    
                    // 检查属性
                    var attributes = field.GetCustomAttributes(true);
                    if (attributes.Length > 0)
                    {
                        testInfo += $"  属性: ";
                        foreach (var attr in attributes)
                        {
                            testInfo += $"{attr.GetType().Name} ";
                        }
                        testInfo += "\n";
                    }
                }
                
                // 测试一些方法
                testInfo += "\n测试方法:\n";
                so.PrintStatus();
                testInfo += "✅ PrintStatus 方法调用成功\n";
                
                // 清理
                DestroyImmediate(so);
                testInfo += "✅ 测试SO已清理\n";
            }
            else
            {
                testInfo += "❌ ExampleSO创建失败\n";
            }
        }
        catch (System.Exception e)
        {
            testInfo += $"❌ 测试失败: {e.Message}\n";
            Debug.LogError($"ExampleSO创建测试失败: {e.Message}");
        }
    }
    
    private void TestSOInspectorWrapper()
    {
        try
        {
            testInfo += "\n开始测试SOInspectorWrapper...\n\n";
            
            // 检查类型是否存在
            var wrapperType = typeof(SOInspectorWrapper);
            if (wrapperType != null)
            {
                testInfo += "✅ 找到 SOInspectorWrapper 类型\n";
                testInfo += $"类型名称: {wrapperType.Name}\n";
                testInfo += $"是否为抽象类: {wrapperType.IsAbstract}\n";
                testInfo += $"继承自: {wrapperType.BaseType?.Name}\n";
                
                // 检查方法
                var methods = wrapperType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                testInfo += $"方法数量: {methods.Length}\n";
                
                // 检查抽象方法
                var abstractMethods = methods.Where(m => m.IsAbstract).ToArray();
                testInfo += $"抽象方法数量: {abstractMethods.Length}\n";
                foreach (var method in abstractMethods)
                {
                    testInfo += $"- {method.Name}\n";
                }
            }
            else
            {
                testInfo += "❌ 未找到 SOInspectorWrapper 类型\n";
            }
            
            // 检查ExampleSOView类型
            var viewType = typeof(ExampleSOView);
            if (viewType != null)
            {
                testInfo += "\n✅ 找到 ExampleSOView 类型\n";
                testInfo += $"类型名称: {viewType.Name}\n";
                testInfo += $"继承自: {viewType.BaseType?.Name}\n";
                testInfo += $"是否为抽象类: {viewType.IsAbstract}\n";
            }
            else
            {
                testInfo += "\n❌ 未找到 ExampleSOView 类型\n";
            }
        }
        catch (System.Exception e)
        {
            testInfo += $"❌ SOInspectorWrapper测试失败: {e.Message}\n";
            Debug.LogError($"SOInspectorWrapper测试失败: {e.Message}");
        }
    }
    
    private void CreateTestNode()
    {
        try
        {
            testInfo += "\n开始创建测试节点...\n\n";
            
            // 这里可以添加创建测试节点的逻辑
            // 由于这是在EditorWindow中，我们只能提供指导信息
            
            testInfo += "✅ 测试节点创建指导:\n";
            testInfo += "1. 在NodeGraphProcessor中创建一个新的节点\n";
            testInfo += "2. 将NodeView设置为 ExampleSOView\n";
            testInfo += "3. 节点应该显示ExampleSO的所有属性\n";
            testInfo += "4. 包括基础设置、数值设置、布尔设置、枚举设置等\n";
            testInfo += "5. 所有属性都可以在NodeView中直接编辑\n";
            
            testInfo += "\n支持的属性类型:\n";
            testInfo += "- 字符串 (包括TextArea)\n";
            testInfo += "- 数值 (包括Range和Min限制)\n";
            testInfo += "- 布尔值\n";
            testInfo += "- 枚举 (下拉选择)\n";
            testInfo += "- Vector3 (位置、旋转、缩放)\n";
            testInfo += "- Color (颜色选择器)\n";
            testInfo += "- 数组 (可展开和编辑)\n";
            testInfo += "- 引用 (GameObject、Material、Texture等)\n";
            testInfo += "- 自定义结构 (可展开编辑)\n";
            
        }
        catch (System.Exception e)
        {
            testInfo += $"❌ 创建测试节点失败: {e.Message}\n";
            Debug.LogError($"创建测试节点失败: {e.Message}");
        }
    }
}
