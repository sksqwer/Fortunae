using System;

namespace StateMachine
{
    /// <summary>
    /// 상태 머신에서 사용되는 상수들을 정의합니다.
    /// </summary>
    public static class StateMachineConstants
    {
        /// <summary>
        /// 명령어 없이 조건만을 나타내는 값
        /// </summary>
        public const int NULL_COMMAND = int.MinValue;
        
        /// <summary>
        /// 기본 레이어 번호
        /// </summary>
        public const int DEFAULT_LAYER = 0;
        
        /// <summary>
        /// 기본 전환 우선순위
        /// </summary>
        public const int DEFAULT_TRANSITION_PRIORITY = 0;
        
    }

    /// <summary>
    /// 상태 머신 관련 예외 클래스들
    /// </summary>
    public class StateMachineException : Exception
    {
        public StateMachineException(string message) : base(message) { }
        public StateMachineException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class StateNotFoundException : StateMachineException
    {
        public StateNotFoundException(string message) : base(message) { }
    }

    public class TransitionNotFoundException : StateMachineException
    {
        public TransitionNotFoundException(string message) : base(message) { }
    }

    public class InvalidLayerException : StateMachineException
    {
        public InvalidLayerException(string message) : base(message) { }
    }
}