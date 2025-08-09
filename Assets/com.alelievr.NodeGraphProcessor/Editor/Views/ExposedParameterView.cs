using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;
using System;

namespace GraphProcessor
{
	/// <summary>
	/// 暴露参数视图类
	/// 继承自PinnedElementView，提供暴露参数的管理界面
	/// 用于在图形编辑器中显示、添加、编辑和删除暴露参数
	/// </summary>
	public class ExposedParameterView : PinnedElementView
	{
		/// <summary>
		/// 图形视图
		/// 暴露参数视图所属的图形视图
		/// </summary>
		protected BaseGraphView	graphView;

		/// <summary>
		/// 视图标题
		/// 暴露参数视图的显示标题
		/// </summary>
		new const string title = "Parameters";
        
        /// <summary>
        /// 暴露参数视图样式路径
        /// 暴露参数视图的样式定义文件路径
        /// </summary>
        readonly string exposedParameterViewStyle = "GraphProcessorStyles/ExposedParameterView";

        /// <summary>
        /// 黑板布局列表
        /// 存储黑板元素的布局信息
        /// </summary>
        List<Rect> blackboardLayouts = new List<Rect>();

        /// <summary>
        /// 构造函数
        /// 初始化暴露参数视图
        /// </summary>
        public ExposedParameterView()
        {
            // 加载样式表
            var style = Resources.Load<StyleSheet>(exposedParameterViewStyle);
            if (style != null)
                styleSheets.Add(style);
        }

        /// <summary>
        /// 添加按钮点击处理
        /// 显示参数类型选择菜单并创建新的暴露参数
        /// </summary>
        protected virtual void OnAddClicked()
        {
            var parameterType = new GenericMenu();

            // 为每种参数类型添加菜单项
            foreach (var paramType in GetExposedParameterTypes())
                parameterType.AddItem(new GUIContent(GetNiceNameFromType(paramType)), false, () =>
                {
                    string uniqueName = "New " + GetNiceNameFromType(paramType);

                    uniqueName = GetUniqueExposedPropertyName(uniqueName);
                    graphView.graph.AddExposedParameter(uniqueName, paramType);
                });

            parameterType.ShowAsContext();
        }

        /// <summary>
        /// 从类型获取友好名称
        /// 将类型名称转换为用户友好的显示名称
        /// </summary>
        /// <param name="type">参数类型</param>
        /// <returns>友好的显示名称</returns>
        protected string GetNiceNameFromType(Type type)
        {
            string name = type.Name;

            // 如果类型名称中存在参数，则移除它
            name = name.Replace("Parameter", "");

            return ObjectNames.NicifyVariableName(name);
        }

        /// <summary>
        /// 获取唯一的暴露属性名称
        /// 确保参数名称的唯一性，避免重复
        /// </summary>
        /// <param name="name">原始名称</param>
        /// <returns>唯一的名称</returns>
        protected string GetUniqueExposedPropertyName(string name)
        {
            // 生成唯一名称
            string uniqueName = name;
            int i = 0;
            while (graphView.graph.exposedParameters.Any(e => e.name == name))
                name = uniqueName + " " + i++;
            return name;
        }

        /// <summary>
        /// 获取暴露参数类型
        /// 获取所有可用的暴露参数类型
        /// </summary>
        /// <returns>暴露参数类型集合</returns>
        protected virtual IEnumerable< Type > GetExposedParameterTypes()
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<ExposedParameter>())
            {
                if (type.IsGenericType)
                    continue ;

                yield return type;
            }
        }

        /// <summary>
        /// 更新参数列表
        /// 重新构建暴露参数的显示列表
        /// </summary>
        protected virtual void UpdateParameterList()
        {
            content.Clear();

            // 为每个暴露参数创建显示行
            foreach (var param in graphView.graph.exposedParameters)
            {
                var row = new BlackboardRow(new ExposedParameterFieldView(graphView, param), new ExposedParameterPropertyView(graphView, param));
                row.expanded = param.settings.expanded;
                row.RegisterCallback<GeometryChangedEvent>(e => {
                    param.settings.expanded = row.expanded;
                });

                content.Add(row);
            }
        }

        /// <summary>
        /// 初始化暴露参数视图
        /// 设置视图的基本属性和事件监听
        /// </summary>
        /// <param name="graphView">图形视图</param>
        protected override void Initialize(BaseGraphView graphView)
        {
			this.graphView = graphView;
			base.title = title;
			scrollable = true;

            // 订阅参数列表变化事件
            graphView.onExposedParameterListChanged += UpdateParameterList;
            graphView.initialized += UpdateParameterList;
            Undo.undoRedoPerformed += UpdateParameterList;

            RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
            RegisterCallback<MouseDownEvent>(OnMouseDownEvent, TrickleDown.TrickleDown);
            RegisterCallback<DetachFromPanelEvent>(OnViewClosed);

            UpdateParameterList();

            // 添加暴露参数按钮
            header.Add(new Button(OnAddClicked){
                text = "+"
            });
        }

        void OnViewClosed(DetachFromPanelEvent evt)
            => Undo.undoRedoPerformed -= UpdateParameterList;

        void OnMouseDownEvent(MouseDownEvent evt)
        {
            blackboardLayouts = content.Children().Select(c => c.layout).ToList();
        }

        int GetInsertIndexFromMousePosition(Vector2 pos)
        {
            pos = content.WorldToLocal(pos);
            // 我们只需要查找y轴；
            float mousePos = pos.y;

            if (mousePos < 0)
                return 0;

            int index = 0;
            foreach (var layout in blackboardLayouts)
            {
                if (mousePos > layout.yMin && mousePos < layout.yMax)
                    return index + 1;
                index++;
            }

            return content.childCount;
        }

        void OnDragUpdatedEvent(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            int newIndex = GetInsertIndexFromMousePosition(evt.mousePosition);
            var graphSelectionDragData = DragAndDrop.GetGenericData("DragSelection");

            if (graphSelectionDragData == null)
                return;

            foreach (var obj in graphSelectionDragData as List<ISelectable>)
            {
                if (obj is ExposedParameterFieldView view)
                {
                    var blackBoardRow = view.parent.parent.parent.parent.parent.parent;
                    int oldIndex = content.Children().ToList().FindIndex(c => c == blackBoardRow);
                    // 尝试查找黑板行
                    content.Remove(blackBoardRow);

                    if (newIndex > oldIndex)
                        newIndex--;

                    content.Insert(newIndex, blackBoardRow);
                }
            }
        }

        void OnDragPerformEvent(DragPerformEvent evt)
        {
            bool updateList = false;

            int newIndex = GetInsertIndexFromMousePosition(evt.mousePosition);
            foreach (var obj in DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>)
            {
                if (obj is ExposedParameterFieldView view)
                {
                    if (!updateList)
                        graphView.RegisterCompleteObjectUndo("Moved parameters");

                    int oldIndex = graphView.graph.exposedParameters.FindIndex(e => e == view.parameter);
                    var parameter = graphView.graph.exposedParameters[oldIndex];
                    graphView.graph.exposedParameters.RemoveAt(oldIndex);

                    // 移除操作后修补新索引：
                    if (newIndex > oldIndex)
                        newIndex--;

                    graphView.graph.exposedParameters.Insert(newIndex, parameter);

                    updateList = true;
                }
            }

            if (updateList)
            {
                graphView.graph.NotifyExposedParameterListChanged();
                evt.StopImmediatePropagation();
                UpdateParameterList();
            }
        }
    }
}