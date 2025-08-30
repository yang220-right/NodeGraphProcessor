using System;
using System.Collections.Generic;
using GraphProcessor;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// TimelineNode的NodeView实现
/// 继承自SOInspectorWrapper，专门用于显示TimelineNode的所有属性
/// </summary>
[NodeCustomEditor(typeof(TimelineNode))]
public class TimelineNodeView : BaseSONodeView
{
    /// <summary>
    /// 创建目标ScriptableObject
    /// 这里我们创建一个包装了TimelineNode数据的SO对象
    /// </summary>
    /// <returns>包装了TimelineNode数据的SO对象</returns>
    protected override ScriptableObject CreateSO()
    {
        var so = ScriptableObject.CreateInstance<TimelineNodeWrapper>();
        
        // 设置初始值
        so.timelineName = "My Timeline";
        so.duration = 60f;
        so.loop = false;
        so.autoPlay = false;
        so.currentPlayTime = 0f;
        so.playbackSpeed = 1f;
        so.playbackState = TimelineNodeWrapper.PlaybackState.Stopped;
        so.maxTracks = 10;
        so.interpolationMode = TimelineNodeWrapper.InterpolationMode.Linear;
        so.playCount = 0;
        so.lastPlayTime = 0f;
        so.isInitialized = false;
        
        // 创建默认轨道
        so.tracks = new List<TimelineNodeWrapper.TimelineTrackWrapper>
        {
            new TimelineNodeWrapper.TimelineTrackWrapper
            {
                trackName = "Animation Track",
                trackType = TimelineNodeWrapper.TimelineTrackWrapper.TrackType.Animation,
                isEnabled = true,
                color = Color.blue,
                volume = 1f,
                offset = 0f,
                length = 60f
            },
            new TimelineNodeWrapper.TimelineTrackWrapper
            {
                trackName = "Audio Track",
                trackType = TimelineNodeWrapper.TimelineTrackWrapper.TrackType.Audio,
                isEnabled = true,
                color = Color.green,
                volume = 1f,
                offset = 0f,
                length = 60f
            },
            new TimelineNodeWrapper.TimelineTrackWrapper
            {
                trackName = "Event Track",
                trackType = TimelineNodeWrapper.TimelineTrackWrapper.TrackType.Event,
                isEnabled = true,
                color = Color.yellow,
                volume = 1f,
                offset = 0f,
                length = 60f
            }
        };
        
        // 创建默认关键帧
        so.keyframes = new List<TimelineNodeWrapper.KeyframeWrapper>
        {
            new TimelineNodeWrapper.KeyframeWrapper
            {
                time = 0f,
                value = 0f,
                trackIndex = 0,
                keyframeType = TimelineNodeWrapper.KeyframeWrapper.KeyframeType.Animation,
                easeInTime = 0f,
                easeOutTime = 0f,
                isLocked = false
            },
            new TimelineNodeWrapper.KeyframeWrapper
            {
                time = 30f,
                value = 0.5f,
                trackIndex = 0,
                keyframeType = TimelineNodeWrapper.KeyframeWrapper.KeyframeType.Animation,
                easeInTime = 0f,
                easeOutTime = 0f,
                isLocked = false
            },
            new TimelineNodeWrapper.KeyframeWrapper
            {
                time = 60f,
                value = 1f,
                trackIndex = 0,
                keyframeType = TimelineNodeWrapper.KeyframeWrapper.KeyframeType.Animation,
                easeInTime = 0f,
                easeOutTime = 0f,
                isLocked = false
            }
        };
        
        Debug.Log("TimelineNodeWrapper 已创建并初始化");
        return so;
    }

    protected override void SetWidth(){
        style.width = 450f;
    }

    /// <summary>
    /// 重写自动创建方法，添加更多日志信息
    /// </summary>
    protected override void AutoCreateAndDisplaySO()
    {
        Debug.Log("TimelineNodeView 开始自动创建SO对象...");
        base.AutoCreateAndDisplaySO();
        
        if (targetSO != null)
        {
            Debug.Log($"TimelineNodeView SO对象创建成功: {targetSO.name}");
        }
        else
        {
            Debug.LogError("TimelineNodeView SO对象创建失败");
        }
    }
}

