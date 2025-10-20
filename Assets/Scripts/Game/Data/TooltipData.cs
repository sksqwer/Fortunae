using UnityEngine;

/// <summary>
/// 툴팁 데이터 구조체 (박싱/언박싱 방지)
/// </summary>
public struct TooltipData
{
    public readonly Spot spotData;
    public readonly Vector2 screenPos;
    
    public TooltipData(Spot spotData, Vector2 screenPos)
    {
        this.spotData = spotData;
        this.screenPos = screenPos;
    }
}
