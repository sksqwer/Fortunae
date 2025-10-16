#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

namespace GB
{

    public class EditorUIManager : EditorWindow
    {
        // Start is called before the first frame update
        [MenuItem("GB/UI/UIManager")]
        static void init()
        {

            EditorWindow.GetWindow(typeof(EditorUIManager));
        }

        private void OnEnable()
        {
            Load();
        }

        void Load()
        {
            var popups = Resources.LoadAll<UIScreen>("UI/Popup/");
            _popupList = new GameObject[popups.Length];
            for (int i = 0; i < popups.Length; ++i)
                _popupList[i] = popups[i].gameObject;

            var scenes = Resources.LoadAll<UIScreen>("UI/Scene/");
            _sceneList = new GameObject[scenes.Length];
            for (int i = 0; i < scenes.Length; ++i)
                _sceneList[i] = scenes[i].gameObject;

            _canvas = GameObject.Find("UI");
        }

        GameObject _canvas;


        GameObject[] _popupList;
        GameObject[] _sceneList;

        GUIStyle CustomHeaderStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 50;
            style.alignment = TextAnchor.MiddleCenter;
            style.margin = new RectOffset(0, 0, 0, 30);
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        GUIStyle CustomSectionStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 18;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        GUIStyle TextStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 12;
            return style;
        }

        GUIStyle ButtonStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fontSize = 13;
            style.margin = new RectOffset(5, 5, 10, 10);
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.padding = new RectOffset(10, 10, 5, 5);
            return style;

        }

        private Vector2 popupScrollPosition;
        private Vector2 sceneScrollPosition;

        private void OnGUI()
        {
            GUIStyle customHeaderStyle = CustomHeaderStyle();
            GUIStyle customSectionStyle = CustomSectionStyle();
            GUIStyle textstyle = TextStyle();
            GUIStyle buttonStyle = ButtonStyle();



            GUILayout.BeginArea(new Rect(10, 20, position.width - 20, position.height - 20));


            GUILayout.BeginVertical("box");
            GUILayout.Label("GB UIManager", customHeaderStyle);
            GUILayout.EndVertical();

            //=========================POPUP===============================

            GUILayout.Space(20);
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.8f, 1.0f);
            GUILayout.BeginVertical("box");
            GUILayout.Label("POPUP LIST", customSectionStyle);
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;


            popupScrollPosition = GUILayout.BeginScrollView(popupScrollPosition);

            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = new Color(0f, 0f, 1f, 1.0f);
            EditorGUILayout.LabelField("PopupName", GUILayout.Width(300f));
            EditorGUILayout.LabelField("Link", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            if (_popupList != null)
            {
                for (int i = 0; i < _popupList.Length; ++i)
                {
                    if (_popupList[i] != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(_popupList[i].gameObject.name, GUILayout.Width(300f));
                        GUI.backgroundColor = new Color(0f, 0f, 1f, 1f);

                        if (GUILayout.Button("Link", GUILayout.Width(100))) Selection.activeGameObject = _popupList[i];
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();

                    }

                }

            }

            GUILayout.EndScrollView();



            //=============================================================



            GUILayout.Space(20);
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.8f, 1.0f);
            GUILayout.BeginVertical("box");
            GUILayout.Label("SCENE LIST", customSectionStyle);
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;

            sceneScrollPosition = GUILayout.BeginScrollView(sceneScrollPosition);

            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = new Color(0f, 0f, 1f, 1.0f);
            EditorGUILayout.LabelField("SCENE Name", GUILayout.Width(300f));
            EditorGUILayout.LabelField("Link", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();



            if (_popupList != null)
            {
                for (int i = 0; i < _sceneList.Length; ++i)
                {
                    if (_sceneList[i] != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(_sceneList[i].gameObject.name, GUILayout.Width(300f));
                        GUI.backgroundColor = new Color(0f, 0f, 1f, 1f);

                        if (GUILayout.Button("Link", GUILayout.Width(100))) Selection.activeGameObject = _sceneList[i];
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();

                    }

                }

            }

            GUILayout.EndScrollView();




            if (_canvas == null)
            {
                if (GUILayout.Button("Create Canvas", buttonStyle, GUILayout.ExpandWidth(true)))
                {
                    CreateCanvas();
                    Load();
                }
            }
            else
            {
                GUI.backgroundColor = new Color(0f, 0f, 1f, 1f);
                if (GUILayout.Button("Create POPUP", buttonStyle, GUILayout.ExpandWidth(true)))
                {
                    CreatePopup();
                }

                GUI.backgroundColor = new Color(0f, 1f, 0f, 1f);

                if (GUILayout.Button("Create Scene", buttonStyle, GUILayout.ExpandWidth(true)))
                {
                    CreateScene();

                }
                GUI.backgroundColor = Color.white;

            }


            GUI.backgroundColor = new Color(0f, 0.5f, 1f, 1f);

            if (GUILayout.Button("Refresh", buttonStyle, GUILayout.ExpandWidth(true)))
            {
                Load();
            }



            GUILayout.EndArea();


        }

        void CreateCanvas()
        {
            var ui = GameObject.Find("UI");
            if (ui == null)
            {
                GameObject prefab = Resources.Load<GameObject>("UI");
                if (prefab != null)
                {
                    var oj = Instantiate(prefab, null);
                    oj.name = "UI";
                    Selection.activeGameObject = oj;
                }
            }

        }

        void CreatePopup()
        {
            var popup = GameObject.Find("UIPopup");
            if (popup != null)
            {
                GameObject prefab = Resources.Load<GameObject>("Screen");
                if (prefab != null)
                {
                    var oj = Instantiate(prefab, popup.transform);
                    oj.name = "Popup";
                    oj.GetComponent<UICreate>().ScreenType = ScreenType.POPUP;
                    Selection.activeGameObject = oj;
                }
            }
        }

        void CreateScene()
        {
            var popup = GameObject.Find("UIScreen");
            if (popup != null)
            {
                GameObject prefab = Resources.Load<GameObject>("Screen");
                if (prefab != null)
                {
                    var oj = Instantiate(prefab, popup.transform);
                    oj.name = "Scene";
                    oj.GetComponent<UICreate>().ScreenType = ScreenType.SCENE;
                    Selection.activeGameObject = oj;
                }
            }
        }




    }

}
#endif