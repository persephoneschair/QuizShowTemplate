using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;
using System.IO;
using PWCommon3;
using UnityEditor.Rendering;
using UnityEngine.Experimental.Rendering;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
#if HDPipeline && !UNITY_2019_3_OR_NEWER
using UnityEngine.Experimental.Rendering.HDPipeline;
#elif HDPipeline && UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.HighDefinition;
#endif
#if UPPipeline
using UnityEngine.Rendering.Universal;
#endif
#if LWPipeline && UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering.LWRP;
#elif LWPipeline && !UNITY_2019_1_OR_NEWER
using UnityEngine.Experimental.Rendering.LightweightPipeline;
#endif

namespace AmbientSkies
{
    public static class AmbientSkiesEditorFunctions
    {
        #region On Enable

        /// <summary>
        /// Loads icons
        /// </summary>
        public static void LoadIcons(out Texture2D m_skiesIcon, out Texture2D m_additionalLightIcon, out Texture2D m_postProcessingIcon, out Texture2D m_lightingIcon, out Texture2D m_infoIcon)
        {
            m_skiesIcon = null;
            m_additionalLightIcon = null;
            m_postProcessingIcon = null;
            m_lightingIcon = null;
            m_infoIcon = null;

            if (EditorGUIUtility.isProSkin)
            {
                if (m_skiesIcon == null)
                {
                    m_skiesIcon = Resources.Load("Skybox_Pro_icon") as Texture2D;
                }

                if (m_additionalLightIcon == null)
                {
                    m_additionalLightIcon = Resources.Load("Lightbulb_Pro_Icon") as Texture2D;
                }

                if (m_postProcessingIcon == null)
                {
                    m_postProcessingIcon = Resources.Load("Post_Processing_Pro_icon") as Texture2D;
                }

                if (m_lightingIcon == null)
                {
                    m_lightingIcon = Resources.Load("Light_Bake_Pro_icon") as Texture2D;
                }

                if (m_infoIcon == null)
                {
                    m_infoIcon = Resources.Load("Help_Pro_icon") as Texture2D;
                }
            }
            else
            {
                if (m_skiesIcon == null)
                {
                    m_skiesIcon = Resources.Load("Skybox_Standard_icon") as Texture2D;
                }

                if (m_additionalLightIcon == null)
                {
                    m_additionalLightIcon = Resources.Load("Lightbulb_Standard_Icon") as Texture2D;
                }

                if (m_postProcessingIcon == null)
                {
                    m_postProcessingIcon = Resources.Load("Post_Processing_Standard_icon") as Texture2D;
                }

                if (m_lightingIcon == null)
                {
                    m_lightingIcon = Resources.Load("Light_Bake_Standard_icon") as Texture2D;
                }

                if (m_infoIcon == null)
                {
                    m_infoIcon = Resources.Load("Help_Standard_icon") as Texture2D;
                }
            }
        }

        /// <summary>
        /// Adds post processing if required
        /// </summary>
        public static void AddPostProcessingV2Only(AmbientSkyProfiles m_profiles, out AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings, AmbientSkiesEditorWindow editorWindow)
        {
            renderPipelineSettings = m_profiles.m_selectedRenderPipeline;
            if (m_profiles.m_showDebug)
            {
                Debug.Log("Load Post Processing");
            }

            if (EditorUtility.DisplayDialog("Missing Post Processing V2", "We're about to import post processing v2 from the package manager. This process may take a few minutes and will setup your current scenes environment.", "OK"))
            {
                if (GraphicsSettings.renderPipelineAsset == null)
                {
                    m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;
                    renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;
                }
                else if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
                {
                    m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.HighDefinition;
                    renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.HighDefinition;
                }
                else
                {
                    m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.Universal;
                    renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.Universal;
                }

                AmbientSkiesPipelineUtilsEditor.ShowAmbientSkiesPipelineUtilsEditor(m_profiles.m_selectedRenderPipeline, renderPipelineSettings, false, false, editorWindow);
            }
        }

        /// <summary>
        /// Resets the profiles index if it exceeds the count
        /// </summary>
        public static void CheckProfilesLength(AmbientSkyProfiles m_profiles, CreationToolsSettings m_creationToolSettings, int m_selectedSkyboxProfileIndex, int m_selectedProceduralSkyboxProfileIndex, int m_selectedGradientSkyboxProfileIndex, int m_selectedPostProcessingProfileIndex, int m_selectedLightingProfileIndex)
        {
            if (m_selectedSkyboxProfileIndex > m_profiles.m_skyProfiles.Count)
            {
                m_selectedSkyboxProfileIndex = 6;

                EditorUtility.SetDirty(m_creationToolSettings);
                m_creationToolSettings.m_selectedHDRI = 6;

                Debug.Log("Profile HDRI has to be reset due to index being out of range. It has now been defualted to factory profile index");
            }

            if (m_selectedProceduralSkyboxProfileIndex > m_profiles.m_proceduralSkyProfiles.Count)
            {
                m_selectedProceduralSkyboxProfileIndex = 1;

                EditorUtility.SetDirty(m_creationToolSettings);
                m_creationToolSettings.m_selectedProcedural = 1;

                Debug.Log("Profile Procedural has to be reset due to index being out of range. It has now been defualted to factory profile index");
            }

            if (m_selectedGradientSkyboxProfileIndex > m_profiles.m_gradientSkyProfiles.Count)
            {
                m_selectedGradientSkyboxProfileIndex = 1;

                EditorUtility.SetDirty(m_creationToolSettings);
                m_creationToolSettings.m_selectedGradient = 1;

                Debug.Log("Profile Gradient has to be reset due to index being out of range. It has now been defualted to factory profile index");
            }

            if (m_selectedPostProcessingProfileIndex > m_profiles.m_ppProfiles.Count)
            {
                m_selectedPostProcessingProfileIndex = 14;

                EditorUtility.SetDirty(m_creationToolSettings);
                m_creationToolSettings.m_selectedPostProcessing = 14;

                Debug.Log("Profile Post Processing has to be reset due to index being out of range. It has now been defualted to factory profile index");
            }

            if (m_selectedLightingProfileIndex > m_profiles.m_lightingProfiles.Count)
            {
                m_selectedLightingProfileIndex = 0;

                EditorUtility.SetDirty(m_creationToolSettings);
                m_creationToolSettings.m_selectedLighting = 0;

                Debug.Log("Profile Lighting has to be reset due to index being out of range. It has now been defualted to factory profile index");
            }
        }

