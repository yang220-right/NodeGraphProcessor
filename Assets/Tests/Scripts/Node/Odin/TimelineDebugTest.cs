using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

/// <summary>
/// Timeline调试测试脚本
/// 用于测试Timeline在编辑器模式下的播放功能
/// </summary>
public class TimelineDebugTest : MonoBehaviour
{
    [Header("Timeline 调试测试")]
    [LabelText("Timeline 数据")]
    public TimelineSO timelineData;
    
    [LabelText("自动测试")]
    public bool autoTest = false;
    
    private void Start()
    {
        if (timelineData == null)
        {
            timelineData = ScriptableObject.CreateInstance<TimelineSO>();
            timelineData.timelineName = "Debug Test Timeline";
            timelineData.totalFrames = 30;
            timelineData.frameRate = 30f;
            timelineData.InitializeFrameData();
        }
    }
    
    private void Update()
    {
        if (timelineData != null && timelineData.isPlaying)
        {
            timelineData.UpdateTimeline();
            Debug.Log($"运行时播放 - 帧: {timelineData.currentFrame}, 时间: {timelineData.playTime:F2}s");
        }
    }
    
    [Button("测试编辑器播放")]
    public void TestEditorPlayback()
    {
        if (timelineData == null)
        {
            Debug.LogError("Timeline数据为空！");
            return;
        }
        
        Debug.Log("开始测试编辑器播放...");
        timelineData.Play();
        
        // 启动编辑器更新
        EditorApplication.update += OnEditorUpdate;
    }
    
    [Button("停止编辑器播放")]
    public void StopEditorPlayback()
    {
        if (timelineData != null)
        {
            timelineData.Pause();
        }
        EditorApplication.update -= OnEditorUpdate;
        Debug.Log("编辑器播放已停止");
    }
    
    private void OnEditorUpdate()
    {
        if (timelineData != null && timelineData.isPlaying)
        {
            timelineData.UpdateTimeline();
            Debug.Log($"编辑器播放 - 帧: {timelineData.currentFrame}, 时间: {timelineData.playTime:F2}s");
        }
    }
    
    private void OnDestroy()
    {
        EditorApplication.update -= OnEditorUpdate;
    }
    
    [Button("创建测试Timeline")]
    public void CreateTestTimeline()
    {
        timelineData = ScriptableObject.CreateInstance<TimelineSO>();
        timelineData.timelineName = "Test Timeline";
        timelineData.totalFrames = 60;
        timelineData.frameRate = 30f;
        timelineData.playbackSpeed = 1f;
        timelineData.loopPlayback = true;
        timelineData.InitializeFrameData();
        
        Debug.Log("测试Timeline已创建");
    }
    
    [Button("打印Timeline状态")]
    public void PrintTimelineStatus()
    {
        if (timelineData != null)
        {
            Debug.Log($"Timeline状态:\n" +
                     $"名称: {timelineData.timelineName}\n" +
                     $"总帧数: {timelineData.totalFrames}\n" +
                     $"当前帧: {timelineData.currentFrame}\n" +
                     $"播放状态: {timelineData.isPlaying}\n" +
                     $"播放时间: {timelineData.playTime:F2}s\n" +
                     $"帧率: {timelineData.frameRate}\n" +
                     $"播放速度: {timelineData.playbackSpeed}");
        }
        else
        {
            Debug.LogWarning("Timeline数据为空");
        }
    }
}
