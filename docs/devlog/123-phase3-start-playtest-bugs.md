# 데브로그 123 — Phase 3 시작: 플레이 테스트 첫 세션 & 버그 6건 수정

**날짜**: 2026-04-10  
**저자**: Claude Code  
**관련**: Phase 2→3 전환, QA-001~004

---

## 1. Phase 2 완료 확인

`docs/mcp/progress.md` 최종 점검 결과 모든 Phase A–G ✅ 확인:

| Phase | 내용 | 상태 |
|-------|------|------|
| A | Foundation (씬/기본환경) | ✅ |
| B | Core Systems (저장/시간/진행) | ✅ |
| C | Content (작물/시설/인벤토리) | ✅ |
| D | Feature Systems (도구/NPC/퀘스트/튜토리얼) | ✅ |
| E | UI & UX (UIManager/사운드) | ✅ |
| F | Advanced Features (농장확장/낚시/채집/축산) | ✅ |
| G | Polish (컬렉션/데코) | ✅ |

Phase 2 → Phase 3 공식 전환 완료.

---

## 2. Phase 3 플레이 테스트 착수 — 첫 세션

Unity Editor에서 Play Mode 진입 후 SCN_MainMenu 로딩 시 즉시 오류 다수 발견.  
아래 6건을 당일 모두 수정 완료.

---

## 3. 수정된 버그 목록

### BUG-001 — InputActions UUID 오류 (우선순위: 긴급)

**증상**: 키보드·마우스 입력 전혀 동작하지 않음.  
**원인**: `InputActions` 에셋의 actionMap/action id 필드가 UUID 형식이 아닌 단순 문자열로 기록되어 있었음. New Input System은 런타임에 UUID로 액션을 참조하므로 입력 자체가 무효.  
**수정**: InputActions 에셋 id 필드를 올바른 UUID 형식으로 재생성.

---

### BUG-002 — SoundManager DontDestroyOnLoad 경고

**증상**: Play Mode 진입 시 Console에 `DontDestroyOnLoad only works for root GameObjects` 경고 반복 출력.  
**원인**: SoundManager가 씬 계층 내부(자식) GO로 배치되어 있어 DontDestroyOnLoad 호출 불가.  
**수정**: SoundManager GO를 씬 루트로 이동 후 DontDestroyOnLoad 정상 적용.

---

### BUG-003 — 한국어 TMP 폰트 누락 → □□□□ 표시

**증상**: UI 텍스트 전체가 빈 사각형(□)으로 표시됨.  
**원인**: TextMeshPro 기본 폰트 에셋에 한국어 글리프 미포함.  
**수정**: 한국어 지원 TMP 폰트 에셋 임포트 및 Canvas 전체 TMP 컴포넌트에 폰트 할당.

---

### BUG-004 — SCN_MainMenu UI 컴포넌트 없음

**증상**: SCN_MainMenu 진입 시 빈 화면 — 버튼, 타이틀 텍스트 등 UI 요소 없음.  
**원인**: scene-setup-tasks.md 실행 시 SCN_MainMenu에 Canvas와 UI 계층이 생성되지 않았음 (Transform 오브젝트만 존재).  
**수정**: SCN_MainMenu에 Canvas(Screen Space - Overlay), EventSystem, 타이틀 텍스트, New Game / Continue / Quit 버튼 UI 계층 생성.

---

### BUG-005 — EventSystem StandaloneInputModule → InputSystemUIInputModule 교체

**증상**: New Input System 환경에서 버튼 클릭 인식 불가.  
**원인**: scene-setup-tasks.md가 생성한 EventSystem에 구 InputSystem의 `StandaloneInputModule`이 붙어 있었음. 프로젝트는 New Input System 전용 설정.  
**수정**: StandaloneInputModule 제거 → `InputSystemUIInputModule` 추가.

---

### BUG-006 — MainMenuController 없어 버튼 클릭 시 씬 전환 안 됨

**증상**: New Game 버튼 클릭 시 아무 반응 없음.  
**원인**: SCN_MainMenu에 버튼 OnClick 이벤트를 처리할 `MainMenuController` 컴포넌트가 존재하지 않았음.  
**수정**: `MainMenuController.cs` 생성 (New Game → SCN_Farm 로드, Continue → 저장 슬롯 확인 후 로드, Quit → Application.Quit). Canvas GO에 컴포넌트 추가 및 버튼 OnClick 연결.

---

## 4. 잔여 QA 항목

| ID | 내용 | 우선순위 |
|----|------|---------|
| QA-001 | SCN_Farm 전체 플레이 (이동·농사·HUD·저장) | 4 |
| QA-002 | SCN_Farm UI(Overlay) 표시 검증 | 3 |
| QA-003 | 씬 전환 후 시스템 초기화 검증 | 3 |
| QA-004 | AudioMixer 에셋 수동 생성 | 2 |

---

## 5. 의사결정 기록

- **폰트 전략**: 한국어 TMP 폰트는 프로젝트 에셋으로 포함. 모든 TMP 컴포넌트의 기본 폰트로 설정하여 이후 추가 UI 생성 시 자동 적용.
- **MainMenuController 설계**: 씬 전환은 `SceneManager.LoadSceneAsync`를 사용하여 SCN_Loading 씬을 경유하는 방식으로 구현. Continue 버튼은 SaveManager.HasSaveData() 확인 후 활성화.
- **InputSystem 정책**: 프로젝트 전체 New Input System 전용 확인. 구 `UnityEngine.Input` API 사용 스크립트가 있는 경우 후속 QA에서 발견 즉시 교체.

---

## 6. 다음 세션 목표

1. SCN_Farm Play Mode 진입 → 플레이어 이동 검증 (QA-001)
2. 농사 사이클 1회 완주 (씨앗 구매 → 경작 → 파종 → 물주기 → 수확 → 판매)
3. HUD(골드·에너지·날짜·계절) 표시 확인
4. 저장/로드 기본 동작 확인