        /// <summary>
        /// Sets scripting defines for pipeline
        /// </summary>
        public static bool ApplyScriptingDefine(RenderPipelineAsset m_currentRenderpipelineAsset, AmbientSkyProfiles m_profiles, out AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings, CreationToolsSettings creationTools)
        {
            //Sets the current renderer asset
            m_currentRenderpipelineAsset = GraphicsSettings.renderPipelineAsset;
            renderPipelineSettings = m_profiles.m_selectedRenderPipeline;
            //Marks the pipeline asset to be saved
            if (m_currentRenderpipelineAsset != null)
            {
                EditorUtility.SetDirty(m_currentRenderpipelineAsset);
            }

            bool isChanged = false;
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            foreach (AmbientProceduralSkyboxProfile profile in m_profiles.m_proceduralSkyProfiles)
            {
                profile.enableSunDisk = true;
            }

            #region Built-In
            if (GraphicsSettings.renderPipelineAsset == null)
            {
                m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;
                renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;

                if (currBuildSettings.Contains("LWPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("LWPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("LWPipeline", "");
                    isChanged = true;
                }
                if (currBuildSettings.Contains("HDPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("HDPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("HDPipeline", "");
                    isChanged = true;
                }
                if (currBuildSettings.Contains("UPPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("UPPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("UPPipeline", "");
                    isChanged = true;
                }

                //Check for enviro plugin is there
                bool enviroPresent = Directory.Exists(SkyboxUtils.GetAssetPath("Enviro - Sky and Weather"));
                if (enviroPresent)
                {
                    if (!currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings += ";AMBIENT_SKIES_ENVIRO";
                        isChanged = true;
                    }
                }
                else
                {
                    if (currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings = currBuildSettings.Replace("AMBIENT_SKIES_ENVIRO;", "");
                        currBuildSettings = currBuildSettings.Replace(";AMBIENT_SKIES_ENVIRO", "");
                        isChanged = true;
                    }
                }

                CleanUpAll(true, m_profiles, creationTools);

                if (isChanged)
                {
                    if (EditorUtility.DisplayDialog("Status Changed", "The scripting defines need to updated, this will cause a code recompile. Depending on how big your project is this could take a few minutes...", "Ok"))
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
                    }

                    AmbientSkiesEditorFunctions.UpdateAllFogType(AmbientSkiesConsts.VolumeFogType.Linear, m_profiles);

                    if (EditorUtility.DisplayDialog("Clear Lightmaps!", "You've switched pipeline, lighting will behave differently in this render pipeline. We recommend clearing the lightmap data in your scene. Would you like to clear it?", "Yes", "No"))
                    {
                        LightingUtils.ClearLightmapData();
                    }

                    return true;
                }
            }
            #endregion

            #region High Definition
            else if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
            {
                m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.HighDefinition;
                renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.HighDefinition;

                if (!currBuildSettings.Contains("HDPipeline"))
                {
                    currBuildSettings += ";HDPipeline";
                    isChanged = true;
                }
                if (currBuildSettings.Contains("LWPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("LWPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("LWPipeline", "");
                    isChanged = true;
                }
                if (currBuildSettings.Contains("UPPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("UPPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("UPPipeline", "");
                    isChanged = true;
                }

                //Check for enviro plugin is there
                bool enviroPresent = Directory.Exists(SkyboxUtils.GetAssetPath("Enviro - Sky and Weather"));
                if (enviroPresent)
                {
                    if (!currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings += ";AMBIENT_SKIES_ENVIRO";
                        isChanged = true;
                    }
                }
                else
                {
                    if (currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings = currBuildSettings.Replace("AMBIENT_SKIES_ENVIRO;", "");
                        currBuildSettings = currBuildSettings.Replace(";AMBIENT_SKIES_ENVIRO", "");
                        isChanged = true;
                    }
                }

                CleanUpURP(true, m_profiles, creationTools);
                CleanUpLWRP(true, m_profiles);
                CleanUpBuiltIn(true, m_profiles, creationTools);

                if (isChanged)
                {
                    EditorUtility.DisplayDialog("Status Changed", "The scripting defines need to updated this will cause a code recompile. Depending on how big your project is this could take a few minutes", "Ok");

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);

                    AmbientSkiesEditorFunctions.UpdateAllFogType(AmbientSkiesConsts.VolumeFogType.Volumetric, m_profiles);

                    m_profiles.m_configurationType = AmbientSkiesConsts.AutoConfigureType.Manual;

                    if (GraphicsSettings.renderPipelineAsset.name != "Procedural Worlds HDRPRenderPipelineAsset")
                    {
                        if (EditorUtility.DisplayDialog("Update Pipeline Asset!", "Would you like to change your render pipeline asset settings to use Ambient Skies settings?", "Yes", "No"))
                        {
                            GraphicsSettings.renderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SkyboxUtils.GetAssetPath("Procedural Worlds HDRPRenderPipelineAsset"));
                        }
                    }

                    /*
                    m_highQualityMode = false;
                    if (EditorUtility.DisplayDialog("Enable High Quality!", "Would you like to enable High Quality Volumetrics and Subsurface Scattering?", "Yes", "No"))
                    {
                        m_highQualityMode = true;
                        m_enableHDRP = true;
                    }
                    */

                    if (EditorUtility.DisplayDialog("Clear Lightmaps!", "You've switched pipeline, lighting will behave differently in this render pipeline. We recommend clearing the lightmap data in your scene. Would you like to clear it?", "Yes", "No"))
                    {
                        LightingUtils.ClearLightmapData();
                    }

                    return true;
                }
            }
            #endregion

            #region Universal
            else if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("UniversalRenderPipelineAsset"))
            {
                m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.Universal;
                renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.Universal;

                if (!currBuildSettings.Contains("UPPipeline"))
                {
                    currBuildSettings += ";UPPipeline";
                    isChanged = true;
                }
                if (currBuildSettings.Contains("LWPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("LWPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("LWPipeline", "");
                    isChanged = true;
                }
                if (currBuildSettings.Contains("HDPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("HDPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("HDPipeline", "");
                    isChanged = true;
                }

                //Check for enviro plugin is there
                bool enviroPresent = Directory.Exists(SkyboxUtils.GetAssetPath("Enviro - Sky and Weather"));
                if (enviroPresent)
                {
                    if (!currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings += ";AMBIENT_SKIES_ENVIRO";
                        isChanged = true;
                    }
                }
                else
                {
                    if (currBuildSettings.Contains("AMBIENT_SKIES_ENVIRO"))
                    {
                        currBuildSettings = currBuildSettings.Replace("AMBIENT_SKIES_ENVIRO;", "");
                        currBuildSettings = currBuildSettings.Replace(";AMBIENT_SKIES_ENVIRO", "");
                        isChanged = true;
                    }
                }

                CleanUpURP(true, m_profiles, creationTools);
                CleanUpHDRP(true, m_profiles, creationTools);
                CleanUpBuiltIn(true, m_profiles, creationTools);

                if (isChanged)
                {
                    EditorUtility.DisplayDialog("Status Changed", "The scripting defines need to updated this will cause a code recompile. Depending on how big your project is this could take a few minutes", "Ok");

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);

                    AmbientSkiesEditorFunctions.UpdateAllFogType(AmbientSkiesConsts.VolumeFogType.Linear, m_profiles);

                    m_profiles.m_configurationType = AmbientSkiesConsts.AutoConfigureType.Manual;

                    if (GraphicsSettings.renderPipelineAsset.name != "Procedural Worlds UniversalRenderPipelineAsset")
                    {
                        if (EditorUtility.DisplayDialog("Update Pipeline Asset!", "Would you like to change your render pipeline asset settings to use Ambient Skies settings?", "Yes", "No"))
                        {
                            GraphicsSettings.renderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SkyboxUtils.GetAssetPath("Procedural Worlds UniversalRenderPipelineAsset"));
                        }
                    }

                    /*
                    m_highQualityMode = false;
                    if (EditorUtility.DisplayDialog("Enable High Quality!", "Would you like to enable High Quality Volumetrics and Subsurface Scattering?", "Yes", "No"))
                    {
                        m_highQualityMode = true;
                        m_enableHDRP = true;
                    }
                    */

                    if (EditorUtility.DisplayDialog("Clear Lightmaps!", "You've switched pipeline, lighting will behave differently in this render pipeline. We recommend clearing the lightmap data in your scene. Would you like to clear it?", "Yes", "No"))
                    {
                        LightingUtils.ClearLightmapData();
                    }

                    return true;
                }
            }
            #endregion

            AmbientSkiesEditorFunctions.UpdateLightingBakeType(m_profiles);

            return false;
        }

        /// <summary>
        /// Sets the high quality Subsurface and volumetrics
        /// </summary>
        /// <param name="pipelineAsset"></param>
        /// <param name="enableHighQuality"></param>
        public static bool EnableHighQualityHDRP(RenderPipelineAsset pipelineAsset, AmbientSkyProfiles m_profiles, bool enableHighQuality)
        {
            bool hasCompleted = false;
            if (pipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
            {
#if HDPipeline && UNITY_2018_3_OR_NEWER
                HDRenderPipelineAsset hDRender = pipelineAsset as HDRenderPipelineAsset;
                if (hDRender != null)
                {
#if UNITY_2019_1_OR_NEWER
                    RenderPipelineSettings settings = hDRender.currentPlatformRenderPipelineSettings;
#else
                    RenderPipelineSettings settings = hDRender.renderPipelineSettings;
#endif

                    settings.supportSubsurfaceScattering = enableHighQuality;
                    settings.supportVolumetrics = enableHighQuality;
                    settings.increaseResolutionOfVolumetrics = enableHighQuality;
                    settings.increaseSssSampleCount = enableHighQuality;

                    if (hDRender != null)
                    {
                        EditorUtility.SetDirty(hDRender);
                    }

                    if (m_profiles.m_showDebug)
                    {
                        Debug.Log("Applying High Quality HDRP");
                    }

                    hasCompleted = true;
                }
#endif
            }
            return hasCompleted;
        }

        /// <summary>
        /// Finds all profiles, to find type search t:OBJECT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeToSearch"></param>
        /// <returns></returns>
        public static List<AmbientSkyProfiles> GetAllSkyProfilesProjectSearch(string typeToSearch)
        {
            string[] skyProfilesGUIDs = AssetDatabase.FindAssets(typeToSearch);
            List<AmbientSkyProfiles> newSkyProfiles = new List<AmbientSkyProfiles>(skyProfilesGUIDs.Length);
            for (int x = 0; x < skyProfilesGUIDs.Length; ++x)
            {
                string path = AssetDatabase.GUIDToAssetPath(skyProfilesGUIDs[x]);
                AmbientSkyProfiles data = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(path);
                if (data == null)
                    continue;
                newSkyProfiles.Add(data);
            }

            return newSkyProfiles;
        }

        /// <summary>
        /// Finds all profiles, to find type search t:OBJECT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeToSearch"></param>
        /// <returns></returns>
        public static List<Material> GetAllSkyMaterialsProjectSearch(string typeToSearch)
        {
            string[] skyProfilesGUIDs = AssetDatabase.FindAssets(typeToSearch);
            List<Material> newSkyProfiles = new List<Material>(skyProfilesGUIDs.Length);
            for (int x = 0; x < skyProfilesGUIDs.Length; ++x)
            {
                string path = AssetDatabase.GUIDToAssetPath(skyProfilesGUIDs[x]);
                Material data = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (data == null)
                    continue;
                newSkyProfiles.Add(data);
            }

            return newSkyProfiles;
        }

        /// <summary>
        /// Creates a new time of day profile object for you to uses
        /// </summary>
        /// <param name="oldProfile"></param>
        public static AmbientSkiesTimeOfDayProfile CreateNewTimeOfDayProfile(AmbientSkiesTimeOfDayProfile oldProfile, AmbientSkyProfiles m_profiles)
        {
            AmbientSkiesTimeOfDayProfile asset = null;
            asset = ScriptableObject.CreateInstance<AmbientSkiesTimeOfDayProfile>();
            AssetDatabase.CreateAsset(asset, "Assets/Time Of Day Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            if (oldProfile != null)
            {
                EditorUtility.SetDirty(asset);
                EditorUtility.SetDirty(m_profiles);

                asset.m_currentTime = oldProfile.m_currentTime;
                asset.m_day = oldProfile.m_day;
                asset.m_dayColor = oldProfile.m_dayColor;
                asset.m_dayFogColor = oldProfile.m_dayFogColor;
                asset.m_dayFogDensity = oldProfile.m_dayFogDensity;
                asset.m_dayFogDistance = oldProfile.m_dayFogDistance;
                asset.m_dayLengthInSeconds = oldProfile.m_dayLengthInSeconds;
                asset.m_daySunGradientColor = oldProfile.m_daySunGradientColor;
                asset.m_daySunIntensity = oldProfile.m_daySunIntensity;
                asset.m_dayTempature = oldProfile.m_dayTempature;
                asset.m_debugMode = oldProfile.m_debugMode;
                asset.m_environmentSeason = oldProfile.m_environmentSeason;
                asset.m_fogMode = oldProfile.m_fogMode;
                asset.m_gIUpdateIntervalInSeconds = oldProfile.m_gIUpdateIntervalInSeconds;
                asset.m_incrementDownKey = oldProfile.m_incrementDownKey;
                asset.m_incrementUpKey = oldProfile.m_incrementUpKey;
                asset.m_lightAnisotropy = oldProfile.m_lightAnisotropy;
                asset.m_lightDepthExtent = oldProfile.m_lightDepthExtent;
                asset.m_lightProbeDimmer = oldProfile.m_lightProbeDimmer;
                asset.m_month = oldProfile.m_month;
                asset.m_nightColor = oldProfile.m_nightColor;
                asset.m_nightFogColor = oldProfile.m_nightFogColor;
                asset.m_nightFogDensity = oldProfile.m_nightFogDensity;
                asset.m_nightFogDistance = oldProfile.m_nightFogDistance;
                asset.m_nightSunGradientColor = oldProfile.m_nightSunGradientColor;
                asset.m_nightSunIntensity = oldProfile.m_nightSunIntensity;
                asset.m_nightTempature = oldProfile.m_nightTempature;
                asset.m_pauseTime = oldProfile.m_pauseTime;
                asset.m_pauseTimeKey = oldProfile.m_pauseTimeKey;
                asset.m_realtimeGIUpdate = oldProfile.m_realtimeGIUpdate;
                asset.m_renderPipeline = oldProfile.m_renderPipeline;
                asset.m_rotateSunLeftKey = oldProfile.m_rotateSunLeftKey;
                asset.m_rotateSunRightKey = oldProfile.m_rotateSunRightKey;
                asset.m_skyboxMaterial = oldProfile.m_skyboxMaterial;
                asset.m_skyExposure = oldProfile.m_skyExposure;
                asset.m_startFogDistance = oldProfile.m_startFogDistance;
                asset.m_sunRotation = oldProfile.m_sunRotation;
                asset.m_sunRotationAmount = oldProfile.m_sunRotationAmount;
                asset.m_sunSize = oldProfile.m_sunSize;
                asset.m_syncPostFXToTimeOfDay = oldProfile.m_syncPostFXToTimeOfDay;
                asset.m_timeOfDayController = oldProfile.m_timeOfDayController;
                asset.m_timeOfDayHour = oldProfile.m_timeOfDayHour;
                asset.m_timeOfDayMinutes = oldProfile.m_timeOfDayMinutes;
                asset.m_timeOfDaySeconds = oldProfile.m_timeOfDaySeconds;
                asset.m_timeToAddOrRemove = oldProfile.m_timeToAddOrRemove;
                asset.m_year = oldProfile.m_year;

                AssetDatabase.SaveAssets();

                if (EditorUtility.DisplayDialog("New Profile Created!", "Would you like to apply your new profile to Ambient Skies?", "Yes", "No"))
                {
                    m_profiles.m_timeOfDayProfile = asset;
                    return asset;
                }
            }

            return null;
        }

        #endregion

        #region UX

        /// <summary>
        /// Creates a tempature GUI layout
        /// </summary>
        /// <param name="property"></param>
        public static void TemperatureSlider(string key, SerializedProperty property, EditorUtils m_editorUtils)
        {
            var rect = EditorGUILayout.GetControlRect();
            var controlID = GUIUtility.GetControlID(FocusType.Passive, rect);
            var line = new Rect(rect);

            GUIContent label = m_editorUtils.GetContent(key);

            EditorGUI.Slider(rect, property, 1000f, 20000f, label);

            if (Event.current.GetTypeForControl(controlID) != EventType.Repaint)
                return;

            rect = EditorGUI.PrefixLabel(rect, controlID, new GUIContent(" "));
            rect.xMax -= 55f;

            line.width = 1f;
            line.x = rect.xMin;

            for (int x = 0; x < rect.width - 1; x++, line.x++)
            {
                var temperature = Mathf.Lerp(1000f, 20000f, x / rect.width);
                EditorGUI.DrawRect(line, ColorFromTemperature(temperature));
            }

            new GUIStyle("ColorPickerBox").Draw(rect, GUIContent.none, controlID);

            line.width = 2f;
            line.yMin--;
            line.yMin++;
            line.x = rect.xMin + Mathf.Lerp(0f, rect.width - 1f, (property.floatValue - 1000f) / (20000f - 1000f));

            EditorGUI.DrawRect(line, new Color32(56, 56, 56, 255));
        }

        /// <summary>
        /// Returns a color from the given temperature, thanks to tannerhelland.com for the algorithm
        /// </summary>
        /// <param name="temperature">Temperature of the color in Kelvin</param>
        public static Color ColorFromTemperature(float temperature)
        {
            temperature /= 100f;

            var red = 255f;
            var green = 255f;
            var blue = 255f;

            if (temperature >= 66f)
            {
                red = temperature - 60f;
                red = 329.698727446f * Mathf.Pow(red, -0.1332047592f);
            }

            if (temperature < 66f)
            {
                green = temperature;
                green = 99.4708025861f * Mathf.Log(green) - 161.1195681661f;
            }
            else
            {
                green = temperature - 60f;
                green = 288.1221695283f * Mathf.Pow(green, -0.0755148492f);
            }

            if (temperature <= 19f)
            {
                blue = 0f;
            }
            else if (temperature <= 66f)
            {
                blue = temperature - 10f;
                blue = 138.5177312231f * Mathf.Log(blue) - 305.0447927307f;
            }

            red /= 255f;
            green /= 255f;
            blue /= 255f;

            return new Color(red, green, blue);
        }

        /// <summary>
        /// GUI Layout for a gradient field on GUI inspector
        /// </summary>
        /// <param name="key"></param>
        /// <param name="gradient"></param>
        /// <param name="helpEnabled"></param>
        /// <returns></returns>
        public static Gradient GradientField(EditorUtils m_editorUtils, string key, Gradient gradient, bool helpEnabled)
        {
#if UNITY_2018_3_OR_NEWER
            GUIContent label = m_editorUtils.GetContent(key);
            gradient = EditorGUILayout.GradientField(label, gradient);
            m_editorUtils.InlineHelp(key, helpEnabled);
            return gradient;
#else
            return gradient;
#endif
        }

        /// <summary>
        /// Handy layer mask interface
        /// </summary>
        /// <param name="label"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static LayerMask LayerMaskField(EditorUtils m_editorUtils, string label, LayerMask layerMask, bool helpEnabled)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                {
                    maskWithoutEmpty |= (1 << i);
                }
            }
            maskWithoutEmpty = m_editorUtils.MaskField(label, maskWithoutEmpty, layers.ToArray(), helpEnabled);
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                {
                    mask |= (1 << layerNumbers[i]);
                }
            }
            layerMask.value = mask;
            return layerMask;
        }

