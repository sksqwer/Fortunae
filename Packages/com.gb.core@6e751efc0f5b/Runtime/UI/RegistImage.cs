using UnityEngine;
using UnityEngine.UI;

namespace GB
{

    public class RegistImage : UIRegister
    {
        
        public override void SetBind()
        {
            

            var screen = GetScreen();

            if (screen != null)
                screen.Add(Key, GetComponent<Image>());
        }
    }


}