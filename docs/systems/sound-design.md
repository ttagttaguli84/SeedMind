# 사운드 디자인 (Sound Design) 상세 설계

> 작성: Claude Code (Opus 4.6) | 2026-04-07
> 문서 ID: DES-014

---

## Context

이 문서는 SeedMind의 전체 사운드 설계를 기술한다. BGM, 효과음(SFX), 환경음(Ambient)의 목록, 트리거 조건, 믹싱 가이드라인, 구현 우선순위를 포함한다.

**설계 목표**: 사운드는 플레이어가 계절의 변화를 "듣고", 행동의 결과를 "느끼며", 농장의 생동감을 "감각"하게 하는 핵심 피드백 채널이다. 로우폴리 아트 스타일에 맞춰 과도한 리얼리즘보다는 따뜻하고 명쾌한 사운드 톤을 지향한다.

**사운드 톤 키워드**: 따뜻한, 아기자기한, 명쾌한, 자연적인, 편안한

**본 문서가 canonical인 데이터**:
- BGM 트랙 목록, 분위기/악기 구성, 루프 사양
- SFX 전체 목록 (ID, 트리거 조건, 음향 특성)
- AudioMixer 채널 구조 및 볼륨 믹싱 비율
- Variation 처리 규칙, 거리 감쇠 대상 목록
- SFX 구현 우선순위 분류 (MVP / Polish)

**본 문서가 canonical이 아닌 데이터 (참조만)**:

| 데이터 종류 | 참조처 |
|------------|--------|
| 시간대 정의 (Dawn/Morning/Afternoon/Evening/Night) | `docs/systems/time-season.md` 섹션 1.2 |
| 날씨 종류 (Clear/Cloudy/Rain/HeavyRain/Storm/Snow/Blizzard) | `docs/systems/time-season.md` 섹션 3.1 |
| 계절별 환경 사운드 개요 | `docs/systems/time-season.md` 섹션 2.3 |
| 도구 등급 (Basic/Reinforced/Legendary) | `docs/systems/tool-upgrade.md` 섹션 1.1 |
| 시설 목록, 가공소 종류 | `docs/design.md` 섹션 4.6 |
| 작물 성장 단계, 품질 등급 | `docs/systems/crop-growth.md` |
| 낚시 미니게임 구조 | `docs/systems/fishing-system.md` |
| UI 시스템 구조 | `docs/systems/ui-system.md` |
| 목축 시스템 동물 종류 | `docs/content/livestock-system.md` |
| 가공 레시피, 가공소별 기계 | `docs/content/processing-system.md` |
| NPC 목록, 상점 운영 | `docs/content/npcs.md` |
| 퀘스트/업적 시스템 | `docs/systems/quest-system.md`, `docs/systems/achievement-system.md` |

---

## 1. BGM 설계

### 1.1 설계 원칙

- **비간섭적(Non-intrusive)**: BGM은 배경으로 존재하며, 작업 집중을 방해하지 않는다
- **계절 아이덴티티**: 각 계절 BGM은 고유한 악기/선율로 계절감을 전달한다
- **자연스러운 전환**: BGM 전환 시 1.5초 크로스페이드 적용
- **루프 이음매 투명**: 루프 포인트에서 끊김이 느껴지지 않도록 설계

### 1.2 계절별 BGM

| ID | 트랙명 | 계절 | BPM | 조성 | 악기 구성 | 분위기 | 루프 사양 |
|----|--------|------|-----|------|-----------|--------|-----------|
| `bgm_spring` | 봄바람의 시작 | 봄 (Spring) | 100~110 | C Major / F Major | 어쿠스틱 기타, 글로켄슈필, 플루트, 가벼운 퍼커션 | 설렘, 새로운 시작, 따뜻한 햇살 | 8마디 x 4 (32마디), 약 70초 루프 |
| `bgm_summer` | 뜨거운 오후 | 여름 (Summer) | 110~120 | G Major / D Major | 우쿨렐레, 마림바, 탬버린, 가벼운 베이스 | 활기참, 풍요, 에너지 넘침 | 8마디 x 4 (32마디), 약 65초 루프 |
| `bgm_autumn` | 노을빛 수확 | 가을 (Autumn) | 85~95 | A minor / D minor | 첼로, 어쿠스틱 기타, 오보에, 부드러운 스네어 브러시 | 따뜻한 노스탤지어, 풍성한 수확, 차분함 | 8마디 x 4 (32마디), 약 80초 루프 |
| `bgm_winter` | 고요한 눈밭 | 겨울 (Winter) | 70~80 | E minor / A minor | 피아노, 첼레스타, 현악 패드, 하프 | 고요함, 쓸쓸함, 따뜻한 벽난로 | 8마디 x 4 (32마디), 약 90초 루프 |

### 1.3 특수 상황 BGM