        #endregion

        #region Functions

        /// <summary>
        /// Checks the post processing index
        /// </summary>
        /// <param name="checkInt"></param>
        /// <param name="m_profiles"></param>
        /// <returns></returns>
        public static int CheckPostProcessing(int checkInt, AmbientSkyProfiles m_profiles)
        {
            if (m_profiles == null)
            {
                return -1;
            }

            if (checkInt > m_profiles.m_ppProfiles.Count)
            {
                return 0;
            }

            if (checkInt == -1 && m_profiles.m_ppProfiles.Count > 0)
            {
                return 0;
            }

            return checkInt;
        }

        /// <summary>
        /// Checks the post processing index
        /// </summary>
        /// <param name="checkInt"></param>
        /// <param name="m_profiles"></param>
        /// <returns></returns>
        public static int CheckSkybox(int checkInt, AmbientSkyProfiles m_profiles)
        {
            if (m_profiles == null)
            {
                return -1;
            }

            if (checkInt > m_profiles.m_skyProfiles.Count)
            {
                return 0;
            }

            if (checkInt == -1 && m_profiles.m_skyProfiles.Count > 0)
            {
                return 0;
            }

            return checkInt;
        }

        /// <summary>
        /// Udpates fog distance to camera
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="divisionAmount"></param>
        public static float UpdateFogToCamera(float divisionAmount, Camera mainCam)
        {
            float newFogDistance = 500f;
            if (mainCam == null)
            {
                mainCam = Object.FindObjectOfType<Camera>();
            }

            if (mainCam == null) return newFogDistance;
            newFogDistance = Mathf.Round(mainCam.farClipPlane / divisionAmount);
            return newFogDistance;

        }

        /// <summary>
        /// Update all the fog modes in the active profile
        /// </summary>
        /// <param name="fogmode"></param>
        public static void UpdateAllFogType(AmbientSkiesConsts.VolumeFogType fogmode, AmbientSkyProfiles m_profiles)
        {
            if (m_profiles.m_showDebug)
            {
                Debug.Log("Updating fog modes to " + fogmode.ToString());
            }

            EditorUtility.SetDirty(m_profiles);

            //Get each skybox profile
            foreach (AmbientSkyboxProfile profile in m_profiles.m_skyProfiles)
            {
                //Change fog mode to Linear
                profile.fogType = fogmode;
            }
            //Get each procedural profile
            foreach (AmbientProceduralSkyboxProfile profile in m_profiles.m_proceduralSkyProfiles)
            {
                //Change fog mode to Linear
                profile.fogType = fogmode;
            }
            //Get each gradient profile
            foreach (AmbientGradientSkyboxProfile profile in m_profiles.m_gradientSkyProfiles)
            {
                //Change fog mode to Linear
                profile.fogType = fogmode;
            }
        }

        /// <summary>
        /// Updates the lightmaper mode to GPU or CPU depending on engine version
        /// </summary>
        public static void UpdateLightingBakeType(AmbientSkyProfiles m_profiles)
        {
#if UNITY_2018_3_OR_NEWER
            foreach (AmbientLightingProfile profile in m_profiles.m_lightingProfiles)
            {
                profile.lightmappingMode = AmbientSkiesConsts.LightmapperMode.ProgressiveGPU;
            }
#else
            foreach(AmbientLightingProfile profile in m_profiles.m_lightingProfiles)
            {
                profile.lightmappingMode = AmbientSkiesConsts.LightmapperMode.ProgressiveCPU;
            }
#endif
        }

