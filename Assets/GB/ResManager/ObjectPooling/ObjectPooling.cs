
using UnityEngine;
using QuickEye.Utility;




namespace GB
{
    public class ObjectPooling : AutoSingleton<ObjectPooling>
    {
        [SerializeField] UnityDictionary<string,GameObjectPool<PoolingType>> _dictPooling;

        bool _isInit;
        void Init()
        {
            if(_isInit) return;

            _dictPooling = new UnityDictionary<string, GameObjectPool<PoolingType>>();
            _isInit = true;
        }

  

         bool Check(string prefabPath,int startSize)
        {
            var obj  = ResManager.GetGameObject(prefabPath);
            if(obj == null) return false;

             var t = obj.GetComponent<PoolingType>();
            if(t == null) t = obj.AddComponent<PoolingType>();
                _dictPooling[prefabPath] = new GameObjectPool<PoolingType>(transform,t,startSize);

            return true;
        }

        public static GameObject Create(string prefabPath,int startSize = 10)
        {
            I.Init();
            if(!I._dictPooling.ContainsKey(prefabPath)) 
            {
                if(I.Check(prefabPath,startSize) == false)
                return null;
            }
            var o = I._dictPooling[prefabPath];
            var g = o.Rent();
            g.Name = prefabPath;
            g.IsActive = true;
            g.transform.SetParent(null);
            g.transform.position = o.Original.transform.position;
            g.transform.rotation = o.Original.transform.rotation;
            g.transform.localScale = o.Original.transform.localScale;
            g.gameObject.SetActive(true);

            return g.gameObject;
        }


        public static void Return(GameObject obj)
        {
            
            I.Init();
            var poolType = obj.GetComponent<PoolingType>();
            if(poolType == null) return;
            if(I._dictPooling.ContainsKey(poolType.Name) == false) return;

            var o = I._dictPooling[poolType.Name];
            o.Return(poolType);
        }



        public static void Clear()
        {
             I.Init();
            foreach(var v in I._dictPooling)
            {
                v.Value.ReturnAll();
            }
        }

        public static void Clear(string name)
        {
            I.Init();
            if(!I._dictPooling.ContainsKey(name)) return;

            I._dictPooling[name].ReturnAll();

        }
    }
}