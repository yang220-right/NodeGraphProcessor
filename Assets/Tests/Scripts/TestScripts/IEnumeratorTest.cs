using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using Sirenix.OdinInspector;
using UnityEngine;

public class IEnumeratorTest : MonoBehaviour
{
    [Header("Graph to Run on Start")] public BaseGraph graph;
    private ConditionalProcessor processor;
    [Button]
    private void Start(){
        if (graph != null)
            processor = new ConditionalProcessor(graph);

        processor.Run();
    }
   
}
