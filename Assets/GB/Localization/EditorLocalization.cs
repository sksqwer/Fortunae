#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using Newtonsoft.Json;
using QuickEye.Utility;
using System.Text.RegularExpressions;


namespace GB
{
    public class EditorLocalization : EditorWindow
    {
        static GSheetDomain domain = new GSheetDomain();
        [MenuItem("GB/UI/Localization")]
        static void init()
        {
            EditorWindow.GetWindow(typeof(EditorLocalization));
        }
        private void OnEnable()
        {
            domain.Load();
            Load();
        }

        public UnityDictionary<string, UnityDictionary<SystemLanguage, string>> localizeDict = new UnityDictionary<string, UnityDictionary<SystemLanguage, string>>();

        void Load()
        {

            var data = Resources.Load<LocalizationData>("LocalizationData");
            if(data == null) return;
             localizeDict = data.Datas;

            _serializedObject = new SerializedObject(this);
            _myUser = _serializedObject.FindProperty("localizeDict");
            if (_myUser == null)
                Debug.Log("_myUser == null");
        }

        SystemLanguage GetLanguage(string strLanguage)
        {
            int Length = (int)SystemLanguage.Unknown;

            for (int i = 0; i < Length; ++i)
            {
                SystemLanguage lan = (SystemLanguage)i;
                if (string.Equals(lan.ToString(), strLanguage))
                    return lan;
            }

            return SystemLanguage.Unknown;
        }




        private Vector2 scrollPosition;

        private SerializedObject _serializedObject;
        private SerializedProperty _myUser;

        private void OnGUI()
        {

            GUILayout.BeginArea(new Rect(10, 20, position.width - 20, position.height - 20));
            GB.EditorGUIUtil.DrawHeaderLabel("GB Localization");
            GB.EditorGUIUtil.Space(5);



            if (localizeDict != null && _myUser != null)
            {
                GB.EditorGUIUtil.Start_VerticalBox();
                scrollPosition = GB.EditorGUIUtil.Start_ScrollView(scrollPosition);

                _serializedObject.Update();
                EditorGUILayout.PropertyField(_myUser);
                _serializedObject.ApplyModifiedProperties();

                GB.EditorGUIUtil.End_ScrollView();
                GB.EditorGUIUtil.End_Vertical();

            }

            GB.EditorGUIUtil.BackgroundColor(Color.blue);

            if (GB.EditorGUIUtil.DrawSyleButton("Load"))
            {
                Load();
                Repaint();
            }
            GB.EditorGUIUtil.BackgroundColor(Color.white);
            GUILayout.BeginVertical("box");


            GUI.backgroundColor = new Color(0f, 1f, 0.5f, 1f);
            GB.EditorGUIUtil.Start_Horizontal();
            domain.LocailUrl = GB.EditorGUIUtil.DrawTextField("URL", domain.LocailUrl);

            if (GUILayout.Button("Link", GUILayout.Width(100)))
                Application.OpenURL(domain.LocailUrl);

            GB.EditorGUIUtil.End_Horizontal();
            if (!string.IsNullOrEmpty(domain.LocailUrl))
            {
                if (GB.EditorGUIUtil.DrawSyleButton("Download"))
                {
                    domain.Save();
                        

                    string tsv = UrlDownload(domain.GetURL_TSV(domain.LocailUrl));
                    string json = PaserTsvToJson(tsv);
                    var dict = PaserJsonToDict(json);

                     var info = new DirectoryInfo(Application.dataPath + "/Resources");
                    if (info.Exists == false) info.Create();

                    LocalizationData data  = ScriptableObject.CreateInstance<LocalizationData>();
                    data.Datas = dict;
                    AssetDatabase.CreateAsset(data, "Assets/Resources/LocalizationData.asset"); // 파일 생성
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Repaint();
                     Selection.activeObject = data;


                }
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();

            GUILayout.EndArea();

        }

        UnityDictionary<string, UnityDictionary<SystemLanguage, string>> PaserJsonToDict(string json)
        {

            // Debug.Log(json);
            UnityDictionary<string, UnityDictionary<SystemLanguage, string>> dict = new UnityDictionary<string, UnityDictionary<SystemLanguage, string>>();


            var d = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var datas = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(d["Datas"].ToString());
            Debug.Log("datas Len : " + datas.Count);

              for (int i = 0; i < datas.Count; ++i)
            {
                int idx = 0;
                string dataKey = string.Empty;

                foreach (var v in datas[i])
                {
                    if (idx == 0)
                    {
                        dict[v.Value] = new UnityDictionary<SystemLanguage, string>();
                        dataKey = v.Value;
                    }
                    else
                    {
                        SystemLanguage language = GetLanguage(v.Key);
                        if (language != SystemLanguage.Unknown)
                            dict[dataKey][language] = v.Value;
                        else
                            Debug.Log("None Language : " + v.Key);
                    }

                    ++idx;
                }
            }

            return dict;



        }

        private string UrlDownload(string url)
        {

            if (url == null)
            {
                return null;
            }


            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SendWebRequest();
                while (!www.isDone) { }
                Debug.Log(www.downloadHandler.text);
                return www.downloadHandler.text;
            }
        }
        private void FileSave(string path, string fileName, string data)
        {
            //string path = Application.dataPath + "/" + "Resources/Json";
            DirectoryInfo info = new DirectoryInfo(path);
            if (info.Exists == false)
                info.Create();

            System.IO.File.WriteAllText(path + "/" + fileName, data);
            UnityEditor.AssetDatabase.Refresh();
        }


