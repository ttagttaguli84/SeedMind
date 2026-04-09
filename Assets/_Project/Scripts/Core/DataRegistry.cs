using System.Collections.Generic;
using UnityEngine;

namespace SeedMind.Core
{
    /// <summary>
    /// 모든 GameDataSO를 dataId로 검색하는 런타임 레지스트리.
    /// Resources/Data/ 하위의 SO를 자동 스캔하여 딕셔너리에 등록.
    /// -> see docs/pipeline/data-pipeline.md Part II 섹션 1.3
    /// -> see docs/systems/inventory-architecture.md 섹션 3.1
    ///
    /// 현재 단계(Phase C): 기본 구조 생성.
    /// 완전 구현은 inventory-tasks.md (ARC-013) Phase E에서 진행.
    /// </summary>
    public class DataRegistry : Singleton<DataRegistry>
    {
        private Dictionary<string, GameDataSO> _registry;

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        /// <summary>
        /// Resources/Data/ 하위의 모든 GameDataSO를 스캔하여 등록.
        /// SO는 Assets/_Project/Resources/Data/ 하위에 위치해야 한다.
        /// (현재 단계에서는 Assets/_Project/Data/에 위치 — inventory-tasks에서 이전 예정)
        /// </summary>
        public void Initialize()
        {
            _registry = new Dictionary<string, GameDataSO>();
            var allData = Resources.LoadAll<GameDataSO>("Data");
            foreach (var so in allData)
            {
                if (string.IsNullOrEmpty(so.dataId))
                {
                    Debug.LogWarning($"[DataRegistry] dataId가 비어있는 SO 발견: {so.name}");
                    continue;
                }
                if (_registry.ContainsKey(so.dataId))
                {
                    Debug.LogWarning($"[DataRegistry] 중복 dataId: '{so.dataId}' — {so.name}이(가) 무시됩니다.");
                    continue;
                }
                _registry[so.dataId] = so;
            }
            Debug.Log($"[DataRegistry] 초기화 완료: {_registry.Count}개 SO 등록.");
        }

        /// <summary>
        /// dataId로 GameDataSO를 조회한다.
        /// </summary>
        public T Get<T>(string dataId) where T : GameDataSO
        {
            if (_registry == null) Initialize();
            if (_registry.TryGetValue(dataId, out var so))
                return so as T;
            Debug.LogWarning($"[DataRegistry] '{dataId}'를 찾을 수 없습니다.");
            return null;
        }

        /// <summary>
        /// T 타입의 모든 SO를 반환한다.
        /// </summary>
        public List<T> GetAll<T>() where T : GameDataSO
        {
            if (_registry == null) Initialize();
            var result = new List<T>();
            foreach (var so in _registry.Values)
            {
                if (so is T typed)
                    result.Add(typed);
            }
            return result;
        }

        /// <summary>
        /// 런타임에 SO를 직접 등록한다 (에디터 테스트 등 용도).
        /// </summary>
        public void Register(GameDataSO so)
        {
            if (_registry == null) _registry = new Dictionary<string, GameDataSO>();
            if (!string.IsNullOrEmpty(so.dataId))
                _registry[so.dataId] = so;
        }
    }
}
