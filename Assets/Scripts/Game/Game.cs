using UnityEngine;
using GB;
using System;
using System.Collections.Generic;

/// <summary>
/// 게임 메인 컨트롤러 (Model + Controller + Presenter)
/// </summary>
public class Game : MonoBehaviour, IView
{
    #region Inspector Settings
    
    [Header("룰렛 시스템")]
    [SerializeField] private Board _board;
    [SerializeField] private WheelController _wheelController;
    [SerializeField] private BallController _ballController;
    
    
    [Header("게임 설정")]
    [SerializeField] private int maxTurns = 3;
    [SerializeField] private SpotSO spotDefinition;
    [SerializeField] private ItemTable itemTable;

    #endregion
    
    public static bool isPopupOpen = false;
    
    #region Presenter Constants
    
    public const string DOMAIN = "Game";
    
    // Presenter 메시지 키
    public static class Keys
    {
        // UI -> Game (Command)
        public const string CMD_START_SPIN = "CMD_START_SPIN";
        public const string CMD_PLACE_BET = "CMD_PLACE_BET";
        public const string CMD_REMOVE_BET = "CMD_REMOVE_BET";
        public const string CMD_USE_ITEM = "CMD_USE_ITEM";
        public const string CMD_USE_ITEM_BY_ID = "CMD_USE_ITEM_BY_ID";
        public const string CMD_RESET_GAME = "CMD_RESET_GAME";
        public const string CMD_SPOT_CLICKED = "CMD_SPOT_CLICKED";
        public const string CMD_BET_OBJECT_CLICKED = "CMD_BET_OBJECT_CLICKED";
        public const string CMD_SHOW_BET_POPUP = "CMD_SHOW_BET_POPUP";

        // Game -> UI (Update)
        public const string UPDATE_GAME_STATE = "UPDATE_GAME_STATE";
        public const string UPDATE_TURN_INFO = "UPDATE_TURN_INFO";
        public const string UPDATE_SPIN_START = "UPDATE_SPIN_START";
        public const string UPDATE_SPIN_COMPLETE = "UPDATE_SPIN_COMPLETE";
        public const string UPDATE_GAME_OVER = "UPDATE_GAME_OVER";
        public const string UPDATE_BET_LIST = "UPDATE_BET_LIST";
        public const string UPDATE_SPIN_BUTTON = "UPDATE_SPIN_BUTTON";
        public const string UPDATE_RESET_BUTTON = "UPDATE_RESET_BUTTON";
        public const string UPDATE_ITEM_USED = "UPDATE_ITEM_USED";
        public const string UPDATE_GAME_RESET = "UPDATE_GAME_RESET";
    }
    
    #endregion
        
    #region Private Fields
    
    // 게임 상태
    private GameState gameState;
    private bool isSpinning = false;  // 현재 스핀 중인지 여부
    private int currentWinningSpotID = -1; // 현재 스핀의 당첨 번호 (미리 결정됨)
    
    public bool IsSpinning => isSpinning; // 외부에서 스핀 상태 확인용
    
    // 아이템 관리
    private ItemManager itemManager;
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        Debug.Log("[Game] Awake called");
        
