using System.Collections;
using UnityEngine;
using TMPro;

namespace SeedMind.Collection.UI
{
    /// <summary>
    /// 채집물 최초 발견 토스트 알림 UI.
    /// GatheringCatalogManager.OnItemDiscovered를 구독하여 화면 상단에 토스트 표시.
    /// -> see docs/systems/collection-architecture.md 섹션 6.4
    /// </summary>
    public class GatheringCatalogToastUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private TMP_Text _itemNameText;
        [SerializeField] private float _displayDuration = 2.5f;
        [SerializeField] private float _fadeDuration = 0.4f;

        private CanvasGroup _canvasGroup;
        private Coroutine _toastCoroutine;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            GatheringCatalogManager.OnItemDiscovered += ShowToast;
        }

        private void OnDisable()
        {
            GatheringCatalogManager.OnItemDiscovered -= ShowToast;
        }

        private void ShowToast(string itemId)
        {
            var data = GatheringCatalogManager.Instance?.GetCatalogData(itemId);
            string displayName = data != null ? data.name : itemId;

            if (_messageText != null)
                _messageText.text = "새로운 채집물 발견!";
            if (_itemNameText != null)
                _itemNameText.text = displayName;

            if (_toastCoroutine != null)
                StopCoroutine(_toastCoroutine);
            _toastCoroutine = StartCoroutine(ToastRoutine());
        }

        private IEnumerator ToastRoutine()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0f;

            // Fade in
            float t = 0f;
            while (t < _fadeDuration)
            {
                _canvasGroup.alpha = t / _fadeDuration;
                t += Time.deltaTime;
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(_displayDuration);

            // Fade out
            t = 0f;
            while (t < _fadeDuration)
            {
                _canvasGroup.alpha = 1f - (t / _fadeDuration);
                t += Time.deltaTime;
                yield return null;
            }
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}
