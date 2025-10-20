using UnityEngine;
using GB;
using System;
using System.Collections.Generic;

/// <summary>
/// 게임 메인 컨트롤러 (Model + Controller)
/// </summary>
public class Game : MonoBehaviour
{
    #region Inspector Settings
    
    [Header("룰렛 시스템")]
    [SerializeField] private Board _board;
    [SerializeField] private WheelController _wheelController;
    [SerializeField] private BallController _ballController;
    
    [Header("MVP 시스템")]
    [SerializeField] private GamePresenter _gamePresenter;
    
    [Header("게임 설정")]
    [SerializeField] private int maxTurns = 3;
    [SerializeField] private SpotSO spotDefinition;
    [SerializeField] private ItemTable itemTable;

    #endregion
    
    #region Events (Game -> Presenter)
    
    public event Action<GameState> OnGameStateChanged;
    public event Action OnSpinStarted;
    public event Action<SpinResult> OnSpinCompleted;
    public event Action<double> OnGameOver;
    public event Action OnBetListChanged;
    
    #endregion
    
    #region Private Fields
    
    // 게임 상태
    private GameState gameState;
    private List<BetData> currentBets;
    private bool isSpinning = false;  // 현재 스핀 중인지 여부
    
    // 아이템 관리
    private ItemManager itemManager;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {        
        _board.Init();
        InitializeGame();
    }
    
    private void Start()
    {
        // 이벤트 구독
        if (_wheelController != null)
        {
            _wheelController.OnSpinComplete += OnSpinComplete;
        }
        
        StartNewTurn();
    }

