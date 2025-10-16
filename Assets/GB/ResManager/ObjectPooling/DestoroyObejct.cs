using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoroyObejct : MonoBehaviour
{
    const string GAME = "GAME";
    public float EndTime = 2.0f;

    float _time;
    private void OnEnable() 
    {
        _time = 0;
        
    }
    void Update()
    {
        _time += GBTime.GetDeltaTime(GAME);
        if(_time > EndTime)
        {
            GB.ObjectPooling.Return(this.gameObject);
        }
    }
}
