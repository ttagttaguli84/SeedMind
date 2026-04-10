# SeedMind

> AI가 스스로 설계하고, 구축하고, 배포하는 게임 프로젝트

## 프로젝트 개요

**SeedMind**는 하나의 실험이다.

AI(Claude Code)가 사람의 개입 없이 게임을 **설계 → 구축 → 배포**까지 완수할 수 있는가?

사람은 씨앗(Seed)을 심을 뿐, 나머지는 AI의 판단(Mind)에 맡긴다.

## 게임 컨셉

**"씨앗 하나로 시작하는 나만의 농장"** — SeedMind Farm Simulation

- **장르**: 농장 시뮬레이션
- **핵심 메카닉**: 경작(Farming) → 성장(Growing) → 수확/판매(Harvest) → 확장(Expand)
- **비주얼**: 로우폴리 / 스타일라이즈드 3D
- **시점**: 탑다운 또는 쿼터뷰 3D
- **특징**: AI가 Unity MCP를 통해 에셋, 씬, 스크립트를 직접 제작

> 자세한 내용: [Game Design Document](docs/design.md) | [Architecture](docs/architecture.md)

## 실험 목적

| 질문 | 검증 방식 |
|------|-----------|
| AI가 게임 컨셉을 스스로 결정할 수 있는가? | 메카닉, 시스템, 아트 방향성을 자율 설계 |
| AI가 실제 리소스를 제작할 수 있는가? | Unity MCP를 통해 씬, 오브젝트, 머티리얼 직접 생성 |
| AI가 동작하는 게임을 만들 수 있는가? | Unity 기반 플레이 가능한 빌드 생성 |
| AI가 빌드/배포까지 완료할 수 있는가? | 실행 가능한 빌드 산출물 생성 |

## 규칙

1. **사람은 지시하지 않는다** — 최초 주제 선정 이후 구체적인 구현 지시 없음
2. **AI가 모든 판단을 내린다** — 기술 선택, 구조 설계, 구현 순서, 문제 해결
3. **과정을 기록한다** — 모든 의사결정과 그 근거를 문서화
4. **결과를 있는 그대로 공개한다** — 성공이든 실패든 투명하게 공유

## 기술 스택

- **Engine**: Unity 6
- **Language**: C#
- **AI Agent**: Claude Code (Opus)
- **AI ↔ Unity 연동**: MCP for Unity (씬 편집, 오브젝트 생성, 스크립트 연결)
- **Version Control**: Git + GitHub

## 프로젝트 구조

```
SeedMind/
├── README.md                # 프로젝트 소개
├── docs/                    # AI 의사결정 로그
│   ├── design.md            # 게임 설계 문서
│   ├── architecture.md      # 기술 아키텍처
│   └── devlog/              # 개발 일지
├── Assets/                  # Unity 에셋
│   ├── Scripts/             # C# 스크립트
│   ├── Prefabs/             # 프리팹
│   ├── Materials/           # 머티리얼
│   ├── Scenes/              # 씬
│   └── UI/                  # UI 에셋
├── Packages/                # Unity 패키지
└── ProjectSettings/         # Unity 프로젝트 설정
```

## 결과: 실패

**이 프로젝트는 목표를 달성하지 못했습니다.**

Phase 2까지 Unity MCP를 통해 씬·시스템·콘텐츠·UI를 모두 구현했으나, Phase 3 QA에서 게임이 플레이 가능한 상태에 도달하지 못했습니다. 자동화 테스트(48/48)는 통과했지만 실제 플레이는 불가합니다.

- [x] Phase 0: GitHub 저장소 생성
- [x] Phase 1: AI 자율 게임 설계
- [x] Phase 2: Unity MCP 전체 구현 (씬·시스템·콘텐츠·UI) — 완료 (2026-04-10)
- [x] Phase 3: QA & 플레이 테스트 — **게임 플레이 불가, 실험 종료**
- [ ] ~~Phase 4: 빌드 및 배포~~ — 진행하지 않음

### Phase 1 완료 산출물

| 카테고리 | 문서 수 |
|---------|--------|
| 시스템 설계 (DES) | 26개 |
| 기술 아키텍처 (ARC) | 26개 |
| MCP 태스크 시퀀스 | 26개 |
| 밸런스 시트 (BAL) | 9개 |
| 콘텐츠 스펙 (CON) | 10개 |
| 파이프라인/기타 | 5개 |

## 라이선스

MIT License

---

*이 README를 포함한 모든 코드와 문서는 Claude Code가 자율적으로 작성했습니다.*
