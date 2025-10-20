using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GB;

/// <summary>
/// Spot 정보 툴팁 팝업 (UIScreen 기반)
/// </summary>
public class SpotItemUI : UIScreen
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text spotIDText;
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text probabilityText;
    [SerializeField] private TMP_Text payoutText;
    [SerializeField] private TMP_Text appliedItemsText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button closeButton;
    
    [Header("Colors")]
    [SerializeField] private Color redColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color blackColor = new Color(0f, 0f, 0f);
    [SerializeField] private Color destroyedColor = new Color(0.5f, 0.5f, 0.5f);
    
    private Spot spot;
    
    public override void Initialize()
    {
        base.Initialize();
        SetScreenType(ScreenType.POPUP);
        
        // 닫기 버튼 이벤트
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
        
        // UIManager에 등록
        UIManager.I.RegistUIScreen(this);
    }
    
    /// <summary>
    /// UIManager.ShowPopup으로 호출 시 데이터 수신
    /// </summary>
    public override void SetData(object data)
    {
        base.SetData(data);
        
        if (data is Spot spotData)
        {
            ShowSpotInfo(spotData);
        }
        else if (data is TooltipData tooltipData)
        {
            // TooltipData 구조체로 받은 경우 (박싱/언박싱 없음)
            ShowSpotInfo(tooltipData.spotData, tooltipData.screenPos);
        }
        else if (data is object[] parameters && parameters.Length >= 2)
        {
            // 기존 방식 (하위 호환성)
            Spot spotDataParam = parameters[0] as Spot;
            Vector2 screenPos = (Vector2)parameters[1];
            
            ShowSpotInfo(spotDataParam, screenPos);
        }
    }
    
    /// <summary>
    /// Spot 정보 표시
    /// </summary>
    public void ShowSpotInfo(Spot spotData, Vector2? screenPos = null)
    {
        this.spot = spotData;
        Refresh();
        
        // 툴팁 크기 강제 설정
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(200, 150);
        }
        
        // 위치 설정
        if (screenPos.HasValue)
        {
            SetTooltipPosition(screenPos.Value);
        }
        
        gameObject.SetActive(true);
        Debug.Log($"[SpotItemUI] Tooltip activated for Spot {spotData.SpotID}");
    }
    
    /// <summary>
    /// 툴팁 위치 설정
    /// </summary>
    private void SetTooltipPosition(Vector2 screenPos)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 툴팁 크기 강제 설정 (Content Size Fitter가 있으면 자동 조절됨)
            rectTransform.sizeDelta = new Vector2(200, 150); // 기본 크기 설정
            
            // 스크린 좌표를 Canvas 좌표로 변환
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Vector2 localPoint;
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // Screen Space - Overlay의 경우
                    Vector2 screenPoint = screenPos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.transform as RectTransform,
                        screenPoint,
                        null,
                        out localPoint);
                }
                else
                {
                    // Screen Space - Camera 또는 World Space의 경우
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.transform as RectTransform,
                        screenPos,
                        canvas.worldCamera,
                        out localPoint);
                }
                
                // 위치 설정 (마우스 커서 옆에 표시)
                Vector2 offset = new Vector2(30, -30);
                Vector2 finalPosition = localPoint + offset;
                
                // 화면 경계 체크 (Canvas 크기 내에서만 표시)
                RectTransform canvasRect = canvas.transform as RectTransform;
                Vector2 canvasSize = canvasRect.sizeDelta;
                Vector2 tooltipSize = rectTransform.sizeDelta;
                
                // 오른쪽 경계 체크
                if (finalPosition.x + tooltipSize.x > canvasSize.x * 0.5f)
                {
                    finalPosition.x = localPoint.x - tooltipSize.x - 20; // 왼쪽으로 이동
                }
                
                // 위쪽 경계 체크
                if (finalPosition.y > canvasSize.y * 0.5f)
                {
                    finalPosition.y = localPoint.y - tooltipSize.y - 20; // 아래쪽으로 이동
                }
                
                rectTransform.anchoredPosition = finalPosition;
            }
        }
    }
    
    public override void Refresh()
    {
        if (spot == null)
            return;
        
        // Spot ID
        if (spotIDText != null)
            spotIDText.text = $"#{spot.SpotID}";
        
        // 현재 숫자
        if (numberText != null)
        {
            if (spot.isDestroyed)
                numberText.text = "X";
            else
                numberText.text = spot.currentNumber.ToString();
        }
        
        // 확률
        if (probabilityText != null)
        {
            if (spot.isDestroyed)
                probabilityText.text = "0%";
            else
                probabilityText.text = $"{spot.currentProbability * 100:F2}%";
        }
        
        // 배당률
        if (payoutText != null)
        {
            if (spot.isDestroyed)
                payoutText.text = "x0";
            else
                payoutText.text = $"x{spot.currentPayoutMultiplier:F1}";
        }
        
        // 적용된 아이템 목록
        if (appliedItemsText != null)
        {
            if (spot.appliedRecords.Count > 0)
            {
                string itemsInfo = "Applied Items:\n";
                foreach (var record in spot.appliedRecords)
                {
                    itemsInfo += $"• {record.itemData.itemID} (x{record.multiplierValue:F2})\n";
                }
                appliedItemsText.text = itemsInfo;
            }
            else
            {
                appliedItemsText.text = "No items applied";
            }
        }
        
        // 배경 색깔
        if (backgroundImage != null)
        {
            if (spot.isDestroyed)
                backgroundImage.color = destroyedColor;
            else if (spot.currentColor == SpotColor.Red)
                backgroundImage.color = redColor;
            else
                backgroundImage.color = blackColor;
        }
    }
    
    private void OnCloseClicked()
    {
        Close();
    }
}