/// <summary>
/// TimelineNode数据包装器
/// 将TimelineNode的数据包装成ScriptableObject，以便在Inspector中显示
/// </summary>
[CreateAssetMenu(fileName = "TimelineNodeWrapper", menuName = "Examples/TimelineNodeWrapper")]
public class TimelineNodeWrapper : SerializedScriptableObject
{
    [Header("时间轴设置")]
    [Tooltip("时间轴名称")]
    public string timelineName = "My Timeline";
    
    [Tooltip("时间轴总长度（秒）")]
    [Range(1f, 300f)]
    public float duration = 60f;
    
    [Tooltip("是否循环播放")]
    public bool loop = false;
    
    [Tooltip("是否自动播放")]
    public bool autoPlay = false;
    
    [Header("播放控制")]
    [Tooltip("当前播放时间")]
    [Range(0f, 300f)]
    public float currentPlayTime = 0f;
    
    [Tooltip("播放速度")]
    [Range(0.1f, 5f)]
    public float playbackSpeed = 1f;
    
    [Tooltip("播放状态")]
    public PlaybackState playbackState = PlaybackState.Stopped;
    
    [Header("轨道设置")]
    [Tooltip("轨道列表")]
    [TableList(AlwaysExpanded = true, DrawScrollView = false)]
    public List<TimelineTrackWrapper> tracks = new List<TimelineTrackWrapper>();
    
    [Tooltip("最大轨道数量")]
    [Range(1, 20)]
    public int maxTracks = 10;
    
    [Header("关键帧设置")]
    [Tooltip("关键帧列表")]
    [TableList(AlwaysExpanded = true, DrawScrollView = false)]
    public List<KeyframeWrapper> keyframes = new List<KeyframeWrapper>();
    
    [Tooltip("关键帧插值模式")]
    public InterpolationMode interpolationMode = InterpolationMode.Linear;
    
    [Header("状态信息")]
    [Tooltip("播放次数")]
    public int playCount = 0;
    
    [Tooltip("最后播放时间")]
    public float lastPlayTime = 0f;
    
    [Tooltip("是否已初始化")]
    public bool isInitialized = false;
    
    [Header("时间轴显示设置")]
    [Tooltip("时间轴缩放")]
    [Range(0.1f, 5f)]
    public float timelineZoom = 1f;
    
    [Tooltip("时间轴偏移")]
    public float timelineOffset = 0f;
    
    [Tooltip("显示时间刻度")]
    public bool showTimeMarkers = true;
    
    [Tooltip("时间刻度间隔（秒）")]
    [Range(1f, 30f)]
    public float timeMarkerInterval = 5f;
    
    [Tooltip("显示播放头")]
    public bool showPlayhead = true;
    
    [Tooltip("播放头颜色")]
    public Color playheadColor = Color.red;
    
    [Tooltip("轨道高度")]
    [Range(20f, 100f)]
    public float trackHeight = 40f;
    
    [Tooltip("轨道间距")]
    [Range(5f, 20f)]
    public float trackSpacing = 10f;
    
    /// <summary>
    /// 播放状态枚举
    /// </summary>
    public enum PlaybackState
    {
        Stopped,
        Playing,
        Paused,
        Recording
    }
    
    /// <summary>
    /// 插值模式枚举
    /// </summary>
    public enum InterpolationMode
    {
        None,
        Linear,
        Smooth,
        EaseIn,
        EaseOut,
        EaseInOut
    }
    
    /// <summary>
    /// 时间轴轨道包装器
    /// </summary>
    [Serializable]
    public class TimelineTrackWrapper
    {
        [TableColumnWidth(120)]
        [Tooltip("轨道名称")]
        public string trackName = "New Track";
        
        [TableColumnWidth(100)]
        [Tooltip("轨道类型")]
        public TrackType trackType = TrackType.Animation;
        
        [TableColumnWidth(60)]
        [Tooltip("是否启用")]
        public bool isEnabled = true;
        
        [TableColumnWidth(80)]
        [Tooltip("轨道颜色")]
        [ColorPalette]
        public Color color = Color.white;
        
        [TableColumnWidth(80)]
        [Tooltip("轨道音量")]
        [Range(0f, 1f)]
        public float volume = 1f;
        
