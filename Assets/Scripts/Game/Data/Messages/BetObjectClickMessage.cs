/// <summary>
/// BetObject 클릭 메시지 (통신용)
/// </summary>
public struct BetObjectClickMessage
{
    public readonly int objectID;
    public readonly BetType betType;
    public readonly int targetValue;
    
    public BetObjectClickMessage(int id, BetType type, int value)
    {
        objectID = id;
        betType = type;
        targetValue = value;
    }
}

