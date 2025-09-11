using System;
using GraphProcessor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Timeline节点的NodeView实现
/// 继承自BaseSONodeView，专门用于显示Timeline的所有属性
/// </summary>
[NodeCustomEditor(typeof(TimelineNode))]
public class TimelineNodeView : BaseSONodeView
{
    private TimelineSO timelineSO;
    private bool isInitialized = false;
    private bool isEditorPlaying = false;
    
    protected override void SetWidth()
    {
        style.width = 500f;
    }
    
    /// <summary>
    /// 创建目标ScriptableObject
    /// 这里我们创建一个TimelineSO对象
    /// </summary>
    /// <returns>TimelineSO对象</returns>
    protected override ScriptableObject CreateSO()
    {
        timelineSO = CreateInstance<TimelineSO>();
        return timelineSO;
    }
    
    public override void Enable()
    {
        base.Enable();
        
        // 确保TimelineSO被正确初始化
        if (timelineSO == null && targetSO is TimelineSO ts)
        {
            timelineSO = ts;
        }
        
        // 初始化帧数据
        if (timelineSO != null && !isInitialized)
        {
            InitializeTimeline();
            isInitialized = true;
        }
        
        // 不在这里注册，而是在播放时注册
    }
    
    public override void Disable()
    {
        // 取消注册编辑器更新回调
        EditorApplication.update -= OnEditorUpdate;
        
        // 停止编辑器播放
        if (isEditorPlaying)
        {
            StopEditorPlayback();
        }
        
        base.Disable();
    }
    
    /// <summary>
    /// 初始化Timeline
    /// </summary>
    private void InitializeTimeline()
    {
        if (timelineSO == null) return;
        
        // 如果帧数据为空，自动初始化
        if (timelineSO.frameData == null || timelineSO.frameData.Length == 0)
        {
            timelineSO.InitializeFrameData();
        }
        
        // 设置保存路径
        SetSavePath("Assets/NodeSO/TimelineNodeView");
        SetFileName("TimelineNodeView");
    }
    