        [TableColumnWidth(80)]
        [Tooltip("轨道偏移")]
        public float offset = 0f;
        
        [TableColumnWidth(80)]
        [Tooltip("轨道长度")]
        public float length = 0f;
        
        [TableColumnWidth(120)]
        [VerticalGroup("Actions")]
        [Button("编辑轨道")]
        public void EditTrack()
        {
            Debug.Log($"编辑轨道: {trackName}");
        }
        
        [TableColumnWidth(120)]
        [VerticalGroup("Actions")]
        [Button("删除轨道")]
        public void DeleteTrack()
        {
            Debug.Log($"删除轨道: {trackName}");
        }
        
        /// <summary>
        /// 轨道类型枚举
        /// </summary>
        public enum TrackType
        {
            Animation,
            Audio,
            Event,
            Control
        }
    }
    
    /// <summary>
    /// 关键帧包装器
    /// </summary>
    [Serializable]
    public class KeyframeWrapper
    {
        [TableColumnWidth(80)]
        [Tooltip("关键帧时间")]
        public float time = 0f;
        
        [TableColumnWidth(80)]
        [Tooltip("关键帧值")]
        [Range(0f, 1f)]
        public float value = 0f;
        
        [TableColumnWidth(80)]
        [Tooltip("所属轨道索引")]
        public int trackIndex = 0;
        
        [TableColumnWidth(100)]
        [Tooltip("关键帧类型")]
        public KeyframeType keyframeType = KeyframeType.Animation;
        
        [TableColumnWidth(80)]
        [Tooltip("缓入时间")]
        public float easeInTime = 0f;
        
        [TableColumnWidth(80)]
        [Tooltip("缓出时间")]
        public float easeOutTime = 0f;
        
        [TableColumnWidth(60)]
        [Tooltip("是否锁定")]
        public bool isLocked = false;
        
        [TableColumnWidth(120)]
        [VerticalGroup("Actions")]
        [Button("编辑关键帧")]
        public void EditKeyframe()
        {
            Debug.Log($"编辑关键帧: 时间 {time:F2}s, 值 {value:F2}");
        }
        
        [TableColumnWidth(120)]
        [VerticalGroup("Actions")]
        [Button("删除关键帧")]
        public void DeleteKeyframe()
        {
            Debug.Log($"删除关键帧: 时间 {time:F2}s");
        }
        
        /// <summary>
        /// 关键帧类型枚举
        /// </summary>
        public enum KeyframeType
        {
            Animation,
            Audio,
            Event,
            Control
        }
    }
    
    /// <summary>
    /// 重置所有值为默认值
    /// </summary>
    [ContextMenu("重置为默认值")]
    public void ResetToDefaults()
    {
        timelineName = "My Timeline";
        duration = 60f;
        loop = false;
        autoPlay = false;
        currentPlayTime = 0f;
        playbackSpeed = 1f;
        playbackState = PlaybackState.Stopped;
        maxTracks = 10;
        interpolationMode = InterpolationMode.Linear;
        playCount = 0;
        lastPlayTime = 0f;
        isInitialized = false;
        
        // 重置轨道
        tracks.Clear();
        CreateDefaultTracks();
        
        // 重置关键帧
        keyframes.Clear();
        CreateDefaultKeyframes();
        
        Debug.Log("TimelineNodeWrapper 已重置为默认值");
    }
    
    /// <summary>
    /// 创建默认轨道
    /// </summary>
    private void CreateDefaultTracks()
    {
        tracks.Add(new TimelineTrackWrapper
        {
            trackName = "Animation Track",
            trackType = TimelineTrackWrapper.TrackType.Animation,
            isEnabled = true,
            color = Color.blue,
            volume = 1f,
            offset = 0f,
            length = 60f
        });
        
        tracks.Add(new TimelineTrackWrapper
        {
            trackName = "Audio Track",
            trackType = TimelineTrackWrapper.TrackType.Audio,
            isEnabled = true,
            color = Color.green,
            volume = 1f,
            offset = 0f,
            length = 60f
        });
        
        tracks.Add(new TimelineTrackWrapper
        {
            trackName = "Event Track",
            trackType = TimelineTrackWrapper.TrackType.Event,
            isEnabled = true,
            color = Color.yellow,
            volume = 1f,
            offset = 0f,
            length = 60f
        });
    }
    
