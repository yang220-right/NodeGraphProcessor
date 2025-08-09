using UnityEngine;
using System.Collections.Generic;

namespace GraphProcessor
{
    /// <summary>
    /// StackNode视图的数据容器
    /// </summary>
    [System.Serializable]
    public class BaseStackNode
    {
        public Vector2 position;
        public string title = "New Stack";
        
        /// <summary>
        /// 堆栈是否接受拖放节点
        /// </summary>
        public bool acceptDrop;

        /// <summary>
        /// 堆栈是否接受通过在堆栈节点上按空格创建的节点
        /// </summary>
        public bool acceptNewNode;

        /// <summary>
        /// 堆栈中节点的GUID列表
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public List< string >   nodeGUIDs = new List< string >();

        public BaseStackNode(Vector2 position, string title = "Stack", bool acceptDrop = true, bool acceptNewNode = true)
        {
            this.position = position;
            this.title = title;
            this.acceptDrop = acceptDrop;
            this.acceptNewNode = acceptNewNode;
        }
    }
}