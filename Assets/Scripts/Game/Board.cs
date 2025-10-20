using UnityEngine;
using System.Collections.Generic;
using GB;

/// <summary>
/// 룰렛 보드 (3D Spot 오브젝트 관리)
/// </summary>
public class Board : MonoBehaviour
{
    [SerializeField] private SpotObject[] spotObjects = new SpotObject[36];
    
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
            Debug.Log($"[Board] Found {spotObjects.Length} SpotObjects");
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
                    Debug.Log($"[Board] Initialized SpotObject {spotID}");
                }
                else
                {
                    Debug.LogError($"[Board] Spot data not found for ID {spotID}");
                }
            }
        }
        
        UpdateAllVisuals();
        
        Debug.Log($"[Board] Connected {spotObjects.Length} SpotObjects to data");
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
}
