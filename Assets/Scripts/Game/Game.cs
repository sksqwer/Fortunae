using UnityEngine;
using GB;

/// <summary>
/// 게임 메인 컨트롤러
/// </summary>
public class Game : MonoBehaviour, IView
{
    #region Inspector Settings
    
    [SerializeField] private Board _board;
    [SerializeField] private WheelController _wheelController;
    [SerializeField] private BallController _ballController;
    
    #endregion
    
    #region Private Fields
    
    private bool isSpinning = false;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        _board.Init();
    }
    
    private void Start()
    {
        // 이벤트 구독
        if (_wheelController != null)
        {
            _wheelController.OnSpinComplete += OnSpinComplete;
        }
    }
    
    private void Update()
    {
        // 스페이스바로 스핀 시작
        if (!isSpinning && Input.GetKeyDown(KeyCode.Space))
        {
            StartSpin();
        }
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
    
    #region Game Logic
    
    private void StartSpin()
    {
        if (_wheelController == null || _ballController == null)
        {
            Debug.LogWarning("[Game] WheelController 또는 BallController가 없습니다.");
            return;
        }
        
        isSpinning = true;
        
        // 당첨 번호 계산
        int winningNumber = CalculateWinningNumber();
        Debug.Log($"[Game] 스핀 시작 - 당첨 번호: {winningNumber}");
        
        // 룰렛과 공 스핀 시작
        _wheelController.StartSpin(winningNumber);
    }
    
    private void OnSpinComplete()
    {
        Debug.Log("[Game] 스핀 완료");
        isSpinning = false;
    }
    
    private int CalculateWinningNumber()
    {
        // 유러피언 룰렛 번호 (0-36)
        int[] rouletteNumbers = {
            0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10,
            5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
        };
        
        int randomIndex = Random.Range(0, rouletteNumbers.Length);
        return rouletteNumbers[randomIndex];
    }
    
    #endregion
    
    #region IView Implementation
    
    public void ViewQuick(string key, IOData data)
    {
        // 필요한 경우 구현
    }
    
    #endregion
}
