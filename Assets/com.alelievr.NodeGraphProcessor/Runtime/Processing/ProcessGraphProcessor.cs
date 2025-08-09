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
	/// 图形处理器
	/// </summary>
	public class ProcessGraphProcessor : BaseGraphProcessor
	{
		List< BaseNode >		processList;
		
		/// <summary>
		/// 管理图形调度和处理
		/// </summary>
		/// <param name="graph">要处理的图形</param>
		public ProcessGraphProcessor(BaseGraph graph) : base(graph) {}

		public override void UpdateComputeOrder()
		{
			processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
		}

		/// <summary>
		/// 按照计算顺序处理所有节点。
		/// </summary>
		public override void Run()
		{
			int count = processList.Count;

			for (int i = 0; i < count; i++)
				processList[i].OnProcess();
		}
	}
}