| ID | 트랙명 | 트리거 조건 | BPM | 악기 구성 | 분위기 | 루프 사양 |
|----|--------|------------|-----|-----------|--------|-----------|
| `bgm_rain` | 빗소리 속에서 | 날씨가 Rain 또는 HeavyRain일 때 (계절 BGM 대체) | 80~90 | 피아노, 부드러운 현악, 뮤트 기타 | 잔잔함, 명상적 | 16마디, 약 45초 루프 |
| `bgm_storm` | 거센 바람 | 날씨가 Storm일 때 | 90~100 | 저음 현악, 팀파니 트레몰로, 긴장감 있는 패드 | 긴장, 위협, 드라마틱 | 16마디, 약 40초 루프 |
| `bgm_blizzard` | 눈보라 | 날씨가 Blizzard일 때 | 65~75 | 저음 패드, 바람 신스, 희미한 피아노 | 혹독함, 고립감 | 16마디, 약 50초 루프 |
| `bgm_night` | 별빛 아래 | 시간대가 Night(20:00~24:00)일 때 (계절 BGM과 크로스페이드) | 60~70 | 피아노, 뮤직 박스, 부드러운 패드 | 평온, 하루의 마무리 | 16마디, 약 55초 루프 |
| `bgm_indoor_home` | 우리집 | 플레이어가 자택 실내에 있을 때 | 85~95 | 어쿠스틱 기타 솔로, 가벼운 퍼커션 | 아늑함, 편안함 | 8마디 x 2 (16마디), 약 40초 루프 |
| `bgm_indoor_shop` | 상점 | 상점 NPC 인터페이스가 열려 있을 때 | 100~110 | 아코디언, 글로켄슈필, 가벼운 베이스 | 활기참, 친근함 | 8마디 x 2 (16마디), 약 35초 루프 |
| `bgm_festival` | 축제의 날 | 계절 이벤트(축제) 활성 시 | 120~130 | 풀 앙상블(기타+바이올린+탬버린+아코디언+드럼) | 흥겨움, 축하, 화려함 | 16마디 x 2 (32마디), 약 60초 루프 |

### 1.4 BGM 전환 규칙

| 전환 상황 | 우선순위 | 전환 방식 |
|-----------|---------|-----------|
| 계절 BGM -> 날씨 BGM | 날씨 BGM 우선 | 1.5초 크로스페이드 |
| 야외 BGM -> 실내 BGM | 실내 BGM 우선 | 1.0초 크로스페이드 |
| 일반 BGM -> 축제 BGM | 축제 BGM 우선 | 2.0초 크로스페이드 |
| 낮 BGM -> 밤 BGM | 밤 BGM 우선 | 3.0초 크로스페이드 (자연스러운 전환) |
| 날씨 종료 -> 계절 BGM 복귀 | - | 1.5초 크로스페이드 |

**BGM 우선순위 스택** (높을수록 우선):
1. 축제 BGM
2. 실내 BGM
3. 날씨 BGM (Storm > Blizzard > Rain)
4. 밤 BGM
5. 계절 BGM (기본, 항상 재생)

---

## 2. 효과음(SFX) 목록

### 2.1 경작 (Farming)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_hoe_basic` | 괭이질 (기본) | Basic 호미로 타일 경작 시 | 둔탁한 흙 파는 소리, 0.3초 | 등급별 분리 |
| `sfx_hoe_reinforced` | 괭이질 (강화) | Reinforced 호미 사용 시 | 기본 + 금속 잔향 추가, 0.35초 | |
| `sfx_hoe_legendary` | 괭이질 (전설) | Legendary 호미 사용 시 | 기본 + 마법 반짝임 레이어, 0.4초 | |
| `sfx_seed_plant` | 씨앗 심기 | 경작된 타일에 씨앗 심을 때 | 부드러운 흙 덮는 소리 + 작은 "톡", 0.25초 | |
| `sfx_water_basic` | 물주기 (기본) | Basic 물뿌리개 사용 시 | 물 붓는 짧은 "철철", 0.4초 | |
| `sfx_water_reinforced` | 물주기 (강화) | Reinforced 물뿌리개 사용 시 | 넓게 뿌리는 "쏴아", 0.5초 | |
| `sfx_water_legendary` | 물주기 (전설) | Legendary 물뿌리개 사용 시 | 광역 분무 + 은방울 레이어, 0.6초 | |
| `sfx_scythe_basic` | 낫질 (기본) | Basic 낫으로 수확/풀 베기 | 날카로운 바람 가르는 "쉭", 0.3초 | |
| `sfx_scythe_reinforced` | 낫질 (강화) | Reinforced 낫 사용 시 | 기본 + 금속 울림, 0.35초 | |
| `sfx_scythe_legendary` | 낫질 (전설) | Legendary 낫 사용 시 | 기본 + 바람 잔향 레이어, 0.4초 | |
| `sfx_harvest` | 작물 수확 | 수확 가능 작물 수확 시 | "뽁" 뽑는 소리 + 짧은 만족 징글(상승 3음), 0.5초 | |
| `sfx_fertilize` | 비료 뿌리기 | 경작 타일에 비료 사용 시 | 가루 흩뿌리는 "사삭", 0.3초 | |

### 2.2 작물 (Crop)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_crop_grow` | 성장 단계 전환 | 작물이 다음 성장 단계로 전환될 때 | 부드러운 "퐁" + 반짝임, 0.4초 | 시각 이펙트와 동기화 |
| `sfx_crop_wither` | 작물 시들음 | 작물이 Withered 상태로 전환될 때 | 바스락 마른 소리 + 낮은 톤 "뿌웅", 0.5초 | |
| `sfx_crop_golden` | 황금 작물 수확 | Gold 등급 이상 작물 수확 시 | 밝은 종소리 징글 + 반짝임 잔향, 0.8초 | sfx_harvest와 레이어링 |
| `sfx_crop_giant` | 거대 작물 발견 | Giant Crop 생성 시 | 깊은 울림 + 경이로운 상승 징글, 1.0초 | 드문 이벤트 강조 |

