using System.Collections;
using UnityEngine;
using NaughtyAttributes;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace GB
{
    public class UICreate : MonoBehaviour
    {
#if UNITY_EDITOR
        public ScreenType ScreenType;
        public string UIName;

        const string EDITOR_SAVECS_PATH = "$PATH$/$FILENAME$";
        


        [Button]
        public void Bind()
        {
            GetComponent<UIScreen>().SetBind();
        }

        void CreateDirectory()
        {
            string savePath = Application.dataPath+"/Scripts/UI";
           
            DirectoryInfo info = new DirectoryInfo(EDITOR_SAVECS_PATH.Replace("$PATH$", savePath).Replace("$FILENAME$", ""));
            if (info.Exists == false)
                info.Create();

            info = new DirectoryInfo(Application.dataPath + "/Resources/UI/Scene");
            if (info.Exists == false)
                info.Create();
            info = new DirectoryInfo(Application.dataPath + "/Resources/UI/Popup");
            if (info.Exists == false)
                info.Create();

        }

        [Button]
        public void Generate()
        {
            if (GetComponent<UIScreen>() != null) return;
            if (string.IsNullOrEmpty(UIName)) return;

            CreateDirectory();

            string savePath = Application.dataPath+"/Scripts/UI";

            string text = Resources.Load<TextAsset>("UIText").text;
            text = text.Replace("$POPUPNAME$", UIName);


            WriteTxt(EDITOR_SAVECS_PATH.Replace("$PATH$", savePath).Replace("$FILENAME$", UIName + ".cs"), text);

            AssetDatabase.Refresh();
            GB.Edit.EditorCoroutines.StartCoroutine(settingCorutine(), this);

        }
        IEnumerator settingCorutine()
        {
            yield return new WaitForSeconds(1);
            
            FirstSetting();
         
            
        }

        public void FirstSetting()
        {
            Setting(false);
        }
        
        [Button("Setting")]
        public void PopupNameSetting()
        {
            Setting(true);
        }

        
        
        public void Setting(bool isPopupNameSetting)
        {
            
            if (string.IsNullOrEmpty(UIName)) return;
            if (GetComponent<UIScreen>() == null)
            {
                //We need to fetch the Type
                System.Type MyScriptType = System.Type.GetType(UIName + ",Assembly-CSharp");
                //Now that we have the Type we can use it to Add Component
                var component = gameObject.AddComponent(MyScriptType);

                gameObject.GetComponent<UIScreen>().SetScreenType(ScreenType);
            }

            if (isPopupNameSetting)
            {
                
                var textAsset = Resources.Load<TextAsset>("Popup");


                string temp = "\tpublic const string ##NAME =  \"##NAME\"; \n";
                string propertis = string.Empty;


                UIScreen[] screens = Resources.LoadAll<UIScreen>("UI/Popup");

                for (int i = 0; i < screens.Length; ++i)
                {
                    if (screens[i].UIType == ScreenType.POPUP)
                    {
                        propertis += temp.Replace("##NAME", screens[i].gameObject.name);
                    }
                }

                string value = textAsset.text.Replace("##POPUP", propertis);
                string savePath = Application.dataPath + "/Scripts/UI";
                WriteTxt(EDITOR_SAVECS_PATH.Replace("$PATH$", savePath).Replace("$FILENAME$", "POPUP.cs"), value);
                AssetDatabase.Refresh();
             
            }




        }

        // public string AnimationName;

        // [Button]
        // public void CreateAnimation()
        // {
        //     if(string.IsNullOrEmpty( UIName)) return;
        //     if(string.IsNullOrEmpty( AnimationName)) return;

        //     if(GetComponent<UIScreen>() == null) return;
        //     if(GetComponent<Animation>() == null) gameObject.AddComponent<Animation>();
            
        //     var obj = Resources.Load<AnimationClip>("UIAnimation");

        //     string oriPath = AssetDatabase.GetAssetPath(obj);
        //     string directory = GetComponent<UIScreen>().UIType == ScreenType.SCENE ? "Scene":"Popup";
        //     string p = Application.dataPath+"/"+EDITOR_SAVECS_PATH.Replace("$PATH$", "Animation/UI/"+ directory).Replace("$FILENAME$", UIName);

        //     DirectoryInfo info = new DirectoryInfo(p);
        //     if (info.Exists == false) info.Create();

        //     string path = "Assets/Animation/UI/"+ directory+"/" +  UIName+"/"+ AnimationName+ ".anim";
        //     bool isSuccess = AssetDatabase.CopyAsset(oriPath, path);  
        //     AssetDatabase.Refresh();

        //     LoadAnimation();
            
        //     EditorApplication.ExecuteMenuItem("Window/Animation/Animation"); 

        // }
        // [Button]
        // void LoadAnimation()
        // {

        //     string directory = GetComponent<UIScreen>().UIType == ScreenType.SCENE ? "Scene":"Popup";
        //     string p = Application.dataPath+"/"+EDITOR_SAVECS_PATH.Replace("$PATH$", "Animation/UI/"+ directory).Replace("$FILENAME$", UIName);

        //     DirectoryInfo info = new DirectoryInfo(p);
        //     if (info.Exists == false) return;

            
        //     List<string> fileList = new List<string>();

        //     foreach (System.IO.FileInfo File in info.GetFiles())
        //     {
        //         if (File.Extension.ToLower().CompareTo(".anim") == 0)
        //         {
        //             string FullFileName = File.FullName;
        //             int len = FullFileName.Length - Application.dataPath.Length;
        //             fileList.Add("Assets" + FullFileName.Substring(Application.dataPath.Length, len));
        //         }
        //     }

        //     List<AnimationClip> animList = new List<AnimationClip>();

        //     for (int i = 0; i < fileList.Count; ++i)
        //     {
        //         UnityEngine.Object[] data = AssetDatabase.LoadAllAssetsAtPath(fileList[i]);
        //         foreach (UnityEngine.Object v in data)
        //         {
        //             if (v.GetType() == typeof(AnimationClip))
        //             {
        //                 animList.Add((AnimationClip)v);
        //             }
        //         }
        //     }
            
            
        //     DestroyImmediate(GetComponent<Animation>());
        //     GetComponent<UIScreen>().ClearAnim();

        //     var anim = gameObject.AddComponent<Animation>();
        //     for(int i = 0; i< animList.Count; ++i)
        //     {
        //         anim.AddClip(animList[i],animList[i].name);
        //         GetComponent<UIScreen>().Add(animList[i].name,animList[i]);
        //     }
        // }


        [Button]
        public void QuickSelect()
        {
            if(string.IsNullOrEmpty( UIName)) return;
            if(GetComponent<UIScreen>() == null) return;
            string directory = GetComponent<UIScreen>().UIType == ScreenType.SCENE ? "Scene/":"Popup/";
            var obj = Resources.Load<GameObject>("UI/"+directory+UIName);
            if(obj != null)
            Selection.activeGameObject = obj;

        }

        


        [Button]
        public void Save()
        {
            if(string.IsNullOrEmpty( UIName)) return;
            if(GetComponent<UIScreen>() == null) return;
            CreateDirectory();

            string fileName = UIName;

            string directory = GetComponent<UIScreen>().UIType == ScreenType.SCENE ? "Scene/":"Popup/";

            string path = "Assets/Resources/UI/"+ directory + fileName + ".prefab";
            bool isSuccess = false;
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(gameObject, path, out isSuccess); //저장
            UnityEditor.AssetDatabase.Refresh();
        }


        void WriteTxt(string filePath, string message)
        {
            File.WriteAllText(filePath, message);

        }

        string ReadTxt(string filePath)
        {
            return File.ReadAllText(filePath);
        }

#endif

    }

}

