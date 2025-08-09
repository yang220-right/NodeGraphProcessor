using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace GraphProcessor
{
    // 所以，这是一个变通类，用于在[SerializeReference]上应用的PropertyFields周围添加包装器。
    // 因为Property Fields绑定极其缓慢（https://forum.unity.com/threads/propertyfield-extremely-slow.966191/）
    // 并且AppliedModifiedProperties()在调用时重新创建ScriptableObject（在NGP中导致图形重新构建）
    // 我们不能直接使用PropertyFields。此类提供了一组函数来为暴露参数创建PropertyFields
    // 但不附加到图形，所以当我们调用AppliedModifiedProperties时，图形不会重新构建。
    // 缺点是我们必须自己检查值变化，然后将它们应用到图形参数上，
    // 但这比每次参数或设置更改时都必须重新创建图形要好得多。
    public class ExposedParameterFieldFactory : IDisposable
    {
        BaseGraph graph;
        [SerializeField]
        ExposedParameterWorkaround  exposedParameterObject;
        SerializedObject            serializedObject;
        SerializedProperty          serializedParameters;

        Dictionary<ExposedParameter, object> oldParameterValues = new Dictionary<ExposedParameter, object>();
        Dictionary<ExposedParameter, ExposedParameter.Settings> oldParameterSettings = new Dictionary<ExposedParameter, ExposedParameter.Settings>();

        public ExposedParameterFieldFactory(BaseGraph graph, List<ExposedParameter> customParameters = null)
        {
            this.graph = graph;

            exposedParameterObject = ScriptableObject.CreateInstance<ExposedParameterWorkaround>();
            exposedParameterObject.graph = graph;
            exposedParameterObject.hideFlags = HideFlags.HideAndDontSave ^ HideFlags.NotEditable;
            serializedObject = new SerializedObject(exposedParameterObject);
            UpdateSerializedProperties(customParameters);
        }

        public void UpdateSerializedProperties(List<ExposedParameter> parameters = null)
        {
            if (parameters != null)
                exposedParameterObject.parameters = parameters;
            else
                exposedParameterObject.parameters = graph.exposedParameters;
            serializedObject.Update();
            serializedParameters = serializedObject.FindProperty(nameof(ExposedParameterWorkaround.parameters));
        }

        public VisualElement GetParameterValueField(ExposedParameter parameter, Action<object> valueChangedCallback)
        {
            serializedObject.Update();
            int propIndex = FindPropertyIndex(parameter);
            var field = new PropertyField(serializedParameters.GetArrayElementAtIndex(propIndex));
            field.Bind(serializedObject);

            VisualElement view = new VisualElement();
            view.Add(field);

            oldParameterValues[parameter] = parameter.value;
            view.Add(new IMGUIContainer(() =>
            {
                if (oldParameterValues.TryGetValue(parameter, out var value))
                {
                    if (parameter.value != null && !parameter.value.Equals(value))
                        valueChangedCallback(parameter.value);
                }
                oldParameterValues[parameter] = parameter.value;
            }));

			// 当图形未链接到场景时，禁止选择场景对象
            if (!this.graph.IsLinkedToScene())
            {
				var objectField = view.Q<ObjectField>();
				if (objectField != null)
					objectField.allowSceneObjects = false;
            }
            return view;
        }

        public VisualElement GetParameterSettingsField(ExposedParameter parameter, Action<object> valueChangedCallback)
        {
            serializedObject.Update();
            int propIndex = FindPropertyIndex(parameter);
            var serializedParameter = serializedParameters.GetArrayElementAtIndex(propIndex);
            serializedParameter.managedReferenceValue = exposedParameterObject.parameters[propIndex];
            var serializedSettings = serializedParameter.FindPropertyRelative(nameof(ExposedParameter.settings));
            serializedSettings.managedReferenceValue = exposedParameterObject.parameters[propIndex].settings;
            var settingsField = new PropertyField(serializedSettings);
            settingsField.Bind(serializedObject);

            VisualElement view = new VisualElement();
            view.Add(settingsField);

            // TODO: 看看我们是否可以用事件替换这个
            oldParameterSettings[parameter] = parameter.settings;
            view.Add(new IMGUIContainer(() =>
            {
                if (oldParameterSettings.TryGetValue(parameter, out var settings))
                {
                    if (!settings.Equals(parameter.settings))
                        valueChangedCallback(parameter.settings);
                }
                oldParameterSettings[parameter] = parameter.settings;
            }));

            return view;
        }

        public void ResetOldParameter(ExposedParameter parameter)
        {
            oldParameterValues.Remove(parameter);
            oldParameterSettings.Remove(parameter);
        }

        int FindPropertyIndex(ExposedParameter param) => exposedParameterObject.parameters.FindIndex(p => p == param);

        public void Dispose()
        {
            GameObject.DestroyImmediate(exposedParameterObject);
        }
    }
}