using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GB;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 모든 Spot의 정보를 텍스트로 표시하는 팝업
/// </summary>
public class SpotInfoPopup : UIScreen
{
    // === UI 요소 필드 ===
    [Header("UI Elements")]
    [SerializeField] private Button closeButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TMP_Text contentText;
    
    // === 데이터 필드 ===
    private GameState gameState;

    private void Awake()
    {
        Initialize();
    }
    
    public override void Initialize()
    {
        base.Initialize();

        Regist();

        // 닫기 버튼
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }
    
    public override void SetData(object data)
    {
        base.SetData(data);
        
        if (data is GameState state)
        {
            gameState = state;
            RefreshContent();
        }
    }
    
    private void RefreshContent()
    {
        if (gameState == null || contentText == null)
            return;
        
        StringBuilder sb = new StringBuilder();
        
        // 헤더
        int activeSpots = gameState.spots.Values.Count(s => !s.isDestroyed);
        int destroyedSpots = gameState.spots.Values.Count(s => s.isDestroyed);
        
        sb.AppendLine($"=== Spot Information ===");
        sb.AppendLine($"Active: {activeSpots} / Destroyed: {destroyedSpots}");
        sb.AppendLine();
        
        // Number별 확률 정보
        if (SpotCalculator.numberProbabilities != null && SpotCalculator.numberProbabilities.Count > 0)
        {
            sb.AppendLine($"=== Number Probabilities ===");
            var sortedNumberProbs = SpotCalculator.numberProbabilities.OrderBy(kvp => kvp.Key);
            double totalProb = 0.0;
            
            foreach (var kvp in sortedNumberProbs)
            {
                sb.AppendLine($"NUM:{kvp.Key:D2} → {kvp.Value * 100:F2}%");
                totalProb += kvp.Value;
            }
            
            sb.AppendLine($"───────────────────────────");
            sb.AppendLine($"Total: {totalProb * 100:F2}% (Expected: 100.00%)");
            sb.AppendLine();
        }
        
        // 모든 Spot 정보 (ID 순서대로)
        sb.AppendLine($"=== Spot Details ===");
        var sortedSpots = gameState.spots.OrderBy(s => s.Key);
        
        foreach (var pair in sortedSpots)
        {
            Spot spot = pair.Value;
            
            // 기본 정보
            string spotInfo = $"#{spot.SpotID:D2} | ";
            
            // 숫자
            if (spot.isDestroyed)
                spotInfo += "NUM:X | ";
            else if (spot.currentNumber != spot.SpotID)
                spotInfo += $"NUM:{spot.currentNumber}* | ";
            else
                spotInfo += $"NUM:{spot.currentNumber} | ";
            
            // 확률
            if (spot.isDestroyed)
            {
                spotInfo += "PROB:0% | ";
            }
            else if (SpotCalculator.numberProbabilities.ContainsKey(spot.currentNumber))
            {
                double prob = SpotCalculator.numberProbabilities[spot.currentNumber];
                spotInfo += $"PROB:{prob * 100:F2}% | ";
            }
            else
            {
                spotInfo += "PROB:0% | ";
            }
            
            // 배당률
            if (spot.isDestroyed)
                spotInfo += "PAY:x0 | ";
            else if (spot.currentPayoutMultiplier != spot.basePayoutMultiplier)
                spotInfo += $"PAY:x{spot.currentPayoutMultiplier:F1}* | ";
            else
                spotInfo += $"PAY:x{spot.currentPayoutMultiplier:F1} | ";
            
            // 적용된 아이템
            if (spot.appliedRecords.Count == 0)
            {
                spotInfo += "ITEMS:-";
            }
            else
            {
                var itemNames = spot.appliedRecords
                    .Select(r => GetItemShortName(r));
                spotInfo += $"ITEMS:{string.Join(",", itemNames)}";
            }
            
            sb.AppendLine(spotInfo);
        }
        
        contentText.text = sb.ToString();
        
        // ContentText의 크기를 텍스트 길이에 맞게 조정
        if (contentText != null)
        {
            // TextMeshPro의 ForceMeshUpdate로 텍스트 크기 재계산
            contentText.ForceMeshUpdate();
            
            // RectTransform 크기를 preferredHeight로 설정
            RectTransform textRect = contentText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                float preferredHeight = contentText.preferredHeight;
                textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, preferredHeight);
            }
        }
        
        // 레이아웃 강제 업데이트 후 스크롤을 맨 위로 이동
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
    
    private string GetItemShortName(AppliedItemRecord record)
    {
        switch (record.itemType)
        {
            case ItemType.SpotItem:
                switch (record.spotItemType)
                {
                    case SpotItemType.PlusSpot: return "PlusSpot";
                    case SpotItemType.CopySpot: return "CopySpot";
                    case SpotItemType.UpgradedMultiSpot: return "UpgradedMultiSpot";
                    default: return "SpotItem";
                }
            
            case ItemType.CharmItem:
                switch (record.charmType)
                {
                    case CharmType.Death: return "Death";
                    case CharmType.Chameleon: return "Chameleon";
                    default: return "CharmItem";
                }
            
            case ItemType.ChipItem:
                return "ChipItem";
            
            default:
                return "?";
        }
    }
    
    private void OnCloseClicked()
    {
        Game.isPopupOpen = false;
        Close();
    }
    
    public override void Refresh()
    {
        base.Refresh();
        RefreshContent();
    }
}

