using UnityEngine;

/// <summary>
/// 툴팁 메시지 구조체 (통신용)
/// </summary>
public struct TooltipMessage
{
    public readonly Spot spotData;
    public readonly Vector2 screenPos;
    
    public TooltipMessage(Spot spotData, Vector2 screenPos)
    {
        this.spotData = spotData;
        this.screenPos = screenPos;
    }
}

