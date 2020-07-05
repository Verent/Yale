using System;
using System.Resources;

namespace Yale.Resources
{
    internal static class ElementResourceManager
    {
        private const string FileName = "ElementNames";

        private static ResourceManager? manager;

        private static ResourceManager Manager
        {
            get
            {
                var type = typeof(ElementResourceManager);
                if (manager != null) return manager;
                manager = new ResourceManager($"{type.Namespace}.{FileName}", type.Assembly);
                return manager;
            }
        }

        public static string GetElementNameString(string key)
        {
            return Manager.GetString(key) ?? throw new InvalidOperationException();
        }
    }
}