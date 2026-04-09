// S-11: 업적 목록 패널 UI
// -> see docs/systems/achievement-architecture.md 섹션 8.1
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Achievement;
using SeedMind.Achievement.Data;

namespace SeedMind.UI
{
    public class AchievementPanel : MonoBehaviour
    {
        [SerializeField] private Transform _contentParent;
        [SerializeField] private AchievementItemUI _itemPrefab;
        [SerializeField] private Button[] _categoryTabs;
        [SerializeField] private TMP_Text _progressText;

        private bool _isOpen;
        private AchievementCategory _currentCategory;

        public void Toggle()
        {
            if (_isOpen) Close(); else Open();
        }

        public void Open()
        {
            _isOpen = true;
            gameObject.SetActive(true);
            RefreshList();
            UpdateProgressText();
        }

        public void Close()
        {
            _isOpen = false;
            gameObject.SetActive(false);
        }

        public void SetCategory(AchievementCategory category)
        {
            _currentCategory = category;
            RefreshList();
        }

        private void RefreshList()
        {
            // 기존 항목 정리, 카테고리 필터링, AchievementItemUI 인스턴스 생성
            Debug.Log($"[AchievementPanel] Refreshing for category: {_currentCategory}");
        }

        private void UpdateProgressText()
        {
            var manager = AchievementManager.Instance;
            if (manager == null) return;
            int total = manager.GetUnlockedAchievements().Count;
            // 전체 업적 수 -> see docs/systems/achievement-system.md 섹션 1
            _progressText.text = $"{total} 달성";
        }
    }
}
