using UnityEngine;

namespace GB
{
    public class RegistGameObject : UIRegister
    {
        

        public override void SetBind()
        {

            var screen = GetScreen();

            if (screen != null)
                screen.Add(Key, gameObject);
        }
    }
}