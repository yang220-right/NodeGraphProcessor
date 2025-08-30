using GraphProcessor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Timeline节点
/// 提供类似Unity Timeline的功能，包括时间轴、轨道、关键帧等
/// </summary>
[System.Serializable, NodeMenuItem("Action/Timeline")]
public class TimelineNode : BaseNode
{
    [Input(name = "Time Input")]
    public float timeInput;
    
    [Input(name = "Speed Multiplier")]
    public float speedMultiplier = 1f;
    
    [Output(name = "Current Time")]
    public float currentTime;
    
    [Output(name = "Is Playing")]
    public bool isPlaying;
    
    [Output(name = "Timeline Data")]
    public TimelineData timelineData;
    
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
    public List<TimelineTrack> tracks = new List<TimelineTrack>();
    
    [Tooltip("最大轨道数量")]
    [Range(1, 20)]
    public int maxTracks = 10;
    
    [Tooltip("轨道混合模式")]
    public TrackBlendMode trackBlendMode = TrackBlendMode.Override;
    
    [Tooltip("轨道权重")]
    [Range(0f, 1f)]
    public float trackWeight = 1f;
    
    [Header("关键帧设置")]
    [Tooltip("关键帧列表")]
    public List<KeyframeData> keyframes = new List<KeyframeData>();
    
    [Tooltip("关键帧插值模式")]
    public InterpolationMode interpolationMode = InterpolationMode.Linear;
    
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
    
    [Header("状态信息")]
    [Tooltip("播放次数")]
    public int playCount = 0;
    
    [Tooltip("最后播放时间")]
    public float lastPlayTime = 0f;
    
    [Tooltip("是否已初始化")]
    public bool isInitialized = false;
    
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
    /// 轨道混合模式枚举
    /// </summary>
    public enum TrackBlendMode
    {
        Override,    // 覆盖模式
        Additive,    // 叠加模式
        Multiply,    // 乘法模式
        Blend        // 混合模式
    }
    
    /// <summary>
    /// 轨道优先级枚举
    /// </summary>
    public enum TrackPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
    
    public override string name => "Timeline";
    
    protected override void Process()
    {
        if (!isInitialized)
        {
            InitializeTimeline();
        }
        
        // 处理时间输入
        if (timeInput > 0)
        {
            currentPlayTime = Mathf.Clamp(timeInput, 0f, duration);
        }
        
        // 应用速度倍数
        float actualSpeed = playbackSpeed * speedMultiplier;
        
        // 更新播放时间
        if (playbackState == PlaybackState.Playing)
        {
            currentPlayTime += actualSpeed * Time.deltaTime;
            
            // 检查循环
            if (currentPlayTime >= duration)
            {
                if (loop)
                {
                    currentPlayTime = 0f;
                    playCount++;
                }
                else
                {
                    StopPlayback();
                }
            }
        }
        
        // 更新输出
        currentTime = currentPlayTime;
        isPlaying = playbackState == PlaybackState.Playing;
        
        // 处理轨道和关键帧
        ProcessTracks();
        ProcessKeyframes();
        
        // 更新状态
        lastPlayTime = currentPlayTime;
    }
    
    /// <summary>
    /// 初始化时间轴
    /// </summary>
    private void InitializeTimeline()
    {
        // 确保列表已初始化
        if (tracks == null)
        {
            tracks = new List<TimelineTrack>();
        }
        
        if (keyframes == null)
        {
            keyframes = new List<KeyframeData>();
        }
        
        if (tracks.Count == 0)
        {
            // 创建默认轨道
            CreateDefaultTracks();
        }
        
        if (keyframes.Count == 0)
        {
            // 创建默认关键帧
            CreateDefaultKeyframes();
        }
        
        isInitialized = true;
        Debug.Log($"Timeline '{timelineName}' 已初始化，轨道数: {tracks.Count}, 关键帧数: {keyframes.Count}");
    }
    
