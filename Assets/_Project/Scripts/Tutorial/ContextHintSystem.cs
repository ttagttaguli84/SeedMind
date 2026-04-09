// S-14: 상황별 자동 힌트 시스템
// -> see docs/systems/tutorial-architecture.md 섹션 8.3
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Tutorial.Data;
using SeedMind.UI;

namespace SeedMind.Tutorial
{
    public class ContextHintSystem : MonoBehaviour
    {
        [SerializeField] private ContextHintData[] _allHints;
        [SerializeField] private TutorialUI _tutorialUI;

        private Dictionary<string, int> _cooldowns = new Dictionary<string, int>();
        private Dictionary<string, int> _showCounts = new Dictionary<string, int>();
        private float _checkInterval = 10f;
        private float _timer;

        private void OnEnable()
        {
            Farm.FarmEvents.OnCropHarvested += _ => DecrementCooldowns();
        }

        private void OnDisable()
        {
            Farm.FarmEvents.OnCropHarvested -= _ => DecrementCooldowns();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < _checkInterval) return;
            _timer = 0f;

            if (TutorialManager.Instance != null &&
                TutorialManager.Instance.State != TutorialState.Idle) return;

            EvaluateHints();
        }

        private void EvaluateHints()
        {
            if (_allHints == null) return;

            ContextHintData bestHint = null;
            int bestPriority = int.MinValue;

            foreach (var hint in _allHints)
            {
                if (hint == null) continue;
                if (!IsHintAvailable(hint)) continue;
                if (!EvaluateCondition(hint)) continue;
                if (hint.priority > bestPriority)
                {
                    bestHint = hint;
                    bestPriority = hint.priority;
                }
            }

            if (bestHint != null)
                ShowHint(bestHint);
        }

        private bool IsHintAvailable(ContextHintData hint)
        {
            if (_cooldowns.TryGetValue(hint.hintId, out int remaining) && remaining > 0)
                return false;

            if (hint.maxShowCount > 0)
            {
                _showCounts.TryGetValue(hint.hintId, out int count);
                if (count >= hint.maxShowCount) return false;
            }

            if (hint.requireTutorialComplete && TutorialManager.Instance != null &&
                !TutorialManager.Instance.IsSequenceCompleted("SEQ_MainTutorial"))
                return false;

            return true;
        }

        private bool EvaluateCondition(ContextHintData hint)
        {
            // 런타임 조건 평가 — 각 시스템 연동은 Phase E 이후 처리
            return false;
        }

        private void ShowHint(ContextHintData hint)
        {
            _cooldowns[hint.hintId] = hint.cooldownDays;
            _showCounts.TryGetValue(hint.hintId, out int count);
            _showCounts[hint.hintId] = count + 1;

            TutorialEvents.OnContextHintShown?.Invoke(hint.hintId);
        }

        private void DecrementCooldowns()
        {
            var keys = new List<string>(_cooldowns.Keys);
            foreach (var key in keys)
            {
                _cooldowns[key] = Mathf.Max(0, _cooldowns[key] - 1);
            }
        }

        // --- 세이브/로드 ---

        public Dictionary<string, int> ExportCooldowns()
        {
            return new Dictionary<string, int>(_cooldowns);
        }

        public void ImportCooldowns(Dictionary<string, int> data)
        {
            if (data == null) return;
            _cooldowns = new Dictionary<string, int>(data);
        }
    }
}