        private string PaserTsvToJson(string csv)
        {

            string json = string.Empty;
            List<string> rowKeys = new List<string>();
            List<string> columnKeys = new List<string>();
            List<string> typeKeys = new List<string>();
            List<string> values = new List<string>();


            string[] columns = csv.Replace("\r", "").Split("\n");
            string[] rowKeyArr = columns[0].Split("\t");
            string[] typeKeyArr = columns[1].Split("\t");

            for (int i = 1; i < typeKeyArr.Length; ++i)
            {
                if (string.IsNullOrEmpty(typeKeyArr[i])) break;

                typeKeys.Add(typeKeyArr[i]);
            }

            for (int i = 1; i < rowKeyArr.Length; ++i)
            {
                if (string.IsNullOrEmpty(rowKeyArr[i])) break;

                rowKeys.Add(rowKeyArr[i]);
            }



            for (int y = 0; y < columns.Length; ++y)
            {
                string[] rows = columns[y].Split("\t");
                if (string.IsNullOrEmpty(rows[0])) continue;

                for (int x = 0; x < rows.Length; ++x)
                {
                    if (y >= 3 && x == 0)
                        columnKeys.Add(rows[x]);

                    if (y >= 3 && x >= 1)
                    {
                        values.Add(rows[x].Replace("@COMMA", ",").Replace("\\n", "\n"));
                    }
                }
            }


            json = @"{""Datas"":[$DATAS$]}";
            string datas = string.Empty;

            for (int y = 0; y < columnKeys.Count; ++y)
            {
                Dictionary<string, object> dicData = new Dictionary<string, object>();
                dicData.Add(rowKeyArr[0], columnKeys[y]);

                for (int x = 0; x < rowKeys.Count; ++x)
                {
                    dicData.Add(rowKeys[x], GetObjType(typeKeys[x], values[y * rowKeys.Count + x]));

                }

                if (string.IsNullOrEmpty(datas))
                    datas = JsonConvert.SerializeObject(dicData);
                else
                    datas = string.Format("{0},{1}", datas, JsonConvert.SerializeObject(dicData));
            }


            json = json.Replace("$DATAS$", datas);
            return json;

        }
        public object GetObjType(string type, string value)
        {
            object oj = null;
            switch (type)
            {
                case "int":
                    if (string.IsNullOrEmpty(value)) oj = 0;
                    else oj = int.Parse(value);
                    break;

                case "float":
                    if (string.IsNullOrEmpty(value)) oj = 0.0f;
                    else oj = float.Parse(value);
                    break;

                case "long":
                    if (string.IsNullOrEmpty(value)) oj = 0;
                    else oj = long.Parse(value);
                    break;

                case "string":
                    oj = value;
                    break;

                case "double":
                    if (string.IsNullOrEmpty(value)) oj = 0;
                    else oj = double.Parse(value);
                    break;

                default:
                    if (string.IsNullOrEmpty(value)) oj = "Empty";
                    else oj = value;
                    break;
            }

