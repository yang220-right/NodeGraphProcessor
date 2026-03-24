using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEditor;

/// <summary>
/// 可拖拽的时间刻度条控件
/// </summary>
public class TimeScaleSlider : VisualElement
{
    private VisualElement content;

    // 子元素
    private VisualElement track; // 轨道背景
    private VisualElement ticksContainer; // 刻度容器（用于绘制刻度和标签）
    private VisualElement handleContainer; // 拖拽指针容器
    private VisualElement handle;//拖拽指针

    // 绘制
    private float defaultContentHeight = 80;//整个容器高度
    private Color drawColor = Color.gray; //颜色
    private Color drawSelect = Color.red; //颜色
    private float handContentHeight = 80;//指针容器高度
    private float handContentWidth = 20;//宽
    private float handContentLeftOffset => -handContentWidth / 2;
    private float trackHeight = 2; //轨道宽度
    private float ticksWidth = 1; //刻度宽度
    private float ticksLength = 10; //刻度长度
    private float handWidth = 1;//指针宽度

    public TimeScaleSlider()
    {
#if UNITY_EDITOR
        EditorApplication.update += OnEditorUpdate;
#endif
        SetSettings();
        CreateContent();
        CreateTrack();
        CreateTicks();

        SetTicksCount();
        SetPointPos();
        // 监听布局变化，当尺寸改变时重新计算位置
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        //当元素从视觉上中移除,例如当窗口关闭 空间被摧毁等 就会出发DetachFromPannelEvent
        // RegisterCallback<AttachToPanelEvent>(OnAttachFromPanel);
        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }

    #region API

    
    /// <summary>
    /// 默认120
    /// </summary>
    /// <param name="count"></param>
    public void SetTicksCount(int count = 120)
    {
        tickCount = count;
        DrawHand();
        ticksContainer.MarkDirtyRepaint();
    }

    public void SetPointPos(int index = 10)
    {
        currentPointIndex = index;
        DrawHand();
        ticksContainer.MarkDirtyRepaint();
    }

