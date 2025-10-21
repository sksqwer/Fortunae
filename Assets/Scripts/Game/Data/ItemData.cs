using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 아이템 데이터 (범용)
/// </summary>
public class ItemData
{
    public ItemType itemType;           // Spot/Chip/Charm
    public string itemID;                // 고유 ID
    public int count;                    // 보유 개수
    public double multiplier;            // 배수 값 (ItemDefinition에서 가져옴)
    public int SpriteIndex;             // 아이템 아이콘 인덱스
    
    // === Spot 아이템 전용 ===
    public SpotItemType spotItemType;
    
    // === Chip 아이템 전용 ===
    public ChipItemType chipItemType;
    
    // === Charm 아이템 전용 ===
    public CharmType charmType;
    
    // 생성자 - ItemDefinition에서 생성 (유일한 생성자)
    public ItemData(ItemDefinition definition, int count = -1)
    {
        if (definition == null)
        {
            Debug.LogError("[ItemData] ItemDefinition is null!");
            return;
        }
        
        itemType = definition.itemType;
        itemID = definition.itemID;
        this.count = count >= 0 ? count : definition.baseCount;
        
        multiplier = definition.multiplier; // ItemDefinition에서 배수 값 가져오기
        SpriteIndex = definition.SpriteIndex;
        
        // 아이템 타입별 세부 설정
        switch (itemType)
        {
            case ItemType.SpotItem:
                spotItemType = definition.spotItemType;
                break;
            case ItemType.ChipItem:
                chipItemType = definition.chipItemType;
                break;
            case ItemType.CharmItem:
                charmType = definition.charmType;
                break;
        }
    }
}