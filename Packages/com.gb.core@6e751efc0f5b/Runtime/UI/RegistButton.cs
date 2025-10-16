using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace GB
{
    public class RegistButton : UIRegister
    {

        public override void SetBind()
        {

            var btn = GetComponent<Button>();
            if(btn == null)
            {
                UnityEngine.Debug.LogWarning("None Button");
                return;
            }

            var screen = GetScreen();

         

            if (screen != null)
            {
                screen.Add(Key, btn);

            }
        }
    }

}
