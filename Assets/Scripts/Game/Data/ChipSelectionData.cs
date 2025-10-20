using System;

/// <summary>
/// 칩 선택 팝업 데이터 (박싱/언박싱 방지)
/// </summary>
public struct ChipSelectionData
{
    public readonly int objectID;
    public readonly ChipCollection availableChips;
    public readonly Action<int, ChipType, int> callback;
    
    public ChipSelectionData(int objectID, ChipCollection availableChips, Action<int, ChipType, int> callback)
    {
        this.objectID = objectID;
        this.availableChips = availableChips;
        this.callback = callback;
    }
}
