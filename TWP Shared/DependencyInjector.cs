using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    /// <summary>
    /// Dependency Injector
    /// </summary>
    public static class DI
    {
        static Dictionary<Type, object> binds;

        static DI()
        {
            binds = new Dictionary<Type, object>();

            binds.Add(typeof(PanelGenerator), ReversePanelGenerator.Instance);
        }

        public static T Get<T>()
        {
            if (binds.ContainsKey(typeof(T)))
                return (T) binds[typeof(T)];
            else
                return default(T);
        }
    }
}
