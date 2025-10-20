using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 칩 묶음을 관리하는 클래스
/// Dictionary<ChipType, int>를 래핑하여 편리한 연산 제공
/// </summary>
[System.Serializable]
public class ChipCollection
{
    // === 내부 데이터 ===
    private Dictionary<ChipType, int> chips;

    // === 생성자 ===
    public ChipCollection()
    {
        chips = new Dictionary<ChipType, int>();
        // 모든 ChipType을 0으로 초기화
        foreach (ChipType type in ChipTypeCache.AllTypes)
        {
            chips[type] = 0;
        }
    }

    public ChipCollection(ChipType chipType, int count)
    {
        chips = new Dictionary<ChipType, int>();
        foreach (ChipType type in ChipTypeCache.AllTypes)
        {
            chips[type] = (type == chipType) ? count : 0;
        }
    }

    public ChipCollection(Dictionary<ChipType, int> source)
    {
        chips = new Dictionary<ChipType, int>();
        foreach (ChipType type in ChipTypeCache.AllTypes)
        {
            chips[type] = source.ContainsKey(type) ? source[type] : 0;
        }
    }

    // === 접근자 ===
    /// <summary>
    /// 특정 ChipType의 개수를 가져오거나 설정
    /// </summary>
    public int this[ChipType chipType]
    {
        get => chips.ContainsKey(chipType) ? chips[chipType] : 0;
        set
        {
            if (value < 0)
            {
                throw new ArgumentException($"칩 개수는 음수가 될 수 없습니다: {chipType} = {value}");
            }
            chips[chipType] = value;
        }
    }

    // === 계산 메서드 ===
    /// <summary>
    /// 총 칩 개수 (모든 타입 합계)
    /// </summary>
    public int GetTotalCount()
    {
        int total = 0;
        foreach (var pair in chips)
        {
            total += pair.Value;
        }
        return total;
    }

    /// <summary>
    /// 총 칩 금액 ($)
    /// </summary>
    public int GetTotalValue()
    {
        int total = 0;
        foreach (var pair in chips)
        {
            total += pair.Value * ChipTypeCache.Values[pair.Key];
        }
        return total;
    }

    /// <summary>
    /// 특정 ChipType이 충분한지 확인
    /// </summary>
    public bool HasChip(ChipType chipType, int count)
    {
        return this[chipType] >= count;
    }

    /// <summary>
    /// 다른 ChipCollection과 비교하여 모든 타입이 충분한지 확인
    /// </summary>
    public bool HasEnough(ChipCollection other)
    {
        foreach (ChipType type in ChipTypeCache.AllTypes)
        {
            if (this[type] < other[type])
                return false;
        }
        return true;
    }

    // === 수정 메서드 ===
    /// <summary>
    /// 칩 추가
    /// </summary>
    public void AddChip(ChipType chipType, int count)
    {
        if (count < 0)
        {
            throw new ArgumentException($"추가할 칩 개수는 음수가 될 수 없습니다: {count}");
        }
        this[chipType] += count;
    }

    /// <summary>
    /// 칩 제거 (부족하면 false 반환)
    /// </summary>
    public bool RemoveChip(ChipType chipType, int count)
    {
        if (count < 0)
        {
            throw new ArgumentException($"제거할 칩 개수는 음수가 될 수 없습니다: {count}");
        }

        if (!HasChip(chipType, count))
            return false;

        this[chipType] -= count;
        return true;
    }

    /// <summary>
    /// 모든 칩 초기화
    /// </summary>
    public void Clear()
    {
        foreach (ChipType type in ChipTypeCache.AllTypes)
        {
            chips[type] = 0;
        }
    }

    // === 유틸리티 ===
    /// <summary>
    /// 빈 칩인지 확인 (모든 타입이 0)
    /// </summary>
    public bool IsEmpty()
    {
        return GetTotalCount() == 0;
    }
    
    /// <summary>
    /// 칩 컬렉션 복사
    /// </summary>
    public ChipCollection Clone()
    {
        ChipCollection clone = new ChipCollection();
        foreach (var pair in chips)
        {
            clone.chips[pair.Key] = pair.Value;
        }
        return clone;
    }

    // === ToString ===
    public override string ToString()
    {
        string result = "";
        foreach (var pair in chips)
        {
            result += $"{pair.Key}x{pair.Value} ";
        }
        return result.TrimEnd() + $" (총 ${GetTotalValue()})";
    }
}

