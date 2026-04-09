// S-03: 세이브 버전 불일치 예외
// -> see docs/systems/save-load-architecture.md 섹션 6.3
namespace SeedMind.Save
{
    public class SaveVersionException : System.Exception
    {
        public SaveVersionException(string message) : base(message) { }
    }
}
