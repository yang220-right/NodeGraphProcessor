using System;
using GraphProcessor;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Timeline帧模式节点
/// 提供简单的播放和暂停功能
/// </summary>
[System.Serializable, NodeMenuItem("OdinNode/TimelineNode")]
public class TimelineNode : BaseNode
{
    [Input(name = "Frame Input")]
    public int frameInput;
    
    [Output(name = "Current Frame")]
    public int currentFrame;
    
    [Output(name = "Is Playing")]
    public bool isPlaying;
    
    [Output(name = "Track Count")]
    public int trackCount;
    
    [Output(name = "Track Values")]
    public float[] trackValues;
    
    public override string name => "Timeline Frame Node";
}
