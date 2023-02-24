using UdonSharp;
using UnityEngine;
using Varneon.VUdon.ArrayExtensions;
using Varneon.VUdon.UdonAssetDatabase.Enums;

namespace Varneon.VUdon.UdonAssetDatabase
{
    /// <summary>
    /// Asset database for Udon
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UdonAssetDatabase : UdonSharpBehaviour
    {
        public string[] FolderPaths => folderPaths;

        [SerializeField]
        internal UdonAssetDatabaseScope scope = UdonAssetDatabaseScope.Scene;

        [SerializeField]
        internal GameObject[] roots;

        [SerializeField, HideInInspector]
        internal string pathLookup = string.Empty;

        [SerializeField, HideInInspector]
        internal int pathCount = 0;

        [SerializeField, HideInInspector]
        internal Object[] assets = new Object[0];

        [SerializeField, HideInInspector]
        internal string shaderData = string.Empty;

        [SerializeField, HideInInspector]
        internal int shaderCount = 0;

        [SerializeField, HideInInspector]
        internal string[] folderPaths = new string[0];

        [SerializeField, HideInInspector]
        internal string[] assetPaths = new string[0];

        [SerializeField, HideInInspector]
        internal string[] assetGUIDs = new string[0];

        [SerializeField]
        private Logger.Abstract.UdonLogger logger;

        private const string LOG_PREFIX = "[<color=#B8B>UdonAssetDatabase</color>]:";

        public Object LoadAssetAtPath(string path)
        {
            int assetIndex = assetPaths.IndexOf(path);

            if(assetIndex < 0) { return null; }

            return assets[assetIndex];
        }

        public Object LoadAssetWithGUID(string guid)
        {
            int assetIndex = assetGUIDs.IndexOf(guid);

            if(assetIndex < 0) { return null; }

            return assets[assetIndex];
        }

        public string GetAssetPath(Object asset)
        {
            int assetIndex = assets.IndexOf(asset);

            if(assetIndex < 0) { Log($"Couldn't find asset from database: <color=#888>{asset}</color>"); return string.Empty; }

            return assetPaths[assetIndex];
        }

        public string[] GetFilesInDirectory(string directory)
        {
            string[] files = new string[0];

            int currentIndex = 0;

            int maxIndex = pathLookup.Length;

            string directoryLookupTemplate = $"\n{directory}";

            while (currentIndex >= 0)
            {
                currentIndex = pathLookup.IndexOf(directoryLookupTemplate, currentIndex + 1);

                if(currentIndex >= 0 && currentIndex < maxIndex)
                {
                    int returnIndex = pathLookup.IndexOf('\n', currentIndex + 1);

                    if(returnIndex >= 0)
                    {
                        files = files.Add(pathLookup.Substring(currentIndex + 1, returnIndex - currentIndex - 1));
                    }
                }
            }

            return files;
        }

        public string GetShaderData(Shader shader)
        {
            string name = shader.name;

            int dataNameHeaderIndex = shaderData.IndexOf($"\n{name}\n");

            if(dataNameHeaderIndex < 0) { Log($"Couldn't find shader from database: <color=#888>{name}</color>"); return string.Empty; }

            int dataNameHeaderEndIndex = shaderData.IndexOf("\n", dataNameHeaderIndex + 1) + 1;

            int dataEndIndex = shaderData.IndexOf("\n\n", dataNameHeaderEndIndex + 1);

            if(dataEndIndex < 0) { LogError("Invalid/corrupted database shader data!"); return string.Empty; }

            return shaderData.Substring(dataNameHeaderEndIndex, dataEndIndex - dataNameHeaderEndIndex);
        }

        private void Log(string message)
        {
            logger.LogFormat("{0} {1}", LOG_PREFIX, message);
        }

        private void LogError(string message)
        {
            logger.LogErrorFormat("{0} {1}", LOG_PREFIX, message);
        }
    }
}
