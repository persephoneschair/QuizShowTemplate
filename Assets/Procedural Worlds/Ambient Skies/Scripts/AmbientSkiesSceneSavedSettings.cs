using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AmbientSkies
{
    public class AmbientSkiesSceneSavedSettings : MonoBehaviour
    {
        #region Variables

        [Header("Profile Settings")]

        public int m_hdriSelectedProfile;
        public int m_proceduralSelectedProfile;
        public int m_gradientSelectedProfile;
        public int m_postProcessSelectedProfile;
        public int m_selectedSystemType;

        [SerializeField]
        public bool m_editSettings = false;

        [SerializeField]
        public int m_inspectorUpdateLimit = 2;

        [SerializeField]
        [HideInInspector]
        public string m_version = "Version 1.0";

        [SerializeField]
        public CreationToolsSettings m_CreationToolSettings;

        [Header("Debug Settings")]

        [SerializeField]
        public bool m_showDebug = false;

        [SerializeField]
        public bool m_showTimersInDebug = false;

        [SerializeField]
        public bool m_showHasChangedDebug = false;

        [SerializeField]
        public bool m_showFunctionDebugsOnly = false;

        [SerializeField]
        public bool m_smartConsoleClean = false;

        [SerializeField]
        [HideInInspector]
        public bool m_isProceduralCreatedProfile = true;

        #region Global Settings

        [Header("Global Settings")]

        public string m_skyboxShaderName;
        public Material m_skyboxMaterial;

        [SerializeField]
        public AmbientSkiesConsts.SystemTypes m_systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;

        [SerializeField]
        [HideInInspector]
        public AmbientSkiesConsts.RenderPipelineSettings m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;

        [SerializeField]
        [HideInInspector]
        public AmbientSkiesConsts.PlatformTarget m_targetPlatform = AmbientSkiesConsts.PlatformTarget.DesktopAndConsole;

        [SerializeField]
        public AmbientSkiesConsts.DisableAndEnable m_useTimeOfDay;

        [SerializeField]
        [HideInInspector]
        public AmbientSkiesConsts.VSyncMode m_vSyncMode = AmbientSkiesConsts.VSyncMode.DontSync;

        [SerializeField]
        [HideInInspector]
        public AmbientSkiesConsts.AutoConfigureType m_configurationType = AmbientSkiesConsts.AutoConfigureType.Terrain;

        [SerializeField]
        [HideInInspector]
        public bool m_autoMatchProfile = true;

        #endregion

        #region Interior Settings
        [Header("Interior Settings")]

        [SerializeField]
        //[HideInInspector]
        public AmbientSkiesConsts.EnvironmentLightingEditMode m_lightingEditMode = AmbientSkiesConsts.EnvironmentLightingEditMode.Interior;

        [SerializeField]
        //[HideInInspector]
        public string m_lightName = "New Light";

        [SerializeField]
        //[HideInInspector]
        public bool m_spawnAtCamera = true;

        [SerializeField]
        //[HideInInspector]
        public bool m_parentToActiveObject = true;

        #endregion

        #region System Enable Bools

        [Header("Systems Enabled")]

        [SerializeField]
        public bool m_useSkies;

        [SerializeField]
        public bool m_usePostFX;

        [SerializeField]
        public bool m_useLighting;

        #endregion

        #region Profiles

        [Header("Profiles")]

        [SerializeField]
        public List<AmbientSkyboxProfile> m_skyProfiles = new List<AmbientSkyboxProfile>();

        [SerializeField]
        public List<AmbientProceduralSkyboxProfile> m_proceduralSkyProfiles = new List<AmbientProceduralSkyboxProfile>();

        [SerializeField]
        public List<AmbientGradientSkyboxProfile> m_gradientSkyProfiles = new List<AmbientGradientSkyboxProfile>();

        [SerializeField]
        public List<AmbientPostProcessingProfile> m_ppProfiles = new List<AmbientPostProcessingProfile>();

        [SerializeField]
        public List<AmbientLightingProfile> m_lightingProfiles = new List<AmbientLightingProfile>();

        #endregion

        #endregion

        #region Functions

        public void SetSettings(AmbientSkyProfiles profile, CreationToolsSettings creationTools)
        {
            if (profile.m_showDebug)
            {
                Debug.Log("Set Multi Scene Settings");
            }

            //Profile Settings
            m_editSettings = profile.m_editSettings;
            m_isProceduralCreatedProfile = profile.m_isProceduralCreatedProfile;
            m_inspectorUpdateLimit = profile.m_inspectorUpdateLimit;
            m_version = profile.m_version;
            m_CreationToolSettings = profile.m_CreationToolSettings;
            m_hdriSelectedProfile = creationTools.m_selectedHDRI;
            m_proceduralSelectedProfile = creationTools.m_selectedProcedural;
            m_gradientSelectedProfile = creationTools.m_selectedGradient;
            m_postProcessSelectedProfile = creationTools.m_selectedPostProcessing;
            m_selectedSystemType = creationTools.m_selectedSystem;

            //Debug Settings
            m_showDebug = profile.m_showDebug;
            m_showTimersInDebug = profile.m_showTimersInDebug;
            m_showHasChangedDebug = profile.m_showHasChangedDebug;
            m_showFunctionDebugsOnly = profile.m_showFunctionDebugsOnly;
            m_smartConsoleClean = profile.m_smartConsoleClean;

            //Global Settings
            m_skyboxMaterial = CreateMaterialInstance(profile.m_skyboxMaterial);
            m_systemTypes = profile.m_systemTypes;
            m_selectedRenderPipeline = profile.m_selectedRenderPipeline;
            m_targetPlatform = profile.m_targetPlatform;
            m_useTimeOfDay = profile.m_useTimeOfDay;
            m_vSyncMode = profile.m_vSyncMode;
            m_configurationType = profile.m_configurationType;
            m_autoMatchProfile = profile.m_autoMatchProfile;

            //Interior Settings
            m_lightingEditMode = profile.m_lightingEditMode;
            m_lightName = profile.m_lightName;
            m_spawnAtCamera = profile.m_spawnAtCamera;
            m_parentToActiveObject = profile.m_parentToActiveObject;

            //System Bool Settings
            m_useSkies = profile.m_useSkies;
            m_useLighting = profile.m_useLighting;
            m_usePostFX = profile.m_usePostFX;

            //Profiles
            m_skyProfiles = profile.m_skyProfiles;
            m_proceduralSkyProfiles = profile.m_proceduralSkyProfiles;
            m_gradientSkyProfiles = profile.m_gradientSkyProfiles;
            m_ppProfiles = profile.m_ppProfiles;
            m_lightingProfiles = profile.m_lightingProfiles;

            //Skybox
            if (profile.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
            {
                if (m_skyboxMaterial != null)
                {
                    RenderSettings.skybox = m_skyboxMaterial;
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void GetSettings(AmbientSkyProfiles profile, CreationToolsSettings creationTools)
        {
            if (profile.m_showDebug)
            {
                Debug.Log("Get Multi Scene Settings");
            }

            //Profile Settings
            profile.m_editSettings = m_editSettings;
            profile.m_isProceduralCreatedProfile = m_isProceduralCreatedProfile;
            profile.m_inspectorUpdateLimit = m_inspectorUpdateLimit;
            profile.m_version = m_version;
            profile.m_CreationToolSettings = m_CreationToolSettings;
            creationTools.m_selectedHDRI = m_hdriSelectedProfile;
            creationTools.m_selectedProcedural = m_proceduralSelectedProfile;
            creationTools.m_selectedGradient = m_gradientSelectedProfile;
            creationTools.m_selectedPostProcessing = m_postProcessSelectedProfile;
            creationTools.m_selectedSystem = m_selectedSystemType;

            //Debug Settings
            profile.m_showDebug = m_showDebug;
            profile.m_showTimersInDebug = m_showTimersInDebug;
            profile.m_showHasChangedDebug = m_showHasChangedDebug;
            profile.m_showFunctionDebugsOnly = m_showFunctionDebugsOnly;
            profile.m_smartConsoleClean = m_smartConsoleClean;

            //Global Settings
            profile.m_systemTypes = m_systemTypes;
            profile.m_selectedRenderPipeline = m_selectedRenderPipeline;
            profile.m_targetPlatform = m_targetPlatform;
            profile.m_useTimeOfDay = m_useTimeOfDay;
            profile.m_vSyncMode = m_vSyncMode;
            profile.m_configurationType = m_configurationType;
            profile.m_autoMatchProfile = m_autoMatchProfile;

            //Interior Settings
            profile.m_lightingEditMode = m_lightingEditMode;
            profile.m_lightName = m_lightName;
            profile.m_spawnAtCamera = m_spawnAtCamera;
            profile.m_parentToActiveObject = m_parentToActiveObject;

            //System Bool Settings
            profile.m_useSkies = m_useSkies;
            profile.m_useLighting = m_useLighting;
            profile.m_usePostFX = m_usePostFX;

            //Profiles
            profile.m_skyProfiles = m_skyProfiles;
            profile.m_proceduralSkyProfiles = m_proceduralSkyProfiles;
            profile.m_gradientSkyProfiles = m_gradientSkyProfiles;
            profile.m_ppProfiles = m_ppProfiles;
            profile.m_lightingProfiles = m_lightingProfiles;

            //Skybox
            if (profile.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
            {
                if (RenderSettings.skybox == null) return;
                Material skybox = RenderSettings.skybox;
                if (skybox.name == "Ambient Skies Skybox")
                {
                    skybox.shader = Shader.Find(m_skyboxShaderName);
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(profile);
#endif
        }

        public Material CreateMaterialInstance(Material source)
        {
            Material instance = null;
            if (source != null)
            {
                instance = new Material(source.shader);
                instance.CopyPropertiesFromMaterial(source);
                instance.shader = source.shader;
                return instance;
            }

            return null;
        }

        #endregion
    }
}