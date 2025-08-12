using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

public class CustomToolbarView : ToolbarView{
  public CustomToolbarView(BaseGraphView graphView) : base(graphView){
  }

  protected override void AddButtons(){
    // 在工具栏左侧添加hello world按钮
    AddButton("Hello !", () => Debug.Log("Hello World"), left: false);

    // 添加默认按钮（居中、显示处理器和在项目中显示）
    base.AddButtons();

    bool conditionalProcessorVisible = graphView.GetPinnedElementStatus<ConditionalProcessorView>() != Status.Hidden;
    AddToggle("Show Conditional Processor", conditionalProcessorVisible,
      (v) => graphView.ToggleView<ConditionalProcessorView>());
  }
}