using UnityEngine;
using StateMachine;

/// <summary>
/// 공 대기 상태
/// </summary>
public class BallIdleState : FSMState<BallController>
{
    public BallIdleState(StateMachine<BallController> sm, BallController actor, int layer) 
        : base(sm, actor, layer) { }
    
    protected override void OnEnter()
    {
        Debug.Log("[BallFSM] Idle 진입");
    }
}

