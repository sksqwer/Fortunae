using UnityEngine;
using GB;
public class Game : MonoBehaviour, IView
{
    [SerializeField] Board _board;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void ViewQuick(string key, IOData data)
    {
        switch (key)
        {

        }
    }
}

