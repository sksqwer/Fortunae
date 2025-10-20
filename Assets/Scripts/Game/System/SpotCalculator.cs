using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spot 상태, 확률, 배당률을 계산하는 통합 시스템
/// </summary>
public static class SpotCalculator
{
    // ===================================================
    // 메인 계산 메서드
    // ===================================================
    
    /// <summary>
    /// 모든 Spot의 상태, 확률, 배당률을 재계산
    /// </summary>
    public static void RecalculateAll(GameState gameState)
    {
        Debug.Log("[SpotCalculator] ========== Recalculating All Spots ==========");
        
        RecalculateSpotStates(gameState);
        CalculateProbabilities(gameState.spots);
        CalculatePayouts(gameState.spots);
        
        Debug.Log("[SpotCalculator] ========== Recalculation Complete ==========");
    }
    
    
    // ===================================================
    // 1. Spot 상태 계산 (숫자, 색깔, 파괴 여부)
    // ===================================================
    
    /// <summary>
    /// 모든 Spot을 초기화하고 appliedRecords 기반으로 상태 재계산
    /// </summary>
    private static void RecalculateSpotStates(GameState gameState)
    {
        Debug.Log("[SpotCalculator] === Step 1: Recalculating Spot States ===");
        
        // 1단계: 모든 Spot 초기화 (기록은 유지)
        foreach (var spot in gameState.spots.Values)
        {
            spot.currentNumber = spot.spotBase.id;
            spot.currentColor = spot.spotBase.color;
            // isDestroyed는 리셋 안 함 (Death 효과 유지)
        }
        
        // 2단계: 모든 Spot의 기록을 순회하며 효과 적용
        foreach (var spot in gameState.spots.Values)
        {
            ApplyRecordsToSpot(spot);
        }
    }
    
    /// <summary>
    /// 특정 Spot에 기록된 모든 효과 적용
    /// </summary>
    private static void ApplyRecordsToSpot(Spot spot)
    {
        if (spot.appliedRecords.Count == 0)
            return;
        
        Debug.Log($"  [Spot {spot.SpotID}] Applying {spot.appliedRecords.Count} records");
        
        foreach (var record in spot.appliedRecords)
        {
            // PlusSpot: 숫자 변경
            if (record.itemData.itemType == ItemType.SpotItem && 
                record.itemData.spotItemType == SpotItemType.PlusSpot)
            {
                spot.currentNumber = record.appliedValue;
                Debug.Log($"    - PlusSpot: → {record.appliedValue}");
            }
            
            // Death: 파괴 상태 적용
            if (record.itemData.itemType == ItemType.CharmItem && 
                record.itemData.charmType == CharmType.Death)
            {
                spot.isDestroyed = true;
                Debug.Log($"    - Death: Destroyed");
            }
        }
    }
    
    
    // ===================================================
    // 2. 확률 계산
    // ===================================================
    
