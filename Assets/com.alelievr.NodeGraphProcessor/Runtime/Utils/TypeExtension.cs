using UnityEngine;
using System;
using System.Linq.Expressions;

namespace GraphProcessor
{
	/// <summary>
	/// 类型扩展工具类
	/// 提供类型兼容性检查的扩展方法
	/// 支持检查类型间的隐式转换和继承关系
	/// </summary>
	public static class TypeExtension
	{
		/// <summary>
		/// 检查类型是否真的可以相互赋值
		/// 不仅检查继承关系，还检查是否存在隐式转换
		/// </summary>
		/// <param name="type">目标类型</param>
		/// <param name="otherType">源类型</param>
		/// <returns>如果可以赋值则返回true，否则返回false</returns>
		public static bool IsReallyAssignableFrom(this Type type, Type otherType)
		{
			// 首先检查标准的继承关系
			if (type.IsAssignableFrom(otherType))
				return true;
			if (otherType.IsAssignableFrom(type))
				return true;

			// 使用表达式树检查是否存在隐式转换
			try
			{
				var v = Expression.Variable(otherType);
				var expr = Expression.Convert(v, type);
				// 检查转换方法是否存在且不是隐式转换操作符
				return expr.Method != null && expr.Method.Name != "op_Implicit";
			}
			catch (InvalidOperationException)
			{
				// 如果转换失败，返回false
				return false;
			}
		}

	}
}