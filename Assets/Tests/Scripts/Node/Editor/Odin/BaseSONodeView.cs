using System;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.IO;
using Sirenix.OdinInspector.Editor;

/// <summary>
/// ScriptableObject属性显示包装器
/// 可以将任何SO对象的所有属性显示在BaseNodeView上
/// </summary>
public abstract class BaseSONodeView : BaseNodeView{
  protected ScriptableObject targetSO;
  private SerializedObject serializedObject;

  #region 生命周期

  public override void Enable(){
    // 设置节点视图的宽度
    SetWidth();
    // 自动创建并显示SO对象
    AutoCreateAndDisplaySO();
    // 设置界面
    SetupInspector();
  }

  /// <summary>
  /// 初始化Inspector
  /// </summary>
  private void InitializeInspector(){
    try{
      if (targetSO != null){
        // 创建SerializedObject
        serializedObject = new SerializedObject(targetSO);
        // 标记需要重建 Odin 属性树
        isOdinTreeDirty = true;
        Debug.Log($"SO Inspector 初始化成功，目标: {targetSO.GetType().Name}");
      }
    }
    catch (ExitGUIException){
      // ExitGUIException是Unity的正常行为，不需要记录错误
      return;
    }
    catch (System.Exception e){
      Debug.LogError($"SO Inspector 初始化失败: {e.Message}");
    }
  }

  /// <summary>
  /// 清理资源
  /// </summary>
  public override void Disable(){
    if (odinPropertyTree != null){
      odinPropertyTree.Dispose();
      odinPropertyTree = null;
    }

    if (serializedObject != null){
      serializedObject.Dispose();
      serializedObject = null;
    }

    if (targetSO != null){
      targetSO = null;
    }

    base.Disable();
  }

  #endregion

  #region 子类继承

  protected abstract void SetWidth();

  /// <summary>
  /// 创建目标ScriptableObject
  /// 子类需要重写此方法来创建具体的SO对象
  /// </summary>
  protected abstract ScriptableObject CreateSO();

  #endregion

  #region SO保存与加载

  /// <summary>
  /// SO文件保存路径配置
  /// </summary>
  private string soSavePath = "Assets/NodeSO";

  protected string SOSavePath{
    get => Path.Combine(soSavePath,GetType().Name);
    set => soSavePath = value;
  }

  protected string soFileName = "";

  /// <summary>
  /// 保存路径的完整路径
  /// </summary>
  protected string FullSavePath => Path.Combine(Application.dataPath, SOSavePath.Replace("Assets/", ""));

  protected ScriptableObject CreateInstance<T>() where T : ScriptableObject => ScriptableObject.CreateInstance<T>();

  /// <summary>
  /// 自动创建并显示SO对象
  /// 这个方法会在Enable时自动调用
  /// </summary>
  protected virtual void AutoCreateAndDisplaySO(){
    // 清理旧的属性树
    CleanupOdinTree();
    
    // 尝试从保存路径加载SO文件
    if (TryLoadSOFromPath()){
      Debug.Log($"从保存路径加载SO对象成功: {targetSO.GetType().Name}");
    }
    else{
      // 如果加载失败，创建新的SO对象
      targetSO = CreateSO();
      SaveSOToPath();
      if (targetSO != null){
        // 自动设置SO的名称
        targetSO.name = $"{GetType().Name}";
        // 初始化Inspector
        InitializeInspector();
        Debug.Log($"自动创建SO对象成功: {targetSO.GetType().Name}");
      }
    }
  }

  /// <summary>
  /// 获取SO文件的默认文件名
  /// 子类可以重写此方法来提供自定义文件名
  /// </summary>
  protected virtual string GetDefaultFileName(){
    if (string.IsNullOrEmpty(soFileName)){
      soFileName = $"{GetType().Name}";
    }

    return soFileName;
  }

  /// <summary>
  /// 设置SO文件的保存路径
  /// </summary>
  /// <param name="path">保存路径，相对于Assets文件夹</param>
  public void SetSavePath(string path){
    SOSavePath = path;
  }

