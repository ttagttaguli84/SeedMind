// ScreenBase.cs — ARC-022(ui-tasks.md) 완전 구현 전 stub
// -> see docs/systems/ui-architecture.md 섹션 1.1
using UnityEngine;

namespace SeedMind.UI
{
    /// <summary>
    /// UIManager Screen FSM에 등록되는 모든 화면의 기반 클래스.
    /// ui-tasks.md(ARC-022)에서 완전 구현 예정.
    /// </summary>
    public abstract class ScreenBase : MonoBehaviour
    {
        public virtual void OnBeforeOpen() { }
        public virtual void OnAfterOpen() { }
        public virtual void OnBeforeClose() { }
        public virtual void OnAfterClose() { }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnBeforeOpen();
            OnAfterOpen();
        }

        public virtual void Hide()
        {
            OnBeforeClose();
            gameObject.SetActive(false);
            OnAfterClose();
        }
    }
}
