using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;

public static class DefineSymbolManager
{
    public struct DefineSymbolData
    {
        public BuildTargetGroup buildTargetGroup; // ���� ���� Ÿ�� �׷�
        public string fullSymbolString;           // ���� ���� Ÿ�� �׷쿡�� ���ǵ� �ɺ� ���ڿ� ��ü
        public Regex symbolRegex;

        public DefineSymbolData(string symbol)
        {
            buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            fullSymbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            symbolRegex = new Regex(@"\b" + symbol + @"\b(;|$)");
        }
    }

    /// <summary> �ɺ��� �̹� ���ǵǾ� �ִ��� �˻� </summary>
    public static bool IsSymbolAlreadyDefined(string symbol)
    {
        DefineSymbolData dsd = new DefineSymbolData(symbol);

        return dsd.symbolRegex.IsMatch(dsd.fullSymbolString);
    }

    /// <summary> �ɺ��� �̹� ���ǵǾ� �ִ��� �˻� </summary>
    public static bool IsSymbolAlreadyDefined(string symbol, out DefineSymbolData dsd)
    {
        dsd = new DefineSymbolData(symbol);

        return dsd.symbolRegex.IsMatch(dsd.fullSymbolString);
    }

    /// <summary> Ư�� ������ �ɺ� �߰� </summary>
    public static void AddDefineSymbol(string symbol)
    {
        // ������ �������� ������ ���� �߰�
        if (!IsSymbolAlreadyDefined(symbol, out var dsd))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(dsd.buildTargetGroup, $"{dsd.fullSymbolString};{symbol}");
        }
    }

    /// <summary> Ư�� ������ �ɺ� ���� </summary>
    public static void RemoveDefineSymbol(string symbol)
    {
        // ������ �����ϸ� ����
        if (IsSymbolAlreadyDefined(symbol, out var dsd))
        {
            string strResult = dsd.symbolRegex.Replace(dsd.fullSymbolString, "");

            PlayerSettings.SetScriptingDefineSymbolsForGroup(dsd.buildTargetGroup, strResult);
        }
    }
}
#endif