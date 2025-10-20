using UnityEngine;

/// <summary>
/// 아이템 생성 및 관리 전담 (ID 기반)
/// </summary>
public class ItemManager
{
    private ItemTable itemTable;
    
    public ItemManager(ItemTable itemTable)
    {
        this.itemTable = itemTable;
    }
    
    /// <summary>
    /// ID로 아이템 생성 (유일한 생성 메서드)
    /// </summary>
    public ItemData CreateItem(string itemID, int count = -1)
    {
        if (itemTable == null)
        {
            Debug.LogError("[ItemManager] ItemTable is null!");
            return null;
        }
        
        var itemDef = itemTable.GetItemById(itemID);
        if (itemDef == null)
        {
            Debug.LogError($"[ItemManager] Item with ID '{itemID}' not found!");
            return null;
        }
        
        return new ItemData(itemDef, count);
    }
}

