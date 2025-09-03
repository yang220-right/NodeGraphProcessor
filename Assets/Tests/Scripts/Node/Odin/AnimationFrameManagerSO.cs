using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 动画帧管理器的ScriptableObject
/// 用于存储和管理动画序列数据
/// </summary>
[CreateAssetMenu(fileName = "AnimationFrameManager", menuName = "Animation/Frame Manager")]
public class AnimationFrameManagerSO : SerializedScriptableObject
{
    [Header("管理器基本信息")]
    [LabelText("管理器名称")]
    public string managerName = "动画帧管理器";
    
    [LabelText("管理器描述")]
    [TextArea(3, 5)]
    public string description = "";
    
    [LabelText("版本号")]
    public string version = "1.0.0";
    
    [Header("动画序列管理")]
    [LabelText("动画序列列表")]
    [TableList(AlwaysExpanded = true, ShowIndexLabels = true, DrawScrollView = false)]
    public List<AnimationSequence> sequences = new List<AnimationSequence>();
    
    [Header("全局设置")]
    [LabelText("默认帧率")]
    [Range(1, 120)]
    public int defaultFrameRate = 30;
    
    [LabelText("默认总帧数")]
    [Range(1, 1000)]
    public int defaultTotalFrames = 60;
    
    [LabelText("自动保存")]
    public bool autoSave = true;
    
    [LabelText("自动排序")]
    public bool autoSort = true;
    
    [Header("导出设置")]
    [LabelText("导出格式")]
    public ExportFormat exportFormat = ExportFormat.AnimationClip;
    
    [LabelText("导出路径")]
    [FolderPath]
    public string exportPath = "Assets/ExportedAnimations";
    
    /// <summary>
    /// 导出格式枚举
    /// </summary>
    public enum ExportFormat
    {
        [LabelText("动画剪辑")]
        AnimationClip,
        [LabelText("JSON数据")]
        JsonData,
        [LabelText("CSV数据")]
        CsvData,
        [LabelText("二进制数据")]
        BinaryData
    }
    
    /// <summary>
    /// 初始化管理器
    /// </summary>
    public void Initialize()
    {
        if (sequences == null)
        {
            sequences = new List<AnimationSequence>();
        }
        
        // 添加默认序列
        if (sequences.Count == 0)
        {
            AddDefaultSequence();
        }
        
        Debug.Log($"动画帧管理器 {managerName} 已初始化");
    }
    
    /// <summary>
    /// 添加默认序列
    /// </summary>
    private void AddDefaultSequence()
    {
        var defaultSequence = new AnimationSequence("默认序列", defaultTotalFrames, defaultFrameRate);
        sequences.Add(defaultSequence);
    }
    
    #region 序列管理方法
    
    /// <summary>
    /// 添加动画序列
    /// </summary>
    /// <param name="sequenceName">序列名称</param>
    /// <param name="totalFrames">总帧数</param>
    /// <param name="frameRate">帧率</param>
    /// <returns>添加的序列</returns>
    public AnimationSequence AddSequence(string sequenceName, int totalFrames = -1, int frameRate = -1)
    {
        if (sequences == null)
        {
            sequences = new List<AnimationSequence>();
        }
        
        // 使用默认值
        if (totalFrames <= 0) totalFrames = defaultTotalFrames;
        if (frameRate <= 0) frameRate = defaultFrameRate;
        
        // 检查名称是否重复
        string finalName = sequenceName;
        int counter = 1;
        while (sequences.Any(s => s.sequenceName == finalName))
        {
            finalName = $"{sequenceName} {counter}";
            counter++;
        }
        
        var newSequence = new AnimationSequence(finalName, totalFrames, frameRate);
        sequences.Add(newSequence);
        
        Debug.Log($"添加动画序列: {finalName}");
        return newSequence;
    }
    
    /// <summary>
    /// 移除动画序列
    /// </summary>
    /// <param name="index">序列索引</param>
    public void RemoveSequence(int index)
    {
        if (sequences != null && index >= 0 && index < sequences.Count)
        {
            var removedSequence = sequences[index];
            sequences.RemoveAt(index);
            Debug.Log($"移除动画序列: {removedSequence.sequenceName}");
        }
    }
    
