/// <summary>
/// BetType 관련 유틸리티 클래스
/// </summary>
public static class BetTypeHelper
{
    /// <summary>
    /// 배팅 타입과 오브젝트 ID에 따른 표시 이름 반환
    /// </summary>
    public static string GetBetDisplayName(BetType betType, int objectID)
    {
        switch (betType)
        {
            case BetType.Number:
                return $"Number {objectID}";
                
            case BetType.Color:
                return GetColorName(objectID);
                
            case BetType.OddEven:
                return objectID == 0 ? "Even" : "Odd";
                
            case BetType.HighLow:
                return objectID == 0 ? "Low (1-18)" : "High (19-36)";
                
            case BetType.Dozen:
                return $"Dozen {GetDozenName(objectID)}";
                
            case BetType.Column:
                return $"Column {objectID}";
                
            default:
                return $"{betType} {objectID}";
        }
    }
    
    /// <summary>
    /// 팝업 제목 생성
    /// </summary>
    public static string GetPopupTitle(BetType betType, int objectID)
    {
        return $"Select Chip for {GetBetDisplayName(betType, objectID)}";
    }
    
    /// <summary>
    /// 색상 이름 반환
    /// </summary>
    public static string GetColorName(int colorID)
    {
        return colorID == 0 ? "Red" : "Black";
    }
    
    /// <summary>
    /// Dozen 이름 반환
    /// </summary>
    public static string GetDozenName(int dozenID)
    {
        switch (dozenID)
        {
            case 0: return "1st (1-12)";
            case 1: return "2nd (13-24)";
            case 2: return "3rd (25-36)";
            default: return $"{dozenID + 1}th";
        }
    }
    
    /// <summary>
    /// BetType을 한글 이름으로 변환
    /// </summary>
    public static string GetBetTypeName(BetType betType)
    {
        switch (betType)
        {
            case BetType.Number: return "Number";
            case BetType.Color: return "Color";
            case BetType.OddEven: return "Odd/Even";
            case BetType.HighLow: return "High/Low";
            case BetType.Dozen: return "Dozen";
            case BetType.Column: return "Column";
            default: return betType.ToString();
        }
    }
}