        /// <summary>
        /// Sets and the new equator and ground colors and formats it as a out to be used
        /// </summary>
        /// <param name="equatorColor"></param>
        /// <param name="groundColor"></param>
        /// <param name="newEquatorColor"></param>
        /// <param name="newGroundColor"></param>
        public static void ConvertAmbientEquatorAndGroundColor(Color equatorColor, Color groundColor, out Color newEquatorColor, out Color newGroundColor)
        {
            //Equator calculations
            Color eColor = equatorColor;
            eColor.r = eColor.r / 2.4f;
            eColor.g = eColor.g / 2.4f;
            eColor.b = eColor.b / 2.4f;
            //Outs the new equator color
            newEquatorColor = eColor;

            //Ground calculations
            Color gColor = groundColor;
            gColor.r = gColor.r / 3.4f;
            gColor.g = gColor.g / 3.4f;
            gColor.b = gColor.b / 3.4f;
            //Outs the new ground color
            newGroundColor = gColor;
        }

        /// <summary>
        /// Enables texture streaming
        /// </summary>
        public static void EnableTextureStreaming()
        {
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsAddAllCameras = true;
            QualitySettings.streamingMipmapsMemoryBudget = 2048f;
        }

        /// <summary>
        /// Enables texture streaming
        /// </summary>
        public static void DisableTextureStreaming()
        {
            QualitySettings.streamingMipmapsActive = false;
            QualitySettings.streamingMipmapsAddAllCameras = false;
        }

#if HDPipeline
        /// <summary>
        /// Sets the HDRP Quality settings
        /// </summary>
        /// <param name="pipelineAsset"></param>
        /// <param name="visualQuality"></param>
        public static void SetHDRPQuality(HDRenderPipelineAsset pipelineAsset, AmbientSkiesConsts.HDRPVisualQuality visualQuality)
        {
            if (pipelineAsset != null)
            {
                RenderPipelineSettings settings = pipelineAsset.currentPlatformRenderPipelineSettings;
                switch (visualQuality)
                {
                    case AmbientSkiesConsts.HDRPVisualQuality.Low:
                        settings.colorBufferFormat = RenderPipelineSettings.ColorBufferFormat.R11G11B10;
                        settings.customBufferFormat = RenderPipelineSettings.CustomBufferFormat.R8G8B8A8;
                        settings.decalSettings.drawDistance = 250;
                        settings.decalSettings.atlasWidth = 1024;
                        settings.decalSettings.atlasHeight = 1024;
                        settings.supportSSAO = false;
                        settings.supportSSR = false;
                        settings.increaseResolutionOfVolumetrics = false;
                        settings.supportSubsurfaceScattering = false;
                        settings.postProcessSettings.lutFormat = GradingLutFormat.R11G11B10;
                        break;
                    case AmbientSkiesConsts.HDRPVisualQuality.Medium:
                        settings.colorBufferFormat = RenderPipelineSettings.ColorBufferFormat.R11G11B10;
                        settings.customBufferFormat = RenderPipelineSettings.CustomBufferFormat.R11G11B10;
                        settings.decalSettings.drawDistance = 500;
                        settings.decalSettings.atlasWidth = 2048;
                        settings.decalSettings.atlasHeight = 2048;
                        settings.supportSSAO = true;
                        settings.supportSSR = false;
                        settings.increaseResolutionOfVolumetrics = false;
                        settings.supportSubsurfaceScattering = true;
                        settings.postProcessSettings.lutFormat = GradingLutFormat.R16G16B16A16;
                        break;
                    case AmbientSkiesConsts.HDRPVisualQuality.High:
                        settings.colorBufferFormat = RenderPipelineSettings.ColorBufferFormat.R16G16B16A16;
                        settings.customBufferFormat = RenderPipelineSettings.CustomBufferFormat.R16G16B16A16;
                        settings.decalSettings.drawDistance = 1000;
                        settings.decalSettings.atlasWidth = 4096;
                        settings.decalSettings.atlasHeight = 4096;
                        settings.supportSSAO = true;
                        settings.supportSSR = true;
                        settings.increaseResolutionOfVolumetrics = true;
                        settings.supportSubsurfaceScattering = true;
                        settings.postProcessSettings.lutFormat = GradingLutFormat.R32G32B32A32;
                        break;
                }

                EditorUtility.SetDirty(pipelineAsset);
            }
        }
#endif

        /// <summary>
        /// Sets current open scene as active
        /// </summary>
        public static bool MarkActiveSceneAsDirty(AmbientSkyProfiles m_profiles)
        {
            bool haschanged = false;
            if (Application.isPlaying) return haschanged;
            if (m_profiles.m_showDebug)
            {
                Debug.Log("Marking scene dirty");
            }

            if (!EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            haschanged = true;

            return haschanged;
        }

        /// <summary>
        /// Creates the new scene object
        /// </summary>
        public static void NewSceneObjectCreation()
        {
            //Created object to resemble a new scene
            GameObject newSceneObject = GameObject.Find("Ambient Skies New Scene Object (Don't Delete Me)");
            //Parent object in the scene
            GameObject parentObject = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Global", false);
            //If the object to resemble a new scene is not there
            if (newSceneObject == null)
            {
                //Create it
                newSceneObject = new GameObject("Ambient Skies New Scene Object (Don't Delete Me)");
                //Parent it
                newSceneObject.transform.SetParent(parentObject.transform);
            }
        }

        /// <summary>
        /// Creates volume object for HDRP
        /// </summary>
        /// <param name="volumeName"></param>
        public static void CreateHDRPVolume(string volumeName)
        {
            //Get parent object
            GameObject parentObject = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Global", false);

            //Hd Pipeline Volume Setup
            GameObject volumeObject = GameObject.Find(volumeName);
            if (volumeObject == null)
            {
                volumeObject = new GameObject(volumeName) {layer = LayerMask.NameToLayer("TransparentFX")};
                volumeObject.transform.SetParent(parentObject.transform);
            }
            else
            {
                volumeObject.layer = LayerMask.NameToLayer("TransparentFX");
                volumeObject.transform.SetParent(parentObject.transform);
            }

#if HDPipeline
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath("Ambient Skies HD Volume Profile"));
            volume.isGlobal = true;
#endif
        }

        /// <summary>
        /// Updates all environment updates to true
        /// </summary>
        public static void SetAllEnvironmentUpdateToTrue(AmbientSkyProfiles profiles)
        {
            //Skybox
            profiles.m_updateVisualEnvironment = true;
            profiles.m_updateFog = true;
            profiles.m_updateShadows = true;
            profiles.m_updateAmbientLight = true;
            profiles.m_updateScreenSpaceReflections = true;
            profiles.m_updateScreenSpaceRefractions = true;
            profiles.m_updateSun = true;
        }

        /// <summary>
        /// Updates all environment updates to true
        /// </summary>
        public static void SetAllEnvironmentUpdateToFalse(AmbientSkyProfiles profiles)
        {
            //Skybox
            profiles.m_updateVisualEnvironment = false;
            profiles.m_updateFog = false;
            profiles.m_updateShadows = false;
            profiles.m_updateAmbientLight = false;
            profiles.m_updateScreenSpaceReflections = false;
            profiles.m_updateScreenSpaceRefractions = false;
            profiles.m_updateSun = false;
        }

        /// <summary>
        /// Updates all post fx updates to true
        /// </summary>
        public static void SetAllPostFxUpdateToTrue(AmbientSkyProfiles profiles)
        {
            //Post Processing
            profiles.m_updateAO = true;
            profiles.m_updateAutoExposure = true;
            profiles.m_updateBloom = true;
            profiles.m_updateChromatic = true;
            profiles.m_updateColorGrading = true;
            profiles.m_updateDOF = true;
            profiles.m_updateGrain = true;
            profiles.m_updateLensDistortion = true;
            profiles.m_updateMotionBlur = true;
            profiles.m_updateSSR = true;
            profiles.m_updateVignette = true;
            profiles.m_updatePanini = true;
            profiles.m_updateTargetPlaform = true;
        }

        /// <summary>
        /// Updates all post fx updates to true
        /// </summary>
        public static void SetAllPostFxUpdateToFalse(AmbientSkyProfiles profiles)
        {
            //Post Processing
            profiles.m_updateAO = false;
            profiles.m_updateAutoExposure = false;
            profiles.m_updateBloom = false;
            profiles.m_updateChromatic = false;
            profiles.m_updateColorGrading = false;
            profiles.m_updateDOF = false;
            profiles.m_updateGrain = false;
            profiles.m_updateLensDistortion = false;
            profiles.m_updateMotionBlur = false;
            profiles.m_updateSSR = false;
            profiles.m_updateVignette = false;
            profiles.m_updatePanini = false;
            profiles.m_updateTargetPlaform = false;
        }

        /// <summary>
        /// Updates all lighting updates to false
        /// </summary>
        public static void SetAllLightingUpdateToFalse(AmbientSkyProfiles profiles)
        {
            profiles.m_updateRealtime = false;
            profiles.m_updateBaked = false;
        }

        /// <summary>
        /// Updates all lighting updates to true
        /// </summary>
        public static void SetAllLightingUpdateToTrue(AmbientSkyProfiles profiles)
        {
            profiles.m_updateRealtime = true;
            profiles.m_updateBaked = true;
        }

