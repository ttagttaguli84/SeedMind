// Core Singleton base class — DontDestroyOnLoad 싱글턴 패턴
// -> see docs/architecture.md 섹션 4.1
namespace SeedMind.Core
{
    using UnityEngine;

    /// <summary>
    /// DontDestroyOnLoad 싱글턴 base class.
    /// 파생 클래스는 Awake()를 override할 때 반드시 base.Awake()를 호출해야 한다.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<T>();
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}
