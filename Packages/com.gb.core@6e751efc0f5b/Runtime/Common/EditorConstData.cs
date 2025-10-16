#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using QuickEye.Utility;
using System.IO;
using System.Reflection;
using System;

namespace GB
{

    public class EditorConstData : EditorWindow
    {
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
        GUIStyle TextFiledSyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.textField);
            style.fontSize = 12;
            return style;
        }
        GUIStyle ButtonSyle()
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 13;
            buttonStyle.margin = new RectOffset(5, 5, 10, 10);
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.padding = new RectOffset(10, 10, 5, 5);

            return buttonStyle;

        }

        private Vector2 scrollPosition;


        [MenuItem("GB/Core/ConstData")]
        static void init()
        {
            EditorWindow.GetWindow(typeof(EditorConstData));

        }

        void OnEnable()
        {
            Load();


        }

        const string DEF_CLASS = "public static class DEF\n{\n\t@@VALUE\n}";

        void Save()
        {
            string Dataform = "\tpublic const @TYPE @NAME = @VALUE;";
            string mergeData = string.Empty;

            foreach(var v in ListConstData)
            {
                string pname = v.Key;
                string ptype = "string";
                string pvalue = string.Empty;
                if(v.Value.P_TYPE == P_TYPE.INT)
                {
                    ptype = "int";
                    pvalue = v.Value.Value;
                }
                else if(v.Value.P_TYPE == P_TYPE.FLOAT)
                {
                    ptype = "float";
                    pvalue = v.Value.Value + "f";
                }
                else
                {
                    pvalue = "\""+ v.Value.Value+ "\"";
                }
                string prob = Dataform.Replace("@TYPE",ptype).Replace("@NAME",pname).Replace("@VALUE",pvalue);
                mergeData += prob + "\n";
            }

            string data = DEF_CLASS.Replace("@@VALUE",mergeData);

            string diPath = Application.dataPath+"/Scripts";
            DirectoryInfo info = new DirectoryInfo(diPath);
            if (info.Exists == false) info.Create();
            File.WriteAllText(diPath+"/DEF.cs", data);
            UnityEditor.AssetDatabase.Refresh();
        }

        void Load()
        {
            ListConstData = new UnityDictionary<string, ConstData>();
            System.Type myType = System.Type.GetType("DEF" + ",Assembly-CSharp");
            if (myType != null)
            {
                BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
                var fields = myType.GetFields(flags);
                foreach (FieldInfo field in fields)
                {
                    P_TYPE p_TYPE = P_TYPE.STRING;
                    if (field.FieldType == typeof(int))p_TYPE = P_TYPE.INT;
                    else if (field.FieldType == typeof(float))p_TYPE = P_TYPE.FLOAT;

                    var v = field.GetValue(null);

                    ListConstData[field.Name] = new ConstData
                    {
                        P_TYPE = p_TYPE,
                        Value = v.ToString()
                    };
                }
            }

        }


        public enum P_TYPE { STRING = 0, INT, FLOAT }
        UnityDictionary<string, ConstData> ListConstData = new UnityDictionary<string, ConstData>();

        string _Key;
        ConstData _ConstData = new ConstData();

        private void OnGUI()
        {
            var customHeaderStyle = CustomHeaderStyle();
            var customSectionStyle = CustomSectionStyle();
            var buttonStyle = ButtonSyle();

            GUILayout.BeginArea(new Rect(10, 20, position.width - 20, position.height - 20));

            GUILayout.BeginVertical("box");
            GUILayout.Label("GB CONST DATA", customHeaderStyle);
            GUILayout.EndVertical();

            GUILayout.Space(20);
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.8f, 1.0f);
            GUILayout.BeginVertical("box");
            GUILayout.Label("DEF.", customSectionStyle);
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;

            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = new Color(0f, 0f, 1f, 1.0f);
            EditorGUILayout.LabelField("Key", GUILayout.Width(150f));
            EditorGUILayout.LabelField("Type", GUILayout.Width(150f));
            EditorGUILayout.LabelField("Value", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            string removeData = null;


            foreach (var v in ListConstData)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(v.Key, GUILayout.Width(150));
                DrawDropdownBtn(v.Value);
                DrawTextField(v.Value);
                GUI.backgroundColor = new Color(1f, 0f, 0f, 1f);
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    removeData = v.Key;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            if (!string.IsNullOrEmpty(removeData)) ListConstData.Remove(removeData);

            GUILayout.EndScrollView();



            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = new Color(0f, 0f, 1f, 1.0f);
            EditorGUILayout.LabelField("Key", GUILayout.Width(150f));
            EditorGUILayout.LabelField("Type", GUILayout.Width(150f));
            EditorGUILayout.LabelField("Value", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;


            EditorGUILayout.BeginHorizontal();
            _Key = EditorGUILayout.TextField(_Key, GUILayout.Width(150f));
            DrawDropdownBtn(_ConstData);
            DrawTextField(_ConstData);

            GUI.backgroundColor = new Color(0f, 1f, 0f, 1f);
            if (GUILayout.Button("ADD", GUILayout.Width(50))) AddData();

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();



            GUI.backgroundColor = new Color(0f, 0.5f, 0.7f, 1f);

            if (GUILayout.Button("Create Scripts", buttonStyle))
            {
                Save();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndArea();

        }

        void AddData()
        {
            if (string.IsNullOrEmpty(_Key)) return;

            ListConstData[_Key] = new ConstData
            {
                P_TYPE = _ConstData.P_TYPE,
                Value = _ConstData.Value
            };

        }



        void DrawDropdownBtn(ConstData data)
        {
            GUIContent buttonContent = new GUIContent(data.P_TYPE.ToString());

            if (EditorGUILayout.DropdownButton(buttonContent, FocusType.Passive, GUILayout.Width(150f)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("STRING"), data.P_TYPE == P_TYPE.STRING, () => { data.P_TYPE = P_TYPE.STRING; });
                menu.AddItem(new GUIContent("INT"), data.P_TYPE == P_TYPE.INT, () => { data.P_TYPE = P_TYPE.INT; });
                menu.AddItem(new GUIContent("FLOAT"), data.P_TYPE == P_TYPE.FLOAT, () => { data.P_TYPE = P_TYPE.FLOAT; });
                menu.ShowAsContext();
            }
        }
        void DrawTextField(ConstData data)
        {

            switch (data.P_TYPE)
            {
                case P_TYPE.INT:
                    int intData = 0;
                    int.TryParse(data.Value, out intData);
                    intData = EditorGUILayout.IntField("", intData, GUILayout.ExpandWidth(true));
                    data.Value = intData.ToString();
                    break;

                case P_TYPE.FLOAT:
                    float floatData = 0;
                    float.TryParse(data.Value, out floatData);
                    floatData = EditorGUILayout.FloatField("", floatData, GUILayout.ExpandWidth(true));
                    data.Value = floatData.ToString();
                    break;

                default:
                    data.Value = EditorGUILayout.TextField(data.Value, GUILayout.ExpandWidth(true));
                    break;

            }

        }






        [System.Serializable]
        public class ConstData
        {
            public P_TYPE P_TYPE;
            public string Value;
        }
    }

}
#endif