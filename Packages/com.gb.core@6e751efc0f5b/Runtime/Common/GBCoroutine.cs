using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct GBCoroutine
{
    List<IEnumerator> _coroutineList;

    MonoBehaviour _mono;

    Coroutine  _coroutine;

    Action _result;

    public static GBCoroutine Create(MonoBehaviour monoBehaviour)
    {
        var gBCoroutine = new GBCoroutine
        {
            _mono = monoBehaviour
        };

        return gBCoroutine;
    }

    public GBCoroutine AddIEnumerator(IEnumerator coroutine)
    {
        if(_coroutineList == null)_coroutineList = new List<IEnumerator>();
        _coroutineList.Add(coroutine);
        return this;
    }

    public GBCoroutine OnComplete(Action result)
    {
        _result = result;
        return this;
    }

    public GBCoroutine Play()
    {
        if(_mono == null) return this;
        if(_coroutineList == null) return this;
        
        _coroutine = _mono.StartCoroutine(RunCoroutines());
        return this;
    }

    IEnumerator RunCoroutines()
    {
        for (int i = 0; i < _coroutineList.Count; i++)
        {
            yield return _mono.StartCoroutine(_coroutineList[i]);
        }

        _result?.Invoke();
        Clear();
    }

    void Clear()
    {
        _mono = null;
        _coroutine = null;
        if(_coroutineList != null) _coroutineList.Clear();
    }

    public void Stop()
    {
        if(_mono != null &&_coroutine != null)
        {
            _mono.StopCoroutine(_coroutine);
        }

        Clear();

        
    }


}

