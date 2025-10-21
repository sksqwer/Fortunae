/// <summary>
/// 게임 전체 상수 정의
/// </summary>
public static class ConstDef
{
    // 아이템 ID
    public const string PLUS_SPOT = "PLUS_SPOT";
    public const string COPY_SPOT = "COPY_SPOT";
    public const string UPGRADED_MULTI_SPOT = "UPGRADED_MULTI_SPOT";
    public const string HAT_WING = "HAT_WING";
    
    // 아이템 표시 포맷
    public const string ITEM_COUNT_FORMAT = "• <sprite={0}> {1} : ({2})\n";
    
    // 칩 표시 포맷
    public const string CHIP_COUNT_FORMAT = "{0}chips × {1} = ${2}";
    
    // 디버그 메시지
    public const string ITEM_BUTTON_CLICKED = "[ChipSelectionPopup] Item button clicked: {0} for object {1}";
    public const string HAT_WING_TOGGLED = "[ChipSelectionPopup] HatWing toggled to: {0}";
    public const string SLIDER_SET = "[ChipSelectionPopup] Slider set to 0 / {0}";
    public const string HAT_WING_STATE_RESET = "[ChipSelectionPopup] HatWing state reset on cancel";
    public const string HAT_WING_USED = "[ChipSelectionPopup] HatWing used for new bet: {0} on {1}";
    public const string HAT_WING_ALREADY_APPLIED = "[ChipSelectionPopup] HatWing already applied to existing bet: {0} on {1}, not using item again";
    public const string ITEM_BUTTONS_PANEL_SHOWN = "[ChipSelectionPopup] Item buttons panel shown for bet type: {0}";
    public const string ITEM_BUTTONS_PANEL_HIDDEN = "[ChipSelectionPopup] Item buttons panel hidden for bet type: {0}";
    public const string CANNOT_CONFIRM_ZERO_CHIPS = "[ChipSelectionPopup] Cannot confirm with 0 chips selected";
    public const string CANNOT_TOGGLE_HAT_WING = "[ChipSelectionPopup] Cannot toggle HatWing - no items available!";
    
    // Game.cs 상수들
    public const string GAME_AWAKE_CALLED = "[Game] Awake called";
    public const string GAME_START_CALLED = "[Game] Start called";
    public const string GAME_DESTROY_CALLED = "[Game] OnDestroy called";
    public const string GAME_INITIALIZE_CALLED = "[Game] InitializeGame called";
    public const string GAME_RESET_CALLED = "[Game] ResetGame called";
    public const string GAME_SPIN_STARTED = "[Game] StartSpin called";
    public const string GAME_SPIN_COMPLETE = "[Game] OnSpinComplete called";
    public const string GAME_END_CALLED = "[Game] EndGame called";
    public const string GAME_NEW_TURN = "[Game] StartNewTurn called - Turn {0}";
    public const string GAME_PLACE_BET = "[Game] PlaceBet called";
    public const string GAME_REMOVE_BET = "[Game] RemoveBet called";
    public const string GAME_USE_ITEM = "[Game] UseItem called";
    public const string GAME_USE_ITEM_BY_ID = "[Game] UseItemByID called: {0} with targets: [{1}]";
    
    // 게임 상태 메시지
    public const string GAME_INITIALIZING = "[Game] ========== Initializing Game ==========";
    public const string GAME_RESETTING = "[Game] ========== Resetting Game ==========";
    public const string GAME_OVER = "[Game] ========== Game Over ==========";
    public const string SPIN_STARTED = "[Game] ========== Spin Started (Turn {0}) ==========";
    public const string SPIN_COMPLETE = "[Game] ========== Spin Complete ==========";
    public const string TURN_STARTED = "[Game] ========== Turn {0} / {1} Started ==========";
    
    // 에러 메시지
    public const string ALREADY_SPINNING = "[Game] Already spinning!";
    public const string NO_BETS_PLACED = "[Game] No bets placed! Please place at least one bet.";
    public const string WHEEL_CONTROLLER_SUBSCRIBED = "[Game] WheelController event subscribed";
    public const string WHEEL_CONTROLLER_UNSUBSCRIBED = "[Game] WheelController event unsubscribed";
    public const string ITEM_TABLE_NULL = "[Game] ItemTable is null! No items will be initialized.";
    public const string GAME_STATE_NULL = "[Game] GameState is null!";
    public const string FAILED_TO_USE_ITEM = "[Game] Failed to use item from inventory: {0}";
    public const string ITEM_USED_FROM_INVENTORY = "[Game] Item used from inventory: {0}, remaining: {1}";
    public const string CANNOT_USE_ITEM = "[Game] Cannot use item: {0}";
    public const string CANNOT_COPY_SAME_SPOT = "[Game] Cannot copy to the same Spot! Cancelling CopySpot mode.";
    public const string COPY_SPOT_MODE_STARTED = "[Game] CopySpot mode started - Source: Spot {0}";
    public const string COPY_SPOT_MODE_COMPLETED = "[Game] CopySpot completed and mode deactivated";
    public const string COPY_SPOT_SECOND_SELECTION = "[Game] CopySpot second selection: {0} → {1}";
    public const string COPY_SPOT_CANCELLED = "[Game] CopySpot cancelled - same spot selected";
    
