using System.Collections.Generic;

/// <summary>
/// 스핀 결과 메시지 (통신용)
/// </summary>
public struct SpinResultMessage
{
    public readonly int winningSpotID;
    public readonly int winningNumber;
    public readonly SpotColor winningColor;
    public readonly double totalPayout;
    public readonly bool hasWon;
    public readonly List<BetData> allBets; // 모든 배팅들
    public readonly List<BetData> winningBets; // 당첨된 배팅들
    
    public SpinResultMessage(int spotID, int number, SpotColor color, double payout, List<BetData> allBetsList, List<BetData> wonBets = null)
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