        /// <summary>
        /// Sets the multi Scene system to be present or removed from the scene
        /// </summary>
        /// <param name="multiSceneSupported"></param>
        /// <param name="rootObjectName"></param>
        /// <param name="isGaiaParented"></param>
        public static bool MultiSceneSupport(bool multiSceneSupported ,string rootObjectName, bool isGaiaParented, AmbientSkyProfiles profile, CreationToolsSettings creationTools)
        {
            bool isPresent = false;
            GameObject newParentObject = SkyboxUtils.GetOrCreateParentObject(rootObjectName, isGaiaParented);
            AmbientSkiesSceneSavedSettings savedSettings = newParentObject.GetComponent<AmbientSkiesSceneSavedSettings>();
            if (multiSceneSupported)
            {
                if (savedSettings != null)
                {
                    isPresent = true;
                    savedSettings.GetSettings(profile, creationTools);
                    return isPresent;
                }
                else
                {
                    savedSettings = newParentObject.AddComponent<AmbientSkiesSceneSavedSettings>();
                    savedSettings.SetSettings(profile, creationTools);
                    isPresent = true;
                }
            }
            else
            {
                savedSettings = newParentObject.GetComponent<AmbientSkiesSceneSavedSettings>();
                if (savedSettings != null)
                {
                    Object.DestroyImmediate(savedSettings);
                    isPresent = false;
                }

                SkyboxUtils.DestroyParent(rootObjectName, multiSceneSupported);
            }

            return isPresent;
        }

        /// <summary>
        /// Gets the saved settings component from the scene
        /// </summary>
        /// <param name="rootObjectName"></param>
        /// <param name="isGaiaParented"></param>
        /// <returns></returns>
        public static AmbientSkiesSceneSavedSettings GetSavedSettingsComponent(string rootObjectName, bool isGaiaParented)
        {
            GameObject newParentObject = SkyboxUtils.GetOrCreateParentObject(rootObjectName, isGaiaParented);

            AmbientSkiesSceneSavedSettings savedSettings = null;
            savedSettings = newParentObject.GetComponent<AmbientSkiesSceneSavedSettings>();

            return savedSettings;
        }

        /// <summary>
        /// Loads the first is or is not PW Profile
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="currentProfileName"></param>
        /// <returns></returns>
        public static int GetSkyProfile(List<AmbientSkyProfiles> profiles, string currentProfileName)
        {
            for (int idx = 0; idx < profiles.Count; idx++)
            {
                if (profiles[idx].name == currentProfileName)
                {
                    return idx;
                }
            }
            return 0;
        }

        /// <summary>
        /// Set linear deffered lighting (the best for outdoor scenes)
        /// </summary>
        public static void SetLinearDeferredLighting()
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            var tier1 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1);
            tier1.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1, tier1);
            var tier2 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2);
            tier2.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2, tier2);
            var tier3 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3);
            tier3.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3, tier3);
        }

        /// <summary>
        /// Get the currently active terrain - or any terrain
        /// </summary>
        /// <returns>A terrain if there is one</returns>
        public static Terrain[] GetActiveTerrain()
        {
            //Grab active terrain if we can
            Terrain[] terrain = Terrain.activeTerrains;
            if (terrain != null)
            {
                return terrain;
            }

            return null;
        }

        /// <summary>
        /// This method saves all the settings for HDRP pipeline
        /// </summary>
        public static void SaveHDRPSettings(AmbientSkyProfiles m_profiles, AmbientSkiesConsts.SystemTypes systemtype, int m_selectedSkyboxProfileIndex, AmbientSkyboxProfile m_selectedSkyboxProfile, AmbientProceduralSkyboxProfile m_selectedProceduralSkyboxProfile, AmbientGradientSkyboxProfile m_selectedGradientSkyboxProfile)
        {
#if HDPipeline && UNITY_2018_3_OR_NEWER
            //Marks the profiles as dirty so settings can be saved
            EditorUtility.SetDirty(m_profiles);

            NewSceneObjectCreation();

            m_profiles.m_systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;
            systemtype = AmbientSkiesConsts.SystemTypes.ThirdParty;

            //Finds user profile
            m_selectedSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromProfileName(m_profiles, "User");
            //Sets the skybox profile settings to User
            m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];

            //Get main sun light
            GameObject mainLight = SkyboxUtils.GetMainDirectionalLight();
            if (mainLight != null)
            {
                //Gets light component of main sun light
                Light lightComponent = mainLight.GetComponent<Light>();
                if (lightComponent != null)
                {
                    //Stores the light rotation
                    Vector3 lightRotation = mainLight.transform.rotation.eulerAngles;

                    if (m_selectedSkyboxProfile != null)
                    {
                        //Gets the rotation
                        m_selectedSkyboxProfile.skyboxRotation = lightRotation.y;
                        //Gets the pitch
                        m_selectedSkyboxProfile.skyboxPitch = lightRotation.x;
                    }

                    if (m_selectedProceduralSkyboxProfile != null)
                    {
                        //Gets the procedural rotation
                        m_selectedProceduralSkyboxProfile.proceduralSkyboxRotation = lightRotation.y;
                        //Gets the procedural pitch
                        m_selectedProceduralSkyboxProfile.proceduralSkyboxPitch = lightRotation.x;
                    }

                    if (m_selectedSkyboxProfile != null)
                    {
                        //Gets the sun intensity
                        m_selectedSkyboxProfile.sunIntensity = lightComponent.intensity;
                        if (m_selectedProceduralSkyboxProfile != null)
                        {
                            //Gets the procedural sun intensity
                            m_selectedProceduralSkyboxProfile.proceduralSunIntensity = lightComponent.intensity;
                        }
                    }

                    if (m_selectedProceduralSkyboxProfile != null)
                    {
                        //Gets the sun color
                        m_selectedSkyboxProfile.sunColor = lightComponent.color;
                        if (m_selectedProceduralSkyboxProfile != null)
                        {
                            //Gets the procedural sun color
                            m_selectedProceduralSkyboxProfile.proceduralSunColor = lightComponent.color;
                        }
                    }
                }

            }

            Volume volumeObject = GameObject.FindObjectOfType<Volume>();

            //Finds the volume profine for the environment
            VolumeProfile volumeProfile = volumeObject.sharedProfile;
            if (volumeProfile != null)
            {
                //If the profile has the visual environment added to it
                if (volumeProfile.Has<VisualEnvironment>())
                {
                    //Local visual environment component
                    VisualEnvironment visual;
                    if (volumeProfile.TryGet(out visual))
                    {
                        //If it's = to Gradient
                        if (visual.skyType == 1)
                        {
                            m_profiles.m_systemTypes = AmbientSkiesConsts.SystemTypes.AmbientGradientSkies;
                        }
                        //If it's = to HDRI sky
                        else if (visual.skyType == 2)
                        {
                            m_profiles.m_systemTypes = AmbientSkiesConsts.SystemTypes.AmbientHDRISkies;
                        }
                        //If it's = to Procedural sky
                        else if (visual.skyType == 3)
                        {
                            m_profiles.m_systemTypes = AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies;
                        }
                    }
                }

                //If the profile has a gradient sky added to it
                if (volumeProfile.Has<GradientSky>())
                {
                    GradientSky gradientSky;
                    if (volumeProfile.TryGet(out gradientSky))
                    {
                        m_selectedGradientSkyboxProfile.topColor = gradientSky.top.value;
                        m_selectedGradientSkyboxProfile.middleColor = gradientSky.middle.value;
                        m_selectedGradientSkyboxProfile.bottomColor = gradientSky.bottom.value;
                        m_selectedGradientSkyboxProfile.gradientDiffusion = gradientSky.gradientDiffusion.value;
                    }
                }

                //If the profile has a HDRI sky to it
                if (volumeProfile.Has<HDRISky>())
                {
                    HDRISky hDRISky;
                    if (volumeProfile.TryGet(out hDRISky))
                    {
                        m_selectedSkyboxProfile.customSkybox = hDRISky.hdriSky.value;
                        m_selectedSkyboxProfile.skyboxExposure = hDRISky.exposure.value;
                        m_selectedSkyboxProfile.skyMultiplier = hDRISky.multiplier.value;
                        m_selectedSkyboxProfile.skyboxRotation = hDRISky.rotation.value;
                    }
                }

                //If the profile has volumetric fog added to it
                if (volumeProfile.Has<UnityEngine.Rendering.HighDefinition.Fog>())
                {
                    UnityEngine.Rendering.HighDefinition.Fog fog;
                    if (volumeProfile.TryGet(out fog))
                    {
                        m_selectedSkyboxProfile.singleScatteringAlbedo = fog.albedo.value;
                        m_selectedSkyboxProfile.volumetricBaseFogDistance = fog.meanFreePath.value;
                        m_selectedSkyboxProfile.volumetricBaseFogHeight = fog.baseHeight.value;
                        m_selectedSkyboxProfile.volumetricGlobalAnisotropy = fog.globalLightProbeDimmer.value;
                        m_selectedSkyboxProfile.volumetricGlobalLightProbeDimmer = fog.globalLightProbeDimmer.value;
                        m_selectedSkyboxProfile.volumetricMaxFogDistance = fog.maxFogDistance.value;
                        m_selectedSkyboxProfile.volumetricMipFogNear = fog.mipFogNear.value;
                        m_selectedSkyboxProfile.volumetricMipFogFar = fog.mipFogFar.value;
                        m_selectedSkyboxProfile.volumetricMipFogMaxMip = fog.mipFogMaxMip.value;
                    }
                }
                //If the profile has HD Shadow Settings added to it
                if (volumeProfile.Has<HDShadowSettings>())
                {
                    HDShadowSettings shadowSettings;
                    if (volumeProfile.TryGet(out shadowSettings))
                    {
                        m_selectedSkyboxProfile.shadowDistance = shadowSettings.maxShadowDistance.value;

                        if (shadowSettings.cascadeShadowSplitCount.value == 0)
                        {
                            m_selectedSkyboxProfile.cascadeCount = AmbientSkiesConsts.ShadowCascade.CascadeCount1;
                        }
                        else if (shadowSettings.cascadeShadowSplitCount.value == 1)
                        {
                            m_selectedSkyboxProfile.cascadeCount = AmbientSkiesConsts.ShadowCascade.CascadeCount2;
                        }
                        else if (shadowSettings.cascadeShadowSplitCount.value == 2)
                        {
                            m_selectedSkyboxProfile.cascadeCount = AmbientSkiesConsts.ShadowCascade.CascadeCount3;
                        }
                        else
                        {
                            m_selectedSkyboxProfile.cascadeCount = AmbientSkiesConsts.ShadowCascade.CascadeCount4;
                        }

                        m_selectedSkyboxProfile.cascadeSplit1 = shadowSettings.cascadeShadowSplit0.value;
                        m_selectedSkyboxProfile.cascadeSplit2 = shadowSettings.cascadeShadowSplit1.value;
                        m_selectedSkyboxProfile.cascadeSplit3 = shadowSettings.cascadeShadowSplit2.value;
                    }
                }

                //If the profile has Contact Shadow added to it
                if (volumeProfile.Has<ContactShadows>())
                {
                    ContactShadows contact;
                    if (volumeProfile.TryGet(out contact))
                    {
                        m_selectedSkyboxProfile.useContactShadows = contact.enable.value;
                        m_selectedSkyboxProfile.contactShadowsLength = contact.length.value;
                        m_selectedSkyboxProfile.contactShadowsDistanceScaleFactor = contact.distanceScaleFactor.value;
                        m_selectedSkyboxProfile.contactShadowsMaxDistance = contact.maxDistance.value;
                        m_selectedSkyboxProfile.contactShadowsFadeDistance = contact.fadeDistance.value;
                        m_selectedSkyboxProfile.contactShadowsSampleCount = contact.sampleCount;
                        m_selectedSkyboxProfile.contactShadowsOpacity = contact.opacity.value;
                    }
                }

                //If the profile has Micro Shadow added to it
                if (volumeProfile.Has<MicroShadowing>())
                {
                    MicroShadowing micro;
                    if (volumeProfile.TryGet(out micro))
                    {
                        m_selectedSkyboxProfile.useMicroShadowing = micro.enable.value;
                        m_selectedSkyboxProfile.microShadowOpacity = micro.opacity.value;
                    }
                }

                //If the profile has Micro Shadow added to it
                if (volumeProfile.Has<MicroShadowing>())
                {
                    MicroShadowing microShadowing;
                    if (volumeProfile.TryGet(out microShadowing))
                    {
                        m_selectedSkyboxProfile.useMicroShadowing = microShadowing.active;
                        m_selectedSkyboxProfile.microShadowOpacity = microShadowing.opacity.value;
                    }
                }

                //If the profile has Screen Space Reflection added to it
                if (volumeProfile.Has<ScreenSpaceReflection>())
                {
                    ScreenSpaceReflection screenSpace;
                    if (volumeProfile.TryGet(out screenSpace))
                    {
                        m_selectedSkyboxProfile.enableScreenSpaceReflections = screenSpace.active;
                        m_selectedSkyboxProfile.screenEdgeFadeDistance = screenSpace.screenFadeDistance.value;
                        m_selectedSkyboxProfile.maxNumberOfRaySteps = screenSpace.rayMaxIterations;
                        m_selectedSkyboxProfile.objectThickness = screenSpace.depthBufferThickness.value;
                        m_selectedSkyboxProfile.minSmoothness = screenSpace.minSmoothness.value;
                        m_selectedSkyboxProfile.smoothnessFadeStart = screenSpace.smoothnessFadeStart.value;
                        m_selectedSkyboxProfile.reflectSky = screenSpace.reflectSky.value;
                    }
                }

                //If the profile has Screen Space Refraction added to it
                if (volumeProfile.Has<ScreenSpaceRefraction>())
                {
                    ScreenSpaceRefraction spaceRefraction;
                    if (volumeProfile.TryGet(out spaceRefraction))
                    {
                        m_selectedSkyboxProfile.enableScreenSpaceRefractions = spaceRefraction.active;
                        m_selectedSkyboxProfile.screenWeightDistance = spaceRefraction.screenFadeDistance.value;
                    }
                }
            }

            //Gets the shadow mask mode
            m_selectedSkyboxProfile.shadowmaskMode = QualitySettings.shadowmaskMode;

            //If vsync mode is on DontSync
            if (QualitySettings.vSyncCount == 0)
            {
                //Sets the vsync mode to DontSync
                m_profiles.m_vSyncMode = AmbientSkiesConsts.VSyncMode.DontSync;
            }
            //If vsync mode is on EveryVBlank
            else if (QualitySettings.vSyncCount == 1)
            {
                //Sets the vsync mode to EveryVBlank
                m_profiles.m_vSyncMode = AmbientSkiesConsts.VSyncMode.EveryVBlank;
            }
            //If vsync mode is on EverySecondVBlank
            else
            {
                //Sets the vsync mode to EverySecondVBlank
                m_profiles.m_vSyncMode = AmbientSkiesConsts.VSyncMode.EverySecondVBlank;
            }
