using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;

// using Unity.Entities;

namespace GraphProcessor{
  /// <summary>
  /// 作业图形处理器
  /// 继承自BaseGraphProcessor，提供基于Unity作业系统的异步图形执行功能
  /// 支持并行处理和依赖关系管理，适用于计算密集型任务
  /// 利用Unity的Job System来实现高性能的节点执行
  /// </summary>
  public class JobGraphProcessor : BaseGraphProcessor{
    /// <summary>
    /// 图形调度列表数组
    /// 存储所有节点及其依赖关系信息，用于作业系统的调度
    /// </summary>
    GraphScheduleList[] scheduleList;

    /// <summary>
    /// 图形调度列表项
    /// 内部类，用于存储单个节点的调度信息
    /// </summary>
    internal class GraphScheduleList{
      /// <summary>
      /// 要调度的节点
      /// </summary>
      public BaseNode node;

      /// <summary>
      /// 该节点的依赖节点数组
      /// 这些节点必须在当前节点之前执行完成
      /// </summary>
      public BaseNode[] dependencies;

      /// <summary>
      /// 构造函数，初始化调度列表项
      /// </summary>
      /// <param name="node">要调度的节点</param>
      public GraphScheduleList(BaseNode node){
        this.node = node;
      }
    }

    /// <summary>
    /// 构造函数，初始化作业图形处理器
    /// </summary>
    /// <param name="graph">要处理的图形对象</param>
    public JobGraphProcessor(BaseGraph graph) : base(graph){
    }

    /// <summary>
    /// 更新节点计算顺序并构建调度列表
    /// 根据节点的computeOrder属性排序，并为每个节点收集其依赖关系
    /// </summary>
    public override void UpdateComputeOrder(){
      // 按照计算顺序对节点进行排序，并为每个节点创建调度列表项
      scheduleList = graph.nodes.OrderBy(n => n.computeOrder).Select(n => {
        GraphScheduleList gsl = new GraphScheduleList(n);
        // 获取该节点的所有输入节点作为依赖关系
        gsl.dependencies = n.GetInputNodes().ToArray();
        return gsl;
      }).ToArray();
    }

    /// <summary>
    /// 将图形调度到Unity作业系统中执行
    /// 根据节点间的依赖关系创建作业依赖链，实现并行处理
    /// </summary>
    public override void Run(){
      // 获取需要调度的节点数量
      int count = scheduleList.Length;
      // 存储每个节点对应的作业句柄
      var scheduledHandles = new Dictionary<BaseNode, JobHandle>();

      // 遍历所有节点，创建作业依赖关系
      for (int i = 0; i < count; i++){
        // 初始化依赖作业句柄
        JobHandle dep = default(JobHandle);
        var schedule = scheduleList[i];
        int dependenciesCount = schedule.dependencies.Length;

        // 合并所有依赖节点的作业句柄
        for (int j = 0; j < dependenciesCount; j++)
          dep = JobHandle.CombineDependencies(dep, scheduledHandles[schedule.dependencies[j]]);

        // TODO: 在当前节点上调用onSchedule
        // 这里应该调用节点的OnSchedule方法来创建实际的作业
        // JobHandle currentJob = schedule.node.OnSchedule(dep);
        // scheduledHandles[schedule.node] = currentJob;
      }

      // 调度所有批处理的作业
      JobHandle.ScheduleBatchedJobs();
    }
  }
}