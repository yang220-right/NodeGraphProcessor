using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 动画属性类型枚举
/// </summary>
public enum AnimationPropertyType
{
    [LabelText("位置")]
    Position,
    [LabelText("旋转")]
    Rotation,
    [LabelText("缩放")]
    Scale,
    [LabelText("颜色")]
    Color,
    [LabelText("透明度")]
    Alpha,
    [LabelText("材质属性")]
    MaterialProperty,
    [LabelText("自定义属性")]
    CustomProperty
}

/// <summary>
/// 动画插值类型
/// </summary>
public enum AnimationInterpolationType
{
    [LabelText("线性")]
    Linear,
    [LabelText("缓入")]
    EaseIn,
    [LabelText("缓出")]
    EaseOut,
    [LabelText("缓入缓出")]
    EaseInOut,
    [LabelText("弹性")]
    Elastic,
    [LabelText("弹跳")]
    Bounce,
    [LabelText("步进")]
    Step
}

/// <summary>
/// 动画播放模式
/// </summary>
public enum AnimationPlayMode
{
    [LabelText("播放一次")]
    Once,
    [LabelText("循环播放")]
    Loop,
    [LabelText("来回播放")]
    PingPong,
    [LabelText("反向播放")]
    Reverse
}

/// <summary>
/// 动画关键帧数据结构
/// </summary>
[System.Serializable]
public class AnimationKeyframe
{
    [Header("关键帧基本信息")]
    [LabelText("帧号")]
    [Range(0, 1000)]
    public int frameNumber = 0;
    
    [LabelText("时间")]
    [Range(0f, 10f)]
    public float time = 0f;
    
    [LabelText("插值类型")]
    public AnimationInterpolationType interpolationType = AnimationInterpolationType.Linear;
    