        _board.Init();
        InitializeGame();
    }
    
    private void Start()
    {
        Debug.Log("[Game] Start called");
        
        // 이벤트 구독
        if (_wheelController != null)
        {
            _wheelController.OnSpinComplete += OnSpinComplete;
            Debug.Log("[Game] WheelController event subscribed");
        }
        
        GB.Presenter.Send("GameUI", Keys.UPDATE_GAME_RESET, maxTurns);
        StartNewTurn();
    }

    private void Update()
    {
        // 게임 로직은 메시지 기반으로 처리
        // Update에서는 특별한 처리가 필요하지 않음
    }
    
    private void OnDestroy()
    {
        Debug.Log("[Game] OnDestroy called");
        
        // 이벤트 구독 해제
        if (_wheelController != null)
        {
            _wheelController.OnSpinComplete -= OnSpinComplete;
            Debug.Log("[Game] WheelController event unsubscribed");
        }
    }
    
    #endregion
    
    #region Game Initialization
    
    /// <summary>
    /// 게임 초기화
    /// </summary>
    private void InitializeGame()
    {
        Debug.Log("[Game] InitializeGame called");
        Debug.Log("[Game] ========== Initializing Game ==========");
        
        // ItemManager 초기화
        itemManager = new ItemManager(itemTable);
        
        // GameState 생성
        gameState = new GameState(spotDefinition);
        // currentBets는 이제 gameState에 포함됨
                
        // 초기 아이템 지급 (ItemManager 사용)
        if (itemTable != null)
        {
            // Charm 추가 (항상 보유)
            gameState.AddItem(itemManager.CreateItem("DEATH_CHARM", 1));
            gameState.AddItem(itemManager.CreateItem("CHAMELEON_CHARM", 1));
            
            // 기본 아이템 추가
            gameState.AddItem(itemManager.CreateItem("PLUS_SPOT", 3));
            gameState.AddItem(itemManager.CreateItem("COPY_SPOT", 2));
            gameState.AddItem(itemManager.CreateItem("UPGRADED_MULTI_SPOT", 1));
            gameState.AddItem(itemManager.CreateItem("HAT_WING", 2));
            Debug.Log("[Game] Initial items added using ItemManager");
        }
        else
        {
            Debug.LogWarning("[Game] ItemTable is null! No items will be initialized.");
        }
        
        UIManager.I.Init();
        
        Debug.Log($"[Game] Initialized: {gameState.spots.Count} spots, {gameState.inventory.Count} items");
        
        // Board와 데이터 연결
        if (_board != null)
            _board.ConnectData(gameState.spots);
            
        // 게임 상태 변경 메시지 전송
        GB.Presenter.Send("GameUI", Keys.UPDATE_GAME_STATE, gameState);
    }
    
    
    /// <summary>
    /// 게임 리셋
    /// </summary>
    public void ResetGame()
    {
        Debug.Log("[Game] ResetGame called");
        Debug.Log("[Game] ========== Resetting Game ==========");
        
        // 게임 리셋 메시지 전송 (maxTurns 포함)
        GB.Presenter.Send("GameUI", Keys.UPDATE_GAME_RESET, maxTurns);
        
        InitializeGame();
        StartNewTurn();
    }
    
    #endregion
    
    #region Turn Management
    
    /// <summary>
    /// 새 턴 시작
    /// </summary>
    private void StartNewTurn()
    {
        Debug.Log($"[Game] StartNewTurn called - Turn {gameState.currentTurn + 1}");
        
        // 게임 종료 체크
        if (gameState.currentTurn > maxTurns)
        {
            Debug.Log("[Game] Max turns reached, ending game");
            EndGame();
            return;
        }
        
        Debug.Log($"[Game] Clearing current bets");
        gameState.currentBets.Clear();
        
        // 당첨 번호 리셋
        currentWinningSpotID = -1;
        
        Debug.Log($"[Game] Before StartNewTurn - availableChips: {gameState.availableChips.ToString()}");
        
        // 칩 & Spot 리셋
        gameState.StartNewTurn();
        
        Debug.Log($"[Game] After StartNewTurn - availableChips: {gameState.availableChips.ToString()}");
        
        gameState.ResetSpotsForNewTurn();
        
        Debug.Log($"[Game] ========== Turn {gameState.currentTurn} / {maxTurns} Started ==========");
        Debug.Log($"[Game] Available Chips: {gameState.availableChips.ToString()}");
        
        // 게임 상태 변경 메시지 전송
        GB.Presenter.Send("GameUI", Keys.UPDATE_GAME_STATE, gameState);
        
        // 스핀 버튼 비활성화 (배팅이 없으므로)
        GB.Presenter.Send("GameUI", Keys.UPDATE_SPIN_BUTTON, false);
        Debug.Log($"[Game] New turn started - spin button disabled (no bets)");
        
        // 리셋 버튼 활성화 (턴 시작 시 항상 활성화)
        GB.Presenter.Send("GameUI", Keys.UPDATE_RESET_BUTTON, true);
    }
    
    /// <summary>
    /// 게임 종료
    /// </summary>
    private void EndGame()
    {
        Debug.Log("[Game] EndGame called");
        Debug.Log("[Game] ========== Game Over ==========");
        
        // 총 획득 금액 계산
        double totalEarned = 0.0;
        foreach (var turnData in gameState.turnHistory)
        {
            totalEarned += turnData.totalPayout;
        }
        
        Debug.Log($"[Game] Total Earned: ${totalEarned:F2}");
        Debug.Log($"[Game] Press R to restart");
        
        // 게임 종료 메시지 전송
        GB.Presenter.Send("GameUI", Keys.UPDATE_GAME_OVER, totalEarned);
    }
    
    #endregion
    
    #region Betting
    
    /// <summary>
    /// 현재 배팅 목록 가져오기
    /// </summary>
    public List<BetData> GetCurrentBets()
    {
        return gameState.currentBets;
    }
    
    /// <summary>
    /// 배팅 제거
    /// </summary>
    public void RemoveBet(BetData bet)
    {
        Debug.Log($"[Game] RemoveBet called for bet: {bet?.betType} on {bet?.targetValue}");
        
        if (bet == null)
        {
            Debug.LogWarning("[Game] BetData is null!");
            return;
        }
        
        // 베팅 목록에서 해당 베팅 찾기
        int betIndex = gameState.currentBets.IndexOf(bet);
        if (betIndex == -1)
        {
            Debug.LogWarning($"[Game] Bet not found in current bets: {bet.betType} {bet.targetValue}");
            return;
        }
        
        Debug.Log($"[Game] Removing bet: {bet.betType} {bet.targetValue}");
        
        // 칩을 다시 플레이어에게 반환
        foreach (ChipType chipType in ChipTypeCache.AllTypes)
        {
            int chipCount = bet.chips[chipType];
            if (chipCount > 0)
            {
                gameState.availableChips.AddChip(chipType, chipCount);
                Debug.Log($"[Game] Returned {chipType} x{chipCount} to player");
            }
        }
        
        // 베팅 제거
        gameState.currentBets.RemoveAt(betIndex);
        
        Debug.Log($"[Game] Bet removed successfully. Remaining bets: {gameState.currentBets.Count}");
        
        // 배팅 리스트 변경 메시지 전송
        GB.Presenter.Send("GameUI", Keys.UPDATE_BET_LIST, gameState);
        GB.Presenter.Send("GameUI", Keys.UPDATE_TURN_INFO, gameState);
        
        // 배팅이 0개면 스핀 버튼 비활성화
        if (gameState.currentBets.Count == 0)
        {
            GB.Presenter.Send("GameUI", Keys.UPDATE_SPIN_BUTTON, false);
            Debug.Log($"[Game] No bets remaining - spin button disabled");
        }
    }
    
    /// <summary>
    /// ItemManager로 아이템 생성 (ID 기반)
    /// </summary>
    public ItemData CreateItem(string itemID, int count = -1)
    {
        return itemManager?.CreateItem(itemID, count);
    }
    
    /// <summary>
    /// 배팅 추가
    /// </summary>
    public void PlaceBet(BetType betType, int targetValue, ChipType chipType, int chipCount)
    {
        Debug.Log($"[Game] PlaceBet called: {betType} on {targetValue}, {chipType} x{chipCount}");
        
        // 이미 배팅이 있는지 확인 (한 턴에 한 번만 배팅 가능)
        if (gameState.currentBets.Count > 0)
        {
            Debug.LogWarning($"[Game] Already placed a bet this turn! Only one bet allowed per turn.");
            return;
        }
        
        // 칩 보유 여부 확인
        if (!gameState.availableChips.HasChip(chipType, chipCount))
        {
            Debug.LogWarning($"[Game] Not enough chips! Available: {gameState.availableChips[chipType]}, Needed: {chipCount}");
            return;
        }
        
        // 배팅 유효성 검사
        if (!IsValidBet(betType, targetValue))
        {
            Debug.LogWarning($"[Game] Invalid bet: {betType} on {targetValue}");
            return;
        }
        
        // 배팅 생성
        BetData bet = new BetData(betType, targetValue);
        bet.AddChip(chipType, chipCount);
        gameState.currentBets.Add(bet);
        
        // 칩 차감
        Debug.Log($"[Game] Before RemoveChip: {gameState.availableChips.ToString()}");
        gameState.availableChips.RemoveChip(chipType, chipCount);
        Debug.Log($"[Game] After RemoveChip: {gameState.availableChips.ToString()}");
        
        int chipValue = ChipTypeCache.Values[chipType];
        Debug.Log($"[Game] Bet placed: {betType} {targetValue} | {chipCount}x ${chipValue} chips");
        
        // 배팅 리스트 변경 메시지 전송
        GB.Presenter.Send("GameUI", Keys.UPDATE_BET_LIST, gameState);
        GB.Presenter.Send("GameUI", Keys.UPDATE_TURN_INFO, gameState);
        
        // 스핀 버튼 활성화 (배팅이 있으므로)
        Debug.Log($"<color=cyan>[Game] Sending UPDATE_SPIN_BUTTON with value: true</color>");
        GB.Presenter.Send("GameUI", Keys.UPDATE_SPIN_BUTTON, true);
        Debug.Log($"<color=cyan>[Game] UPDATE_SPIN_BUTTON sent successfully</color>");
    }
    
    /// <summary>
    /// 배팅 유효성 검사
    /// </summary>
    private bool IsValidBet(BetType betType, int targetValue)
    {
        Debug.Log($"[Game] IsValidBet called: {betType} on {targetValue}");
        
        switch (betType)
        {
            case BetType.Number:
                return targetValue >= 1 && targetValue <= 36;
            case BetType.Color:
                return targetValue == 0 || targetValue == 1; // 0=Red, 1=Black
            case BetType.OddEven:
                return targetValue == 0 || targetValue == 1; // 0=Odd, 1=Even
            case BetType.HighLow:
                return targetValue == 0 || targetValue == 1; // 0=Low(1-18), 1=High(19-36)
            case BetType.Dozen:
                return targetValue >= 1 && targetValue <= 3; // 1=1-12, 2=13-24, 3=25-36
            case BetType.Column:
                return targetValue >= 1 && targetValue <= 3; // 1,2,3 열
            default:
                return false;
        }
    }
    
    
    /// <summary>
    /// 아이템 사용
    /// </summary>
    public void UseItem(ItemData item, params int[] targetIDs)
    {
        Debug.Log($"[Game] UseItem called: {item.itemType} with targets: [{string.Join(", ", targetIDs)}]");
        
        switch (item.itemType)
        {
            case ItemType.SpotItem:
                UseSpotItem(item, targetIDs);
                break;
                
            case ItemType.ChipItem:
                UseChipItem(item, targetIDs);
                break;
                
            default:
                Debug.LogWarning($"[Game] Cannot use item: {item.itemType}");
                break;
        }
    }
    
    private void UseSpotItem(ItemData item, int[] targetIDs)
    {
        Debug.Log($"[Game] UseSpotItem called: {item.spotItemType} with targets: [{string.Join(", ", targetIDs)}]");
        
        switch (item.spotItemType)
        {
            case SpotItemType.PlusSpot:
                if (targetIDs.Length >= 1)
                {
                    SpotCalculator.ApplyPlusSpot(gameState, item, targetIDs[0]);
                    SpotCalculator.RecalculateAll(gameState);
                    
                    // Board 갱신 및 상태 메시지 전송
                    if (_board != null)
                        _board.UpdateAllVisuals();
                    GB.Presenter.Send("GameUI", Keys.UPDATE_GAME_STATE, gameState);
                }
                break;
                
            case SpotItemType.CopySpot:
                if (targetIDs.Length >= 2)
                {
                    SpotCalculator.ApplyCopySpot(gameState, item, targetIDs[0], targetIDs[1]);
                    SpotCalculator.RecalculateAll(gameState);
                    
                    // Board 갱신 및 상태 메시지 전송
                    if (_board != null)
                        _board.UpdateAllVisuals();
                    GB.Presenter.Send("GameUI", Keys.UPDATE_GAME_STATE, gameState);
                }
                break;
                
            case SpotItemType.UpgradedMultiSpot:
                if (targetIDs.Length >= 1)
                {
                    SpotCalculator.ApplyUpgradedMultiSpot(gameState, item, targetIDs[0]);
                    SpotCalculator.RecalculateAll(gameState);
                    
                    // Board 갱신 및 상태 메시지 전송
                    if (_board != null)
                        _board.UpdateAllVisuals();
                    GB.Presenter.Send("GameUI", Keys.UPDATE_GAME_STATE, gameState);
                }
                break;
        }
    }
    
    private void UseChipItem(ItemData item, int[] targetIDs)
    {
        Debug.Log($"[Game] UseChipItem called: {item.chipItemType} with targets: [{string.Join(", ", targetIDs)}]");
        
        if (item.chipItemType == ChipItemType.HatWing)
        {
            // Wing은 배팅에 적용
            // TODO: UI에서 배팅 선택 후 적용하도록 수정
            Debug.LogWarning("[Game] Wing item requires bet selection");
        }
    }
    
    #endregion
    
    #region Spin & Result
    
    /// <summary>
    /// 스핀 시작 (public으로 UI에서 호출 가능)
    /// </summary>
    public void StartSpin()
    {
        Debug.Log("[Game] StartSpin called");
        
        if (isSpinning)
        {
            Debug.LogWarning("[Game] Already spinning!");
            return;
        }                                                                                                                                                                                                                                                                                                                                                                                       
        
        if (gameState.currentBets.Count == 0)
        {
            Debug.LogWarning("[Game] No bets placed! Please place at least one bet.");
            return;
        }
        
        // WheelController 없어도 계산은 진행 (룰렛 비주얼 없이도 테스트 가능)
        isSpinning = true;
        
        Debug.Log($"[Game] ========== Spin Started (Turn {gameState.currentTurn}) ==========");
        
        // 전체 재계산
        SpotCalculator.RecalculateAll(gameState);
        
        // 확률/배당 출력
        SpotCalculator.PrintProbabilityDistribution(gameState.spots);
        SpotCalculator.PrintPayoutDistribution(gameState.spots);
        
        // 당첨 번호 추첨 (한 번만!)
        currentWinningSpotID = SpotCalculator.DetermineWinner(gameState);
        Spot winningSpot = gameState.spots[currentWinningSpotID];
        
        Debug.Log($"<color=yellow>[Game] ★ Winning Spot Determined: {currentWinningSpotID} (Number: {winningSpot.currentNumber}) ★</color>");
        
        // 스핀 시작 메시지 전송
        GB.Presenter.Send("GameUI", Keys.UPDATE_SPIN_START);
        
        // 스핀 버튼 비활성화
        GB.Presenter.Send("GameUI", Keys.UPDATE_SPIN_BUTTON, false);
        
        // 룰렛과 공 스핀 시작 (있으면)
        if (_wheelController != null && _ballController != null)
        {
            _wheelController.StartSpin(currentWinningSpotID);
        }
        else
        {
            // 룰렛 비주얼 없으면 바로 결과 처리
            Debug.LogWarning("[Game] No WheelController - processing result immediately");
            OnSpinComplete();
        }
    }
    
    /// <summary>
    /// 스핀 완료 콜백
    /// </summary>
    private void OnSpinComplete()
    {
        Debug.Log("[Game] OnSpinComplete called");
        Debug.Log("[Game] ========== Spin Complete ==========");
        
        // 이미 결정된 당첨 번호 사용 (DetermineWinner 다시 호출 안 함!)
        int winningSpotID = currentWinningSpotID;
        Spot winningSpot = gameState.spots[winningSpotID];
        
        Debug.Log($"<color=yellow>[Game] Using pre-determined winning spot: {winningSpotID}</color>");
        
        // 당첨된 배팅 찾기
        List<BetData> winningBets = new List<BetData>();
        foreach (var bet in gameState.currentBets)
        {
            if (SpotCalculator.IsBetWin(bet, winningSpotID, winningSpot))
            {
                winningBets.Add(bet);
                Debug.Log($"[Game] Winning bet: {bet.betType} on {bet.targetValue}");
            }
        }
        
        // 배당 계산
        float totalPayout = SpotCalculator.CalculateTotalPayout(gameState, winningSpotID, gameState.currentBets);
        
        Debug.Log($"[Game] Total payout calculated: ${totalPayout:F2}");
        Debug.Log($"[Game] Winning bets count: {winningBets.Count}");
        Debug.Log($"[Game] Before payout - availableChips: {gameState.availableChips.ToString()}");
        
        // 배당금이 있으면 칩으로 지급
        if (totalPayout > 0)
        {
            Debug.Log($"[Game] WIN! Paying out ${totalPayout:F2}");
            
            // totalPayout을 칩으로 변환하여 지급
            int payoutInt = (int)totalPayout;
            
            // 큰 칩부터 지급 (Chip100 -> Chip50 -> Chip10 -> Chip5 -> Chip1)
            foreach (ChipType chipType in ChipTypeCache.AllTypesReversed)
            {
                int chipValue = ChipTypeCache.Values[chipType];
                int chipCount = payoutInt / chipValue;
                
                if (chipCount > 0)
                {
                    gameState.availableChips.AddChip(chipType, chipCount);
                    payoutInt -= chipCount * chipValue;
                    Debug.Log($"[Game] Added {chipType} x{chipCount} (${chipCount * chipValue})");
                }
            }
            
            Debug.Log($"[Game] After payout - availableChips: {gameState.availableChips.ToString()}");
        }
        else
        {
            Debug.Log($"[Game] LOSE! No payout");
            Debug.Log($"[Game] After loss - availableChips: {gameState.availableChips.ToString()}");
        }
        
        // Death Charm 처리
        SpotCalculator.ProcessDeathCharm(gameState, winningSpotID);
        
        // Board 비주얼 업데이트 (파괴된 Spot 표시)
        if (_board != null)
            _board.UpdateAllVisuals();
        
        // Turn 데이터 저장
        TurnData turnData = new TurnData(gameState.currentTurn);
        turnData.bets = new List<BetData>(gameState.currentBets); // 복사
        turnData.winningSpotID = winningSpotID;
        turnData.winningNumber = winningSpot.currentNumber;
        turnData.totalPayout = totalPayout;
        turnData.SaveSpotSnapshot(gameState.spots);
        gameState.turnHistory.Add(turnData);
        
        Debug.Log($"[Game] Total Payout: ${totalPayout:F2}");
        
        // 스핀 완료 메시지 전송 (모든 배팅 + 당첨 배팅 + 색상 포함)
        SpinResult spinResult = new SpinResult(
            winningSpotID, 
            winningSpot.currentNumber, 
            winningSpot.currentColor, 
            totalPayout, 
            new List<BetData>(gameState.currentBets), // 모든 배팅 복사
            winningBets // 당첨 배팅
        );
        GB.Presenter.Send("GameUI", Keys.UPDATE_SPIN_COMPLETE, spinResult);
        GB.Presenter.Send("GameUI", Keys.UPDATE_TURN_INFO, gameState);
        
        isSpinning = false;
        
        // 다음 턴
        StartNewTurn();
    }
    
    #endregion
    
    #region Debug & Test
    
    /// <summary>
    /// 테스트: 배팅 + 스핀
    /// </summary>
    private void TestBettingAndSpin()
    {
        Debug.Log("[Game] TestBettingAndSpin called");
        Debug.Log("[Game] ========== Test Betting ==========");
        
        // 테스트 아이템 사용
        Debug.Log("[Game] Using test items");
        ItemData plusSpot = itemManager.CreateItem("PLUS_SPOT");
        Debug.Log("[Game] Created PLUS_SPOT item");
        UseItem(plusSpot, 5); // Spot 5를 6으로
        Debug.Log("[Game] PLUS_SPOT item used");
        
        ItemData upgradeMulti = itemManager.CreateItem("UPGRADED_MULTI_SPOT");
        Debug.Log("[Game] Created UPGRADED_MULTI_SPOT item");
        UseItem(upgradeMulti, 10); // Spot 10 + 인접 강화
        Debug.Log("[Game] UPGRADED_MULTI_SPOT item used");
        
        // 테스트 배팅 - 다양한 타입
        Debug.Log("[Game] Placing test bets");
        PlaceBet(BetType.Number, 6, ChipType.Chip1, 2);     // 숫자 배팅 (x36)
        PlaceBet(BetType.Color, 0, ChipType.Chip1, 1);      // 빨간색 배팅 (x2)
        PlaceBet(BetType.OddEven, 1, ChipType.Chip1, 1);    // 짝수 배팅 (x2)
        PlaceBet(BetType.HighLow, 0, ChipType.Chip1, 1);    // Low(1-18) 배팅 (x2)
        PlaceBet(BetType.Dozen, 1, ChipType.Chip1, 1);      // 1-12 배팅 (x3)
        PlaceBet(BetType.Column, 2, ChipType.Chip1, 1);     // 2열 배팅 (x3)
        Debug.Log("[Game] All test bets placed");
        
        // 스핀
        Debug.Log("[Game] Starting test spin");
        StartSpin();
        Debug.Log("[Game] Test spin started");
    }
    
    #endregion
    
    #region Public Getters
    
    public GameState GetGameState()
    {
        Debug.Log($"[Game] GetGameState called - Turn {gameState?.currentTurn ?? -1}");
        return gameState;
    }
    
    #endregion
    
    #region Bet Management
    
    /// <summary>
    /// 모든 베팅 제거
    /// </summary>
    public void RemoveAllBets()
    {
        Debug.Log("[Game] RemoveAllBets called");
        
        int betCount = gameState.currentBets.Count;
        Debug.Log($"[Game] Removing all {betCount} bets");
        
        // 모든 베팅의 칩을 플레이어에게 반환
        foreach (BetData bet in gameState.currentBets)
        {
            foreach (ChipType chipType in ChipTypeCache.AllTypes)
            {
                int chipCount = bet.chips[chipType];
                if (chipCount > 0)
                {
                    gameState.availableChips.AddChip(chipType, chipCount);
                }
            }
        }
        
        // 모든 베팅 제거
        gameState.currentBets.Clear();
        
        Debug.Log($"[Game] All bets removed successfully");
        
        // UI 업데이트 메시지 전송
        GB.Presenter.Send("GameUI", Keys.UPDATE_BET_LIST, gameState);
        GB.Presenter.Send("GameUI", Keys.UPDATE_TURN_INFO, gameState);
    }
    
    #endregion
    
    #region IView Implementation
    
    private void OnEnable()
    {
        // Presenter에 바인딩
        Debug.Log($"[Game] OnEnable - Binding to domain: {DOMAIN}");
        GB.Presenter.Bind(DOMAIN, this);
        Debug.Log($"[Game] Binding complete - Game is ready to receive messages");
    }

    private void OnDisable()
    {
        // Presenter에서 언바인딩
        Debug.Log($"[Game] OnDisable - Unbinding from domain: {DOMAIN}");
        GB.Presenter.UnBind(DOMAIN, this);
        Debug.Log($"[Game] Unbinding complete - Game is no longer receiving messages");
    }
    
    /// <summary>
    /// Presenter에서 받는 메시지 처리 (IView 구현)
    /// </summary>
    public void ViewQuick(string key, IOData data)
    {
        Debug.Log($"[Game] ViewQuick called with key: {key}, data: {data?.GetType()}");
        
        switch (key)
        {
            case Keys.CMD_SPOT_CLICKED:
                HandleSpotClicked(data);
                break;
                
            case Keys.CMD_BET_OBJECT_CLICKED:
                HandleBetObjectClicked(data);
                break;
                
            case Keys.CMD_PLACE_BET:
                HandlePlaceBet(data);
                break;
                
            case Keys.CMD_REMOVE_BET:
                HandleRemoveBet(data);
                break;
                
            case Keys.CMD_USE_ITEM:
                HandleUseItem(data);
                break;
                
            case Keys.CMD_USE_ITEM_BY_ID:
                HandleUseItemByID(data);
                break;
                
            case Keys.CMD_START_SPIN:
                StartSpin();
                break;
                
            case Keys.CMD_RESET_GAME:
                ResetGame();
                break;
                
            case Keys.CMD_SHOW_BET_POPUP:
                HandleShowBetPopup(data);
                break;
        }
    }
    
    #endregion
    
    #region Message Handlers
    
    /// <summary>
    /// Spot 클릭 처리 - 팝업 표시 요청
    /// </summary>
    private void HandleSpotClicked(IOData data)
    {
        Debug.Log($"[Game] HandleSpotClicked called with data: {data?.GetType()}");
        
        if (data is OData<int> spotData)
        {
            int spotID = spotData.Get();
            Debug.Log($"[Game] Spot clicked: {spotID}");
            GameState gameState = GetGameState();
            
            if (gameState == null)
            {
                Debug.LogError("[Game] GameState is null!");
                return;
            }
            
            // 팝업 직접 표시
            ShowChipSelectionPopup(spotID, gameState);
            Debug.Log($"[Game] Spot {spotID} click processed, popup shown directly");
        }
        else
        {
            Debug.LogError($"[Game] Invalid data type for CMD_SPOT_CLICKED: {data?.GetType()}");
        }
    }
    
    /// <summary>
    /// BetObject 클릭 처리 - 팝업 직접 표시
    /// </summary>
    private void HandleBetObjectClicked(IOData data)
    {
        Debug.Log($"[Game] HandleBetObjectClicked called with data: {data?.GetType()}");
        
        if (data is OData<BetObjectClickData> clickData)
        {
            BetObjectClickData betData = clickData.Get();
            Debug.Log($"[Game] BetObject clicked: {betData.objectID}, Type: {betData.betType}, Value: {betData.targetValue}");
            GameState gameState = GetGameState();
            
            if (gameState == null)
            {
                Debug.LogError("[Game] GameState is null!");
                return;
            }
            
            // 칩 선택 팝업 직접 표시
            ShowChipSelectionPopupForBetObject(betData, gameState);
            Debug.Log($"[Game] BetObject {betData.betType} {betData.targetValue} popup shown directly");
        }
        else
        {
            Debug.LogError($"[Game] Invalid data type for CMD_BET_OBJECT_CLICKED: {data?.GetType()}");
        }
    }
    
    /// <summary>
    /// 배팅 처리
    /// </summary>
    private void HandlePlaceBet(IOData data)
    {
        Debug.Log($"[Game] HandlePlaceBet called with data: {data?.GetType()}");
        
        if (data is OData<object[]> paramData)
        {
            object[] parameters = paramData.Get();
            if (parameters.Length >= 4)
            {
                BetType betType = (BetType)parameters[0];
                int targetValue = (int)parameters[1];
                ChipType chipType = (ChipType)parameters[2];
                int chipCount = (int)parameters[3];
                Debug.Log($"[Game] Placing bet: {betType} on {targetValue}, {chipType} x{chipCount}");
                PlaceBet(betType, targetValue, chipType, chipCount);
            }
        }
        else
        {
            Debug.LogError($"[Game] Invalid data type for CMD_PLACE_BET: {data?.GetType()}");
        }
    }
    
    /// <summary>
    /// 배팅 제거 처리
    /// </summary>
    private void HandleRemoveBet(IOData data)
    {
        Debug.Log($"[Game] HandleRemoveBet called with data: {data?.GetType()}");
        
        if (data is OData<BetData> betData)
        {
            RemoveBet(betData.Get());
        }
        else
        {
            Debug.LogError($"[Game] Invalid data type for CMD_REMOVE_BET: {data?.GetType()}");
        }
    }
    
    /// <summary>
    /// 아이템 사용 처리
    /// </summary>
    private void HandleUseItem(IOData data)
    {
        Debug.Log($"[Game] HandleUseItem called with data: {data?.GetType()}");
        
        if (data is OData<ItemData> itemData)
        {
            ItemData item = itemData.Get();
            Debug.Log($"[Game] Using item: {item.itemType}");
            UseItem(item);
        }
        else
        {
            Debug.LogError($"[Game] Invalid data type for CMD_USE_ITEM: {data?.GetType()}");
        }
    }
    
    /// <summary>
    /// ID로 아이템 사용 처리
    /// </summary>
    private void HandleUseItemByID(IOData data)
    {
        Debug.Log($"[Game] HandleUseItemByID called with data: {data?.GetType()}");
        
        if (data is OData<object[]> paramData)
        {
            object[] parameters = paramData.Get();
            if (parameters.Length >= 2)
            {
                string itemID = (string)parameters[0];
                int[] targetIDs = (int[])parameters[1];
                Debug.Log($"[Game] Using item by ID: {itemID} with targets: [{string.Join(", ", targetIDs)}]");
                UseItemByID(itemID, targetIDs);
            }
        }
        else
        {
            Debug.LogError($"[Game] Invalid data type for CMD_USE_ITEM_BY_ID: {data?.GetType()}");
        }
    }
    
    /// <summary>
    /// ID로 아이템 사용
    /// </summary>
    public void UseItemByID(string itemID, params int[] targetIDs)
    {
        Debug.Log($"[Game] UseItemByID called: {itemID} with targets: [{string.Join(", ", targetIDs)}]");
        
        if (itemManager == null)
        {
            Debug.LogError("[Game] ItemManager is null!");
            return;
        }
        
        ItemData item = itemManager.CreateItem(itemID);
        if (item == null)
        {
            Debug.LogWarning($"[Game] Item not found: {itemID}");
            return;
        }
        
        UseItem(item, targetIDs);
    }
    
    /// <summary>
    /// 베팅 팝업 표시 처리
    /// </summary>
    private void HandleShowBetPopup(IOData data)
    {
        Debug.Log($"[Game] HandleShowBetPopup called with data: {data?.GetType()}");
        
        if (data is OData<object[]> paramData)
        {
            object[] parameters = paramData.Get();
            if (parameters.Length >= 2)
            {
                BetType betType = (BetType)parameters[0];
                int targetValue = (int)parameters[1];
                Debug.Log($"[Game] Showing bet popup: {betType} on {targetValue}");
                ShowBetPopup(betType, targetValue);
            }
        }
    }
    
    #endregion
    
    #region Popup Management
    
    /// <summary>
    /// 칩 선택 팝업 표시 (Spot용)
    /// </summary>
    private void ShowChipSelectionPopup(int spotID, GameState gameState)
    {
        Debug.Log($"[Game] Showing chip selection popup for Spot {spotID}");
        
        // UIManager를 통해 팝업 표시
        ChipSelectionData chipData = new ChipSelectionData(spotID, gameState.availableChips, OnChipSelected);
        
        UIManager.ShowPopup("ChipSelectionPopup", chipData);
        Game.isPopupOpen = true;
    }
    
    /// <summary>
    /// 칩 선택 팝업 표시 (BetObject용)
    /// </summary>
    private void ShowChipSelectionPopupForBetObject(BetObjectClickData betData, GameState gameState)
    {
        Debug.Log($"[Game] Showing chip selection popup for BetObject: {betData.betType} on {betData.targetValue}");
        
        // BetObject용 콜백을 사용하여 팝업 표시
        ChipSelectionData chipData = new ChipSelectionData(
            betData.targetValue,
            gameState.availableChips,
            (targetValue, chipType, chipCount) =>
            {
                Debug.Log($"[Game] BetObject chip selected: {betData.betType} on {targetValue}, {chipType} x{chipCount}");
                
                // 배팅 처리
                PlaceBet(betData.betType, betData.targetValue, chipType, chipCount);
                
                Game.isPopupOpen = false;
            }
        );
        
        UIManager.ShowPopup("ChipSelectionPopup", chipData);
        Game.isPopupOpen = true;
    }
    
    /// <summary>
    /// 칩 선택 완료 콜백
    /// </summary>
    private void OnChipSelected(int SpotID, ChipType chipType, int chipCount)
    {
        Debug.Log($"[Game] OnChipSelected called: Spot {SpotID}, Chip {chipType} x{chipCount}");
        
        // 배팅 처리
        GameState gameState = GetGameState();
        if (gameState == null)
        {
            Debug.LogError("[Game] GameState is null!");
            return;
        }
        
        // PlaceBet 호출 (통일된 로직 사용)
        PlaceBet(BetType.Number, SpotID, chipType, chipCount);
        
        Game.isPopupOpen = false;
    }
    
    /// <summary>
    /// 베팅 팝업 표시
    /// </summary>
    private void ShowBetPopup(BetType betType, int targetValue)
    {
        Debug.Log($"[Game] ShowBetPopup called: {betType} on {targetValue}");
        // TODO: 베팅 팝업 표시 로직 구현
    }
    
    #endregion
    
    #endregion
}
