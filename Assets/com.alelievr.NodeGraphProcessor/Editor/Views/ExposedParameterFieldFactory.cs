using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace GraphProcessor
{
    /// <summary>
    /// 暴露参数字段工厂类
    /// 实现IDisposable接口，用于创建和管理暴露参数的UI字段
    /// 这是一个变通类，用于在[SerializeReference]上应用的PropertyFields周围添加包装器。
    /// 因为Property Fields绑定极其缓慢（https://forum.unity.com/threads/propertyfield-extremely-slow.966191/）
    /// 并且AppliedModifiedProperties()在调用时重新创建ScriptableObject（在NGP中导致图形重新构建）
    /// 我们不能直接使用PropertyFields。此类提供了一组函数来为暴露参数创建PropertyFields
    /// 但不附加到图形，所以当我们调用AppliedModifiedProperties时，图形不会重新构建。
    /// 缺点是我们必须自己检查值变化，然后将它们应用到图形参数上，
    /// 但这比每次参数或设置更改时都必须重新创建图形要好得多。
    /// </summary>
    public class ExposedParameterFieldFactory : IDisposable
    {
        /// <summary>
        /// 基础图形
        /// 关联的基础图形对象
        /// </summary>
        BaseGraph graph;
        
        /// <summary>
        /// 暴露参数临时对象
        /// 用于序列化的临时ScriptableObject
        /// </summary>
        [SerializeField]
        ExposedParameterWorkaround  exposedParameterObject;
        
        /// <summary>
        /// 序列化对象
        /// 用于管理序列化的SerializedObject
        /// </summary>
        SerializedObject            serializedObject;
        
        /// <summary>
        /// 序列化参数属性
        /// 参数的序列化属性
        /// </summary>
        SerializedProperty          serializedParameters;

        /// <summary>
        /// 旧参数值字典
        /// 存储参数的旧值，用于检测变化
        /// </summary>
        Dictionary<ExposedParameter, object> oldParameterValues = new Dictionary<ExposedParameter, object>();
        
        /// <summary>
        /// 旧参数设置字典
        /// 存储参数的旧设置，用于检测变化
        /// </summary>
        Dictionary<ExposedParameter, ExposedParameter.Settings> oldParameterSettings = new Dictionary<ExposedParameter, ExposedParameter.Settings>();

        /// <summary>
        /// 构造函数
        /// 初始化暴露参数字段工厂
        /// </summary>
        /// <param name="graph">基础图形</param>
        /// <param name="customParameters">自定义参数列表（可选）</param>
        public ExposedParameterFieldFactory(BaseGraph graph, List<ExposedParameter> customParameters = null)
        {
            this.graph = graph;

            // 创建临时ScriptableObject用于序列化
            exposedParameterObject = ScriptableObject.CreateInstance<ExposedParameterWorkaround>();
            exposedParameterObject.graph = graph;
            exposedParameterObject.hideFlags = HideFlags.HideAndDontSave ^ HideFlags.NotEditable;
            serializedObject = new SerializedObject(exposedParameterObject);
            UpdateSerializedProperties(customParameters);
        }

        /// <summary>
        /// 更新序列化属性
        /// 更新序列化对象中的参数列表
        /// </summary>
        /// <param name="parameters">参数列表（可选）</param>
        public void UpdateSerializedProperties(List<ExposedParameter> parameters = null)
        {
            if (parameters != null)
                exposedParameterObject.parameters = parameters;
            else
                exposedParameterObject.parameters = graph.exposedParameters;
            serializedObject.Update();
            serializedParameters = serializedObject.FindProperty(nameof(ExposedParameterWorkaround.parameters));
        }

        /// <summary>
        /// 获取参数值字段
        /// 为指定参数创建值编辑字段
        /// </summary>
        /// <param name="parameter">暴露参数</param>
        /// <param name="valueChangedCallback">值变化回调</param>
        /// <returns>参数值字段的UI元素</returns>
        public VisualElement GetParameterValueField(ExposedParameter parameter, Action<object> valueChangedCallback)
        {
            serializedObject.Update();
            int propIndex = FindPropertyIndex(parameter);
            var field = new PropertyField(serializedParameters.GetArrayElementAtIndex(propIndex));
            field.Bind(serializedObject);

            VisualElement view = new VisualElement();
            view.Add(field);

            // 存储旧值并设置变化检测
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

        /// <summary>
        /// 获取参数设置字段
        /// 为指定参数创建设置编辑字段
        /// </summary>
        /// <param name="parameter">暴露参数</param>
        /// <param name="valueChangedCallback">值变化回调</param>
        /// <returns>参数设置字段的UI元素</returns>
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