    // 배팅 관련
    public const string BET_PLACED = "[Game] Bet placed: {0} on {1} with {2} chips";
    public const string BET_REMOVED = "[Game] Bet removed: {0} on {1}";
    public const string ALL_BETS_REMOVED = "[Game] All bets removed successfully";
    public const string BET_NOT_FOUND = "[Game] Bet not found in current bets";
    public const string CHIP_RETURNED = "[Game] Returned {0} x{1} to player";
    public const string HAT_WING_ITEM_RETURNED = "[Game] Returned HatWing item to inventory";
    
    // 스핀 관련
    public const string WINNING_SPOT_DETERMINED = "[Game] ★ Winning Spot Determined: {0} (Number: {1}) ★";
    public const string USING_PREDETERMINED_WINNING_SPOT = "[Game] Using pre-determined winning spot: {0}";
    public const string TOTAL_PAYOUT_CALCULATED = "[Game] Total payout calculated: ${0:F2}";
    public const string WINNING_BETS_COUNT = "[Game] Winning bets count: {0}";
    public const string BEFORE_PAYOUT = "[Game] Before payout - availableChips: {0}";
    public const string AFTER_PAYOUT = "[Game] After payout - availableChips: {0}";
    public const string AFTER_LOSS = "[Game] After loss - availableChips: {0}";
    public const string WIN_PAYOUT = "[Game] WIN! Paying out ${0:F2}";
    public const string TOTAL_PAYOUT = "[Game] Total Payout: ${0:F2}";
    public const string TOTAL_EARNED = "[Game] Total Earned: ${0:F2}";
    public const string PRESS_R_TO_RESTART = "[Game] Press R to restart";
    
    // UI 메시지
    public const string SPIN_BUTTON_DISABLED = "[Game] Spin button disabled (no bets on initialization)";
    public const string CLICK_UNLOCKED_ALL_BETS_REMOVED = "[Game] Click unlocked - all bets removed";
    public const string CLICK_UNLOCKED_BET_REMOVED = "[Game] Click unlocked - bet removed";
    public const string CLICK_UNLOCKED_NEW_TURN = "[Game] Click unlocked - new turn started";
    public const string CLICK_UNLOCKED_GAME_RESET = "[Game] Click unlocked - game reset";
    public const string POPUP_OPENED = "[Game] Popup opened";
    public const string POPUP_CLOSED = "[Game] Popup closed (via message)";
    public const string SHOWING_CHIP_SELECTION_POPUP = "[Game] Showing chip selection popup for Spot {0}";
    public const string SHOWING_CHIP_SELECTION_POPUP_BET_OBJECT = "[Game] Showing chip selection popup for BetObject: {0} on {1}";
    public const string SHOWING_SPOT_INFO_POPUP = "[Game] Showing spot info popup";
    
    // CopySpot 관련
    public const string COPY_SPOT_SOURCE_SELECTED = "Source Spot #{0} selected\nClick target Spot to copy";
    public const string HIDE_COPY_SPOT_MESSAGE = "HIDE_COPY_SPOT_MESSAGE";
    public const string SHOW_COPY_SPOT_MESSAGE = "SHOW_COPY_SPOT_MESSAGE";
    
