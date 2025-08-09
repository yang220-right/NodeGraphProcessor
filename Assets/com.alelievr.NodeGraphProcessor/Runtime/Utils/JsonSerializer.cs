using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

// 警告，当前的序列化代码不处理编辑器外播放模式中的unity对象（因为JsonUtility）

namespace GraphProcessor
{
	/// <summary>
	/// JSON元素结构体
	/// 用于存储序列化对象的基本信息，包括类型和JSON数据
	/// </summary>
	[Serializable]
	public struct JsonElement
	{
		/// <summary>
		/// 对象的类型名称
		/// 使用AssemblyQualifiedName确保类型的完整标识
		/// </summary>
		public string		type;
		
		/// <summary>
		/// 对象的JSON序列化数据
		/// </summary>
		public string		jsonDatas;

		/// <summary>
		/// 转换为字符串表示
		/// 用于调试和日志输出
		/// </summary>
		/// <returns>JSON元素的字符串表示</returns>
		public override string ToString()
		{
			return "type: " + type + " | JSON: " + jsonDatas;
		}
	}

	/// <summary>
	/// JSON序列化工具类
	/// 提供对象序列化和反序列化的功能
	/// 支持在编辑器和运行时使用不同的JSON序列化器
	/// </summary>
	public static class JsonSerializer
	{
		/// <summary>
		/// 序列化对象为JSON元素
		/// </summary>
		/// <param name="obj">要序列化的对象</param>
		/// <returns>包含类型和JSON数据的元素</returns>
		public static JsonElement	Serialize(object obj)
		{
			JsonElement	elem = new JsonElement();

			// 存储对象的完整类型名称
			elem.type = obj.GetType().AssemblyQualifiedName;
			
#if UNITY_EDITOR
			// 在编辑器中使用EditorJsonUtility
			elem.jsonDatas = EditorJsonUtility.ToJson(obj);
#else
			// 在运行时使用JsonUtility
			elem.jsonDatas = JsonUtility.ToJson(obj);
#endif

			return elem;
		}

		/// <summary>
		/// 从JSON元素反序列化对象
		/// </summary>
		/// <typeparam name="T">目标类型</typeparam>
		/// <param name="e">JSON元素</param>
		/// <returns>反序列化的对象</returns>
		/// <exception cref="ArgumentException">当反序列化类型与JSON元素类型不匹配时抛出</exception>
		public static T	Deserialize< T >(JsonElement e)
		{
			// 验证类型匹配
			if (typeof(T) != Type.GetType(e.type))
				throw new ArgumentException("Deserializing type is not the same than Json element type");

			// 创建目标类型的实例
			var obj = Activator.CreateInstance< T >();
			
#if UNITY_EDITOR
			// 在编辑器中使用EditorJsonUtility
			EditorJsonUtility.FromJsonOverwrite(e.jsonDatas, obj);
#else
			// 在运行时使用JsonUtility
			JsonUtility.FromJsonOverwrite(e.jsonDatas, obj);
#endif

			return obj;
		}

		/// <summary>
		/// 序列化节点为JSON元素
		/// 专门用于序列化BaseNode对象
		/// </summary>
		/// <param name="node">要序列化的节点</param>
		/// <returns>包含节点类型和JSON数据的元素</returns>
		public static JsonElement	SerializeNode(BaseNode node)
		{
			return Serialize(node);
		}

		/// <summary>
		/// 从JSON元素反序列化节点
		/// 专门用于反序列化BaseNode对象，包含错误处理
		/// </summary>
		/// <param name="e">JSON元素</param>
		/// <returns>反序列化的节点，如果失败则返回null</returns>
		public static BaseNode	DeserializeNode(JsonElement e)
		{
			try {
				// 获取节点类型
				var baseNodeType = Type.GetType(e.type);

				// 检查JSON数据是否为空
				if (e.jsonDatas == null)
					return null;

				// 创建节点实例
				var node = Activator.CreateInstance(baseNodeType) as BaseNode;
				
#if UNITY_EDITOR
				// 在编辑器中使用EditorJsonUtility
				EditorJsonUtility.FromJsonOverwrite(e.jsonDatas, node);
#else
				// 在运行时使用JsonUtility
				JsonUtility.FromJsonOverwrite(e.jsonDatas, node);
#endif
				return node;
			} catch {
				// 如果反序列化失败，返回null
				return null;
			}
		}
	}
}