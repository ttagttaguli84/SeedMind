// S-15: 튜토리얼 UI 컴포넌트
// -> see docs/systems/tutorial-architecture.md 섹션 6.2
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Tutorial;
using SeedMind.Tutorial.Data;

namespace SeedMind.UI
{
    public class TutorialUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject _dimmerPanel;
        [SerializeField] private RectTransform _highlightMask;
        [SerializeField] private GameObject _bubblePanel;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _continueButton;
        [SerializeField] private RectTransform _arrowPanel;
        [SerializeField] private Image _arrowImage;
        [SerializeField] private GameObject _popupPanel;
        [SerializeField] private TMP_Text _popupTitle;
        [SerializeField] private TMP_Text _popupBody;
        [SerializeField] private TMP_Text _stepCounterText;
        [SerializeField] private Slider _progressSlider;

        [Header("설정")]
        [SerializeField] private float _fadeInDuration = 0.2f;
        [SerializeField] private float _arrowBobSpeed = 2f;
        [SerializeField] private float _arrowBobAmplitude = 5f;

        private Camera _mainCamera;
        private TutorialStepData _activeStep;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void ShowStep(TutorialStepData step)
        {
            _activeStep = step;
            HideAll();

            switch (step.uiType)
            {
                case TutorialUIType.Bubble:
                    ShowBubble(step);
                    break;
                case TutorialUIType.Popup:
                    ShowPopup(step);
                    break;
                case TutorialUIType.Arrow:
                    ShowArrow(step);
                    break;
                case TutorialUIType.Highlight:
                    ShowHighlight(step);
                    break;
                case TutorialUIType.Combined:
                    ShowBubble(step);
                    ShowArrow(step);
                    ShowHighlight(step);
                    break;
            }

            if (step.blockOtherInput)
                ShowDimmer();

            UpdateProgress();
        }

        public void HideStep()
        {
            HideAll();
            _activeStep = null;
        }

        private void ShowBubble(TutorialStepData step)
        {
            if (_bubblePanel == null) return;
            _bubblePanel.SetActive(true);

            if (_messageText != null) _messageText.text = step.messageText;
            if (_iconImage != null)
            {
                _iconImage.sprite = step.iconOverride;
                _iconImage.gameObject.SetActive(step.iconOverride != null);
            }

            var rt = _bubblePanel.GetComponent<RectTransform>();
            if (rt != null) PositionToAnchor(rt, step);

            if (_continueButton != null)
                _continueButton.gameObject.SetActive(
                    step.completionType == StepCompletionType.ClickToContinue);
        }

        private void ShowArrow(TutorialStepData step)
        {
            if (_arrowPanel == null || !step.showArrow) return;
            _arrowPanel.gameObject.SetActive(true);
            PositionToAnchor(_arrowPanel, step);
            _arrowPanel.anchoredPosition += step.arrowOffset;
        }

        private void ShowHighlight(TutorialStepData step)
        {
            if (_highlightMask == null || !step.showHighlight) return;
            _highlightMask.gameObject.SetActive(true);
            PositionToAnchor(_highlightMask, step);
            _highlightMask.sizeDelta = Vector2.one * step.highlightRadius * 2f;
        }

        private void ShowDimmer()
        {
            if (_dimmerPanel != null)
                _dimmerPanel.SetActive(true);
        }

        private void ShowPopup(TutorialStepData step)
        {
            if (_popupPanel == null) return;
            _popupPanel.SetActive(true);
            if (_popupBody != null) _popupBody.text = step.messageText;
        }

        private void HideAll()
        {
            if (_dimmerPanel != null) _dimmerPanel.SetActive(false);
            if (_highlightMask != null) _highlightMask.gameObject.SetActive(false);
            if (_bubblePanel != null) _bubblePanel.SetActive(false);
            if (_arrowPanel != null) _arrowPanel.gameObject.SetActive(false);
            if (_popupPanel != null) _popupPanel.SetActive(false);
        }

        private void PositionToAnchor(RectTransform target, TutorialStepData step)
        {
            if (target == null) return;

            switch (step.anchorType)
            {
                case TutorialAnchorType.WorldTarget:
                    var go = GameObject.Find(step.anchorTargetId);
                    if (go != null && _mainCamera != null)
                    {
                        Vector2 screenPos = _mainCamera.WorldToScreenPoint(go.transform.position);
                        target.position = screenPos;
                    }
                    break;
                case TutorialAnchorType.ScreenPosition:
                    // anchorTargetId를 "x,y" 정규화 좌표로 파싱
                    break;
                case TutorialAnchorType.UIElement:
                    // anchorTargetId로 UI 요소 탐색
                    break;
            }
        }

        private void UpdateProgress()
        {
            var manager = TutorialManager.Instance;
            if (manager == null) return;
            // stepCounterText, progressSlider 업데이트 (향후 구현)
        }

        private void Update()
        {
            if (_arrowPanel != null && _arrowPanel.gameObject.activeSelf)
            {
                float bob = Mathf.Sin(Time.time * _arrowBobSpeed) * _arrowBobAmplitude;
                // 흔들림은 향후 적용
            }
        }

        // 클릭 진행 버튼 핸들러 (Button OnClick에 연결)
        public void OnContinueClicked()
        {
            var manager = TutorialManager.Instance;
            if (manager != null)
                manager.OnStepCompleted();
        }
    }
}