### 2.3 도구 (Tool)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_tool_equip` | 도구 장착 | 도구 슬롯 전환 시 (1~5키) | 짧은 금속/나무 "칙" 소리, 0.15초 | |
| `sfx_tool_upgrade_start` | 업그레이드 시작 | 대장장이에게 도구 맡길 때 | 모루 위 망치질 시작음, 0.5초 | |
| `sfx_tool_upgrade_complete` | 업그레이드 완료 | 업그레이드된 도구 수령 시 | 대장장이 망치 마무리 + 빛나는 징글(상승 5음), 1.0초 | 달성감 강조 |
| `sfx_tool_break` | 에너지 부족 사용 | 에너지 0에서 도구 사용 시도 시 | 둔탁한 "텅" + 낮은 경고음, 0.3초 | |

### 2.4 시설/건설 (Facility/Construction)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_construct_start` | 건설 시작 | 시설 건설 확인 시 | 나무 프레임 세우는 소리 + 망치질, 0.8초 | |
| `sfx_construct_complete` | 건설 완료 | 시설 건설 완료 시 (다음 날 배치) | 팡파르형 짧은 징글(트럼펫 느낌) + 먼지 날림, 1.2초 | |
| `sfx_construct_upgrade` | 시설 업그레이드 | 시설 업그레이드 완료 시 | 건설 완료음의 고급 버전 + 반짝임 레이어, 1.2초 | |
| `sfx_facility_activate` | 시설 가동 | 가공소/시설에 아이템 투입 시 | 기계 시동 걸리는 "윙" 소리, 0.5초 | |
| `sfx_facility_idle` | 시설 대기 | 가공 중인 시설 근처 접근 시 | 저음의 기계 웅웅거림 루프 | 3D 거리 감쇠 적용 |

### 2.5 가공 (Processing)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_process_start` | 가공 시작 | 가공소에 재료 투입하여 가공 시작 시 | 재료 투입 "달깍" + 기계 시동, 0.6초 | |
| `sfx_process_complete` | 가공 완료 | 가공 완료되어 산출물 수령 가능 시 | 밝은 벨 소리 + 김 빠지는 "쉬익", 0.8초 | |
| `sfx_mill_running` | 제분소 가동음 | 제분소 가동 중 근처 접근 시 | 맷돌 돌아가는 "그르르" 루프 | 3D 거리 감쇠 |
| `sfx_ferment_bubble` | 발효실 가동음 | 발효실 가동 중 근처 접근 시 | 거품 올라오는 "보글보글" 루프 | 3D 거리 감쇠 |
| `sfx_bakery_oven` | 베이커리 가동음 | 베이커리 가동 중 근처 접근 시 | 오븐 타닥타닥 불소리 + 빵 굽는 김 루프 | 3D 거리 감쇠 |
| `sfx_cheese_churn` | 치즈 공방 가동음 | 치즈 공방 가동 중 근처 접근 시 | 통 돌리는 "찰랑찰랑" 루프 | 3D 거리 감쇠 |

### 2.6 목축 (Livestock)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_animal_feed` | 동물 먹이주기 | 동물에게 사료 제공 시 | 사료통에 사료 쏟는 "사삭" + 동물 반응음, 0.5초 | |
| `sfx_milk` | 우유 짜기 | 소에게 우유 짜기 시 | 리듬감 있는 "쉭쉭" 짜는 소리, 0.8초 | |
| `sfx_shear` | 털 깎기 | 양에게 털 깎기 시 | 가위 "사각사각" + 양털 뭉치 떨어지는 "뿅", 0.7초 | |
| `sfx_egg_collect` | 알 줍기 | 닭장에서 알 수집 시 | 둥지 바스락 + 달걀 집는 "톡", 0.3초 | |
| `sfx_chicken_cluck` | 닭 울음 | 닭 근처 접근 / 상호작용 시 | "꼬꼬꼬" 3종 랜덤 variation | 3D 거리 감쇠 |
| `sfx_cow_moo` | 소 울음 | 소 근처 접근 / 상호작용 시 | "음메" 2종 랜덤 variation | 3D 거리 감쇠 |
| `sfx_sheep_baa` | 양 울음 | 양 근처 접근 / 상호작용 시 | "메에" 2종 랜덤 variation | 3D 거리 감쇠 |
| `sfx_goat_bleat` | 염소 울음 | 염소 근처 접근 / 상호작용 시 | 높은 톤 "매애" 2종 랜덤 variation | 3D 거리 감쇠 |
| `sfx_animal_happy` | 동물 행복 반응 | 동물 친밀도 상승 시 | 짧은 하트 팝 징글 + 동물 반응, 0.5초 | 시각 하트 이펙트 동기화 |
| `sfx_animal_sick` | 동물 아픔 | 동물이 아플 때 근처 접근 시 | 기운 없는 낮은 울음 | |

