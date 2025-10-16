using System.Collections.Generic;
using GB;
using UnityEngine;

public class GBTime : AutoSingleton<GBTime>
{
    Dictionary<string,bool> _dictTimes = new Dictionary<string, bool>();
    Dictionary<string,float> _dictTimeScale = new Dictionary<string, float>();

    void Awake()
    {
        if(I != null && I != this) 
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this);

    }
    public static float GetTimeScale(string key)
    {
        if (!I._dictTimeScale.ContainsKey(key))
        {
            I._dictTimeScale[key] = 1;
        }
        if(!I._dictTimes.ContainsKey(key))
        {
            I._dictTimes[key] = true;
        }
        
        if(!I._dictTimes[key]) return 0;
        
        
        
        return I._dictTimeScale[key];
    }

    public static float GetDeltaTime(string key)
    {
        
        if(I._dictTimes.ContainsKey(key) && I._dictTimeScale.ContainsKey(key))
        {
            if(I._dictTimes[key]) return Time.deltaTime * I._dictTimeScale[key];
            else return 0;
        }
        else
        {
            I._dictTimeScale[key] = 1;
            I._dictTimes[key] = true;
            return Time.deltaTime;
        }
    }
    public void SetTimeScale(string key, float timeScale)
    {
        _dictTimeScale[key] = timeScale;
    }
    
    public static void Stop(string key)
    {
         I._dictTimes[key] = false; 
    }

    public static void Play(string key)
    {
         I._dictTimes[key] = true; 
    }



}
