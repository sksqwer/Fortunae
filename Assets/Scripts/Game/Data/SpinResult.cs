/// <summary>
/// 스핀 결과 데이터
/// </summary>
[System.Serializable]
public class SpinResult
{
    public int winningSpotID;
    public int winningNumber;
    public double totalPayout;
    public bool hasWon;
    
    public SpinResult(int spotID, int number, double payout)
    {
        winningSpotID = spotID;
        winningNumber = number;
        totalPayout = payout;
        hasWon = payout > 0;
    }
}

