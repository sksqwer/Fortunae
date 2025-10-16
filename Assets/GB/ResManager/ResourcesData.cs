using QuickEye.Utility;
using UnityEngine;

namespace GB
{
    public class ResourcesData : ScriptableObject
    {
        #if UNITY_EDITOR
        public string spritePath;
        public string audioClipPath;
        public string prefabPath;

        #endif

        public UnityDictionary<string,Sprite> Sprites;
        public UnityDictionary<string,AudioClip> AudioClips;
        public UnityDictionary<string,GameObject> Prefabs;
    }
}