  /// <summary>
  /// 设置SO文件的文件名
  /// </summary>
  /// <param name="fileName">文件名（不包含扩展名）</param>
  public void SetFileName(string fileName){
    soFileName = fileName;
  }

  /// <summary>
  /// 检查是否存在已保存的SO文件
  /// </summary>
  /// <returns>如果存在已保存的文件返回true，否则返回false</returns>
  public bool HasSavedFile(){
    var fullPath = GetSavedFilePath();
    return AssetDatabase.LoadAssetAtPath<ScriptableObject>(fullPath) != null;
  }

  /// <summary>
  /// 获取已保存SO文件的完整路径
  /// </summary>
  /// <returns>完整路径字符串</returns>
  public string GetSavedFilePath(){
    var fileName = GetDefaultFileName();
    return Path.Combine(SOSavePath, fileName + ".asset");
  }

  /// <summary>
  /// 重新创建SO对象
  /// </summary>
  private void RecreateSO(){
    try{
      // 清理旧的资源
      if (odinPropertyTree != null){
        odinPropertyTree.Dispose();
        odinPropertyTree = null;
      }

      // 清理现有资源
      if (serializedObject != null){
        serializedObject.Dispose();
        serializedObject = null;
      }

      if (targetSO != null){
        targetSO = null;
      }

      // 清空控件容器
      controlsContainer.Clear();
      // 重新创建和设置
      CreateSO();
      SetupInspector();
      Debug.Log("SO对象已重新创建");
    }
    catch (ExitGUIException){
      // ExitGUIException是Unity的正常行为，不需要记录错误
      return;
    }
    catch (System.Exception e){
      Debug.LogError($"重新创建SO对象时出错: {e.Message}");
    }
  }

  /// <summary>
  /// 保存SO对象到指定路径
  /// </summary>
  private void SaveSOToPath(){
    try{
      if (targetSO == null){
        Debug.LogWarning("没有SO对象可供保存。");
        return;
      }

      var fullPath = GetSavedFilePath();

      // 确保保存路径存在
      if (!Directory.Exists(Path.GetDirectoryName(fullPath))){
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
      }

      // 如果SO还没有保存过，创建新的资产文件
      if (AssetDatabase.GetAssetPath(targetSO) == ""){
        AssetDatabase.CreateAsset(targetSO, fullPath);
      }

      // 标记为已修改并保存
      EditorUtility.SetDirty(targetSO);
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      Debug.Log($"SO对象已保存到: {fullPath}");
    }
    catch (System.Exception e){
      Debug.LogError($"保存SO对象时出错: {e.Message}");
    }
  }

  /// <summary>
  /// 尝试从保存路径加载SO对象
  /// </summary>
  private bool TryLoadSOFromPath(){
    var fullPath = GetSavedFilePath();
    // 使用UnityEditor.AssetDatabase加载SO
    var loadedSO = AssetDatabase.LoadAssetAtPath<ScriptableObject>(fullPath);
    if (loadedSO != null){
      // 清理旧的属性树
      CleanupOdinTree();
      targetSO = loadedSO;
      // 重新初始化Inspector以反映加载的SO
      InitializeInspector();
      Debug.Log($"SO对象已从 {fullPath} 加载");
      return true;
    }
    else{
      Debug.LogWarning($"未找到SO对象或加载失败: {fullPath}");
      return false;
    }
  }

  #endregion

  #region UI绘制

  private IMGUIContainer imguiContainer;
  private Vector2 scrollPosition;
  private PropertyTree odinPropertyTree; // Odin 的属性树
  private bool isOdinTreeDirty = true; // 标记 Odin 属性树是否需要重建
  private float lastRepaintTime = 0f; // 上次重绘时间
  private const float REPAINT_INTERVAL = 0.1f; // 重绘间隔（秒）- 增加到0.1秒减少刷新频率
  private bool isDropdownOpen = false; // 下拉框是否打开
  private float dropdownOpenTime = 0f; // 下拉框打开时间
  private const float DROPDOWN_TIMEOUT = 2.0f; // 下拉框超时时间
  private bool isDragging = false; // 是否正在拖动
  private float dragStartTime = 0f; // 拖动开始时间
  private const float DRAG_TIMEOUT = 3.0f; // 拖动超时时间
  
