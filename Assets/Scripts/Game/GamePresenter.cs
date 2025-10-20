using UnityEngine;
using GB;
using System;
using System.Collections.Generic;

/// <summary>
/// Game과 GameUI 사이의 Presenter (MVP 패턴)
/// </summary>
public class GamePresenter : MonoBehaviour, IView
{
    public const string DOMAIN = "Game";
    
    // Presenter 메시지 키
    public static class Keys
    {
        // UI -> Game (Command)
        public const string CMD_START_SPIN = "CMD_START_SPIN";
        public const string CMD_PLACE_BET = "CMD_PLACE_BET";
        public const string CMD_REMOVE_BET = "CMD_REMOVE_BET";
        public const string CMD_USE_ITEM = "CMD_USE_ITEM";
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
        public const string UPDATE_ITEM_USED = "UPDATE_ITEM_USED";
    }
    
    [SerializeField] private Game game;
    
    private void Awake()
    {
        if (game == null)
            game = GetComponent<Game>();
    }
    
    private void OnEnable()
    {
        // Presenter에 바인딩
        Debug.Log($"[GamePresenter] OnEnable - Binding to domain: {DOMAIN}");
        GB.Presenter.Bind(DOMAIN, this);
        Debug.Log($"[GamePresenter] Binding complete");
    }
    
    private void OnDisable()
    {
        // Presenter에서 언바인딩
        // Debug.Log($"[GamePresenter] OnDisable - Unbinding from domain: {DOMAIN}");
        GB.Presenter.UnBind(DOMAIN, this);
    }
    
    private void Start()
    {
        // Game에서 Presenter로 이벤트 전달받을 핸들러 등록
        if (game != null)
        {
            game.OnGameStateChanged += OnGameStateChanged;
            game.OnSpinStarted += OnSpinStarted;
            game.OnSpinCompleted += OnSpinCompleted;
            game.OnGameOver += OnGameOver;
            game.OnBetListChanged += OnBetListChanged;
        }
    }
    
    private void OnDestroy()
    {
        if (game != null)
        {
            game.OnGameStateChanged -= OnGameStateChanged;
            game.OnSpinStarted -= OnSpinStarted;
            game.OnSpinCompleted -= OnSpinCompleted;
            game.OnGameOver -= OnGameOver;
            game.OnBetListChanged -= OnBetListChanged;
        }
    }
    
    #region Game -> UI Events
    
    private void OnGameStateChanged(GameState gameState)
    {
        Presenter.Send(DOMAIN, Keys.UPDATE_GAME_STATE, gameState);
    }
    
    private void OnSpinStarted()
    {
        Presenter.Send(DOMAIN, Keys.UPDATE_SPIN_START);
    }
    
    private void OnSpinCompleted(SpinResult result)
    {
        Presenter.Send(DOMAIN, Keys.UPDATE_SPIN_COMPLETE, result);
    }
    
    private void OnGameOver(double totalEarned)
    {
        Presenter.Send(DOMAIN, Keys.UPDATE_GAME_OVER, totalEarned);
    }
    
    private void OnBetListChanged()
    {
        Presenter.Send(DOMAIN, Keys.UPDATE_BET_LIST);
    }
    
    #endregion
    
    #region UI -> Game Commands
    
    public void StartSpin()
    {
        if (game != null)
            game.StartSpin();
    }
    
    public void PlaceBet(BetType betType, int targetValue, ChipType chipType, int chipCount)
    {
        if (game != null)
            game.PlaceBet(betType, targetValue, chipType, chipCount);
    }
    
    public void RemoveBet(BetData bet)
    {
        if (game != null)
            game.RemoveBet(bet);
    }
    
    public void UseItem(ItemData item, params int[] targetIDs)
    {
        if (game != null)
            game.UseItem(item, targetIDs);
    }
    
    /// <summary>
    /// 아이템 ID로 생성 후 사용 (UI에서 호출)
    /// </summary>
    public void UseItemByID(string itemID, params int[] targetIDs)
    {
        if (game != null)
        {
            ItemData item = game.CreateItem(itemID);
            if (item != null)
            {
                game.UseItem(item, targetIDs);
            }
        }
    }
    