#endif
        }

        /// <summary>
        /// This method saves all the settings for the Built-in and LWRP pipelines
        /// </summary>
        public static void SaveBuiltInAndLWRPSettings(AmbientSkyProfiles m_profiles, CreationToolsSettings m_creationToolSettings, AmbientSkiesConsts.SystemTypes systemtype, int m_selectedSkyboxProfileIndex, AmbientSkyboxProfile m_selectedSkyboxProfile, AmbientProceduralSkyboxProfile m_selectedProceduralSkyboxProfile, AmbientGradientSkyboxProfile m_selectedGradientSkyboxProfile, float skyboxRotation, int m_selectedPostProcessingProfileIndex, AmbientPostProcessingProfile m_selectedPostProcessingProfile, bool usePostProcess)
        {
            //Marks the profiles as dirty so settings can be saved
            EditorUtility.SetDirty(m_profiles);
            EditorUtility.SetDirty(m_creationToolSettings);

            m_profiles.m_systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;
            systemtype = AmbientSkiesConsts.SystemTypes.ThirdParty;

            NewSceneObjectCreation();

            //Finds user profile
            m_selectedSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromProfileName(m_profiles, "User");
            //Sets the skybox profile settings to User
            if (m_selectedSkyboxProfileIndex > m_profiles.m_skyProfiles.Count)
            {
                m_selectedSkyboxProfileIndex = 0;
            }

            m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];

            //Get main sun light
            GameObject mainLight = SkyboxUtils.GetMainDirectionalLight();
            if (mainLight != null)
            {
                //Gets light component of main sun light
                Light lightComponent = mainLight.GetComponent<Light>();
                if (lightComponent != null)
                {
                    //Stores the light rotation
                    Vector3 lightRotation = mainLight.transform.rotation.eulerAngles;

                    if (m_selectedSkyboxProfile != null)
                    {
                        //Gets the rotation
                        m_selectedSkyboxProfile.skyboxRotation = lightRotation.y;
                        //Gets the pitch
                        m_selectedSkyboxProfile.skyboxPitch = lightRotation.x;

                        //Gets the sun color
                        m_selectedSkyboxProfile.sunColor = lightComponent.color;
                        //Gets the sun intensity
                        m_selectedSkyboxProfile.sunIntensity = lightComponent.intensity;

                        //Gets fog color
                        m_selectedSkyboxProfile.fogColor = RenderSettings.fogColor;
                        //Gets fog density
                        m_selectedSkyboxProfile.fogDensity = RenderSettings.fogDensity;
                        //Gets fog end distance
                        m_selectedSkyboxProfile.fogDistance = RenderSettings.fogEndDistance;
                        //Gets start fog distance
                        m_selectedSkyboxProfile.nearFogDistance = RenderSettings.fogStartDistance;
                    }
                }
            }

            if (m_selectedSkyboxProfile != null)
            {
                //If fog is not enabled
                if (!RenderSettings.fog)
                {
                    //Gets fog enabled to false
                    m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.None;
                }
                //If fog mode equals Exponential
                else if (RenderSettings.fogMode == FogMode.Exponential)
                {
                    //Gets fog to Exponential
                    m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.Exponential;
                }
                //If fog mode equal Exponential Squared
                else if (RenderSettings.fogMode == FogMode.ExponentialSquared)
                {
                    //Get fog to Exponential Squared
                    m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.ExponentialSquared;
                }
                //If fog mode equals Linear
                else
                {
                    //Gets fog to Linear
                    m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.Linear;
                }

                //Gets the skybox ambient intensity
                m_selectedSkyboxProfile.skyboxGroundIntensity = RenderSettings.ambientIntensity;
                //Gets the sky ambeint color
                m_selectedSkyboxProfile.skyColor = RenderSettings.ambientSkyColor;
                //Gets the ground ambient color
                m_selectedSkyboxProfile.groundColor = RenderSettings.ambientGroundColor;
                //Gets the equator ambient color
                m_selectedSkyboxProfile.equatorColor = RenderSettings.ambientEquatorColor;

                //If the ambient mode is Flat
                if (RenderSettings.ambientMode == AmbientMode.Flat)
                {
                    //Sets the ambient mode to Color
                    m_selectedSkyboxProfile.ambientMode = AmbientSkiesConsts.AmbientMode.Color;
                }
                //If the ambient mode is Trilight
                else if (RenderSettings.ambientMode == AmbientMode.Trilight)
                {
                    //Sets the ambient mode to Gradient
                    m_selectedSkyboxProfile.ambientMode = AmbientSkiesConsts.AmbientMode.Gradient;
                }
                //If the ambient mode is Skybox
                else if (RenderSettings.ambientMode == AmbientMode.Skybox)
                {
                    //Sets the ambient mode to Skybox
                    m_selectedSkyboxProfile.ambientMode = AmbientSkiesConsts.AmbientMode.Skybox;
                }

                //If skybox is not empty
                if (RenderSettings.skybox != null)
                {
                    //Stores the local material
                    Material skyboxMaterial = RenderSettings.skybox;
                    //If the skybox shader equals Procedural
                    if (skyboxMaterial.shader == Shader.Find("Skybox/Procedural"))
                    {
                        /*
                        //If sun disk is enabled
                        if (skyboxMaterial.GetFloat("_SunDisk") == 2f)
                        {
                            //Get sun disk and set it as true
                            m_selectedSkyboxProfile.enableSunDisk = true;
                        }
                        //If sun disk is not enabled
                        else
                        {
                            //Get sun disk and set it as false
                            m_selectedSkyboxProfile.enableSunDisk = false;
                        }
                        */

                        //Sets the custom skybox as procedural sky
                        m_selectedSkyboxProfile.isProceduralSkybox = true;
                        //Gets the sun size
                        m_selectedSkyboxProfile.customProceduralSunSize = skyboxMaterial.GetFloat("_SunSize");
                        //Gets the sun size convergence
                        m_selectedSkyboxProfile.customProceduralSunSizeConvergence = skyboxMaterial.GetFloat("_SunSizeConvergence");
                        //Gets the atmosphere thickness
                        m_selectedSkyboxProfile.customProceduralAtmosphereThickness = skyboxMaterial.GetFloat("_AtmosphereThickness");
                        //Gets the ground color
                        m_selectedSkyboxProfile.customProceduralGroundColor = skyboxMaterial.GetColor("_GroundColor");
                        //Gets the skybox tint
                        m_selectedSkyboxProfile.customSkyboxTint = skyboxMaterial.GetColor("_SkyTint");
                        //Gets the skybox exposure
                        m_selectedSkyboxProfile.customSkyboxExposure = skyboxMaterial.GetFloat("_Exposure");
                    }
                    //If the skybox shader equals Cubemap
                    else if (skyboxMaterial.shader == Shader.Find("Skybox/Cubemap"))
                    {
                        //Sets the custom skybox not as procedural sky
                        m_selectedSkyboxProfile.isProceduralSkybox = false;
                        //Gets Skybox cubemap texture
                        m_selectedSkyboxProfile.customSkybox = skyboxMaterial.GetTexture("_Tex") as Cubemap;
                        //Gets the skybox tint
                        m_selectedSkyboxProfile.skyboxTint = skyboxMaterial.GetColor("_Tint");
                        //Gets the skybox exposure
                        m_selectedSkyboxProfile.skyboxExposure = skyboxMaterial.GetFloat("_Exposure");
                        //Sets the hdri skybox rotation
                        skyboxMaterial.SetFloat("_Rotation", skyboxRotation);
                    }
                }

                //Defaults the system type to third party to stop changing settings unless user enabled system
                //m_selectedSkyboxProfile.systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;
                //Gets the shadow distance
                m_selectedSkyboxProfile.shadowDistance = QualitySettings.shadowDistance;
                //Gets the shadow mask mode
                m_selectedSkyboxProfile.shadowmaskMode = QualitySettings.shadowmaskMode;
                //Gets the shadow projection
                m_selectedSkyboxProfile.shadowProjection = QualitySettings.shadowProjection;
                //Getws the shadow resolution
                m_selectedSkyboxProfile.shadowResolution = QualitySettings.shadowResolution;

                //If vsync mode is on DontSync
                if (QualitySettings.vSyncCount == 0)
                {
                    //Sets the vsync mode to DontSync
                    m_profiles.m_vSyncMode = AmbientSkiesConsts.VSyncMode.DontSync;
                }
                //If vsync mode is on EveryVBlank
                else if (QualitySettings.vSyncCount == 1)
                {
                    //Sets the vsync mode to EveryVBlank
                    m_profiles.m_vSyncMode = AmbientSkiesConsts.VSyncMode.EveryVBlank;
                }
                //If vsync mode is on EverySecondVBlank
                else
                {
                    //Sets the vsync mode to EverySecondVBlank
                    m_profiles.m_vSyncMode = AmbientSkiesConsts.VSyncMode.EverySecondVBlank;
                }
            }

