using UnityEngine;

namespace Varneon.VUdon.UdonAssetDatabase.Enums
{
    public enum UdonAssetDatabaseScope
    {
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