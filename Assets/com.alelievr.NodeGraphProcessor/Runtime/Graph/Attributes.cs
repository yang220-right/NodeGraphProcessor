using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GraphProcessor
{
	/// <summary>
	/// 输入属性
	/// 告诉这个字段将生成一个输入端口
	/// 用于标记节点中需要接收数据的字段
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class InputAttribute : Attribute
	{
		/// <summary>
		/// 端口显示名称
		/// </summary>
		public string		name;
		
		/// <summary>
		/// 是否允许连接多条边
		/// </summary>
		public bool			allowMultiple = false;

		/// <summary>
		/// 将字段标记为输入端口
		/// </summary>
		/// <param name="name">显示名称</param>
		/// <param name="allowMultiple">是否允许连接多条边</param>
		public InputAttribute(string name = null, bool allowMultiple = false)
		{
			this.name = name;
			this.allowMultiple = allowMultiple;
		}
	}

	/// <summary>
	/// 输出属性
	/// 告诉这个字段将生成一个输出端口
	/// 用于标记节点中需要输出数据的字段
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class OutputAttribute : Attribute
	{
		/// <summary>
		/// 端口显示名称
		/// </summary>
		public string		name;
		
		/// <summary>
		/// 是否允许连接多条边
		/// </summary>
		public bool			allowMultiple = true;

		/// <summary>
		/// 将字段标记为输出端口
		/// </summary>
		/// <param name="name">显示名称</param>
		/// <param name="allowMultiple">是否允许连接多条边</param>
		public OutputAttribute(string name = null, bool allowMultiple = true)
		{
			this.name = name;
			this.allowMultiple = allowMultiple;
		}
	}

	/// <summary>
	/// 垂直属性
	/// 创建垂直端口而不是默认的水平端口
	/// 用于控制端口在节点上的显示方向
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class VerticalAttribute : Attribute
	{
	}

	/// <summary>
	/// 节点菜单项属性
	/// 在NodeProvider类中注册节点。节点也将在节点创建窗口中可用
	/// 用于定义节点在创建菜单中的位置和兼容性
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class NodeMenuItemAttribute : Attribute
	{
		/// <summary>
		/// 菜单标题路径
		/// </summary>
		public string	menuTitle;
		
		/// <summary>
		/// 仅兼容的图形类型
		/// </summary>
		public Type		onlyCompatibleWithGraph;

		/// <summary>
		/// 在NodeProvider类中注册节点。节点也将在节点创建窗口中可用
		/// </summary>
		/// <param name="menuTitle">菜单中的路径，使用/作为文件夹分隔符</param>
		/// <param name="onlyCompatibleWithGraph">仅兼容的图形类型</param>
		public NodeMenuItemAttribute(string menuTitle = null, Type onlyCompatibleWithGraph = null)
		{
			this.menuTitle = menuTitle;
			this.onlyCompatibleWithGraph = onlyCompatibleWithGraph;
		}
	}

	/// <summary>
	/// 字段绘制器属性（已过时）
	/// 为字段设置自定义绘制器。然后可以使用FieldFactory创建它
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	[Obsolete("You can use the standard Unity CustomPropertyDrawer instead.")]
	public class FieldDrawerAttribute : Attribute
	{
		/// <summary>
		/// 字段类型
		/// </summary>
		public Type		fieldType;

		/// <summary>
		/// 在FieldFactory类中为类型注册自定义视图
		/// </summary>
		/// <param name="fieldType">字段类型</param>
		public FieldDrawerAttribute(Type fieldType)
		{
			this.fieldType = fieldType;
		}
	}

	/// <summary>
	/// 自定义端口输入属性
	/// 允许您自定义端口的输入函数
	/// 用于实现自定义的数据输入逻辑
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CustomPortInputAttribute : Attribute
	{
		/// <summary>
		/// 字段名称
		/// </summary>
		public string	fieldName;
		
		/// <summary>
		/// 输入类型
		/// </summary>
		public Type		inputType;
		
		/// <summary>
		/// 是否允许类型转换
		/// </summary>
		public bool		allowCast;

		/// <summary>
		/// 允许您自定义端口的输入函数
		/// 请参阅Samples中的CustomPortsNode示例
		/// </summary>
		/// <param name="fieldName">节点的本地字段</param>
		/// <param name="inputType">端口的输入类型</param>
		/// <param name="allowCast">连接边时是否允许转换</param>
		public CustomPortInputAttribute(string fieldName, Type inputType, bool allowCast = true)
		{
			this.fieldName = fieldName;
			this.inputType = inputType;
			this.allowCast = allowCast;
		}
	}

	/// <summary>
	/// 自定义端口输出属性
	/// 允许您自定义端口的输出函数
	/// 用于实现自定义的数据输出逻辑
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CustomPortOutputAttribute : Attribute
	{
		/// <summary>
		/// 字段名称
		/// </summary>
		public string	fieldName;
		
		/// <summary>
		/// 输出类型
		/// </summary>
		public Type		outputType;
		
		/// <summary>
		/// 是否允许类型转换
		/// </summary>
		public bool		allowCast;

		/// <summary>
		/// 允许您自定义端口的输出函数
		/// 请参阅Samples中的CustomPortsNode示例
		/// </summary>
		/// <param name="fieldName">节点的本地字段</param>
		/// <param name="outputType">端口的输出类型</param>
		/// <param name="allowCast">连接边时是否允许转换</param>
		public CustomPortOutputAttribute(string fieldName, Type outputType, bool allowCast = true)
		{
			this.fieldName = fieldName;
			this.outputType = outputType;
			this.allowCast = allowCast;
		}
	}

	/// <summary>
	/// 自定义端口行为属性
	/// 允许您修改从字段生成的端口视图。可用于从一个字段生成多个端口
	/// 用于实现动态端口生成和自定义端口行为
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CustomPortBehaviorAttribute : Attribute
	{
		/// <summary>
		/// 字段名称
		/// </summary>
		public string		fieldName;

		/// <summary>
		/// 允许您修改从字段生成的端口视图。可用于从一个字段生成多个端口
		/// 您必须在此签名的函数上添加此属性
		/// <code>
		/// IEnumerable&lt;PortData&gt; MyCustomPortFunction(List&lt;SerializableEdge&gt; edges);
		/// </code>
		/// </summary>
		/// <param name="fieldName">本地节点字段名</param>
		public CustomPortBehaviorAttribute(string fieldName)
		{
			this.fieldName = fieldName;
		}
	}

	/// <summary>
	/// 自定义端口类型行为属性
	/// 允许绑定方法以基于节点中的字段类型生成特定的端口集
	/// 用于根据字段类型动态生成端口
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class CustomPortTypeBehavior : Attribute
	{
		/// <summary>
		/// 目标类型
		/// </summary>
		public Type type;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="type">目标类型</param>
		public CustomPortTypeBehavior(Type type)
		{
			this.type = type;
		}
	}

	/// <summary>
	/// 自定义堆栈节点视图属性
	/// 允许您为堆栈节点拥有自定义视图
	/// 用于自定义堆栈节点的UI显示
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class CustomStackNodeView : Attribute
	{
		/// <summary>
		/// 堆栈节点类型
		/// </summary>
		public Type	stackNodeType;

		/// <summary>
		/// 允许您为堆栈节点拥有自定义视图
		/// </summary>
		/// <param name="stackNodeType">您目标的堆栈节点类型</param>
		public CustomStackNodeView(Type stackNodeType)
		{
			this.stackNodeType = stackNodeType;
		}
	}

	/// <summary>
	/// 条件可见属性
	/// 根据其他字段的值控制当前字段的可见性
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class VisibleIf : Attribute
	{
		/// <summary>
		/// 条件字段名称
		/// </summary>
		public string fieldName;
		
		/// <summary>
		/// 条件值
		/// </summary>
		public object value;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="fieldName">条件字段名称</param>
		/// <param name="value">条件值</param>
		public VisibleIf(string fieldName, object value)
		{
			this.fieldName = fieldName;
			this.value = value;
		}
	}

	/// <summary>
	/// 在检查器中显示属性
	/// 控制字段是否在检查器中显示
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class ShowInInspector : Attribute
	{
		/// <summary>
		/// 是否在节点中显示
		/// </summary>
		public bool showInNode;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="showInNode">是否在节点中显示</param>
		public ShowInInspector(bool showInNode = false)
		{
			this.showInNode = showInNode;
		}
	}
	
	/// <summary>
	/// 显示为绘制器属性
	/// 将字段显示为自定义绘制器
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class ShowAsDrawer : Attribute
	{
	}
	
	/// <summary>
	/// 设置属性
	/// 用于标记节点设置相关的字段
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class SettingAttribute : Attribute
	{
		/// <summary>
		/// 设置名称
		/// </summary>
		public string name;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="name">设置名称</param>
		public SettingAttribute(string name = null)
		{
			this.name = name;
		}
	}

	/// <summary>
	/// 图形兼容性属性
	/// 用于标记方法以检查节点与图形的兼容性
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class IsCompatibleWithGraph : Attribute {}
}