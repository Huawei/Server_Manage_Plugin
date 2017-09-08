﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.DAO
{
    public class SingletonProvider<T> where T : new()
    {
        private SingletonProvider() { }
        private static readonly object SyncObject = new object();
        private static T _singleton;

        public static T Instance
        {
            get
            {
                if (null == _singleton)
                {
                    lock (SyncObject)
                    {
                        _singleton = new T();
                    }
                }
                return _singleton;
            }
        }

    }
}
