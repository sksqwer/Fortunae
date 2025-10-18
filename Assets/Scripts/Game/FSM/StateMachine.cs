using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StateMachine;

namespace StateMachine
{
    /// <summary>
    /// 고성능 유한 상태 머신 클래스입니다.
    /// 다중 레이어, AnyState 패턴, 메시지 시스템을 지원합니다.
    /// </summary>
    /// <typeparam name="TEntityType">상태 머신을 소유하는 엔티티 타입</typeparam>
    public class StateMachine<TEntityType>
    {
        #region Events and Delegates

        /// <summary>
        /// 상태 변경 이벤트 핸들러
        /// </summary>
        /// <param name="stateMachine">상태 머신 인스턴스</param>
        /// <param name="newState">새로운 상태</param>
        /// <param name="previousState">이전 상태</param>
        /// <param name="layer">레이어 번호</param>
        public delegate void StateChangedHandler(
            StateMachine<TEntityType> stateMachine,
            FSMState<TEntityType> newState,
            FSMState<TEntityType> previousState,
            int layer);

        /// <summary>
        /// 상태 변경 이벤트
        /// </summary>
        public event StateChangedHandler OnStateChanged;

        #endregion


        #region Private Classes

        /// <summary>
        /// 상태 데이터를 관리하는 내부 클래스
        /// </summary>
        private class StateData
        {
            public int Layer { get; private set; }
            public FSMState<TEntityType> State { get; private set; }
            public List<StateTransition<TEntityType>> Transitions { get; private set; }
            public Type StateType { get; private set; }

            public StateData(int layer, FSMState<TEntityType> state)
            {
                Debug.Assert(state != null, "StateData 생성 시 state가 null입니다!");

                Layer = layer;
                State = state;
                StateType = state.StateType;
                Transitions = new List<StateTransition<TEntityType>>();
            }

            public void AddTransition(StateTransition<TEntityType> transition)
            {
                Debug.Assert(transition != null, "AddTransition에 transition이 null입니다!");

                Transitions.Add(transition);
            }

            public StateTransition<TEntityType> FindTransition(int command)
            {
                // 조건을 만족하는 전환 중 우선순위(낮을수록 높음)가 가장 높은 전환 선택
                StateTransition<TEntityType> best = null;
                int bestPriority = int.MaxValue;
                for (int i = 0; i < Transitions.Count; i++)
                {
                    var transition = Transitions[i];
                    if (transition != null && transition.Condition(command))
                    {
                        if (transition.Priority < bestPriority)
                        {
                            best = transition;
                            bestPriority = transition.Priority;
                        }
                    }
                }
                return best;
            }

            public StateTransition<TEntityType> FindConditionalTransition()
            {
                // 조건 전환 중 우선순위(낮을수록 높음)가 가장 높은 전환 선택
                StateTransition<TEntityType> best = null;
                int bestPriority = int.MaxValue;
                for (int i = 0; i < Transitions.Count; i++)
                {
                    var transition = Transitions[i];
                    if (transition != null && transition.Condition())
                    {
                        if (transition.Priority < bestPriority)
                        {
                            best = transition;
                            bestPriority = transition.Priority;
                        }
                    }
                }
                return best;
            }
        }

        #endregion

        #region Private Fields

        // Dictionary 기반 스토리지 (동적인 레이어 관리)
        private readonly Dictionary<int, List<StateData>> layerStates;
        private readonly Dictionary<int, StateData> currentStates;
        private readonly Dictionary<int, StateData> previousStates;
        private readonly Dictionary<int, StateData> anyStates;
        private readonly List<int> layerIterationBuffer;

        #endregion

        #region Public Properties

        /// <summary>
        /// 상태 머신의 소유자
        /// </summary>
        public TEntityType Owner { get; private set; }

        /// <summary>
        /// 활성화된 레이어 수
        /// </summary>
        public int ActiveLayerCount => layerStates.Count;

        #endregion

        #region Constructor

