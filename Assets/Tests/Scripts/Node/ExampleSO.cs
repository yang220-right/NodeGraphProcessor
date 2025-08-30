using UnityEngine;
using System;
using Sirenix.OdinInspector;

/// <summary>
/// 示例ScriptableObject类
/// 用于演示SOInspectorWrapper的功能
/// </summary>
[CreateAssetMenu(fileName = "ExampleSO", menuName = "Examples/ExampleSO")]
public class ExampleSO : SerializedScriptableObject
{
    [Header("基础设置")]
    [Tooltip("示例名称")]
    public string exampleName = "默认名称";
    
    [Tooltip("示例描述")]
    [TextArea(3, 5)]
    public string description = "这是一个示例描述";
    
    [Header("数值设置")]
    [Range(0f, 100f)]
    [Tooltip("数值范围")]
    public float rangeValue = 50f;
    
    [Min(0)]
    [Tooltip("整数值")]
    public int intValue = 10;
    
    [Header("布尔设置")]
    [Tooltip("是否启用")]
    public bool isEnabled = true;
    
    [Tooltip("是否可见")]
    public bool isVisible = false;
    
    [Header("枚举设置")]
    [Tooltip("示例枚举")]
    public ExampleEnum exampleEnum = ExampleEnum.Option1;
    
    [Tooltip("状态枚举")]
    public StatusEnum statusEnum = StatusEnum.Idle;
    
    [Header("向量设置")]
    [Tooltip("位置向量")]
    public Vector3 position = Vector3.zero;
    
    [Tooltip("旋转向量")]
    public Vector3 rotation = Vector3.zero;
    
    [Tooltip("缩放向量")]
    public Vector3 scale = Vector3.one;
    
    [Header("颜色设置")]
    [Tooltip("主颜色")]
    public Color mainColor = Color.white;
    
    [Tooltip("辅助颜色")]
    public Color secondaryColor = Color.black;
    
    [Header("数组设置")]
    [Tooltip("字符串数组")]
    public string[] stringArray = new string[] { "项目1", "项目2", "项目3" };
    
    [Tooltip("数值数组")]
    public float[] floatArray = new float[] { 1f, 2f, 3f, 4f, 5f };
    
    [Header("引用设置")]
    [Tooltip("游戏对象引用")]
    public GameObject gameObjectRef;
    
    [Tooltip("材质引用")]
    public Material materialRef;
    
    [Tooltip("纹理引用")]
    public Texture textureRef;
    
    [Header("自定义结构")]
    [Tooltip("自定义数据")]
    public CustomData customData = new CustomData();
    
    [Tooltip("自定义数据数组")]
    public CustomData[] customDataArray = new CustomData[3];
    
    /// <summary>
    /// 示例枚举
    /// </summary>
    public enum ExampleEnum
    {
        Option1,
        Option2,
        Option3,
        Option4,
        Option5
    }
    
    /// <summary>
    /// 状态枚举
    /// </summary>
    public enum StatusEnum
    {
        Idle,
        Active,
        Paused,
        Completed,
        Error
    }
    
    /// <summary>
    /// 自定义数据结构
    /// </summary>
    [Serializable]
    public struct CustomData
    {
        [Tooltip("数据名称")]
        public string dataName;
        
        [Tooltip("数据值")]
        public float dataValue;
        
        [Tooltip("是否有效")]
        public bool isValid;
        
        [Tooltip("数据颜色")]
        public Color dataColor;
        
        public CustomData(string name, float value, bool valid, Color color)
        {
            dataName = name;
            dataValue = value;
            isValid = valid;
            dataColor = color;
        }
    }
    
    /// <summary>
    /// 重置所有值为默认值
    /// </summary>
    [ContextMenu("重置为默认值")]
    public void ResetToDefaults()
    {
        exampleName = "默认名称";
        description = "这是一个示例描述";
        rangeValue = 50f;
        intValue = 10;
        isEnabled = true;
        isVisible = false;
        exampleEnum = ExampleEnum.Option1;
        statusEnum = StatusEnum.Idle;
        position = Vector3.zero;
        rotation = Vector3.zero;
        scale = Vector3.one;
        mainColor = Color.white;
        secondaryColor = Color.black;
        stringArray = new string[] { "项目1", "项目2", "项目3" };
        floatArray = new float[] { 1f, 2f, 3f, 4f, 5f };
        gameObjectRef = null;
        materialRef = null;
        textureRef = null;
        customData = new CustomData();
        customDataArray = new CustomData[3];
        
        Debug.Log("ExampleSO 已重置为默认值");
    }
    
    /// <summary>
    /// 随机化所有数值
    /// </summary>
    [ContextMenu("随机化数值")]
    public void RandomizeValues()
    {
        rangeValue = UnityEngine.Random.Range(0f, 100f);
        intValue = UnityEngine.Random.Range(0, 100);
        exampleEnum = (ExampleEnum)UnityEngine.Random.Range(0, 5);
        statusEnum = (StatusEnum)UnityEngine.Random.Range(0, 5);
        position = new Vector3(
            UnityEngine.Random.Range(-10f, 10f),
            UnityEngine.Random.Range(-10f, 10f),
            UnityEngine.Random.Range(-10f, 10f)
        );
        rotation = new Vector3(
            UnityEngine.Random.Range(0f, 360f),
            UnityEngine.Random.Range(0f, 360f),
            UnityEngine.Random.Range(0f, 360f)
        );
        scale = new Vector3(
            UnityEngine.Random.Range(0.5f, 2f),
            UnityEngine.Random.Range(0.5f, 2f),
            UnityEngine.Random.Range(0.5f, 2f)
        );
        mainColor = new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            1f
        );
        secondaryColor = new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            1f
        );
        
        Debug.Log("ExampleSO 数值已随机化");
    }
    
    /// <summary>
    /// 打印当前状态
    /// </summary>
    [ContextMenu("打印状态")]
    public void PrintStatus()
    {
        Debug.Log($"ExampleSO 状态:\n" +
                  $"名称: {exampleName}\n" +
                  $"数值: {rangeValue}\n" +
                  $"整数值: {intValue}\n" +
                  $"启用状态: {isEnabled}\n" +
                  $"枚举值: {exampleEnum}\n" +
                  $"状态: {statusEnum}");
    }
}
