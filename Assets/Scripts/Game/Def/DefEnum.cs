using System;
using System.Collections.Generic; // Dictionary
using System.Linq; // GetValues()를 위해 필요
// public enum SpotColor
public enum SpotColor
{
    Red = 0,
    Black
}

// === 아이템 타입 ===
public enum ItemType
{
    SpotItem,   // Spot에 적용되는 아이템
    ChipItem,   // Chip에 적용되는 아이템
    CharmItem   // 항상 보유한 패시브 아이템
}

// === Spot 아이템 ===
public enum SpotItemType
{
    None,               // 없음
    PlusSpot,           // 숫자 +1
    CopySpot,           // 스팟 복사
    UpgradedMultiSpot   // x1.2 (인접 포함)
}

// === Chip 아이템 ===
public enum ChipItemType
{
    None,       // 없음
    HatWing     // [Hat]Wing - 50% 보상 보장
}

// === Charm 아이템 ===
public enum CharmType
{
    None,       // 없음
    Death,      // 4 포함 스팟 파괴
    Chameleon   // 변경 시 x1.3
}

// === 효과 적용 구분 (아이템 내역 관리용) ===
public enum EffectType
{
    Active,   // 턴마다 리셋되는 아이템 효과 (PlusSpot, UpgradedMultiSpot 등)
    Passive   // 게임 내내 유지되는 효과 (Chameleon, Death)
}

// 1. 기본 배팅 유형
public enum BetType
{
    Number,   // (x36) 단일 숫자
    OddEven,  // (x2) 홀수/짝수
    Color,    // (x2) 색상 (Red/Black)
    HighLow,  // (x2) 1-18(Low) / 19-36(High)
    Dozen,    // (x3) 1-12 / 13-24 / 25-36
    Column    // (x3) 1, 2, 3열
}

// 2. "추가 데이터"를 위한 Enum (선택지)

public enum OddEvenChoice
{
    Odd,
    Even
}

public enum HighLowChoice
{
    Low,  // 1-18
    High  // 19-36
}

public enum DozenChoice
{
    First,  // 1-12
    Second, // 13-24
    Third   // 25-36
}

public enum ColumnChoice
{
    First,  // 1, 4, 7, ...
    Second, // 2, 5, 8, ...
    Third   // 3, 6, 9, ...
}


// === 칩 타입 (금액별) ===
public enum ChipType
{
    Chip1 = 1,   // $1
    Chip5 = 5,   // $5
    Chip10 = 10,  // $10
    Chip50 = 50,  // $50

}


/// <summary>
/// ChipType 열거형의 캐시를 저장하는 static 헬퍼 클래스
/// </summary>
public static class ChipTypeCache
{
    // --- 1. 빠른 '조회(Lookup)'를 위한 Dictionary 캐시 ---
    public static readonly Dictionary<ChipType, int> Values;

    // --- 2. 빠른 '반복(Iteration)'을 위한 Array 캐시 ---
    public static readonly ChipType[] AllTypes;

    // --- 3. 개수 캐시 ---
    public static readonly int Count;

    /// <summary>
    /// Static 생성자 (핵심)
    /// 이 코드는 게임이 로드될 때 "단 한 번"만 실행됩니다.
    /// </summary>
    static ChipTypeCache()
    {
        AllTypes = (ChipType[])Enum.GetValues(typeof(ChipType));
        Count = AllTypes.Length;

        Values = new Dictionary<ChipType, int>(Count);

        foreach (ChipType type in AllTypes)
        {
            Values.Add(type, (int)type);
        }
    }
}