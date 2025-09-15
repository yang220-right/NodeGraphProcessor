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
    private bool isDragging = false;
    
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
            timelineSO = ts;
        
        // 初始化帧数据
        if (timelineSO != null && !isInitialized)
        {
            InitializeTimeline();
            isInitialized = true;
        }
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
            timelineSO.InitializeFrameData();
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
                timelineNode.trackCount = timelineSO.tracks != null ? timelineSO.tracks.Length : 0;
                
                // 更新轨道值
                if (timelineSO.tracks is { Length: > 0 })
                {
                    timelineNode.trackValues = new float[timelineSO.tracks.Length];
                    for (int i = 0; i < timelineSO.tracks.Length; i++)
                        timelineNode.trackValues[i] = timelineSO.GetTrackValueAtFrame(i, timelineSO.currentFrame);
                }
                else timelineNode.trackValues = new float[0];
            }
            
            // 标记需要重绘
            if (imguiContainer != null)
                imguiContainer.MarkDirtyRepaint();
            
            // 标记场景为已修改
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(timelineSO);
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
        }
    }
    
    /// <summary>
    /// 重写SetupInspector以添加Timeline特定的UI
    /// </summary>
    protected override void SetupInspector()
    {
        base.SetupInspector();
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
        // 绘制轨道时间轴
        DrawTrackTimeline();
        // 绘制轨道管理界面
        DrawTrackManagement();
        // 绘制时间轴额外信息
        DrawTimeLineCurrentInfo();
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
    }

    private void DrawTimeLineCurrentInfo()
    {
        // 显示当前帧信息
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"当前帧: {timelineSO.currentFrame} / {timelineSO.totalFrames}");
        EditorGUILayout.LabelField($"播放时间: {timelineSO.playTime:F2}s");
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
        // 处理鼠标点击和拖动
        HandleTimelineClick(timelineRect, frameWidth);
        // 设置鼠标光标
        if (timelineRect.Contains(Event.current.mousePosition))
        {
            EditorGUIUtility.AddCursorRect(timelineRect, MouseCursor.MoveArrow);
        }
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
        
        // 根据拖动状态选择颜色
        Color indicatorColor = isDragging ? Color.yellow : Color.red;
        Color shadowColor = isDragging ? new Color(0.8f, 0.8f, 0.1f, 0.8f) : new Color(0.5f, 0.1f, 0.1f, 0.8f);
        
        // 绘制当前帧指示线（带阴影效果）
        EditorGUI.DrawRect(new Rect(x - 1, timelineRect.y, 3, timelineRect.height), shadowColor);
        EditorGUI.DrawRect(new Rect(x, timelineRect.y, 1, timelineRect.height), indicatorColor);
        
        // 绘制当前帧指示器（三角形，带阴影）
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
        
        Handles.color = shadowColor;
        Handles.DrawAAConvexPolygon(shadowTriangle);
        
        Handles.color = indicatorColor;
        Handles.DrawAAConvexPolygon(triangle);
        Handles.color = Color.white;
        
        // 绘制当前帧数字
        string frameText = currentFrame.ToString();
        Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(frameText));
        Rect textRect = new Rect(x - textSize.x / 2, timelineRect.y + 15, textSize.x, 15);
        
        // 绘制文字背景（拖动时高亮）
        Color textBgColor = isDragging ? new Color(1f, 1f, 0f, 0.8f) : new Color(0, 0, 0, 0.7f);
        EditorGUI.DrawRect(textRect, textBgColor);
        
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
    /// 处理时间轴点击和拖动
    /// </summary>
    private void HandleTimelineClick(Rect timelineRect, float frameWidth)
    {
        Event e = Event.current;
        
        if (timelineRect.Contains(e.mousePosition))
        {
            // 计算鼠标位置对应的帧
            float clickX = e.mousePosition.x - timelineRect.x;
            int targetFrame = Mathf.RoundToInt(clickX / frameWidth);
            targetFrame = Mathf.Clamp(targetFrame, 0, timelineSO.totalFrames - 1);
            
            if (e.type == EventType.MouseDown)
            {
                // 开始拖动
                isDragging = true;
                SetCurrentFrame(targetFrame);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && isDragging)
            {
                // 拖动中
                SetCurrentFrame(targetFrame);
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                // 结束拖动
                isDragging = false;
                SetCurrentFrame(targetFrame);
                e.Use();
            }
        }
        else if (e.type == EventType.MouseUp)
        {
            // 在时间轴外释放鼠标，结束拖动
            isDragging = false;
        }
    }
    
    /// <summary>
    /// 设置当前帧
    /// </summary>
    private void SetCurrentFrame(int frame)
    {
        timelineSO.currentFrame = frame;
        timelineSO.GoToFrame();
        
        // 更新节点输出
        if (nodeTarget is TimelineNode timelineNode)
        {
            timelineNode.currentFrame = timelineSO.currentFrame;
            timelineNode.trackCount = timelineSO.tracks != null ? timelineSO.tracks.Length : 0;
            
            // 更新轨道值
            if (timelineSO.tracks != null && timelineSO.tracks.Length > 0)
            {
                timelineNode.trackValues = new float[timelineSO.tracks.Length];
                for (int i = 0; i < timelineSO.tracks.Length; i++)
                {
                    timelineNode.trackValues[i] = timelineSO.GetTrackValueAtFrame(i, timelineSO.currentFrame);
                }
            }
            else
            {
                timelineNode.trackValues = new float[0];
            }
        }
        
        // 标记需要重绘
        if (imguiContainer != null)
        {
            imguiContainer.MarkDirtyRepaint();
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
    /// 绘制轨道管理界面
    /// </summary>
    private void DrawTrackManagement()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("轨道管理", EditorStyles.boldLabel);
        
        // 轨道操作按钮
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("➕ 添加动画轨道", GUILayout.Height(25)))
        {
            timelineSO.AddTrack("动画轨道", TimelineSO.TrackType.Animation);
            EditorUtility.SetDirty(timelineSO);
        }
        
        if (GUILayout.Button("🎵 添加音频轨道", GUILayout.Height(25)))
        {
            timelineSO.AddTrack("音频轨道", TimelineSO.TrackType.Audio);
            EditorUtility.SetDirty(timelineSO);
        }
        
        if (GUILayout.Button("⚡ 添加事件轨道", GUILayout.Height(25)))
        {
            timelineSO.AddTrack("事件轨道", TimelineSO.TrackType.Event);
            EditorUtility.SetDirty(timelineSO);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("📜 添加脚本轨道", GUILayout.Height(25)))
        {
            timelineSO.AddTrack("脚本轨道", TimelineSO.TrackType.Script);
            EditorUtility.SetDirty(timelineSO);
        }
        
        if (GUILayout.Button("🔧 添加自定义轨道", GUILayout.Height(25)))
        {
            timelineSO.AddTrack("自定义轨道", TimelineSO.TrackType.Custom);
            EditorUtility.SetDirty(timelineSO);
        }
        
        if (GUILayout.Button("🗑️ 清除所有轨道", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("确认删除", "确定要删除所有轨道吗？", "确定", "取消"))
            {
                timelineSO.tracks = new TimelineSO.TrackData[0];
                EditorUtility.SetDirty(timelineSO);
            }
        }
        
        if (GUILayout.Button("📝 添加示例轨道", GUILayout.Height(25)))
        {
            AddExampleTracks();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 显示轨道列表
        if (timelineSO.tracks != null && timelineSO.tracks.Length > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"轨道列表 ({timelineSO.tracks.Length} 个轨道)", EditorStyles.boldLabel);
            
            for (int i = 0; i < timelineSO.tracks.Length; i++)
            {
                DrawTrackItem(i);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("暂无轨道，请添加轨道", MessageType.Info);
        }
    }
    
    /// <summary>
    /// 绘制单个轨道项
    /// </summary>
    private void DrawTrackItem(int trackIndex)
    {
        var track = timelineSO.tracks[trackIndex];
        if (track == null) return;
        
        EditorGUILayout.BeginVertical("box");
        
        // 轨道头部信息
        EditorGUILayout.BeginHorizontal();
        
        // 轨道启用/禁用开关
        track.isEnabled = EditorGUILayout.Toggle(track.isEnabled, GUILayout.Width(20));
        
        // 轨道锁定开关
        track.isLocked = EditorGUILayout.Toggle(track.isLocked, GUILayout.Width(20));
        
        // 轨道颜色
        track.trackColor = EditorGUILayout.ColorField(track.trackColor, GUILayout.Width(30));
        
        // 轨道名称
        track.trackName = EditorGUILayout.TextField(track.trackName);
        
        // 轨道类型
        track.trackType = (TimelineSO.TrackType)EditorGUILayout.EnumPopup(track.trackType, GUILayout.Width(80));
        
        // 删除按钮
        if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(20)))
        {
            if (EditorUtility.DisplayDialog("确认删除", $"确定要删除轨道 '{track.trackName}' 吗？", "确定", "取消"))
            {
                timelineSO.RemoveTrack(trackIndex);
                EditorUtility.SetDirty(timelineSO);
                return;
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 轨道详细信息
        EditorGUILayout.BeginHorizontal();
        
        // 轨道高度
        // EditorGUILayout.LabelField("高度:", GUILayout.Width(40));
        // track.trackHeight = EditorGUILayout.Slider(track.trackHeight, 20f, 100f);
        
        // 关键帧数量
        int keyFrameCount = track.keyFrames != null ? track.keyFrames.Length : 0;
        EditorGUILayout.LabelField($"关键帧: {keyFrameCount}", GUILayout.Width(80));
        
        // 添加关键帧按钮
        if (GUILayout.Button("添加关键帧", GUILayout.Width(80), GUILayout.Height(20)))
        {
            timelineSO.AddKeyFrameToTrack(trackIndex, timelineSO.currentFrame, 0f);
            EditorUtility.SetDirty(timelineSO);
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 显示关键帧信息
        if (keyFrameCount > 0)
        {
            EditorGUILayout.BeginVertical("helpBox");
            EditorGUILayout.LabelField("关键帧信息:", EditorStyles.miniBoldLabel);
            
            for (int i = 0; i < keyFrameCount; i++)
            {
                var keyFrame = track.keyFrames[i];
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField($"帧 {keyFrame.frame}:", GUILayout.Width(50));
                keyFrame.value = EditorGUILayout.FloatField(keyFrame.value, GUILayout.Width(60));
                
                if (GUILayout.Button("删除", GUILayout.Width(40), GUILayout.Height(16)))
                {
                    timelineSO.RemoveKeyFrameFromTrack(trackIndex, i);
                    EditorUtility.SetDirty(timelineSO);
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// 绘制轨道时间轴
    /// </summary>
    private void DrawTrackTimeline()
    {
        if (timelineSO.tracks == null || timelineSO.tracks.Length == 0)
            return;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("轨道时间轴", EditorStyles.boldLabel);
        
        // 计算时间轴区域
        float trackAreaHeight = 0f;
        foreach (var track in timelineSO.tracks)
        {
            if (track.isEnabled)
            {
                trackAreaHeight += track.trackHeight + 2f; // 2f for spacing
            }
        }
        
        Rect trackAreaRect = GUILayoutUtility.GetRect(0, trackAreaHeight, GUILayout.ExpandWidth(true));
        
        // 绘制轨道区域背景
        DrawTrackAreaBackground(trackAreaRect);
        
        // 绘制每个轨道
        float currentY = trackAreaRect.y;
        for (int i = 0; i < timelineSO.tracks.Length; i++)
        {
            var track = timelineSO.tracks[i];
            if (!track.isEnabled) continue;
            
            Rect trackRect = new Rect(trackAreaRect.x, currentY, trackAreaRect.width, track.trackHeight);
            DrawSingleTrack(trackRect, track, i);
            
            currentY += track.trackHeight + 2f;
        }
        
        // 绘制当前帧指示线
        DrawCurrentFrameIndicatorForTracks(trackAreaRect);
    }
    
    /// <summary>
    /// 绘制轨道区域背景
    /// </summary>
    private void DrawTrackAreaBackground(Rect rect)
    {
        // 绘制基础背景
        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1f));
        
        // 绘制网格线
        float frameWidth = rect.width / timelineSO.totalFrames;
        for (int i = 0; i <= timelineSO.totalFrames; i += 10)
        {
            float x = rect.x + i * frameWidth;
            EditorGUI.DrawRect(new Rect(x, rect.y, 1, rect.height), new Color(0.3f, 0.3f, 0.3f, 0.5f));
        }
        
        // 绘制边框
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), Color.gray);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), Color.gray);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), Color.gray);
        EditorGUI.DrawRect(new Rect(rect.x + rect.width - 1, rect.y, 1, rect.height), Color.gray);
    }
    
    /// <summary>
    /// 绘制单个轨道
    /// </summary>
    private void DrawSingleTrack(Rect trackRect, TimelineSO.TrackData track, int trackIndex)
    {
        // 绘制轨道背景
        Color trackBgColor = track.isLocked ? 
            new Color(track.trackColor.r * 0.3f, track.trackColor.g * 0.3f, track.trackColor.b * 0.3f, 0.5f) :
            new Color(track.trackColor.r * 0.2f, track.trackColor.g * 0.2f, track.trackColor.b * 0.2f, 0.3f);
        
        EditorGUI.DrawRect(trackRect, trackBgColor);
        
        // 绘制轨道边框
        Color borderColor = track.isLocked ? Color.red : track.trackColor;
        EditorGUI.DrawRect(new Rect(trackRect.x, trackRect.y, trackRect.width, 1), borderColor);
        EditorGUI.DrawRect(new Rect(trackRect.x, trackRect.y + trackRect.height - 1, trackRect.width, 1), borderColor);
        
        // 绘制轨道名称
        GUI.Label(new Rect(trackRect.x + 5, trackRect.y + 2, 100, 20), track.trackName, EditorStyles.whiteLabel);
        
        // 绘制关键帧
        if (track.keyFrames != null)
        {
            float frameWidth = trackRect.width / timelineSO.totalFrames;
            
            foreach (var keyFrame in track.keyFrames)
            {
                float x = trackRect.x + keyFrame.frame * frameWidth;
                Rect keyFrameRect = new Rect(x - 3, trackRect.y + trackRect.height / 2 - 3, 6, 6);
                
                // 绘制关键帧
                EditorGUI.DrawRect(keyFrameRect, track.trackColor);
                
                // 绘制关键帧边框
                EditorGUI.DrawRect(new Rect(keyFrameRect.x, keyFrameRect.y, keyFrameRect.width, 1), Color.white);
                EditorGUI.DrawRect(new Rect(keyFrameRect.x, keyFrameRect.y + keyFrameRect.height - 1, keyFrameRect.width, 1), Color.white);
                EditorGUI.DrawRect(new Rect(keyFrameRect.x, keyFrameRect.y, 1, keyFrameRect.height), Color.white);
                EditorGUI.DrawRect(new Rect(keyFrameRect.x + keyFrameRect.width - 1, keyFrameRect.y, 1, keyFrameRect.height), Color.white);
            }
        }
        
        
    }
    
    /// <summary>
    /// 为轨道绘制当前帧指示线
    /// </summary>
    private void DrawCurrentFrameIndicatorForTracks(Rect trackAreaRect)
    {
        float frameWidth = trackAreaRect.width / timelineSO.totalFrames;
        float x = trackAreaRect.x + timelineSO.currentFrame * frameWidth;
        
        // 绘制当前帧指示线
        EditorGUI.DrawRect(new Rect(x - 1, trackAreaRect.y, 3, trackAreaRect.height), Color.red);
        EditorGUI.DrawRect(new Rect(x, trackAreaRect.y, 1, trackAreaRect.height), Color.white);
    }
    
    /// <summary>
    /// 添加示例轨道
    /// </summary>
    private void AddExampleTracks()
    {
        // 清除现有轨道
        timelineSO.tracks = new TimelineSO.TrackData[0];
        
        // 添加动画轨道
        timelineSO.AddTrack("位置X", TimelineSO.TrackType.Animation);
        timelineSO.AddKeyFrameToTrack(0, 0, 0f);
        timelineSO.AddKeyFrameToTrack(0, 30, 10f);
        timelineSO.AddKeyFrameToTrack(0, 60, 5f);
        
        // 添加动画轨道
        timelineSO.AddTrack("位置Y", TimelineSO.TrackType.Animation);
        timelineSO.AddKeyFrameToTrack(1, 0, 0f);
        timelineSO.AddKeyFrameToTrack(1, 20, 5f);
        timelineSO.AddKeyFrameToTrack(1, 40, 0f);
        timelineSO.AddKeyFrameToTrack(1, 60, -5f);
        
        // 添加音频轨道
        timelineSO.AddTrack("音量", TimelineSO.TrackType.Audio);
        timelineSO.AddKeyFrameToTrack(2, 0, 0f);
        timelineSO.AddKeyFrameToTrack(2, 10, 1f);
        timelineSO.AddKeyFrameToTrack(2, 50, 0.5f);
        timelineSO.AddKeyFrameToTrack(2, 60, 0f);
        
        // 添加事件轨道
        timelineSO.AddTrack("事件触发器", TimelineSO.TrackType.Event);
        timelineSO.AddKeyFrameToTrack(3, 15, 1f);
        timelineSO.AddKeyFrameToTrack(3, 35, 1f);
        timelineSO.AddKeyFrameToTrack(3, 55, 1f);
        
        // 添加脚本轨道
        timelineSO.AddTrack("脚本执行", TimelineSO.TrackType.Script);
        timelineSO.AddKeyFrameToTrack(4, 5, 1f);
        timelineSO.AddKeyFrameToTrack(4, 25, 1f);
        timelineSO.AddKeyFrameToTrack(4, 45, 1f);
        
        EditorUtility.SetDirty(timelineSO);
    }
}
