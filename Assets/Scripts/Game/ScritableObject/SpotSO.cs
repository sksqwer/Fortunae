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
    public float basePayout = 36f; // 생성자에서 사용되므로 필드 선언이 필요합니다.

    [Header("그룹 속성 (베팅 판정용)")]
    public bool isOdd;
    public bool isHighNumber; // (19~36이면 true)
    public int dozenGroup;    // (1: 1-12, 2: 13-24, 3: 25-36)
    public int columnGroup;   // (1, 2, 3)

    /// <summary>
    /// (데이터 자동 생성을 위해 생성자를 만들어 둡니다)
    /// </summary>
    public SpotBase(int id)
    {
        this.id = id;
        this.basePayout = 36f;

        // --- 모든 속성 자동 계산 ---

        // 홀/짝
        this.isOdd = (id % 2 != 0);

        // High/Low (19-36이 High)
        this.isHighNumber = (id >= 19);

        // Dozen (1-12, 13-24, 25-36)
        if (id <= 12) this.dozenGroup = 1;
        else if (id <= 24) this.dozenGroup = 2;
        else this.dozenGroup = 3;

        // Column (1, 2, 3)
        if (id % 3 == 1) this.columnGroup = 1;      // 1, 4, 7...
        else if (id % 3 == 2) this.columnGroup = 2; // 2, 5, 8...
        else this.columnGroup = 3;                  // 3, 6, 9...
    }
}