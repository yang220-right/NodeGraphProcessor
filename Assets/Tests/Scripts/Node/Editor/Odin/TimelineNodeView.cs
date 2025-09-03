using System;
using System.Collections.Generic;
using GraphProcessor;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;

/// <summary>
/// 时间轴节点的自定义视图
/// 提供时间轴的可视化编辑界面
/// </summary>
[NodeCustomEditor(typeof(TimelineNode))]
public class TimelineNodeView : BaseSONodeView
{
    private TimelineNode timelineNode;
    private TimelineSO timelineSO;
    
    // UI元素
    private IMGUIContainer timelineContainer;
    private IMGUIContainer controlContainer;
    private VisualElement playbackControls;
    
    // 时间轴绘制相关
    private Rect timelineRect;
    private float timelineWidth = 400f;
    private float timelineHeight = 200f;
    private float keyframeSize = 8f;
    private Color timelineColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    private Color keyframeColor = new Color(1f, 0.8f, 0.2f, 1f);
    private Color selectedKeyframeColor = new Color(1f, 0.4f, 0.2f, 1f);
    
    // 轨道相关
    private int selectedTrackIndex = 0;
    private float trackHeight = 30f;
    private float trackSpacing = 5f;
    
    // 交互状态
    private int selectedKeyframeIndex = -1;
    private bool isDragging = false;
    private Vector2 dragStartPos;
    private float dragStartTime;
    private float dragStartValue;
    
    protected override void SetWidth()
    {
        style.width = 500f;
    }
    
    protected override ScriptableObject CreateSO()
    {
        timelineSO = CreateInstance<TimelineSO>() as TimelineSO;
        timelineSO.Initialize();
        return timelineSO;
    }
    
    public override void Enable()
    {
        base.Enable();
        
        timelineNode = nodeTarget as TimelineNode;
        if (timelineSO == null)
        {
            timelineSO = targetSO as TimelineSO;
        }
        
        SetupTimelineUI();
    }
    
    public override void Disable()
    {
        base.Disable();
    }
    
    /// <summary>
    /// 设置时间轴UI
    /// </summary>
    private void SetupTimelineUI()
    {
        // 创建时间轴容器
        timelineContainer = new IMGUIContainer();
        timelineContainer.style.height = timelineHeight + 50f;
        timelineContainer.style.marginBottom = 10f;
        timelineContainer.onGUIHandler = OnTimelineGUI;
        
        // 创建控制容器
        controlContainer = new IMGUIContainer();
        controlContainer.style.height = 100f;
        controlContainer.onGUIHandler = OnControlGUI;
        
        // 创建播放控制按钮
        CreatePlaybackControls();
        
        // 添加到控件容器
        controlsContainer.Add(timelineContainer);
        controlsContainer.Add(controlContainer);
        controlsContainer.Add(playbackControls);
    }
    
    /// <summary>
    /// 创建播放控制按钮
    /// </summary>
    private void CreatePlaybackControls()
    {
        playbackControls = new VisualElement();
        playbackControls.style.flexDirection = FlexDirection.Row;
        playbackControls.style.justifyContent = Justify.SpaceAround;
        playbackControls.style.marginTop = 10f;
        
        // 播放按钮
        var playButton = CreateControlButton("播放", () => {
            if (timelineNode != null)
            {
                timelineNode.StartTimeline();
            }
        });
        
        // 暂停按钮
        var pauseButton = CreateControlButton("暂停", () => {
            if (timelineNode != null)
            {
                timelineNode.PauseTimeline();
            }
        });
        
        // 停止按钮
        var stopButton = CreateControlButton("停止", () => {
            if (timelineNode != null)
            {
                timelineNode.StopTimeline();
            }
        });
        
        // 重置按钮
        var resetButton = CreateControlButton("重置", () => {
            if (timelineNode != null)
            {
                timelineNode.SeekToTime(0f);
            }
        });
        
        playbackControls.Add(playButton);
        playbackControls.Add(pauseButton);
        playbackControls.Add(stopButton);
        playbackControls.Add(resetButton);
    }
    
