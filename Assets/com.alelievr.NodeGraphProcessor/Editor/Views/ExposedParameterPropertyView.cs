using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;

namespace GraphProcessor
{
	/// <summary>
	/// 暴露参数属性视图类
	/// 继承自VisualElement，提供暴露参数属性的显示和编辑功能
	/// 用于在检查器中显示暴露参数的设置选项
	/// </summary>
	public class ExposedParameterPropertyView : VisualElement
	{
		/// <summary>
		/// 基础图形视图
		/// 暴露参数属性视图所属的图形视图
		/// </summary>
		protected BaseGraphView baseGraphView;

		/// <summary>
		/// 暴露参数
		/// 对应的暴露参数对象
		/// </summary>
		public ExposedParameter parameter { get; private set; }

		/// <summary>
		/// 隐藏检查器切换
		/// 控制参数是否在检查器中隐藏
		/// </summary>
		public Toggle     hideInInspector { get; private set; }

		/// <summary>
		/// 构造函数
		/// 初始化暴露参数属性视图
		/// </summary>
		/// <param name="graphView">图形视图</param>
		/// <param name="param">暴露参数</param>
		public ExposedParameterPropertyView(BaseGraphView graphView, ExposedParameter param)
		{
			baseGraphView = graphView;
			parameter      = param;

			// 获取参数设置字段
			var field = graphView.exposedParameterFactory.GetParameterSettingsField(param, (newValue) => {
				param.settings = newValue as ExposedParameter.Settings;
			});

			Add(field);
		}
	}
} 