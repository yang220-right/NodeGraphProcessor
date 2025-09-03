using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 轨道类型枚举
/// </summary>
public enum TrackType
{
    [LabelText("数值轨道")]
    Float,
    [LabelText("向量轨道")]
    Vector3,
    [LabelText("颜色轨道")]
    Color,
    [LabelText("布尔轨道")]
    Boolean,
    [LabelText("事件轨道")]
    Event,
    [LabelText("音频轨道")]
    Audio,
    [LabelText("动画轨道")]
    Animation
}

/// <summary>
/// 轨道混合模式
/// </summary>
public enum TrackBlendMode
{
    [LabelText("覆盖")]
    Override,
    [LabelText("相加")]
    Additive,
    [LabelText("相乘")]
    Multiplicative,
    [LabelText("插值")]
    Interpolate
}

/// <summary>
/// 时间轴轨道数据结构
/// </summary>
[System.Serializable]
public class TimelineTrack
{
    [Header("轨道基本信息")]
    [LabelText("轨道名称")]
    public string trackName = "新轨道";
    
    [LabelText("轨道类型")]
    public TrackType trackType = TrackType.Float;
    
    [LabelText("轨道描述")]
    [TextArea(2, 3)]
    public string description = "";
    
    [LabelText("是否启用")]
    public bool isEnabled = true;
    
    [LabelText("是否锁定")]
    public bool isLocked = false;
    
    [Header("轨道设置")]
    [LabelText("混合模式")]
    public TrackBlendMode blendMode = TrackBlendMode.Override;
    
    [LabelText("权重")]
    [Range(0f, 1f)]
    public float weight = 1f;
    
    [LabelText("偏移时间")]
    public float timeOffset = 0f;
    
    [LabelText("时间缩放")]
    [Range(0.1f, 3f)]
    public float timeScale = 1f;
    
    [Header("关键帧数据")]
    [LabelText("关键帧列表")]
    [TableList(AlwaysExpanded = true, ShowIndexLabels = true, DrawScrollView = false)]
    public List<TimelineKeyframe> keyframes = new List<TimelineKeyframe>();
    
    [Header("轨道颜色")]
    [LabelText("轨道颜色")]
    public Color trackColor = Color.white;
    
    [Header("轨道范围")]
    [LabelText("值范围")]
    [ShowIf("trackType", TrackType.Float)]
    public Vector2 valueRange = new Vector2(-1f, 1f);
    
    [LabelText("默认值")]
    [ShowIf("trackType", TrackType.Float)]
    public float defaultValue = 0f;
    
    [LabelText("默认向量")]
    [ShowIf("trackType", TrackType.Vector3)]
    public Vector3 defaultVector = Vector3.zero;
    
    [LabelText("默认颜色")]
    [ShowIf("trackType", TrackType.Color)]
    public Color defaultColor = Color.white;
    
    [LabelText("默认布尔值")]
    [ShowIf("trackType", TrackType.Boolean)]
    public bool defaultBoolean = false;
    
    // 私有字段
    private float lastEvaluatedTime = -1f;
    private object lastEvaluatedValue = null;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public TimelineTrack()
    {
        trackName = "新轨道";
        trackType = TrackType.Float;
        isEnabled = true;
        blendMode = TrackBlendMode.Override;
        weight = 1f;
        timeOffset = 0f;
        timeScale = 1f;
        trackColor = Color.white;
        keyframes = new List<TimelineKeyframe>();
    }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="name">轨道名称</param>
    /// <param name="type">轨道类型</param>
    public TimelineTrack(string name, TrackType type)
    {
        trackName = name;
        trackType = type;
        isEnabled = true;
        blendMode = TrackBlendMode.Override;
        weight = 1f;
        timeOffset = 0f;
        timeScale = 1f;
        trackColor = GetDefaultTrackColor(type);
        keyframes = new List<TimelineKeyframe>();
    }
    