    /// <summary>
    /// 创建默认轨道
    /// </summary>
    private void CreateDefaultTracks()
    {
        tracks.Add(new TimelineTrack
        {
            trackIndex = 0,
            trackName = "Animation Track",
            trackType = TimelineTrack.TrackType.Animation,
            isEnabled = true,
            color = Color.blue,
            volume = 1f,
            offset = 0f,
            length = duration,
            blendMode = TrackBlendMode.Override,
            weight = 1f,
            priority = TrackPriority.Normal,
            isLocked = false,
            isMuted = false,
            isSolo = false,
            loop = false,
            clips = new List<TimelineClip>(),
            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
        });
        
        tracks.Add(new TimelineTrack
        {
            trackIndex = 1,
            trackName = "Audio Track",
            trackType = TimelineTrack.TrackType.Audio,
            isEnabled = true,
            color = Color.green,
            volume = 1f,
            offset = 0f,
            length = duration,
            blendMode = TrackBlendMode.Override,
            weight = 1f,
            priority = TrackPriority.Normal,
            isLocked = false,
            isMuted = false,
            isSolo = false,
            loop = false,
            clips = new List<TimelineClip>(),
            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
        });
        
        tracks.Add(new TimelineTrack
        {
            trackIndex = 2,
            trackName = "Event Track",
            trackType = TimelineTrack.TrackType.Event,
            isEnabled = true,
            color = Color.yellow,
            volume = 1f,
            offset = 0f,
            length = duration,
            blendMode = TrackBlendMode.Override,
            weight = 1f,
            priority = TrackPriority.Normal,
            isLocked = false,
            isMuted = false,
            isSolo = false,
            loop = false,
            clips = new List<TimelineClip>(),
            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
        });
        
        // 为每个轨道添加默认剪辑
        CreateDefaultClips();
    }
    
    /// <summary>
    /// 创建默认剪辑
    /// </summary>
    private void CreateDefaultClips()
    {
        if (tracks.Count == 0) return;
        
        // 为动画轨道添加默认剪辑
        var animationTrack = tracks[0];
        var animationClip = new TimelineClip
        {
            clipName = "Default Animation",
            startTime = 0f,
            duration = duration * 0.5f,
            clipType = TimelineClip.ClipType.Animation,
            blendMode = TimelineClip.ClipBlendMode.Override,
            weight = 1f,
            offset = 0f,
            scale = 1f,
            loop = false,
            isLocked = false,
            isMuted = false,
            isSolo = false,
            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
            events = new List<ClipEvent>()
        };
        animationTrack.clips.Add(animationClip);
        
        // 为音频轨道添加默认剪辑
        var audioTrack = tracks[1];
        var audioClip = new TimelineClip
        {
            clipName = "Default Audio",
            startTime = 0f,
            duration = duration,
            clipType = TimelineClip.ClipType.Audio,
            blendMode = TimelineClip.ClipBlendMode.Override,
            weight = 1f,
            offset = 0f,
            scale = 1f,
            loop = false,
            isLocked = false,
            isMuted = false,
            isSolo = false,
            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
            events = new List<ClipEvent>()
        };
        audioTrack.clips.Add(audioClip);
        
        // 为事件轨道添加默认剪辑
        var eventTrack = tracks[2];
        var eventClip = new TimelineClip
        {
            clipName = "Default Event",
            startTime = duration * 0.25f,
            duration = duration * 0.25f,
            clipType = TimelineClip.ClipType.Event,
            blendMode = TimelineClip.ClipBlendMode.Override,
            weight = 1f,
            offset = 0f,
            scale = 1f,
            loop = false,
            isLocked = false,
            isMuted = false,
            isSolo = false,
            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
            events = new List<ClipEvent>()
        };
        eventTrack.clips.Add(eventClip);
        
        Debug.Log("已创建默认剪辑");
    }
    
    /// <summary>
    /// 创建默认关键帧
    /// </summary>
    private void CreateDefaultKeyframes()
    {
        // 在开始位置添加关键帧
        keyframes.Add(new KeyframeData
        {
            time = 0f,
            value = 0f,
            trackIndex = 0,
            keyframeType = KeyframeData.KeyframeType.Animation
        });
        
        // 在中间位置添加关键帧
        keyframes.Add(new KeyframeData
        {
            time = duration * 0.5f,
            value = 0.5f,
            trackIndex = 0,
            keyframeType = KeyframeData.KeyframeType.Animation
        });
        
        // 在结束位置添加关键帧
        keyframes.Add(new KeyframeData
        {
            time = duration,
            value = 1f,
            trackIndex = 0,
            keyframeType = KeyframeData.KeyframeType.Animation
        });
    }
    
    /// <summary>
    /// 处理轨道
    /// </summary>
    private void ProcessTracks()
    {
        if (tracks == null || tracks.Count == 0)
        {
            return;
        }
        
        foreach (var track in tracks)
        {
            if (track == null || !track.isEnabled) continue;
            
            switch (track.trackType)
            {
                case TimelineTrack.TrackType.Animation:
                    ProcessAnimationTrack(track);
                    break;
                case TimelineTrack.TrackType.Audio:
                    ProcessAudioTrack(track);
                    break;
                case TimelineTrack.TrackType.Event:
                    ProcessEventTrack(track);
                    break;
            }
        }
    }
    