    [LabelText("缓动曲线")]
    [ShowIf("interpolationType", AnimationInterpolationType.EaseInOut)]
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("属性值")]
    [LabelText("位置")]
    [ShowIf("propertyType", AnimationPropertyType.Position)]
    public Vector3 position = Vector3.zero;
    
    [LabelText("旋转")]
    [ShowIf("propertyType", AnimationPropertyType.Rotation)]
    public Vector3 rotation = Vector3.zero;
    
    [LabelText("缩放")]
    [ShowIf("propertyType", AnimationPropertyType.Scale)]
    public Vector3 scale = Vector3.one;
    
    [LabelText("颜色")]
    [ShowIf("propertyType", AnimationPropertyType.Color)]
    public Color color = Color.white;
    
    [LabelText("透明度")]
    [ShowIf("propertyType", AnimationPropertyType.Alpha)]
    [Range(0f, 1f)]
    public float alpha = 1f;
    
    [LabelText("数值")]
    [ShowIf("propertyType", AnimationPropertyType.CustomProperty)]
    public float value = 0f;
    
    [Header("关键帧设置")]
    [LabelText("属性类型")]
    public AnimationPropertyType propertyType = AnimationPropertyType.Position;
    
    [LabelText("描述")]
    [TextArea(2, 3)]
    public string description = "";
    
    [LabelText("是否锁定")]
    public bool isLocked = false;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public AnimationKeyframe()
    {
        easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="frame">帧号</param>
    /// <param name="time">时间</param>
    /// <param name="propertyType">属性类型</param>
    public AnimationKeyframe(int frame, float time, AnimationPropertyType propertyType)
    {
        this.frameNumber = frame;
        this.time = time;
        this.propertyType = propertyType;
        this.interpolationType = AnimationInterpolationType.Linear;
        this.easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
    
    /// <summary>
    /// 获取属性值
    /// </summary>
    /// <returns>属性值</returns>
    public object GetPropertyValue()
    {
        switch (propertyType)
        {
            case AnimationPropertyType.Position:
                return position;
            case AnimationPropertyType.Rotation:
                return rotation;
            case AnimationPropertyType.Scale:
                return scale;
            case AnimationPropertyType.Color:
                return color;
            case AnimationPropertyType.Alpha:
                return alpha;
            case AnimationPropertyType.CustomProperty:
                return value;
            default:
                return Vector3.zero;
        }
    }
    
    /// <summary>
    /// 设置属性值
    /// </summary>
    /// <param name="value">属性值</param>
    public void SetPropertyValue(object value)
    {
        switch (propertyType)
        {
            case AnimationPropertyType.Position:
                if (value is Vector3 v3Pos) position = v3Pos;
                break;
            case AnimationPropertyType.Rotation:
                if (value is Vector3 v3Rot) rotation = v3Rot;
                break;
            case AnimationPropertyType.Scale:
                if (value is Vector3 v3Scale) scale = v3Scale;
                break;
            case AnimationPropertyType.Color:
                if (value is Color col) color = col;
                break;
            case AnimationPropertyType.Alpha:
                if (value is float f) alpha = f;
                break;
            case AnimationPropertyType.CustomProperty:
                if (value is float val) this.value = val;
                break;
        }
    }
    
    /// <summary>
    /// 复制关键帧
    /// </summary>
    /// <returns>复制的关键帧</returns>
    public AnimationKeyframe Clone()
    {
        var clone = new AnimationKeyframe
        {
            frameNumber = frameNumber,
            time = time,
            interpolationType = interpolationType,
            easingCurve = new AnimationCurve(easingCurve.keys),
            position = position,
            rotation = rotation,
            scale = scale,
            color = color,
            alpha = alpha,
            value = value,
            propertyType = propertyType,
            description = description,
            isLocked = isLocked
        };
        
        return clone;
    }
}

/// <summary>
/// 动画序列数据结构
/// </summary>
[System.Serializable]
public class AnimationSequence
{
    [Header("序列基本信息")]
    [LabelText("序列名称")]
    public string sequenceName = "新动画序列";
    
    [LabelText("序列描述")]
    [TextArea(3, 5)]
    public string description = "";
    
    [LabelText("总帧数")]
    [Range(1, 1000)]
    public int totalFrames = 60;
    
    [LabelText("帧率")]
    [Range(1, 120)]
    public int frameRate = 30;
    
    [LabelText("播放模式")]
    public AnimationPlayMode playMode = AnimationPlayMode.Once;
    
    [LabelText("是否启用")]
    public bool isEnabled = true;
    
    [Header("关键帧数据")]
    [LabelText("关键帧列表")]
    [TableList(AlwaysExpanded = true, ShowIndexLabels = true, DrawScrollView = false)]
    public List<AnimationKeyframe> keyframes = new List<AnimationKeyframe>();
    
    [Header("序列设置")]
    [LabelText("自动排序")]
    public bool autoSort = true;
    
    [LabelText("循环次数")]
    [Range(1, 100)]
    public int loopCount = 1;
    
    [LabelText("延迟时间")]
    [Range(0f, 5f)]
    public float delayTime = 0f;
    
    // 私有字段
    private float lastEvaluatedTime = -1f;
    private object lastEvaluatedValue = null;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public AnimationSequence()
    {
        keyframes = new List<AnimationKeyframe>();
    }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="name">序列名称</param>
    /// <param name="frames">总帧数</param>
    /// <param name="frameRate">帧率</param>
    public AnimationSequence(string name, int frames, int frameRate)
    {
        sequenceName = name;
        totalFrames = frames;
        this.frameRate = frameRate;
        keyframes = new List<AnimationKeyframe>();
    }
    
    /// <summary>
    /// 获取序列总时长
    /// </summary>
    /// <returns>总时长（秒）</returns>
    public float GetDuration()
    {
        return (float)totalFrames / frameRate;
    }
    
    /// <summary>
    /// 添加关键帧
    /// </summary>
    /// <param name="keyframe">关键帧</param>
    public void AddKeyframe(AnimationKeyframe keyframe)
    {
        if (keyframe == null) return;
        
        keyframes.Add(keyframe);
        
        if (autoSort)
        {
            SortKeyframes();
        }
        
        Debug.Log($"序列 {sequenceName} 添加关键帧: Frame={keyframe.frameNumber}, Time={keyframe.time:F2}");
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
            Debug.Log($"序列 {sequenceName} 移除关键帧: Frame={removedKeyframe.frameNumber}");
        }
    }
    
    /// <summary>
    /// 更新关键帧
    /// </summary>
    /// <param name="index">关键帧索引</param>
    /// <param name="keyframe">新关键帧</param>
    public void UpdateKeyframe(int index, AnimationKeyframe keyframe)
    {
        if (index >= 0 && index < keyframes.Count && keyframe != null)
        {
            keyframes[index] = keyframe;
            
            if (autoSort)
            {
                SortKeyframes();
            }
            
            Debug.Log($"序列 {sequenceName} 更新关键帧: Frame={keyframe.frameNumber}");
        }
    }
    
    /// <summary>
    /// 排序关键帧
    /// </summary>
    public void SortKeyframes()
    {
        keyframes = keyframes.OrderBy(k => k.frameNumber).ToList();
    }
    
    /// <summary>
    /// 计算指定时间的属性值
    /// </summary>
    /// <param name="time">时间点</param>
    /// <param name="propertyType">属性类型</param>
    /// <returns>计算得到的值</returns>
    public object Evaluate(float time, AnimationPropertyType propertyType)
    {
        if (!isEnabled || keyframes == null || keyframes.Count == 0)
        {
            return GetDefaultValue(propertyType);
        }
        
        // 缓存优化
        if (Mathf.Abs(time - lastEvaluatedTime) < 0.001f && lastEvaluatedValue != null)
        {
            return lastEvaluatedValue;
        }
        
        // 过滤指定属性类型的关键帧
        var propertyKeyframes = keyframes.Where(k => k.propertyType == propertyType).ToList();
        
        if (propertyKeyframes.Count == 0)
        {
            return GetDefaultValue(propertyType);
        }
        
        // 如果时间小于第一个关键帧，返回第一个关键帧的值
        if (time <= propertyKeyframes[0].time)
        {
            return propertyKeyframes[0].GetPropertyValue();
        }
        
        // 如果时间大于最后一个关键帧，返回最后一个关键帧的值
        if (time >= propertyKeyframes[propertyKeyframes.Count - 1].time)
        {
            return propertyKeyframes[propertyKeyframes.Count - 1].GetPropertyValue();
        }
        
        // 找到时间点前后的关键帧
        for (int i = 0; i < propertyKeyframes.Count - 1; i++)
        {
            var currentKeyframe = propertyKeyframes[i];
            var nextKeyframe = propertyKeyframes[i + 1];
            
            if (time >= currentKeyframe.time && time <= nextKeyframe.time)
            {
                return InterpolateBetweenKeyframes(currentKeyframe, nextKeyframe, time, propertyType);
            }
        }
        
        return GetDefaultValue(propertyType);
    }
    
    /// <summary>
    /// 在两个关键帧之间插值
    /// </summary>
    /// <param name="from">起始关键帧</param>
    /// <param name="to">结束关键帧</param>
    /// <param name="time">当前时间</param>
    /// <param name="propertyType">属性类型</param>
    /// <returns>插值结果</returns>
    private object InterpolateBetweenKeyframes(AnimationKeyframe from, AnimationKeyframe to, float time, AnimationPropertyType propertyType)
    {
        float t = (time - from.time) / (to.time - from.time);
        t = Mathf.Clamp01(t);
        
        // 应用插值类型
        float interpolatedT = ApplyInterpolationType(t, from.interpolationType, from.easingCurve);
        
        switch (propertyType)
        {
            case AnimationPropertyType.Position:
                return Vector3.Lerp(from.position, to.position, interpolatedT);
                
            case AnimationPropertyType.Rotation:
                return Vector3.Lerp(from.rotation, to.rotation, interpolatedT);
                
            case AnimationPropertyType.Scale:
                return Vector3.Lerp(from.scale, to.scale, interpolatedT);
                
            case AnimationPropertyType.Color:
                return Color.Lerp(from.color, to.color, interpolatedT);
                
            case AnimationPropertyType.Alpha:
                return Mathf.Lerp(from.alpha, to.alpha, interpolatedT);
                
            case AnimationPropertyType.CustomProperty:
                return Mathf.Lerp(from.value, to.value, interpolatedT);
                
            default:
                return Vector3.zero;
        }
    }
    
    /// <summary>
    /// 应用插值类型
    /// </summary>
    /// <param name="t">原始插值值</param>
    /// <param name="interpolationType">插值类型</param>
    /// <param name="easingCurve">缓动曲线</param>
    /// <returns>应用插值后的值</returns>
    private float ApplyInterpolationType(float t, AnimationInterpolationType interpolationType, AnimationCurve easingCurve)
    {
        switch (interpolationType)
        {
            case AnimationInterpolationType.Linear:
                return t;
                
            case AnimationInterpolationType.EaseIn:
                return t * t;
                
            case AnimationInterpolationType.EaseOut:
                return 1f - (1f - t) * (1f - t);
                
            case AnimationInterpolationType.EaseInOut:
                return easingCurve.Evaluate(t);
                
            case AnimationInterpolationType.Elastic:
                return ElasticEase(t);
                
            case AnimationInterpolationType.Bounce:
                return BounceEase(t);
                
            case AnimationInterpolationType.Step:
                return t < 1f ? 0f : 1f;
                
            default:
                return t;
        }
    }
    
    /// <summary>
    /// 弹性缓动
    /// </summary>
    private float ElasticEase(float t)
    {
        if (t == 0f || t == 1f) return t;
        
        float p = 0.3f;
        float s = p / 4f;
        
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f;
    }
    
    /// <summary>
    /// 弹跳缓动
    /// </summary>
    private float BounceEase(float t)
    {
        if (t < 1f / 2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2f / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }
    
    /// <summary>
    /// 获取默认值
    /// </summary>
    /// <param name="propertyType">属性类型</param>
    /// <returns>默认值</returns>
    private object GetDefaultValue(AnimationPropertyType propertyType)
    {
        switch (propertyType)
        {
            case AnimationPropertyType.Position:
                return Vector3.zero;
            case AnimationPropertyType.Rotation:
                return Vector3.zero;
            case AnimationPropertyType.Scale:
                return Vector3.one;
            case AnimationPropertyType.Color:
                return Color.white;
            case AnimationPropertyType.Alpha:
                return 1f;
            case AnimationPropertyType.CustomProperty:
                return 0f;
            default:
                return Vector3.zero;
        }
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
    /// 获取指定属性的关键帧数量
    /// </summary>
    /// <param name="propertyType">属性类型</param>
    /// <returns>关键帧数量</returns>
    public int GetKeyframeCount(AnimationPropertyType propertyType)
    {
        if (keyframes == null) return 0;
        return keyframes.Count(k => k.propertyType == propertyType);
    }
    
    /// <summary>
    /// 清空所有关键帧
    /// </summary>
    public void ClearKeyframes()
    {
        keyframes.Clear();
        Debug.Log($"序列 {sequenceName} 清空所有关键帧");
    }
    
    /// <summary>
    /// 复制序列
    /// </summary>
    /// <returns>复制的序列</returns>
    public AnimationSequence Clone()
    {
        var clone = new AnimationSequence
        {
            sequenceName = sequenceName + " (副本)",
            description = description,
            totalFrames = totalFrames,
            frameRate = frameRate,
            playMode = playMode,
            isEnabled = isEnabled,
            autoSort = autoSort,
            loopCount = loopCount,
            delayTime = delayTime
        };
        
        // 复制关键帧
        clone.keyframes = new List<AnimationKeyframe>();
        foreach (var keyframe in keyframes)
        {
            clone.keyframes.Add(keyframe.Clone());
        }
        
        return clone;
    }
    
    /// <summary>
    /// 验证序列数据
    /// </summary>
    /// <returns>验证结果</returns>
    public bool ValidateSequence()
    {
        if (string.IsNullOrEmpty(sequenceName))
        {
            Debug.LogWarning("序列名称不能为空");
            return false;
        }
        
        if (totalFrames <= 0)
        {
            Debug.LogWarning($"序列 {sequenceName} 总帧数必须大于0");
            return false;
        }
        
        if (frameRate <= 0)
        {
            Debug.LogWarning($"序列 {sequenceName} 帧率必须大于0");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取序列统计信息
    /// </summary>
    /// <returns>统计信息字符串</returns>
    public string GetSequenceStats()
    {
        if (keyframes == null || keyframes.Count == 0)
        {
            return $"序列: {sequenceName}\n总帧数: {totalFrames}\n帧率: {frameRate}\n时长: {GetDuration():F2}s\n关键帧: 0";
        }
        
        return $"序列: {sequenceName}\n" +
               $"总帧数: {totalFrames}\n" +
               $"帧率: {frameRate}\n" +
               $"时长: {GetDuration():F2}s\n" +
               $"关键帧: {keyframes.Count}\n" +
               $"播放模式: {playMode}\n" +
               $"状态: {(isEnabled ? "启用" : "禁用")}";
    }
}