    /// <summary>
    /// 获取默认轨道颜色
    /// </summary>
    /// <param name="type">轨道类型</param>
    /// <returns>默认颜色</returns>
    public static Color GetDefaultTrackColor(TrackType type)
    {
        switch (type)
        {
            case TrackType.Float:
                return new Color(0.2f, 0.8f, 1f, 1f); // 蓝色
            case TrackType.Vector3:
                return new Color(1f, 0.6f, 0.2f, 1f); // 橙色
            case TrackType.Color:
                return new Color(1f, 0.2f, 0.8f, 1f); // 粉色
            case TrackType.Boolean:
                return new Color(0.2f, 1f, 0.2f, 1f); // 绿色
            case TrackType.Event:
                return new Color(1f, 1f, 0.2f, 1f); // 黄色
            case TrackType.Audio:
                return new Color(0.8f, 0.2f, 1f, 1f); // 紫色
            case TrackType.Animation:
                return new Color(1f, 0.4f, 0.4f, 1f); // 红色
            default:
                return Color.white;
        }
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
        var keyframe = new TimelineKeyframe
        {
            time = time,
            value = value,
            interpolationType = interpolationType,
            description = description
        };
        
        keyframes.Add(keyframe);
        SortKeyframes();
        
        Debug.Log($"轨道 {trackName} 添加关键帧: Time={time:F2}, Value={value:F2}");
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
            Debug.Log($"轨道 {trackName} 移除关键帧: Time={removedKeyframe.time:F2}, Value={removedKeyframe.value:F2}");
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
            keyframes[index].time = time;
            keyframes[index].value = value;
            keyframes[index].interpolationType = interpolationType;
            
            SortKeyframes();
            Debug.Log($"轨道 {trackName} 更新关键帧: Index={index}, Time={time:F2}, Value={value:F2}");
        }
    }
    
    /// <summary>
    /// 清空所有关键帧
    /// </summary>
    public void ClearKeyframes()
    {
        keyframes.Clear();
        Debug.Log($"轨道 {trackName} 清空所有关键帧");
    }
    
    /// <summary>
    /// 排序关键帧
    /// </summary>
    public void SortKeyframes()
    {
        keyframes = keyframes.OrderBy(k => k.time).ToList();
    }
    
    /// <summary>
    /// 计算指定时间的值
    /// </summary>
    /// <param name="time">时间点</param>
    /// <returns>计算得到的值</returns>
    public object Evaluate(float time)
    {
        if (!isEnabled)
        {
            return GetDefaultValue();
        }
        
        // 应用时间偏移和缩放
        float adjustedTime = (time + timeOffset) * timeScale;
        
        // 缓存优化
        if (Mathf.Abs(adjustedTime - lastEvaluatedTime) < 0.001f && lastEvaluatedValue != null)
        {
            return lastEvaluatedValue;
        }
        
        float floatValue = EvaluateFloat(adjustedTime);
        object result = ConvertToTrackType(floatValue);
        
        lastEvaluatedTime = adjustedTime;
        lastEvaluatedValue = result;
        
        return result;
    }
    
    /// <summary>
    /// 计算浮点值
    /// </summary>
    /// <param name="time">时间点</param>
    /// <returns>浮点值</returns>
    private float EvaluateFloat(float time)
    {
        if (keyframes == null || keyframes.Count == 0)
        {
            return defaultValue;
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
        
        return defaultValue;
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
                float bezierT = t * t * (3f - 2f * t);
                return Mathf.Lerp(from.value, to.value, bezierT);
                
            default:
                return Mathf.Lerp(from.value, to.value, t);
        }
    }
    
    /// <summary>
    /// 转换为轨道类型
    /// </summary>
    /// <param name="floatValue">浮点值</param>
    /// <returns>转换后的值</returns>
    private object ConvertToTrackType(float floatValue)
    {
        switch (trackType)
        {
            case TrackType.Float:
                return Mathf.Clamp(floatValue, valueRange.x, valueRange.y);
                
            case TrackType.Vector3:
                return new Vector3(floatValue, floatValue, floatValue);
                
            case TrackType.Color:
                return new Color(floatValue, floatValue, floatValue, 1f);
                
            case TrackType.Boolean:
                return floatValue > 0.5f;
                
            case TrackType.Event:
                return floatValue > 0.5f;
                
            case TrackType.Audio:
                return Mathf.Clamp(floatValue, 0f, 1f);
                
            case TrackType.Animation:
                return Mathf.Clamp(floatValue, 0f, 1f);
                
            default:
                return floatValue;
        }
    }
    
    /// <summary>
    /// 获取默认值
    /// </summary>
    /// <returns>默认值</returns>
    public object GetDefaultValue()
    {
        switch (trackType)
        {
            case TrackType.Float:
                return defaultValue;
            case TrackType.Vector3:
                return defaultVector;
            case TrackType.Color:
                return defaultColor;
            case TrackType.Boolean:
                return defaultBoolean;
            case TrackType.Event:
                return false;
            case TrackType.Audio:
                return 0f;
            case TrackType.Animation:
                return 0f;
            default:
                return 0f;
        }
    }
    
    /// <summary>
    /// 获取轨道长度
    /// </summary>
    /// <returns>轨道长度</returns>
    public float GetTrackLength()
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
    /// 检查指定时间是否有关键帧
    /// </summary>
    /// <param name="time">时间点</param>
    /// <returns>是否有关键帧</returns>
    public bool HasKeyframeAtTime(float time)
    {
        if (keyframes == null) return false;
        return keyframes.Any(k => Mathf.Abs(k.time - time) < 0.001f);
    }
    
    /// <summary>
    /// 获取指定时间的关键帧索引
    /// </summary>
    /// <param name="time">时间点</param>
    /// <returns>关键帧索引，-1表示未找到</returns>
    public int GetKeyframeIndexAtTime(float time)
    {
        if (keyframes == null) return -1;
        
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
    /// 复制轨道
    /// </summary>
    /// <returns>复制的轨道</returns>
    public TimelineTrack Clone()
    {
        var clonedTrack = new TimelineTrack
        {
            trackName = trackName + " (副本)",
            trackType = trackType,
            description = description,
            isEnabled = isEnabled,
            isLocked = isLocked,
            blendMode = blendMode,
            weight = weight,
            timeOffset = timeOffset,
            timeScale = timeScale,
            trackColor = trackColor,
            valueRange = valueRange,
            defaultValue = defaultValue,
            defaultVector = defaultVector,
            defaultColor = defaultColor,
            defaultBoolean = defaultBoolean
        };
        
        // 复制关键帧
        clonedTrack.keyframes = new List<TimelineKeyframe>();
        foreach (var keyframe in keyframes)
        {
            clonedTrack.keyframes.Add(new TimelineKeyframe
            {
                time = keyframe.time,
                value = keyframe.value,
                interpolationType = keyframe.interpolationType,
                easingCurve = new AnimationCurve(keyframe.easingCurve.keys),
                description = keyframe.description
            });
        }
        
        return clonedTrack;
    }
    
    /// <summary>
    /// 验证轨道数据
    /// </summary>
    /// <returns>验证结果</returns>
    public bool ValidateTrack()
    {
        if (string.IsNullOrEmpty(trackName))
        {
            Debug.LogWarning($"轨道名称不能为空");
            return false;
        }
        
        if (weight < 0f || weight > 1f)
        {
            Debug.LogWarning($"轨道 {trackName} 权重必须在 [0, 1] 范围内");
            return false;
        }
        
        if (timeScale <= 0f)
        {
            Debug.LogWarning($"轨道 {trackName} 时间缩放必须大于0");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取轨道统计信息
    /// </summary>
    /// <returns>统计信息字符串</returns>
    public string GetTrackStats()
    {
        if (keyframes == null || keyframes.Count == 0)
        {
            return $"轨道: {trackName}\n类型: {trackType}\n关键帧: 0\n状态: {(isEnabled ? "启用" : "禁用")}";
        }
        
        float minValue = keyframes.Min(k => k.value);
        float maxValue = keyframes.Max(k => k.value);
        float avgValue = keyframes.Average(k => k.value);
        
        return $"轨道: {trackName}\n" +
               $"类型: {trackType}\n" +
               $"关键帧: {keyframes.Count}\n" +
               $"时长: {GetTrackLength():F2}s\n" +
               $"值范围: [{minValue:F2}, {maxValue:F2}]\n" +
               $"平均值: {avgValue:F2}\n" +
               $"状态: {(isEnabled ? "启用" : "禁用")}\n" +
               $"权重: {weight:F2}";
    }
}
