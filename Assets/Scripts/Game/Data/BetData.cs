using UnityEngine;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic; // 1. EventSystems 네임스페이스 추가

/// <summary>
/// 플레이어의 단일 베팅 정보를 저장하는 데이터 모델입니다.
/// </summary>
[System.Serializable]
public class BetData
{
    [Tooltip("베팅의 종류 (숫자, 색깔, 홀짝 등)")]
    public BetType betType;

    [Tooltip("베팅 대상 값 (아래 주석 참고)")]
    public int targetValue;
    // - Number: 1~36
    // - OddEven: 0 (Odd), 1 (Even)
    // - Color: 0 (Red), 1 (Black)
    // - HighLow: 0 (Low), 1 (High)
    // - Dozen: 1, 2, 3
    // - Column: 1, 2, 3

    [Tooltip("베팅한 칩 (타입별 개수)")]
    public ChipCollection chips;

    [Tooltip("이 배팅에 적용된 아이템 (Wing 등)")]
    public List<ItemData> appliedItems;
    
    [Tooltip("HatWing 적용 여부 (반값 배팅)")]
    public bool isHatWingApplied;

    /// <summary>
    /// (참고) 가독성을 위해 베팅 대상을 상수로 정의해둘 수 있습니다.
    /// 예: public const int TARGET_RED = 0;
    /// 예: public const int TARGET_EVEN = 1;
    /// </summary>

    /// <summary>
    /// 배팅 생성자
    /// </summary>
    public BetData(BetType type, int target, bool hatWingApplied = false)
    {
        this.betType = type;
        this.targetValue = target;
        this.chips = new ChipCollection();
        this.appliedItems = new List<ItemData>();
        this.isHatWingApplied = hatWingApplied;
    }
    
    // 칩 추가
    public void AddChip(ChipType chipType, int count)
    {
        chips.AddChip(chipType, count);
    }
    
    // 총 칩 개수
    public int GetTotalChipCount()
    {
        return chips.GetTotalCount();
    }
    
    // 총 칩 금액 ($)
    public int GetTotalChipValue()
    {
        return chips.GetTotalValue();
    }
    
    // Wing 아이템 적용 여부
    public bool isAppliedItem(ChipItemType chipItemType)
    {
        return appliedItems.Exists(item => 
            item.itemType == ItemType.ChipItem && 
            item.chipItemType == chipItemType);
    }
}