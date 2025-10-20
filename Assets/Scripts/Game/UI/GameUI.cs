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
    
    // 동적 생성된 UI 요소들
    private List<GameObject> betItems = new List<GameObject>();
    
    // Spot 선택 모드
    private bool isSpotSelectionMode = false;
    private SpotItemType currentItemType;
    private int selectedSpotID = -1; // Copy Spot 첫 번째 선택용
    
    public void Awake()
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
        Debug.Log("[GameUI] RegisterButtonEvents called");
        
        if (mButtons.ContainsKey("SpinButton"))
            mButtons["SpinButton"].onClick.AddListener(OnSpinButtonClicked);
        
        if (mButtons.ContainsKey("ResetButton"))
            mButtons["ResetButton"].onClick.AddListener(OnResetButtonClicked);
        
        // 아이템 버튼들
        if (mButtons.ContainsKey("PlusSpotButton"))
            mButtons["PlusSpotButton"].onClick.AddListener(() => OnItemButtonClicked(SpotItemType.PlusSpot));
        
        if (mButtons.ContainsKey("CopySpotButton"))
            mButtons["CopySpotButton"].onClick.AddListener(() => OnItemButtonClicked(SpotItemType.CopySpot));
        
        if (mButtons.ContainsKey("UpgradedMultiButton"))
            mButtons["UpgradedMultiButton"].onClick.AddListener(() => OnItemButtonClicked(SpotItemType.UpgradedMultiSpot));
        
        // Charm 버튼들
        if (mButtons.ContainsKey("DeathCharmButton"))
            mButtons["DeathCharmButton"].onClick.AddListener(() => OnCharmToggled(CharmType.Death));
        
        if (mButtons.ContainsKey("ChameleonCharmButton"))
            mButtons["ChameleonCharmButton"].onClick.AddListener(() => OnCharmToggled(CharmType.Chameleon));
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
                if (data != null && data is OData<SpinResult> resultData)
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
                
            case Game.Keys.CMD_SHOW_BET_POPUP:
                HandleShowBetPopup(data);
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
            string inventoryText = "Inventory:\n";
            
            foreach (var item in gameState.inventory)
            {
                if (item.count > 0)
                {
                    inventoryText += $"• {item.itemID} : ({item.count})\n";
                }
            }
            
            mTMPText["InventoryText"].text = inventoryText;
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
    
    private void OnItemButtonClicked(SpotItemType itemType)
    {
        Debug.Log($"[GameUI] Item button clicked: {itemType}");
        Debug.Log($"[GameUI] Activating spot selection mode for {itemType}");
        
        // Spot 선택 모드 활성화
        isSpotSelectionMode = true;
        currentItemType = itemType;
        selectedSpotID = -1;
        
        Debug.Log($"[GameUI] Spot selection mode active: isSpotSelectionMode={isSpotSelectionMode}");
    }
    
    private void OnCharmToggled(CharmType charmType)
    {
        Debug.Log($"[GameUI] Charm toggled: {charmType}");
        
        // Game에서 현재 GameState 가져오기
        var game = FindFirstObjectByType<Game>();
        if (game == null)
        {
            Debug.LogError("[GameUI] Game object not found!");
            return;
        }
        
        Debug.Log("[GameUI] Getting GameState from Game");
        GameState gameState = game.GetGameState();
        if (gameState == null)
        {
            Debug.LogError("[GameUI] GameState is null!");
            return;
        }
        
        // Charm 아이템 ID 결정
        string charmID = charmType == CharmType.Death ? "DEATH_CHARM" : "CHAMELEON_CHARM";
        Debug.Log($"[GameUI] Looking for charm: {charmID}");
        
        ItemData charmItem = gameState.GetItemByID(charmID);
        if (charmItem == null || charmItem.count <= 0)
        {
            Debug.LogWarning($"[GameUI] Charm {charmID} not available (count={charmItem?.count ?? 0})");
            return;
        }
        
        Debug.Log($"[GameUI] Charm {charmID} available (count={charmItem.count})");
        
        // Charm 사용 명령 전송 (타겟 없음)
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_USE_ITEM, charmItem);
        Debug.Log($"[GameUI] Sent CMD_USE_ITEM for Charm: {charmID}");
    }
    
    #endregion
    
    #region Spot Click Handling
    
    public void OnSpotClicked(int spotID, GameState gameState)
    {
        Debug.Log($"[GameUI] OnSpotClicked called with spotID: {spotID}");
        
        // gameState 체크
        if (gameState == null)
        {
            Debug.LogError("[GameUI] gameState is null! Cannot process spot click.");
            return;
        }
        
        // Spot 선택 모드일 경우
        if (isSpotSelectionMode)
        {
            HandleSpotSelection(spotID);
            return;
        }
        
        Debug.Log($"[GameUI] 3D Spot {spotID} clicked!");
        
        // 칩 선택 팝업 표시
        int totalChips = gameState.availableChips.GetTotalCount();
        Debug.Log($"[GameUI] Total chips available: {totalChips}");
        
        if (totalChips > 0)
        {
            Debug.Log($"[GameUI] Showing chip selection popup for spot {spotID}");
            
            // 박싱/언박싱 없이 구조체 사용
            var chipSelectionData = new ChipSelectionData(
                spotID,
                gameState.availableChips,
                (spotID, chipType, chipCount) =>
                {
                    // 배팅 명령 전송
                    GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_PLACE_BET, 
                        new object[] { BetType.Number, spotID, chipType, chipCount });
                    
                    int chipValue = ChipTypeCache.Values[chipType];
                    Debug.Log($"[GameUI] Bet placed on Spot {spotID}: {chipCount}x ${chipValue} chips");
                }
            );
            
            Debug.Log($"[GameUI] Attempting to show ChipSelectionPopup...");
            UIManager.ShowPopup("ChipSelectionPopup", chipSelectionData);
            Debug.Log($"[GameUI] UIManager.ShowPopup call completed");
        }
        else
        {
            Debug.LogWarning("[GameUI] No chips available!");
        }
    }
    
    private void OnSpotClickedFallback(int spotID, GameState gameState)
    {
        // 팝업이 없을 때의 대체 동작
        if (gameState != null)
        {
            // 사용 가능한 첫 번째 칩 타입으로 1개 배팅
            ChipType firstAvailable = GetFirstAvailableChipType(gameState);
            if (gameState.availableChips.HasChip(firstAvailable, 1))
            {
                // 배팅 명령 전송
                GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_PLACE_BET, 
                    new object[] { BetType.Number, spotID, firstAvailable, 1 });
                
                Debug.Log($"[GameUI] Bet placed on Spot {spotID}: 1x ${ChipTypeCache.Values[firstAvailable]} chip");
            }
        }
    }
    
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
    
    /// <summary>
    /// SpotItemType을 itemID로 변환
    /// </summary>
    private string GetItemIDFromType(SpotItemType spotItemType)
    {
        switch (spotItemType)
        {
            case SpotItemType.PlusSpot:
                return "PLUS_SPOT";
            case SpotItemType.CopySpot:
                return "COPY_SPOT";
            case SpotItemType.UpgradedMultiSpot:
                return "UPGRADED_MULTI_SPOT";
            default:
                return "";
        }
    }
    
    private void HandleSpotSelection(int spotID)
    {
        Debug.Log($"[GameUI] HandleSpotSelection called: spotID={spotID}, currentItemType={currentItemType}");
        
        string itemID = GetItemIDFromType(currentItemType);
        Debug.Log($"[GameUI] Item ID for {currentItemType}: {itemID}");
        
        switch (currentItemType)
        {
            case SpotItemType.PlusSpot:
            case SpotItemType.UpgradedMultiSpot:
                Debug.Log($"[GameUI] Single target item - sending CMD_USE_ITEM_BY_ID for {itemID} on spot {spotID}");
                // 단일 타겟 아이템 사용 명령 전송
                GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_USE_ITEM_BY_ID, 
                    new object[] { itemID, new int[] { spotID } });
                
                isSpotSelectionMode = false;
                Debug.Log($"[GameUI] {itemID} used on Spot {spotID}");
                break;
            
            case SpotItemType.CopySpot:
                Debug.Log($"[GameUI] CopySpot handling - selectedSpotID={selectedSpotID}, new spotID={spotID}");
                // 2개 타겟 필요
                if (selectedSpotID == -1)
                {
                    selectedSpotID = spotID;
                    Debug.Log($"[GameUI] First spot selected for copy: {spotID} - waiting for target selection");
                }
                else
                {
                    Debug.Log($"[GameUI] Second spot selected for copy: {spotID} - sending CMD_USE_ITEM_BY_ID");
                    // 두 번째 선택 완료 - 아이템 사용 명령 전송
                    GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_USE_ITEM_BY_ID, 
                        new object[] { itemID, new int[] { selectedSpotID, spotID } });
                    
                    Debug.Log($"[GameUI] Copy Spot command sent: {selectedSpotID} -> {spotID}");
                    isSpotSelectionMode = false;
                    selectedSpotID = -1;
                    Debug.Log($"[GameUI] Spot selection mode deactivated");
                }
                break;
        }
    }
    
    #endregion
    
    #region Game Event Display
    
    // OnSpinStarted 메서드 제거됨 - Presenter.Send로 통일
    
    private void ShowResult(SpinResult result)
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
                    resultMessage += "<color=#CCCCCC>My Bets:</color>";
                    foreach (var bet in result.allBets)
                    {
                        string betDetail = GetBetDetail(bet);
                        bool isWinning = result.winningBets != null && result.winningBets.Contains(bet);
                        
                        if (isWinning)
                        {
                            resultMessage += $"<color=#00FF00>  ✓ {betDetail} (WIN!)</color>\n";
                        }
                        else
                        {
                            resultMessage += $"<color=#FF6666>  ✗ {betDetail} (LOSE)</color>\n";
                        }
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
                    resultMessage += "<color=#CCCCCC>My Bets:</color>";
                    foreach (var bet in result.allBets)
                    {
                        string betDetail = GetBetDetail(bet);
                        resultMessage += $"<color=#FF6666>  ✗ {betDetail} (LOSE)</color>\n";
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
        switch (bet.betType)
        {
            case BetType.Number:
                return $"Number {bet.targetValue}";
                
            case BetType.Color:
                string colorName = bet.targetValue == 0 ? "Red" : "Black";
                return $"Color: {colorName}";
                
            case BetType.OddEven:
                string oddEven = bet.targetValue == 0 ? "Odd" : "Even";
                return $"{oddEven}";
                
            case BetType.HighLow:
                string highLow = bet.targetValue == 0 ? "Low(1-18)" : "High(19-36)";
                return $"{highLow}";
                
            case BetType.Dozen:
                string dozenRange = bet.targetValue == 1 ? "1-12" : bet.targetValue == 2 ? "13-24" : "25-36";
                return $"Dozen {bet.targetValue} ({dozenRange})";
                
            case BetType.Column:
                return $"Column {bet.targetValue}";
                
            default:
                return $"{bet.betType} ({bet.targetValue})";
        }
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
    /// BetObject 배팅 팝업 표시 처리
    /// </summary>
    private void HandleShowBetPopup(IOData data)
    {
        Debug.Log($"[GameUI] HandleShowBetPopup called with data: {data?.GetType()}");
        
        if (data is OData<BetObjectClickData> clickData)
        {
            BetObjectClickData betClickData = clickData.Get();
            Debug.Log($"[GameUI] BetObject clicked: ID={betClickData.objectID}, Type={betClickData.betType}, Value={betClickData.targetValue}");
            
            // GameState 가져오기
            var game = FindFirstObjectByType<Game>();
            if (game != null)
            {
                GameState gameState = game.GetGameState();
                ShowBetObjectPopup(betClickData, gameState);
            }
        }
    }
    
    // OnBetObjectClicked 메서드 제거됨 - Game에서 직접 팝업 처리
    
    /// <summary>
    /// BetObject 배팅 팝업 표시
    /// </summary>
    private void ShowBetObjectPopup(BetObjectClickData clickData, GameState gameState)
    {
        Debug.Log($"[GameUI] ShowBetObjectPopup called: Type={clickData.betType}, Value={clickData.targetValue}");
        
        if (gameState == null)
        {
            Debug.LogError("[GameUI] gameState is null! Cannot show bet popup.");
            return;
        }
        
        int totalChips = gameState.availableChips.GetTotalCount();
        Debug.Log($"[GameUI] Total chips available: {totalChips}");
        
        if (totalChips > 0)
        {
            Debug.Log($"[GameUI] Showing chip selection popup for BetObject {clickData.objectID}");
            
            // 박싱/언박싱 없이 구조체 사용
            var chipSelectionData = new ChipSelectionData(
                clickData.objectID,
                gameState.availableChips,
                (spotID, chipType, chipCount) =>
                {
                    // 배팅 명령 전송
                    GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_PLACE_BET, 
                        new object[] { clickData.betType, clickData.targetValue, chipType, chipCount });
                    
                    int chipValue = ChipTypeCache.Values[chipType];
                    double payout = SpotCalculator.GetBasePayout(clickData.betType, gameState, clickData.targetValue);
                    Debug.Log($"[GameUI] Bet placed on BetObject {clickData.objectID}: {chipCount}x ${chipValue} chips (Payout: x{payout})");
                }
            );
            
            Debug.Log($"[GameUI] Attempting to show ChipSelectionPopup...");
            UIManager.ShowPopup("ChipSelectionPopup", chipSelectionData);
            Debug.Log($"[GameUI] UIManager.ShowPopup call completed");
        }
        else
        {
            Debug.LogWarning("[GameUI] Cannot show bet popup - no chips available or popup not found");
        }
    }
    
    /// <summary>
    /// Spot 아이템 사용
    /// </summary>
    private void UseSpotItem(int spotID, SpotItemType itemType, int? targetSpotID = null)
    {
        // Game에서 현재 GameState 가져오기
        var game = FindFirstObjectByType<Game>();
        if (game == null) return;
        
        GameState gameState = game.GetGameState();
        if (gameState == null) return;
        
        // 해당 아이템이 있는지 확인
        ItemData item = gameState.inventory.Find(i => i.itemType == ItemType.SpotItem && i.spotItemType == itemType);
        if (item == null)
        {
            Debug.LogWarning($"[GameUI] Item {itemType} not found in inventory");
            return;
        }
        
        // 아이템 사용 명령 전송
        if (targetSpotID.HasValue)
        {
            // 타겟이 있는 경우 (Copy 등)
            GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_USE_ITEM, 
                new object[] { item, new int[] { spotID, targetSpotID.Value } });
        }
        else
        {
            // 타겟이 없는 경우 (Plus 등)
            GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_USE_ITEM, 
                new object[] { item, new int[] { spotID } });
        }
        
        Debug.Log($"[GameUI] Used SpotItem: {itemType} on spot {spotID}");
    }
    
    
    #endregion
}