    /// <summary>
    /// 移除动画序列
    /// </summary>
    /// <param name="sequence">要移除的序列</param>
    public void RemoveSequence(AnimationSequence sequence)
    {
        if (sequences != null && sequence != null)
        {
            sequences.Remove(sequence);
            Debug.Log($"移除动画序列: {sequence.sequenceName}");
        }
    }
    
    /// <summary>
    /// 获取动画序列
    /// </summary>
    /// <param name="index">序列索引</param>
    /// <returns>序列对象</returns>
    public AnimationSequence GetSequence(int index)
    {
        if (sequences != null && index >= 0 && index < sequences.Count)
        {
            return sequences[index];
        }
        return null;
    }
    
    /// <summary>
    /// 根据名称获取动画序列
    /// </summary>
    /// <param name="sequenceName">序列名称</param>
    /// <returns>序列对象</returns>
    public AnimationSequence GetSequenceByName(string sequenceName)
    {
        if (sequences != null)
        {
            return sequences.FirstOrDefault(s => s.sequenceName == sequenceName);
        }
        return null;
    }
    
    /// <summary>
    /// 复制动画序列
    /// </summary>
    /// <param name="index">序列索引</param>
    /// <returns>复制的序列</returns>
    public AnimationSequence DuplicateSequence(int index)
    {
        if (sequences != null && index >= 0 && index < sequences.Count)
        {
            var originalSequence = sequences[index];
            var duplicatedSequence = originalSequence.Clone();
            sequences.Add(duplicatedSequence);
            
            Debug.Log($"复制动画序列: {originalSequence.sequenceName} -> {duplicatedSequence.sequenceName}");
            return duplicatedSequence;
        }
        return null;
    }
    
    /// <summary>
    /// 移动动画序列
    /// </summary>
    /// <param name="fromIndex">源索引</param>
    /// <param name="toIndex">目标索引</param>
    public void MoveSequence(int fromIndex, int toIndex)
    {
        if (sequences != null && fromIndex >= 0 && fromIndex < sequences.Count && toIndex >= 0 && toIndex < sequences.Count)
        {
            var sequence = sequences[fromIndex];
            sequences.RemoveAt(fromIndex);
            sequences.Insert(toIndex, sequence);
            
            Debug.Log($"移动动画序列: {sequence.sequenceName} 从 {fromIndex} 到 {toIndex}");
        }
    }
    
    /// <summary>
    /// 获取序列数量
    /// </summary>
    /// <returns>序列数量</returns>
    public int GetSequenceCount()
    {
        return sequences?.Count ?? 0;
    }
    
    /// <summary>
    /// 清空所有序列
    /// </summary>
    public void ClearAllSequences()
    {
        if (sequences != null)
        {
            sequences.Clear();
            Debug.Log("清空所有动画序列");
        }
    }
    
    #endregion
    
    #region 关键帧管理方法
    
    /// <summary>
    /// 添加关键帧到指定序列
    /// </summary>
    /// <param name="sequenceIndex">序列索引</param>
    /// <param name="keyframe">关键帧</param>
    public void AddKeyframeToSequence(int sequenceIndex, AnimationKeyframe keyframe)
    {
        var sequence = GetSequence(sequenceIndex);
        if (sequence != null && keyframe != null)
        {
            sequence.AddKeyframe(keyframe);
        }
    }
    
    /// <summary>
    /// 从指定序列移除关键帧
    /// </summary>
    /// <param name="sequenceIndex">序列索引</param>
    /// <param name="keyframeIndex">关键帧索引</param>
    public void RemoveKeyframeFromSequence(int sequenceIndex, int keyframeIndex)
    {
        var sequence = GetSequence(sequenceIndex);
        if (sequence != null)
        {
            sequence.RemoveKeyframe(keyframeIndex);
        }
    }
    
    /// <summary>
    /// 更新指定序列的关键帧
    /// </summary>
    /// <param name="sequenceIndex">序列索引</param>
    /// <param name="keyframeIndex">关键帧索引</param>
    /// <param name="keyframe">新关键帧</param>
    public void UpdateKeyframeInSequence(int sequenceIndex, int keyframeIndex, AnimationKeyframe keyframe)
    {
        var sequence = GetSequence(sequenceIndex);
        if (sequence != null && keyframe != null)
        {
            sequence.UpdateKeyframe(keyframeIndex, keyframe);
        }
    }
    
