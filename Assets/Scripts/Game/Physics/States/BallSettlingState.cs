using UnityEngine;
using StateMachine;

/// <summary>
/// 공 안착 상태 - 빙글빙글 돌면서 타겟 위치로 이동
/// </summary>
public class BallSettlingState : FSMState<BallController>
{
    private float currentAngle;
    private float targetAngle;
    private float orbitRadius;
    private Vector2 wheelCenterAtStart;
    private float currentRotationSpeed; // 현재 회전 속도
    private float initialRotationSpeed; // 시작 회전 속도
    private float noiseTime; // Perlin Noise 시간
    private float noiseIntensity; // 노이즈 강도
    
    public BallSettlingState(StateMachine<BallController> sm, BallController actor, int layer) 
        : base(sm, actor, layer) { }
    
    protected override void OnEnter()
    {
        Debug.Log("[BallFSM] Settling 진입");
        
        // 이전 상태에서 회전하던 각도 이어받기
        currentAngle = Actor.currentOrbitAngle;
        
        // 바퀴 중심 고정
        wheelCenterAtStart = Actor.wheelTransform.position;
        
        // 궤도 반지름 고정 (포켓이 있는 반지름)
        orbitRadius = Actor.rotationMinRadius;
        
        // 타겟의 각도 계산 (바퀴 중심 기준)
        Vector2 targetPos = Actor.targetTransform.position;
        Vector2 directionToTarget = targetPos - wheelCenterAtStart;
        targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        
        // 이전 상태의 실제 회전 속도 이어받기
        initialRotationSpeed = Actor.currentAngularSpeed;
        currentRotationSpeed = initialRotationSpeed;
        
        // Perlin Noise 초기화
        noiseTime = Random.Range(0f, 100f); // 랜덤 시드
        noiseIntensity = 0.4f; // 초기 흔들림 강도
        
        Debug.Log($"[Settling] 시작각도:{currentAngle:F1}, 타겟각도:{targetAngle:F1}, 반지름:{orbitRadius:F2}, 초기속도:{initialRotationSpeed:F0}");
    }

    public override void Update()
    {
        Vector2 targetPos = Actor.targetTransform.position;
        
        // 반시계방향으로 회전하면서 타겟 각도로 접근
        // 타겟 각도를 현재 각도보다 앞쪽으로 정규화 (반시계방향 기준)
        float normalizedTargetAngle = targetAngle;
        while (normalizedTargetAngle < currentAngle)
        {
            normalizedTargetAngle += 360f;
        }
        
        // 남은 각도 계산 (반시계방향)
        float remainingAngle = normalizedTargetAngle - currentAngle;
        
        // 남은 각도에 비례하여 속도 점진적 감소 (AnimationCurve 사용)
        float normalizedRemainingAngle = Mathf.Clamp01(remainingAngle / 360f); // 남은 각도 비율 (1=시작, 0=도달)
        float progress = 1f - normalizedRemainingAngle; // 진행도 (0=시작, 1=도달)
        float speedMultiplier = Mathf.Max(Actor.settleDecelerationCurve.Evaluate(progress), 0.05f); // 최소 5% 속도 보장
        currentRotationSpeed = initialRotationSpeed * speedMultiplier;
        
        // 공유 변수 업데이트
        Actor.currentAngularSpeed = currentRotationSpeed;
        
        // 반시계방향으로 회전
        float rotationStep = currentRotationSpeed * Time.deltaTime;
        
        // 타겟 각도를 넘지 않도록 제한
        if (remainingAngle > 0.5f)
        {
            currentAngle += Mathf.Min(rotationStep, remainingAngle);
        }
        else
        {
            currentAngle = normalizedTargetAngle; // 타겟 각도 도달
        }
        
        Actor.currentOrbitAngle = currentAngle;
        
        // Perlin Noise로 반지름 오프셋 계산
        noiseTime += Time.deltaTime * 3f;
        float radiusNoise = (Mathf.PerlinNoise(noiseTime, 0f) - 0.5f) * 2f; // -1 ~ 1
        
        // 노이즈 강도 곡선 사용 (별도 커브)
        float noiseMultiplier = Actor.settleNoiseCurve.Evaluate(progress);
        float currentNoiseIntensity = noiseIntensity * noiseMultiplier;
        
        // 노이즈 적용된 반지름
        float noisyRadius = orbitRadius + (radiusNoise * currentNoiseIntensity);
        
        // 바퀴 중심 기준으로 현재 위치 계산 (노이즈 적용)
        float angleRad = currentAngle * Mathf.Deg2Rad;
        Vector2 orbitPos = wheelCenterAtStart + new Vector2(
            Mathf.Cos(angleRad) * noisyRadius,
            Mathf.Sin(angleRad) * noisyRadius
        );
        
        Actor.transform.position = orbitPos;
        
        // 타겟까지 거리
        float distanceToTarget = Vector2.Distance(orbitPos, targetPos);
        
        Debug.Log($"[Settling] 현재각:{currentAngle:F1}, 남은각도:{remainingAngle:F1}, 진행도:{progress:F2}, 속도배율:{speedMultiplier:F2}, 거리:{distanceToTarget:F3}");
        
        // 완료 체크: 타겟 각도에 도달하고 거리도 가까우면 안착
        if (progress >= 0.98f)
        {
            Actor.transform.position = targetPos;
            Debug.Log("[Settling] 안착 완료!");
            
            // 안착 완료 통보 및 상태 전환
            Actor.NotifySettlingComplete();
            StateMachine.ExecuteCommand(BallController.BallCommands.ToIdle);
        }
    }
}
