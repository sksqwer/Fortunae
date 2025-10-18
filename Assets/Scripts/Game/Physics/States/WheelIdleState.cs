using UnityEngine;
using StateMachine;

/// <summary>
/// 룰렛 대기 상태
/// </summary>
public class WheelIdleState : FSMState<WheelController>
{
    public WheelIdleState(StateMachine<WheelController> sm, WheelController actor, int layer) 
        : base(sm, actor, layer) { }
    
    protected override void OnEnter()
    {
        Debug.Log("[RouletteFSM] Idle 진입");
    }
}

