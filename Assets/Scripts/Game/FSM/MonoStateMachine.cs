using System;
using UnityEngine;

namespace StateMachine
{
    /// <summary>
    /// MonoBehaviour를 상속받는 StateMachine 래퍼 클래스입니다.
    /// Unity의 Update 루프와 통합되어 자동으로 상태 머신을 업데이트합니다.
    /// </summary>
    /// <typeparam name="TEntityType">상태 머신을 소유하는 엔티티 타입</typeparam>
    public abstract class MonoStateMachine<TEntityType> : MonoBehaviour where TEntityType : class
    {
        #region Fields

        /// <summary>
        /// 내부 상태 머신
        /// </summary>
        private StateMachine<TEntityType> stateMachine;

        /// <summary>
        /// 상태 머신의 소유자
        /// </summary>
        protected TEntityType owner;

        /// <summary>
        /// 디버거 (Inspector에서 설정 가능)
        /// Unity 에디터를 새로고침하면 활성화됩니다.
        /// </summary>
        // [SerializeField] protected StateMachineDebugger debugger = new StateMachineDebugger();

        #endregion

        #region Properties

        /// <summary>
        /// 상태 머신 인스턴스
        /// </summary>
        public StateMachine<TEntityType> StateMachine => stateMachine;

        /// <summary>
        /// 활성화된 레이어 수
        /// </summary>
        public int ActiveLayerCount => stateMachine?.ActiveLayerCount ?? 0;

        /// <summary>
        /// 디버거 인스턴스
        /// Unity 에디터를 새로고침하면 활성화됩니다.
        /// </summary>
        // public StateMachineDebugger Debugger => debugger;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Unity Awake 콜백
        /// </summary>
        protected virtual void Awake()
        {
            InitializeOwner();
            
            if (owner == null)
            {
                enabled = false;
                return;
            }
            
            InitializeStateMachine();
        }

        /// <summary>
        /// Unity Update 콜백 - 상태 머신 업데이트
        /// </summary>
        private void Update()
        {
            stateMachine?.Update();
            OnUpdate();
        }

        /// <summary>
        /// Unity OnDestroy 콜백
        /// </summary>
        protected void OnDestroy()
        {
            stateMachine = null;
            owner = null;
        }
        
