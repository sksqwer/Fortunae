using UnityEngine;

/// <summary>
/// 플레이어가 한 턴에 실행한 '단일 베팅' 정보를 저장합니다.
/// GameManager는 이 BetData의 리스트를 들고 있게 됩니다.
/// </summary>
[System.Serializable]
public class BetData
{
    public int spotId;          // 베팅한 Spot의 고유 ID (1~36)
    public int chipAmount;      // 이 Spot에 베팅한 칩의 개수
    public bool isWingApplied;   // 이 베팅에 '[Hat]Wing' 아이템을 사용했는지 여부

    public BetData(int spotId, int chipAmount, bool isWingApplied)
    {
        this.spotId = spotId;
        this.chipAmount = chipAmount;
        this.isWingApplied = isWingApplied;
    }
}