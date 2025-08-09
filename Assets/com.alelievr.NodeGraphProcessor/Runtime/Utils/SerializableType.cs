using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	/// <summary>
	/// 可序列化类型类
	/// 提供Type对象的序列化和反序列化功能
	/// 使用缓存机制提高性能，避免重复的类型查找
	/// </summary>
	[Serializable]
	public class SerializableType : ISerializationCallbackReceiver
	{
		/// <summary>
		/// 类型名称到Type对象的缓存
		/// 用于快速查找已解析的类型
		/// </summary>
		static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
		
		/// <summary>
		/// Type对象到类型名称的缓存
		/// 用于快速获取类型的序列化名称
		/// </summary>
		static Dictionary<Type, string> typeNameCache = new Dictionary<Type, string>();

		/// <summary>
		/// 序列化的类型名称
		/// 在序列化时存储类型的完整名称
		/// </summary>
		[SerializeField]
		public string	serializedType;

		/// <summary>
		/// 实际的Type对象
		/// 在运行时使用的类型引用
		/// </summary>
		[NonSerialized]
		public Type		type;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="t">要序列化的类型</param>
		public SerializableType(Type t)
		{
			type = t;
		}

        /// <summary>
        /// 反序列化后的回调
        /// 从序列化的类型名称恢复Type对象
        /// </summary>
        public void OnAfterDeserialize()
        {
			if (!String.IsNullOrEmpty(serializedType))
			{
				// 首先尝试从缓存中获取类型
				if (!typeCache.TryGetValue(serializedType, out type))
				{
					// 如果缓存中没有，则通过反射获取类型并缓存
					type = Type.GetType(serializedType);
					typeCache[serializedType] = type;
				}
			}
        }

        /// <summary>
        /// 序列化前的回调
        /// 将Type对象转换为可序列化的类型名称
        /// </summary>
        public void OnBeforeSerialize()
        {
			if (type != null)
			{
				// 首先尝试从缓存中获取类型名称
				if (!typeNameCache.TryGetValue(type, out serializedType))
				{
					// 如果缓存中没有，则获取完整类型名称并缓存
					serializedType = type.AssemblyQualifiedName;
					typeNameCache[type] = serializedType;
				}
			}
        }
    }
}