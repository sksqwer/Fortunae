using UnityEngine;
using QuickEye.Utility;
using NaughtyAttributes;

namespace GB
{

    public class LocalizationManager : AutoSingleton<LocalizationManager>
    {
        LocalizationData _DataAsset;


        [OnValueChanged("ChangeLanguage")][SerializeField] SystemLanguage _Language;
        public SystemLanguage Language { get { return _Language; } }

        [SerializeField] UnityDictionary<SystemLanguage, Font> _fonts;

        [SerializeField] Font _defaultFont;
        public Font GetFont()
        {
            if (_fonts.ContainsKey(_Language)) return _fonts[_Language];
            return _defaultFont;
        }


        private void Awake()
        {
            if (I != this)
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(this.gameObject);

            Load();
            
            if(string.IsNullOrEmpty( PlayerPrefs.GetString("Language", null))) getSystemLanguage();
            else SetSystemLanguage(PlayerPrefs.GetString("Language", "English"));
        }

        public void Load()
        {
            if (_DataAsset == null) _DataAsset = Resources.Load<LocalizationData>("LocalizationData");
        }


        public static string GetValue(string id)
        {
            I.Load();
            if (I._DataAsset == null) return string.Empty;

            if (string.IsNullOrEmpty(id))
                return string.Empty;

            if (string.IsNullOrEmpty(id) == false)
                id = id.Replace(" ", "").Replace("\r", "");

            if (!I._DataAsset.Datas.ContainsKey(id))
            {
                return "<color=red>" + id + "</color>";
            }

            if (!I._DataAsset.Datas[id].ContainsKey(I._Language))
                I._Language = SystemLanguage.English;

            string str = I._DataAsset.Datas[id][I._Language];

            return str;

        }

        private void ChangeLanguage()
        {
            ChangeLanguage(I.Language);
        }

        bool CheckLanguage(SystemLanguage language)
        {
            if (I._DataAsset != null)
            {
                if (I._DataAsset.Datas.Count > 0)
                {
                    foreach (var v in I._DataAsset.Datas)
                    {
                        if (v.Value.ContainsKey(language))
                            return true;
                    }

                }
                else return false;
            }

            return false;

        }


        public static void ChangeLanguage(SystemLanguage language)
        {
            I.Load();

            if(I.CheckLanguage(language))
                 I._Language = language;
            else
                I._Language = SystemLanguage.English;
                
            if(I._DataAsset == null) return;

            foreach (var v in I._DataAsset.Datas)
            {
                Presenter.Send("Localization", v.Key);
            }

            PlayerPrefs.SetString("Language", I._Language.ToString());
        }


        public void SetSystemLanguage(SystemLanguage language)
        {
            _Language = language;
            ChangeLanguage(_Language);
        }




        public void SetSystemLanguage(string language)
        {
            _Language = GetLanguage(language);
            if (_Language == SystemLanguage.Unknown)
                _Language = SystemLanguage.English;

            ChangeLanguage(_Language);
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


        private void getSystemLanguage()
        {
            _Language = GetLanguage(Application.systemLanguage.ToString());
            PlayerPrefs.SetString("Language", _Language.ToString());
        }




    }

}