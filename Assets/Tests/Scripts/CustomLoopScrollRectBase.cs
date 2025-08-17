using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using ScrollRect;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI{
  /// <summary>
  /// loop方向
  /// </summary>
  public enum LoopScrollRectDirection{
    Vertical,
    Horizontal,
  }
  /// <summary>
  /// ScrollRect使用的事件类型。
  /// 显示在面板上 可以拖动事件 也可以赋值一个
  /// </summary>
  [Serializable]
  public class ScrollRectEvent : UnityEvent<Vector2>{ }
  
  /// <summary>
  /// 用于使子RectTransform可滚动的组件，支持内容重用。
  /// LoopScrollRect本身不会进行任何裁剪。结合Mask组件，可以将其转换为循环滚动视图。
  /// </summary>
  [RequireComponent(typeof(RectTransform))]
  public abstract class CustomLoopScrollRectBase :
    UIBehaviour,
    //作用：当检测到可能的拖动开始时（在BeginDrag之前）
    //初始化拖动前的状态
    //记录初始位置/数据
    //重要特性：即使后续取消拖动也会触发
    IInitializePotentialDragHandler,
    //拖动操作正式开始时
    IBeginDragHandler,
    //拖动结束时（释放鼠标/触摸）
    IEndDragHandler,
    //拖动过程中每帧调用
    IDragHandler,
    //鼠标滚轮滚动时 
    //OnScroll(PointerEventData eventData)
    // IScrollHandler,
    //定义UI元素与Canvas系统的交互协议
    //void Rebuild(CanvasUpdate executing);  // Canvas更新时调用
    //bool IsDestroyed();                    // 检查元素是否被销毁
    //元素通过CanvasUpdateRegistry注册自己
    //Canvas渲染前触发Rebuild（分三个阶段）：
    //Prelayout → Layout → PostLayout
    ICanvasElement,
    ILayoutElement,
    ILayoutGroup{

    /// <summary>
    /// 滚动项目的总数量，项目ID在[0, totalCount]范围内。负值如-1表示无限项目模式。
    /// </summary>
    [Tooltip("总数量，负值表示无限模式")] protected int totalCount;

    /// <summary>
    /// [可选] 用于精确尺寸的辅助器，以便实现更好的滚动效果。 也是个接口 只有一个方法 就是获取item的大小
    /// </summary>
    [NonSerialized] public LoopScrollSizeHelper sizeHelper = null;

    /// <summary>
    /// 当达到阈值时，再生成几个预制体。这将扩展到至少1.5 * itemSize。
    /// </summary>
    protected float threshold = 0;

    /// <summary>
    /// 取反 第一个变成最后一个
    /// </summary>
    [Tooltip("拖拽的反向")] public bool reverseDirection = false;

    /// <summary>
    /// 滚动起始index 如果一行有多个项目 则值为这一行的最后一个项目的index
    /// </summary>
    protected int itemTypeStart = 0;

    /// <summary>
    /// 滚动中的最后一个index
    /// </summary>
    protected int itemTypeEnd = 0;
    /// <summary>
    /// 预制体大小
    /// </summary>
    protected float itemTypeSize = 0;
    /// <summary>
    /// 获取预制体大小
    /// </summary>
    /// <param name="item">哪一个预制体</param>
    /// <param name="includeSpacing">是否包含间距</param>
    /// <returns></returns>
    protected abstract float GetSize(RectTransform item, bool includeSpacing = true);
    /// <summary>
    /// 用于计算结果后 获得vector的x还是y的值
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    protected abstract float GetDimension(Vector2 vector);
    /// <summary>
    /// 绝对值
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    protected abstract float GetAbsDimension(Vector2 vector);
    /// <summary>
    /// 返回一个v2
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected abstract Vector2 GetVector(float value);
    /// <summary>
    /// 方向 默认为水平
    /// </summary>
    protected LoopScrollRectDirection direction = LoopScrollRectDirection.Horizontal;
    /// <summary>
    /// content是否初始化
    /// </summary>
    private bool m_ContentSpaceInit = false;
    /// <summary>
    /// content间距 和 上下左右的间距
    /// </summary>
    private float m_ContentSpacing = 0;
    protected float m_ContentLeftPadding = 0;
    protected float m_ContentRightPadding = 0;
    protected float m_ContentTopPadding = 0;
    protected float m_ContentBottomPadding = 0;
    /// <summary>
    /// 自定义布局组件
    /// </summary>
    protected GridLayoutGroup m_GridLayout = null;
    /// <summary>
    /// 得到间距大小
    /// </summary>
    protected float contentSpacing{
      get{
        if (m_ContentSpaceInit){
          return m_ContentSpacing;
        }
        //计算出group间距
        m_ContentSpaceInit = true;
        m_ContentSpacing = 0;
        if (m_Content != null){
          HorizontalOrVerticalLayoutGroup layout1 = m_Content.GetComponent<HorizontalOrVerticalLayoutGroup>();
          if (layout1 != null){
            m_ContentSpacing = layout1.spacing;
            m_ContentLeftPadding = layout1.padding.left;
            m_ContentRightPadding = layout1.padding.right;
            m_ContentTopPadding = layout1.padding.top;
            m_ContentBottomPadding = layout1.padding.bottom;
          }

          m_GridLayout = m_Content.GetComponent<GridLayoutGroup>();
          if (m_GridLayout != null){
            m_ContentSpacing = GetAbsDimension(m_GridLayout.spacing);
            m_ContentLeftPadding = m_GridLayout.padding.left;
            m_ContentRightPadding = m_GridLayout.padding.right;
            m_ContentTopPadding = m_GridLayout.padding.top;
            m_ContentBottomPadding = m_GridLayout.padding.bottom;
          }
        }

        return m_ContentSpacing;
      }
    }

    /// <summary>
    /// GridLayout一行最多显示多少
    /// </summary>
    private bool m_ContentConstraintCountInit = false;
    private int m_ContentConstraintCount = 0;
    protected int contentConstraintCount{
      get{
        if (m_ContentConstraintCountInit){
          return m_ContentConstraintCount;
        }

        m_ContentConstraintCountInit = true;
        m_ContentConstraintCount = 1;
        if (m_Content != null){
          GridLayoutGroup layout2 = m_Content.GetComponent<GridLayoutGroup>();
          if (layout2 != null){
            if (layout2.constraint == GridLayoutGroup.Constraint.Flexible){
              Debug.LogWarning("现在还没有自动拉伸的功能");
            }

            m_ContentConstraintCount = layout2.constraintCount;
          }
        }

        return m_ContentConstraintCount;
      }
    }

    /// <summary>
    /// 滚动中的第一行。可能在一行中有多个item。
    /// itemTypeStart 为这一行的之后一个item的index
    /// </summary>
    protected int StartLine{
      get{ return Mathf.CeilToInt((float)(itemTypeStart) / contentConstraintCount); }
    }

    /// <summary>
    /// 当前滚动行
    /// </summary>
    protected int CurrentLines{
      get{ return Mathf.CeilToInt((float)(itemTypeEnd - itemTypeStart) / contentConstraintCount); }
    }

    /// <summary>
    /// 滚动中的总行数 如果totalCount为-1则是无限列表
    /// </summary>
    protected int TotalLines{
      get{ return Mathf.CeilToInt((float)(totalCount) / contentConstraintCount); }
    }
    /// <summary>
    /// 更新item 传入
    /// 需要重载
    /// </summary>
    /// <param name="viewBounds">视口包围盒</param>
    /// <param name="contentBounds">实际content包围盒</param>
    /// <returns></returns>
    protected virtual bool UpdateItems(ref Bounds viewBounds, ref Bounds contentBounds){
      return false;
    }

    /// <summary>
    /// 一种在内容超出其容器范围时应采用何种行为的设置。
    /// </summary>
    public enum MovementType{
      /// <summary>
      /// 无限制移动。内容可以无限移动。
      /// </summary>
      Unrestricted,
      /// <summary>
      /// 弹性移动。内容允许暂时超出容器，但会被弹性拉回。
      /// </summary>
      Elastic,
      /// <summary>
      /// 限制移动。内容不能超出其容器。
      /// </summary>
      Clamped,
    }
    
    /// <summary>
    /// 实际content大小
    /// </summary>
    [SerializeField] protected RectTransform m_Content;
    /// <summary>
    /// 可以滚动的内容 用于外部访问
    /// </summary>
    public RectTransform content{
      get{ return m_Content; }
      set{ m_Content = value; }
    }
    
    [SerializeField] private bool m_Horizontal = true;
    /// <summary>
    /// 是否应该启用水平滚动
    /// </summary>
    public bool horizontal{
      get{ return m_Horizontal; }
      set{ m_Horizontal = value; }
    }

    [SerializeField] private bool m_Vertical = true;
    /// <summary>
    /// 是否应该启用垂直滚动？
    /// </summary>
    public bool vertical{
      get{ return m_Vertical; }
      set{ m_Vertical = value; }
    }
    
    [SerializeField] private MovementType m_MovementType = MovementType.Elastic;
    /// <summary>
    /// 当内容超出滚动矩形时使用的行为。
    /// </summary>
    public MovementType movementType{
      get{ return m_MovementType; }
      set{ m_MovementType = value; }
    }
    
    [SerializeField] private float m_Elasticity = 0.1f;
    /// <summary>
    /// 当内容超出滚动矩形时使用的弹性量。
    /// </summary>
    public float elasticity{
      get{ return m_Elasticity; }
      set{ m_Elasticity = value; }
    }

    [SerializeField] private bool m_Inertia = true;
    /// <summary>
    /// 是否应该启用移动惯性？
    /// </summary>
    /// <remarks>
    /// 惯性意味着滚动矩形内容在被拖拽后会继续滚动一段时间。它会根据减速率逐渐减慢。
    /// </remarks>
    public bool inertia{
      get{ return m_Inertia; }
      set{ m_Inertia = value; }
    }

    [SerializeField] private float m_DecelerationRate = 0.135f; // 仅在惯性功能启用时使用
    /// <summary>
    /// 移动减慢的速率。
    /// </summary>
    /// <remarks>
    /// 减速率是每秒的速度减少量。值为0.5表示每秒速度减半。默认值为0.135。减速率仅在启用惯性时使用。
    /// </remarks>
    public float decelerationRate{
      get{ return m_DecelerationRate; }
      set{ m_DecelerationRate = value; }
    }

    [SerializeField] private float m_ScrollSensitivity = 1.0f;
    /// <summary>
    /// 对滚轮和触控板滚动事件的敏感度。
    /// </summary>
    /// <remarks>
    /// 更高的值表示更高的敏感度。
    /// </remarks>
    public float scrollSensitivity{
      get{ return m_ScrollSensitivity; }
      set{ m_ScrollSensitivity = value; }
    }
    
    /// <summary>
    /// 视口显示区域 如果为空则挂载本脚本的区域作为视口
    /// </summary>
    [SerializeField] private RectTransform m_Viewport;
    /// <summary>
    /// 对作为内容RectTransform父级的视口RectTransform的引用。
    /// </summary>
    public RectTransform viewport{
      get{ return m_Viewport; }
      set{
        m_Viewport = value;
        SetDirtyCaching();
      }
    }

    [SerializeField] private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();
    /// <summary>
    /// 当子对象位置改变时执行的回调。
    /// </summary>
    public ScrollRectEvent onValueChanged{
      get{ return m_OnValueChanged; }
      set{ m_OnValueChanged = value; }
    }

    // 鼠标按下的位置
    private Vector2 m_PointerStartLocalCursor = Vector2.zero;
    protected Vector2 m_ContentStartPosition = Vector2.zero;
    protected Vector2 m_accumulatedDistance = Vector2.zero;
    /// <summary>
    /// 视口rect
    /// </summary>
    private RectTransform m_ViewRect;
    protected RectTransform viewRect{
      get{
        if (m_ViewRect == null)
          m_ViewRect = m_Viewport;
        if (m_ViewRect == null)
          m_ViewRect = (RectTransform)transform;
        return m_ViewRect;
      }
    }
    /// <summary>
    /// content大小
    /// </summary>
    protected Bounds m_ContentBounds;
    /// <summary>
    /// 视口大小
    /// </summary>
    private Bounds m_ViewBounds;
    /// <summary>
    /// 当前拖动的力
    /// </summary>
    private Vector2 m_Velocity;
    /// <summary>
    /// 内容的当前速度。
    /// </summary>
    /// <remarks>
    /// 速度以每秒单位定义。
    /// </remarks>
    public Vector2 velocity{
      get{ return m_Velocity; }
      set{ m_Velocity = value; }
    }
    /// <summary>
    /// 是否被拖动
    /// </summary>
    private bool m_Dragging;
    /// <summary>
    /// 是否滚动
    /// </summary>
    private bool m_Scrolling;
    /// <summary>
    /// 上一次点击的位置
    /// </summary>
    private Vector2 m_PrevPosition = Vector2.zero;
    /// <summary>
    /// 上一次的content大小
    /// </summary>
    private Bounds m_PrevContentBounds;
    /// <summary>
    /// 上一次的视口大小
    /// </summary>
    private Bounds m_PrevViewBounds;
    /// <summary>
    /// 是否需要重新布局
    /// </summary>
    [NonSerialized] private bool m_HasRebuiltLayout = false;
    
    /// <summary>
    /// 当前挂载脚本的rect
    /// </summary>
    [System.NonSerialized] private RectTransform m_Rect;
    private RectTransform rectTransform{
      get{
        if (m_Rect == null)
          m_Rect = GetComponent<RectTransform>();
        return m_Rect;
      }
    }
    
    /// <summary>
    /// 清除数据
    /// </summary>
    public void ClearCells(){
      if (Application.isPlaying){
        itemTypeStart = 0;
        itemTypeEnd = 0;
        itemTypeSize = 0;
        totalCount = 0;
        for (int i = m_Content.childCount - 1; i >= 0; i--){
          ReturnObject(m_Content.GetChild(i));
        }
      }
    }
    /// <summary>
    /// 获取第一个item
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public int GetFirstItem(out float offset){
      if (direction == LoopScrollRectDirection.Vertical)
        offset = m_ContentBounds.max.y - m_ViewBounds.max.y;
      else
        offset = m_ViewBounds.min.x - m_ContentBounds.min.x;
      int idx = 0;
      if (itemTypeEnd > itemTypeStart){
        float size = GetSize(m_Content.GetChild(0) as RectTransform);
        while (offset - size >= 0 && itemTypeStart + idx + contentConstraintCount < itemTypeEnd){
          offset -= size;
          idx += contentConstraintCount;
          size = GetSize(m_Content.GetChild(idx) as RectTransform);
        }
      }

      int item = idx + itemTypeStart;
      return item;
    }
    /// <summary>
    /// 获取最后一个item
    /// </summary>
    /// <param name="offset">content距离view的值</param>
    /// <returns></returns>
    public int GetLastItem(out float offset){
      if (direction == LoopScrollRectDirection.Vertical)
        offset = m_ViewBounds.min.y - m_ContentBounds.min.y;
      else
        offset = m_ContentBounds.max.x - m_ViewBounds.max.x;
      int idx = 0;
      if (itemTypeEnd > itemTypeStart){
        int totalChildCount = m_Content.childCount;
        float size = GetSize(m_Content.GetChild(totalChildCount - idx - 1) as RectTransform);
        while (offset - size >= 0 && itemTypeStart < itemTypeEnd - idx - contentConstraintCount){
          offset -= size;
          idx += contentConstraintCount;
          size = GetSize(m_Content.GetChild(totalChildCount - idx - 1) as RectTransform);
        }
      }

      int item = itemTypeEnd - idx;
      if (totalCount >= 0 && idx > 0 && item % contentConstraintCount != 0){
        item = (item / contentConstraintCount) * contentConstraintCount;
      }

      return item;
    }

    public enum ScrollMode{
      /// <summary>
      /// 将指定的单元格滚动到视口开始位置
      /// </summary>
      ToStart,

      /// <summary>
      /// 将指定的单元格滚动到视口中心
      /// </summary>
      ToCenter,

      /// <summary>
      /// 滚动直到指定的单元格出现在视口中
      /// </summary>
      JustAppear,
    }
    /// <summary>
    /// 滚动到index协程
    /// </summary>
    /// <param name="index">位置</param>
    /// <param name="speed">速度</param>
    /// <param name="offset">偏移</param>
    /// <param name="mode">模式</param>
    /// <returns></returns>
    IEnumerator ScrollToCellCoroutine(int index, float speed, float offset, ScrollMode mode){
      bool needMoving = true;
      while (needMoving){
        yield return null;
        if (!m_Dragging){
          float move = 0;
          if (index < itemTypeStart){
            move = -Time.deltaTime * speed;
          }
          else if (index >= itemTypeEnd){
            move = Time.deltaTime * speed;
          }
          else{
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            var m_ItemBounds = GetBounds4Item(index);
            var delta = 0.0f;
            if (mode == ScrollMode.ToStart){
              if (direction == LoopScrollRectDirection.Vertical){
                delta = reverseDirection
                  ? (m_ViewBounds.min.y - m_ItemBounds.min.y)
                  : (m_ViewBounds.max.y - m_ItemBounds.max.y);
              }
              else{
                delta = reverseDirection
                  ? (m_ItemBounds.max.x - m_ViewBounds.max.x)
                  : (m_ItemBounds.min.x - m_ViewBounds.min.x);
              }
            }
            else if (mode == ScrollMode.ToCenter){
              delta = GetDimension(m_ViewBounds.center - m_ItemBounds.center);
            }
            else // ScrollMode.FirstAppear
            {
              float min_delta = GetDimension(m_ViewBounds.min - m_ItemBounds.min);
              float max_delta = GetDimension(m_ViewBounds.max - m_ItemBounds.max);
              if (direction == LoopScrollRectDirection.Vertical){
                if (min_delta > 0){
                  delta = min_delta;
                }
                else if (max_delta < 0){
                  delta = max_delta;
                }
              }
              else{
                if (min_delta < 0){
                  delta = min_delta;
                }
                else if (max_delta > 0){
                  delta = max_delta;
                }
              }
            }

            delta += offset;
            // 检查我们是否无法继续移动
            if (totalCount >= 0){
              if (delta > 0 && itemTypeEnd == totalCount){
                m_ItemBounds = GetBounds4Item(totalCount - 1);
                // 到达底部
                if ((direction == LoopScrollRectDirection.Vertical && m_ItemBounds.min.y > m_ViewBounds.min.y) ||
                    (direction == LoopScrollRectDirection.Horizontal && m_ItemBounds.max.x < m_ViewBounds.max.x)){
                  needMoving = false;
                  break;
                }
              }
              else if (delta < 0 && itemTypeStart == 0){
                m_ItemBounds = GetBounds4Item(0);
                if ((direction == LoopScrollRectDirection.Vertical && m_ItemBounds.max.y < m_ViewBounds.max.y) ||
                    (direction == LoopScrollRectDirection.Horizontal && m_ItemBounds.min.x > m_ViewBounds.min.x)){
                  needMoving = false;
                  break;
                }
              }
            }

            float maxMove = Time.deltaTime * speed;
            if (Mathf.Abs(delta) < maxMove){
              needMoving = false;
              move = delta;
            }
            else{
              move = Mathf.Sign(delta) * maxMove;
            }
          }

          if (move != 0){
            Vector2 delta = GetVector(move);
            m_Content.anchoredPosition += delta;
            m_PrevPosition += delta;
            m_ContentStartPosition += delta;
            UpdateBounds(true);
          }
        }
      }

      StopMovement();
      UpdatePrevData();
    }

    #region 创建对象

    protected int deletedItemTypeStart = 0;
    protected int deletedItemTypeEnd = 0;
    /// <summary>
    /// 在头部开始创建对象
    /// </summary>
    /// <returns></returns>
    protected float NewItemAtStart(){
      if (totalCount >= 0 && itemTypeStart - contentConstraintCount < 0){
        return -1;
      }

      bool includeSpacing = (CurrentLines > 0);
      float size = 0;
      for (int i = 0; i < contentConstraintCount; i++){
        itemTypeStart--;
        RectTransform newItem = GetFromTempPool(itemTypeStart);
        newItem.SetSiblingIndex(deletedItemTypeStart);
        size = Mathf.Max(GetSize(newItem, includeSpacing), size);
      }

      threshold = Mathf.Max(threshold, size * 1.5f);

      if (size > 0){
        SetDirtyCaching();
        m_HasRebuiltLayout = false;
        if (!reverseDirection){
          Vector2 offset = GetVector(size);
          m_Content.anchoredPosition += offset;
          m_PrevPosition += offset;
          m_ContentStartPosition += offset;
        }

        itemTypeSize += size;
      }

      return size;
    }
    /// <summary>
    /// 头部删除对象
    /// </summary>
    /// <returns></returns>
    protected float DeleteItemAtStart(){
      // 特殊情况：当移动或拖拽时，如果我们已经到达末尾，我们不能简单地删除开始位置
      if ((m_Dragging || m_Velocity != Vector2.zero) && totalCount >= 0 &&
          itemTypeEnd >= totalCount - contentConstraintCount){
        return 0;
      }

      int availableChilds = m_Content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
      Debug.Assert(availableChilds >= 0);
      if (availableChilds == 0){
        return 0;
      }

      bool includeSpacing = (CurrentLines > 1);
      float size = 0;
      for (int i = 0; i < contentConstraintCount; i++){
        RectTransform oldItem = m_Content.GetChild(deletedItemTypeStart) as RectTransform;
        size = Mathf.Max(GetSize(oldItem, includeSpacing), size);
        ReturnToTempPool(true);
        availableChilds--;
        itemTypeStart++;

        if (availableChilds == 0){
          break;
        }
      }

      if (size > 0){
        SetDirtyCaching();
        m_HasRebuiltLayout = false;
        if (!reverseDirection){
          Vector2 offset = GetVector(size);
          m_Content.anchoredPosition -= offset;
          m_PrevPosition -= offset;
          m_ContentStartPosition -= offset;
        }

        itemTypeSize -= size;
      }

      return size;
    }
    /// <summary>
    /// 在尾部创建对象
    /// </summary>
    /// <returns></returns>
    protected float NewItemAtEnd(){
      if (totalCount >= 0 && itemTypeEnd >= totalCount){
        return -1;
      }

      bool includeSpacing = (CurrentLines > 0);
      float size = 0;
      // 问题 #4: 首先填充行到末尾
      int availableChilds = m_Content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
      int count = contentConstraintCount - (availableChilds % contentConstraintCount);
      for (int i = 0; i < count; i++){
        RectTransform newItem = GetFromTempPool(itemTypeEnd);
        newItem.SetSiblingIndex(m_Content.childCount - deletedItemTypeEnd - 1);
        size = Mathf.Max(GetSize(newItem, includeSpacing), size);
        itemTypeEnd++;
        if (totalCount >= 0 && itemTypeEnd >= totalCount){
          break;
        }
      }

      threshold = Mathf.Max(threshold, size * 1.5f);

      if (size > 0){
        SetDirtyCaching();
        m_HasRebuiltLayout = false;
        if (reverseDirection){
          Vector2 offset = GetVector(size);
          m_Content.anchoredPosition -= offset;
          m_PrevPosition -= offset;
          m_ContentStartPosition -= offset;
        }

        itemTypeSize += size;
      }

      return size;
    }
    /// <summary>
    /// 尾部删除对象
    /// </summary>
    /// <returns></returns>
    protected float DeleteItemAtEnd(){
      if ((m_Dragging || m_Velocity != Vector2.zero) && totalCount >= 0 && itemTypeStart < contentConstraintCount){
        return 0;
      }

      int availableChilds = m_Content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
      Debug.Assert(availableChilds >= 0);
      if (availableChilds == 0){
        return 0;
      }

      bool includeSpacing = (CurrentLines > 1);
      float size = 0;
      for (int i = 0; i < contentConstraintCount; i++){
        RectTransform oldItem = m_Content.GetChild(m_Content.childCount - deletedItemTypeEnd - 1) as RectTransform;
        size = Mathf.Max(GetSize(oldItem, includeSpacing), size);
        ReturnToTempPool(false);
        availableChilds--;
        itemTypeEnd--;
        if (itemTypeEnd % contentConstraintCount == 0 || availableChilds == 0){
          break; // 只删除整行
        }
      }

      if (size > 0){
        SetDirtyCaching();
        m_HasRebuiltLayout = false;
        if (reverseDirection){
          Vector2 offset = GetVector(size);
          m_Content.anchoredPosition += offset;
          m_PrevPosition += offset;
          m_ContentStartPosition += offset;
        }

        itemTypeSize -= size;
      }

      return size;
    }

    #endregion
    
    
    /// <summary>
    /// 当rebuid的时候执行
    /// </summary>
    /// <param name="executing"></param>
    public virtual void Rebuild(CanvasUpdate executing){
      if (executing == CanvasUpdate.PostLayout){
        UpdateBounds();
        UpdatePrevData();

        m_HasRebuiltLayout = true;
      }
    }
    
    /// <summary>
    /// 确保布局已重新构建
    /// </summary>
    private void EnsureLayoutHasRebuilt(){
      if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// 将两个轴的速度设置为零，使内容停止移动。
    /// </summary>
    public virtual void StopMovement(){
      m_Velocity = Vector2.zero;
    }

    /// <summary>
    /// 设置内容的锚定位置。
    /// </summary>
    protected virtual void SetContentAnchoredPosition(Vector2 position){
      if (!m_Horizontal)
        position.x = m_Content.anchoredPosition.x;
      if (!m_Vertical)
        position.y = m_Content.anchoredPosition.y;


      if ((position - m_Content.anchoredPosition).sqrMagnitude > 0.001f){
        m_Content.anchoredPosition = position;
        UpdateBounds(true);
      }
    }

    /// <summary>
    /// 更新ScrollRect上先前数据字段的辅助函数。在更改ScrollRect中的数据之前调用此函数。
    /// </summary>
    protected void UpdatePrevData(){
      if (m_Content == null)
        m_PrevPosition = Vector2.zero;
      else
        m_PrevPosition = m_Content.anchoredPosition;
      m_PrevViewBounds = m_ViewBounds;
      m_PrevContentBounds = m_ContentBounds;
    }
    
    /// <summary>
    /// 估算大小
    /// </summary>
    /// <returns></returns>
    protected float EstimiateElementSize(){
      if (CurrentLines == 0){
        return 0;
      }

      float elementSize = (itemTypeSize - contentSpacing * (CurrentLines - 1)) / CurrentLines;
      return elementSize;
    }
    /// <summary>
    /// 获取水平的偏移和大小
    /// </summary>
    /// <param name="totalSize"></param>
    /// <param name="offset"></param>
    private void GetHorizonalOffsetAndSize(out float totalSize, out float offset){
      float paddingSize = m_ContentLeftPadding + m_ContentRightPadding;
      if (sizeHelper != null){
        totalSize = sizeHelper.GetItemsSize(TotalLines).x + contentSpacing * (TotalLines - 1) + paddingSize;
        offset = m_ContentBounds.min.x - sizeHelper.GetItemsSize(StartLine).x - contentSpacing * StartLine;
      }
      else{
        float elementSize = EstimiateElementSize();
        totalSize = elementSize * TotalLines + contentSpacing * (TotalLines - 1) + paddingSize;
        offset = m_ContentBounds.min.x - elementSize * StartLine - contentSpacing * StartLine;
      }
    }
    /// <summary>
    /// 获取垂直的偏移和大小
    /// </summary>
    /// <param name="totalSize"></param>
    /// <param name="offset"></param>
    private void GetVerticalOffsetAndSize(out float totalSize, out float offset){
      float paddingSize = m_ContentTopPadding + m_ContentBottomPadding;
      if (sizeHelper != null){
        totalSize = sizeHelper.GetItemsSize(TotalLines).y + contentSpacing * (TotalLines - 1) + paddingSize;
        offset = m_ContentBounds.max.y + sizeHelper.GetItemsSize(StartLine).y + contentSpacing * StartLine;
      }
      else{
        float elementSize = EstimiateElementSize();
        totalSize = elementSize * TotalLines + contentSpacing * (TotalLines - 1) + paddingSize;
        offset = m_ContentBounds.max.y + elementSize * StartLine + contentSpacing * StartLine;
      }
    }
    
    /// <summary>
    /// 类似scrollRect的normalized 对位置进行归一化了
    /// </summary>
    public Vector2 normalizedPosition{
      get{ return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition); }
      set{
        SetNormalizedPosition(value.x, 0);
        SetNormalizedPosition(value.y, 1);
      }
    }
    public float horizontalNormalizedPosition{
      get{
        UpdateBounds();
        if (totalCount > 0 && itemTypeEnd > itemTypeStart){
          float totalSize, offset;
          GetHorizonalOffsetAndSize(out totalSize, out offset);

          if (totalSize <= m_ViewBounds.size.x)
            return (m_ViewBounds.min.x > offset) ? 1 : 0;
          return (m_ViewBounds.min.x - offset) / (totalSize - m_ViewBounds.size.x);
        }
        else
          return 0.5f;
      }
      set{ SetNormalizedPosition(value, 0); }
    }
    public float verticalNormalizedPosition{
      get{
        UpdateBounds();

        if (totalCount > 0 && itemTypeEnd > itemTypeStart){
          float totalSize, offset;
          GetVerticalOffsetAndSize(out totalSize, out offset);

          if (totalSize <= m_ViewBounds.size.y)
            return (offset > m_ViewBounds.max.y) ? 1 : 0;
          return (offset - m_ViewBounds.max.y) / (totalSize - m_ViewBounds.size.y);
        }
        else
          return 0.5f;
      }
      set{ SetNormalizedPosition(value, 1); }
    }

    /// <summary>
    /// 将水平或垂直滚动位置设置为0到1之间的值，其中0在左侧或底部。
    /// </summary>
    /// <param name="value">要设置的位置，范围在0到1之间。</param>
    /// <param name="axis">要设置的轴：0表示水平，1表示垂直。</param>
    protected virtual void SetNormalizedPosition(float value, int axis){
      if (totalCount <= 0 || itemTypeEnd <= itemTypeStart)
        return;

      EnsureLayoutHasRebuilt();
      UpdateBounds();

      float totalSize, offset;
      float newAnchoredPosition = m_Content.anchoredPosition[axis];
      if (axis == 0){
        GetHorizonalOffsetAndSize(out totalSize, out offset);

        if (totalSize >= m_ViewBounds.size.x){
          newAnchoredPosition += m_ViewBounds.min.x - value * (totalSize - m_ViewBounds.size.x) - offset;
        }
      }
      else{
        GetVerticalOffsetAndSize(out totalSize, out offset);

        if (totalSize >= m_ViewBounds.size.y){
          newAnchoredPosition -= offset - value * (totalSize - m_ViewBounds.size.y) - m_ViewBounds.max.y;
        }
      }

      Vector3 anchoredPosition = m_Content.anchoredPosition;
      if (Mathf.Abs(anchoredPosition[axis] - newAnchoredPosition) > 0.01f){
        anchoredPosition[axis] = newAnchoredPosition;
        m_Content.anchoredPosition = anchoredPosition;
        m_Velocity[axis] = 0;
        UpdateBounds(true);
      }
    }
    
    /// <summary>
    /// 橡皮筋效果,滚动到内容边界时,会产生弹性阻尼效果,而不是立即停止
    /// </summary>
    /// <param name="overStretching">超出边界的距离（正：超出底部/右边界；负：超出顶部/左边界）</param>
    /// <param name="viewSize">可视区域尺寸（垂直滚动时为视口高度，水平滚动为宽度）</param>
    /// <returns></returns>
    private static float RubberDelta(float overStretching, float viewSize){
      return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
    }

    /// <summary>
    /// 计算ScrollRect应该使用的边界。
    /// </summary>
    protected void UpdateBounds(bool updateItems = false){
      m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
      m_ContentBounds = GetBounds();

      if (m_Content == null)
        return;

      // 不要在Rebuild中执行此操作。在这里调整之前使用ContentBounds。
      if (!m_HasRebuiltLayout){
        updateItems = false;
      }

      if (Application.isPlaying && updateItems && UpdateItems(ref m_ViewBounds, ref m_ContentBounds)){
        EnsureLayoutHasRebuilt();
        m_ContentBounds = GetBounds();
      }

      Vector3 contentSize = m_ContentBounds.size;
      Vector3 contentPos = m_ContentBounds.center;
      var contentPivot = m_Content.pivot;
      AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
      m_ContentBounds.size = contentSize;
      m_ContentBounds.center = contentPos;

      if (movementType == MovementType.Clamped){
        // 调整内容，使内容边界底部（右侧）永远不会高于（在左侧）视图边界底部（右侧）。
        // 顶部（左侧）永远不会低于（在右侧）视图边界顶部（左侧）。
        // 如果内容已缩小，所有这些都可能发生。
        // 这是可行的，因为内容大小至少与视图大小一样大（因为上面调用了InternalUpdateBounds）。
        Vector2 delta = Vector2.zero;
        if (m_ViewBounds.max.x > m_ContentBounds.max.x){
          delta.x = Math.Min(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
        }
        else if (m_ViewBounds.min.x < m_ContentBounds.min.x){
          delta.x = Math.Max(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
        }

        if (m_ViewBounds.min.y < m_ContentBounds.min.y){
          delta.y = Math.Max(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
        }
        else if (m_ViewBounds.max.y > m_ContentBounds.max.y){
          delta.y = Math.Min(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
        }

        if (delta.sqrMagnitude > float.Epsilon){
          contentPos = m_Content.anchoredPosition + delta;
          if (!m_Horizontal)
            contentPos.x = m_Content.anchoredPosition.x;
          if (!m_Vertical)
            contentPos.y = m_Content.anchoredPosition.y;
          AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
        }
      }
    }
    /// <summary>
    /// 调整盒子大小
    /// </summary>
    /// <param name="viewBounds"></param>
    /// <param name="contentPivot"></param>
    /// <param name="contentSize"></param>
    /// <param name="contentPos"></param>
    internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize,
      ref Vector3 contentPos){
      // 通过添加填充确保内容边界至少与视图一样大。
      // 人们可能首先认为如果内容小于视图，应该允许滚动。
      // 然而，这不是滚动视图通常的工作方式。
      // 只有当内容*大于*视图时，滚动才*可能*。
      // 我们使用内容矩形的轴心点来决定内容边界应该向哪个方向扩展。
      // 例如，如果轴心点在顶部，边界向下扩展。
      // 当在内容上使用ContentSizeFitter时，这也很好用。
      Vector3 excess = viewBounds.size - contentSize;
      if (excess.x > 0){
        contentPos.x -= excess.x * (contentPivot.x - 0.5f);
        contentSize.x = viewBounds.size.x;
      }

      if (excess.y > 0){
        contentPos.y -= excess.y * (contentPivot.y - 0.5f);
        contentSize.y = viewBounds.size.y;
      }
    }
    /// <summary>
    /// 四个角的位置
    /// </summary>
    private readonly Vector3[] m_Corners = new Vector3[4];
    /// <summary>
    /// 获取content包围盒的大小
    /// </summary>
    /// <returns></returns>
    private Bounds GetBounds(){
      if (m_Content == null)
        return new Bounds();
      //获取计算所得m_Content矩形在世界空间中的各个角点 赋值给m_Corners
      m_Content.GetWorldCorners(m_Corners);
      //得到rect的世界转本地矩阵
      var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
      return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
    }
    /// <summary>
    /// 获取包围盒大小
    /// </summary>
    /// <param name="corners">rect四个角的世界坐标位置</param>
    /// <param name="viewWorldToLocalMatrix">rect矩阵</param>
    /// <returns></returns>
    internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix){
      var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
      var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
      //转换到本地坐标并进行大小比较 得到左下角和右上角
      for (int j = 0; j < 4; j++){
        Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
        vMin = Vector3.Min(v, vMin);
        vMax = Vector3.Max(v, vMax);
      }
      //形成四边形的包围盒 相当于鼠标点一个点 然后拉到另一个点
      //vMin 中心点 
      var bounds = new Bounds(vMin, Vector3.zero);
      //将界限扩大以涵盖该点 包含vMax
      bounds.Encapsulate(vMax);
      return bounds;
    }
    /// <summary>
    /// item大小
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private Bounds GetBounds4Item(int index){
      if (m_Content == null)
        return new Bounds();

      int offset = index - itemTypeStart;
      if (offset < 0 || offset >= m_Content.childCount)
        return new Bounds();

      var rt = m_Content.GetChild(offset) as RectTransform;
      if (rt == null)
        return new Bounds();
      rt.GetWorldCorners(m_Corners);

      var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
      return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
    }
    /// <summary>
    /// 计算偏移
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    private Vector2 CalculateOffset(Vector2 delta){
      if (totalCount < 0 || movementType == MovementType.Unrestricted)
        return delta;

      Bounds contentBound = m_ContentBounds;
      if (m_Horizontal){
        float totalSize, offset;
        GetHorizonalOffsetAndSize(out totalSize, out offset);

        Vector3 center = contentBound.center;
        center.x = offset;
        contentBound.Encapsulate(center);
        center.x = offset + totalSize;
        contentBound.Encapsulate(center);
      }

      if (m_Vertical){
        float totalSize, offset;
        GetVerticalOffsetAndSize(out totalSize, out offset);

        Vector3 center = contentBound.center;
        center.y = offset;
        contentBound.Encapsulate(center);
        center.y = offset - totalSize;
        contentBound.Encapsulate(center);
      }

      return InternalCalculateOffset(ref m_ViewBounds, ref contentBound, m_Horizontal, m_Vertical, m_MovementType,
        ref delta);
    }
    internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds, bool horizontal,
      bool vertical, MovementType movementType, ref Vector2 delta){
      Vector2 offset = Vector2.zero;
      if (movementType == MovementType.Unrestricted)
        return offset;

      Vector2 min = contentBounds.min;
      Vector2 max = contentBounds.max;

      // 提取min/max偏移以检查是否接近0，避免每帧重新计算布局（案例1010178）

      if (horizontal){
        min.x += delta.x;
        max.x += delta.x;

        float maxOffset = viewBounds.max.x - max.x;
        float minOffset = viewBounds.min.x - min.x;

        if (minOffset < -0.001f)
          offset.x = minOffset;
        else if (maxOffset > 0.001f)
          offset.x = maxOffset;
      }

      if (vertical){
        min.y += delta.y;
        max.y += delta.y;

        float maxOffset = viewBounds.max.y - max.y;
        float minOffset = viewBounds.min.y - min.y;

        if (maxOffset > 0.001f)
          offset.y = maxOffset;
        else if (minOffset < -0.001f)
          offset.y = minOffset;
      }

      return offset;
    }

    /// <summary>
    /// 重写以更改或添加到保持滚动矩形外观与其数据同步的代码。
    /// </summary>
    protected void SetDirty(){
      if (!IsActive())
        return;
      //将给定的RectTransform 标记为需要在下一次布局过程中重新计算其布局。
      LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    /// <summary>
    /// 重写以更改或添加到缓存数据以避免重复重操作的代码。
    /// </summary>
    protected void SetDirtyCaching(){
      if (!IsActive())
        return;

      CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
      LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    #region 生命周期
    /// <summary>
    /// 由布局系统调用。
    /// </summary>
    public virtual float minWidth{
      get{ return -1; }
    }
    /// <summary>
    /// 由布局系统调用。
    /// </summary>
    public virtual float preferredWidth{
      get{ return -1; }
    }
    /// <summary>
    /// 由布局系统调用。
    /// </summary>
    public virtual float flexibleWidth{
      get{ return -1; }
    }
    /// <summary>
    /// 由布局系统调用。
    /// </summary>
    public virtual float minHeight{
      get{ return -1; }
    }
    /// <summary>
    /// 由布局系统调用。
    /// </summary>
    public virtual float preferredHeight{
      get{ return -1; }
    }
    /// <summary>
    /// 由布局系统调用。
    /// </summary>
    public virtual float flexibleHeight{
      get{ return -1; }
    }
    /// <summary>
    /// 由布局系统调用。
    /// </summary>
    public virtual int layoutPriority{
      get{ return -1; }
    }
    /// <summary>
    /// 由布局系统调用。
    /// </summary>
    public virtual void CalculateLayoutInputHorizontal(){ }
    /// <summary>
    /// 由布局系统调用。
    /// </summary>
    public virtual void CalculateLayoutInputVertical(){ }
    /// <summary>
    /// 重新布局完成
    /// </summary>
    public virtual void LayoutComplete(){
    }
    /// <summary>
    /// 绘制完成
    /// </summary>
    public virtual void GraphicUpdateComplete(){
    }
    /// <summary>
    /// 水平发生变动
    /// </summary>
    public void SetLayoutHorizontal(){ }
    /// <summary>
    /// 垂直发生变动
    /// </summary>
    public virtual void SetLayoutVertical(){
      m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
      m_ContentBounds = GetBounds();
    }
    /// <summary>
    /// 开始拖动之前
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnInitializePotentialDrag(PointerEventData eventData){
      if (eventData.button != PointerEventData.InputButton.Left)
        return;

      m_Velocity = Vector2.zero;
    }
    public virtual void OnBeginDrag(PointerEventData eventData){
      if (eventData.button != PointerEventData.InputButton.Left)
        return;

      if (!IsActive())
        return;
      UpdateBounds();

      m_PointerStartLocalCursor = Vector2.zero;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera,
        out m_PointerStartLocalCursor);
      m_ContentStartPosition = m_Content.anchoredPosition;
      m_Dragging = true;
      
      m_accumulatedDistance = Vector2.zero;
      _currentDelayTime = BaseDelayTime;

    }

    public float speedTimeTest = 300;
    public virtual void OnEndDrag(PointerEventData eventData){
      if (eventData.button != PointerEventData.InputButton.Left)
        return;
        
      m_Dragging = false;
      Vector2 localCursor;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position,
        eventData.pressEventCamera, out localCursor);
      m_accumulatedDistance = localCursor - m_PointerStartLocalCursor;
      Debug.Log($"累计距离为:{m_accumulatedDistance}");
    }
    public virtual void OnDrag(PointerEventData eventData){
      if (!m_Dragging)
        return;

      if (eventData.button != PointerEventData.InputButton.Left)
        return;

      if (!IsActive())
        return;

      Vector2 localCursor;
      if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position,
            eventData.pressEventCamera, out localCursor))
        return;

      UpdateBounds();

      var pointerDelta = localCursor - m_PointerStartLocalCursor;
      Vector2 position = m_ContentStartPosition + pointerDelta;

      // 偏移以使内容在视图中就位。
      Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
      position += offset;
      if (m_MovementType == MovementType.Elastic){
        if (offset.x != 0)
          position.x -= RubberDelta(offset.x, m_ViewBounds.size.x);
        if (offset.y != 0)
          position.y -= RubberDelta(offset.y, m_ViewBounds.size.y);
      }

      SetContentAnchoredPosition(position);
    }
    
    protected override void OnEnable(){
      base.OnEnable();
      //注册build回调
      CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
      SetDirty();
    }
    protected override void OnDisable(){
      CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

      m_Dragging = false;
      m_Scrolling = false;
      m_HasRebuiltLayout = false;
      m_Velocity = Vector2.zero;
      LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
      base.OnDisable();
    }
    public override bool IsActive(){
      return base.IsActive() && m_Content != null;
    }
    protected override void OnRectTransformDimensionsChange(){
      SetDirty();
      if (isActiveAndEnabled){
        UpdateBounds(true);
      }
    }
    protected virtual void LateUpdate(){
      if (!m_Content)
        return;

      EnsureLayoutHasRebuilt();
      UpdateBounds();
      float deltaTime = Time.unscaledDeltaTime;
      Vector2 offset = CalculateOffset(Vector2.zero);
      if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero)){
        Vector2 position = m_Content.anchoredPosition;
        for (int axis = 0; axis < 2; axis++){
          // 如果移动是弹性的且内容与视图有偏移，则应用弹簧物理。
          if (m_MovementType == MovementType.Elastic && offset[axis] != 0){
            float speed = m_Velocity[axis];
            float smoothTime = m_Elasticity;
            if (m_Scrolling)
              smoothTime *= 3.0f;
            position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis],
              m_Content.anchoredPosition[axis] + offset[axis],
              ref speed,
              smoothTime,
              Mathf.Infinity,
              deltaTime);
            if (Mathf.Abs(speed) < 1)
              speed = 0;
            m_Velocity[axis] = speed;
          }
          // 否则根据应用了减速度的速度移动内容。
          else if (m_Inertia){
            m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
            if (Mathf.Abs(m_Velocity[axis]) < 1)
              m_Velocity[axis] = 0;
            position[axis] += m_Velocity[axis] * deltaTime;
          }
          // 如果我们既没有弹性也没有摩擦力，就不应该有速度。
          else{
            m_Velocity[axis] = 0;
          }
        }

        if (m_MovementType == MovementType.Clamped){
          offset = CalculateOffset(position - m_Content.anchoredPosition);
          position += offset;
        }

        SetContentAnchoredPosition(position);
      }

      if (m_Dragging && m_Inertia){
        Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
        m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
      }

      if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds ||
          m_Content.anchoredPosition != m_PrevPosition){
        m_OnValueChanged.Invoke(normalizedPosition);
        UpdatePrevData();
      }
      if (!m_Dragging && _currentDelayTime > 0){
        _currentDelayTime -= deltaTime;
        if (_currentDelayTime < 0){
          if (IsAutomaticAdsorption){
            ScrollToCellWithinTime(itemTypeStart + (itemTypeEnd - itemTypeStart)/2, 0.3f,0,ScrollMode.ToCenter);
          }
        }
      }
      m_Scrolling = false;
    }

    #endregion

    #region 需要继承的接口

    public virtual void Init(ScrollInitData data){
      PoolManager.Ins.Init();
      PoolManager.Ins.Add(item, transform, 5);
      totalCount = data.TotalNum;
      RefillCells(data.beginIndex);
    }

    /// <summary>
    /// 提供并更新item的数据
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="index"></param>
    protected virtual void ProvideData(BaseUpdateData data, int index){
      data.UpdateData(_data.OnUpdateData.Invoke(index,index));
    }
    /// <summary>
    /// 获取对象池子 需要重构
    /// </summary>
    /// <param name="itemIdx"></param>
    /// <returns></returns>
    protected abstract RectTransform GetFromTempPool(int itemIdx);
    /// <summary>
    /// 回收
    /// </summary>
    /// <param name="fromStart"></param>
    /// <param name="count"></param>
    protected abstract void ReturnToTempPool(bool fromStart, int count = 1);
    /// <summary>
    /// 清除池子
    /// </summary>
    protected abstract void ClearTempPool();
    protected virtual GameObject GetObject(){
      return PoolManager.Release(item);
    }
    protected virtual void ReturnObject(Transform trans){
      trans.gameObject.SetActive(false);
      trans.SetParent(transform);
    }
    #endregion

    #region API

    /// <summary>
    /// 以什么方式滚动到某个index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="speed"></param>
    /// <param name="offset"></param>
    /// <param name="mode"></param>
    public void ScrollToCell(int index, float speed, float offset = 0, ScrollMode mode = ScrollMode.ToStart){
      if (totalCount >= 0 && (index < 0 || index >= totalCount)){
        Debug.LogErrorFormat("invalid index {0}", index);
        return;
      }

      if (speed <= 0){
        Debug.LogErrorFormat("invalid speed {0}", index);
        return;
      }

      StopAllCoroutines();
      StartCoroutine(ScrollToCellCoroutine(index, speed, offset, mode));
    }
    /// <summary>
    /// 多少时间滚动到某个index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="time"></param>
    /// <param name="offset"></param>
    /// <param name="mode"></param>
    public void ScrollToCellWithinTime(int index, float time, float offset = 0, ScrollMode mode = ScrollMode.ToStart){
      if (totalCount >= 0 && (index < 0 || index >= totalCount)){
        Debug.LogErrorFormat("invalid index {0}", index);
        return;
      }

      if (time <= 0){
        Debug.LogErrorFormat("invalid time {0}", time);
        return;
      }

      if (mode == ScrollMode.JustAppear){
        Debug.LogErrorFormat("scroll mode {0} not supported yet.", mode);
        return;
      }

      StopAllCoroutines();
      float dist = 0;
      float currentOffset = 0;
      int currentFirst = reverseDirection ? GetLastItem(out currentOffset) : GetFirstItem(out currentOffset);

      int TargetLine = (index / contentConstraintCount);
      int CurrentLine = (currentFirst / contentConstraintCount);

      if (TargetLine == CurrentLine){
        dist = -currentOffset;
      }
      else{
        if (sizeHelper != null){
          dist = GetDimension(sizeHelper.GetItemsSize(currentFirst) - sizeHelper.GetItemsSize(index)) +
                 contentSpacing * (CurrentLine - TargetLine);
        }
        else{
          float elementSize = EstimiateElementSize();
          dist = elementSize * (CurrentLine - TargetLine) + contentSpacing * (CurrentLine - TargetLine);
        }
        dist -= currentOffset;
      }

      dist += offset;
      if (mode == ScrollMode.ToCenter){
        float sizeToFill = GetAbsDimension(viewRect.rect.size);
        if (sizeHelper != null){
          sizeToFill -= GetDimension(sizeHelper.GetItemsSize(index));
        }
        else{
          float elementSize = EstimiateElementSize();
          sizeToFill -= elementSize;
        }
        dist += sizeToFill * 0.5f;
      }
      if (IsAutomaticAdsorption)
      {
        if (m_Horizontal){
          float elementSize = EstimiateElementSize();
          //向左
          if (MathF.Abs(m_accumulatedDistance.x) >= elementSize * 0.2f){
            if (m_accumulatedDistance.x > 0){
              m_accumulatedDistance = Vector2.zero;
              ScrollToCellWithinTime(index - 1, time, offset, mode);
              return;
            }
          }
        }

        dist = speedTimeTest;
      }

      StartCoroutine(ScrollToCellCoroutine(index, Mathf.Abs(dist) / time, offset, mode));
    }
    /// <summary>
    /// 刷新项目数据
    /// </summary>
    public void RefreshCells(){
      if (Application.isPlaying && this.isActiveAndEnabled){
        itemTypeEnd = itemTypeStart;
        itemTypeSize = 0;
        // 如果可以的话回收项目
        for (int i = 0; i < m_Content.childCount; i++){
          if (itemTypeEnd < totalCount || totalCount < 0){
            ProvideData(m_Content.GetChild(i).GetComponent<BaseUpdateData>(), itemTypeEnd);
            if (itemTypeEnd % contentConstraintCount == 0){
              itemTypeSize += GetSize(m_Content.GetChild(i).GetComponent<RectTransform>());
            }

            itemTypeEnd++;
          }
          else{
            ReturnObject(m_Content.GetChild(i));
            i--;
          }
        }

        UpdateBounds(true);
      }
    }
    /// <summary>
    /// 从末尾的endItem重新填充单元格，同时清除现有的单元格
    /// </summary>
    public void RefillCellsFromEnd(int endItem = 0, float contentOffset = 0){
      if (!Application.isPlaying)
        return;

      itemTypeEnd = reverseDirection ? endItem : totalCount - endItem;
      itemTypeStart = itemTypeEnd;
      itemTypeSize = 0;

      if (totalCount >= 0 && itemTypeStart % contentConstraintCount != 0){
        itemTypeStart = (itemTypeStart / contentConstraintCount) * contentConstraintCount;
      }

      ReturnToTempPool(!reverseDirection, m_Content.childCount);

      float sizeToFill = GetAbsDimension(viewRect.rect.size) + contentOffset;
      float sizeFilled = 0;
      // 问题 #169: 填充最后一行
      if (itemTypeStart < itemTypeEnd){
        itemTypeEnd = itemTypeStart;
        float size = NewItemAtEnd();
        if (size >= 0){
          sizeFilled += size;
        }
      }

      while (sizeToFill > sizeFilled){
        float size = NewItemAtStart();
        if (size < 0)
          break;
        sizeFilled += size;
      }

      float sizeFilledAtStart = sizeFilled;

      // 如果还没有填满，从开始重新填充
      while (sizeToFill > sizeFilled){
        float size = NewItemAtEnd();
        if (size < 0)
          break;
        sizeFilled += size;
      }

      float sizeFilledAtEnd = sizeFilled - sizeFilledAtStart;

      Vector2 pos = m_Content.anchoredPosition;
      float padding_dist = GetAbsDimension(new Vector2(m_ContentLeftPadding + m_ContentRightPadding,
        m_ContentTopPadding + m_ContentBottomPadding));
      float offset = 0;
      if (reverseDirection)
        offset = Mathf.Max(0, sizeFilledAtEnd + padding_dist - sizeToFill) + contentOffset;
      else
        offset = Mathf.Max(0, sizeFilledAtStart + padding_dist - sizeToFill);
      if (direction == LoopScrollRectDirection.Vertical)
        pos.y = reverseDirection ? -offset : offset;
      else
        pos.x = reverseDirection ? offset : -offset;
      m_Content.anchoredPosition = pos;
      m_ContentStartPosition = pos;

      ClearTempPool();
      // 强制在这里构建边界，以便滚动条可以访问最新的边界
      LayoutRebuilder.ForceRebuildLayoutImmediate(m_Content);
      Canvas.ForceUpdateCanvases();
      UpdateBounds(false);
      StopMovement();
      UpdatePrevData();
    }
    /// <summary>
    /// 在开始位置用startItem重新填充单元格，同时清除现有的单元格
    /// </summary>
    /// <param name="startItem">要填充的第一个项目</param>
    /// <param name="contentOffset">第一个项目相对于viewBound的偏移</param>
    public void RefillCells(int startItem = 0, float contentOffset = 0){
      if (!Application.isPlaying)
        return;

      itemTypeStart = reverseDirection ? totalCount - startItem : startItem;
      if (totalCount >= 0 && itemTypeStart % contentConstraintCount != 0){
        itemTypeStart = (itemTypeStart / contentConstraintCount) * contentConstraintCount;
      }

      itemTypeEnd = itemTypeStart;
      itemTypeSize = 0;

      // 不要在这里使用`Canvas.ForceUpdateCanvases();`，否则它会新建/删除单元格来改变itemTypeStart/End
      ReturnToTempPool(reverseDirection, m_Content.childCount);

      float sizeToFill = GetAbsDimension(viewRect.rect.size) + contentOffset;
      float sizeFilled = 0;
      // 在Start时RefillCells，m_ViewBounds可能还没有准备好

      while (sizeToFill > sizeFilled){
        float size = NewItemAtEnd();
        if (size < 0)
          break;
        sizeFilled += size;
      }

      float sizeFilledAtEnd = sizeFilled;

      // 如果还没有填满，从开始重新填充
      while (sizeToFill > sizeFilled){
        float size = NewItemAtStart();
        if (size < 0)
          break;
        sizeFilled += size;
      }

      float sizeFilledAtStart = sizeFilled - sizeFilledAtEnd;

      Vector2 pos = m_Content.anchoredPosition;
      float padding_dist = GetAbsDimension(new Vector2(m_ContentLeftPadding + m_ContentRightPadding,
        m_ContentTopPadding + m_ContentBottomPadding));
      float offset = 0;
      if (reverseDirection)
        offset = Mathf.Max(0, sizeFilledAtEnd + padding_dist - sizeToFill);
      else
        offset = sizeFilledAtStart + contentOffset;
      if (direction == LoopScrollRectDirection.Vertical)
        pos.y = reverseDirection ? -offset : offset;
      else
        pos.x = reverseDirection ? offset : -offset;
      m_Content.anchoredPosition = pos;
      m_ContentStartPosition = pos;

      ClearTempPool();
      // 强制在这里构建边界，以便滚动条可以访问最新的边界
      LayoutRebuilder.ForceRebuildLayoutImmediate(m_Content);
      Canvas.ForceUpdateCanvases();
      UpdateBounds(false);
      StopMovement();
      UpdatePrevData();
    }
    /// <summary>
    /// 设置循环
    /// </summary>
    /// <param name="isLoop"></param>
    public void SetLoop(bool isLoop){
      if (isLoop) totalCount = -1;
      else totalCount = _data.TotalNum;
    }
    
    #endregion

    #region 开放数据 需要拖动

    /// <summary>
    /// 需要生成的预制体
    /// </summary>
    public GameObject item;

    #endregion

    #region 新加数据

    protected ScrollInitData _data;
    
    public bool IsAutomaticAdsorption; //自动吸附最近的Index
    private bool isAutomaticAdsorption = false;
    private bool _isAutomaticAdsorption{
      get{
        if (!IsAutomaticAdsorption) return false;
        return isAutomaticAdsorption;
      }
      set => isAutomaticAdsorption = value;
    }
    public float BaseDelayTime = 0.2f; //松开按钮延迟吸附

    private float _currentDelayTime = 0.2f;

    #endregion

    #region 注释的功能

    /// <summary>
    /// 鼠标滚轮滚动时候
    /// </summary>
    /// <param name="eventData"></param>
    // public virtual void OnScroll(PointerEventData data){
    //   if (!IsActive())
    //     return;
    //
    //   EnsureLayoutHasRebuilt();
    //   UpdateBounds();
    //
    //   Vector2 delta = data.scrollDelta;
    //   // 对于滚动事件，向下是正的，而在UI系统中向上是正的。
    //   delta.y *= -1;
    //   if (vertical && !horizontal){
    //     if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
    //       delta.y = delta.x;
    //     delta.x = 0;
    //   }
    //
    //   if (horizontal && !vertical){
    //     if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
    //       delta.x = delta.y;
    //     delta.y = 0;
    //   }
    //
    //   if (data.IsScrolling())
    //     m_Scrolling = true;
    //
    //   Vector2 position = m_Content.anchoredPosition;
    //   position += delta * m_ScrollSensitivity;
    //   if (m_MovementType == MovementType.Clamped)
    //     position += CalculateOffset(position - m_Content.anchoredPosition);
    //
    //   SetContentAnchoredPosition(position);
    //   UpdateBounds();
    // }
    
    // #if UNITY_EDITOR
    // protected override void Awake(){
    //   base.Awake();
    //   if (Application.isPlaying){
    //     float value = (reverseDirection ^ (direction == LoopScrollRectDirection.Horizontal)) ? 0 : 1;
    //     if (m_Content != null){
    //       Debug.Assert(GetAbsDimension(m_Content.pivot) == value, this);
    //       Debug.Assert(GetAbsDimension(m_Content.anchorMin) == value, this);
    //       Debug.Assert(GetAbsDimension(m_Content.anchorMax) == value, this);
    //     }
    //
    //     if (direction == LoopScrollRectDirection.Vertical)
    //       Debug.Assert(m_Vertical && !m_Horizontal, this);
    //     else
    //       Debug.Assert(!m_Vertical && m_Horizontal, this);
    //   }
    // }
    // #endif


    #endregion
    
  }
  public static class LoopScrollSizeUtils
  {
    public static float GetPreferredHeight(RectTransform item)
    {
      ILayoutElement minLayoutElement;
      ILayoutElement preferredLayoutElement;
      var minHeight = LayoutUtility.GetLayoutProperty(item, e => e.minHeight, 0, out minLayoutElement);
      var preferredHeight = LayoutUtility.GetLayoutProperty(item, e => e.preferredHeight, 0, out preferredLayoutElement);
      var result = Mathf.Max(minHeight, preferredHeight);
      if (preferredLayoutElement == null && minLayoutElement == null)
      {
        result = item.rect.height;
      }
      Debug.Assert(result > 0);
      return result;
    }
        
    public static float GetPreferredWidth(RectTransform item)
    {
      ILayoutElement minLayoutElement;
      ILayoutElement preferredLayoutElement;
      var minWidth = LayoutUtility.GetLayoutProperty(item, e => e.minWidth, 0, out minLayoutElement);
      var preferredWidth = LayoutUtility.GetLayoutProperty(item, e => e.preferredWidth, 0, out preferredLayoutElement);
      var result = Mathf.Max(minWidth, preferredWidth);
      if (preferredLayoutElement == null && minLayoutElement == null)
      {
        result = item.rect.width;
      }
      Debug.Assert(result > 0);
      return result;
    }
  }

}
//对象池
namespace ScrollRect{
  public struct ScrollInitData{
    public int beginIndex;
    public int TotalNum;

