using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;

// using Unity.Entities;

namespace GraphProcessor{
  /// <summary>
  /// 图形处理器基类
  /// 提供图形节点执行的基础框架，负责管理节点的计算顺序和执行流程
  /// 所有具体的图形处理器都应该继承此类并实现抽象方法
  /// </summary>
  public abstract class BaseGraphProcessor{
    /// <summary>
    /// 要处理的图形对象
    /// 包含所有节点、边和图形结构信息
    /// </summary>
    protected BaseGraph graph;

    /// <summary>
    /// 构造函数，初始化图形处理器
    /// </summary>
    /// <param name="graph">要处理的图形对象</param>
    public BaseGraphProcessor(BaseGraph graph){
      this.graph = graph;

      // 初始化时更新计算顺序
      UpdateComputeOrder();
    }

    /// <summary>
    /// 更新节点计算顺序的抽象方法
    /// 子类必须实现此方法来定义如何确定节点的执行顺序
    /// 通常基于节点间的依赖关系和拓扑排序算法
    /// </summary>
    public abstract void UpdateComputeOrder();

    /// <summary>
    /// 执行图形的抽象方法
    /// 子类必须实现此方法来定义如何执行图形中的节点
    /// 可以包括同步执行、异步执行或作业系统调度等不同策略
    /// </summary>
    public abstract void Run();
  }
}