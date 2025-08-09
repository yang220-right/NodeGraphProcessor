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
	public abstract class BaseGraphProcessor
	{
		protected BaseGraph			graph;
		
		/// <summary>
		/// 管理图形调度和处理
		/// </summary>
		/// <param name="graph">要处理的图形</param>
		public BaseGraphProcessor(BaseGraph graph)
		{
			this.graph = graph;

			UpdateComputeOrder();
		}

		public abstract void UpdateComputeOrder();

		/// <summary>
		/// 将图形调度到作业系统中
		/// </summary>
		public abstract void Run();
	}
}
