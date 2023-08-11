//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AmbientSkies
{
    [System.Serializable]
    public class CreationToolsSettings : ScriptableObject
    {
        #region Variables

        [Header("System Settings")]
        [SerializeField]
        public int m_selectedSystem;

        [Header("Skybox Settings")]
        [SerializeField]
        public int m_selectedHDRI;
        [SerializeField]
        public int m_selectedProcedural;
        [SerializeField]
        public int m_selectedGradient;

        [Header("Post Processing Settings")]
        [SerializeField]
        public int m_selectedPostProcessing;

        [Header("Lighting Settings")]
        [SerializeField]
        public int m_selectedLighting;

        #endregion

        #region Create Profile

        #if UNITY_EDITOR
        /// <summary>
        /// Create sky profile asset
        /// </summary>
        [MenuItem("Assets/Create/Ambient Skies/Creation Tool Settings")]
        public static void CreateSkyProfiles()
        {
            CreationToolsSettings asset = ScriptableObject.CreateInstance<CreationToolsSettings>();
            AssetDatabase.CreateAsset(asset, "Assets/Ambient Skies Creation Tool Settings.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        #endif

        #endregion

        #region Utils

        /// <summary>
        /// Fixes the scriptable object values
        /// </summary>
        public void RepairScriptableObject()
        {
            m_selectedSystem = 0;
            m_selectedHDRI = 6;
            m_selectedProcedural = 1;
            m_selectedGradient = 1;
            m_selectedPostProcessing = 14;
            m_selectedLighting = 0;
            Debug.Log("Reseted values to Default");
        }

        public void RevertToFactorySettings()
        {
            m_selectedSystem = 0;
            m_selectedHDRI = 6;
            m_selectedProcedural = 1;
            m_selectedGradient = 1;
            m_selectedPostProcessing = 14;
            m_selectedLighting = 0;
        }

        #endregion
    }
}