### 2.7 낚시 (Fishing)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_cast_line` | 낚싯줄 던지기 | 낚시 시작, 줄 캐스팅 시 | 줄 "휘이익" 날아가는 소리 + 물 "풍덩" 착수, 0.7초 | |
| `sfx_fish_nibble` | 물고기 탐색 | 물고기가 미끼에 관심 보일 때 (찌 흔들림) | 가벼운 물 출렁 "통통", 0.2초 반복 | |
| `sfx_fish_bite` | 물고기 입질 | 물고기가 미끼를 물었을 때 (미니게임 시작) | 강한 물 당김 "철퍽!" + 긴장 알림, 0.4초 | |
| `sfx_reel_in` | 줄 감기 | 미니게임 중 줄 감기 액션 시 | 릴 돌아가는 "째깍째깍" 루프 | 미니게임 중 반복 |
| `sfx_fish_struggle` | 물고기 저항 | 미니게임 중 물고기가 반대로 당길 때 | 줄 팽팽한 "삐이익" + 물 튀기는 소리, 0.5초 | |
| `sfx_fish_catch_normal` | 낚시 성공 (일반) | 일반/흔한 어종 낚시 성공 시 | 물에서 "철퍽" + 짧은 성공 징글(상승 3음), 0.6초 | |
| `sfx_fish_catch_rare` | 낚시 성공 (희귀) | 희귀/전설 어종 낚시 성공 시 | 강화된 성공 징글 + 반짝임 + 팡파르, 1.2초 | |
| `sfx_fish_escape` | 낚시 실패 | 미니게임 실패, 물고기 도주 시 | 줄 끊어지는 "뚝" + 낮은 하강 3음, 0.5초 | |
| `sfx_fish_splash` | 물 튀김 | 낚시터 주변 물고기 점프 (환경 연출) | 작은 물 튀기는 "첨벙", 0.3초 | 3D 거리 감쇠 |

### 2.8 NPC/상점 (NPC/Shop)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_shop_open` | 상점 열기 | 상점 UI 열릴 때 | 나무 문 "삐걱" + 작은 벨 "딸랑", 0.5초 | |
| `sfx_shop_close` | 상점 닫기 | 상점 UI 닫힐 때 | 문 "탁" 닫히는 소리, 0.3초 | |
| `sfx_purchase` | 구매 완료 | 아이템 구매 확정 시 | 동전 "짤랑" + 가벼운 확인음, 0.4초 | |
| `sfx_sell` | 판매 완료 | 아이템 판매 확정 시 | 동전 쏟아지는 "철렁" + 확인음, 0.5초 | |
| `sfx_dialog_start` | 대화 시작 | NPC 대화 시작 시 | 짧은 "띠링" 알림음, 0.2초 | |
| `sfx_dialog_advance` | 대화 진행 | 대화 텍스트 넘기기 시 | 종이 넘기는 부드러운 "스윽", 0.15초 | |
| `sfx_dialog_choice` | 대화 선택지 | 대화 선택지 중 하나를 고를 때 | 짧은 선택 확인 "톡", 0.1초 | |
| `sfx_affinity_up` | NPC 친밀도 상승 | NPC 친밀도 등급 상승 시 | 따뜻한 하프 아르페지오 상승 징글, 1.0초 | 시각 하트 이펙트 동기화 |
| `sfx_shipping_bin` | 출하함 투입 | 출하함에 아이템 넣을 때 | 나무 상자 "통" + 아이템 떨어지는 소리, 0.3초 | |

### 2.9 퀘스트/업적 (Quest/Achievement)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_quest_accept` | 퀘스트 수락 | 퀘스트 수락 시 | 두루마리 펼치는 "사각" + 확인 징글, 0.5초 | |
| `sfx_quest_progress` | 퀘스트 진행 | 퀘스트 목표 일부 달성 시 | 짧은 체크 "딩", 0.2초 | |
| `sfx_quest_complete` | 퀘스트 완료 | 퀘스트 모든 목표 달성 시 | 팡파르 상승 징글(트럼펫풍) + 반짝임, 1.5초 | |
| `sfx_quest_reward` | 퀘스트 보상 수령 | 퀘스트 보상 아이템/골드 획득 시 | 보물 상자 열리는 "딸깍" + 아이템 획득 사운드, 0.8초 | |
| `sfx_achievement_toast` | 업적 달성 | 업적 조건 충족 시 (토스트 팝업) | 밝은 메달 "짠" + 짧은 팡파르, 1.2초 | UI 토스트와 동기화 |

