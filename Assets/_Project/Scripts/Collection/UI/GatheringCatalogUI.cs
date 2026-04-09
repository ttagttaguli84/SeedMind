using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SeedMind.Gathering;

namespace SeedMind.Collection.UI
{
    /// <summary>
    /// 채집 도감 패널. 아이템 목록 표시, 카테고리 필터, 상세 패널 연동.
    /// -> see docs/systems/collection-architecture.md 섹션 6.3
    /// </summary>
    public class GatheringCatalogUI : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GatheringCatalogManager _catalogManager;

        [Header("UI 참조")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _contentParent;
        [SerializeField] private GatheringCatalogItemUI _itemPrefab;
        [SerializeField] private GatheringCatalogDetailPanel _detailPanel;

        private GatheringCategory? _categoryFilter = null;
        private readonly List<GatheringCatalogItemUI> _itemPool = new();

        private void Start()
        {
            if (_catalogManager == null)
                _catalogManager = GatheringCatalogManager.Instance;
        }

        public void Refresh()
        {
            if (_catalogManager == null || _contentParent == null || _itemPrefab == null) return;

            // 풀 재사용
            int idx = 0;
            foreach (var data in GetFilteredData())
            {
                GatheringCatalogItemUI item;
                if (idx < _itemPool.Count)
                {
                    item = _itemPool[idx];
                    item.gameObject.SetActive(true);
                }
                else
                {
                    item = Instantiate(_itemPrefab, _contentParent);
                    _itemPool.Add(item);
                }

                var entry = _catalogManager.GetEntry(data.itemId);
                item.SetData(data, entry, this);
                idx++;
            }

            // 초과 풀 항목 비활성화
            for (int i = idx; i < _itemPool.Count; i++)
                _itemPool[i].gameObject.SetActive(false);
        }

        public void SetCategoryFilter(GatheringCategory? category)
        {
            _categoryFilter = category;
            Refresh();
        }

        public void SelectItem(string itemId)
        {
            if (_detailPanel == null) return;
            var data = _catalogManager?.GetCatalogData(itemId);
            var entry = _catalogManager?.GetEntry(itemId);
            if (data != null)
                _detailPanel.ShowItem(data, entry);
        }

        private IEnumerable<GatheringCatalogData> GetFilteredData()
        {
            if (_catalogManager == null) yield break;

            // Resources에서 전체 로드 (sortOrder 기준 정렬)
            var allData = Resources.LoadAll<GatheringCatalogData>("Data/GatheringCatalog");
            System.Array.Sort(allData, (a, b) => a.sortOrder.CompareTo(b.sortOrder));

            foreach (var data in allData)
            {
                if (_categoryFilter == null) { yield return data; continue; }

                // GatheringItemData에서 카테고리 확인
                var itemData = Resources.Load<SeedMind.Gathering.GatheringItemData>($"Data/{data.itemId}");
                if (itemData != null && itemData.gatheringCategory == _categoryFilter.Value)
                    yield return data;
                else if (itemData == null)
                    yield return data; // 필터 기준 불명확 시 표시
            }
        }
    }
}
