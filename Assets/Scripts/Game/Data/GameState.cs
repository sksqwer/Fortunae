using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 게임 상태 관리 (3턴 전체)
/// </summary>
public class GameState
{
    // === 플레이어 상태 ===
    public ChipCollection availableChips;  // 현재 사용 가능한 칩 (턴마다 5개로 리셋, 총 금액 계산 가능)
    public int currentTurn;                // 1~3
    public List<ItemData> inventory;       // 보유 아이템 (Charm 포함)
    
    // === 스팟 상태 (1~36) ===
    public Dictionary<int, Spot> spots;
    
    // === 턴 기록 ===
    public List<TurnData> turnHistory;
    
    public GameState(SpotSO spotDefinition)
    {
        availableChips = new ChipCollection(ChipType.Chip1, 5);
        currentTurn = 1;
        inventory = new List<ItemData>();
        spots = new Dictionary<int, Spot>();
        turnHistory = new List<TurnData>();
        
        // Charm은 외부(Game)에서 추가됨
        
        // 36개 스팟 초기화 (SpotBase 기반)
        for (int i = 0; i < spotDefinition.spotBaseList.Count; i++)
        {
            SpotBase spotBase = spotDefinition.spotBaseList[i];
            spots[spotBase.id] = new Spot(spotBase);
        }
    }
    
    // 턴 시작 (칩 리셋)
    public void StartNewTurn()
    {
        availableChips.Clear();
        availableChips.AddChip(ChipType.Chip1, 5);
        currentTurn++;
    }
    
    // 턴 시작 시 스팟 리셋 (Active 아이템만)
    public void ResetSpotsForNewTurn()
    {
        foreach (var spot in spots.Values)
        {
            spot.ResetForNewTurn();
        }
    }
    
    // 새 게임 시작 시 완전 리셋
    public void ResetAllSpots()
    {
        foreach (var spot in spots.Values)
        {
            spot.ResetAll();
        }
    }
    
    // Charm 활성 여부 (inventory에서 확인)
    public bool HasDeathCharm() => inventory.Exists(i => i.charmType == CharmType.Death);
    public bool HasChameleonCharm() => inventory.Exists(i => i.charmType == CharmType.Chameleon);
    
    // Charm 아이템 가져오기
    public ItemData GetDeathCharm() => inventory.Find(i => i.charmType == CharmType.Death);
    public ItemData GetChameleonCharm() => inventory.Find(i => i.charmType == CharmType.Chameleon);
    
    // 총 보유 금액 계산 (ChipCollection에서)
    public double GetTotalMoney() => availableChips.GetTotalValue();
    
    // 아이템 추가
    public void AddItem(ItemData item)
    {
        // 같은 아이템이 있으면 개수만 증가
        var existing = inventory.Find(i => i.itemID == item.itemID);
        if (existing != null)
        {
            existing.count += item.count;
        }
        else
        {
            inventory.Add(item);
        }
    }
    
    
    // 아이템 사용 (개수 감소)
    public bool UseItemFromInventory(string itemID)
    {
        var item = inventory.Find(i => i.itemID == itemID);
        if (item != null && item.count > 0)
        {
            item.count--;
            if (item.count <= 0)
            {
                inventory.Remove(item);
            }
            return true;
        }
        return false;
    }
}