    /// <summary>
    /// 모든 Spot의 당첨 확률 계산
    /// </summary>
    private static void CalculateProbabilities(Dictionary<int, Spot> spots)
    {
        Debug.Log("[SpotCalculator] === Step 2: Calculating Probabilities ===");
        
        // 배수 효과 수집 (확률 계산에는 UpgradedMultiSpot만 영향)
        Dictionary<int, double> multipliers = CollectMultipliers(spots);
        
        // 각 Spot의 가중치 계산
        Dictionary<int, double> weights = new Dictionary<int, double>();
        double totalWeight = 0.0;
        
        foreach (var pair in spots)
        {
            int spotID = pair.Key;
            Spot spot = pair.Value;
            
            // 파괴된 Spot은 확률 0
            if (spot.isDestroyed)
            {
                weights[spotID] = 0.0;
                spot.currentProbability = 0.0;
                Debug.Log($"  Spot {spotID}: Destroyed → 0%");
                continue;
            }
            
            // 기본 가중치 = 1.0
            double weight = 1.0;
            
            // 배수 효과 적용
            if (multipliers.ContainsKey(spotID))
            {
                weight *= multipliers[spotID];
            }
            
            weights[spotID] = weight;
            totalWeight += weight;
        }
        
        // 3단계: 정규화 (전체 합 = 1.0)
        if (totalWeight > 0)
        {
            foreach (var pair in spots)
            {
                int spotID = pair.Key;
                Spot spot = pair.Value;
                
                if (spot.isDestroyed)
                    continue;
                
                spot.currentProbability = weights[spotID] / totalWeight;
                Debug.Log($"  Spot {spotID}: {spot.currentProbability * 100:F2}%");
            }
        }
        else
        {
            Debug.LogError("[SpotCalculator] Total weight is 0! All spots destroyed?");
        }
        
        // 4단계: 검증 (합이 1.0인지)
        double sum = 0.0;
        foreach (var spot in spots.Values)
        {
            sum += spot.currentProbability;
        }
        
        Debug.Log($"  Total Probability: {sum * 100:F4}% (Expected: 100%)");
        
        if (Mathf.Abs((float)(sum - 1.0)) > 0.0001f)
        {
            Debug.LogWarning($"  [Warning] Probability sum is not 1.0! ({sum})");
        }
    }
    
    
    // ===================================================
    // 3. 배당률 계산
    // ===================================================
    
    /// <summary>
    /// 모든 Spot의 배당률 계산
    /// </summary>
    private static void CalculatePayouts(Dictionary<int, Spot> spots)
    {
        Debug.Log("[SpotCalculator] === Step 3: Calculating Payouts ===");
        
        // 배수 효과 수집 (UpgradedMultiSpot, Chameleon 통합)
        Dictionary<int, double> multipliers = CollectMultipliers(spots);
        
        // 각 Spot의 배당률 계산
        foreach (var pair in spots)
        {
            int spotID = pair.Key;
            Spot spot = pair.Value;
            
            // 파괴된 Spot은 배당 0
            if (spot.isDestroyed)
            {
                spot.currentPayoutMultiplier = 0.0;
                Debug.Log($"  Spot {spotID}: Destroyed → x0");
                continue;
            }
            
            // 기본 배당 = x36
            double payout = spot.basePayoutMultiplier;
            
            // 배수 효과 적용
            if (multipliers.ContainsKey(spotID))
            {
                payout *= multipliers[spotID];
            }
            
            spot.currentPayoutMultiplier = payout;
            Debug.Log($"  Spot {spotID}: x{payout:F2}");
        }
    }
    
    
    // ===================================================
    // 아이템 적용 메서드
    // ===================================================
    
    /// <summary>
    /// PlusSpot 적용: 숫자 +1 증가
    /// </summary>
    public static bool ApplyPlusSpot(GameState gameState, ItemData item, int targetSpotID)
    {
        if (!gameState.spots.ContainsKey(targetSpotID))
        {
            Debug.LogError($"[SpotCalculator] Invalid spot ID: {targetSpotID}");
            return false;
        }
        
        Spot targetSpot = gameState.spots[targetSpotID];
        
        if (targetSpot.isDestroyed)
        {
            Debug.LogWarning($"[SpotCalculator] Spot {targetSpotID} is destroyed!");
            return false;
        }
        
        // 기록 생성 (생성자에서 자동으로 +1 계산)
        var record = new AppliedItemRecord(item, targetSpotID, targetSpot.currentNumber);
        
        if (targetSpot.currentNumber == record.appliedValue)
        {
            Debug.LogWarning($"[SpotCalculator] Spot {targetSpotID} is already 36!");
            return false;
        }
        
        targetSpot.AddRecord(record);
        
        // 숫자 변경 트리거 (Chameleon 등)
        TriggerOnNumberChanged(gameState, targetSpotID);
        
        Debug.Log($"[SpotCalculator] PlusSpot applied: Spot {targetSpotID} → {record.appliedValue}");
        return true;
    }
    
