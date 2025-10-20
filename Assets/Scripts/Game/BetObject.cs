using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using GB;

/// <summary>
/// 배팅 가능한 오브젝트의 부모 클래스 (SpotObject, ColorObject, OddEvenObject 등)
/// Inspector에서 직접 사용 가능하도록 abstract 제거
/// </summary>
public class BetObject : MonoBehaviour, IPointerClickHandler
{
    [Header("Basic Info")]
    [SerializeField] protected int objectID;
    [SerializeField] protected Renderer objectRenderer;
    [SerializeField] protected SpriteRenderer highlightRenderer;
    [SerializeField] protected TextMeshPro displayText;
    
    [Header("Bet Configuration")]
    [SerializeField] protected BetType betType = BetType.Number;
    [SerializeField] protected int targetValue = 0;
     // Color(0=Red,1=Black), OddEven(0=Odd,1=Even), HighLow(0=Low,1=High), Dozen(1-3), Column(1-3)

    [Header("Colors")]
    protected Color normalColor = new Color(0.2f, 0.2f, 0.2f);
    protected Color highlightColor = new Color(1f, 1f, 0f);
    protected Color betColor = new Color(0.3f, 0.6f, 0.9f);
    
    protected bool isHighlighted = false;
    protected float lastClickTime = 0f;
    
    public int ObjectID => objectID;
    
    /// <summary>
    /// 초기화 (하위 클래스에서 override)
    /// </summary>
    public virtual void Initialize(int id)
    {
        objectID = id;
        objectRenderer = GetComponent<Renderer>();
        // 자신의 SpriteRenderer는 제외하고 자식에서만 찾기
        SpriteRenderer[] allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        highlightRenderer = null;
        
        // 자신을 제외한 자식 오브젝트에서만 찾기
        foreach (SpriteRenderer sr in allSpriteRenderers)
        {
            if (sr.gameObject != gameObject) // 자신이 아닌 경우만
            {
                highlightRenderer = sr;
                highlightRenderer.gameObject.SetActive(false);
                break;
            }
        }
        
        displayText = GetComponentInChildren<TextMeshPro>();
        Debug.Log($"[BetObject] Basic initialization for ID: {id}");
    }
    
    /// <summary>
    /// 비주얼 업데이트 (하위 클래스에서 override)
    /// </summary>
    public virtual void UpdateVisual()
    {
        if (objectRenderer != null && !isHighlighted)
        {
            objectRenderer.material.color = normalColor;
        }
        
        if (displayText != null)
        {
            displayText.text = $"ID: {objectID}";
        }
    }
    
    /// <summary>
    /// 강조 표시
    /// </summary>
    public virtual void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        
        if (highlightRenderer != null)
        {
            if (highlight)
            {
                highlightRenderer.gameObject.SetActive(true);
            }
            else
            {
                highlightRenderer.gameObject.SetActive(false);
                UpdateVisual();
            }
        }
    }
    
    /// <summary>
    /// 클릭 이벤트 (3D Object 클릭 - IPointerClickHandler)
    /// </summary>
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        HandleClick();
    }
    
    /// <summary>
    /// 마우스 클릭 (3D Collider용 - OnMouseDown)
    /// </summary>
    protected virtual void OnMouseDown()
    {
        Debug.Log($"[BetObject] OnMouseDown called for {GetType().Name} {objectID}");
        HandleClick();
    }
    
    /// <summary>
    /// 공통 클릭 처리
    /// </summary>
    protected virtual void HandleClick()
    {
        // 중복 클릭 방지
        if (Time.time - lastClickTime < 0.5f)
            return;
            
        lastClickTime = Time.time;
        
        double payout = Game.GetPayoutMultiplier(GetBetType());
        Debug.Log($"[BetObject] {GetType().Name} {objectID} clicked! (Payout: x{payout})");
        
        // Presenter로 클릭 이벤트 전달
        GB.Presenter.Send(GamePresenter.DOMAIN, GamePresenter.Keys.CMD_BET_OBJECT_CLICKED, 
                         new BetObjectClickData(objectID, GetBetType(), GetTargetValue()));
    }
    
    /// <summary>
    /// 배팅 타입 반환 (하위 클래스에서 override 가능)
    /// </summary>
    protected virtual BetType GetBetType()
    {
        return betType; // Inspector에서 설정된 값 반환
    }
    
    /// <summary>
    /// 타겟 값 반환 (하위 클래스에서 override 가능)
    /// </summary>
    protected virtual int GetTargetValue()
    {
        // BetType.Number인 경우 objectID 사용, 그 외엔 targetValue 사용
        return betType == BetType.Number ? objectID : targetValue;
    }
    
    
    /// <summary>
    /// 마우스 호버
    /// </summary>
    protected virtual void OnMouseEnter()
    {
        SetHighlight(true);
    }
    
    protected virtual void OnMouseExit()
    {
        SetHighlight(false);
    }
}

/// <summary>
/// BetObject 클릭 데이터
/// </summary>
[System.Serializable]
public class BetObjectClickData
{
    public int objectID;
    public BetType betType;
    public int targetValue;
    
    public BetObjectClickData(int id, BetType type, int value)
    {
        objectID = id;
        betType = type;
        targetValue = value;
    }
}
