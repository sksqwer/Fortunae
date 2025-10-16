using UnityEngine;
using UnityEngine.UI;

namespace GB
{
    public class RegistText : UIRegister
    {
        public override void SetBind()
        {
            

            var screen = GetScreen();

            if (screen != null)
                screen.Add(Key, GetComponent<Text>());
        }
    }

}
