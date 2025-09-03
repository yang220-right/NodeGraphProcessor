using System;
using System.Collections.Generic;
using GraphProcessor;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;

/// <summary>
/// 时间轴关键帧数据结构
/// </summary>
[System.Serializable]
public class TimelineKeyframe
{
    [LabelText("时间")]
    [Range(0f, 10f)]
    public float time = 0f;
    
    [LabelText("值")]
    public float value = 0f;
    
    [LabelText("插值类型")]
    public InterpolationType interpolationType = InterpolationType.Linear;
    
    [LabelText("缓动曲线")]
    [ShowIf("interpolationType", InterpolationType.EaseInOut)]
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [LabelText("描述")]
    [TextArea(2, 3)]
    public string description = "";
    
    public TimelineKeyframe()
    {
        easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}

/// <summary>
/// 插值类型枚举
/// </summary>
public enum InterpolationType
{
    [LabelText("线性")]
    Linear,
    [LabelText("缓入缓出")]
    EaseInOut,
    [LabelText("步进")]
    Step,
    [LabelText("贝塞尔")]
    Bezier
}

/// <summary>
/// 时间轴播放状态
/// </summary>
public enum TimelinePlayState
{
    [LabelText("停止")]
    Stopped,
    [LabelText("播放")]
    Playing,
    [LabelText("暂停")]
    Paused,
    [LabelText("循环")]
    Looping
}

/// <summary>
/// 时间轴节点
/// 支持关键帧动画、时间轴播放控制和可视化编辑
/// </summary>
[System.Serializable, NodeMenuItem("Timeline/Timeline Node")]
public class TimelineNode : BaseNode
{
    [Input(name = "Trigger")]
    public bool trigger;
    
    [Input(name = "Duration")]
    public float duration = 5f;
    
    [Output(name = "Current Value")]
    public float currentValue;
    
    [Output(name = "Current Time")]
    public float currentTime;
    
    [Output(name = "Is Playing")]
    public bool isPlaying;
    
    [Output(name = "On Complete")]
    public bool onComplete;
    
    public override string name => "时间轴节点";
    
    [Header("时间轴设置")]
    [LabelText("播放状态")]
    [ReadOnly]
    public TimelinePlayState playState = TimelinePlayState.Stopped;
    
    [LabelText("当前时间")]
    [ReadOnly, Range(0f, 10f)]
    public float timelineTime = 0f;
    
    [LabelText("播放速度")]
    [Range(0.1f, 3f)]
    public float playbackSpeed = 1f;
    
    [LabelText("自动播放")]
    public bool autoPlay = false;
    
    [LabelText("循环播放")]
    public bool loopPlayback = false;
    
    [Header("关键帧数据")]
    [LabelText("关键帧列表")]
    [TableList(AlwaysExpanded = true, ShowIndexLabels = true, DrawScrollView = false)]
    public List<TimelineKeyframe> keyframes = new List<TimelineKeyframe>();
    
    [Header("调试信息")]
    [LabelText("调试模式")]
    public bool debugMode = false;
    
    // 私有字段
    private float lastTriggerTime;
    private bool wasTriggered = false;
    private float startTime;
    private bool hasCompleted = false;
    
    protected override void Process()
    {
        // 处理输入
        inputPorts.PullDatas();
        
        // 检查触发条件
        bool currentTrigger = trigger;
        if (currentTrigger && !wasTriggered)
        {
            StartTimeline();
            wasTriggered = true;
        }
        else if (!currentTrigger)
        {
            wasTriggered = false;
        }
        
        // 更新时间轴
        UpdateTimeline();
        
        // 计算当前值
        currentValue = CalculateValueAtTime(timelineTime);
        currentTime = timelineTime;
        isPlaying = playState == TimelinePlayState.Playing || playState == TimelinePlayState.Looping;
        
        // 检查完成状态
        if (timelineTime >= duration && !hasCompleted)
        {
            onComplete = true;
            hasCompleted = true;
            if (loopPlayback)
            {
                playState = TimelinePlayState.Looping;
                timelineTime = 0f;
                hasCompleted = false;
            }
            else
            {
                playState = TimelinePlayState.Stopped;
            }
        }
        else
        {
            onComplete = false;
        }
        
        // 输出数据
        outputPorts.PushDatas();
        
        if (debugMode)
        {
            Debug.Log($"TimelineNode: Time={timelineTime:F2}, Value={currentValue:F2}, State={playState}");
        }
    }
    
    /// <summary>
    /// 开始时间轴播放
    /// </summary>
    public void StartTimeline()
    {
        playState = TimelinePlayState.Playing;
        timelineTime = 0f;
        startTime = Time.time;
        hasCompleted = false;
        onComplete = false;
        
        if (debugMode)
        {
            Debug.Log("时间轴开始播放");
        }
    }
    
    /// <summary>
    /// 暂停时间轴
    /// </summary>
    public void PauseTimeline()
    {
        if (playState == TimelinePlayState.Playing || playState == TimelinePlayState.Looping)
        {
            playState = TimelinePlayState.Paused;
            
            if (debugMode)
            {
                Debug.Log("时间轴已暂停");
            }
        }
    }
    
    /// <summary>
    /// 恢复时间轴播放
    /// </summary>
    public void ResumeTimeline()
    {
        if (playState == TimelinePlayState.Paused)
        {
            playState = TimelinePlayState.Playing;
            startTime = Time.time - timelineTime / playbackSpeed;
            
            if (debugMode)
            {
                Debug.Log("时间轴恢复播放");
            }
        }
    }
    
