using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// [데이터 원본]
/// 룰렛의 모든 Spot (1~36)의 '기본 정의' 데이터를 리스트로 담는 
/// 단일 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "RouletteDefinition", menuName = "Roulette/Roulette Spot Definition (SO)")]
public class SpotSO : ScriptableObject
{
    // 36개의 원본 데이터를 이 리스트 하나에 모두 저장합니다.
    public List<SpotBase> spotBaseList;
}


/// <summary>
/// [데이터 구조체]
/// Spot의 '변하지 않는' 기본 속성. (테이블 데이터)
/// MonoBehaviour가 아니므로 [System.Serializable]이 필수입니다.
/// </summary>
[System.Serializable]
public class SpotBase
{
    [Header("테이블 원본 데이터")]
    public int id;
    public SpotColor color;

    /// <summary>
    /// (데이터 자동 생성을 위해 생성자를 만들어 둡니다)
    /// </summary>
    public SpotBase(int id, SpotColor color)
    {
        this.id = id;
        this.color = color;
    }
}