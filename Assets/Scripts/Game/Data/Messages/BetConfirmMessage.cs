using System.Collections.Generic;

/// <summary>
/// 배팅 확정 시 전달되는 메시지 (통신용)
/// </summary>
public struct BetConfirmMessage
{
    public readonly BetType betType;
    public readonly int targetValue;
    public readonly ChipType chipType;
    public readonly int chipCount;
    public readonly List<string> appliedItems; // 적용된 아이템 ID 리스트 (HatWing 등)
    
    public BetConfirmMessage(
        BetType betType, 
        int targetValue, 
        ChipType chipType, 
        int chipCount,
        List<string> appliedItems = null)
    {
        this.betType = betType;
        this.targetValue = targetValue;
        this.chipType = chipType;
        this.chipCount = chipCount;
        this.appliedItems = appliedItems ?? new List<string>();
    }
    
    /// <summary>
    /// HatWing이 적용되었는지 확인
    /// </summary>
    public bool HasHatWing()
    {
        return appliedItems.Contains("HAT_WING");
    }
    
    /// <summary>
    /// 특정 아이템이 적용되었는지 확인
    /// </summary>
    public bool HasItem(string itemID)
    {
        return appliedItems.Contains(itemID);
    }
}

