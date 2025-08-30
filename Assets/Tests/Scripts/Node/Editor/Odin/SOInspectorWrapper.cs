using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Sirenix.OdinInspector;

/// <summary>
/// ScriptableObject属性显示包装器
/// 可以将任何SO对象的所有属性显示在BaseNodeView上
/// </summary>
public abstract class SOInspectorWrapper : BaseNodeView
{
    private IMGUIContainer imguiContainer;
    protected ScriptableObject targetSO;
    private SerializedObject serializedObject;
    private Vector2 scrollPosition;
    
    public override void Enable()
    {
        // 设置节点视图的宽度
        style.width = 450f;
        
        // 自动创建并显示SO对象
        AutoCreateAndDisplaySO();
        
        // 设置界面
        SetupInspector();
    }
    
    /// <summary>
    /// 创建目标ScriptableObject
    /// 子类需要重写此方法来创建具体的SO对象
    /// </summary>
    protected abstract SerializedScriptableObject CreateTargetSO();
    
    /// <summary>
    /// 自动创建并显示SO对象
    /// 这个方法会在Enable时自动调用
    /// </summary>
    protected virtual void AutoCreateAndDisplaySO()
    {
        if (targetSO == null)
        {
            targetSO = CreateTargetSO();
            if (targetSO != null)
            {
                // 自动设置SO的名称
                targetSO.name = $"{GetType().Name}_AutoCreated";
                
                // 初始化Inspector
                InitializeInspector();
                
                Debug.Log($"自动创建SO对象成功: {targetSO.GetType().Name}");
            }
        }
    }
    
    /// <summary>
    /// 设置Inspector界面
    /// </summary>
    private void SetupInspector()
    {
        // 如果SO对象还没有创建，尝试自动创建
        if (targetSO == null)
        {
            AutoCreateAndDisplaySO();
        }
        
        if (targetSO == null)
        {
            SetupErrorUI("目标SO对象创建失败");
            return;
        }
        
        // 创建标题标签
        var titleLabel = new Label($"SO Inspector: {targetSO.GetType().Name}");
        titleLabel.style.fontSize = 16;
        titleLabel.style.color = new Color(0.2f, 0.6f, 0.8f, 1f);
        titleLabel.style.marginBottom = 15;
        titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        
        // 创建说明标签
        var infoLabel = new Label($"显示 {targetSO.GetType().Name} 的所有属性");
        infoLabel.style.fontSize = 12;
        infoLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        infoLabel.style.marginBottom = 20;
        titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        
        // 创建IMGUI容器
        imguiContainer = new IMGUIContainer();
        imguiContainer.style.minHeight = 500;
        imguiContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.1f);
        imguiContainer.style.borderTopWidth = 2;
        imguiContainer.style.borderBottomWidth = 2;
        imguiContainer.style.borderLeftWidth = 2;
        imguiContainer.style.borderRightWidth = 2;
        imguiContainer.style.borderTopColor = new Color(0.2f, 0.6f, 0.8f, 1f);
        imguiContainer.style.borderBottomColor = new Color(0.2f, 0.6f, 0.8f, 1f);
        imguiContainer.style.borderLeftColor = new Color(0.2f, 0.6f, 0.8f, 1f);
        imguiContainer.style.borderRightColor = new Color(0.2f, 0.6f, 0.8f, 1f);
        imguiContainer.style.paddingTop = 15;
        imguiContainer.style.paddingBottom = 15;
        imguiContainer.style.paddingLeft = 15;
        imguiContainer.style.paddingRight = 15;
        imguiContainer.style.marginBottom = 15;
        
        // 注册IMGUI绘制回调
        imguiContainer.onGUIHandler = OnInspectorGUI;
        
        // 创建控制按钮
        var buttonContainer = new VisualElement();
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.justifyContent = Justify.SpaceBetween;
        buttonContainer.style.marginBottom = 15;
        
