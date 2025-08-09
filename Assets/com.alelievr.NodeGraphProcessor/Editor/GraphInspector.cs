using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace GraphProcessor
{
    /// <summary>
    /// 图形检查器类
    /// 提供图形在Unity检查器中的自定义显示界面
    /// 负责显示和管理图形的暴露参数
    /// </summary>
    public class GraphInspector : Editor
    {
        /// <summary>
        /// 根UI元素
        /// 检查器界面的根容器
        /// </summary>
        protected VisualElement root;
        
        /// <summary>
        /// 当前检查的图形
        /// 被检查器编辑的图形对象
        /// </summary>
        protected BaseGraph     graph;
        
        /// <summary>
        /// 暴露参数字段工厂
        /// 用于创建暴露参数的UI字段
        /// </summary>
        protected ExposedParameterFieldFactory exposedParameterFactory;

        /// <summary>
        /// 参数容器
        /// 用于容纳所有暴露参数的UI元素
        /// </summary>
        VisualElement           parameterContainer;

        /// <summary>
        /// 启用时的初始化
        /// 设置图形事件监听和参数工厂
        /// </summary>
        protected virtual void OnEnable()
        {
            graph = target as BaseGraph;
            graph.onExposedParameterListChanged += UpdateExposedParameters;
            graph.onExposedParameterModified += UpdateExposedParameters;
            if (exposedParameterFactory == null)
                exposedParameterFactory = new ExposedParameterFieldFactory(graph);
        }

        /// <summary>
        /// 禁用时的清理
        /// 移除事件监听并释放资源
        /// </summary>
        protected virtual void OnDisable()
        {
            graph.onExposedParameterListChanged -= UpdateExposedParameters;
            graph.onExposedParameterModified -= UpdateExposedParameters;
            exposedParameterFactory?.Dispose(); //  Graphs that created in GraphBehaviour sometimes gives null ref.
            exposedParameterFactory = null;
        }

        /// <summary>
        /// 创建检查器GUI
        /// 密封方法，使用UIElements创建检查器界面
        /// </summary>
        /// <returns>检查器的根UI元素</returns>
        public sealed override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            CreateInspector();
            return root;
        }

        /// <summary>
        /// 创建检查器
        /// 虚拟方法，子类可以重写以自定义检查器界面
        /// </summary>
        protected virtual void CreateInspector()
        {
            parameterContainer = new VisualElement{
                name = "ExposedParameters"
            };
            FillExposedParameters(parameterContainer);

            root.Add(parameterContainer);
        }

        /// <summary>
        /// 填充暴露参数
        /// 在参数容器中创建所有暴露参数的UI字段
        /// </summary>
        /// <param name="parameterContainer">参数容器</param>
        protected void FillExposedParameters(VisualElement parameterContainer)
        {
            if (graph.exposedParameters.Count != 0)
                parameterContainer.Add(new Label("Exposed Parameters:"));

            foreach (var param in graph.exposedParameters)
            {
                if (param.settings.isHidden)
                    continue;

                var field = exposedParameterFactory.GetParameterValueField(param, (newValue) => {
                    param.value = newValue;
                    serializedObject.ApplyModifiedProperties();
                    graph.NotifyExposedParameterValueChanged(param);
                });
                parameterContainer.Add(field);
            }
        }

        /// <summary>
        /// 更新暴露参数（单参数版本）
        /// 当单个参数发生变化时更新界面
        /// </summary>
        /// <param name="param">发生变化的参数</param>
        void UpdateExposedParameters(ExposedParameter param) => UpdateExposedParameters();

        /// <summary>
        /// 更新暴露参数
        /// 重新填充参数容器以反映最新的参数状态
        /// </summary>
        void UpdateExposedParameters()
        {
            parameterContainer.Clear();
            FillExposedParameters(parameterContainer);
        }

        /// <summary>
        /// 不使用ImGUI
        /// 密封方法，禁用传统的ImGUI检查器界面
        /// </summary>
        public sealed override void OnInspectorGUI() {}

    }
}