using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;
using NodeGraphProcessor.Examples;

public class ConditionalProcessorView : PinnedElementView{
  ConditionalProcessor processor;
  BaseGraphView graphView;

  public ConditionalProcessorView() => title = "Conditional Processor";

  protected override void Initialize(BaseGraphView graphView){
    processor = new ConditionalProcessor(graphView.graph);
    this.graphView = graphView;

    graphView.computeOrderUpdated += processor.UpdateComputeOrder;

    Button runButton = new Button(OnPlay){ name = "ActionButton", text = "Run" };
    Button stepButton = new Button(OnStep){ name = "ActionButton", text = "Step" };

    content.Add(runButton);
    content.Add(stepButton);
  }

  void OnPlay() => processor.Run();

  void OnStep(){
    BaseNodeView view;

    if (processor.currentGraphExecution != null){
      // 取消高亮最后执行的节点
      view = graphView.nodeViews.Find(v => v.nodeTarget == processor.currentGraphExecution.Current);
      view.UnHighlight();
    }

    processor.Step();

    // 显示调试信息，currentGraphExecution在上面的Step()函数中被修改
    if (processor.currentGraphExecution != null){
      view = graphView.nodeViews.Find(v => v.nodeTarget == processor.currentGraphExecution.Current);
      view.Highlight();
    }
  }
}