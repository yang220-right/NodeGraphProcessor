using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 时间轴数据的ScriptableObject
/// 用于存储和管理时间轴的关键帧数据
/// </summary>
[CreateAssetMenu(fileName = "TimelineSO", menuName = "Timeline/Timeline Data")]
public class TimelineSO : SerializedScriptableObject
{
    [Header("时间轴基本信息")]
    [LabelText("时间轴名称")]
    public string timelineName = "新时间轴";
    
    [LabelText("时间轴描述")]
    [TextArea(3, 5)]
    public string description = "";
    
    [LabelText("总时长")]
    [Range(0.1f, 60f)]
    public float duration = 5f;
    
    [LabelText("值范围")]
    public Vector2 valueRange = new Vector2(-1f, 1f);
    
    [Header("轨道管理")]
    [LabelText("轨道列表")]
    [TableList(AlwaysExpanded = true, ShowIndexLabels = true, DrawScrollView = false)]
    public List<TimelineTrack> tracks = new List<TimelineTrack>();
    
    [Header("关键帧数据")]
    [LabelText("关键帧列表")]
    [TableList(AlwaysExpanded = true, ShowIndexLabels = true, DrawScrollView = false)]
    [HideInInspector] // 隐藏旧的关键帧列表，使用轨道系统
    public List<TimelineKeyframe> keyframes = new List<TimelineKeyframe>();
    
    [Header("时间轴设置")]
    [LabelText("默认插值类型")]
    public InterpolationType defaultInterpolationType = InterpolationType.Linear;
    
    [LabelText("自动排序关键帧")]
    public bool autoSortKeyframes = true;
    
    [LabelText("允许重复时间")]
    public bool allowDuplicateTime = false;
    
    [Header("导出设置")]
    [LabelText("导出采样率")]
    [Range(10, 120)]
    public int exportSampleRate = 60;
    
    [LabelText("导出格式")]
    public ExportFormat exportFormat = ExportFormat.AnimationCurve;
    
    /// <summary>
    /// 导出格式枚举
    /// </summary>
    public enum ExportFormat
    {
        [LabelText("动画曲线")]
        AnimationCurve,
        [LabelText("关键帧数据")]
        KeyframeData,
        [LabelText("采样数据")]
        SampledData
    }
    
    /// <summary>
    /// 初始化时间轴数据
    /// </summary>
    public void Initialize()
    {
        if (tracks == null)
        {
            tracks = new List<TimelineTrack>();
        }
        
        if (keyframes == null)
        {
            keyframes = new List<TimelineKeyframe>();
        }
        
        // 添加默认轨道
        if (tracks.Count == 0)
        {
            AddDefaultTrack();
        }
        
        // 添加默认关键帧（向后兼容）
        if (keyframes.Count == 0)
        {
            AddDefaultKeyframes();
        }
        
        // 自动排序
        if (autoSortKeyframes)
        {
            SortKeyframes();
        }
    }
    
    /// <summary>
    /// 添加默认轨道
    /// </summary>
    private void AddDefaultTrack()
    {
        var defaultTrack = new TimelineTrack("主轨道", TrackType.Float);
        defaultTrack.valueRange = valueRange;
        defaultTrack.defaultValue = (valueRange.x + valueRange.y) * 0.5f;
        
        // 添加默认关键帧
        defaultTrack.AddKeyframe(0f, valueRange.x, defaultInterpolationType, "起始关键帧");
        defaultTrack.AddKeyframe(duration, valueRange.y, defaultInterpolationType, "结束关键帧");
        
        tracks.Add(defaultTrack);
    }
    
    /// <summary>
    /// 添加默认关键帧
    /// </summary>
    private void AddDefaultKeyframes()
    {
        // 起始关键帧
        var startKeyframe = new TimelineKeyframe
        {
            time = 0f,
            value = valueRange.x,
            interpolationType = defaultInterpolationType,
            description = "起始关键帧"
        };
        
        // 结束关键帧
        var endKeyframe = new TimelineKeyframe
        {
            time = duration,
            value = valueRange.y,
            interpolationType = defaultInterpolationType,
            description = "结束关键帧"
        };
        
        keyframes.Add(startKeyframe);
        keyframes.Add(endKeyframe);
    }
    
    /// <summary>
    /// 添加关键帧
    /// </summary>
    /// <param name="time">时间点</param>
    /// <param name="value">值</param>
    /// <param name="interpolationType">插值类型</param>
    /// <param name="description">描述</param>
    public void AddKeyframe(float time, float value, InterpolationType interpolationType = InterpolationType.Linear, string description = "")
    {
        // 检查时间是否重复
        if (!allowDuplicateTime && HasKeyframeAtTime(time))
        {
            Debug.LogWarning($"时间 {time} 已存在关键帧，无法添加重复时间的关键帧");
            return;
        }
        
        var keyframe = new TimelineKeyframe
        {
            time = Mathf.Clamp(time, 0f, duration),
            value = Mathf.Clamp(value, valueRange.x, valueRange.y),
            interpolationType = interpolationType,
            description = description
        };
        
        keyframes.Add(keyframe);
        
        if (autoSortKeyframes)
        {
            SortKeyframes();
        }
        
        Debug.Log($"添加关键帧: Time={time:F2}, Value={value:F2}, Type={interpolationType}");
    }
    
