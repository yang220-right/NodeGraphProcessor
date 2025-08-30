using GraphProcessor;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// SimpleSONode的NodeView实现
/// 继承自SOInspectorWrapper，自动创建和显示SO对象
/// </summary>
[NodeCustomEditor(typeof(SimpleSONode))]
public class SimpleSONodeView : SOInspectorWrapper
{
    /// <summary>
    /// 创建目标ScriptableObject
    /// 这里我们创建一个包装了SimpleSONode数据的SO对象
    /// </summary>
    /// <returns>包装了SimpleSONode数据的SO对象</returns>
    protected override SerializedScriptableObject CreateTargetSO()
    {
        var so = SerializedScriptableObject.CreateInstance<SimpleSONodeWrapper>();
        
        // 设置初始值
        so.nodeName = "Simple SO Node";
        so.isEnabled = true;
        so.multiplier = 2f;
        so.offset = 5;
        so.processCount = 0;
        
        Debug.Log("SimpleSONodeWrapper 已创建并初始化");
        return so;
    }
    
    /// <summary>
    /// 重写自动创建方法，添加更多日志信息
    /// </summary>
    protected override void AutoCreateAndDisplaySO()
    {
        Debug.Log("SimpleSONodeView 开始自动创建SO对象...");
        base.AutoCreateAndDisplaySO();
        
        if (targetSO != null)
        {
            Debug.Log($"SimpleSONodeView SO对象创建成功: {targetSO.name}");
            
            // 可以在这里添加一些自定义的初始化逻辑
            var wrapper = targetSO as SimpleSONodeWrapper;
            if (wrapper != null)
            {
                Debug.Log($"SimpleSONodeWrapper 初始化完成，当前倍数: {wrapper.multiplier}, 偏移: {wrapper.offset}");
            }
        }
        else
        {
            Debug.LogError("SimpleSONodeView SO对象创建失败");
        }
    }
}
