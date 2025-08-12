using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using System;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace GraphProcessor{
  /// <summary>
  /// 工具栏视图类
  /// 提供图形编辑器的工具栏功能，支持按钮、切换、下拉菜单等UI元素
  /// 继承自VisualElement，使用IMGUI绘制工具栏界面
  /// </summary>
  public class ToolbarView : VisualElement{
    /// <summary>
    /// 元素类型枚举
    /// 定义工具栏中支持的各种UI元素类型
    /// </summary>
    protected enum ElementType{
      /// <summary>
      /// 按钮
      /// </summary>
      Button,

      /// <summary>
      /// 切换开关
      /// </summary>
      Toggle,

      /// <summary>
      /// 下拉按钮
      /// </summary>
      DropDownButton,

      /// <summary>
      /// 分隔符
      /// </summary>
      Separator,

      /// <summary>
      /// 自定义元素
      /// </summary>
      Custom,

      /// <summary>
      /// 弹性空间
      /// </summary>
      FlexibleSpace,
    }

    /// <summary>
    /// 工具栏按钮数据类
    /// 存储工具栏中每个元素的数据和回调信息
    /// </summary>
    protected class ToolbarButtonData{
      /// <summary>
      /// GUI内容
      /// 按钮的显示内容（文本、图标等）
      /// </summary>
      public GUIContent content;

      /// <summary>
      /// 元素类型
      /// 工具栏元素的类型
      /// </summary>
      public ElementType type;

      /// <summary>
      /// 值
      /// 主要用于切换开关的当前状态
      /// </summary>
      public bool value;

      /// <summary>
      /// 是否可见
      /// 控制元素的显示状态
      /// </summary>
      public bool visible = true;

      /// <summary>
      /// 按钮回调
      /// 按钮点击时执行的回调函数
      /// </summary>
      public Action buttonCallback;

      /// <summary>
      /// 切换回调
      /// 切换开关状态改变时执行的回调函数
      /// </summary>
      public Action<bool> toggleCallback;

      /// <summary>
      /// 大小
      /// 元素的大小（主要用于分隔符）
      /// </summary>
      public int size;

      /// <summary>
      /// 自定义绘制函数
      /// 用于自定义元素的绘制逻辑
      /// </summary>
      public Action customDrawFunction;
    }

    /// <summary>
    /// 左侧按钮数据列表
    /// 存储工具栏左侧的按钮数据
    /// </summary>
    List<ToolbarButtonData> leftButtonDatas = new List<ToolbarButtonData>();

    /// <summary>
    /// 右侧按钮数据列表
    /// 存储工具栏右侧的按钮数据
    /// </summary>
    List<ToolbarButtonData> rightButtonDatas = new List<ToolbarButtonData>();

    /// <summary>
    /// 图形视图
    /// 工具栏所属的图形视图
    /// </summary>
    protected BaseGraphView graphView;

    /// <summary>
    /// 显示处理器按钮
    /// 控制处理器显示的切换按钮
    /// </summary>
    ToolbarButtonData showProcessor;

    /// <summary>
    /// 显示参数按钮
    /// 控制参数显示的切换按钮
    /// </summary>
    ToolbarButtonData showParameters;

    /// <summary>
    /// 构造函数
    /// 初始化工具栏视图
    /// </summary>
    /// <param name="graphView">图形视图</param>
    public ToolbarView(BaseGraphView graphView){
      name = "ToolbarView";
      this.graphView = graphView;

      // 当图形视图初始化完成后，清空按钮数据并添加按钮
      graphView.initialized += () => {
        leftButtonDatas.Clear();
        rightButtonDatas.Clear();
        AddButtons();
      };

      // 添加IMGUI容器来绘制工具栏
      Add(new IMGUIContainer(DrawImGUIToolbar));
    }

    /// <summary>
    /// 添加按钮（字符串版本）
    /// 使用字符串名称添加按钮
    /// </summary>
    /// <param name="name">按钮名称</param>
    /// <param name="callback">按钮回调</param>
    /// <param name="left">是否添加到左侧</param>
    /// <returns>创建的按钮数据</returns>
    protected ToolbarButtonData AddButton(string name, Action callback, bool left = true)
      => AddButton(new GUIContent(name), callback, left);

    /// <summary>
    /// 添加按钮（GUIContent版本）
    /// 使用GUIContent添加按钮
    /// </summary>
    /// <param name="content">按钮内容</param>
    /// <param name="callback">按钮回调</param>
    /// <param name="left">是否添加到左侧</param>
    /// <returns>创建的按钮数据</returns>
    protected ToolbarButtonData AddButton(GUIContent content, Action callback, bool left = true){
      var data = new ToolbarButtonData{
        content = content,
        type = ElementType.Button,
        buttonCallback = callback
      };
      ((left) ? leftButtonDatas : rightButtonDatas).Add(data);
      return data;
    }

    /// <summary>
    /// 添加分隔符
    /// 在工具栏中添加分隔符
    /// </summary>
    /// <param name="sizeInPixels">分隔符大小（像素）</param>
    /// <param name="left">是否添加到左侧</param>
    protected void AddSeparator(int sizeInPixels = 10, bool left = true){
      var data = new ToolbarButtonData{
        type = ElementType.Separator,
        size = sizeInPixels,
      };
      ((left) ? leftButtonDatas : rightButtonDatas).Add(data);
    }

    /// <summary>
    /// 添加自定义元素
    /// 添加自定义绘制的UI元素
    /// </summary>
    /// <param name="imguiDrawFunction">IMGUI绘制函数</param>
    /// <param name="left">是否添加到左侧</param>
    protected void AddCustom(Action imguiDrawFunction, bool left = true){
      if (imguiDrawFunction == null)
        throw new ArgumentException("imguiDrawFunction can't be null");

      var data = new ToolbarButtonData{
        type = ElementType.Custom,
        customDrawFunction = imguiDrawFunction,
      };
      ((left) ? leftButtonDatas : rightButtonDatas).Add(data);
    }

    /// <summary>
    /// 添加弹性空间
    /// 添加可伸缩的空间元素
    /// </summary>
    /// <param name="left">是否添加到左侧</param>
    protected void AddFlexibleSpace(bool left = true){
      ((left) ? leftButtonDatas : rightButtonDatas).Add(new ToolbarButtonData{ type = ElementType.FlexibleSpace });
    }

    /// <summary>
    /// 添加切换开关（字符串版本）
    /// 使用字符串名称添加切换开关
    /// </summary>
    /// <param name="name">切换开关名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="callback">切换回调</param>
    /// <param name="left">是否添加到左侧</param>
    /// <returns>创建的切换开关数据</returns>
    protected ToolbarButtonData AddToggle(string name, bool defaultValue, Action<bool> callback, bool left = true)
      => AddToggle(new GUIContent(name), defaultValue, callback, left);

    /// <summary>
    /// 添加切换开关（GUIContent版本）
    /// 使用GUIContent添加切换开关
    /// </summary>
    /// <param name="content">切换开关内容</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="callback">切换回调</param>
    /// <param name="left">是否添加到左侧</param>
    /// <returns>创建的切换开关数据</returns>
    protected ToolbarButtonData AddToggle(GUIContent content, bool defaultValue, Action<bool> callback,
      bool left = true){
      var data = new ToolbarButtonData{
        content = content,
        type = ElementType.Toggle,
        value = defaultValue,
        toggleCallback = callback
      };
      ((left) ? leftButtonDatas : rightButtonDatas).Add(data);
      return data;
    }

    /// <summary>
    /// 添加下拉按钮
    /// 使用字符串名称添加下拉按钮
    /// </summary>
    /// <param name="name">下拉按钮名称</param>
    /// <param name="callback">下拉按钮回调</param>
    /// <param name="left">是否添加到左侧</param>
    /// <returns>创建的下拉按钮数据</returns>
    protected ToolbarButtonData AddDropDownButton(string name, Action callback, bool left = true)
      => AddDropDownButton(new GUIContent(name), callback, left);

    /// <summary>
    /// 添加下拉按钮（GUIContent版本）
    /// 使用GUIContent添加下拉按钮
    /// </summary>
    /// <param name="content">下拉按钮内容</param>
    /// <param name="callback">下拉按钮回调</param>
    /// <param name="left">是否添加到左侧</param>
    /// <returns>创建的下拉按钮数据</returns>
    protected ToolbarButtonData AddDropDownButton(GUIContent content, Action callback, bool left = true){
      var data = new ToolbarButtonData{
        content = content,
        type = ElementType.DropDownButton,
        buttonCallback = callback
      };
      ((left) ? leftButtonDatas : rightButtonDatas).Add(data);
      return data;
    }

    /// <summary>
    /// 移除按钮
    /// 根据名称移除左侧或右侧的按钮
    /// </summary>
    /// <param name="name">按钮名称</param>
    /// <param name="left">是否从左侧移除</param>
    protected void RemoveButton(string name, bool left){
      ((left) ? leftButtonDatas : rightButtonDatas).RemoveAll(b => b.content.text == name);
    }

    /// <summary>
    /// 隐藏按钮
    /// 根据名称隐藏按钮
    /// </summary>
    /// <param name="name">按钮显示名称</param>
    protected void HideButton(string name){
      leftButtonDatas.Concat(rightButtonDatas).All(b => {
        if (b?.content?.text == name)
          b.visible = false;
        return true;
      });
    }

    /// <summary>
    /// 显示按钮
    /// 根据名称显示按钮
    /// </summary>
    /// <param name="name">按钮显示名称</param>
    protected void ShowButton(string name){
      leftButtonDatas.Concat(rightButtonDatas).All(b => {
        if (b?.content?.text == name)
          b.visible = true;
        return true;
      });
    }

    protected virtual void AddButtons(){
      AddButton("Center", graphView.ResetPositionAndZoom);

      bool processorVisible = graphView.GetPinnedElementStatus<ProcessorView>() != Status.Hidden;
      showProcessor = AddToggle("Show Processor", processorVisible, (v) => graphView.ToggleView<ProcessorView>());
      bool exposedParamsVisible = graphView.GetPinnedElementStatus<ExposedParameterView>() != Status.Hidden;
      showParameters = AddToggle("Show Parameters", exposedParamsVisible,
        (v) => graphView.ToggleView<ExposedParameterView>());

      AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph), false);
    }

    public virtual void UpdateButtonStatus(){
      if (showProcessor != null)
        showProcessor.value = graphView.GetPinnedElementStatus<ProcessorView>() != Status.Hidden;
      if (showParameters != null)
        showParameters.value = graphView.GetPinnedElementStatus<ExposedParameterView>() != Status.Hidden;
    }

    void DrawImGUIButtonList(List<ToolbarButtonData> buttons){
      foreach (var button in buttons.ToList()){
        if (!button.visible)
          continue;

        switch (button.type){
          case ElementType.Button:
            if (GUILayout.Button(button.content, EditorStyles.toolbarButton) && button.buttonCallback != null)
              button.buttonCallback();
            break;
          case ElementType.Toggle:
            EditorGUI.BeginChangeCheck();
            button.value = GUILayout.Toggle(button.value, button.content, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck() && button.toggleCallback != null)
              button.toggleCallback(button.value);
            break;
          case ElementType.DropDownButton:
            if (EditorGUILayout.DropdownButton(button.content, FocusType.Passive, EditorStyles.toolbarDropDown))
              button.buttonCallback();
            break;
          case ElementType.Separator:
            EditorGUILayout.Separator();
            EditorGUILayout.Space(button.size);
            break;
          case ElementType.Custom:
            button.customDrawFunction();
            break;
          case ElementType.FlexibleSpace:
            GUILayout.FlexibleSpace();
            break;
        }
      }
    }

    protected virtual void DrawImGUIToolbar(){
      GUILayout.BeginHorizontal(EditorStyles.toolbar);

      DrawImGUIButtonList(leftButtonDatas);

      GUILayout.FlexibleSpace();

      DrawImGUIButtonList(rightButtonDatas);

      GUILayout.EndHorizontal();
    }
  }
}