    /// <summary>
    /// CopySpot 적용: 원본 Spot의 모든 효과를 대상에 복사
    /// </summary>
    public static bool ApplyCopySpot(GameState gameState, ItemData item, int sourceSpotID, int destSpotID)
    {
        if (!gameState.spots.ContainsKey(sourceSpotID) || !gameState.spots.ContainsKey(destSpotID))
        {
            Debug.LogError($"[SpotCalculator] Invalid spot IDs: {sourceSpotID}, {destSpotID}");
            return false;
        }
        
        Spot sourceSpot = gameState.spots[sourceSpotID];
        Spot destSpot = gameState.spots[destSpotID];
        
        if (sourceSpot.isDestroyed || destSpot.isDestroyed)
        {
            Debug.LogWarning($"[SpotCalculator] Source or dest spot is destroyed!");
            return false;
        }
        
        // 원본의 모든 상태를 대상에 복사
        destSpot.currentNumber = sourceSpot.currentNumber;
        destSpot.currentColor = sourceSpot.currentColor;
        
        // 원본의 모든 기록도 복사
        int copiedCount = sourceSpot.appliedRecords.Count;
        foreach (var sourceRecord in sourceSpot.appliedRecords)
        {
            destSpot.AddRecord(sourceRecord);
        }
        
        Debug.Log($"[SpotCalculator] CopySpot applied: {sourceSpotID} → {destSpotID} (Number: {destSpot.currentNumber}, Color: {destSpot.currentColor}, {copiedCount} records)");
        return true;
    }
    
    /// <summary>
    /// UpgradedMultiSpot 적용: 중심 + 인접 4개 Spot에 x1.2 배당
    /// </summary>
    public static bool ApplyUpgradedMultiSpot(GameState gameState, ItemData item, int centerSpotID)
    {
        if (!gameState.spots.ContainsKey(centerSpotID))
        {
            Debug.LogError($"[SpotCalculator] Invalid spot ID: {centerSpotID}");
            return false;
        }
        
        Spot centerSpot = gameState.spots[centerSpotID];
        
        if (centerSpot.isDestroyed)
        {
            Debug.LogWarning($"[SpotCalculator] Center spot {centerSpotID} is destroyed!");
            return false;
        }
        
        // 중심 + 인접 스팟에 각각 개별 record 추가
        List<int> affectedIDs = new List<int> { centerSpotID };
        affectedIDs.AddRange(centerSpot.GetAdjacentSpots());
        
        // 파괴된 스팟 제외
        affectedIDs.RemoveAll(id => !gameState.spots.ContainsKey(id) || gameState.spots[id].isDestroyed);
        
        // 각 스팟에 개별 record 추가 (각자 확률 계산)
        foreach (int spotID in affectedIDs)
        {
            var record = new AppliedItemRecord(item, spotID);
            gameState.spots[spotID].AddRecord(record);
        }
        
        Debug.Log($"[SpotCalculator] UpgradedMultiSpot applied: {affectedIDs.Count} spots affected (Center {centerSpotID} + {affectedIDs.Count - 1} adjacent)");
        return true;
    }
    
    /// <summary>
    /// Wing 아이템 적용 (BetData에 추가)
    /// </summary>
    public static bool ApplyChipItem(ItemData item, BetData bet)
    {
        if (bet.isAppliedItem(item.chipItemType))
        {
            Debug.LogWarning($"[SpotCalculator] Wing already applied to this bet!");
            return false;
        }
        
        bet.appliedItems.Add(item);
        Debug.Log($"[SpotCalculator] Wing applied to bet: {bet.betType} {bet.targetValue}");
        return true;
    }
    
    /// <summary>
    /// 숫자/색상 변경 시 트리거 (Chameleon 등 확장 가능)
    /// </summary>
    private static void TriggerOnNumberChanged(GameState gameState, int changedSpotID)
    {
        if (!gameState.spots.ContainsKey(changedSpotID))
            return;
        
        Spot changedSpot = gameState.spots[changedSpotID];
        
        // 1. Chameleon 트리거
        ItemData chameleonCharm = gameState.GetItemByID("CHAMELEON_CHARM");
        if (chameleonCharm != null && chameleonCharm.count > 0)
        {
            TriggerChameleon(changedSpot, chameleonCharm, changedSpotID);
        }
        
        // 2. 추가 트리거 (미래 확장용)
        // if (gameState.HasXxxCharm())
        // {
        //     TriggerXxx(changedSpot, gameState.GetXxxCharm(), changedSpotID);
        // }
    }
    
