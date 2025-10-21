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
    private Color redColor = new Color(1f, 0f, 0f);
    private Color blackColor = new Color(0f, 0f, 0f);
    private Color destroyedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    public int SpotID => objectID;

    public void SetSpotData(Spot data)
    {
        Debug.Log($"[SpotObject] SetSpotData: Spot {objectID}, Number={data?.currentNumber}, Multiplier={data?.currentPayoutMultiplier}");
        spotData = data;
        UpdateVisual();
    }

    /// <summary>
    /// 비주얼 업데이트 (BetObject override)
    /// </summary>
    public override void UpdateVisual()
    {
        Debug.Log($"[SpotObject] UpdateVisual: Spot {objectID}");

        if (objectRenderer == null || spotData == null)
        {
            Debug.LogWarning($"[SpotObject] UpdateVisual failed: objectRenderer={objectRenderer != null}, spotData={spotData != null}");
            return;
        }

        // 숫자 표시
        if (displayText != null)
        {
            if (spotData.isDestroyed)
                displayText.text = "X";
            else
                displayText.text = spotData.currentNumber.ToString();
        }

        if (spotData.isDestroyed)
            objectRenderer.material.color = destroyedColor;
        else if (spotData.currentColor == SpotColor.Red)
            objectRenderer.material.color = redColor;
        else
            objectRenderer.material.color = blackColor;
    }

    /// <summary>
    /// 타겟 값 반환 (BetObject override)
    /// </summary>
    protected override int GetTargetValue()
    {
        return spotData.currentNumber;
    }

    /// <summary>
    /// SpotObject 특별한 클릭 처리 (아이템 사용 + 배팅)
    /// </summary>
    protected override void HandleClick()
    {        
        // 클릭 락이 걸려있으면 차단 (배팅/스핀/게임종료)
        if (Game.isClickLocked)
        {
            return;
        }
        
        if (spotData == null || spotData.isDestroyed)
        {
            return;
        }

        // Game으로 메시지 전달 (Game이 직접 처리)
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_SPOT_CLICKED, objectID);
    }

    protected override void OnMouseExit()
    {
        if (!Game.isCopySpotMode)
        {
            SetHighlight(false);
        }

        // 툴팁 상태 리셋
        isTooltipShowing = false;

        UIManager.ClosePopup("SpotItemUI");
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
        // 팝업이 열려있으면 툴팁 표시 안 함
        if (Game.isPopupOpen)
        {
            return;
        }
        
        // CopySpot 모드에서는 호버링/하이라이트 차단
        if (Game.isCopySpotMode)
        {
            return;
        }
        
        // 마우스가 오브젝트 위에 있을 때 툴팁 표시
        if (!isTooltipShowing)
        {
            ShowTooltip();
            if (spotData != null && !spotData.isDestroyed)
            {
                SetHighlight(true);
            }
        }
    }

    private bool isTooltipShowing = false;

    private void ShowTooltip()
    {
        // 마우스가 여전히 오브젝트 위에 있고 툴팁이 표시되지 않았다면
        if (!isTooltipShowing)
        {
            isTooltipShowing = true;

            // SpotObject의 스크린 좌표 계산
            Vector3 worldPos = transform.position;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // 박싱/언박싱 없이 직접 데이터 전달
            var tooltipData = new TooltipMessage(spotData, screenPos);
            UIManager.ShowPopup("SpotItemUI", tooltipData);            
        }
    }
}

