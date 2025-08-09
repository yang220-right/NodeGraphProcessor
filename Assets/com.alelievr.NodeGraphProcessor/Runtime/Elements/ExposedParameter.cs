using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	/// <summary>
	/// 暴露参数类
	/// 用于在图形中暴露可配置的参数，支持序列化和反序列化
	/// 提供各种数据类型的参数支持，如浮点数、向量、颜色等
	/// </summary>
	[Serializable]
	public class ExposedParameter : ISerializationCallbackReceiver
	{
        /// <summary>
        /// 参数设置类
        /// 存储参数的显示和行为设置
        /// </summary>
        [Serializable]
        public class Settings
        {
            /// <summary>
            /// 参数是否隐藏
            /// 控制参数在UI中是否可见
            /// </summary>
            public bool isHidden = false;
            
            /// <summary>
            /// 参数是否展开
            /// 控制参数在UI中的展开状态
            /// </summary>
            public bool expanded = false;

            /// <summary>
            /// 参数的唯一标识符
            /// 用于跟踪和识别参数
            /// </summary>
            [SerializeField]
            internal string guid = null;

            /// <summary>
            /// 比较两个Settings对象是否相等
            /// </summary>
            /// <param name="obj">要比较的对象</param>
            /// <returns>如果相等则返回true</returns>
            public override bool Equals(object obj)
            {
                if (obj is Settings s && s != null)
                    return Equals(s);
                else
                    return false;
            }

            /// <summary>
            /// 比较两个Settings对象是否相等
            /// </summary>
            /// <param name="param">要比较的Settings</param>
            /// <returns>如果相等则返回true</returns>
            public virtual bool Equals(Settings param)
                => isHidden == param.isHidden && expanded == param.expanded;

            /// <summary>
            /// 获取哈希码
            /// </summary>
            /// <returns>哈希码</returns>
            public override int GetHashCode() => base.GetHashCode();
        }

		/// <summary>
		/// 参数的唯一标识符
		/// 用于跟踪参数的唯一ID
		/// </summary>
		public string				guid;
		
		/// <summary>
		/// 参数名称
		/// 在UI中显示的参数名称
		/// </summary>
		public string				name;
		
		/// <summary>
		/// 参数类型（已过时）
		/// 使用GetValueType()替代
		/// </summary>
		[Obsolete("Use GetValueType()")]
		public string				type;
		
		/// <summary>
		/// 序列化值（已过时）
		/// 使用value替代
		/// </summary>
		[Obsolete("Use value instead")]
		public SerializableObject	serializedValue;
		
		/// <summary>
		/// 是否为输入参数
		/// true表示输入参数，false表示输出参数
		/// </summary>
		public bool					input = true;
        
        /// <summary>
        /// 参数设置
        /// 包含参数的显示和行为配置
        /// </summary>
        [SerializeReference]
		public Settings             settings;
		
		/// <summary>
		/// 参数类型的简短名称
		/// </summary>
		public string shortType => GetValueType()?.Name;

        /// <summary>
        /// 初始化参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        public void Initialize(string name, object value)
        {
			guid = Guid.NewGuid().ToString(); // 每个参数生成一次且唯一
            settings = CreateSettings();
            settings.guid = guid;
			this.name = name;
			this.value = value;
        }

		/// <summary>
		/// 反序列化后的回调
		/// 处理从旧版本迁移数据
		/// </summary>
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			// SerializeReference迁移步骤：
#pragma warning disable CS0618
			if (serializedValue?.value != null) // 旧序列化系统无法序列化null值
			{
				value = serializedValue.value;
				Debug.Log("Migrated: " + serializedValue.value + " | " + serializedValue.serializedName);
				serializedValue.value = null;
			}
#pragma warning restore CS0618
		}

		/// <summary>
		/// 序列化前的回调
		/// </summary>
		void ISerializationCallbackReceiver.OnBeforeSerialize() {}

        /// <summary>
        /// 创建设置对象
        /// 子类可以重写此方法以提供自定义设置
        /// </summary>
        /// <returns>设置对象</returns>
        protected virtual Settings CreateSettings() => new Settings();

        /// <summary>
        /// 参数值
        /// 存储参数的实际数据
        /// </summary>
        public virtual object value { get; set; }
        
        /// <summary>
        /// 获取参数值的类型
        /// </summary>
        /// <returns>参数值的类型</returns>
        public virtual Type GetValueType() => value == null ? typeof(object) : value.GetType();

        /// <summary>
        /// 暴露参数类型缓存
        /// 用于快速查找参数类型
        /// </summary>
        static Dictionary<Type, Type> exposedParameterTypeCache = new Dictionary<Type, Type>();
        
        /// <summary>
        /// 迁移参数到新版本
        /// 用于处理版本升级时的数据迁移
        /// </summary>
        /// <returns>迁移后的参数对象</returns>
        internal ExposedParameter Migrate()
        {
            // 初始化类型缓存
            if (exposedParameterTypeCache.Count == 0)
            {
                foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
                {
                    if (type.IsSubclassOf(typeof(ExposedParameter)) && !type.IsAbstract)
                    {
                        var paramType = Activator.CreateInstance(type) as ExposedParameter;
                        exposedParameterTypeCache[paramType.GetValueType()] = type;
                    }
                }
            }
#pragma warning disable CS0618 // Use of obsolete fields
            var oldType = Type.GetType(type);
#pragma warning restore CS0618
            if (oldType == null || !exposedParameterTypeCache.TryGetValue(oldType, out var newParamType))
                return null;
            
            var newParam = Activator.CreateInstance(newParamType) as ExposedParameter;

            newParam.guid = guid;
            newParam.name = name;
            newParam.input = input;
            newParam.settings = newParam.CreateSettings();
            newParam.settings.guid = guid;

            return newParam;
     
        }

        public static bool operator ==(ExposedParameter param1, ExposedParameter param2)
        {
            if (ReferenceEquals(param1, null) && ReferenceEquals(param2, null))
                return true;
            if (ReferenceEquals(param1, param2))
                return true;
            if (ReferenceEquals(param1, null))
                return false;
            if (ReferenceEquals(param2, null))
                return false;

            return param1.Equals(param2);
        }

        public static bool operator !=(ExposedParameter param1, ExposedParameter param2) => !(param1 == param2);

        public bool Equals(ExposedParameter parameter) => guid == parameter.guid;

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            else
                return Equals((ExposedParameter)obj);
        }

        public override int GetHashCode() => guid.GetHashCode();

        public ExposedParameter Clone()
        {
            var clonedParam = Activator.CreateInstance(GetType()) as ExposedParameter;

            clonedParam.guid = guid;
            clonedParam.name = name;
            clonedParam.input = input;
            clonedParam.settings = settings;
            clonedParam.value = value;

            return clonedParam;
        }
	}

    // 由于[SerializeReference]的多态约束，我们需要为图形中可用的每种参数类型显式创建一个类
    // （即模板化不起作用）
    [System.Serializable]
    public class ColorParameter : ExposedParameter
    {
        public enum ColorMode
        {
            Default,
            HDR
        }

        [Serializable]
        public class ColorSettings : Settings
        {
            public ColorMode mode;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((ColorSettings)param).mode;
        }

        [SerializeField] Color val;

        public override object value { get => val; set => val = (Color)value; }
        protected override Settings CreateSettings() => new ColorSettings();
    }

    [System.Serializable]
    public class FloatParameter : ExposedParameter
    {
        public enum FloatMode
        {
            Default,
            Slider,
        }

        [Serializable]
        public class FloatSettings : Settings
        {
            public FloatMode mode;
            public float min = 0;
            public float max = 1;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((FloatSettings)param).mode && min == ((FloatSettings)param).min && max == ((FloatSettings)param).max;
        }

        [SerializeField] float val;

        public override object value { get => val; set => val = (float)value; }
        protected override Settings CreateSettings() => new FloatSettings();
    }

    [System.Serializable]
    public class Vector2Parameter : ExposedParameter
    {
        public enum Vector2Mode
        {
            Default,
            MinMaxSlider,
        }

        [Serializable]
        public class Vector2Settings : Settings
        {
            public Vector2Mode mode;
            public float min = 0;
            public float max = 1;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((Vector2Settings)param).mode && min == ((Vector2Settings)param).min && max == ((Vector2Settings)param).max;
        }

        [SerializeField] Vector2 val;

        public override object value { get => val; set => val = (Vector2)value; }
        protected override Settings CreateSettings() => new Vector2Settings();
    }

    [System.Serializable]
    public class Vector3Parameter : ExposedParameter
    {
        [SerializeField] Vector3 val;

        public override object value { get => val; set => val = (Vector3)value; }
    }

    [System.Serializable]
    public class Vector4Parameter : ExposedParameter
    {
        [SerializeField] Vector4 val;

        public override object value { get => val; set => val = (Vector4)value; }
    }

    [System.Serializable]
    public class IntParameter : ExposedParameter
    {
        public enum IntMode
        {
            Default,
            Slider,
        }

        [Serializable]
        public class IntSettings : Settings
        {
            public IntMode mode;
            public int min = 0;
            public int max = 10;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((IntSettings)param).mode && min == ((IntSettings)param).min && max == ((IntSettings)param).max;
        }

        [SerializeField] int val;

        public override object value { get => val; set => val = (int)value; }
        protected override Settings CreateSettings() => new IntSettings();
    }

    [System.Serializable]
    public class Vector2IntParameter : ExposedParameter
    {
        [SerializeField] Vector2Int val;

        public override object value { get => val; set => val = (Vector2Int)value; }
    }

    [System.Serializable]
    public class Vector3IntParameter : ExposedParameter
    {
        [SerializeField] Vector3Int val;

        public override object value { get => val; set => val = (Vector3Int)value; }
    }

    [System.Serializable]
    public class DoubleParameter : ExposedParameter
    {
        [SerializeField] Double val;

        public override object value { get => val; set => val = (Double)value; }
    }

    [System.Serializable]
    public class LongParameter : ExposedParameter
    {
        [SerializeField] long val;

        public override object value { get => val; set => val = (long)value; }
    }

    [System.Serializable]
    public class StringParameter : ExposedParameter
    {
        [SerializeField] string val;

        public override object value { get => val; set => val = (string)value; }
        public override Type GetValueType() => typeof(String);
    }

    [System.Serializable]
    public class RectParameter : ExposedParameter
    {
        [SerializeField] Rect val;

        public override object value { get => val; set => val = (Rect)value; }
    }

    [System.Serializable]
    public class RectIntParameter : ExposedParameter
    {
        [SerializeField] RectInt val;

        public override object value { get => val; set => val = (RectInt)value; }
    }

    [System.Serializable]
    public class BoundsParameter : ExposedParameter
    {
        [SerializeField] Bounds val;

        public override object value { get => val; set => val = (Bounds)value; }
    }

    [System.Serializable]
    public class BoundsIntParameter : ExposedParameter
    {
        [SerializeField] BoundsInt val;

        public override object value { get => val; set => val = (BoundsInt)value; }
    }

    [System.Serializable]
    public class AnimationCurveParameter : ExposedParameter
    {
        [SerializeField] AnimationCurve val;

        public override object value { get => val; set => val = (AnimationCurve)value; }
        public override Type GetValueType() => typeof(AnimationCurve);
    }

    [System.Serializable]
    public class GradientParameter : ExposedParameter
    {
        public enum GradientColorMode
        {
            Default,
            HDR,
        }

        [Serializable]
        public class GradientSettings : Settings
        {
            public GradientColorMode mode;

            public override bool Equals(Settings param)
                => base.Equals(param) && mode == ((GradientSettings)param).mode;
        }

        [SerializeField] Gradient val;
        [SerializeField, GradientUsage(true)] Gradient hdrVal;

        public override object value { get => val; set => val = (Gradient)value; }
        public override Type GetValueType() => typeof(Gradient);
        protected override Settings CreateSettings() => new GradientSettings();
    }

    [System.Serializable]
    public class GameObjectParameter : ExposedParameter
    {
        [SerializeField] GameObject val;

        public override object value { get => val; set => val = (GameObject)value; }
        public override Type GetValueType() => typeof(GameObject);
    }

    [System.Serializable]
    public class BoolParameter : ExposedParameter
    {
        [SerializeField] bool val;

        public override object value { get => val; set => val = (bool)value; }
    }

    [System.Serializable]
    public class Texture2DParameter : ExposedParameter
    {
        [SerializeField] Texture2D val;

        public override object value { get => val; set => val = (Texture2D)value; }
        public override Type GetValueType() => typeof(Texture2D);
    }

    [System.Serializable]
    public class RenderTextureParameter : ExposedParameter
    {
        [SerializeField] RenderTexture val;

        public override object value { get => val; set => val = (RenderTexture)value; }
        public override Type GetValueType() => typeof(RenderTexture);
    }

    [System.Serializable]
    public class MeshParameter : ExposedParameter
    {
        [SerializeField] Mesh val;

        public override object value { get => val; set => val = (Mesh)value; }
        public override Type GetValueType() => typeof(Mesh);
    }

    [System.Serializable]
    public class MaterialParameter : ExposedParameter
    {
        [SerializeField] Material val;

        public override object value { get => val; set => val = (Material)value; }
        public override Type GetValueType() => typeof(Material);
    }
}