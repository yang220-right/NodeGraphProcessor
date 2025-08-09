using System;
using System.Collections;
using GraphProcessor;
using UnityEngine;

namespace NodeGraphProcessor.Examples
{
	[Serializable, NodeMenuItem("Functions/Wait")]
	public class WaitNode : WaitableNode
	{
		public override string name => "Wait";

		[SerializeField, Input(name = "Seconds")]
		public float waitTime = 1f;

		private static WaitMonoBehaviour waitMonoBehaviour;

		protected override void Process()
		{
					//	我们应该检查这个Process()是从哪里调用的。但我不知道这是否是一个优雅且高效的方法。
		//	如果这个函数不是从ConditionalNode调用的，那么就会出现问题、错误、不可预见的后果、眼泪。
		// var isCalledFromConditionalProcessor = new StackTrace().GetFrame(5).GetMethod().ReflectedType == typeof(ConditionalProcessor);
		// if(!isCalledFromConditionalProcessor) return;
			
			if(waitMonoBehaviour == null)
			{
				var go = new GameObject(name: "WaitGameObject");
				waitMonoBehaviour = go.AddComponent<WaitMonoBehaviour>();
			}

			waitMonoBehaviour.Process(waitTime, ProcessFinished);
		}
	}

	public class WaitMonoBehaviour : MonoBehaviour
	{
		public void Process(float time, Action callback)
		{
			StartCoroutine(_Process(time, callback));
		}

		private IEnumerator _Process(float time, Action callback)
		{
			yield return new WaitForSeconds(time);
			callback.Invoke();
		}
	}
}