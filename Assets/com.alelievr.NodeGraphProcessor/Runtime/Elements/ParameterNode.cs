using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

namespace GraphProcessor
{
	[System.Serializable]
	public class ParameterNode : BaseNode
	{
		[Input]
		public object input;

		[Output]
		public object output;

		public override string name => "Parameter";

		// 我们在图形中序列化暴露参数的GUID，以便我们可以从图形中检索真正的ExposedParameter
		[SerializeField, HideInInspector]
		public string parameterGUID;

		public ExposedParameter parameter { get; private set; }

		public event Action onParameterChanged;

		public ParameterAccessor accessor;

		protected override void Enable()
		{
			// 加载参数
			LoadExposedParameter();

			graph.onExposedParameterModified += OnParamChanged;
			if (onParameterChanged != null)
				onParameterChanged?.Invoke();
		}

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

			output = parameter.value;
		}

		void OnParamChanged(ExposedParameter modifiedParam)
		{
			if (parameter == modifiedParam)
			{
				onParameterChanged?.Invoke();
			}
		}

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

			if (accessor == ParameterAccessor.Get)
				output = parameter.value;
			else
				graph.UpdateExposedParameter(parameter.guid, input);
		}
	}

	public enum ParameterAccessor
	{
		Get,
		Set
	}
}
