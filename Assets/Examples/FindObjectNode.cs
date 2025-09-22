using UnityEngine;
using GraphProcessor;

/// <summary>
/// 根据对象名称在场景中查找GameObject的节点
/// </summary>
[System.Serializable, NodeMenuItem("Utility/Find Object")]
public class FindObjectNode : BaseNode
{
    [Input("inputSceneObj")]
    public string objectName;
    
    [Output("outSceneOjb")]
    public GameObject foundObject;
    
    public override string name => "FindSceneGameObject";
    
    protected override void Process()
    {
        // 如果对象名称为空，直接返回
        if (string.IsNullOrEmpty(objectName))
        {
            Debug.LogWarning("对象名称为空，无法查找");
            return;
        }
        
        // 在场景中查找对象
        GameObject obj = GameObject.Find(objectName);
        
        if (obj != null)
        {
            foundObject = obj;
            Debug.Log($"成功找到对象: {objectName}");
        }
        else
        {
            Debug.LogWarning($"未找到名称为 '{objectName}' 的对象");
        }
    }
}
