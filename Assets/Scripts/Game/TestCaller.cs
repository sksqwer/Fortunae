using System;
using UnityEngine;
using GB;

/// <summary>
/// 실제 호출 테스트용 클래스
/// </summary>
public class TestCaller : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool autoTest = false;
    [SerializeField] private float testInterval = 2f;
    
    private float lastTestTime = 0f;
    
    private void Start()
    {
        if (autoTest)
        {
            InvokeRepeating(nameof(RunTests), 1f, testInterval);
        }
    }
    
    private void Update()
    {
        // 키보드 테스트
        if (Input.GetKeyDown(KeyCode.T))
        {
            RunTests();
        }
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            TestBetObjectClick();
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            TestItemUsage();
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            TestSpin();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            TestChipSelectionPopup();
        }
    }
    
    /// <summary>
    /// 모든 테스트 실행
    /// </summary>
    [ContextMenu("Run All Tests")]
    public void RunTests()
    {
        Debug.Log("=== 테스트 시작 ===");
        
        TestBetObjectClick();
        TestItemUsage();
        TestSpin();
        
        Debug.Log("=== 테스트 완료 ===");
    }
    
    /// <summary>
    /// BetObject 클릭 테스트
    /// </summary>
    [ContextMenu("Test BetObject Click")]
    public void TestBetObjectClick()
    {
        Debug.Log("[TestCaller] BetObject 클릭 테스트 시작");
        
        // ColorBetObject 클릭 시뮬레이션
        BetObjectClickData redClick = new BetObjectClickData(100, BetType.Color, 0); // Red
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_BET_OBJECT_CLICKED, redClick);
        
        // OddEvenBetObject 클릭 시뮬레이션
        BetObjectClickData evenClick = new BetObjectClickData(101, BetType.OddEven, 1); // Even
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_BET_OBJECT_CLICKED, evenClick);
        
        // DozenBetObject 클릭 시뮬레이션
        BetObjectClickData dozenClick = new BetObjectClickData(102, BetType.Dozen, 1); // 1-12
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_BET_OBJECT_CLICKED, dozenClick);
    }
    
    /// <summary>
    /// 아이템 사용 테스트
    /// </summary>
    [ContextMenu("Test Item Usage")]
    public void TestItemUsage()
    {
        Debug.Log("[TestCaller] 아이템 사용 테스트 시작");
        Debug.LogWarning("[TestCaller] Item usage tests disabled - use ItemManager through GamePresenter");
        
        // TODO: ItemManager를 통한 아이템 생성 테스트
        // GamePresenter의 UseItemByType 메서드 사용
    }
    
    /// <summary>
    /// 배팅 테스트
    /// </summary>
    [ContextMenu("Test Betting")]
    public void TestBetting()
    {
        Debug.Log("[TestCaller] 배팅 테스트 시작");
        
        // 다양한 배팅 테스트
        object[] bet1 = { BetType.Number, 7, ChipType.Chip1, 2 }; // Spot 7에 2개 칩
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_PLACE_BET, bet1);
        
        object[] bet2 = { BetType.Color, 0, ChipType.Chip5, 1 }; // Red에 1개 칩
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_PLACE_BET, bet2);
        
        object[] bet3 = { BetType.Dozen, 2, ChipType.Chip1, 3 }; // 2nd Dozen에 3개 칩
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_PLACE_BET, bet3);
    }
    
    /// <summary>
    /// 스핀 테스트
    /// </summary>
    [ContextMenu("Test Spin")]
    public void TestSpin()
    {
        Debug.Log("[TestCaller] 스핀 테스트 시작");
        
        // 스핀 시작
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_START_SPIN);
        
        // 게임 리셋
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_RESET_GAME);
    }
    
    /// <summary>
    /// Spot 클릭 테스트
    /// </summary>
    [ContextMenu("Test Spot Click")]
    public void TestSpotClick()
    {
        Debug.Log("[TestCaller] Spot 클릭 테스트 시작");
        
        // Spot 클릭 시뮬레이션
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_SPOT_CLICKED, 15);
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_SPOT_CLICKED, 22);
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_SPOT_CLICKED, 33);
    }
    
    /// <summary>
    /// ChipSelection 팝업 테스트
    /// </summary>
    [ContextMenu("Test ChipSelection Popup")]
    public void TestChipSelectionPopup()
    {
        Debug.Log("[TestCaller] ChipSelection 팝업 테스트 시작");
        
        // UIManager 상태 확인
        Debug.Log($"[TestCaller] UIManager.I != null: {UIManager.I != null}");
        if (UIManager.I != null)
        {
            var popup = UIManager.FindUIScreen("ChipSelectionPopup");
            Debug.Log($"[TestCaller] ChipSelectionPopup found: {popup != null}");
            if (popup != null)
            {
                Debug.Log($"[TestCaller] Popup GameObject: {popup.gameObject.name}");
                Debug.Log($"[TestCaller] Popup active: {popup.gameObject.activeInHierarchy}");
            }
        }
        
        // UIManager를 통한 직접 테스트
        try
        {
            ChipCollection testChips = new ChipCollection(ChipType.Chip1, 5);
            object[] parameters = {
                999, // Test spot ID
                testChips,
                (Action<ChipType, int>)((chipType, chipCount) => {
                    Debug.Log($"[TestCaller] Callback called: {chipType} x {chipCount}");
                })
            };
            
            Debug.Log($"[TestCaller] Attempting to show ChipSelectionPopup...");
            UIManager.ShowPopup("ChipSelectionPopup", parameters);
            Debug.Log("[TestCaller] UIManager.ShowPopup called successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TestCaller] UIManager.ShowPopup failed: {e.Message}");
        }
    }
}