    /// <summary>
    /// 创建默认关键帧
    /// </summary>
    private void CreateDefaultKeyframes()
    {
        keyframes.Add(new KeyframeWrapper
        {
            time = 0f,
            value = 0f,
            trackIndex = 0,
            keyframeType = KeyframeWrapper.KeyframeType.Animation,
            easeInTime = 0f,
            easeOutTime = 0f,
            isLocked = false
        });
        
        keyframes.Add(new KeyframeWrapper
        {
            time = 30f,
            value = 0.5f,
            trackIndex = 0,
            keyframeType = KeyframeWrapper.KeyframeType.Animation,
            easeInTime = 0f,
            easeOutTime = 0f,
            isLocked = false
        });
        
        keyframes.Add(new KeyframeWrapper
        {
            time = 60f,
            value = 1f,
            trackIndex = 0,
            keyframeType = KeyframeWrapper.KeyframeType.Animation,
            easeInTime = 0f,
            easeOutTime = 0f,
            isLocked = false
        });
    }
    
    /// <summary>
    /// 随机化所有数值
    /// </summary>
    [ContextMenu("随机化数值")]
    public void RandomizeValues()
    {
        duration = Random.Range(30f, 120f);
        playbackSpeed = Random.Range(0.5f, 2f);
        currentPlayTime = Random.Range(0f, duration);
        playbackState = (PlaybackState)Random.Range(0, 4);
        interpolationMode = (InterpolationMode)Random.Range(0, 6);
        
        // 随机化轨道
        foreach (var track in tracks)
        {
            track.volume = Random.Range(0.5f, 1f);
            track.offset = Random.Range(-10f, 10f);
            track.length = Random.Range(duration * 0.8f, duration * 1.2f);
        }
        
        // 随机化关键帧
        foreach (var keyframe in keyframes)
        {
            keyframe.time = Random.Range(0f, duration);
            keyframe.value = Random.Range(0f, 1f);
            keyframe.trackIndex = Random.Range(0, tracks.Count);
            keyframe.easeInTime = Random.Range(0f, 2f);
            keyframe.easeOutTime = Random.Range(0f, 2f);
        }
        
        Debug.Log("TimelineNodeWrapper 数值已随机化");
    }
    
    /// <summary>
    /// 打印当前状态
    /// </summary>
    [ContextMenu("打印状态")]
    public void PrintStatus()
    {
        Debug.Log($"TimelineNodeWrapper 状态:\n" +
                  $"时间轴名称: {timelineName}\n" +
                  $"总长度: {duration:F2}s\n" +
                  $"循环播放: {loop}\n" +
                  $"自动播放: {autoPlay}\n" +
                  $"当前时间: {currentPlayTime:F2}s\n" +
                  $"播放速度: {playbackSpeed:F2}\n" +
                  $"播放状态: {playbackState}\n" +
                  $"轨道数量: {tracks.Count}\n" +
                  $"关键帧数量: {keyframes.Count}\n" +
                  $"插值模式: {interpolationMode}\n" +
                  $"播放次数: {playCount}\n" +
                  $"最后播放时间: {lastPlayTime:F3}s\n" +
                  $"已初始化: {isInitialized}");
    }
    
