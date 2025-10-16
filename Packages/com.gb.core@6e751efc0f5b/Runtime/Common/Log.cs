using UnityEngine;


namespace GB
{
    public static class LogWrapperUtility
    {
        public static void GBLog(this object obj)
        {
            string colorValue = ColorUtility.ToHtmlStringRGBA(Color.cyan);
            UnityColorLog(obj.ToString(),null,colorValue);
        }

        public static void GBLog(this object obj,string title)
        {
            string colorValue = ColorUtility.ToHtmlStringRGBA(Color.cyan);
            UnityColorLog(obj.ToString(),title,colorValue);
        }

        public static void GBLog(this object obj,string title, Color color)
        {
            string colorValue ="#"+ ColorUtility.ToHtmlStringRGBA(color);
            UnityColorLog(obj.ToString(),title,colorValue);
        }

        private static void UnityColorLog(string log, string title, string colorValue)
        {
            log = log.Replace("\r\n", $"</color>\r\n<color={colorValue}>");

            const int maxAddCount = 100;
            const int maxTextSize = 11000;
            int startLogPos = 0;

            int pos = 0;

            for (int i = 0; i < maxAddCount && startLogPos < log.Length; i++)
            {
                pos += Mathf.Min(log.Length - pos, maxTextSize);

                while (pos < log.Length && !(log[pos] == ',' || log[pos] == '\"')) 
                    pos++;

                string frontMsg = (i == 0) ? string.Empty : $"<color=#ffffff>(===============>)</color> ";

                
                string viewMsg = log.Substring(startLogPos, pos - startLogPos);

                if (colorValue == null)
                    Debug.Log(viewMsg);
                else
                {
                    if (string.IsNullOrEmpty(title))
                        Debug.Log($"<b>{frontMsg}</b><color={colorValue}>{viewMsg}</color>");
                        
                    else
                        Debug.Log($"<b>{frontMsg}</b><color={colorValue}>[{title}] {viewMsg}</color>");
                }
                startLogPos = pos;
            }
        }
   }


    public static class GBLog
    {
        public static void Log(string log){log.GBLog();}
        public static void Log(string title,string log){log.GBLog(title);}
        public static void Log(string title,string log, Color color){log.GBLog(title,color);}
    }
}