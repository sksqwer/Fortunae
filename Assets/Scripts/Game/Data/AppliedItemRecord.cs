using System.Collections.Generic;

/// <summary>
/// Spot의 특정 시점 상태를 저장하는 스냅샷 (깊은 복사)
/// </summary>
[System.Serializable]
public class SpotSnapshot
{
    public int spotID;
    public int currentNumber;
    public SpotColor currentColor;
    public bool isDestroyed;
    public double currentProbability;
    public double currentPayoutMultiplier;
    public List<AppliedItemRecord> appliedRecords; // 재귀 깊은 복사
    
    /// <summary>
    /// Spot의 현재 상태를 스냅샷으로 저장 (깊은 복사)
    /// </summary>
    public SpotSnapshot(Spot spot)
    {
        spotID = spot.SpotID;
        currentNumber = spot.currentNumber;
        currentColor = spot.currentColor;
        isDestroyed = spot.isDestroyed;
        currentProbability = spot.currentProbability;
        currentPayoutMultiplier = spot.currentPayoutMultiplier;
        
        // AppliedItemRecord도 깊은 복사 (재귀 방지를 위해 얕은 복사)
        appliedRecords = new List<AppliedItemRecord>(spot.appliedRecords);
    }
    
    public string GetDebugInfo()
    {
        return $"Spot {spotID}: Number={currentNumber}, Color={currentColor}, " +
               $"Destroyed={isDestroyed}, Prob={currentProbability:F4}, Payout=x{currentPayoutMultiplier:F2}, " +
               $"Records={appliedRecords.Count}";
    }
}

/// <summary>
/// 아이템 사용 내역을 기록하는 클래스
/// (ItemData는 아이템 "정의", 이건 아이템 "사용 기록")
/// </summary>
public class AppliedItemRecord
{
    // === 기본 정보 ===
    public ItemData itemData;           // 어떤 아이템인지
    public EffectType effectType;       // Active/Passive
    
    // === 공통 ===
    public int targetSpotID;            // 대상 스팟 (모든 아이템 공통)
    
    // === PlusSpot 전용 ===
    public int appliedValue;            // 적용된 값 (PlusSpot: 변경 후 숫자)
    
    // === 배수 효과 (UpgradedMultiSpot, Chameleon 공통) ===
    public double multiplierValue;      // 배수 값 (각 Spot에 개별 적용)
    
    // 통합 생성자
    public AppliedItemRecord(ItemData item, int targetID, int currentValue = 0)
    {
        itemData = item;
        effectType = item.effectType;
        targetSpotID = targetID;
        multiplierValue = item.multiplier; // ItemData에서 배수 값 가져오기
        
        // PlusSpot의 경우 자동으로 appliedValue 계산
        if (item.itemType == ItemType.SpotItem && item.spotItemType == SpotItemType.PlusSpot)
        {
            appliedValue = UnityEngine.Mathf.Min(currentValue + 1, 36);
        }
    }
}

