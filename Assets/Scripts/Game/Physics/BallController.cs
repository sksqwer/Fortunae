using System;
using UnityEngine;
using StateMachine;

/// <summary>
/// MonoStateMachine을 활용한 BallController
/// </summary>
public class BallController : MonoStateMachine<BallController>
{
    #region Commands & Messages
    
    public enum BallCommands
    {
        ToIdle,
        ToOrbiting,
        ToSettling
    }
    
    public enum BallMessages
    {
        SettleInPocket
    }
    
    #endregion
    
    #region Inspector Settings
    
    [Header("[ 오브젝트 참조 ]")]
    public Transform wheelTransform;
    
    [Header("[ 궤도 회전 ]")]
    public float rotationMaxRadius = 2.8f;
    public float rotationMinRadius = 1.5f;
    public float rotationSpeed = 300f;
    public float rotationDuration = 3f;
    public AnimationCurve rotationSpeedCurve = AnimationCurve.Linear(0, 1, 1, 0.3f);
    public AnimationCurve rotationRadiusCurve = AnimationCurve.Linear(0, 2.8f, 1, 1.5f);
    
    [Header("[ 안착 ]")]
    public AnimationCurve settleDecelerationCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.1f); // 감속 곡선 (x: 진행도 0~1, y: 속도배율 1~0.1)
    public AnimationCurve settleNoiseCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // 노이즈 강도 곡선 (x: 진행도 0~1, y: 노이즈강도 1~0)
    
    [Header("[ 시각 효과 ]")]
    public bool enableRotationEffect = true;
    public float visualRotationSpeed = -360f;
    
    #endregion
    
    #region Public Fields
    
    [HideInInspector] public float stateTimer;
    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public int winningNumber;
    [HideInInspector] public float currentOrbitAngle; // 현재 궤도 각도 (State간 공유)
    [HideInInspector] public float currentAngularSpeed; // 현재 회전 속도 (도/초, State간 공유)
    
    public event Action<int> OnBallSettled;
    
    #endregion
    
    #region MonoStateMachine Override
    
    protected override void InitializeOwner()
    {
        owner = this;
    }
    
    protected override void SetupStates()
    {
        AddState<BallIdleState>();
        AddState<BallOrbitingState>();
        AddState<BallSettlingState>();
    }
    
    protected override void SetupTransitions()
    {
        // Idle -> Orbiting
        MakeTransition<BallIdleState, BallOrbitingState>(BallCommands.ToOrbiting);
        
        // Orbiting -> Settling
        MakeTransition<BallOrbitingState, BallSettlingState>(BallCommands.ToSettling);
        
        // Settling -> Idle
        MakeTransition<BallSettlingState, BallIdleState>(BallCommands.ToIdle);
    }
    
    protected override void OnUpdate()
    {
        stateTimer += Time.deltaTime;
        
        if (enableRotationEffect && !IsInState<BallIdleState>())
        {
            transform.Rotate(0, 0, visualRotationSpeed * Time.deltaTime);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    public void StartSpin(int _winningNumber, Transform _pocketTransform)
    {
        if (!IsInState<BallIdleState>())
        {
            Debug.LogWarning("[BallFSM] 이미 스핀 중");
            return;
        }
        
        winningNumber = _winningNumber;
        targetTransform = _pocketTransform;
        ExecuteCommand(BallCommands.ToOrbiting);
    }
    
    public void SettleInPocket()
    {
        SendMessage(BallMessages.SettleInPocket);
    }
    
    public void NotifySettlingComplete()
    {
        // State에서 호출: 안착 완료 알림
        OnBallSettled?.Invoke(winningNumber);
        Debug.Log($"[BallController] 당첨 번호: {winningNumber}");
    }
    
    public void ResetBall()
    {
        transform.rotation = Quaternion.identity;
        stateTimer = 0f;
        ExecuteCommand(BallCommands.ToIdle);
    }
    
    public bool IsIdle => IsInState<BallIdleState>();
    public bool IsActive => !IsIdle;
    
    #endregion
}

