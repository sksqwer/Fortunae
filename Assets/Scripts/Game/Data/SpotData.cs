using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 룰렛의 각 숫자 칸(Spot)이 가지는 모든 '데이터'를 정의하는 모델 클래스입니다.
/// '두뇌' 역할을 하며, MonoBehaviour를 상속받지 않습니다.
/// </summary>
[System.Serializable]
public class SpotData
{
    // --- 기본 정보 (변하지 않음) ---
    [Tooltip("Spot의 고유 ID (1~36)")]
    public int id;
    [Tooltip("Spot의 기본 색상")]
    public string color;

    // --- 동적 상태 정보 (아이템으로 변경됨) ---
    [Tooltip("플레이어에게 실제 표시되는 숫자")]
    public int displayNumber;
    [Tooltip("Death 참으로 파괴되었는지 여부")]
    public bool isDestroyed;
    [Tooltip("배당률에 곱해지는 배수 (Chameleon, Upgraded 등)")]
    public float payoutMultiplier;

    // --- 실시간 계산 결과 ---
    [Tooltip("현재 턴의 실시간 당첨 확률 (GameManager가 계산)")]
    public float currentProbability;

    // --- 상태 기록용 ---
    [Tooltip("현재 이 Spot에 적용된 효과 목록")]
    public List<string> effects;


    /// <summary>
    /// 이 Spot의 최종 배당률을 계산하여 반환합니다.
    /// (기본 배당률 x36 * 아이템 배수)
    /// </summary>
    public float FinalPayout
    {
        get { return 36f * payoutMultiplier; }
    }
    
    /// <summary>
    /// SpotData 생성자 (GameManager가 처음 36개를 만들 때 호출)
    /// </summary>
    public SpotData(int id, string color)
    {
        this.id = id;
        this.color = color;
        
        // 초기 상태 설정
        this.displayNumber = id;
        this.isDestroyed = false;
        this.payoutMultiplier = 1.0f;
        this.effects = new List<string>();

        this.currentProbability = 1f; 
    }
}