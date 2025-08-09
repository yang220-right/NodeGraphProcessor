using UnityEngine;
using UnityEditor;

namespace GraphProcessor
{
	/// <summary>
	/// 删除回调类
	/// 处理图形资源删除时的回调逻辑
	/// 当BaseGraph资源被删除时，通知所有相关的图形窗口
	/// </summary>
	[ExecuteAlways]
	public class DeleteCallback : UnityEditor.AssetModificationProcessor
	{
		/// <summary>
		/// 资源删除前回调
		/// 当资源即将被删除时调用，用于处理图形资源的清理工作
		/// </summary>
		/// <param name="path">要删除的资源路径</param>
		/// <param name="options">删除选项</param>
		/// <returns>删除结果</returns>
		static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
		{
			// 加载路径下的所有资源
			var objects = AssetDatabase.LoadAllAssetsAtPath(path);

			// 遍历所有资源，查找BaseGraph类型的资源
			foreach (var obj in objects)
			{
				if (obj is BaseGraph b)
				{
					// 通知所有图形窗口图形已被删除
					foreach (var graphWindow in Resources.FindObjectsOfTypeAll< BaseGraphWindow >())
						graphWindow.OnGraphDeleted();
					
					// 调用图形的删除回调
					b.OnAssetDeleted();
				}
			}

			// 返回未删除结果，允许Unity继续删除过程
			return AssetDeleteResult.DidNotDelete;
		}
	}
}