#if UNITY_POST_PROCESSING_STACK_V2
            GameObject postProcessingObject = GameObject.Find("Global Post Processing");
            PostProcessVolume processVol;
            if (postProcessingObject != null)
            {
                processVol = postProcessingObject.GetComponent<PostProcessVolume>();
            }
            else
            {
                processVol = GameObject.FindObjectOfType<PostProcessVolume>();
            }

            if (processVol != null)
            {
                //Finds user profile
                m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromProfileName(m_profiles, "User");
                //Sets the post fx profile settings to User
                m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_creationToolSettings.m_selectedPostProcessing];

                PostProcessProfile profileFX = processVol.sharedProfile;
                if (m_selectedPostProcessingProfile != null)
                {
                    if (profileFX != null)
                    {
                        usePostProcess = true;
                        m_profiles.m_usePostFX = true;
                        ///FIX ASAP
                        //m_selectedPostProcessingProfile.assetName = profileFX.name;
                        ///
                        m_selectedPostProcessingProfile.customPostProcessingProfile = profileFX;
                        //Update post processing
                        PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);

                    }
                    else
                    {
                        m_profiles.m_usePostFX = false;
                    }

                    GameObject.DestroyImmediate(processVol.gameObject);
                }
            }
            else
            {
                m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromProfileName(m_profiles, "Alpine");
                m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                m_profiles.m_usePostFX = false;
            }