    /// <summary>
    /// 获取所有关键帧
    /// </summary>
    /// <returns>所有关键帧列表</returns>
    public List<AnimationKeyframe> GetAllKeyframes()
    {
        var allKeyframes = new List<AnimationKeyframe>();
        
        if (sequences != null)
        {
            foreach (var sequence in sequences)
            {
                if (sequence.keyframes != null)
                {
                    allKeyframes.AddRange(sequence.keyframes);
                }
            }
        }
        
        return allKeyframes;
    }
    
    /// <summary>
    /// 获取指定属性的所有关键帧
    /// </summary>
    /// <param name="propertyType">属性类型</param>
    /// <returns>关键帧列表</returns>
    public List<AnimationKeyframe> GetKeyframesByProperty(AnimationPropertyType propertyType)
    {
        var keyframes = new List<AnimationKeyframe>();
        
        if (sequences != null)
        {
            foreach (var sequence in sequences)
            {
                if (sequence.keyframes != null)
                {
                    keyframes.AddRange(sequence.keyframes.Where(k => k.propertyType == propertyType));
                }
            }
        }
        
        return keyframes;
    }
    
    #endregion
    
    #region 动画计算和播放
    
    /// <summary>
    /// 计算指定序列在指定时间的属性值
    /// </summary>
    /// <param name="sequenceIndex">序列索引</param>
    /// <param name="time">时间点</param>
    /// <param name="propertyType">属性类型</param>
    /// <returns>计算得到的值</returns>
    public object EvaluateSequence(int sequenceIndex, float time, AnimationPropertyType propertyType)
    {
        var sequence = GetSequence(sequenceIndex);
        if (sequence != null)
        {
            return sequence.Evaluate(time, propertyType);
        }
        return null;
    }
    
    /// <summary>
    /// 计算指定序列在指定时间的属性值
    /// </summary>
    /// <param name="sequenceName">序列名称</param>
    /// <param name="time">时间点</param>
    /// <param name="propertyType">属性类型</param>
    /// <returns>计算得到的值</returns>
    public object EvaluateSequence(string sequenceName, float time, AnimationPropertyType propertyType)
    {
        var sequence = GetSequenceByName(sequenceName);
        if (sequence != null)
        {
            return sequence.Evaluate(time, propertyType);
        }
        return null;
    }
    
    /// <summary>
    /// 计算所有序列在指定时间的属性值
    /// </summary>
    /// <param name="time">时间点</param>
    /// <param name="propertyType">属性类型</param>
    /// <returns>序列值字典</returns>
    public Dictionary<string, object> EvaluateAllSequences(float time, AnimationPropertyType propertyType)
    {
        var results = new Dictionary<string, object>();
        
        if (sequences != null)
        {
            foreach (var sequence in sequences)
            {
                if (sequence.isEnabled)
                {
                    results[sequence.sequenceName] = sequence.Evaluate(time, propertyType);
                }
            }
        }
        
        return results;
    }
    
    #endregion
    
    #region 数据验证和统计
    
    /// <summary>
    /// 验证所有序列数据
    /// </summary>
    /// <returns>验证结果</returns>
    public bool ValidateAllSequences()
    {
        if (sequences == null || sequences.Count == 0)
        {
            Debug.LogWarning("没有动画序列数据");
            return false;
        }
        
        bool allValid = true;
        foreach (var sequence in sequences)
        {
            if (!sequence.ValidateSequence())
            {
                allValid = false;
            }
        }
        
        return allValid;
    }
    
    /// <summary>
    /// 获取管理器统计信息
    /// </summary>
    /// <returns>统计信息字符串</returns>
    public string GetManagerStats()
    {
        if (sequences == null || sequences.Count == 0)
        {
            return "没有动画序列数据";
        }
        
        int totalKeyframes = sequences.Sum(s => s.GetKeyframeCount());
        float totalDuration = sequences.Sum(s => s.GetDuration());
        
        return $"管理器: {managerName}\n" +
               $"序列数量: {sequences.Count}\n" +
               $"总关键帧: {totalKeyframes}\n" +
               $"总时长: {totalDuration:F2}s\n" +
               $"版本: {version}";
    }
    