    #endregion

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        ticksContainer.MarkDirtyRepaint();
    }

    private void SetSettings()
    {
        // 设置样式：必须能容纳子元素，并允许绝对定位
        style.position = Position.Relative;
        style.flexShrink = 1; //高度自适应
        style.justifyContent = Justify.Center; //子物体居中
        // style.alignItems = Align.FlexStart;
        style.height = defaultContentHeight; // 默认高度
    }

    private void CreateContent()
    {
        content = new VisualElement();
        content.style.flexDirection = FlexDirection.Column;
        content.style.position = Position.Relative;
        content.style.left = 0;
        content.style.right = 0;

        Add(content);
    }

    private void CreateTrack()
    {
        // 创建轨道背景（浅灰色条）
        track = new VisualElement();
        track.style.position = Position.Absolute;
        track.style.top = 0;
        track.style.left = 0;
        track.style.right = 0;
        track.style.height = trackHeight;
        track.style.backgroundColor = drawColor;
        content.Add(track);
    }

    #region Ticks

    /// <summary>
    /// 总帧数
    /// </summary>
    private int _tickCount = 120;

    private int tickCount
    {
        get => _tickCount;
        set => _tickCount = Mathf.Clamp(value, 120, 3000);
    }
    private int allTickCount => tickCount + 1;//绘制的总帧数

    private bool isPointDragging = false;
    private bool isDragging = false;
    private float mouseContentX;
    private float mouseHandX = 0;
    private const int ShowTick = 30;
    private const int MinShowTick = ShowTick + 1;//最少显示30个格子
    private const int MaxShowTick = 120 + 1;//最多显示120格子
    private int _showIntervalTick = 0;// 间隔
    private int showIntervalTick
    {
        get
        {
            if (_showIntervalTick < MinShowTick) _showIntervalTick = MinShowTick; 
            return _showIntervalTick;
        }
        set
        {
            _showIntervalTick = Mathf.Clamp(value, MinShowTick, MaxShowTick);
        }
    }
    /// <summary>
    /// 左侧起始点
    /// </summary>
    private int _beginDrawTick = 0;
    private int beginDrawTick
    {
        get
        {
            if (_beginDrawTick < 0) _beginDrawTick = 0;
            return _beginDrawTick;
        }
        set => _beginDrawTick = Mathf.Clamp(value,0,allTickCount - showIntervalTick);
    }
    private float contentWidth => ticksContainer.contentRect.width;
    /// <summary>
    /// 当前指针index
    /// </summary>
    private int _currentPointIndex = 0;

    private int currentPointIndex
    {
        get => _currentPointIndex;
        set => _currentPointIndex = Mathf.Clamp(value, 0, tickCount);
    }
    private float tickStep => contentWidth / (showIntervalTick - 1);//每个刻度的间隔
    /// <summary>
    /// 刻度缩放
    /// </summary>
    private float _ticksScale = 1;
    private float ticksScale
    {
        get => _ticksScale;
        set => _ticksScale = Mathf.Clamp(value, 1, 5);
    }
    /// <summary>
    /// 拖拽的时候超框 然后总帧数自增
    /// </summary>
    private bool isDrawOutHand = false;
    private float drawOutHandPosX = 0;
    
    private void CreateTicks()
    {
        // 创建刻度容器（用于自定义绘制）
        CreateTicksContente();
        CreateHandContent();
        CreateHand();
    }

    private void CreateTicksContente()
    {
        ticksContainer = new VisualElement();
        ticksContainer.style.position = Position.Absolute;
        ticksContainer.style.left = 0;
        ticksContainer.style.right = 0;
        ticksContainer.style.height = trackHeight + ticksLength;
        ticksContainer.style.height = handContentHeight;
        ticksContainer.generateVisualContent += DrawTicks; // 注册绘制回调
        ticksContainer.RegisterCallback<WheelEvent>(OnWheel);
        ticksContainer.RegisterCallback<MouseDownEvent>(OnMouseDown);
        ticksContainer.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        ticksContainer.RegisterCallback<MouseUpEvent>(OnMouseUp);
        content.Add(ticksContainer);
    }
    private void CreateHandContent()
    {
        handleContainer = new VisualElement();
        handleContainer.style.position = Position.Absolute;
        // handleContainer.style.backgroundColor = Color.blue;
        handleContainer.style.top = 0;
        handleContainer.style.bottom = 0;
        handleContainer.style.left = handContentLeftOffset;
        handleContainer.style.width = handContentWidth;
        handleContainer.style.height = trackHeight + ticksLength * 2;
        handleContainer.style.height = handContentHeight;
        handleContainer.RegisterCallback<MouseDownEvent>(OnPointMouseDown);
        handleContainer.RegisterCallback<MouseUpEvent>(OnPointMouseUp);
        ticksContainer.Add(handleContainer);
    }
    private void CreateHand()
    {
        handle = new VisualElement();
        handle.style.backgroundColor = drawSelect;
        handle.style.top = 0;
        handle.style.bottom = 0;
        handle.style.left = (handContentWidth - handWidth) / 2;
        handle.style.width = handWidth;
        handle.style.height = trackHeight + ticksLength * 3;
        handleContainer.Add(handle);
    }
    /// <summary>
    /// 绘制当前指针
    /// </summary>
    private void DrawHand()
    {
        //绘制指针
        int realIndex = currentPointIndex - beginDrawTick;
        if (realIndex * tickStep <= contentWidth + tickStep / 2)
            handleContainer.style.left = realIndex * tickStep + handContentLeftOffset;
        //指针显影
        handleContainer.visible = realIndex * tickStep < contentWidth + tickStep / 2 && realIndex >= 0;
    }
    
    private void ExecuteDrawOutHand(bool value)
    {
        isDrawOutHand = value;
    }
    /// <summary>
    /// 当不在容器内的时候
    /// </summary>
    /// <param name="posX"></param>
    private void DrawOutHand()
    {
        if (!isDrawOutHand) return;
        currentDragMoveTime -= GetDeltaTime();
        if (currentDragMoveTime < 0)
        {
            currentDragMoveTime = baseDragMoveTime;
            if (drawOutHandPosX < 0)
            {
                --tickCount;
                --beginDrawTick;
                --currentPointIndex;
            }
            else
            {
                ++tickCount;
                ++beginDrawTick;
                ++currentPointIndex;
            }
            DrawHand();
            ticksContainer.MarkDirtyRepaint();
        }
    }
    /// <summary>
    ///  绘制刻度线和标签（在 generateVisualContent 中实现）
    /// </summary>
    /// <param name="mgc"></param>
    private void DrawTicks(MeshGenerationContext mgc)
    {
        if (allTickCount < 2) return;
        Painter2D painter = mgc.painter2D;
        painter.lineWidth = ticksWidth;
        painter.strokeColor = drawColor;
        painter.fillColor = drawColor;
        float lineTop = 0;
        float lineBottom = trackHeight + ticksLength;
        // 绘制每个刻度的竖线（以及可选文本）
        for (int i = 0; i < allTickCount; i++)
        {
            int realIndex = i + beginDrawTick;
            //beginDrawTick endDrawTick
            float x = i * tickStep;
            if (x > contentWidth + tickStep / 2)
            {
                if (i + beginDrawTick > tickCount)
                {
                    tickCount += i;
                }
                break;
            }
            // 刻度线：从轨道上方开始画到轨道下方
            painter.BeginPath();
            painter.MoveTo(new Vector2(x, lineTop));
            painter.LineTo(new Vector2(x, lineBottom + (realIndex % 5 == 0 ? ticksLength : 0)));
            painter.Stroke();
            if (realIndex % 5 == 0)
            {
                //一个数字 fontSize / 2 个像素
                int fontSize = 12;
                int p = fontSize / 2;
                mgc.DrawText($"{realIndex}",new Vector2(x - (realIndex<10?p * 0.5f: realIndex<100 ?p :  p * 1.5f), lineTop - ticksLength - trackHeight),fontSize,Color.white);
            }
        }
    }

    private void OnPointMouseDown(MouseDownEvent e)
    {
        if (e.button != 0) return;
        isPointDragging = true;
    }
    private void OnPointMouseUp(MouseUpEvent e)
    {
        if (!isPointDragging) return;
        isPointDragging = false;
    }
    private void OnMouseDown(MouseDownEvent e)
    {
        if (e.button != 0) return;
        isDragging = true;
        mouseContentX = e.localMousePosition.x;
        currentDragMoveTime = baseDragMoveTime;
        if (isPointDragging)
            UpdateIndex(e.localMousePosition.x);
        
        // 捕获鼠标，防止拖拽超出控件范围
        ticksContainer.CaptureMouse();
        e.StopPropagation();
    }

    private float baseDragMoveTime = 0.2f;
    private float currentDragMoveTime = 0.2f;
    private void OnMouseMove(MouseMoveEvent e)
    {
        if (!isDragging) return;
        // Debug.Log($"e.localMousePosition.x : {e.localMousePosition.x}");
        ExecuteDrawOutHand(false);
        if (isPointDragging)
        {
            if (e.localMousePosition.x < 0 || e.localMousePosition.x > contentWidth)
            {
                drawOutHandPosX = e.localMousePosition.x;
                ExecuteDrawOutHand(true);
            }
            else UpdateIndex(e.localMousePosition.x);
        }
        else
        {
            if (Mathf.Abs(mouseContentX - e.localMousePosition.x) >= tickStep)
            {
                if(mouseContentX - e.localMousePosition.x < 0)
                {
                    --tickCount;
                    --beginDrawTick;
                }
                else
                {
                    ++tickCount;
                    ++beginDrawTick;
                }
                mouseContentX = e.localMousePosition.x;
                DrawHand();
                ticksContainer.MarkDirtyRepaint();
            }
        }
    }

    private void OnMouseUp(MouseUpEvent e)
    {
        if (!isDragging) return;
        isDragging = false;
        ExecuteDrawOutHand(false);
        
        if (isPointDragging)
            UpdateIndex(e.localMousePosition.x);
        isPointDragging = false;
        //释放鼠标 防止一直占用鼠标点击事件
        ticksContainer.ReleaseMouse();
    }

    private void UpdateIndex(float x)
    {
        mouseHandX = x;
        mouseHandX = Mathf.Clamp(mouseHandX, 0, contentWidth);
        int index = Mathf.RoundToInt(mouseHandX / tickStep) + beginDrawTick;
        if (currentPointIndex != index)
        {
            currentPointIndex = index;
            DrawHand();
        }
    }

    private void OnWheel(WheelEvent e)
    {
        ticksScale += e.delta.y > 0 ? -0.1f : 0.1f;
        showIntervalTick += e.delta.y > 0 ? 1 : -1;
        DrawHand();
        ticksContainer.MarkDirtyRepaint();
        //阻止事件传播
        e.StopPropagation();
    }

    #endregion

    private void OnEditorUpdate()
    {
        DrawOutHand();
    }
    
    private void OnAttachFromPanel(AttachToPanelEvent evt)
    {
#if UNITY_EDITOR
        EditorApplication.update += OnEditorUpdate;
#endif
    }

    private void OnDetachFromPanel(DetachFromPanelEvent evt)
    {
#if UNITY_EDITOR
        EditorApplication.update -= OnEditorUpdate;
#endif
    }
    /// <summary>
    /// 获取时间增量（支持编辑器和运行时）
    /// </summary>
    private float GetDeltaTime()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 编辑器模式下使用固定时间增量
            return 1f / 60f; // 假设60FPS
        }
#endif
        
        return Time.deltaTime;
    }
}