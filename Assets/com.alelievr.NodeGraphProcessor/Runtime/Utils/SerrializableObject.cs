using System;
using UnityEngine;
using System.Globalization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GraphProcessor
{
    /// <summary>
    /// 可序列化对象类
    /// 警告：此类仅支持UnityObject和基本类型的序列化
    /// 提供通用对象的序列化和反序列化功能
    /// </summary>
    [System.Serializable]
    public class SerializableObject
    {
        /// <summary>
        /// 对象包装器类
        /// 用于包装UnityEngine.Object对象以便JSON序列化
        /// </summary>
        [System.Serializable]
        class ObjectWrapper
        {
            /// <summary>
            /// 包装的Unity对象
            /// </summary>
            public UnityEngine.Object value;
        }

        /// <summary>
        /// 序列化的类型名称
        /// 存储对象的完整类型名称
        /// </summary>
        public string serializedType;
        
        /// <summary>
        /// 序列化的对象名称
        /// 用于标识对象的名称
        /// </summary>
        public string serializedName;
        
        /// <summary>
        /// 序列化的值
        /// 存储对象的JSON序列化数据
        /// </summary>
        public string serializedValue;

        /// <summary>
        /// 实际的对象值
        /// 在运行时使用的对象引用
        /// </summary>
        public object value;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="value">要序列化的对象</param>
        /// <param name="type">对象的类型</param>
        /// <param name="name">对象的名称（可选）</param>
        public SerializableObject(object value, Type type, string name = null)
        {
            this.value = value;
            this.serializedName = name;
            this.serializedType = type.AssemblyQualifiedName;
        }

        /// <summary>
        /// 反序列化对象
        /// 从序列化的数据恢复对象值
        /// </summary>
        public void Deserialize()
        {
            // 检查类型名称是否为空
            if (String.IsNullOrEmpty(serializedType))
            {
                Debug.LogError("Can't deserialize the object from null type");
                return;
            }

            // 获取类型
            Type type = Type.GetType(serializedType);

            // 处理基本类型
            if (type.IsPrimitive)
            {
                if (string.IsNullOrEmpty(serializedValue))
                    value = Activator.CreateInstance(type);
                else
                    value = Convert.ChangeType(serializedValue, type, CultureInfo.InvariantCulture);
            }
            // 处理Unity对象
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                ObjectWrapper obj = new ObjectWrapper();
                JsonUtility.FromJsonOverwrite(serializedValue, obj);
                value = obj.value;
            }
            // 处理字符串类型
            else if (type == typeof(string))
                value = serializedValue.Length > 1 ? serializedValue.Substring(1, serializedValue.Length - 2).Replace("\\\"", "\"") : "";
            // 处理其他复杂类型
            else
            {
                try {
                    value = Activator.CreateInstance(type);
                    JsonUtility.FromJsonOverwrite(serializedValue, value);
                } catch (Exception e){
                    Debug.LogError(e);
                    Debug.LogError("Can't serialize type " + serializedType);
                }
            }
        }

        /// <summary>
        /// 序列化对象
        /// 将对象值转换为序列化数据
        /// </summary>
        public void Serialize()
        {
            // 检查对象是否为空
            if (value == null)
                return ;

            // 存储类型名称
            serializedType = value.GetType().AssemblyQualifiedName;

            // 处理基本类型
            if (value.GetType().IsPrimitive)
                serializedValue = Convert.ToString(value, CultureInfo.InvariantCulture);
            // 处理Unity对象
            else if (value is UnityEngine.Object)
            {
                if ((value as UnityEngine.Object) == null)
                    return ;

                ObjectWrapper wrapper = new ObjectWrapper { value = value as UnityEngine.Object };
                serializedValue = JsonUtility.ToJson(wrapper);
            }
            // 处理字符串类型
            else if (value is string)
                serializedValue = "\"" + ((string)value).Replace("\"", "\\\"") + "\"";
            // 处理其他复杂类型
            else
            {
                try {
                    serializedValue = JsonUtility.ToJson(value);
                    if (String.IsNullOrEmpty(serializedValue))
                        throw new Exception();
                } catch {
                    Debug.LogError("Can't serialize type " + serializedType);
                }
            }
        }
    }
}