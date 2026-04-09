// S-12: 튜토리얼 매니저 (Singleton)
// -> see docs/systems/tutorial-architecture.md 섹션 3.2
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Tutorial.Data;
using SeedMind.UI;

namespace SeedMind.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("설정")]
        [SerializeField] private TutorialSequenceData[] _allSequences;
        [SerializeField] private TutorialUI _tutorialUI;

        // 런타임 상태
        private TutorialState _state = TutorialState.Idle;
        private TutorialSequenceData _activeSequence;
        private int _currentStepIndex;
        private HashSet<string> _completedSequences = new HashSet<string>();
        private HashSet<string> _completedSteps = new HashSet<string>();

        public TutorialState State => _state;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // --- 시퀀스 제어 ---

        public bool TryStartSequence(string sequenceId)
        {
            if (_state != TutorialState.Idle) return false;
            if (_completedSequences.Contains(sequenceId)) return false;

            var seq = FindSequence(sequenceId);
            if (seq == null) return false;

            _activeSequence = seq;
            _currentStepIndex = 0;
            _state = TutorialState.RunningSequence;

            TutorialEvents.OnSequenceStarted?.Invoke(sequenceId);
            AdvanceToStep(0);
            return true;
        }

        public bool TryStartSequenceByTrigger(TutorialTriggerType triggerType, string param)
        {
            if (_allSequences == null) return false;
            foreach (var seq in _allSequences)
            {
                if (seq.startTriggerType != triggerType) continue;
                if (!string.IsNullOrEmpty(seq.startTriggerParam) && seq.startTriggerParam != param) continue;
                if (TryStartSequence(seq.sequenceId)) return true;
            }
            return false;
        }

        public void AdvanceToStep(int stepIndex)
        {
            if (_activeSequence == null) return;
            if (stepIndex >= _activeSequence.steps.Length)
            {
                CompleteSequence();
                return;
            }

            _currentStepIndex = stepIndex;
            var step = _activeSequence.steps[stepIndex];

            if (_tutorialUI != null)
                _tutorialUI.ShowStep(step);

            TutorialEvents.OnStepStarted?.Invoke(_activeSequence.sequenceId, step.stepId);
        }

        public void OnStepCompleted()
        {
            if (_activeSequence == null) return;
            var step = _activeSequence.steps[_currentStepIndex];
            _completedSteps.Add(step.stepId);
            TutorialEvents.OnStepCompleted?.Invoke(_activeSequence.sequenceId, step.stepId);

            if (_tutorialUI != null)
                _tutorialUI.HideStep();

            AdvanceToStep(_currentStepIndex + 1);
        }

        public void SkipSequence()
        {
            if (_activeSequence == null) return;
            string seqId = _activeSequence.sequenceId;
            _completedSequences.Add(seqId);

            if (_tutorialUI != null)
                _tutorialUI.HideStep();

            _activeSequence = null;
            _state = TutorialState.Idle;
            TutorialEvents.OnSequenceSkipped?.Invoke(seqId);
        }

        public TutorialStepData GetActiveStep()
        {
            if (_activeSequence == null || _state != TutorialState.RunningSequence) return null;
            if (_currentStepIndex >= _activeSequence.steps.Length) return null;
            return _activeSequence.steps[_currentStepIndex];
        }

        public bool IsSequenceCompleted(string sequenceId)
        {
            return _completedSequences.Contains(sequenceId);
        }

        // --- 세이브/로드 ---

        public TutorialSaveData ExportSaveData()
        {
            return new TutorialSaveData
            {
                completedSequenceIds = new List<string>(_completedSequences),
                completedStepIds = new List<string>(_completedSteps),
                activeSequenceId = _activeSequence?.sequenceId ?? "",
                activeStepIndex = _currentStepIndex,
                contextHintCooldowns = GetComponent<ContextHintSystem>()?.ExportCooldowns()
                    ?? new Dictionary<string, int>()
            };
        }

        public void ImportSaveData(TutorialSaveData data)
        {
            if (data == null) return;
            _completedSequences = new HashSet<string>(data.completedSequenceIds ?? new List<string>());
            _completedSteps = new HashSet<string>(data.completedStepIds ?? new List<string>());

            if (!string.IsNullOrEmpty(data.activeSequenceId))
            {
                var seq = FindSequence(data.activeSequenceId);
                if (seq != null)
                {
                    _activeSequence = seq;
                    _currentStepIndex = data.activeStepIndex;
                    _state = TutorialState.RunningSequence;
                    AdvanceToStep(_currentStepIndex);
                }
            }

            if (data.contextHintCooldowns != null)
                GetComponent<ContextHintSystem>()?.ImportCooldowns(data.contextHintCooldowns);
        }

        private void CompleteSequence()
        {
            string seqId = _activeSequence.sequenceId;
            _completedSequences.Add(seqId);
            _activeSequence = null;
            _state = TutorialState.Idle;
            TutorialEvents.OnSequenceCompleted?.Invoke(seqId);

            // 모든 시퀀스 완료 확인
            if (_allSequences != null)
            {
                bool allDone = true;
                foreach (var seq in _allSequences)
                {
                    if (!_completedSequences.Contains(seq.sequenceId))
                    {
                        allDone = false;
                        break;
                    }
                }
                if (allDone)
                    TutorialEvents.OnAllTutorialsCompleted?.Invoke();
            }
        }

        private TutorialSequenceData FindSequence(string id)
        {
            if (_allSequences == null) return null;
            foreach (var seq in _allSequences)
                if (seq != null && seq.sequenceId == id) return seq;
            return null;
        }
    }

    public enum TutorialState
    {
        Idle,
        RunningSequence
    }
}
