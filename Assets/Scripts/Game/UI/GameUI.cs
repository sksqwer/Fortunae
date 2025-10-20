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
    // UI 도메인 상수
    public const string DOMAIN_UI = "GameUI";
    
    [Header("Presenter Reference")]
    [SerializeField] private GamePresenter presenter; // MVP: View -> Presenter 명령 전달용
    
    [Header("UI Panels")]
    [SerializeField] private GameObject spotListPanel;
    [SerializeField] private GameObject bettingPanel;
    [SerializeField] private GameObject itemPanel;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject spotItemPrefab;
    [SerializeField] private GameObject betItemPrefab;
    [SerializeField] private ChipSelectionPopup chipSelectionPopup;
    
    // 동적 생성된 UI 요소들
    private List<SpotItemUI> spotItems = new List<SpotItemUI>();
    private List<GameObject> betItems = new List<GameObject>();
    
    private GameState currentGameState;
    
    // Spot 선택 모드0
    private bool isSpotSelectionMode = false;
    private SpotItemType currentItemType;
    private int selectedSpotID = -1; // Copy Spot 첫 번째 선택용
    
    public override void Initialize()
    {
        base.Initialize();
              
        // GameUI 도메인에 바인딩 (직접 메시지 수신용)
        Presenter.Bind(DOMAIN_UI, this);
        
        // 버튼 이벤트 등록
        RegisterButtonEvents();

        // 초기 GameState 요청
        if (presenter != null)
        {
            currentGameState = presenter.GetGameState();
            Debug.Log($"[GameUI] Initialized with GameState: {(currentGameState != null ? "OK" : "NULL")}");
        }
        
        Debug.Log("[GameUI] Initialized and bound to Presenter");
    }
    
    private void OnDestroy()
    {
        // Presenter에서 View 언바인딩
        Presenter.UnBind(DOMAIN_UI, this);
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
            case GamePresenter.Keys.UPDATE_GAME_STATE:
                if (data != null && data is OData<GameState> gameStateData)
                {
                    SetGameState(gameStateData.Get());
                }
                break;
                
            case GamePresenter.Keys.UPDATE_SPIN_START:
                OnSpinStarted();
                break;
                
            case GamePresenter.Keys.UPDATE_SPIN_COMPLETE:
                if (data != null && data is OData<SpinResult> resultData)
                {
                    ShowResult(resultData.Get());
                }
                break;
                
            case GamePresenter.Keys.UPDATE_GAME_OVER:
                if (data != null && data is OData<double> totalData)
                {
                    ShowGameOver(totalData.Get());
                }
                break;
                
            case GamePresenter.Keys.UPDATE_BET_LIST:
                RefreshBettingInfo();
                break;
                
            case GamePresenter.Keys.CMD_SHOW_BET_POPUP:
                HandleShowBetPopup(data);
                break;
                
            case GamePresenter.Keys.CMD_SPOT_CLICKED:
                Debug.Log($"[GameUI] CMD_SPOT_CLICKED received! data: {data}");
                if (data != null && data is OData<int> spotData)
                {
                    int spotID = spotData.Get();
                    Debug.Log($"[GameUI] Received spot click for Spot {spotID}");
                    OnSpotClicked(spotID);
                }
                else
                {
                    Debug.LogError($"[GameUI] Invalid data for CMD_SPOT_CLICKED: {data}");
                }
                break;
                
            case GamePresenter.Keys.CMD_BET_OBJECT_CLICKED:
                Debug.Log($"[GameUI] CMD_BET_OBJECT_CLICKED received! data: {data}");
                if (data != null && data is OData<BetObjectClickData> clickData)
                {
                    BetObjectClickData betData = clickData.Get();
                    Debug.Log($"[GameUI] Received bet object click for {betData.betType} {betData.targetValue}");
                    OnBetObjectClicked(betData);
                }
                else
                {
                    Debug.LogError($"[GameUI] Invalid data for CMD_BET_OBJECT_CLICKED: {data}");
                }
                break;
        }
    }
    
    #endregion
    
    #region UI Update Methods
    
    public override void Refresh()
    {
        base.Refresh();
        
        if (currentGameState == null)
            return;
        
        RefreshTurnInfo();
        RefreshSpotList();
        RefreshBettingInfo();
        RefreshInventory();
    }
    
    private void SetGameState(GameState gameState)
    {
        currentGameState = gameState;
        
        // UI 업데이트
        RefreshSpotList();
        RefreshBettingInfo();
        RefreshInventory();
        
        if (mTMPText.ContainsKey("MoneyText"))
            mTMPText["MoneyText"].text = $"Money: ${currentGameState.GetTotalMoney():F2}";
    }
    
    private void RefreshTurnInfo()
    {
        if (mTMPText.ContainsKey("TurnText"))
            mTMPText["TurnText"].text = $"Turn {currentGameState.currentTurn} / 3";
        
        if (mTMPText.ContainsKey("ChipText"))
            mTMPText["ChipText"].text = $"Chips: {currentGameState.availableChips.ToString()}";
        
        if (mTMPText.ContainsKey("MoneyText"))
            mTMPText["MoneyText"].text = $"Money: ${currentGameState.GetTotalMoney():F2}";
    }
    
    private void RefreshSpotList()
    {
        if (currentGameState == null || currentGameState.spots == null) return;
        
        // SpotItemUI들 업데이트
        for (int i = 0; i < spotItems.Count; i++)
        {
            if (spotItems[i] != null)
            {
                spotItems[i].Refresh();
            }
        }
        
        Debug.Log($"[GameUI] Spot list refreshed: {spotItems.Count} spots");
    }
    
    private void RefreshBettingInfo()
    {
        if (currentGameState == null || presenter == null) return;
        
        // 현재 배팅 리스트 가져오기
        var currentBets = presenter.GetCurrentBets();
        
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
    
    private void RefreshInventory()
    {
        if (currentGameState == null) return;
        
        // 인벤토리 아이템 표시 업데이트
        if (mTMPText.ContainsKey("InventoryText"))
        {
            string inventoryText = "Inventory:\n";
            
            foreach (var item in currentGameState.inventory)
            {
                if (item.count > 0)
                {
                    inventoryText += $"• {item.itemType} ({item.count})\n";
                }
            }
            
            mTMPText["InventoryText"].text = inventoryText;
        }
        
        // Charm 상태 표시
        if (mTMPText.ContainsKey("DeathCharmStatus"))
        {
            string deathStatus = currentGameState.HasDeathCharm() ? "Active" : "Inactive";
            mTMPText["DeathCharmStatus"].text = $"Death Charm: {deathStatus}";
        }
        
        if (mTMPText.ContainsKey("ChameleonCharmStatus"))
        {
            string chameleonStatus = currentGameState.HasChameleonCharm() ? "Active" : "Inactive";
            mTMPText["ChameleonCharmStatus"].text = $"Chameleon Charm: {chameleonStatus}";
        }
        
        Debug.Log($"[GameUI] Inventory refreshed: {currentGameState.inventory.Count} items");
    }
    
    #endregion
    
    #region Button Event Handlers (View -> Presenter)
    
    private void OnSpinButtonClicked()
    {
        Debug.Log("[GameUI] Spin button clicked");
        
        if (presenter != null)
            presenter.StartSpin();
    }
    
    private void OnResetButtonClicked()
    {
        Debug.Log("[GameUI] Reset button clicked");
        
        if (presenter != null)
            presenter.ResetGame();
    }
    
    private void OnItemButtonClicked(SpotItemType itemType)
    {
        Debug.Log($"[GameUI] Item button clicked: {itemType}");
        
        // Spot 선택 모드 활성화
        isSpotSelectionMode = true;
        currentItemType = itemType;
        selectedSpotID = -1;
    }
    
    private void OnCharmToggled(CharmType charmType)
    {
        Debug.Log($"[GameUI] Charm toggled: {charmType}");
        
        if (currentGameState == null) return;
        
        // Charm 아이템 찾기
        ItemData charmItem = null;
        if (charmType == CharmType.Death)
            charmItem = currentGameState.GetDeathCharm();
        else if (charmType == CharmType.Chameleon)
            charmItem = currentGameState.GetChameleonCharm();
            
        if (charmItem == null || charmItem.count <= 0)
        {
            Debug.LogWarning($"[GameUI] Charm {charmType} not available");
            return;
        }
        
        // Charm 사용 (타겟 없음)
        if (presenter != null)
        {
            presenter.UseItem(charmItem);
            Debug.Log($"[GameUI] Used Charm: {charmType}");
        }
    }
    
    #endregion
    
    #region Spot Click Handling
    
    public void OnSpotClicked(int spotID)
    {
        Debug.Log($"[GameUI] OnSpotClicked called with spotID: {spotID}");
        
        // currentGameState 체크
        if (currentGameState == null)
        {
            Debug.LogError("[GameUI] currentGameState is null! Cannot process spot click.");
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
        int totalChips = currentGameState.availableChips.GetTotalCount();
        Debug.Log($"[GameUI] Total chips available: {totalChips}");
        
        Debug.Log($"[GameUI] chipSelectionPopup != null: {chipSelectionPopup != null}, totalChips: {totalChips}");
        
        if (chipSelectionPopup != null && totalChips > 0)
        {
            Debug.Log($"[GameUI] Showing chip selection popup for spot {spotID}");
            
            // 박싱/언박싱 없이 구조체 사용
            var chipSelectionData = new ChipSelectionData(
                spotID,
                currentGameState.availableChips,
                (chipType, chipCount) =>
                {
                    if (presenter != null)
                    {
                        presenter.PlaceBet(BetType.Number, spotID, chipType, chipCount);
                    }
                    int chipValue = ChipTypeCache.Values[chipType];
                    Debug.Log($"[GameUI] Bet placed on Spot {spotID}: {chipCount}x ${chipValue} chips");
                }
            );
            
            Debug.Log($"[GameUI] Attempting to show ChipSelectionPopup...");
            UIManager.ShowPopup("ChipSelectionPopup", chipSelectionData);
            Debug.Log($"[GameUI] UIManager.ShowPopup call completed");
        }
        else if (totalChips <= 0)
        {
            Debug.LogWarning("[GameUI] No chips available!");
        }
        else
        {
            // 팝업 없으면 사용 가능한 첫 번째 칩 타입으로 1개 배팅
            ChipType firstAvailable = GetFirstAvailableChipType();
            if (currentGameState.availableChips.HasChip(firstAvailable, 1))
            {
                if (presenter != null)
                {
                    presenter.PlaceBet(BetType.Number, spotID, firstAvailable, 1);
                }
                Debug.Log($"[GameUI] Bet placed on Spot {spotID}: 1x ${ChipTypeCache.Values[firstAvailable]} chip");
            }
        }
    }
    
    private ChipType GetFirstAvailableChipType()
    {
        foreach (ChipType type in ChipTypeCache.AllTypes)
        {
            if (currentGameState.availableChips[type] > 0)
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
        string itemID = GetItemIDFromType(currentItemType);
        
        switch (currentItemType)
        {
            case SpotItemType.PlusSpot:
            case SpotItemType.UpgradedMultiSpot:
                // 단일 타겟 아이템 (ID로 요청)
                if (presenter != null)
                {
                    presenter.UseItemByID(itemID, spotID);
                }
                isSpotSelectionMode = false;
                Debug.Log($"[GameUI] {itemID} used on Spot {spotID}");
                break;
                
            case SpotItemType.CopySpot:
                // 2개 타겟 필요
                if (selectedSpotID == -1)
                {
                    selectedSpotID = spotID;
                    Debug.Log($"[GameUI] Copy from Spot {spotID} - select target");
                }
                else
                {
                    if (presenter != null)
                    {
                        presenter.UseItemByID(itemID, selectedSpotID, spotID);
                    }
                    Debug.Log($"[GameUI] Copy Spot: {selectedSpotID} -> {spotID}");
                    isSpotSelectionMode = false;
                    selectedSpotID = -1;
                }
                break;
        }
    }
    
    #endregion
    
    #region Game Event Display
    
    private void OnSpinStarted()
    {
        Debug.Log("[GameUI] Spin started - disable buttons");
        
        if (mButtons.ContainsKey("SpinButton"))
            mButtons["SpinButton"].interactable = false;
    }
    
    private void ShowResult(SpinResult result)
    {
        Debug.Log($"[GameUI] Showing result: Spot {result.winningSpotID}, Number {result.winningNumber}, Payout ${result.totalPayout:F2}");
        
        // 결과 텍스트 표시
        if (mTMPText.ContainsKey("ResultText"))
        {
            string resultMessage = result.hasWon
                ? $"WIN!\nSpot: {result.winningSpotID}\nNumber: {result.winningNumber}\nPayout: ${result.totalPayout:F2}"
                : $"LOSE\nSpot: {result.winningSpotID}\nNumber: {result.winningNumber}";
            
            mTMPText["ResultText"].text = resultMessage;
        }
        
        // 스핀 버튼 재활성화
        if (mButtons.ContainsKey("SpinButton"))
            mButtons["SpinButton"].interactable = true;
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
    
    /// <summary>
    /// BetObject 배팅 팝업 표시 처리
    /// </summary>
    private void HandleShowBetPopup(IOData data)
    {
        if (data is OData<BetObjectClickData> clickData)
        {
            BetObjectClickData betClickData = clickData.Get();
            ShowBetObjectPopup(betClickData);
        }
    }
    
    /// <summary>
    /// BetObject 클릭 처리 (직접 메시지 수신)
    /// </summary>
    private void OnBetObjectClicked(BetObjectClickData clickData)
    {
        Debug.Log($"[GameUI] OnBetObjectClicked: {clickData.betType} {clickData.targetValue}");
        ShowBetObjectPopup(clickData);
    }
    
    /// <summary>
    /// BetObject 배팅 팝업 표시
    /// </summary>
    private void ShowBetObjectPopup(BetObjectClickData clickData)
    {
        if (currentGameState == null)
        {
            Debug.LogError("[GameUI] currentGameState is null! Cannot show bet popup.");
            return;
        }
        
        int totalChips = currentGameState.availableChips.GetTotalCount();
        Debug.Log($"[GameUI] Total chips available: {totalChips}");
        
        if (chipSelectionPopup != null && totalChips > 0)
        {
            Debug.Log($"[GameUI] Showing chip selection popup for BetObject {clickData.objectID}");
            
            // 박싱/언박싱 없이 구조체 사용
            var chipSelectionData = new ChipSelectionData(
                clickData.objectID,
                currentGameState.availableChips,
                (chipType, chipCount) =>
                {
                    if (presenter != null)
                    {
                        presenter.PlaceBet(clickData.betType, clickData.targetValue, chipType, chipCount);
                    }
                    int chipValue = ChipTypeCache.Values[chipType];
                    double payout = Game.GetPayoutMultiplier(clickData.betType);
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
        if (currentGameState == null) return;
        
        // 해당 아이템이 있는지 확인
        ItemData item = currentGameState.inventory.Find(i => i.itemType == ItemType.SpotItem && i.spotItemType == itemType);
        if (item == null)
        {
            Debug.LogWarning($"[GameUI] Item {itemType} not found in inventory");
            return;
        }
        
        // 아이템 사용
        if (presenter != null)
        {
            if (targetSpotID.HasValue)
            {
                // 타겟이 있는 경우 (Copy 등)
                presenter.UseItem(item, spotID, targetSpotID.Value);
            }
            else
            {
                // 타겟이 없는 경우 (Plus 등)
                presenter.UseItem(item, spotID);
            }
            
            Debug.Log($"[GameUI] Used SpotItem: {itemType} on spot {spotID}");
        }
    }
    
    
    #endregion
}

