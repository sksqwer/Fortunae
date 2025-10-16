using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Player
{
    public int money;                 // 총 자산
    public int currentTurn;           // 현재 턴 (1~3)
    public int maxTurn = 3;           // 총 턴 수
    public int totalChips = 5;        // 매 턴 시작 시 보유 칩 수
    public int availableChips;        // 현재 턴에서 남은 칩
    public float totalEarnings;       // 누적 보상 금액

    
    // ▶ 아이템 관리
    // public List<ChipItem> ownedChips; // 칩에 장착 가능한 아이템 (예: [Hat]Wing)
    // public List<Charm> ownedCharms;   // 플레이어가 보유한 Charm 아이템들
    // public List<SpotItem> ownedSpotItems; // Spot 조작용 아이템들 (Plus, Copy 등)
}
