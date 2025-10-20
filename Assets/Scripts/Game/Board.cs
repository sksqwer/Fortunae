using UnityEngine;
using System.Collections.Generic;
using GB;

/// <summary>
/// 룰렛 보드 (3D Spot 및 BetObject 오브젝트 관리)
/// </summary>
public class Board : MonoBehaviour
{
    [SerializeField] private SpotObject[] spotObjects = new SpotObject[36];
    [SerializeField] private BetObject[] betObjects; // Color, OddEven, Dozen, Column 등
    
    private Dictionary<int, Spot> spotDataDictionary;
    
    /// <summary>
    /// 초기화
    /// </summary>
    public void Init()
    {
        Debug.Log("[Board] Init called");
        
        // SpotObject 자동 찾기
        if (spotObjects == null || spotObjects.Length == 0 || spotObjects[0] == null)
        {
            spotObjects = GetComponentsInChildren<SpotObject>();
            Debug.Log($"[Board] Found {spotObjects.Length} SpotObjects");
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
            Debug.Log($"[Board] Found {betObjects.Length} BetObjects (excluding SpotObjects)");
        }
        
        Debug.Log("[Board] Init completed");
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
                    Debug.Log($"[Board] Initialized SpotObject {spotID}");
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
        
        Debug.Log($"[Board] Connected {spotObjects.Length} SpotObjects to data");
    }
    
    /// <summary>
    /// BetObject 초기화
    /// </summary>
    private void InitializeBetObjects()
    {
        if (betObjects == null || betObjects.Length == 0)
        {
            Debug.LogWarning("[Board] No BetObjects to initialize");
            return;
        }
        
        Debug.Log($"[Board] Initializing {betObjects.Length} BetObjects");
        
        foreach (var betObj in betObjects)
        {
            if (betObj != null)
            {
                betObj.Initialize(betObj.ObjectID);
                // BetObject는 Inspector에서 이미 설정되어 있음 (betType, objectID 등)
                // 추가 초기화가 필요하면 여기서 처리
                Debug.Log($"[Board] BetObject initialized: {betObj.GetType().Name} ID={betObj.name}");
            }
        }
        
        Debug.Log("[Board] BetObjects initialized");
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
}