    /// <summary>
    /// 移除关键帧
    /// </summary>
    /// <param name="index">关键帧索引</param>
    public void RemoveKeyframe(int index)
    {
        if (index >= 0 && index < keyframes.Count)
        {
            var removedKeyframe = keyframes[index];
            keyframes.RemoveAt(index);
            Debug.Log($"移除关键帧: Time={removedKeyframe.time:F2}, Value={removedKeyframe.value:F2}");
        }
    }
    
    /// <summary>
    /// 移除指定时间的关键帧
    /// </summary>
    /// <param name="time">时间点</param>
    public void RemoveKeyframeAtTime(float time)
    {
        int index = GetKeyframeIndexAtTime(time);
        if (index >= 0)
        {
            RemoveKeyframe(index);
        }
    }
    
    /// <summary>
    /// 更新关键帧
    /// </summary>
    /// <param name="index">关键帧索引</param>
    /// <param name="time">新时间</param>
    /// <param name="value">新值</param>
    /// <param name="interpolationType">插值类型</param>
    public void UpdateKeyframe(int index, float time, float value, InterpolationType interpolationType = InterpolationType.Linear)
    {
        if (index >= 0 && index < keyframes.Count)
        {
            keyframes[index].time = Mathf.Clamp(time, 0f, duration);
            keyframes[index].value = Mathf.Clamp(value, valueRange.x, valueRange.y);
            keyframes[index].interpolationType = interpolationType;
            
            if (autoSortKeyframes)
            {
                SortKeyframes();
            }
            
            Debug.Log($"更新关键帧: Index={index}, Time={time:F2}, Value={value:F2}");
        }
    }
    
    /// <summary>
    /// 清空所有关键帧
    /// </summary>
    public void ClearKeyframes()
    {
        keyframes.Clear();
        Debug.Log("清空所有关键帧");
    }
    
    /// <summary>
    /// 排序关键帧
    /// </summary>
    public void SortKeyframes()
    {
        keyframes = keyframes.OrderBy(k => k.time).ToList();
    }
    
    /// <summary>
    /// 检查指定时间是否有关键帧
    /// </summary>
    /// <param name="time">时间点</param>
    /// <returns>是否有关键帧</returns>
    public bool HasKeyframeAtTime(float time)
    {
        return keyframes.Any(k => Mathf.Abs(k.time - time) < 0.001f);
    }
    