  /// <summary>
  /// 设置Inspector界面
  /// </summary>
  private void SetupInspector(){
    // 如果SO对象还没有创建，尝试自动创建
    if (targetSO == null){
      AutoCreateAndDisplaySO();
    }

    if (targetSO == null){
      SetupErrorUI("目标SO对象创建失败");
      return;
    }

    // 创建IMGUI容器
    imguiContainer = CreateDefaultGUIContainer();
    // 注册IMGUI绘制回调
    imguiContainer.onGUIHandler = OnInspectorGUI;
    
    // 设置事件优先级，确保 IMGUI 事件优先处理
    SetupEventPriority();
    
    // 创建控制按钮
    var buttonContainer = CreateContent();
    // 刷新按钮
    var refreshButton = CreateButton(RefreshInspector, "刷新 Inspector");
    // 重新创建按钮
    var recreateButton = CreateButton(RecreateSO, "重新创建SO");

    buttonContainer.Add(refreshButton);
    buttonContainer.Add(recreateButton);

    // 先添加 IMGUI 容器，确保它获得更高的事件优先级
    // 使用 Insert 方法将 IMGUI 容器插入到最前面
    controlsContainer.Insert(0, imguiContainer);
    controlsContainer.Add(buttonContainer);

    // 初始化Inspector
    InitializeInspector();
  }

  /// <summary>
  /// Inspector的GUI绘制方法
  /// </summary>
  private void OnInspectorGUI(){
    if (serializedObject == null || targetSO == null){
      EditorGUILayout.HelpBox("Inspector 未初始化", MessageType.Warning);
      return;
    }

    try{
      // 检测下拉框状态
      CheckDropdownState();
      
      // 设置背景色
      GUI.backgroundColor = new Color(1, 1, 1, 1f);
      // 绘制背景
      var rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
      EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1f));
      
      // 开始滚动视图
      scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
      
      // 显示SO信息
      EditorGUILayout.LabelField("SO info", EditorStyles.boldLabel);
      EditorGUILayout.LabelField("name:", targetSO.name);

      // 显示保存路径信息
      EditorGUILayout.Space(5);
      EditorGUILayout.LabelField("Save Path Info:", EditorStyles.boldLabel);
      EditorGUILayout.LabelField("Save Path:", SOSavePath);
      EditorGUILayout.LabelField("File Name:", GetDefaultFileName() + ".asset");

      // 添加保存路径设置
      EditorGUILayout.Space(5);
      var newSavePath = EditorGUILayout.TextField("Save Path:", soSavePath);
      var newFileName = EditorGUILayout.TextField("File Name:", soFileName);
      
      // 检查路径是否发生变化
      if (newSavePath != soSavePath || newFileName != soFileName){
        soSavePath = newSavePath;
        soFileName = newFileName;
        isOdinTreeDirty = true; // 标记需要重建属性树
      }

      // 使用 Odin Inspector 绘制所有属性
      EditorGUILayout.Space(10);
      EditorGUILayout.LabelField("All Properties:", EditorStyles.boldLabel);
      EditorGUILayout.Space(5);
      
      if (serializedObject != null){
        DrawOdin();
      }

      EditorGUILayout.EndScrollView();
      
