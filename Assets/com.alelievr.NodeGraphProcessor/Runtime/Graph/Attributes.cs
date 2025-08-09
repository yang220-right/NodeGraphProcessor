using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GraphProcessor
{
	/// <summary>
	/// 告诉这个字段将生成一个输入端口
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class InputAttribute : Attribute
	{
		public string		name;
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
	/// 告诉这个字段将生成一个输出端口
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class OutputAttribute : Attribute
	{
		public string		name;
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
	/// 创建垂直端口而不是默认的水平端口
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class VerticalAttribute : Attribute
	{
	}

	/// <summary>
	/// 在NodeProvider类中注册节点。节点也将在节点创建窗口中可用。
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class NodeMenuItemAttribute : Attribute
	{
		public string	menuTitle;
		public Type		onlyCompatibleWithGraph;

		/// <summary>
		/// 在NodeProvider类中注册节点。节点也将在节点创建窗口中可用。
		/// </summary>
		/// <param name="menuTitle">菜单中的路径，使用/作为文件夹分隔符</param>
		public NodeMenuItemAttribute(string menuTitle = null, Type onlyCompatibleWithGraph = null)
		{
			this.menuTitle = menuTitle;
			this.onlyCompatibleWithGraph = onlyCompatibleWithGraph;
		}
	}

	/// <summary>
	/// 为字段设置自定义绘制器。然后可以使用FieldFactory创建它
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	[Obsolete("You can use the standard Unity CustomPropertyDrawer instead.")]
	public class FieldDrawerAttribute : Attribute
	{
		public Type		fieldType;

		/// <summary>
		/// 在FieldFactory类中为类型注册自定义视图
		/// </summary>
		/// <param name="fieldType"></param>
		public FieldDrawerAttribute(Type fieldType)
		{
			this.fieldType = fieldType;
		}
	}

	/// <summary>
	/// 允许您自定义端口的输入函数
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CustomPortInputAttribute : Attribute
	{
		public string	fieldName;
		public Type		inputType;
		public bool		allowCast;

		/// <summary>
		/// 允许您自定义端口的输入函数。
		/// 请参阅Samples中的CustomPortsNode示例。
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
	/// 允许您自定义端口的输出函数
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CustomPortOutputAttribute : Attribute
	{
		public string	fieldName;
		public Type		outputType;
		public bool		allowCast;

		/// <summary>
		/// 允许您自定义端口的输出函数。
		/// 请参阅Samples中的CustomPortsNode示例。
		/// </summary>
		/// <param name="fieldName">节点的本地字段</param>
		/// <param name="inputType">端口的输入类型</param>
		/// <param name="allowCast">连接边时是否允许转换</param>
		public CustomPortOutputAttribute(string fieldName, Type outputType, bool allowCast = true)
		{
			this.fieldName = fieldName;
			this.outputType = outputType;
			this.allowCast = allowCast;
		}
	}

	/// <summary>
	/// 允许您修改从字段生成的端口视图。可用于从一个字段生成多个端口。
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CustomPortBehaviorAttribute : Attribute
	{
		public string		fieldName;

		/// <summary>
		/// 允许您修改从字段生成的端口视图。可用于从一个字段生成多个端口。
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
	/// 允许绑定方法以基于节点中的字段类型生成特定的端口集
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class CustomPortTypeBehavior : Attribute
	{
		/// <summary>
		/// 目标类型
		/// </summary>
		public Type type;

		public CustomPortTypeBehavior(Type type)
		{
			this.type = type;
		}
	}

	/// <summary>
	/// 允许您为堆栈节点拥有自定义视图
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class CustomStackNodeView : Attribute
	{
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

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class VisibleIf : Attribute
	{
		public string fieldName;
		public object value;

		public VisibleIf(string fieldName, object value)
		{
			this.fieldName = fieldName;
			this.value = value;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class ShowInInspector : Attribute
	{
		public bool showInNode;

		public ShowInInspector(bool showInNode = false)
		{
			this.showInNode = showInNode;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class ShowAsDrawer : Attribute
	{
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class SettingAttribute : Attribute
	{
		public string name;

		public SettingAttribute(string name = null)
		{
			this.name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class IsCompatibleWithGraph : Attribute {}
}