    public Func<int, int, object[]> OnUpdateData;
  }
  
  //因为没有继承Mono类，所以得添加命名空间
  //这样才能正确暴露

  [System.Serializable]
  public class Pool{
    public int Size => size;
    public int RuntimeSize => queue.Count;

    public GameObject Prefab => prefab;
    GameObject prefab;
    int size = 1;
    Queue<GameObject> queue;

    public void Init(GameObject obj, int size){
      prefab = obj;
      this.size = size;
    }

    Transform parent;

    public void SetParent(Transform parent){
      queue = new Queue<GameObject>();
      this.parent = parent;
      for (int i = 0; i < size; i++){
        queue.Enqueue(Copy());
      }
    }

    GameObject Copy(){
      GameObject copy = GameObject.Instantiate(prefab, parent);
      copy.SetActive(false);
      return copy;
    }

    GameObject AvailableObject(){
      GameObject availableObject = null;

      if (queue.Count > 0 && !queue.Peek().activeSelf)
        availableObject = queue.Dequeue();
      else availableObject = Copy();

      queue.Enqueue(availableObject);

      return availableObject;
    }

    public GameObject PrepareObject(){
      GameObject prepareObject = AvailableObject();

      prepareObject.SetActive(true);

      return prepareObject;
    }

    public GameObject PrepareObject(Vector3 pos){
      GameObject prepareObject = AvailableObject();

      prepareObject.SetActive(true);
      prepareObject.transform.position = pos;

      return prepareObject;
    }

