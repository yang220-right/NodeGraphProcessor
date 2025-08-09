using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using UnityEditor.ProjectWindowCallback;

namespace GraphProcessor
{
	/// <summary>
	/// 节点图形处理器菜单项类
	/// 要添加创建节点C#脚本模板文件的菜单项，您可以继承此类并使用其API结合[MenuItem]
	/// 有关实现详细信息，请参阅examples中的GraphProcessorMenuItems.cs
	/// 提供节点和节点视图脚本模板的创建功能
	/// </summary>
	public class NodeGraphProcessorMenuItems
	{
		/// <summary>
		/// 节点基础文件名
		/// 默认的节点脚本文件名
		/// </summary>
		static readonly string		nodeBaseName = "Node.cs";
		
		/// <summary>
		/// 节点视图基础文件名
		/// 默认的节点视图脚本文件名
		/// </summary>
		static readonly string		nodeViewBaseName = "NodeView.cs";
        
		/// <summary>
		/// 节点模板路径缓存
		/// </summary>
        static string      _nodeTemplatePath = null;
        
		/// <summary>
		/// 节点模板路径属性
		/// 获取节点脚本模板文件的路径
		/// </summary>
        static string      nodeTemplatePath
		{
			get
			{
				if (_nodeTemplatePath == null)
				{
					var template = Resources.Load<TextAsset>("NodeTemplate.cs");
					_nodeTemplatePath = AssetDatabase.GetAssetPath(template);
				}
				return _nodeTemplatePath;
			}
		}
        
		/// <summary>
		/// 节点视图模板路径缓存
		/// </summary>
        static string      _nodeViewTemplatePath;
        
		/// <summary>
		/// 节点视图模板路径属性
		/// 获取节点视图脚本模板文件的路径
		/// </summary>
        static string      nodeViewTemplatePath
		{
			get
			{
				if (_nodeViewTemplatePath == null)
				{
					var template = Resources.Load<TextAsset>("NodeViewTemplate.cs");
					_nodeViewTemplatePath = AssetDatabase.GetAssetPath(template);
				}
				return _nodeViewTemplatePath;
			}
		}

		/// <summary>
		/// 菜单项位置类
		/// 定义菜单项在Unity菜单中的位置常量
		/// </summary>
		protected static class MenuItemPosition
		{
			/// <summary>
			/// 在创建脚本之后的位置
			/// </summary>
			public const int afterCreateScript = 81;
			
			/// <summary>
			/// 在创建脚本之前的位置
			/// </summary>
			public const int beforeCreateScript = 79;
		}

        /// <summary>
        /// 获取当前项目窗口路径
        /// 获取当前在项目窗口中选中的路径
        /// </summary>
        /// <returns>当前选中的路径，如果没有选中则返回null</returns>
        protected static string GetCurrentProjectWindowPath()
        {
			var path = "";
			var obj = Selection.activeObject;

			if (obj == null)
                return null;
			else
				path = AssetDatabase.GetAssetPath(obj.GetInstanceID());

			if (path.Length > 0)
			{
				if (Directory.Exists(path))
					return path;
				else
					return new FileInfo(path).Directory.FullName;
			}
			return null;
        }

		/// <summary>
		/// 创建默认节点C#脚本
		/// 使用模板文件创建新的节点脚本
		/// </summary>
		protected static void CreateDefaultNodeCSharpScritpt()
		{
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(nodeTemplatePath, nodeBaseName);
		}

		/// <summary>
		/// 创建默认节点视图C#脚本
		/// 使用模板文件创建新的节点视图脚本
		/// </summary>
		protected static void CreateDefaultNodeViewCSharpScritpt()
		{
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(nodeViewTemplatePath, nodeViewBaseName);
		}
	}
}
