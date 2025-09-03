using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 动画帧管理器的自定义编辑器
/// 提供可视化的动画编辑界面
/// </summary>
[CustomEditor(typeof(AnimationFrameManagerSO))]
public class AnimationFrameManagerEditor : OdinEditor
{
    private AnimationFrameManagerSO manager;
    private int selectedSequenceIndex = 0;
    private int selectedKeyframeIndex = -1;
    private AnimationPropertyType selectedPropertyType = AnimationPropertyType.Position;
    
    // 播放控制
    private bool isPlaying = false;
    private float currentTime = 0f;
    private float playbackSpeed = 1f;
    
    // UI设置
    private float timelineHeight = 200f;
    private float keyframeSize = 8f;
    private Color timelineColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    private Color keyframeColor = new Color(1f, 0.8f, 0.2f, 1f);
    private Color selectedKeyframeColor = new Color(1f, 0.4f, 0.2f, 1f);
    
    // 交互状态
    private bool isDragging = false;
    private Vector2 dragStartPos;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        manager = target as AnimationFrameManagerSO;
        
        if (manager != null)
        {
            manager.Initialize();
        }
        
        EditorApplication.update += OnEditorUpdate;
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        EditorApplication.update -= OnEditorUpdate;
    }
    
    private void OnEditorUpdate()
    {
        if (isPlaying && manager != null)
        {
            currentTime += Time.deltaTime * playbackSpeed;
            
            // 检查是否超出当前序列的时长
            var currentSequence = manager.GetSequence(selectedSequenceIndex);
            if (currentSequence != null && currentTime >= currentSequence.GetDuration())
            {
                switch (currentSequence.playMode)
                {
                    case AnimationPlayMode.Once:
                        isPlaying = false;
                        currentTime = currentSequence.GetDuration();
                        break;
                    case AnimationPlayMode.Loop:
                        currentTime = 0f;
                        break;
                    case AnimationPlayMode.PingPong:
                        playbackSpeed = -playbackSpeed;
                        break;
                    case AnimationPlayMode.Reverse:
                        currentTime = currentSequence.GetDuration();
                        isPlaying = false;
                        break;
                }
            }
            
            Repaint();
        }
    }
    
    public override void OnInspectorGUI()
    {
        if (manager == null)
        {
            EditorGUILayout.HelpBox("动画帧管理器未初始化", MessageType.Warning);
            return;
        }
        
        // 绘制默认Inspector
        base.OnInspectorGUI();
        
        EditorGUILayout.Space(20f);
        
        // 绘制动画编辑器
        DrawAnimationEditor();
    }
    
    /// <summary>
    /// 绘制动画编辑器
    /// </summary>
    private void DrawAnimationEditor()
    {
        EditorGUILayout.LabelField("动画编辑器", EditorStyles.boldLabel);
        
        // 序列选择
        DrawSequenceSelector();
        
        EditorGUILayout.Space(10f);
        
        // 播放控制
        DrawPlaybackControls();
        
        EditorGUILayout.Space(10f);
        
        // 属性选择
        DrawPropertySelector();
        
        EditorGUILayout.Space(10f);
        
        // 时间轴
        DrawTimeline();
        
        EditorGUILayout.Space(10f);
        
        // 关键帧编辑
        DrawKeyframeEditor();
    }
    
    /// <summary>
    /// 绘制序列选择器
    /// </summary>
    private void DrawSequenceSelector()
    {
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("当前序列:", GUILayout.Width(80));
        
        if (manager.sequences != null && manager.sequences.Count > 0)
        {
            string[] sequenceNames = manager.sequences.Select(s => s.sequenceName).ToArray();
            int newSelectedSequence = EditorGUILayout.Popup(selectedSequenceIndex, sequenceNames);
            
            if (newSelectedSequence != selectedSequenceIndex)
            {
                selectedSequenceIndex = newSelectedSequence;
                selectedKeyframeIndex = -1;
            }
        }
        else
        {
            EditorGUILayout.LabelField("无序列", EditorStyles.helpBox);
        }
        
        // 添加序列按钮
        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            manager.AddSequence("新序列");
        }
        
        // 删除序列按钮
        if (GUILayout.Button("-", GUILayout.Width(30)) && manager.sequences.Count > 0)
        {
            manager.RemoveSequence(selectedSequenceIndex);
            if (selectedSequenceIndex >= manager.sequences.Count)
            {
                selectedSequenceIndex = Mathf.Max(0, manager.sequences.Count - 1);
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// 绘制播放控制
    /// </summary>
    private void DrawPlaybackControls()
    {
        EditorGUILayout.BeginHorizontal();
        
        // 播放/暂停按钮
        if (GUILayout.Button(isPlaying ? "暂停" : "播放", GUILayout.Width(60)))
        {
            isPlaying = !isPlaying;
        }
        
        // 停止按钮
        if (GUILayout.Button("停止", GUILayout.Width(60)))
        {
            isPlaying = false;
            currentTime = 0f;
        }
        
        // 重置按钮
        if (GUILayout.Button("重置", GUILayout.Width(60)))
        {
            currentTime = 0f;
        }
        
        EditorGUILayout.Space(20f);
        
        // 播放速度
        EditorGUILayout.LabelField("速度:", GUILayout.Width(40));
        playbackSpeed = EditorGUILayout.Slider(playbackSpeed, 0.1f, 3f, GUILayout.Width(100));
        
        EditorGUILayout.Space(20f);
        
        // 当前时间
        EditorGUILayout.LabelField("时间:", GUILayout.Width(40));
        float newTime = EditorGUILayout.FloatField(currentTime, GUILayout.Width(60));
        if (Mathf.Abs(newTime - currentTime) > 0.01f)
        {
            currentTime = newTime;
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// 绘制属性选择器
    /// </summary>
    private void DrawPropertySelector()
    {
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("编辑属性:", GUILayout.Width(80));
        
        AnimationPropertyType newPropertyType = (AnimationPropertyType)EditorGUILayout.EnumPopup(selectedPropertyType);
        if (newPropertyType != selectedPropertyType)
        {
            selectedPropertyType = newPropertyType;
            selectedKeyframeIndex = -1;
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// 绘制时间轴
    /// </summary>
    private void DrawTimeline()
    {
        EditorGUILayout.LabelField("时间轴", EditorStyles.boldLabel);
        
        var currentSequence = manager.GetSequence(selectedSequenceIndex);
        if (currentSequence == null)
        {
            EditorGUILayout.HelpBox("请选择一个有效的动画序列", MessageType.Info);
            return;
        }
        
        // 获取时间轴绘制区域
        var rect = GUILayoutUtility.GetRect(0, timelineHeight);
        
        // 绘制背景
        EditorGUI.DrawRect(rect, timelineColor);
        
        // 绘制网格
        DrawTimelineGrid(rect, currentSequence);
        
        // 绘制关键帧
        DrawKeyframes(rect, currentSequence);
        
        // 绘制当前时间指示器
        DrawCurrentTimeIndicator(rect, currentSequence);
        
        // 处理鼠标交互
        HandleTimelineInteraction(rect, currentSequence);
        
        // 绘制时间轴信息
        DrawTimelineInfo(rect, currentSequence);
    }
    
    /// <summary>
    /// 绘制时间轴网格
    /// </summary>
    private void DrawTimelineGrid(Rect rect, AnimationSequence sequence)
    {
        // 绘制水平网格线
        for (int i = 0; i <= 10; i++)
        {
            float y = rect.y + (rect.height / 10f) * i;
            Color gridColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            EditorGUI.DrawRect(new Rect(rect.x, y, rect.width, 1f), gridColor);
        }
        
        // 绘制垂直网格线（时间标记）
        float duration = sequence.GetDuration();
        for (int i = 0; i <= 10; i++)
        {
            float x = rect.x + (rect.width / 10f) * i;
            Color gridColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            EditorGUI.DrawRect(new Rect(x, rect.y, 1f, rect.height), gridColor);
            
            // 绘制时间标签
            float time = (duration / 10f) * i;
            string timeLabel = time.ToString("F1") + "s";
            GUI.Label(new Rect(x - 15f, rect.yMax + 2f, 30f, 15f), timeLabel, EditorStyles.miniLabel);
        }
    }
    
    /// <summary>
    /// 绘制关键帧
    /// </summary>
    private void DrawKeyframes(Rect rect, AnimationSequence sequence)
    {
        if (sequence.keyframes == null) return;
        
        // 过滤当前属性类型的关键帧
        var propertyKeyframes = sequence.keyframes.Where(k => k.propertyType == selectedPropertyType).ToList();
        
        foreach (var keyframe in propertyKeyframes)
        {
            Vector2 pos = TimeToPosition(rect, keyframe.time, sequence.GetDuration());
            
            Color color = keyframeColor;
            if (sequence.keyframes.IndexOf(keyframe) == selectedKeyframeIndex)
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
    /// 绘制当前时间指示器
    /// </summary>
    private void DrawCurrentTimeIndicator(Rect rect, AnimationSequence sequence)
    {
        float normalizedTime = currentTime / sequence.GetDuration();
        float x = rect.x + rect.width * normalizedTime;
        
        Color indicatorColor = new Color(1f, 0.2f, 0.2f, 0.8f);
        EditorGUI.DrawRect(new Rect(x, rect.y, 2f, rect.height), indicatorColor);
    }
    
    /// <summary>
    /// 绘制时间轴信息
    /// </summary>
    private void DrawTimelineInfo(Rect rect, AnimationSequence sequence)
    {
        GUIStyle infoStyle = new GUIStyle(EditorStyles.label);
        infoStyle.fontSize = 10;
        infoStyle.normal.textColor = Color.white;
        
        string info = $"序列: {sequence.sequenceName}\n" +
                     $"时长: {sequence.GetDuration():F2}s\n" +
                     $"帧率: {sequence.frameRate}\n" +
                     $"关键帧: {sequence.GetKeyframeCount(selectedPropertyType)}";
        
        EditorGUI.LabelField(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, 60f), info, infoStyle);
    }
    
    /// <summary>
    /// 处理时间轴交互
    /// </summary>
    private void HandleTimelineInteraction(Rect rect, AnimationSequence sequence)
    {
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            // 检查是否点击了关键帧
            int clickedKeyframe = GetKeyframeAtPosition(rect, e.mousePosition, sequence);
            if (clickedKeyframe >= 0)
            {
                selectedKeyframeIndex = clickedKeyframe;
                isDragging = true;
                dragStartPos = e.mousePosition;
                e.Use();
            }
            else
            {
                // 点击空白区域，添加新关键帧
                float time = PositionToTime(rect, e.mousePosition, sequence.GetDuration());
                AddKeyframeAtTime(sequence, time);
                e.Use();
            }
        }
        else if (e.type == EventType.MouseDrag && isDragging && selectedKeyframeIndex >= 0)
        {
            // 拖拽关键帧
            float time = PositionToTime(rect, e.mousePosition, sequence.GetDuration());
            UpdateKeyframeTime(sequence, selectedKeyframeIndex, time);
            e.Use();
        }
        else if (e.type == EventType.MouseUp)
        {
            isDragging = false;
        }
        else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete && selectedKeyframeIndex >= 0)
        {
            // 删除选中的关键帧
            sequence.RemoveKeyframe(selectedKeyframeIndex);
            selectedKeyframeIndex = -1;
            e.Use();
        }
    }
    
    /// <summary>
    /// 绘制关键帧编辑器
    /// </summary>
    private void DrawKeyframeEditor()
    {
        EditorGUILayout.LabelField("关键帧编辑器", EditorStyles.boldLabel);
        
        var currentSequence = manager.GetSequence(selectedSequenceIndex);
        if (currentSequence == null)
        {
            EditorGUILayout.HelpBox("请选择一个有效的动画序列", MessageType.Info);
            return;
        }
        
        // 过滤当前属性类型的关键帧
        var propertyKeyframes = currentSequence.keyframes.Where(k => k.propertyType == selectedPropertyType).ToList();
        
        if (propertyKeyframes.Count == 0)
        {
            EditorGUILayout.HelpBox($"没有 {selectedPropertyType} 类型的关键帧", MessageType.Info);
            return;
        }
        
        // 显示关键帧列表
        for (int i = 0; i < propertyKeyframes.Count; i++)
        {
            var keyframe = propertyKeyframes[i];
            bool isSelected = currentSequence.keyframes.IndexOf(keyframe) == selectedKeyframeIndex;
            
            EditorGUILayout.BeginHorizontal();
            
            // 选择按钮
            if (GUILayout.Button(isSelected ? "●" : "○", GUILayout.Width(20)))
            {
                selectedKeyframeIndex = currentSequence.keyframes.IndexOf(keyframe);
            }
            
            // 关键帧信息
            EditorGUILayout.LabelField($"帧 {keyframe.frameNumber}", GUILayout.Width(60));
            EditorGUILayout.LabelField($"时间 {keyframe.time:F2}s", GUILayout.Width(80));
            
            // 属性值
            EditorGUILayout.LabelField($"值: {GetPropertyValueString(keyframe)}", GUILayout.Width(100));
            
            // 插值类型
            keyframe.interpolationType = (AnimationInterpolationType)EditorGUILayout.EnumPopup(keyframe.interpolationType, GUILayout.Width(100));
            
            EditorGUILayout.EndHorizontal();
        }
        
        // 关键帧操作按钮
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("添加关键帧"))
        {
            AddKeyframeAtCurrentTime(currentSequence);
        }
        
        if (GUILayout.Button("删除选中关键帧") && selectedKeyframeIndex >= 0)
        {
            currentSequence.RemoveKeyframe(selectedKeyframeIndex);
            selectedKeyframeIndex = -1;
        }
        
        if (GUILayout.Button("清空关键帧"))
        {
            if (EditorUtility.DisplayDialog("确认", "确定要清空所有关键帧吗？", "确定", "取消"))
            {
                currentSequence.ClearKeyframes();
                selectedKeyframeIndex = -1;
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    #region 辅助方法
    
    /// <summary>
    /// 将时间转换为位置
    /// </summary>
    private Vector2 TimeToPosition(Rect rect, float time, float duration)
    {
        float normalizedTime = time / duration;
        float x = rect.x + rect.width * normalizedTime;
        float y = rect.y + rect.height / 2f; // 居中显示
        
        return new Vector2(x, y);
    }
    
    /// <summary>
    /// 将位置转换为时间
    /// </summary>
    private float PositionToTime(Rect rect, Vector2 position, float duration)
    {
        float normalizedTime = (position.x - rect.x) / rect.width;
        return Mathf.Clamp(normalizedTime * duration, 0f, duration);
    }
    
    /// <summary>
    /// 获取指定位置的关键帧索引
    /// </summary>
    private int GetKeyframeAtPosition(Rect rect, Vector2 position, AnimationSequence sequence)
    {
        if (sequence.keyframes == null) return -1;
        
        var propertyKeyframes = sequence.keyframes.Where(k => k.propertyType == selectedPropertyType).ToList();
        
        for (int i = 0; i < propertyKeyframes.Count; i++)
        {
            var keyframe = propertyKeyframes[i];
            Vector2 keyframePos = TimeToPosition(rect, keyframe.time, sequence.GetDuration());
            
            if (Vector2.Distance(position, keyframePos) <= keyframeSize)
            {
                return sequence.keyframes.IndexOf(keyframe);
            }
        }
        
        return -1;
    }
    
    /// <summary>
    /// 在指定时间添加关键帧
    /// </summary>
    private void AddKeyframeAtTime(AnimationSequence sequence, float time)
    {
        var keyframe = new AnimationKeyframe
        {
            frameNumber = Mathf.RoundToInt(time * sequence.frameRate),
            time = time,
            propertyType = selectedPropertyType
        };
        
        sequence.AddKeyframe(keyframe);
    }
    
    /// <summary>
    /// 在当前时间添加关键帧
    /// </summary>
    private void AddKeyframeAtCurrentTime(AnimationSequence sequence)
    {
        AddKeyframeAtTime(sequence, currentTime);
    }
    
    /// <summary>
    /// 更新关键帧时间
    /// </summary>
    private void UpdateKeyframeTime(AnimationSequence sequence, int keyframeIndex, float newTime)
    {
        if (keyframeIndex >= 0 && keyframeIndex < sequence.keyframes.Count)
        {
            var keyframe = sequence.keyframes[keyframeIndex];
            keyframe.time = newTime;
            keyframe.frameNumber = Mathf.RoundToInt(newTime * sequence.frameRate);
            
            sequence.SortKeyframes();
        }
    }
    
    /// <summary>
    /// 获取属性值字符串
    /// </summary>
    private string GetPropertyValueString(AnimationKeyframe keyframe)
    {
        switch (keyframe.propertyType)
        {
            case AnimationPropertyType.Position:
                return keyframe.position.ToString("F2");
            case AnimationPropertyType.Rotation:
                return keyframe.rotation.ToString("F2");
            case AnimationPropertyType.Scale:
                return keyframe.scale.ToString("F2");
            case AnimationPropertyType.Color:
                return keyframe.color.ToString();
            case AnimationPropertyType.Alpha:
                return keyframe.alpha.ToString("F2");
            case AnimationPropertyType.CustomProperty:
                return keyframe.value.ToString("F2");
            default:
                return "N/A";
        }
    }
    
    #endregion
}
