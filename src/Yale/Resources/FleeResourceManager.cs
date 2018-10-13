using System;
using System.Collections.Generic;
using System.Resources;

namespace Yale.Resources
{
    internal class FleeResourceManager
    {

        private readonly Dictionary<string, ResourceManager> _myResourceManagers;

        private static readonly object RmLock = new object();

        private FleeResourceManager()
        {
            _myResourceManagers = new Dictionary<string, ResourceManager>(StringComparer.OrdinalIgnoreCase);
        }

        private ResourceManager GetResourceManager(string resourceFile)
        {
            lock (RmLock)
            {
                if (_myResourceManagers.TryGetValue(resourceFile, out var rm) == false)
                {
                    var type = typeof(FleeResourceManager);
                    rm = new ResourceManager($"{type.Namespace}.{resourceFile}", type.Assembly);
                    _myResourceManagers.Add(resourceFile, rm);
                }
                return rm;
            }
        }

        private string GetResourceString(string resourceFile, string key)
        {
            var resourceManager = GetResourceManager(resourceFile);
            return resourceManager.GetString(key);
        }

        public string GetCompileErrorString(string key)
        {
            return GetResourceString("CompileErrors", key);
        }

        public string GetElementNameString(string key)
        {
            return GetResourceString("ElementNames", key);
        }

        public string GetGeneralErrorString(string key)
        {
            return GetResourceString("GeneralErrors", key);
        }

        public static FleeResourceManager Instance { get; } = new FleeResourceManager();
    }
}
