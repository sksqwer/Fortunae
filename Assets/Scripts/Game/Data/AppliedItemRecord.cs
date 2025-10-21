using System.Collections.Generic;
/// <summary>
/// 아이템 사용 내역을 기록하는 클래스
/// </summary>
public class AppliedItemRecord
{
    public ItemType itemType;
    public SpotItemType spotItemType;
    public CharmType charmType;
    public int appliedValue;
    public int appliedNumber;
    public SpotColor appliedColor;
    public double multiplierValue;
    
    // 생성자
    public AppliedItemRecord(ItemData itemData, int currentValue, int appliedValue, 
        int appliedNumber, SpotColor appliedColor, double multiplierValue)
    {
        itemType = itemData.itemType;
        spotItemType = itemData.spotItemType;
        charmType = itemData.charmType;
        
        this.appliedValue = appliedValue;
        this.appliedNumber = appliedNumber;
        this.appliedColor = appliedColor;
        this.multiplierValue = multiplierValue;
    }
}