    private void Update()
    {
        
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_wheelController != null)
        {
            _wheelController.OnSpinComplete -= OnSpinComplete;
        }
    }
    
    #endregion
    
    #region Game Initialization
    
    /// <summary>
    /// 게임 초기화
    /// </summary>
    private void InitializeGame()
    {
        Debug.Log("[Game] ========== Initializing Game ==========");
        
        // ItemManager 초기화
        itemManager = new ItemManager(itemTable);
        
        // GameState 생성
        gameState = new GameState(spotDefinition);
        currentBets = new List<BetData>();
                
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
        
        Debug.Log($"[Game] Initialized: {gameState.spots.Count} spots, {gameState.inventory.Count} items");
        
        // Board와 데이터 연결
        if (_board != null)
            _board.ConnectData(gameState.spots);
            
        // 게임 상태 변경 이벤트 발행
        OnGameStateChanged?.Invoke(gameState);
    }
    
    
    /// <summary>
    /// 게임 리셋
    /// </summary>
    public void ResetGame()
    {
        Debug.Log("[Game] ========== Resetting Game ==========");
        
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
        // 게임 종료 체크
        if (gameState.currentTurn > maxTurns)
        {
            EndGame();
            return;
        }
        
        currentBets.Clear();
        
        // 칩 & Spot 리셋
        gameState.StartNewTurn();
        gameState.ResetSpotsForNewTurn();
        
        Debug.Log($"[Game] ========== Turn {gameState.currentTurn} / {maxTurns} Started ==========");
        Debug.Log($"[Game] Available Chips: {gameState.availableChips.ToString()}");
        
        // 게임 상태 변경 이벤트 발행
        OnGameStateChanged?.Invoke(gameState);
    }
    
    /// <summary>
    /// 게임 종료
    /// </summary>
    private void EndGame()
    {
        Debug.Log("[Game] ========== Game Over ==========");
        
        // 총 획득 금액 계산
        double totalEarned = 0.0;
        foreach (var turnData in gameState.turnHistory)
        {
            totalEarned += turnData.totalPayout;
        }
        
        Debug.Log($"[Game] Total Earned: ${totalEarned:F2}");
        Debug.Log($"[Game] Press R to restart");
        
        // 게임 종료 이벤트 발행
        OnGameOver?.Invoke(totalEarned);
    }
    
    #endregion
    
    #region Betting
    
    /// <summary>
    /// 현재 배팅 목록 가져오기
    /// </summary>
    public List<BetData> GetCurrentBets()
    {
        return currentBets;
    }
    
    /// <summary>
    /// 배팅 제거
    /// </summary>
    public void RemoveBet(BetData bet)
    {
        if (currentBets.Contains(bet))
        {
            // 칩 반환
            int chipValue = bet.GetTotalChipValue();

            int chipCount = chipValue / 10;
            gameState.availableChips.AddChip(ChipType.Chip1, chipCount);
            
            currentBets.Remove(bet);
            
            Debug.Log($"[Game] Bet removed, {chipValue} chip(s) returned");
            
            // 배팅 리스트 변경 이벤트 발행
            OnBetListChanged?.Invoke();
            OnGameStateChanged?.Invoke(gameState);
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
        currentBets.Add(bet);
        
        // 칩 차감
        gameState.availableChips.RemoveChip(chipType, chipCount);
        
        int chipValue = ChipTypeCache.Values[chipType];
        double payout = GetPayoutMultiplier(betType);
        Debug.Log($"[Game] Bet placed: {betType} {targetValue} | {chipCount}x ${chipValue} chips (Payout: x{payout})");
        
        // 배팅 리스트 변경 이벤트 발행
        OnBetListChanged?.Invoke();
        OnGameStateChanged?.Invoke(gameState);
    }
    
    /// <summary>
    /// 배팅 유효성 검사
    /// </summary>
    private bool IsValidBet(BetType betType, int targetValue)
    {
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
    /// 배당률 계산
    /// </summary>
    public static double GetPayoutMultiplier(BetType betType)
    {
        switch (betType)
        {
            case BetType.Number:
                return 36.0; // x36
            case BetType.Color:
            case BetType.OddEven:
            case BetType.HighLow:
                return 2.0;  // x2
            case BetType.Dozen:
            case BetType.Column:
                return 3.0;  // x3
            default:
                return 1.0;
        }
    }
    
    /// <summary>
    /// 아이템 사용
    /// </summary>
    public void UseItem(ItemData item, params int[] targetIDs)
    {
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
        switch (item.spotItemType)
        {
            case SpotItemType.PlusSpot:
                if (targetIDs.Length >= 1)
                {
                    SpotCalculator.ApplyPlusSpot(gameState, item, targetIDs[0]);
                    SpotCalculator.RecalculateAll(gameState);
                    
                    // Board 갱신 및 상태 이벤트 발행
                    if (_board != null)
                        _board.UpdateAllVisuals();
                    OnGameStateChanged?.Invoke(gameState);
                }
                break;
                
            case SpotItemType.CopySpot:
                if (targetIDs.Length >= 2)
                {
                    SpotCalculator.ApplyCopySpot(gameState, item, targetIDs[0], targetIDs[1]);
                    SpotCalculator.RecalculateAll(gameState);
                    
                    // Board 갱신 및 상태 이벤트 발행
                    if (_board != null)
                        _board.UpdateAllVisuals();
                    OnGameStateChanged?.Invoke(gameState);
                }
                break;
                
            case SpotItemType.UpgradedMultiSpot:
                if (targetIDs.Length >= 1)
                {
                    SpotCalculator.ApplyUpgradedMultiSpot(gameState, item, targetIDs[0]);
                    SpotCalculator.RecalculateAll(gameState);
                    
                    // Board 갱신 및 상태 이벤트 발행
                    if (_board != null)
                        _board.UpdateAllVisuals();
                    OnGameStateChanged?.Invoke(gameState);
                }
                break;
        }
    }
    
    private void UseChipItem(ItemData item, int[] targetIDs)
    {
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
        if (isSpinning)
        {
            Debug.LogWarning("[Game] Already spinning!");
            return;
        }                                                                                                                                                                                                                                                                                                                                                                                       
        
        if (currentBets.Count == 0)
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
        
        // 당첨 번호 추첨
        int winningSpotID = SpotCalculator.DetermineWinner(gameState);
        Spot winningSpot = gameState.spots[winningSpotID];
        
        Debug.Log($"[Game] Winning Spot: {winningSpotID} (Number: {winningSpot.currentNumber})");
        
        // 스핀 시작 이벤트 발행
        OnSpinStarted?.Invoke();
        
        // 룰렛과 공 스핀 시작 (있으면)
        if (_wheelController != null && _ballController != null)
        {
            _wheelController.StartSpin(winningSpot.currentNumber);
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
        Debug.Log("[Game] ========== Spin Complete ==========");
        
        // 당첨 번호 재확인 (이미 결정됨)
        int winningSpotID = SpotCalculator.DetermineWinner(gameState);
        Spot winningSpot = gameState.spots[winningSpotID];
        
        // 배당 계산
        float totalPayout = SpotCalculator.CalculateTotalPayout(gameState, winningSpotID, currentBets);
        
        // 돈 지급 (칩으로 변환하여 추가)
        // totalPayout을 칩으로 변환하는 로직 (나중에 구현)
        // 현재는 로그만 출력
        Debug.Log($"[Game] Total payout: ${totalPayout:F2}");
        
        // Death Charm 처리
        SpotCalculator.ProcessDeathCharm(gameState, winningSpotID);
        
        // Board 비주얼 업데이트 (파괴된 Spot 표시)
        if (_board != null)
            _board.UpdateAllVisuals();
        
        // Turn 데이터 저장
        TurnData turnData = new TurnData(gameState.currentTurn);
        turnData.bets = new List<BetData>(currentBets); // 복사
        turnData.winningSpotID = winningSpotID;
        turnData.winningNumber = winningSpot.currentNumber;
        turnData.totalPayout = totalPayout;
        turnData.SaveSpotSnapshot(gameState.spots);
        gameState.turnHistory.Add(turnData);
        
        Debug.Log($"[Game] Total Payout: ${totalPayout:F2}");
        
        // 스핀 완료 이벤트 발행
        SpinResult spinResult = new SpinResult(winningSpotID, winningSpot.currentNumber, totalPayout);
        OnSpinCompleted?.Invoke(spinResult);
        
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
        Debug.Log("[Game] ========== Test Betting ==========");
        
        // 테스트 아이템 사용
        ItemData plusSpot = itemManager.CreateItem("PLUS_SPOT");
        UseItem(plusSpot, 5); // Spot 5를 6으로
        
        ItemData upgradeMulti = itemManager.CreateItem("UPGRADED_MULTI_SPOT");
        UseItem(upgradeMulti, 10); // Spot 10 + 인접 강화
        
        // 테스트 배팅 - 다양한 타입
        PlaceBet(BetType.Number, 6, ChipType.Chip1, 2);     // 숫자 배팅 (x36)
        PlaceBet(BetType.Color, 0, ChipType.Chip1, 1);      // 빨간색 배팅 (x2)
        PlaceBet(BetType.OddEven, 1, ChipType.Chip1, 1);    // 짝수 배팅 (x2)
        PlaceBet(BetType.HighLow, 0, ChipType.Chip1, 1);    // Low(1-18) 배팅 (x2)
        PlaceBet(BetType.Dozen, 1, ChipType.Chip1, 1);      // 1-12 배팅 (x3)
        PlaceBet(BetType.Column, 2, ChipType.Chip1, 1);     // 2열 배팅 (x3)
        
        // 스핀
        StartSpin();
    }
    
    #endregion
    
    #region Public Getters
    
    public GameState GetGameState()
    {
        return gameState;
    }
    
    #endregion
}
