﻿using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GraphProcessor{
  public class GroupView : UnityEditor.Experimental.GraphView.Group{
    public BaseGraphView owner;
    public Group group;
    Label titleLabel;
    ColorField colorField;

    readonly string groupStyle = "GraphProcessorStyles/GroupView";

    public GroupView(){
      styleSheets.Add(Resources.Load<StyleSheet>(groupStyle));
    }

    private static void BuildContextualMenu(ContextualMenuPopulateEvent evt){
    }

    public void Initialize(BaseGraphView graphView, Group block){
      group = block;
      owner = graphView;

      title = block.title;
      SetPosition(block.position);

      this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

      headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>(TitleChangedCallback);
      titleLabel = headerContainer.Q<Label>();

      colorField = new ColorField{ value = group.color, name = "headerColorPicker" };
      colorField.RegisterValueChangedCallback(e => { UpdateGroupColor(e.newValue); });
      UpdateGroupColor(group.color);

      headerContainer.Add(colorField);

      InitializeInnerNodes();
    }

    void InitializeInnerNodes(){
      foreach (var nodeGUID in group.innerNodeGUIDs.ToList()){
        if (!owner.graph.nodesPerGUID.ContainsKey(nodeGUID)){
          Debug.LogWarning("Node GUID not found: " + nodeGUID);
          group.innerNodeGUIDs.Remove(nodeGUID);
          continue;
        }

        var node = owner.graph.nodesPerGUID[nodeGUID];
        var nodeView = owner.nodeViewsPerNode[node];

        AddElement(nodeView);
      }
    }

    protected override void OnElementsAdded(IEnumerable<GraphElement> elements){
      foreach (var element in elements){
        var node = element as BaseNodeView;

        // 添加当前不支持的非节点元素
        if (node == null)
          continue;

        if (!group.innerNodeGUIDs.Contains(node.nodeTarget.GUID))
          group.innerNodeGUIDs.Add(node.nodeTarget.GUID);
      }

      base.OnElementsAdded(elements);
    }

    protected override void OnElementsRemoved(IEnumerable<GraphElement> elements){
      // 仅当组存在于层次结构中时才移除节点
      if (parent != null){
        foreach (var elem in elements){
          if (elem is BaseNodeView nodeView){
            group.innerNodeGUIDs.Remove(nodeView.nodeTarget.GUID);
          }
        }
      }

      base.OnElementsRemoved(elements);
    }

    public void UpdateGroupColor(Color newColor){
      group.color = newColor;
      style.backgroundColor = newColor;
    }

    void TitleChangedCallback(ChangeEvent<string> e){
      group.title = e.newValue;
    }

    public override void SetPosition(Rect newPos){
      base.SetPosition(newPos);

      group.position = newPos;
    }
  }
}