    public void ResetGame()
    {
        if (game != null)
            game.ResetGame();
    }
    
    public void OnSpotClicked(int spotID)
    {
        Debug.Log($"[GamePresenter] OnSpotClicked called for Spot {spotID} - forwarding to GameUI");
        // 무한 루프 방지: ViewQuick에서 직접 GameUI로 전달
        // Presenter.Send(DOMAIN, Keys.CMD_SPOT_CLICKED, spotID); // 이 줄 제거!
        Debug.Log($"[GamePresenter] Message forwarded to GameUI for Spot {spotID}");
    }
    
    #endregion
    
    #region Getters
    
    public GameState GetGameState()
    {
        return game != null ? game.GetGameState() : null;
    }
    
    public List<BetData> GetCurrentBets()
    {
        return game != null ? game.GetCurrentBets() : new List<BetData>();
    }
    
    #endregion
    
    #region IView Implementation
    
    /// <summary>
    /// Presenter에서 받는 메시지 처리 (IView 구현)
    /// </summary>
    public void ViewQuick(string key, IOData data)
    {
        Debug.Log($"[GamePresenter] ViewQuick called with key: {key}");
        
        // CMD_SPOT_CLICKED는 GameUI에서 직접 처리하므로 여기서 무시
        if (key == Keys.CMD_SPOT_CLICKED)
        {
            Debug.Log($"[GamePresenter] Ignoring CMD_SPOT_CLICKED - handled by GameUI directly");
            return;
        }
        
        switch (key)
        {
            case Keys.CMD_BET_OBJECT_CLICKED:
                BetObjectClickData clickData = data.Get<BetObjectClickData>();
                HandleBetObjectClick(clickData);
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
                
            case Keys.CMD_START_SPIN:
                StartSpin();
                break;
                
            case Keys.CMD_RESET_GAME:
                ResetGame();
                break;
                
             // CMD_SPOT_CLICKED는 GameUI에서 직접 처리 (무한 루프 방지)
        }
    }
    
    /// <summary>
    /// BetObject 클릭 처리
    /// </summary>
    private void HandleBetObjectClick(BetObjectClickData clickData)
    {
        Debug.Log($"[GamePresenter] BetObject clicked: {clickData.betType} on {clickData.targetValue} (Payout: x{Game.GetPayoutMultiplier(clickData.betType)})");
        
        // GameUI로 배팅 팝업 표시 요청
        Presenter.Send(DOMAIN, Keys.CMD_SHOW_BET_POPUP, clickData);
    }
    
    /// <summary>
    /// 배팅 처리
    /// </summary>
    private void HandlePlaceBet(IOData data)
    {
        Debug.Log($"[GamePresenter] HandlePlaceBet called with data type: {data?.GetType()}");
        
        // 간단한 파라미터로 직접 처리
        if (data is OData<object[]> paramData)
        {
            object[] parameters = paramData.Get();
            if (parameters.Length >= 4)
            {
                BetType betType = (BetType)parameters[0];
                int targetValue = (int)parameters[1];
                ChipType chipType = (ChipType)parameters[2];
                int chipCount = (int)parameters[3];
                
                PlaceBet(betType, targetValue, chipType, chipCount);
            }
        }
    }
    
    /// <summary>
    /// 배팅 제거 처리
    /// </summary>
    private void HandleRemoveBet(IOData data)
    {
        if (data is OData<BetData> betData)
        {
            RemoveBet(betData.Get());
        }
    }
    
    /// <summary>
    /// 아이템 사용 처리
    /// </summary>
    private void HandleUseItem(IOData data)
    {
        Debug.Log($"[GamePresenter] HandleUseItem called");
        
        if (data is OData<ItemData> itemData)
        {
            // 타겟 없이 아이템 사용
            UseItem(itemData.Get());
        }
        else if (data is OData<object[]> paramData)
        {
            // 타겟과 함께 아이템 사용
            object[] parameters = paramData.Get();
            if (parameters.Length >= 2)
            {
                ItemData item = (ItemData)parameters[0];
                int[] targetIDs = (int[])parameters[1];
                UseItem(item, targetIDs);
            }
        }
    }
    
    #endregion
}

