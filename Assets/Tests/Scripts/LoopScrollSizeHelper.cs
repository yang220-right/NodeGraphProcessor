using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
  // 用于更好滚动支持的可选类
  public interface LoopScrollSizeHelper
  {
    Vector2 GetItemsSize(int itemsCount);
  }
}