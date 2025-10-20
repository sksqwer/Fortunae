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
    protected Color normalColor = new Color(1f, 1f, 1f);
    protected Color highlightColor = new Color(1f, 1f, 0f);
    protected Color betColor = new Color(0.3f, 0.6f, 0.9f);
    
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


        // 자신을 제외한 자식 오브젝트에서만 찾기
        if(highlightRenderer == null)
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
        Debug.Log($"[BetObject] UpdateVisual: {GetType().Name} {objectID}");
        
        if (objectRenderer != null)
        {
            objectRenderer.material.color = normalColor;
        }
    }
    
    /// <summary>
    /// 강조 표시
    /// </summary>
    public virtual void SetHighlight(bool highlight)
    {
        Debug.Log($"[BetObject] SetHighlight: {GetType().Name} {objectID}, highlight={highlight}");
        
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
        Debug.Log($"[BetObject] OnPointerClick: {GetType().Name} {objectID}");
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
        // 팝업이 켜져 있으면 클릭 차단
        if (Game.isPopupOpen)
        {
            Debug.Log($"[BetObject] Popup is open, blocking click for {GetType().Name} {objectID}");
            return;
        }
        
        // 스핀 중이면 클릭 차단
        var game = FindFirstObjectByType<Game>();
        if (game != null && game.IsSpinning)
        {
            Debug.LogWarning($"[BetObject] Spinning in progress, blocking click for {GetType().Name} {objectID}");
            return;
        }
        
        Debug.Log($"[BetObject] {GetType().Name} {objectID} clicked! Type: {GetBetType()}");
        
        // Game으로 클릭 이벤트 전달 (Game이 직접 처리)
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_BET_OBJECT_CLICKED, 
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
        Debug.Log($"[BetObject] OnMouseEnter: {GetType().Name} {objectID}");
        SetHighlight(true);
    }
    
    protected virtual void OnMouseExit()
    {
        Debug.Log($"[BetObject] OnMouseExit: {GetType().Name} {objectID}");
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
