using UnityEngine;

namespace GB
{
    public class SoundManager : AutoSingleton<SoundManager>
    {

        [SerializeField] AudioSource _EffAudioSource;
        [SerializeField] AudioSource _BgAudioSource;
        

        bool _isMute
        {
            get
            {
                return PlayerPrefs.GetInt("AudioMute", 1) == 0;
            }
            set
            {
                PlayerPrefs.SetInt("AudioMute", value == true ? 1 : 0);
            }

        }

        public static bool IsMute
        {
            get
            {
                return I._isMute;
            }
        }

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Load();


            DontDestroyOnLoad(this.gameObject);


        }

        void Load()
        {
            _EffAudioSource = GetComponent<AudioSource>();
            if(_EffAudioSource == null) 
            {
                _EffAudioSource = gameObject.AddComponent<AudioSource>();
                _EffAudioSource.loop = false;

            }
            if(_BgAudioSource == null) 
            {
                _BgAudioSource = gameObject.AddComponent<AudioSource>();
                _BgAudioSource.loop = true;
            }

        }


        public static void SetMute(bool isMute)
        {
            I.Load();
            I._EffAudioSource.mute = isMute;
            I._BgAudioSource.mute = isMute;

            I._isMute = isMute;
        }

        public static void SetMuteEffect(bool isMute)
        {
            I.Load();
            I._EffAudioSource.mute = isMute;
        }
        public static void SetMuteBg(bool isMute)
        {
            I.Load();
            I._BgAudioSource.mute = isMute;
        }

        public static void SetVolume(float volume)
        {
            I.Load();
            I._BgAudioSource.volume = volume;
            I._EffAudioSource.volume = volume;
        }

        public static void SetVolumeEffect(float volume)
        {
            I.Load();
            I._EffAudioSource.volume = volume;
        }

        public static void SetVolumeBg(float volume)
        {
            I.Load();
            I._BgAudioSource.volume = volume;
        }


        private float _BgVolume = 1;
        private float _EffVolume = 1;

        public static void Pasue()
        {
            I.Load();
            I._EffVolume = I._EffAudioSource.volume;
            I._BgVolume = I._BgAudioSource.volume;

            I._BgAudioSource.volume = 0;
            I._EffAudioSource.volume = 0;
        }

        public static void Resume()
        {
            I.Load();
            I._BgAudioSource.volume = I._BgVolume;
            I._EffAudioSource.volume = I._EffVolume;

        }

        public static void Play(string path,float volume = 1)
        {
             I.Load();
            
            var audioClip = ResManager.GetAudioClip(path);
            I._EffAudioSource.PlayOneShot(audioClip,volume);

        }

        
        public static void PlayBG(string path,float volume = 1,  bool isLoop = true)
        {
            
            I.Load();
               var audioClip = ResManager.GetAudioClip(path);
            I._BgAudioSource.loop = isLoop;
            I._BgAudioSource.volume = volume;
            I._BgAudioSource.clip = audioClip;
            I._BgAudioSource.Play();
        }

    
    }
}
