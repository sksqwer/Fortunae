using System;
using UnityEngine;
using UnityEngine.UI;
using StateMachine;

/// <summary>
/// MonoStateMachine을 활용한 RouletteController
/// </summary>
public class WheelController : MonoStateMachine<WheelController>
{
    #region Commands
    
    public enum WheelCommands
    {
        ToIdle,
        ToSpinning
    }
    
    #endregion
    
    #region Events
    
    public event Action OnSpinComplete;
    
    #endregion
    
    #region Inspector Settings
    
    [Header("[ 게임 오브젝트 ]")]
    public Transform wheelTransform;
    public BallController ballController;
    
    [Header("[ 룰렛 설정 ]")]
    public float maxSpinSpeed = 1000f;
    public float spinDuration = 5f; // 회전 시간
    public AnimationCurve spinDecelerationCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // 0에서 완전히 멈춤
    
    [Header("[ UI ]")]
    public Text resultText;
    
    [SerializeField] private PocketTrigger[] pockets;
    
    #endregion
    
    #region Public Fields
    
    [HideInInspector] public float stateTimer;
    [HideInInspector] public float totalRotation;
    [HideInInspector] public float startAngle;
    [HideInInspector] public int winningNumber;
    
    private readonly int[] numbers = {
        0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10,
        5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
    };
    
    #endregion
    
    #region MonoStateMachine Override
    
    protected override void InitializeOwner()
    {
        owner = this;
        ballController.OnBallSettled += (winningNumberv) =>
        {
            NotifySpinComplete();
        };
        SetupPockets();
    }
    
    protected override void SetupStates()
    {
        AddState<WheelIdleState>();
        AddState<WheelSpinningState>();
    }
    
    protected override void SetupTransitions()
    {
        // Idle -> Spinning
        MakeTransition<WheelIdleState, WheelSpinningState>(WheelCommands.ToSpinning);
        
        // Spinning -> Idle
        MakeTransition<WheelSpinningState, WheelIdleState>(WheelCommands.ToIdle);
    }
    
    protected override void OnUpdate()
    {
        stateTimer += Time.deltaTime;
    }
    
    #endregion
    
    #region Public Methods
    
    public void StartSpin(int winningNum)
    {
        if (!IsInState<WheelIdleState>())
        {
            Debug.LogWarning("[WheelController] 이미 스핀 중");
            return;
        }
        
        winningNumber = winningNum;
        int segmentIndex = GetSegmentIndex(winningNum);
        
        // 공과 함께 시작
        if (ballController != null && pockets != null && segmentIndex < pockets.Length)
        {
            ballController.StartSpin(winningNum, pockets[segmentIndex].transform);
        }
        
        ExecuteCommand(WheelCommands.ToSpinning);
    }
    
    public void NotifySpinComplete()
    {
        // State에서 호출: 스핀 완료 알림
        OnSpinComplete?.Invoke();
        Debug.Log($"[WheelController] 스핀 완료");
    }
    
    public bool IsIdle => IsInState<WheelIdleState>();
    
    #endregion
    
    #region Helper Methods
    
    private int GetSegmentIndex(int winningNumber)
    {
        return System.Array.IndexOf(numbers, winningNumber);
    }
    
    private void SetupPockets()
    {
        if (pockets != null)
        {
            for (int i = 0; i < pockets.Length && i < numbers.Length; i++)
            {
                pockets[i].pocketNumber = numbers[i];
            }
        }
    }
    
    #endregion
}

