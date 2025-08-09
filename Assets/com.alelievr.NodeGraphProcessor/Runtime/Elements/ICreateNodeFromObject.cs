using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace GraphProcessor
{
    /// <summary>
    /// 在BaseNode上实现此接口，如果类型为T的资源被拖放到图形视图区域，它允许您自动生成节点
    /// </summary>
    /// <typeparam name="T">您的节点将从中创建的类型对象，它必须是UnityEngine.Object的子类</typeparam>
    public interface ICreateNodeFrom<T> where T : Object
    {
        /// <summary>
        /// 从对象创建节点后立即调用此函数，它允许您使用对象数据初始化节点。
        /// </summary>
        /// <param name="value">对象值</param>
        /// <returns>如果初始化正确发生则为True。否则为False，返回false将丢弃您的节点。</returns>
        bool InitializeNodeFromObject(T value);
    }
}