    /// <summary>
    /// Chameleon 트리거 처리
    /// </summary>
    private static void TriggerChameleon(Spot spot, ItemData chameleonCharm, int spotID)
    {
        // 기존 Chameleon 기록 찾기
        var existingRecord = spot.appliedRecords.Find(r => 
            r.itemData.itemType == ItemType.CharmItem && 
            r.itemData.charmType == CharmType.Chameleon);
        
        if (existingRecord != null)
        {
            // multiplierValue 누적 (1.3 * 1.3 = 1.69)
            existingRecord.multiplierValue *= 1.3;
            Debug.Log($"[SpotCalculator] Chameleon boosted: Spot {spotID} (x{existingRecord.multiplierValue:F2})");
        }
        else
        {
            // 새 Chameleon 기록 생성
            var chameleonRecord = new AppliedItemRecord(chameleonCharm, spotID);
            spot.AddRecord(chameleonRecord);
            Debug.Log($"[SpotCalculator] Chameleon triggered: Spot {spotID} (x1.3)");
        }
    }
    
    /// <summary>
    /// Death Charm 처리: 4 포함 Spot 파괴
    /// </summary>
    public static void ProcessDeathCharm(GameState gameState, int winningSpotID)
    {
        ItemData deathCharm = gameState.GetItemByID("DEATH_CHARM");
        if (deathCharm == null || deathCharm.count <= 0)
            return;
        
        Spot winningSpot = gameState.spots[winningSpotID];
        
        if (!winningSpot.HasNumber4())
            return;
        
        // 파괴 처리
        winningSpot.isDestroyed = true;
        
        // Death 기록 추가 (targetSpotID에 파괴된 스팟 정보 포함)
        var deathRecord = new AppliedItemRecord(deathCharm, winningSpotID);
        
        winningSpot.AddRecord(deathRecord);
        
        Debug.Log($"[SpotCalculator] Death Charm: Spot {winningSpotID} destroyed! (Number: {winningSpot.currentNumber})");
    }
    
    
    // ===================================================
    // 헬퍼 메서드
    // ===================================================
    
    /// <summary>
    /// 배수 효과 수집 (모든 배수 아이템 통합)
    /// </summary>
    private static Dictionary<int, double> CollectMultipliers(Dictionary<int, Spot> spots)
    {
        Dictionary<int, double> multipliers = new Dictionary<int, double>();
        
        foreach (var pair in spots)
        {
            int spotID = pair.Key;
            Spot spot = pair.Value;
            
            // 각 Spot의 모든 record를 확인
            foreach (var record in spot.appliedRecords)
            {
                // multiplierValue가 1.0보다 크면 적용
                if (record.multiplierValue > 1.0 && record.targetSpotID == spotID)
                {
                    if (!multipliers.ContainsKey(spotID))
                        multipliers[spotID] = 1.0;
                    
                    multipliers[spotID] *= record.multiplierValue;
                }
            }
        }
        
        return multipliers;
    }
    
    
    // ===================================================
    // 결과 계산 메서드
    // ===================================================
    
