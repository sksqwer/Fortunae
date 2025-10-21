using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using GB;

/// <summary>
/// 룰렛 게임 메인 UI (MVP View)
/// </summary>
public class GameUI : UIScreen
{
    // === UI 패널 필드 ===
    [Header("UI Panels")]
    [SerializeField] private GameObject bettingPanel;
    [SerializeField] private GameObject itemPanel;
    
    // === 프리팹 필드 ===
    [Header("Prefabs")]
    [SerializeField] private GameObject betItemPrefab;
    
    // === CopySpot 메시지 필드 ===
    [Header("CopySpot Message")]
    [SerializeField] private GameObject copySpotMessagePanel;
    [SerializeField] private TMP_Text copySpotMessageText;
    
    // === 스크롤 필드 ===
    [Header("Result Scroll")]
    [SerializeField] private ScrollRect resultScrollRect;
    
    // === 동적 UI 요소들 ===
    private List<GameObject> betItems = new List<GameObject>();
    

    private void Awake()
    {
        Initialize();
    }
    
    public override void Initialize()
    {
        base.Initialize();

        Regist();
        
        // 버튼 이벤트 등록
        RegisterButtonEvents();
        
        // CopySpot 메시지 패널 초기 상태 (숨김)
        if (copySpotMessagePanel != null)
        {
            copySpotMessagePanel.SetActive(false);
        }

        Debug.Log("[GameUI] GameUI Initialized");
    }
    
    private void OnEnable()
    {
        // Game으로부터 메시지 수신용 바인드
        Presenter.Bind("GameUI", this);
    }

    private void OnDisable() 
    {
        // 메시지 수신 해제
        Presenter.UnBind("GameUI", this);
    }

    /// <summary>
    /// 버튼 이벤트 등록
    /// </summary>
    private void RegisterButtonEvents()
    {
        if (mButtons.ContainsKey("SpinButton"))
            mButtons["SpinButton"].onClick.AddListener(OnSpinButtonClicked);
        
        if (mButtons.ContainsKey("ResetButton"))
            mButtons["ResetButton"].onClick.AddListener(OnResetButtonClicked);
        
        // Spot Info 버튼
        if (mButtons.ContainsKey("SpotInfoButton"))
            mButtons["SpotInfoButton"].onClick.AddListener(OnSpotInfoButtonClicked);
    }
    
    #region IView Implementation (MVP Pattern)
    
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case Game.Keys.UPDATE_GAME_STATE:
                if (data != null && data is OData<GameState> gameStateData)
                {
                    SetGameState(gameStateData.Get());
                }
                break;
                
            case Game.Keys.UPDATE_TURN_INFO:
                if (data != null && data is OData<GameState> turnInfoGameState)
                {
                    RefreshTurnInfo(turnInfoGameState.Get());
                }
                break;
                
            case Game.Keys.UPDATE_SPIN_START:
                if (mButtons.ContainsKey("SpinButton"))
                    mButtons["SpinButton"].interactable = false;
                if (mButtons.ContainsKey("ResetButton"))
                    mButtons["ResetButton"].interactable = false;                                        
                break;
                
            case Game.Keys.UPDATE_SPIN_BUTTON:
                if (data != null && data is OData<bool> buttonData)
                {
                    bool isEnabled = buttonData.Get();

                    if (mButtons.ContainsKey("SpinButton"))
                    {
                        mButtons["SpinButton"].interactable = isEnabled;                        
                    }
                }
                break;
                
            case Game.Keys.UPDATE_RESET_BUTTON:
                if (data != null && data is OData<bool> resetButtonData)
                {
                    bool isEnabled = resetButtonData.Get();

                    if (mButtons.ContainsKey("ResetButton"))
                    {
                        mButtons["ResetButton"].interactable = isEnabled;
                    }
                }
                break;
                
            case Game.Keys.UPDATE_SPIN_COMPLETE:
                if (data != null && data is OData<SpinResultMessage> resultData)
                {
                    ShowResult(resultData.Get());
                }
                break;
                
