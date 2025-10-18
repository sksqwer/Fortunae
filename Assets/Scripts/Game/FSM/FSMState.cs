using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace StateMachine
{
    /// <summary>
    /// 유한 상태 머신의 기본 상태 클래스입니다.
    /// 모든 상태는 이 클래스를 상속받아 구현해야 합니다.
    /// </summary>
    /// <typeparam name="TEntityType">상태 머신을 소유하는 엔티티 타입</typeparam>
    public abstract class FSMState<TEntityType>
    {
        #region Properties
        
        /// <summary>
        /// 이 상태가 속한 상태 머신
        /// </summary>
        public StateMachine<TEntityType> StateMachine { get; private set; }
        
        /// <summary>
        /// 이 상태를 소유하는 엔티티
        /// </summary>
        public TEntityType Actor { get; private set; }
        
        /// <summary>
        /// 이 상태가 속한 레이어
        /// </summary>
        public int Layer { get; private set; }

        /// <summary>
        /// 상태 이름
        /// </summary>
        public string StateName { get; private set; }

        /// <summary>
        /// 상태 타입
        /// </summary>
        public Type StateType { get; private set; }
        
        /// <summary>
        /// 상태가 활성화되어 있는지 여부
        /// </summary>
        public bool IsActive { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// FSMState 생성자
        /// </summary>
        /// <param name="stateMachine">상태 머신 인스턴스</param>
        /// <param name="actor">엔티티 인스턴스</param>
        /// <param name="layer">레이어 번호</param>
        /// <exception cref="ArgumentNullException">stateMachine 또는 actor가 null인 경우</exception>
        /// <exception cref="InvalidLayerException">layer가 유효하지 않은 경우</exception>
        public FSMState(StateMachine<TEntityType> stateMachine, TEntityType actor, int layer)
        {
            StateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));

            if (layer < 0)
                throw new ArgumentOutOfRangeException($"Layer {layer}는 0 이상이어야 합니다.");

            Layer = layer;
            
            StateName = GetType().Name;
            StateType = GetType();
        }

        #endregion

        #region State Lifecycle Methods

        public void Enter()
        {
            IsActive = true;
            
            OnEnter();
        }

        protected virtual void OnEnter()
        {
            
        }

        public virtual void Update()
        {
            Debug.Assert(IsActive, $"[{StateName}] Update가 비활성 상태에서 호출되었습니다!");
        }
        
        public void Exit()
        {
            IsActive = false;

            OnExit();
        }

        protected virtual void OnExit()
        {
            
        }

        
        #endregion

        #region Message Handling

        /// <summary>
        /// 메시지를 받았을 때 호출됩니다.
        /// </summary>
        /// <param name="message">메시지 ID</param>
        /// <param name="data">추가 데이터</param>
        /// <returns>메시지가 처리되었는지 여부</returns>
        public virtual bool OnReceiveMessage(int message, object data = null)
        {
            Debug.Assert(IsActive, $"[{StateName}] 메시지가 비활성 상태에서 수신되었습니다: {message}");
            return false;
        }
        
        #endregion

        #region Utility Methods
        
        /// <summary>
        /// 상태가 특정 타입인지 확인합니다.
        /// </summary>
        /// <typeparam name="T">확인할 상태 타입</typeparam>
        /// <returns>상태가 해당 타입인지 여부</returns>
        public bool IsOfType<T>() where T : FSMState<TEntityType>
        {
            return this is T;
        }
        
        /// <summary>
        /// 상태 정보를 문자열로 반환합니다.
        /// </summary>
        /// <returns>상태 정보 문자열</returns>
        public override string ToString()
        {
            return $"{StateName} (Layer: {Layer}, Active: {IsActive})";
        }
        
        #endregion
    }
}