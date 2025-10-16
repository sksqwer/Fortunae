#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace GB
{

    public static class EditorGUIUtil 
    {
        public static void BackgroundColor(Color color){GUI.backgroundColor = color;}
        public static void Space(float pixel){GUILayout.Space(pixel);}
        public static Vector2 Start_ScrollView(Vector2 scrollPosition){return GUILayout.BeginScrollView(scrollPosition);}
        public static void End_ScrollView(){GUILayout.EndScrollView();}
        public static void Start_Vertical(){GUILayout.BeginVertical();}
        public static void Start_VerticalBox(){GUILayout.BeginVertical("box");}
        public static void Start_Horizontal(){GUILayout.BeginHorizontal();}
        public static void Start_HorizontalBox(){GUILayout.BeginHorizontal("box");}
        public static void End_Vertical(){GUILayout.EndVertical();}
        public static void End_Horizontal() {GUILayout.EndHorizontal();}
        public static bool DrawButton(string key){ return GUILayout.Button(key);}
        public static bool DrawButton(string key,GUILayoutOption width){ return GUILayout.Button(key, width);}
        public static bool DrawSyleButton(string key){ return GUILayout.Button(key, ButtonSyle());}
        public static bool DrawSyleButton(string key,GUILayoutOption width){ return GUILayout.Button(key, ButtonSyle(),width);}
        public static string DrawTextField(string key,string value){return EditorGUILayout.TextField(key, value, GUILayout.ExpandWidth(true));}
        public static string DrawTextField(string key,string value,GUILayoutOption width){return EditorGUILayout.TextField(key, value, width);}
        public static string DrawSyleTextField(string key,string value){return EditorGUILayout.TextField(key, value, TextFiledSyle(), GUILayout.ExpandWidth(true));}
        public static string DrawSyleTextField(string key,string value,GUILayoutOption width){return EditorGUILayout.TextField(key, value, TextFiledSyle(), width);}
        public static void DrawLabel(string text){GUILayout.Label(text);}
        public static void DrawLabel(string text,GUILayoutOption width){GUILayout.Label(text, width);}
        public static void DrawStyleLabel(string text){GUILayout.Label(text, TextStyle());}
        public static void DrawSectionStyleLabel(string text){GUILayout.Label(text, CustomSectionStyle());}
        public static void DrawStyleLabel(string text,GUILayoutOption width){GUILayout.Label(text, TextStyle(),width);}
        public static void DrawHeaderLabel(string text){GUILayout.Label(text, CustomHeaderStyle());}


        
        public static E DrawDropdown<E>(E state) where E: System.Enum
        {
            string[] names  = System.Enum.GetNames(typeof(E));

            GUIContent buttonContent = new GUIContent(state.ToString());

            if (EditorGUILayout.DropdownButton(buttonContent, FocusType.Passive, GUILayout.Width(150f)))
            {
                GenericMenu menu = new GenericMenu();
                for(int i = 0; i< names.Length; ++i)
                    menu.AddItem(new GUIContent(names[i]), state.ToString() == names[i], () => { state = (E)System.Enum.Parse(typeof(E), names[i]); });
                
                menu.ShowAsContext();
            }

            return state;
        }

        public static string DrawDropdown(string state,string[] states)
        {
            string[] names  = states;

            GUIContent buttonContent = new GUIContent(state.ToString());

            if (EditorGUILayout.DropdownButton(buttonContent, FocusType.Passive, GUILayout.Width(150f)))
            {
                GenericMenu menu = new GenericMenu();
                for(int i = 0; i< names.Length; ++i)
                    menu.AddItem(new GUIContent(names[i]), state == names[i], () => { state = names[i]; });
                
                menu.ShowAsContext();
            }

            return state;
        }

        public static GUIStyle CustomHeaderStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 50;
            style.alignment = TextAnchor.MiddleCenter;
            style.margin = new RectOffset(0, 0, 0, 30);
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        static GUIStyle CustomSectionStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 18;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        static GUIStyle TextStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 12;
            return style;
        }
        static GUIStyle TextFiledSyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.textField);
            style.fontSize = 12;
            return style;
        }
        static GUIStyle ButtonSyle()
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 13;
            buttonStyle.margin = new RectOffset(5, 5, 10, 10);
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.padding = new RectOffset(10, 10, 5, 5);

            return buttonStyle;
        }
    }

}

#endif