    /// <summary>
    /// 应用设置到实际的TimelineNode
    /// </summary>
    /// <param name="targetNode">目标TimelineNode</param>
    public void ApplyToNode(TimelineNode targetNode)
    {
        if (targetNode == null) return;
        
        targetNode.timelineName = timelineName;
        targetNode.duration = duration;
        targetNode.loop = loop;
        targetNode.autoPlay = autoPlay;
        targetNode.currentPlayTime = currentPlayTime;
        targetNode.playbackSpeed = playbackSpeed;
        targetNode.playbackState = (TimelineNode.PlaybackState)playbackState;
        targetNode.maxTracks = maxTracks;
        targetNode.interpolationMode = (TimelineNode.InterpolationMode)interpolationMode;
        targetNode.playCount = playCount;
        targetNode.lastPlayTime = lastPlayTime;
        targetNode.isInitialized = isInitialized;
        
        // 应用轨道设置
        targetNode.tracks.Clear();
        foreach (var trackWrapper in tracks)
        {
            var track = new TimelineTrack
            {
                trackName = trackWrapper.trackName,
                trackType = (TimelineTrack.TrackType)trackWrapper.trackType,
                isEnabled = trackWrapper.isEnabled,
                color = trackWrapper.color,
                volume = trackWrapper.volume,
                offset = trackWrapper.offset,
                length = trackWrapper.length
            };
            targetNode.tracks.Add(track);
        }
        
        // 应用关键帧设置
        targetNode.keyframes.Clear();
        foreach (var keyframeWrapper in keyframes)
        {
            var keyframe = new KeyframeData
            {
                time = keyframeWrapper.time,
                value = keyframeWrapper.value,
                trackIndex = keyframeWrapper.trackIndex,
                keyframeType = (KeyframeData.KeyframeType)keyframeWrapper.keyframeType,
                easeInTime = keyframeWrapper.easeInTime,
                easeOutTime = keyframeWrapper.easeOutTime,
                isLocked = keyframeWrapper.isLocked
            };
            targetNode.keyframes.Add(keyframe);
        }
        
        Debug.Log("设置已应用到目标TimelineNode");
    }
    
    /// <summary>
    /// 从实际的TimelineNode同步设置
    /// </summary>
    /// <param name="targetNode">目标TimelineNode</param>
    public void SyncFromNode(TimelineNode targetNode)
    {
        if (targetNode == null) return;
        
        timelineName = targetNode.timelineName;
        duration = targetNode.duration;
        loop = targetNode.loop;
        autoPlay = targetNode.autoPlay;
        currentPlayTime = targetNode.currentPlayTime;
        playbackSpeed = targetNode.playbackSpeed;
        playbackState = (PlaybackState)targetNode.playbackState;
        maxTracks = targetNode.maxTracks;
        interpolationMode = (InterpolationMode)targetNode.interpolationMode;
        playCount = targetNode.playCount;
        lastPlayTime = targetNode.lastPlayTime;
        isInitialized = targetNode.isInitialized;
        
        // 同步轨道设置
        tracks.Clear();
        foreach (var track in targetNode.tracks)
        {
            var trackWrapper = new TimelineTrackWrapper
            {
                trackName = track.trackName,
                trackType = (TimelineTrackWrapper.TrackType)track.trackType,
                isEnabled = track.isEnabled,
                color = track.color,
                volume = track.volume,
                offset = track.offset,
                length = track.length
            };
            tracks.Add(trackWrapper);
        }
        
        // 同步关键帧设置
        keyframes.Clear();
        foreach (var keyframe in targetNode.keyframes)
        {
            var keyframeWrapper = new KeyframeWrapper
            {
                time = keyframe.time,
                value = keyframe.value,
                trackIndex = keyframe.trackIndex,
                keyframeType = (KeyframeWrapper.KeyframeType)keyframe.keyframeType,
                easeInTime = keyframe.easeInTime,
                easeOutTime = keyframe.easeOutTime,
                isLocked = keyframe.isLocked
            };
            keyframes.Add(keyframeWrapper);
        }
        
        Debug.Log("设置已从目标TimelineNode同步");
    }
    
    /// <summary>
    /// 添加新轨道
    /// </summary>
    [Button("添加轨道")]
    public void AddTrack()
    {
        if (tracks.Count >= maxTracks)
        {
            Debug.LogWarning($"无法添加更多轨道，已达到最大数量: {maxTracks}");
            return;
        }
        
        var newTrack = new TimelineTrackWrapper
        {
            trackName = $"Track {tracks.Count + 1}",
            trackType = TimelineTrackWrapper.TrackType.Animation,
            isEnabled = true,
            color = new Color(Random.value, Random.value, Random.value),
            volume = 1f,
            offset = 0f,
            length = duration
        };
        
        tracks.Add(newTrack);
        Debug.Log($"已添加新轨道: {newTrack.trackName}");
    }
    