      // 应用修改（拖动期间不应用，避免值被重置）
      if (serializedObject != null && serializedObject.hasModifiedProperties && !isDragging){
        serializedObject.ApplyModifiedProperties();
        Debug.Log("SO属性已修改并应用");
      }
    }
    catch (ExitGUIException){
      // ExitGUIException是Unity的正常行为，不需要记录错误
      // 直接return即可
      return;
    }
    catch (System.Exception e){
      EditorGUILayout.HelpBox($"Inspector 绘制错误: {e.Message}", MessageType.Error);
      Debug.LogError($"Inspector 绘制错误: {e.Message}");
    }
  }



  /// <summary>
  /// 使用 Odin Inspector 绘制属性
  /// </summary>
  private void DrawOdin(){
    try{
      // 检查是否正在拖动，如果是则使用现有属性树避免重建
      if (isDragging && odinPropertyTree != null){
        odinPropertyTree.BeginDraw(true);
        odinPropertyTree.Draw(false);
        odinPropertyTree.EndDraw();
        return;
      }
      
      // 检查是否需要重建属性树
      if (odinPropertyTree == null || isOdinTreeDirty){
        if (odinPropertyTree != null){
          odinPropertyTree.Dispose();
        }
        odinPropertyTree = PropertyTree.Create(serializedObject.targetObject);
        isOdinTreeDirty = false;
        Debug.Log("Odin 属性树已重建");
      }

      if (odinPropertyTree == null) return;

      // 检查是否有事件冲突，如果有则暂停绘制
      if (HasEventConflict()){
        // 使用现有的属性树进行绘制，避免重建
        odinPropertyTree.BeginDraw(true);
        odinPropertyTree.Draw(false);
        odinPropertyTree.EndDraw();
        return;
      }

      // 使用 Odin 的 PropertyTree 来绘制属性
      // 这将自动处理所有 Odin 自定义属性（如 [ShowInInspector], [BoxGroup] 等）
      odinPropertyTree.BeginDraw(true); // true 表示包含 ScrollView
      odinPropertyTree.Draw(false); // false 表示不立即绘制 ScrollView，因为上面用了 true
      odinPropertyTree.EndDraw();
    }
    catch (ExitGUIException){
      // ExitGUIException是Unity的正常行为，不需要记录错误
      // 直接return即可
      return;
    }
    catch (System.Exception e){
      Debug.LogError($"DrawOdin 绘制错误: {e.Message}");
      EditorGUILayout.HelpBox($"Odin 绘制错误: {e.Message}", MessageType.Error);
    }
  }

  /// <summary>
  /// 刷新Inspector
  /// </summary>
  private void RefreshInspector(){
    try{
      // 检查是否需要刷新（避免频繁刷新）
      if (Time.realtimeSinceStartup - lastRepaintTime < REPAINT_INTERVAL){
        return;
      }

      // 检查是否应该暂停刷新（避免与下拉框冲突和拖动卡顿）
      if (ShouldPauseRefresh()){
        return;
      }

      if (serializedObject != null){
        serializedObject.Update();
      }

      // 只有在非拖动状态下才标记属性树为脏
      if (!isDragging){
        // 标记 Odin 属性树需要重建
        isOdinTreeDirty = true;
      }

      if (imguiContainer != null){
        imguiContainer.MarkDirtyRepaint();
      }

      lastRepaintTime = Time.realtimeSinceStartup;
      Debug.Log("Inspector 已刷新");
    }
    catch (ExitGUIException){
      // ExitGUIException是Unity的正常行为，不需要记录错误
      return;
    }
    catch (System.Exception e){
      Debug.LogError($"刷新Inspector时出错: {e.Message}");
    }
  }

  /// <summary>
  /// 设置错误UI界面
  /// </summary>
  private void SetupErrorUI(string errorMessage){
    // 创建错误标题
    var errorTitle = new Label("SO Inspector 错误");
    errorTitle.style.fontSize = 16;
    errorTitle.style.color = new Color(0.8f, 0.4f, 0.4f, 1f);
    errorTitle.style.marginBottom = 15;
    errorTitle.style.unityTextAlign = TextAnchor.MiddleCenter;

    // 创建错误信息
    var errorInfo = new Label(errorMessage);
    errorInfo.style.fontSize = 12;
    errorInfo.style.color = new Color(0.8f, 0.4f, 0.4f, 1f);
    errorInfo.style.marginBottom = 20;
    errorInfo.style.unityTextAlign = TextAnchor.MiddleCenter;

    // 创建重试按钮
    var retryButton = new Button(() => {
      CreateSO();
      SetupInspector();
    }){
      text = "重试"
    };
    retryButton.style.backgroundColor = new Color(0.8f, 0.4f, 0.4f, 0.8f);
    retryButton.style.height = 30;
    retryButton.style.width = 100;
    retryButton.style.alignSelf = Align.Center;

    // 添加控件
    controlsContainer.Add(errorTitle);
    controlsContainer.Add(errorInfo);
    controlsContainer.Add(retryButton);
  }

  #endregion

  #region 额外方法

  /// <summary>
  /// 标记 Odin 属性树需要重建
  /// </summary>
  public void MarkOdinTreeDirty(){
    isOdinTreeDirty = true;
  }

  /// <summary>
  /// 强制刷新 Inspector（忽略时间间隔限制）
  /// </summary>
  public void ForceRefreshInspector(){
    lastRepaintTime = 0f; // 重置时间，允许立即刷新
    RefreshInspector();
  }



  /// <summary>
  /// 清理 Odin 属性树资源
  /// </summary>
  private void CleanupOdinTree(){
    if (odinPropertyTree != null){
      odinPropertyTree.Dispose();
      odinPropertyTree = null;
      isOdinTreeDirty = true;
    }
  }

  /// <summary>
  /// 检测下拉框状态和拖动状态
  /// </summary>
  private void CheckDropdownState(){
    var currentEvent = Event.current;
    if (currentEvent != null){
      switch (currentEvent.type){
        case EventType.MouseDown:
          // 检查点击位置是否在下拉框区域
          var mousePos = currentEvent.mousePosition;
          var controlRect = GUILayoutUtility.GetLastRect();
          
          // 如果点击在控件区域内，可能是下拉框操作
          if (controlRect.Contains(mousePos)){
            isDropdownOpen = true;
            dropdownOpenTime = Time.realtimeSinceStartup;
            
            // 延长下拉框状态时间，确保有足够时间完成操作
            EditorApplication.delayCall += () => {
              if (Time.realtimeSinceStartup - dropdownOpenTime > DROPDOWN_TIMEOUT){
                isDropdownOpen = false;
              }
            };
          }
          break;
          
        case EventType.MouseDrag:
          // 检测拖动状态
          if (!isDragging){
            isDragging = true;
            dragStartTime = Time.realtimeSinceStartup;
            Debug.Log("检测到拖动开始");
          }
          break;
          
        case EventType.MouseUp:
          // 拖动结束
          if (isDragging){
            isDragging = false;
            Debug.Log("拖动结束");
          }
          break;
      }
    }
    
    // 检查是否有焦点控件（可能是下拉框）
    if (GUIUtility.keyboardControl != 0 && !isDropdownOpen){
      isDropdownOpen = true;
      dropdownOpenTime = Time.realtimeSinceStartup;
    }
    
    // 检查是否有热控件（正在编辑的控件）
    if (GUIUtility.hotControl != 0 && !isDropdownOpen){
      isDropdownOpen = true;
      dropdownOpenTime = Time.realtimeSinceStartup;
    }
    
    // 检查是否有弹出窗口，这通常表示下拉框已打开
    if (EditorWindow.mouseOverWindow != null && 
        EditorWindow.mouseOverWindow.GetType().Name.Contains("Popup") && !isDropdownOpen){
      isDropdownOpen = true;
      dropdownOpenTime = Time.realtimeSinceStartup;
    }
    
    // 检查拖动超时
    if (isDragging && Time.realtimeSinceStartup - dragStartTime > DRAG_TIMEOUT){
      isDragging = false;
      Debug.Log("拖动超时，重置状态");
    }
  }

  /// <summary>
  /// 检查是否应该暂停刷新（避免与下拉框冲突和拖动卡顿）
  /// </summary>
  private bool ShouldPauseRefresh(){
    // 检查是否正在拖动
    if (isDragging){
      return true;
    }
    
    // 检查是否有下拉框打开
    if (isDropdownOpen && Time.realtimeSinceStartup - dropdownOpenTime < DROPDOWN_TIMEOUT){
      return true;
    }
    
    // 检查是否有焦点控件（可能是下拉框）
    if (GUIUtility.keyboardControl != 0){
      return true;
    }
    
    // 检查是否有弹出窗口
    if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow != EditorWindow.mouseOverWindow){
      return true;
    }
    
    // 检查是否有事件冲突
    if (HasEventConflict()){
      return true;
    }
    
    return false;
  }

  /// <summary>
  /// 智能事件冲突检测
  /// </summary>
  private bool HasEventConflict(){
    var currentEvent = Event.current;
    if (currentEvent == null) return false;
    
    // 检查是否有弹出菜单
    if (EditorWindow.mouseOverWindow != null && 
        EditorWindow.mouseOverWindow.GetType().Name.Contains("Popup")){
      return true;
    }
    
    // 检查是否有热控件（正在编辑的控件）
    if (GUIUtility.hotControl != 0){
      return true;
    }
    
    // 检查是否有焦点控件（可能是下拉框）
    if (GUIUtility.keyboardControl != 0){
      return true;
    }
    
    // 移除过于激进的事件类型检测，避免干扰正常操作
    // 只在确实有冲突时才暂停刷新
    
    return false;
  }

  /// <summary>
  /// 设置事件优先级，确保 IMGUI 事件优先处理
  /// </summary>
  private void SetupEventPriority(){
    if (imguiContainer != null){
      // 确保 IMGUI 容器能够捕获所有鼠标事件
      imguiContainer.style.cursor = new StyleCursor();
      
      // 设置焦点策略，确保 IMGUI 控件能够获得焦点
      imguiContainer.focusable = true;
      imguiContainer.tabIndex = 0;
      
      // 注册事件回调，使用 TrickleDown.TrickleDown 确保事件优先处理
      // 但不阻止事件传播，只监控状态变化
      imguiContainer.RegisterCallback<MouseDownEvent>(OnIMGUIMouseDown, TrickleDown.TrickleDown);
      imguiContainer.RegisterCallback<MouseUpEvent>(OnIMGUIMouseUp, TrickleDown.TrickleDown);
      
      // 注册焦点事件，监控焦点变化
      imguiContainer.RegisterCallback<FocusInEvent>(OnIMGUIFocusIn, TrickleDown.TrickleDown);
      imguiContainer.RegisterCallback<FocusOutEvent>(OnIMGUIFocusOut, TrickleDown.TrickleDown);
    }
  }

  /// <summary>
  /// IMGUI 鼠标按下事件处理
  /// </summary>
  private void OnIMGUIMouseDown(MouseDownEvent evt){
    // 标记下拉框可能打开
    isDropdownOpen = true;
    dropdownOpenTime = Time.realtimeSinceStartup;
    
    // 检查是否点击在 IMGUI 容器内
    if (imguiContainer != null && imguiContainer.worldBound.Contains(evt.mousePosition)){
      // 不阻止事件传播，让 IMGUI 能够正常处理
      // 只标记状态，不干扰正常的事件流程
      Debug.Log("IMGUI 容器内鼠标按下，标记下拉框状态");
    }
  }

  /// <summary>
  /// IMGUI 鼠标抬起事件处理
  /// </summary>
  private void OnIMGUIMouseUp(MouseUpEvent evt){
    // 延迟重置下拉框状态
    EditorApplication.delayCall += () => {
      if (Time.realtimeSinceStartup - dropdownOpenTime > DROPDOWN_TIMEOUT){
        isDropdownOpen = false;
      }
    };
    
    // 如果拖动结束，确保值被保存
    if (isDragging){
      isDragging = false;
      // 强制保存当前值
      if (serializedObject != null && serializedObject.hasModifiedProperties){
        serializedObject.ApplyModifiedProperties();
        Debug.Log("拖动结束，保存修改的值");
      }
    }
    
    // 不阻止事件传播，让 IMGUI 能够正常处理
    Debug.Log("IMGUI 鼠标抬起，延迟重置下拉框状态");
  }

  /// <summary>
  /// IMGUI 鼠标移动事件处理
  /// </summary>
  private void OnIMGUIMouseMove(MouseMoveEvent evt){
    // 如果鼠标在移动，可能是在操作下拉框
    if (isDropdownOpen){
      dropdownOpenTime = Time.realtimeSinceStartup; // 延长下拉框状态时间
    }
    
    // 不阻止事件传播，让 IMGUI 能够正常处理
    // evt.StopPropagation();
    // evt.PreventDefault();
  }

  /// <summary>
  /// IMGUI 键盘按下事件处理
  /// </summary>
  private void OnIMGUIKeyDown(KeyDownEvent evt){
    // 标记可能有键盘操作（如下拉框导航）
    if (evt.keyCode == KeyCode.DownArrow || evt.keyCode == KeyCode.UpArrow || 
        evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Escape){
      isDropdownOpen = true;
      dropdownOpenTime = Time.realtimeSinceStartup;
    }
    
    // 不阻止事件传播，让 IMGUI 能够正常处理
    // evt.StopPropagation();
    // evt.PreventDefault();
  }

  /// <summary>
  /// IMGUI 键盘抬起事件处理
  /// </summary>
  private void OnIMGUIKeyUp(KeyUpEvent evt){
    // 延迟重置下拉框状态
    EditorApplication.delayCall += () => {
      if (Time.realtimeSinceStartup - dropdownOpenTime > DROPDOWN_TIMEOUT){
        isDropdownOpen = false;
      }
    };
    
    // 不阻止事件传播，让 IMGUI 能够正常处理
    // evt.StopPropagation();
    // evt.PreventDefault();
  }

  /// <summary>
  /// IMGUI 获得焦点事件处理
  /// </summary>
  private void OnIMGUIFocusIn(FocusInEvent evt){
    // IMGUI 容器获得焦点时，确保能够处理事件
    Debug.Log("IMGUI 容器获得焦点");
  }

  /// <summary>
  /// IMGUI 失去焦点事件处理
  /// </summary>
  private void OnIMGUIFocusOut(FocusOutEvent evt){
    // IMGUI 容器失去焦点时，重置下拉框状态
    isDropdownOpen = false;
    Debug.Log("IMGUI 容器失去焦点");
  }



  public VisualElement CreateContent(){
    var content = new VisualElement();
    content.style.flexDirection = FlexDirection.Row;
    content.style.justifyContent = Justify.SpaceAround;
    return content;
  }

  /// <summary>
  /// 创建一个默认的IMGUIContainer
  /// </summary>
  public IMGUIContainer CreateDefaultGUIContainer(){
    var temp = new IMGUIContainer();
    // 设置基本样式以确保正常显示
    temp.style.flexGrow = 1;
    temp.style.minHeight = 200;
    
    return temp;
  }

  public Label CreateLabel(string text){
    var statusLabel = new Label(text);
    statusLabel.style.fontSize = 11;
    statusLabel.style.color = new Color(0.4f, 0.8f, 0.4f, 1f);
    statusLabel.style.marginBottom = 10;
    statusLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
    return statusLabel;
  }

  public Button CreateButton(Action ac, string label = ""){
    var recreateButton = new Button(ac){
      text = label
    };
    recreateButton.style.backgroundColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
    recreateButton.style.borderTopWidth = 1;
    recreateButton.style.borderBottomWidth = 1;
    recreateButton.style.borderLeftWidth = 1;
    recreateButton.style.borderRightWidth = 1;
    recreateButton.style.borderTopColor = new Color(1f, 0.8f, 0.4f, 1f);
    recreateButton.style.borderBottomColor = new Color(1f, 0.8f, 1f, 1f);
    recreateButton.style.borderLeftColor = new Color(1f, 0.8f, 0.4f, 1f);
    recreateButton.style.borderRightColor = new Color(1f, 0.8f, 0.4f, 1f);
    recreateButton.style.height = 30;
    recreateButton.style.flexGrow = 1;
    recreateButton.style.marginLeft = 5;

    return recreateButton;
  }

  #endregion
}