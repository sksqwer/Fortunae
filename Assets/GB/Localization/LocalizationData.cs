using QuickEye.Utility;
using UnityEngine;

namespace GB
{
    public class LocalizationData : ScriptableObject
    {
        public UnityDictionary<string, UnityDictionary<SystemLanguage, string>> Datas;
    }

}