    /// <summary>
    /// 확률 분포 기반으로 당첨 SpotID 추첨
    /// </summary>
    public static int DetermineWinner(GameState gameState)
    {
        Debug.Log("[SpotCalculator] === Determining Winner ===");
        
        // 1. 누적 확률 배열 생성
        List<int> spotIDs = new List<int>();
        List<double> cumulativeProbabilities = new List<double>();
        double cumulative = 0.0;
        
        foreach (var pair in gameState.spots)
        {
            int spotID = pair.Key;
            Spot spot = pair.Value;
            
            if (spot.isDestroyed || spot.currentProbability <= 0)
                continue;
            
            spotIDs.Add(spotID);
            cumulative += spot.currentProbability;
            cumulativeProbabilities.Add(cumulative);
        }
        
        // 2. 랜덤 값 생성 (0.0 ~ 1.0)
        double randomValue = UnityEngine.Random.value;
        
        // 3. 누적 확률에서 당첨 SpotID 찾기
        for (int i = 0; i < cumulativeProbabilities.Count; i++)
        {
            if (randomValue <= cumulativeProbabilities[i])
            {
                int winningSpotID = spotIDs[i];
                Spot winningSpot = gameState.spots[winningSpotID];
                
                Debug.Log($"[SpotCalculator] Winner: Spot {winningSpotID} (Number: {winningSpot.currentNumber}, Probability: {winningSpot.currentProbability * 100:F2}%)");
                return winningSpotID;
            }
        }
        
        // 예외: 마지막 Spot 반환
        int fallbackID = spotIDs[spotIDs.Count - 1];
        Debug.LogWarning($"[SpotCalculator] Fallback winner: Spot {fallbackID}");
        return fallbackID;
    }
    
    /// <summary>
    /// 배팅 결과 계산 (총 지급액)
    /// </summary>
    public static float CalculateTotalPayout(GameState gameState, int winningSpotID, List<BetData> bets)
    {
        Debug.Log("[SpotCalculator] === Calculating Total Payout ===");
        
        Spot winningSpot = gameState.spots[winningSpotID];
        float totalPayout = 0f;
        
        foreach (var bet in bets)
        {
            bool hasWing = bet.isAppliedItem(ChipItemType.HatWing);
            bool isWin = IsBetWin(bet, winningSpotID, winningSpot);
            
            // Wing 적용: 당첨되지 않아도 당첨 처리 (보상 50%)
            if (!isWin && !hasWing)
            {
                Debug.Log($"  Bet: {bet.betType} {bet.targetValue} → LOSE");
                continue;
            }
            
            // 베팅 금액
            float betAmount = bet.GetTotalChipValue();
            
            // 배당률 (BetType별)
            double payoutMultiplier = GetPayoutMultiplier(bet, winningSpot);
            
            // Wing 적용 시 50% 지급
            if (hasWing && !isWin)
            {
                payoutMultiplier *= 0.5;
                Debug.Log($"  Bet: {bet.betType} {bet.targetValue} → LOSE but WING | ${betAmount} x {payoutMultiplier:F2} = ${betAmount * (float)payoutMultiplier:F2}");
            }
            else if (hasWing && isWin)
            {
                Debug.Log($"  Bet: {bet.betType} {bet.targetValue} → WIN (with Wing) | ${betAmount} x {payoutMultiplier:F2} = ${betAmount * (float)payoutMultiplier:F2}");
            }
            else
            {
                Debug.Log($"  Bet: {bet.betType} {bet.targetValue} → WIN | ${betAmount} x {payoutMultiplier:F2} = ${betAmount * (float)payoutMultiplier:F2}");
            }
            
            // 지급액 계산
            float payout = betAmount * (float)payoutMultiplier;
            totalPayout += payout;
        }
        
        Debug.Log($"[SpotCalculator] Total Payout: ${totalPayout:F2}");
        return totalPayout;
    }
    
    /// <summary>
    /// 특정 베팅이 당첨됐는지 확인 (public - Game에서도 사용)
    /// </summary>
    public static bool IsBetWin(BetData bet, int winningSpotID, Spot winningSpot)
    {
        switch (bet.betType)
        {
            case BetType.Number:
                return winningSpot.SpotID == bet.targetValue;
                
            case BetType.Color:
                return winningSpot.currentColor == (SpotColor)bet.targetValue;
                
            case BetType.OddEven:
                bool isEven = winningSpot.currentNumber % 2 == 0;
                return (bet.targetValue == 1 && isEven) || (bet.targetValue == 0 && !isEven);
                
            case BetType.HighLow:
                bool isHigh = winningSpot.currentNumber >= 19;
                return (bet.targetValue == 1 && isHigh) || (bet.targetValue == 0 && !isHigh);
                
            case BetType.Dozen:
                int dozen = (winningSpot.currentNumber - 1) / 12 + 1; // 1~12=1, 13~24=2, 25~36=3
                return dozen == bet.targetValue;
                
            case BetType.Column:
                int column = (winningSpot.currentNumber - 1) % 3 + 1; // 1,4,7...=1, 2,5,8...=2, 3,6,9...=3
                return column == bet.targetValue;
                
            default:
                Debug.LogWarning($"[SpotCalculator] Unknown BetType: {bet.betType}");
                return false;
        }
    }
    