    /// <summary>
    /// 添加新关键帧
    /// </summary>
    [Button("添加关键帧")]
    public void AddKeyframe()
    {
        var newKeyframe = new KeyframeWrapper
        {
            time = currentPlayTime,
            value = Random.Range(0f, 1f),
            trackIndex = 0,
            keyframeType = KeyframeWrapper.KeyframeType.Animation,
            easeInTime = 0f,
            easeOutTime = 0f,
            isLocked = false
        };
        
        keyframes.Add(newKeyframe);
        Debug.Log($"已在时间 {currentPlayTime:F2}s 添加关键帧，值: {newKeyframe.value:F2}");
    }
    
    /// <summary>
    /// 播放时间轴
    /// </summary>
    [Button("播放")]
    public void Play()
    {
        playbackState = PlaybackState.Playing;
        Debug.Log($"Timeline '{timelineName}' 开始播放");
    }
    
    /// <summary>
    /// 暂停播放
    /// </summary>
    [Button("暂停")]
    public void Pause()
    {
        playbackState = PlaybackState.Paused;
        Debug.Log($"Timeline '{timelineName}' 已暂停");
    }
    
    /// <summary>
    /// 停止播放
    /// </summary>
    [Button("停止")]
    public void Stop()
    {
        playbackState = PlaybackState.Stopped;
        currentPlayTime = 0f;
        Debug.Log($"Timeline '{timelineName}' 已停止");
    }
    
    /// <summary>
    /// 跳转到开始
    /// </summary>
    [Button("跳转到开始")]
    public void JumpToStart()
    {
        currentPlayTime = 0f;
        Debug.Log($"Timeline '{timelineName}' 跳转到开始位置");
    }
    
    /// <summary>
    /// 跳转到结束
    /// </summary>
    [Button("跳转到结束")]
    public void JumpToEnd()
    {
        currentPlayTime = duration;
        Debug.Log($"Timeline '{timelineName}' 跳转到结束位置");
    }
    
    /// <summary>
    /// 时间轴控制
    /// </summary>
    [HorizontalGroup("时间轴控制")]
    [Button("放大")]
    public void ZoomIn()
    {
        timelineZoom = Mathf.Min(timelineZoom * 1.2f, 5f);
        Debug.Log($"时间轴已放大，当前缩放: {timelineZoom:F2}");
    }
    
    [HorizontalGroup("时间轴控制")]
    [Button("缩小")]
    public void ZoomOut()
    {
        timelineZoom = Mathf.Max(timelineZoom / 1.2f, 0.1f);
        Debug.Log($"时间轴已缩小，当前缩放: {timelineZoom:F2}");
    }
    
    [HorizontalGroup("时间轴控制")]
    [Button("重置视图")]
    public void ResetView()
    {
        timelineZoom = 1f;
        timelineOffset = 0f;
        Debug.Log("时间轴视图已重置");
    }
    
    /// <summary>
    /// 时间轴显示设置
    /// </summary>
    [HorizontalGroup("显示设置")]
    [Button("切换时间刻度")]
    public void ToggleTimeMarkers()
    {
        showTimeMarkers = !showTimeMarkers;
        Debug.Log($"时间刻度显示: {showTimeMarkers}");
    }
    
    [HorizontalGroup("显示设置")]
    [Button("切换播放头")]
    public void TogglePlayhead()
    {
        showPlayhead = !showPlayhead;
        Debug.Log($"播放头显示: {showPlayhead}");
    }
    
    /// <summary>
    /// 时间跳转
    /// </summary>
    [HorizontalGroup("时间跳转")]
    [Button("跳转25%")]
    public void JumpTo25Percent()
    {
        currentPlayTime = duration * 0.25f;
        Debug.Log($"Timeline '{timelineName}' 跳转到25%位置: {currentPlayTime:F2}s");
    }
    
    [HorizontalGroup("时间跳转")]
    [Button("跳转50%")]
    public void JumpTo50Percent()
    {
        currentPlayTime = duration * 0.5f;
        Debug.Log($"Timeline '{timelineName}' 跳转到50%位置: {currentPlayTime:F2}s");
    }
    
    [HorizontalGroup("时间跳转")]
    [Button("跳转75%")]
    public void JumpTo75Percent()
    {
        currentPlayTime = duration * 0.75f;
        Debug.Log($"Timeline '{timelineName}' 跳转到75%位置: {currentPlayTime:F2}s");
    }
}
