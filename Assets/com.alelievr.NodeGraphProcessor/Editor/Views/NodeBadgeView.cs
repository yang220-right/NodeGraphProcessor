using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace GraphProcessor
{
	public class NodeBadgeView : IconBadge
	{
		Label		label;
		Texture		icon;
		Color		color;
		bool		isCustom;

		public NodeBadgeView(string message, NodeMessageType messageType)
		{
			switch (messageType)
			{
				case NodeMessageType.Warning:
					CreateCustom(message, EditorGUIUtility.IconContent("Collab.Warning").image, Color.yellow);
					break ;
				case NodeMessageType.Error:	
					CreateCustom(message, EditorGUIUtility.IconContent("Collab.Warning").image, Color.red);
					break ;
				case NodeMessageType.Info:
					CreateCustom(message, EditorGUIUtility.IconContent("console.infoicon").image, Color.white);
					break ;
				default:
				case NodeMessageType.None:
					CreateCustom(message, null, Color.grey);
					break ;
			}
		}

		public NodeBadgeView(string message, Texture icon, Color color)
		{
			CreateCustom(message, icon, color);
		}

		void CreateCustom(string message, Texture icon, Color color)
		{
			badgeText = message;
			this.color = color;
			
			var image = this.Q< Image >("icon");
			image.image = icon;
			image.style.backgroundColor = color;
			style.color = color;
			// 这将设置一个包含字符串哈希码的类名
			// 我们使用这个小技巧在标签添加到图形后检索它
			visualStyle = badgeText.GetHashCode().ToString();
		}

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