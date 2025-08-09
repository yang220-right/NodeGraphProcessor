using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	/// <summary>
	/// 创建时对选定的节点进行分组
	/// </summary>
	[System.Serializable]
	public class Group
	{
		public string			title;
		public Color			color = new Color(0, 0, 0, 0.3f);
		public Rect				position;
        public Vector2          size;

		/// <summary>
		/// 存储组中节点的GUID
		/// </summary>
		/// <typeparam name="string">节点的GUID</typeparam>
		/// <returns></returns>
		public List< string >	innerNodeGUIDs = new List< string >();

		// 用于序列化加载
        public Group() {}

		/// <summary>
		/// 使用标题和位置创建新组
		/// </summary>
		/// <param name="title"></param>
		/// <param name="position"></param>
        public Group(string title, Vector2 position)
		{
			this.title = title;
            this.position.position = position;
		}

		/// <summary>
		/// 创建组时调用
		/// </summary>
        public virtual void OnCreated()
        {
            size = new Vector2(400, 200);
            position.size = size;
        }
	}
}