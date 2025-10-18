using UnityEngine;
using StateMachine;

/// <summary>
/// 공 궤도 회전 상태
/// </summary>
public class BallOrbitingState : FSMState<BallController>
{
    private float rotationAngle;
    private float currentRadius;
    private Vector2 wheelCenterAtStart;
    
    public BallOrbitingState(StateMachine<BallController> sm, BallController actor, int layer) 
        : base(sm, actor, layer) { }
    
    protected override void OnEnter()
    {
        Debug.Log("[BallFSM] Orbiting 진입");
        Actor.stateTimer = 0f;
        rotationAngle = 0f;
        currentRadius = Actor.rotationMaxRadius;
        wheelCenterAtStart = Actor.wheelTransform.position;
    }
    
    public override void Update()
    {
        float t = Mathf.Clamp01(Actor.stateTimer / Actor.rotationDuration);
        
        if (t >= 1f)
        {
            StateMachine.ExecuteCommand(BallController.BallCommands.ToSettling);
            return;
        }
        
        // 궤도 회전
        float speedMultiplier = Actor.rotationSpeedCurve.Evaluate(t);
        float angularSpeed = Actor.rotationSpeed * speedMultiplier; // 실제 회전 속도 (도/초)
        rotationAngle += angularSpeed * Time.deltaTime;
        currentRadius = Actor.rotationRadiusCurve.Evaluate(t);
        
        // 공유 변수에 각도 및 속도 저장 (다음 상태에서 이어받기)
        Actor.currentOrbitAngle = rotationAngle;
        Actor.currentAngularSpeed = angularSpeed;
        
        Vector2 offset = GetOrbitPosition(rotationAngle, currentRadius);
        Actor.transform.position = wheelCenterAtStart + offset;
    }
    
    private Vector2 GetOrbitPosition(float angleDegrees, float radius)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRad) * radius, Mathf.Sin(angleRad) * radius);
    }
}

