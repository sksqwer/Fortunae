using System.Collections.Generic;

/// <summary>
/// 스핀 결과 데이터
/// </summary>
[System.Serializable]
public class SpinResult
{
    public int winningSpotID;
    public int winningNumber;
    public SpotColor winningColor;
    public double totalPayout;
    public bool hasWon;
    public List<BetData> allBets; // 모든 배팅들
    public List<BetData> winningBets; // 당첨된 배팅들
    
    public SpinResult(int spotID, int number, SpotColor color, double payout, List<BetData> allBetsList, List<BetData> wonBets = null)
    {
        winningSpotID = spotID;
        winningNumber = number;
        winningColor = color;
        totalPayout = payout;
        hasWon = payout > 0;
        allBets = allBetsList ?? new List<BetData>();
        winningBets = wonBets ?? new List<BetData>();
    }
}

