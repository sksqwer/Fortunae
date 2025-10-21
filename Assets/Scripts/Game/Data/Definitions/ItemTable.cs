using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 아이템 테이블 스크립터블 오브젝트
/// 모든 아이템 정의를 관리하는 중앙 집중식 데이터
/// </summary>
[CreateAssetMenu(fileName = "ItemTable", menuName = "Game/Item Table")]
public class ItemTable : ScriptableObject
{
    [Header("아이템 정의 리스트")]
    [SerializeField] private List<ItemDefinition> items = new List<ItemDefinition>();
    
    [Header("테이블 정보")]
    [SerializeField] private string tableName = "Default Item Table";
    [SerializeField] private string version = "1.0.0";
    
    /// <summary>
    /// 모든 아이템 정의
    /// </summary>
    public List<ItemDefinition> Items => items;
    
    /// <summary>
    /// 테이블 이름
    /// </summary>
    public string TableName => tableName;
    
    /// <summary>
    /// 테이블 버전
    /// </summary>
    public string Version => version;
    
    /// <summary>
    /// 아이템 ID로 아이템 정의 찾기
    /// </summary>
    public ItemDefinition GetItemById(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return null;
        
        foreach (var item in items)
        {
            if (item.itemID == itemID)
                return item;
        }
        
        Debug.LogWarning($"[ItemTable] Item with ID '{itemID}' not found!");
        return null;
    }
    
    
    /// <summary>
    /// 특정 Spot 아이템 타입의 아이템들 가져오기
    /// </summary>
    public List<ItemDefinition> GetSpotItemsByType(SpotItemType spotItemType)
    {
        List<ItemDefinition> result = new List<ItemDefinition>();
        
        foreach (var item in items)
        {
            if (item.itemType == ItemType.SpotItem && item.spotItemType == spotItemType)
                result.Add(item);
        }
        
        return result;
    }
    
    /// <summary>
    /// 특정 Chip 아이템 타입의 아이템들 가져오기
    /// </summary>
    public List<ItemDefinition> GetChipItemsByType(ChipItemType chipItemType)
    {
        List<ItemDefinition> result = new List<ItemDefinition>();
        
        foreach (var item in items)
        {
            if (item.itemType == ItemType.ChipItem && item.chipItemType == chipItemType)
                result.Add(item);
        }
        
        return result;
    }
    
    /// <summary>
    /// 특정 Charm 타입의 아이템들 가져오기
    /// </summary>
    public List<ItemDefinition> GetCharmItemsByType(CharmType charmType)
    {
        List<ItemDefinition> result = new List<ItemDefinition>();
        
        foreach (var item in items)
        {
            if (item.itemType == ItemType.CharmItem && item.charmType == charmType)
                result.Add(item);
        }
        
        return result;
    }
    
    
    /// <summary>
    /// 테이블 초기화 (기본 아이템들 추가) - Inspector에서 사용
    /// </summary>
    [ContextMenu("Initialize Default Items")]
    public void InitializeDefaultItems()
    {
        items.Clear();
        
        // Spot 아이템들
        items.Add(CreateItemDef("PLUS_SPOT", "Plus Spot", "숫자 +1", ItemType.SpotItem, SpotItemType.PlusSpot, 1, 99, 1.0f));
        items.Add(CreateItemDef("COPY_SPOT", "Copy Spot", "스팟 복사", ItemType.SpotItem, SpotItemType.CopySpot, 1, 10, 1.0f));
        items.Add(CreateItemDef("UPGRADED_MULTI_SPOT", "Upgraded Multi Spot", "x1.2 (인접 포함)", ItemType.SpotItem, SpotItemType.UpgradedMultiSpot, 1, 5, 1.2f));
        
        // Chip 아이템들
        items.Add(CreateItemDef("HAT_WING", "Hat Wing", "[Hat]Wing - 50% 보상 보장", ItemType.ChipItem, ChipItemType.HatWing, 1, 3, 1.5f));
        
        // Charm 아이템들
        items.Add(CreateItemDef("DEATH_CHARM", "Death Charm", "4 포함 스팟 파괴", ItemType.CharmItem, CharmType.Death, 1, 1, 1.0f));
        items.Add(CreateItemDef("CHAMELEON_CHARM", "Chameleon Charm", "변경 시 x1.3", ItemType.CharmItem, CharmType.Chameleon, 1, 1, 1.3f));
        
        Debug.Log($"[ItemTable] Initialized with {items.Count} default items");
    }
    
    /// <summary>
    /// ItemDefinition 생성 헬퍼
    /// </summary>
    private ItemDefinition CreateItemDef(string id, string name, string desc, ItemType itemType, object subType, int baseCount, int maxCount, float multiplier)
    {
        var def = new ItemDefinition
        {
            itemID = id,
            itemName = name,
            description = desc,
            itemType = itemType,
            baseCount = baseCount,
            maxCount = maxCount,
            multiplier = multiplier,
            isConsumable = itemType != ItemType.CharmItem,
            isStackable = itemType != ItemType.CharmItem
        };
        
        // 서브 타입 설정
        if (subType is SpotItemType spotType)
        {
            def.spotItemType = spotType;
            def.chipItemType = ChipItemType.None;
            def.charmType = CharmType.None;
        }
        else if (subType is ChipItemType chipType)
        {
            def.spotItemType = SpotItemType.None;
            def.chipItemType = chipType;
            def.charmType = CharmType.None;
        }
        else if (subType is CharmType charm)
        {
            def.spotItemType = SpotItemType.None;
            def.chipItemType = ChipItemType.None;
            def.charmType = charm;
        }
        
        return def;
    }
}
