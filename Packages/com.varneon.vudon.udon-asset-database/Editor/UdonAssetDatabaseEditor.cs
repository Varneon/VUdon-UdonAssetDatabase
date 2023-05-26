using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Varneon.VUdon.UdonAssetDatabase.Editor
{
    [CustomEditor(typeof(UdonAssetDatabase))]
    public class UdonAssetDatabaseEditor : UnityEditor.Editor
    {
        private UdonAssetDatabase database;

        private void OnEnable()
        {
            database = (UdonAssetDatabase)target;
        }

        private readonly HashSet<string> invalidFormats = new HashSet<string>() { ".dll" };

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(20);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Database statistics:", EditorStyles.largeLabel);

                GUILayout.Label($"{database.assets.Length} Assets");

                GUILayout.Label($"{database.shaderCount} Shaders");
            }

            if(GUILayout.Button("Generate Asset Database"))
            {
                GenerateAssetDatabase();
            }

            if(GUILayout.Button("Generate Shader Library"))
            {
                GenerateShaderLibrary();
            }

            GUI.color = Color.red;

            if (GUILayout.Button("Delete Database Data"))
            {
                DeleteDatabaseData();
            }

            GUI.color = Color.white;
        }

        private void GenerateAssetDatabase()
        {
            Object[] scopeRoots = database.scope == Enums.UdonAssetDatabaseScope.Scene ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects() : database.roots;

            SortedSet<string> dependencies = new SortedSet<string>(EditorUtility.CollectDependencies(EditorUtility.CollectDeepHierarchy(scopeRoots)).Select(d => AssetDatabase.GetAssetPath(d)));

            SortedSet<string> databasePaths = new SortedSet<string>();

            SortedSet<string> folders = new SortedSet<string>();

            foreach (string dependency in dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependency)) { continue; }

                if (!dependency.StartsWith("Assets/") && !dependency.StartsWith("Packages/")) { continue; }

                string directoryName = Path.GetDirectoryName(dependency).Replace('\\', '/');

                if (directoryName.Contains("/Editor/") || directoryName.Contains("/Editor")) { continue; }

                if (invalidFormats.Contains(Path.GetExtension(dependency))) { continue; }

                string[] folderChain = directoryName.Split('/');

                int depth = folderChain.Length;

                for (int i = 0; i <= depth; i++)
                {
                    string path = string.Join("/", folderChain, 0, i);

                    if (!folders.Contains(path) && !string.IsNullOrWhiteSpace(path)) { folders.Add(path); }
                }

                databasePaths.Add(dependency);
            }

            Undo.RecordObject(database, "Generate UdonAssetDatabase");

            database.assets = databasePaths.Select(p => AssetDatabase.LoadAssetAtPath<Object>(p)).ToArray();

            database.pathLookup = string.Format("\n{0}\n", string.Join("\n", databasePaths));

            database.pathCount = databasePaths.Count;

            database.folderPaths = folders.ToArray();

            database.assetPaths = databasePaths.ToArray();

            database.assetGUIDs = databasePaths.Select(p => AssetDatabase.AssetPathToGUID(p)).ToArray();
        }

        private void GenerateShaderLibrary()
        {
            HashSet<Shader> shaders = new HashSet<Shader>();

            shaders.UnionWith(database.assets.OfType<Shader>());

            shaders.UnionWith(database.assets.OfType<Material>().Select(m => m.shader));

            string[][] shaderData = new string[shaders.Count][];

            for (int i = 0; i < shaders.Count; i++)
            {
                Shader shader = shaders.ElementAt(i);

                List<string> properties = new List<string>();

                for (int j = 0; j < shader.GetPropertyCount(); j++)
                {
                    properties.Add(string.Join(" ", (int)shader.GetPropertyType(j), shader.GetPropertyName(j)));
                }

                shaderData[i] = properties.ToArray();
            }

            Undo.RecordObject(database, "Generate shader library");

            database.shaderNames = shaders.Select(s => s.name).ToArray();

            database.shaderData = shaderData;

            database.shaderCount = shaders.Count;

            #region JSON Placeholder
            //JObject root = new JObject();

            //foreach(Shader shader in shaders)
            //{
            //    JObject container = new JObject();

            //    for(int i = 0; i < shader.GetPropertyCount(); i++)
            //    {
            //        container.Add(new JProperty(shader.GetPropertyName(i), shader.GetPropertyType(i)));
            //    }

            //    root.Add(new JProperty(shader.name, container));
            //}

            //Debug.Log(JsonConvert.SerializeObject(root, Formatting.Indented));
            #endregion
        }

        private void DeleteDatabaseData()
        {
            Undo.RecordObject(database, "Delete UdonAssetDatabase data");

            ArrayUtility.Clear(ref database.assets);

            database.pathLookup = string.Empty;

            database.pathCount = 0;

            ArrayUtility.Clear(ref database.folderPaths);

            ArrayUtility.Clear(ref database.assetPaths);

            ArrayUtility.Clear(ref database.assetGUIDs);

            ArrayUtility.Clear(ref database.shaderData);

            database.shaderCount = 0;
        }
    }
}
