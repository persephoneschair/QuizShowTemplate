using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace AmbientSkies
{
    public static class PWAmbientSkiesPrefKeys
    {
        internal const string m_multiSceneEditorPrefs = "PW.AMBIENTSKIES_MultiSceneSupport";
        internal const string m_enableTextureStreaming = "PW.AMBIENTSKIES_EnableTextureStreaming";
        internal const string m_hdrpVisualQualityEditorPrefs = "PW.AMBIENTSKIES_HDRPVisualQuality";
        internal const string m_showDebuggingEditorPrefs = "PW.AMBIENTSKIES_ShowDebugging";
        internal const string m_showTimersInDebugEditorPrefs = "PW.AMBIENTSKIES_ShowTimersInDebug";
        internal const string m_showHasChangedDebugEditorPrefs = "PW.AMBIENTSKIES_ShowHasChangedDebug";
        internal const string m_showFunctionDebugsOnlyEditorPrefs = "PW.AMBIENTSKIES_ShowFunctionDebugsOnly";
        internal const string m_smartConsoleCleanEditorPrefs = "PW.AMBIENTSKIES_SmartConsoleClean";
    }

    public static class AmbientSkiesPreferences
    {
        public static bool m_refreshSettings = false;

        private static bool multiSceneSupport;
        private static bool enableTextureStreaming;
        private static bool showDebug;
        private static bool showTimersInDebug;
        private static bool showHasChangedDebug;
        private static bool showFunctionDebugsOnly;
        private static bool smartConsoleClean;
        private static GUIStyle m_boxStyle;
        private static AmbientSkyProfiles skyProfiles;
        private static List<AmbientSkyProfiles> skyProfilesList = new List<AmbientSkyProfiles>();
        private static CreationToolsSettings creationToolSettings;
        private static AmbientSkiesConsts.HDRPVisualQuality hdrpVisualQuality;

        [SettingsProvider]
        public static SettingsProvider PreferenceGUI()
        {
            return new SettingsProvider("Preferences/Procedural Worlds/Ambient Skies", SettingsScope.User)
            {
                guiHandler = searchContext =>
                {
                    //Load settings
                    if (!m_refreshSettings)
                    {
                        Load();
                    }

                    //Set up the box style
                    if (m_boxStyle == null)
                    {
                        m_boxStyle = new GUIStyle(GUI.skin.box)
                        {
                            normal = {textColor = GUI.skin.label.normal.textColor},
                            fontStyle = FontStyle.Bold,
                            alignment = TextAnchor.UpperLeft
                        };
                    }

                    //header info
                    EditorGUILayout.LabelField("Ambient Skies Preferences allows you to modify some global settings to suit your development.", EditorStyles.largeLabel);

                    //Multi scene support
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
                    multiSceneSupport = EditorGUILayout.Toggle(new GUIContent("Multi Scene Support", "If Multi Scene Support is enabled ambient skies will creates a saved settings component in your scene. When you are using the ambient skies window. This will allows you to use your own settings in each scene where the save system will use the get and set functions."), multiSceneSupport);

                    //Texture streaming
                    enableTextureStreaming = EditorGUILayout.Toggle(new GUIContent("Enable Texture Streaming", "Enables or disable texture streaming check quality settings for more information about Texture Streaming."), enableTextureStreaming);

                    //Debugging
                    EditorGUILayout.LabelField("Debugging Settings", EditorStyles.boldLabel);
                    showDebug = EditorGUILayout.Toggle(new GUIContent("Show Debugging", "Enables Ambient Skies to show debug. Error message will and always show if this is disabled"), showDebug);
                    if (showDebug)
                    {
                        EditorGUI.indentLevel++;
                        showTimersInDebug = EditorGUILayout.Toggle(new GUIContent("Show Timers", "Shows any timers in Ambient Skies coutn down or count up."), showTimersInDebug);
                        showHasChangedDebug = EditorGUILayout.Toggle(new GUIContent("Has Changed Updates", "Shows all the changes being made in Ambient Skies and what part of Ambient Skies is being updated currently"), showHasChangedDebug);
                        showFunctionDebugsOnly = EditorGUILayout.Toggle(new GUIContent("Function Debugging Only", "Shows only the function calls in Ambient Skies only"), showFunctionDebugsOnly);
                        smartConsoleClean = EditorGUILayout.Toggle(new GUIContent("Smart Clean Console", "This if enabled will clean the console every time Ambient Skies makes an update to show a clean debugging"), smartConsoleClean);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndVertical();

                    //Copyrights
                    DrawFooter("Ambient Skies...", 2020, true);

                    //Check if needs to be updated
                    if (CheckPrefrences(skyProfiles))
                    {
                        //Set all the settings and editor prefs
                        SetPrefs(skyProfiles, creationToolSettings);
                    }
                }
            };
        }

        /// <summary>
        /// Load all settings
        /// </summary>
        private static void Load()
        {
            //Get creation tools
            creationToolSettings = AssetDatabase.LoadAssetAtPath<CreationToolsSettings>(SkyboxUtils.GetAssetPath("Ambient Skies Creation Tool Settings"));
            if (creationToolSettings == null) return;
            //Get all sky profiles in project
            skyProfilesList = AmbientSkiesEditorFunctions.GetAllSkyProfilesProjectSearch("t:AmbientSkyProfiles");

            //Load current profile
            skyProfiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath(skyProfilesList[creationToolSettings.m_selectedSystem].name));
            if (skyProfiles == null) return;
            //Does key exist
            if (EditorPrefs.HasKey(PWAmbientSkiesPrefKeys.m_multiSceneEditorPrefs))
            {
                //Load from key
                multiSceneSupport = EditorPrefs.GetBool(PWAmbientSkiesPrefKeys.m_multiSceneEditorPrefs);
            }
            else
            {
                //Load from profile
                multiSceneSupport = skyProfiles.m_multiSceneSupport;
            }

            if (EditorPrefs.HasKey(PWAmbientSkiesPrefKeys.m_enableTextureStreaming))
            {
                //Load from key
                enableTextureStreaming = EditorPrefs.GetBool(PWAmbientSkiesPrefKeys.m_enableTextureStreaming);
            }
            else
            {
                //Load from profile
                enableTextureStreaming = QualitySettings.streamingMipmapsActive;
                skyProfiles.m_enableTextureStreaming = enableTextureStreaming;
            }

            //Does key exist
            if (EditorPrefs.HasKey(PWAmbientSkiesPrefKeys.m_showDebuggingEditorPrefs))
            {
                //Load from key
                showDebug = EditorPrefs.GetBool(PWAmbientSkiesPrefKeys.m_showDebuggingEditorPrefs);
            }
            else
            {
                //Load from profile
                showDebug = skyProfiles.m_showDebug;
            }

            //Does key exist
            if (EditorPrefs.HasKey(PWAmbientSkiesPrefKeys.m_showTimersInDebugEditorPrefs))
            {
                //Load from key
                showTimersInDebug = EditorPrefs.GetBool(PWAmbientSkiesPrefKeys.m_showTimersInDebugEditorPrefs);
            }
            else
            {
                //Load from profile
                showTimersInDebug = skyProfiles.m_showTimersInDebug;
            }

            //Does key exist
            if (EditorPrefs.HasKey(PWAmbientSkiesPrefKeys.m_showHasChangedDebugEditorPrefs))
            {
                //Load from key
                showHasChangedDebug = EditorPrefs.GetBool(PWAmbientSkiesPrefKeys.m_showHasChangedDebugEditorPrefs);
            }
            else
            {
                //Load from profile
                showHasChangedDebug = skyProfiles.m_showHasChangedDebug;
            }

            //Does key exist
            if (EditorPrefs.HasKey(PWAmbientSkiesPrefKeys.m_showFunctionDebugsOnlyEditorPrefs))
            {
                //Load from key
                showFunctionDebugsOnly = EditorPrefs.GetBool(PWAmbientSkiesPrefKeys.m_showFunctionDebugsOnlyEditorPrefs);
            }
            else
            {
                //Load from profile
                showFunctionDebugsOnly = skyProfiles.m_showFunctionDebugsOnly;
            }

            //Does key exist
            if (EditorPrefs.HasKey(PWAmbientSkiesPrefKeys.m_smartConsoleCleanEditorPrefs))
            {
                //Load from key
                smartConsoleClean = EditorPrefs.GetBool(PWAmbientSkiesPrefKeys.m_smartConsoleCleanEditorPrefs);
            }
            else
            {
                //Load from profile
                smartConsoleClean = skyProfiles.m_smartConsoleClean;
            }

            //Does key exist
            if (EditorPrefs.HasKey(PWAmbientSkiesPrefKeys.m_multiSceneEditorPrefs))
            {
                //Load from key
                hdrpVisualQuality = (AmbientSkiesConsts.HDRPVisualQuality)EditorPrefs.GetInt(PWAmbientSkiesPrefKeys.m_hdrpVisualQualityEditorPrefs);
            }
            else
            {
                //Load from profile
                hdrpVisualQuality = skyProfiles.m_hdrpVisualQuality;
            }

            //Load successful
            m_refreshSettings = true;
        }

        /// <summary>
        /// Check if prefs need to be set
        /// </summary>
        /// <param name="skyProfiles"></param>
        /// <returns></returns>
        private static bool CheckPrefrences(AmbientSkyProfiles skyProfiles)
        {
            bool updateChanges = false;
            if (skyProfiles != null)
            {
                if (skyProfiles.m_multiSceneSupport != multiSceneSupport)
                {
                    updateChanges = true;
                }

                if (skyProfiles.m_showDebug != showDebug)
                {
                    updateChanges = true;
                }

                if (skyProfiles.m_showTimersInDebug != showTimersInDebug)
                {
                    updateChanges = true;
                }

                if (skyProfiles.m_showHasChangedDebug != showHasChangedDebug)
                {
                    updateChanges = true;
                }

                if (skyProfiles.m_showFunctionDebugsOnly != showFunctionDebugsOnly)
                {
                    updateChanges = true;
                }

                if (skyProfiles.m_smartConsoleClean != smartConsoleClean)
                {
                    updateChanges = true;
                }

                if (skyProfiles.m_hdrpVisualQuality != hdrpVisualQuality)
                {
                    updateChanges = true;
                }

                if (skyProfiles.m_enableTextureStreaming != enableTextureStreaming)
                {
                    updateChanges = true;
                }
            }

            return updateChanges;
        }

        /// <summary>
        /// Sets all the settings and prefs
        /// </summary>
        /// <param name="skyProfiles"></param>
        private static void SetPrefs(AmbientSkyProfiles skyProfiles, CreationToolsSettings creationTools)
        {
            if (skyProfiles != null)
            {
                //Multi Scene Support
                skyProfiles.m_multiSceneSupport = multiSceneSupport;
                EditorPrefs.SetBool(PWAmbientSkiesPrefKeys.m_multiSceneEditorPrefs, multiSceneSupport);
                AmbientSkiesEditorFunctions.MultiSceneSupport(skyProfiles.m_multiSceneSupport, "Ambient Skies Global", false, skyProfiles, creationTools);
                //Texture Streaming
                skyProfiles.m_enableTextureStreaming = enableTextureStreaming;
                EditorPrefs.SetBool(PWAmbientSkiesPrefKeys.m_enableTextureStreaming, enableTextureStreaming);
                if (enableTextureStreaming)
                {
                    AmbientSkiesEditorFunctions.EnableTextureStreaming();
                }
                else
                {
                    AmbientSkiesEditorFunctions.DisableTextureStreaming();
                }
                //Debugging
                skyProfiles.m_showDebug = showDebug;
                EditorPrefs.SetBool(PWAmbientSkiesPrefKeys.m_showDebuggingEditorPrefs, showDebug);
                skyProfiles.m_showTimersInDebug = showTimersInDebug;
                EditorPrefs.SetBool(PWAmbientSkiesPrefKeys.m_showTimersInDebugEditorPrefs, showTimersInDebug);
                skyProfiles.m_showHasChangedDebug = showHasChangedDebug;
                EditorPrefs.SetBool(PWAmbientSkiesPrefKeys.m_showHasChangedDebugEditorPrefs, showHasChangedDebug);
                skyProfiles.m_showFunctionDebugsOnly = showFunctionDebugsOnly;
                EditorPrefs.SetBool(PWAmbientSkiesPrefKeys.m_showFunctionDebugsOnlyEditorPrefs, showFunctionDebugsOnly);
                skyProfiles.m_smartConsoleClean = smartConsoleClean;
                EditorPrefs.SetBool(PWAmbientSkiesPrefKeys.m_smartConsoleCleanEditorPrefs, smartConsoleClean);
                if (multiSceneSupport)
                {
                    AmbientSkiesSceneSavedSettings savedSettings = Object.FindObjectOfType<AmbientSkiesSceneSavedSettings>();
                    if (savedSettings != null)
                    {
                        savedSettings.SetSettings(skyProfiles, creationTools);
                    }
                }

#if HDPipeline
                skyProfiles.m_hdrpVisualQuality = (AmbientSkiesConsts.HDRPVisualQuality)hdrpVisualQuality;
                EditorPrefs.SetInt(PWAmbientSkiesPrefKeys.m_hdrpVisualQualityEditorPrefs, (int)hdrpVisualQuality);
                AmbientSkiesEditorFunctions.SetHDRPQuality((UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset)GraphicsSettings.renderPipelineAsset, hdrpVisualQuality);
#endif
                EditorUtility.SetDirty(skyProfiles);
            }
        }

        /// <summary>
        /// Draws the footer displayed at the bottom of the window
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="currentYear"></param>
        /// <param name="pinToBottom"></param>
        private static void DrawFooter(string productName, int currentYear, bool pinToBottom)
        {
            if (pinToBottom)
            {
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.LabelField(productName + " Copyright © " + currentYear.ToString() + " Procedural Worlds Pty Limited. All Rights Reserved.", EditorStyles.toolbar);
        }
    }
}