using UnityEngine;
using System.Collections.Generic;

namespace GraphProcessor
{
    /// <summary>
    /// 堆栈节点基类
    /// StackNode视图的数据容器，用于创建可折叠的节点组
    /// 提供节点的组织和管理功能，支持拖放和动态创建节点
    /// </summary>
    [System.Serializable]
    public class BaseStackNode
    {
        /// <summary>
        /// 堆栈位置
        /// 堆栈在图形中的位置坐标
        /// </summary>
        public Vector2 position;
        
        /// <summary>
        /// 堆栈标题
        /// 在UI中显示的堆栈名称
        /// </summary>
        public string title = "New Stack";
        
        /// <summary>
        /// 是否接受拖放节点
        /// 控制堆栈是否允许通过拖放操作添加节点
        /// </summary>
        public bool acceptDrop;

        /// <summary>
        /// 是否接受新节点
        /// 控制堆栈是否接受通过在堆栈节点上按空格创建的节点
        /// </summary>
        public bool acceptNewNode;

        /// <summary>
        /// 堆栈中节点的GUID列表
        /// 存储堆栈内所有节点的唯一标识符
        /// </summary>
        public List< string >   nodeGUIDs = new List< string >();

        /// <summary>
        /// 构造函数
        /// 创建新的堆栈节点
        /// </summary>
        /// <param name="position">堆栈位置</param>
        /// <param name="title">堆栈标题</param>
        /// <param name="acceptDrop">是否接受拖放</param>
        /// <param name="acceptNewNode">是否接受新节点</param>
        public BaseStackNode(Vector2 position, string title = "Stack", bool acceptDrop = true, bool acceptNewNode = true)
        {
            this.position = position;
            this.title = title;
            this.acceptDrop = acceptDrop;
            this.acceptNewNode = acceptNewNode;
        }
    }
}