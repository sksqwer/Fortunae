using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Networking;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GB
{

    public class AssetsDownloader : EditorWindow
    {

        [MenuItem("GB/Assets")]
        static void Init()
        {
            Load();
            EditorWindow.GetWindow(typeof(AssetsDownloader));

        }

        void OnEnable()
        {
            Load();
        }

        static List<GB.AssetData> _assets = new List<AssetData>();
        static Dictionary<string,string> _versions = new Dictionary<string, string>();

        static string _version= "1.0.0";

        static void Load()
        {
            string path = "Packages/com.gb.core/Runtime/Editor/";
            _assets = AssetDatabase.LoadAssetAtPath<TextAsset>(path+"GBAssetVersion.txt").text.FromJson<List<AssetData>>();
            _versions = new Dictionary<string, string>();

            _version = AssetDatabase.LoadAssetAtPath<TextAsset>(path+"Version.txt").text;


            for(int i = 0; i< _assets.Count; ++i)
            {
                if(_assets[i].Model!= null)
                {
                    var t = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GB/Version/"+_assets[i].Model.Key + ".txt");
                    if(t != null)
                    {
                        var data = t.text.FromJson<AssetData>();
                        _versions[data.Model.Key] = data.Version;
                    }
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 20, position.width - 20, position.height - 20));
            GB.EditorGUIUtil.DrawHeaderLabel("Assets");
            GB.EditorGUIUtil.DrawSectionStyleLabel("V "+_version);

            GB.EditorGUIUtil.BackgroundColor(Color.blue);

            GB.EditorGUIUtil.Start_VerticalBox();
            GB.EditorGUIUtil.DrawSectionStyleLabel("Download Assets");
            GB.EditorGUIUtil.End_Vertical();
            GB.EditorGUIUtil.BackgroundColor(Color.gray);


            GB.EditorGUIUtil.Start_HorizontalBox();
            GB.EditorGUIUtil.DrawStyleLabel("Name",GUILayout.Width(200));
            GB.EditorGUIUtil.DrawStyleLabel("InstallVersion",GUILayout.Width(100));
            GB.EditorGUIUtil.DrawStyleLabel("Version",GUILayout.Width(100));
            GB.EditorGUIUtil.DrawStyleLabel("Doc",GUILayout.Width(100));
            GB.EditorGUIUtil.DrawStyleLabel("Download",GUILayout.Width(150));
            GB.EditorGUIUtil.End_Horizontal();
            GB.EditorGUIUtil.BackgroundColor(Color.white);

            if (_assets != null)
            {
                GB.EditorGUIUtil.Start_VerticalBox();
                _scrollPos = GB.EditorGUIUtil.Start_ScrollView(_scrollPos);

                for (int i = 0; i < _assets.Count; ++i)
                {
                    bool isNewVersion = false;
                    if(_versions.ContainsKey(_assets[i].Model.Key)) isNewVersion =_versions[_assets[i].Model.Key] != _assets[i].Version;
                    
                    DrawDownloadButton(_assets[i], isNewVersion);
                    GB.EditorGUIUtil.BackgroundColor(Color.white);
                }

                GB.EditorGUIUtil.End_ScrollView();
                GB.EditorGUIUtil.End_Vertical();

            }


            GB.EditorGUIUtil.BackgroundColor(Color.gray);
            GB.EditorGUIUtil.Start_VerticalBox();
            GB.EditorGUIUtil.DrawSectionStyleLabel("Link SDK");
            GB.EditorGUIUtil.End_Vertical();
            GB.EditorGUIUtil.BackgroundColor(Color.white);

            GB.EditorGUIUtil.Start_VerticalBox();
            _linkScrollPos = GB.EditorGUIUtil.Start_ScrollView(_linkScrollPos);

            DrawLinkButton("Spine Unity SDK", Spine_URL);
            DrawLinkButton("Firebase Unity SDK", Firebase_URL);
            DrawLinkButton("Admob Unity SDK", ADMOB_URL);
            DrawLinkButton("Google Play Games plugin for Unity", GPGS_URL);
            DrawLinkButton("NHN Game Package Manager for Unity", NHNGamePackageManager_URL);
            GB.EditorGUIUtil.End_Vertical();
            GB.EditorGUIUtil.End_ScrollView();

            if(GB.EditorGUIUtil.DrawSyleButton("Update Assets Package"))
            {
                UpdatePackage();
            }

            GUILayout.EndArea();

        }

        private void UpdatePackage()
        {

            AddRequest request = UnityEditor.PackageManager.Client.Add("https://github.com/shogun0331/gbassets.git");
            while (!request.IsCompleted)
            {
                // 필요에 따라 진행 상황을 표시하거나 다른 작업을 수행할 수 있습니다.
                // 예: EditorUtility.DisplayProgressBar("패키지 추가 중...", request.Progress * 100, 100);
            }

            if (request.Status == StatusCode.Success)
            {
                Debug.Log("Package Add Success: " + request.Result.packageId);
                AssetDatabase.Refresh();

            }
            else
            {
                Debug.LogError("Package Add Fail!! : " + request.Error.message);
            }
        }



        Vector2 _scrollPos;
        Vector2 _linkScrollPos;

        void DrawDownloadButton(AssetData data, bool isNew)
        {
            if (data == null) return;
            if (data.Model == null) return;

            bool isInstall = false;

            GB.EditorGUIUtil.Start_Horizontal();
            GB.EditorGUIUtil.DrawStyleLabel(data.Model.Key,GUILayout.Width(200));
            if(_versions.ContainsKey(data.Model.Key))
            {
                GB.EditorGUIUtil.DrawStyleLabel("V"+_versions[data.Model.Key],GUILayout.Width(100));
                isInstall = true;
            }
            else
            {
                if(data.Model.PackageType == AssetModel.PACKAGE_TYPE.UNITY)
                GB.EditorGUIUtil.DrawStyleLabel("",GUILayout.Width(100));
                else
                GB.EditorGUIUtil.DrawStyleLabel("GIT",GUILayout.Width(100));
            }

            
            GB.EditorGUIUtil.DrawStyleLabel("V"+data.Version,GUILayout.Width(100));
            

            GB.EditorGUIUtil.BackgroundColor(Color.green);
            if (string.IsNullOrEmpty(data.DOC))
            {
                if (GB.EditorGUIUtil.DrawButton("Doc", GUILayout.Width(100))) Application.OpenURL("https://gb-framework.gitbook.io/gb-framework-docs");
            }
            else
            {
                if (GB.EditorGUIUtil.DrawButton("Doc", GUILayout.Width(100))) Application.OpenURL(data.DOC);
            }

            if (isNew)
            {
                GB.EditorGUIUtil.BackgroundColor(Color.yellow);
                if (GB.EditorGUIUtil.DrawButton("Update", GUILayout.Width(150))) data.Download(()=>{Load();Repaint();});
            } 
            else 
            {
                if(isInstall)
                {
                    GB.EditorGUIUtil.BackgroundColor(Color.cyan);
                    if (GB.EditorGUIUtil.DrawButton("ReDownload", GUILayout.Width(150))) data.Download(()=>{Load();Repaint();});
                    
                }
                else
                {
                    GB.EditorGUIUtil.BackgroundColor(Color.white);
                    if (GB.EditorGUIUtil.DrawButton("Download", GUILayout.Width(150))) data.Download(()=>{Load();Repaint();});
                }
                
            }

            
            GB.EditorGUIUtil.BackgroundColor(Color.white);
            GB.EditorGUIUtil.End_Horizontal();
        }

        void DrawLinkButton(string key, string url)
        {
            GB.EditorGUIUtil.Start_Horizontal();
            GB.EditorGUIUtil.DrawStyleLabel(key);
            GB.EditorGUIUtil.BackgroundColor(Color.green);
            if (GB.EditorGUIUtil.DrawButton("Link", GUILayout.Width(100))) Application.OpenURL(url);
            GB.EditorGUIUtil.BackgroundColor(Color.white);
            GB.EditorGUIUtil.End_Horizontal();
        }

        const string Spine_URL = "https://ko.esotericsoftware.com/spine-unity-download";
        const string Firebase_URL = "https://github.com/firebase/firebase-unity-sdk/releases";
        const string ADMOB_URL = "https://github.com/googleads/googleads-mobile-unity/releases";
        const string GPGS_URL = "https://github.com/playgameservices/play-games-plugin-for-unity";
        const string NHNGamePackageManager_URL = "https://github.com/nhn/gpm.unity?tab=readme-ov-file";


    }
}