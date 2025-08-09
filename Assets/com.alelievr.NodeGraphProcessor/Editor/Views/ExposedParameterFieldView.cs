using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;

namespace GraphProcessor
{
	/// <summary>
	/// 暴露参数字段视图类
	/// 继承自BlackboardField，提供暴露参数在黑板中的显示和编辑功能
	/// 用于在图形编辑器中显示和管理暴露参数
	/// </summary>
	public class ExposedParameterFieldView : BlackboardField
	{
		/// <summary>
		/// 图形视图
		/// 暴露参数字段所属的图形视图
		/// </summary>
		protected BaseGraphView	graphView;

		/// <summary>
		/// 暴露参数
		/// 对应的暴露参数对象
		/// </summary>
		public ExposedParameter	parameter { get; private set; }

		/// <summary>
		/// 构造函数
		/// 初始化暴露参数字段视图
		/// </summary>
		/// <param name="graphView">图形视图</param>
		/// <param name="param">暴露参数</param>
		public ExposedParameterFieldView(BaseGraphView graphView, ExposedParameter param) : base(null, param.name, param.shortType)
		{
			this.graphView = graphView;
			parameter = param;
			
			// 添加上下文菜单操作器
			this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
			
			// 设置图标样式和可见性
			this.Q("icon").AddToClassList("parameter-" + param.shortType);
			this.Q("icon").visible = true;

			// 注册文本字段变化事件
			(this.Q("textField") as TextField).RegisterValueChangedCallback((e) => {
				param.name = e.newValue;
				text = e.newValue;
				graphView.graph.UpdateExposedParameterName(param, e.newValue);
			});
        }

		/// <summary>
		/// 构建上下文菜单
		/// 创建暴露参数的右键菜单选项
		/// </summary>
		/// <param name="evt">上下文菜单事件</param>
		void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // 添加重命名选项
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            
            // 添加删除选项
            evt.menu.AppendAction("Delete", (a) => graphView.graph.RemoveExposedParameter(parameter), DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
	}
}