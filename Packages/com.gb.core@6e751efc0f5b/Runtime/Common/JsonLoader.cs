using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Newtonsoft.Json
{
    public static class JsonLoader
    {
        public static void LoadJson<T>(MonoBehaviour monoBehaviour, string json ,Action<T> result)
        {
            monoBehaviour.StartCoroutine(LoadDataCoroutine<T>(json,result));
        }

        static async Task<T> LoadJsonAsync<T>(string json)
        {
            return await Task.Run(() => JsonConvert.DeserializeObject<T>(json));
        }

        public static IEnumerator LoadDataCoroutine<T>(string json,Action<T> result)
        {
            Task<T> task = LoadJsonAsync<T>(json);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsCompletedSuccessfully)
            {
                result?.Invoke(task.Result);
            }
            else
            {
                Debug.LogError("JSON Load Failed");
            }
    }

    }

    public static class JsonWrapperUtility
    {
           //
    // Summary:
    //     If object is a StructWrapper, the value will be extracted. If not, the object
    //     will be cast to T. Wrapper is will not be returned to its pool until it is Unwrapped,
    //     or the pool is cleared.
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static T  FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);

        }

    }

}