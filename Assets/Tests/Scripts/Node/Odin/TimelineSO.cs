using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Timeline帧模式数据类
/// 包含播放控制逻辑和帧数据
/// </summary>
[Serializable]
public class TimelineSO : SerializedScriptableObject
{
    // [Header("Timeline 设置")]
    // [LabelText("Timeline 名称")]
    // [Tooltip("Timeline的名称")]
    [HideInInspector]
    public string timelineName = "Simple Timeline";
    
    [LabelText("总帧数")]
    [Tooltip("Timeline的总帧数")]
    [MinValue(1)]
    public int totalFrames = 100;
    
    // [LabelText("当前帧")]
    // [Tooltip("当前播放的帧")]
    // [ReadOnly]
    // [ProgressBar(0, 100, ColorMember = "GetFrameColor", MaxGetter = "GetMaxFrames")]
    [HideInInspector]
    public int currentFrame = 0;
    
    [LabelText("播放速度")]
    [Tooltip("播放速度倍数")]
    [Range(0.1f, 5f)]
    public float playbackSpeed = 1f;
    
    [Header("播放控制")]
    [LabelText("是否播放")]
    [Tooltip("Timeline是否正在播放")]
    [HideInInspector]
    public bool isPlaying = false;
    
    [LabelText("循环播放")]
    [Tooltip("是否循环播放")]
    public bool loopPlayback = true;
    
