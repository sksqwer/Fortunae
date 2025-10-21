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

    // === 현재 배팅 정보 ===
    public List<BetData> currentBets;      // 현재 턴의 배팅 리스트

    // === 턴 기록 ===
    public List<TurnData> turnHistory;

    public GameState(SpotSO spotDefinition)
    {
        availableChips = new ChipCollection(ChipType.Chip1, 5);
        currentTurn = 0;
        inventory = new List<ItemData>();
        spots = new Dictionary<int, Spot>();
        currentBets = new List<BetData>();
        turnHistory = new List<TurnData>();

        // Charm은 외부(Game)에서 추가됨

        // 36개 스팟 초기화 (SpotBase 기반)
        for (int i = 0; i < spotDefinition.spotBaseList.Count; i++)
        {
            SpotBase spotBase = spotDefinition.spotBaseList[i];
            spots[spotBase.id] = new Spot(spotBase);
        }
    }

    // 턴 시작 (첫 턴에만 칩 지급, 이후는 유지)
    public void StartNewTurn()
    {
        currentTurn++;

        // 첫 턴에만 초기 칩 지급
        if (currentTurn == 1)
        {
            availableChips.Clear();
            availableChips.AddChip(ChipType.Chip1, 5);
        }
    }

    // 아이템 ID로 아이템 가져오기
    public ItemData GetItemByID(string itemID)
    {
        return inventory.Find(i => i.itemID == itemID);
    }

    // 아이템 ID로 아이템 존재 여부 확인
    public bool HasItem(string itemID)
    {
        var item = GetItemByID(itemID);
        return item != null && item.count > 0;
    }

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

    public string InventoryToString()
    {

        string inventoryText = "Inventory:\n";

        foreach (var item in inventory)
        {
            if (item.count > 0)
            {
                inventoryText += $"• <sprite={item.SpriteIndex}> {item.itemID} : ({item.count})\n";
            }
        }

        return inventoryText;
    }
    
    /// <summary>
    /// 게임 리셋 (전체 게임 상태 초기화)
    /// </summary>
    public void ResetGame()
    {
        // 칩 초기화
        availableChips = new ChipCollection(ChipType.Chip1, 5);
        
        // 턴 초기화
        currentTurn = 0;
        
        // 배팅 초기화
        currentBets.Clear();
        
        // 턴 기록 초기화
        turnHistory.Clear();
        
        // 모든 Spot 초기화
        foreach (var spot in spots.Values)
        {
            spot.currentNumber = spot.SpotID;
            spot.currentColor = spot.OriginalColor;
            spot.isDestroyed = false;
            spot.appliedRecords.Clear();
        }
    }
}

