using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using Sirenix.OdinInspector;
using UnityEngine;

public class ActionMono : MonoBehaviour
{
    [Header("Graph to Run on Start")] public BaseGraph graph;
    public ActionProcessor processor;
        
    [Button]
    private void Run(){
        if(graph != null)
            processor = new ActionProcessor(graph);
        processor.Run();
    }
    public Animator childAnim;
}