        // 刷新按钮
        var refreshButton = new Button(() => {
            RefreshInspector();
        })
        {
            text = "刷新 Inspector"
        };
        refreshButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.8f, 0.8f);
        refreshButton.style.borderTopWidth = 1;
        refreshButton.style.borderBottomWidth = 1;
        refreshButton.style.borderLeftWidth = 1;
        refreshButton.style.borderRightWidth = 1;
        refreshButton.style.borderTopColor = new Color(0.4f, 0.8f, 1f, 1f);
        refreshButton.style.borderBottomColor = new Color(0.4f, 0.8f, 1f, 1f);
        refreshButton.style.borderLeftColor = new Color(0.4f, 0.8f, 1f, 1f);
        refreshButton.style.borderRightColor = new Color(0.4f, 0.8f, 1f, 1f);
        refreshButton.style.height = 30;
        refreshButton.style.flexGrow = 1;
        refreshButton.style.marginRight = 5;
        
        // 重新创建按钮
        var recreateButton = new Button(() => {
            RecreateSO();
        })
        {
            text = "重新创建SO"
        };
        recreateButton.style.backgroundColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
        recreateButton.style.borderTopWidth = 1;
        recreateButton.style.borderBottomWidth = 1;
        recreateButton.style.borderLeftWidth = 1;
        recreateButton.style.borderRightWidth = 1;
        recreateButton.style.borderTopColor = new Color(1f, 0.8f, 0.4f, 1f);
        recreateButton.style.borderBottomColor = new Color(1f, 0.8f, 1f, 1f);
        recreateButton.style.borderLeftColor = new Color(1f, 0.8f, 0.4f, 1f);
        recreateButton.style.borderRightColor = new Color(1f, 0.8f, 0.4f, 1f);
        recreateButton.style.height = 30;
        recreateButton.style.flexGrow = 1;
        recreateButton.style.marginLeft = 5;
        
        buttonContainer.Add(refreshButton);
        buttonContainer.Add(recreateButton);
        
        // 状态标签
        var statusLabel = new Label($"状态: 显示 {targetSO.GetType().Name} 的属性");
        statusLabel.style.fontSize = 11;
        statusLabel.style.color = new Color(0.4f, 0.8f, 0.4f, 1f);
        statusLabel.style.marginBottom = 10;
        statusLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        
        // 添加所有控件
        controlsContainer.Add(titleLabel);
        controlsContainer.Add(infoLabel);
        controlsContainer.Add(imguiContainer);
        controlsContainer.Add(buttonContainer);
        controlsContainer.Add(statusLabel);
        
        // 初始化Inspector
        InitializeInspector();
    }
    
    /// <summary>
    /// 设置错误UI界面
    /// </summary>
    private void SetupErrorUI(string errorMessage)
    {
        // 创建错误标题
        var errorTitle = new Label("SO Inspector 错误");
        errorTitle.style.fontSize = 16;
        errorTitle.style.color = new Color(0.8f, 0.4f, 0.4f, 1f);
        errorTitle.style.marginBottom = 15;
        errorTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
        
        // 创建错误信息
        var errorInfo = new Label(errorMessage);
        errorInfo.style.fontSize = 12;
        errorInfo.style.color = new Color(0.8f, 0.4f, 0.4f, 1f);
        errorInfo.style.marginBottom = 20;
        errorInfo.style.unityTextAlign = TextAnchor.MiddleCenter;
        
        // 创建重试按钮
        var retryButton = new Button(() => {
            CreateTargetSO();
            SetupInspector();
        })
        {
            text = "重试"
        };
        retryButton.style.backgroundColor = new Color(0.8f, 0.4f, 0.4f, 0.8f);
        retryButton.style.height = 30;
        retryButton.style.width = 100;
        retryButton.style.alignSelf = Align.Center;
        
        // 添加控件
        controlsContainer.Add(errorTitle);
        controlsContainer.Add(errorInfo);
        controlsContainer.Add(retryButton);
    }
    
    /// <summary>
    /// 初始化Inspector
    /// </summary>
    private void InitializeInspector()
    {
        try
        {
            if (targetSO != null)
            {
                // 创建SerializedObject
                serializedObject = new SerializedObject(targetSO);
                Debug.Log($"SO Inspector 初始化成功，目标: {targetSO.GetType().Name}");
            }
        }
        catch (ExitGUIException)
        {
            // ExitGUIException是Unity的正常行为，不需要记录错误
            return;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SO Inspector 初始化失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 刷新Inspector
    /// </summary>
    private void RefreshInspector()
    {
        try
        {
            if (serializedObject != null)
            {
                serializedObject.Update();
            }
            
            if (imguiContainer != null)
            {
                imguiContainer.MarkDirtyRepaint();
            }
            
            Debug.Log("Inspector 已刷新");
        }
        catch (ExitGUIException)
        {
            // ExitGUIException是Unity的正常行为，不需要记录错误
            return;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"刷新Inspector时出错: {e.Message}");
        }
    }
    
    /// <summary>
    /// 重新创建SO对象
    /// </summary>
    private void RecreateSO()
    {
        try
        {
            // 清理现有资源
            if (serializedObject != null)
            {
                serializedObject.Dispose();
                serializedObject = null;
            }
            
            if (targetSO != null)
            {
                targetSO = null;
            }
            
            // 清空控件容器
            controlsContainer.Clear();
            
            // 重新创建和设置
            CreateTargetSO();
            SetupInspector();
            
            Debug.Log("SO对象已重新创建");
        }
        catch (ExitGUIException)
        {
            // ExitGUIException是Unity的正常行为，不需要记录错误
            return;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"重新创建SO对象时出错: {e.Message}");
        }
    }
    
    /// <summary>
    /// Inspector的GUI绘制方法
    /// </summary>
    private void OnInspectorGUI()
    {
        if (serializedObject == null || targetSO == null)
        {
            EditorGUILayout.HelpBox("Inspector 未初始化", MessageType.Warning);
            return;
        }
        
        // 修复ExitGUIException报错问题：捕获异常后直接return，避免GUI状态异常
        try
        {
            // 设置背景色
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            
            // 绘制背景
            var rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1f));
            
            // 开始滚动视图
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 显示SO信息
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("ScriptableObject 信息", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("类型:", targetSO.GetType().Name);
            EditorGUILayout.LabelField("名称:", targetSO.name);
            EditorGUILayout.Space(10);
            
            // 使用默认Inspector绘制所有属性
            EditorGUILayout.LabelField("属性列表:", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            if (serializedObject != null)
            {
                serializedObject.Update(); // 更新序列化对象
                
                // 绘制所有序列化属性
                var iterator = serializedObject.GetIterator();
                bool enterChildren = true;
                
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    
                    // 跳过根对象
                    if (iterator.propertyPath == "m_Script")
                        continue;
                    
                    // 绘制属性
                    try
                    {
                        // 检查属性是否有效
                        if (iterator.serializedObject == null)
                        {
                            EditorGUILayout.HelpBox("属性无效或已被销毁，无法绘制。", MessageType.Warning);
                            EditorGUILayout.EndScrollView();
                            return; // 避免继续绘制无效属性
                        }
                        
                        // 绘制属性字段
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                    catch (ExitGUIException)
                    {
                        // ExitGUIException是Unity的正常行为，用于中断GUI绘制
                        // 我们不需要记录警告，直接结束绘制即可
                        EditorGUILayout.EndScrollView();
                        return;
                    }
                    catch (System.Exception ex)
                    {
                        // 记录其他异常，但继续绘制其他属性
                        EditorGUILayout.HelpBox($"属性 {iterator?.displayName ?? "未知"} 绘制出错: {ex.Message}", MessageType.Error);
                        Debug.LogError($"属性 {iterator?.displayName ?? "未知"} 绘制出错: {ex}");
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();
            
            // 应用修改
            if (serializedObject != null && serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                Debug.Log("SO属性已修改并应用");
            }
        }
        catch (ExitGUIException)
        {
            // ExitGUIException是Unity的正常行为，不需要记录错误
            // 直接return即可
            return;
        }
        catch (System.Exception e)
        {
            EditorGUILayout.HelpBox($"Inspector 绘制错误: {e.Message}", MessageType.Error);
            Debug.LogError($"Inspector 绘制错误: {e.Message}");
        }
    }
    
    /// <summary>
    /// 清理资源
    /// </summary>
    public override void Disable()
    {
        if (serializedObject != null)
        {
            serializedObject.Dispose();
            serializedObject = null;
        }
        
        if (targetSO != null)
        {
            targetSO = null;
        }
        
        base.Disable();
    }
}
