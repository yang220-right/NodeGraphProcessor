using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
// using Unity.Entities;

namespace GraphProcessor
{

	/// <summary>
	/// 同步图形处理器
	/// 继承自BaseGraphProcessor，提供同步执行图形节点的功能
	/// 按照节点的计算顺序依次执行每个节点的OnProcess方法
	/// 适用于需要立即获得执行结果的场景
	/// </summary>
	public class ProcessGraphProcessor : BaseGraphProcessor
	{
		/// <summary>
		/// 按计算顺序排序的节点列表
		/// 存储所有需要处理的节点，按照它们的computeOrder属性排序
		/// </summary>
		List< BaseNode >		processList;
		
		/// <summary>
		/// 构造函数，初始化同步图形处理器
		/// </summary>
		/// <param name="graph">要处理的图形对象</param>
		public ProcessGraphProcessor(BaseGraph graph) : base(graph) {}

		/// <summary>
		/// 更新节点计算顺序
		/// 根据节点的computeOrder属性对节点进行排序
		/// 确保节点按照正确的依赖顺序执行
		/// </summary>
		public override void UpdateComputeOrder()
		{
			// 按照计算顺序对节点进行排序并存储到列表中
			processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
		}

		/// <summary>
		/// 同步执行图形中的所有节点
		/// 按照预先计算好的顺序依次调用每个节点的OnProcess方法
		/// 所有节点都会在当前帧内完成执行
		/// </summary>
		public override void Run()
		{
			// 获取需要处理的节点数量
			int count = processList.Count;

			// 按照计算顺序依次处理每个节点
			for (int i = 0; i < count; i++)
				processList[i].OnProcess();
		}
	}
}
