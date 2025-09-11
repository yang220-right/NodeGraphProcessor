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
    [Header("Timeline 设置")]
    [LabelText("Timeline 名称")]
    [Tooltip("Timeline的名称")]
    public string timelineName = "Simple Timeline";
    
    [LabelText("总帧数")]
    [Tooltip("Timeline的总帧数")]
    [MinValue(1)]
    public int totalFrames = 100;
    
    [LabelText("当前帧")]
    [Tooltip("当前播放的帧")]
    [ReadOnly]
    [ProgressBar(0, 100, ColorMember = "GetFrameColor")]
    public int currentFrame = 0;
    
    [LabelText("播放速度")]
    [Tooltip("播放速度倍数")]
    [Range(0.1f, 5f)]
    public float playbackSpeed = 1f;
    
    [Header("播放控制")]
    [LabelText("是否播放")]
    [Tooltip("Timeline是否正在播放")]
    public bool isPlaying = false;
    
    [LabelText("循环播放")]
    [Tooltip("是否循环播放")]
    public bool loopPlayback = true;
    
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
    /// 播放Timeline
    /// </summary>
    [Button("播放", ButtonSizes.Medium)]
    [PropertyOrder(-1)]
    public void Play()
    {
        isPlaying = true;
        Debug.Log($"Timeline '{timelineName}' 开始播放 - 总帧数: {totalFrames}, 帧率: {frameRate}");
    }
    
    /// <summary>
    /// 暂停Timeline
    /// </summary>
    [Button("暂停", ButtonSizes.Medium)]
    [PropertyOrder(-1)]
    public void Pause()
    {
        isPlaying = false;
        Debug.Log($"Timeline '{timelineName}' 已暂停");
    }
    
    /// <summary>
    /// 停止Timeline
    /// </summary>
    [Button("停止", ButtonSizes.Medium)]
    [PropertyOrder(-1)]
    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        playTime = 0f;
        Debug.Log($"Timeline '{timelineName}' 已停止");
    }
    
    /// <summary>
    /// 重置Timeline
    /// </summary>
    [Button("重置", ButtonSizes.Medium)]
    [PropertyOrder(-1)]
    public void Reset()
    {
        isPlaying = false;
        currentFrame = 0;
        playTime = 0f;
        Debug.Log($"Timeline '{timelineName}' 已重置");
    }
    
    /// <summary>
    /// 跳转到指定帧
    /// </summary>
    [Button("跳转到帧", ButtonSizes.Medium)]
    [PropertyOrder(-1)]
    public void GoToFrame()
    {
        currentFrame = Mathf.Clamp(currentFrame, 0, totalFrames - 1);
        playTime = currentFrame / frameRate;
        Debug.Log($"Timeline '{timelineName}' 跳转到第 {currentFrame} 帧");
    }
    
    /// <summary>
    /// 初始化帧数据
    /// </summary>
    [Button("初始化帧数据", ButtonSizes.Medium)]
    [PropertyOrder(-1)]
    [HideInInspector] // 隐藏初始化按钮
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
    /// 获取Timeline状态信息
    /// </summary>
    [Button("打印状态", ButtonSizes.Medium)]
    [PropertyOrder(-1)]
    [HideInInspector] // 隐藏打印状态按钮
    public void PrintStatus()
    {
        Debug.Log($"Timeline '{timelineName}' 状态:\n" +
                  $"当前帧: {currentFrame}/{totalFrames}\n" +
                  $"播放状态: {(isPlaying ? "播放中" : "已暂停")}\n" +
                  $"播放时间: {playTime:F2}s\n" +
                  $"播放速度: {playbackSpeed}x\n" +
                  $"循环播放: {(loopPlayback ? "是" : "否")}");
    }
}
