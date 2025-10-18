using UnityEngine;
using StateMachine;

namespace StateMachine
{
    /// <summary>
    /// AnyState는 모든 상태에서 전환하기 가능한 특별한 상태입니다.
    /// 이 상태는 실제로 활성화되지 않으며, 전환 조건만을 제공합니다.
    /// </summary>
    /// <typeparam name="TEntityType">상태 머신을 소유하는 엔티티 타입</typeparam>
    public class AnyState<TEntityType> : FSMState<TEntityType>
    {
        #region Constructor
        
        /// <summary>
        /// AnyState 생성자
        /// </summary>
        /// <param name="stateMachine">상태 머신 인스턴스</param>
        /// <param name="actor">엔티티 인스턴스</param>
        /// <param name="layer">레이어 번호</param>
        public AnyState(StateMachine<TEntityType> stateMachine, TEntityType actor, int layer) 
            : base(stateMachine, actor, layer)
        {
        }

        #endregion

        #region Utility Methods
        
        public override string ToString()
        {
            return $"AnyState (Layer: {Layer}, Virtual State)";
        }
        
        #endregion
    }
}