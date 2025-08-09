using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace GraphProcessor
{
	/// <summary>
	/// 自定义端口IO委托，用于处理节点的自定义输入输出逻辑
	/// </summary>
	/// <param name="node">要处理的节点</param>
	/// <param name="edges">与该端口相连的边列表</param>
	/// <param name="outputPort">输出端口（可选参数）</param>
	public delegate void CustomPortIODelegate(BaseNode node, List< SerializableEdge > edges, NodePort outputPort = null);

	/// <summary>
	/// 自定义端口IO处理类，负责管理节点的自定义输入输出方法
	/// 通过反射自动发现和注册带有CustomPortInputAttribute或CustomPortOutputAttribute的方法
	/// </summary>
	public static class CustomPortIO
	{
		/// <summary>
		/// 按字段名存储自定义IO委托的字典
		/// </summary>
		class PortIOPerField : Dictionary< string, CustomPortIODelegate > {}
		
		/// <summary>
		/// 按节点类型存储字段IO委托的字典
		/// </summary>
		class PortIOPerNode : Dictionary< Type, PortIOPerField > {}

		/// <summary>
		/// 存储可分配类型关系的字典，用于类型兼容性检查
		/// </summary>
		static Dictionary< Type, List< Type > >	assignableTypes = new Dictionary< Type, List< Type > >();
		
		/// <summary>
		/// 存储所有自定义IO端口方法的字典
		/// </summary>
		static PortIOPerNode					customIOPortMethods = new PortIOPerNode();

		/// <summary>
		/// 静态构造函数，在类首次使用时自动调用
		/// </summary>
		static CustomPortIO()
		{
			LoadCustomPortMethods();
		}

		/// <summary>
		/// 加载所有自定义端口方法
		/// 通过反射扫描所有继承自BaseNode的类型，查找带有自定义端口属性的方法
		/// </summary>
		static void LoadCustomPortMethods()
		{
			// 设置反射绑定标志，包括公共、私有和实例成员
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			// 遍历当前应用程序域中的所有类型
			foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
			{
				// 跳过抽象类和包含泛型参数的类型
				if (type.IsAbstract || type.ContainsGenericParameters)
					continue ;
				// 只处理继承自BaseNode的类型
				if (!(type.IsSubclassOf(typeof(BaseNode))))
					continue ;

				// 获取该类型的所有方法
				var methods = type.GetMethods(bindingFlags);

				// 遍历每个方法，查找自定义端口属性
				foreach (var method in methods)
				{
					// 检查方法是否具有自定义端口输入或输出属性
					var portInputAttr = method.GetCustomAttribute< CustomPortInputAttribute >();
					var portOutputAttr = method.GetCustomAttribute< CustomPortOutputAttribute >();

					// 如果方法没有自定义端口属性，跳过
					if (portInputAttr == null && portOutputAttr == null)
						continue ;
					
					// 获取方法的参数信息
					var p = method.GetParameters();
					bool nodePortSignature = false;

					// 检查函数是否可以在可选参数中接受NodePort
					// 如果方法有两个参数且第二个参数是NodePort类型，则支持NodePort签名
					if (p.Length == 2 && p[1].ParameterType == typeof(NodePort))
						nodePortSignature = true;

					CustomPortIODelegate deleg;
#if ENABLE_IL2CPP
					// IL2CPP不支持表达式构建器，使用反射调用
					if (nodePortSignature)
					{
						// 创建支持NodePort参数的委托
						deleg = new CustomPortIODelegate((node, edges, port) => {
							Debug.Log(port);
							method.Invoke(node, new object[]{ edges, port});
						});
					}
					else
					{
						// 创建不支持NodePort参数的委托
						deleg = new CustomPortIODelegate((node, edges, port) => {
							method.Invoke(node, new object[]{ edges });
						});
					}
#else
					// 使用表达式树创建委托（性能更好）
					var p1 = Expression.Parameter(typeof(BaseNode), "node");
					var p2 = Expression.Parameter(typeof(List< SerializableEdge >), "edges");
					var p3 = Expression.Parameter(typeof(NodePort), "port");

					MethodCallExpression ex;
					if (nodePortSignature)
						// 创建包含NodePort参数的方法调用表达式
						ex = Expression.Call(Expression.Convert(p1, type), method, p2, p3);
					else
						// 创建不包含NodePort参数的方法调用表达式
						ex = Expression.Call(Expression.Convert(p1, type), method, p2);

					// 编译表达式树为委托
					deleg = Expression.Lambda< CustomPortIODelegate >(ex, p1, p2, p3).Compile();
#endif

					// 如果委托创建失败，记录警告并跳过
					if (deleg == null)
					{
						Debug.LogWarning("无法使用自定义IO端口函数 " + method + ": 该方法必须遵循此格式: " + typeof(CustomPortIODelegate));
						continue ;
					}

					// 获取字段名和自定义类型信息
					string fieldName = (portInputAttr == null) ? portOutputAttr.fieldName : portInputAttr.fieldName;
					Type customType = (portInputAttr == null) ? portOutputAttr.outputType : portInputAttr.inputType;
					Type fieldType = type.GetField(fieldName, bindingFlags).FieldType;

					// 添加自定义IO方法到字典中
					AddCustomIOMethod(type, fieldName, deleg);

					// 添加类型兼容性关系
					AddAssignableTypes(customType, fieldType);
					AddAssignableTypes(fieldType, customType);
				}
			}
		}

		/// <summary>
		/// 获取指定节点类型和字段名的自定义端口方法
		/// </summary>
		/// <param name="nodeType">节点类型</param>
		/// <param name="fieldName">字段名</param>
		/// <returns>自定义端口IO委托，如果不存在则返回null</returns>
		public static CustomPortIODelegate GetCustomPortMethod(Type nodeType, string fieldName)
		{
			PortIOPerField			portIOPerField;
			CustomPortIODelegate	deleg;

			// 尝试获取节点类型的字段IO字典
			customIOPortMethods.TryGetValue(nodeType, out portIOPerField);

			// 如果节点类型不存在，返回null
			if (portIOPerField == null)
				return null;

			// 尝试获取指定字段的委托
			portIOPerField.TryGetValue(fieldName, out deleg);

			return deleg;
		}

		/// <summary>
		/// 添加自定义IO方法到内部字典中
		/// </summary>
		/// <param name="nodeType">节点类型</param>
		/// <param name="fieldName">字段名</param>
		/// <param name="deleg">自定义IO委托</param>
		static void AddCustomIOMethod(Type nodeType, string fieldName, CustomPortIODelegate deleg)
		{
			// 如果节点类型不存在，创建新的字段IO字典
			if (!customIOPortMethods.ContainsKey(nodeType))
				customIOPortMethods[nodeType] = new PortIOPerField();

			// 添加或更新字段的IO委托
			customIOPortMethods[nodeType][fieldName] = deleg;
		}

		/// <summary>
		/// 添加类型兼容性关系
		/// 记录fromType可以分配给toType的关系
		/// </summary>
		/// <param name="fromType">源类型</param>
		/// <param name="toType">目标类型</param>
		static void AddAssignableTypes(Type fromType, Type toType)
		{
			// 如果源类型不存在，创建新的类型列表
			if (!assignableTypes.ContainsKey(fromType))
				assignableTypes[fromType] = new List< Type >();

			// 添加目标类型到源类型的可分配类型列表中
			assignableTypes[fromType].Add(toType);
		}

		/// <summary>
		/// 检查输入类型是否可以分配给输出类型
		/// 用于验证端口连接的类型兼容性
		/// </summary>
		/// <param name="input">输入类型</param>
		/// <param name="output">输出类型</param>
		/// <returns>如果可以分配则返回true，否则返回false</returns>
		public static bool IsAssignable(Type input, Type output)
		{
			// 检查输入类型是否存在可分配类型列表
			if (assignableTypes.ContainsKey(input))
				// 检查输出类型是否在可分配类型列表中
				return assignableTypes[input].Contains(output);
			return false;
		}
	}
}