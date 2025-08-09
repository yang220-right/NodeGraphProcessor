using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace GraphProcessor
{
	/// <summary>
	/// 节点徽章视图类
	/// 继承自IconBadge，提供节点消息的可视化显示
	/// 用于显示警告、错误、信息等不同类型的节点消息
	/// </summary>
	public class NodeBadgeView : IconBadge
	{
		/// <summary>
		/// 标签
		/// 显示消息文本的UI元素
		/// </summary>
		Label		label;
		
		/// <summary>
		/// 图标
		/// 显示消息类型的图标
		/// </summary>
		Texture		icon;
		
		/// <summary>
		/// 颜色
		/// 徽章的颜色
		/// </summary>
		Color		color;
		
		/// <summary>
		/// 是否自定义
		/// 标识是否为自定义徽章
		/// </summary>
		bool		isCustom;

		/// <summary>
		/// 构造函数（消息类型版本）
		/// 根据消息类型创建对应的徽章
		/// </summary>
		/// <param name="message">消息文本</param>
		/// <param name="messageType">消息类型</param>
		public NodeBadgeView(string message, NodeMessageType messageType)
		{
			switch (messageType)
			{
				case NodeMessageType.Warning:
					// 警告类型：黄色图标
					CreateCustom(message, EditorGUIUtility.IconContent("Collab.Warning").image, Color.yellow);
					break ;
				case NodeMessageType.Error:	
					// 错误类型：红色图标
					CreateCustom(message, EditorGUIUtility.IconContent("Collab.Warning").image, Color.red);
					break ;
				case NodeMessageType.Info:
					// 信息类型：白色图标
					CreateCustom(message, EditorGUIUtility.IconContent("console.infoicon").image, Color.white);
					break ;
				default:
				case NodeMessageType.None:
					// 无类型：灰色图标
					CreateCustom(message, null, Color.grey);
					break ;
			}
		}

		/// <summary>
		/// 构造函数（自定义版本）
		/// 创建自定义图标和颜色的徽章
		/// </summary>
		/// <param name="message">消息文本</param>
		/// <param name="icon">自定义图标</param>
		/// <param name="color">自定义颜色</param>
		public NodeBadgeView(string message, Texture icon, Color color)
		{
			CreateCustom(message, icon, color);
		}

		/// <summary>
		/// 创建自定义徽章
		/// 设置徽章的文本、图标和颜色
		/// </summary>
		/// <param name="message">消息文本</param>
		/// <param name="icon">图标</param>
		/// <param name="color">颜色</param>
		void CreateCustom(string message, Texture icon, Color color)
		{
			badgeText = message;
			this.color = color;
			
			// 设置图标和样式
			var image = this.Q< Image >("icon");
			image.image = icon;
			image.style.backgroundColor = color;
			style.color = color;
			
			// 这将设置一个包含字符串哈希码的类名
			// 我们使用这个小技巧在标签添加到图形后检索它
			visualStyle = badgeText.GetHashCode().ToString();
		}

		/// <summary>
		/// 执行默认动作
		/// 处理鼠标进入事件，动态设置标签颜色
		/// </summary>
		/// <param name="evt">事件基类</param>
		protected override void ExecuteDefaultAction(EventBase evt)
		{
			// 当鼠标进入图标时，这将把标签添加到层次结构中
			base.ExecuteDefaultAction(evt);

            if (evt.eventTypeId == MouseEnterEvent.TypeId())
			{
				// 然后我们可以在这里获取它：
				GraphView gv = GetFirstAncestorOfType<GraphView>();
				var label = gv.Q<Label>(classes: new string[]{"icon-badge__text--" + badgeText.GetHashCode()});
				if (label != null)
					label.style.color = color;
			}
		}
	}
}