    /// <summary>
    /// 배팅 타입별 실제 배당률 반환 (배당 계산용)
    /// Number 타입은 Spot의 동적 배당률(currentPayoutMultiplier)을 사용합니다.
    /// </summary>
    private static double GetPayoutMultiplier(BetData bet, Spot winningSpot)
    {
        switch (bet.betType)
        {
            case BetType.Number:
                // 숫자 배팅: Spot의 현재 배당률 적용 (동적 배당률)
                return winningSpot.currentPayoutMultiplier;
                
            case BetType.Color:
            case BetType.OddEven:
            case BetType.HighLow:
                // 2배 배팅
                return 2.0;
                
            case BetType.Dozen:
            case BetType.Column:
                // 3배 배팅
                return 3.0;
                
            default:
                return 1.0;
        }
    }
    
    /// <summary>
    /// 배팅 타입별 기본 배당률 반환 (UI 표시용)
    /// Number 타입의 경우 특정 Spot이 필요하면 GameState를 넘겨서 조회 가능
    /// </summary>
    public static double GetBasePayout(BetType betType, GameState gameState = null, int spotID = -1)
    {
        switch (betType)
        {
            case BetType.Number:
                // 숫자 배팅: GameState가 있고 spotID가 유효하면 Spot의 배당률 반환
                if (gameState != null && spotID >= 0 && gameState.spots.ContainsKey(spotID))
                {
                    return gameState.spots[spotID].currentPayoutMultiplier;
                }
                return 36.0; // 기본값
                
            case BetType.Color:
            case BetType.OddEven:
            case BetType.HighLow:
                return 2.0;
                
            case BetType.Dozen:
            case BetType.Column:
                return 3.0;
                
            default:
                return 1.0;
        }
    }
    
    
    // ===================================================
    // 디버그 & 유틸리티
    // ===================================================
    
    /// <summary>
    /// 전체 확률 분포 출력
    /// </summary>
    public static void PrintProbabilityDistribution(Dictionary<int, Spot> spots)
    {
        Debug.Log("========== Probability Distribution ==========");
        
        List<int> sortedIDs = new List<int>(spots.Keys);
        sortedIDs.Sort();
        
        foreach (int spotID in sortedIDs)
        {
            Spot spot = spots[spotID];
            
            if (spot.isDestroyed)
            {
                Debug.Log($"Spot {spotID:D2}: DESTROYED");
            }
            else
            {
                Debug.Log($"Spot {spotID:D2}: {spot.currentProbability * 100:F4}% (Number: {spot.currentNumber})");
            }
        }
        
        Debug.Log("==============================================");
    }
    
    /// <summary>
    /// 전체 배당률 분포 출력
    /// </summary>
    public static void PrintPayoutDistribution(Dictionary<int, Spot> spots)
    {
        Debug.Log("========== Payout Distribution ==========");
        
        List<int> sortedIDs = new List<int>(spots.Keys);
        sortedIDs.Sort();
        
        foreach (int spotID in sortedIDs)
        {
            Spot spot = spots[spotID];
            
            if (spot.isDestroyed)
            {
                Debug.Log($"Spot {spotID:D2}: DESTROYED (x0)");
            }
            else
            {
                Debug.Log($"Spot {spotID:D2}: x{spot.currentPayoutMultiplier:F2} (Number: {spot.currentNumber})");
            }
        }
        
        Debug.Log("=========================================");
    }
}

