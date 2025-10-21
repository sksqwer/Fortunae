using UnityEngine;
using System.Collections.Generic;
using GB;
using DG.Tweening;

/// <summary>
/// 룰렛 보드 (3D Spot 및 BetObject 오브젝트 관리)
/// </summary>
public class Board : MonoBehaviour
{
    // === 3D 오브젝트 필드 ===
    [SerializeField] private SpotObject[] spotObjects = new SpotObject[36];
    [SerializeField] private BetObject[] betObjects; // Color, OddEven, Dozen, Column 등

    // === Win/Lose 효과 필드 ===
    [Header("Win/Lose Effects")]
    [SerializeField] private GameObject winObject; // Win 오브젝트
    [SerializeField] private GameObject loseObject; // Lose 오브젝트

    // === 데이터 필드 ===
    private Dictionary<int, Spot> spotDataDictionary;

    /// <summary>
    /// 초기화
    /// </summary>
    public void Init()
    {
        // SpotObject 자동 찾기
        if (spotObjects == null || spotObjects.Length == 0 || spotObjects[0] == null)
        {
            spotObjects = GetComponentsInChildren<SpotObject>();
        }

        // BetObject 자동 찾기 (SpotObject 제외)
        if (betObjects == null || betObjects.Length == 0)
        {
            var allBetObjects = GetComponentsInChildren<BetObject>();
            List<BetObject> nonSpotBetObjects = new List<BetObject>();

            foreach (var betObj in allBetObjects)
            {
                // SpotObject가 아닌 BetObject만 추가
                if (!(betObj is SpotObject))
                {
                    nonSpotBetObjects.Add(betObj);
                }
            }

            betObjects = nonSpotBetObjects.ToArray();
        }
    }

    /// <summary>
    /// 데이터 연결
    /// </summary>
    public void ConnectData(Dictionary<int, Spot> spots)
    {
        spotDataDictionary = spots;

        // SpotObject들을 데이터와 연결
        for (int i = 0; i < spotObjects.Length; i++)
        {
            if (spotObjects[i] != null)
            {
                int spotID = i + 1; // 1~36
                if (spots.ContainsKey(spotID))
                {
                    spotObjects[i].Initialize(spotID);
                    spotObjects[i].SetSpotData(spots[spotID]);
                }
                else
                {
                    Debug.LogError($"[Board] Spot data not found for ID {spotID}");
                }
            }
        }

        UpdateAllVisuals();

        // BetObject 초기화
        InitializeBetObjects();

        // Win/Lose 오브젝트 초기 상태 설정
        InitializeWinLoseObjects();
    }

    /// <summary>
    /// BetObject 초기화
    /// </summary>
    private void InitializeBetObjects()
    {
        if (betObjects == null || betObjects.Length == 0)
        {
            return;
        }


        foreach (var betObj in betObjects)
        {
            if (betObj != null)
            {
                betObj.Initialize(betObj.ObjectID);
            }
        }
    }

    /// <summary>
    /// 모든 Spot 비주얼 업데이트
    /// </summary>
    public void UpdateAllVisuals()
    {
        foreach (var spotObj in spotObjects)
        {
            if (spotObj != null)
            {
                spotObj.UpdateVisual();
            }
        }
    }

    /// <summary>
    /// 특정 Spot 강조
    /// </summary>
    public void HighlightSpot(int spotID, bool highlight)
    {
        var spotObj = System.Array.Find(spotObjects, s => s != null && s.SpotID == spotID);
        if (spotObj != null)
        {
            spotObj.SetHighlight(highlight);
        }
    }

    /// <summary>
    /// 모든 BetObject 목록 가져오기
    /// </summary>
    public BetObject[] GetAllBetObjects()
    {
        return betObjects;
    }

    /// <summary>
    /// 모든 SpotObject 목록 가져오기
    /// </summary>
    public SpotObject[] GetAllSpotObjects()
    {
        return spotObjects;
    }

    /// <summary>
    /// Win/Lose 오브젝트 초기화
    /// </summary>
    private void InitializeWinLoseObjects()
    {
        // Win 오브젝트 초기 상태 (비활성화)
        if (winObject != null)
        {
            winObject.SetActive(false);
        }

        // Lose 오브젝트 초기 상태 (비활성화)
        if (loseObject != null)
        {
            loseObject.SetActive(false);
        }
    }

    /// <summary>
    /// Win 오브젝트 표시
    /// </summary>
    public void ShowWinEffect()
    {
        // Lose 오브젝트 숨김
        if (loseObject != null)
        {
            loseObject.SetActive(false);
        }

        // Win 오브젝트 표시
        if (winObject != null)
        {
            winObject.SetActive(true);
            winObject.transform.localScale = Vector3.zero;
            winObject.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 0);

            // 승리 애니메이션 시퀀스
            Sequence winSequence = DOTween.Sequence();
            winSequence.Append(winObject.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
            winSequence.Join(winObject.GetComponentInChildren<SpriteRenderer>().DOFade(1f, 0.3f));
            winSequence.AppendInterval(2f);
            winSequence.Append(winObject.GetComponentInChildren<SpriteRenderer>().DOFade(0f, 0.3f));
            winSequence.AppendCallback(() => winObject.SetActive(false));
        }
    }

    /// <summary>
    /// Lose 오브젝트 표시
    /// </summary>
    public void ShowLoseEffect()
    {
        // Win 오브젝트 숨김
        if (winObject != null)
        {
            winObject.SetActive(false);
        }

        // Lose 오브젝트 표시
        if (loseObject != null)
        {
            loseObject.SetActive(true);

            loseObject.transform.localScale = Vector3.zero;
            loseObject.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 0);

            // 승리 애니메이션 시퀀스
            Sequence winSequence = DOTween.Sequence();
            winSequence.Append(loseObject.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
            winSequence.Join(loseObject.GetComponentInChildren<SpriteRenderer>().DOFade(1f, 0.3f));
            winSequence.AppendInterval(2f);
            winSequence.Append(loseObject.GetComponentInChildren<SpriteRenderer>().DOFade(0f, 0.3f));
            winSequence.AppendCallback(() => loseObject.SetActive(false));
        }
    }

    /// <summary>
    /// 모든 Win/Lose 오브젝트 숨김
    /// </summary>
    public void HideAllEffects()
    {
        if (winObject != null)
        {
            winObject.SetActive(false);
        }

        if (loseObject != null)
        {
            loseObject.SetActive(false);
        }
    }
}