    /// <summary>
    /// 处理动画轨道
    /// </summary>
    private void ProcessAnimationTrack(TimelineTrack track)
    {
        // 查找当前时间的关键帧
        var currentKeyframe = FindKeyframeAtTime(track.trackIndex, currentPlayTime);
        if (currentKeyframe != null)
        {
            // 应用动画值
            float interpolatedValue = InterpolateKeyframe(currentKeyframe, currentPlayTime);
            // 这里可以应用到实际的动画组件
            Debug.Log($"动画轨道 {track.trackName}: 应用值 {interpolatedValue:F3} 在时间 {currentPlayTime:F2}s");
        }
    }
    
    /// <summary>
    /// 处理音频轨道
    /// </summary>
    private void ProcessAudioTrack(TimelineTrack track)
    {
        // 音频轨道处理逻辑
        // 可以控制音频的播放、暂停、音量等
    }
    
    /// <summary>
    /// 处理事件轨道
    /// </summary>
    private void ProcessEventTrack(TimelineTrack track)
    {
        // 事件轨道处理逻辑
        // 可以触发各种游戏事件
    }
    
    /// <summary>
    /// 处理关键帧
    /// </summary>
    private void ProcessKeyframes()
    {
        // 检查是否需要触发关键帧事件
        if (keyframes == null || keyframes.Count == 0)
        {
            return;
        }
        
        foreach (var keyframe in keyframes)
        {
            if (keyframe != null && Mathf.Abs(currentPlayTime - keyframe.time) < 0.1f)
            {
                TriggerKeyframeEvent(keyframe);
            }
        }
    }
    
    /// <summary>
    /// 在指定时间查找关键帧
    /// </summary>
    private KeyframeData FindKeyframeAtTime(int trackIndex, float time)
    {
        if (keyframes == null || keyframes.Count == 0)
        {
            return null;
        }
        
        return keyframes.Find(k => k != null && k.trackIndex == trackIndex && 
                                   Mathf.Abs(k.time - time) < 0.1f);
    }
    
    /// <summary>
    /// 关键帧插值
    /// </summary>
    private float InterpolateKeyframe(KeyframeData keyframe, float currentTime)
    {
        if (keyframe == null)
        {
            return 0f;
        }
        
        // 这里实现插值逻辑
        // 目前返回关键帧的原始值，后续可以扩展为更复杂的插值算法
        return keyframe.value;
    }
    
    /// <summary>
    /// 触发关键帧事件
    /// </summary>
    private void TriggerKeyframeEvent(KeyframeData keyframe)
    {
        if (keyframe == null)
        {
            Debug.LogWarning("尝试触发空的关键帧事件");
            return;
        }
        
        Debug.Log($"触发关键帧事件: 轨道 {keyframe.trackIndex}, 时间 {keyframe.time:F2}s, 值 {keyframe.value}");
    }
    
    /// <summary>
    /// 播放时间轴
    /// </summary>
    [ContextMenu("播放")]
    public void Play()
    {
        if (playbackState == PlaybackState.Stopped)
        {
            currentPlayTime = 0f;
        }
        
        playbackState = PlaybackState.Playing;
        Debug.Log($"Timeline '{timelineName}' 开始播放");
    }
    
    /// <summary>
    /// 暂停播放
    /// </summary>
    [ContextMenu("暂停")]
    public void Pause()
    {
        playbackState = PlaybackState.Paused;
        Debug.Log($"Timeline '{timelineName}' 已暂停");
    }
    
    /// <summary>
    /// 停止播放
    /// </summary>
    [ContextMenu("停止")]
    public void Stop()
    {
        StopPlayback();
    }
    
    /// <summary>
    /// 停止播放
    /// </summary>
    private void StopPlayback()
    {
        playbackState = PlaybackState.Stopped;
        currentPlayTime = 0f;
        Debug.Log($"Timeline '{timelineName}' 已停止");
    }
    
    /// <summary>
    /// 跳转到指定时间
    /// </summary>
    [ContextMenu("跳转到开始")]
    public void JumpToStart()
    {
        currentPlayTime = 0f;
        Debug.Log($"Timeline '{timelineName}' 跳转到开始位置");
    }
    
    /// <summary>
    /// 跳转到结束
    /// </summary>
    [ContextMenu("跳转到结束")]
    public void JumpToEnd()
    {
        currentPlayTime = duration;
        Debug.Log($"Timeline '{timelineName}' 跳转到结束位置");
    }
    