        /// <summary>
        /// StateMachine 생성자
        /// </summary>
        /// <param name="owner">상태 머신의 소유자</param>
        /// <exception cref="ArgumentNullException">owner가 null인 경우</exception>
        public StateMachine(TEntityType owner)
        {
            Debug.Assert(owner != null, "StateMachine 생성 시 owner가 null입니다!");
            Owner = owner;


            // Dictionary 초기화
            layerStates = new Dictionary<int, List<StateData>>();
            currentStates = new Dictionary<int, StateData>();
            previousStates = new Dictionary<int, StateData>();
            anyStates = new Dictionary<int, StateData>();
            layerIterationBuffer = new List<int>();

            SetupStates();
            SetupTransitions();
            SetupLayers();

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 상태 머신을 업데이트합니다.
        /// </summary>
        public void Update()
        {
            Debug.Assert(currentStates.Count > 0, "[StateMachine] currentStates가 비어있습니다! 상태 머신이 제대로 초기화되지 않았습니다.");

            // 열거 중 컬렉션 수정 방지를 위한 레이어 키 스냅샷
            layerIterationBuffer.Clear();
            layerIterationBuffer.AddRange(currentStates.Keys);

            for (int i = 0; i < layerIterationBuffer.Count; i++)
            {
                int layer = layerIterationBuffer[i];
                if (!currentStates.TryGetValue(layer, out var stateData) || stateData == null)
                    continue;

                bool transitionOccurred = CheckAllTransitions(layer);

                if (!transitionOccurred)
                {
                    stateData.State.Update();
                }
            }
        }

        /// <summary>
        /// 특정 레이어에 명령을 전달합니다.
        /// </summary>
        /// <param name="transitionCommand">전환 명령</param>
        /// <param name="layer">레이어 번호</param>
        /// <returns>명령 실행 성공 여부</returns>
        public bool ExecuteCommand(int transitionCommand, int layer)
        {
            if (!currentStates.ContainsKey(layer))
                return false;

            StateTransition<TEntityType> foundTransition = null;

            // AnyState 전환 확인
            if (anyStates.TryGetValue(layer, out var anyState))
            {
                foundTransition = anyState.FindTransition(transitionCommand);
            }

            // 현재 상태 전환 확인
            if (foundTransition == null && currentStates.TryGetValue(layer, out var currentState))
            {
                foundTransition = currentState.FindTransition(transitionCommand);
            }

            if (foundTransition != null && foundTransition.IsValid())
            {
                if (!foundTransition.CanTransitionToSelf &&
                    currentStates[layer].State == foundTransition.ToState)
                {
                    return false;
                }

                ChangeState(foundTransition.ToState, layer);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Enum 명령을 전달합니다.
        /// </summary>
        /// <param name="transitionCommand">전환 명령</param>
        /// <param name="layer">레이어 번호</param>
        /// <returns>명령 실행 성공 여부</returns>
        public bool ExecuteCommand(Enum transitionCommand, int layer)
            => ExecuteCommand(Convert.ToInt32(transitionCommand), layer);

        /// <summary>
        /// 모든 레이어에 명령을 전달합니다.
        /// </summary>
        /// <param name="transitionCommand">전환 명령</param>
        /// <returns>최소 하나의 레이어가 성공했는지 여부</returns>
        public bool ExecuteCommand(int transitionCommand)
        {
            bool success = false;
            foreach (var layer in layerStates.Keys)
            {
                if (ExecuteCommand(transitionCommand, layer))
                    success = true;
            }
            return success;
        }

        /// <summary>
        /// 모든 레이어에 Enum 명령을 전달합니다.
        /// </summary>
        /// <param name="transitionCommand">전환 명령</param>
        /// <returns>최소 하나의 레이어가 성공했는지 여부</returns>
        public bool ExecuteCommand(Enum transitionCommand)
            => ExecuteCommand(Convert.ToInt32(transitionCommand));

        /// <summary>
        /// 특정 레이에 메시지를 전달합니다.
        /// </summary>
        /// <param name="message">메시지 ID</param>
        /// <param name="layer">레이어 번호</param>
        /// <param name="extraData">추가 데이터</param>
        /// <returns>메시지 처리 성공 여부</returns>
        public bool SendMessage(int message, int layer, object extraData = null)
        {
            if (!currentStates.TryGetValue(layer, out var stateData) || stateData == null)
                return false;

            return stateData.State.OnReceiveMessage(message, extraData);
        }

        /// <summary>
        /// Enum 메시지를 전달합니다.
        /// </summary>
        /// <param name="message">메시지</param>
        /// <param name="layer">레이어 번호</param>
        /// <param name="extraData">추가 데이터</param>
        /// <returns>메시지 처리 성공 여부</returns>
        public bool SendMessage(Enum message, int layer, object extraData = null)
            => SendMessage(Convert.ToInt32(message), layer, extraData);

        /// <summary>
        /// 모든 레이에 메시지를 전달합니다.
        /// </summary>
        /// <param name="message">메시지 ID</param>
        /// <param name="extraData">추가 데이터</param>
        /// <returns>최소 하나의 레이어가 성공했는지 여부</returns>
        public bool SendMessage(int message, object extraData = null)
        {
            bool success = false;
            foreach (var layer in layerStates.Keys)
            {
                if (SendMessage(message, layer, extraData))
                    success = true;
            }
            return success;
        }

        /// <summary>
        /// 모든 레이에 Enum 메시지를 전달합니다.
        /// </summary>
        /// <param name="message">메시지</param>
        /// <param name="extraData">추가 데이터</param>
        /// <returns>최소 하나의 레이어가 성공했는지 여부</returns>
        public bool SendMessage(Enum message, object extraData = null)
            => SendMessage(Convert.ToInt32(message), extraData);

        /// <summary>
        /// 특정 상태에 있는지 확인합니다.
        /// </summary>
        /// <typeparam name="T">확인할 상태 타입</typeparam>
        /// <returns>해당 상태에 있는지 여부</returns>
        public bool IsInState<T>() where T : FSMState<TEntityType>
        {
            foreach (var stateData in currentStates.Values)
            {
                if (stateData?.State is T)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 특정 레이어가 특정 상태에 있는지 확인합니다.
        /// </summary>
        /// <typeparam name="T">확인할 상태 타입</typeparam>
        /// <param name="layer">레이어 번호</param>
        /// <returns>해당 상태에 있는지 여부</returns>
        public bool IsInState<T>(int layer) where T : FSMState<TEntityType>
        {
            if (!currentStates.TryGetValue(layer, out var stateData) || stateData == null)
                return false;

            return stateData.State is T;
        }

        /// <summary>
        /// 현재 상태를 반환합니다.
        /// </summary>
        /// <param name="layer">레이어 번호</param>
        /// <returns>현재 상태</returns>
        /// <exception cref="StateNotFoundException">레이어 현재 상태가 없는 경우</exception>
        public FSMState<TEntityType> GetCurrentState(int layer = StateMachineConstants.DEFAULT_LAYER)
        {
            Debug.Assert(currentStates.TryGetValue(layer, out var stateData) && stateData != null,
                $"Layer {layer}에 현재 상태가 없습니다!");

            return stateData.State;
        }

        /// <summary>
        /// 현재 상태의 타입을 반환합니다.
        /// </summary>
        /// <param name="layer">레이어 번호</param>
        /// <returns>현재 상태의 타입</returns>
        /// <exception cref="StateNotFoundException">레이어 현재 상태가 없는 경우</exception>
        public Type GetCurrentStateType(int layer = StateMachineConstants.DEFAULT_LAYER)
        {
            Debug.Assert(currentStates.TryGetValue(layer, out var stateData) && stateData != null,
                $"Layer {layer}에 현재 상태가 없습니다!");

            return stateData.StateType;
        }

        /// <summary>
        /// 이전 상태를 반환합니다.
        /// </summary>
        /// <param name="layer">레이어 번호</param>
        /// <returns>이전 상태 (없으면 null)</returns>
        public FSMState<TEntityType> GetPreviousState(int layer = StateMachineConstants.DEFAULT_LAYER)
        {
            if (!previousStates.TryGetValue(layer, out var stateData))
                return null;

            return stateData?.State;
        }

        #endregion

        #region State Management

        /// <summary>
        /// 상태를 추가합니다.
        /// </summary>
        /// <typeparam name="TStateType">추가할 상태 타입</typeparam>
        /// <param name="layer">레이어 번호</param>
        /// <exception cref="InvalidOperationException">이미 상태가 존재하거나 레이어 최대 수 초과</exception>
        public void AddState<TStateType>(int layer = StateMachineConstants.DEFAULT_LAYER)
            where TStateType : FSMState<TEntityType>
        {
            // 레이어 리스트 생성
            if (!layerStates.ContainsKey(layer))
            {
                layerStates[layer] = new List<StateData>();


                var anyState = CreateState<AnyState<TEntityType>>(layer);
                var anyStateData = new StateData(layer, anyState);
                layerStates[layer].Add(anyStateData);
                anyStates[layer] = anyStateData;
            }

            Debug.Assert(!HasState<TStateType>(layer),
                $"Layer {layer}에 {typeof(TStateType).Name} 상태가 이미 존재합니다!");

            var state = CreateState<TStateType>(layer);
            var stateData = new StateData(layer, state);

            layerStates[layer].Add(stateData);
        }

        /// <summary>
        /// 상태 간 전환을 정의합니다.
        /// </summary>
        /// <typeparam name="FromStateType">시작 상태 타입</typeparam>
        /// <typeparam name="ToStateType">목표 상태 타입</typeparam>
        /// <param name="transitionCommand">전환 명령</param>
        /// <param name="transitionCondition">전환 조건</param>
        /// <param name="layer">레이어 번호</param>
        /// <param name="canTransitionToSelf">자기 자신으로의 전환 허용 여부</param>
        /// <param name="priority">전환 우선순위</param>
        public void MakeTransition<FromStateType, ToStateType>(
            int transitionCommand = StateMachineConstants.NULL_COMMAND,
            Func<FSMState<TEntityType>, bool> transitionCondition = null,
            int layer = StateMachineConstants.DEFAULT_LAYER,
            bool canTransitionToSelf = false,
            int priority = StateMachineConstants.DEFAULT_TRANSITION_PRIORITY)
            where FromStateType : FSMState<TEntityType>
            where ToStateType : FSMState<TEntityType>
        {
            var fromStateData = GetStateData<FromStateType>(layer);
            var toStateData = GetStateData<ToStateType>(layer);

            Debug.Assert(fromStateData != null,
                $"Layer {layer}에 {typeof(FromStateType).Name} 상태를 찾을 수 없습니다!");

            Debug.Assert(toStateData != null,
                $"Layer {layer}에 {typeof(ToStateType).Name} 상태를 찾을 수 없습니다!");

            var transition = new StateTransition<TEntityType>(
                fromStateData.State,
                toStateData.State,
                transitionCommand,
                transitionCondition,
                canTransitionToSelf,
                priority);

            fromStateData.AddTransition(transition);
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
            => MakeTransition<FromStateType, ToStateType>(Convert.ToInt32(transitionCommand), transitionCondition, layer, canTransitionToSelf, priority);

        #endregion

        #region Private Methods

        /// <summary>
        /// 레이어 초기화합니다.
        /// </summary>
        private void SetupLayers()
        {
            foreach (var layer in layerStates.Keys)
            {
                var initialStateData = GetInitialStateData(layer);
                if (initialStateData != null)
                {
                    ChangeState(initialStateData);
                }
            }
        }

        /// <summary>
        /// 모든 레이어의 초기 상태를 설정합니다. (외부에서 호출 가능)
        /// </summary>
        public void InitializeAllLayers()
        {
            SetupLayers();
        }

        /// <summary>
        /// 상태를 변경합니다.
        /// </summary>
        /// <param name="newStateData">새로운 상태 데이터</param>
        private void ChangeState(StateData newStateData)
        {
            if (newStateData == null)
                return;

            int layer = newStateData.Layer;

            // 안전하게 이전 상태 가져오기
            currentStates.TryGetValue(layer, out var previousStateData);
            previousStateData?.State.Exit();

            // 이전 상태 저장
            if (previousStateData != null)
            {
                previousStates[layer] = previousStateData;
            }

            currentStates[layer] = newStateData;
            newStateData.State.Enter();

            OnStateChanged?.Invoke(this, newStateData.State, previousStateData?.State, layer);
        }

        /// <summary>
        /// 상태를 변경합니다.
        /// </summary>
        /// <param name="newState">새로운 상태</param>
        /// <param name="layer">레이어 번호</param>
        private void ChangeState(FSMState<TEntityType> newState, int layer)
        {
            var newStateData = GetStateDataByType(newState.StateType, layer); // 캐시된 타입 사용
            if (newStateData != null)
            {
                ChangeState(newStateData);
            }
        }

        /// <summary>
        /// 모든 전환을 확인합니다.
        /// </summary>
        /// <param name="layer">레이어 번호</param>
        /// <returns>전환이 발생했는지 여부</returns>
        private bool CheckAllTransitions(int layer)
        {
            if (!currentStates.TryGetValue(layer, out var currentState) || currentState == null)
                return false;

            // AnyState 전환 확인
            if (anyStates.TryGetValue(layer, out var anyState))
            {
                var anyTransition = anyState.FindConditionalTransition();
                if (anyTransition != null && anyTransition.IsValid())
                {
                    if (anyTransition.CanTransitionToSelf || currentState.State != anyTransition.ToState)
                    {
                        ChangeState(anyTransition.ToState, layer);
                        return true;
                    }
                }
            }

            // 현재 상태 전환 확인
            var currentTransition = currentState.FindConditionalTransition();
            if (currentTransition != null && currentTransition.IsValid())
            {
                if (currentTransition.CanTransitionToSelf || currentState.State != currentTransition.ToState)
                {
                    ChangeState(currentTransition.ToState, layer);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 상태를 생성합니다.
        /// </summary>
        /// <typeparam name="TStateType">상태 타입</typeparam>
        /// <param name="layer">레이어 번호</param>
        /// <returns>생성된 상태</returns>
        private FSMState<TEntityType> CreateState<TStateType>(int layer) where TStateType : FSMState<TEntityType>
        {
            return (TStateType)Activator.CreateInstance(typeof(TStateType), this, Owner, layer);
        }


        /// <summary>
        /// 특정 타입의 상태 데이터를 반환합니다.
        /// </summary>
        /// <typeparam name="TStateType">상태 타입</typeparam>
        /// <param name="layer">레이어 번호</param>
        /// <returns>상태 데이터</returns>
        private StateData GetStateData<TStateType>(int layer) where TStateType : FSMState<TEntityType>
        {
            return GetStateDataByType(typeof(TStateType), layer);
        }

        /// <summary>
        /// 특정 타입의 상태 데이터를 반환합니다.
        /// </summary>
        /// <param name="stateType">상태 타입</param>
        /// <param name="layer">레이어 번호</param>
        /// <returns>상태 데이터</returns>
        private StateData GetStateDataByType(Type stateType, int layer)
        {
            if (!layerStates.TryGetValue(layer, out var states))
                return null;

            for (int i = 0; i < states.Count; i++)
            {
                var stateData = states[i];
                if (stateData != null && stateData.StateType == stateType)
                    return stateData;
            }

            return null;
        }

        /// <summary>
        /// 특정 상태가 존재하는지 확인합니다.
        /// </summary>
        /// <typeparam name="TStateType">상태 타입</typeparam>
        /// <param name="layer">레이어 번호</param>
        /// <returns>상태 존재 여부</returns>
        private bool HasState<TStateType>(int layer) where TStateType : FSMState<TEntityType>
        {
            return GetStateData<TStateType>(layer) != null;
        }

        /// <summary>
        /// 초기 상태 데이터를 반환합니다.
        /// 첫 번째로 추가된 상태를 초기 상태로 선택합니다 (AnyState 제외).
        /// </summary>
        /// <param name="layer">레이어 번호</param>
        /// <returns>초기 상태 데이터</returns>
        private StateData GetInitialStateData(int layer)
        {
            if (!layerStates.TryGetValue(layer, out var states) || states.Count == 0)
                return null;

            // AnyState가 아닌 첫 번째 상태를 초기 상태로 선택
            for (int i = 0; i < states.Count; i++)
            {
                var stateData = states[i];
                if (stateData != null && !(stateData.State is AnyState<TEntityType>))
                {
                    return stateData;
                }
            }

            return null;
        }


        #endregion

        #region Virtual Setup Methods (for sub-classes)

        /// <summary>
        /// 상태 설정 (서브클래스에서 오버라이드)
        /// </summary>
        protected virtual void SetupStates() { }

        /// <summary>
        /// 전환 설정 (서브클래스에서 오버라이드)
        /// </summary>
        protected virtual void SetupTransitions() { }

        #endregion
    }
}