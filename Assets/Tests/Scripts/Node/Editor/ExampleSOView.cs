using GraphProcessor;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// ExampleSO的NodeView实现
/// 继承自SOInspectorWrapper，显示ExampleSO的所有属性
/// </summary>
[NodeCustomEditor(typeof(OdinFeatureNode))]
public class ExampleSOView : SOInspectorWrapper
{
    /// <summary>
    /// 创建目标ScriptableObject
    /// </summary>
    /// <returns>ExampleSO实例</returns>
    protected override SerializedScriptableObject CreateTargetSO()
    {
        var so = SerializedScriptableObject.CreateInstance<ExampleSO>();
        so.name = "ExampleSO_Node";
        
        // 可以在这里设置一些初始值
        so.exampleName = "节点中的ExampleSO";
        so.description = "这是在NodeView中创建的ExampleSO实例";
        
        return so;
    }
}
