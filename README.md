# 🎰 Fortunae - 룰렛 게임

**Unity 기반 3D 룰렛 배팅 게임**

## 📋 프로젝트 개요

Fortunae는 Unity로 개발된 전략적 룰렛 배팅 게임입니다. 플레이어는 3턴 동안 다양한 아이템과 전략을 활용하여 최대한 많은 칩을 획득하는 것이 목표입니다.

### 🎮 주요 기능

- **3턴 기반 게임**: 턴마다 전략적인 배팅 결정
- **다양한 배팅 타입**: Number, Color, Dozen, Column, OddEven, HighLow
- **아이템 시스템**: 게임을 변화시키는 특수 아이템
- **물리 기반 룰렛**: 실제감 있는 공 물리 시뮬레이션
- **다중 배팅**: 한 턴에 여러 곳에 배팅 가능
- **실시간 확률 계산**: 아이템 효과가 적용된 동적 확률 시스템

---

## 🏗️ 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── Game/
│   │   ├── Game.cs                    # 메인 게임 로직 (Presenter + Controller)
│   │   ├── Board.cs                   # 3D 오브젝트 관리
│   │   ├── SpotObject.cs              # 개별 스팟 (1-36)
│   │   ├── BetObject.cs               # 배팅 오브젝트 기본 클래스
│   │   ├── WheelController.cs         # 룰렛 휠 제어
│   │   ├── BallController.cs          # 공 물리 제어
│   │   │
│   │   ├── Data/
│   │   │   ├── GameState.cs           # 전체 게임 상태
│   │   │   ├── BetData.cs             # 배팅 데이터
│   │   │   ├── PlayerInventory.cs     # 플레이어 인벤토리
│   │   │   ├── TurnData.cs            # 턴 기록
│   │   │   ├── ItemData.cs            # 아이템 데이터
│   │   │   ├── AppliedItemRecord.cs   # 적용된 아이템 기록
│   │   │   ├── ConstDef.cs            # 게임 상수 정의
│   │   │   └── Messages/              # 통신용 메시지 구조체
│   │   │       ├── BetConfirmMessage.cs
│   │   │       ├── ChipSelectionMessage.cs
│   │   │       ├── SpinResultMessage.cs
│   │   │       └── TooltipMessage.cs
│   │   │
│   │   ├── Model/
│   │   │   └── Spot.cs                # 스팟 모델
│   │   │
│   │   ├── System/
│   │   │   ├── ItemManager.cs         # 아이템 관리
│   │   │   ├── SpotCalculator.cs      # 확률/배당 계산
│   │   │   └── BetTypeHelper.cs       # 배팅 타입 유틸리티
│   │   │
│   │   ├── UI/
│   │   │   ├── GameUI.cs              # 메인 게임 UI
│   │   │   ├── ChipSelectionPopup.cs  # 칩 선택 팝업
│   │   │   ├── SpotInfoPopup.cs       # 스팟 정보 팝업
│   │   │   ├── BetItemUI.cs           # 배팅 아이템 UI
│   │   │   └── SpotItemUI.cs          # 스팟 아이템 UI
│   │   │
│   │   ├── Physics/
│   │   │   └── States/                # 공 물리 상태 머신
│   │   │
│   │   ├── ScritableObject/
│   │   │   ├── SpotSO.cs              # 스팟 정의
│   │   │   └── ItemTable.cs           # 아이템 테이블
│   │   │
│   │   └── Def/
│   │       └── DefEnum.cs             # 열거형 정의
│   │
│   └── UI/                             # 공통 UI 시스템
│
├── Resources/
│   ├── Sprite/                         # 게임 스프라이트
│   └── UI/                             # UI 프리팹
│
├── RouletteData/
│   ├── RouletteDefinition.asset        # 룰렛 정의
│   └── SpotBaseDefinitions/            # 스팟 기본 정의
│
└── Scenes/
    └── GameScene.unity                 # 메인 게임 씬
