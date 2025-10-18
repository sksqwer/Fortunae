using System;
using UnityEngine;
using StateMachine;

namespace StateMachine
{
    /// <summary>
    /// 상태 간 전환을 정의하는 클래스입니다.
    /// 전환 조건과 명령어 기반 상태 변화를 지원합니다.
    /// </summary>
    /// <typeparam name="TEntityType">상태 머신을 소유하는 엔티티 타입</typeparam>
    public class StateTransition<TEntityType>
    {
        #region Fields
        
        private readonly Func<FSMState<TEntityType>, bool> transitionCondition;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// 자기 자신으로의 전환이 허용되는지 여부
        /// </summary>
        public bool CanTransitionToSelf { get; private set; }

        /// <summary>
        /// 전환의 시작 상태
        /// </summary>
        public FSMState<TEntityType> FromState { get; private set; }

        /// <summary>
        /// 전환의 목적 상태
        /// </summary>
        public FSMState<TEntityType> ToState { get; private set; }

        /// <summary>
        /// 전환을 트리거하는 명령 ID
        /// </summary>
        public int TransitionCommand { get; private set; }
        
        /// <summary>
        /// 전환의 우선순위 (낮을수록 높은 우선순위)
        /// </summary>
        public int Priority { get; private set; }
        
        /// <summary>
        /// 전환 이름 (디버깅 및 로깅용)
        /// </summary>
        public string TransitionName => $"{FromState?.StateName ?? "Any"} -> {ToState?.StateName ?? "Unknown"}";

        #endregion

        #region Constructor
        
        /// <summary>
        /// StateTransition 생성자
        /// </summary>
        /// <param name="fromState">시작 상태</param>
        /// <param name="toState">목적 상태</param>
        /// <param name="transitionCommand">전환 명령</param>
        /// <param name="transitionCondition">전환 조건</param>
        /// <param name="canTransitionToSelf">자기 자신으로의 전환 허용 여부</param>
        /// <param name="priority">전환 우선순위</param>
        /// <exception cref="ArgumentNullException">fromState 또는 toState가 null인 경우</exception>
        /// <exception cref="ArgumentException">transitionCommand와 transitionCondition이 모두 유효하지 않은 경우</exception>
        public StateTransition(
            FSMState<TEntityType> fromState,
            FSMState<TEntityType> toState,
            int transitionCommand = StateMachineConstants.NULL_COMMAND,
            Func<FSMState<TEntityType>, bool> transitionCondition = null,
            bool canTransitionToSelf = false,
            int priority = StateMachineConstants.DEFAULT_TRANSITION_PRIORITY)
        {
            FromState = fromState ?? throw new ArgumentNullException(nameof(fromState));
            ToState = toState ?? throw new ArgumentNullException(nameof(toState));
            
            if (transitionCommand == StateMachineConstants.NULL_COMMAND && transitionCondition == null)
            {
                throw new ArgumentException("TransitionCommand와 TransitionCondition 중 최소 하나는 유효해야 합니다.");
            }
            
            TransitionCommand = transitionCommand;
            this.transitionCondition = transitionCondition;
            CanTransitionToSelf = canTransitionToSelf;
            Priority = priority;
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// 전환 조건을 확인합니다.
        /// </summary>
        /// <param name="command">확인할 명령 (기본값: NULL_COMMAND)</param>
        /// <returns>전환이 가능한지 여부</returns>
        public bool Condition(int command = StateMachineConstants.NULL_COMMAND)
        {
            // 명령이 지정된 경우 명령이 일치해야 함
            if (TransitionCommand != StateMachineConstants.NULL_COMMAND && TransitionCommand != command)
                return false;

            if (transitionCondition != null)
            {
                return transitionCondition.Invoke(FromState);
            }

            return true;
        }
        
        /// <summary>
        /// 전환이 유효한지 확인합니다.
        /// </summary>
        /// <returns>전환이 유효한지 여부</returns>
        public bool IsValid()
        {
            if (FromState == null || ToState == null)
                return false;
                
            if (!CanTransitionToSelf && FromState == ToState)
                return false;
                
            return true;
        }
        
        /// <summary>
        /// 전환 정보를 문자열로 반환합니다.
        /// </summary>
        /// <returns>전환 정보 문자열</returns>
        public override string ToString()
        {
            return $"{TransitionName} (Command: {TransitionCommand}, Priority: {Priority}, Self: {CanTransitionToSelf})";
        }
        
        #endregion
    }
}