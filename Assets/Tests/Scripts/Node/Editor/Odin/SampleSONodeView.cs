using System;
using System.Collections.Generic;
using GraphProcessor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Examples;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// SO Inspector节点
/// 用于演示在NodeView中显示ScriptableObject的所有属性
/// </summary>
[System.Serializable, NodeMenuItem("OdinNode/SampleSONode")]
public class SampleSONode : BaseNode
{
  [Input(name = "frame")]
  public int input;
  [Output(name = "SO Data")]
  public ScriptableObject soData;
  public override string name => "Base Odin Node";
}

/// <summary>
/// SONode的NodeView实现
/// 继承自BaseSONodeView，专门用于显示SONode的所有属性
/// </summary>
[NodeCustomEditor(typeof(SampleSONode))]
public class SampleSONodeView : BaseSONodeView{
  protected override void SetWidth(){
    style.width = 450f;
  }
  /// <summary>
  /// 创建目标ScriptableObject
  /// 这里我们创建一个包装了SONode数据的SO对象
  /// </summary>
  /// <returns>包装了SONode数据的SO对象</returns>
  protected override ScriptableObject CreateSO(){
    return CreateInstance<SampleSO>();
  }
}

public class SampleSO : SerializedScriptableObject{
  
  #region list

  [TableList(AlwaysExpanded = true, DrawScrollView = false)] //一直可拓展 不可折叠
  public List<SomeCustomClass> AlwaysExpandedTable = new List<SomeCustomClass>(){
    new SomeCustomClass(),
    new SomeCustomClass(),
  };

  [Serializable]
  public class SomeCustomClass{
    
    [TableColumnWidth(57, Resizable = false)] [PreviewField(Alignment = ObjectFieldAlignment.Center)]
    public Texture Icon;

    [TextArea] public string Description;

    [VerticalGroup("Combined Column"), LabelWidth(22)]
    public string A, B, C;

    [TableColumnWidth(60)]
    [Button, VerticalGroup("Actions")]
    public void Test1(){
    }

    [TableColumnWidth(60)]
    [Button, VerticalGroup("Actions")]
    public void Test2(){
    }

    [OnInspectorInit]
    private void CreateData(){
      Description = ExampleHelper.GetString();
      Icon = ExampleHelper.GetTexture();
    }
  }

  [AssetSelector] public Material ScriptableObjectsFromMultipleFolders;

  #endregion

  [Header("节点设置")] [Tooltip("节点名称")] public string nodeName = "SO Inspector Node";
  [Tooltip("是否启用节点")] public bool isEnabled = true;
  [Header("数据处理")] [Tooltip("处理倍数")] [Range(1f, 10f)] public float multiplier = 2f;
  [Tooltip("偏移值")] public int offset = 10;
  [Header("配置选项")] [Tooltip("配置模式")] public ConfigMode configMode = ConfigMode.Normal;

  /// <summary>
  /// 配置模式枚举
  /// </summary>
  public enum ConfigMode{
    Normal,
    Advanced,
    Expert,
    Custom
  }

  /// <summary>
  /// 随机化所有数值
  /// </summary>
  [ContextMenu("随机化数值")]
  public void RandomizeValues(){
    multiplier = Random.Range(1f, 10f);
    offset = Random.Range(-20, 21);
    configMode = (ConfigMode)Random.Range(0, 4);

    Debug.Log("SONodeWrapper 数值已随机化");
  }

  /// <summary>
  /// 打印当前状态
  /// </summary>
  [ContextMenu("打印状态")]
  public void PrintStatus(){
    Debug.Log($"SONodeWrapper 状态:\n" +
              $"节点名称: {nodeName}\n" +
              $"启用状态: {isEnabled}\n" +
              $"配置模式: {configMode}\n" +
              $"倍数: {multiplier}\n" +
              $"偏移: {offset}\n");
  }
}