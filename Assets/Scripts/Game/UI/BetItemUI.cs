using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GB;

/// <summary>
/// 개별 배팅 UI 아이템
/// </summary>
public class BetItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text betInfoText;
    [SerializeField] private TMP_Text betAmountText;
    [SerializeField] private Button removeButton;
    [SerializeField] private Image backgroundImage;
    
    private BetData betData;
    
    public void Initialize(BetData bet)
    {
        betData = bet;
        
        // 버튼 이벤트
        if (removeButton != null)
            removeButton.onClick.AddListener(OnRemoveClicked);
        
        Refresh();
    }
    
    public void Refresh()
    {
        if (betData == null)
            return;
        
        // 배팅 정보
        if (betInfoText != null)
        {
            string betTypeStr = GetBetTypeString(betData.betType);
            string targetStr = GetTargetString(betData);
            betInfoText.text = $"{betTypeStr}: {targetStr}";
        }
        
        // 배팅 금액
        if (betAmountText != null)
        {
            int totalValue = betData.GetTotalChipValue();
            int totalCount = betData.GetTotalChipCount();
            betAmountText.text = $"${totalValue} ({totalCount} chips)";
        }
    }
    
    private string GetBetTypeString(BetType type)
    {
        switch (type)
        {
            case BetType.Number: return "Number";
            case BetType.Color: return "Color";
            case BetType.OddEven: return "Odd/Even";
            case BetType.HighLow: return "High/Low";
            case BetType.Dozen: return "Dozen";
            case BetType.Column: return "Column";
            default: return "Unknown";
        }
    }
    
    private string GetTargetString(BetData bet)
    {
        switch (bet.betType)
        {
            case BetType.Number:
                return $"Spot {bet.targetValue}";
            case BetType.Color:
                return bet.targetValue == 0 ? "Red" : "Black";
            case BetType.OddEven:
                return bet.targetValue == 0 ? "Odd" : "Even";
            case BetType.HighLow:
                return bet.targetValue == 0 ? "Low (1-18)" : "High (19-36)";
            case BetType.Dozen:
                return $"Dozen {bet.targetValue}";
            case BetType.Column:
                return $"Column {bet.targetValue}";
            default:
                return bet.targetValue.ToString();
        }
    }
    
    private void OnRemoveClicked()
    {
        if (betData != null)
        {
            // Presenter로 배팅 제거 이벤트 전달
            GB.Presenter.Send(GamePresenter.DOMAIN, GamePresenter.Keys.CMD_REMOVE_BET, betData);
        }
    }
}

