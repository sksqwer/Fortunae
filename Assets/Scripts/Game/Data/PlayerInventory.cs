using UnityEngine;

/// <summary>
/// 플레이어의 보유 아이템, 참, 누적 점수 등을 관리합니다.
/// </summary>
[System.Serializable]
public class PlayerInventory
{
    [Header("소모성 아이템 (개수)")]
    public int plusSpotItems;
    public int copySpotItems;
    public int upgradedMultiSpotItems;

    [Header("특수 아이템 (보유 여부)")]
    public bool hasWingItem; // '[Hat]Wing' 아이템을 보유했는지

    [Header("참 (Charm) (보유 여부)")]
    public bool hasDeathCharm;
    public bool hasChameleonCharm;

    [Header("플레이어 점수")]
    public float totalWinnings; // 누적 획득 금액

    /// <summary>
    /// 클래스 생성자 (초기화)
    /// </summary>
    public PlayerInventory()
    {
        // 과제 요구사항에 맞춰 초기 아이템 설정
        plusSpotItems = 1; // 예시로 1개씩 지급
        copySpotItems = 1;
        upgradedMultiSpotItems = 1;
        hasWingItem = true;

        // 참(Charm)은 이미 보유한 상태로 시작
        hasDeathCharm = true;
        hasChameleonCharm = true;
        
        totalWinnings = 0;
    }
}