#endif
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Cleans up Lightweight Render Pipeline
        /// </summary>
        /// <param name="updateMultiScene"></param>
        /// <param name="profiles"></param>
        private static void CleanUpLWRP(bool updateMultiScene, AmbientSkyProfiles profiles)
        {
#if LWPipeline
            //Remove Post Processing
            GameObject lwPostProcessing = GameObject.Find("Global Post Processing");
            if (lwPostProcessing != null)
            {
                GameObject.DestroyImmediate(lwPostProcessing);
            }

            //Remove light data
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            if (lights.Length > 0)
            {
                List<UniversalAdditionalLightData> lightDatas = new List<UniversalAdditionalLightData>();
                foreach(Light light in lights)
                {
                    if (light.GetComponent<UniversalAdditionalLightData>() != null)
                    {
                        lightDatas.Add(light.GetComponent<UniversalAdditionalLightData>());
                    }
                }

                if (lightDatas.Count > 0)
                {
                    foreach(UniversalAdditionalLightData data in lightDatas)
                    {
                        GameObject.DestroyImmediate(data);
                    }
                }
            }

            //Remove camera data
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
            {
                List<UniversalAdditionalCameraData> cameraDatas = new List<UniversalAdditionalCameraData>();
                foreach (Camera camera in cameras)
                {
                    if (camera.GetComponent<UniversalAdditionalCameraData>() != null)
                    {
                        cameraDatas.Add(camera.GetComponent<UniversalAdditionalCameraData>());
                    }
                }

                if (cameraDatas.Count > 0)
                {
                    foreach (UniversalAdditionalCameraData data in cameraDatas)
                    {
                        GameObject.DestroyImmediate(data);
                    }
                }
            }

            if (updateMultiScene)
            {
                AmbientSkiesSceneSavedSettings settings = GameObject.FindObjectOfType<AmbientSkiesSceneSavedSettings>();
                if (settings != null)
                {
                    settings.SetSettings(profiles);
                }
            }
#endif
        }

        /// <summary>
        /// Cleans up Universal Render Pipeline
        /// </summary>
        /// <param name="updateMultiScene"></param>
        /// <param name="profiles"></param>
        private static void CleanUpURP(bool updateMultiScene, AmbientSkyProfiles profiles, CreationToolsSettings creationTools )
        {
#if UPPipeline
            //Remove Post Processing
            GameObject urpPostProcessing = GameObject.Find("Post Processing URP Volume");
            if (urpPostProcessing != null)
            {
                GameObject.DestroyImmediate(urpPostProcessing);
            }

            //Remove light data
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            if (lights.Length > 0)
            {
                List<UniversalAdditionalLightData> lightDatas = new List<UniversalAdditionalLightData>();
                foreach(Light light in lights)
                {
                    if (light.GetComponent<UniversalAdditionalLightData>() != null)
                    {
                        lightDatas.Add(light.GetComponent<UniversalAdditionalLightData>());
                    }
                }

                if (lightDatas.Count > 0)
                {
                    foreach(UniversalAdditionalLightData data in lightDatas)
                    {
                        GameObject.DestroyImmediate(data);
                    }
                }
            }

            //Remove camera data
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
            {
                List<UniversalAdditionalCameraData> cameraDatas = new List<UniversalAdditionalCameraData>();
                foreach (Camera camera in cameras)
                {
                    if (camera.GetComponent<UniversalAdditionalCameraData>() != null)
                    {
                        cameraDatas.Add(camera.GetComponent<UniversalAdditionalCameraData>());
                    }
                }

                if (cameraDatas.Count > 0)
                {
                    foreach (UniversalAdditionalCameraData data in cameraDatas)
                    {
                        GameObject.DestroyImmediate(data);
                    }
                }
            }

            if (updateMultiScene)
            {
                AmbientSkiesSceneSavedSettings settings = GameObject.FindObjectOfType<AmbientSkiesSceneSavedSettings>();
                if (settings != null)
                {
                    settings.SetSettings(profiles, creationTools);
                }
            }
#endif
        }

        /// <summary>
        /// Cleans up High Definition Render Pipeline
        /// </summary>
        /// <param name="updateMultiScene"></param>
        /// <param name="profiles"></param>
        private static void CleanUpHDRP(bool updateMultiScene, AmbientSkyProfiles profiles, CreationToolsSettings creationTools)
        {
#if HDPipeline
            //Remove Post Processing
            GameObject hdPostProcessing = GameObject.Find("Post Processing HDRP Volume");
            if (hdPostProcessing != null)
            {
                GameObject.DestroyImmediate(hdPostProcessing);
            }

            //Remove Density Volume
            GameObject desnityVolume = GameObject.Find("Density Volume");
            if (desnityVolume != null)
            {
                GameObject.DestroyImmediate(desnityVolume);
            }

            //Remove Sky Volume
            GameObject skyVolume = GameObject.Find("High Definition Environment Volume");
            if (skyVolume != null)
            {
                GameObject.DestroyImmediate(skyVolume);
            }

            //Remove light data
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            if (lights.Length > 0)
            {
                List<HDAdditionalLightData> lightDatas = new List<HDAdditionalLightData>();
                foreach (Light light in lights)
                {
                    if (light.GetComponent<HDAdditionalLightData>() != null)
                    {
                        lightDatas.Add(light.GetComponent<HDAdditionalLightData>());
                    }
                }

                if (lightDatas.Count > 0)
                {
                    foreach (HDAdditionalLightData data in lightDatas)
                    {
                        GameObject.DestroyImmediate(data);
                    }
                }
            }

            //Remove camera data
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
            {
                List<HDAdditionalCameraData> cameraDatas = new List<HDAdditionalCameraData>();
                foreach (Camera camera in cameras)
                {
                    if (camera.GetComponent<HDAdditionalCameraData>() != null)
                    {
                        cameraDatas.Add(camera.GetComponent<HDAdditionalCameraData>());
                    }
                }

                if (cameraDatas.Count > 0)
                {
                    foreach (HDAdditionalCameraData data in cameraDatas)
                    {
                        GameObject.DestroyImmediate(data);
                    }
                }
            }

            if (updateMultiScene)
            {
                AmbientSkiesSceneSavedSettings settings = GameObject.FindObjectOfType<AmbientSkiesSceneSavedSettings>();
                if (settings != null)
                {
                    settings.SetSettings(profiles, creationTools);
                }
            }

#endif
        }

        /// <summary>
        /// Cleans up Built-In Render Pipeline
        /// </summary>
        /// <param name="updateMultiScene"></param>
        /// <param name="profiles"></param>
        private static void CleanUpBuiltIn(bool updateMultiScene, AmbientSkyProfiles profiles, CreationToolsSettings creationTools)
        {
            //Remove Post Processing
            GameObject postProcessing = GameObject.Find("Global Post Processing");
            if (postProcessing != null)
            {
                Object.DestroyImmediate(postProcessing);
            }
        }

        /// <summary>
        /// Cleans up all Render Pipelines
        /// </summary>
        /// <param name="updateMultiScene"></param>
        /// <param name="profiles"></param>
        private static void CleanUpAll(bool updateMultiScene, AmbientSkyProfiles profiles, CreationToolsSettings creationTools)
        {
            CleanUpLWRP(updateMultiScene, profiles);
            CleanUpHDRP(updateMultiScene, profiles, creationTools);
            CleanUpURP(updateMultiScene, profiles, creationTools);
            CleanUpBuiltIn(updateMultiScene, profiles, creationTools);
        }

        #endregion

        #region Post FX Functions

        /// <summary>
        /// Matches a post processing profile to the selected skybox name
        /// </summary>
        /// <param name="profiles">Profile list to search</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetPostProcessingHDRIName(AmbientSkyProfiles profiles, AmbientSkyboxProfile profile)
        {
            //Check if asset name is there
            if (string.IsNullOrEmpty(profile.postProcessingAssetName))
            {
                Debug.LogError("Warning matched post processing asset name is empty Please insure the string Post Processing Asset Name has an asset name. First profile found will be selected in replacement due to this error");
                return 0;
            }

            //Sky Five Low
            if (profile.name == "Sky Five Low")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Sky Five Mid
            else if (profile.name == "Sky Five Mid")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Sky Five High
            else if (profile.name == "Sky Five High")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Sky Six Low
            else if (profile.name == "Sky Six Low")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Sky Six Mid
            else if (profile.name == "Sky Six Mid")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Sky Six High
            else if (profile.name == "Sky Six High")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Sky One Low
            else if (profile.name == "Sky One Low")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Sky One Mid
            else if (profile.name == "Sky One Mid")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Sky One High
            else
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// Matches a post processing profile to the selected skybox name
        /// </summary>
        /// <param name="profiles">Profile list to search</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetPostProcessingProceduralName(AmbientSkyProfiles profiles, AmbientProceduralSkyboxProfile profile)
        {
            //Check if asset name is there
            if (string.IsNullOrEmpty(profile.postProcessingAssetName))
            {
                Debug.LogError("Warning matched post processing asset name is empty Please insure the string Post Processing Asset Name has an asset name. First profile found will be selected in replacement due to this error");
                return 0;
            }

            //Sunny Morning
            if (profile.name == "Sunny Morning")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Clear Day
            else if (profile.name == "Clear Day")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Sunny Evening
            else if (profile.name == "Sunny Evening")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Clear Night
            else if (profile.name == "Clear Night")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Foggy Morning
            else if (profile.name == "Foggy Morning")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Foggy Day
            else if (profile.name == "Foggy Day")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Foggy Evening
            else if (profile.name == "Foggy Evening")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Foggy Night
            else if (profile.name == "Foggy Night")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Overcast Morning
            else if (profile.name == "Overcast Morning")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Overcast Day
            else if (profile.name == "Overcast Day")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Overcast Evening
            else if (profile.name == "Overcast Evening")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Overcast Night
            else
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// Matches a post processing profile to the selected skybox name
        /// </summary>
        /// <param name="profiles">Profile list to search</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetPostProcessingGradientName(AmbientSkyProfiles profiles, AmbientGradientSkyboxProfile profile)
        {
            //Check if asset name is there
            if (string.IsNullOrEmpty(profile.postProcessingAssetName))
            {
                Debug.LogError("Warning matched post processing asset name is empty Please insure the string Post Processing Asset Name has an asset name. First profile found will be selected in replacement due to this error");
                return -1;
            }

            //Morning
            if (profile.name == "Morning")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Day
            else if (profile.name == "Day")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Evening
            else if (profile.name == "Evening")
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
            //Night
            else
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].name == profile.postProcessingAssetName)
                    {
                        return idx;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// Udpates fog distance to camera
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="divisionAmount"></param>
        public static float UpdateDepthOfFieldEndDistanceToTerrain(float divisionAmount, Camera mainCam)
        {
            Terrain terrain = Terrain.activeTerrain;
            float newDistance = 500f;

            if (terrain != null)
            {
                newDistance = Mathf.Round(terrain.terrainData.size.x / divisionAmount);
                return newDistance;
            }

            if (mainCam == null)
            {
                mainCam = GameObject.FindObjectOfType<Camera>();
            }

            if (mainCam != null)
            {
                newDistance = Mathf.Round(mainCam.farClipPlane / divisionAmount);
                return newDistance;
            }

            return newDistance;
        }

        /// <summary>
        /// Creates a post processing v2 volume
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
#if UNITY_POST_PROCESSING_STACK_V2
        public static GameObject CreateVolumeFromPostProcessingV2Profile(PostProcessProfile profile)
        {
            GameObject volumeObject = null;
            if (profile == null)
            {
                Debug.LogError("CreateVolumeFromPostProcessingV2Profile() missing profile to use in the function. Please make sure the profile is not null before calling the function");
            }
            else
            {
                volumeObject = new GameObject("New Post Processing Volume");
                volumeObject.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
                volumeObject.layer = 2;
                ParentVolumeToAParent(volumeObject);

                BoxCollider boxCollider = volumeObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(200f, 200f, 200f);

                PostProcessVolume processVolume = volumeObject.AddComponent<PostProcessVolume>();
                processVolume.priority = 1;
                processVolume.sharedProfile = profile;
            }

            return volumeObject;
        }
#endif

        /// <summary>
        /// Creates a post processing pipeline volume
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
#if HDPipeline || LWPipeline || UPPipeline
        public static GameObject CreateVolumeFromPostProcessingPipelineProfile(VolumeProfile profile)
        {
            GameObject volumeObject = null;
            if (profile == null)
            {
                Debug.LogError("CreateVolumeFromPostProcessingPipelineProfile() missing profile to use in the function. Please make sure the profile is not null before calling the function");
            }
            else
            {
                volumeObject = new GameObject("New Post Processing Volume");
                volumeObject.transform.position = SceneView.currentDrawingSceneView.camera.transform.position;
                ParentVolumeToAParent(volumeObject);

                BoxCollider boxCollider = volumeObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(200f, 200f, 200f);

                Volume processVolume = volumeObject.AddComponent<Volume>();
                processVolume.priority = 1;
                processVolume.sharedProfile = profile;
            }

            return volumeObject;
        }
#endif

        /// <summary>
        /// Parents the object
        /// </summary>
        /// <param name="gameObject"></param>
        public static void ParentVolumeToAParent(GameObject gameObject)
        {
            GameObject parentRootObject = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Global", false);
            if (gameObject != null)
            {
                GameObject parentObject = GameObject.Find("Post Processing Volume Areas");
                if (parentObject == null)
                {
                    parentObject = new GameObject("Post Processing Volume Areas");
                }

                parentObject.transform.SetParent(parentRootObject.transform);
                gameObject.transform.SetParent(parentObject.transform);
            }
        }

        #endregion

        #region Set Scripting Defines

        /// <summary>
        /// Set up the High Definition defines
        /// </summary>
        public static void SetHighDefinitionDefinesStatic()
        {
#if UNITY_2018_3_OR_NEWER
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject HDPipeline
            if (!currBuildSettings.Contains("HDPipeline"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";HDPipeline");
            }
#endif
        }

        /// <summary>
        /// Set up the Lightweight defines
        /// </summary>
        public static void SetLightweightDefinesStatic()
        {
#if UNITY_2018_3_OR_NEWER
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject LWPipeline
            if (!currBuildSettings.Contains("LWPipeline"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";LWPipeline");
            }
#endif
        }

        /// <summary>
        /// Set up the Lightweight defines
        /// </summary>
        public static void SetAmbientSkiesDefinesStatic()
        {
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject LWPipeline
            if (!currBuildSettings.Contains("AMBIENT_SKIES"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";AMBIENT_SKIES");
            }
        }

        #endregion
    }
}