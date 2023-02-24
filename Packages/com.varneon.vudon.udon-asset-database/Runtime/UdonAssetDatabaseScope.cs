using UnityEngine;

namespace Varneon.VUdon.UdonAssetDatabase.Enums
{
    public enum UdonAssetDatabaseScope
    {
        /// <summary>
        /// Include everything from the current project
        /// </summary>
        /// <remarks>ONLY USE THIS IF NECESSARY!</remarks>
        [InspectorName("Project (Only if necessary!)")]
        Project,

        /// <summary>
        /// Include everything in the current scene
        /// </summary>
        [InspectorName("Scene (Recommended)")]
        Scene,

        /// <summary>
        /// Include everything under provided GameObjects
        /// </summary>
        Roots
    }
}