    /// <summary>
    /// 创建控制按钮
    /// </summary>
    private Button CreateControlButton(string text, Action onClick)
    {
        var button = new Button(onClick)
        {
            text = text
        };
        
        button.style.backgroundColor = new Color(0.3f, 0.6f, 0.9f, 0.8f);
        button.style.color = Color.white;
        button.style.height = 25f;
        button.style.flexGrow = 1;
        button.style.marginLeft = 2f;
        button.style.marginRight = 2f;
        
        return button;
    }
    
    /// <summary>
    /// 时间轴GUI绘制
    /// </summary>
    private void OnTimelineGUI()
    {
        if (timelineNode == null || timelineSO == null) return;
        
        // 绘制轨道管理界面
        DrawTrackManagement();
        
        // 获取绘制区域
        var rect = GUILayoutUtility.GetRect(timelineWidth, timelineHeight);
        timelineRect = rect;
        
        // 绘制背景
        EditorGUI.DrawRect(rect, timelineColor);
        
        // 绘制网格
        DrawTimelineGrid(rect);
        
        // 绘制轨道
        DrawTracks(rect);
        
        // 绘制当前时间指示器
        DrawCurrentTimeIndicator(rect);
        
        // 处理鼠标交互
        HandleMouseInteraction(rect);
        
        // 绘制信息
        DrawTimelineInfo(rect);
    }
    
    /// <summary>
    /// 绘制时间轴网格
    /// </summary>
    private void DrawTimelineGrid(Rect rect)
    {
        // 绘制水平网格线
        for (int i = 0; i <= 10; i++)
        {
            float y = rect.y + (rect.height / 10f) * i;
            Color gridColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            EditorGUI.DrawRect(new Rect(rect.x, y, rect.width, 1f), gridColor);
        }
        
        // 绘制垂直网格线
        for (int i = 0; i <= 10; i++)
        {
            float x = rect.x + (rect.width / 10f) * i;
            Color gridColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            EditorGUI.DrawRect(new Rect(x, rect.y, 1f, rect.height), gridColor);
        }
    }
    
