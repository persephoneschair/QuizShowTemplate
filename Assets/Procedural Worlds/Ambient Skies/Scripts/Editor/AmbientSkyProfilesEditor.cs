//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using UnityEditor;
using AmbientSkies.Internal;
using PWCommon3;

namespace AmbientSkies
{
    /// <summary>
    /// Custom Editor
    /// </summary>
    [CustomEditor(typeof(AmbientSkyProfiles))]
    public class AmbientSkyProfilesEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        Color m_defaultBackgroundColor;
        Color m_notDefaultsSetColor = Color.red;
        Color m_defaultsSetColor = Color.green;

        #region Target variables

        private AmbientSkyProfiles m_profile;
        private Material skyMaterial;
        private string m_version;

        #endregion

        #region Constructors destructors and related delegates

        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }
        }

        void OnEnable()
        {
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }

            m_profile = (AmbientSkyProfiles) target;

            m_version = PWApp.CONF.Version;

            m_defaultBackgroundColor = GUI.backgroundColor;
        }

        #endregion

        #region GUI main

        public override void OnInspectorGUI()
        {
            //Initialization
            m_editorUtils.Initialize(); // Do not remove this!

            if (m_defaultsSetColor != Color.green)
            {
                //Remove warning
            }

            m_profile = (AmbientSkyProfiles) target;
            if (m_profile != null)
            {
                EditorGUI.BeginChangeCheck();

                m_editorUtils.Panel("GlobalSettings", GlobalSettings, true);

                if (EditorGUI.EndChangeCheck())
                {
                    if (m_profile.m_editSettings)
                    {
                        //Make sure profile indexes are correct
                        for (int profileIdx = 0; profileIdx < m_profile.m_skyProfiles.Count; profileIdx++)
                        {
                            m_profile.m_skyProfiles[profileIdx].profileIndex = profileIdx;
                        }
                        //Make sure profile indexes are correct
                        for (int profileIdx = 0; profileIdx < m_profile.m_proceduralSkyProfiles.Count; profileIdx++)
                        {
                            m_profile.m_proceduralSkyProfiles[profileIdx].profileIndex = profileIdx;
                        }
                        //Make sure profile indexes are correct
                        for (int profileIdx = 0; profileIdx < m_profile.m_gradientSkyProfiles.Count; profileIdx++)
                        {
                            m_profile.m_gradientSkyProfiles[profileIdx].profileIndex = profileIdx;
                        }
                        for (int profileIdx = 0; profileIdx < m_profile.m_ppProfiles.Count; profileIdx++)
                        {
                            m_profile.m_ppProfiles[profileIdx].profileIndex = profileIdx;
                        }
                    }
                    EditorUtility.SetDirty(m_profile);
                }
            }
        }

        private void GlobalSettings(bool helpEnabled)
        {
            EditorGUILayout.LabelField("Version: " + m_version);
            //m_profile.m_multiSceneSupport = m_editorUtils.Toggle("MultiSceneSupport", m_profile.m_multiSceneSupport, helpEnabled);

            if (!CheckIfDefaultsSet())
            {
                GUI.backgroundColor = m_notDefaultsSetColor;
                if (m_editorUtils.Button("RevertToFactorySettings"))
                {
                    if (EditorUtility.DisplayDialog("Warning!", "You are about to revert to factory settings. This will close the editor window if open, and reset Ambient Skies back to it's default installation settings. Doing this will overwrite your changes, are you sure you want to proceed?", "Yes", "No"))
                    {
                        AmbientSkiesEditorWindow editor = (AmbientSkiesEditorWindow)EditorWindow.GetWindow(typeof(AmbientSkiesEditorWindow));
                        if (editor != null)
                        {
                            editor.Close();
                        }
                        m_profile.RevertAllDefaults(m_profile.m_selectedRenderPipeline);
                    }
                }
            }

            if (m_profile.m_editSettings)
            {
                GUI.backgroundColor = m_defaultBackgroundColor;
                DrawDefaultInspector();
                if (GUILayout.Button("Save All Defaults"))
                {
                    m_profile.SaveAllDefaults(m_profile.m_selectedRenderPipeline);
                }
            }
        }

        #endregion

        #region Utils

        /// <summary>
        /// Check if the default settings are not set
        /// </summary>
        /// <returns></returns>
        private bool CheckIfDefaultsSet()
        {
            bool isDefaulted = true;

            if (skyMaterial == null)
            {
                skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Ambient Skies Skybox"));
            }
            else
            {
                if (skyMaterial.shader != Shader.Find("Skybox/Procedural"))
                {
                    isDefaulted = false;
                }
            }

            if (m_profile.m_CreationToolSettings != null)
            {
                if (m_profile.m_CreationToolSettings.m_selectedSystem != 0)
                {
                    isDefaulted = false;
                }
                if (m_profile.m_CreationToolSettings.m_selectedHDRI != 6)
                {
                    isDefaulted = false;
                }
                if (m_profile.m_CreationToolSettings.m_selectedProcedural != 1)
                {
                    isDefaulted = false;
                }
                if (m_profile.m_CreationToolSettings.m_selectedGradient != 1)
                {
                    isDefaulted = false;
                }
                if (m_profile.m_CreationToolSettings.m_selectedPostProcessing != 14)
                {
                    isDefaulted = false;
                }
                if (m_profile.m_CreationToolSettings.m_selectedLighting != 0)
                {
                    isDefaulted = false;
                }
            }

            if (m_profile.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
            {
                isDefaulted = false;
            }

            if (m_profile.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
            {
                isDefaulted = false;
            }

            if (m_profile.m_useSkies)
            {
                isDefaulted = false;
            }
            if (m_profile.m_usePostFX)
            {
                isDefaulted = false;
            }
            if (m_profile.m_useLighting)
            {
                isDefaulted = false;
            }
            if (m_profile.m_editSettings)
            {
                isDefaulted = false;
            }
            if (m_profile.m_showDebug)
            {
                isDefaulted = false;
            }
            if (m_profile.m_showFunctionDebugsOnly)
            {
                isDefaulted = false;
            }
            if (m_profile.m_showHasChangedDebug)
            {
                isDefaulted = false;
            }
            if (m_profile.m_showTimersInDebug)
            {
                isDefaulted = false;
            }
            if (m_profile.m_smartConsoleClean)
            {
                isDefaulted = false;
            }

            return isDefaulted;
        }

        #endregion
    }
}