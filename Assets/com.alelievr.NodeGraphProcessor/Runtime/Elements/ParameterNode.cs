using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

namespace GraphProcessor
{
	/// <summary>
	/// 参数节点类
	/// 用于在图形中访问和修改暴露参数
	/// 支持Get和Set两种访问模式，可以读取或设置参数值
	/// </summary>
	[System.Serializable]
	public class ParameterNode : BaseNode
	{
		/// <summary>
		/// 输入值
		/// 用于Set模式时接收要设置到参数的值
		/// </summary>
		[Input]
		public object input;

		/// <summary>
		/// 输出值
		/// 用于Get模式时输出参数的当前值
		/// </summary>
		[Output]
		public object output;

		/// <summary>
		/// 节点名称
		/// 在图形中显示的节点名称
		/// </summary>
		public override string name => "Parameter";

		/// <summary>
		/// 参数GUID
		/// 我们在图形中序列化暴露参数的GUID，以便我们可以从图形中检索真正的ExposedParameter
		/// </summary>
		[SerializeField, HideInInspector]
		public string parameterGUID;

		/// <summary>
		/// 关联的暴露参数
		/// 指向图形中实际的暴露参数对象
		/// </summary>
		public ExposedParameter parameter { get; private set; }

		/// <summary>
		/// 参数变化事件
		/// 当参数值发生变化时触发
		/// </summary>
		public event Action onParameterChanged;

		/// <summary>
		/// 参数访问模式
		/// 定义节点是用于获取还是设置参数值
		/// </summary>
		public ParameterAccessor accessor;

		/// <summary>
		/// 启用节点
		/// 初始化参数节点，加载关联的暴露参数
		/// </summary>
		protected override void Enable()
		{
			// 加载参数
			LoadExposedParameter();

			// 订阅图形参数变化事件
			graph.onExposedParameterModified += OnParamChanged;
			if (onParameterChanged != null)
				onParameterChanged?.Invoke();
		}

		/// <summary>
		/// 加载暴露参数
		/// 根据GUID从图形中获取关联的暴露参数
		/// </summary>
		void LoadExposedParameter()
		{
			parameter = graph.GetExposedParameterFromGUID(parameterGUID);

			if (parameter == null)
			{
				Debug.Log("Property \"" + parameterGUID + "\" Can't be found !");

				// 删除此节点，因为找不到属性
				graph.RemoveNode(this);
				return;
			}

			// 初始化输出值为参数的当前值
			output = parameter.value;
		}

		/// <summary>
		/// 参数变化回调
		/// 当图形中的参数发生变化时的处理函数
		/// </summary>
		/// <param name="modifiedParam">被修改的参数</param>
		void OnParamChanged(ExposedParameter modifiedParam)
		{
			if (parameter == modifiedParam)
			{
				onParameterChanged?.Invoke();
			}
		}

		/// <summary>
		/// 获取输出端口
		/// 自定义输出端口行为，仅在Get模式下创建输出端口
		/// </summary>
		/// <param name="edges">连接的边列表</param>
		/// <returns>端口数据集合</returns>
		[CustomPortBehavior(nameof(output))]
		IEnumerable<PortData> GetOutputPort(List<SerializableEdge> edges)
		{
			if (accessor == ParameterAccessor.Get)
			{
				yield return new PortData
				{
					identifier = "output",
					displayName = "Value",
					displayType = (parameter == null) ? typeof(object) : parameter.GetValueType(),
					acceptMultipleEdges = true
				};
			}
		}

		/// <summary>
		/// 获取输入端口
		/// 自定义输入端口行为，仅在Set模式下创建输入端口
		/// </summary>
		/// <param name="edges">连接的边列表</param>
		/// <returns>端口数据集合</returns>
		[CustomPortBehavior(nameof(input))]
		IEnumerable<PortData> GetInputPort(List<SerializableEdge> edges)
		{
			if (accessor == ParameterAccessor.Set)
			{
				yield return new PortData
				{
					identifier = "input",
					displayName = "Value",
					displayType = (parameter == null) ? typeof(object) : parameter.GetValueType(),
				};
			}
		}

		/// <summary>
		/// 处理节点逻辑
		/// 根据访问模式执行相应的操作
		/// </summary>
		protected override void Process()
		{
#if UNITY_EDITOR // 在编辑器中，撤销/重做可以更改图形中的参数实例，在这种情况下，此类中的字段将指向错误的参数
			parameter = graph.GetExposedParameterFromGUID(parameterGUID);
#endif

			ClearMessages();
			if (parameter == null)
			{
				AddMessage($"Parameter not found: {parameterGUID}", NodeMessageType.Error);
				return;
			}

			// 根据访问模式执行相应操作
			if (accessor == ParameterAccessor.Get)
				output = parameter.value; // Get模式：输出参数值
			else
				graph.UpdateExposedParameter(parameter.guid, input); // Set模式：更新参数值
		}
	}

	/// <summary>
	/// 参数访问模式枚举
	/// 定义参数节点的访问行为
	/// </summary>
	public enum ParameterAccessor
	{
		/// <summary>
		/// 获取模式
		/// 用于读取参数值
		/// </summary>
		Get,
		
		/// <summary>
		/// 设置模式
		/// 用于修改参数值
		/// </summary>
		Set
	}
}
