using JetBrains.Annotations;
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

        public string[] ShaderNames => shaderNames;

        [Header("Settings")]
        [SerializeField]
        internal UdonAssetDatabaseScope scope = UdonAssetDatabaseScope.Scene;

        [SerializeField]
        internal GameObject[] roots;

        [Header("Debug")]
        [SerializeField]
        private Logger.Abstract.UdonLogger logger;

        [SerializeField, HideInInspector]
        internal string pathLookup = string.Empty;

        [SerializeField, HideInInspector]
        internal int pathCount = 0;

        [Header("Database Data")]
#if UNITY_2020_2_OR_NEWER
        [NonReorderable]
#endif
        [SerializeField]
        internal Object[] assets = new Object[0];

        [SerializeField, HideInInspector]
        internal string[] shaderNames = new string[0];

        [SerializeField, HideInInspector]
        internal string[][] shaderData = new string[0][];

        [SerializeField, HideInInspector]
        internal int shaderCount = 0;

#if UNITY_2020_2_OR_NEWER
        [NonReorderable]
#endif
        [SerializeField]
        internal string[] folderPaths = new string[0];

#if UNITY_2020_2_OR_NEWER
        [NonReorderable]
#endif
        [SerializeField]
        internal string[] assetPaths = new string[0];

        [SerializeField, HideInInspector]
        internal string[] assetGUIDs = new string[0];

        private const string LOG_PREFIX = "[<color=#B8B>UdonAssetDatabase</color>]:";

        [PublicAPI]
        public Object LoadAssetAtPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { return null; }

            int assetIndex = assetPaths.IndexOf(path);

            if(assetIndex < 0) { return null; }

            return assets[assetIndex];
        }

        [PublicAPI]
        public Object LoadAssetWithGUID(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid)) { return null; }

            int assetIndex = assetGUIDs.IndexOf(guid);

            if(assetIndex < 0) { return null; }

            return assets[assetIndex];
        }

        [PublicAPI]
        public string GetAssetPath(Object asset)
        {
            if(asset == null) { return string.Empty; }

            int assetIndex = assets.IndexOf(asset);

            if(assetIndex < 0) { Log($"Couldn't find asset from database: <color=#888>{asset}</color>"); return string.Empty; }

            return assetPaths[assetIndex];
        }

        [PublicAPI]
        public string[] GetFilesInDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) { return new string[0]; }

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

        [PublicAPI]
        public string[] GetShaderData(Shader shader)
        {
            if(shader == null) { return new string[0]; }

            string name = shader.name;

            int shaderIndex = shaderNames.IndexOf(name);

            if (shaderIndex < 0) { Log($"Couldn't find shader from database: <color=#888>{name}</color>"); return null; }

            return shaderData[shaderIndex];
        }

        [PublicAPI]
        public bool TryAssignDefaultLogger(Logger.Abstract.UdonLogger udonLogger)
        {
            if(logger != null) { return false; }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
            if (!UnityEditor.BuildPipeline.isBuildingPlayer && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                UnityEditor.Undo.RecordObject(this, "Assign UdonAssetDatabase Logger");
            }
#endif

            logger = udonLogger;

            return true;
        }

        private void Log(string message)
        {
            if (logger) { logger.LogFormat("{0} {1}", LOG_PREFIX, message); }
        }

        private void LogError(string message)
        {
            if (logger) { logger.LogErrorFormat("{0} {1}", LOG_PREFIX, message); }
        }
    }
}
