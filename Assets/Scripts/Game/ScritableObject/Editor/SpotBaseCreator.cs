using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// SpotSO 에셋을 생성하고, 그 안에 36개의 SpotBase 데이터를
/// 자동으로 채워주는 에디터 스크립트입니다.
/// </summary>
public class SpotSOCreator
{
    // 생성된 에셋을 저장할 경로
    private const string savePath = "Assets/RouletteData";
    private const string assetName = "RouletteDefinition.asset";

    // 룰렛의 빨간색 숫자 목록 (유러피안 룰렛 기준)
    private static readonly int[] redNumbers = { 
        1, 3, 5, 7, 9, 12, 14, 16, 18, 
        19, 21, 23, 25, 27, 30, 32, 34, 36 
    };

    /// <summary>
    /// 유니티 상단 메뉴에 "Tools > Roulette > Create/Update Roulette Definition" 항목을 추가합니다.
    /// </summary>
    [MenuItem("Tools/Roulette/Create/Update Roulette Definition")]
    public static void CreateOrUpdateAsset()
    {
        // 1. 저장할 폴더가 없으면 생성합니다.
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string assetPath = $"{savePath}/{assetName}";

        // 2. 파일이 이미 존재하는지 확인합니다.
        SpotSO definitionSO = AssetDatabase.LoadAssetAtPath<SpotSO>(assetPath);

        if (definitionSO == null)
        {
            // 3-A. 파일이 없으면: 새로 생성합니다.
            definitionSO = ScriptableObject.CreateInstance<SpotSO>();
            AssetDatabase.CreateAsset(definitionSO, assetPath);
            Debug.Log($"[생성] 새 RouletteDefinition 에셋을 생성했습니다: {assetPath}");
        }
        else
        {
            // 3-B. 파일이 있으면: 기존 데이터를 덮어쓸 것임을 알립니다.
            Debug.Log($"[업데이트] 기존 RouletteDefinition 에셋을 찾았습니다. 데이터를 덮어씁니다.");
        }

        // 4. SpotBase 리스트를 생성 (또는 초기화) 합니다.
        definitionSO.spotBaseList = new List<SpotBase>();

        // 5. 1부터 36까지 데이터를 계산하여 리스트에 추가합니다.
        for (int i = 1; i <= 36; i++)
        {
            // 생성자에서 (ID, 홀짝, 그룹 등) 자동 계산
            SpotBase spotBase = new SpotBase(i, System.Array.Exists(redNumbers, num => num == i) ? SpotColor.Red : SpotColor.Black);
            
            definitionSO.spotBaseList.Add(spotBase);
        }
        
        // 6. 변경된 ScriptableObject를 저장합니다.
        EditorUtility.SetDirty(definitionSO); // (중요) SO가 변경되었음을 에디터에 알림
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 7. 생성된 파일을 하이라이트해줍니다.
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = definitionSO;

        Debug.Log($"[성공] SpotSO 에 36개의 SpotBase 데이터를 성공적으로 생성/업데이트 했습니다.");
    }
}