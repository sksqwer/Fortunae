using System.Collections.Generic;
using UnityEngine;


namespace GB
{
    /// <remarks>
    /// <copyright file="Presenter.cs" company="GB">
    /// The MIT License (MIT)
    /// 
    /// Copyright (c) 2022 GB
    /// 
    /// Permission is hereby granted, free of charge, to any person obtaining a copy
    /// of this software and associated documentation files (the "Software"), to deal
    /// in the Software without restriction, including without limitation the rights
    /// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    /// copies of the Software, and to permit persons to whom the Software is
    /// furnished to do so, subject to the following conditions:
    /// 
    /// The above copyright notice and this permission notice shall be included in
    /// all copies or substantial portions of the Software.
    /// 
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    /// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    /// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    /// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    /// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    /// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    /// THE SOFTWARE.

    public class Presenter : AutoSingleton<Presenter>
    {
        void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(this.gameObject);
        }

        Dictionary<string, List<IView>> _dicView = new Dictionary<string, List<IView>>();
        public IReadOnlyDictionary<string, List<IView>> Views { get { return _dicView; } }

        public List<IView> _nullList = new List<IView>();

        public static void Clear()
        {
            I._dicView.Clear();
        }

        public static bool ContainsView(string key)
        {
            return I._dicView.ContainsKey(key);
        }

        public static void Remove(string domain)
        {
            if (I._dicView.ContainsKey(domain)) I._dicView.Remove(domain);
        }


        public static void Bind(string domain, IView view)
        {
            if (I._dicView.ContainsKey(domain))
            {
                if (I._dicView[domain] != null)
                {
                    I._dicView[domain].Add(view);
                }
                else
                {
                    I._dicView[domain] = new List<IView>
                    {
                        view
                    };
                }
            }
            else
            {

                List<IView> viewList = new List<IView> { view };
                I._dicView.Add(domain, viewList);
            }
        }


        public static void UnBind(string domain, IView view)
        {
            if (I._dicView.ContainsKey(domain) == false) return;
            I._dicView[domain].Remove(view);
        }

        public static void Send(string domain, string key)
        {
            if (I._dicView.ContainsKey(domain) == false)
            {
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            List<IView> viewList = I._dicView[domain];
           
            for (int i = 0; i < viewList.Count; ++i)
            {
                if (viewList[i] == null)
                {
                    I._nullList.Add(viewList[i]);
                    continue;
                }

                viewList[i].ViewQuick(key, null);
            }

            
            if(I._nullList.Count > 0)
            {
                for (int i = 0; i < I._nullList.Count; ++i) viewList.Remove(I._nullList[i]);
                I._nullList.Clear();
            } 
        }


        public static void Send<T>(string domain, string key, T data)
        {
            if (I._dicView.ContainsKey(domain) == false)
            {
                Debug.LogWarning("Presenter - None Domain : " + domain);
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("Presenter - Key : null - " + domain);
                return;
            }

            List<IView> viewList = I._dicView[domain];

            for (int i = 0; i < viewList.Count; ++i)
            {
                if (viewList[i] == null)
                {
                    I._nullList.Add(viewList[i]);
                    continue;
                }

                viewList[i].ViewQuick(key, new OData<T>(data));
            }

             
            if(I._nullList.Count > 0)
            {
                for (int i = 0; i < I._nullList.Count; ++i) viewList.Remove(I._nullList[i]);
                I._nullList.Clear();
            } 
        }

    }

}