```

---

## 🎯 게임 규칙

### 기본 규칙

1. **시작 칩**: Chip1 x5 ($5)
2. **최대 턴**: 3턴
3. **목표**: 3턴 동안 최대한 많은 칩 획득

### 배팅 타입

| 타입 | 설명 | 기본 배당 |
|------|------|-----------|
| **Number** | 특정 숫자 (1-36) | 36배 |
| **Color** | Red/Black | 2배 |
| **Dozen** | First(1-12)/Second(13-24)/Third(25-36) | 3배 |
| **Column** | Column1/Column2/Column3 | 3배 |
| **OddEven** | Odd/Even | 2배 |
| **HighLow** | Low(1-18)/High(19-36) | 2배 |

---

## 🎁 아이템 시스템

### 스팟 아이템 (SpotItem)

| 아이템 | 효과 | 사용법 |
|--------|------|--------|
| **PLUS_SPOT** | 스팟의 숫자를 +1 증가 | Number 배팅 시 선택 가능 |
| **COPY_SPOT** | 다른 스팟의 설정을 복사 | 원본 선택 → 대상 선택 |
| **UPGRADED_MULTI_SPOT** | 배당률 2배 증가 | Number 배팅 시 선택 가능 |

### 참 아이템 (CharmItem)

| 아이템 | 효과 | 특징 |
|--------|------|------|
| **DEATH_CHARM** | 특정 숫자 배제 | 패시브 (소지 시 자동 적용) |
| **CHAMELEON_CHARM** | 숫자 변경 시 배당 2배 | 패시브 (PlusSpot 사용 시 발동) |

### 특수 아이템

| 아이템 | 효과 | 사용법 |
|--------|------|--------|
| **HAT_WING** | 패배 시에도 50% 배당 지급 | 배팅 전 토글 활성화 |

---

## 🔧 핵심 시스템

### 1. MVP 패턴 기반 아키텍처

```
Game.cs (Model + Presenter)
    ↕ GB.Presenter.Send/Bind
GameUI.cs (View)
```

- **단방향 데이터 흐름**: Model → Presenter → View
- **메시지 기반 통신**: `GB.Presenter.Send("GameUI", Keys.UPDATE_XXX, data)`

### 2. 확률 계산 시스템 (`SpotCalculator.cs`)

```csharp
// 동적 확률 계산
public static void RecalculateAll(GameState gameState)
{
    // 1. 숫자별 확률 계산 (PlusSpot, DeathCharm 고려)
    CalculateNumberProbabilities(gameState);
    
    // 2. 각 스팟의 확률/배당 계산 (Chameleon, UpgradedMultiSpot 고려)
    foreach (var spot in gameState.spots.Values)
    {
        CalculateSpotProbability(spot, gameState);
        CalculateSpotPayout(spot, gameState);
    }
}
```

**주요 기능**:
- 아이템 효과를 반영한 실시간 확률 계산
- 가중치 기반 확률 분배
- 배당률과 확률 분리 관리

### 3. 아이템 관리 시스템 (`ItemManager.cs`)

```csharp
// 아이템 사용
public bool UseItem(ItemData item, GameState gameState, int[] targetSpotIDs)
{
    switch (item.itemType)
    {
        case ItemType.SpotItem:
            return UseSpotItem(item, gameState, targetSpotIDs);
        case ItemType.CharmItem:
            return true; // 패시브 아이템
    }
}
```

### 4. 칩 시스템 (`ChipCollection.cs`)

```csharp
// 칩 타입: Chip1($1), Chip5($5), Chip25($25), Chip100($100), Chip500($500)
public class ChipCollection
{
    // 자동 교환 (큰 칩으로)
    public void ExchangeToLarger()
    
    // 칩 제거 (작은 칩으로 교환)
    public bool TryRemove(ChipType type, int count)
    