    /// <summary>
    /// 绘制轨道管理界面
    /// </summary>
    private void DrawTrackManagement()
    {
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.LabelField("轨道管理", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // 添加轨道按钮
        if (GUILayout.Button("添加轨道", GUILayout.Width(80)))
        {
            ShowAddTrackMenu();
        }
        
        // 删除轨道按钮
        if (GUILayout.Button("删除轨道", GUILayout.Width(80)))
        {
            if (timelineSO.tracks != null && selectedTrackIndex >= 0 && selectedTrackIndex < timelineSO.tracks.Count)
            {
                timelineSO.RemoveTrack(selectedTrackIndex);
                if (selectedTrackIndex >= timelineSO.tracks.Count)
                {
                    selectedTrackIndex = Mathf.Max(0, timelineSO.tracks.Count - 1);
                }
            }
        }
        
        // 复制轨道按钮
        if (GUILayout.Button("复制轨道", GUILayout.Width(80)))
        {
            if (timelineSO.tracks != null && selectedTrackIndex >= 0 && selectedTrackIndex < timelineSO.tracks.Count)
            {
                timelineSO.DuplicateTrack(selectedTrackIndex);
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 轨道选择
        if (timelineSO.tracks != null && timelineSO.tracks.Count > 0)
        {
            EditorGUILayout.Space(5f);
            EditorGUILayout.LabelField("选择轨道:", EditorStyles.label);
            
            string[] trackNames = timelineSO.tracks.Select(t => t.trackName).ToArray();
            int newSelectedTrack = EditorGUILayout.Popup(selectedTrackIndex, trackNames);
            
            if (newSelectedTrack != selectedTrackIndex)
            {
                selectedTrackIndex = newSelectedTrack;
            }
            
            // 显示当前轨道信息
            if (selectedTrackIndex >= 0 && selectedTrackIndex < timelineSO.tracks.Count)
            {
                var currentTrack = timelineSO.tracks[selectedTrackIndex];
                EditorGUILayout.LabelField($"类型: {currentTrack.trackType}");
                EditorGUILayout.LabelField($"关键帧: {currentTrack.GetKeyframeCount()}");
                EditorGUILayout.LabelField($"状态: {(currentTrack.isEnabled ? "启用" : "禁用")}");
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// 显示添加轨道菜单
    /// </summary>
    private void ShowAddTrackMenu()
    {
        var menu = new GenericMenu();
        
        foreach (TrackType trackType in System.Enum.GetValues(typeof(TrackType)))
        {
            string typeName = GetTrackTypeDisplayName(trackType);
            menu.AddItem(new GUIContent(typeName), false, () => {
                timelineSO.AddTrack($"新{typeName}", trackType);
            });
        }
        
        menu.ShowAsContext();
    }
    
    /// <summary>
    /// 获取轨道类型显示名称
    /// </summary>
    private string GetTrackTypeDisplayName(TrackType trackType)
    {
        switch (trackType)
        {
            case TrackType.Float: return "数值轨道";
            case TrackType.Vector3: return "向量轨道";
            case TrackType.Color: return "颜色轨道";
            case TrackType.Boolean: return "布尔轨道";
            case TrackType.Event: return "事件轨道";
            case TrackType.Audio: return "音频轨道";
            case TrackType.Animation: return "动画轨道";
            default: return trackType.ToString();
        }
    }
    
    /// <summary>
    /// 绘制轨道
    /// </summary>
    private void DrawTracks(Rect rect)
    {
        if (timelineSO.tracks == null || timelineSO.tracks.Count == 0) return;
        
        float trackY = rect.y;
        
        for (int trackIndex = 0; trackIndex < timelineSO.tracks.Count; trackIndex++)
        {
            var track = timelineSO.tracks[trackIndex];
            if (track == null) continue;
            
            Rect trackRect = new Rect(rect.x, trackY, rect.width, trackHeight);
            
            // 绘制轨道背景
            Color trackBgColor = track.isEnabled ? track.trackColor * 0.3f : Color.gray * 0.2f;
            EditorGUI.DrawRect(trackRect, trackBgColor);
            
            // 绘制轨道边框
            Color borderColor = (trackIndex == selectedTrackIndex) ? Color.yellow : Color.gray;
            EditorGUI.DrawRect(new Rect(trackRect.x, trackRect.y, trackRect.width, 1f), borderColor);
            EditorGUI.DrawRect(new Rect(trackRect.x, trackRect.yMax - 1f, trackRect.width, 1f), borderColor);
            
            // 绘制轨道名称
            GUIStyle trackNameStyle = new GUIStyle(EditorStyles.label);
            trackNameStyle.fontSize = 10;
            trackNameStyle.normal.textColor = track.isEnabled ? Color.white : Color.gray;
            EditorGUI.LabelField(new Rect(trackRect.x + 5f, trackRect.y + 2f, 100f, trackHeight), track.trackName, trackNameStyle);
            
            // 绘制轨道关键帧
            DrawTrackKeyframes(track, trackRect);
            
            trackY += trackHeight + trackSpacing;
        }
    }
    
    /// <summary>
    /// 绘制轨道关键帧
    /// </summary>
    private void DrawTrackKeyframes(TimelineTrack track, Rect trackRect)
    {
        if (track.keyframes == null || track.keyframes.Count == 0) return;
        
        // 绘制连接线
        for (int i = 0; i < track.keyframes.Count - 1; i++)
        {
            var from = track.keyframes[i];
            var to = track.keyframes[i + 1];
            
            Vector2 fromPos = TimeValueToPosition(trackRect, from.time, from.value);
            Vector2 toPos = TimeValueToPosition(trackRect, to.time, to.value);
            
            // 根据插值类型绘制不同的曲线
            Color lineColor = track.isEnabled ? track.trackColor : Color.gray;
            switch (from.interpolationType)
            {
                case InterpolationType.Linear:
                    DrawLine(fromPos, toPos, lineColor);
                    break;
                case InterpolationType.EaseInOut:
                    DrawEasedCurve(trackRect, from, to, lineColor);
                    break;
                case InterpolationType.Step:
                    DrawStepLine(fromPos, toPos, lineColor);
                    break;
                case InterpolationType.Bezier:
                    DrawBezierCurve(trackRect, from, to, lineColor);
                    break;
            }
        }
        
        // 绘制关键帧
        for (int i = 0; i < track.keyframes.Count; i++)
        {
            var keyframe = track.keyframes[i];
            Vector2 pos = TimeValueToPosition(trackRect, keyframe.time, keyframe.value);
            
            Color color = track.isEnabled ? track.trackColor : Color.gray;
            if (i == selectedKeyframeIndex && track == timelineSO.tracks[selectedTrackIndex])
            {
                color = selectedKeyframeColor;
            }
            
            // 绘制关键帧
            Rect keyframeRect = new Rect(pos.x - keyframeSize / 2f, pos.y - keyframeSize / 2f, keyframeSize, keyframeSize);
            EditorGUI.DrawRect(keyframeRect, color);
            
            // 绘制关键帧边框
            EditorGUI.DrawRect(keyframeRect, Color.black);
        }
    }
    
    /// <summary>
    /// 绘制关键帧
    /// </summary>
    private void DrawKeyframes(Rect rect)
    {
        if (timelineSO.keyframes == null) return;
        
        for (int i = 0; i < timelineSO.keyframes.Count; i++)
        {
            var keyframe = timelineSO.keyframes[i];
            Vector2 pos = TimeValueToPosition(rect, keyframe.time, keyframe.value);
            
            Color color = (i == selectedKeyframeIndex) ? selectedKeyframeColor : keyframeColor;
            
            // 绘制关键帧
            Rect keyframeRect = new Rect(pos.x - keyframeSize / 2f, pos.y - keyframeSize / 2f, keyframeSize, keyframeSize);
            EditorGUI.DrawRect(keyframeRect, color);
            
            // 绘制关键帧边框
            EditorGUI.DrawRect(keyframeRect, Color.black);
        }
    }
    
    /// <summary>
    /// 绘制当前时间指示器
    /// </summary>
    private void DrawCurrentTimeIndicator(Rect rect)
    {
        if (timelineNode == null) return;
        
        float normalizedTime = timelineNode.timelineTime / timelineNode.duration;
        float x = rect.x + rect.width * normalizedTime;
        
        Color indicatorColor = new Color(1f, 0.2f, 0.2f, 0.8f);
        EditorGUI.DrawRect(new Rect(x, rect.y, 2f, rect.height), indicatorColor);
    }
    
    /// <summary>
    /// 绘制时间轴信息
    /// </summary>
    private void DrawTimelineInfo(Rect rect)
    {
        if (timelineNode == null) return;
        
        GUIStyle infoStyle = new GUIStyle(EditorStyles.label);
        infoStyle.fontSize = 10;
        infoStyle.normal.textColor = Color.white;
        
        string info = $"时间: {timelineNode.timelineTime:F2}s / {timelineNode.duration:F2}s\n" +
                     $"值: {timelineNode.currentValue:F2}\n" +
                     $"状态: {timelineNode.playState}";
        
        EditorGUI.LabelField(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, 60f), info, infoStyle);
    }
    
    /// <summary>
    /// 处理鼠标交互
    /// </summary>
    private void HandleMouseInteraction(Rect rect)
    {
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            // 检查是否点击了轨道
            int clickedTrack = GetTrackAtPosition(rect, e.mousePosition);
            if (clickedTrack >= 0)
            {
                selectedTrackIndex = clickedTrack;
                selectedKeyframeIndex = -1;
                e.Use();
                return;
            }
            
            // 检查是否点击了关键帧
            var keyframeInfo = GetKeyframeAtPosition(rect, e.mousePosition);
            if (keyframeInfo.trackIndex >= 0 && keyframeInfo.keyframeIndex >= 0)
            {
                selectedTrackIndex = keyframeInfo.trackIndex;
                selectedKeyframeIndex = keyframeInfo.keyframeIndex;
                isDragging = true;
                dragStartPos = e.mousePosition;
                
                var track = timelineSO.tracks[selectedTrackIndex];
                if (track != null && track.keyframes != null && selectedKeyframeIndex < track.keyframes.Count)
                {
                    var keyframe = track.keyframes[selectedKeyframeIndex];
                    dragStartTime = keyframe.time;
                    dragStartValue = keyframe.value;
                }
                
                e.Use();
            }
            else
            {
                // 点击空白区域，添加新关键帧到当前选中的轨道
                if (selectedTrackIndex >= 0 && selectedTrackIndex < timelineSO.tracks.Count)
                {
                    Vector2 timeValue = PositionToTimeValue(rect, e.mousePosition);
                    AddKeyframeToTrack(selectedTrackIndex, timeValue.x, timeValue.y);
                }
                e.Use();
            }
        }
        else if (e.type == EventType.MouseDrag && isDragging && selectedKeyframeIndex >= 0 && selectedTrackIndex >= 0)
        {
            // 拖拽关键帧
            Vector2 timeValue = PositionToTimeValue(rect, e.mousePosition);
            UpdateTrackKeyframe(selectedTrackIndex, selectedKeyframeIndex, timeValue.x, timeValue.y);
            e.Use();
        }
        else if (e.type == EventType.MouseUp)
        {
            isDragging = false;
        }
        else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete && selectedKeyframeIndex >= 0 && selectedTrackIndex >= 0)
        {
            // 删除选中的关键帧
            RemoveTrackKeyframe(selectedTrackIndex, selectedKeyframeIndex);
            selectedKeyframeIndex = -1;
            e.Use();
        }
    }
    
    /// <summary>
    /// 控制GUI绘制
    /// </summary>
    private void OnControlGUI()
    {
        if (timelineNode == null || timelineSO == null) return;
        
        EditorGUILayout.BeginVertical();
        
        // 播放控制
        EditorGUILayout.LabelField("播放控制", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("播放"))
        {
            timelineNode.StartTimeline();
        }
        
        if (GUILayout.Button("暂停"))
        {
            timelineNode.PauseTimeline();
        }
        
        if (GUILayout.Button("停止"))
        {
            timelineNode.StopTimeline();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 时间控制
        EditorGUILayout.Space(5f);
        EditorGUILayout.LabelField("时间控制", EditorStyles.boldLabel);
        
        float newTime = EditorGUILayout.Slider("当前时间", timelineNode.timelineTime, 0f, timelineNode.duration);
        if (Mathf.Abs(newTime - timelineNode.timelineTime) > 0.01f)
        {
            timelineNode.SeekToTime(newTime);
        }
        
        // 关键帧控制
        EditorGUILayout.Space(5f);
        EditorGUILayout.LabelField("关键帧控制", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加关键帧"))
        {
            AddKeyframeAtCurrentTime();
        }
        
        if (GUILayout.Button("清空关键帧"))
        {
            ClearAllKeyframes();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// 将时间和值转换为屏幕位置
    /// </summary>
    private Vector2 TimeValueToPosition(Rect rect, float time, float value)
    {
        float normalizedTime = time / timelineNode.duration;
        float normalizedValue = Mathf.InverseLerp(-1f, 1f, value); // 假设值范围是-1到1
        
        float x = rect.x + rect.width * normalizedTime;
        float y = rect.y + rect.height * (1f - normalizedValue); // 翻转Y轴
        
        return new Vector2(x, y);
    }
    
    /// <summary>
    /// 将屏幕位置转换为时间和值
    /// </summary>
    private Vector2 PositionToTimeValue(Rect rect, Vector2 position)
    {
        float normalizedTime = (position.x - rect.x) / rect.width;
        float normalizedValue = 1f - (position.y - rect.y) / rect.height; // 翻转Y轴
        
        float time = normalizedTime * timelineNode.duration;
        float value = Mathf.Lerp(-1f, 1f, normalizedValue);
        
        return new Vector2(time, value);
    }
    
    /// <summary>
    /// 获取指定位置的轨道索引
    /// </summary>
    private int GetTrackAtPosition(Rect rect, Vector2 position)
    {
        if (timelineSO.tracks == null) return -1;
        
        float trackY = rect.y;
        
        for (int trackIndex = 0; trackIndex < timelineSO.tracks.Count; trackIndex++)
        {
            Rect trackRect = new Rect(rect.x, trackY, rect.width, trackHeight);
            
            if (trackRect.Contains(position))
            {
                return trackIndex;
            }
            
            trackY += trackHeight + trackSpacing;
        }
        
        return -1;
    }
    
    /// <summary>
    /// 关键帧信息结构
    /// </summary>
    private struct KeyframeInfo
    {
        public int trackIndex;
        public int keyframeIndex;
        
        public KeyframeInfo(int trackIndex, int keyframeIndex)
        {
            this.trackIndex = trackIndex;
            this.keyframeIndex = keyframeIndex;
        }
    }
    
    /// <summary>
    /// 获取指定位置的关键帧信息
    /// </summary>
    private KeyframeInfo GetKeyframeAtPosition(Rect rect, Vector2 position)
    {
        if (timelineSO.tracks == null) return new KeyframeInfo(-1, -1);
        
        float trackY = rect.y;
        
        for (int trackIndex = 0; trackIndex < timelineSO.tracks.Count; trackIndex++)
        {
            var track = timelineSO.tracks[trackIndex];
            if (track == null || track.keyframes == null) continue;
            
            Rect trackRect = new Rect(rect.x, trackY, rect.width, trackHeight);
            
            // 检查是否在轨道范围内
            if (trackRect.Contains(position))
            {
                // 检查是否点击了关键帧
                for (int keyframeIndex = 0; keyframeIndex < track.keyframes.Count; keyframeIndex++)
                {
                    var keyframe = track.keyframes[keyframeIndex];
                    Vector2 keyframePos = TimeValueToPosition(trackRect, keyframe.time, keyframe.value);
                    
                    if (Vector2.Distance(position, keyframePos) <= keyframeSize)
                    {
                        return new KeyframeInfo(trackIndex, keyframeIndex);
                    }
                }
            }
            
            trackY += trackHeight + trackSpacing;
        }
        
        return new KeyframeInfo(-1, -1);
    }
    
    /// <summary>
    /// 在指定位置添加关键帧
    /// </summary>
    private void AddKeyframeAtPosition(float time, float value)
    {
        if (timelineSO == null) return;
        
        var keyframe = new TimelineKeyframe
        {
            time = Mathf.Clamp(time, 0f, timelineNode.duration),
            value = Mathf.Clamp(value, -1f, 1f)
        };
        
        timelineSO.keyframes.Add(keyframe);
        timelineSO.keyframes = timelineSO.keyframes.OrderBy(k => k.time).ToList();
        
        // 同步到节点
        timelineNode.keyframes = new List<TimelineKeyframe>(timelineSO.keyframes);
        
        EditorUtility.SetDirty(timelineSO);
    }
    
    /// <summary>
    /// 向指定轨道添加关键帧
    /// </summary>
    private void AddKeyframeToTrack(int trackIndex, float time, float value)
    {
        if (timelineSO == null || trackIndex < 0 || trackIndex >= timelineSO.tracks.Count) return;
        
        var track = timelineSO.tracks[trackIndex];
        if (track == null) return;
        
        track.AddKeyframe(time, value);
        EditorUtility.SetDirty(timelineSO);
    }
    
    /// <summary>
    /// 更新轨道关键帧
    /// </summary>
    private void UpdateTrackKeyframe(int trackIndex, int keyframeIndex, float time, float value)
    {
        if (timelineSO == null || trackIndex < 0 || trackIndex >= timelineSO.tracks.Count) return;
        
        var track = timelineSO.tracks[trackIndex];
        if (track == null || keyframeIndex < 0 || keyframeIndex >= track.keyframes.Count) return;
        
        track.UpdateKeyframe(keyframeIndex, time, value);
        EditorUtility.SetDirty(timelineSO);
    }
    
    /// <summary>
    /// 移除轨道关键帧
    /// </summary>
    private void RemoveTrackKeyframe(int trackIndex, int keyframeIndex)
    {
        if (timelineSO == null || trackIndex < 0 || trackIndex >= timelineSO.tracks.Count) return;
        
        var track = timelineSO.tracks[trackIndex];
        if (track == null || keyframeIndex < 0 || keyframeIndex >= track.keyframes.Count) return;
        
        track.RemoveKeyframe(keyframeIndex);
        EditorUtility.SetDirty(timelineSO);
    }
    
    /// <summary>
    /// 更新关键帧
    /// </summary>
    private void UpdateKeyframe(int index, float time, float value)
    {
        if (timelineSO == null || index < 0 || index >= timelineSO.keyframes.Count) return;
        
        timelineSO.keyframes[index].time = Mathf.Clamp(time, 0f, timelineNode.duration);
        timelineSO.keyframes[index].value = Mathf.Clamp(value, -1f, 1f);
        
        // 重新排序
        timelineSO.keyframes = timelineSO.keyframes.OrderBy(k => k.time).ToList();
        
        // 同步到节点
        timelineNode.keyframes = new List<TimelineKeyframe>(timelineSO.keyframes);
        
        EditorUtility.SetDirty(timelineSO);
    }
    
    /// <summary>
    /// 移除关键帧
    /// </summary>
    private void RemoveKeyframe(int index)
    {
        if (timelineSO == null || index < 0 || index >= timelineSO.keyframes.Count) return;
        
        timelineSO.keyframes.RemoveAt(index);
        
        // 同步到节点
        timelineNode.keyframes = new List<TimelineKeyframe>(timelineSO.keyframes);
        
        EditorUtility.SetDirty(timelineSO);
    }
    
    /// <summary>
    /// 在当前时间添加关键帧
    /// </summary>
    private void AddKeyframeAtCurrentTime()
    {
        if (timelineNode == null) return;
        
        AddKeyframeAtPosition(timelineNode.timelineTime, timelineNode.currentValue);
    }
    
    /// <summary>
    /// 清空所有关键帧
    /// </summary>
    private void ClearAllKeyframes()
    {
        if (timelineSO == null) return;
        
        timelineSO.keyframes.Clear();
        timelineNode.keyframes.Clear();
        
        EditorUtility.SetDirty(timelineSO);
    }
    
    /// <summary>
    /// 绘制直线
    /// </summary>
    private void DrawLine(Vector2 from, Vector2 to, Color color)
    {
        Handles.color = color;
        Handles.DrawLine(from, to);
    }
    
    /// <summary>
    /// 绘制步进线
    /// </summary>
    private void DrawStepLine(Vector2 from, Vector2 to, Color color)
    {
        Handles.color = color;
        Vector2 midPoint = new Vector2(to.x, from.y);
        Handles.DrawLine(from, midPoint);
        Handles.DrawLine(midPoint, to);
    }
    
    /// <summary>
    /// 绘制缓动曲线
    /// </summary>
    private void DrawEasedCurve(Rect rect, TimelineKeyframe from, TimelineKeyframe to, Color color)
    {
        Handles.color = color;
        
        int segments = 20;
        Vector2 prevPoint = TimeValueToPosition(rect, from.time, from.value);
        
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float easedT = from.easingCurve.Evaluate(t);
            float time = Mathf.Lerp(from.time, to.time, t);
            float value = Mathf.Lerp(from.value, to.value, easedT);
            
            Vector2 currentPoint = TimeValueToPosition(rect, time, value);
            Handles.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
    
    /// <summary>
    /// 绘制贝塞尔曲线
    /// </summary>
    private void DrawBezierCurve(Rect rect, TimelineKeyframe from, TimelineKeyframe to, Color color)
    {
        Handles.color = color;
        
        Vector2 fromPos = TimeValueToPosition(rect, from.time, from.value);
        Vector2 toPos = TimeValueToPosition(rect, to.time, to.value);
        
        // 简单的贝塞尔曲线控制点
        Vector2 controlPoint1 = fromPos + Vector2.right * (rect.width * 0.1f);
        Vector2 controlPoint2 = toPos + Vector2.left * (rect.width * 0.1f);
        
        Handles.DrawBezier(fromPos, toPos, controlPoint1, controlPoint2, color, null, 2f);
    }
}
