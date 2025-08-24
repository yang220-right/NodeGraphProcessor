using UnityEngine;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using Sirenix.OdinInspector;

public class RuntimeConditionalGraph : MonoBehaviour{
  [Header("Graph to Run on Start")] public BaseGraph graph;

  private ConditionalProcessor processor;

  [Button]
  private void Start(){
    if (graph != null)
      processor = new ConditionalProcessor(graph);

    processor.Run();
  }
}