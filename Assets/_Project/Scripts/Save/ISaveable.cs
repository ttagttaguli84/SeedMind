// S-01: 저장/로드 대상 시스템이 구현하는 인터페이스
// -> see docs/systems/save-load-architecture.md 섹션 7
namespace SeedMind.Save
{
    /// <summary>
    /// 저장/로드 대상 시스템이 구현하는 인터페이스.
    /// SaveManager가 이 인터페이스를 통해 각 시스템과 통신한다.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>복원 순서 (낮을수록 먼저 로드).</summary>
        int SaveLoadOrder { get; }

        /// <summary>현재 상태를 직렬화 가능한 객체로 반환.</summary>
        object GetSaveData();

        /// <summary>직렬화된 데이터에서 상태를 복원.</summary>
        void LoadSaveData(object data);
    }
}
