using System.Collections.Generic;

/// <summary>
/// 단일 턴의 데이터 (배팅, 결과, 스냅샷)
/// </summary>
public class TurnData
{
    public int turnNumber;                        // 1~3
    public List<BetData> bets;                    // 이번 턴 배팅들
    public List<AppliedItemRecord> appliedRecords; // 이번 턴 적용된 아이템 기록
    
    public int winningSpotID;            // 당첨 스팟 ID
    public int winningNumber;            // 당첨 숫자 (effectiveNumber)
    public double totalPayout;           // 총 획득 금액
    
    // === 확률/배당 스냅샷 (로그용) ===
    public Dictionary<int, double> spotProbabilities;      // SpotID → 확률
    public Dictionary<int, double> spotPayoutMultipliers;  // SpotID → 배당률
    
    public TurnData(int turn)
    {
        turnNumber = turn;
        bets = new List<BetData>();
        appliedRecords = new List<AppliedItemRecord>();
        spotProbabilities = new Dictionary<int, double>();
        spotPayoutMultipliers = new Dictionary<int, double>();
    }
    
    // 스냅샷 저장
    public void SaveSpotSnapshot(Dictionary<int, Spot> spots)
    {
        spotProbabilities.Clear();
        spotPayoutMultipliers.Clear();
        
        foreach (var pair in spots)
        {
            if (!pair.Value.isDestroyed)
            {
                spotProbabilities[pair.Key] = pair.Value.currentProbability;
                spotPayoutMultipliers[pair.Key] = pair.Value.currentPayoutMultiplier;
            }
        }
    }
}