        /// <summary>
        /// Unity Update 콜백 - 상태 머신 업데이트
        /// </summary>
        protected virtual void OnUpdate()
        {
            stateMachine?.Update();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 소유자를 초기화합니다. 서브클래스에서 구현해야 합니다.
        /// </summary>
        protected abstract void InitializeOwner();

        /// <summary>
        /// 상태 머신을 초기화합니다.
        /// </summary>
        private void InitializeStateMachine()
        {
            // StateMachine 생성
            stateMachine = new StateMachine<TEntityType>(owner);
            
            // 1단계: 상태 설정 (전환 설정 전에 먼저 상태를 추가해야 함)
            SetupStates();
            
            // 2단계: 전환 설정 (상태가 추가된 후에 전환 설정)
            SetupTransitions();
            
            // 3단계: 초기 상태 설정
            stateMachine.InitializeAllLayers();
        }
        
        /// <summary>
        /// 상태 변경 디버그 콜백 (Unity 에디터 새로고침 후 활성화)
        /// </summary>
        // private void OnStateChangedDebug(StateMachine<TEntityType> sm, FSMState<TEntityType> newState, FSMState<TEntityType> prevState, int layer)
        // {
        //     debugger?.RecordTransition(
        //         prevState?.StateName ?? "None",
        //         newState?.StateName ?? "None",
        //         layer,
        //         "Auto Transition"
        //     );
        // }
        
        /// <summary>
        /// 상태들을 설정합니다. 서브클래스에서 오버라이드하여 사용합니다.
        /// </summary>
        protected virtual void SetupStates()
        {
            // 서브클래스에서 구현
            // 예:
            // AddState<IdleState>();
            // AddState<MoveState>();
        }
        
        /// <summary>
        /// 전환들을 설정합니다. 서브클래스에서 오버라이드하여 사용합니다.
        /// </summary>
        protected virtual void SetupTransitions()
        {
            // 서브클래스에서 구현
            // 예:
            // MakeTransition<IdleState, MoveState>(...);
        }

        #endregion

        #region Public Methods - State Management

        /// <summary>
        /// 상태를 추가합니다.
        /// </summary>
        /// <typeparam name="TStateType">추가할 상태 타입</typeparam>
        /// <param name="layer">레이어 번호</param>
        public void AddState<TStateType>(int layer = StateMachineConstants.DEFAULT_LAYER)
            where TStateType : FSMState<TEntityType>
        {
            stateMachine?.AddState<TStateType>(layer);
        }

        /// <summary>
        /// 상태 간 전환을 정의합니다.
        /// </summary>
        public void MakeTransition<FromStateType, ToStateType>(
            int transitionCommand = StateMachineConstants.NULL_COMMAND,
            Func<FSMState<TEntityType>, bool> transitionCondition = null,
            int layer = StateMachineConstants.DEFAULT_LAYER,
            bool canTransitionToSelf = false,
            int priority = StateMachineConstants.DEFAULT_TRANSITION_PRIORITY)
            where FromStateType : FSMState<TEntityType>
            where ToStateType : FSMState<TEntityType>
        {
            stateMachine?.MakeTransition<FromStateType, ToStateType>(
                transitionCommand, transitionCondition, layer, canTransitionToSelf, priority);
        }

        /// <summary>
        /// Enum 명령을 사용한 전환을 정의합니다.
        /// </summary>
        public void MakeTransition<FromStateType, ToStateType>(
            Enum transitionCommand,
            Func<FSMState<TEntityType>, bool> transitionCondition = null,
            int layer = StateMachineConstants.DEFAULT_LAYER,
            bool canTransitionToSelf = false,
            int priority = StateMachineConstants.DEFAULT_TRANSITION_PRIORITY)
            where FromStateType : FSMState<TEntityType>
            where ToStateType : FSMState<TEntityType>
        {
            stateMachine?.MakeTransition<FromStateType, ToStateType>(
                transitionCommand, transitionCondition, layer, canTransitionToSelf, priority);
        }

        #endregion

        #region Public Methods - Command Execution

        /// <summary>
        /// 특정 레이어에 명령을 전달합니다.
        /// </summary>
        public bool ExecuteCommand(int command, int layer)
        {
            return stateMachine?.ExecuteCommand(command, layer) ?? false;
        }

        /// <summary>
        /// 모든 레이어에 명령을 전달합니다.
        /// </summary>
        public bool ExecuteCommand(int command)
        {
            return stateMachine?.ExecuteCommand(command) ?? false;
        }

        /// <summary>
        /// Enum 명령을 전달합니다.
        /// </summary>
        public bool ExecuteCommand(Enum command, int layer)
        {
            return stateMachine?.ExecuteCommand(command, layer) ?? false;
        }

        /// <summary>
        /// 모든 레이어에 Enum 명령을 전달합니다.
        /// </summary>
        public bool ExecuteCommand(Enum command)
        {
            return stateMachine?.ExecuteCommand(command) ?? false;
        }

        #endregion

        #region Public Methods - Message System

        /// <summary>
        /// 특정 레이에 메시지를 전달합니다.
        /// </summary>
        public bool SendMessage(int message, int layer, object extraData = null)
        {
            return stateMachine?.SendMessage(message, layer, extraData) ?? false;
        }

        /// <summary>
        /// 모든 레이에 메시지를 전달합니다.
        /// </summary>
        public bool SendMessage(int message, object extraData = null)
        {
            return stateMachine?.SendMessage(message, extraData) ?? false;
        }

        /// <summary>
        /// Enum 메시지를 전달합니다.
        /// </summary>
        public bool SendMessage(Enum message, int layer, object extraData = null)
        {
            return stateMachine?.SendMessage(message, layer, extraData) ?? false;
        }

        /// <summary>
        /// 모든 레이에 Enum 메시지를 전달합니다.
        /// </summary>
        public bool SendMessage(Enum message, object extraData = null)
        {
            return stateMachine?.SendMessage(message, extraData) ?? false;
        }

        #endregion

        #region Public Methods - State Queries

        /// <summary>
        /// 특정 상태에 있는지 확인합니다.
        /// </summary>
        public bool IsInState<T>() where T : FSMState<TEntityType>
        {
            return stateMachine?.IsInState<T>() ?? false;
        }

        /// <summary>
        /// 특정 레이어가 특정 상태에 있는지 확인합니다.
        /// </summary>
        public bool IsInState<T>(int layer) where T : FSMState<TEntityType>
        {
            return stateMachine?.IsInState<T>(layer) ?? false;
        }

        /// <summary>
        /// 현재 상태를 반환합니다.
        /// </summary>
        public FSMState<TEntityType> GetCurrentState(int layer = StateMachineConstants.DEFAULT_LAYER)
        {
            return stateMachine?.GetCurrentState(layer);
        }

        /// <summary>
        /// 현재 상태의 타입을 반환합니다.
        /// </summary>
        public Type GetCurrentStateType(int layer = StateMachineConstants.DEFAULT_LAYER)
        {
            return stateMachine?.GetCurrentStateType(layer);
        }

        /// <summary>
        /// 이전 상태를 반환합니다.
        /// </summary>
        public FSMState<TEntityType> GetPreviousState(int layer = StateMachineConstants.DEFAULT_LAYER)
        {
            return stateMachine?.GetPreviousState(layer);
        }

        #endregion
    }
}