// using System.Collections;
// using System.Collections.Generic;
// using GraphProcessor;
// using UnityEngine;
// using UnityEngine.UIElements;
// using UnityEditor;
// using UnityEditor.UIElements;
//
// [NodeCustomEditor(typeof(OdinFeatureNode))]
// public class OdinFeatureNodeView : BaseNodeView
// {
//     public override void Enable()
//     {
//         var odinNode = nodeTarget as OdinFeatureNode;
//
//         // 设置节点视图的宽度
//         style.width = 280f;
//         
//         // 创建标题标签
//         var titleLabel = new Label("Odin特性演示");
//         titleLabel.style.fontSize = 14;
//         titleLabel.style.color = new Color(0.8f, 0.8f, 1f, 1f);
//         titleLabel.style.marginBottom = 10;
//         titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
//         
//         // 创建信息框
//         var infoBox = new Label("这是一个使用Odin Inspector特性的节点");
//         infoBox.style.backgroundColor = new Color(0.2f, 0.4f, 0.8f, 0.3f);
//         infoBox.style.borderTopWidth = 1;
//         infoBox.style.borderBottomWidth = 1;
//         infoBox.style.borderLeftWidth = 1;
//         infoBox.style.borderRightWidth = 1;
//         infoBox.style.borderTopColor = new Color(0.4f, 0.6f, 1f, 1f);
//         infoBox.style.borderBottomColor = new Color(0.4f, 0.6f, 1f, 1f);
//         infoBox.style.borderLeftColor = new Color(0.4f, 0.6f, 1f, 1f);
//         infoBox.style.borderRightColor = new Color(0.4f, 0.6f, 1f, 1f);
//         infoBox.style.paddingTop = 5;
//         infoBox.style.paddingBottom = 5;
//         infoBox.style.paddingLeft = 8;
//         infoBox.style.paddingRight = 8;
//         infoBox.style.marginBottom = 10;
//         infoBox.style.unityTextAlign = TextAnchor.MiddleCenter;
//         infoBox.style.fontSize = 11;
//         
//         // 创建基础设置折叠组
//         var basicFoldout = CreateFoldoutGroup("基础设置", true);
//         
//         // 创建数值范围滑动条
//         var rangeSlider = new Slider
//         {
//             value = odinNode.rangeValue,
//             lowValue = 0f,
//             highValue = 100f,
//             showInputField = true,
//             label = "数值范围"
//         };
//         rangeSlider.style.marginBottom = 10;
//         
//         // 创建启用功能开关
//         var toggleField = new Toggle
//         {
//             value = odinNode.isEnabled,
//             label = "启用功能"
//         };
//         toggleField.style.marginBottom = 10;
//         
//         // 创建枚举选择器
//         var enumField = new EnumField("测试枚举", odinNode.testEnum);
//         enumField.style.marginBottom = 10;
//         
//         // 将基础设置控件添加到折叠组
//         basicFoldout.Add(rangeSlider);
//         basicFoldout.Add(toggleField);
//         basicFoldout.Add(enumField);
//         
//         // 创建高级设置折叠组
//         var advancedFoldout = CreateFoldoutGroup("高级设置", false);
//         
//         // 创建高级数值滑动条
//         var advancedSlider = new Slider
//         {
//             value = odinNode.advancedValue,
//             lowValue = 0f,
//             highValue = 200f,
//             showInputField = true,
//             label = "高级数值"
//         };
//         advancedSlider.style.marginBottom = 10;
//         
//         // 创建高级功能开关
//         var advancedToggleField = new Toggle
//         {
//             value = odinNode.isAdvancedEnabled,
//             label = "高级功能"
//         };
//         advancedToggleField.style.marginBottom = 10;
//         
//         // 创建高级枚举选择器
//         var advancedEnumField = new EnumField("高级枚举", odinNode.advancedEnum);
//         advancedEnumField.style.marginBottom = 10;
//         
//         // 将高级设置控件添加到折叠组
//         advancedFoldout.Add(advancedSlider);
//         advancedFoldout.Add(advancedToggleField);
//         advancedFoldout.Add(advancedEnumField);
//         
//         // 创建按钮操作折叠组
//         var buttonFoldout = CreateFoldoutGroup("按钮操作", true);
//         
//         // 创建按钮容器
//         var buttonContainer = new VisualElement();
//         buttonContainer.style.flexDirection = FlexDirection.Column;
//         buttonContainer.style.alignItems = Align.Stretch;
//         buttonContainer.style.marginBottom = 5;
//         
//         // 创建执行操作按钮
//         var executeButton = new Button(() => {
//             odinNode.ExecuteAction();
//         })
//         {
//             text = "执行操作"
//         };
//         executeButton.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
//         executeButton.style.borderTopWidth = 1;
//         executeButton.style.borderBottomWidth = 1;
//         executeButton.style.borderLeftWidth = 1;
//         executeButton.style.borderRightWidth = 1;
//         executeButton.style.borderTopColor = new Color(0.4f, 1f, 0.4f, 1f);
//         executeButton.style.borderBottomColor = new Color(0.4f, 1f, 0.4f, 1f);
//         executeButton.style.borderLeftColor = new Color(0.4f, 1f, 0.4f, 1f);
//         executeButton.style.borderRightColor = new Color(0.4f, 1f, 0.4f, 1f);
//         executeButton.style.marginBottom = 5;
//         executeButton.style.height = 25;
//         
//         // 创建重置数值按钮
//         var resetButton = new Button(() => {
//             odinNode.ResetValues();
//             // 更新UI显示
//             rangeSlider.value = odinNode.rangeValue;
//             toggleField.value = odinNode.isEnabled;
//             enumField.value = odinNode.testEnum;
//             advancedSlider.value = odinNode.advancedValue;
//             advancedToggleField.value = odinNode.isAdvancedEnabled;
//             advancedEnumField.value = odinNode.advancedEnum;
//         })
//         {
//             text = "重置数值"
//         };
//         resetButton.style.backgroundColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
//         resetButton.style.borderTopWidth = 1;
//         resetButton.style.borderBottomWidth = 1;
//         resetButton.style.borderLeftWidth = 1;
//         resetButton.style.borderRightWidth = 1;
//         resetButton.style.borderTopColor = new Color(1f, 0.8f, 0.4f, 1f);
//         resetButton.style.borderBottomColor = new Color(1f, 0.8f, 0.4f, 1f);
//         resetButton.style.borderLeftColor = new Color(1f, 0.8f, 0.4f, 1f);
//         resetButton.style.borderRightColor = new Color(1f, 0.8f, 0.4f, 1f);
//         resetButton.style.marginBottom = 5;
//         resetButton.style.height = 25;
//         
//         // 创建随机数值按钮
//         var randomButton = new Button(() => {
//             odinNode.RandomizeValues();
//             // 更新UI显示
//             rangeSlider.value = odinNode.rangeValue;
//             enumField.value = odinNode.testEnum;
//             advancedSlider.value = odinNode.advancedValue;
//             advancedEnumField.value = odinNode.advancedEnum;
//         })
//         {
//             text = "随机数值"
//         };
//         randomButton.style.backgroundColor = new Color(0.6f, 0.2f, 0.8f, 0.8f);
//         randomButton.style.borderTopWidth = 1;
//         randomButton.style.borderBottomWidth = 1;
//         randomButton.style.borderLeftWidth = 1;
//         randomButton.style.borderRightWidth = 1;
//         randomButton.style.borderTopColor = new Color(0.8f, 0.4f, 1f, 1f);
//         randomButton.style.borderBottomColor = new Color(0.8f, 0.4f, 1f, 1f);
//         randomButton.style.borderLeftColor = new Color(0.8f, 0.4f, 1f, 1f);
//         randomButton.style.borderRightColor = new Color(0.8f, 0.4f, 1f, 1f);
//         randomButton.style.height = 25;
//         
//         // 将按钮添加到按钮容器
//         buttonContainer.Add(executeButton);
//         buttonContainer.Add(resetButton);
//         buttonContainer.Add(randomButton);
//         
//         // 将按钮容器添加到按钮折叠组
//         buttonFoldout.Add(buttonContainer);
//         
//         // 创建高级操作折叠组
//         var advancedActionFoldout = CreateFoldoutGroup("高级操作", false);
//         
//         // 创建高级操作按钮
//         var advancedActionButton = new Button(() => {
//             odinNode.AdvancedAction();
//         })
//         {
//             text = "高级操作"
//         };
//         advancedActionButton.style.backgroundColor = new Color(0.8f, 0.2f, 0.6f, 0.8f);
//         advancedActionButton.style.borderTopWidth = 1;
//         advancedActionButton.style.borderBottomWidth = 1;
//         advancedActionButton.style.borderLeftWidth = 1;
//         advancedActionButton.style.borderRightWidth = 1;
//         advancedActionButton.style.borderTopColor = new Color(1f, 0.4f, 0.8f, 1f);
//         advancedActionButton.style.borderBottomColor = new Color(1f, 0.4f, 0.8f, 1f);
//         advancedActionButton.style.borderLeftColor = new Color(1f, 0.4f, 0.8f, 1f);
//         advancedActionButton.style.borderRightColor = new Color(1f, 0.4f, 0.8f, 1f);
//         advancedActionButton.style.marginBottom = 5;
//         advancedActionButton.style.height = 25;
//         
//         // 创建切换高级状态按钮
//         var toggleAdvancedButton = new Button(() => {
//             odinNode.ToggleAdvanced();
//             // 更新UI显示
//             advancedToggleField.value = odinNode.isAdvancedEnabled;
//         })
//         {
//             text = "切换高级状态"
//         };
//         toggleAdvancedButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.8f, 0.8f);
//         toggleAdvancedButton.style.borderTopWidth = 1;
//         toggleAdvancedButton.style.borderBottomWidth = 1;
//         toggleAdvancedButton.style.borderLeftWidth = 1;
//         toggleAdvancedButton.style.borderRightWidth = 1;
//         toggleAdvancedButton.style.borderTopColor = new Color(0.4f, 0.8f, 1f, 1f);
//         toggleAdvancedButton.style.borderBottomColor = new Color(0.4f, 0.8f, 1f, 1f);
//         toggleAdvancedButton.style.borderLeftColor = new Color(0.4f, 0.8f, 1f, 1f);
//         toggleAdvancedButton.style.borderRightColor = new Color(0.4f, 0.8f, 1f, 1f);
//         toggleAdvancedButton.style.height = 25;
//         
//         // 将高级操作按钮添加到高级操作折叠组
//         advancedActionFoldout.Add(advancedActionButton);
//         advancedActionFoldout.Add(toggleAdvancedButton);
//         
//         // 注册值变化回调
//         rangeSlider.RegisterValueChangedCallback((v) => {
//             owner.RegisterCompleteObjectUndo("Updated rangeValue");
//             odinNode.rangeValue = v.newValue;
//             TriggerConnectedNodesProcess(odinNode);
//         });
//         
//         toggleField.RegisterValueChangedCallback((v) => {
//             owner.RegisterCompleteObjectUndo("Updated isEnabled");
//             odinNode.isEnabled = v.newValue;
//             TriggerConnectedNodesProcess(odinNode);
//         });
//         
//         enumField.RegisterValueChangedCallback((v) => {
//             owner.RegisterCompleteObjectUndo("Updated testEnum");
//             odinNode.testEnum = (OdinFeatureNode.TestEnum)v.newValue;
//             TriggerConnectedNodesProcess(odinNode);
//         });
//         
//         advancedSlider.RegisterValueChangedCallback((v) => {
//             owner.RegisterCompleteObjectUndo("Updated advancedValue");
//             odinNode.advancedValue = v.newValue;
//             TriggerConnectedNodesProcess(odinNode);
//         });
//         
//         advancedToggleField.RegisterValueChangedCallback((v) => {
//             owner.RegisterCompleteObjectUndo("Updated isAdvancedEnabled");
//             odinNode.isAdvancedEnabled = v.newValue;
//             TriggerConnectedNodesProcess(odinNode);
//         });
//         
//         advancedEnumField.RegisterValueChangedCallback((v) => {
//             owner.RegisterCompleteObjectUndo("Updated advancedEnum");
//             odinNode.advancedEnum = (OdinFeatureNode.AdvancedEnum)v.newValue;
//             TriggerConnectedNodesProcess(odinNode);
//         });
//         
//         // 注册处理完成回调
//         odinNode.onProcessed += () => {
//             rangeSlider.value = odinNode.rangeValue;
//             toggleField.value = odinNode.isEnabled;
//             enumField.value = odinNode.testEnum;
//             advancedSlider.value = odinNode.advancedValue;
//             advancedToggleField.value = odinNode.isAdvancedEnabled;
//             advancedEnumField.value = odinNode.advancedEnum;
//         };
//         
//         // 将所有控件添加到控制容器
//         controlsContainer.Add(titleLabel);
//         controlsContainer.Add(infoBox);
//         controlsContainer.Add(basicFoldout);
//         controlsContainer.Add(advancedFoldout);
//         controlsContainer.Add(buttonFoldout);
//         controlsContainer.Add(advancedActionFoldout);
//     }
//     
//     /// <summary>
//     /// 创建折叠组
//     /// </summary>
//     /// <param name="title">折叠组标题</param>
//     /// <param name="expanded">是否默认展开</param>
//     /// <returns>折叠组控件</returns>
//     private Foldout CreateFoldoutGroup(string title, bool expanded)
//     {
//         var foldout = new Foldout
//         {
//             text = title,
//             value = expanded
//         };
//         
//         // 设置折叠组样式
//         foldout.style.marginBottom = 10;
//         foldout.style.borderTopWidth = 1;
//         foldout.style.borderBottomWidth = 1;
//         foldout.style.borderLeftWidth = 1;
//         foldout.style.borderRightWidth = 1;
//         foldout.style.borderTopColor = new Color(0.4f, 0.4f, 0.4f, 1f);
//         foldout.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 1f);
//         foldout.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f, 1f);
//         foldout.style.borderRightColor = new Color(0.4f, 0.4f, 0.4f, 1f);
//         foldout.style.paddingTop = 5;
//         foldout.style.paddingBottom = 5;
//         foldout.style.paddingLeft = 8;
//         foldout.style.paddingRight = 8;
//         
//         return foldout;
//     }
//     
//     /// <summary>
//     /// 触发连接到指定节点输出端口的节点立即处理
//     /// </summary>
//     /// <param name="node">要触发连接的节点</param>
//     private void TriggerConnectedNodesProcess(BaseNode node)
//     {
//         // 触发自己
//         node.OnProcess();
//         // 获取节点的所有输出端口
//         foreach (var outputPort in node.outputPorts)
//         {
//             // 获取连接到每个输出端口的所有边
//             var edges = outputPort.GetEdges();
//             foreach (var edge in edges)
//             {
//                 // 获取输入节点（连接到输出端口的节点）
//                 var inputNode = edge.inputNode;
//                 if (inputNode != null)
//                 {
//                     // 立即触发输入节点的Process方法
//                     inputNode.OnProcess();
//                 }
//             }
//         }
//     }
// }
