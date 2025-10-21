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
    
    [Header("UI Panels")]
    [SerializeField] private GameObject bettingPanel;
    [SerializeField] private GameObject itemPanel;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject betItemPrefab;
    
    [Header("CopySpot Message")]
    [SerializeField] private GameObject copySpotMessagePanel;
    [SerializeField] private TMP_Text copySpotMessageText;
    
    // 동적 생성된 UI 요소들
    private List<GameObject> betItems = new List<GameObject>();
    

    private void Awake()
    {
        Initialize();
    }
    
    public override void Initialize()
    {
        Debug.Log("[GameUI] Initialize called");

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
        Debug.Log("<color=cyan>[GameUI] OnEnable - Binding to GameUI domain</color>");
        // Game으로부터 메시지 수신용 바인드
        Presenter.Bind("GameUI", this);
        Debug.Log("<color=cyan>[GameUI] Successfully bound to GameUI domain</color>");
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
        Debug.Log("[GameUI] RegisterButtonEvents called");

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
        Debug.Log($"[GameUI] ViewQuick called with key: {key}");
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
                Debug.Log("[GameUI] Spin started - disable spin and reset buttons");
                
                // 스핀 버튼과 리셋 버튼 비활성화
                if (mButtons.ContainsKey("SpinButton"))
                    mButtons["SpinButton"].interactable = false;
                if (mButtons.ContainsKey("ResetButton"))
                    mButtons["ResetButton"].interactable = false;
                    
                break;
                
            case Game.Keys.UPDATE_SPIN_BUTTON:
                Debug.Log($"<color=magenta>[GameUI] UPDATE_SPIN_BUTTON received! data: {data?.GetType()}</color>");
                if (data != null && data is OData<bool> buttonData)
                {
                    bool isEnabled = buttonData.Get();
                    Debug.Log($"<color=magenta>[GameUI] Setting spin button enabled: {isEnabled}</color>");
                    
                    // 스핀 버튼만 활성화/비활성화
                    if (mButtons.ContainsKey("SpinButton"))
                    {
                        mButtons["SpinButton"].interactable = isEnabled;
                        Debug.Log($"<color=magenta>[GameUI] SpinButton.interactable = {isEnabled}</color>");
                    }
                    else
                    {
                        Debug.LogError("[GameUI] SpinButton not found in mButtons!");
                    }
                }
                else
                {
                    Debug.LogError($"[GameUI] Invalid data type for UPDATE_SPIN_BUTTON: {data?.GetType()}");
                }
                break;
                
            case Game.Keys.UPDATE_RESET_BUTTON:
                if (data != null && data is OData<bool> resetButtonData)
                {
                    bool isEnabled = resetButtonData.Get();
                    Debug.Log($"[GameUI] Setting reset button enabled: {isEnabled}");
                    
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
                if (data != null && data is OData<double> totalData)
                {
                    ShowGameOver(totalData.Get());
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
                
            // CMD_SPOT_CLICKED와 CMD_BET_OBJECT_CLICKED는 Game에서 직접 팝업 처리
        }
    }
    
    #endregion
    
    #region UI Update Methods
    
    private void SetGameState(GameState gameState)
    {
        Debug.Log($"[GameUI] SetGameState called");
        
        if (gameState == null)
        {
            Debug.LogWarning("[GameUI] Received null GameState!");
            return;
        }
        
        Debug.Log($"[GameUI] Updating UI with GameState: Turn {gameState.currentTurn}, Money ${gameState.GetTotalMoney():F2}");
        
        // UI 전체 업데이트 (파라미터로 gameState 전달)
        RefreshTurnInfo(gameState);
        RefreshSpotList(gameState);
        RefreshBettingInfo(gameState);
        RefreshInventory(gameState);
        
        Debug.Log($"[GameUI] SetGameState completed");
    }
    
    private void RefreshTurnInfo(GameState gameState)
    {
        Debug.Log($"[GameUI] RefreshTurnInfo: Turn {gameState.currentTurn}, Money ${gameState.GetTotalMoney():F2}");
        
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
        
        // SpotItemUI는 더 이상 GameUI에서 관리하지 않음 (3D SpotObject가 직접 관리)
        Debug.Log($"[GameUI] Spot list refresh requested (handled by 3D objects)");
    }
    
    private void RefreshBettingInfo(GameState gameState)
    {
        Debug.Log($"[GameUI] RefreshBettingInfo called");
        
        if (gameState == null)
        {
            Debug.LogWarning("[GameUI] RefreshBettingInfo: gameState is null");
            return;
        }
        
        // 배팅 리스트는 gameState에서 직접 가져오기
        var currentBets = gameState.currentBets;
        Debug.Log($"[GameUI] RefreshBettingInfo: {currentBets.Count} bets to display");
        
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
        
        Debug.Log($"[GameUI] Betting info refreshed: {currentBets.Count} bets");
    }
    
    private void RefreshInventory(GameState gameState)
    {
        Debug.Log($"[GameUI] RefreshInventory called");
        
        if (gameState == null)
        {
            Debug.LogWarning("[GameUI] RefreshInventory: gameState is null");
            return;
        }
        
        Debug.Log($"[GameUI] RefreshInventory: {gameState.inventory.Count} items in inventory");
        
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
        Debug.Log("[GameUI] Reset button clicked");
        
        // Game에 게임 리셋 명령 전송
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_RESET_GAME);
    }
    
    private void OnSpotInfoButtonClicked()
    {
        Debug.Log("[GameUI] Spot Info button clicked");
        
        // Game에 Spot 정보 팝업 표시 명령 전송
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_SHOW_SPOT_INFO);
    }
    
    #endregion
    
    #region Spot Click Handling
        
    private ChipType GetFirstAvailableChipType(GameState gameState)
    {
        foreach (ChipType type in ChipTypeCache.AllTypes)
        {
            if (gameState.availableChips[type] > 0)
            {
                return type;
            }
        }
        return ChipType.Chip1; // 기본값
    }
    
    #endregion
    
    #region Game Event Display
    
    // OnSpinStarted 메서드 제거됨 - Presenter.Send로 통일
    
    private void ShowResult(SpinResultMessage result)
    {
        Debug.Log($"[GameUI] ShowResult called");
        
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
            
            mTMPText["ResultText"].text = resultMessage;
        }
        
        // 스핀 버튼 재활성화
        if (mButtons.ContainsKey("SpinButton"))
            mButtons["SpinButton"].interactable = true;
    }
    
    /// <summary>
    /// BetType을 읽기 쉬운 이름으로 변환
    /// </summary>
    private string GetBetTypeName(BetType betType)
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
    
    /// <summary>
    /// 배팅의 구체적인 정보 반환
    /// </summary>
    private string GetBetDetail(BetData bet)
    {
        string betInfo = "";
        
        switch (bet.betType)
        {
            case BetType.Number:
                betInfo = $"Number {bet.targetValue}";
                break;
                
            case BetType.Color:
                string colorName = bet.targetValue == 0 ? "Red" : "Black";
                betInfo = $"Color: {colorName}";
                break;
                
            case BetType.OddEven:
                string oddEven = bet.targetValue == 0 ? "Odd" : "Even";
                betInfo = $"{oddEven}";
                break;
                
            case BetType.HighLow:
                string highLow = bet.targetValue == 0 ? "Low(1-18)" : "High(19-36)";
                betInfo = $"{highLow}";
                break;
                
            case BetType.Dozen:
                string dozenRange = bet.targetValue == 1 ? "1-12" : bet.targetValue == 2 ? "13-24" : "25-36";
                betInfo = $"Dozen {bet.targetValue} ({dozenRange})";
                break;
                
            case BetType.Column:
                betInfo = $"Column {bet.targetValue}";
                break;
                
            default:
                betInfo = $"{bet.betType} ({bet.targetValue})";
                break;
        }
        
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
        
        Debug.Log($"[GameUI] Game Over: ${totalEarned:F2}");
    }
    
    private void ShowGameReset(int maxTurns)
    {
        Debug.Log($"<color=cyan>[GameUI] ═══════════ GAME RESET ═══════════</color>");
        
        if (mTMPText.ContainsKey("ResultText"))
        {
            string resetMessage = "";
            resetMessage += $"<color=#00FFFF><b>═══════════════════════</b></color>\n";
            resetMessage += $"<color=#FFFF00><size=24><b>- GAME RESET -</b></size></color>\n";
            resetMessage += $"<color=#00FFFF><b>═══════════════════════</b></color>\n\n";
            resetMessage += $"<color=#FFFFFF>New game started!</color>\n";
            resetMessage += $"<color=#AAFFAA>• Starting chips: <b><sprite=0> x5</b></color>\n";
            resetMessage += $"<color=#AAFFAA>• Total turns: <b>{maxTurns}</b></color>\n\n";
            resetMessage += $"<color=#FFAA44>Place your bet and spin!</color>";
            
            mTMPText["ResultText"].text = resetMessage;
        }
        
        Debug.Log($"<color=lime>[GameUI] Game reset message displayed (maxTurns: {maxTurns})</color>");
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

