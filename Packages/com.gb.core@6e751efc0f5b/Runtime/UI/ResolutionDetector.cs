using UnityEngine;

namespace GB
{

    public class ResolutionDetector : MonoBehaviour
    {
        RectTransform _canvasRect;
        Vector2 _screen;

        void Start()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas != null) _canvasRect = canvas.GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_canvasRect != null)
            {
                if (_screen.x != _canvasRect.sizeDelta.x || _screen.y != _canvasRect.sizeDelta.y)
                {
                    _screen = _canvasRect.sizeDelta;
                    Presenter.Send("ScreenResolution", "ScreenResolution",_canvasRect);

                }
            }
        }

        void OnDrawGizmos()
        {
            if (_canvasRect != null)
            {
                if (_screen.x != _canvasRect.sizeDelta.x || _screen.y != _canvasRect.sizeDelta.y)
                {
                    _screen = _canvasRect.sizeDelta;
                    Presenter.Send("ScreenResolution", "ScreenResolution",_canvasRect);

                }
            }
        }
    }
}