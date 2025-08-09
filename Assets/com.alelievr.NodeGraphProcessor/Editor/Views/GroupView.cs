using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GraphProcessor{
  /// <summary>
  /// 组视图类
  /// 继承自Unity的Group类，提供图形中组的可视化表示
  /// 负责组的显示、节点管理和用户交互
  /// </summary>
  public class GroupView : UnityEditor.Experimental.GraphView.Group{
    /// <summary>
    /// 组所有者
    /// 拥有此组的图形视图
    /// </summary>
    public BaseGraphView owner;
    
    /// <summary>
    /// 组数据
    /// 对应的组对象
    /// </summary>
    public Group group;
    
    /// <summary>
    /// 标题标签
    /// 显示组标题的UI元素
    /// </summary>
    Label titleLabel;
    
    /// <summary>
    /// 颜色字段
    /// 用于选择组颜色的UI元素
    /// </summary>
    ColorField colorField;

    /// <summary>
    /// 组样式表路径
    /// 组视图的样式定义文件路径
    /// </summary>
    readonly string groupStyle = "GraphProcessorStyles/GroupView";

    /// <summary>
    /// 组视图构造函数
    /// 初始化组视图并加载样式
    /// </summary>
    public GroupView(){
      styleSheets.Add(Resources.Load<StyleSheet>(groupStyle));
    }

    /// <summary>
    /// 构建上下文菜单
    /// 静态方法，用于创建组的右键菜单
    /// </summary>
    /// <param name="evt">上下文菜单事件</param>
    private static void BuildContextualMenu(ContextualMenuPopulateEvent evt){
    }

    /// <summary>
    /// 初始化组视图
    /// 设置组的基本属性和UI元素
    /// </summary>
    /// <param name="graphView">图形视图</param>
    /// <param name="block">组数据</param>
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

    /// <summary>
    /// 初始化内部节点
    /// 根据组数据中的节点GUID列表添加节点到组中
    /// </summary>
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

    /// <summary>
    /// 元素添加处理
    /// 当元素被添加到组中时调用
    /// </summary>
    /// <param name="elements">被添加的元素集合</param>
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

    /// <summary>
    /// 元素移除处理
    /// 当元素从组中移除时调用
    /// </summary>
    /// <param name="elements">被移除的元素集合</param>
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

    /// <summary>
    /// 更新组颜色
    /// 设置组的新颜色并更新UI显示
    /// </summary>
    /// <param name="newColor">新的组颜色</param>
    public void UpdateGroupColor(Color newColor){
      group.color = newColor;
      style.backgroundColor = newColor;
    }

    /// <summary>
    /// 标题变化回调
    /// 当组标题发生变化时更新组数据
    /// </summary>
    /// <param name="e">变化事件</param>
    void TitleChangedCallback(ChangeEvent<string> e){
      group.title = e.newValue;
    }

    /// <summary>
    /// 设置位置
    /// 设置组的位置并更新组数据
    /// </summary>
    /// <param name="newPos">新的位置</param>
    public override void SetPosition(Rect newPos){
      base.SetPosition(newPos);

      group.position = newPos;
    }
  }
}