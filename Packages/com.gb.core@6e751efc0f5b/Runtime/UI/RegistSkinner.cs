using UnityEngine;

namespace GB
{
    public class RegistSkinner : UIRegister
    {

        [SerializeField] UISkinner _skinner;

        public override void SetBind()
        {
            var screen = GetScreen();

            if (screen != null)
            {
                if (_skinner != null)
                {
                    screen.Add(Key, _skinner);
                }
                else
                {

                    screen.Add(Key, GetComponent<UISkinner>());
                }
            }
        }

    }
}