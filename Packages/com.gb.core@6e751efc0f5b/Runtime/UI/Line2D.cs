using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(UILineRenderer))]
public class Line2D : MonoBehaviour
{
    UILineRenderer _lineRenderer;
    [SerializeField] Sprite[] _sprites;

     [Range(10,100)] public float Speed = 25.0f;
    float _time = 0.0f;
    int _index = 0;
    void Awake()
    {
        
    }

    void OnEnable()
    {
        if(_lineRenderer == null)_lineRenderer = GetComponent<UILineRenderer>();
        if(_lineRenderer == null) return;
        if(_sprites == null) return;
        if(_sprites.Length <= 0) return;
        _index = 0;
        _lineRenderer.sprite = _sprites[_index];
    }

    public void SetPoints(Vector2[] points)
    {
        if(_lineRenderer == null)_lineRenderer = GetComponent<UILineRenderer>();

        _lineRenderer.Points = points;
    }

    void Update()
    {
        if(_lineRenderer == null) return;
        if(_sprites == null) return;
        if(_sprites.Length <= 0) return;

        _time += Time.deltaTime * Speed;
        if(_time > 1.0f)
        {
            _time = 0;
            _lineRenderer.sprite = _sprites[_index];
            ++_index;

            if (_index >= _sprites.Length)
                _index = 0;
            
        }
    }
}