    // 총 가치 계산
    public double GetTotalValue()
}
```

### 5. 물리 기반 룰렛

- **BallController**: 공 물리 시뮬레이션
- **State Machine**: Rolling → Settling → Stopped
- **WheelController**: 휠 회전 제어

---

## 🎨 UI 시스템

### 팝업 시스템

1. **ChipSelectionPopup**: 칩/아이템 선택
2. **SpotInfoPopup**: 전체 스팟 정보 (확률, 배당, 적용 아이템)
3. **GameUI**: 메인 게임 화면

### 상태 관리

```csharp
// 전역 상태 플래그
Game.isClickLocked    // 클릭 잠금 (스핀 중, 게임 종료 시)
Game.isPopupOpen      // 팝업 열림 상태
Game.isCopySpotMode   // CopySpot 선택 모드
```

---

## 🚀 시작하기

### 필요 사항

- **Unity 2022.3 LTS** 이상
- **DOTween** (Pro 권장)
- **TextMeshPro**

### 설치 및 실행

1. 프로젝트 클론
```bash
git clone [repository-url]
```

2. Unity Hub에서 프로젝트 열기

3. 패키지 설치 (Package Manager)
   - TextMesh Pro
   - DOTween (via Asset Store)

4. `Scenes/GameScene.unity` 열기

5. Play 버튼 클릭!

---

## 🔑 주요 클래스 설명

### `Game.cs`
메인 게임 로직을 담당하는 Presenter + Controller 역할.

**주요 메서드**:
- `InitializeGame()`: 게임 초기화
- `StartSpin()`: 스핀 시작
- `PlaceBet()`: 배팅 배치
- `UseItemByID()`: 아이템 사용

### `GameState.cs`
전체 게임 상태를 관리하는 Model.

**주요 필드**:
- `availableChips`: 현재 보유 칩
- `currentBets`: 현재 턴 배팅 목록
- `inventory`: 아이템 인벤토리
- `spots`: 스팟 데이터 (1-36)
- `turnHistory`: 턴 기록

### `SpotCalculator.cs`
확률 및 배당을 계산하는 정적 유틸리티 클래스.

**주요 메서드**:
- `RecalculateAll()`: 전체 재계산
- `CalculateNumberProbabilities()`: 숫자별 확률 계산
- `CalculateSpotProbability()`: 스팟 확률 계산
- `CalculateSpotPayout()`: 스팟 배당 계산

### `ItemManager.cs`
아이템 생성 및 사용을 관리.

**주요 메서드**:
- `CreateItem()`: 아이템 생성
- `UseItem()`: 아이템 사용
- `UseSpotItem()`: 스팟 아이템 적용

### `BetTypeHelper.cs`
배팅 타입 관련 유틸리티 메서드 모음.

**주요 메서드**:
- `GetPopupTitle()`: 팝업 제목 생성
- `GetBetTypeName()`: 배팅 타입 이름
- `GetBetDisplayName()`: 배팅 표시 이름

---

## 📊 데이터 흐름

### 배팅 프로세스

```
1. SpotObject/BetObject 클릭
   ↓
2. CMD_SPOT_CLICKED / CMD_BET_OBJECT_CLICKED
   ↓
3. ChipSelectionPopup 표시
   ↓
4. 칩 선택 + 아이템 선택 (선택)
   ↓
5. BetConfirmMessage 생성
   ↓
6. Game.PlaceBet()
   ↓
7. UPDATE_BET_LIST → GameUI 갱신
```

### 스핀 프로세스

```
1. Spin 버튼 클릭
   ↓
2. CMD_START_SPIN
   ↓
3. Game.StartSpin()
   ↓
4. 당첨 번호 미리 결정
   ↓
5. WheelController.StartSpin()
   ↓
6. 물리 시뮬레이션 (Ball + Wheel)
   ↓
7. OnSpinComplete()
   ↓
8. 배당 계산 + 칩 지급
   ↓
9. SpinResultMessage → GameUI
   ↓
10. Win/Lose 효과 + 결과 표시
```

---

## 🎓 코드 컨벤션

### 필드 구성

```csharp
public class ExampleClass
{
    // === 데이터 필드 ===
    private GameState gameState;
    
    // === 상태 필드 ===
    private bool isActive;
    
    // === UI 필드 ===
    [SerializeField] private Button confirmButton;
}
```

### 상수 정의

모든 매직 넘버/문자열은 `ConstDef.cs`에 정의:

```csharp
public static class ConstDef
{
    // 아이템 ID
    public const string PLUS_SPOT = "PLUS_SPOT";
    
    // 디버그 메시지
    public const string GAME_AWAKE_CALLED = "[Game] Awake called";
}
```

### 메시지 구조체

값 타입으로 정의하여 Boxing/Unboxing 방지:

```csharp
public struct BetConfirmMessage
{
    public readonly BetType betType;
    public readonly int targetValue;
    public readonly ChipCollection chips;
    // ...
}
```

---

## 🐛 디버깅

### 유용한 디버그 정보

```csharp
// 스팟 정보 확인
SpotInfoPopup을 열어 모든 스팟의 확률/배당 확인

// 배팅 내역 확인
GameUI의 BettingPanel에서 실시간 배팅 목록 확인

// 인벤토리 확인
ChipSelectionPopup에서 아이템 개수 확인
```

---

## 📈 향후 개선 방향

- [ ] 세이브/로드 시스템
- [ ] 리더보드
- [ ] 더 많은 아이템 추가
- [ ] 멀티플레이어 모드
- [ ] 사운드/이펙트 강화
- [ ] 튜토리얼 추가

---

## 📝 라이선스

이 프로젝트는 개인 학습 목적으로 제작되었습니다.

---

## 👥 기여

버그 리포트 및 기능 제안은 이슈로 등록해주세요!

---

## 📧 연락처

프로젝트 관련 문의: [sksqwer123@gmail.com, 010-2382-7100]

---

<div align="center">

**Made with ❤️ using Unity**

</div>