    [Header("轨道系统")]
    [LabelText("轨道列表")]
    [Tooltip("Timeline的轨道列表")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "trackName")]
    public TrackData[] tracks = new TrackData[0];
    
    [Header("帧数据")]
    [LabelText("帧数据")]
    [Tooltip("每一帧的数据")]
    [HideInInspector] // 隐藏帧数据表格
    public FrameData[] frameData = new FrameData[0];
    
    [Header("调试信息")]
    [LabelText("播放时间")]
    [ReadOnly]
    [HideInInspector] // 隐藏调试信息
    public float playTime = 0f;
    
    [LabelText("帧率")]
    [ReadOnly]
    [HideInInspector] // 隐藏调试信息
    public float frameRate = 30f;
    
    /// <summary>
    /// 帧数据结构
    /// </summary>
    [Serializable]
    public class FrameData
    {
        [LabelText("帧号")]
        [ReadOnly]
        public int frameNumber;
        
        [LabelText("帧内容")]
        [TextArea(2, 4)]
        public string frameContent = "";
        
        [LabelText("是否关键帧")]
        public bool isKeyFrame = false;
        
        [LabelText("颜色标记")]
        [HideLabel]
        public Color frameColor = Color.white;
        
        [LabelText("操作")]
        [Button("设置为关键帧")]
        public void SetAsKeyFrame()
        {
            isKeyFrame = true;
            frameColor = Color.yellow;
        }
        
        [Button("清除关键帧")]
        public void ClearKeyFrame()
        {
            isKeyFrame = false;
            frameColor = Color.white;
        }
    }
    
    /// <summary>
    /// 轨道数据结构
    /// </summary>
    [Serializable]
    public class TrackData
    {
        [LabelText("轨道名称")]
        [Tooltip("轨道的显示名称")]
        public string trackName = "新轨道";
        
        [LabelText("轨道类型")]
        [Tooltip("轨道的类型")]
        public TrackType trackType = TrackType.Animation;
        
        [LabelText("轨道颜色")]
        [Tooltip("轨道在时间轴上的显示颜色")]
        public Color trackColor = Color.white;
        
        [LabelText("是否启用")]
        [Tooltip("轨道是否启用")]
        public bool isEnabled = true;
        
        [LabelText("是否锁定")]
        [Tooltip("轨道是否锁定编辑")]
        public bool isLocked = false;
        
        [LabelText("轨道高度")]
        [Tooltip("轨道在时间轴上的高度")]
        [Range(20f, 100f)]
        public float trackHeight = 40f;
        
        [LabelText("关键帧数据")]
        [Tooltip("轨道的关键帧数据")]
        [HideInInspector]
        public KeyFrameData[] keyFrames = new KeyFrameData[0];
        
        [LabelText("轨道属性")]
        [Tooltip("轨道的自定义属性")]
        [HideInInspector]
        public TrackProperty[] properties = new TrackProperty[0];
        
        [Button("添加关键帧")]
        public void AddKeyFrame()
        {
            var newKeyFrame = new KeyFrameData
            {
                frame = 0,
                value = 0f,
                interpolationType = InterpolationType.Linear
            };
            
            var newArray = new KeyFrameData[keyFrames.Length + 1];
            keyFrames.CopyTo(newArray, 0);
            newArray[keyFrames.Length] = newKeyFrame;
            keyFrames = newArray;
        }
        
        [Button("清除所有关键帧")]
        public void ClearKeyFrames()
        {
            keyFrames = new KeyFrameData[0];
        }
        
        [Button("删除轨道")]
        public void DeleteTrack()
        {
            // 这个功能需要在TimelineSO中实现
        }
    }
    
    /// <summary>
    /// 轨道类型枚举
    /// </summary>
    public enum TrackType
    {
        [LabelText("动画轨道")]
        Animation,
        [LabelText("音频轨道")]
        Audio,
        [LabelText("事件轨道")]
        Event,
        [LabelText("脚本轨道")]
        Script,
        [LabelText("自定义轨道")]
        Custom
    }
    
    /// <summary>
    /// 关键帧数据结构
    /// </summary>
    [Serializable]
    public class KeyFrameData
    {
        [LabelText("帧号")]
        [Tooltip("关键帧所在的帧号")]
        public int frame = 0;
        
        [LabelText("数值")]
        [Tooltip("关键帧的数值")]
        public float value = 0f;
        
        [LabelText("插值类型")]
        [Tooltip("关键帧之间的插值类型")]
        public InterpolationType interpolationType = InterpolationType.Linear;
        
        [LabelText("缓动曲线")]
        [Tooltip("关键帧的缓动曲线")]
        public AnimationCurve easingCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [LabelText("是否选中")]
        [Tooltip("关键帧是否被选中")]
        public bool isSelected = false;
    }
    
    /// <summary>
    /// 插值类型枚举
    /// </summary>
    public enum InterpolationType
    {
        [LabelText("线性插值")]
        Linear,
        [LabelText("贝塞尔插值")]
        Bezier,
        [LabelText("步进插值")]
        Step,
        [LabelText("自定义曲线")]
        Custom
    }
    
    /// <summary>
    /// 轨道属性数据结构
    /// </summary>
    [Serializable]
    public class TrackProperty
    {
        [LabelText("属性名称")]
        [Tooltip("属性的名称")]
        public string propertyName = "新属性";
        
        [LabelText("属性类型")]
        [Tooltip("属性的数据类型")]
        public PropertyType propertyType = PropertyType.Float;
        
        [LabelText("属性值")]
        [Tooltip("属性的当前值")]
        public object propertyValue;
        
        [LabelText("是否可见")]
        [Tooltip("属性是否在Inspector中可见")]
        public bool isVisible = true;
    }
    
    /// <summary>
    /// 属性类型枚举
    /// </summary>
    public enum PropertyType
    {
        [LabelText("浮点数")]
        Float,
        [LabelText("整数")]
        Int,
        [LabelText("布尔值")]
        Bool,
        [LabelText("字符串")]
        String,
        [LabelText("颜色")]
        Color,
        [LabelText("向量2")]
        Vector2,
        [LabelText("向量3")]
        Vector3,
        [LabelText("对象引用")]
        Object
    }
    
    /// <summary>
    /// 获取当前帧的颜色
    /// </summary>
    private Color GetFrameColor()
    {
        if (isPlaying)
            return Color.green;
        else
            return Color.red;
    }
    
    /// <summary>
    /// 获取ProgressBar的最大值
    /// </summary>
    private int GetMaxFrames()
    {
        return totalFrames - 1;
    }
    
    public void Play()
    {
        isPlaying = true;
        Debug.Log($"Timeline '{timelineName}' 开始播放 - 总帧数: {totalFrames}, 帧率: {frameRate}");
    }
    
    public void Pause()
    {
        isPlaying = false;
        Debug.Log($"Timeline '{timelineName}' 已暂停");
    }
    
    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        playTime = 0f;
        Debug.Log($"Timeline '{timelineName}' 已停止");
    }
    
    public void Reset()
    {
        isPlaying = false;
        currentFrame = 0;
        playTime = 0f;
        Debug.Log($"Timeline '{timelineName}' 已重置");
    }
    
    public void GoToFrame()
    {
        currentFrame = Mathf.Clamp(currentFrame, 0, totalFrames - 1);
        playTime = currentFrame / frameRate;
        Debug.Log($"Timeline '{timelineName}' 跳转到第 {currentFrame} 帧");
    }
    
    public void InitializeFrameData()
    {
        frameData = new FrameData[totalFrames];
        for (int i = 0; i < totalFrames; i++)
        {
            frameData[i] = new FrameData
            {
                frameNumber = i,
                frameContent = $"Frame {i} Content",
                isKeyFrame = false,
                frameColor = Color.white
            };
        }
        Debug.Log($"Timeline '{timelineName}' 帧数据已初始化，共 {totalFrames} 帧");
    }
    
    /// <summary>
    /// 更新Timeline（支持编辑器和运行时）
    /// </summary>
    public void UpdateTimeline()
    {
        if (!isPlaying) return;
        
        // 获取时间增量（支持编辑器和运行时）
        float deltaTime = GetDeltaTime();
        playTime += deltaTime * playbackSpeed;
        int newFrame = Mathf.FloorToInt(playTime * frameRate);
        
        if (newFrame != currentFrame)
        {
            currentFrame = newFrame;
            Debug.Log($"Timeline更新 - 帧: {currentFrame}, 时间: {playTime:F2}s, 增量: {deltaTime:F3}s");
            
            if (currentFrame >= totalFrames)
            {
                if (loopPlayback)
                {
                    currentFrame = 0;
                    playTime = 0f;
                    Debug.Log("Timeline循环播放");
                }
                else
                {
                    currentFrame = totalFrames - 1;
                    isPlaying = false;
                    Debug.Log("Timeline播放结束");
                }
            }
        }
    }
    
    /// <summary>
    /// 获取时间增量（支持编辑器和运行时）
    /// </summary>
    private float GetDeltaTime()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 编辑器模式下使用固定时间增量
            return 1f / 60f; // 假设60FPS
        }
        #endif
        
        return Time.deltaTime;
    }
    
    /// <summary>
    /// 获取当前帧的数据
    /// </summary>
    public FrameData GetCurrentFrameData()
    {
        if (frameData == null || frameData.Length == 0 || currentFrame < 0 || currentFrame >= frameData.Length)
            return null;
            
        return frameData[currentFrame];
    }
    
    /// <summary>
    /// 设置帧内容
    /// </summary>
    public void SetFrameContent(int frame, string content)
    {
        if (frameData == null || frame < 0 || frame >= frameData.Length) return;
        
        frameData[frame].frameContent = content;
    }
    
    /// <summary>
    /// 添加新轨道
    /// </summary>
    public void AddTrack(string trackName = "新轨道", TrackType trackType = TrackType.Animation)
    {
        var newTrack = new TrackData
        {
            trackName = trackName,
            trackType = trackType,
            trackColor = GetTrackColorByType(trackType),
            isEnabled = true,
            isLocked = false,
            trackHeight = 40f,
            keyFrames = new KeyFrameData[0],
            properties = new TrackProperty[0]
        };
        
        var newArray = new TrackData[tracks.Length + 1];
        tracks.CopyTo(newArray, 0);
        newArray[tracks.Length] = newTrack;
        tracks = newArray;
        
        Debug.Log($"已添加新轨道: {trackName} (类型: {trackType})");
    }
    
    /// <summary>
    /// 删除轨道
    /// </summary>
    public void RemoveTrack(int trackIndex)
    {
        if (tracks == null || trackIndex < 0 || trackIndex >= tracks.Length) return;
        
        var newArray = new TrackData[tracks.Length - 1];
        for (int i = 0, j = 0; i < tracks.Length; i++)
        {
            if (i != trackIndex)
            {
                newArray[j++] = tracks[i];
            }
        }
        tracks = newArray;
        
        Debug.Log($"已删除轨道索引: {trackIndex}");
    }
    
    /// <summary>
    /// 根据类型获取轨道颜色
    /// </summary>
    private Color GetTrackColorByType(TrackType trackType)
    {
        switch (trackType)
        {
            case TrackType.Animation: return new Color(0.2f, 0.8f, 0.2f, 1f); // 绿色
            case TrackType.Audio: return new Color(0.2f, 0.2f, 0.8f, 1f); // 蓝色
            case TrackType.Event: return new Color(0.8f, 0.8f, 0.2f, 1f); // 黄色
            case TrackType.Script: return new Color(0.8f, 0.2f, 0.8f, 1f); // 紫色
            case TrackType.Custom: return new Color(0.8f, 0.4f, 0.2f, 1f); // 橙色
            default: return Color.white;
        }
    }
    
    /// <summary>
    /// 获取轨道
    /// </summary>
    public TrackData GetTrack(int trackIndex)
    {
        if (tracks == null || trackIndex < 0 || trackIndex >= tracks.Length) return null;
        return tracks[trackIndex];
    }
    
    /// <summary>
    /// 在指定轨道添加关键帧
    /// </summary>
    public void AddKeyFrameToTrack(int trackIndex, int frame, float value)
    {
        var track = GetTrack(trackIndex);
        if (track == null) return;
        
        var newKeyFrame = new KeyFrameData
        {
            frame = frame,
            value = value,
            interpolationType = InterpolationType.Linear,
            easingCurve = AnimationCurve.Linear(0, 0, 1, 1),
            isSelected = false
        };
        
        var newArray = new KeyFrameData[track.keyFrames.Length + 1];
        track.keyFrames.CopyTo(newArray, 0);
        newArray[track.keyFrames.Length] = newKeyFrame;
        track.keyFrames = newArray;
        
        Debug.Log($"已在轨道 {track.trackName} 添加关键帧: 帧{frame}, 值{value}");
    }
    
    /// <summary>
    /// 从指定轨道删除关键帧
    /// </summary>
    public void RemoveKeyFrameFromTrack(int trackIndex, int keyFrameIndex)
    {
        var track = GetTrack(trackIndex);
        if (track == null || keyFrameIndex < 0 || keyFrameIndex >= track.keyFrames.Length) return;
        
        var newArray = new KeyFrameData[track.keyFrames.Length - 1];
        for (int i = 0, j = 0; i < track.keyFrames.Length; i++)
        {
            if (i != keyFrameIndex)
            {
                newArray[j++] = track.keyFrames[i];
            }
        }
        track.keyFrames = newArray;
        
        Debug.Log($"已从轨道 {track.trackName} 删除关键帧索引: {keyFrameIndex}");
    }
    
    /// <summary>
    /// 获取轨道在指定帧的值
    /// </summary>
    public float GetTrackValueAtFrame(int trackIndex, int frame)
    {
        var track = GetTrack(trackIndex);
        if (track == null || track.keyFrames.Length == 0) return 0f;
        
        // 如果没有关键帧，返回0
        if (track.keyFrames.Length == 0) return 0f;
        
        // 如果只有一个关键帧，返回该关键帧的值
        if (track.keyFrames.Length == 1) return track.keyFrames[0].value;
        
        // 找到当前帧前后的关键帧
        KeyFrameData prevKeyFrame = null;
        KeyFrameData nextKeyFrame = null;
        
        for (int i = 0; i < track.keyFrames.Length; i++)
        {
            if (track.keyFrames[i].frame <= frame)
            {
                prevKeyFrame = track.keyFrames[i];
            }
            if (track.keyFrames[i].frame >= frame && nextKeyFrame == null)
            {
                nextKeyFrame = track.keyFrames[i];
                break;
            }
        }
        
        // 如果当前帧正好是关键帧，直接返回
        if (prevKeyFrame != null && prevKeyFrame.frame == frame)
            return prevKeyFrame.value;
        
        // 如果当前帧在第一个关键帧之前，返回第一个关键帧的值
        if (prevKeyFrame == null)
            return track.keyFrames[0].value;
        
        // 如果当前帧在最后一个关键帧之后，返回最后一个关键帧的值
        if (nextKeyFrame == null)
            return track.keyFrames[track.keyFrames.Length - 1].value;
        
        // 在两个关键帧之间进行插值
        float t = (float)(frame - prevKeyFrame.frame) / (nextKeyFrame.frame - prevKeyFrame.frame);
        
        switch (prevKeyFrame.interpolationType)
        {
            case InterpolationType.Linear:
                return Mathf.Lerp(prevKeyFrame.value, nextKeyFrame.value, t);
            case InterpolationType.Step:
                return prevKeyFrame.value;
            case InterpolationType.Bezier:
                // 简单的贝塞尔插值
                return Mathf.Lerp(prevKeyFrame.value, nextKeyFrame.value, t * t * (3f - 2f * t));
            case InterpolationType.Custom:
                return Mathf.Lerp(prevKeyFrame.value, nextKeyFrame.value, prevKeyFrame.easingCurve.Evaluate(t));
            default:
                return Mathf.Lerp(prevKeyFrame.value, nextKeyFrame.value, t);
        }
    }
}
