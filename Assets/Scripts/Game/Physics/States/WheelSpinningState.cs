using UnityEngine;
using StateMachine;

/// <summary>
/// 룰렛 스핀 상태
/// </summary>
public class WheelSpinningState : FSMState<WheelController>
{
    public WheelSpinningState(StateMachine<WheelController> sm, WheelController actor, int layer) 
        : base(sm, actor, layer) { }
    
    protected override void OnEnter()
    {
        Debug.Log("[RouletteFSM] Spinning 진입");
        Actor.stateTimer = 0f;
        Actor.totalRotation = 0f;
        Actor.startAngle = Actor.wheelTransform.eulerAngles.z;
        
        if (Actor.resultText != null)
            Actor.resultText.text = "돌아가는 중...";
    }
    
    public override void Update()
    {
        float t = Mathf.Clamp01(Actor.stateTimer / Actor.spinDuration);
        
        // 회전 계산
        float speedMultiplier = Actor.spinDecelerationCurve.Evaluate(t);
        float currentSpinSpeed = Actor.maxSpinSpeed * speedMultiplier;
        float angleIncrement = currentSpinSpeed * Time.deltaTime;
        Actor.totalRotation += angleIncrement;
        
        float currentAngle = Actor.startAngle - Actor.totalRotation;
        Actor.wheelTransform.eulerAngles = new Vector3(0, 0, currentAngle);
        
        // 시간 종료 체크
        if (t >= 1f)
        {            
            // 결과 표시
            Debug.Log($"당첨 번호: {Actor.winningNumber}");
            if (Actor.resultText != null)
                Actor.resultText.text = $"결과: {Actor.winningNumber}";
            
            // 스핀 완료 통보 및 상태 전환
            // Actor.NotifySpinComplete();
            StateMachine.ExecuteCommand(WheelController.WheelCommands.ToIdle);
            return;
        }
    }
}

