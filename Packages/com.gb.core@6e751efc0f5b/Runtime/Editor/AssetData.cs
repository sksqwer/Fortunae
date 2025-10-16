using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Networking;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;

namespace GB
{
    [Serializable]
    public class AssetData 
    {
        public string Version;
        public string DOC;
        public AssetModel Model;
        public List<AssetModel> DependencyList;

        public void Download(Action onComplete = null)
        {
            if(DependencyList != null && DependencyList.Count > 0)
            {
                for(int i = 0; i< DependencyList.Count; ++i)
                {
                    if( DependencyList[i].PackageType  == AssetModel.PACKAGE_TYPE.GIT)
                        DownloadURLPackage(DependencyList[i].URL, null);
                    else 
                        DownloadUnityPackage(DependencyList[i].URL, DependencyList[i].Key, null);
                }
            }

            if(Model.PackageType == AssetModel.PACKAGE_TYPE.GIT)
                DownloadURLPackage(Model.URL, onComplete);
            else
                DownloadUnityPackage(Model.URL, Model.Key, onComplete);
        }

        private void DownloadURLPackage(string url, Action onComplete)
        {
            AddRequest request = UnityEditor.PackageManager.Client.Add(url);
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

            onComplete?.Invoke();
        }

        private void DownloadUnityPackage(string url, string fileName, Action onComplete)
        {
            string downloadUrl = url;

            using (var www = UnityWebRequest.Get(downloadUrl))
            {
                www.SendWebRequest();

                while (!www.isDone)
                {
                    Debug.Log("Downloading: " + www.downloadProgress * 100 + "%");
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string savePath = Path.Combine(Application.dataPath, fileName + ".unitypackage");
                    File.WriteAllBytes(savePath, www.downloadHandler.data);
                    Debug.Log("Package downloaded to: " + savePath);

                    AssetDatabase.ImportPackage(savePath, true); // 유니티 프로젝트에 임포트

                    // 파일 삭제
                    File.Delete(savePath);
                    AssetDatabase.Refresh(); // 유니티 에디터에 변경 사항 반영
                }
                else
                {
                    Debug.LogError("Download failed: " + www.error);
                }
            }
            onComplete?.Invoke();
        }
    }

    [Serializable]
    public class AssetModel
    {
        public string Key;
        public enum PACKAGE_TYPE { UNITY = 0, GIT }
        public string URL;
        
        public PACKAGE_TYPE PackageType = PACKAGE_TYPE.UNITY;
        
    }
}