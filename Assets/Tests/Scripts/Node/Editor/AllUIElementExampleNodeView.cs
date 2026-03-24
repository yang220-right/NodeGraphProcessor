using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(AllUIElementExampleNode))]
public class AllUIElementExampleNodeView : BaseNodeView{
  [Flags]
  enum MyEnum{
    None = 0,
    First = 1,
    Second = 2,
    Third = 4
  }

  public override void Enable(){
    BoundsField BoundsField = new BoundsField("BoundsField"){ value = new Bounds(Vector3.zero, Vector3.one) };
    BoundsIntField BoundsIntField = new BoundsIntField("BoundsIntField")
      { value = new BoundsInt(0, 0, 0, 1, 1, 1) };
    Box Box = new Box{ style ={ marginBottom = 5 } };
    Box.Add(new Label("Box 分组框"));
    Button Button = new Button(() => Debug.Log("按钮点击")){ text = "Button" };
    ColorField ColorField = new ColorField("ColorField"){ value = Color.red };
    CurveField CurveField = new CurveField("CurveField");
    DoubleField DoubleField = new DoubleField("DoubleField"){ value = 2.71828 };
    DropdownField DropdownField = new DropdownField(new List<string>{ "选项A", "选项B", "选项C" }, 0){
      label = "DropdownField"
    };
    EnumField EnumField = new EnumField("EnumField", System.DayOfWeek.Monday);
    EnumFlagsField EnumFlagsField = new EnumFlagsField("EnumFlagsField", MyEnum.First | MyEnum.Second);
    FloatField FloatField = new FloatField("FloatField"){ value = 3.14f };
    Foldout Foldout = new Foldout{ text = "折叠面板（Foldout）", value = true };
    Foldout.Add(new Label("折叠内部内容"));
    GradientField GradientField = new GradientField("GradientField");
    GroupBox GroupBox = new GroupBox{ text = "GroupBoxGroupBoxGroupBoxGroupBox" };
    GroupBox.Add(new Label("分组内元素"));
    Hash128Field Hash128Field = new Hash128Field();
    Hash128Field.label = "Hash128 Value:";
    Hash128Field.value = new Hash128(0x8f14e45f, 0xceea167a, 0x5a36dedd, 0x4bea2543);
    HelpBox HelpBox = new HelpBox("这是帮助信息（HelpBox）", HelpBoxMessageType.Info);
    IMGUIContainer IMGUIContainer = new IMGUIContainer(() => {
      GUILayout.Label("IMGUI 区域");
      if (GUILayout.Button("IMGUI 按钮"))
        Debug.Log("IMGUI 按钮点击");
    });
    IMGUIContainer.style.backgroundColor = Color.gray;
    Image Image = new Image();
    Image.image = Texture2D.whiteTexture; // 占位白色图片
    Image.style.width = 50;
    Image.style.height = 50;
    // InspectorElement InspectorElement = new InspectorElement();
    // InspectorElement.name = "InspectorElement";
    IntegerField IntegerField = new IntegerField("IntegerField"){ value = 42 };
    Label Label = new Label("这是标签（Label）");
    LayerField LayerField = new LayerField("LayerField", 0);
    LayerMaskField LayerMaskField = new LayerMaskField("LayerMaskField", 0);
    var listData = new List<string>{ "项目1", "项目2", "项目3", "项目4" };
    ListView ListView =
      new ListView(listData, 20, () => new Label(), (elem, i) => (elem as Label).text = listData[i]);
    ListView.style.height = 100;
    LongField LongField = new LongField("LongField"){ value = 123456789L };
    MaskField MaskField = new MaskField("MaskField", new List<string>(){ "标志1", "标志2", "标志3" }, 2);
    MinMaxSlider MinMaxSlider = new MinMaxSlider("MinMaxSlider", 20, 80, 0, 100);
    // MultiColumnListView MultiColumnListView = new MultiColumnListView();
    // MultiColumnListView.columns.Add(new Column { name = "姓名", title = "姓名", width = 80 });
    // MultiColumnListView.columns.Add(new Column { name = "年龄", title = "年龄", width = 50 });
    // MultiColumnTreeView MultiColumnTreeView = new MultiColumnTreeView();
    // MultiColumnTreeView.columns.Add(new Column { name = "名称", title = "名称", width = 100 });
    ObjectField ObjectField = new ObjectField("ObjectField"){ objectType = typeof(GameObject) };
    // PopupWindow PopupWindow = new PopupWindow();
    ProgressBar ProgressBar = new ProgressBar{ title = "ProgressBar", lowValue = 0, highValue = 100, value = 45 };
    // PropertyField PropertyField = new PropertyField();
    RadioButton RadioButton = new RadioButton{ text = "选项1", value = false };
    RadioButtonGroup RadioButtonGroup = new RadioButtonGroup{ style ={ flexDirection = FlexDirection.Row } };
    RadioButtonGroup.Add(new RadioButton{ text = "选项2", value = false });
    RadioButtonGroup.Add(new RadioButton{ text = "选项3", value = true });
    RadioButtonGroup.RegisterValueChangedCallback(evt => Debug.Log($"选中值: {evt.newValue}"));
    RectField RectField = new RectField("RectField"){ value = new Rect(0, 0, 100, 50) };
    RectIntField RectIntField = new RectIntField("RectIntField"){ value = new RectInt(0, 0, 100, 50) };
    int a = 0;
    RepeatButton RepeatButton = new RepeatButton(() => Debug.Log($"长按重复{a++}"), 1, 10)
      { text = "RepeatButton" };
    ScrollView ScrollView = new ScrollView();
    ScrollView.Add(new Label("嵌套的 ScrollView 内容"));
    Scroller Scroller = new Scroller();
    Scroller.lowValue = 0; // 最小值
    Scroller.highValue = 100; // 最大值
    Scroller.value = 42; // 当前值
    Scroller.direction = SliderDirection.Vertical; // 垂直或水平
    Slider Slider = new Slider("Slider (0-100)", 0, 100){ value = 50 };
    Slider.RegisterValueChangedCallback(evt => Debug.Log($"Slider: {evt.newValue}"));
    SliderInt SliderInt = new SliderInt("SliderInt (0-10)", 0, 10){ value = 5 };
    TagField TagField = new TagField("TagField", "Untagged");
    // TextElement TextElement = new TextElement(); //文本的基类 通常使用Label
    TextField TextField = new TextField("TextField"){ value = "文本内容" };
    Toggle Toggle = new Toggle{ label = "Toggle 开关", value = true };
    Toggle.RegisterValueChangedCallback(evt => Debug.Log($"Toggle: {evt.newValue}"));
    Toolbar Toolbar = new Toolbar();
    Toolbar.Add(new ToolbarButton(() => Debug.Log("Toolbar按钮")){ text = "ToolbarButton" });
    Toolbar.Add(new ToolbarToggle(){ text = "ToolbarToggle" });
    Toolbar.Add(new ToolbarMenu(){ text = "ToolbarMenu" });
    Toolbar.Add(new ToolbarSearchField());
    Toolbar.Add(new ToolbarSpacer());
    ToolbarBreadcrumbs ToolbarBreadcrumbs = new ToolbarBreadcrumbs();
    // 添加导航项（PushItem 将项添加到末尾）
    ToolbarBreadcrumbs.PushItem("Root", () => {
      // 保留第一个元素（根），移除其他所有
      while (ToolbarBreadcrumbs.childCount > 1)
        ToolbarBreadcrumbs.PopItem();
    });
    ToolbarBreadcrumbs.PushItem("Parent", () => ToolbarBreadcrumbs.PopItem()); // 点击时移除自身
    ToolbarBreadcrumbs.PushItem("Current");
    ToolbarButton ToolbarButton = new ToolbarButton(() => Debug.Log("Toolbar按钮")){ text = "ToolbarButton" };
    ToolbarMenu ToolbarMenu = new ToolbarMenu(){ text = "ToolbarMenu" };
    ToolbarPopupSearchField ToolbarPopupSearchField = new ToolbarPopupSearchField();
    // 设置搜索文本
    ToolbarPopupSearchField.value = "keyword";
    // 注册搜索文本变化回调（用户输入时实时触发）
    ToolbarPopupSearchField.RegisterValueChangedCallback(evt => {
      Debug.Log($"搜索文本: {evt.newValue}");
      // 执行搜索过滤逻辑
    });
    // 注册搜索按钮点击或回车事件
    ToolbarPopupSearchField.RegisterCallback<KeyDownEvent>(evt => {
      if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        Debug.Log($"执行搜索: {ToolbarPopupSearchField.value}");
    });
    // 添加搜索历史菜单项
    var menu = ToolbarPopupSearchField.menu;
    menu.AppendAction("预设搜索1", a => ToolbarPopupSearchField.value = "预设1");
    menu.AppendAction("预设搜索2", a => ToolbarPopupSearchField.value = "预设2");
    menu.AppendSeparator();
    menu.AppendAction("清除历史", a => ToolbarPopupSearchField.value = "");

    ToolbarSearchField ToolbarSearchField = new ToolbarSearchField();
    ToolbarSpacer ToolbarSpacer = new ToolbarSpacer();
    ToolbarToggle ToolbarToggle = new ToolbarToggle(){ text = "ToolbarToggle" };

    TreeView TreeView = new TreeView();
    // 设置数据源（需实现 ITreeViewItemData 或使用 TreeViewItemData<T>）
    List<TreeViewItemData<string>> treeData = new List<TreeViewItemData<string>>{
      new TreeViewItemData<string>(1, "1"),
      new TreeViewItemData<string>(2, "2"),
      new TreeViewItemData<string>(3, "3")
    };
    TreeView.SetRootItems(treeData);
    // 设置制作元素和绑定回调
    TreeView.makeItem = () => new Label();
    TreeView.bindItem = (element, index) => {
      var item = TreeView.GetItemDataForIndex<string>(index);
      (element as Label).text = item;
    };
    // 处理选中事件
    TreeView.onSelectionChange += selectedItems => {
      foreach (var item in selectedItems){
        Debug.Log($"Selected: {item}");
      }
    };
    // 处理展开/折叠
    // TreeView.viewController.itemExpanded += (item) => Debug.Log($"Expanded: {item}");
    TwoPaneSplitView TwoPaneSplitView = new TwoPaneSplitView(0, 100, TwoPaneSplitViewOrientation.Horizontal);
    TwoPaneSplitView.Add(new Label("左面板"));
    TwoPaneSplitView.Add(new Label("右面板"));
    UnsignedIntegerField UnsignedIntegerField = new UnsignedIntegerField("Unsigned Integer:");
    UnsignedIntegerField.value = 12345;
    UnsignedLongField UnsignedLongField = new UnsignedLongField("Unsigned Long:");
    UnsignedLongField.value = 9223372036854775808UL;
    Vector2Field Vector2Field = new Vector2Field("Vector2Field"){ value = new Vector2(1, 2) };
    Vector2IntField Vector2IntField = new Vector2IntField("Vector2IntField"){ value = new Vector2Int(3, 4) };
    Vector3Field Vector3Field = new Vector3Field("Vector3Field"){ value = new Vector3(1, 2, 3) };
    Vector3IntField Vector3IntField = new Vector3IntField("Vector3IntField"){ value = new Vector3Int(5, 6, 7) };
    Vector4Field Vector4Field = new Vector4Field("Vector4Field"){ value = new Vector4(1, 2, 3, 4) };
    contentContainer.Add(BoundsField);
    contentContainer.Add(BoundsIntField);
    contentContainer.Add(Box);
    contentContainer.Add(Button);
    contentContainer.Add(ColorField);
    contentContainer.Add(CurveField);
    contentContainer.Add(DoubleField);
    contentContainer.Add(DropdownField);
    contentContainer.Add(EnumField);
    contentContainer.Add(EnumFlagsField);
    contentContainer.Add(FloatField);
    contentContainer.Add(Foldout);
    contentContainer.Add(GradientField);
    contentContainer.Add(GroupBox);
    contentContainer.Add(Hash128Field);
    contentContainer.Add(HelpBox);
    contentContainer.Add(IMGUIContainer);
    contentContainer.Add(Image);
    // contentContainer.Add(InspectorElement);
    contentContainer.Add(IntegerField);
    contentContainer.Add(Label);
    contentContainer.Add(LayerField);
    contentContainer.Add(LayerMaskField);
    contentContainer.Add(ListView);
    contentContainer.Add(LongField);
    contentContainer.Add(MaskField);
    contentContainer.Add(MinMaxSlider);
    // contentContainer.Add(MultiColumnListView);
    // contentContainer.Add(MultiColumnTreeView);
    contentContainer.Add(ObjectField);
    contentContainer.Add(ProgressBar);
    // contentContainer.Add(PropertyField);
    contentContainer.Add(RadioButton);
    contentContainer.Add(RadioButtonGroup);
    contentContainer.Add(RectField);
    contentContainer.Add(RectIntField);
    contentContainer.Add(RepeatButton);
    contentContainer.Add(ScrollView);
    contentContainer.Add(Scroller);
    contentContainer.Add(Slider);
    contentContainer.Add(SliderInt);
    contentContainer.Add(TagField);
    // contentContainer.Add(TextElement);
    contentContainer.Add(TextField);
    contentContainer.Add(Toggle);
    contentContainer.Add(Toolbar);
    contentContainer.Add(ToolbarBreadcrumbs);
    contentContainer.Add(ToolbarButton);
    contentContainer.Add(ToolbarMenu);
    contentContainer.Add(ToolbarPopupSearchField);
    contentContainer.Add(ToolbarSearchField);
    contentContainer.Add(ToolbarSpacer);
    contentContainer.Add(ToolbarToggle);
    contentContainer.Add(TreeView);
    contentContainer.Add(TwoPaneSplitView);
    contentContainer.Add(UnsignedIntegerField);
    contentContainer.Add(UnsignedLongField);
    contentContainer.Add(Vector2Field);
    contentContainer.Add(Vector2IntField);
    contentContainer.Add(Vector3Field);
    contentContainer.Add(Vector3IntField);
    contentContainer.Add(Vector4Field);
    TimeScaleSlider s = new TimeScaleSlider();
    contentContainer.Add(s);
  }
}