using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	/// <summary>
	/// 组类
	/// 用于在图形中对节点进行分组管理
	/// 创建时对选定的节点进行分组，提供视觉组织和逻辑分组功能
	/// </summary>
	[System.Serializable]
	public class Group
	{
		/// <summary>
		/// 组标题
		/// 在UI中显示的组名称
		/// </summary>
		public string			title;
		
		/// <summary>
		/// 组颜色
		/// 组的背景颜色，默认为半透明黑色
		/// </summary>
		public Color			color = new Color(0, 0, 0, 0.3f);
		
		/// <summary>
		/// 组位置
		/// 组在图形中的位置和大小
		/// </summary>
		public Rect				position;
        
		/// <summary>
		/// 组大小
		/// 组的尺寸信息
		/// </summary>
        public Vector2          size;

		/// <summary>
		/// 组内节点GUID列表
		/// 存储组中节点的GUID，用于标识和管理组内的节点
		/// </summary>
		public List< string >	innerNodeGUIDs = new List< string >();

		/// <summary>
		/// 默认构造函数
		/// 用于序列化加载
		/// </summary>
        public Group() {}

		/// <summary>
		/// 带参数的构造函数
		/// 使用标题和位置创建新组
		/// </summary>
		/// <param name="title">组标题</param>
		/// <param name="position">组位置</param>
        public Group(string title, Vector2 position)
		{
			this.title = title;
            this.position.position = position;
		}

		/// <summary>
		/// 组创建时的初始化
		/// 设置组的默认大小和位置
		/// </summary>
        public virtual void OnCreated()
        {
            // 设置默认组大小
            size = new Vector2(400, 200);
            position.size = size;
        }
	}
}