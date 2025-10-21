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
            string betTypeStr = BetTypeHelper.GetBetTypeName(betData.betType);
            string targetStr = BetTypeHelper.GetBetDisplayName(betData.betType, betData.targetValue);
            string betInfo = $"{betTypeStr}: {targetStr}";
            
            // HatWing 적용 여부 표시
            if (betData.isHatWingApplied)
            {
                betInfo += " <color=#FFD700>[HW]</color>";
            }
            
            betInfoText.text = betInfo;
        }
        
        // 배팅 금액
        if (betAmountText != null)
        {
            int totalValue = betData.GetTotalChipValue();
            int totalCount = betData.GetTotalChipCount();
            betAmountText.text = $"${totalValue} ({totalCount} chips)";
        }
    }
    private void OnRemoveClicked()
    {
        if (betData != null)
        {
            // Presenter로 배팅 제거 이벤트 전달
            GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_REMOVE_BET, betData);
        }
    }
}