            return oj;

        }


    }

    

[System.Serializable]
public class GSheetDomain
{
    public string LocailUrl;
    public List<string> SheetNameList = new List<string>();
    public List<string> UrlList = new List<string>();

    public int GetIndex(string sheetName)
    {
        for (int i = 0; i < SheetNameList.Count; ++i)
        {
            if (sheetName == SheetNameList[i])
                return i;
        }
        return -1;
    }

    public void SetJson(string json)
    {
        var data = JsonConvert.DeserializeObject<GSheetDomain>(json);
        UrlList = data.UrlList;
        SheetNameList = data.SheetNameList;
        LocailUrl = data.LocailUrl;
    }

    public void Add(string sheetName, string Url)
    {
        SheetNameList.Add(sheetName);
        UrlList.Add(Url);

    }

    public void OpenURL(int index)
    {
        Application.OpenURL(UrlList[index]);

    }

    public void Remove(int index)
    {
        SheetNameList.RemoveAt(index);
        UrlList.RemoveAt(index);

        Save();
    }

    public string GetURL_TSV(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        try
        {
            var arr = url.Split("/");
            var f = arr[6].Split("gid=");

            string urlID = arr[5];
            string gid = Regex.Replace(f[1], @"\D", "");

            string URL_SHEET = $"https://docs.google.com/spreadsheets/d/$URL_ID$/export?format=tsv&gid=$GID$";
            return URL_SHEET.Replace("$URL_ID$", urlID).Replace("$GID$", gid);
        }
        catch
        {
            Debug.LogError("URL Error : " + url);
            return null;
        }
    }



    public string GetURL(int index)
    {
        string url = UrlList[index];

        if (string.IsNullOrEmpty(url))
            return null;

        try
        {

            var arr = url.Split("/");
            var f = arr[6].Split("gid=");

            string urlID = arr[5];
            string gid = Regex.Replace(f[1], @"\D", "");

            string URL_SHEET = $"https://docs.google.com/spreadsheets/d/$URL_ID$/export?format=csv&gid=$GID$";
            return URL_SHEET.Replace("$URL_ID$", urlID).Replace("$GID$", gid);
        }
        catch
        {
            Debug.LogError("URL Error : " + url);

            return null;
        }

    }

    public string ToJson()
    {
        GSheetDomain gsheet = new GSheetDomain();
        gsheet.LocailUrl = LocailUrl;
        gsheet.UrlList = UrlList;
        gsheet.SheetNameList = SheetNameList;
        return JsonConvert.SerializeObject(gsheet);
    }


  public void Save()
    {
        string json = ToJson();

        string gbPath = Application.dataPath + "/" + "GB/GSheet/GameData";

        DirectoryInfo info = new DirectoryInfo(gbPath);
        if (info.Exists == false)
            info.Create();

        string value = Gzip.Compression(json);

        System.IO.File.WriteAllText(gbPath + "/O.txt", value);
        UnityEditor.AssetDatabase.Refresh();

    }

    public void Load()
    {
        string filePath = Application.dataPath + "/" + "GB/GSheet/GameData/O.txt";
        if (System.IO.File.Exists(filePath))
        {
            string data = System.IO.File.ReadAllText(filePath);
            string json = Gzip.DeCompression(data);
            SetJson(json);
        }

    }




    public int Count
    {
        get
        {
            return UrlList.Count;
        }
    }

}
}

#endif