            case Game.Keys.UPDATE_GAME_OVER:
                if (data != null && data is OData<GameState> gameOverState)
                {
                    ShowGameOver(gameOverState.Get());
                }
                break;
                
            case Game.Keys.UPDATE_GAME_RESET:
                if (data != null && data is OData<int> maxTurnsData)
                {
                    ShowGameReset(maxTurnsData.Get());
                }
                else
                {
                    ShowGameReset(3); // 기본값
                }
                break;
                
            case Game.Keys.UPDATE_BET_LIST:
                if (data != null && data is OData<GameState> betListGameState)
                {
                    RefreshBettingInfo(betListGameState.Get());
                }
                break;
            
            case "SHOW_COPY_SPOT_MESSAGE":
                if (data != null && data is OData<string> messageData)
                {
                    ShowCopySpotMessage(messageData.Get());
                }
                break;
            
            case "HIDE_COPY_SPOT_MESSAGE":
                HideCopySpotMessage();
                break;
        }
    }
    
    #endregion
    
    #region UI Update Methods
    
    private void SetGameState(GameState gameState)
    {
        if (gameState == null)
        {
            return;
        }
        
        // UI 전체 업데이트 (파라미터로 gameState 전달)
        RefreshTurnInfo(gameState);
        RefreshSpotList(gameState);
        RefreshBettingInfo(gameState);
        RefreshInventory(gameState);
    }
    
    private void RefreshTurnInfo(GameState gameState)
    {        
        if (mTMPText.ContainsKey("TurnText"))
            mTMPText["TurnText"].text = $"Turn {gameState.currentTurn} / 3";
        
        if (mTMPText.ContainsKey("ChipText"))
            mTMPText["ChipText"].text = $"Chips: {gameState.availableChips.ToSpriteString()}";
        
        if (mTMPText.ContainsKey("MoneyText"))
            mTMPText["MoneyText"].text = $"Money: ${gameState.GetTotalMoney():F2}";
    }
    
    private void RefreshSpotList(GameState gameState)
    {
        if (gameState == null || gameState.spots == null) return;        
    }
    
    private void RefreshBettingInfo(GameState gameState)
    {        
        if (gameState == null)
        {
            return;
        }
        
        // 배팅 리스트는 gameState에서 직접 가져오기
        var currentBets = gameState.currentBets;
        
        // 기존 BetItemUI들 제거
        foreach (var betItem in betItems)
        {
            if (betItem != null)
                Destroy(betItem);
        }
        betItems.Clear();
        
        // 새로운 BetItemUI들 생성
        if (betItemPrefab != null)
        {
            foreach (var bet in currentBets)
            {
                GameObject betItemObj = Instantiate(betItemPrefab, bettingPanel.transform);
                BetItemUI betItemUI = betItemObj.GetComponent<BetItemUI>();
                if (betItemUI != null)
                {
                    betItemUI.Initialize(bet);
                    betItems.Add(betItemObj);
                }
            }
        }
    }
    
    private void RefreshInventory(GameState gameState)
    {        
        if (gameState == null)
        {
            return;
        }
        
        // 인벤토리 아이템 표시 업데이트
        if (mTMPText.ContainsKey("InventoryText"))
        {            
            mTMPText["InventoryText"].text = gameState.InventoryToString();
        }
        
        // Charm 상태 표시 (ID 기반)
        if (mTMPText.ContainsKey("DeathCharmStatus"))
        {
            string deathStatus = gameState.HasItem("DEATH_CHARM") ? "Active" : "Inactive";
            mTMPText["DeathCharmStatus"].text = $"Death Charm: {deathStatus}";
        }
        
        if (mTMPText.ContainsKey("ChameleonCharmStatus"))
        {
            string chameleonStatus = gameState.HasItem("CHAMELEON_CHARM") ? "Active" : "Inactive";
            mTMPText["ChameleonCharmStatus"].text = $"Chameleon Charm: {chameleonStatus}";
        }
        
        Debug.Log($"[GameUI] Inventory refreshed: {gameState.inventory.Count} items");
    }
    
    #endregion
    
    #region Button Event Handlers (View -> Presenter)
    
    private void OnSpinButtonClicked()
    {
        Debug.Log("[GameUI] Spin button clicked");
        
        // Game에 스핀 시작 명령 전송
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_START_SPIN);
    }
    
    private void OnResetButtonClicked()
    {        
        // Game에 게임 리셋 명령 전송
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_RESET_GAME);
    }
    
    private void OnSpotInfoButtonClicked()
    {
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_SHOW_SPOT_INFO);
    }
    
    #endregion
    
    #region Game Event Display
    
    private void ShowResult(SpinResultMessage result)
    {        
        // 색깔 이름과 Rich Text 색상 코드
        string colorName = result.winningColor == SpotColor.Red ? "Red" : "Black";
        string colorCodeConsole = result.winningColor == SpotColor.Red ? "red" : "white";
        string colorCodeUI = result.winningColor == SpotColor.Red ? "#FF4444" : "#FFFFFF";
        
        // 번호 속성
        bool isOdd = result.winningNumber % 2 == 1;
        bool isHigh = result.winningNumber >= 19;
        int dozen = (result.winningNumber - 1) / 12 + 1;
        int column = ((result.winningNumber - 1) % 3) + 1;
        
        // Console 로그에 색깔 입히기
        if (result.hasWon)
        {
            Debug.Log($"<color=green>[GameUI] ★★★ WIN ★★★</color>");
        }
        else
        {
            Debug.Log($"<color=red>[GameUI] ✖✖✖ LOSE ✖✖✖</color>");
        }
        
        Debug.Log($"<color={colorCodeConsole}>[GameUI] Winning: Spot {result.winningSpotID}, Number {result.winningNumber} ({colorName})</color>");
        Debug.Log($"[GameUI] Properties: {(isOdd ? "Odd" : "Even")}, {(isHigh ? "High" : "Low")}, Dozen {dozen}, Column {column}");
        Debug.Log($"<color=yellow>[GameUI] Payout: ${result.totalPayout:F2}</color>");
        
        // 당첨된 배팅 로그
        if (result.winningBets != null && result.winningBets.Count > 0)
        {
            Debug.Log($"<color=cyan>[GameUI] Winning Bets ({result.winningBets.Count}):</color>");
            foreach (var bet in result.winningBets)
            {
                string betDetail = GetBetDetail(bet);
                Debug.Log($"<color=lime>  ✓ {betDetail}</color>");
            }
        }
        
        // 결과 텍스트 표시
        if (mTMPText.ContainsKey("ResultText"))
        {
            string resultMessage = "";
            
            if (result.hasWon)
            {
                resultMessage = $"<color=#00FF00><b>WIN!</b></color>\n";
                resultMessage += $"<color={colorCodeUI}><b>Spot: {result.winningSpotID}  Number: {result.winningNumber}  ({colorName})</b></color>\n";
                
                // 속성들 (강조)
                string oddEvenText = isOdd ? "<color=#FF88FF>Odd</color>" : "<color=#88FF88>Even</color>";
                string highLowText = isHigh ? "<color=#FFAA44>High(19-36)</color>" : "<color=#44AAFF>Low(1-18)</color>";
                string dozenRange = dozen == 1 ? "1-12" : dozen == 2 ? "13-24" : "25-36";
                string dozenText = $"<color=#FF99CC>Dozen {dozen}({dozenRange})</color>";
                string columnText = $"<color=#99CCFF>Column {column}</color>";
                
                resultMessage += $"• {oddEvenText}  • {highLowText} ";
                resultMessage += $"• {dozenText}  • {columnText}\n";
                resultMessage += $"<color=#FFFF00><b>Payout: ${result.totalPayout:F2}</b></color>\n";
                
                // 내 배팅 표시
                if (result.allBets != null && result.allBets.Count > 0)
                {
                    resultMessage += "\n<color=#CCCCCC>My Bets:</color>";
                    foreach (var bet in result.allBets)
                    {
                        string betDetail = GetBetDetail(bet);
                        resultMessage += $"<color=#00FF00>  {betDetail} (WIN!)</color>\n";
                    }
                }
            }
            else
            {
                resultMessage = $"<color=#FF0000><b>LOSE</b></color>\n";
                resultMessage += $"<color={colorCodeUI}><b>Spot: {result.winningSpotID}  Number: {result.winningNumber}  ({colorName})</b></color>\n";
                
                // 속성들 (강조)
                string oddEvenText = isOdd ? "<color=#FF88FF>Odd</color>" : "<color=#88FF88>Even</color>";
                string highLowText = isHigh ? "<color=#FFAA44>High(19-36)</color>" : "<color=#44AAFF>Low(1-18)</color>";
                string dozenRange = dozen == 1 ? "1-12" : dozen == 2 ? "13-24" : "25-36";
                string dozenText = $"<color=#FF99CC>Dozen {dozen}({dozenRange})</color>";
                string columnText = $"<color=#99CCFF>Column {column}</color>";
                
                resultMessage += $"• {oddEvenText}  • {highLowText} "; 
                resultMessage += $"• {dozenText}  • {columnText}\n";
                
                // 내 배팅 표시
                if (result.allBets != null && result.allBets.Count > 0)
                {
                    resultMessage += "\n<color=#CCCCCC>My Bets:</color>";
                    foreach (var bet in result.allBets)
                    {
                        string betDetail = GetBetDetail(bet);
                        resultMessage += $"<color=#FF6666>  {betDetail} (LOSE)</color>\n";
                    }
                }
            }
            
            // 마지막 턴이면 GameOver 정보도 추가
            if (result.isLastTurn)
            {
                resultMessage += "\n\n";
                resultMessage += GetGameOverSummary(result.gameState);
            }
            
            mTMPText["ResultText"].text = resultMessage;
        }
    }
    
    /// <summary>
    /// 게임 종료 화면 표시 (UPDATE_GAME_OVER 전용, 사용 안 함)
    /// </summary>
    private void ShowGameOver(GameState gameState)
    {
        // ShowResult에서 마지막 턴과 함께 표시하므로 비워둠
    }
    
    /// <summary>
    /// GameOver 요약 정보 생성
    /// </summary>
    private string GetGameOverSummary(GameState gameState)
    {
        string summary = "";
        
        // 게임 오버 헤더
        summary += $"<color=#FF0000><b>═══════════════════════</b></color>\n";
        summary += $"<color=#FF4444><size=28><b>GAME OVER</b></size></color>\n";
        summary += $"<color=#FF0000><b>═══════════════════════</b></color>\n\n";
        
        // 최종 결과 계산
        int startingChips = 5;
        int startingValue = startingChips; // Chip1 기준
        double finalValue = gameState.availableChips.GetTotalValue();
        double totalEarned = finalValue - startingValue;
        
        summary += $"<color=#FFFFFF><b>Final Result:</b></color>\n";
        summary += $"<color=#AAFFAA>Starting Chips: <sprite=0> x{startingChips} (${startingValue})</color>\n";
        
        // 최종 칩 구성 표시 (Sprite로)
        summary += $"<color=#AAFFAA>Final Chips: {gameState.availableChips.ToSpriteString()}</color>\n";
        
        // 각 칩 타입별 개수 표시
        foreach (ChipType chipType in ChipTypeCache.AllTypesReversed)
        {
            int count = gameState.availableChips[chipType];
            if (count > 0)
            {
                string chipSprite = ChipTypeCache.ToSpriteTag(chipType);
                summary += $"  <color=#CCCCCC>• {chipSprite} x{count}</color>\n";
            }
        }
        
        summary += $"<color=#AAFFAA>Total Value: <b>${finalValue:F0}</b></color>\n";
        
        if (totalEarned > 0)
            summary += $"<color=#00FF00><size=22><b>Total Earned: +${totalEarned:F0}</b></size></color>\n\n";
        else if (totalEarned < 0)
            summary += $"<color=#FF0000><size=22><b>Total Lost: ${totalEarned:F0}</b></size></color>\n\n";
        else
            summary += $"<color=#FFFF00><size=22><b>Break Even: $0</b></size></color>\n\n";
        
        // 턴별 내역
        summary += $"<color=#00FFFF><b>═══ Turn History ═══</b></color>\n\n";
        
        for (int i = 0; i < gameState.turnHistory.Count; i++)
        {
            TurnData turn = gameState.turnHistory[i];
            
            summary += $"<color=#FFFF00><b>Turn {turn.turnNumber}:</b></color>\n";
            summary += $"<color=#AAAAAA>Win: Spot #{turn.winningSpotID} (Num {turn.winningNumber})</color>\n";
            
            // 배팅 내역
            if (turn.bets.Count > 0)
            {
                summary += $"<color=#CCCCCC>Bets ({turn.bets.Count}):</color>\n";
                
                foreach (var bet in turn.bets)
                {
                    string betDetail = GetBetDetail(bet);
                    summary += $"  • {betDetail}\n";
                }
            }
            
            // 수익
            if (turn.totalPayout > 0)
                summary += $"<color=#00FF00>Payout: +{turn.totalPayout:F0}</color>\n";
            else
                summary += $"<color=#FF6666>Lost all bets</color>\n";
            
            summary += "\n";
        }
        
        return summary;
    }
    
    /// <summary>
    /// 배팅의 구체적인 정보 반환
    /// </summary>
    private string GetBetDetail(BetData bet)
    {
        string betInfo = "";
        
        // BetTypeHelper 사용
        betInfo = BetTypeHelper.GetBetDisplayName(bet.betType, bet.targetValue);
        
        // 칩 정보 추가
        betInfo += $" - {bet.chips.ToSpriteString()}";
        
        // HatWing 적용 여부 표시
        if (bet.isHatWingApplied)
        {
            betInfo += " <color=#FFD700>[HatWing: Force Win 50%]</color>";
        }
        
        return betInfo;
    }
    
    private void ShowGameOver(double totalEarned)
    {
        if (mTMPText.ContainsKey("GameOverText"))
        {
            mTMPText["GameOverText"].text = $"Game Over!\nTotal Earned: ${totalEarned:F2}";
            mGameObject["GameOverText"].SetActive(true);
        }
    }
    
    private void ShowGameReset(int maxTurns)
    {
        Debug.Log($"<color=cyan>[GameUI] ═══════════ GAME RESET ═══════════</color>");
        
        
        if (mTMPText.ContainsKey("ResultText"))
        {
            string resetMessage = "";
            resetMessage += $"<color=#00FFFF><b>═══════════════════════</b></color>\n";
            resetMessage += $"<color=#FFFF00><size=24><b>- GAME START -</b></size></color>\n";
            resetMessage += $"<color=#00FFFF><b>═══════════════════════</b></color>\n\n";
            resetMessage += $"<color=#FFFFFF>New game started!</color>\n";
            resetMessage += $"<color=#AAFFAA>• Starting chips: <b><sprite=0> x5</b></color>\n";
            resetMessage += $"<color=#AAFFAA>• Total turns: <b>{maxTurns}</b></color>\n\n";
            resetMessage += $"<color=#FFAA44>Place your bet and spin!</color>";
            
            mTMPText["ResultText"].text = resetMessage;
            
        }
    }
    
    /// <summary>
    /// CopySpot 선택 메시지 표시
    /// </summary>
    private void ShowCopySpotMessage(string message)
    {
        Debug.Log($"[GameUI] ShowCopySpotMessage: {message}");
        
        if (copySpotMessagePanel != null)
        {
            copySpotMessagePanel.SetActive(true);
        }
        
        if (copySpotMessageText != null)
        {
            copySpotMessageText.text = message;
        }
    }
    
    /// <summary>
    /// CopySpot 선택 메시지 숨김
    /// </summary>
    private void HideCopySpotMessage()
    {
        Debug.Log("[GameUI] HideCopySpotMessage");
        
        if (copySpotMessagePanel != null)
        {
            copySpotMessagePanel.SetActive(false);
        }
    }
    
    #endregion
}

