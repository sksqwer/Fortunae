#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

namespace GB
{

    public class EditorODataBase : EditorWindow
    {
        [MenuItem("GB/Core/ODataBase")]
        static void init()
        {
            EditorWindow.GetWindow(typeof(EditorODataBase));
        }

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



        private Vector2 scrollPosition;

        private void OnGUI()
        {
            //  ------------------------------ Header ----------------------------------------------------------
            GUIStyle customHeaderStyle = CustomHeaderStyle();
            GUIStyle customSectionStyle = CustomSectionStyle();
            GUIStyle textstyle = TextStyle();
            


            GUILayout.BeginArea(new Rect(10, 20, position.width - 20, position.height - 20));


            GUILayout.BeginVertical("box");
            GUILayout.Label("GB OData Base", customHeaderStyle);
            GUILayout.EndVertical();

            
            GUILayout.Space(20);
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.8f, 1.0f);
            GUILayout.BeginVertical("box");
            GUILayout.Label("GAME DATAS", customSectionStyle);
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;



            if (!Application.isPlaying)
            {
                GUI.backgroundColor = new Color(1f, 0f, 0f, 1.0f);
                GUILayout.BeginVertical("box");
                GUILayout.Label("No Playing...", customHeaderStyle);
                GUILayout.EndVertical();
                GUI.backgroundColor = Color.white;
                GUILayout.EndArea();
                return;
            }


            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = new Color(0f, 0f, 1f, 1.0f);
            EditorGUILayout.LabelField("Key", GUILayout.Width(150f));
            EditorGUILayout.LabelField("Type",GUILayout.Width(150f));
            EditorGUILayout.LabelField("Value",GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach(var v in ODataBaseManager.I.DictDataType) 
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(v.Key, GUILayout.Width(150));
                EditorGUILayout.LabelField(v.Value.ToString(), GUILayout.Width(150));
                EditorGUILayout.LabelField(GetTypeData(v.Key,v.Value), GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private string GetTypeData(string key, Type type)
        {
            if(type == typeof(int))
                return ODataBaseManager.Get<int>(key).ToString();
            else if(type == typeof(float))
                return ODataBaseManager.Get<float>(key).ToString();
            else if(type == typeof(string))
                return ODataBaseManager.Get<string>(key);
            else if(type == typeof(double))
                return ODataBaseManager.Get<double>(key).ToString();
            else if(type == typeof(byte))
                return ODataBaseManager.Get<byte>(key).ToString();
            else if(type == typeof(long))
                return ODataBaseManager.Get<long>(key).ToString();
            else if(type == typeof(GameObject))
                return ODataBaseManager.Get<GameObject>(key).name.ToString();
            else if(type == typeof(Transform))
                return ODataBaseManager.Get<Transform>(key).gameObject.name.ToString();
            else if(type == typeof(Vector2))
                return ODataBaseManager.Get<Vector2>(key).ToString();
            else if(type == typeof(Vector3))
                return ODataBaseManager.Get<Vector3>(key).ToString();
            else if(type == typeof(bool))
                return ODataBaseManager.Get<bool>(key).ToString();
            else if(type == typeof(SpriteRenderer))
                return ODataBaseManager.Get<SpriteRenderer>(key).gameObject.name.ToString();
            else if(type == typeof(Rigidbody))
                return ODataBaseManager.Get<Rigidbody>(key).gameObject.name.ToString();
            else if(type == typeof(Rigidbody2D))
                return ODataBaseManager.Get<Rigidbody2D>(key).gameObject.name.ToString();
            else if(type == typeof(BoxCollider))
                return ODataBaseManager.Get<BoxCollider>(key).gameObject.name.ToString();
            else if(type == typeof(BoxCollider2D))
                return ODataBaseManager.Get<BoxCollider2D>(key).gameObject.name.ToString();
            else if(type == typeof(CircleCollider2D))
                return ODataBaseManager.Get<CircleCollider2D>(key).gameObject.name.ToString();
            else if(type == typeof(Image))
                return ODataBaseManager.Get<Image>(key).gameObject.name.ToString();
            else if(type == typeof(RectTransform))
                return ODataBaseManager.Get<RectTransform>(key).gameObject.name.ToString();
            

                
            return string.Empty;

        }
    }
}
#endif