### 2.10 UI

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_ui_click` | 버튼 클릭 | 모든 UI 버튼 클릭 시 | 가벼운 "톡", 0.1초 | |
| `sfx_ui_hover` | 버튼 호버 | UI 버튼 위에 마우스 올릴 때 | 매우 가벼운 "틱", 0.05초 | 볼륨 낮게 |
| `sfx_ui_tab` | 탭 전환 | UI 패널 탭 변경 시 | 짧은 "슬릭" 슬라이드, 0.15초 | |
| `sfx_inventory_open` | 인벤토리 열기 | Tab 키로 인벤토리 열 때 | 가방 여는 "척" + 가죽 소리, 0.3초 | |
| `sfx_inventory_close` | 인벤토리 닫기 | 인벤토리 UI 닫을 때 | 가방 닫는 "탁", 0.2초 | |
| `sfx_item_move` | 아이템 이동 | 인벤토리 내 아이템 드래그 시 | 아이템 집는/놓는 "톡톡", 0.15초 | |
| `sfx_notification` | 알림 팝업 | 시스템 알림(일기예보, 경고 등) 표시 시 | 부드러운 벨 "띵", 0.3초 | |
| `sfx_error` | 에러/불가 | 불가능한 행동 시도 시(재화 부족, 공간 없음 등) | 낮은 "뿡" 거부음, 0.2초 | |
| `sfx_confirm` | 확인 | 확인/결정 버튼 클릭 시 | 명쾌한 "딩!" 확인음, 0.15초 | |
| `sfx_cancel` | 취소 | 취소 버튼/Esc 시 | 부드러운 하강 "둥", 0.15초 | |

### 2.11 환경/날씨 (Environment/Weather)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `amb_rain_light` | 비 (약) | 날씨 Rain 시 | 빗방울 떨어지는 소리 루프, 중간 밀도 | Ambient 채널 |
| `amb_rain_heavy` | 비 (강) | 날씨 HeavyRain 시 | 빗줄기 쏟아지는 소리 루프, 높은 밀도 + 물 흐르는 소리 | Ambient 채널 |
| `amb_storm` | 폭풍 | 날씨 Storm 시 | 강한 빗소리 + 간헐적 천둥(5~15초 간격 랜덤) + 바람 | Ambient 채널 |
| `sfx_thunder` | 천둥 | Storm 날씨 중 랜덤 트리거 | "쿠르르릉" 천둥, 볼륨/지연 랜덤(가까운/먼 천둥), 2~4초 | |
| `sfx_lightning_flash` | 번개 섬광 | 천둥 직전 화면 플래시와 동기 | 짧은 "짜자직" 전기음, 0.3초 | |
| `amb_snow` | 눈 | 날씨 Snow 시 | 고요한 바람 + 눈 사각거림, 매우 부드러운 루프 | Ambient 채널 |
| `amb_blizzard` | 눈보라 | 날씨 Blizzard 시 | 강한 바람 "휘이잉" + 눈 때리는 소리 루프 | Ambient 채널 |
| `amb_wind_light` | 바람 (약) | 맑은 날씨 기본 환경음 | 가벼운 바람 스치는 소리 루프 | Ambient 채널 |
| `amb_birds_day` | 새소리 (낮) | 시간대 Dawn/Morning/Afternoon, 봄/여름 | 참새/뻐꾸기 울음 간헐적(8~20초 간격 랜덤) | Ambient 채널 |
| `amb_cicada` | 매미소리 | 시간대 Afternoon, 여름 | 매미 울음 루프 (-> see `docs/systems/time-season.md` 섹션 2.3) | Ambient 채널 |
| `amb_cricket` | 귀뚜라미 | 시간대 Evening/Night, 가을 | 귀뚜라미 울음 루프 (-> see `docs/systems/time-season.md` 섹션 2.3) | Ambient 채널 |
| `amb_insects_night` | 벌레소리 (밤) | 시간대 Night, 봄/여름 | 풀벌레 울음 루프 | Ambient 채널 |
| `amb_waves` | 파도소리 | 낚시터(Zone F) 근처 | 잔잔한 물결 루프 | 3D 거리 감쇠 |
| `sfx_footstep_dirt` | 발걸음 (흙) | 흙/경작지 위 이동 시 | 흙 밟는 "사벅사벅", 0.2초 | 이동 속도에 맞춤 |
| `sfx_footstep_grass` | 발걸음 (풀) | 잔디 위 이동 시 | 풀 밟는 "서걱서걱", 0.2초 | |
| `sfx_footstep_wood` | 발걸음 (나무) | 실내 나무 바닥 이동 시 | 나무 바닥 "톡톡", 0.2초 | |
| `sfx_footstep_snow` | 발걸음 (눈) | 겨울 눈 위 이동 시 (-> see `docs/systems/time-season.md` 섹션 2.3) | 눈 밟는 "뽀드득", 0.25초 | |
| `sfx_footstep_stone` | 발걸음 (돌) | 돌 길/건물 내부 이동 시 | 돌 밟는 "딱딱", 0.2초 | |

### 2.12 진행/레벨 (Progression/Level)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_level_up` | 레벨업 | 플레이어 레벨 상승 시 | 화려한 팡파르 징글(금관+현악) + 반짝임, 2.0초 | 가장 화려한 징글 |
| `sfx_xp_gain` | XP 획득 | 경험치 획득 시 | 짧은 "띵" 상승음, 0.15초 | 빈번하므로 매우 짧고 가벼움 |
| `sfx_gold_gain` | 골드 획득 | 골드 증가 시 (판매 정산 등) | 동전 "짤랑짤랑", 0.3초 | 획득 금액에 따라 볼륨/레이어 수 변화 |
| `sfx_gold_spend` | 골드 지출 | 골드 감소 시 (구매, 건설 등) | 동전 꺼내는 "찰랑", 0.3초 | |
| `sfx_energy_warning` | 에너지 경고 | 에너지 20% 이하 시 | 낮은 톤 심장 박동 "둥둥" + 경고 알림, 0.8초 | 1회만 재생(같은 날) |
| `sfx_energy_depleted` | 에너지 고갈 | 에너지 0 도달 시 | 무거운 숨소리 + 낮은 경고 징글, 1.0초 | |

### 2.13 시간/계절 (Time/Season)

