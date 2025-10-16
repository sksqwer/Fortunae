using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro;






#if UNITY_EDITOR
using UnityEditor;
using GB.Edit;

namespace GB
{
    [CustomEditor(typeof(UICreate))]
    public class EditorUICreate : Editor
    {
        void OnEnable()
        {
            var t = (UICreate)target;
            GetLoadList(t.gameObject);
        }



        static string _selectionMenu = "Buttons";
        private GameObject myFileObject;

        List<UIRegister> _list = new List<UIRegister>();


        List<string> _listKeys = new List<string>();



        void GetLoadList(GameObject myTarget)
        {
            _list.Clear();
            _listKeys.Clear();


            switch (_selectionMenu)
            {
                case "Buttons":
                    var btns = myTarget.GetComponentsInChildren<RegistButton>(true);
                    for (int i = 0; i < btns.Length; ++i) _list.Add(btns[i]);
                    break;

                case "Images":
                    var imgs = myTarget.GetComponentsInChildren<RegistImage>(true);
                    for (int i = 0; i < imgs.Length; ++i) _list.Add(imgs[i]);
                    break;

                case "Texts":
                    var texts = myTarget.GetComponentsInChildren<RegistText>(true);
                    for (int i = 0; i < texts.Length; ++i) _list.Add(texts[i]);
                    break;

                case "Skinners":
                    var skinners = myTarget.GetComponentsInChildren<RegistSkinner>(true);
                    for (int i = 0; i < skinners.Length; ++i) _list.Add(skinners[i]);
                    break;
                
                case "TMP_Text":
                    var tmpTexts = myTarget.GetComponentsInChildren<RegistTMPText>(true);
                    for (int i = 0; i < tmpTexts.Length; ++i) _list.Add(tmpTexts[i]);
                    break;

                case "GameObjects":
                    var gameObjects = myTarget.GetComponentsInChildren<RegistGameObject>(true);
                    for (int i = 0; i < gameObjects.Length; ++i) _list.Add(gameObjects[i]);
                    break;

            }

            for (int i = 0; i < _list.Count; ++i) _listKeys.Add(_list[i].Key);

        }





        Vector2 _scrollPos;




