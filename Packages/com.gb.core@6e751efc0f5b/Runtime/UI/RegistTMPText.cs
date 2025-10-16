using TMPro;

namespace GB
{
    public class RegistTMPText : UIRegister
    {
        public override void SetBind()
        {
            var screen = GetScreen();

            if (screen != null)
                screen.Add(Key, GetComponent<TMP_Text>());
        }
    }
}