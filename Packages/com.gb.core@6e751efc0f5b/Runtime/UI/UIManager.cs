using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace GB
{

    public class UIManager : AutoSingleton<UIManager>
    {

        RectTransform _Canvas;
        public RectTransform Canvas
        {
            get
            {
                if (_Canvas == null)
                {
                    var obj = GameObject.Find("UI");
                    if (obj != null)
                        _Canvas = obj.GetComponent<RectTransform>();
                }

                return _Canvas;
            }
        }


        [SerializeField] string _parentPopupName = "UIPopup";


        [SerializeField] string _PopupPath = "UI/Popup";

        private Dictionary<string, UIScreen> _UIScreenList = new Dictionary<string, UIScreen>();
        [SerializeField] private UIScreen _scene; // null로 초기화 (Inspector에서 할당)
        [SerializeField] List<UIScreen> _popupList = new List<UIScreen>();

        Transform _popupParent;
        public Transform PopupParent
        {
            get
            {
                if (_popupParent == null)
                {

                    var g = GameObject.Find(_parentPopupName);
                    if (g != null) _popupParent = g.transform;
                }
                return _popupParent;

            }

        }

        public int PopupCount { get { return _popupList.Count; } }

        Dictionary<string, GameObject> _PopupPrefabs = new Dictionary<string, GameObject>();


        private void Awake()
        {

            if (I != null && I != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Init();
            DontDestroyOnLoad(this.gameObject);
        }

        public void Init()
        {

            var popups = Resources.LoadAll<GameObject>(_PopupPath);
            for (int i = 0; i < popups.Length; ++i) _PopupPrefabs[popups[i].name] = popups[i];

            //Popup Regist
            Transform popupParent = PopupParent;
            if (popupParent == null) return;


            //UIScreen Regist
            UIScreen[] allChildren = GetComponentsInChildren<UIScreen>(true);
            int len = allChildren.Length;
            for (int i = 0; i < len; ++i)
            {
                allChildren[i].Initialize();
                if (allChildren[i].UIType == ScreenType.POPUP) allChildren[i].gameObject.SetActive(false);
            }


        }


   
        public void Clear()
        {
            _UIScreenList.Clear();
            _scene = null;
            _popupList.Clear();
            TimerManager.I.Clear();

        }


     
        public static void ChangeScene(string sceneName)
        {
            I.Clear();

            SceneManager.LoadScene(sceneName);
        }

      
        public void RegistUIScreen(UIScreen UIScreen)
        {
            if (_UIScreenList.ContainsKey(UIScreen.gameObject.name))
                return;

            _UIScreenList.Add(UIScreen.gameObject.name, UIScreen);

            if (UIScreen.UIType == ScreenType.SCENE)
                _scene = UIScreen;
        }

     
        public static void Refresh(string name)
        {
            if (I._UIScreenList.ContainsKey(name))
            {
                I._UIScreenList[name].Refresh();
            }
            else
            {
                if (I._scene != null)
                {
                    if (string.Equals(I._scene.name, name))
                        I._scene.Refresh();
                }
            }
        }


 
        public static void RefreshAll()
        {

            if (I._scene != null)
                I._scene.Refresh();

            foreach (var v in I._UIScreenList)
                I._UIScreenList[v.Key].Refresh();
        }

        public static UIScreen Find(string name)
        {
            UIScreen screen = null;
            if (I._UIScreenList.ContainsKey(name))
                screen =  I._UIScreenList[name];

            if(screen != null && !screen.gameObject.activeSelf) screen = null;

            return screen;
        }

   
        public static UIScreen FindUIScreen(string name)
        {
            if (I._UIScreenList.ContainsKey(name))
                return I._UIScreenList[name];

            return null;
        }

        public static void ShowPopup(string name, bool priority = true)
        {
            I.SortingPopup();

            if (I._popupList.Count > 0)
            {

                if (I._popupList[0] == null)
                {
                    I._popupList.RemoveAt(0);
                    ShowPopup(name, priority);
                    return;
                }
            }

            if (I._UIScreenList.ContainsKey(name))
            {
                I._UIScreenList[name].gameObject.SetActive(true);
                if (!I._popupList.Contains(I._UIScreenList[name])) I._popupList.Add(I._UIScreenList[name]);
            }
            else
            {
                I.LoadFromResources(name, true);
            }

            if (priority)
                I._UIScreenList[name].GetComponent<RectTransform>().SetAsLastSibling();
            else
                I._popupList[0].GetComponent<RectTransform>().SetAsLastSibling();

            
            I.SortingPopup();

        }
        public static void SetData(string name, object data)
        {
            var screen = FindUIScreen(name);
            if (screen != null) screen.SetData(data);
        }


        public static void ShowPopup(string name, object data)
        {
            I.SortingPopup();

            if (I._popupList.Count > 0)
            {

                if (I._popupList[0] == null)
                {
                    I._popupList.RemoveAt(0);
                    ShowPopup(name);
                    return;
                }
            }

            if (I._UIScreenList.ContainsKey(name))
            {
                I._UIScreenList[name].gameObject.SetActive(true);

                if (!I._popupList.Contains(I._UIScreenList[name]))
                    I._popupList.Add(I._UIScreenList[name]);

                I._UIScreenList[name].GetComponent<RectTransform>().SetAsLastSibling();
                I.SortingPopup();
                I._UIScreenList[name].SetData(data);
            }
            else
            {
                var screen = I.LoadFromResources(name, true);
                screen.SetData(data);
            }
        }

        private UIScreen LoadFromResources(string name, bool isPopup = false)
        {

            GameObject UIScreen = null;

            if (isPopup)
            {
                if (_PopupPrefabs.ContainsKey(name))
                    UIScreen = _PopupPrefabs[name];
                else
                    Debug.LogError("None Popup " + name);
            }
            else
            {
                UIScreen = Resources.Load<GameObject>(string.Format("{0}/{1}", _PopupPath, name));
            }

            if (UIScreen == null)
            {
                Debug.LogError(string.Format("can not load UI '{0}'", name));
                return null;
            }

            UIScreen = Instantiate(UIScreen);
            UIScreen.name = name;
            UIScreen.transform.SetParent(PopupParent);

            // reset transform info
            UIScreen.GetComponent<RectTransform>().localScale = Vector3.one;
            UIScreen.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            UIScreen.GetComponent<RectTransform>().offsetMin = Vector2.zero;

            _UIScreenList.Add(name, UIScreen.GetComponent<UIScreen>());
            if (isPopup)
            {
                _popupList.Add(UIScreen.GetComponent<UIScreen>());
            }

            // SortingPopup();
            return UIScreen.GetComponent<UIScreen>();
        }


        void OnBackKey()
        {
            if (_popupList.Count > 0)
            {
                _popupList[0].BackKey();
            }
            else
            {
                if (_scene != null)
                    _scene.BackKey();
            }
        }

        public static void ClosePopup()
        {

            if (I._popupList.Count > 0)
            {
                UIScreen popup = I._popupList[0];
                popup.gameObject.SetActive(false);
                I._popupList.RemoveAt(0);
            }

            if (I._popupList.Count > 0)
                I._popupList[0].gameObject.SetActive(true);
        }

        public static void ClosePopup(string popupName)
        {
            var screen = FindUIScreen(popupName);
            if (screen != null)
                ClosePopup(screen);

        }
        public static void ClosePopup(UIScreen UIScreen)
        {
            UIScreen.gameObject.SetActive(false);
            I._popupList.Remove(UIScreen);
        }




        private void SortingPopup()
        {
            _popupList.Sort((s1, s2) => s2.Weight.CompareTo(s1.Weight));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackKey();
            }
        }

    }

    public static class UiUtil
    {
        public static UIScreen FindUIScreen(Transform tr)
        {
            UIScreen uIUIScreen = null;

            Transform parent = tr;

            for (int i = 0; i < 1000; ++i)
            {
                if (parent == null) break;

                if (parent.GetComponent<UIScreen>() != null)
                {
                    uIUIScreen = parent.GetComponent<UIScreen>();
                    break;
                }

                if (parent != null)
                    parent = parent.parent;

            }

            return uIUIScreen;

        }
    }

}