| ID | 이름 | 트리거 조건 | 음향 특성 | 비고 |
|----|------|------------|-----------|------|
| `sfx_morning_chime` | 아침 시작 | 매일 06:00 하루 시작 시 | 수탉 울음 + 부드러운 오르골 상승 3음, 1.5초 | 하루 시작 의식감 |
| `sfx_evening_bell` | 저녁 시작 | 17:00 저녁 시간대 전환 시 | 종 울리는 "땡" 1회, 1.0초 | 시간 경과 인지 |
| `sfx_midnight_warning` | 자정 경고 | 23:00 접근 시 | 시계 째깍 "틱틱틱" 빨라지는 효과 + 경고음, 1.5초 | |
| `sfx_pass_out` | 기절 | 24:00 자동 기절 시 | 힘 빠지는 하강 "슈웅" + 쿵 쓰러지는 소리, 1.0초 | |
| `sfx_sleep` | 수면 | 침대에서 잠자기 선택 시 | 부드러운 하프 하강 아르페지오 + 불끄는 소리, 1.5초 | |
| `sfx_season_transition` | 계절 전환 | 계절이 바뀔 때 (Day 28 -> Day 1) | 바람 스치는 효과 + 새 계절 테마 악기 프리뷰(2초) + 글로켄슈필 팡파르, 3.0초 | 특별한 순간 강조 |
| `sfx_day_summary` | 하루 정산 | 잠자기 후 정산 화면 표시 시 | 가벼운 종이 펼치는 소리 + 배경 오르골, 0.5초 | |

---

## 3. 사운드 구현 가이드라인

### 3.1 Unity AudioMixer 채널 구조

```
Master
├── BGM           (배경 음악)
│   ├── BGM_Main      (계절/실내/축제 BGM)
│   └── BGM_Weather   (날씨 BGM, 크로스페이드용)
├── SFX           (효과음)
│   ├── SFX_Player    (도구, 발걸음, 수확 등 플레이어 행동)
│   ├── SFX_World     (동물, 시설 가동음 등 월드 오브젝트)
│   └── SFX_Jingle    (레벨업, 업적, 퀘스트 등 징글)
├── Ambient       (환경음)
│   ├── AMB_Weather   (비, 눈, 바람)
│   ├── AMB_Nature    (새소리, 벌레, 매미)
│   └── AMB_Water     (파도, 강, 연못)
└── UI            (인터페이스)
    ├── UI_Click      (버튼, 탭 전환)
    └── UI_Notify     (알림, 에러, 확인)
```

### 3.2 기본 볼륨 믹싱 비율

| 채널 | 기본 볼륨 (dB) | 비율 설명 | 설정 메뉴 노출 |
|------|---------------|-----------|---------------|
| Master | 0 dB | 전체 마스터 | O |
| BGM | -8 dB | SFX 대비 약하게, 배경 역할 | O |
| SFX | -3 dB | 플레이어 행동 피드백, 가장 명료 | O |
| Ambient | -12 dB | 배경 환경, BGM보다 더 약하게 | O |
| UI | -6 dB | 적당한 존재감, SFX보다 약간 약하게 | O |

**상황별 자동 조정 (Ducking)**:
- 징글(SFX_Jingle) 재생 시: BGM -6 dB 추가 감쇠 (0.3초 fade), 징글 종료 후 복귀
- 대화(Dialog) 중: BGM -4 dB 추가 감쇠
- 상점/메뉴 UI 열림 시: Ambient -6 dB 추가 감쇠

### 3.3 Variation 처리

같은 액션을 반복할 때 사운드 피로(Sound Fatigue)를 방지하기 위한 규칙.

| 기법 | 적용 대상 | 상세 |
|------|-----------|------|
| **Pitch Randomization** | 모든 SFX_Player 효과음 | 기본 pitch 1.0 기준 +/-0.05 랜덤 변동 |
| **Multi-Sample Round Robin** | 발걸음, 괭이질, 물주기, 낫질 | 2~3개 variation 파일, 순차 재생 (연속 동일 파일 방지) |
| **Volume Randomization** | 환경음 (새소리, 벌레소리) | 기본 볼륨 기준 +/-2 dB 랜덤 |
| **Interval Randomization** | 간헐적 환경음 (새소리, 천둥) | 기본 간격 기준 +/-30% 랜덤 |
| **Cooldown** | sfx_xp_gain, sfx_gold_gain | 동일 SFX 최소 간격 0.1초 (빠른 연속 획득 시 겹침 방지) |

### 3.4 거리 기반 감쇠 (3D Spatial Audio)

3D 거리 감쇠가 적용되는 SFX 목록. 이 목록에 포함되지 않은 SFX는 2D(글로벌) 재생.

| 대상 SFX | Min Distance | Max Distance | Rolloff 모드 |
|----------|-------------|-------------|-------------|
| `sfx_facility_idle` (시설 가동 루프) | 2 타일 | 10 타일 | Logarithmic |
| `sfx_mill_running` (제분소) | 2 타일 | 10 타일 | Logarithmic |
| `sfx_ferment_bubble` (발효실) | 2 타일 | 10 타일 | Logarithmic |
| `sfx_bakery_oven` (베이커리) | 2 타일 | 10 타일 | Logarithmic |
| `sfx_cheese_churn` (치즈 공방) | 2 타일 | 10 타일 | Logarithmic |
| `sfx_chicken_cluck` (닭) | 1 타일 | 8 타일 | Logarithmic |
| `sfx_cow_moo` (소) | 2 타일 | 12 타일 | Logarithmic |
| `sfx_sheep_baa` (양) | 1 타일 | 10 타일 | Logarithmic |
| `sfx_goat_bleat` (염소) | 1 타일 | 10 타일 | Logarithmic |
| `sfx_fish_splash` (물고기 점프) | 1 타일 | 8 타일 | Logarithmic |
| `amb_waves` (파도) | 3 타일 | 15 타일 | Linear |

**참고**: 쿼터뷰 카메라 특성상 화면 내 오브젝트는 대부분 Max Distance 이내이므로, 감쇠는 "화면 밖 오브젝트 소리 줄이기" 목적이 주된 용도이다.

