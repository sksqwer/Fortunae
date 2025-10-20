using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using GB;

/// <summary>
/// 3D Spot GameObject (Board 위의 물리적인 Spot 오브젝트)
/// 배팅과 아이템 사용이 모두 가능한 특별한 BetObject
/// </summary>
public class SpotObject : BetObject
{
    private Spot spotData; // 데이터 참조
    
    // 색상
    private Color redColor = new Color(1f, 0.2f, 0.2f);
    private Color blackColor = new Color(0.2f, 0.2f, 0.2f);
    private Color destroyedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    public int SpotID => objectID;
        
    public void SetSpotData(Spot data)
    {
        spotData = data;
        UpdateVisual();
    }
    
    /// <summary>
    /// 비주얼 업데이트 (BetObject override)
    /// </summary>
    public override void UpdateVisual()
    {
        if (objectRenderer == null || spotData == null)
            return;
        
        // 숫자 표시
        if (displayText != null)
        {
            if (spotData.isDestroyed)
                displayText.text = "X";
            else
                displayText.text = spotData.currentNumber.ToString();
        }
        
        // 색상 업데이트
        if (!isHighlighted)
        {
            if (spotData.isDestroyed)
                objectRenderer.material.color = destroyedColor;
            else if (spotData.currentColor == SpotColor.Red)
                objectRenderer.material.color = redColor;
            else
                objectRenderer.material.color = blackColor;
        }
    }
    
    /// <summary>
    /// 배팅 타입 반환 (BetObject override)
    /// </summary>
    protected override BetType GetBetType()
    {
        return BetType.Number;
    }
    
    /// <summary>
    /// 타겟 값 반환 (BetObject override)
    /// </summary>
    protected override int GetTargetValue()
    {
        return spotData != null ? spotData.currentNumber : objectID;
    }
    
    /// <summary>
    /// SpotObject 특별한 클릭 처리 (아이템 사용 + 배팅)
    /// </summary>
    protected override void HandleClick()
    {
        Debug.Log($"[SpotObject] HandleClick called for Spot {objectID}");
        
        if (spotData == null || spotData.isDestroyed)
        {
            Debug.LogWarning($"[SpotObject] Spot {objectID} is destroyed or not initialized!");
            return;
        }
        
        Debug.Log($"[SpotObject] Spot {objectID} clicked successfully! Sending message to GameUI...");
        
        // GameUI로 직접 메시지 전달 (DOMAIN_UI 사용)
        GB.Presenter.Send(GameUI.DOMAIN_UI, GamePresenter.Keys.CMD_SPOT_CLICKED, objectID);
        Debug.Log($"[SpotObject] Message sent to GameUI for Spot {objectID}");
    }
    
    /// <summary>
    /// 마우스 호버 (툴팁 표시)
    /// </summary>
    protected override void OnMouseEnter()
    {
        if (!spotData.isDestroyed)
        {
            SetHighlight(true);
        }
    }

    protected override void OnMouseExit()
    {
        SetHighlight(false);

        // 툴팁 상태 리셋
        isTooltipShowing = false;

        // 툴팁 자동 닫기 (지연을 늘려서 더 부드럽게)
        CloseTooltipAfterDelay();
    }
    
    private void CloseTooltipAfterDelay()
    {        
        if (!isTooltipShowing)
        {
            UIManager.ClosePopup("SpotItemUI");
        }
    }
    
    private bool IsMouseOverObject()
    {
        // 2D Collider와 3D Collider 모두 확인
        Collider2D collider2D = GetComponent<Collider2D>();
        
        if (collider2D != null)
        {
            // 2D Collider 사용
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return collider2D.bounds.Contains(mouseWorldPos);
        }
        else
        {
            Debug.LogWarning($"[SpotObject] No Collider or Collider2D found on object {objectID}. Mouse over detection disabled.");
            return false;
        }
    }
    
    /// <summary>
    /// 마우스 호버 시 Spot 정보 툴팁 표시
    /// </summary>
    private void OnMouseOver()
    {
        // 마우스가 오브젝트 위에 있을 때 툴팁 표시 (짧은 지연)
        if (!isTooltipShowing)
        {
            ShowTooltipAfterDelay(); // 0.3초 → 0.2초로 단축
        }
    }
    
    private void ShowTooltipAfterDelay()
    {        
        // 마우스가 여전히 오브젝트 위에 있고 툴팁이 표시되지 않았다면
        if (!isTooltipShowing)
        {
            ShowSpotTooltip();
            isTooltipShowing = true;
        }
    }
    
    private bool isTooltipShowing = false;
    
    private void ShowSpotTooltip()
    {
        if (spotData != null)
        {
            // SpotObject의 스크린 좌표 계산
            Vector3 worldPos = transform.position;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            
            // 박싱/언박싱 없이 직접 데이터 전달
            var tooltipData = new TooltipData(spotData, screenPos);
            UIManager.ShowPopup("SpotItemUI", tooltipData);
            Debug.Log($"[SpotObject] Tooltip shown for Spot {objectID}");
        }
    }
    
}

