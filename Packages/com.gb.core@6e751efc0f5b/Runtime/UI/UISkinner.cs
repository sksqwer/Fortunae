using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using QuickEye.Utility;
using UnityEngine.UI;
using System;

namespace GB
{
    public class UISkinner : MonoBehaviour
    {
        [SerializeField] UnityDictionary<string, USkin> _USkins = new UnityDictionary<string, USkin>();
        [SerializeField] string _CurrentID;

        public string CurrentID{get{return _CurrentID;}}


        public void SetSkin(string key)
        {
            if (_USkins.ContainsKey(key) == false) return;
            _CurrentID = key;
            _USkins[_CurrentID].Apply();
        }

        [Button]
        public void Apply()
        {
            if (_USkins.ContainsKey(_CurrentID) == false) return;
            _USkins[_CurrentID].Apply();
        }
    }

    [Serializable]
    public class USkin
    {
        [SerializeField] UnityDictionary<Image, Sprite> _ChangeImages = new UnityDictionary<Image, Sprite>();
        [HorizontalLine(color: EColor.Red)]
        [SerializeField] UnityDictionary<Transform, Color> _ChangeColors = new UnityDictionary<Transform, Color>();
        [HorizontalLine(color: EColor.Green)]
        [SerializeField] UnityDictionary<Transform, Vector3> _ChangeScale = new UnityDictionary<Transform, Vector3>();
        [HorizontalLine(color: EColor.Blue)]
        [SerializeField] List<GameObject> _IsOnGameObjects = new List<GameObject>();
        [HorizontalLine(color: EColor.Black)]
        [SerializeField] List<GameObject> _IsOffGameObjects = new List<GameObject>();


        public void Apply()
        {
            foreach (var v in _ChangeImages) v.Key.sprite = v.Value;
            foreach (var v in _ChangeColors)  
            {
                var image = v.Key.GetComponent<Image>();
                if(image != null) image.color = v.Value;
                var text = v.Key.GetComponent<Text>();
                if(text != null) text.color = v.Value;
            }
            foreach (var v in _ChangeScale)   v.Key.localScale = v.Value;

            for (int i = 0; i < _IsOnGameObjects.Count; ++i)
                _IsOnGameObjects[i].SetActive(true);

            for (int i = 0; i < _IsOffGameObjects.Count; ++i)
                _IsOffGameObjects[i].SetActive(false);
        }
    }
}
