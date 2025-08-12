using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System;

namespace GraphProcessor{
  /// <summary>
  /// 固定元素视图抽象基类
  /// 继承自GraphElement，提供固定元素的基础UI功能
  /// 用于创建可拖拽、可调整大小的固定面板，如黑板、参数面板等
  /// </summary>
  public abstract class PinnedElementView : GraphElement{
    /// <summary>
    /// 固定元素数据
    /// 对应的固定元素对象
    /// </summary>
    protected PinnedElement pinnedElement;

    /// <summary>
    /// 根元素
    /// 视图的根UI元素
    /// </summary>
    protected VisualElement root;

    /// <summary>
    /// 内容元素
    /// 用于容纳具体内容的UI元素
    /// </summary>
    protected VisualElement content;

    /// <summary>
    /// 头部元素
    /// 视图的头部UI元素
    /// </summary>
    protected VisualElement header;

    /// <summary>
    /// 大小调整事件
    /// 当元素大小调整时触发
    /// </summary>
    protected event Action onResized;

    /// <summary>
    /// 主容器
    /// 主要的UI容器元素
    /// </summary>
    VisualElement main;

    /// <summary>
    /// 标题标签
    /// 显示标题的UI元素
    /// </summary>
    Label titleLabel;

    /// <summary>
    /// 是否可滚动
    /// 标识内容是否支持滚动
    /// </summary>
    bool _scrollable;

    /// <summary>
    /// 滚动视图
    /// 用于内容滚动的UI元素
    /// </summary>
    ScrollView scrollView;

    /// <summary>
    /// 固定元素样式路径
    /// 固定元素视图的样式定义文件路径
    /// </summary>
    static readonly string pinnedElementStyle = "GraphProcessorStyles/PinnedElementView";

    /// <summary>
    /// 固定元素模板路径
    /// 固定元素视图的UI模板文件路径
    /// </summary>
    static readonly string pinnedElementTree = "GraphProcessorElements/PinnedElement";

    /// <summary>
    /// 标题属性
    /// 获取或设置固定元素的标题
    /// </summary>
    public override string title{
      get{ return titleLabel.text; }
      set{ titleLabel.text = value; }
    }

    /// <summary>
    /// 可滚动属性
    /// 控制内容是否支持滚动显示
    /// </summary>
    protected bool scrollable{
      get{ return _scrollable; }
      set{
        if (_scrollable == value)
          return;

        _scrollable = value;

        style.position = Position.Absolute;
        if (_scrollable){
          // 启用滚动：将内容添加到滚动视图中
          content.RemoveFromHierarchy();
          root.Add(scrollView);
          scrollView.Add(content);
          AddToClassList("scrollable");
        }
        else{
          // 禁用滚动：将内容直接添加到根元素
          scrollView.RemoveFromHierarchy();
          content.RemoveFromHierarchy();
          root.Add(content);
          RemoveFromClassList("scrollable");
        }
      }
    }

    /// <summary>
    /// 构造函数
    /// 初始化固定元素视图的基础UI结构
    /// </summary>
    public PinnedElementView(){
      // 加载UI模板和样式
      var tpl = Resources.Load<VisualTreeAsset>(pinnedElementTree);
      styleSheets.Add(Resources.Load<StyleSheet>(pinnedElementStyle));

      main = tpl.CloneTree();
      main.AddToClassList("mainContainer");
      scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);

      // 获取UI元素引用
      root = main.Q("content");
      header = main.Q("header");
      titleLabel = main.Q<Label>(name: "titleLabel");
      content = main.Q<VisualElement>(name: "contentContainer");

      hierarchy.Add(main);

      // 设置基本能力
      capabilities |= Capabilities.Movable | Capabilities.Resizable;
      style.overflow = Overflow.Hidden;

      // 设置样式类
      ClearClassList();
      AddToClassList("pinnedElement");

      // 添加拖拽操作器
      this.AddManipulator(new Dragger{ clampToParentEdges = true });

      scrollable = false;

      // 添加大小调整器
      hierarchy.Add(new Resizer(() => onResized?.Invoke()));

      // 注册拖拽事件处理
      RegisterCallback<DragUpdatedEvent>(e => { e.StopPropagation(); });

      title = "PinnedElementView";
    }

    /// <summary>
    /// 初始化图形视图
    /// 设置固定元素与图形视图的关联
    /// </summary>
    /// <param name="pinnedElement">固定元素数据</param>
    /// <param name="graphView">图形视图</param>
    public void InitializeGraphView(PinnedElement pinnedElement, BaseGraphView graphView){
      this.pinnedElement = pinnedElement;
      SetPosition(pinnedElement.position);

      // 订阅大小调整事件
      onResized += () => { pinnedElement.position.size = layout.size; };

      // 注册鼠标释放事件
      RegisterCallback<MouseUpEvent>(e => { pinnedElement.position.position = layout.position; });

      Initialize(graphView);
    }

    /// <summary>
    /// 重置位置
    /// 将固定元素重置到默认位置和大小
    /// </summary>
    public void ResetPosition(){
      pinnedElement.position = new Rect(Vector2.zero, PinnedElement.defaultSize);
      SetPosition(pinnedElement.position);
    }

    /// <summary>
    /// 初始化抽象方法
    /// 子类必须实现此方法来初始化具体的UI内容
    /// </summary>
    /// <param name="graphView">图形视图</param>
    protected abstract void Initialize(BaseGraphView graphView);

    /// <summary>
    /// 析构函数
    /// 确保资源正确释放
    /// </summary>
    ~PinnedElementView(){
      Destroy();
    }

    /// <summary>
    /// 销毁方法
    /// 子类可以重写此方法来执行清理操作
    /// </summary>
    protected virtual void Destroy(){
    }
  }
}