### 3.5 동시 재생 제한 (Polyphony)

| 채널 | 최대 동시 재생 수 | 초과 시 처리 |
|------|-----------------|-------------|
| BGM_Main | 1 | 크로스페이드로 교체 |
| BGM_Weather | 1 | 크로스페이드로 교체 |
| SFX_Player | 4 | 가장 오래된 것 중단 |
| SFX_World | 8 | 가장 작은 볼륨 것 중단 |
| SFX_Jingle | 2 | 우선순위 낮은 것 중단 |
| AMB_Weather | 2 | 크로스페이드 교체 |
| AMB_Nature | 4 | 가장 작은 볼륨 것 중단 |
| UI_Click | 2 | 이전 것 즉시 중단 |
| UI_Notify | 3 | 가장 오래된 것 중단 |

### 3.6 오디오 파일 사양

| 항목 | 사양 |
|------|------|
| 포맷 | BGM: `.ogg` (Vorbis), SFX/UI: `.wav` |
| 샘플레이트 | 44,100 Hz |
| 비트 뎁스 | 16-bit |
| 채널 | BGM: Stereo, SFX: Mono (3D 대상), UI: Mono |
| BGM 파일 크기 목표 | 트랙당 1~3 MB (Vorbis quality 5~6) |
| SFX 파일 크기 목표 | 개당 10~100 KB |

---

## 4. 구현 우선순위

### 4.1 MVP (Phase 2 -- 코딩 시작 시 필수)

핵심 게임 루프와 플레이어 피드백에 직결되는 최소 사운드 세트.

| 카테고리 | 포함 SFX ID | 수량 |
|---------|-------------|------|
| 경작 (기본 등급만) | `sfx_hoe_basic`, `sfx_seed_plant`, `sfx_water_basic`, `sfx_scythe_basic`, `sfx_harvest`, `sfx_fertilize` | 6 |
| 작물 | `sfx_crop_grow`, `sfx_crop_wither` | 2 |
| 도구 | `sfx_tool_equip` | 1 |
| UI (핵심) | `sfx_ui_click`, `sfx_inventory_open`, `sfx_inventory_close`, `sfx_notification`, `sfx_error`, `sfx_confirm` | 6 |
| 상점/NPC | `sfx_shop_open`, `sfx_shop_close`, `sfx_purchase`, `sfx_sell` | 4 |
| 진행 | `sfx_level_up`, `sfx_xp_gain`, `sfx_gold_gain`, `sfx_gold_spend` | 4 |
| 시간 | `sfx_morning_chime`, `sfx_sleep` | 2 |
| 환경 | `sfx_footstep_dirt`, `sfx_footstep_grass` | 2 |
| BGM | `bgm_spring` (1 계절 BGM으로 시작) | 1 |
| **MVP 합계** | | **28** |

### 4.2 Phase 3 -- 시스템 확장기

시스템별 확장에 맞춰 추가되는 사운드.

| 카테고리 | 포함 SFX ID | 수량 |
|---------|-------------|------|
| 경작 (강화/전설 등급) | `sfx_hoe_reinforced`, `sfx_hoe_legendary`, `sfx_water_reinforced`, `sfx_water_legendary`, `sfx_scythe_reinforced`, `sfx_scythe_legendary` | 6 |
| 도구 | `sfx_tool_upgrade_start`, `sfx_tool_upgrade_complete`, `sfx_tool_break` | 3 |
| 시설/건설 | `sfx_construct_start`, `sfx_construct_complete`, `sfx_construct_upgrade`, `sfx_facility_activate`, `sfx_facility_idle` | 5 |
| 가공 | `sfx_process_start`, `sfx_process_complete`, `sfx_mill_running`, `sfx_ferment_bubble`, `sfx_bakery_oven`, `sfx_cheese_churn` | 6 |
| 목축 | 전 항목 (`sfx_animal_feed` ~ `sfx_animal_sick`) | 10 |
| 낚시 | 전 항목 (`sfx_cast_line` ~ `sfx_fish_splash`) | 9 |
| 퀘스트/업적 | 전 항목 (`sfx_quest_accept` ~ `sfx_achievement_toast`) | 5 |
| BGM | `bgm_summer`, `bgm_autumn`, `bgm_winter` (나머지 계절) | 3 |
| 환경 | `amb_rain_light`, `amb_rain_heavy`, `amb_snow`, `amb_wind_light`, `amb_birds_day` | 5 |
| 시간 | `sfx_evening_bell`, `sfx_midnight_warning`, `sfx_pass_out`, `sfx_season_transition` | 4 |
| **Phase 3 합계** | | **56** |

### 4.3 Phase 4~5 -- 폴리싱

게임 경험을 풍부하게 만드는 보조 사운드.

