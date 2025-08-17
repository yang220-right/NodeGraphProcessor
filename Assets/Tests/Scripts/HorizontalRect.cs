using ScrollRect;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    public class HorizontalRect : CustomLoopScrollRectBase
    {
        protected override void Start(){
            base.Start();
            _data = new ScrollInitData(){
                beginIndex = 20,
                TotalNum = 50,
                OnUpdateData = (i, j) => {
                    return new object[]{ i , 50};
                }
            };
            Init(_data);
        }

        protected HorizontalRect()
        {
            direction = LoopScrollRectDirection.Horizontal;
        }
        protected override float GetSize(RectTransform item, bool includeSpacing)
        {
            float size = includeSpacing ? contentSpacing : 0;
            if (m_GridLayout != null)
            {
                size += m_GridLayout.cellSize.x;
            }
            else
            {
                size += LoopScrollSizeUtils.GetPreferredWidth(item);
            }
            size *= m_Content.localScale.x;
            return size;
        }
        protected override float GetDimension(Vector2 vector)
        {
            return -vector.x;
        }
        protected override float GetAbsDimension(Vector2 vector)
        {
            return vector.x;
        }
        protected override Vector2 GetVector(float value)
        {
            return new Vector2(-value, 0);
        }
        protected override bool UpdateItems(ref Bounds viewBounds, ref Bounds contentBounds)
        {
            bool changed = false;

            // 特殊情况：处理在一个框架中移动多页的情况
            if ((viewBounds.size.x < contentBounds.min.x - viewBounds.max.x) && itemTypeEnd > itemTypeStart)
            {
                float currentSize = contentBounds.size.x;
                float elementSize = EstimiateElementSize();
                ReturnToTempPool(false, itemTypeEnd - itemTypeStart);
                itemTypeEnd = itemTypeStart;

                int offsetCount = Mathf.FloorToInt((contentBounds.min.x - viewBounds.max.x) / (elementSize + contentSpacing));
                if (totalCount >= 0 && itemTypeStart - offsetCount * contentConstraintCount < 0)
                {
                    offsetCount = Mathf.FloorToInt((float)(itemTypeStart) / contentConstraintCount);
                }
                itemTypeStart -= offsetCount * contentConstraintCount;
                if (totalCount >= 0)
                {
                    itemTypeStart = Mathf.Max(itemTypeStart, 0);
                }
                itemTypeEnd = itemTypeStart;
                itemTypeSize = 0;

                float offset = offsetCount * (elementSize + contentSpacing);
                m_Content.anchoredPosition -= new Vector2(offset + (reverseDirection ? currentSize : 0), 0);
                contentBounds.center -= new Vector3(offset + currentSize / 2, 0, 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }

            if ((viewBounds.min.x - contentBounds.max.x > viewBounds.size.x)  && itemTypeEnd > itemTypeStart)
            {
                int maxItemTypeStart = -1;
                if (totalCount >= 0)
                {
                    maxItemTypeStart = Mathf.Max(0, totalCount - (itemTypeEnd - itemTypeStart));
                    maxItemTypeStart = (maxItemTypeStart / contentConstraintCount) * contentConstraintCount;
                }
                float currentSize = contentBounds.size.x;
                float elementSize = EstimiateElementSize();
                ReturnToTempPool(true, itemTypeEnd - itemTypeStart);
                // TODO: fix with contentConstraint?
                itemTypeStart = itemTypeEnd;
            
                int offsetCount = Mathf.FloorToInt((viewBounds.min.x - contentBounds.max.x) / (elementSize + contentSpacing));
                if (maxItemTypeStart >= 0 && itemTypeStart + offsetCount * contentConstraintCount > maxItemTypeStart)
                {
                    offsetCount = Mathf.FloorToInt((float)(maxItemTypeStart - itemTypeStart) / contentConstraintCount);
                }
                itemTypeStart += offsetCount * contentConstraintCount;
                if (totalCount >= 0)
                {
                    itemTypeStart = Mathf.Max(itemTypeStart, 0);
                }
                itemTypeEnd = itemTypeStart;
                itemTypeSize = 0;

                float offset = offsetCount * (elementSize + contentSpacing);
                m_Content.anchoredPosition += new Vector2(offset + (reverseDirection ? 0 : currentSize), 0);
                contentBounds.center += new Vector3(offset + currentSize / 2, 0, 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }

            if (viewBounds.max.x > contentBounds.max.x - m_ContentRightPadding)
            {
                float size = NewItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.max.x > contentBounds.max.x - m_ContentRightPadding + totalSize)
                {
                    size = NewItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }
            else if ((itemTypeEnd % contentConstraintCount != 0) && (itemTypeEnd < totalCount || totalCount < 0))
            {
                NewItemAtEnd();
            }

            if (viewBounds.min.x < contentBounds.min.x + m_ContentLeftPadding)
            {
                float size = NewItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.min.x < contentBounds.min.x + m_ContentLeftPadding - totalSize)
                {
                    size = NewItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.max.x < contentBounds.max.x - threshold - m_ContentRightPadding
                && viewBounds.size.x < contentBounds.size.x - threshold)
            {
                float size = DeleteItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.max.x < contentBounds.max.x - threshold - m_ContentRightPadding - totalSize)
                {
                    size = DeleteItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.min.x > contentBounds.min.x + threshold + m_ContentLeftPadding
                && viewBounds.size.x < contentBounds.size.x - threshold)
            {
                float size = DeleteItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.min.x > contentBounds.min.x + threshold + m_ContentLeftPadding + totalSize)
                {
                    size = DeleteItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (changed)
            {
                ClearTempPool();
            }

            return changed;
        }
        protected override RectTransform GetFromTempPool(int itemIdx)
        {
            RectTransform nextItem = null;
            if (deletedItemTypeStart > 0)
            {
                deletedItemTypeStart--;
                nextItem = m_Content.GetChild(0) as RectTransform;
                nextItem.SetSiblingIndex(itemIdx - itemTypeStart + deletedItemTypeStart);
            }
            else if (deletedItemTypeEnd > 0)
            {
                deletedItemTypeEnd--;
                nextItem = m_Content.GetChild(m_Content.childCount - 1) as RectTransform;
                nextItem.SetSiblingIndex(itemIdx - itemTypeStart + deletedItemTypeStart);
            }
            else
            {
                nextItem = GetObject().transform as RectTransform;
                nextItem.transform.SetParent(m_Content, false);
                nextItem.gameObject.SetActive(true);
            }

            if (!nextItem.TryGetComponent<BaseUpdateData>(out var data)){
                Debug.Log("添加BaseUpdateData!!!");
            }
            ProvideData(data, itemIdx);
            return nextItem;
        }
        protected override void ReturnToTempPool(bool fromStart, int count)
        {
            if (fromStart)
                deletedItemTypeStart += count;
            else
                deletedItemTypeEnd += count;
        }
        protected override void ClearTempPool()
        {
            Debug.Assert(m_Content.childCount >= deletedItemTypeStart + deletedItemTypeEnd);
            if (deletedItemTypeStart > 0)
            {
                for (int i = deletedItemTypeStart - 1; i >= 0; i--)
                {
                    ReturnObject(m_Content.GetChild(i));
                }
                deletedItemTypeStart = 0;
            }
            if (deletedItemTypeEnd > 0)
            {
                int t = m_Content.childCount - deletedItemTypeEnd;
                for (int i = m_Content.childCount - 1; i >= t; i--)
                {
                    ReturnObject(m_Content.GetChild(i));
                }
                deletedItemTypeEnd = 0;
            }
        }
        public override void OnBeginDrag(PointerEventData eventData){
            StopAllCoroutines();
            base.OnBeginDrag(eventData);
        }
        private void Update(){
            if (Input.GetKeyDown(KeyCode.A)){
                ScrollToCellWithinTime(Random.Range(0, totalCount), 1);
            }

            if (Input.GetKeyDown(KeyCode.S)){
                ScrollToCellWithinTime(itemTypeStart + (itemTypeEnd - itemTypeStart)/2 , 0.3f,0,ScrollMode.ToCenter);
                Debug.Log(CurrentLines);
            }
            if (Input.GetKeyDown(KeyCode.D)){
                SetLoop(totalCount != -1);
            }
        }
    }
}