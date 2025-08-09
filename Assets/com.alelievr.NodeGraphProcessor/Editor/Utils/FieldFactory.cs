using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using System.Reflection;
using System.Globalization;

namespace GraphProcessor
{
	/// <summary>
	/// 字段工厂类（已过时）
	/// 用于创建UI字段的工厂类，现在可以直接使用SerializedProperty替代
	/// 提供类型到UI字段的映射和创建功能
	/// </summary>
	[Obsolete("Field Factory is not necessary anymore. You can use a SerializedProperty directly instead.")]
	public static class FieldFactory
	{
		/// <summary>
		/// 字段绘制器映射
		/// 存储类型到对应UI字段类型的映射
		/// </summary>
		static readonly Dictionary< Type, Type >    fieldDrawers = new Dictionary< Type, Type >();

		/// <summary>
		/// 创建字段方法信息
		/// 通过反射获取CreateFieldSpecific方法的MethodInfo
		/// </summary>
		static readonly MethodInfo	        		createFieldMethod = typeof(FieldFactory).GetMethod("CreateFieldSpecific", BindingFlags.Static | BindingFlags.Public);

		/// <summary>
		/// 静态构造函数
		/// 初始化字段工厂，注册所有字段绘制器
		/// </summary>
		static FieldFactory()
		{
			// 扫描所有类型，查找带有FieldDrawerAttribute的类型
			foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
			{
				var drawerAttribute = type.GetCustomAttributes(typeof(FieldDrawerAttribute), false).FirstOrDefault() as FieldDrawerAttribute;

				if (drawerAttribute == null)
					continue ;

				AddDrawer(drawerAttribute.fieldType, type);
			}

			// 注册内置类型的字段绘制器
            AddDrawer(typeof(bool), typeof(Toggle));
            AddDrawer(typeof(int), typeof(IntegerField));
            AddDrawer(typeof(long), typeof(LongField));
            AddDrawer(typeof(float), typeof(FloatField));
			AddDrawer(typeof(double), typeof(DoubleField));
			AddDrawer(typeof(string), typeof(TextField));
			AddDrawer(typeof(Bounds), typeof(BoundsField));
			AddDrawer(typeof(Color), typeof(ColorField));
			AddDrawer(typeof(Vector2), typeof(Vector2Field));
			AddDrawer(typeof(Vector2Int), typeof(Vector2IntField));
			AddDrawer(typeof(Vector3), typeof(Vector3Field));
			AddDrawer(typeof(Vector3Int), typeof(Vector3IntField));
			AddDrawer(typeof(Vector4), typeof(Vector4Field));
			AddDrawer(typeof(AnimationCurve), typeof(CurveField));
			AddDrawer(typeof(Enum), typeof(EnumField));
			AddDrawer(typeof(Gradient), typeof(GradientField));
			AddDrawer(typeof(UnityEngine.Object), typeof(ObjectField));
			AddDrawer(typeof(Rect), typeof(RectField));
		}

		/// <summary>
		/// 添加绘制器
		/// 将字段类型和对应的UI字段类型添加到映射中
		/// </summary>
		/// <param name="fieldType">字段类型</param>
		/// <param name="drawerType">绘制器类型</param>
		static void AddDrawer(Type fieldType, Type drawerType)
		{
			var iNotifyType = typeof(INotifyValueChanged<>).MakeGenericType(fieldType);

			if (!iNotifyType.IsAssignableFrom(drawerType))
			{
				Debug.LogWarning("The custom field drawer " + drawerType + " does not implements INotifyValueChanged< " + fieldType + " >");
				return ;
			}

			fieldDrawers[fieldType] = drawerType;
		}

		/// <summary>
		/// 创建泛型字段
		/// 为指定类型创建UI字段
		/// </summary>
		/// <typeparam name="T">字段类型</typeparam>
		/// <param name="value">字段值</param>
		/// <param name="label">字段标签</param>
		/// <returns>创建的UI字段</returns>
		public static INotifyValueChanged< T > CreateField< T >(T value, string label = null)
		{
			return CreateField(value != null ? value.GetType() : typeof(T), label) as INotifyValueChanged< T >;
		}