    // GameUI.cs 상수들
    public const string GAME_UI_AWAKE = "[GameUI] Awake called";
    public const string GAME_UI_INITIALIZE = "[GameUI] Initialize called";
    public const string GAME_UI_REGIST = "[GameUI] Regist called";
    public const string GAME_UI_UNREGIST = "[GameUI] Unregist called";
    public const string GAME_UI_SHOW_RESULT = "[GameUI] ShowResult called";
    public const string GAME_UI_SHOW_GAME_OVER = "[GameUI] ShowGameOver called";
    public const string GAME_UI_SHOW_GAME_RESET = "[GameUI] ShowGameReset called";
    public const string GAME_UI_UPDATE_GAME_STATE = "[GameUI] UpdateGameState called";
    public const string GAME_UI_UPDATE_TURN_INFO = "[GameUI] UpdateTurnInfo called";
    public const string GAME_UI_UPDATE_SPIN_BUTTON = "[GameUI] UpdateSpinButton called";
    public const string GAME_UI_UPDATE_RESET_BUTTON = "[GameUI] UpdateResetButton called";
    public const string GAME_UI_UPDATE_BET_LIST = "[GameUI] UpdateBetList called";
    public const string GAME_UI_UPDATE_INVENTORY = "[GameUI] UpdateInventory called";
    public const string GAME_UI_UPDATE_ITEM_USED = "[GameUI] UpdateItemUsed called";
    public const string GAME_UI_UPDATE_GAME_RESET = "[GameUI] UpdateGameReset called";
    public const string GAME_UI_UPDATE_SPIN_START = "[GameUI] UpdateSpinStart called";
    public const string GAME_UI_UPDATE_SPIN_COMPLETE = "[GameUI] UpdateSpinComplete called";
    public const string GAME_UI_UPDATE_GAME_OVER = "[GameUI] UpdateGameOver called";
    public const string GAME_UI_UPDATE_POPUP_CLOSED = "[GameUI] UpdatePopupClosed called";
    public const string GAME_UI_UPDATE_SHOW_SPOT_INFO = "[GameUI] UpdateShowSpotInfo called";
    public const string GAME_UI_UPDATE_START_COPY_SPOT = "[GameUI] UpdateStartCopySpot called";
    public const string GAME_UI_UPDATE_HIDE_COPY_SPOT_MESSAGE = "[GameUI] UpdateHideCopySpotMessage called";
    public const string GAME_UI_UPDATE_SHOW_COPY_SPOT_MESSAGE = "[GameUI] UpdateShowCopySpotMessage called";
    
    // GameUI 메시지
    public const string GAME_UI_BUTTON_EVENTS_REGISTERED = "[GameUI] Button events registered";
    public const string GAME_UI_COPY_SPOT_MESSAGE_PANEL_HIDDEN = "[GameUI] CopySpot message panel hidden";
    public const string GAME_UI_COPY_SPOT_MESSAGE_PANEL_SHOWN = "[GameUI] CopySpot message panel shown";
    public const string GAME_UI_COPY_SPOT_MESSAGE_TEXT_SET = "[GameUI] CopySpot message text set: {0}";
    public const string GAME_UI_COPY_SPOT_MESSAGE_HIDDEN = "[GameUI] CopySpot message hidden";
    public const string GAME_UI_BET_ITEMS_CLEARED = "[GameUI] Bet items cleared";
    public const string GAME_UI_BET_ITEM_CREATED = "[GameUI] Bet item created: {0}";
    public const string GAME_UI_BET_ITEM_DESTROYED = "[GameUI] Bet item destroyed: {0}";
    public const string GAME_UI_RESULT_TEXT_UPDATED = "[GameUI] Result text updated";
    public const string GAME_UI_GAME_OVER_SUMMARY_GENERATED = "[GameUI] Game over summary generated";
    public const string GAME_UI_SCROLL_CONTENT_UPDATED = "[GameUI] Scroll content updated";
    public const string GAME_UI_CONTENT_HEIGHT_SET = "[GameUI] Content height set to: {0}";
    public const string GAME_UI_SCROLL_ENABLED = "[GameUI] Scroll enabled - position set to top";
    public const string GAME_UI_NO_SCROLL_NEEDED = "[GameUI] No scroll needed - content fits in viewport";
    public const string GAME_UI_SCROLL_CONTENT_SIZE = "[GameUI] Content height: {0}, Viewport height: {1}";
    
    // GameUI 에러 메시지
    public const string GAME_UI_BET_ITEM_PREFAB_NULL = "[GameUI] BetItem prefab is null!";
    public const string GAME_UI_RESULT_SCROLL_RECT_NULL = "[GameUI] Result scroll rect is null!";
    public const string GAME_UI_COPY_SPOT_MESSAGE_PANEL_NULL = "[GameUI] CopySpot message panel is null!";
    public const string GAME_UI_COPY_SPOT_MESSAGE_TEXT_NULL = "[GameUI] CopySpot message text is null!";
    public const string GAME_UI_GAME_STATE_NULL = "[GameUI] GameState is null!";
    public const string GAME_UI_BET_DATA_NULL = "[GameUI] BetData is null!";
    public const string GAME_UI_BET_ITEM_UI_NULL = "[GameUI] BetItemUI is null!";
    public const string GAME_UI_RESULT_TEXT_NULL = "[GameUI] Result text is null!";
    public const string GAME_UI_CONTENT_RECT_NULL = "[GameUI] Content rect is null!";
    public const string GAME_UI_VIEWPORT_NULL = "[GameUI] Viewport is null!";
    public const string GAME_UI_SCROLL_RECT_NULL = "[GameUI] Scroll rect is null!";
    public const string GAME_UI_VERTICAL_SCROLLBAR_NULL = "[GameUI] Vertical scrollbar is null!";
}
