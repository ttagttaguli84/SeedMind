# UnityMCP 로컬 수정 내역

원본: https://github.com/CoplayDev/unity-mcp  
버전: 9.6.5 (commit `d6e58c68cf4b`)  
전환: git UPM 패키지 → 로컬 패키지 (`Packages/com.coplaydev.unity-mcp/`)

---

## [1] wait_for_log 독립 툴 추가

**파일**: `Editor/Tools/WaitForLog.cs` (신규)

### 문제
테스트 실행 후 `[TEST_SUMMARY]` 로그를 기다리기 위해 2초마다 `read_console`을 반복 폴링해야 했음.

### 변경사항
`AutoRegister = true` 독립 툴로 구현 → Python 서버 스키마 수정/재시작 없이 자동 등록.

- `Application.logMessageReceivedThreaded` 콜백으로 Unity 로그 실시간 감지
- HTTP 핸들러 스레드에서 `ManualResetEventSlim.Wait(timeout_ms)` 블로킹
- 패턴 매칭 시 즉시 반환, 타임아웃 시 에러 반환

**사용 예**:
```
wait_for_log(pattern="[TEST_SUMMARY]", timeout_ms=120000)
```

---

## [2] manage_editor get_state 액션 추가

**파일**: `Editor/Tools/ManageEditor.cs`

`case "get_state":` 분기 추가. 반환값:
- `isPlaying`, `isPaused`, `isCompiling`, `isUpdating`
- `timeSinceStartup`
- `activeScene`, `activeScenePath`, `activeSceneDirty`

---

## [3] manage_scene 풀 경로 전달 시 path 중복 버그 수정

**파일**: `Editor/Tools/ManageScene.cs`

`name` 파라미터에 풀 경로(`Assets/Scenes/MainScene.unity`)를 넘기면 `path`가 이중으로 붙던 버그 수정.  
`name`에 `/` 또는 `.unity`가 포함되면 자동으로 `path`/`name`으로 분리 정규화.

---

## [4] execute_code TargetInvocationException 에러 메시지 개선

**파일**: `Editor/Tools/ExecuteCode.cs`

Roslyn 컴파일 코드 실행 시 `TargetInvocationException`이 발생하면 원인 예외(`InnerException`)를 unwrap하여 실제 에러 메시지를 반환하도록 수정.

---

## [5] 서버 Start/Stop/Restart 메뉴 추가

**파일**: `Editor/MenuItems/ServerControlMenuItems.cs` (신규)

Unity 메뉴에 서버 제어 항목 추가:
- `MCP For Unity/Server/Start Server`
- `MCP For Unity/Server/Stop Server`
- `MCP For Unity/Server/Restart Server`
- `MCP For Unity/Server/Server Status`

---

## [6] 서버 시작 시 콘솔 창 2개 뜨는 문제 수정

**파일**: `Editor/Services/Server/TerminalLauncher.cs`

**이전**: `cmd.exe /c start "MCP Server" cmd.exe /k "{script}"` → 런처 cmd(즉시 종료) + 서버 cmd(유지) = 창 2개

**이후**: `cmd.exe /k "{script}"` 직접 실행 (`UseShellExecute = true`) → 창 1개

---

## [7] CS0618 Obsolete 경고 전부 제거

**대상**: 패키지 내 30개 파일

| 원인 | 수정 방법 |
|------|-----------|
| `GetInstanceID()` deprecated (Unity 6) | 영향 파일 전체에 `#pragma warning disable CS0618` 추가 |
| `FindObjectsSortMode` deprecated | `FindObjectsByType<T>(FindObjectsInactive.Exclude)` 로 교체 |
| `FindFirstObjectByType` deprecated | `FindAnyObjectByType` 으로 교체 |
| `Physics2D.autoSyncTransforms` deprecated | `#pragma warning disable CS0618` 추가 |
| `EntityId.implicit operator EntityId(int)` deprecated | `ResolveInstanceID`에서 `EntityIdToObject` → `InstanceIDToObject` 로 교체 |

---

## Python 서버 측 수정

**경로**: `C:\Users\kntop\AppData\Local\uv\cache\archive-v0\HVWJ4PVecfJcvLBA3nSWV\Lib\site-packages\services\tools\`

| 파일 | 변경 내용 |
|------|-----------|
| `manage_editor.py` | `action` 타입: `Literal[...]` → `str` |
| `manage_scene.py` | `action` 타입: `Literal[...]` → `str` |

`Literal` 검증을 `str`로 변경하여 C#에 새 액션을 추가해도 Python 서버 재시작 불필요.

> **주의**: uv 캐시 경로(`HVWJ4PVecfJcvLBA3nSWV`)는 서버 업데이트 시 바뀔 수 있음.
