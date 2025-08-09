using System;
using UnityEngine;

namespace GraphProcessor
{
    /// <summary>
    /// 便签类
    /// 可序列化的便签节点类，用于在图形中添加注释和说明
    /// 提供文本注释功能，帮助用户理解图形的结构和逻辑
    /// </summary>
    [Serializable]
    public class StickyNote
    {
        /// <summary>
        /// 便签位置和大小
        /// 便签在图形中的位置和尺寸信息
        /// </summary>
        public Rect position;
        
        /// <summary>
        /// 便签标题
        /// 便签的标题文本，默认为"Hello World!"
        /// </summary>
        public string title = "Hello World!";
        
        /// <summary>
        /// 便签内容
        /// 便签的主要文本内容，默认为"Description"
        /// </summary>
        public string content = "Description";

        /// <summary>
        /// 构造函数
        /// 创建新的便签
        /// </summary>
        /// <param name="title">便签标题</param>
        /// <param name="position">便签位置</param>
        public StickyNote(string title, Vector2 position)
        {
            this.title = title;
            // 设置便签的默认大小为200x300
            this.position = new Rect(position.x, position.y, 200, 300);
        }
    }
}