| 카테고리 | 포함 SFX ID | 수량 |
|---------|-------------|------|
| 작물 (특수) | `sfx_crop_golden`, `sfx_crop_giant` | 2 |
| NPC | `sfx_dialog_start`, `sfx_dialog_advance`, `sfx_dialog_choice`, `sfx_affinity_up`, `sfx_shipping_bin` | 5 |
| UI (보조) | `sfx_ui_hover`, `sfx_ui_tab`, `sfx_item_move`, `sfx_cancel` | 4 |
| 환경 (고급) | `amb_storm`, `sfx_thunder`, `sfx_lightning_flash`, `amb_blizzard`, `amb_cicada`, `amb_cricket`, `amb_insects_night`, `amb_waves` | 8 |
| 발걸음 (추가) | `sfx_footstep_wood`, `sfx_footstep_snow`, `sfx_footstep_stone` | 3 |
| 진행 (보조) | `sfx_energy_warning`, `sfx_energy_depleted`, `sfx_day_summary` | 3 |
| BGM (특수) | `bgm_rain`, `bgm_storm`, `bgm_blizzard`, `bgm_night`, `bgm_indoor_home`, `bgm_indoor_shop`, `bgm_festival` | 7 |
| Variation 파일 | 발걸음 x3종, 괭이질 x3종, 물주기 x3종, 낫질 x3종 (각 2추가 variation) | 24 |
| **Phase 4~5 합계** | | **56** |

### 4.4 전체 요약

| Phase | 사운드 수 | 누적 | 비고 |
|-------|----------|------|------|
| MVP (Phase 2) | 28 | 28 | 핵심 루프 피드백 |
| Phase 3 | 56 | 84 | 시스템별 확장 |
| Phase 4~5 | 56 | 140 | 폴리싱, variation |

---

## Cross-references

| 문서 | 관계 |
|------|------|
| `docs/systems/sound-architecture.md` | 본 문서의 기술 구현 아키텍처 (SoundManager, BGMScheduler, SFXPool, SO 설계) |
| `docs/design.md` 섹션 8 | 사운드 개요 (본 문서가 상세 확장) |
| `docs/systems/time-season.md` 섹션 1.2, 2.3, 3.1 | 시간대, 계절 환경 사운드, 날씨 종류 정의 |
| `docs/systems/farming-system.md` | 경작 도구 메카닉, 에너지 시스템 |
| `docs/systems/tool-upgrade.md` 섹션 1.1 | 도구 등급 정의 (Basic/Reinforced/Legendary) |
| `docs/systems/crop-growth.md` | 작물 성장 단계, 품질 등급, Giant Crop |
| `docs/systems/fishing-system.md` | 낚시 미니게임 흐름, 어종 희귀도 |
| `docs/systems/economy-system.md` | 골드 획득/지출 이벤트 |
| `docs/systems/quest-system.md` | 퀘스트 수락/완료 이벤트 |
| `docs/systems/achievement-system.md` | 업적 달성 이벤트, 토스트 UI |
| `docs/systems/ui-system.md` | UI 인터랙션 이벤트, 패널 열기/닫기 |
| `docs/content/livestock-system.md` | 동물 종류, 생산물 수집 인터랙션 |
| `docs/content/processing-system.md` | 가공소별 가공 흐름 |
| `docs/content/npcs.md` | NPC 목록, 상점 운영 |
| `docs/content/facilities.md` | 시설 건설/업그레이드 이벤트 |
| `docs/systems/visual-guide.md` | 시각 이펙트와 사운드 동기화 대상 |

---

## Open Questions

1. [OPEN] BGM 제작 방식: 직접 작곡 vs AI 생성 음악(Suno, Udio 등) vs 로열티 프리 라이브러리 활용? 라이선스 이슈와 품질 수준에 따라 결정 필요.

2. [OPEN] SFX 제작 방식: 무료 SFX 라이브러리(Freesound, Sonniss) 기반 편집 vs AI SFX 생성 도구(ElevenLabs 등)? MVP에서는 무료 라이브러리 기반이 현실적.

3. [OPEN] NPC 보이스: 텍스트 대화에 짧은 "뭉게뭉게" 스타일 보이스(Animal Crossing 방식)를 추가할지? 추가한다면 NPC별 피치/톤 차별화 필요. 스코프 확대 우려.

4. [OPEN] 실내/실외 리버브 전환: 실내 진입 시 AudioMixer Snapshot으로 리버브 추가할지? 쿼터뷰 카메라 특성상 실내/실외 전환이 빈번하지 않을 수 있으므로 비용 대비 효과 검토 필요.

5. [OPEN] 계절 BGM 수: 현재 계절당 1트랙인데, 플레이 시간이 길어지면 피로할 수 있음. 계절당 2~3 variation을 둘지, 아니면 적응형(Adaptive) BGM으로 레이어를 추가/제거하는 방식을 쓸지 검토 필요.

---

## Risks

1. [RISK] **사운드 에셋 수량**: 총 140+개의 사운드 에셋 제작/조달이 필요하며, AI 자율 제작 프로젝트 특성상 사운드 에셋 파이프라인이 가장 병목이 될 가능성이 높다. MVP 28개에 집중하여 단계적으로 확장하는 전략 필수.

2. [RISK] **BGM 루프 품질**: 짧은 루프(35~90초)는 반복 재생 시 이음매가 드러나기 쉽다. 루프 포인트 설계와 크로스페이드 처리에 세심한 작업 필요.

3. [RISK] **동시 재생 성능**: 환경음 루프 + BGM + 플레이어 SFX + 시설 가동음이 동시에 재생되는 상황에서 AudioSource 풀 관리와 CPU 부하 모니터링 필요. 특히 시설이 많아지는 후반부 주의.

4. [RISK] **3D 오디오와 쿼터뷰 충돌**: 쿼터뷰 카메라는 리스너 위치가 플레이어 위 멀리에 있으므로, 3D 거리 감쇠 파라미터를 실제 카메라 높이 기준이 아닌 XZ 평면 거리 기준으로 조정해야 자연스럽다. AudioListener 배치 전략 필요.
