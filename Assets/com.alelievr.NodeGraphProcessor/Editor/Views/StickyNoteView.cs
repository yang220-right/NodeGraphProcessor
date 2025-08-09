#if UNITY_2020_1_OR_NEWER
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GraphProcessor
{
    /// <summary>
    /// 便签视图类
    /// 继承自Unity的StickyNote，提供图形中便签的可视化表示
    /// 负责便签的显示、编辑和位置管理
    /// </summary>
    public class StickyNoteView : UnityEditor.Experimental.GraphView.StickyNote
	{
		/// <summary>
		/// 所有者图形视图
		/// 便签视图所属的图形视图
		/// </summary>
		public BaseGraphView	owner;
		
		/// <summary>
		/// 便签数据
		/// 对应的便签对象
		/// </summary>
		public StickyNote		note;

        /// <summary>
        /// 标题标签
        /// 显示便签标题的UI元素
        /// </summary>
        Label                   titleLabel;
        
        /// <summary>
        /// 颜色字段
        /// 用于选择便签颜色的UI元素
        /// </summary>
        ColorField              colorField;

        /// <summary>
        /// 构造函数
        /// 初始化便签视图的默认设置
        /// </summary>
        public StickyNoteView()
        {
            fontSize = StickyNoteFontSize.Small;
            theme = StickyNoteTheme.Classic;
		}

		/// <summary>
		/// 初始化便签视图
		/// 设置便签的基本属性和事件监听
		/// </summary>
		/// <param name="graphView">图形视图</param>
		/// <param name="note">便签数据</param>
		public void Initialize(BaseGraphView graphView, StickyNote note)
		{
			this.note = note;
			owner = graphView;

            // 注册标题字段的变化事件
            this.Q<TextField>("title-field").RegisterCallback<ChangeEvent<string>>(e => {
                note.title = e.newValue;
            });
            
            // 注册内容字段的变化事件
            this.Q<TextField>("contents-field").RegisterCallback<ChangeEvent<string>>(e => {
                note.content = e.newValue;
            });
        
            title = note.title;
            contents = note.content;
            SetPosition(note.position);
		}

		/// <summary>
		/// 设置位置
		/// 设置便签视图的位置并更新数据
		/// </summary>
		/// <param name="newPos">新的位置</param>
		public override void SetPosition(Rect newPos)
		{
			base.SetPosition(newPos);

            if (note != null)
                note.position = newPos;
		}

        /// <summary>
        /// 大小调整完成
        /// 当便签大小调整完成时更新数据
        /// </summary>
        public override void OnResized()
        {
            note.position = layout;
        }
	}
}
#endif