using UnityEngine;

/// <summary>
/// 아이템 정의 데이터 클래스 (스크립터블 오브젝트용)
/// </summary>
[System.Serializable]
public class ItemDefinition
{
    [Header("기본 정보")]
    public string itemID;           // 아이템 고유 ID
    public string itemName;         // 아이템 이름
    public string description;      // 아이템 설명
    
    [Header("아이템 타입")]
    public ItemType itemType;       // 아이템 타입
    
    [Header("아이템 세부 타입")]
    public SpotItemType spotItemType;   // Spot 아이템 타입
    public ChipItemType chipItemType;   // Chip 아이템 타입
    public CharmType charmType;         // Charm 타입
    
    [Header("아이템 속성")]
    public int baseCount;           // 기본 개수
    public int maxCount;            // 최대 개수
    public bool isConsumable;       // 소모품 여부
    public bool isStackable;        // 스택 가능 여부
    
    [Header("아이템 효과")]
    public float multiplier;        // 배율 (예: x1.2, x1.3)
    public int addValue;            // 추가 값 (예: +1, +2)
    public int targetCount;         // 대상 개수 (복사, 파괴 등)
    
    [Header("UI 설정")]
    public int SpriteIndex;         // 아이템 아이콘 인덱스
    public Color itemColor;         // 아이템 색상
    
    /// <summary>
    /// 기본 생성자
    /// </summary>
    public ItemDefinition()
    {
        itemID = "";
        itemName = "";
        description = "";
        itemType = ItemType.SpotItem;
        spotItemType = SpotItemType.None;
        chipItemType = ChipItemType.None;
        charmType = CharmType.None;
        baseCount = 1;
        maxCount = 99;
        isConsumable = true;
        isStackable = true;
        multiplier = 1.0f;
        addValue = 0;
        targetCount = 1;
        SpriteIndex = 0;
        itemColor = Color.white;
    }
    
    /// <summary>
    /// 아이템이 유효한지 확인
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(itemID) && !string.IsNullOrEmpty(itemName);
    }
    
    /// <summary>
    /// 아이템 타입별 세부 타입 가져오기
    /// </summary>
    public object GetSubType()
    {
        switch (itemType)
        {
            case ItemType.SpotItem:
                return spotItemType;
            case ItemType.ChipItem:
                return chipItemType;
            case ItemType.CharmItem:
                return charmType;
            default:
                return null;
        }
    }
}
