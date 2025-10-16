using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GB
{
    public  class ODataBaseManager : AutoSingleton<ODataBaseManager>
    {
        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(this.gameObject);

        }

        public static IReadOnlyDictionary<string, IOData> Data  { get{return I._dictDatas;}}

        Dictionary<string, IOData> _dictDatas = new Dictionary<string, IOData>();
        Dictionary<string,Dictionary<MonoBehaviour,Action<IOData>>> _dictBinds = new Dictionary<string, Dictionary<MonoBehaviour, Action<IOData>>>();
        
        #if UNITY_EDITOR
        public Dictionary<string,Type> DictDataType = new Dictionary<string, Type>();
        #endif
      

         public static void Bind(MonoBehaviour mono,string key, Action<IOData> action)
         {
            if(I._dictBinds.ContainsKey(key))
            {
                I._dictBinds[key][mono] = action;                
            }
            else
            {
                I._dictBinds[key] = new Dictionary<MonoBehaviour, Action<IOData>>();
                I._dictBinds[key][mono] = action;                
            }

         }


        public static void UnBind(MonoBehaviour mono, string key)
        {
            
            if (I._dictBinds.ContainsKey(key) == false) return;
            if (I._dictBinds[key].ContainsKey(mono) == false) return;
            I._dictBinds[key].Remove(mono);
        }

        public static T Get<T>(string key)
        {
            return ODataConverter.Convert<T>(I._dictDatas[key]);
        }

        public static void Set<T>(string key, T data)
        {
            #if UNITY_EDITOR
            I.DictDataType[key] = typeof(T);
            #endif

            I._dictDatas[key] = new OData<T>(data);
            I.OnCall(key);
        }

        void OnCall(string key)
        {
            if(I._dictBinds.ContainsKey(key))
            {
                bool isNull = false;
                var bindings = I._dictBinds[key].ToList();
                
                foreach(var v in bindings)
                {
                    if(v.Key == null) 
                    {
                        isNull = true;
                        continue;
                    }
                    if(v.Key.isActiveAndEnabled) v.Value?.Invoke(_dictDatas[key]);
                }

                if(isNull)
                {
                    var list = I._dictBinds[key].Where(v=> v.Key == null).Select(v=>v.Key).ToList();
                    for(int i =0; i< list.Count;++i) I._dictBinds[key].Remove(list[i]);
                }
            }
        }

        public static void Remove(string key)
        {
            if (I._dictDatas.ContainsKey(key)) I._dictDatas.Remove(key);

            #if UNITY_EDITOR
            I.DictDataType.Remove(key);
            #endif

        }

        public static void Clear()
        {
            #if UNITY_EDITOR
            I.DictDataType.Clear();
            #endif

            I._dictDatas.Clear();
            I._dictBinds.Clear();
            
        }

        public static bool Contains(string key)
        {
            return I._dictDatas.ContainsKey(key);
        }
    }

}
