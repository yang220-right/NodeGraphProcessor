using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace GraphProcessor{
  [System.Serializable]
  public abstract class BaseGraphWindow : EditorWindow{
    protected VisualElement rootView;
    protected BaseGraphView graphView;

    [SerializeField] protected BaseGraph graph;

    readonly string graphWindowStyle = "GraphProcessorStyles/BaseGraphView";

    public bool isGraphLoaded{
      get{ return graphView != null && graphView.graph != null; }
    }

    bool reloadWorkaround = false;

    public event Action<BaseGraph> graphLoaded;
    public event Action<BaseGraph> graphUnloaded;

    /// <summary>
    /// 当窗口启用/打开时由Unity调用
    /// </summary>
    protected virtual void OnEnable(){
      InitializeRootView();

      if (graph != null)
        LoadGraph();
      else
        reloadWorkaround = true;
    }

    protected virtual void Update(){
      // 编辑器窗口刷新选项的解决方案：
      // 当点击刷新时，OnEnable在编辑器窗口中的序列化数据
      // 反序列化之前被调用，导致图形视图无法加载
      if (reloadWorkaround && graph != null){
        LoadGraph();
        reloadWorkaround = false;
      }
    }

    void LoadGraph(){
      // 我们等待图形初始化
      if (graph.isEnabled)
        InitializeGraph(graph);
      else
        graph.onEnabled += () => InitializeGraph(graph);
    }

    /// <summary>
    /// 当窗口禁用时由Unity调用（在域重载时发生）
    /// </summary>
    protected virtual void OnDisable(){
      if (graph != null && graphView != null)
        graphView.SaveGraphToDisk();
    }

    /// <summary>
    /// 当窗口关闭时由Unity调用
    /// </summary>
    protected virtual void OnDestroy(){
    }

    void InitializeRootView(){
      rootView = base.rootVisualElement;

      rootView.name = "graphRootView";

      rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
    }

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

    void LinkGraphWindowToScene(Scene scene){
      EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;

      void CloseWindowWhenSceneIsClosed(Scene closedScene){
        if (scene == closedScene){
          Close();
          EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
        }
      }
    }

    public virtual void OnGraphDeleted(){
      if (graph != null && graphView != null)
        rootView.Remove(graphView);

      graphView = null;
    }

    protected abstract void InitializeWindow(BaseGraph graph);

    protected virtual void InitializeGraphView(BaseGraphView view){
    }
  }
}