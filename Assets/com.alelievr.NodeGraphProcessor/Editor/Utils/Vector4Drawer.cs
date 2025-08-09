using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    /// <summary>
    /// Vector4绘制器类
    /// 自定义Vector4属性的绘制器，用于在单行显示Vector4值
    /// 因为默认情况下Vector4显示为切换开关，所以需要自定义绘制器
    /// </summary>
    [CustomPropertyDrawer(typeof(Vector4))]
    public class IngredientDrawerUIE : PropertyDrawer
    {
        /// <summary>
        /// 创建属性GUI
        /// 创建Vector4Field来显示和编辑Vector4值
        /// </summary>
        /// <param name="property">要绘制的序列化属性</param>
        /// <returns>创建的UI元素</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // 创建Vector4Field并设置当前值
            var vectorField = new Vector4Field() { value = property.vector4Value };
            
            // 注册值变化回调，当值改变时更新序列化属性
            vectorField.RegisterValueChangedCallback(e => {
                property.vector4Value = e.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            return vectorField;
        }
    }
}