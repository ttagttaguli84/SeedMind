# Document Standards

## File Naming
- System docs: `docs/systems/<system-name>.md`
- MCP plans: `docs/mcp/<system-name>-tasks.md`
- Content specs: `docs/content/<category>.md`
- Balance sheets: `docs/balance/<topic>.md`
- Devlogs: `docs/devlog/NNN-<short-title>.md` (3-digit zero-padded)

## Document Structure
Every design document MUST include:
1. **Header**: title, date, author (Claude Code)
2. **Context**: what this document covers and why
3. **Specification**: the actual content
4. **Cross-references**: links to related documents
5. **Open questions**: tagged with `[OPEN]`
6. **Risks**: tagged with `[RISK]`

## Consistency Rules
- 작물 이름, 건물 이름, 시스템 이름: 모든 문서에서 완전히 동일한 문자열 사용
- 수치(가격, 성장일수, 임계값): 가장 구체적인 canonical 문서에 단일 출처 기재
- 수치가 여러 문서에 나타날 경우, canonical 문서 외 모든 곳에는 실제 값 대신 `(→ see docs/X.md)` 참조만 기재
- **아키텍처 문서에서 디자인 수치를 기재할 때는 반드시 `(→ see canonical)` 참조를 붙여야 하며, 독립적으로 수치를 기재하는 것을 금지한다**
- **코드 예시 내 기본값(defaultValue, tuningParam 등)도 canonical 참조 주석 필요**: `// → see docs/systems/X.md`
- 동일 문서 내에서도 섹션 간 수치가 중복 기재되지 않도록 한다. 한 섹션에 기재하고 다른 섹션에서는 해당 섹션을 참조한다
- **enum/타입 확장 시**: 해당 enum을 사용하는 모든 코드 예시(switch 문, GetXxxMultiplier 등)를 같은 문서 내에서 전수 업데이트해야 한다
- **(PATTERN-005) JSON 스키마와 C# 클래스 동기화**: 동일 문서 내에 Part I JSON 예시와 Part II C# 클래스를 함께 작성하는 경우, 모든 필드명·필드 수가 양쪽에 동일하게 존재해야 한다. 필드 추가·삭제·이름 변경 시 JSON 예시와 C# 클래스를 같은 편집에서 동시에 업데이트한다. 리뷰어는 Reviewer Checklist 항목 9를 통해 이를 검증한다.
- **(PATTERN-006) MCP 태스크 내 배열·테이블 수치 기재 금지**: MCP 태스크 문서(Part II 포함)에서 배열·테이블 형태의 수치(XP 테이블, 가격 목록 등)를 직접 기재하지 않는다. canonical 문서를 `(→ see docs/X.md)` 참조로만 가리킨다. 불가피하게 직접 기재할 경우 해당 값 옆에 `// → copied from docs/X.md` 주석을 붙이고, canonical 문서를 동시에 수정하여 값이 동일함을 보장한다.
- **(PATTERN-007) SO 에셋 테이블의 콘텐츠 파라미터 직접 기재 금지**: `data-pipeline.md` 등 파이프라인/아키텍처 문서의 SO 에셋 데이터 테이블(시설별 에셋 데이터 표 등)에서 `tileSize`, `buildTimeDays`, `recipeCount` 등 콘텐츠 정의 파라미터를 구체적 수치로 직접 기재하는 것을 금지한다. 해당 파라미터는 canonical 콘텐츠 문서를 `(→ see docs/content/facilities.md 섹션 X.X)` 형식으로만 참조한다. 파이프라인 문서 내 SO 에셋 테이블의 역할은 필드 타입·기본값 스키마 정의에 국한하며, 실제 콘텐츠 값은 콘텐츠 문서가 단독 출처가 된다.

## Canonical 데이터 매핑

신규 문서 작성 시, 아래 매핑에 따라 canonical 출처를 확인하고 수치를 직접 기재하지 않는다.

| 데이터 종류 | Canonical 문서 | 비고 |
|------------|---------------|------|
| 작물 이름, 씨앗 가격, 판매가, 성장일수 | `docs/design.md` 섹션 4.2 | 작물 목록 전체 |
| 작물 성장 단계, 품질 공식, 특수 성장(giant 등) | `docs/systems/crop-growth.md` | 성장 메카닉 전반 |
| 타일 상태, 도구 인터랙션, 물/비료 효과 | `docs/systems/farming-system.md` | 경작 메카닉 전반 |
| 시간대 정의, 날씨 종류, 날씨 확률, 계절 전환 | `docs/systems/time-season.md` | 시간/날씨 데이터 |
| 경제 수치(초기 골드, 가격 상하한, 수급 보정 등) | `docs/systems/economy-system.md` | 경제 메카닉 전반 |
| 시설 이름, 건설 요건, 업그레이드 경로 | `docs/design.md` 섹션 4.6 | 시설 목록 전체 |
| BuildingData SO 필드 정의 | `docs/pipeline/data-pipeline.md` Part I 섹션 2.4 | 시설 SO 스키마 |
| **시설 tileSize, buildTimeDays, effectRadius 등 구조적 파라미터** | **`docs/content/facilities.md`** | **섹션 2.1 건설 프로세스 / 시설별 상세 섹션** |
| Unity 프로젝트 폴더 구조, 네임스페이스 | `docs/systems/project-structure.md` | 프로젝트 구조 |

## Language
- Document content: Korean (technical terms in English where natural)
- Tags/labels: English (`[OPEN]`, `[RISK]`, `[TODO]`)
- File names: English, kebab-case