    /// <summary>
    /// 获取序列统计信息
    /// </summary>
    /// <returns>统计信息字符串</returns>
    public string GetSequencesStats()
    {
        if (sequences == null || sequences.Count == 0)
        {
            return "没有动画序列数据";
        }
        
        int enabledSequences = sequences.Count(s => s.isEnabled);
        int totalKeyframes = sequences.Sum(s => s.GetKeyframeCount());
        float maxDuration = sequences.Max(s => s.GetDuration());
        
        return $"序列总数: {sequences.Count}\n" +
               $"启用序列: {enabledSequences}\n" +
               $"总关键帧: {totalKeyframes}\n" +
               $"最大时长: {maxDuration:F2}s";
    }
    
    #endregion
    
    #region 导入导出功能
    
    /// <summary>
    /// 导出动画数据
    /// </summary>
    /// <param name="sequenceIndex">序列索引，-1表示导出所有序列</param>
    public void ExportAnimationData(int sequenceIndex = -1)
    {
        try
        {
            switch (exportFormat)
            {
                case ExportFormat.AnimationClip:
                    ExportToAnimationClip(sequenceIndex);
                    break;
                case ExportFormat.JsonData:
                    ExportToJson(sequenceIndex);
                    break;
                case ExportFormat.CsvData:
                    ExportToCsv(sequenceIndex);
                    break;
                case ExportFormat.BinaryData:
                    ExportToBinary(sequenceIndex);
                    break;
            }
            
            Debug.Log($"动画数据导出成功: {exportFormat}");
        }
        catch (Exception e)
        {
            Debug.LogError($"动画数据导出失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 导出为动画剪辑
    /// </summary>
    private void ExportToAnimationClip(int sequenceIndex)
    {
        // 这里可以实现导出为Unity AnimationClip的逻辑
        Debug.Log("导出为动画剪辑功能待实现");
    }
    
    /// <summary>
    /// 导出为JSON数据
    /// </summary>
    private void ExportToJson(int sequenceIndex)
    {
        // 这里可以实现导出为JSON的逻辑
        Debug.Log("导出为JSON数据功能待实现");
    }
    
    /// <summary>
    /// 导出为CSV数据
    /// </summary>
    private void ExportToCsv(int sequenceIndex)
    {
        // 这里可以实现导出为CSV的逻辑
        Debug.Log("导出为CSV数据功能待实现");
    }
    
    /// <summary>
    /// 导出为二进制数据
    /// </summary>
    private void ExportToBinary(int sequenceIndex)
    {
        // 这里可以实现导出为二进制的逻辑
        Debug.Log("导出为二进制数据功能待实现");
    }
    
    /// <summary>
    /// 导入动画数据
    /// </summary>
    /// <param name="data">导入的数据</param>
    public void ImportAnimationData(string data)
    {
        try
        {
            // 这里可以实现导入逻辑
            Debug.Log("导入动画数据功能待实现");
        }
        catch (Exception e)
        {
            Debug.LogError($"动画数据导入失败: {e.Message}");
        }
    }
    
    #endregion
    
    #region 工具方法
    
    /// <summary>
    /// 重置管理器
    /// </summary>
    public void ResetManager()
    {
        ClearAllSequences();
        AddDefaultSequence();
        Debug.Log("动画帧管理器已重置");
    }
    
    /// <summary>
    /// 复制管理器数据
    /// </summary>
    /// <param name="source">源管理器</param>
    public void CopyFrom(AnimationFrameManagerSO source)
    {
        if (source == null) return;
        
        managerName = source.managerName + " (副本)";
        description = source.description;
        version = source.version;
        defaultFrameRate = source.defaultFrameRate;
        defaultTotalFrames = source.defaultTotalFrames;
        autoSave = source.autoSave;
        autoSort = source.autoSort;
        exportFormat = source.exportFormat;
        exportPath = source.exportPath;
        
        sequences = new List<AnimationSequence>();
        foreach (var sequence in source.sequences)
        {
            sequences.Add(sequence.Clone());
        }
        
        Debug.Log($"从 {source.name} 复制动画帧管理器数据");
    }
    
    /// <summary>
    /// 优化数据
    /// </summary>
    public void OptimizeData()
    {
        if (sequences != null)
        {
            foreach (var sequence in sequences)
            {
                if (sequence.autoSort)
                {
                    sequence.SortKeyframes();
                }
            }
        }
        
        Debug.Log("动画数据已优化");
    }
    
    #endregion
}
