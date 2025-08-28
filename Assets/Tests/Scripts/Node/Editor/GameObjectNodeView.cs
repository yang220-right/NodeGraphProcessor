using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[NodeCustomEditor(typeof(SceneGameObjectNode))]
public class SceneGameObjectNodeView : BaseNodeView{
  public override void Enable(){
    var gameObjectNode = nodeTarget as SceneGameObjectNode;

    // 创建ObjectField用于拖拽GameObject
    var objectField = new ObjectField{
      objectType = typeof(GameObject),
      allowSceneObjects = true, // 允许拖拽场景中的GameObject
      value = gameObjectNode.input,
      label = "GameObject"
    };

    // 注册值变化回调
    objectField.RegisterValueChangedCallback((v) => {
      owner.RegisterCompleteObjectUndo("Updated SceneGameObject input");
      gameObjectNode.input = v.newValue as GameObject;
    });

    // 将ObjectField添加到控制容器中
    controlsContainer.Add(objectField);
  }
}