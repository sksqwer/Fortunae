using UnityEngine.UI;

namespace GB
{
    public class LocalizationFontView : View
    {
        private void Start()
        {
            Refresh();
        }

        private void OnEnable()
        {

            Presenter.Bind("Localization", this);
            Refresh();


        }

        private void OnDisable()
        {
            Presenter.UnBind("Localization", this);
        }

        void Refresh()
        {
            var font = LocalizationManager.I.GetFont();
            if (font != null)
            {
                var text = GetComponent<Text>();
                if (text != null)
                    text.font = font;
            }
        }


        public override void ViewQuick(string key, IOData data)
        {
            Refresh();
        }
    }
}