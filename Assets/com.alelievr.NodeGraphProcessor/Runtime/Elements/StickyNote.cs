using System;
using UnityEngine;

namespace GraphProcessor
{
    /// <summary>
    /// 可序列化的便签节点类
    /// </summary>
    [Serializable]
    public class StickyNote
    {
        public Rect position;
        public string title = "Hello World!";
        public string content = "Description";

        public StickyNote(string title, Vector2 position)
        {
            this.title = title;
            this.position = new Rect(position.x, position.y, 200, 300);
        }
    }
}