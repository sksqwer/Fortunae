using System;

/// <summary>
/// 칩 선택 팝업 메시지 (통신용)
/// </summary>
public struct ChipSelectionMessage
{   
    public readonly BetType betType;
    public readonly int objectID;
    public readonly GameState gameState;
    public readonly Action<BetConfirmMessage> callback;
    
    public ChipSelectionMessage(BetType betType, int objectID, GameState gameState, Action<BetConfirmMessage> callback)
    {
        this.betType = betType;
        this.objectID = objectID;
        this.gameState = gameState;
        this.callback = callback;
    }
}

