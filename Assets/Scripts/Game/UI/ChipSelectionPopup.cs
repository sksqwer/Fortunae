using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using GB;

/// <summary>
/// 칩 타입 및 개수 선택 팝업
/// </summary>
public class ChipSelectionPopup : UIScreen
{
    public override void Initialize()
    {
        base.Initialize();
        SetScreenType(ScreenType.POPUP);
        
        // UIManager에 수동 등록
        Debug.Log($"[ChipSelectionPopup] GameObject name: {gameObject.name}");
        Debug.Log($"[ChipSelectionPopup] Registering with UIManager...");
        UIManager.I.RegistUIScreen(this);
        Debug.Log($"[ChipSelectionPopup] Registration complete!");
    }
    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Slider chipSlider;
    [SerializeField] private TMP_Text chipCountText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    [Header("Chip Type Selection (Optional)")]
    [SerializeField] private TMP_Dropdown chipTypeDropdown;
    [SerializeField] private GameObject chipTypePanel; // 칩 타입 선택 패널 (있으면 표시)
    
    private int maxChips;
    private ChipType selectedChipType = ChipType.Chip1;
    private ChipCollection availableChips;
    private Action<ChipType, int> onConfirmWithType;
    
    /// <summary>
    /// UIScreen SetData 오버라이드
    /// </summary>
    public override void SetData(object data)
    {
        base.SetData(data);
        
        // 박싱/언박싱 없이 구조체 사용
        if (data is ChipSelectionData chipData)
        {
            ShowChipSelection(chipData.objectID, chipData.availableChips, chipData.callback);
        }
        // 기존 방식 (하위 호환성)
        else if (data is object[] parameters && parameters.Length >= 3)
        {
            int spotID = (int)parameters[0];
            ChipCollection chips = (ChipCollection)parameters[1];
            Action<ChipType, int> callback = (Action<ChipType, int>)parameters[2];
            
            ShowChipSelection(spotID, chips, callback);
        }
    }
    
    /// <summary>
    /// 칩 타입과 개수를 선택하는 팝업 표시
    /// </summary>
    public void ShowChipSelection(int spotID, ChipCollection chips, Action<ChipType, int> callback)
    {
        gameObject.SetActive(true);
        
        // 칩 컬렉션 복사 (원본 보호)
        availableChips = chips.Clone();
        onConfirmWithType = callback;
        
        if (titleText != null)
            titleText.text = $"Bet on Spot {spotID}";
        
        // 칩 타입 드롭다운 설정
        SetupChipTypeDropdown();
        
        // 첫 번째 사용 가능한 칩 타입 선택
        SelectFirstAvailableChipType();
        
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmWithType);
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancel);
        }
    }
    
    /// <summary>
    /// 칩 타입 드롭다운 설정
    /// </summary>
    private void SetupChipTypeDropdown()
    {
        if (chipTypeDropdown == null)
        {
            // 드롭다운이 없으면 패널 숨기기
            if (chipTypePanel != null)
                chipTypePanel.SetActive(false);
            return;
        }
        
        if (chipTypePanel != null)
            chipTypePanel.SetActive(true);
        
        chipTypeDropdown.ClearOptions();
        
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        
        foreach (ChipType type in ChipTypeCache.AllTypes)
        {
            int count = availableChips[type];
            int value = ChipTypeCache.Values[type];
            string label = $"${value} ({count}개)";
            
            options.Add(new TMP_Dropdown.OptionData(label));
        }
        
        chipTypeDropdown.AddOptions(options);
        chipTypeDropdown.onValueChanged.RemoveAllListeners();
        chipTypeDropdown.onValueChanged.AddListener(OnChipTypeChanged);
    }
    
    /// <summary>
    /// 첫 번째 사용 가능한 칩 타입 선택
    /// </summary>
    private void SelectFirstAvailableChipType()
    {
        foreach (ChipType type in ChipTypeCache.AllTypes)
        {
            if (availableChips[type] > 0)
            {
                selectedChipType = type;
                UpdateSliderForChipType(type);
                return;
            }
        }
        
        // 사용 가능한 칩이 없으면 기본값
        selectedChipType = ChipType.Chip1;
        UpdateSliderForChipType(ChipType.Chip1);
    }
    
    /// <summary>
    /// 칩 타입 변경 시
    /// </summary>
    private void OnChipTypeChanged(int index)
    {
        selectedChipType = ChipTypeCache.AllTypes[index];
        UpdateSliderForChipType(selectedChipType);
    }
    
    /// <summary>
    /// 선택된 칩 타입에 맞게 슬라이더 업데이트
    /// </summary>
    private void UpdateSliderForChipType(ChipType chipType)
    {
        maxChips = availableChips[chipType];
        
        if (chipSlider != null)
        {
            chipSlider.minValue = 1;
            chipSlider.maxValue = Mathf.Max(1, maxChips);
            chipSlider.value = Mathf.Min(1, maxChips);
            chipSlider.onValueChanged.RemoveAllListeners();
            chipSlider.onValueChanged.AddListener(OnSliderChanged);
        }
        
        UpdateChipCountText((int)(chipSlider != null ? chipSlider.value : 1));
    }
    
    private void OnSliderChanged(float value)
    {
        UpdateChipCountText((int)value);
    }
    
    private void UpdateChipCountText(int count)
    {
        if (chipCountText != null)
        {
            int chipValue = ChipTypeCache.Values[selectedChipType];
            int totalValue = count * chipValue;
            chipCountText.text = $"{count}개 × ${chipValue} = ${totalValue}";
        }
    }
    
    private void OnConfirmWithType()
    {
        int chipCount = chipSlider != null ? (int)chipSlider.value : 1;
        onConfirmWithType?.Invoke(selectedChipType, chipCount);
        gameObject.SetActive(false);
    }
    
    private void OnCancel()
    {
        gameObject.SetActive(false);
    }
}

