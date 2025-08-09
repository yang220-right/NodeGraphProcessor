using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace GraphProcessor{
  /// <summary>
  /// 图形窗口基类
  /// 提供图形编辑器的窗口功能，管理图形的加载、显示和保存
  /// 所有具体的图形编辑器窗口都应该继承此类
  /// </summary>
  [System.Serializable]
  public abstract class BaseGraphWindow : EditorWindow{
    /// <summary>
    /// 根视图元素
    /// 窗口的根UI元素容器
    /// </summary>
    protected VisualElement rootView;
    
    /// <summary>
    /// 图形视图
    /// 负责显示和编辑图形的视图组件
    /// </summary>
    protected BaseGraphView graphView;

    /// <summary>
    /// 当前编辑的图形
    /// 序列化存储的图形对象
    /// </summary>
    [SerializeField] protected BaseGraph graph;

    /// <summary>
    /// 图形窗口样式表路径
    /// 用于加载窗口的样式定义
    /// </summary>
    readonly string graphWindowStyle = "GraphProcessorStyles/BaseGraphView";

    /// <summary>
    /// 图形是否已加载
    /// 检查图形视图和图形是否都已正确初始化
    /// </summary>
    public bool isGraphLoaded{
      get{ return graphView != null && graphView.graph != null; }
    }

    /// <summary>
    /// 重新加载工作区标志
    /// 用于处理编辑器窗口刷新时的序列化问题
    /// </summary>
    bool reloadWorkaround = false;

    /// <summary>
    /// 图形加载事件
    /// 当图形被加载时触发
    /// </summary>
    public event Action<BaseGraph> graphLoaded;
    
    /// <summary>
    /// 图形卸载事件
    /// 当图形被卸载时触发
    /// </summary>
    public event Action<BaseGraph> graphUnloaded;

    /// <summary>
    /// 窗口启用时的初始化
    /// 当窗口启用/打开时由Unity调用
    /// </summary>
    protected virtual void OnEnable(){
      InitializeRootView();

      if (graph != null)
        LoadGraph();
      else
        reloadWorkaround = true;
    }

    /// <summary>
    /// 更新方法
    /// 处理编辑器窗口刷新选项的解决方案
    /// </summary>
    protected virtual void Update(){
      // 编辑器窗口刷新选项的解决方案：
      // 当点击刷新时，OnEnable在编辑器窗口中的序列化数据
      // 反序列化之前被调用，导致图形视图无法加载
      if (reloadWorkaround && graph != null){
        LoadGraph();
        reloadWorkaround = false;
      }
    }

    /// <summary>
    /// 加载图形
    /// 初始化并加载图形到窗口中
    /// </summary>
    void LoadGraph(){
      // 我们等待图形初始化
      if (graph.isEnabled)
        InitializeGraph(graph);
      else
        graph.onEnabled += () => InitializeGraph(graph);
    }

    /// <summary>
    /// 窗口禁用时的清理
    /// 当窗口禁用时由Unity调用（在域重载时发生）
    /// </summary>
    protected virtual void OnDisable(){
      if (graph != null && graphView != null)
        graphView.SaveGraphToDisk();
    }

    /// <summary>
    /// 窗口销毁时的清理
    /// 当窗口关闭时由Unity调用
    /// </summary>
    protected virtual void OnDestroy(){
    }

    /// <summary>
    /// 初始化根视图
    /// 设置窗口的根UI元素和样式
    /// </summary>
    void InitializeRootView(){
      rootView = base.rootVisualElement;

      rootView.name = "graphRootView";

      rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
    }

    /// <summary>
    /// 初始化图形
    /// 将图形加载到窗口中并设置相关事件
    /// </summary>
    /// <param name="graph">要加载的图形</param>
    public void InitializeGraph(BaseGraph graph){
      if (this.graph != null && graph != this.graph){
        // 将图形保存到磁盘
        EditorUtility.SetDirty(this.graph);
        AssetDatabase.SaveAssets();
        // 卸载图形
        graphUnloaded?.Invoke(this.graph);
      }

      graphLoaded?.Invoke(graph);
      this.graph = graph;

      if (graphView != null)
        rootView.Remove(graphView);

      //Initialize将提供BaseGraphView
      InitializeWindow(graph);

      graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;

      if (graphView == null){
        Debug.LogError("GraphView尚未添加到BaseGraph根视图！");
        return;
      }

      graphView.Initialize(graph);

      InitializeGraphView(graphView);

      // TODO: onSceneLinked...

      if (graph.IsLinkedToScene())
        LinkGraphWindowToScene(graph.GetLinkedScene());
      else
        graph.onSceneLinked += LinkGraphWindowToScene;
    }

    /// <summary>
    /// 将图形窗口链接到场景
    /// 处理图形与场景的关联关系
    /// </summary>
    /// <param name="scene">要链接的场景</param>
    void LinkGraphWindowToScene(Scene scene){
      EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;

      void CloseWindowWhenSceneIsClosed(Scene closedScene){
        if (scene == closedScene){
          Close();
          EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
        }
      }
    }

    /// <summary>
    /// 图形删除时的处理
    /// 当图形被删除时清理相关资源
    /// </summary>
    public virtual void OnGraphDeleted(){
      if (graph != null && graphView != null)
        rootView.Remove(graphView);

      graphView = null;
    }

    /// <summary>
    /// 初始化窗口
    /// 抽象方法，子类必须实现以提供具体的窗口初始化逻辑
    /// </summary>
    /// <param name="graph">要初始化的图形</param>
    protected abstract void InitializeWindow(BaseGraph graph);

    /// <summary>
    /// 初始化图形视图
    /// 虚拟方法，子类可以重写以提供额外的图形视图初始化
    /// </summary>
    /// <param name="view">要初始化的图形视图</param>
    protected virtual void InitializeGraphView(BaseGraphView view){
    }
  }
}