    /// <summary>
    /// 添加轨道
    /// </summary>
    [ContextMenu("添加轨道")]
    public void AddTrack()
    {
        if (tracks.Count >= maxTracks)
        {
            Debug.LogWarning($"无法添加更多轨道，已达到最大数量: {maxTracks}");
            return;
        }
        
        var newTrack = new TimelineTrack
        {
            trackIndex = tracks.Count,
            trackName = $"Track {tracks.Count + 1}",
            trackType = TimelineTrack.TrackType.Animation,
            isEnabled = true,
            color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value),
            volume = 1f,
            offset = 0f,
            length = duration,
            blendMode = TrackBlendMode.Override,
            weight = 1f,
            priority = TrackPriority.Normal,
            isLocked = false,
            isMuted = false,
            isSolo = false,
            loop = false,
            clips = new List<TimelineClip>(),
            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
        };
        
        tracks.Add(newTrack);
        Debug.Log($"已添加新轨道: {newTrack.trackName}, 索引: {newTrack.trackIndex}");
    }
    
    /// <summary>
    /// 添加关键帧
    /// </summary>
    [ContextMenu("添加关键帧")]
    public void AddKeyframe()
    {
        // 默认添加到第一个轨道
        int targetTrackIndex = 0;
        if (tracks != null && tracks.Count > 0)
        {
            targetTrackIndex = tracks[0].trackIndex;
        }
        
        var newKeyframe = new KeyframeData
        {
            time = currentPlayTime,
            value = UnityEngine.Random.value,
            trackIndex = targetTrackIndex,
            keyframeType = KeyframeData.KeyframeType.Animation,
            easeInTime = 0f,
            easeOutTime = 0f,
            isLocked = false
        };
        
        keyframes.Add(newKeyframe);
        Debug.Log($"已在时间 {currentPlayTime:F2}s 添加关键帧，值: {newKeyframe.value:F2}, 轨道索引: {targetTrackIndex}");
    }
    
    /// <summary>
    /// 添加剪辑
    /// </summary>
    [ContextMenu("添加剪辑")]
    public void AddClip()
    {
        if (tracks == null || tracks.Count == 0)
        {
            Debug.LogWarning("没有可用的轨道，请先添加轨道");
            return;
        }
        
        var targetTrack = tracks[0]; // 默认添加到第一个轨道
        var newClip = new TimelineClip
        {
            clipName = $"Clip {targetTrack.clips.Count + 1}",
            startTime = currentPlayTime,
            duration = 2f,
            clipType = TimelineClip.ClipType.Animation,
            blendMode = TimelineClip.ClipBlendMode.Override,
            weight = 1f,
            offset = 0f,
            scale = 1f,
            loop = false,
            isLocked = false,
            isMuted = false,
            isSolo = false
        };
        
        targetTrack.clips.Add(newClip);
        Debug.Log($"已在轨道 '{targetTrack.trackName}' 添加剪辑: {newClip.clipName}");
    }
    
    /// <summary>
    /// 添加剪辑到指定轨道
    /// </summary>
    /// <param name="trackIndex">轨道索引</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="duration">持续时间</param>
    public void AddClipToTrack(int trackIndex, float startTime, float duration)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Count)
        {
            Debug.LogError($"无效的轨道索引: {trackIndex}");
            return;
        }
        
        var targetTrack = tracks[trackIndex];
        var newClip = new TimelineClip
        {
            clipName = $"Clip {targetTrack.clips.Count + 1}",
            startTime = startTime,
            duration = duration,
            clipType = TimelineClip.ClipType.Animation,
            blendMode = TimelineClip.ClipBlendMode.Override,
            weight = 1f,
            offset = 0f,
            scale = 1f,
            loop = false,
            isLocked = false,
            isMuted = false,
            isSolo = false
        };
        
        targetTrack.clips.Add(newClip);
        Debug.Log($"已在轨道 '{targetTrack.trackName}' 添加剪辑: {newClip.clipName}");
    }
    
    /// <summary>
    /// 删除剪辑
    /// </summary>
    /// <param name="trackIndex">轨道索引</param>
    /// <param name="clipIndex">剪辑索引</param>
    public void DeleteClip(int trackIndex, int clipIndex)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Count)
        {
            Debug.LogError($"无效的轨道索引: {trackIndex}");
            return;
        }
        
        var targetTrack = tracks[trackIndex];
        if (clipIndex < 0 || clipIndex >= targetTrack.clips.Count)
        {
            Debug.LogError($"无效的剪辑索引: {clipIndex}");
            return;
        }
        
        var clipToDelete = targetTrack.clips[clipIndex];
        targetTrack.clips.RemoveAt(clipIndex);
        Debug.Log($"已删除轨道 '{targetTrack.trackName}' 的剪辑: {clipToDelete.clipName}");
    }
    
    /// <summary>
    /// 移动剪辑
    /// </summary>
    /// <param name="trackIndex">轨道索引</param>
    /// <param name="clipIndex">剪辑索引</param>
    /// <param name="newStartTime">新的开始时间</param>
    public void MoveClip(int trackIndex, int clipIndex, float newStartTime)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Count)
        {
            Debug.LogError($"无效的轨道索引: {trackIndex}");
            return;
        }
        
        var targetTrack = tracks[trackIndex];
        if (clipIndex < 0 || clipIndex >= targetTrack.clips.Count)
        {
            Debug.LogError($"无效的剪辑索引: {clipIndex}");
            return;
        }
        
        var clip = targetTrack.clips[clipIndex];
        clip.startTime = Mathf.Max(0f, newStartTime);
        Debug.Log($"已移动剪辑 '{clip.clipName}' 到时间: {clip.startTime:F2}s");
    }
    
    /// <summary>
    /// 调整剪辑持续时间
    /// </summary>
    /// <param name="trackIndex">轨道索引</param>
    /// <param name="clipIndex">剪辑索引</param>
    /// <param name="newDuration">新的持续时间</param>
    public void ResizeClip(int trackIndex, int clipIndex, float newDuration)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Count)
        {
            Debug.LogError($"无效的轨道索引: {trackIndex}");
            return;
        }
        
        var targetTrack = tracks[trackIndex];
        if (clipIndex < 0 || clipIndex >= targetTrack.clips.Count)
        {
            Debug.LogError($"无效的剪辑索引: {clipIndex}");
            return;
        }
        
        var clip = targetTrack.clips[clipIndex];
        clip.duration = Mathf.Max(0.1f, newDuration);
        Debug.Log($"已调整剪辑 '{clip.clipName}' 持续时间为: {clip.duration:F2}s");
    }
    
    /// <summary>
    /// 设置轨道独奏
    /// </summary>
    /// <param name="trackIndex">轨道索引</param>
    /// <param name="solo">是否独奏</param>
    public void SetTrackSolo(int trackIndex, bool solo)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Count)
        {
            Debug.LogError($"无效的轨道索引: {trackIndex}");
            return;
        }
        
        var targetTrack = tracks[trackIndex];
        targetTrack.isSolo = solo;
        
        // 如果设置为独奏，取消其他轨道的独奏
        if (solo)
        {
            foreach (var track in tracks)
            {
                if (track != targetTrack)
                {
                    track.isSolo = false;
                }
            }
        }
        
        Debug.Log($"轨道 '{targetTrack.trackName}' 独奏状态: {solo}");
    }
    
    /// <summary>
    /// 设置轨道静音
    /// </summary>
    /// <param name="trackIndex">轨道索引</param>
    /// <param name="mute">是否静音</param>
    public void SetTrackMute(int trackIndex, bool mute)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Count)
        {
            Debug.LogError($"无效的轨道索引: {trackIndex}");
            return;
        }
        
        var targetTrack = tracks[trackIndex];
        targetTrack.isMuted = mute;
        Debug.Log($"轨道 '{targetTrack.trackName}' 静音状态: {mute}");
    }
    
    /// <summary>
    /// 设置轨道锁定
    /// </summary>
    /// <param name="trackIndex">轨道索引</param>
    /// <param name="locked">是否锁定</param>
    public void SetTrackLock(int trackIndex, bool locked)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Count)
        {
            Debug.LogError($"无效的轨道索引: {trackIndex}");
            return;
        }
        
        var targetTrack = tracks[trackIndex];
        targetTrack.isLocked = locked;
        Debug.Log($"轨道 '{targetTrack.trackName}' 锁定状态: {locked}");
    }
    
    /// <summary>
    /// 获取当前时间的所有剪辑
    /// </summary>
    /// <returns>当前时间的剪辑列表</returns>
    public List<TimelineClip> GetClipsAtCurrentTime()
    {
        var clipsAtTime = new List<TimelineClip>();
        
        if (tracks == null) return clipsAtTime;
        
        foreach (var track in tracks)
        {
            if (!track.isEnabled || track.isMuted) continue;
            
            foreach (var clip in track.clips)
            {
                if (clip.startTime <= currentPlayTime && clip.endTime >= currentPlayTime)
                {
                    clipsAtTime.Add(clip);
                }
            }
        }
        
        return clipsAtTime;
    }
    
    /// <summary>
    /// 获取轨道的有效剪辑
    /// </summary>
    /// <param name="trackIndex">轨道索引</param>
    /// <returns>有效剪辑列表</returns>
    public List<TimelineClip> GetValidClips(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Count)
        {
            return new List<TimelineClip>();
        }
        
        var track = tracks[trackIndex];
        return track.clips.Where(c => c.startTime < duration && c.endTime > 0).ToList();
    }
    
    /// <summary>
    /// 检查时间点是否有剪辑冲突
    /// </summary>
    /// <param name="trackIndex">轨道索引</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="duration">持续时间</param>
    /// <returns>是否有冲突</returns>
    public bool HasClipConflict(int trackIndex, float startTime, float duration)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Count)
        {
            return false;
        }
        
        var track = tracks[trackIndex];
        var endTime = startTime + duration;
        
        foreach (var clip in track.clips)
        {
            if (clip.startTime < endTime && clip.endTime > startTime)
            {
                return true; // 有冲突
            }
        }
        
        return false; // 无冲突
    }
    
    /// <summary>
    /// 重置时间轴
    /// </summary>
    [ContextMenu("重置时间轴")]
    public void ResetTimeline()
    {
        currentPlayTime = 0f;
        playbackState = PlaybackState.Stopped;
        playCount = 0;
        lastPlayTime = 0f;
        Debug.Log($"Timeline '{timelineName}' 已重置");
    }
    
    /// <summary>
    /// 打印时间轴信息
    /// </summary>
    [ContextMenu("打印信息")]
    public void PrintTimelineInfo()
    {
        Debug.Log($"Timeline '{timelineName}' 信息:\n" +
                  $"总长度: {duration:F2}s\n" +
                  $"当前时间: {currentPlayTime:F2}s\n" +
                  $"播放状态: {playbackState}\n" +
                  $"轨道数量: {tracks.Count}\n" +
                  $"关键帧数量: {keyframes.Count}\n" +
                  $"播放次数: {playCount}\n" +
                  $"循环播放: {loop}\n" +
                  $"自动播放: {autoPlay}\n" +
                  $"时间轴缩放: {timelineZoom:F2}\n" +
                  $"时间轴偏移: {timelineOffset:F2}");
    }
    
    /// <summary>
    /// 绘制时间轴
    /// </summary>
    /// <param name="rect">绘制区域</param>
    public void DrawTimeline(Rect rect)
    {
        if (!showTimeMarkers && !showPlayhead) return;
        
        // 绘制时间刻度
        if (showTimeMarkers)
        {
            DrawTimeMarkers(rect);
        }
        
        // 绘制播放头
        if (showPlayhead)
        {
            DrawPlayhead(rect);
        }
        
        // 绘制轨道
        DrawTracks(rect);
        
        // 绘制关键帧
        DrawKeyframes(rect);
    }
    
    /// <summary>
    /// 绘制时间刻度
    /// </summary>
    private void DrawTimeMarkers(Rect rect)
    {
        float startTime = Mathf.Max(0, timelineOffset);
        float endTime = Mathf.Min(duration, timelineOffset + rect.width / timelineZoom);
        
        for (float time = startTime; time <= endTime; time += timeMarkerInterval)
        {
            float x = (time - timelineOffset) * timelineZoom;
            if (x >= 0 && x <= rect.width)
            {
                // 绘制时间刻度线
                Vector3 startPos = new Vector3(rect.x + x, rect.y, 0);
                Vector3 endPos = new Vector3(rect.x + x, rect.y + 20, 0);
                Debug.DrawLine(startPos, endPos, Color.gray, 0.1f);
                
                // 绘制时间标签
                string timeLabel = $"{time:F1}s";
                // 这里可以添加GUI.Label绘制时间文本
            }
        }
    }
    
    /// <summary>
    /// 绘制播放头
    /// </summary>
    private void DrawPlayhead(Rect rect)
    {
        float playheadX = (currentPlayTime - timelineOffset) * timelineZoom;
        if (playheadX >= 0 && playheadX <= rect.width)
        {
            Vector3 startPos = new Vector3(rect.x + playheadX, rect.y, 0);
            Vector3 endPos = new Vector3(rect.x + playheadX, rect.y + rect.height, 0);
            Debug.DrawLine(startPos, endPos, playheadColor, 0.1f);
        }
    }
    
    /// <summary>
    /// 绘制轨道
    /// </summary>
    private void DrawTracks(Rect rect)
    {
        if (tracks == null || tracks.Count == 0) return;
        
        float currentY = rect.y + 30; // 时间刻度下方开始
        
        foreach (var track in tracks)
        {
            if (track == null || !track.isEnabled) continue;
            
            // 绘制轨道背景
            Rect trackRect = new Rect(rect.x, currentY, rect.width, trackHeight);
            // 这里可以添加GUI.Box绘制轨道背景
            
            // 绘制轨道名称
            // GUI.Label(new Rect(rect.x - 100, currentY, 95, trackHeight), track.trackName);
            
            currentY += trackHeight + trackSpacing;
        }
    }
    
    /// <summary>
    /// 绘制关键帧
    /// </summary>
    private void DrawKeyframes(Rect rect)
    {
        if (keyframes == null || keyframes.Count == 0) return;
        
        foreach (var keyframe in keyframes)
        {
            if (keyframe == null) continue;
            
            float keyframeX = (keyframe.time - timelineOffset) * timelineZoom;
            if (keyframeX >= 0 && keyframeX <= rect.width)
            {
                // 计算关键帧在轨道中的Y位置
                int trackIndex = keyframe.trackIndex;
                if (trackIndex < tracks.Count)
                {
                    float keyframeY = rect.y + 30 + trackIndex * (trackHeight + trackSpacing) + trackHeight * 0.5f;
                    
                    // 绘制关键帧
                    Vector3 keyframePos = new Vector3(rect.x + keyframeX, keyframeY, 0);
                    // 这里可以添加GUI.Box绘制关键帧
                    
                    // 绘制关键帧连接线
                    int keyframeIndex = keyframes.IndexOf(keyframe);
                    if (keyframeIndex > 0)
                    {
                        var prevKeyframe = keyframes[keyframeIndex - 1];
                        if (prevKeyframe != null && prevKeyframe.trackIndex == trackIndex)
                        {
                            float prevX = (prevKeyframe.time - timelineOffset) * timelineZoom;
                            Vector3 prevPos = new Vector3(rect.x + prevX, keyframeY, 0);
                            Debug.DrawLine(prevPos, keyframePos, tracks[trackIndex].color, 0.1f);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 时间轴缩放
    /// </summary>
    [ContextMenu("放大时间轴")]
    public void ZoomIn()
    {
        timelineZoom = Mathf.Min(timelineZoom * 1.2f, 5f);
        Debug.Log($"时间轴已放大，当前缩放: {timelineZoom:F2}");
    }
    
    /// <summary>
    /// 时间轴缩小
    /// </summary>
    [ContextMenu("缩小时间轴")]
    public void ZoomOut()
    {
        timelineZoom = Mathf.Max(timelineZoom / 1.2f, 0.1f);
        Debug.Log($"时间轴已缩小，当前缩放: {timelineZoom:F2}");
    }
    
    /// <summary>
    /// 重置时间轴视图
    /// </summary>
    [ContextMenu("重置时间轴视图")]
    public void ResetTimelineView()
    {
        timelineZoom = 1f;
        timelineOffset = 0f;
        Debug.Log("时间轴视图已重置");
    }
    
    /// <summary>
    /// 跳转到指定时间
    /// </summary>
    /// <param name="targetTime">目标时间</param>
    public void JumpToTime(float targetTime)
    {
        currentPlayTime = Mathf.Clamp(targetTime, 0f, duration);
        Debug.Log($"Timeline '{timelineName}' 跳转到时间: {currentPlayTime:F2}s");
    }
    
    /// <summary>
    /// 获取时间轴总高度
    /// </summary>
    public float GetTimelineHeight()
    {
        if (tracks == null) return 0f;
        return 30f + tracks.Count * (trackHeight + trackSpacing);
    }
    
    /// <summary>
    /// 获取时间轴总宽度
    /// </summary>
    public float GetTimelineWidth()
    {
        return duration * timelineZoom;
    }
}

/// <summary>
/// 时间轴轨道数据
/// </summary>
[System.Serializable]
public class TimelineTrack
{
    [Tooltip("轨道索引")]
    public int trackIndex = 0;
    
    [Tooltip("轨道名称")]
    public string trackName = "New Track";
    
    [Tooltip("轨道类型")]
    public TrackType trackType = TrackType.Animation;
    
    [Tooltip("是否启用")]
    public bool isEnabled = true;
    
    [Tooltip("轨道颜色")]
    public Color color = Color.white;
    
    [Tooltip("轨道音量")]
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Tooltip("轨道偏移")]
    public float offset = 0f;
    
    [Tooltip("轨道长度")]
    public float length = 0f;
    
    [Tooltip("轨道混合模式")]
    public TimelineNode.TrackBlendMode blendMode = TimelineNode.TrackBlendMode.Override;
    
    [Tooltip("轨道权重")]
    [Range(0f, 1f)]
    public float weight = 1f;
    
    [Tooltip("轨道优先级")]
    public TimelineNode.TrackPriority priority = TimelineNode.TrackPriority.Normal;
    
    [Tooltip("轨道锁定")]
    public bool isLocked = false;
    
    [Tooltip("轨道静音")]
    public bool isMuted = false;
    
    [Tooltip("轨道独奏")]
    public bool isSolo = false;
    
    [Tooltip("轨道循环")]
    public bool loop = false;
    
    [Tooltip("轨道剪辑列表")]
    public List<TimelineClip> clips = new List<TimelineClip>();
    
    [Tooltip("轨道曲线")]
    public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    
    /// <summary>
    /// 轨道类型枚举
    /// </summary>
    public enum TrackType
    {
        Animation,      // 动画轨道
        Audio,          // 音频轨道
        Event,          // 事件轨道
        Control,        // 控制轨道
        Video,          // 视频轨道
        Cinemachine,    // 相机轨道
        Activation,     // 激活轨道
        Material,       // 材质轨道
        Particle,       // 粒子轨道
        Custom          // 自定义轨道
    }
}

/// <summary>
/// 关键帧数据
/// </summary>
[System.Serializable]
public class KeyframeData
{
    [Tooltip("关键帧时间")]
    public float time = 0f;
    
    [Tooltip("关键帧值")]
    public float value = 0f;
    
    [Tooltip("所属轨道索引")]
    public int trackIndex = 0;
    
    [Tooltip("关键帧类型")]
    public KeyframeType keyframeType = KeyframeType.Animation;
    
    [Tooltip("缓入时间")]
    public float easeInTime = 0f;
    
    [Tooltip("缓出时间")]
    public float easeOutTime = 0f;
    
    [Tooltip("是否锁定")]
    public bool isLocked = false;
    
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
/// Timeline剪辑数据
/// 类似Unity Timeline的TimelineClip
/// </summary>
[System.Serializable]
public class TimelineClip
{
    [Tooltip("剪辑名称")]
    public string clipName = "New Clip";
    
    [Tooltip("剪辑开始时间")]
    public float startTime = 0f;
    
    [Tooltip("剪辑持续时间")]
    public float duration = 1f;
    
    [Tooltip("剪辑结束时间")]
    public float endTime => startTime + duration;
    
    [Tooltip("剪辑类型")]
    public ClipType clipType = ClipType.Animation;
    
    [Tooltip("剪辑资源")]
    public UnityEngine.Object clipAsset;
    
    [Tooltip("剪辑混合模式")]
    public ClipBlendMode blendMode = ClipBlendMode.Override;
    
    [Tooltip("剪辑权重")]
    [Range(0f, 1f)]
    public float weight = 1f;
    
    [Tooltip("剪辑偏移")]
    public float offset = 0f;
    
    [Tooltip("剪辑缩放")]
    public float scale = 1f;
    
    [Tooltip("剪辑循环")]
    public bool loop = false;
    
    [Tooltip("剪辑锁定")]
    public bool isLocked = false;
    
    [Tooltip("剪辑静音")]
    public bool isMuted = false;
    
    [Tooltip("剪辑独奏")]
    public bool isSolo = false;
    
    [Tooltip("剪辑曲线")]
    public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    
    [Tooltip("剪辑事件")]
    public List<ClipEvent> events = new List<ClipEvent>();
    
    /// <summary>
    /// 剪辑类型枚举
    /// </summary>
    public enum ClipType
    {
        Animation,      // 动画剪辑
        Audio,          // 音频剪辑
        Video,          // 视频剪辑
        Event,          // 事件剪辑
        Control,        // 控制剪辑
        Material,       // 材质剪辑
        Particle,       // 粒子剪辑
        Custom          // 自定义剪辑
    }
    
    /// <summary>
    /// 剪辑混合模式枚举
    /// </summary>
    public enum ClipBlendMode
    {
        Override,       // 覆盖
        Additive,       // 叠加
        Multiply,       // 乘法
        Blend           // 混合
    }
}

/// <summary>
/// 剪辑事件数据
/// </summary>
[System.Serializable]
public class ClipEvent
{
    [Tooltip("事件名称")]
    public string eventName = "New Event";
    
    [Tooltip("事件时间")]
    public float time = 0f;
    
    [Tooltip("事件类型")]
    public EventType eventType = EventType.Custom;
    
    [Tooltip("事件参数")]
    public string parameters = "";
    
    [Tooltip("事件是否触发")]
    public bool isTriggered = false;
    
    /// <summary>
    /// 事件类型枚举
    /// </summary>
    public enum EventType
    {
        Custom,         // 自定义事件
        Animation,      // 动画事件
        Audio,          // 音频事件
        Particle,       // 粒子事件
        Material,       // 材质事件
        Activation,     // 激活事件
        Deactivation    // 停用事件
    }
}

/// <summary>
/// 时间轴数据
/// </summary>
[System.Serializable]
public class TimelineData
{
    public string timelineName;
    public float duration;
    public bool loop;
    public List<TimelineTrack> tracks;
    public List<KeyframeData> keyframes;
    public float currentTime;
    public bool isPlaying;
}
