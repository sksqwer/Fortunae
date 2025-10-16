using UnityEngine;
using QuickEye.Utility;
using System;
using NaughtyAttributes;
using UnityEngine.EventSystems;


namespace GB
{

    public class InputController : AutoSingleton<InputController>
    {
   
        public delegate void TouchPointDelegate(TouchPhase phase, int touchID, Vector2 position);

        [Header("Wolrd Position")]
        public TouchPointDelegate TouchWorldEvent;

        [Header("UI Position")]
        public TouchPointDelegate TouchUIEvent;


        private void ProcessWorld()
        {
            if (TouchWorldEvent == null) return;


            if (EventSystem.current != null)
            {

                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    SetTouchWorldCanceled();
                    return;
                }
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    SetTouchWorldCanceled();
                    return;
                }
            }


#if UNITY_EDITOR
            if (EventSystem.current != null)
            {
                if (EventSystem.current.IsPointerOverGameObject() &&
            (Input.GetMouseButton(0)))
                {
                    SetTouchWorldCanceled();
                    return;
                }
            }
#endif
            int touchCount = Input.touchCount;

            if (touchCount != 0)
            {
                for (int i = 0; i < touchCount; ++i)
                {
                    Touch touch = Input.GetTouch(i);
                    var p = Camera.main.ScreenToWorldPoint(touch.position);
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            TouchWorldEvent?.Invoke(TouchPhase.Began, i, p);
                            break;
                        case TouchPhase.Moved:
                            TouchWorldEvent?.Invoke(TouchPhase.Moved, i, p);
                            break;
                        case TouchPhase.Ended:
                            TouchWorldEvent?.Invoke(TouchPhase.Ended, i, p);
                            break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 2; ++i)
                {
                    if (Input.GetMouseButtonDown(i))
                    {
                        var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        TouchWorldEvent?.Invoke(TouchPhase.Began, i, p);
                    }
                    else if (Input.GetMouseButton(i))
                    {
                        var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        TouchWorldEvent?.Invoke(TouchPhase.Moved, i, p);
                    }
                    else if (Input.GetMouseButtonUp(i))
                    {
                        var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        TouchWorldEvent?.Invoke(TouchPhase.Ended, i, p);
                    }
                }

            }

        }
        private void SetTouchWorldCanceled()
        {
             int touchCount = Input.touchCount;

            if (touchCount != 0)
            {
                for (int i = 0; i < touchCount; ++i)
                {
                    Touch touch = Input.GetTouch(i);
                    var p = Camera.main.ScreenToWorldPoint(touch.position);
                    TouchWorldEvent?.Invoke(TouchPhase.Canceled, i, p);
                }
            }
            else
            {
                for (int i = 0; i < 2; ++i)
                {
                    if (Input.GetMouseButtonDown(i))
                    {
                        var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        TouchWorldEvent?.Invoke(TouchPhase.Canceled, i, p);
                    }
                    else if (Input.GetMouseButton(i))
                    {
                        var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        TouchWorldEvent?.Invoke(TouchPhase.Canceled, i, p);
                    }
                    else if (Input.GetMouseButtonUp(i))
                    {
                        var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        TouchWorldEvent?.Invoke(TouchPhase.Canceled, i, p);
                    }
                }
            }

        }



        private void ProcessUI()
        {
            if (TouchUIEvent == null) return;

            if (EventSystem.current != null)
            {

                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    SetTouchUICanceled();
                    return;
                }


                if (EventSystem.current.IsPointerOverGameObject())
                {
                    SetTouchUICanceled();
                    return;
                }
            }

            if (EventSystem.current != null)
            {
                if (EventSystem.current.IsPointerOverGameObject() &&
            (Input.GetMouseButton(0)))
                {
                    SetTouchUICanceled();
                    return;
                }
            }



            int touchCount = Input.touchCount;

            if (touchCount != 0)
            {
                for (int i = 0; i < touchCount; ++i)
                {
                    Touch touch = Input.GetTouch(i);
                    var p = touch.position;
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            TouchUIEvent?.Invoke(TouchPhase.Began, i, p);
                            break;
                        case TouchPhase.Moved:
                            TouchUIEvent?.Invoke(TouchPhase.Moved, i, p);
                            break;
                        case TouchPhase.Ended:
                            TouchUIEvent?.Invoke(TouchPhase.Ended, i, p);
                            break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 2; ++i)
                {
                    if (Input.GetMouseButtonDown(i))
                    {
                        var p = Input.mousePosition;
                        TouchUIEvent?.Invoke(TouchPhase.Began, i, p);
                    }
                    else if (Input.GetMouseButton(i))
                    {
                        var p = Input.mousePosition;
                        TouchUIEvent?.Invoke(TouchPhase.Moved, i, p);
                    }
                    else if (Input.GetMouseButtonUp(i))
                    {
                        var p = Input.mousePosition;
                        TouchUIEvent?.Invoke(TouchPhase.Ended, i, p);
                    }
                }

            }



        }

        private void SetTouchUICanceled()
        {
                   int touchCount = Input.touchCount;

            if (touchCount != 0)
            {
                for (int i = 0; i < touchCount; ++i)
                {
                    Touch touch = Input.GetTouch(i);
                    var p = touch.position;
                    TouchUIEvent?.Invoke(TouchPhase.Canceled, i, p);
                }
            }
            else
            {
                for (int i = 0; i < 2; ++i)
                {
                    if (Input.GetMouseButtonDown(i))
                    {
                        var p = Input.mousePosition;
                        TouchUIEvent?.Invoke(TouchPhase.Canceled, i, p);
                    }
                    else if (Input.GetMouseButton(i))
                    {
                        var p = Input.mousePosition;
                        TouchUIEvent?.Invoke(TouchPhase.Canceled, i, p);
                    }
                    else if (Input.GetMouseButtonUp(i))
                    {
                        var p = Input.mousePosition;
                        TouchUIEvent?.Invoke(TouchPhase.Canceled, i, p);
                    }
                }

            }

        }


        private void Update()
        {
            ProcessWorld();
            ProcessUI();
        }

        private void OnDestroy()
        {
            TouchWorldEvent = null;
            TouchUIEvent = null;
        }
    }

    [Serializable]
    public struct KeyAttribute
    {
        public KeyCode Key;
        public TouchPhase Phase;
    }

}