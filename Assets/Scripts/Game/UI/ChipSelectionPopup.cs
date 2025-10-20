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
    [SerializeField] private int spotID;
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
    private Action<int, ChipType, int> onConfirmWithType;

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
            Action<int, ChipType, int> callback = (Action<int, ChipType, int>)parameters[2];

            ShowChipSelection(spotID, chips, callback);
        }
    }

    /// <summary>
    /// 칩 타입과 개수를 선택하는 팝업 표시
    /// </summary>
    public void ShowChipSelection(int spotID, ChipCollection chips, Action<int, ChipType, int> callback)
    {
        gameObject.SetActive(true);

        // 칩 컬렉션 복사 (원본 보호)
        this.spotID = spotID;
        availableChips = chips.Clone();
        onConfirmWithType = callback;

        if (titleText != null)
            titleText.text = $"Select Chip for Spot {spotID}";

        // 칩 타입 드롭다운 설정
        SetupChipTypeDropdown();

        // 첫 번째 사용 가능한 칩 타입 선택 (초기 값은 0개 선택으로 시작)
        SelectFirstAvailableChipType();

        // 초기 상태에서는 0개 선택 → Confirm 비활성화
        if (confirmButton != null)
            confirmButton.interactable = false;

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
        int firstAvailableIndex = 0;
        int currentIndex = 0;
        bool foundFirst = false;

        foreach (ChipType type in ChipTypeCache.AllTypes)
        {
            int count = availableChips[type];
            string chipSprite = ChipTypeCache.ToSpriteTag(type);
            string label = $"{chipSprite} ({count}chips)";

            options.Add(new TMP_Dropdown.OptionData(label));
            
            // 처음으로 칩이 있는 타입의 인덱스 저장
            if (!foundFirst && count > 0)
            {
                firstAvailableIndex = currentIndex;
                foundFirst = true;
            }
            
            currentIndex++;
        }

        chipTypeDropdown.AddOptions(options);
        chipTypeDropdown.onValueChanged.RemoveAllListeners();
        chipTypeDropdown.onValueChanged.AddListener(OnChipTypeChanged);
        
        // 처음에 가지고 있는 칩 타입으로 드롭다운 설정
        if (foundFirst)
        {
            chipTypeDropdown.value = firstAvailableIndex;
            Debug.Log($"[ChipSelectionPopup] Dropdown set to first available chip at index {firstAvailableIndex}");
        }
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
            chipSlider.wholeNumbers = true;
            chipSlider.minValue = 0;                // 0개부터 시작
            chipSlider.maxValue = Mathf.Max(0, maxChips);
            
            // 항상 0개 선택으로 시작 (사용자가 증가시켜서 1/Max 형태가 되도록)
            chipSlider.value = 0;
            Debug.Log($"[ChipSelectionPopup] Slider set to 0 / {maxChips}");
            
            chipSlider.onValueChanged.RemoveAllListeners();
            chipSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        // 0개 기준으로 텍스트 업데이트 & Confirm 버튼 상태 반영
        int current = (int)(chipSlider != null ? chipSlider.value : 0);
        UpdateChipCountText(current);
        if (confirmButton != null)
            confirmButton.interactable = (current > 0);
    }

    private void OnSliderChanged(float value)
    {
        int count = (int)value;
        UpdateChipCountText(count);
        
        // 0개면 Confirm 비활성화, 1개 이상이면 활성화
        if (confirmButton != null)
            confirmButton.interactable = (count > 0);
    }

    private void UpdateChipCountText(int count)
    {
        if (chipCountText != null)
        {
            string chipSprite = ChipTypeCache.ToSpriteTag(selectedChipType);
            int totalValue = count * ChipTypeCache.Values[selectedChipType];
            chipCountText.text = $"{count}chips × {chipSprite} = ${totalValue}";
        }
    }

    private void OnConfirmWithType()
    {
        int chipCount = chipSlider != null ? (int)chipSlider.value : 0;
        if (chipCount <= 0)
        {
            Debug.LogWarning("[ChipSelectionPopup] Cannot confirm with 0 chips selected");
            return;
        }
        onConfirmWithType?.Invoke(spotID, selectedChipType, chipCount);
        Debug.Log($"[ChipSelectionPopup] OnConfirmWithType: Spot {spotID}, Chip {selectedChipType} x{chipCount}");
        OnClose();
    }

    private void OnCancel()
    {
        Debug.Log($"[ChipSelectionPopup] OnCancel: Spot {spotID}");
        OnClose();
    }
    
    private void OnClose()
    {
        Debug.Log($"[ChipSelectionPopup] OnClose: Spot {spotID}");
        gameObject.SetActive(false);
        Game.isPopupOpen = false;
    }
}

