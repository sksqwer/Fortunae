using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 룰렛 판의 물리적인 '칸' (1~36)의 현재 '상태'를 저장하는
/// 핵심 데이터 모델 (런타임 상태)
/// </summary>
public class Spot
{
    // === 원본 데이터 참조 (불변) ===
    public readonly SpotBase spotBase;  // ScriptableObject 원본 데이터
    public int SpotID => spotBase.id;
    public SpotColor OriginalColor => spotBase.color;

    // === 상태 정보 (아이템으로 변경됨) ===
    public int currentNumber;           // 현재 표시 숫자 (PlusSpot 적용)
    public SpotColor currentColor;      // 현재 색상 (아이템으로 변경 가능)
    public bool isDestroyed;            // 파괴 여부 (Death Charm)
    
    // === 확률/배당 정보 (계산 결과) ===
    public double baseProbability;         // 기본 확률 (1/36)
    public double currentProbability;      // 현재 확률 (계산기가 재계산)
    public double basePayoutMultiplier;    // 기본 배당 (x36)
    public double currentPayoutMultiplier; // 현재 배당 (계산기가 재계산)

    // === 적용된 아이템 내역 (순서가 중요! Active/Passive 구분 없이 하나로) ===
    public List<AppliedItemRecord> appliedRecords;
    
    // 4로 나누어떨어지는지 체크 (Death 판정용: 4, 14, 24, 34)
    public bool HasNumber4() => currentNumber % 10 == 4;

    // 생성자 (SpotBase 기반)
    public Spot(SpotBase spotBase)
    {
        this.spotBase = spotBase;
        this.currentNumber = spotBase.id;
        this.currentColor = spotBase.color;
        this.isDestroyed = false;
        
        // 확률/배당 초기화
        this.baseProbability = 1.0 / 36.0;
        this.currentProbability = baseProbability;
        this.basePayoutMultiplier = 36.0;
        this.currentPayoutMultiplier = basePayoutMultiplier;
        
        // 아이템 내역 초기화
        this.appliedRecords = new List<AppliedItemRecord>();
    }
    
    // 인접 스팟 ID 반환 (UpgradedMultiSpot용)
    public List<int> GetAdjacentSpots()
    {
        List<int> adjacent = new List<int>();
        int id = SpotID;
        
        // 룰렛 레이아웃: 3x12 그리드 가정
        int row = (id - 1) / 3;
        int col = (id - 1) % 3;
        
        // 상하좌우
        if (row > 0) adjacent.Add((row - 1) * 3 + col + 1);    // 위
        if (row < 11) adjacent.Add((row + 1) * 3 + col + 1);   // 아래
        if (col > 0) adjacent.Add(row * 3 + col);              // 왼쪽
        if (col < 2) adjacent.Add(row * 3 + col + 2);          // 오른쪽
        
        return adjacent;
    }   
    
    // 아이템 기록 추가 (순서대로)
    public void AddRecord(AppliedItemRecord record)
    {
        appliedRecords.Add(record);
    }
}