        public override void OnInspectorGUI()
        {
            var t = (UICreate)target;
            if (t.GetComponent<UIScreen>() == null)
                GB.EditorGUIUtil.DrawHeaderLabel("UICreate");
            else
                GB.EditorGUIUtil.DrawHeaderLabel(t.UIName);

            if (t.GetComponent<UIScreen>() == null)
            {
                base.OnInspectorGUI();

                if (GB.EditorGUIUtil.DrawSyleButton("Create Script"))
                {
                    if (!string.IsNullOrEmpty(t.UIName))
                        t.Generate();
                }

                if (GB.EditorGUIUtil.DrawSyleButton("AddComponent"))
                {
                    if (!string.IsNullOrEmpty(t.UIName))
                    {
                        t.Setting(true);
                    }
                }

                return;
            }

            //     myFileObject = t.gameObject;
            //   myFileObject = EditorGUILayout.ObjectField("", myFileObject, typeof(GameObject),false) as GameObject;

            GB.EditorGUIUtil.Start_VerticalBox();

            GB.EditorGUIUtil.Start_Horizontal();

            if (string.Equals(_selectionMenu, "Buttons")) GB.EditorGUIUtil.BackgroundColor(Color.blue);


            if (GB.EditorGUIUtil.DrawSyleButton("Buttons"))
            {
                _selectionMenu = "Buttons";
                GetLoadList(t.gameObject);

            }

            GB.EditorGUIUtil.BackgroundColor(Color.white);


            if (string.Equals(_selectionMenu, "Images")) GB.EditorGUIUtil.BackgroundColor(Color.blue);

            if (GB.EditorGUIUtil.DrawSyleButton("Images"))
            {
                _selectionMenu = "Images";
                GetLoadList(t.gameObject);

            }
            GB.EditorGUIUtil.BackgroundColor(Color.white);

            if (string.Equals(_selectionMenu, "Texts")) GB.EditorGUIUtil.BackgroundColor(Color.blue);

            if (GB.EditorGUIUtil.DrawSyleButton("Texts"))
            {
                _selectionMenu = "Texts";
                GetLoadList(t.gameObject);
            }

            GB.EditorGUIUtil.BackgroundColor(Color.white);

            if (string.Equals(_selectionMenu, "Skinners")) GB.EditorGUIUtil.BackgroundColor(Color.blue);

            if (GB.EditorGUIUtil.DrawSyleButton("Skinners"))
            {
                _selectionMenu = "Skinners";
                GetLoadList(t.gameObject);
            }
            GB.EditorGUIUtil.BackgroundColor(Color.white);


            if (string.Equals(_selectionMenu, "GameObjects")) GB.EditorGUIUtil.BackgroundColor(Color.blue);

            if (GB.EditorGUIUtil.DrawSyleButton("GameObjects"))
            {
                _selectionMenu = "GameObjects";
                GetLoadList(t.gameObject);

            }

            GB.EditorGUIUtil.BackgroundColor(Color.white);

            
            if (string.Equals(_selectionMenu, "TMP_Text")) GB.EditorGUIUtil.BackgroundColor(Color.blue);

            if (GB.EditorGUIUtil.DrawSyleButton("TMP_Text"))
            {
                _selectionMenu = "TMP_Text";
                GetLoadList(t.gameObject);
            }
            GB.EditorGUIUtil.BackgroundColor(Color.white);



            GB.EditorGUIUtil.End_Horizontal();

            _scrollPos = GB.EditorGUIUtil.Start_ScrollView(_scrollPos);
            GB.EditorGUIUtil.BackgroundColor(Color.gray);
            GB.EditorGUIUtil.Start_HorizontalBox();
            EditorGUILayout.LabelField("Key", GUILayout.Width(150f));
            EditorGUILayout.LabelField(_selectionMenu);
            GB.EditorGUIUtil.End_Horizontal();
            GB.EditorGUIUtil.BackgroundColor(Color.white);

            HashSet<string> hashKeys = new HashSet<string>();
            for (int i = 0; i < _list.Count; ++i)
            {
                if (_list[i] == null) continue;
                bool isDuplicate = false;

                if (!hashKeys.Contains(_list[i].Key) && !string.IsNullOrEmpty(_list[i].Key)) hashKeys.Add(_list[i].Key);
                else
                {
                    isDuplicate = true;
                    GB.EditorGUIUtil.BackgroundColor(Color.red);
                }

                GB.EditorGUIUtil.Start_HorizontalBox();

                _listKeys[i] = GB.EditorGUIUtil.DrawTextField("", _listKeys[i], GUILayout.Width(150f));
                _list[i].Key = _listKeys[i];
                var oj = EditorGUILayout.ObjectField("", _list[i].gameObject, typeof(GameObject), false) as GameObject;
                if (!Application.isPlaying)
                {
                    GB.EditorGUIUtil.BackgroundColor(Color.red);

                    if (GB.EditorGUIUtil.DrawButton("REMOVE"))
                    {
                        DestroyImmediate(_list[i]);
                        GetLoadList(t.gameObject);
                        GB.EditorGUIUtil.End_Horizontal();
                        GB.EditorGUIUtil.BackgroundColor(Color.white);
                        break;
                    }
                }

                GB.EditorGUIUtil.End_Horizontal();
                GB.EditorGUIUtil.BackgroundColor(Color.white);
            }


            GB.EditorGUIUtil.End_ScrollView();
            GB.EditorGUIUtil.End_Vertical();

            if (!Application.isPlaying)
            {

                GB.EditorGUIUtil.BackgroundColor(Color.green);
                if (GB.EditorGUIUtil.DrawSyleButton("Save"))
                {
                    t.Bind();
                    t.Save();
                    t.Setting(true);
                }
                GB.EditorGUIUtil.BackgroundColor(Color.white);
            }

            GB.EditorGUIUtil.Start_Horizontal();


            if (GB.EditorGUIUtil.DrawSyleButton("Open Script"))
            {
                string scriptPath = "Assets/Scripts/UI/" + t.UIName + ".cs"; // 열고 싶은 스크립트의 경로
                UnityEngine.Object scriptAsset = AssetDatabase.LoadAssetAtPath(scriptPath, typeof(MonoScript));
                if (scriptAsset != null)
                    AssetDatabase.OpenAsset(scriptAsset);
                else
                    Debug.LogWarning($"Script not found at path: {scriptPath}");

            }

            if (GB.EditorGUIUtil.DrawSyleButton("Quick Selection"))
            {
                t.QuickSelect();
            }

            GB.EditorGUIUtil.End_Horizontal();

        }
    }
}
#endif