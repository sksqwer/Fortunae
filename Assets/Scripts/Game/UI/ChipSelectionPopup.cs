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
    private GameState currentGameState; // 현재 게임 상태 저장
    
    private void Awake()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();

        Regist();
        SetupItemButtons(); // 버튼 이벤트 초기화 (한 번만)

        Debug.Log("[ChipSelectionPopup] ChipSelectionPopup Initialized");
    }
    
    public override void ViewQuick(string key, IOData data)
    {
        base.ViewQuick(key, data);
        
        switch (key)
        {
            case Game.Keys.UPDATE_INVENTORY:
                // 인벤토리가 업데이트되면 아이템 버튼 새로고침
                if (data is OData<GameState> inventoryData)
                {
                    currentGameState = inventoryData.Get();
                    RefreshItemButtons(currentGameState);
                }
                break;
        }
    }

    [Header("UI Elements")]
    [SerializeField] private int objectID;
    [SerializeField] private BetType betType;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Slider chipSlider;
    [SerializeField] private TMP_Text chipCountText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;


    [Header("Chip Type Selection (Optional)")]
    [SerializeField] private TMP_Dropdown chipTypeDropdown;
    [SerializeField] private GameObject chipTypePanel; // 칩 타입 선택 패널 (있으면 표시)
    
    [Header("Item Buttons")]
    [SerializeField] private GameObject itemButtonsPanel; // 아이템 버튼들을 그룹으로 관리하는 패널
    [SerializeField] private Button plusSpotButton;
    [SerializeField] private Button copySpotButton;
    [SerializeField] private Button upgradedMultiSpotButton;
    [SerializeField] private Button hatWingButton;
    
    [Header("Item Count Texts")]
    [SerializeField] private TMP_Text plusSpotCountText;
    [SerializeField] private TMP_Text copySpotCountText;
    [SerializeField] private TMP_Text upgradedMultiSpotCountText;
    [SerializeField] private TMP_Text hatWingCountText;
    
    [Header("HatWing Status")]
    [SerializeField] private UnityEngine.UI.Image hatWingStatusImage; // HatWing ON/OFF 이미지

    private int maxChips;
    private ChipType selectedChipType = ChipType.Chip1;
    private ChipCollection availableChips;
    private Action<BetConfirmMessage> onConfirmWithType;
    
    // HatWing 로컬 상태 (팝업별로 관리)
    private bool isHatWingActive = false;

    /// <summary>
    /// UIScreen SetData 오버라이드
    /// </summary>
    public override void SetData(object data)
    {
        base.SetData(data);

        // 박싱/언박싱 없이 구조체 사용
        if (data is ChipSelectionMessage chipData)
        {
            ShowChipSelection(chipData);
        }
    }

    /// <summary>
    /// 칩 타입과 개수를 선택하는 팝업 표시
    /// </summary>
    public void ShowChipSelection(ChipSelectionMessage chipData)
    {
        gameObject.SetActive(true);

        // 칩 컬렉션 복사 (원본 보호)
        this.objectID = chipData.objectID;
        this.betType = chipData.betType;
        this.currentGameState = chipData.gameState; // 게임 상태 저장
        availableChips = chipData.gameState.availableChips.Clone();
        onConfirmWithType = chipData.callback;

        if (titleText != null)
            titleText.text = BetTypeHelper.GetPopupTitle(betType, objectID);

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

        isHatWingActive = false;
        
        // 아이템 버튼 패널 표시/숨김 (Number Bet만 아이템 사용 가능)
        if (itemButtonsPanel != null)
        {
            bool isNumberBet = (betType == BetType.Number);
            itemButtonsPanel.SetActive(isNumberBet);
            Debug.Log($"[ChipSelectionPopup] Item buttons panel {(isNumberBet ? "shown" : "hidden")} for bet type: {betType}");
        }
        
        // 아이템 버튼 갱신 (GameState가 바뀌었을 수 있으므로)
        RefreshItemButtons(chipData.gameState);
    }

    /// <summary>
    /// 아이템 버튼 초기화 (한 번만)
    /// </summary>
    private void SetupItemButtons()
    {
        // PLUS_SPOT 버튼
        if (plusSpotButton != null)
        {
            plusSpotButton.onClick.RemoveAllListeners();
            plusSpotButton.onClick.AddListener(() => OnItemButtonClicked("PLUS_SPOT"));
        }
        
        // COPY_SPOT 버튼 (2개 선택 필요)
        if (copySpotButton != null)
        {
            copySpotButton.onClick.RemoveAllListeners();
            copySpotButton.onClick.AddListener(() => OnCopySpotClicked());
        }
        
        // UPGRADED_MULTI_SPOT 버튼
        if (upgradedMultiSpotButton != null)
        {
            upgradedMultiSpotButton.onClick.RemoveAllListeners();
            upgradedMultiSpotButton.onClick.AddListener(() => OnItemButtonClicked("UPGRADED_MULTI_SPOT"));
        }
        
        // HAT_WING 버튼
        if (hatWingButton != null)
        {
            hatWingButton.onClick.RemoveAllListeners();
            hatWingButton.onClick.AddListener(() => OnItemButtonClicked("HAT_WING"));
        }
    }
    
    /// <summary>
    /// 아이템 버튼 갱신 (데이터 변경 시)
    /// </summary>
    private void RefreshItemButtons(GameState gameState)
    {
        // PLUS_SPOT 버튼
        if (plusSpotButton != null)
        {            
            plusSpotButton.gameObject.SetActive(true);

            bool hasPlusSpot = gameState != null && gameState.HasItem("PLUS_SPOT");
            plusSpotButton.interactable = hasPlusSpot;

            var item = gameState?.GetItemByID("PLUS_SPOT");

            if (plusSpotCountText != null && item != null && item.count > 0)
            {
                plusSpotCountText.text = $"• <sprite={item.SpriteIndex}> {item.itemID} : ({item.count})\n";
            }
            else if (plusSpotCountText != null)
            {
                plusSpotCountText.text = "";
                plusSpotButton.gameObject.SetActive(false);
            }
        }
        
        // COPY_SPOT 버튼
        if (copySpotButton != null)
        {
            copySpotButton.gameObject.SetActive(true);
            
            bool hasCopySpot = gameState != null && gameState.HasItem("COPY_SPOT");
            copySpotButton.interactable = hasCopySpot;

            var item = gameState?.GetItemByID("COPY_SPOT");

            if (copySpotCountText != null && item != null && item.count > 0)
            {
                copySpotCountText.text = $"• <sprite={item.SpriteIndex}> {item.itemID} : ({item.count})\n";
            }
            else if (copySpotCountText != null)
            {
                copySpotCountText.text = "";
                copySpotButton.gameObject.SetActive(false);
            }
        }
        
        // UPGRADED_MULTI_SPOT 버튼
        if (upgradedMultiSpotButton != null)
        {
            upgradedMultiSpotButton.gameObject.SetActive(true);

            bool hasUpgradedMultiSpot = gameState != null && gameState.HasItem("UPGRADED_MULTI_SPOT");
            upgradedMultiSpotButton.interactable = hasUpgradedMultiSpot;

            var item = gameState?.GetItemByID("UPGRADED_MULTI_SPOT");

            if (upgradedMultiSpotCountText != null && item != null && item.count > 0)
            {
                upgradedMultiSpotCountText.text = $"• <sprite={item.SpriteIndex}> {item.itemID} : ({item.count})\n";
            }
            else if (upgradedMultiSpotCountText != null)
            {
                upgradedMultiSpotCountText.text = "";
                upgradedMultiSpotButton.gameObject.SetActive(false);
            }
        }
        
        // HAT_WING 버튼 (토글 방식)
        if (hatWingButton != null)
        {
            hatWingButton.gameObject.SetActive(true);

            bool hasHatWing = gameState != null && gameState.HasItem("HAT_WING");
            hatWingButton.interactable = hasHatWing;

            var item = gameState?.GetItemByID("HAT_WING");

            if (hatWingCountText != null && item != null && item.count > 0)
            {
                hatWingCountText.text = $"• <sprite={item.SpriteIndex}> {item.itemID} : ({item.count})\n";
            }
            else if (hatWingCountText != null)
            {
                hatWingCountText.text = "";
                hatWingButton.gameObject.SetActive(false);
            }   
            
            // HatWing 상태 표시 (이미지로 ON/OFF)
            UpdateHatWingStatus();
        }
    }
    
    /// <summary>
    /// 아이템 버튼 클릭 처리
    /// </summary>
    private void OnItemButtonClicked(string itemID)
    {
        Debug.Log($"[ChipSelectionPopup] Item button clicked: {itemID} for object {objectID}");
        
        // HatWing은 토글 방식
        if (itemID == "HAT_WING")
        {
            OnHatWingToggle();
        }
        else
        {
            // 다른 아이템들은 기존 방식
            GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_USE_ITEM_BY_ID, new object[] { itemID, new int[] { objectID } });
            OnClose();
        }
    }
    
    /// <summary>
    /// CopySpot 클릭 처리 (원본 Spot 선택 → 팝업 닫고 대기)
    /// </summary>
    private void OnCopySpotClicked()
    {
        Debug.Log($"[ChipSelectionPopup] CopySpot clicked - Source Spot: {objectID}");
        
        // Game에 CopySpot 모드 활성화 명령 전송 (원본 SpotID 전달)
        GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_START_COPY_SPOT, objectID);
        
        // 팝업 닫기
        OnClose();
    }
    
    /// <summary>
    /// HatWing 토글 처리 (로컬 상태 변경 + UI 업데이트)
    /// </summary>
    private void OnHatWingToggle()
    {
        // 현재 HatWing 보유 개수 확인
        int hatWingCount = currentGameState != null ? (currentGameState.GetItemByID("HAT_WING")?.count ?? 0) : 0;
        
        Debug.Log($"[ChipSelectionPopup] HatWing toggle clicked - Current Active: {isHatWingActive}, Available: {hatWingCount}");
        
        // HatWing이 없으면 토글 불가
        if (!isHatWingActive && hatWingCount <= 0)
        {
            Debug.LogWarning("[ChipSelectionPopup] Cannot toggle HatWing - no items available!");
            return;
        }
        
        // 로컬 HatWing 상태 토글
        isHatWingActive = !isHatWingActive;
        
        Debug.Log($"[ChipSelectionPopup] HatWing toggled to: {isHatWingActive}");
        
        // HatWing 상태 UI 즉시 업데이트
        UpdateHatWingStatus();
    }
    
    /// <summary>
    /// HatWing 상태 UI 업데이트
    /// </summary>
    private void UpdateHatWingStatus()
    {
        // HatWing 상태 표시 (이미지로 ON/OFF)
        if (hatWingStatusImage != null)
        {
            hatWingStatusImage.color = isHatWingActive ? Color.green : Color.red;
            hatWingStatusImage.gameObject.SetActive(true);
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
        
        // 적용된 아이템 리스트 생성
        List<string> appliedItems = new List<string>();
        
        // HatWing이 활성화되어 있는지 확인
        if (isHatWingActive)
        {
            // 같은 배팅(betType + targetValue + isHatWingApplied)이 이미 있는지 확인
            bool existingBetWithHatWing = false;
            
            if (currentGameState != null && currentGameState.currentBets != null)
            {
                existingBetWithHatWing = currentGameState.currentBets.Exists(b =>
                    b.betType == betType && b.targetValue == objectID && b.isHatWingApplied);
            }
            
            // 이미 HatWing이 적용된 배팅이 없으면 사용
            if (!existingBetWithHatWing)
            {
                // HatWing은 배팅당 1개만 사용 (중복 사용 방지)
                GB.Presenter.Send(Game.DOMAIN, Game.Keys.CMD_USE_ITEM_BY_ID, new object[] { "HAT_WING", new int[] { objectID } });
                appliedItems.Add("HAT_WING");
                Debug.Log($"[ChipSelectionPopup] HatWing used for new bet: {betType} on {objectID}");
            }
            else
            {
                // 기존 배팅에 칩만 추가 (HatWing 재사용하지 않음)
                appliedItems.Add("HAT_WING"); // BetConfirmMessage에는 포함 (isHatWingApplied 플래그용)
                Debug.Log($"[ChipSelectionPopup] HatWing already applied to existing bet: {betType} on {objectID}, not using item again");
            }
            
            // HatWing 사용 후 로컬 상태 리셋 (중복 사용 방지)
            isHatWingActive = false;
            UpdateHatWingStatus();
        }
        
        // BetConfirmMessage 생성 및 콜백 호출
        var confirmData = new BetConfirmMessage(betType, objectID, selectedChipType, chipCount, appliedItems);
        onConfirmWithType?.Invoke(confirmData);
        
        OnClose();
    }

    private void OnCancel()
    {
        // 취소 시 HatWing 상태 리셋 (아이템 복구)
        if (isHatWingActive)
        {
            isHatWingActive = false;
            UpdateHatWingStatus();
            Debug.Log("[ChipSelectionPopup] HatWing state reset on cancel");
        }
        
        OnClose();
    }
    
    private void OnClose()
    {
        GB.Presenter.Send<int>(Game.DOMAIN, Game.Keys.CMD_POPUP_CLOSED, 0);
        gameObject.SetActive(false);
    }
}