    /// <summary>
    /// 停止时间轴
    /// </summary>
    public void StopTimeline()
    {
        playState = TimelinePlayState.Stopped;
        timelineTime = 0f;
        hasCompleted = false;
        onComplete = false;
        
        if (debugMode)
        {
            Debug.Log("时间轴已停止");
        }
    }
    
    /// <summary>
    /// 跳转到指定时间
    /// </summary>
    /// <param name="time">目标时间</param>
    public void SeekToTime(float time)
    {
        timelineTime = Mathf.Clamp(time, 0f, duration);
        currentValue = CalculateValueAtTime(timelineTime);
        
        if (debugMode)
        {
            Debug.Log($"时间轴跳转到: {timelineTime:F2}");
        }
    }
    
    /// <summary>
    /// 更新时间轴
    /// </summary>
    private void UpdateTimeline()
    {
        if (playState == TimelinePlayState.Playing || playState == TimelinePlayState.Looping)
        {
            float deltaTime = (Time.time - startTime) * playbackSpeed;
            timelineTime = deltaTime;
            
            if (timelineTime >= duration)
            {
                if (loopPlayback)
                {
                    timelineTime = 0f;
                    startTime = Time.time;
                    playState = TimelinePlayState.Looping;
                }
                else
                {
                    timelineTime = duration;
                    playState = TimelinePlayState.Stopped;
                }
            }
        }
    }
    
    /// <summary>
    /// 计算指定时间的值
    /// </summary>
    /// <param name="time">时间点</param>
    /// <returns>计算得到的值</returns>
    private float CalculateValueAtTime(float time)
    {
        if (keyframes == null || keyframes.Count == 0)
        {
            return 0f;
        }
        
        // 如果时间小于第一个关键帧，返回第一个关键帧的值
        if (time <= keyframes[0].time)
        {
            return keyframes[0].value;
        }
        
        // 如果时间大于最后一个关键帧，返回最后一个关键帧的值
        if (time >= keyframes[keyframes.Count - 1].time)
        {
            return keyframes[keyframes.Count - 1].value;
        }
        
        // 找到时间点前后的关键帧
        for (int i = 0; i < keyframes.Count - 1; i++)
        {
            var currentKeyframe = keyframes[i];
            var nextKeyframe = keyframes[i + 1];
            
            if (time >= currentKeyframe.time && time <= nextKeyframe.time)
            {
                return InterpolateBetweenKeyframes(currentKeyframe, nextKeyframe, time);
            }
        }
        
        return 0f;
    }
    
    /// <summary>
    /// 在两个关键帧之间插值
    /// </summary>
    /// <param name="from">起始关键帧</param>
    /// <param name="to">结束关键帧</param>
    /// <param name="time">当前时间</param>
    /// <returns>插值结果</returns>
    private float InterpolateBetweenKeyframes(TimelineKeyframe from, TimelineKeyframe to, float time)
    {
        float t = (time - from.time) / (to.time - from.time);
        t = Mathf.Clamp01(t);
        
        switch (from.interpolationType)
        {
            case InterpolationType.Linear:
                return Mathf.Lerp(from.value, to.value, t);
                
            case InterpolationType.EaseInOut:
                float easedT = from.easingCurve.Evaluate(t);
                return Mathf.Lerp(from.value, to.value, easedT);
                
            case InterpolationType.Step:
                return t < 1f ? from.value : to.value;
                
            case InterpolationType.Bezier:
                // 简单的贝塞尔曲线实现
                float bezierT = t * t * (3f - 2f * t); // 平滑步进
                return Mathf.Lerp(from.value, to.value, bezierT);
                
            default:
                return Mathf.Lerp(from.value, to.value, t);
        }
    }
    
    /// <summary>
    /// 添加关键帧
    /// </summary>
    /// <param name="time">时间点</param>
    /// <param name="value">值</param>
    public void AddKeyframe(float time, float value)
    {
        var keyframe = new TimelineKeyframe
        {
            time = time,
            value = value
        };
        
        keyframes.Add(keyframe);
        keyframes = keyframes.OrderBy(k => k.time).ToList();
        
        if (debugMode)
        {
            Debug.Log($"添加关键帧: Time={time:F2}, Value={value:F2}");
        }
    }
    
    /// <summary>
    /// 移除关键帧
    /// </summary>
    /// <param name="index">关键帧索引</param>
    public void RemoveKeyframe(int index)
    {
        if (index >= 0 && index < keyframes.Count)
        {
            keyframes.RemoveAt(index);
            
            if (debugMode)
            {
                Debug.Log($"移除关键帧: Index={index}");
            }
        }
    }
    
    /// <summary>
    /// 清空所有关键帧
    /// </summary>
    public void ClearKeyframes()
    {
        keyframes.Clear();
        
        if (debugMode)
        {
            Debug.Log("清空所有关键帧");
        }
    }
    
    /// <summary>
    /// 获取时间轴总长度
    /// </summary>
    public float GetTimelineLength()
    {
        if (keyframes == null || keyframes.Count == 0)
        {
            return 0f;
        }
        
        return keyframes.Max(k => k.time);
    }
    
    /// <summary>
    /// 获取当前播放进度（0-1）
    /// </summary>
    public float GetPlaybackProgress()
    {
        if (duration <= 0f)
        {
            return 0f;
        }
        
        return Mathf.Clamp01(timelineTime / duration);
    }
}