    /// <summary>
    /// 获取指定时间的关键帧索引
    /// </summary>
    /// <param name="time">时间点</param>
    /// <returns>关键帧索引，-1表示未找到</returns>
    public int GetKeyframeIndexAtTime(float time)
    {
        for (int i = 0; i < keyframes.Count; i++)
        {
            if (Mathf.Abs(keyframes[i].time - time) < 0.001f)
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// 计算指定时间的值
    /// </summary>
    /// <param name="time">时间点</param>
    /// <returns>计算得到的值</returns>
    public float Evaluate(float time)
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
    /// 获取时间轴长度
    /// </summary>
    /// <returns>时间轴长度</returns>
    public float GetTimelineLength()
    {
        if (keyframes == null || keyframes.Count == 0)
        {
            return 0f;
        }
        
        return keyframes.Max(k => k.time);
    }
    
    /// <summary>
    /// 获取关键帧数量
    /// </summary>
    /// <returns>关键帧数量</returns>
    public int GetKeyframeCount()
    {
        return keyframes?.Count ?? 0;
    }
    
    /// <summary>
    /// 导出为动画曲线
    /// </summary>
    /// <returns>动画曲线</returns>
    public AnimationCurve ExportToAnimationCurve()
    {
        var curve = new AnimationCurve();
        
        if (keyframes == null || keyframes.Count == 0)
        {
            return curve;
        }
        
        foreach (var keyframe in keyframes)
        {
            Keyframe unityKeyframe = new Keyframe(keyframe.time, keyframe.value);
            
            // 设置切线
            switch (keyframe.interpolationType)
            {
                case InterpolationType.Linear:
                    unityKeyframe.inTangent = 0f;
                    unityKeyframe.outTangent = 0f;
                    break;
                case InterpolationType.Step:
                    unityKeyframe.inTangent = float.PositiveInfinity;
                    unityKeyframe.outTangent = float.PositiveInfinity;
                    break;
                default:
                    unityKeyframe.inTangent = 0f;
                    unityKeyframe.outTangent = 0f;
                    break;
            }
            
            curve.AddKey(unityKeyframe);
        }
        
        return curve;
    }
    
    /// <summary>
    /// 导出采样数据
    /// </summary>
    /// <returns>采样数据数组</returns>
    public float[] ExportSampledData()
    {
        float sampleInterval = 1f / exportSampleRate;
        int sampleCount = Mathf.CeilToInt(duration * exportSampleRate);
        float[] samples = new float[sampleCount];
        
        for (int i = 0; i < sampleCount; i++)
        {
            float time = i * sampleInterval;
            samples[i] = Evaluate(time);
        }
        
        return samples;
    }
    
    /// <summary>
    /// 验证时间轴数据
    /// </summary>
    /// <returns>验证结果</returns>
    public bool ValidateTimeline()
    {
        if (keyframes == null || keyframes.Count == 0)
        {
            Debug.LogWarning("时间轴没有关键帧");
            return false;
        }
        
        if (duration <= 0f)
        {
            Debug.LogWarning("时间轴时长必须大于0");
            return false;
        }
        
        // 检查关键帧时间范围
        foreach (var keyframe in keyframes)
        {
            if (keyframe.time < 0f || keyframe.time > duration)
            {
                Debug.LogWarning($"关键帧时间 {keyframe.time} 超出范围 [0, {duration}]");
                return false;
            }
            
            if (keyframe.value < valueRange.x || keyframe.value > valueRange.y)
            {
                Debug.LogWarning($"关键帧值 {keyframe.value} 超出范围 [{valueRange.x}, {valueRange.y}]");
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 重置时间轴
    /// </summary>
    public void ResetTimeline()
    {
        ClearKeyframes();
        AddDefaultKeyframes();
        Debug.Log("时间轴已重置");
    }
    
    /// <summary>
    /// 复制时间轴数据
    /// </summary>
    /// <param name="source">源时间轴</param>
    public void CopyFrom(TimelineSO source)
    {
        if (source == null) return;
        
        timelineName = source.timelineName;
        description = source.description;
        duration = source.duration;
        valueRange = source.valueRange;
        defaultInterpolationType = source.defaultInterpolationType;
        autoSortKeyframes = source.autoSortKeyframes;
        allowDuplicateTime = source.allowDuplicateTime;
        exportSampleRate = source.exportSampleRate;
        exportFormat = source.exportFormat;
        
        keyframes = new List<TimelineKeyframe>();
        foreach (var keyframe in source.keyframes)
        {
            keyframes.Add(new TimelineKeyframe
            {
                time = keyframe.time,
                value = keyframe.value,
                interpolationType = keyframe.interpolationType,
                easingCurve = new AnimationCurve(keyframe.easingCurve.keys),
                description = keyframe.description
            });
        }
        
        Debug.Log($"从 {source.name} 复制时间轴数据");
    }
    
    /// <summary>
    /// 获取时间轴统计信息
    /// </summary>
    /// <returns>统计信息字符串</returns>
    public string GetTimelineStats()
    {
        if (tracks == null || tracks.Count == 0)
        {
            return "时间轴为空";
        }
        
        int totalKeyframes = tracks.Sum(t => t.GetKeyframeCount());
        float maxLength = tracks.Max(t => t.GetTrackLength());
        
        return $"轨道数量: {tracks.Count}\n" +
               $"总关键帧: {totalKeyframes}\n" +
               $"时长: {duration:F2}s\n" +
               $"最大轨道长度: {maxLength:F2}s";
    }
    
    #region 轨道管理方法
    
    /// <summary>
    /// 添加轨道
    /// </summary>
    /// <param name="trackName">轨道名称</param>
    /// <param name="trackType">轨道类型</param>
    /// <returns>添加的轨道</returns>
    public TimelineTrack AddTrack(string trackName, TrackType trackType)
    {
        if (tracks == null)
        {
            tracks = new List<TimelineTrack>();
        }
        
        // 检查名称是否重复
        string finalName = trackName;
        int counter = 1;
        while (tracks.Any(t => t.trackName == finalName))
        {
            finalName = $"{trackName} {counter}";
            counter++;
        }
        
        var newTrack = new TimelineTrack(finalName, trackType);
        tracks.Add(newTrack);
        
        Debug.Log($"添加轨道: {finalName}, 类型: {trackType}");
        return newTrack;
    }
    
    /// <summary>
    /// 移除轨道
    /// </summary>
    /// <param name="index">轨道索引</param>
    public void RemoveTrack(int index)
    {
        if (tracks != null && index >= 0 && index < tracks.Count)
        {
            var removedTrack = tracks[index];
            tracks.RemoveAt(index);
            Debug.Log($"移除轨道: {removedTrack.trackName}");
        }
    }
    
    /// <summary>
    /// 移除轨道
    /// </summary>
    /// <param name="track">要移除的轨道</param>
    public void RemoveTrack(TimelineTrack track)
    {
        if (tracks != null && track != null)
        {
            tracks.Remove(track);
            Debug.Log($"移除轨道: {track.trackName}");
        }
    }
    
    /// <summary>
    /// 获取轨道
    /// </summary>
    /// <param name="index">轨道索引</param>
    /// <returns>轨道对象</returns>
    public TimelineTrack GetTrack(int index)
    {
        if (tracks != null && index >= 0 && index < tracks.Count)
        {
            return tracks[index];
        }
        return null;
    }
    
    /// <summary>
    /// 根据名称获取轨道
    /// </summary>
    /// <param name="trackName">轨道名称</param>
    /// <returns>轨道对象</returns>
    public TimelineTrack GetTrackByName(string trackName)
    {
        if (tracks != null)
        {
            return tracks.FirstOrDefault(t => t.trackName == trackName);
        }
        return null;
    }
    
    /// <summary>
    /// 复制轨道
    /// </summary>
    /// <param name="index">轨道索引</param>
    /// <returns>复制的轨道</returns>
    public TimelineTrack DuplicateTrack(int index)
    {
        if (tracks != null && index >= 0 && index < tracks.Count)
        {
            var originalTrack = tracks[index];
            var duplicatedTrack = originalTrack.Clone();
            tracks.Add(duplicatedTrack);
            
            Debug.Log($"复制轨道: {originalTrack.trackName} -> {duplicatedTrack.trackName}");
            return duplicatedTrack;
        }
        return null;
    }
    
    /// <summary>
    /// 移动轨道
    /// </summary>
    /// <param name="fromIndex">源索引</param>
    /// <param name="toIndex">目标索引</param>
    public void MoveTrack(int fromIndex, int toIndex)
    {
        if (tracks != null && fromIndex >= 0 && fromIndex < tracks.Count && toIndex >= 0 && toIndex < tracks.Count)
        {
            var track = tracks[fromIndex];
            tracks.RemoveAt(fromIndex);
            tracks.Insert(toIndex, track);
            
            Debug.Log($"移动轨道: {track.trackName} 从 {fromIndex} 到 {toIndex}");
        }
    }
    
    /// <summary>
    /// 获取轨道数量
    /// </summary>
    /// <returns>轨道数量</returns>
    public int GetTrackCount()
    {
        return tracks?.Count ?? 0;
    }
    
    /// <summary>
    /// 清空所有轨道
    /// </summary>
    public void ClearAllTracks()
    {
        if (tracks != null)
        {
            tracks.Clear();
            Debug.Log("清空所有轨道");
        }
    }
    
    /// <summary>
    /// 计算所有轨道在指定时间的值
    /// </summary>
    /// <param name="time">时间点</param>
    /// <returns>轨道值字典</returns>
    public Dictionary<string, object> EvaluateAllTracks(float time)
    {
        var results = new Dictionary<string, object>();
        
        if (tracks != null)
        {
            foreach (var track in tracks)
            {
                if (track.isEnabled)
                {
                    results[track.trackName] = track.Evaluate(time);
                }
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// 计算指定轨道在指定时间的值
    /// </summary>
    /// <param name="trackName">轨道名称</param>
    /// <param name="time">时间点</param>
    /// <returns>轨道值</returns>
    public object EvaluateTrack(string trackName, float time)
    {
        var track = GetTrackByName(trackName);
        if (track != null && track.isEnabled)
        {
            return track.Evaluate(time);
        }
        return null;
    }
    
    /// <summary>
    /// 验证所有轨道
    /// </summary>
    /// <returns>验证结果</returns>
    public bool ValidateAllTracks()
    {
        if (tracks == null || tracks.Count == 0)
        {
            Debug.LogWarning("没有轨道数据");
            return false;
        }
        
        bool allValid = true;
        foreach (var track in tracks)
        {
            if (!track.ValidateTrack())
            {
                allValid = false;
            }
        }
        
        return allValid;
    }
    
    /// <summary>
    /// 获取轨道统计信息
    /// </summary>
    /// <returns>统计信息字符串</returns>
    public string GetTracksStats()
    {
        if (tracks == null || tracks.Count == 0)
        {
            return "没有轨道数据";
        }
        
        int enabledTracks = tracks.Count(t => t.isEnabled);
        int totalKeyframes = tracks.Sum(t => t.GetKeyframeCount());
        float maxLength = tracks.Max(t => t.GetTrackLength());
        
        return $"轨道总数: {tracks.Count}\n" +
               $"启用轨道: {enabledTracks}\n" +
               $"总关键帧: {totalKeyframes}\n" +
               $"最大长度: {maxLength:F2}s";
    }
    
    #endregion
}