		/// <summary>
		/// 创建字段
		/// 根据类型创建对应的UI字段
		/// </summary>
		/// <param name="t">字段类型</param>
		/// <param name="label">字段标签</param>
		/// <returns>创建的UI字段</returns>
		public static VisualElement CreateField(Type t, string label)
		{
			Type drawerType;

			// 尝试直接查找类型映射
			fieldDrawers.TryGetValue(t, out drawerType);

			// 如果没有直接匹配，尝试查找可分配的类型
			if (drawerType == null)
				drawerType = fieldDrawers.FirstOrDefault(kp => kp.Key.IsReallyAssignableFrom(t)).Value;

			if (drawerType == null)
			{
				Debug.LogWarning("Can't find field drawer for type: " + t);
				return null;
			}

			// 调用带标签的构造函数
			object field;
			
			if (drawerType == typeof(EnumField))
			{
				field = new EnumField(label, Activator.CreateInstance(t) as Enum);
			}
			else
			{
				try {
					field = Activator.CreateInstance(drawerType,
						BindingFlags.CreateInstance |
						BindingFlags.Public |
						BindingFlags.NonPublic |
						BindingFlags.Instance | 
						BindingFlags.OptionalParamBinding, null,
						new object[]{ label, Type.Missing }, CultureInfo.CurrentCulture);
				} catch {
					field = Activator.CreateInstance(drawerType,
						BindingFlags.CreateInstance |
						BindingFlags.Public |
						BindingFlags.NonPublic |
						BindingFlags.Instance | 
						BindingFlags.OptionalParamBinding, null,
						new object[]{ label }, CultureInfo.CurrentCulture);
				}
			}

			// For mutiline
			switch (field)
			{
				case TextField textField:
					textField.multiline = true;
					break;
				case ObjectField objField:
					objField.allowSceneObjects = true;
					objField.objectType = typeof(UnityEngine.Object);
					break;
			}
			
			return field as VisualElement;
		}

		public static INotifyValueChanged< T > CreateFieldSpecific< T >(T value, Action< object > onValueChanged, string label)
		{
			var fieldDrawer = CreateField< T >(value, label);

			if (fieldDrawer == null)
				return null;

			fieldDrawer.value = value;
			fieldDrawer.RegisterValueChangedCallback((e) => {
				onValueChanged(e.newValue);
			});

			return fieldDrawer as INotifyValueChanged< T >;
		}

		public static VisualElement CreateField(Type fieldType, object value, Action< object > onValueChanged, string label)
		{
			if (typeof(Enum).IsAssignableFrom(fieldType))
				fieldType = typeof(Enum);

			VisualElement field = null;

			// Handle special cases here
			if (fieldType == typeof(LayerMask))
			{
				// LayerMasks inherit from INotifyValueChanged<int> instead of INotifyValueChanged<LayerMask>
				// so we can't register it inside our factory system :(
				var layerField = new LayerMaskField(label, ((LayerMask)value).value);
				layerField.RegisterValueChangedCallback(e => {
					onValueChanged(new LayerMask{ value = e.newValue});
				});

				field = layerField;
			}
			else
			{
				try
				{
					var createFieldSpecificMethod = createFieldMethod.MakeGenericMethod(fieldType);
					try
					{
						field = createFieldSpecificMethod.Invoke(null, new object[]{value, onValueChanged, label}) as VisualElement;
					} catch {}

					// handle the Object field case
					if (field == null && (value == null || value is UnityEngine.Object))
					{
						createFieldSpecificMethod = createFieldMethod.MakeGenericMethod(typeof(UnityEngine.Object));
						field = createFieldSpecificMethod.Invoke(null, new object[]{value, onValueChanged, label}) as VisualElement;
						if (field is ObjectField objField)
						{
							objField.objectType = fieldType;
							objField.value = value as UnityEngine.Object;
						}
					}
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}

			return field;
		}
	}
}