using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

public class ActionProcessor : BaseGraphProcessor
{
    List<BaseNode> processList;
    public ActionProcessor(BaseGraph graph) : base(graph){ }


    public override void UpdateComputeOrder(){
        processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
    }

    public override void Run(){
        IEnumerator<BaseNode> enumerator = RunTheGraph();
        while (enumerator.MoveNext());
    }
    private IEnumerator<BaseNode> RunTheGraph(){
        int count = processList.Count;

        for (int i = 0; i < count; i++){
            processList[i].OnProcess();
            yield return processList[i];
        }
    }
}