    public GameObject PrepareObject(Vector3 pos, Quaternion rotation){
      GameObject prepareObject = AvailableObject();

      prepareObject.SetActive(true);
      prepareObject.transform.position = pos;
      prepareObject.transform.rotation = rotation;

      return prepareObject;
    }

    public GameObject PrepareObject(Vector3 pos, Quaternion rotation, Vector3 localScale){
      GameObject prepareObject = AvailableObject();

      prepareObject.SetActive(true);
      prepareObject.transform.position = pos;
      prepareObject.transform.rotation = rotation;
      prepareObject.transform.localScale = localScale;

      return prepareObject;
    }
  }

  public class PoolManager : Singleton<PoolManager>{
    public override void Init(){
      base.Init();
      dictionary = new Dictionary<GameObject, Pool>();
    }

    static Dictionary<GameObject, Pool> dictionary;

    public void Add(GameObject obj, Transform parent, int size = 20){
      if (dictionary.ContainsKey(obj)){
        Debug.LogError("存在相同的预制体" + obj.name);
      }

      var pool = new Pool();
      pool.Init(obj, size);
      pool.SetParent(parent);
      dictionary.Add(obj, pool); //key不同，值相同
    }

    public static GameObject Release(GameObject prefab){ //释放出一个预制体
      return dictionary[prefab].PrepareObject();
    }

    public static GameObject Release(GameObject prefab, Vector3 pos){ //释放出一个预制体
      return dictionary[prefab].PrepareObject(pos);
    }

    public static GameObject Release(GameObject prefab, Vector3 pos, Quaternion rotation){ //释放出一个预制体
      return dictionary[prefab].PrepareObject(pos, rotation);
    }

    public static GameObject Release(GameObject prefab, Vector3 pos, Quaternion rotation, Vector3 localScale){
      //释放出一个预制体
      return dictionary[prefab].PrepareObject(pos, rotation, localScale);
    }
  }
  
}