    /// <summary>
    /// 编辑器更新回调
    /// </summary>
    private void OnEditorUpdate()
    {
        // 确保timelineSO被正确初始化
        if (timelineSO == null && targetSO is TimelineSO ts)
        {
            timelineSO = ts;
            Debug.Log("TimelineSO已初始化");
        }
        
        if (timelineSO != null && timelineSO.isPlaying)
        {
            // 更新Timeline
            timelineSO.UpdateTimeline();
            
            // 更新节点输出
            if (nodeTarget is TimelineNode timelineNode)
            {
                timelineNode.currentFrame = timelineSO.currentFrame;
                timelineNode.isPlaying = timelineSO.isPlaying;
            }
            
            // 标记需要重绘
            if (imguiContainer != null)
            {
                imguiContainer.MarkDirtyRepaint();
            }
            
            // 标记场景为已修改
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(timelineSO);
            }
            
            // 每10帧打印一次调试信息
            if (timelineSO.currentFrame % 10 == 0)
            {
                Debug.Log($"编辑器播放中 - 当前帧: {timelineSO.currentFrame}, 播放时间: {timelineSO.playTime:F2}s");
            }
        }
    }
    
    /// <summary>
    /// 开始编辑器播放
    /// </summary>
    private void StartEditorPlayback()
    {
        if (!isEditorPlaying)
        {
            isEditorPlaying = true;
            EditorApplication.update += OnEditorUpdate;
            Debug.Log("编辑器播放已启动");
        }
    }
    
    /// <summary>
    /// 停止编辑器播放
    /// </summary>
    private void StopEditorPlayback()
    {
        if (isEditorPlaying)
        {
            isEditorPlaying = false;
            EditorApplication.update -= OnEditorUpdate;
            Debug.Log("编辑器播放已停止");
        }
    }
    
    /// <summary>
    /// 重写SetupInspector以添加Timeline特定的UI
    /// </summary>
    protected override void SetupInspector()
    {
        // 调用基类的SetupInspector
        base.SetupInspector();
        
        // 创建Timeline特定的IMGUI容器
        var timelineContainer = CreateDefaultGUIContainer();
        timelineContainer.onGUIHandler = OnTimelineGUI;
        
        // 将Timeline容器添加到控件容器中
        controlsContainer.Add(timelineContainer);
    }
    
    /// <summary>
    /// Timeline特定的GUI绘制方法
    /// </summary>
    private void OnTimelineGUI()
    {
        if (timelineSO == null)
        {
            EditorGUILayout.HelpBox("TimelineSO 未初始化", MessageType.Warning);
            return;
        }
        
        // 绘制Timeline控制按钮
        DrawTimelineControls();
    }
    
    /// <summary>
    /// 绘制Timeline控制按钮
    /// </summary>
    private void DrawTimelineControls()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Timeline 控制", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // 播放/暂停按钮
        if (timelineSO.isPlaying)
        {
            if (GUILayout.Button("⏸️ 暂停", GUILayout.Height(30)))
            {
                timelineSO.Pause();
                StopEditorPlayback();
            }
        }
        else
        {
            if (GUILayout.Button("▶️ 播放", GUILayout.Height(30)))
            {
                timelineSO.Play();
                StartEditorPlayback();
            }
        }
        
        // 停止按钮
        if (GUILayout.Button("⏹️ 停止", GUILayout.Height(30)))
        {
            timelineSO.Stop();
            StopEditorPlayback();
        }
        
        // 重置按钮
        if (GUILayout.Button("🔄 重置", GUILayout.Height(30)))
        {
            timelineSO.Reset();
            StopEditorPlayback();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 绘制播放进度条
        DrawPlaybackProgressBar();
        
        // 绘制时间轴
        DrawTimelineRuler();
        
        // 帧跳转控制
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("跳转到帧:", GUILayout.Width(80));
        int targetFrame = EditorGUILayout.IntField(timelineSO.currentFrame, GUILayout.Width(60));
        if (targetFrame != timelineSO.currentFrame)
        {
            timelineSO.currentFrame = Mathf.Clamp(targetFrame, 0, timelineSO.totalFrames - 1);
            timelineSO.GoToFrame();
        }
        
        if (GUILayout.Button("跳转", GUILayout.Width(50)))
        {
            timelineSO.GoToFrame();
        }
        EditorGUILayout.EndHorizontal();
        
        // 显示当前帧信息
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"当前帧: {timelineSO.currentFrame} / {timelineSO.totalFrames - 1}");
        
        // 显示播放状态和模式
        string playStatus = timelineSO.isPlaying ? "播放中" : "已暂停";
        string playMode = Application.isPlaying ? "运行时模式" : "编辑器模式";
        EditorGUILayout.LabelField($"播放状态: {playStatus} ({playMode})");
        EditorGUILayout.LabelField($"播放时间: {timelineSO.playTime:F2}s");
        
        // 显示编辑器播放状态
        if (!Application.isPlaying && timelineSO.isPlaying)
        {
            EditorGUILayout.HelpBox("编辑器模式下播放中 - 无需运行Unity即可预览Timeline", MessageType.Info);
        }
        
        // 隐藏调试按钮，保持界面简洁
        // EditorGUILayout.Space(5);
        // EditorGUILayout.BeginHorizontal();
        // if (GUILayout.Button("测试播放", GUILayout.Height(25)))
        // {
        //     TestPlayback();
        // }
        // if (GUILayout.Button("强制更新", GUILayout.Height(25)))
        // {
        //     ForceUpdate();
        // }
        // EditorGUILayout.EndHorizontal();
        
        // 隐藏当前帧数据，保持界面简洁
        // var currentFrameData = timelineSO.GetCurrentFrameData();
        // if (currentFrameData != null)
        // {
        //     EditorGUILayout.Space(5);
        //     EditorGUILayout.LabelField("当前帧数据:", EditorStyles.boldLabel);
        //     EditorGUILayout.LabelField($"内容: {currentFrameData.frameContent}");
        //     EditorGUILayout.LabelField($"关键帧: {(currentFrameData.isKeyFrame ? "是" : "否")}");
        // }
    }
    
    /// <summary>
    /// 绘制播放进度条
    /// </summary>
    private void DrawPlaybackProgressBar()
    {
        EditorGUILayout.Space(5);
        
        // 计算播放进度
        float progress = timelineSO.totalFrames > 0 ? (float)timelineSO.currentFrame / (timelineSO.totalFrames - 1) : 0f;
        progress = Mathf.Clamp01(progress);
        
        // 绘制进度条背景
        Rect progressRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(progressRect, new Color(0.3f, 0.3f, 0.3f, 1f));
        
        // 绘制进度条填充
        Rect fillRect = new Rect(progressRect.x, progressRect.y, progressRect.width * progress, progressRect.height);
        Color progressColor = timelineSO.isPlaying ? new Color(0.2f, 0.8f, 0.2f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
        EditorGUI.DrawRect(fillRect, progressColor);
        
        // 绘制进度条边框
        EditorGUI.DrawRect(new Rect(progressRect.x, progressRect.y, progressRect.width, 1), Color.gray);
        EditorGUI.DrawRect(new Rect(progressRect.x, progressRect.y + progressRect.height - 1, progressRect.width, 1), Color.gray);
        EditorGUI.DrawRect(new Rect(progressRect.x, progressRect.y, 1, progressRect.height), Color.gray);
        EditorGUI.DrawRect(new Rect(progressRect.x + progressRect.width - 1, progressRect.y, 1, progressRect.height), Color.gray);
        
        // 绘制进度文本
        string progressText = $"进度: {progress * 100:F1}% ({timelineSO.currentFrame}/{timelineSO.totalFrames - 1})";
        GUI.Label(progressRect, progressText, EditorStyles.centeredGreyMiniLabel);
    }
    
    /// <summary>
    /// 绘制时间轴刻度
    /// </summary>
    private void DrawTimelineRuler()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("时间轴", EditorStyles.boldLabel);
        
        // 获取时间轴区域
        Rect timelineRect = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
        
        // 绘制背景渐变
        DrawGradientBackground(timelineRect);
        
        // 绘制边框
        DrawTimelineBorder(timelineRect);
        
        // 计算刻度参数
        float totalFrames = timelineSO.totalFrames;
        float currentFrame = timelineSO.currentFrame;
        float frameWidth = timelineRect.width / totalFrames;
        
        // 绘制刻度线
        DrawTimelineTicks(timelineRect, totalFrames, frameWidth);
        
        // 绘制关键帧标记
        DrawKeyFrameMarkers(timelineRect, frameWidth);
        
        // 绘制当前帧指示器
        DrawCurrentFrameIndicator(timelineRect, currentFrame, frameWidth);
        
        // 绘制时间标签
        DrawTimeLabels(timelineRect, totalFrames);
        
        // 处理鼠标点击
        HandleTimelineClick(timelineRect, frameWidth);
    }
    
    /// <summary>
    /// 绘制时间轴刻度线
    /// </summary>
    private void DrawTimelineTicks(Rect timelineRect, float totalFrames, float frameWidth)
    {
        // 计算主要刻度间隔（每10帧一个主要刻度）
        int majorTickInterval = Mathf.Max(1, Mathf.RoundToInt(totalFrames / 10));
        
        for (int i = 0; i <= totalFrames; i++)
        {
            float x = timelineRect.x + i * frameWidth;
            
            // 主要刻度线
            if (i % majorTickInterval == 0)
            {
                // 绘制主要刻度线（白色，更粗）
                EditorGUI.DrawRect(new Rect(x - 0.5f, timelineRect.y + 5, 2, 25), Color.white);
                
                // 绘制刻度线阴影效果
                EditorGUI.DrawRect(new Rect(x - 0.5f, timelineRect.y + 5, 1, 25), new Color(0.8f, 0.8f, 0.8f, 0.5f));
            }
            // 次要刻度线
            else
            {
                // 绘制次要刻度线（灰色，较细）
                EditorGUI.DrawRect(new Rect(x, timelineRect.y + 12, 1, 12), new Color(0.6f, 0.6f, 0.6f, 0.8f));
            }
        }
    }
    
    /// <summary>
    /// 绘制关键帧标记
    /// </summary>
    private void DrawKeyFrameMarkers(Rect timelineRect, float frameWidth)
    {
        if (timelineSO.frameData == null) return;
        
        for (int i = 0; i < timelineSO.frameData.Length; i++)
        {
            if (timelineSO.frameData[i].isKeyFrame)
            {
                float x = timelineRect.x + i * frameWidth;
                // 绘制关键帧标记（黄色菱形）
                Vector3[] diamond = new Vector3[4]
                {
                    new Vector3(x, timelineRect.y + 15, 0),
                    new Vector3(x + 3, timelineRect.y + 12, 0),
                    new Vector3(x, timelineRect.y + 9, 0),
                    new Vector3(x - 3, timelineRect.y + 12, 0)
                };
                
                // 使用Handles绘制菱形
                Handles.color = Color.yellow;
                Handles.DrawPolyLine(diamond);
                Handles.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// 绘制当前帧指示器
    /// </summary>
    private void DrawCurrentFrameIndicator(Rect timelineRect, float currentFrame, float frameWidth)
    {
        float x = timelineRect.x + currentFrame * frameWidth;
        
        // 绘制当前帧指示线（红色，带阴影效果）
        EditorGUI.DrawRect(new Rect(x - 1, timelineRect.y, 3, timelineRect.height), new Color(1f, 0.2f, 0.2f, 0.8f));
        EditorGUI.DrawRect(new Rect(x, timelineRect.y, 1, timelineRect.height), Color.red);
        
        // 绘制当前帧指示器（红色三角形，带阴影）
        Vector3[] triangle = new Vector3[3]
        {
            new Vector3(x, timelineRect.y + 2, 0),
            new Vector3(x - 5, timelineRect.y + 10, 0),
            new Vector3(x + 5, timelineRect.y + 10, 0)
        };
        
        // 绘制三角形阴影
        Vector3[] shadowTriangle = new Vector3[3]
        {
            new Vector3(x + 1, timelineRect.y + 3, 0),
            new Vector3(x - 4, timelineRect.y + 11, 0),
            new Vector3(x + 6, timelineRect.y + 11, 0)
        };
        
        Handles.color = new Color(0.5f, 0.1f, 0.1f, 0.8f);
        Handles.DrawAAConvexPolygon(shadowTriangle);
        
        Handles.color = Color.red;
        Handles.DrawAAConvexPolygon(triangle);
        Handles.color = Color.white;
        
        // 绘制当前帧数字
        string frameText = currentFrame.ToString();
        Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(frameText));
        Rect textRect = new Rect(x - textSize.x / 2, timelineRect.y + 15, textSize.x, 15);
        
        // 绘制文字背景
        EditorGUI.DrawRect(textRect, new Color(0, 0, 0, 0.7f));
        
        // 绘制文字
        GUI.Label(textRect, frameText, EditorStyles.centeredGreyMiniLabel);
    }
    
    /// <summary>
    /// 绘制时间标签
    /// </summary>
    private void DrawTimeLabels(Rect timelineRect, float totalFrames)
    {
        // 计算时间标签间隔
        int labelInterval = Mathf.Max(1, Mathf.RoundToInt(totalFrames / 5));
        
        for (int i = 0; i <= totalFrames; i += labelInterval)
        {
            float x = timelineRect.x + i * (timelineRect.width / totalFrames);
            float time = i / timelineSO.frameRate;
            
            // 绘制时间标签
            GUI.Label(new Rect(x - 20, timelineRect.y + 25, 40, 20), 
                     $"{time:F1}s", EditorStyles.centeredGreyMiniLabel);
        }
    }
    
    /// <summary>
    /// 处理时间轴点击
    /// </summary>
    private void HandleTimelineClick(Rect timelineRect, float frameWidth)
    {
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown && timelineRect.Contains(e.mousePosition))
        {
            // 计算点击的帧
            float clickX = e.mousePosition.x - timelineRect.x;
            int clickedFrame = Mathf.RoundToInt(clickX / frameWidth);
            
            // 设置当前帧
            timelineSO.currentFrame = Mathf.Clamp(clickedFrame, 0, timelineSO.totalFrames - 1);
            timelineSO.GoToFrame();
            
            // 标记需要重绘
            if (imguiContainer != null)
            {
                imguiContainer.MarkDirtyRepaint();
            }
            
            e.Use();
        }
    }
    
    /// <summary>
    /// 绘制渐变背景
    /// </summary>
    private void DrawGradientBackground(Rect rect)
    {
        // 绘制基础背景
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));
        
        // 绘制渐变效果
        for (int i = 0; i < rect.height; i++)
        {
            float t = (float)i / rect.height;
            Color color = Color.Lerp(new Color(0.2f, 0.2f, 0.2f, 1f), new Color(0.1f, 0.1f, 0.1f, 1f), t);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + i, rect.width, 1), color);
        }
    }
    
    /// <summary>
    /// 绘制时间轴边框
    /// </summary>
    private void DrawTimelineBorder(Rect rect)
    {
        // 绘制外边框
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), new Color(0.6f, 0.6f, 0.6f, 1f));
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), new Color(0.6f, 0.6f, 0.6f, 1f));
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), new Color(0.6f, 0.6f, 0.6f, 1f));
        EditorGUI.DrawRect(new Rect(rect.x + rect.width - 1, rect.y, 1, rect.height), new Color(0.6f, 0.6f, 0.6f, 1f));
        
        // 绘制内边框（高光效果）
        EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, 1), new Color(0.8f, 0.8f, 0.8f, 0.3f));
        EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, 1, rect.height - 2), new Color(0.8f, 0.8f, 0.8f, 0.3f));
    }
    
    /// <summary>
    /// 测试播放功能
    /// </summary>
    private void TestPlayback()
    {
        if (timelineSO == null)
        {
            Debug.LogError("TimelineSO为空！");
            return;
        }
        
        Debug.Log("开始测试播放...");
        Debug.Log($"Timeline状态 - 总帧数: {timelineSO.totalFrames}, 帧率: {timelineSO.frameRate}, 播放速度: {timelineSO.playbackSpeed}");
        
        timelineSO.Play();
        StartEditorPlayback();
        
        // 手动更新一次
        timelineSO.UpdateTimeline();
        Debug.Log($"手动更新后 - 当前帧: {timelineSO.currentFrame}, 播放时间: {timelineSO.playTime:F2}s");
    }
    
    /// <summary>
    /// 强制更新
    /// </summary>
    private void ForceUpdate()
    {
        if (timelineSO == null)
        {
            Debug.LogError("TimelineSO为空！");
            return;
        }
        
        Debug.Log("强制更新Timeline...");
        timelineSO.UpdateTimeline();
        Debug.Log($"强制更新后 - 当前帧: {timelineSO.currentFrame}, 播放时间: {timelineSO.playTime:F2}s, 播放状态: {timelineSO.isPlaying}");
        
        // 标记需要重绘
        if (imguiContainer != null)
        {
            imguiContainer.MarkDirtyRepaint();
        }
    }
    
    /// <summary>
    /// 重写CreateContent以添加Timeline特定的按钮
    /// </summary>
    public override VisualElement CreateContent()
    {
        var content = base.CreateContent();
        
        // 隐藏额外的按钮，保持界面简洁
        // var timelineButton = CreateButton(() => {
        //     if (timelineSO != null)
        //     {
        //         timelineSO.PrintStatus();
        //     }
        // }, "打印状态");
        
        // var initButton = CreateButton(() => {
        //     if (timelineSO != null)
        //     {
        //         timelineSO.InitializeFrameData();
        //     }
        // }, "初始化帧数据");
        
        // content.Add(timelineButton);
        // content.Add(initButton);
        
        return content;
    }
    
    /// <summary>
    /// 更新方法，用于更新Timeline状态（仅在运行时调用）
    /// </summary>
    public void Update()
    {
        // 只在运行时调用，编辑器模式使用OnEditorUpdate
        if (Application.isPlaying && timelineSO != null && timelineSO.isPlaying)
        {
            timelineSO.UpdateTimeline();
            
            // 更新节点输出
            if (nodeTarget is TimelineNode timelineNode)
            {
                timelineNode.currentFrame = timelineSO.currentFrame;
                timelineNode.isPlaying = timelineSO.isPlaying;
            }
            
            // 标记需要重绘
            if (imguiContainer != null)
            {
                imguiContainer.MarkDirtyRepaint();
            }
        }
    }
}
