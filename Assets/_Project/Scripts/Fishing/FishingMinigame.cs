// FishingMinigame — 릴링 미니게임 상태 머신 (Pure C#)
// -> see docs/systems/fishing-architecture.md 섹션 2
using UnityEngine;
using SeedMind.Fishing.Data;
using SeedMind.Economy;

namespace SeedMind.Fishing
{
    /// <summary>
    /// 낚시 릴링 미니게임 로직. MonoBehaviour에 의존하지 않는 Pure C# 클래스.
    /// FishingManager가 인스턴스를 소유하며 Update를 호출한다.
    /// </summary>
    public class FishingMinigame
    {
        // --- 설정 ---
        private FishingConfig _config;
        private FishData      _currentFish;

        // --- 상태 ---
        private float _excitementGauge;     // 0.0 ~ 1.0
        private float _targetZoneCenter;    // 0.0 ~ 1.0 (게이지 위 목표 구역 중앙)
        private float _targetZoneWidth;     // 목표 구역 너비 (0.0 ~ 1.0)
        private float _elapsedTime;
        private bool  _isActive;
        private float _zoneMoveDir;         // +1 또는 -1

        // 읽기 전용 프로퍼티
        public bool    IsActive           => _isActive;
        public float   ExcitementGauge    => _excitementGauge;
        public float   TargetZoneCenter   => _targetZoneCenter;
        public float   TargetZoneWidth    => _targetZoneWidth;
        public float   ElapsedTime        => _elapsedTime;

        /// <summary>미니게임 초기화 및 시작.</summary>
        public void Start(FishingConfig config, FishData fish)
        {
            _config = config;
            _currentFish = fish;

            _excitementGauge  = 0.5f;           // 중간값에서 시작
            _targetZoneCenter = Random.Range(0.1f, 0.9f);
            _targetZoneWidth  = Mathf.Clamp(0.3f * fish.targetZoneWidthMul, 0.1f, 0.6f);
            _elapsedTime      = 0f;
            _zoneMoveDir      = Random.value > 0.5f ? 1f : -1f;
            _isActive         = true;
        }

        /// <summary>프레임마다 호출 — FishingManager.Update()에서 호출.</summary>
        public MinigameResult Update(float deltaTime)
        {
            if (!_isActive) return MinigameResult.Fail;

            _elapsedTime += deltaTime;

            // 목표 구역 이동
            MoveTargetZone(deltaTime);

            // 게이지 감소
            float decayRate = _config.excitementDecayRate;
            // 물고기 이동 속도가 높을수록 감소율 증가
            if (_currentFish != null)
                decayRate *= Mathf.Max(1f, _currentFish.moveSpeed);
            _excitementGauge -= decayRate * deltaTime;

            // 결과 판정
            return CheckCompletion();
        }

        /// <summary>플레이어 입력 처리 — 입력 키 누를 때마다 호출.</summary>
        public void ProcessInput()
        {
            if (!_isActive) return;

            // 목표 구역 내에 있으면 게이지 상승, 아니면 소량 상승
            bool inZone = IsInTargetZone();
            float gain = _config.excitementGainPerInput;
            if (!inZone) gain *= 0.5f;

            _excitementGauge = Mathf.Clamp01(_excitementGauge + gain);
        }

        /// <summary>미니게임 리셋.</summary>
        public void Reset()
        {
            _isActive         = false;
            _excitementGauge  = 0f;
            _elapsedTime      = 0f;
        }

        /// <summary>최종 낚시 품질 계산.</summary>
        public CropQuality CalculateQuality()
        {
            var thresholds = _config.qualityThresholds;
            if (thresholds == null || thresholds.Length < 4) return CropQuality.Normal;

            if (_excitementGauge >= thresholds[3]) return CropQuality.Iridium;
            if (_excitementGauge >= thresholds[2]) return CropQuality.Gold;
            if (_excitementGauge >= thresholds[1]) return CropQuality.Silver;
            return CropQuality.Normal;
        }

        // --- 내부 메서드 ---

        private void MoveTargetZone(float deltaTime)
        {
            if (_currentFish == null) return;
            float speed = _currentFish.moveSpeed * 0.05f * deltaTime;
            _targetZoneCenter += speed * _zoneMoveDir;

            float halfWidth = _targetZoneWidth * 0.5f;
            if (_targetZoneCenter - halfWidth <= 0f)
            {
                _targetZoneCenter = halfWidth;
                _zoneMoveDir = 1f;
            }
            else if (_targetZoneCenter + halfWidth >= 1f)
            {
                _targetZoneCenter = 1f - halfWidth;
                _zoneMoveDir = -1f;
            }
        }

        private bool IsInTargetZone()
        {
            float halfWidth = _targetZoneWidth * 0.5f;
            float gaugePos  = _excitementGauge;
            return gaugePos >= _targetZoneCenter - halfWidth
                && gaugePos <= _targetZoneCenter + halfWidth;
        }

        private MinigameResult CheckCompletion()
        {
            // 시간 초과
            if (_elapsedTime >= _config.reelingDuration)
            {
                _isActive = false;
                return MinigameResult.Fail;
            }

            // 게이지 상한 달성 → 성공
            if (_excitementGauge >= _config.successThreshold)
            {
                _isActive = false;
                return MinigameResult.Success;
            }

            // 게이지 하한 도달 → 실패
            if (_excitementGauge <= _config.failThreshold)
            {
                _isActive = false;
                return MinigameResult.Fail;
            }

            return MinigameResult.InProgress;
        }
    }
}
