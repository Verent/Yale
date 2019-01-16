using System.Resources;

namespace Yale.Resources
{
    internal static class ElementResourceManager
    {
        private const string FileName = "ElementNames";

        private static ResourceManager _manager;

        private static ResourceManager Manager
        {
            get
            {
                var type = typeof(ElementResourceManager);
                if (_manager != null) return _manager;
                _manager = new ResourceManager($"{type.Namespace}.{FileName}", type.Assembly);
                return _manager;
            }
        }

        public static string GetElementNameString(string key)
        {
            return Manager.GetString(key);
        }
    }
}