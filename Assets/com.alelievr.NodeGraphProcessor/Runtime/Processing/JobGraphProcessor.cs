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
	public class JobGraphProcessor : BaseGraphProcessor
	{
		GraphScheduleList[]			scheduleList;
		
		internal class GraphScheduleList
		{
			public BaseNode			node;
			public BaseNode[]		dependencies;
	
			public GraphScheduleList(BaseNode node)
			{
				this.node = node;
			}
		}

		/// <summary>
		/// 管理图形调度和处理
		/// </summary>
		/// <param name="graph">要处理的图形</param>
		public JobGraphProcessor(BaseGraph graph) : base(graph) {}

		public override void UpdateComputeOrder()
		{
			scheduleList = graph.nodes.OrderBy(n => n.computeOrder).Select(n => {
				GraphScheduleList gsl = new GraphScheduleList(n);
				gsl.dependencies = n.GetInputNodes().ToArray();
				return gsl;
			}).ToArray();
		}

		/// <summary>
		/// 将图形调度到作业系统中
		/// </summary>
		public override void Run()
		{
			int count = scheduleList.Length;
			var scheduledHandles = new Dictionary< BaseNode, JobHandle >();

			for (int i = 0; i < count; i++)
			{
				JobHandle dep = default(JobHandle);
				var schedule = scheduleList[i];
				int dependenciesCount = schedule.dependencies.Length;

				for (int j = 0; j < dependenciesCount; j++)
					dep = JobHandle.CombineDependencies(dep, scheduledHandles[schedule.dependencies[j]]);

				// TODO: 在当前节点上调用onSchedule
				// JobHandle currentJob = schedule.node.OnSchedule(dep);
				// scheduledHandles[schedule.node] = currentJob;
			}

			JobHandle.ScheduleBatchedJobs();
		}
	}
}
