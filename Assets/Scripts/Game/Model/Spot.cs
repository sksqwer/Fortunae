using UnityEngine;
using UnityEngine.UI; // (UGUI 사용 시)
using TMPro; // (TextMeshPro 사용 시)
using System;


[Serializable]
public class Spot
{[Header("시각적 요소 연결")]
    // TextMeshPro를 사용하지 않는다면 'Text'로 변경하세요.
    [SerializeField] private TextMeshProUGUI numberText; 
    [SerializeField] private Image background; 
    
    // (선택 사항) 디버깅이나 확률/배당률 표시에 사용
    [SerializeField] private TextMeshProUGUI debugInfoText; 

    // --- 내부 참조 ---
    private SpotData myData; // GameManager로부터 주입받을 나의 '데이터(두뇌)'
    private GameManager gameManager; // GameManager의 싱글톤 인스턴스

    /// <summary>
    /// GameManager가 호출하여 이 Spot 객체에게 '너의 데이터는 이것이다'라고 알려주는 함수
    /// </summary>
    public void Initialize(SpotData data, GameManager manager)
    {
        this.myData = data;
        this.gameManager = manager;
        
        // 초기 시각적 설정
        UpdateVisuals();
    }

    /// <summary>
    /// GameManager의 데이터(myData)를 기반으로 화면의 UI를 갱신합니다.
    /// GameManager가 데이터 변경 후 이 함수를 호출해줘야 합니다.
    /// </summary>
    public void UpdateVisuals()
    {
        if (myData == null) return; // 데이터가 아직 없으면 중단

        // 1. 숫자 표시
        numberText.text = myData.displayNumber.ToString();

        // 2. 파괴 상태 표시
        if (myData.isDestroyed)
        {
            background.color = Color.grey; // (예시) 파괴되면 회색으로
            numberText.color = Color.black;
            debugInfoText.text = "DESTROYED";
        }
        else
        {
            // (예시) 원래 색상으로 복구 (myData.color 값에 따라 Red/Black 설정)
            background.color = (myData.color == "Red") ? Color.red : Color.black;
            numberText.color = Color.white;

            // 3. (선택 사항) 확률 및 배당률 표시
            debugInfoText.text = $"P: {(myData.currentProbability * 100f).ToString("F2")}%\n" +
                                 $"X: {myData.FinalPayout.ToString("F1")}";
        }
    }

    /// <summary>
    /// 플레이어가 이 Spot 객체를 클릭했을 때 호출됩니다.
    /// (UI Button의 OnClick() 이벤트에 이 함수를 연결하세요)
    /// </summary>
    public void OnSpotClicked()
    {
        if (myData.isDestroyed) return; // 파괴된 Spot은 클릭 불가

        // 나는 계산하지 않는다. GameManager에게 보고만 한다.
        // gameManager.HandleSpotClick(myData);
    }
    
}
