//Copyright © 2020 Procedural Worlds Pty Limited. All Rights Reserved.
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AmbientSkies.Internal;
using PWCommon3;
using UnityEngine.Rendering;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.Experimental.Rendering;
using System.Collections;
using UnityEditor.Rendering;
#if Mewlist_Clouds
using Mewlist;
#endif
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif
#if GAIA_2_PRESENT
using Gaia;
#endif

namespace AmbientSkies
{
    /// <summary>
    /// Main Workflow Editor Window
    /// </summary>
    public class AmbientSkiesEditorWindow : EditorWindow, IPWEditor
    {
        #region Editor Window Variables

        //Render Pipeline selection options
        private AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;

        #region GUI

        //Editor window 
        private Vector2 m_scrollPosition = Vector2.zero;
        private EditorUtils m_editorUtils;
        private TabSet m_mainTabs;

        private Texture2D m_skiesIcon;
        private Texture2D m_additionalLightIcon;
        private Texture2D m_postProcessingIcon;
        private Texture2D m_lightingIcon;
        private Texture2D m_infoIcon;

        #endregion

        #region Main Settings

        //Main profiles
        [SerializeField]private CreationToolsSettings m_creationToolSettings;
        private List<AmbientSkyProfiles> m_profileList;
        private int m_profileListIndex;
        private int newProfileListIndex;
        private AmbientSkyProfiles m_profiles;
        private int m_selectedSkyboxProfileIndex;
        private AmbientSkyboxProfile m_selectedSkyboxProfile;
        private int m_selectedProceduralSkyboxProfileIndex;
        private AmbientProceduralSkyboxProfile m_selectedProceduralSkyboxProfile;
        private int m_selectedGradientSkyboxProfileIndex;
        private AmbientGradientSkyboxProfile m_selectedGradientSkyboxProfile;
        private int m_selectedPostProcessingProfileIndex;
        private AmbientPostProcessingProfile m_selectedPostProcessingProfile;
        private int m_selectedLightingProfileIndex;
        private AmbientLightingProfile m_selectedLightingProfile;
        private AmbientSkiesSceneSavedSettings m_savedSettings;

        public bool m_hasChanged;
        private bool m_multiScenePresent;
        //private bool m_enableHDRP = false;
        //private bool m_highQualityMode;
        private string m_activeSceneName;

        private RenderPipelineAsset m_currentRenderpipelineAsset;

        //Main camera variable for shadow distance system
        private Camera mainCam;
#if HDPipeline
        private HDAdditionalCameraData cameraData;
#endif
        private string probeSpawnCount;
        private string lightProbeSpawnCount;

        //Tempature
        private SerializedProperty temperature;
        private bool useTemperature;
        [SerializeField]
        private float currentTemperature = 6500f;
        private SerializedObject serializedObject;

        //Profile choices on all 4 tabs
        [SerializeField] private List<string> profileChoices = new List<string>();
        [SerializeField] private List<string> skyboxChoices = new List<string>();
        [SerializeField] private List<string> proceduralSkyboxChoices = new List<string>();
        [SerializeField] private List<string> gradientSkyboxChoices = new List<string>();
        [SerializeField] private List<string> ppChoices = new List<string>();
        [SerializeField] private List<string> lightmappingChoices = new List<string>();

        //Position checked
        public bool PositionChecked { get; set; }

        //Editor/Inspector Update
        private float m_countdown = 0.2f;
        private bool m_extraCheck = true;
        private bool m_inspectorUpdate = false;
        private int m_currentInspectorUpdate = 0;

        private bool m_isCompiling;
#if UNITY_2018_1_OR_NEWER
        private bool m_needCompile;
#endif

        #endregion

        #region Mewlist Clouds

        private bool m_massiveCloudsPath = false;

#if Mewlist_Clouds
        //Massive Clouds System
        private bool massiveCloudsEnabled;
        private MassiveCloudsProfile cloudProfile;
        private bool syncGlobalFogColor = true;
        private Color32 cloudsFogColor = new Color32(200, 200, 230, 255);
        private bool syncBaseFogColor = true;
        private Color32 cloudsBaseFogColor = new Color32(200, 200, 230, 255);
        private bool cloudIsHDRP = false;
#endif

        #endregion

        #region Foldouts

        //Global
        private bool m_foldoutGlobalSettings;

        //Skybox
        private bool m_foldoutMainSkySettings;
        private bool m_foldoutTimeOfDaySettings;
        private bool m_foldoutProfileSettings;
        private bool m_foldoutSkySettings;
        private bool m_foldoutFogSettings;
        private bool m_foldoutAmbientSettings;
        private bool m_foldoutSunSettings;
        private bool m_foldoutHorizonSettings;
        private bool m_foldoutShadowSettings;
        private bool m_foldoutHDShadowSettings;
        private bool m_foldoutScreenSpaceReflectionSettings;
        private bool m_foldoutScreenSpaceRefractionSettings;

        //Interior Mode
        private bool m_foldoutInteriorLightingModeSettings = true;
        private bool m_foldoutGlobalInteriorLightingSettings;

        //Post FX
        private bool m_foldoutMainPostProcessing;
        private bool m_foldoutPostProcessingProfile;
        private bool m_foldoutAmbientOcclusion;
        private bool m_foldoutAutoExposure;
        private bool m_foldoutBloom;
        private bool m_foldoutColorGrading;
        private bool m_foldoutDepthOfField;
        private bool m_foldoutGrain;
        private bool m_foldoutLensDistortion;
        private bool m_foldoutScreenSpaceReflections;
        private bool m_foldoutVignette;
        private bool m_foldoutMotionBlur;
        private bool m_foldoutMassiveClouds;
        private bool m_foldoutChromaticAberration;
        private bool m_foldoutPaniniProjection;
        private bool m_foldoutCustomPPProfile;

        //Lighting
        private bool m_foldoutMainLighting;
        private bool m_foldoutLightmapConfiguration;
        private bool m_foldoutRealtimeGI;
        private bool m_foldoutBakedGI;
        private bool m_foldoutReflectionProbes;
        private bool m_foldoutLightProbes;

        //Info
        private bool m_foldoutBuiltIn;
        private bool m_foldoutLightweight;
        private bool m_foldoutHighDefinition;
        private bool m_foldoutCreationMode;
        private bool m_foldoutPipelineSettings;

        //Creation
        private bool m_foldoutCreationModeSettings;

        #endregion

        #region Time Of Day

        //Time Of Day
        private bool m_foldoutTODFogSettings;
        private bool m_foldoutTODHDRPSettings;
        private bool m_foldoutTODKeyBindings;
        private bool m_foldoutTODLightSettings;
        private bool m_foldoutTODRealtimeGI;
        private bool m_foldoutTODSeasons;
        private bool m_foldoutTODTime;
        private bool m_foldoutTODPostProcessing;
        private bool m_foldoutTODSkybox;

        //Time Of Day Settings
        private string seasonString;
        private AmbientSkiesConsts.CurrentSeason currentSeason;
        private AmbientSkiesConsts.HemisphereOrigin hemisphereOrigin;
        private AmbientSkiesTimeOfDayProfile timeOfDayProfile;
        private KeyCode pauseTimeKey;
        private KeyCode incrementUpKey;
        private KeyCode incrementDownKey;
        private KeyCode rotateSunLeftKey;
        private KeyCode rotateSunRightKey;
        private float timeToAddOrRemove;
        private float sunRotationAmount;
        private bool pauseTime;
        private bool syncPostProcessing;
        private bool realtimeGIUpdate;
        private int gIUpdateInterval;
        private bool realtimeEmission;
        private bool syncRealtimeEmissionToTimeOfDay;
        private float currentTimeOfDay;
        private float dayLengthInSeconds;
        private float nightLengthInSeconds;
        private int dayDate;
        private int monthDate;
        private int yearDate;
        private float timeOfDaySkyboxRotation;
        private AnimationCurve daySunIntensity;
        private Gradient daySunGradientColor;
        private AnimationCurve nightSunIntensity;
        private Gradient nightSunGradientColor;
        private float startFogDistance = 20f;
        private AnimationCurve dayFogDensity;
        private AnimationCurve nightFogDensity;
        private AnimationCurve dayFogDistance;
        private Gradient dayFogColor;
        private AnimationCurve nightFogDistance;
        private Gradient nightFogColor;
        private Gradient dayPostFXColor;
        private Gradient nightPostFXColor;
        private AnimationCurve dayTempature;
        private AnimationCurve nightTempature;
        private AnimationCurve lightAnisotropy;
        private AnimationCurve lightProbeDimmer;
        private AnimationCurve lightDepthExtent;
        private AnimationCurve sunSizeAmount;
        private AnimationCurve skyExposureAmount;
        private Gradient pBSGroundColor;
        private AnimationCurve pBSAreosolDensity;
        private Cubemap pBSSpaceEmissionTexture;
        private float pBSTODSpaceEmissionMultiplier;

        #endregion

        #region Sky Tab Values

        //Skies variables
        private bool useSkies;
        //Sky main settings
        private AmbientSkiesConsts.SystemTypes systemtype;
        private int newSkyboxSelection;
        private int newProceduralSkyboxSelection;
        private int newGradientSkyboxSelection;
        private AmbientSkiesConsts.VolumeFogType fogType;
        private AmbientSkiesConsts.AmbientMode ambientMode;
        //Skybox settings
        private string profileName;
        private AmbientSkiesConsts.DisableAndEnable useTimeOfDay;
        private Color skyboxTint;
        private float skyboxExposure;
        private float skyboxRotation;
        private float skyboxPitch;
        private float skyMultiplier;
        private Cubemap customSkybox;
        private bool isProceduralSkybox;
        //Planet
        private bool pBSEarthPreset = true;
        private bool pBSSphericalMode = true;
        private Vector3 pBSPlanetCenterPosition = new Vector3(0, -6378100, 0);
        private Vector3 pBSPlanetRotation = new Vector3(0, -0, 0);
        private Cubemap pBSGroundColorTexture;
        private Color pBSGroundTint = new Color(0.255874f, 0.350156f, 0.4339623f);
        private Cubemap pBSGroundEmissionTexture;
        private float pBSGroundEmissionMultiplier = 1f;
        //Space
        private Vector3 pBSSpaceRotation = new Vector3(0, 0, 0);
        private Cubemap pBSSpaceEmission;
        private float pBSSpaceEmissionMultiplier = 1f;
        //Air
        private float pBSAirMaximumAltitude = 55261.97f;
        private float pBSAirDensityR = 0.04534f;
        private float pBSAirDensityG = 0.1023724f;
        private float pBSAirDensityB = 0.2326406f;
        private Color pBSAirTint = new Color(0.9f, 0.9f, 1f);
        //Aerosols
        private float pBSAerosolMaximumAltitude = 8289.296f;
        private float pBSAerosolDensity = 0.01192826f;
        private Color pBSAerosolTint = new Color(0.9f, 0.9f, 0.9f);
        private float pBSAerosolAnisotropy = 0f;
        //Artistic Overrides
        private float pBSColorSaturation = 1f;
        private float pBSAlphaSaturation = 1f;
        private float pBSAlphaMultiplier = 1f;
        private Color pBSHorizonTint = Color.white;
        private float pBSHorizonZenithShift = 0f;
        private Color pBSZenithTint = Color.white;
        //Miscellaneous
        private int pBSNumberOfBounces = 8;
        private AmbientSkiesConsts.HDRPIntensityMode pBSIntensityMode = AmbientSkiesConsts.HDRPIntensityMode.Multiplier;
        private float pBSExposure = 1f;
        private float pBSMultiplier = 1.5f;
        private AmbientSkiesConsts.EnvironementSkyUpdateMode pBSUpdateMode = AmbientSkiesConsts.EnvironementSkyUpdateMode.OnChanged;
        private bool pBSIncludeSunInBaking = true;
        //Fog settings
#if HDPipeline
        private FogColorMode fogColorMode;
#endif
        private AmbientSkiesConsts.AutoConfigureType configurationType;
        private Color fogColor;
        private float hDRPFogDistance;
        private float fogDistance;
        private float nearFogDistance;
        private float fogDensity;
        //Ambient settings
        private AmbientSkiesConsts.HDRPAmbientMode hdrpAmbientMode;
        private Color skyColor;
        private Color32 equatorColor;
        private Color32 groundColor;
        private Color32 lwrpSkyColor;
        private Color32 lwrpEquatorColor;
        private Color32 lwrpGroundColor;
        private float skyboxGroundIntensity;
        private float diffuseAmbientIntensity;
        private float specularAmbientIntensity;
        private bool useStaticLighting;
        //Sun settings
        public float shadowStrength;
        public float indirectLightMultiplier;
        private Color sunColor;
        private float sunIntensity;
        private float sunLWRPIntensity;
        private float sunHDRPIntensity;
        private float shadowDistance;
        //Shadow Settings
        private ShadowmaskMode shadowmaskMode;
        private LightShadows shadowType;
        private ShadowResolution shadowResolution;
        private ShadowProjection shadowProjection;
        //Vsync
        private AmbientSkiesConsts.VSyncMode vSyncMode;
        //Horizon settings
        private bool scaleHorizonObjectWithFog;
        private bool horizonEnabled;
        private float horizonScattering;
        private float horizonFogDensity;
        private float horizonFalloff;
        private float horizonBlend;
        private Vector3 horizonScale;
        private bool followPlayer;
        private float horizonUpdateTime;
        private Vector3 horizonPosition;
        //Gradient Sky
        private Color32 topSkyColor;
        private Color32 middleSkyColor;
        private Color32 bottomSkyColor;
        private float gradientDiffusion;
        //Procedural Sky
        private bool enableSunDisk;
        private float proceduralSunSize;
        private float hdrpProceduralSunSize;
        private float proceduralSunSizeConvergence;
        private float hdrpProceduralSunSizeConvergence;
        private float proceduralAtmosphereThickness;
        private Color32 proceduralGroundColor;
        private bool includeSunInBaking;
        //Volumetric Fog
        private bool useDistanceFog;
        private float baseFogDistance;
        private float baseFogHeight;
        private float meanFogHeight;
        private float globalAnisotropy;
        private float globalLightProbeDimmer;
        //Exponential Fog
        private float exponentialFogDensity;
        private float exponentialBaseFogHeight;
        private float exponentialHeightAttenuation;
        private float exponentialMaxFogDistance;
        private float exponentialMipFogNear;
        private float exponentialMipFogFar;
        private float exponentialMipFogMax;
        //Linear Fog
        private float linearFogDensity;
        private float linearFogHeightStart;
        private float linearFogHeightEnd;
        private float linearFogMaxDistance;
        private float linearMipFogNear;
        private float linearMipFogFar;
        private float linearMipFogMax;
        //Volumetric Light Controller
        private float depthExtent;
        private float sliceDistribution;
        //Density Fog Volume
        private bool useDensityFogVolume;
        private Color32 singleScatteringAlbedo;
        private float densityVolumeFogDistance;
        private Texture3D fogDensityMaskTexture;
        private Vector3 densityMaskTiling;
        private Vector3 densityScale;
        //HD Shadows
        private AmbientSkiesConsts.HDShadowQuality hDShadowQuality;
        private AmbientSkiesConsts.ShadowCascade shadowCascade;
        private float split1;
        private float split2;
        private float split3;
        //Contact Shadows
        private bool enableContactShadows;
        private float contactLength;
        private float contactScaleFactor;
        private float contactMaxDistance;
        private float contactFadeDistance;
        private int contactSampleCount;
        private float contactOpacity;
        //Micro Shadows
        private bool enableMicroShadows;
        private float microShadowOpacity;
        //SS Reflection
        private bool enableSSReflection;
        private float ssrEdgeFade;
        private int ssrNumberOfRays;
        private float ssrObjectThickness;
        private float ssrMinSmoothness;
        private float ssrSmoothnessFade;
        private bool ssrReflectSky;
        //SS Refract
        private bool enableSSRefraction;
        private float ssrWeightDistance;

        #endregion

        #region Additional Lighting

        //Interior Lighting
        private AmbientSkiesConsts.EnvironmentLightingEditMode lightingEditMode;
        private AmbientSkiesConsts.LightTypeMode lightTypeMode;
        private string lightName;
        private bool spawnAtCamera;
        private bool parentToActiveObject;

        #endregion

        #region Post Process Values

        //Countdown float
        private float autoPostProcessingApplyTimer;
#if UNITY_POST_PROCESSING_STACK_V2
        private string postProcessingAssetName;
#endif
        private bool dithering;
        //Use Post FX
        private bool usePostProcess;
        //Selection
        private int newPPSelection;
        //Hides the post fx gizmo
        private bool hideGizmos;
        //Auto match profile
        private bool autoMatchProfile = true;
        //HDR Mode
        private AmbientSkiesConsts.HDRMode hDRMode;
        //Anti Aliasing Mode
        private AmbientSkiesConsts.AntiAliasingMode antiAliasingMode;
        //Target Platform
        private AmbientSkiesConsts.PlatformTarget targetPlatform;
        //AO settings
        private bool aoEnabled;
        private float aoAmount;
        private Color32 aoColor;
        //Exposure settings
        private bool autoExposureEnabled;
        private float exposureAmount;
        private float exposureMin;
        private float exposureMax;
        //Bloom settings
        private bool bloomEnabled;
        private float bloomIntensity;
        private float bloomThreshold;
        private float lensIntensity;
        private Texture2D lensTexture;
        //Chromatic Aberration
        private bool chromaticAberrationEnabled;
        private float chromaticAberrationIntensity;
        //Color Grading settings
        private bool colorGradingEnabled;
        private Texture2D colorGradingLut;
        private float colorGradingPostExposure;
        private Color32 colorGradingColorFilter;
        private int colorGradingTempature;
        private int colorGradingTint;
        private float colorGradingSaturation;
        private float colorGradingContrast;
        //Channel Mixer
        private int channelMixerRed;
        private int channelMixerGreen;
        private int channelMixerBlue;
        //DOF settings
        private AmbientSkiesConsts.DepthOfFieldMode depthOfFieldMode;
        private AmbientSkiesConsts.DOFTrackingType depthOfFieldTrackingType;
        private bool depthOfFieldEnabled;
        private float autoDepthOfFieldFocusDistance;
        private float depthOfFieldFocusDistance;
        private float depthOfFieldAperture;
        private float depthOfFieldFocalLength;
        private float focusOffset;
        private LayerMask targetLayer;
        private float maxFocusDistance;
        private string depthOfFieldDistanceString;
        //Distortion settings
        private bool distortionEnabled;
        private float distortionIntensity;
        private float distortionScale;
        //Grain settings
        private bool grainEnabled;
        private float grainIntensity;
        private float grainSize;
        //SSR settings
        private bool screenSpaceReflectionsEnabled;
        private int maximumIterationCount;
        private float thickness;
#if UNITY_POST_PROCESSING_STACK_V2
        private GradingMode colorGradingMode;
        private PostProcessProfile customPostProcessingProfile;
        private KernelSize maxBlurSize;
        private ScreenSpaceReflectionResolution screenSpaceReflectionResolution;
        private ScreenSpaceReflectionPreset screenSpaceReflectionPreset;
        private AmbientOcclusionMode ambientOcclusionMode;
#endif
        private float maximumMarchDistance;
        private float distanceFade;
        private float screenSpaceVignette;
        //Vignette settings
        private bool vignetteEnabled;
        private float vignetteIntensity;
        private float vignetteSmoothness;
        //Motion Blur settings
        private bool motionBlurEnabled;
        private int motionShutterAngle;
        private int motionSampleCount;

        #endregion

        #region HDRP Post Process Values

#if HDPipeline && UNITY_2019_1_OR_NEWER

        //Main Settings
        private VolumeProfile customHDRPPostProcessingprofile;

        //AO
        private float hDRPAOIntensity;
        private float hDRPAOThicknessModifier;
        private float hDRPAODirectLightingStrength;

        //Bloom
        private float hDRPBloomIntensity;
        private float hDRPBloomScatter;
        private Color32 hDRPBloomTint;
        private Texture2D hDRPBloomDirtLensTexture;
        private float hDRPBloomDirtLensIntensity;
#if !UNITY_2019_3_OR_NEWER
        public UnityEngine.Experimental.Rendering.HDPipeline.BloomResolution hDRPBloomResolution;
#else
        public UnityEngine.Rendering.HighDefinition.BloomResolution hDRPBloomResolution;
#endif
        public bool hDRPBloomHighQualityFiltering;
        public bool hDRPBloomPrefiler;
        public bool hDRPBloomAnamorphic;

        //Channel Mixer
        private int hDRPChannelMixerRed;
        private int hDRPChannelMixerGreen;
        private int hDRPChannelMixerBlue;

        //Chromatic Aberration
        private Texture2D hDRPChromaticAberrationSpectralLut;
        private float hDRPChromaticAberrationIntensity;
        private int hDRPChromaticAberrationMaxSamples;

        //Color Adjustments
        private float hDRPColorAdjustmentPostExposure;
        private float hDRPColorAdjustmentContrast;
        private Color32 hDRPColorAdjustmentColorFilter;
        private int hDRPColorAdjustmentHueShift;
        private float hDRPColorAdjustmentSaturation;

        //Color Curves
        //private UnityEngine.Experimental.Rendering.HDPipeline.ColorCurves hDRPColorCurves;

        //Color Lookup
        private Texture hDRPColorLookupTexture;
        private float hDRPColorLookupContribution;

        //Depth Of Field
#if !UNITY_2019_3_OR_NEWER
        private UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldMode hDRPDepthOfFieldFocusMode;
#else
        private UnityEngine.Rendering.HighDefinition.DepthOfFieldMode hDRPDepthOfFieldFocusMode;
#endif
        //private float hDRPDepthOfFieldFocusDistance;
        private float hDRPDepthOfFieldNearBlurStart;
        private float hDRPDepthOfFieldNearBlurEnd;
        private int hDRPDepthOfFieldNearBlurSampleCount;
        private float hDRPDepthOfFieldNearBlurMaxRadius;
        private float hDRPDepthOfFieldFarBlurStart;
        private float hDRPDepthOfFieldFarBlurEnd;
        private int hDRPDepthOfFieldFarBlurSampleCount;
        private float hDRPDepthOfFieldFarBlurMaxRadius;

#if !UNITY_2019_3_OR_NEWER
        private UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldResolution hDRPDepthOfFieldResolution;
#else
        private UnityEngine.Rendering.HighDefinition.DepthOfFieldResolution hDRPDepthOfFieldResolution;
#endif
        private bool hDRPDepthOfFieldHighQualityFiltering;

        //Film Grain
#if !UNITY_2019_3_OR_NEWER
        private UnityEngine.Experimental.Rendering.HDPipeline.FilmGrainLookup hDRPFilmGrainType;
#else
        private UnityEngine.Rendering.HighDefinition.FilmGrainLookup hDRPFilmGrainType;
#endif
        private float hDRPFilmGrainIntensity;
        private float hDRPFilmGrainResponse;

        //Lens Distortion
        private float hDRPLensDistortionIntensity;
        private float hDRPLensDistortionXMultiplier;
        private float hDRPLensDistortionYMultiplier;
        private Vector2 hDRPLensDistortionCenter;
        private float hDRPLensDistortionScale;

        //Lift Gamma Gain
        //private UnityEngine.Experimental.Rendering.HDPipeline.LiftGammaGain hDRPLiftGammaGain;

        //Motion Blur
        private float hDRPMotionBlurIntensity;
        private int hDRPMotionBlurSampleCount;
        private int hDRPMotionBlurMaxVelocity;
        private float hDRPMotionBlurMinVelocity;
        private float hDRPMotionBlurCameraRotationVelocityClamp;

        //Panini Projection
        private bool hDRPPaniniProjectionEnabled;
        private float hDRPPaniniProjectionDistance;
        private float hDRPPaniniProjectionCropToFit;

        //Shadow Midtones Hightlights
        //private UnityEngine.Experimental.Rendering.HDPipeline.ShadowsMidtonesHighlights hDRPShadowsMidtonesHighlights;
        //private float hDRPShadowsMidtonesHighlightsShadowLimitStart = 0f;
        //private float hDRPShadowsMidtonesHighlightsShadowLimitEnd = 0.3f;
        //private float hDRPShadowsMidtonesHighlightsHightlightLimitStart = 0.55f;
        //private float hDRPShadowsMidtonesHighlightsHighlightLimitEnd = 1f;

        //Split Toning
        private Color32 hDRPSplitToningShadows;
        private Color32 hDRPSplitToningHighlights;
        private int hDRPSplitToningBalance;

        //Tonemapping
#if !UNITY_2019_3_OR_NEWER
        private UnityEngine.Experimental.Rendering.HDPipeline.TonemappingMode hDRPTonemappingMode;
#else
        private UnityEngine.Rendering.HighDefinition.TonemappingMode hDRPTonemappingMode;
#endif
        public float hDRPTonemappingToeStrength;
        public float hDRPTonemappingToeLength;
        public float hDRPTonemappingShoulderStrength;
        public float hDRPTonemappingShoulderLength;
        public float hDRPTonemappingShoulderAngle;
        public float hDRPTonemappingGamma;

        //Vignette 
#if !UNITY_2019_3_OR_NEWER
        private UnityEngine.Experimental.Rendering.HDPipeline.VignetteMode hDRPVignetteMode;
#else
        private UnityEngine.Rendering.HighDefinition.VignetteMode hDRPVignetteMode;
#endif
        private Texture2D hDRPVignetteMask;
        private float hDRPVignetteMaskOpacity;
        private Color32 hDRPVignetteColor;
        private Vector2 hDRPVignetteCenter;
        private float hDRPVignetteIntensity;
        private float hDRPVignetteSmoothness;
        private float hDRPVignetteRoundness;
        private bool hDRPVignetteRounded;

        //White Balance
        private int hDRPWhiteBalanceTempature;
        private int hDRPWhiteBalanceTint;

        //Exposure
#if !UNITY_2019_3_OR_NEWER
        private UnityEngine.Experimental.Rendering.HDPipeline.ExposureMode hDRPExposureMode;
#else
        private UnityEngine.Rendering.HighDefinition.ExposureMode hDRPExposureMode;
#endif
        private float hDRPExposureCompensation;
        private float hDRPExposureFixedExposure;
#if !UNITY_2019_3_OR_NEWER
        private UnityEngine.Experimental.Rendering.HDPipeline.MeteringMode hDRPExposureMeteringMode;
#else
        private UnityEngine.Rendering.HighDefinition.MeteringMode hDRPExposureMeteringMode;
#endif
#if !UNITY_2019_3_OR_NEWER
        private UnityEngine.Experimental.Rendering.HDPipeline.LuminanceSource hDRPExposureLuminationSource;
#else
        private UnityEngine.Rendering.HighDefinition.LuminanceSource hDRPExposureLuminationSource;
#endif
        private AnimationCurve hDRPExposureCurveMap;
        private float hDRPExposureLimitMin;
        private float hDRPExposureLimitMax;
#if !UNITY_2019_3_OR_NEWER
        private UnityEngine.Experimental.Rendering.HDPipeline.AdaptationMode hDRPExposureAdaptionMode;
#else
        private UnityEngine.Rendering.HighDefinition.AdaptationMode hDRPExposureAdaptionMode;
#endif
        private float hDRPExposureAdaptionSpeedDarkToLight;
        private float hDRPExposureAdaptionSpeedLightToDark;

#endif

#endregion

        #region URP Post Process Values

#if UPPipeline && UNITY_2019_3_OR_NEWER

        //Main Settings
        private VolumeProfile customURPPostProcessingprofile;

        //AO

        //Bloom
        private float uRPBloomIntensity;
        private float uRPBloomScatter;
        private Color32 uRPBloomTint;
        private Texture2D uRPBloomDirtLensTexture;
        private float uRPBloomDirtLensIntensity;
        public bool uRPBloomHighQualityFiltering;
        public bool uRPBloomPrefiler;
        public bool uRPBloomAnamorphic;

        //Channel Mixer
        private int uRPChannelMixerRed;
        private int uRPChannelMixerGreen;
        private int uRPChannelMixerBlue;

        //Chromatic Aberration
        private Texture2D uRPChromaticAberrationSpectralLut;
        private float uRPChromaticAberrationIntensity;
        private int uRPChromaticAberrationMaxSamples;

        //Color Adjustments
        private float uRPColorAdjustmentPostExposure;
        private float uRPColorAdjustmentContrast;
        private Color32 uRPColorAdjustmentColorFilter;
        private int uRPColorAdjustmentHueShift;
        private float uRPColorAdjustmentSaturation;

        //Color Curves
        //private UnityEngine.Experimental.Rendering.HDPipeline.ColorCurves hDRPColorCurves;

        //Color Lookup
        private Texture uRPColorLookupTexture;
        private float uRPColorLookupContribution;

        //Depth Of Field
        private AmbientSkiesConsts.URPDepthOfFieldMode uRPDepthOfFieldMode;
        //Gaussian
        private float uRPDepthOfFieldStart;
        private float uRPDepthOfFieldEnd;
        private float uRPDepthOfFieldMaxRadius;
        private bool uRPDepthOfFieldHighQualityFiltering;
        //Bokeh
        private float uRPDepthOfFieldFocusDitance;
        private int uRPDepthOfFieldFocalLength;
        private float uRPDepthOfFieldAperture;
        private int uRPDepthOfFieldBladeCount;
        private float uRPDepthOfFieldBladeCurvature;
        private int uRPDepthOfFieldBladeRotation;

        //Film Grain
        private AmbientSkiesConsts.URPFilmGrainType uRPFilmGrainType;
        private float uRPFilmGrainIntensity;
        private float uRPFilmGrainResponse;

        //Lens Distortion
        private float uRPLensDistortionIntensity;
        private float uRPLensDistortionXMultiplier;
        private float uRPLensDistortionYMultiplier;
        private Vector2 uRPLensDistortionCenter;
        private float uRPLensDistortionScale;

        //Lift Gamma Gain
        //private UnityEngine.Experimental.Rendering.HDPipeline.LiftGammaGain hDRPLiftGammaGain;

        //Motion Blur
        public AmbientSkiesConsts.URPMotionBlurQuality uRPMotionBlurQuality;
        private float uRPMotionBlurIntensity;
        private int uRPMotionBlurSampleCount;
        private int uRPMotionBlurMaxVelocity;
        private float uRPMotionBlurMinVelocity;
        private float uRPMotionBlurCameraRotationVelocityClamp;

        //Panini Projection
        private bool uRPPaniniProjectionEnabled;
        private float uRPPaniniProjectionDistance;
        private float uRPPaniniProjectionCropToFit;

        //Shadow Midtones Hightlights
        //private UnityEngine.Experimental.Rendering.HDPipeline.ShadowsMidtonesHighlights hDRPShadowsMidtonesHighlights;
        //private float hDRPShadowsMidtonesHighlightsShadowLimitStart = 0f;
        //private float hDRPShadowsMidtonesHighlightsShadowLimitEnd = 0.3f;
        //private float hDRPShadowsMidtonesHighlightsHightlightLimitStart = 0.55f;
        //private float hDRPShadowsMidtonesHighlightsHighlightLimitEnd = 1f;

        //Split Toning
        private Color32 uRPSplitToningShadows;
        private Color32 uRPSplitToningHighlights;
        private int uRPSplitToningBalance;

        //Tonemapping
        private AmbientSkiesConsts.URPTonemappingMode uRPTonemappingMode;
        public float uRPTonemappingToeStrength;
        public float uRPTonemappingToeLength;
        public float uRPTonemappingShoulderStrength;
        public float uRPTonemappingShoulderLength;
        public float uRPTonemappingShoulderAngle;
        public float uRPTonemappingGamma;

        //Vignette 
        private Texture2D uRPVignetteMask;
        private float uRPVignetteMaskOpacity;
        private Color32 uRPVignetteColor;
        private Vector2 uRPVignetteCenter;
        private float uRPVignetteIntensity;
        private float uRPVignetteSmoothness;
        private float uRPVignetteRoundness;
        private bool uRPVignetteRounded;

        //White Balance
        private int uRPWhiteBalanceTempature;
        private int uRPWhiteBalanceTint;

        //Exposure

#endif

        #endregion

        #region Lighting Values

        //Use Lightmaps
        private bool enableLightmapSettings;
        //Current Lightmap Setting
        private bool autoLightmapGeneration;
        private int newLightmappingSettings;
        //Realtime GI
        private bool realtimeGlobalIllumination;
        private float indirectRelolution;
        private bool useDirectionalMode;
        //Baked GI
        private bool bakedGlobalIllumination;
        private AmbientSkiesConsts.LightmapperMode lightmappingMode;
        private float lightmapResolution;
        private int lightmapPadding;
        private bool useHighResolutionLightmapSize;
        private bool compressLightmaps;
        private bool ambientOcclusion;
        private float maxDistance;
        private float indirectContribution;
        private float directContribution;
        private float lightIndirectIntensity;
        private float lightBoostIntensity;
        private bool finalGather;
        private int finalGatherRayCount;
        private bool finalGatherDenoising;
        //Reflection Probe Settings
        private string reflectionProbeName;
        private AmbientSkiesConsts.ReflectionProbeSpawnType reflectionProbeSpawnType;
        private ReflectionProbeMode reflectionProbeMode;
        private ReflectionProbeRefreshMode reflectionProbeRefresh;
        private ReflectionCubemapCompression reflectionCubemapCompression;
        private ReflectionProbeTimeSlicingMode reflectionProbeTimeSlicingMode;
        private Vector3 reflectionProbeScale;
        private float reflectionProbeOffset;
        private float reflectionProbeClipPlaneDistance;
        //private float reflectionProbeBlendDistance;
        private LayerMask reflectionprobeCullingMask;
        private float reflectionProbeShadowDistance;
        private AmbientSkiesConsts.ReflectionProbeResolution reflectionProbeResolution;
        //Light Probe Settings
        private AmbientSkiesConsts.LightProbeSpawnType lightProbeSpawnType;
        private int lightProbesPerRow;
        private int reflectionProbesPerRow;
        //private int lightProbeSpawnRadius;
        private float seaLevel;
#if !UNITY_2020_1_OR_NEWER
        private LightmapEditorSettings.FilterMode filterMode;
#else
        private LightingSettings.FilterMode filterMode;
#endif

        #endregion

        #region Creation

        //Create mode
        private string m_newSystemName = "New Selected System";
        private bool m_enableEditMode = false;
        private string m_skiesProfileName;
#if UNITY_POST_PROCESSING_STACK_V2
        private string m_postProfileName;
#endif
        private string m_lightProfileName;
        private Cubemap m_hdriAssetName;
#if UNITY_POST_PROCESSING_STACK_V2
        private PostProcessProfile m_postProcessAssetName;
        private int m_creationMatchPostProcessing;
#endif
        private int m_newHDRIProfileIndex;
        private int m_newProceduralProfileIndex;
        private int m_newGradientProfileIndex;
        private int m_newPostProcessingIndex;
        private int m_newLightingProfileIndex;

        private SerializedObject objectSer { get; set; }
#if UNITY_POST_PROCESSING_STACK_V2
        private PostProcessProfile convertPostProfile;
#endif
        private bool focusAsset;
        private bool renamePostProcessProfile;
        private string convertPostProfileName;

        #endregion

        #endregion

        #region Custom Menu Items

        /// <summary>
        /// Creates menu and opens up Ambient skies
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/Ambient Skies/Ambient Skies...", false, 40)]
        public static void ShowMenu()
        {
            //Ambient Skies Editor Window
            var mainWindow = GetWindow<AmbientSkiesEditorWindow>(false, "Ambient Skies");
            //Show window
            mainWindow.Show();
        }

        #endregion

        #region Constructors destructors and related delegates

        /// <summary>
        /// Destroys when window is closed
        /// </summary>
        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }

            if (m_profiles == null)
            {
                return;
            }

            //Apply settings
            if (m_profiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
            {
                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
            }

            if (m_selectedPostProcessingProfile != null)
            {
                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);
            }

            //Update lightmap settings   
            if (m_selectedLightingProfile != null)
            {
                LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);
            }

            if (m_multiScenePresent)
            {
                if (m_savedSettings == null)
                {
                    m_savedSettings = AmbientSkiesEditorFunctions.GetSavedSettingsComponent("Ambient Skies Global", false);
                }

                if (m_savedSettings != null)
                {
                    m_hasChanged = true;
                    ApplyCreationToolSettings(m_hasChanged);
                    m_savedSettings.SetSettings(m_profiles, m_creationToolSettings);
                }
            }

            if (m_massiveCloudsPath)
            {
                //Remove warning
            }

            if (m_multiScenePresent)
            {
                //Remove warning
            }

            if (m_skiesProfileName != "")
            {
                //Remove warning
            }

            if (m_hdriAssetName != null)
            {
                //Remove warning
            }

            //Apply index to profile in creation settings
            if (m_creationToolSettings != null)
            {
                EditorUtility.SetDirty(m_creationToolSettings);
                m_creationToolSettings.m_selectedHDRI = m_selectedSkyboxProfileIndex;
                m_creationToolSettings.m_selectedProcedural = m_selectedProceduralSkyboxProfileIndex;
                m_creationToolSettings.m_selectedGradient = m_selectedGradientSkyboxProfileIndex;
                m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                m_creationToolSettings.m_selectedLighting = m_selectedLightingProfileIndex;
            }

            EditorApplication.update -= EditorUpdate;
            EditorApplication.playModeStateChanged -= HandleOnPlayModeChanged;
        }

        /// <summary>
        /// Setup when window opens
        /// </summary>
        private void OnEnable()
        {
            #region Load UX

            AmbientSkiesEditorFunctions.LoadIcons(out m_skiesIcon, out m_additionalLightIcon, out m_postProcessingIcon, out m_lightingIcon, out m_infoIcon);

            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }

            // Ambient Skies tabs
            var tabs = new Tab[]
            {
                new Tab("SkyboxesTab", m_skiesIcon, SkyboxesTab),
                new Tab("LightsTab", m_additionalLightIcon, AdditionalLightsTab),
                new Tab("PostProcessingTab", m_postProcessingIcon, PostProcessingTab),
                new Tab("LightingTab", m_lightingIcon, LightingTab),
                new Tab("InfoTab", m_infoIcon, InfoTab),
            };

            //Assign tabs
            m_mainTabs = new TabSet(m_editorUtils, tabs);

            #endregion

            #region Camera Setup

            //Get main camera
            if (mainCam == null)
            {
                mainCam = FindObjectOfType<Camera>();
            }

            ReflectionProbeUtils.m_probeRenderActive = false;

            #endregion

            #region Load Profile

            m_extraCheck = true;

            CreationToolsSettings tools = AssetDatabase.LoadAssetAtPath<CreationToolsSettings>(SkyboxUtils.GetAssetPath("Ambient Skies Creation Tool Settings"));
            m_creationToolSettings = tools;
            if (m_creationToolSettings == null)
            {
                Debug.LogError("Missing creation tools asset, Ambient Skies requires this asset. (Ambient Skies Creation Tool Settings) Ambient Skies will now close due to this error");
                Close();
            }

            m_profileList = AmbientSkiesEditorFunctions.GetAllSkyProfilesProjectSearch("t:AmbientSkyProfiles");

            AmbientSkyProfiles skyProfiles = null;
            //Add global profile names
            profileChoices.Clear();
            foreach (var profile in m_profileList)
            {
                profileChoices.Add(profile.name);
            }

            if (m_creationToolSettings.m_selectedSystem > m_profileList.Count - 1)
            {
                newProfileListIndex = 0;
                m_profileListIndex = newProfileListIndex;
            }
            else
            {
                newProfileListIndex = m_creationToolSettings.m_selectedSystem;
                m_profileListIndex = newProfileListIndex;
            }

            //Load system
            if (m_profileList[newProfileListIndex].name.Length < 1)
            {
                m_profileListIndex = 0;
                newProfileListIndex = 0;
                skyProfiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath("Ambient Skies Volume 1"));
            }
            else
            {
                skyProfiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath(m_profileList[newProfileListIndex].name));
            }

            m_profiles = skyProfiles;

            if (m_profiles != null)
            {
                if (m_profiles.m_version != AppConfig.VERSION)
                {
                    m_profiles.m_version = PWApp.CONF.Version;
                    if (m_profiles.m_showDebug)
                    {
                        Debug.Log(m_profiles.name + " Has been updated to version " + PWApp.CONF.Version);
                    }
                }
            }

            #endregion

#if UNITY_POST_PROCESSING_STACK_V2
            if (!usePostProcess)
            {
                GameObject postProcessObject = GameObject.Find("Global Post Processing");
                if (postProcessObject != null)
                {
                    PostProcessVolume volume = postProcessObject.GetComponent<PostProcessVolume>();
                    if (volume != null)
                    {
                        PostProcessProfile profile = volume.sharedProfile;
                        if (profile.name.Contains("Gaia"))
                        {
                            usePostProcess = true;
                            m_profiles.m_usePostFX = true;
                            m_selectedPostProcessingProfileIndex = m_profiles.m_ppProfiles.Count - 1;
                            m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                            m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex].customPostProcessingProfile = profile;
                        }
                    }
                }
            }
#endif

            #region Scripting Define

            m_needCompile = AmbientSkiesEditorFunctions.ApplyScriptingDefine(m_currentRenderpipelineAsset, m_profiles, out renderPipelineSettings, m_creationToolSettings);
            //Selected pipeline
            if (m_profiles != null)
            {
                m_profiles.m_selectedRenderPipeline = renderPipelineSettings;
            }
            if (m_needCompile)
            {
                //Resolves warning
            }

            if (m_currentRenderpipelineAsset == null)
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("Pipeline asset is not assigned, grabbing from Graphics selected asset");
                }

                m_currentRenderpipelineAsset = GraphicsSettings.renderPipelineAsset;
            }

            #endregion

            #region Apply Settings

            //If ambient skies system is not loaded
            if (m_profiles == null)
            {
                //Debug and exit
                if (m_profiles.m_showDebug)
                {
                    Debug.LogWarning("Missing Ambient Skies Volume 1.asset, please make sure it's in your project. Ambient Skies will now close due to this error.");
                }

                //Close ambient skies window
                Close();

                return;
            }
            else
            {
                systemtype = m_profiles.m_systemTypes;

                //GaiaUnderwaterScriptFix(false);

                //New scene detected, option to save settings LWRP / Built-In
                bool loadingFromNewScene = false;
                if (m_profiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && RenderSettings.skybox == null || m_profiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && RenderSettings.skybox.name != "Ambient Skies Skybox")
                {
                    if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
#if AMBIENT_SKIES_ENVIRO
                        if (FindObjectOfType<EnviroSkyMgr>() != null)
                        {
                            if (EditorUtility.DisplayDialog("Enviro Detected!", "Warning Enviro has been detected in your scene, System Type is not set to Third Party. Would you like to switch System Type to Third Party?", "Yes", "No"))
                            {
                                m_profiles.m_systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;
                                systemtype = AmbientSkiesConsts.SystemTypes.ThirdParty;;
                                AmbientSkiesEditorFunctions.NewSceneObjectCreation();
                            }
                        }
#endif
                        if (RenderSettings.skybox == null)
                        {
                            SkyboxUtils.AddSkyboxIfNull("Ambient Skies Skybox", m_profiles);
                        }

                        // if (GameObject.Find("Ambient Skies New Scene Object (Don't Delete Me)") == null && RenderSettings.skybox.shader != Shader.Find("Enviro/Skybox"))
                        // {
                        //     if (EditorUtility.DisplayDialog("New Scene Detected", "This scene isn't using ambient skies content. We have saved your settings to the User Profile", "Ok"))
                        //     {
                        //         AmbientSkiesEditorFunctions.SaveBuiltInAndLWRPSettings(m_profiles, m_creationToolSettings, systemtype, m_selectedSkyboxProfileIndex, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, skyboxRotation, m_selectedPostProcessingProfileIndex, m_selectedPostProcessingProfile, usePostProcess);
                        //         loadingFromNewScene = true;
                        //     }
                        // }

                        GameObject globalObject = GameObject.Find("Ambient Skies Global");
                        if (globalObject != null)
                        {
                            AmbientSkiesSceneSavedSettings settings = globalObject.GetComponent<AmbientSkiesSceneSavedSettings>();
                            if (settings == null)
                            {
                                if (EditorUtility.DisplayDialog("New Scene Detected", "This scene isn't using ambient skies content. We have saved your settings to the User Profile", "Ok"))
                                {
                                    AmbientSkiesEditorFunctions.SaveBuiltInAndLWRPSettings(m_profiles, m_creationToolSettings, systemtype, m_selectedSkyboxProfileIndex, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, skyboxRotation, m_selectedPostProcessingProfileIndex, m_selectedPostProcessingProfile, usePostProcess);
                                    loadingFromNewScene = true;
                                }
                            }
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("New Scene Detected", "This scene isn't using ambient skies content. We have saved your settings to the User Profile", "Ok"))
                            {
                                AmbientSkiesEditorFunctions.SaveBuiltInAndLWRPSettings(m_profiles, m_creationToolSettings, systemtype, m_selectedSkyboxProfileIndex, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, skyboxRotation, m_selectedPostProcessingProfileIndex, m_selectedPostProcessingProfile, usePostProcess);
                                loadingFromNewScene = true;
                            }
                        }
                    }
                    else if (m_profiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        SkyboxUtils.AddSkyboxIfNull("Default-Sky", m_profiles);
                    }
                }
#if HDPipeline
                //New scene detected, option to save settings HDRP
                else if (m_profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && FindObjectOfType<Volume>() != null)
                {
                    if (GameObject.Find("Ambient Skies New Scene Object (Don't Delete Me)") == null)
                    {
                        if (GameObject.Find("High Definition Environment Volume") == null)
                        {
                            if (EditorUtility.DisplayDialog("New Scene Detected", "This scene isn't using ambient skies content. We have saved your settings to the User Profile", "Ok"))
                            {
                                AmbientSkiesEditorFunctions.SaveHDRPSettings(m_profiles, systemtype, m_selectedSkyboxProfileIndex, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile);
                                loadingFromNewScene = true;
                            }
                        }
                    }
                }
#endif
                if (!EditorPrefs.GetBool((PWAmbientSkiesPrefKeys.m_multiSceneEditorPrefs)))
                {
                    AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(m_profiles);
                    LoadAndApplySettings(loadingFromNewScene);
                    AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToFalse(m_profiles);
                }
                else
                {
                    LoadProfiles();
                }

                //Add skies profile names
                skyboxChoices.Clear();
                foreach (var profile in m_profiles.m_skyProfiles)
                {
                    skyboxChoices.Add(profile.name);
                }
                //Add procedural skies profile names
                proceduralSkyboxChoices.Clear();
                foreach (var profile in m_profiles.m_proceduralSkyProfiles)
                {
                    proceduralSkyboxChoices.Add(profile.name);
                }
                //Add gradient skies profile names
                gradientSkyboxChoices.Clear();
                foreach (var profile in m_profiles.m_gradientSkyProfiles)
                {
                    gradientSkyboxChoices.Add(profile.name);
                }
                //Add post processing profile names
                ppChoices.Clear();
                foreach (var profile in m_profiles.m_ppProfiles)
                {
                    ppChoices.Add(profile.name);
                }
                //Add lightmaps profile names
                lightmappingChoices.Clear();
                foreach (var profile in m_profiles.m_lightingProfiles)
                {
                    lightmappingChoices.Add(profile.name);
                }

                //Check for massive clouds plugin is there
                m_massiveCloudsPath = Directory.Exists(SkyboxUtils.GetAssetPath("MassiveClouds"));
                MassiveCloudsUtils.DefineMassiveCouds(m_massiveCloudsPath);

                m_countdown = 0.5f;
                m_inspectorUpdate = true;

                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
                EditorApplication.playModeStateChanged -= HandleOnPlayModeChanged;
                EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
            }

            #endregion

            UpdateMultiScene(m_profiles);

            //Apply settings
            GetAndApplyAllValuesFromAmbientSkies(false);

        }

        private void RemoveWarnings()
        {
            if (lightProbesPerRow > 0)
            {
                lightProbesPerRow = 0;
            }
            if (reflectionProbesPerRow > 0)
            {
                reflectionProbesPerRow = 0;
            }
        }

        /// <summary>
        /// Check if users exits playmode
        /// </summary>
        /// <param name="state"></param>
        private void HandleOnPlayModeChanged(PlayModeStateChange state)
        {
            //Checks state if exiting
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                //Apply Updates
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
                m_inspectorUpdate = true;
                m_countdown = 1f;
            }
        }

        #endregion

        #region GUI Main

        #region GUI Startup Setup

        /// <summary>
        /// On GUI update
        /// </summary>
        void OnGUI()
        {
            m_editorUtils.Initialize(); // Do not remove this!
            m_editorUtils.GUIHeader(); //Header

            //Scroll
            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, false, false);

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }


            if (EditorSceneManager.GetActiveScene().name != m_activeSceneName)
            {
                GameObject globalObject = GameObject.Find("Ambient Skies Global");
                if (globalObject != null)
                {
                    AmbientSkiesSceneSavedSettings settings = globalObject.GetComponent<AmbientSkiesSceneSavedSettings>();
                    if (settings == null)
                    {
                        if (EditorUtility.DisplayDialog("New Scene Detected", "This scene isn't using ambient skies content. We have saved your settings to the User Profile", "Ok"))
                        {
                            AmbientSkiesEditorFunctions.SaveBuiltInAndLWRPSettings(m_profiles, m_creationToolSettings, systemtype, m_selectedSkyboxProfileIndex, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, skyboxRotation, m_selectedPostProcessingProfileIndex, m_selectedPostProcessingProfile, usePostProcess);
                        }
                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog("New Scene Detected", "This scene isn't using ambient skies content. We have saved your settings to the User Profile", "Ok"))
                    {
                        AmbientSkiesEditorFunctions.SaveBuiltInAndLWRPSettings(m_profiles, m_creationToolSettings, systemtype, m_selectedSkyboxProfileIndex, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, skyboxRotation, m_selectedPostProcessingProfileIndex, m_selectedPostProcessingProfile, usePostProcess);
                    }
                }

                UpdateMultiScene(m_profiles);
            }

            EditorGUILayout.BeginHorizontal();
            m_editorUtils.Label("SelectProfilesDropdown", GUILayout.Width(100f));
            newProfileListIndex = EditorGUILayout.Popup(m_profileListIndex, profileChoices.ToArray(), GUILayout.MaxWidth(2000f), GUILayout.Height(16f));
            EditorGUILayout.EndHorizontal();
            m_editorUtils.Text("SystemProfilesInfo");
            EditorGUILayout.Space();

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Settings will be saved to Ambient Skies even when in Play Mode. For changes to be applied upon exiting Play Mode, please make sure the Ambient Skies Editor Window is open.", MessageType.Info);
                //Repaint();
            }

            GUI.enabled = true;

            if (m_profiles.m_skyboxMaterial == null)
            {
                m_profiles.m_skyboxMaterial = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Ambient Skies Skybox"));
            }
            else if (m_profiles.m_skyboxMaterial.name != "Ambient Skies Skybox")
            {
                m_profiles.m_skyboxMaterial = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Ambient Skies Skybox"));
            }

            if (newProfileListIndex != m_profileListIndex)
            {
                m_profileListIndex = newProfileListIndex;
                m_profiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath(m_profileList[m_profileListIndex].name));

                EditorPrefs.SetString("AmbientSkiesActiveProfile_", m_profileList[m_profileListIndex].name);

                //Add skies profile names
                skyboxChoices.Clear();
                foreach (var profile in m_profiles.m_skyProfiles)
                {
                    skyboxChoices.Add(profile.name);
                }
                //Add procedural skies profile names
                proceduralSkyboxChoices.Clear();
                foreach (var profile in m_profiles.m_proceduralSkyProfiles)
                {
                    proceduralSkyboxChoices.Add(profile.name);
                }
#if UNITY_2018_3_OR_NEWER
                //Add gradient skies profile names
                gradientSkyboxChoices.Clear();
                foreach (var profile in m_profiles.m_gradientSkyProfiles)
                {
                    gradientSkyboxChoices.Add(profile.name);
                }
#endif
                //Add post processing profile names
                ppChoices.Clear();
                foreach (var profile in m_profiles.m_ppProfiles)
                {
                    ppChoices.Add(profile.name);
                }
                //Add lightmaps profile names
                lightmappingChoices.Clear();
                foreach (var profile in m_profiles.m_lightingProfiles)
                {
                    lightmappingChoices.Add(profile.name);
                }

                Repaint();

#if UNITY_POST_PROCESSING_STACK_V2

                //Apply settings
                GetAndApplyAllValuesFromAmbientSkies(true);

#endif
            }

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                if (PlayerSettings.colorSpace != ColorSpace.Linear)
                {
                    EditorGUILayout.HelpBox("High Definition Pipeline doesn't work in Gamma color space. Go to the lighting tab and enable lightmaps. Then in the main settings click Set Linear Deferred Lighting", MessageType.Error);
                }
            }

            m_editorUtils.Tabs(m_mainTabs);

            if (EditorApplication.isCompiling || ReflectionProbeUtils.m_probeRenderActive)
            {
                m_isCompiling = true;
            }
            else
            {
                m_isCompiling = false;
            }

            if (m_profiles != null && m_creationToolSettings != null)
            {
                m_profiles.m_selectedRenderPipeline = renderPipelineSettings;
                if (m_hasChanged)
                {
                    if (m_multiScenePresent)
                    {
                        if (m_savedSettings == null)
                        {
                            m_savedSettings = AmbientSkiesEditorFunctions.GetSavedSettingsComponent("Ambient Skies Global", false);
                        }

                        if (m_savedSettings != null)
                        {
                            ApplyCreationToolSettings(m_hasChanged);
                            m_savedSettings.SetSettings(m_profiles, m_creationToolSettings);
                        }
                    }
                    m_hasChanged = false;
                }
            }

            //End scroll
            GUILayout.EndScrollView();
        }


        /// <summary>
        /// On inspector GUI
        /// </summary>
        private void OnInspectorUpdate()
        {
            if (GraphicsSettings.renderPipelineAsset != m_currentRenderpipelineAsset)
            {
                m_needCompile = AmbientSkiesEditorFunctions.ApplyScriptingDefine(m_currentRenderpipelineAsset, m_profiles, out renderPipelineSettings, m_creationToolSettings);
                if (m_needCompile)
                {
                    m_currentRenderpipelineAsset = GraphicsSettings.renderPipelineAsset;
                }
            }

            if (m_hasChanged)
            {
                ApplyCreationToolSettings(m_hasChanged);
            }

            if (m_profiles.m_showDebug && m_profiles.m_showHasChangedDebug)
            {
                Debug.Log("Changes need to be made? " + m_hasChanged);
            }

            //Undo.undoRedoPerformed += UndoCallback;

            bool checkSuccessful = true;
            if (m_inspectorUpdate)
            {
                autoPostProcessingApplyTimer -= Time.deltaTime * 2f;

                if (m_profiles.m_showDebug && m_profiles.m_showTimersInDebug)
                {
                    Debug.Log("OnInspectorUpdate(): autoPostProcessApplyTimer = " + autoPostProcessingApplyTimer);
                }

                //If countdown active
                if (m_countdown > 0)
                {
#if UNITY_POST_PROCESSING_STACK_V2

                    //Debug.Log("Apply settings");

                    //Apply settings
                    GetAndApplyAllValuesFromAmbientSkies(false);

                    checkSuccessful = false;
#endif
                }

                if (autoPostProcessingApplyTimer > 0)
                {
                    if (m_currentInspectorUpdate >= m_profiles.m_inspectorUpdateLimit)
                    {
                        autoPostProcessingApplyTimer = -1;
                        m_currentInspectorUpdate = 0;
                        return;
                    }

                    if (autoMatchProfile)
                    {
                        if (!usePostProcess)
                        {
                            return;
                        }

                        //Debug.Log("Auto post processing");

                        if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                        {
                            if (string.IsNullOrEmpty(m_selectedSkyboxProfile.postProcessingAssetName))
                            {
                                if (m_profiles.m_showDebug)
                                {
                                    Debug.Log(m_selectedSkyboxProfile.name + " Does not contain a post processing asset name. Unable to match post processing to this skybox profile");
                                }
                                return;
                            }
                            else
                            {
                                if (m_selectedPostProcessingProfileIndex >= 0 && m_profiles.m_usePostFX)
                                {
                                    m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.CheckPostProcessing(m_selectedPostProcessingProfileIndex, m_profiles);
                                    m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingHDRIName(m_profiles, m_selectedSkyboxProfile);
                                    newPPSelection = m_selectedPostProcessingProfileIndex;
                                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                                    EditorUtility.SetDirty(m_creationToolSettings);
                                    m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                                }
                            }
                        }
                        else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                        {
                            if (string.IsNullOrEmpty(m_selectedProceduralSkyboxProfile.postProcessingAssetName))
                            {
                                Debug.Log(m_selectedProceduralSkyboxProfile.name + " Does not contain a post processing asset name. Unable to match post processing to this skybox profile");
                                return;
                            }
                            else
                            {
                                if (m_selectedPostProcessingProfileIndex >= 0 && m_profiles.m_usePostFX)
                                {
                                    m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.CheckPostProcessing(m_selectedPostProcessingProfileIndex, m_profiles);
                                    m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingProceduralName(m_profiles, m_selectedProceduralSkyboxProfile);
                                    newPPSelection = m_selectedPostProcessingProfileIndex;
                                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                                    EditorUtility.SetDirty(m_creationToolSettings);
                                    m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(m_selectedGradientSkyboxProfile.postProcessingAssetName))
                            {
                                Debug.Log(m_selectedGradientSkyboxProfile.name + " Does not contain a post processing asset name. Unable to match post processing to this skybox profile");
                                return;
                            }
                            else
                            {
                                if (m_selectedPostProcessingProfileIndex >= 0 && m_profiles.m_usePostFX)
                                {
                                    m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingGradientName(m_profiles, m_selectedGradientSkyboxProfile);
                                    newPPSelection = m_selectedPostProcessingProfileIndex;
                                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                                    EditorUtility.SetDirty(m_creationToolSettings);
                                    m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                                }
                            }
                        }

                        //Update post processing
                        PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);
                        if (m_profiles.m_showDebug)
                        {
                            Debug.Log("Applying post processing to new profile" + " , Current inspector update is: " + m_currentInspectorUpdate);
                        }

                        m_currentInspectorUpdate++;
                    }

                    checkSuccessful = false;
                }
                else
                {
                    checkSuccessful = true;
                    m_inspectorUpdate = false;
                }
            }

            //Bake Probes
            if (ReflectionProbeUtils.m_probeRenderActive)
            {
                if (ReflectionProbeUtils.m_storedProbes.Count > 0)
                {
                    float progrss =
                        Mathf.Round(
                            (ReflectionProbeUtils.m_currentProbeCount - ReflectionProbeUtils.m_storedProbes.Count) /
                            ReflectionProbeUtils.m_currentProbeCount);
                    EditorUtility.DisplayProgressBar("Baking Reflection Probes", "Probes remaining :" + ReflectionProbeUtils.m_storedProbes.Count, progrss);
                    ReflectionProbeUtils.m_storedProbes[0].enabled = true;
                    ReflectionProbeUtils.m_storedProbes.RemoveAt(0);
                }
                else
                {
                    ReflectionProbeUtils.m_probeRenderActive = false;
                    EditorUtility.ClearProgressBar();
                }
            }

            //Update strings
            seasonString = currentSeason.ToString();
            depthOfFieldDistanceString = depthOfFieldFocusDistance.ToString();
            probeSpawnCount = (reflectionProbesPerRow * reflectionProbesPerRow).ToString();
            lightProbeSpawnCount = (lightProbesPerRow * lightProbesPerRow).ToString();

            if (checkSuccessful)
            {
                Repaint();
                m_inspectorUpdate = false;
            }

            if (m_hasChanged)
            {
                AmbientSkiesEditorFunctions.MarkActiveSceneAsDirty(m_profiles);
                m_hasChanged = false;
            }
        }       

        #endregion

        #region Tabs

        /// <summary>
        /// Display the skyboxes tab
        /// </summary>
        private void SkyboxesTab()
        {
#if HDPipeline && !UNITY_2018_3_OR_NEWER
            EditorGUILayout.HelpBox("High Defination Pipeline does not works in this version of Unity. Please use 2018.3 or higher", MessageType.Error);
            return;
#else
            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            if (m_profiles != null)
            {
                if (m_needCompile)
                {
                    m_editorUtils.Text("PleaseWait...");
                    return;
                }
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    if (m_selectedSkyboxProfile == null)
                    {
                        Debug.LogError("Profile is missing, Ambient Skies will now close due to this error. Try reopening Ambient Skies window");
                        Close();
                        return;
                    }
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    if (m_selectedProceduralSkyboxProfile == null)
                    {
                        Debug.LogError("Profile is missing, Ambient Skies will now close due to this error. Try reopening Ambient Skies window");
                        Close();
                        return;
                    }
                }
                else
                {
                    if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
                        if (m_selectedGradientSkyboxProfile == null)
                        {
                            Debug.LogError("Profile is missing, Ambient Skies will now close due to this error. Try reopening Ambient Skies window");
                            Close();
                            return;
                        }
                    }
                }

                //See if we can select an ambiet skies skybox
                if (m_selectedSkyboxProfile == null)
                {
                    if (m_profiles.m_editSettings)
                    {
                        if (RenderSettings.ambientMode == AmbientMode.Skybox && RenderSettings.skybox != null && RenderSettings.skybox.name == "Ambient Skies Skybox")
                        {
                            if (m_editorUtils.Button("CreateSkyboxProfileButton"))
                            {
                                if (EditorUtility.DisplayDialog("ALERT!!",
                                    "Would you like to make a profile from this skybox?", "Yes", "Cancel"))
                                {
                                    if (SkyboxUtils.CreateProfileFromActiveSkybox(m_profiles))
                                    {
                                        m_selectedSkyboxProfileIndex = m_profiles.m_skyProfiles.Count - 1;
                                        m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];
                                        SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
                                    }
                                }
                            }

                            return;
                        }
                    }

                    if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
                        if (m_editorUtils.Button("SelectSkyboxProfileButton"))
                        {
                            EditorUtility.SetDirty(m_creationToolSettings);
                            m_selectedSkyboxProfileIndex = 0;
                            m_selectedProceduralSkyboxProfileIndex = 0;
                            m_selectedGradientSkyboxProfileIndex = 0;
                            m_creationToolSettings.m_selectedHDRI = 0;
                            m_creationToolSettings.m_selectedProcedural = 0;
                            m_creationToolSettings.m_selectedGradient = 0;

                            m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];
                            m_selectedProceduralSkyboxProfile = m_profiles.m_proceduralSkyProfiles[m_selectedProceduralSkyboxProfileIndex];
                            m_selectedGradientSkyboxProfile = m_profiles.m_gradientSkyProfiles[m_selectedGradientSkyboxProfileIndex];

                            SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
                        }

                        return;
                    }
                }
                else if (m_profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    if (GameObject.Find("High Definition Environment Volume") == null && systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        if (m_editorUtils.Button("SelectSkyboxProfileButton"))
                        {
                            EditorUtility.SetDirty(m_creationToolSettings);
                            m_selectedSkyboxProfileIndex = 0;
                            m_selectedProceduralSkyboxProfileIndex = 0;
                            m_selectedGradientSkyboxProfileIndex = 0;
                            m_creationToolSettings.m_selectedHDRI = 0;
                            m_creationToolSettings.m_selectedProcedural = 0;
                            m_creationToolSettings.m_selectedGradient = 0;

                            m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];
                            m_selectedProceduralSkyboxProfile = m_profiles.m_proceduralSkyProfiles[m_selectedProceduralSkyboxProfileIndex];
                            m_selectedGradientSkyboxProfile = m_profiles.m_gradientSkyProfiles[m_selectedGradientSkyboxProfileIndex];

                            AmbientSkiesEditorFunctions.CreateHDRPVolume("High Definition Environment Volume");

                            SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
                        }

                        return;
                    }
                }

                EditorGUI.BeginChangeCheck();

                #region Get Variables

                //Check skies
                useSkies = m_profiles.m_useSkies;

                //Time oof day
                useTimeOfDay = m_profiles.m_useTimeOfDay;

                //Global Settings
                systemtype = m_profiles.m_systemTypes;

                //Target Platform
                targetPlatform = m_profiles.m_targetPlatform;

                //Fog config
                configurationType = m_profiles.m_configurationType;

                //Check post processing
                usePostProcess = m_profiles.m_usePostFX;

                //Enable editing
                m_enableEditMode = m_profiles.m_editSettings;

                //Time of day
                #region Time Of Day Checks

                //Time Of Day Settings

                useTimeOfDay = m_profiles.m_useTimeOfDay;

                bool checkAllTODSettings = true;
                if (m_profiles.m_timeOfDayProfile != timeOfDayProfile)
                {
                    timeOfDayProfile = m_profiles.m_timeOfDayProfile;
                    m_profiles.m_timeOfDayProfile = timeOfDayProfile;

                    checkAllTODSettings = false;

                    /*
                    //Get all settings from new profile
                    currentSeason = m_profiles.timeOfDayProfile.m_environmentSeason;
                    hemisphereOrigin = m_profiles.timeOfDayProfile.m_hemisphereOrigin;
                    pauseTimeKey = m_profiles.timeOfDayProfile.m_pauseTimeKey;
                    incrementUpKey = m_profiles.timeOfDayProfile.m_incrementUpKey;
                    incrementDownKey = m_profiles.timeOfDayProfile.m_incrementDownKey;
                    timeToAddOrRemove = m_profiles.timeOfDayProfile.m_timeToAddOrRemove;
                    rotateSunLeftKey = m_profiles.timeOfDayProfile.m_rotateSunLeftKey;
                    rotateSunRightKey = m_profiles.timeOfDayProfile.m_rotateSunRightKey;
                    sunRotationAmount = m_profiles.timeOfDayProfile.m_sunRotationAmount;
                    pauseTime = m_profiles.timeOfDayProfile.m_pauseTime;
                    currentTimeOfDay = m_profiles.timeOfDayProfile.m_currentTime;
                    timeOfDaySkyboxRotation = m_profiles.timeOfDayProfile.m_sunRotation;
                    daySunIntensity = m_profiles.timeOfDayProfile.m_daySunIntensity;
                    daySunGradientColor = m_profiles.timeOfDayProfile.m_daySunGradientColor;
                    nightSunIntensity = m_profiles.timeOfDayProfile.m_nightSunIntensity;
                    nightSunGradientColor = m_profiles.timeOfDayProfile.m_nightSunGradientColor;
                    lightAnisotropy = m_profiles.timeOfDayProfile.m_lightAnisotropy;
                    lightProbeDimmer = m_profiles.timeOfDayProfile.m_lightProbeDimmer;
                    lightDepthExtent = m_profiles.timeOfDayProfile.m_lightDepthExtent;
                    sunSizeAmount = m_profiles.timeOfDayProfile.m_sunSize;
                    skyExposureAmount = m_profiles.timeOfDayProfile.m_skyExposure;
                    startFogDistance = m_profiles.timeOfDayProfile.m_startFogDistance;
                    dayFogDensity = m_profiles.timeOfDayProfile.m_dayFogDensity;
                    nightFogDensity = m_profiles.timeOfDayProfile.m_nightFogDensity;
                    dayFogDistance = m_profiles.timeOfDayProfile.m_dayFogDistance;
                    dayFogColor = m_profiles.timeOfDayProfile.m_dayFogColor;
                    nightFogDistance = m_profiles.timeOfDayProfile.m_nightFogDistance;
                    nightFogColor = m_profiles.timeOfDayProfile.m_nightFogColor;
                    dayTempature = m_profiles.timeOfDayProfile.m_dayTempature;
                    dayPostFXColor = m_profiles.timeOfDayProfile.m_dayColor;
                    nightTempature = m_profiles.timeOfDayProfile.m_nightTempature;
                    nightPostFXColor = m_profiles.timeOfDayProfile.m_nightColor;
                    syncPostProcessing = m_profiles.timeOfDayProfile.m_syncPostFXToTimeOfDay;
                    realtimeGIUpdate = m_profiles.timeOfDayProfile.m_realtimeGIUpdate;
                    gIUpdateInterval = m_profiles.timeOfDayProfile.m_gIUpdateIntervalInSeconds;
                    dayLengthInSeconds = m_profiles.timeOfDayProfile.m_dayLengthInSeconds;
                    nightLengthInSeconds = m_profiles.timeOfDayProfile.m_nightLengthInSeconds;
                    dayDate = m_profiles.timeOfDayProfile.m_day;
                    monthDate = m_profiles.timeOfDayProfile.m_month;
                    yearDate = m_profiles.timeOfDayProfile.m_year;
                    */
                }
                else
                {
                    timeOfDayProfile = m_profiles.m_timeOfDayProfile;
                }

                if (checkAllTODSettings)
                {
                    realtimeEmission = m_profiles.m_realtimeEmission;
                    syncRealtimeEmissionToTimeOfDay = m_profiles.m_syncRealtimeEmissionToTimeOfDay;

                    if (m_profiles.m_timeOfDayProfile != null)
                    {
                        if (currentSeason != m_profiles.m_timeOfDayProfile.m_environmentSeason)
                        {
                            currentSeason = m_profiles.m_timeOfDayProfile.m_environmentSeason;
                            m_profiles.m_currentSeason = m_profiles.m_timeOfDayProfile.m_environmentSeason;
                        }
                        else
                        {
                            currentSeason = m_profiles.m_currentSeason;
                        }
                        if (hemisphereOrigin != m_profiles.m_timeOfDayProfile.m_hemisphereOrigin)
                        {
                            hemisphereOrigin = m_profiles.m_timeOfDayProfile.m_hemisphereOrigin;
                            m_profiles.m_hemisphereOrigin = m_profiles.m_timeOfDayProfile.m_hemisphereOrigin;
                        }
                        else
                        {
                            hemisphereOrigin = m_profiles.m_hemisphereOrigin;
                        }
                        if (pauseTimeKey != m_profiles.m_timeOfDayProfile.m_pauseTimeKey)
                        {
                            pauseTimeKey = m_profiles.m_timeOfDayProfile.m_pauseTimeKey;
                            m_profiles.m_pauseTimeKey = m_profiles.m_timeOfDayProfile.m_pauseTimeKey;
                        }
                        else
                        {
                            pauseTimeKey = m_profiles.m_pauseTimeKey;
                        }
                        if (incrementUpKey != m_profiles.m_timeOfDayProfile.m_incrementUpKey)
                        {
                            incrementUpKey = m_profiles.m_timeOfDayProfile.m_incrementUpKey;
                            m_profiles.m_incrementUpKey = m_profiles.m_timeOfDayProfile.m_incrementUpKey;
                        }
                        else
                        {
                            incrementUpKey = m_profiles.m_incrementUpKey;
                        }
                        if (incrementDownKey != m_profiles.m_timeOfDayProfile.m_incrementDownKey)
                        {
                            incrementDownKey = m_profiles.m_timeOfDayProfile.m_incrementDownKey;
                            m_profiles.m_incrementDownKey = m_profiles.m_timeOfDayProfile.m_incrementDownKey;
                        }
                        else
                        {
                            incrementDownKey = m_profiles.m_incrementDownKey;
                        }
                        if (timeToAddOrRemove != m_profiles.m_timeOfDayProfile.m_timeToAddOrRemove)
                        {
                            timeToAddOrRemove = m_profiles.m_timeOfDayProfile.m_timeToAddOrRemove;
                            m_profiles.m_timeToAddOrRemove = m_profiles.m_timeOfDayProfile.m_timeToAddOrRemove;
                        }
                        else
                        {
                            timeToAddOrRemove = m_profiles.m_timeToAddOrRemove;
                        }
                        if (rotateSunLeftKey != m_profiles.m_timeOfDayProfile.m_rotateSunLeftKey)
                        {
                            rotateSunLeftKey = m_profiles.m_timeOfDayProfile.m_rotateSunLeftKey;
                            m_profiles.m_rotateSunLeftKey = m_profiles.m_timeOfDayProfile.m_rotateSunLeftKey;
                        }
                        else
                        {
                            rotateSunLeftKey = m_profiles.m_rotateSunLeftKey;
                        }
                        if (rotateSunRightKey != m_profiles.m_timeOfDayProfile.m_rotateSunRightKey)
                        {
                            rotateSunRightKey = m_profiles.m_timeOfDayProfile.m_rotateSunRightKey;
                            m_profiles.m_rotateSunRightKey = m_profiles.m_timeOfDayProfile.m_rotateSunRightKey;
                        }
                        else
                        {
                            rotateSunRightKey = m_profiles.m_rotateSunRightKey;
                        }
                        if (sunRotationAmount != m_profiles.m_timeOfDayProfile.m_sunRotationAmount)
                        {
                            sunRotationAmount = m_profiles.m_timeOfDayProfile.m_sunRotationAmount;
                            m_profiles.m_sunRotationAmount = m_profiles.m_timeOfDayProfile.m_sunRotationAmount;
                        }
                        else
                        {
                            sunRotationAmount = m_profiles.m_sunRotationAmount;
                        }
                        if (pauseTime != m_profiles.m_timeOfDayProfile.m_pauseTime)
                        {
                            pauseTime = m_profiles.m_timeOfDayProfile.m_pauseTime;
                            m_profiles.m_pauseTime = m_profiles.m_timeOfDayProfile.m_pauseTime;
                        }
                        else
                        {
                            pauseTime = m_profiles.m_pauseTime;
                        }
                        if (currentTimeOfDay != m_profiles.m_timeOfDayProfile.m_currentTime)
                        {
                            currentTimeOfDay = m_profiles.m_timeOfDayProfile.m_currentTime;
                            m_profiles.m_currentTimeOfDay = m_profiles.m_timeOfDayProfile.m_currentTime;
                        }
                        else
                        {
                            currentTimeOfDay = m_profiles.m_currentTimeOfDay;
                        }
                        if (timeOfDaySkyboxRotation != m_profiles.m_timeOfDayProfile.m_sunRotation)
                        {
                            timeOfDaySkyboxRotation = m_profiles.m_timeOfDayProfile.m_sunRotation;
                            m_profiles.m_sunRotationAmount = m_profiles.m_timeOfDayProfile.m_sunRotation;
                        }
                        else
                        {
                            timeOfDaySkyboxRotation = m_profiles.m_skyboxRotation;
                        }
                        if (daySunIntensity != m_profiles.m_timeOfDayProfile.m_daySunIntensity)
                        {
                            daySunIntensity = m_profiles.m_timeOfDayProfile.m_daySunIntensity;
                            m_profiles.m_daySunIntensity = m_profiles.m_timeOfDayProfile.m_daySunIntensity;
                        }
                        else
                        {
                            daySunIntensity = m_profiles.m_daySunIntensity;
                        }
                        if (daySunGradientColor != m_profiles.m_timeOfDayProfile.m_daySunGradientColor)
                        {
                            daySunGradientColor = m_profiles.m_timeOfDayProfile.m_daySunGradientColor;
                            m_profiles.m_daySunGradientColor = m_profiles.m_timeOfDayProfile.m_daySunGradientColor;
                        }
                        else
                        {
                            daySunGradientColor = m_profiles.m_daySunGradientColor;
                        }
                        if (nightSunIntensity != m_profiles.m_timeOfDayProfile.m_nightSunIntensity)
                        {
                            nightSunIntensity = m_profiles.m_timeOfDayProfile.m_nightSunIntensity;
                            m_profiles.m_nightSunIntensity = m_profiles.m_timeOfDayProfile.m_nightSunIntensity;
                        }
                        else
                        {
                            nightSunIntensity = m_profiles.m_nightSunIntensity;
                        }
                        if (nightSunGradientColor != m_profiles.m_timeOfDayProfile.m_nightSunGradientColor)
                        {
                            nightSunGradientColor = m_profiles.m_timeOfDayProfile.m_nightSunGradientColor;
                            m_profiles.m_nightSunGradientColor = m_profiles.m_timeOfDayProfile.m_nightSunGradientColor;
                        }
                        else
                        {
                            nightSunGradientColor = m_profiles.m_nightSunGradientColor;
                        }
                        if (lightAnisotropy != m_profiles.m_timeOfDayProfile.m_lightAnisotropy)
                        {
                            lightAnisotropy = m_profiles.m_timeOfDayProfile.m_lightAnisotropy;
                            m_profiles.m_lightAnisotropy = m_profiles.m_timeOfDayProfile.m_lightAnisotropy;
                        }
                        else
                        {
                            lightAnisotropy = m_profiles.m_lightAnisotropy;
                        }
                        if (lightProbeDimmer != m_profiles.m_timeOfDayProfile.m_lightProbeDimmer)
                        {
                            lightProbeDimmer = m_profiles.m_timeOfDayProfile.m_lightProbeDimmer;
                            m_profiles.m_lightProbeDimmer = m_profiles.m_timeOfDayProfile.m_lightProbeDimmer;
                        }
                        else
                        {
                            lightProbeDimmer = m_profiles.m_lightProbeDimmer;
                        }
                        if (lightDepthExtent != m_profiles.m_timeOfDayProfile.m_lightDepthExtent)
                        {
                            lightDepthExtent = m_profiles.m_timeOfDayProfile.m_lightDepthExtent;
                            m_profiles.m_lightDepthExtent = m_profiles.m_timeOfDayProfile.m_lightDepthExtent;
                        }
                        else
                        {
                            lightDepthExtent = m_profiles.m_lightDepthExtent;
                        }
                        if (sunSizeAmount != m_profiles.m_timeOfDayProfile.m_sunSize)
                        {
                            sunSizeAmount = m_profiles.m_timeOfDayProfile.m_sunSize;
                            m_profiles.m_sunSize = m_profiles.m_timeOfDayProfile.m_sunSize;
                        }
                        else
                        {
                            sunSizeAmount = m_profiles.m_sunSize;
                        }
                        if (skyExposureAmount != m_profiles.m_timeOfDayProfile.m_skyExposure)
                        {
                            skyExposureAmount = m_profiles.m_timeOfDayProfile.m_skyExposure;
                            m_profiles.m_skyExposure = m_profiles.m_timeOfDayProfile.m_skyExposure;
                        }
                        else
                        {
                            skyExposureAmount = m_profiles.m_skyExposure;
                        }
                        if (pBSGroundColor != m_profiles.m_timeOfDayProfile.m_pBSGroundColor)
                        {
                            pBSGroundColor = m_profiles.m_timeOfDayProfile.m_pBSGroundColor;
                            m_profiles.m_pBSGroundColor = m_profiles.m_timeOfDayProfile.m_pBSGroundColor;
                        }
                        else
                        {
                            pBSGroundColor = m_profiles.m_pBSGroundColor;
                        }
                        if (pBSAreosolDensity != m_profiles.m_timeOfDayProfile.m_pBSAreosolDensity)
                        {
                            pBSAreosolDensity = m_profiles.m_timeOfDayProfile.m_pBSAreosolDensity;
                            m_profiles.m_pBSAreosolDensity = m_profiles.m_timeOfDayProfile.m_pBSAreosolDensity;
                        }
                        else
                        {
                            pBSAreosolDensity = m_profiles.m_pBSAreosolDensity;
                        }

                        if (pBSSpaceEmissionTexture != m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionTexture)
                        {
                            pBSSpaceEmissionTexture = m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionTexture;
                            m_profiles.m_pBSSpaceEmissionTexture = m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionTexture;
                        }
                        else
                        {
                            pBSSpaceEmissionTexture = m_profiles.m_pBSSpaceEmissionTexture;
                        }
                        if (pBSTODSpaceEmissionMultiplier != m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionMultiplier)
                        {
                            pBSTODSpaceEmissionMultiplier = m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionMultiplier;
                            m_profiles.m_pBSSpaceEmissionMultiplier = m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionMultiplier;
                        }
                        else
                        {
                            pBSTODSpaceEmissionMultiplier = m_profiles.m_pBSSpaceEmissionMultiplier;
                        }

                        if (startFogDistance != m_profiles.m_timeOfDayProfile.m_startFogDistance)
                        {
                            startFogDistance = m_profiles.m_timeOfDayProfile.m_startFogDistance;
                            m_profiles.m_startFogDistance = m_profiles.m_timeOfDayProfile.m_startFogDistance;
                        }
                        else
                        {
                            startFogDistance = m_profiles.m_startFogDistance;
                        }
                        if (dayFogDensity != m_profiles.m_timeOfDayProfile.m_dayFogDensity)
                        {
                            dayFogDensity = m_profiles.m_timeOfDayProfile.m_dayFogDensity;
                            m_profiles.m_dayFogDensity = m_profiles.m_timeOfDayProfile.m_dayFogDensity;
                        }
                        else
                        {
                            dayFogDensity = m_profiles.m_dayFogDensity;
                        }
                        if (nightFogDensity != m_profiles.m_timeOfDayProfile.m_nightFogDensity)
                        {
                            nightFogDensity = m_profiles.m_timeOfDayProfile.m_nightFogDensity;
                            m_profiles.m_nightFogDensity = m_profiles.m_timeOfDayProfile.m_nightFogDensity;
                        }
                        else
                        {
                            nightFogDensity = m_profiles.m_nightFogDensity;
                        }
                        if (dayFogDistance != m_profiles.m_timeOfDayProfile.m_dayFogDistance)
                        {
                            dayFogDistance = m_profiles.m_timeOfDayProfile.m_dayFogDistance;
                            m_profiles.m_dayFogDistance = m_profiles.m_timeOfDayProfile.m_dayFogDistance;
                        }
                        else
                        {
                            dayFogDistance = m_profiles.m_dayFogDistance;
                        }
                        if (dayFogColor != m_profiles.m_timeOfDayProfile.m_dayFogColor)
                        {
                            dayFogColor = m_profiles.m_timeOfDayProfile.m_dayFogColor;
                            m_profiles.m_dayFogColor = m_profiles.m_timeOfDayProfile.m_dayFogColor;
                        }
                        else
                        {
                            dayFogColor = m_profiles.m_dayFogColor;
                        }
                        if (nightFogDistance != m_profiles.m_timeOfDayProfile.m_nightFogDistance)
                        {
                            nightFogDistance = m_profiles.m_timeOfDayProfile.m_nightFogDistance;
                            m_profiles.m_nightFogDistance = m_profiles.m_timeOfDayProfile.m_nightFogDistance;
                        }
                        else
                        {
                            nightFogDistance = m_profiles.m_nightFogDistance;
                        }
                        if (nightFogColor != m_profiles.m_timeOfDayProfile.m_nightFogColor)
                        {
                            nightFogColor = m_profiles.m_timeOfDayProfile.m_nightFogColor;
                            m_profiles.m_nightFogColor = m_profiles.m_timeOfDayProfile.m_nightFogColor;
                        }
                        else
                        {
                            nightFogColor = m_profiles.m_nightFogColor;
                        }
                        if (dayTempature != m_profiles.m_timeOfDayProfile.m_dayTempature)
                        {
                            dayTempature = m_profiles.m_timeOfDayProfile.m_dayTempature;
                            m_profiles.m_dayTempature = m_profiles.m_timeOfDayProfile.m_dayTempature;
                        }
                        else
                        {
                            dayTempature = m_profiles.m_dayTempature;
                        }
                        if (dayPostFXColor != m_profiles.m_timeOfDayProfile.m_dayColor)
                        {
                            dayPostFXColor = m_profiles.m_timeOfDayProfile.m_dayColor;
                            m_profiles.m_dayPostFXColor = m_profiles.m_timeOfDayProfile.m_dayColor;
                        }
                        else
                        {
                            dayPostFXColor = m_profiles.m_dayPostFXColor;
                        }
                        if (nightTempature != m_profiles.m_timeOfDayProfile.m_nightTempature)
                        {
                            nightTempature = m_profiles.m_timeOfDayProfile.m_nightTempature;
                            m_profiles.m_nightTempature = m_profiles.m_timeOfDayProfile.m_nightTempature;
                        }
                        else
                        {
                            nightTempature = m_profiles.m_nightTempature;
                        }
                        if (nightPostFXColor != m_profiles.m_timeOfDayProfile.m_nightColor)
                        {
                            nightPostFXColor = m_profiles.m_timeOfDayProfile.m_nightColor;
                            m_profiles.m_nightPostFXColor = m_profiles.m_timeOfDayProfile.m_nightColor;
                        }
                        else
                        {
                            nightPostFXColor = m_profiles.m_nightPostFXColor;
                        }
                        if (syncPostProcessing != m_profiles.m_timeOfDayProfile.m_syncPostFXToTimeOfDay)
                        {
                            syncPostProcessing = m_profiles.m_timeOfDayProfile.m_syncPostFXToTimeOfDay;
                            m_profiles.m_syncPostProcessing = m_profiles.m_timeOfDayProfile.m_syncPostFXToTimeOfDay;
                        }
                        else
                        {
                            syncPostProcessing = m_profiles.m_syncPostProcessing;
                        }
                        if (realtimeGIUpdate != m_profiles.m_timeOfDayProfile.m_realtimeGIUpdate)
                        {
                            realtimeGIUpdate = m_profiles.m_timeOfDayProfile.m_realtimeGIUpdate;
                            m_profiles.m_realtimeGIUpdate = m_profiles.m_timeOfDayProfile.m_realtimeGIUpdate;
                        }
                        else
                        {
                            realtimeGIUpdate = m_profiles.m_realtimeGIUpdate;
                        }
                        if (gIUpdateInterval != m_profiles.m_timeOfDayProfile.m_gIUpdateIntervalInSeconds)
                        {
                            gIUpdateInterval = m_profiles.m_timeOfDayProfile.m_gIUpdateIntervalInSeconds;
                            m_profiles.m_gIUpdateInterval = m_profiles.m_timeOfDayProfile.m_gIUpdateIntervalInSeconds;
                        }
                        else
                        {
                            gIUpdateInterval = m_profiles.m_gIUpdateInterval;
                        }
                        if (dayLengthInSeconds != m_profiles.m_timeOfDayProfile.m_dayLengthInSeconds)
                        {
                            dayLengthInSeconds = m_profiles.m_timeOfDayProfile.m_dayLengthInSeconds;
                            m_profiles.m_dayLengthInSeconds = m_profiles.m_timeOfDayProfile.m_dayLengthInSeconds;
                        }
                        else
                        {
                            dayLengthInSeconds = m_profiles.m_dayLengthInSeconds;
                        }
                        if (nightLengthInSeconds != m_profiles.m_timeOfDayProfile.m_nightLengthInSeconds)
                        {
                            nightLengthInSeconds = m_profiles.m_timeOfDayProfile.m_nightLengthInSeconds;
                            m_profiles.m_nightLengthInSeconds = m_profiles.m_timeOfDayProfile.m_nightLengthInSeconds;
                        }
                        else
                        {
                            nightLengthInSeconds = m_profiles.m_nightLengthInSeconds;
                        }
                        if (dayDate != m_profiles.m_timeOfDayProfile.m_day)
                        {
                            dayDate = m_profiles.m_timeOfDayProfile.m_day;
                            m_profiles.m_dayDate = m_profiles.m_timeOfDayProfile.m_day;
                        }
                        else
                        {
                            dayDate = m_profiles.m_dayDate;
                        }
                        if (monthDate != m_profiles.m_timeOfDayProfile.m_month)
                        {
                            monthDate = m_profiles.m_timeOfDayProfile.m_month;
                            m_profiles.m_monthDate = m_profiles.m_timeOfDayProfile.m_month;
                        }
                        else
                        {
                            monthDate = m_profiles.m_monthDate;
                        }
                        if (yearDate != m_profiles.m_timeOfDayProfile.m_year)
                        {
                            yearDate = m_profiles.m_timeOfDayProfile.m_year;
                            m_profiles.m_yearDate = m_profiles.m_timeOfDayProfile.m_year;
                        }
                        else
                        {
                            yearDate = m_profiles.m_yearDate;
                        }
                    }
                }

                #endregion

                #endregion

                //Skybox GUI
                m_editorUtils.Heading("SkySettingsHeader");

                useSkies = m_editorUtils.ToggleLeft("UseSkies", useSkies);
                m_profiles.m_useSkies = useSkies;
                EditorGUILayout.Space();

                //If use skies system
                if (useSkies)
                {
                    m_editorUtils.Link("LearnMoreAboutSceneLighting");
                    EditorGUILayout.Space();

                    m_foldoutGlobalSettings = m_editorUtils.Panel("Show Global Settings", GlobalSettingsEnabled, m_foldoutGlobalSettings);

                    if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
                        if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                        {
                            m_foldoutProfileSettings = m_editorUtils.Panel("Show Profile Settings", ProfileSettingsEnabled, m_foldoutProfileSettings);
                        }
                        else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                        {
                            m_foldoutProfileSettings = m_editorUtils.Panel("Show Profile Settings", ProfileSettingsEnabled, m_foldoutProfileSettings);
                        }
                        else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                        {
                            m_foldoutProfileSettings = m_editorUtils.Panel("Show Profile Settings", ProfileSettingsEnabled, m_foldoutProfileSettings);
                        }
                    }
                    else
                    {
                        if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                        {
                            m_foldoutProfileSettings = m_editorUtils.Panel("Show Profile Settings", ProfileSettingsEnabled, m_foldoutProfileSettings);
                        }
                        else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                        {
                            m_foldoutProfileSettings = m_editorUtils.Panel("Show Profile Settings", ProfileSettingsEnabled, m_foldoutProfileSettings);
                        }
                    }

                    HorizonUtils.SetHorizonShaderSettings(m_profiles, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, m_hasChanged);

                    //Save Skybox settings to defaults
                    if (m_profiles.m_editSettings)
                    {
                        if (m_editorUtils.ButtonRight("SaveSkyboxSettingsToDefaultsButton"))
                        {
                            if (EditorUtility.DisplayDialog("WARNING!!", "Are you sure you want to replace the default settings?", "Make My Day!", "Cancel"))
                            {
                                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                                {
                                    m_profiles.SaveSkyboxSettingsToDefault(m_selectedSkyboxProfileIndex);
                                }
                                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                                {
                                    m_profiles.SaveSkyboxSettingsToDefault(m_selectedProceduralSkyboxProfileIndex);
                                }
                                else
                                {
                                    m_profiles.SaveSkyboxSettingsToDefault(m_selectedGradientSkyboxProfileIndex);
                                }
                            }
                        }
                    }

                    //Revert Skybox
                    if (m_editorUtils.ButtonRight("RevertSkyboxButton"))
                    {
                        //Apply index to profile in creation settings
                        if (m_creationToolSettings != null)
                        {
                            EditorUtility.SetDirty(m_creationToolSettings);
                            m_creationToolSettings.m_selectedHDRI = m_selectedSkyboxProfileIndex;
                            m_creationToolSettings.m_selectedProcedural = m_selectedProceduralSkyboxProfileIndex;
                            m_creationToolSettings.m_selectedGradient = m_selectedGradientSkyboxProfileIndex;
                            m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                            m_creationToolSettings.m_selectedLighting = m_selectedLightingProfileIndex;
                        }

                        //Revert Settings
                        m_profiles.RevertSkyboxSettingsToDefault(m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex);

                        AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(m_profiles);

                        SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                        AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToFalse(m_profiles);

                        m_hasChanged = true;
                    }
                }

                //Apply settings
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_profiles, "Made changes");
                    EditorUtility.SetDirty(m_profiles);
                    EditorUtility.SetDirty(m_creationToolSettings);

                    #region Sky tab variables

                    //Skies Settings
                    m_profiles.m_useSkies = useSkies;

                    //Time of day setting
                    m_profiles.m_useTimeOfDay = useTimeOfDay;

                    //Update creation tools
                    m_creationToolSettings.m_selectedSystem = m_profileListIndex;

                    //Global Settings
                    m_profiles.m_systemTypes = systemtype;

                    //Target Platform
                    m_profiles.m_targetPlatform = targetPlatform;

                    //VSync Settings
                    m_profiles.m_vSyncMode = vSyncMode;

                    //Auto match profile
                    m_profiles.m_autoMatchProfile = autoMatchProfile;

                    //Profiles
                    m_selectedSkyboxProfileIndex = newSkyboxSelection;
                    m_selectedProceduralSkyboxProfileIndex = newProceduralSkyboxSelection;
                    m_selectedGradientSkyboxProfileIndex = newGradientSkyboxSelection;

                    //Fog config
                    m_profiles.m_configurationType = configurationType;

                    if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
#if UNITY_POST_PROCESSING_STACK_V2
                        m_selectedSkyboxProfile.creationMatchPostProcessing = m_creationMatchPostProcessing;
                        if (m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile != null)
                        {
                            m_selectedSkyboxProfile.postProcessingAssetName = m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile.name;
                        }
#endif
                    }
                    else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        //Edit mode
                        if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                        {
#if UNITY_POST_PROCESSING_STACK_V2
                            m_selectedProceduralSkyboxProfile.postProcessingAssetName = postProcessingAssetName;
#endif
                        }
                    }
                    else
                    {
                        //Edit mode
                        if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                        {
#if UNITY_POST_PROCESSING_STACK_V2
                            m_selectedGradientSkyboxProfile.postProcessingAssetName = postProcessingAssetName;
#endif
                        }
                    }

                    #region Commented Out

                    //Settings
                    /*
                    if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
                        //Fog mode
                        m_selectedSkyboxProfile.fogType = fogType;

                        //Ambient mode
                        m_selectedSkyboxProfile.ambientMode = ambientMode;

                        //Edit mode
                        if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                        {
                            m_selectedSkyboxProfile.name = m_skiesProfileName;
                            m_selectedSkyboxProfile.creationHDRIAsset = m_hdriAssetName;
                            if (m_hdriAssetName != null)
                            {
                                m_selectedSkyboxProfile.assetName = m_hdriAssetName.name;
                            }
#if UNITY_POST_PROCESSING_STACK_V2
                            m_selectedSkyboxProfile.creationMatchPostProcessing = m_creationMatchPostProcessing;
                            if (m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile != null)
                            {
                                m_selectedSkyboxProfile.postProcessingAssetName = m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile.name;
                            }
#endif
                        }

                        //Skybox
                        m_selectedSkyboxProfile.skyboxTint = skyboxTint;
                        m_selectedSkyboxProfile.skyboxExposure = skyboxExposure;
                        m_selectedSkyboxProfile.skyboxRotation = skyboxRotation;
                        m_selectedSkyboxProfile.skyMultiplier = skyMultiplier;
                        m_selectedSkyboxProfile.skyboxPitch = skyboxPitch;
                        m_selectedSkyboxProfile.customSkybox = customSkybox;
                        m_selectedSkyboxProfile.isProceduralSkybox = isProceduralSkybox;

                        //Fog settings
                        m_selectedSkyboxProfile.fogColor = fogColor;
                        m_selectedSkyboxProfile.fogDistance = fogDistance;
                        m_selectedSkyboxProfile.nearFogDistance = nearFogDistance;
                        m_selectedSkyboxProfile.fogDensity = fogDensity;

                        //Ambient Settings
                        m_selectedSkyboxProfile.skyColor = skyColor;
                        m_selectedSkyboxProfile.equatorColor = equatorColor;
                        m_selectedSkyboxProfile.groundColor = groundColor;
                        m_selectedSkyboxProfile.skyboxGroundIntensity = skyboxGroundIntensity;

                        //Sun Settings
                        m_selectedSkyboxProfile.shadowStrength = shadowStrength;
                        m_selectedSkyboxProfile.indirectLightMultiplier = indirectLightMultiplier;
                        m_selectedSkyboxProfile.sunColor = sunColor;
                        m_selectedSkyboxProfile.sunIntensity = sunIntensity;

                        //Shadow Settings
                        m_selectedSkyboxProfile.shadowDistance = shadowDistance;
                        m_selectedSkyboxProfile.shadowmaskMode = shadowmaskMode;
                        m_selectedSkyboxProfile.shadowType = shadowType;
                        m_selectedSkyboxProfile.shadowResolution = shadowResolution;
                        m_selectedSkyboxProfile.shadowProjection = shadowProjection;
                        m_selectedSkyboxProfile.cascadeCount = shadowCascade;

                        //Horizon Settings
                        m_selectedSkyboxProfile.scaleHorizonObjectWithFog = scaleHorizonObjectWithFog;
                        m_selectedSkyboxProfile.horizonSkyEnabled = horizonEnabled;
                        m_selectedSkyboxProfile.horizonScattering = horizonScattering;
                        m_selectedSkyboxProfile.horizonFogDensity = horizonFogDensity;
                        m_selectedSkyboxProfile.horizonFalloff = horizonFalloff;
                        m_selectedSkyboxProfile.horizonBlend = horizonBlend;
                        m_selectedSkyboxProfile.horizonSize = horizonScale;
                        m_selectedSkyboxProfile.followPlayer = followPlayer;
                        m_selectedSkyboxProfile.horizonUpdateTime = horizonUpdateTime;
                        m_selectedSkyboxProfile.horizonPosition = horizonPosition;
                        m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;

                        //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                        //Fog Mode
                        fogColorMode = m_selectedSkyboxProfile.fogColorMode;

                        //Volumetric Fog
                        m_selectedSkyboxProfile.volumetricEnableDistanceFog = useDistanceFog;
                        m_selectedSkyboxProfile.volumetricBaseFogDistance = baseFogDistance;
                        m_selectedSkyboxProfile.volumetricBaseFogHeight = baseFogHeight;
                        m_selectedSkyboxProfile.volumetricMeanHeight = meanFogHeight;
                        m_selectedSkyboxProfile.volumetricGlobalAnisotropy = globalAnisotropy;
                        m_selectedSkyboxProfile.volumetricGlobalLightProbeDimmer = globalLightProbeDimmer;

                        //Exponential Fog
                        m_selectedSkyboxProfile.exponentialFogDensity = exponentialFogDensity;
                        m_selectedSkyboxProfile.exponentialBaseHeight = exponentialBaseFogHeight;
                        m_selectedSkyboxProfile.exponentialHeightAttenuation = exponentialHeightAttenuation;
                        m_selectedSkyboxProfile.exponentialMaxFogDistance = exponentialMaxFogDistance;
                        m_selectedSkyboxProfile.exponentialMipFogNear = exponentialMipFogNear;
                        m_selectedSkyboxProfile.exponentialMipFogFar = exponentialMipFogFar;
                        m_selectedSkyboxProfile.exponentialMipFogMaxMip = exponentialMipFogMax;

                        //Linear Fog
                        m_selectedSkyboxProfile.linearFogDensity = linearFogDensity;
                        m_selectedSkyboxProfile.linearHeightStart = linearFogHeightStart;
                        m_selectedSkyboxProfile.linearHeightEnd = linearFogHeightEnd;
                        m_selectedSkyboxProfile.linearMaxFogDistance = linearFogMaxDistance;
                        m_selectedSkyboxProfile.linearMipFogNear = linearMipFogNear;
                        m_selectedSkyboxProfile.linearMipFogFar = linearMipFogFar;
                        m_selectedSkyboxProfile.linearMipFogMaxMip = linearMipFogMax;

                        //Volumetric Light Controller
                        m_selectedSkyboxProfile.volumetricDistanceRange = depthExtent;
                        m_selectedSkyboxProfile.volumetricSliceDistributionUniformity = sliceDistribution;

                        //Density Fog Volume
                        m_selectedSkyboxProfile.useFogDensityVolume = useDensityFogVolume;
                        m_selectedSkyboxProfile.singleScatteringAlbedo = singleScatteringAlbedo;
                        m_selectedSkyboxProfile.densityVolumeFogDistance = densityVolumeFogDistance;
                        m_selectedSkyboxProfile.fogDensityMaskTexture = fogDensityMaskTexture;
                        m_selectedSkyboxProfile.densityMaskTiling = densityMaskTiling;
                        m_selectedSkyboxProfile.densityScale = densityScale;

                        //HD Shadows
                        m_selectedSkyboxProfile.shadowQuality = hDShadowQuality;
                        m_selectedSkyboxProfile.cascadeSplit1 = split1;
                        m_selectedSkyboxProfile.cascadeSplit2 = split2;
                        m_selectedSkyboxProfile.cascadeSplit3 = split3;

                        //Contact Shadows
                        m_selectedSkyboxProfile.useContactShadows = enableContactShadows;
                        m_selectedSkyboxProfile.contactShadowsLength = contactLength;
                        m_selectedSkyboxProfile.contactShadowsDistanceScaleFactor = contactScaleFactor;
                        m_selectedSkyboxProfile.contactShadowsMaxDistance = contactMaxDistance;
                        m_selectedSkyboxProfile.contactShadowsFadeDistance = contactFadeDistance;
                        m_selectedSkyboxProfile.contactShadowsSampleCount = contactSampleCount;
                        m_selectedSkyboxProfile.contactShadowsOpacity = contactOpacity;

                        //Micro Shadows
                        m_selectedSkyboxProfile.useMicroShadowing = enableMicroShadows;
                        m_selectedSkyboxProfile.microShadowOpacity = microShadowOpacity;

                        //SS Reflection
                        m_selectedSkyboxProfile.enableScreenSpaceReflections = enableSSReflection;
                        m_selectedSkyboxProfile.screenEdgeFadeDistance = ssrEdgeFade;
                        m_selectedSkyboxProfile.maxNumberOfRaySteps = ssrNumberOfRays;
                        m_selectedSkyboxProfile.objectThickness = ssrObjectThickness;
                        m_selectedSkyboxProfile.minSmoothness = ssrMinSmoothness;
                        m_selectedSkyboxProfile.smoothnessFadeStart = ssrSmoothnessFade;
                        m_selectedSkyboxProfile.reflectSky = ssrReflectSky;

                        //SS Refract
                        m_selectedSkyboxProfile.enableScreenSpaceRefractions = enableSSRefraction;
                        m_selectedSkyboxProfile.screenWeightDistance = ssrWeightDistance;

                        //Ambient Lighting
                        m_selectedSkyboxProfile.useBakingSky = useStaticLighting;
                        m_selectedSkyboxProfile.indirectDiffuseIntensity = diffuseAmbientIntensity;
                        m_selectedSkyboxProfile.indirectSpecularIntensity = specularAmbientIntensity;
#endif

                    }
                    else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        //Fog mode
                        m_selectedProceduralSkyboxProfile.fogType = fogType;

                        //Ambient mode
                        m_selectedProceduralSkyboxProfile.ambientMode = ambientMode;

                        //Edit mode
                        if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                        {
#if UNITY_POST_PROCESSING_STACK_V2
                            m_selectedProceduralSkyboxProfile.postProcessingAssetName = postProcessingAssetName;
#endif
                        }

                        //Skybox
                        m_selectedProceduralSkyboxProfile.proceduralSkyTint = skyboxTint;
                        m_selectedProceduralSkyboxProfile.proceduralSkyExposure = skyboxExposure;
                        m_selectedProceduralSkyboxProfile.proceduralSkyboxRotation = skyboxRotation;
                        m_selectedProceduralSkyboxProfile.proceduralSkyMultiplier = skyMultiplier;
                        m_selectedProceduralSkyboxProfile.proceduralSkyboxPitch = skyboxPitch;
                        m_selectedProceduralSkyboxProfile.proceduralSunSize = proceduralSunSize;
                        m_selectedProceduralSkyboxProfile.proceduralSunSizeConvergence = proceduralSunSizeConvergence;
                        m_selectedProceduralSkyboxProfile.proceduralAtmosphereThickness = proceduralAtmosphereThickness;
                        m_selectedProceduralSkyboxProfile.proceduralGroundColor = proceduralGroundColor;
                        m_selectedProceduralSkyboxProfile.includeSunInBaking = includeSunInBaking;
                        m_selectedProceduralSkyboxProfile.enableSunDisk = enableSunDisk;

                        //Time of day
            #region TOD

                        m_profiles.m_pauseTimeKey = pauseTimeKey;
                        m_profiles.m_incrementUpKey = incrementUpKey;
                        m_profiles.m_incrementDownKey = incrementDownKey;
                        m_profiles.m_timeToAddOrRemove = timeToAddOrRemove;
                        m_profiles.m_rotateSunLeftKey = rotateSunLeftKey;
                        m_profiles.m_rotateSunRightKey = rotateSunRightKey;
                        m_profiles.m_sunRotationAmount = sunRotationAmount;

                        m_profiles.m_daySunIntensity = daySunIntensity;
                        m_profiles.m_daySunGradientColor = daySunGradientColor;
                        m_profiles.m_nightSunIntensity = nightSunIntensity;
                        m_profiles.m_nightSunGradientColor = nightSunGradientColor;

                        m_profiles.m_lightAnisotropy = lightAnisotropy;
                        m_profiles.m_lightProbeDimmer = lightProbeDimmer;
                        m_profiles.m_lightDepthExtent = lightDepthExtent;
                        m_profiles.m_sunSize = sunSizeAmount;
                        m_profiles.m_skyExposure = skyExposureAmount;

                        m_profiles.m_startFogDistance = startFogDistance;
                        m_profiles.m_dayFogDensity = dayFogDensity;
                        m_profiles.m_nightFogDensity = nightFogDensity;
                        m_profiles.m_dayFogDistance = dayFogDistance;
                        m_profiles.m_dayFogColor = dayFogColor;
                        m_profiles.m_nightFogDistance = nightFogDistance;
                        m_profiles.m_nightFogColor = nightFogColor;

                        m_profiles.m_syncPostProcessing = syncPostProcessing;
                        m_profiles.m_dayTempature = dayTempature;
                        m_profiles.m_dayPostFXColor = dayPostFXColor;
                        m_profiles.m_nightTempature = nightTempature;
                        m_profiles.m_nightPostFXColor = nightPostFXColor;

                        m_profiles.m_realtimeGIUpdate = realtimeGIUpdate;
                        m_profiles.m_gIUpdateInterval = gIUpdateInterval;

                        m_profiles.m_currentSeason = currentSeason;
                        m_profiles.m_hemisphereOrigin = hemisphereOrigin;

                        m_profiles.m_skyboxRotation = timeOfDaySkyboxRotation;
                        m_selectedProceduralSkyboxProfile.enableSunDisk = enableSunDisk;

                        m_profiles.m_pauseTime = pauseTime;
                        m_profiles.m_currentTimeOfDay = currentTimeOfDay;
                        m_profiles.m_nightLengthInSeconds = nightLengthInSeconds;
                        m_profiles.m_dayLengthInSeconds = dayLengthInSeconds;
                        m_profiles.m_dayDate = dayDate;
                        m_profiles.m_monthDate = monthDate;
                        m_profiles.m_yearDate = yearDate;

            #endregion

                        //Fog settings
                        m_selectedProceduralSkyboxProfile.proceduralFogColor = fogColor;
                        m_selectedProceduralSkyboxProfile.proceduralFogDistance = fogDistance;
                        m_selectedProceduralSkyboxProfile.proceduralNearFogDistance = nearFogDistance;
                        m_selectedProceduralSkyboxProfile.proceduralFogDensity = fogDensity;

                        //Ambient Settings
                        m_selectedProceduralSkyboxProfile.skyColor = skyColor;
                        m_selectedProceduralSkyboxProfile.equatorColor = equatorColor;
                        m_selectedProceduralSkyboxProfile.groundColor = groundColor;
                        m_selectedProceduralSkyboxProfile.skyboxGroundIntensity = skyboxGroundIntensity;

                        //Sun Settings
                        m_selectedProceduralSkyboxProfile.shadowStrength = shadowStrength;
                        m_selectedProceduralSkyboxProfile.indirectLightMultiplier = indirectLightMultiplier;
                        m_selectedProceduralSkyboxProfile.proceduralSunColor = sunColor;
                        m_selectedProceduralSkyboxProfile.proceduralSunIntensity = sunIntensity;

                        //Shadow Settings
                        m_selectedProceduralSkyboxProfile.shadowDistance = shadowDistance;
                        m_selectedProceduralSkyboxProfile.shadowmaskMode = shadowmaskMode;
                        m_selectedProceduralSkyboxProfile.shadowType = shadowType;
                        m_selectedProceduralSkyboxProfile.shadowResolution = shadowResolution;
                        m_selectedProceduralSkyboxProfile.shadowProjection = shadowProjection;
                        m_selectedProceduralSkyboxProfile.cascadeCount = shadowCascade;

                        //Horizon Settings
                        m_selectedProceduralSkyboxProfile.scaleHorizonObjectWithFog = scaleHorizonObjectWithFog;
                        m_selectedProceduralSkyboxProfile.horizonSkyEnabled = horizonEnabled;
                        m_selectedProceduralSkyboxProfile.horizonScattering = horizonScattering;
                        m_selectedProceduralSkyboxProfile.horizonFogDensity = horizonFogDensity;
                        m_selectedProceduralSkyboxProfile.horizonFalloff = horizonFalloff;
                        m_selectedProceduralSkyboxProfile.horizonBlend = horizonBlend;
                        m_selectedProceduralSkyboxProfile.horizonSize = horizonScale;
                        m_selectedProceduralSkyboxProfile.followPlayer = followPlayer;
                        m_selectedProceduralSkyboxProfile.horizonUpdateTime = horizonUpdateTime;
                        m_selectedProceduralSkyboxProfile.horizonPosition = horizonPosition;

                        //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                        //Fog Mode
                        fogColorMode = m_selectedProceduralSkyboxProfile.fogColorMode;

                        //Volumetric Fog
                        m_selectedProceduralSkyboxProfile.volumetricEnableDistanceFog = useDistanceFog;
                        m_selectedProceduralSkyboxProfile.volumetricBaseFogDistance = baseFogDistance;
                        m_selectedProceduralSkyboxProfile.volumetricBaseFogHeight = baseFogHeight;
                        m_selectedProceduralSkyboxProfile.volumetricMeanHeight = meanFogHeight;
                        m_selectedProceduralSkyboxProfile.volumetricGlobalAnisotropy = globalAnisotropy;
                        m_selectedProceduralSkyboxProfile.volumetricGlobalLightProbeDimmer = globalLightProbeDimmer;

                        //Exponential Fog
                        m_selectedProceduralSkyboxProfile.exponentialFogDensity = exponentialFogDensity;
                        m_selectedProceduralSkyboxProfile.exponentialBaseHeight = exponentialBaseFogHeight;
                        m_selectedProceduralSkyboxProfile.exponentialHeightAttenuation = exponentialHeightAttenuation;
                        m_selectedProceduralSkyboxProfile.exponentialMaxFogDistance = exponentialMaxFogDistance;
                        m_selectedProceduralSkyboxProfile.exponentialMipFogNear = exponentialMipFogNear;
                        m_selectedProceduralSkyboxProfile.exponentialMipFogFar = exponentialMipFogFar;
                        m_selectedProceduralSkyboxProfile.exponentialMipFogMaxMip = exponentialMipFogMax;

                        //Linear Fog
                        m_selectedProceduralSkyboxProfile.linearFogDensity = linearFogDensity;
                        m_selectedProceduralSkyboxProfile.linearHeightStart = linearFogHeightStart;
                        m_selectedProceduralSkyboxProfile.linearHeightEnd = linearFogHeightEnd;
                        m_selectedProceduralSkyboxProfile.linearMaxFogDistance = linearFogMaxDistance;
                        m_selectedProceduralSkyboxProfile.linearMipFogNear = linearMipFogNear;
                        m_selectedProceduralSkyboxProfile.linearMipFogFar = linearMipFogFar;
                        m_selectedProceduralSkyboxProfile.linearMipFogMaxMip = linearMipFogMax;

                        //Volumetric Light Controller
                        m_selectedProceduralSkyboxProfile.volumetricDistanceRange = depthExtent;
                        m_selectedProceduralSkyboxProfile.volumetricSliceDistributionUniformity = sliceDistribution;

                        //Density Fog Volume
                        m_selectedProceduralSkyboxProfile.useFogDensityVolume = useDensityFogVolume;
                        m_selectedProceduralSkyboxProfile.singleScatteringAlbedo = singleScatteringAlbedo;
                        m_selectedProceduralSkyboxProfile.densityVolumeFogDistance = densityVolumeFogDistance;
                        m_selectedProceduralSkyboxProfile.fogDensityMaskTexture = fogDensityMaskTexture;
                        m_selectedProceduralSkyboxProfile.densityMaskTiling = densityMaskTiling;
                        m_selectedProceduralSkyboxProfile.densityScale = densityScale;

                        //HD Shadows
                        m_selectedProceduralSkyboxProfile.shadowQuality = hDShadowQuality;
                        m_selectedProceduralSkyboxProfile.cascadeSplit1 = split1;
                        m_selectedProceduralSkyboxProfile.cascadeSplit2 = split2;
                        m_selectedProceduralSkyboxProfile.cascadeSplit3 = split3;

                        //Contact Shadows
                        m_selectedProceduralSkyboxProfile.useContactShadows = enableContactShadows;
                        m_selectedProceduralSkyboxProfile.contactShadowsLength = contactLength;
                        m_selectedProceduralSkyboxProfile.contactShadowsDistanceScaleFactor = contactScaleFactor;
                        m_selectedProceduralSkyboxProfile.contactShadowsMaxDistance = contactMaxDistance;
                        m_selectedProceduralSkyboxProfile.contactShadowsFadeDistance = contactFadeDistance;
                        m_selectedProceduralSkyboxProfile.contactShadowsSampleCount = contactSampleCount;
                        m_selectedProceduralSkyboxProfile.contactShadowsOpacity = contactOpacity;

                        //Micro Shadows
                        m_selectedProceduralSkyboxProfile.useMicroShadowing = enableMicroShadows;
                        m_selectedProceduralSkyboxProfile.microShadowOpacity = microShadowOpacity;

                        //SS Reflection
                        m_selectedProceduralSkyboxProfile.enableScreenSpaceReflections = enableSSReflection;
                        m_selectedProceduralSkyboxProfile.screenEdgeFadeDistance = ssrEdgeFade;
                        m_selectedProceduralSkyboxProfile.maxNumberOfRaySteps = ssrNumberOfRays;
                        m_selectedProceduralSkyboxProfile.objectThickness = ssrObjectThickness;
                        m_selectedProceduralSkyboxProfile.minSmoothness = ssrMinSmoothness;
                        m_selectedProceduralSkyboxProfile.smoothnessFadeStart = ssrSmoothnessFade;
                        m_selectedProceduralSkyboxProfile.reflectSky = ssrReflectSky;

                        //SS Refract
                        m_selectedProceduralSkyboxProfile.enableScreenSpaceRefractions = enableSSRefraction;
                        m_selectedProceduralSkyboxProfile.screenWeightDistance = ssrWeightDistance;

                        //Ambient Lighting
                        m_selectedProceduralSkyboxProfile.useBakingSky = useStaticLighting;
                        m_selectedProceduralSkyboxProfile.indirectDiffuseIntensity = diffuseAmbientIntensity;
                        m_selectedProceduralSkyboxProfile.indirectSpecularIntensity = specularAmbientIntensity;
#endif
                    }
                    else
                    {
                        //Fog mode
                        m_selectedGradientSkyboxProfile.fogType = fogType;

                        //Ambient mode
                        m_selectedGradientSkyboxProfile.ambientMode = ambientMode;

                        //Edit mode
                        if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                        {
#if UNITY_POST_PROCESSING_STACK_V2
                            m_selectedGradientSkyboxProfile.postProcessingAssetName = postProcessingAssetName;
#endif
                        }

                        //Skybox
                        m_selectedGradientSkyboxProfile.skyboxRotation = skyboxRotation;
                        m_selectedGradientSkyboxProfile.skyboxPitch = skyboxPitch;

                        //Fog settings
                        m_selectedGradientSkyboxProfile.fogColor = fogColor;
                        m_selectedGradientSkyboxProfile.fogDistance = fogDistance;
                        m_selectedGradientSkyboxProfile.nearFogDistance = nearFogDistance;
                        m_selectedGradientSkyboxProfile.fogDensity = fogDensity;

                        //Ambient Settings
                        m_selectedGradientSkyboxProfile.skyColor = skyColor;
                        m_selectedGradientSkyboxProfile.equatorColor = equatorColor;
                        m_selectedGradientSkyboxProfile.groundColor = groundColor;
                        m_selectedGradientSkyboxProfile.skyboxGroundIntensity = skyboxGroundIntensity;

                        //Sun Settings
                        m_selectedGradientSkyboxProfile.shadowStrength = shadowStrength;
                        m_selectedGradientSkyboxProfile.indirectLightMultiplier = indirectLightMultiplier;
                        m_selectedGradientSkyboxProfile.sunColor = sunColor;
                        m_selectedGradientSkyboxProfile.sunIntensity = sunIntensity;

                        //Shadow Settings
                        m_selectedGradientSkyboxProfile.shadowDistance = shadowDistance;
                        m_selectedGradientSkyboxProfile.shadowmaskMode = shadowmaskMode;
                        m_selectedGradientSkyboxProfile.shadowType = shadowType;
                        m_selectedGradientSkyboxProfile.shadowResolution = shadowResolution;
                        m_selectedGradientSkyboxProfile.shadowProjection = shadowProjection;
                        m_selectedGradientSkyboxProfile.cascadeCount = shadowCascade;

                        //Horizon Settings
                        m_selectedGradientSkyboxProfile.scaleHorizonObjectWithFog = scaleHorizonObjectWithFog;
                        m_selectedGradientSkyboxProfile.horizonSkyEnabled = horizonEnabled;
                        m_selectedGradientSkyboxProfile.horizonScattering = horizonScattering;
                        m_selectedGradientSkyboxProfile.horizonFogDensity = horizonFogDensity;
                        m_selectedGradientSkyboxProfile.horizonFalloff = horizonFalloff;
                        m_selectedGradientSkyboxProfile.horizonBlend = horizonBlend;
                        m_selectedGradientSkyboxProfile.horizonSize = horizonScale;
                        m_selectedGradientSkyboxProfile.followPlayer = followPlayer;
                        m_selectedGradientSkyboxProfile.horizonUpdateTime = horizonUpdateTime;
                        m_selectedGradientSkyboxProfile.horizonPosition = horizonPosition;
                        m_selectedGradientSkyboxProfile.enableSunDisk = enableSunDisk;

                        //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                        //Gradient Sky
                        m_selectedGradientSkyboxProfile.topColor = topSkyColor;
                        m_selectedGradientSkyboxProfile.middleColor = middleSkyColor;
                        m_selectedGradientSkyboxProfile.bottomColor = bottomSkyColor;
                        m_selectedGradientSkyboxProfile.gradientDiffusion = gradientDiffusion;
                        m_selectedGradientSkyboxProfile.hDRIExposure = skyboxExposure;
                        m_selectedGradientSkyboxProfile.skyMultiplier = skyMultiplier;

                        //Fog Mode
                        fogColorMode = m_selectedSkyboxProfile.fogColorMode;

                        //Volumetric Fog
                        m_selectedGradientSkyboxProfile.volumetricEnableDistanceFog = useDistanceFog;
                        m_selectedGradientSkyboxProfile.volumetricBaseFogDistance = baseFogDistance;
                        m_selectedGradientSkyboxProfile.volumetricBaseFogHeight = baseFogHeight;
                        m_selectedGradientSkyboxProfile.volumetricMeanHeight = meanFogHeight;
                        m_selectedGradientSkyboxProfile.volumetricGlobalAnisotropy = globalAnisotropy;
                        m_selectedGradientSkyboxProfile.volumetricGlobalLightProbeDimmer = globalLightProbeDimmer;

                        //Exponential Fog
                        m_selectedGradientSkyboxProfile.exponentialFogDensity = exponentialFogDensity;
                        m_selectedGradientSkyboxProfile.exponentialBaseHeight = exponentialBaseFogHeight;
                        m_selectedGradientSkyboxProfile.exponentialHeightAttenuation = exponentialHeightAttenuation;
                        m_selectedGradientSkyboxProfile.exponentialMaxFogDistance = exponentialMaxFogDistance;
                        m_selectedGradientSkyboxProfile.exponentialMipFogNear = exponentialMipFogNear;
                        m_selectedGradientSkyboxProfile.exponentialMipFogFar = exponentialMipFogFar;
                        m_selectedGradientSkyboxProfile.exponentialMipFogMaxMip = exponentialMipFogMax;

                        //Linear Fog
                        m_selectedGradientSkyboxProfile.linearFogDensity = linearFogDensity;
                        m_selectedGradientSkyboxProfile.linearHeightStart = linearFogHeightStart;
                        m_selectedGradientSkyboxProfile.linearHeightEnd = linearFogHeightEnd;
                        m_selectedGradientSkyboxProfile.linearMaxFogDistance = linearFogMaxDistance;
                        m_selectedGradientSkyboxProfile.linearMipFogNear = linearMipFogNear;
                        m_selectedGradientSkyboxProfile.linearMipFogFar = linearMipFogFar;
                        m_selectedGradientSkyboxProfile.linearMipFogMaxMip = linearMipFogMax;

                        //Volumetric Light Controller
                        m_selectedGradientSkyboxProfile.volumetricDistanceRange = depthExtent;
                        m_selectedGradientSkyboxProfile.volumetricSliceDistributionUniformity = sliceDistribution;

                        //Density Fog Volume
                        m_selectedGradientSkyboxProfile.useFogDensityVolume = useDensityFogVolume;
                        m_selectedGradientSkyboxProfile.singleScatteringAlbedo = singleScatteringAlbedo;
                        m_selectedGradientSkyboxProfile.densityVolumeFogDistance = densityVolumeFogDistance;
                        m_selectedGradientSkyboxProfile.fogDensityMaskTexture = fogDensityMaskTexture;
                        m_selectedGradientSkyboxProfile.densityMaskTiling = densityMaskTiling;
                        m_selectedGradientSkyboxProfile.densityScale = densityScale;

                        //HD Shadows
                        m_selectedGradientSkyboxProfile.shadowQuality = hDShadowQuality;
                        m_selectedGradientSkyboxProfile.cascadeSplit1 = split1;
                        m_selectedGradientSkyboxProfile.cascadeSplit2 = split2;
                        m_selectedGradientSkyboxProfile.cascadeSplit3 = split3;

                        //Contact Shadows
                        m_selectedGradientSkyboxProfile.useContactShadows = enableContactShadows;
                        m_selectedGradientSkyboxProfile.contactShadowsLength = contactLength;
                        m_selectedGradientSkyboxProfile.contactShadowsDistanceScaleFactor = contactScaleFactor;
                        m_selectedGradientSkyboxProfile.contactShadowsMaxDistance = contactMaxDistance;
                        m_selectedGradientSkyboxProfile.contactShadowsFadeDistance = contactFadeDistance;
                        m_selectedGradientSkyboxProfile.contactShadowsSampleCount = contactSampleCount;
                        m_selectedGradientSkyboxProfile.contactShadowsOpacity = contactOpacity;

                        //Micro Shadows
                        m_selectedGradientSkyboxProfile.useMicroShadowing = enableMicroShadows;
                        m_selectedGradientSkyboxProfile.microShadowOpacity = microShadowOpacity;

                        //SS Reflection
                        m_selectedGradientSkyboxProfile.enableScreenSpaceReflections = enableSSReflection;
                        m_selectedGradientSkyboxProfile.screenEdgeFadeDistance = ssrEdgeFade;
                        m_selectedGradientSkyboxProfile.maxNumberOfRaySteps = ssrNumberOfRays;
                        m_selectedGradientSkyboxProfile.objectThickness = ssrObjectThickness;
                        m_selectedGradientSkyboxProfile.minSmoothness = ssrMinSmoothness;
                        m_selectedGradientSkyboxProfile.smoothnessFadeStart = ssrSmoothnessFade;
                        m_selectedGradientSkyboxProfile.reflectSky = ssrReflectSky;

                        //SS Refract
                        m_selectedGradientSkyboxProfile.enableScreenSpaceRefractions = enableSSRefraction;
                        m_selectedGradientSkyboxProfile.screenWeightDistance = ssrWeightDistance;

                        //Ambient Lighting
                        m_selectedGradientSkyboxProfile.useBakingSky = useStaticLighting;
                        m_selectedGradientSkyboxProfile.indirectDiffuseIntensity = diffuseAmbientIntensity;
                        m_selectedGradientSkyboxProfile.indirectSpecularIntensity = specularAmbientIntensity;
#endif
                    }
                    */

                    //Time of day
                    #region TOD

                    m_profiles.m_pauseTimeKey = pauseTimeKey;
                    m_profiles.m_incrementUpKey = incrementUpKey;
                    m_profiles.m_incrementDownKey = incrementDownKey;
                    m_profiles.m_timeToAddOrRemove = timeToAddOrRemove;
                    m_profiles.m_rotateSunLeftKey = rotateSunLeftKey;
                    m_profiles.m_rotateSunRightKey = rotateSunRightKey;
                    m_profiles.m_sunRotationAmount = sunRotationAmount;

                    m_profiles.m_daySunIntensity = daySunIntensity;
                    m_profiles.m_daySunGradientColor = daySunGradientColor;
                    m_profiles.m_nightSunIntensity = nightSunIntensity;
                    m_profiles.m_nightSunGradientColor = nightSunGradientColor;

                    m_profiles.m_lightAnisotropy = lightAnisotropy;
                    m_profiles.m_lightProbeDimmer = lightProbeDimmer;
                    m_profiles.m_lightDepthExtent = lightDepthExtent;
                    m_profiles.m_sunSize = sunSizeAmount;
                    m_profiles.m_skyExposure = skyExposureAmount;

                    m_profiles.m_startFogDistance = startFogDistance;
                    m_profiles.m_dayFogDensity = dayFogDensity;
                    m_profiles.m_nightFogDensity = nightFogDensity;
                    m_profiles.m_dayFogDistance = dayFogDistance;
                    m_profiles.m_dayFogColor = dayFogColor;
                    m_profiles.m_nightFogDistance = nightFogDistance;
                    m_profiles.m_nightFogColor = nightFogColor;

                    m_profiles.m_syncPostProcessing = syncPostProcessing;
                    m_profiles.m_dayTempature = dayTempature;
                    m_profiles.m_dayPostFXColor = dayPostFXColor;
                    m_profiles.m_nightTempature = nightTempature;
                    m_profiles.m_nightPostFXColor = nightPostFXColor;

                    m_profiles.m_realtimeGIUpdate = realtimeGIUpdate;
                    m_profiles.m_gIUpdateInterval = gIUpdateInterval;

                    m_profiles.m_currentSeason = currentSeason;
                    m_profiles.m_hemisphereOrigin = hemisphereOrigin;

                    m_profiles.m_skyboxRotation = timeOfDaySkyboxRotation;
                    m_profiles.m_pBSSpaceEmissionTexture = pBSSpaceEmissionTexture;
                    m_profiles.m_pBSSpaceEmissionMultiplier = pBSTODSpaceEmissionMultiplier;
                    m_selectedProceduralSkyboxProfile.enableSunDisk = enableSunDisk;

                    m_profiles.m_pauseTime = pauseTime;
                    m_profiles.m_currentTimeOfDay = currentTimeOfDay;
                    m_profiles.m_nightLengthInSeconds = nightLengthInSeconds;
                    m_profiles.m_dayLengthInSeconds = dayLengthInSeconds;
                    m_profiles.m_dayDate = dayDate;
                    m_profiles.m_monthDate = monthDate;
                    m_profiles.m_yearDate = yearDate;

                    #endregion

                    #endregion

                    #endregion

                    #region Update Settings

                    //Update skies
                    if (systemtype != AmbientSkiesConsts.SystemTypes.DefaultProcedural || systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                        {
                            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                            {
                                horizonEnabled = false;
                                m_selectedSkyboxProfile.horizonSkyEnabled = horizonEnabled;
                            }
                            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                            {
                                horizonEnabled = false;
                                m_selectedProceduralSkyboxProfile.horizonSkyEnabled = horizonEnabled;
                            }
                            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                            {
                                horizonEnabled = false;
                                m_selectedGradientSkyboxProfile.horizonSkyEnabled = horizonEnabled;
                            }
                        }

                        AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToFalse(m_profiles);
                    }

                    EditorPrefs.SetString("AmbientSkiesProfile_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex].name);

                    m_hasChanged = true;

                    #endregion
                }

                GUI.enabled = true;
            }
#endif
        }

        /// <summary>
        /// Display the additional lights tab
        /// </summary>
        private void AdditionalLightsTab()
        {
            EditorGUI.BeginChangeCheck();

            //Skybox GUI
            m_editorUtils.Heading("AdditionalLightsHeader");

            //m_foldoutGlobalInteriorLightingSettings = m_editorUtils.Panel("ShowGlobalInteriorLightingSettings", GlobalInteriorLightingSettings, m_foldoutGlobalInteriorLightingSettings);

            if (lightingEditMode == AmbientSkiesConsts.EnvironmentLightingEditMode.Interior)
            {
                m_foldoutInteriorLightingModeSettings = m_editorUtils.Panel("ShowInteriorLightingMode", ShowInteriorLightingMode, m_foldoutInteriorLightingModeSettings);
            }
            else
            {
                m_foldoutInteriorLightingModeSettings = m_editorUtils.Panel("ShowExteriorLightingMode", ShowExteriorLightingMode, m_foldoutInteriorLightingModeSettings);
            }

            //Apply settings
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);
            }
        }

        /// <summary>
        /// Display the post processing tab
        /// </summary>
        private void PostProcessingTab()
        {
            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            if (m_needCompile)
            {
                m_editorUtils.Text("PleaseWait...");
                return;
            }

            EditorGUI.BeginChangeCheck();

            //See if we can select an ambiet skies skybox
            if (m_selectedPostProcessingProfile == null)
            {
                if (m_editorUtils.Button("SelectPostProcessingProfileButton"))
                {
                    EditorUtility.SetDirty(m_creationToolSettings);
                    m_selectedPostProcessingProfileIndex = 0;
                    m_creationToolSettings.m_selectedPostProcessing = 0;

                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                    PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);

                    return;
                }

                return;
            }

            //If profile is there
            if (m_selectedPostProcessingProfile != null)
            {
                LoadPostProcessingSettings();

                m_editorUtils.Heading("PostProcessingSettingsHeader");
                
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    usePostProcess = m_editorUtils.ToggleLeft("UsePostProcess", usePostProcess);
                    EditorGUILayout.Space();
#else
                    m_profiles.m_usePostFX = false;
                    EditorGUILayout.HelpBox("Post Processing V2 is not installed. Please install it from the package manager. Window/Package Manager to use post processing features in ambient skies", MessageType.Info);
#endif
                }
                else
                {
                    usePostProcess = m_editorUtils.ToggleLeft("UsePostProcess", usePostProcess);
                    EditorGUILayout.Space();
                }

                if (usePostProcess)
                {
                    m_editorUtils.Link("LearnMoreAboutPostFX");
                    EditorGUILayout.Space();

                    if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
#if HDPipeline
                        m_foldoutMainPostProcessing = m_editorUtils.Panel("Show Main Settings", HDRPMainPostProcessSettingsEnabled, m_foldoutMainPostProcessing);
#else
                        m_foldoutMainPostProcessing = m_editorUtils.Panel("Show Main Settings", MainPostProcessSettingsEnabled, m_foldoutMainPostProcessing);
#endif
                    }
                    else if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Universal)
                    {
                        m_foldoutMainPostProcessing = m_editorUtils.Panel("Show Main Settings", URPMainPostProcessSettingsEnabled, m_foldoutMainPostProcessing);
                    }
                    else
                    {
                        m_foldoutMainPostProcessing = m_editorUtils.Panel("Show Main Settings", MainPostProcessSettingsEnabled, m_foldoutMainPostProcessing);                     
                    }

                    m_foldoutPostProcessingProfile = m_editorUtils.Panel("Show Profile Settings", PostProcessingProfileSettingsEnabled, m_foldoutPostProcessingProfile);

                    //Save to defaults
                    if (m_profiles.m_editSettings)
                    {
                        if (m_editorUtils.ButtonRight("SavePostProcessingSettingsToDefaultsButton"))
                        {
                            if (EditorUtility.DisplayDialog("WARNING!!", "Are you sure you want to replace the default settings?", "Make My Day!", "Cancel"))
                            {
                                m_profiles.SavePostProcessingSettingsToDefault(m_selectedPostProcessingProfileIndex, m_profiles.m_selectedRenderPipeline);
                            }
                        }
                    }

                    //Revert
                    if (m_editorUtils.ButtonRight("RevertPostProcesssingButton"))
                    {
                        //Update settings to stop them wiping new setttings                   
                        m_profiles.RevertPostProcessingSettingsToDefault(m_selectedPostProcessingProfileIndex, m_profiles.m_selectedRenderPipeline);

                        AmbientSkiesEditorFunctions.SetAllPostFxUpdateToTrue(m_profiles);

                        PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);

                        AmbientSkiesEditorFunctions.SetAllPostFxUpdateToFalse(m_profiles);
                    }
                }

                GUI.enabled = true;
            }

            //Apply settings
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                #region Save Post Processing Changes

                //Apply changes
                m_profiles.m_usePostFX = usePostProcess;

                //Selection changing things - exit immediately to not polute settings
                if (newPPSelection != m_selectedPostProcessingProfileIndex)
                {
                    Undo.RecordObject(m_profiles, "Made changes");
                    EditorUtility.SetDirty(m_profiles);

                    AmbientSkiesEditorFunctions.SetAllPostFxUpdateToTrue(m_profiles);

                    m_selectedPostProcessingProfileIndex = newPPSelection;
                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                    PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);
                }

                #endregion

                //Update post processing
                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);

                //Apply index to profile in creation settings
                if (m_creationToolSettings != null)
                {
                    EditorUtility.SetDirty(m_creationToolSettings);
                    m_creationToolSettings.m_selectedHDRI = m_selectedSkyboxProfileIndex;
                    m_creationToolSettings.m_selectedProcedural = m_selectedProceduralSkyboxProfileIndex;
                    m_creationToolSettings.m_selectedGradient = m_selectedGradientSkyboxProfileIndex;
                    m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                    m_creationToolSettings.m_selectedLighting = m_selectedLightingProfileIndex;
                }

                EditorPrefs.SetString("AmbientSkiesPostProcessing_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_ppProfiles[newPPSelection].name);

                AmbientSkiesEditorFunctions.SetAllPostFxUpdateToFalse(m_profiles);

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Display the lighting tab
        /// </summary>
        private void LightingTab()
        {
            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            if (m_needCompile)
            {
                m_editorUtils.Text("PleaseWait...");
                return;
            }

            //See if we can select an ambiet skies lightmapping setting
            if (m_selectedLightingProfile == null)
            {
                if (m_editorUtils.Button("SelectLightmappingProfileButton"))
                {
                    m_selectedLightingProfileIndex = 0;
                    m_selectedLightingProfile = m_profiles.m_lightingProfiles[m_selectedLightingProfileIndex];
                    //Udpate lightmap settings                   
                    LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);
                    return;
                }
            }

            //If profile is there
            if (m_selectedLightingProfile != null)
            {
                EditorGUI.BeginChangeCheck();

                m_editorUtils.Heading("LightmappingSettings");

                #region Lightmaps Variables
                //Global Settings
                systemtype = m_profiles.m_systemTypes;

                //Target Platform
                targetPlatform = m_profiles.m_targetPlatform;

                //Local values
                newLightmappingSettings = m_selectedLightingProfile.profileIndex;
                autoLightmapGeneration = m_selectedLightingProfile.autoLightmapGeneration;

                enableLightmapSettings = m_profiles.m_useLighting;

                //Creation mode
                m_lightProfileName = m_selectedLightingProfile.name;

                #endregion

                enableLightmapSettings = m_editorUtils.ToggleLeft("UseLightmaps", enableLightmapSettings);
                EditorGUILayout.Space();

                if (enableLightmapSettings)
                {
                    m_editorUtils.Link("LearnMoreAboutLightmaps");
                    EditorGUILayout.Space();

                    m_foldoutMainLighting = m_editorUtils.Panel("Show Main Settings", MainLightingSettingsEnabled, m_foldoutMainLighting);
                    m_foldoutLightmapConfiguration = m_editorUtils.Panel("Show Lightmap Configuration", LightmapConfigurationEnabled, m_foldoutLightmapConfiguration);                 

                    if (m_profiles.m_editSettings)
                    {
                        if (m_editorUtils.ButtonRight("SaveLightingToDefaults"))
                        {
                            if (EditorUtility.DisplayDialog("WARNING!!", "Are you sure you want to replace the default settings?", "Make My Day!", "Cancel"))
                            {
                                m_profiles.SaveLightingSettingsToDefault(m_selectedLightingProfileIndex);
                            }
                        }
                    }

                    //Revert
                    if (m_editorUtils.ButtonRight("RevertPostProcesssingButton"))
                    {
                        m_profiles.RevertLightingSettingsToDefault(m_selectedLightingProfileIndex);
                    }
                }

                //Apply settings
                if (EditorGUI.EndChangeCheck())
                {                   
                    Undo.RecordObject(m_profiles, "Made changes");
                    EditorUtility.SetDirty(m_profiles);

                    //Apply changes
                    m_profiles.m_useLighting = enableLightmapSettings;

                    LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);
                    AmbientSkiesEditorFunctions.SetAllLightingUpdateToFalse(m_profiles);

                    //Apply index to profile in creation settings
                    if (m_creationToolSettings != null)
                    {
                        EditorUtility.SetDirty(m_creationToolSettings);
                        m_creationToolSettings.m_selectedHDRI = m_selectedSkyboxProfileIndex;
                        m_creationToolSettings.m_selectedProcedural = m_selectedProceduralSkyboxProfileIndex;
                        m_creationToolSettings.m_selectedGradient = m_selectedGradientSkyboxProfileIndex;
                        m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                        m_creationToolSettings.m_selectedLighting = m_selectedLightingProfileIndex;
                    }

                    EditorPrefs.SetString("AmbientSkiesLighting_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_lightingProfiles[newLightmappingSettings].name);

                    m_hasChanged = true;
                }
            }

            GUI.enabled = true;
        }

        /// <summary>
        /// Display the info tab
        /// </summary>
        private void InfoTab()
        {
            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            if (m_needCompile)
            {
                m_editorUtils.Text("PleaseWait...");
                return;
            }

            EditorGUILayout.BeginHorizontal();
            m_editorUtils.Text("RenderPipeline", GUILayout.Width(155f));
            m_editorUtils.TextNonLocalized(renderPipelineSettings.ToString());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            m_editorUtils.Heading("InfoHeading");

            switch (renderPipelineSettings)
            {
                case AmbientSkiesConsts.RenderPipelineSettings.BuiltIn:
                    m_foldoutBuiltIn = m_editorUtils.Panel("BuiltInInfoAndTips", BuiltInInfoSettings, m_foldoutBuiltIn);
                    break;
                case AmbientSkiesConsts.RenderPipelineSettings.Universal:
                    m_foldoutLightweight = m_editorUtils.Panel("LightweightInfoAndTips", LightweightInfoSettings, m_foldoutLightweight);
                    break;
                default:
                    m_foldoutHighDefinition = m_editorUtils.Panel("HighDefinitionInfoAndTips", HighDefinitionInfoSettings, m_foldoutHighDefinition);
                    break;
            }
            // EditorGUILayout.Space();
            //
            // m_editorUtils.Heading("CreationMode");
            // m_foldoutCreationMode = m_editorUtils.Panel("ShowCreationMode", CreateModeSettings, m_foldoutCreationMode);
            // EditorGUILayout.Space();

            GUI.enabled = true;
        }

        #endregion

        #region Editor Update

        /// <summary>
        /// Editor Update
        /// </summary>
        private void EditorUpdate()
        {
            //Check starts as true before searching.
            //This will loop the update till checks are all true
            bool checkSuccess = true;

            //Countdown timer
            m_countdown -= Time.deltaTime;
            if (m_profiles.m_showDebug && m_profiles.m_showTimersInDebug)
            {
                Debug.Log("EditorUpdate(): m_countdown = " + m_countdown);
            }

            //If timer still active
            if (m_countdown > 0)
            {
                //Update system still enabled
                m_inspectorUpdate = true;
                checkSuccess = false;
            }
            //If timer inactive
            else
            {
                //Disable update system
                checkSuccess = true;
                m_countdown = -5f;
            }

            if (m_extraCheck)
            {               
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    if (m_needCompile)
                    {
                        return;
                    }

                    //Checks to make sure the hd volume is there
                    if (GameObject.Find("High Definition Environment Volume") == null)
                    {
                        //Update skybox
                        if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
                        }
                        //Failed on check setting to false
                        checkSuccess = false;
                    }
                }
                else
                {
                    //Makes sure the global post processing is there
                    if (GameObject.Find("Global Post Processing") == null)
                    {
                        //Update post processing
                        PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);

                        //Failed on check setting to false
                        checkSuccess = false;
                    }

                    //Checks to see if using time of day
                    if (m_profiles.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable && renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
                        if (RenderSettings.skybox == null)
                        {
                            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Ambient Skies Skybox"));
                        }
                        if (RenderSettings.skybox.shader == Shader.Find("Skybox/Cubemap"))
                        {
                            //Update skybox
                            if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                            {
                                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
                            }
                            //Failed on check setting to false
                            checkSuccess = false;
                        }
                    }
                }

                //Checks to see if using time of day
                if (m_profiles.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
                {
                    //Looks for time of day prefab
                    if (GameObject.Find("AS Time Of Day") == null)
                    {
                        //Update skybox
                        if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
                        }
                        //Failed on check setting to false
                        checkSuccess = false;
                    }

                    if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && RenderSettings.skybox.shader == Shader.Find("Skybox/Cubemap"))
                    {
                        //Update skybox
                        if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
                        }
                        //Failed on check setting to false
                        checkSuccess = false;
                    }
                }
            }
            else
            {
                checkSuccess = true;
            }

            //If all checks are success stop update
            if (checkSuccess)
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("Check successful, exiting editor update");
                }

                m_extraCheck = true;
                //Stops the editor update
                EditorApplication.update -= EditorUpdate;
            }
            else
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("Check failed, unable to exit editor update");
                }
            }
        }

        #endregion

        #region Global Settings

        /// <summary>
        /// Global settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void GlobalSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            Color backgroundButtonColor = GUI.backgroundColor;

            systemtype = m_profiles.m_systemTypes;

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            systemtype = (AmbientSkiesConsts.SystemTypes)m_editorUtils.EnumPopup("SystemTypes", systemtype, helpEnabled);
#if UNITY_2018_3_OR_NEWER
            if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && systemtype == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
            {
                EditorGUILayout.HelpBox("Gradient Sky is only avaliable in High Definition", MessageType.Warning);
            }
#else
            if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && systemtype == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
            {
                EditorGUILayout.HelpBox("Gradient Sky is only avaliable in 2018.3 or higher and High Definition", MessageType.Warning);
            }
#endif
            if (systemtype == AmbientSkiesConsts.SystemTypes.DefaultProcedural)
            {
                //EditorGUILayout.Space();
                useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
                m_editorUtils.Text("DefaultProceduralText");

                EditorGUILayout.HelpBox("Default Procedural sets up a simple procedural sky system. To use Ambient Skies select one of the Ambient Skies (HDRI, Procedural, Gradient) systems in the System Type dropdown.", MessageType.Info);
            }
            if (systemtype == AmbientSkiesConsts.SystemTypes.ThirdParty)
            {
                SkyboxUtils.RemoveTimeOfDay(m_profiles);
            }
            /*
            else if (systemtype == AmbientSkiesConsts.SystemTypes.WorldManagerAPI)
            {
                //EditorGUILayout.Space();
                m_editorUtils.Text("WorldManagerAPIText");
            }
            */
            else if (systemtype == AmbientSkiesConsts.SystemTypes.ThirdParty)
            {
                //EditorGUILayout.Space();
                m_editorUtils.Text("ThirdPartyText");

                EditorGUILayout.HelpBox("To use Ambient Skies select one of the Ambient Skies (HDRI, Procedural, Gradient) systems in the System Type dropdown.", MessageType.Info);
            }
            else
            {
                //Searching for Enviro
                if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
#if AMBIENT_SKIES_ENVIRO
                    if (FindObjectOfType<EnviroSkyMgr>() != null)
                    {
                        if (EditorUtility.DisplayDialog("Enviro Detected!", "Warning Enviro has been detected in your scene, switching System Type from Third Party will remove Enviro from your scene. Are you sure you want to proceed?", "Yes", "No"))
                        {
                            systemtype = AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies;
                            m_profiles.m_systemTypes = AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies;
                        }
                        else
                        {
                            systemtype = AmbientSkiesConsts.SystemTypes.ThirdParty;
                            m_profiles.m_systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;
                            AmbientSkiesEditorFunctions.NewSceneObjectCreation();
                        }
                    }
#endif
                }

                //EditorGUILayout.Space();
                m_editorUtils.Text("AmbientSkiesText");
            }
            EditorGUILayout.Space();

            targetPlatform = (AmbientSkiesConsts.PlatformTarget)m_editorUtils.EnumPopup("TargetPlatform", targetPlatform, helpEnabled);

            //EditorGUILayout.Space();
            m_editorUtils.Text("TargetPlatformText");
            EditorGUILayout.Space();

            vSyncMode = (AmbientSkiesConsts.VSyncMode)m_editorUtils.EnumPopup("VsyncMode", vSyncMode, helpEnabled);

            //EditorGUILayout.Space();
            m_editorUtils.Text("VsyncText");   
            
            if (systemtype != AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                m_profiles.m_useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
                useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
            }

            if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                GUI.backgroundColor = SkyboxUtils.GetColorFromHTML("FFB66D");

                //Get graphic Tiers
                var tier1 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1);
                var tier2 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2);
                var tier3 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3);
                if (mainCam != null)
                {
                    if (mainCam.actualRenderingPath == RenderingPath.DeferredShading)
                    {
                        if (GameObject.Find("Global Post Processing") == null)
                        {
                            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                            {
#if UNITY_POST_PROCESSING_STACK_V2
                                EditorGUILayout.HelpBox("You are using Deferred Rendering, to view fog this requires post processing to render fog in scene and game view. Click 'Enable Post Processing' below to resolve this.", MessageType.Warning);
                                if (m_editorUtils.Button("FixPostProcessing"))
                                {
                                    FixPostProcessingGlobalPanel();
                                }
#else
                                EditorGUILayout.HelpBox("Post Processing V2 is not installed. Please install it from the package manager. Window/Package Manager to use post processing features in ambient skies", MessageType.Info);
#endif
                            }
                            else
                            {
                                if (!usePostProcess)
                                {
                                    if (m_editorUtils.Button("FixPostProcessing"))
                                    {
                                        FixPostProcessingGlobalPanel();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (tier1.renderingPath == RenderingPath.DeferredShading)
                    {
                        if (GameObject.Find("Global Post Processing") == null)
                        {
                            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                            {
#if UNITY_POST_PROCESSING_STACK_V2
                                EditorGUILayout.HelpBox("You are using Deferred Rendering, to view fog this requires post processing to render fog in scene and game view. Click 'Enable Post Processing' below to resolve this.", MessageType.Warning);
                                if (m_editorUtils.Button("FixPostProcessing"))
                                {
                                    FixPostProcessingGlobalPanel();
                                }
#else
                                EditorGUILayout.HelpBox("Post Processing V2 is not installed. Please install it from the package manager. Window/Package Manager to use post processing features in ambient skies", MessageType.Info);
#endif
                            }
                            else
                            {
                                if (!usePostProcess)
                                {
                                    if (m_editorUtils.Button("FixPostProcessing"))
                                    {
                                        FixPostProcessingGlobalPanel();
                                    }
                                }
                            }
                        }
                    }
                    else if (tier2.renderingPath == RenderingPath.DeferredShading)
                    {
                        if (GameObject.Find("Global Post Processing") == null)
                        {
                            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                            {
#if UNITY_POST_PROCESSING_STACK_V2
                                EditorGUILayout.HelpBox("You are using Deferred Rendering, to view fog this requires post processing to render fog in scene and game view. Click 'Enable Post Processing' below to resolve this.", MessageType.Warning);
                                if (m_editorUtils.Button("FixPostProcessing"))
                                {
                                    FixPostProcessingGlobalPanel();
                                }
#else
                                EditorGUILayout.HelpBox("Post Processing V2 is not installed. Please install it from the package manager. Window/Package Manager to use post processing features in ambient skies", MessageType.Info);
#endif
                            }
                            else
                            {
                                if (!usePostProcess)
                                {
                                    if (m_editorUtils.Button("FixPostProcessing"))
                                    {
                                        FixPostProcessingGlobalPanel();
                                    }
                                }
                            }
                        }
                    }
                    else if (tier3.renderingPath == RenderingPath.DeferredShading)
                    {
                        if (GameObject.Find("Global Post Processing") == null)
                        {
                            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                            {
#if UNITY_POST_PROCESSING_STACK_V2
                                EditorGUILayout.HelpBox("You are using Deferred Rendering, to view fog this requires post processing to render fog in scene and game view. Click 'Enable Post Processing' below to resolve this.", MessageType.Warning);
                                if (m_editorUtils.Button("FixPostProcessing"))
                                {
                                    FixPostProcessingGlobalPanel();
                                }
#else
                                EditorGUILayout.HelpBox("Post Processing V2 is not installed. Please install it from the package manager. Window/Package Manager to use post processing features in ambient skies", MessageType.Info);
#endif
                            }
                            else
                            {
                                if (!usePostProcess)
                                {
                                    if (m_editorUtils.Button("FixPostProcessing"))
                                    {
                                        FixPostProcessingGlobalPanel();
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (targetPlatform == AmbientSkiesConsts.PlatformTarget.DesktopAndConsole)
                {
                    if (PlayerSettings.colorSpace != ColorSpace.Linear)
                    {
                        EditorGUILayout.HelpBox("Ambient Skies works best in Linear color space and Deferred rendering. Press 'Set Linear Deferred Lighting' to resolve this.", MessageType.Warning);

                        if (m_editorUtils.Button("SetLinearDefferedButton"))
                        {
                            if (EditorUtility.DisplayDialog("Alert!!", "Warning you're about to set Linear Color Space and Deferred Rendering Path. If your Color Space is not already Linear this will require a reimport, this will also close the Ambient Skies window. Are you sure you want to proceed?", "Yes", "No"))
                            {
                                LightingUtils.SetLinearDeferredLighting(this);
                            }
                        }
                    }
                }

                GUI.backgroundColor = backgroundButtonColor;
            }
            else
            {
                if (mainCam != null)
                {
#if HDPipeline
                    GUI.backgroundColor = SkyboxUtils.GetColorFromHTML("FFB66D");
                    if (!usePostProcess)
                    {
                        EditorGUILayout.HelpBox("Post processing is disabled. Improve the visual quality in High Definition Render Pipeline with Post Processing. Click 'Enable Post Processing' to add post fx to your scene.", MessageType.Info);

                        if (m_editorUtils.Button("FixPostProcessing"))
                        {
                            FixPostProcessingGlobalPanel();
                        }
                    }

                    if (cameraData == null)
                    {
                        cameraData = mainCam.GetComponent<HDAdditionalCameraData>();
                    }
                    else
                    {
#if UNITY_2019_1_OR_NEWER
                        if (cameraData.fullscreenPassthrough)
                        {
                            EditorGUILayout.HelpBox("Warning! Fullscreen Passthrough is enabled on the camera. This will render the Game View black. This setting is recommended when recording videos. To disable this press 'Diable Fullscreen Passthrough' to resolve this.", MessageType.Warning);

                            if (m_editorUtils.Button("Disable Fullscreen Passthrough"))
                            {
                                if (cameraData != null)
                                {
                                    cameraData.fullscreenPassthrough = false;
                                }
                            }
                        }
#endif
                    }
                    GUI.backgroundColor = backgroundButtonColor;
#endif
                }
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                //Update profiles
                if (systemtype != m_profiles.m_systemTypes)
                {
                    m_profiles.m_updateVisualEnvironment = true;

                    m_selectedProceduralSkyboxProfile = m_profiles.m_proceduralSkyProfiles[m_creationToolSettings.m_selectedProcedural];
                    m_selectedGradientSkyboxProfile = m_profiles.m_gradientSkyProfiles[m_creationToolSettings.m_selectedGradient];
                    m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_creationToolSettings.m_selectedHDRI];

                    m_profiles.m_systemTypes = systemtype;

                    autoPostProcessingApplyTimer = 2f;
                    m_inspectorUpdate = true;

                    LoadAllVariables();

                    AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(m_profiles);

                    AmbientSkiesEditorFunctions.SetAllPostFxUpdateToTrue(m_profiles);                    
                }

                if (targetPlatform != m_profiles.m_targetPlatform)
                {
                    m_profiles.m_updateTargetPlaform = true;

                    //Update post processing
                    PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);

                    //Target Platform
                    m_profiles.m_targetPlatform = targetPlatform;
                }
                //Global Settings
                m_profiles.m_systemTypes = systemtype;

                //VSync Settings
                m_profiles.m_vSyncMode = vSyncMode;

                //Time of day
                m_profiles.m_useTimeOfDay = useTimeOfDay;

                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                AmbientSkiesEditorFunctions.SetAllPostFxUpdateToFalse(m_profiles);

                AmbientSkiesEditorFunctions.SetAllPostFxUpdateToFalse(m_profiles);
            }
        }

        #endregion

        #region Time Of Day Settings

        /// <summary>
        /// Time of day settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TimeOfDaySettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            useTimeOfDay = (AmbientSkiesConsts.DisableAndEnable)m_editorUtils.EnumPopup("UseTimeOfDay", useTimeOfDay, helpEnabled);
            if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
            {
                currentTimeOfDay = m_profiles.m_currentTimeOfDay;

                EditorGUILayout.Space();
                m_editorUtils.Heading("Profile Settings");

                EditorGUILayout.BeginHorizontal();
                timeOfDayProfile = (AmbientSkiesTimeOfDayProfile)m_editorUtils.ObjectField("TimeOfDayProfile", timeOfDayProfile, typeof(AmbientSkiesTimeOfDayProfile), false, helpEnabled, GUILayout.Height(16f), GUILayout.MaxWidth(2000f));
                if (m_editorUtils.ButtonRight("CreateTimeOfDayProfile", GUILayout.Width(45f), GUILayout.Height(16f)))
                {
                    timeOfDayProfile = AmbientSkiesEditorFunctions.CreateNewTimeOfDayProfile(timeOfDayProfile, m_profiles);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                m_editorUtils.InlineHelp("TimeOfDayProfile", helpEnabled);

                if (fogType != AmbientSkiesConsts.VolumeFogType.None)
                {
                    m_foldoutTODFogSettings = m_editorUtils.Panel("FogSettings", TODFogSettings, m_foldoutTODFogSettings);
                }          
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    m_foldoutTODHDRPSettings = m_editorUtils.Panel("HDRP Settings", TODHDRPSettings, m_foldoutTODHDRPSettings);
                }
                m_foldoutTODKeyBindings = m_editorUtils.Panel("Key Binding Settings", TODKeyBinding, m_foldoutTODKeyBindings);
                m_foldoutTODLightSettings = m_editorUtils.Panel("Light Settings", TODLightSettings, m_foldoutTODLightSettings);
                if (usePostProcess)
                {
                    m_foldoutTODPostProcessing = m_editorUtils.Panel("Post Processing Settings", TODPostProcessing, m_foldoutTODPostProcessing);
                }
                m_foldoutTODRealtimeGI = m_editorUtils.Panel("Global GI Settings", TODRealtimeGI, m_foldoutTODRealtimeGI);
                m_foldoutTODSkybox = m_editorUtils.Panel("Sky Settings", TODSkybox, m_foldoutTODSkybox);
                m_foldoutTODSeasons = m_editorUtils.Panel("Season Settings", TODSeasons, m_foldoutTODSeasons);
                m_foldoutTODTime = m_editorUtils.Panel("Time Of Day Settings", TODTime, m_foldoutTODTime);
                if (m_profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    m_foldoutHDShadowSettings = m_editorUtils.Panel("Show HD Shadow Settings", HDShadowSettingsEnabled, m_foldoutHDShadowSettings);
                    m_foldoutScreenSpaceReflectionSettings = m_editorUtils.Panel("Show Screen Space Reflection Settings", ScreenSpaceReflectionSettingsEnabled, m_foldoutScreenSpaceReflectionSettings);
                    m_foldoutScreenSpaceRefractionSettings = m_editorUtils.Panel("Show Screen Space Refraction Settings", ScreenSpaceRefractionSettingsEnabled, m_foldoutScreenSpaceRefractionSettings);
                }
                else
                {
                    m_foldoutShadowSettings = m_editorUtils.Panel("Show Shadow Settings", ShadowSettingsEnabled, m_foldoutShadowSettings);
                }

                m_profiles.m_currentTimeOfDay = currentTimeOfDay;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);
                if (timeOfDayProfile != null)
                {
                    EditorUtility.SetDirty(timeOfDayProfile);
                }

                m_profiles.m_useTimeOfDay = useTimeOfDay;
                m_profiles.m_timeOfDayProfile = timeOfDayProfile;

                AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(m_profiles);

                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToFalse(m_profiles);

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Time of day key bindings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODKeyBinding(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            pauseTimeKey = (KeyCode)m_editorUtils.EnumPopup("PauseTimeKey", pauseTimeKey, helpEnabled);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            incrementUpKey = (KeyCode)m_editorUtils.EnumPopup("IncrementUpKey", incrementUpKey, helpEnabled);
            incrementDownKey = (KeyCode)m_editorUtils.EnumPopup("IncrementDownKey", incrementDownKey, helpEnabled);
            timeToAddOrRemove = m_editorUtils.Slider("TimeToAddOrRemove", timeToAddOrRemove, 0.001f, 0.99f, helpEnabled);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            rotateSunLeftKey = (KeyCode)m_editorUtils.EnumPopup("RotateSunLeftKey", rotateSunLeftKey, helpEnabled);
            rotateSunRightKey = (KeyCode)m_editorUtils.EnumPopup("RotateSunRightKey", rotateSunRightKey, helpEnabled);
            sunRotationAmount = m_editorUtils.Slider("SunRotationAmount", sunRotationAmount, 0f, 359f, helpEnabled);
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_pauseTimeKey = pauseTimeKey;
                m_profiles.m_incrementUpKey = incrementUpKey;
                m_profiles.m_incrementDownKey = incrementDownKey;
                m_profiles.m_timeToAddOrRemove = timeToAddOrRemove;
                m_profiles.m_rotateSunLeftKey = rotateSunLeftKey;
                m_profiles.m_rotateSunRightKey = rotateSunRightKey;
                m_profiles.m_sunRotationAmount = sunRotationAmount;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Time of day light settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODLightSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            daySunIntensity = m_editorUtils.CurveField("DaySunIntensity", daySunIntensity, helpEnabled, GUILayout.Height(16f));
            daySunGradientColor = AmbientSkiesEditorFunctions.GradientField(m_editorUtils, "DaySunGradientColor", daySunGradientColor, helpEnabled);
            nightSunIntensity = m_editorUtils.CurveField("NightSunIntensity", nightSunIntensity, helpEnabled, GUILayout.Height(16f));
            nightSunGradientColor = AmbientSkiesEditorFunctions.GradientField(m_editorUtils, "NightSunGradientColor", nightSunGradientColor, helpEnabled);

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_daySunIntensity = daySunIntensity;
                m_profiles.m_daySunGradientColor = daySunGradientColor;
                m_profiles.m_nightSunIntensity = nightSunIntensity;
                m_profiles.m_nightSunGradientColor = nightSunGradientColor;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Time of day HDRP settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODHDRPSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            lightAnisotropy = m_editorUtils.CurveField("LightAnisotropy", lightAnisotropy, helpEnabled, GUILayout.Height(16f));
            lightProbeDimmer = m_editorUtils.CurveField("LightProbeDimmer", lightProbeDimmer, helpEnabled, GUILayout.Height(16f));
            lightDepthExtent = m_editorUtils.CurveField("LightDepthExtent", lightDepthExtent, helpEnabled, GUILayout.Height(16f));
#if !UNITY_2019_3_OR_NEWER
            sunSizeAmount = m_editorUtils.CurveField("SunSize", sunSizeAmount, helpEnabled, GUILayout.Height(16f));
            skyExposureAmount = m_editorUtils.CurveField("SkyExposureAmount", skyExposureAmount, helpEnabled, GUILayout.Height(16f));
#else
            pBSGroundColor = AmbientSkiesEditorFunctions.GradientField(m_editorUtils, "PBSGroundColor", pBSGroundColor, helpEnabled);
            pBSAreosolDensity = m_editorUtils.CurveField("PBSAreosolDensity", pBSAreosolDensity, helpEnabled, GUILayout.Height(16f));
#endif

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_lightAnisotropy = lightAnisotropy;
                m_profiles.m_lightProbeDimmer = lightProbeDimmer;
                m_profiles.m_lightDepthExtent = lightDepthExtent;
                m_profiles.m_sunSize = sunSizeAmount;
                m_profiles.m_skyExposure = skyExposureAmount;
                m_profiles.m_pBSGroundColor = pBSGroundColor;
                m_profiles.m_pBSAreosolDensity = pBSAreosolDensity;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Time of day fog settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODFogSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            if (fogType == AmbientSkiesConsts.VolumeFogType.Linear || fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
            {
                startFogDistance = m_editorUtils.Slider("StartFogDistance", startFogDistance, -1000f, 1000f, helpEnabled);
            }
            else if (fogType == AmbientSkiesConsts.VolumeFogType.Exponential || fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
            {
                dayFogDensity = m_editorUtils.CurveField("DayFogDensity", dayFogDensity, helpEnabled, GUILayout.Height(16f));
                nightFogDensity = m_editorUtils.CurveField("NightFogDensity", nightFogDensity, helpEnabled, GUILayout.Height(16f));
            }
            
            dayFogDistance = m_editorUtils.CurveField("DayFogDistance", dayFogDistance, helpEnabled, GUILayout.Height(16f));
            dayFogColor = AmbientSkiesEditorFunctions.GradientField(m_editorUtils, "DayFogColor", dayFogColor, helpEnabled);
            nightFogDistance = m_editorUtils.CurveField("NightFogDistance", nightFogDistance, helpEnabled, GUILayout.Height(16f));
            nightFogColor = AmbientSkiesEditorFunctions.GradientField(m_editorUtils, "NightFogColor", nightFogColor, helpEnabled);

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_startFogDistance = startFogDistance;
                m_profiles.m_dayFogDensity = dayFogDensity;
                m_profiles.m_nightFogDensity = nightFogDensity;
                m_profiles.m_dayFogDistance = dayFogDistance;
                m_profiles.m_dayFogColor = dayFogColor;
                m_profiles.m_nightFogDistance = nightFogDistance;
                m_profiles.m_nightFogColor = nightFogColor;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Time of day post processing foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODPostProcessing(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            syncPostProcessing = m_editorUtils.Toggle("SyncPostProcessing", syncPostProcessing, helpEnabled);
            dayTempature = m_editorUtils.CurveField("DayTempature", dayTempature, helpEnabled, GUILayout.Height(16f));
            dayPostFXColor = AmbientSkiesEditorFunctions.GradientField(m_editorUtils, "DayPostFXColor", dayPostFXColor, helpEnabled);
            nightTempature = m_editorUtils.CurveField("NightTempature", nightTempature, helpEnabled, GUILayout.Height(16f));
            nightPostFXColor = AmbientSkiesEditorFunctions.GradientField(m_editorUtils, "NightPostFXColor", nightPostFXColor, helpEnabled);

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_syncPostProcessing = syncPostProcessing;
                m_profiles.m_dayTempature = dayTempature;
                m_profiles.m_dayPostFXColor = dayPostFXColor;
                m_profiles.m_nightTempature = nightTempature;
                m_profiles.m_nightPostFXColor = nightPostFXColor;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Time of day realtime gi foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODRealtimeGI(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            realtimeGIUpdate = m_editorUtils.Toggle("RealtimeGIUpdate", realtimeGIUpdate, helpEnabled);
            if (realtimeGIUpdate)
            {
                gIUpdateInterval = m_editorUtils.IntField("GIUpdateInterval", gIUpdateInterval, helpEnabled);
                EditorGUILayout.Space();

                m_editorUtils.Label("Realtime Emissive Materials Setup");
                realtimeEmission = m_editorUtils.Toggle("UseRealtimeEmissive", realtimeEmission, helpEnabled);
                if (realtimeEmission)
                {
                    syncRealtimeEmissionToTimeOfDay = m_editorUtils.Toggle("SyncToTimeOfDay", syncRealtimeEmissionToTimeOfDay, helpEnabled);
                }
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_realtimeGIUpdate = realtimeGIUpdate;
                m_profiles.m_gIUpdateInterval = gIUpdateInterval;
                m_profiles.m_realtimeEmission = realtimeEmission;
                m_profiles.m_syncRealtimeEmissionToTimeOfDay = syncRealtimeEmissionToTimeOfDay;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Time of day seasons foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODSeasons(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            hemisphereOrigin = (AmbientSkiesConsts.HemisphereOrigin)m_editorUtils.EnumPopup("HemisphereOrigin", hemisphereOrigin, helpEnabled);
            EditorGUILayout.BeginHorizontal();
            m_editorUtils.Text("CurrentSeason", GUILayout.Width(146f));
            m_editorUtils.TextNonLocalized(seasonString, GUILayout.Width(80f), GUILayout.Height(16f));
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_currentSeason = currentSeason;
                m_profiles.m_hemisphereOrigin = hemisphereOrigin;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Time of day skybox foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODSkybox(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //m_editorUtils.Heading("Lighting Settings");
            //EditorGUILayout.Space();
            //m_editorUtils.Heading("Fog Settings");
            //EditorGUILayout.Space();
            //m_editorUtils.Heading("Sky Settings");
            enableSunDisk = m_editorUtils.Toggle("EnableSunDisk", enableSunDisk, helpEnabled);
            timeOfDaySkyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", timeOfDaySkyboxRotation, -0.1f, 360.1f, helpEnabled);
            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                pBSSpaceEmissionTexture = (Cubemap) m_editorUtils.ObjectField("PBSSpaceEmissionTexture", pBSSpaceEmissionTexture, typeof(Cubemap), false, helpEnabled, GUILayout.MaxHeight((16f)));
                pBSTODSpaceEmissionMultiplier = m_editorUtils.Slider("PBSSpaceEmissionMultiplier", pBSTODSpaceEmissionMultiplier, 0f, 5f, helpEnabled);
            }
            //proceduralSunSize = m_editorUtils.Slider("SunSize", proceduralSunSize, 0f, 1f, helpEnabled);
            //skyTint = m_editorUtils.ColorField("SkyTint", skyTint, helpEnabled);
            //skyExposure = m_editorUtils.Slider("SkyExposure", skyExposure, 0f, 5f, helpEnabled);
            //skyMultiplier = m_editorUtils.Slider("SkyMultiplier", skyMultiplier, 0f, 5f, helpEnabled);
            //EditorGUILayout.Space();
            //m_editorUtils.Heading("[Built-In/Lightweight Only] Settings");
            //EditorGUILayout.Space();
            //m_editorUtils.Heading("[High Definition Only] Settings");

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_skyboxRotation = timeOfDaySkyboxRotation;
                m_profiles.m_pBSSpaceEmissionTexture = pBSSpaceEmissionTexture;
                m_profiles.m_pBSSpaceEmissionMultiplier = pBSTODSpaceEmissionMultiplier;
                m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Time of day time foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODTime(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            pauseTime = m_editorUtils.Toggle("PauseTime", pauseTime, helpEnabled);
            currentTimeOfDay = m_editorUtils.Slider("CurrentTimeOfDay", currentTimeOfDay, 0f, 1f, helpEnabled);
            dayLengthInSeconds = m_editorUtils.FloatField("DayLength", dayLengthInSeconds, helpEnabled);
            nightLengthInSeconds = m_editorUtils.FloatField("NightLength", nightLengthInSeconds, helpEnabled);
            dayDate = m_editorUtils.IntSlider("DateDay", dayDate, 1, 31, helpEnabled);
            monthDate = m_editorUtils.IntSlider("DateMonth", monthDate, 1, 12, helpEnabled);
            yearDate = m_editorUtils.IntField("DateYear", yearDate, helpEnabled);

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_pauseTime = pauseTime;
                m_profiles.m_currentTimeOfDay = currentTimeOfDay;
                m_profiles.m_nightLengthInSeconds = nightLengthInSeconds;
                m_profiles.m_dayLengthInSeconds = dayLengthInSeconds;
                m_profiles.m_dayDate = dayDate;
                m_profiles.m_monthDate = monthDate;
                m_profiles.m_yearDate = yearDate;

                m_hasChanged = true;
            }
        }

        #endregion

        #region Sky Tab Functions

        /// <summary>
        /// Profile settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ProfileSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            LoadAllVariables();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    EditorGUILayout.BeginHorizontal();
                    m_editorUtils.Text("SelectSkyboxDropdown", GUILayout.Width(146f));
                    newSkyboxSelection = EditorGUILayout.Popup(m_selectedSkyboxProfileIndex, skyboxChoices.ToArray(), GUILayout.MaxWidth(10000f), GUILayout.Height(16f));
                    //Prev / Next
                    GUILayout.FlexibleSpace();
                    if (newSkyboxSelection == 0)
                    {
                        GUI.enabled = false;
                    }
                    if (m_editorUtils.Button("PrevSkyboxButton"))
                    {
                        newSkyboxSelection--;
                        autoPostProcessingApplyTimer = 2f;
                        m_inspectorUpdate = true;
                    }
                    GUI.enabled = true;
                    if (newSkyboxSelection == skyboxChoices.Count - 1)
                    {
                        GUI.enabled = false;
                    }
                    if (m_editorUtils.Button("NextSkyboxButton"))
                    {
                        newSkyboxSelection++;
                        autoPostProcessingApplyTimer = 2f;
                        m_inspectorUpdate = true;
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                    if (usePostProcess)
                    {
                        autoMatchProfile = m_editorUtils.ToggleLeft("AutoMatchProfile", autoMatchProfile, helpEnabled);
                    }

                    #region Creation Mode

                    //Creation
                    if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                    {
                        if (m_profiles.m_skyProfiles.Count == 1)
                        {
                            GUI.enabled = false;
                        }

                        if (m_editorUtils.Button("RemoveNewProfile"))
                        {
                            m_profiles.m_skyProfiles.Remove(m_selectedSkyboxProfile);

                            m_newHDRIProfileIndex--;

                            //Add skies profile names
                            skyboxChoices.Clear();
                            foreach (var profile in m_profiles.m_skyProfiles)
                            {
                                skyboxChoices.Add(profile.name);
                            }

                            newSkyboxSelection--;
                            m_selectedSkyboxProfileIndex--;

                            Repaint();

                            return;
                        }

                        GUI.enabled = true;

                        if (m_editorUtils.Button("CreateNewProfile"))
                        {
                            AmbientSkyboxProfile newProfile = new AmbientSkyboxProfile();
                            m_profiles.m_skyProfiles.Add(newProfile);
                            newProfile.name = "New HDRI Skybox" + m_newHDRIProfileIndex;
                            newProfile.isPWProfile = true;

                            m_newHDRIProfileIndex++;

                            //Add skies profile names
                            skyboxChoices.Clear();
                            foreach (var profile in m_profiles.m_skyProfiles)
                            {
                                skyboxChoices.Add(profile.name);
                            }

                            newSkyboxSelection++;
                            m_selectedSkyboxProfileIndex++;

                            Repaint();

                            return;
                        }
                    }

                #endregion
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    if (m_profiles.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
                    {
                        EditorGUILayout.BeginHorizontal();
                        m_editorUtils.Text("SelectSkyboxDropdown", GUILayout.Width(146f));
                        newProceduralSkyboxSelection = EditorGUILayout.Popup(m_selectedProceduralSkyboxProfileIndex, proceduralSkyboxChoices.ToArray(), GUILayout.MaxWidth(10000f), GUILayout.Height(16f));
                        //Prev / Next
                        GUILayout.FlexibleSpace();
                        if (newProceduralSkyboxSelection == 0)
                        {
                            GUI.enabled = false;
                        }
                        if (m_editorUtils.Button("PrevSkyboxButton"))
                        {
                            newProceduralSkyboxSelection--;
                            autoPostProcessingApplyTimer = 2f;
                            m_inspectorUpdate = true;
                        }
                        GUI.enabled = true;
                        if (newProceduralSkyboxSelection == proceduralSkyboxChoices.Count - 1)
                        {
                            GUI.enabled = false;
                        }
                        if (m_editorUtils.Button("NextSkyboxButton"))
                        {
                            newProceduralSkyboxSelection++;
                            autoPostProcessingApplyTimer = 2f;
                            m_inspectorUpdate = true;
                        }
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();
                        if (usePostProcess)
                        {
                            autoMatchProfile = m_editorUtils.ToggleLeft("AutoMatchProfile", autoMatchProfile, helpEnabled);
                        }

                        #region Creation Mode

                        //Creation
                        if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                        {
                            if (m_profiles.m_proceduralSkyProfiles.Count == 1)
                            {
                                GUI.enabled = false;
                            }

                            if (m_editorUtils.Button("RemoveNewProfile"))
                            {
                                m_profiles.m_proceduralSkyProfiles.Remove(m_selectedProceduralSkyboxProfile);

                                newProceduralSkyboxSelection--;
                                m_selectedProceduralSkyboxProfileIndex--;

                                //Add procedural skies profile names
                                proceduralSkyboxChoices.Clear();
                                foreach (var profile in m_profiles.m_proceduralSkyProfiles)
                                {
                                    proceduralSkyboxChoices.Add(profile.name);
                                }

                                newProceduralSkyboxSelection--;

                                Repaint();

                                return;
                            }

                            GUI.enabled = true;

                            if (m_editorUtils.Button("CreateNewProfile"))
                            {
                                AmbientProceduralSkyboxProfile newProfile = new AmbientProceduralSkyboxProfile();
                                m_profiles.m_proceduralSkyProfiles.Add(newProfile);
                                newProfile.name = "New Procedural Skybox" + m_newProceduralProfileIndex;

                                m_newProceduralProfileIndex++;

                                //Add procedural skies profile names
                                proceduralSkyboxChoices.Clear();
                                foreach (var profile in m_profiles.m_proceduralSkyProfiles)
                                {
                                    proceduralSkyboxChoices.Add(profile.name);
                                }

                                newProceduralSkyboxSelection++;
                                m_selectedProceduralSkyboxProfileIndex++;

                                Repaint();

                                return;
                            }
                        }

                #endregion
                    }
                }
                else
                {
#if UNITY_2018_3_OR_NEWER
                    EditorGUILayout.BeginHorizontal();
                    m_editorUtils.Text("SelectSkyboxDropdown", GUILayout.Width(146f));
                    newGradientSkyboxSelection = EditorGUILayout.Popup(m_selectedGradientSkyboxProfileIndex, gradientSkyboxChoices.ToArray(), GUILayout.MaxWidth(10000f), GUILayout.Height(16f));
                    //Prev / Next
                    GUILayout.FlexibleSpace();
                    if (newGradientSkyboxSelection == 0)
                    {
                        GUI.enabled = false;
                    }
                    if (m_editorUtils.Button("PrevSkyboxButton"))
                    {
                        newGradientSkyboxSelection--;
                        autoPostProcessingApplyTimer = 2f;
                        m_inspectorUpdate = true;

                    }
                    GUI.enabled = true;
                    if (newGradientSkyboxSelection == gradientSkyboxChoices.Count - 1)
                    {
                        GUI.enabled = false;
                    }
                    if (m_editorUtils.Button("NextSkyboxButton"))
                    {
                        newGradientSkyboxSelection++;
                        autoPostProcessingApplyTimer = 2f;
                        m_inspectorUpdate = true;

                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                    if (usePostProcess)
                    {
                        autoMatchProfile = m_editorUtils.ToggleLeft("AutoMatchProfile", autoMatchProfile, helpEnabled);
                    }

                    #region Creation Mode

                    //Creation
                    if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                    {
                        if (m_profiles.m_gradientSkyProfiles.Count == 1)
                        {
                            GUI.enabled = false;
                        }

                        if (m_editorUtils.Button("RemoveNewProfile"))
                        {
                            m_profiles.m_gradientSkyProfiles.Remove(m_selectedGradientSkyboxProfile);

                            m_newGradientProfileIndex--;
#if UNITY_2018_3_OR_NEWER
                            //Add gradient skies profile names
                            gradientSkyboxChoices.Clear();
                            foreach (var profile in m_profiles.m_gradientSkyProfiles)
                            {
                                gradientSkyboxChoices.Add(profile.name);
                            }
#endif
                            newGradientSkyboxSelection--;
                            m_selectedGradientSkyboxProfileIndex--;

                            Repaint();

                            return;
                        }

                        GUI.enabled = true;

                        if (m_editorUtils.Button("CreateNewProfile"))
                        {
                            AmbientGradientSkyboxProfile newProfile = new AmbientGradientSkyboxProfile();
                            m_profiles.m_gradientSkyProfiles.Add(newProfile);
                            newProfile.name = "New Gradient Skybox" + m_newGradientProfileIndex;

                            m_newGradientProfileIndex++;

#if UNITY_2018_3_OR_NEWER
                            //Add gradient skies profile names
                            gradientSkyboxChoices.Clear();
                            foreach (var profile in m_profiles.m_gradientSkyProfiles)
                            {
                                gradientSkyboxChoices.Add(profile.name);
                            }
#endif
                            newGradientSkyboxSelection++;
                            m_selectedGradientSkyboxProfileIndex++;

                            Repaint();

                            return;
                        }
                    }

                #endregion
#endif
                }

                if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                {
                    m_foldoutCreationModeSettings = m_editorUtils.Panel("ShowCreationModeSettings", CreationSkyboxMode, m_foldoutCreationModeSettings);
                }

                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    m_foldoutTimeOfDaySettings = m_editorUtils.Panel("Show Time Of Day Settings", TimeOfDaySettingsEnabled, m_foldoutTimeOfDaySettings);
                    if (m_profiles.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
                    {
                        m_foldoutSunSettings = m_editorUtils.Panel("Show Sun Settings", SunSettingsEnabled, m_foldoutSunSettings);
                        m_foldoutSkySettings = m_editorUtils.Panel("Show Sky Settings", SkyboxSettingsEnabled, m_foldoutSkySettings);
                        m_foldoutAmbientSettings = m_editorUtils.Panel("Show Ambient Settings", AmbientSettingsEnabled, m_foldoutAmbientSettings);
                        m_foldoutFogSettings = m_editorUtils.Panel("Show Fog Settings", FogSettingsEnabled, m_foldoutFogSettings);
                        m_foldoutHDShadowSettings = m_editorUtils.Panel("Show HD Shadow Settings", HDShadowSettingsEnabled, m_foldoutHDShadowSettings);
                        m_foldoutScreenSpaceReflectionSettings = m_editorUtils.Panel("Show Screen Space Reflection Settings", ScreenSpaceReflectionSettingsEnabled, m_foldoutScreenSpaceReflectionSettings);
                        m_foldoutScreenSpaceRefractionSettings = m_editorUtils.Panel("Show Screen Space Refraction Settings", ScreenSpaceRefractionSettingsEnabled, m_foldoutScreenSpaceRefractionSettings);
                    }
                }
                else
                {
                    m_foldoutSunSettings = m_editorUtils.Panel("Show Sun Settings", SunSettingsEnabled, m_foldoutSunSettings);
                    m_foldoutSkySettings = m_editorUtils.Panel("Show Sky Settings", SkyboxSettingsEnabled, m_foldoutSkySettings);
                    m_foldoutAmbientSettings = m_editorUtils.Panel("Show Ambient Settings", AmbientSettingsEnabled, m_foldoutAmbientSettings);
                    m_foldoutFogSettings = m_editorUtils.Panel("Show Fog Settings", FogSettingsEnabled, m_foldoutFogSettings);
                    m_foldoutHDShadowSettings = m_editorUtils.Panel("Show HD Shadow Settings", HDShadowSettingsEnabled, m_foldoutHDShadowSettings);
                    m_foldoutScreenSpaceReflectionSettings = m_editorUtils.Panel("Show Screen Space Reflection Settings", ScreenSpaceReflectionSettingsEnabled, m_foldoutScreenSpaceReflectionSettings);
                    m_foldoutScreenSpaceRefractionSettings = m_editorUtils.Panel("Show Screen Space Refraction Settings", ScreenSpaceRefractionSettingsEnabled, m_foldoutScreenSpaceRefractionSettings);
                }
#endif
            }
            else
            {
                if (FindObjectOfType<WindZone>() == null)
                {
                    if (m_editorUtils.Button("Missing wind prefab. Add it to your scene?"))
                    {
                        GameObject parentObject = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Global", false);

                        GameObject windPrefab = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(SkyboxUtils.GetAssetPath("Ambient Skies URP Wind")), parentObject.transform, true);
                        windPrefab.name = "Ambient Skies Wind";

                        GameObject hdrpWind = GameObject.Find("Ambient Skies HDRP Wind");
                        if (hdrpWind != null)
                        {
                            DestroyImmediate(hdrpWind);
                        }
                    }
                }

                switch (systemtype)
                {
                    case AmbientSkiesConsts.SystemTypes.AmbientHDRISkies:
                    {
                        EditorGUILayout.BeginHorizontal();
                        m_editorUtils.Text("SelectSkyboxDropdown", GUILayout.Width(146f));
                        newSkyboxSelection = EditorGUILayout.Popup(m_selectedSkyboxProfileIndex, skyboxChoices.ToArray(), GUILayout.MaxWidth(10000f), GUILayout.Height(16f));

                        //Prev / Next
                        //EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (newSkyboxSelection == 0)
                        {
                            GUI.enabled = false;
                        }
                        if (m_editorUtils.Button("PrevSkyboxButton"))
                        {
                            newSkyboxSelection--;
                            autoPostProcessingApplyTimer = 2f;
                            m_inspectorUpdate = true;
                        }
                        GUI.enabled = true;
                        if (newSkyboxSelection == skyboxChoices.Count - 1)
                        {
                            GUI.enabled = false;
                        }
                        if (m_editorUtils.Button("NextSkyboxButton"))
                        {
                            newSkyboxSelection++;
                            autoPostProcessingApplyTimer = 2f;
                            m_inspectorUpdate = true;
                        }
                        GUI.enabled = true;

                        EditorGUILayout.EndHorizontal();

                        if (usePostProcess)
                        {
                            autoMatchProfile = m_editorUtils.ToggleLeft("AutoMatchProfile", autoMatchProfile, helpEnabled);
                        }

                        #region Creation Mode

                        //Creation
                        if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                        {
                            if (m_profiles.m_skyProfiles.Count == 1)
                            {
                                GUI.enabled = false;
                            }

                            if (m_editorUtils.Button("RemoveNewProfile"))
                            {
                                m_profiles.m_skyProfiles.Remove(m_selectedSkyboxProfile);

                                m_newHDRIProfileIndex--;

                                //Add skies profile names
                                skyboxChoices.Clear();
                                foreach (var profile in m_profiles.m_skyProfiles)
                                {
                                    skyboxChoices.Add(profile.name);
                                }

                                newSkyboxSelection--;
                                m_selectedSkyboxProfileIndex--;

                                Repaint();

                                return;
                            }

                            GUI.enabled = true;

                            if (m_editorUtils.Button("CreateNewProfile"))
                            {
                                AmbientSkyboxProfile newProfile = new AmbientSkyboxProfile();
                                m_profiles.m_skyProfiles.Add(newProfile);
                                newProfile.name = "New HDRI Skybox" + m_newHDRIProfileIndex;
                                newProfile.isPWProfile = true;

                                m_newHDRIProfileIndex++;

                                //Add skies profile names
                                skyboxChoices.Clear();
                                foreach (var profile in m_profiles.m_skyProfiles)
                                {
                                    skyboxChoices.Add(profile.name);
                                }

                                newSkyboxSelection++;
                                m_selectedSkyboxProfileIndex++;

                                Repaint();

                                return;
                            }
                        }

                        #endregion

                        //EditorGUILayout.EndHorizontal();
                        break;
                    }
                    case AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies:
                    {
                        if (m_profiles.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
                        {
                            EditorGUILayout.BeginHorizontal();
                            m_editorUtils.Text("SelectSkyboxDropdown", GUILayout.Width(146f));
                            newProceduralSkyboxSelection = EditorGUILayout.Popup(m_selectedProceduralSkyboxProfileIndex, proceduralSkyboxChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));
                            EditorGUILayout.EndHorizontal();
                            if (usePostProcess)
                            {
                                autoMatchProfile = m_editorUtils.ToggleLeft("AutoMatchProfile", autoMatchProfile, helpEnabled);
                            }
                            //Prev / Next
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (newProceduralSkyboxSelection == 0)
                            {
                                GUI.enabled = false;
                            }
                            if (m_editorUtils.Button("PrevSkyboxButton"))
                            {
                                newProceduralSkyboxSelection--;
                                autoPostProcessingApplyTimer = 2f;
                                m_inspectorUpdate = true;
                                AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(m_profiles);
                            }
                            GUI.enabled = true;
                            if (newProceduralSkyboxSelection == proceduralSkyboxChoices.Count - 1)
                            {
                                GUI.enabled = false;
                            }
                            if (m_editorUtils.Button("NextSkyboxButton"))
                            {
                                newProceduralSkyboxSelection++;
                                autoPostProcessingApplyTimer = 2f;
                                m_inspectorUpdate = true;
                                AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(m_profiles);
                            }
                            GUI.enabled = true;

                            #region Creation Mode

                            //Creation
                            if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                            {
                                if (m_profiles.m_proceduralSkyProfiles.Count == 1)
                                {
                                    GUI.enabled = false;
                                }

                                if (m_editorUtils.Button("RemoveNewProfile"))
                                {
                                    m_profiles.m_proceduralSkyProfiles.Remove(m_selectedProceduralSkyboxProfile);

                                    newProceduralSkyboxSelection--;
                                    m_selectedProceduralSkyboxProfileIndex--;

                                    //Add procedural skies profile names
                                    proceduralSkyboxChoices.Clear();
                                    foreach (var profile in m_profiles.m_proceduralSkyProfiles)
                                    {
                                        proceduralSkyboxChoices.Add(profile.name);
                                    }

                                    newProceduralSkyboxSelection--;

                                    Repaint();

                                    return;
                                }

                                GUI.enabled = true;

                                if (m_editorUtils.Button("CreateNewProfile"))
                                {
                                    AmbientProceduralSkyboxProfile newProfile = new AmbientProceduralSkyboxProfile();
                                    m_profiles.m_proceduralSkyProfiles.Add(newProfile);
                                    newProfile.name = "New Procedural Skybox" + m_newProceduralProfileIndex;

                                    m_newProceduralProfileIndex++;

                                    //Add procedural skies profile names
                                    proceduralSkyboxChoices.Clear();
                                    foreach (var profile in m_profiles.m_proceduralSkyProfiles)
                                    {
                                        proceduralSkyboxChoices.Add(profile.name);
                                    }

                                    newProceduralSkyboxSelection++;
                                    m_selectedProceduralSkyboxProfileIndex++;

                                    Repaint();

                                    return;
                                }
                            }

                            #endregion

                            EditorGUILayout.EndHorizontal();
                        }

                        break;
                    }
                }

                if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                {
                    m_foldoutCreationModeSettings = m_editorUtils.Panel("ShowCreationModeSettings", CreationSkyboxMode, m_foldoutCreationModeSettings);
                }

                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    m_foldoutTimeOfDaySettings = m_editorUtils.Panel("Show Time Of Day Settings", TimeOfDaySettingsEnabled, m_foldoutTimeOfDaySettings);
                }
                if (m_profiles.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
                {
                    m_foldoutSunSettings = m_editorUtils.Panel("Show Sun Settings", SunSettingsEnabled, m_foldoutSunSettings);
                    m_foldoutSkySettings = m_editorUtils.Panel("Show Sky Settings", SkyboxSettingsEnabled, m_foldoutSkySettings);
                    m_foldoutAmbientSettings = m_editorUtils.Panel("Show Ambient Settings", AmbientSettingsEnabled, m_foldoutAmbientSettings);
                    m_foldoutFogSettings = m_editorUtils.Panel("Show Fog Settings", FogSettingsEnabled, m_foldoutFogSettings);
                    m_foldoutShadowSettings = m_editorUtils.Panel("Show Shadow Settings", ShadowSettingsEnabled, m_foldoutShadowSettings);
                    if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                    {
                        m_foldoutHorizonSettings = m_editorUtils.Panel("Show Horizon Settings", HorizonSettingsEnabled, m_foldoutHorizonSettings);
                    }
                }
            }

            GUI.enabled = true;

            if (systemtype != AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                m_profiles.m_useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
                useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                //Time of day
                if (useTimeOfDay != m_profiles.m_useTimeOfDay)
                {
                    SkyboxUtils.SetTimeOfDaySettings(m_selectedProceduralSkyboxProfile, m_profiles);
                }

                m_profiles.m_useTimeOfDay = useTimeOfDay;

                if (autoMatchProfile != m_profiles.m_autoMatchProfile)
                {
                    autoPostProcessingApplyTimer = 2f;
                    m_inspectorUpdate = true;
                    m_profiles.m_autoMatchProfile = autoMatchProfile;
                }

                if (newSkyboxSelection != m_selectedSkyboxProfileIndex)
                {
                    m_profiles.m_autoMatchProfile = autoMatchProfile;
                    m_selectedSkyboxProfileIndex = newSkyboxSelection;

                    autoPostProcessingApplyTimer = 2f;
                    m_inspectorUpdate = true;

                    AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(m_profiles);

                    LoadAllVariables();

                    SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                    //m_countdown = 0.1f;
                    m_extraCheck = false;
                    EditorApplication.update -= EditorUpdate;
                    EditorApplication.update += EditorUpdate;
                }
                if (newProceduralSkyboxSelection != m_selectedProceduralSkyboxProfileIndex)
                {
                    m_profiles.m_autoMatchProfile = autoMatchProfile;
                    m_selectedProceduralSkyboxProfileIndex = newProceduralSkyboxSelection;

                    autoPostProcessingApplyTimer = 2f;
                    m_inspectorUpdate = true;

                    AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(m_profiles);

                    LoadAllVariables();

                    SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                    m_countdown = 0.1f;
                    m_extraCheck = false;
                    EditorApplication.update -= EditorUpdate;
                    EditorApplication.update += EditorUpdate;
                }
                if (newGradientSkyboxSelection != m_selectedGradientSkyboxProfileIndex)
                {
                    m_profiles.m_autoMatchProfile = autoMatchProfile;
                    m_selectedGradientSkyboxProfileIndex = newGradientSkyboxSelection;

                    autoPostProcessingApplyTimer = 2f;
                    m_inspectorUpdate = true;

                    AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(m_profiles);

                    SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                    //m_countdown = 0.1f;
                    EditorApplication.update -= EditorUpdate;
                    EditorApplication.update += EditorUpdate;
                }

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Skybox settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void SkyboxSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Load Settings
            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                //Skybox
                if (!m_selectedSkyboxProfile.isPWProfile)
                {
                    profileName = m_selectedSkyboxProfile.name;
                    skyboxTint = m_selectedSkyboxProfile.customSkyboxTint;
                    skyboxExposure = m_selectedSkyboxProfile.customSkyboxExposure;
                    skyboxRotation = m_selectedSkyboxProfile.customSkyboxRotation;
                    skyboxPitch = m_selectedSkyboxProfile.customSkyboxPitch;
                    proceduralSunSize = m_selectedSkyboxProfile.customProceduralSunSize;
                    proceduralSunSizeConvergence = m_selectedSkyboxProfile.customProceduralSunSizeConvergence;
                    proceduralAtmosphereThickness = m_selectedSkyboxProfile.customProceduralAtmosphereThickness;
                    proceduralGroundColor = m_selectedSkyboxProfile.customProceduralGroundColor;
                    enableSunDisk = m_selectedSkyboxProfile.enableSunDisk;
                }
                skyboxTint = m_selectedSkyboxProfile.skyboxTint;
                skyboxExposure = m_selectedSkyboxProfile.skyboxExposure;
                skyboxRotation = m_selectedSkyboxProfile.skyboxRotation;
                skyMultiplier = m_selectedSkyboxProfile.skyMultiplier;
                skyboxPitch = m_selectedSkyboxProfile.skyboxPitch;
                customSkybox = m_selectedSkyboxProfile.customSkybox;
                isProceduralSkybox = m_selectedSkyboxProfile.isProceduralSkybox;

            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                //Skybox
                skyboxTint = m_selectedProceduralSkyboxProfile.proceduralSkyTint;
                skyboxExposure = m_selectedProceduralSkyboxProfile.proceduralSkyExposure;
                skyboxRotation = m_selectedProceduralSkyboxProfile.proceduralSkyboxRotation;
                skyMultiplier = m_selectedProceduralSkyboxProfile.proceduralSkyMultiplier;
                skyboxPitch = m_selectedProceduralSkyboxProfile.proceduralSkyboxPitch;
                proceduralSunSize = m_selectedProceduralSkyboxProfile.proceduralSunSize;
                proceduralSunSizeConvergence = m_selectedProceduralSkyboxProfile.proceduralSunSizeConvergence;
                proceduralAtmosphereThickness = m_selectedProceduralSkyboxProfile.proceduralAtmosphereThickness;
                proceduralGroundColor = m_selectedProceduralSkyboxProfile.proceduralGroundColor;
                includeSunInBaking = m_selectedProceduralSkyboxProfile.includeSunInBaking;
                enableSunDisk = m_selectedProceduralSkyboxProfile.enableSunDisk;

                //Physically Based Sky
                //Planet
                pBSEarthPreset = m_selectedProceduralSkyboxProfile.pBSEarthPreset;
                pBSSphericalMode = m_selectedProceduralSkyboxProfile.pBSSphericalMode;
                pBSPlanetCenterPosition = m_selectedProceduralSkyboxProfile.pBSPlanetCenterPosition;
                pBSPlanetRotation = m_selectedProceduralSkyboxProfile.pBSPlanetRotation;
                pBSGroundColorTexture = m_selectedProceduralSkyboxProfile.pBSGroundColorTexture;
                pBSGroundTint = m_selectedProceduralSkyboxProfile.pBSGroundTint;
                pBSGroundEmissionTexture = m_selectedProceduralSkyboxProfile.pBSGroundEmissionTexture;
                pBSGroundEmissionMultiplier = m_selectedProceduralSkyboxProfile.pBSGroundEmissionMultiplier;
                //Space
                pBSSpaceRotation = m_selectedProceduralSkyboxProfile.pBSSpaceRotation;
                pBSSpaceEmission = m_selectedProceduralSkyboxProfile.pBSSpaceEmission;
                pBSSpaceEmissionMultiplier = m_selectedProceduralSkyboxProfile.pBSSpaceEmissionMultiplier;
                //Air
                pBSAirMaximumAltitude = m_selectedProceduralSkyboxProfile.pBSAirMaximumAltitude;
                pBSAirDensityR = m_selectedProceduralSkyboxProfile.pBSAirDensityR;
                pBSAirDensityG = m_selectedProceduralSkyboxProfile.pBSAirDensityG;
                pBSAirDensityB = m_selectedProceduralSkyboxProfile.pBSAirDensityB;
                pBSAirTint = m_selectedProceduralSkyboxProfile.pBSAirTint;
                //Aerosols
                pBSAerosolMaximumAltitude = m_selectedProceduralSkyboxProfile.pBSAerosolMaximumAltitude;
                pBSAerosolDensity = m_selectedProceduralSkyboxProfile.pBSAerosolDensity;
                pBSAerosolTint = m_selectedProceduralSkyboxProfile.pBSAerosolTint;
                pBSAerosolAnisotropy = m_selectedProceduralSkyboxProfile.pBSAerosolAnisotropy;
                //Artistic Overrides
                pBSColorSaturation = m_selectedProceduralSkyboxProfile.pBSColorSaturation;
                pBSAlphaSaturation = m_selectedProceduralSkyboxProfile.pBSAlphaSaturation;
                pBSAlphaMultiplier = m_selectedProceduralSkyboxProfile.pBSAlphaMultiplier;
                pBSHorizonTint = m_selectedProceduralSkyboxProfile.pBSHorizonTint;
                pBSHorizonZenithShift = m_selectedProceduralSkyboxProfile.pBSHorizonZenithShift;
                pBSZenithTint = m_selectedProceduralSkyboxProfile.pBSZenithTint;
                //Miscellaneous
                pBSNumberOfBounces = m_selectedProceduralSkyboxProfile.pBSNumberOfBounces;
                pBSIntensityMode = m_selectedProceduralSkyboxProfile.pBSIntensityMode;
                pBSMultiplier = m_selectedProceduralSkyboxProfile.pBSMultiplier;
                pBSExposure = m_selectedProceduralSkyboxProfile.pBSExposure;
                pBSUpdateMode = m_selectedProceduralSkyboxProfile.pBSUpdateMode;
                pBSIncludeSunInBaking = m_selectedProceduralSkyboxProfile.pBSIncludeSunInBaking;
            }
            else
            {
                //Skybox
                skyboxRotation = m_selectedGradientSkyboxProfile.skyboxRotation;
                skyboxPitch = m_selectedGradientSkyboxProfile.skyboxPitch;
#if HDPipeline
                topSkyColor = m_selectedGradientSkyboxProfile.topColor;
                middleSkyColor = m_selectedGradientSkyboxProfile.middleColor;
                bottomSkyColor = m_selectedGradientSkyboxProfile.bottomColor;
                gradientDiffusion = m_selectedGradientSkyboxProfile.gradientDiffusion;
                skyMultiplier = m_selectedGradientSkyboxProfile.skyMultiplier;
                skyboxExposure = m_selectedGradientSkyboxProfile.hDRIExposure;
#endif
            }

            m_profiles.m_updateFog = true;
            m_profiles.m_updateSun = true;

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {                   
                    if (!m_selectedSkyboxProfile.isPWProfile)
                    {
                        customSkybox = (Cubemap)m_editorUtils.ObjectField("CustomSkybox", customSkybox, typeof(Cubemap), false, helpEnabled, GUILayout.Height(16f));
                    }
                    skyboxExposure = m_editorUtils.Slider("SkyboxExposureSlider", skyboxExposure, 0f, 10f, helpEnabled);
                    skyMultiplier = m_editorUtils.Slider("SkyMultiplier", skyMultiplier, 0f, 5f, helpEnabled);
                    skyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", skyboxRotation, 0f, 360f, helpEnabled);
                    skyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", skyboxPitch, 0f, 360f, helpEnabled);
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    m_editorUtils.Heading("Planet Settings");
                    pBSEarthPreset = m_editorUtils.Toggle("PBSEarthPreset", pBSEarthPreset, helpEnabled);
                    pBSSphericalMode = m_editorUtils.Toggle("PBSSphericalMode", pBSSphericalMode, helpEnabled);
                    pBSPlanetCenterPosition = m_editorUtils.Vector3Field("PBSPlanetCenterPosition", pBSPlanetCenterPosition, helpEnabled);
                    pBSPlanetRotation = m_editorUtils.Vector3Field("PBSPlanetRotation", pBSPlanetRotation, helpEnabled);
                    pBSGroundColorTexture = (Cubemap)m_editorUtils.ObjectField("PBSGroundColorTexture", pBSGroundColorTexture, typeof(Cubemap), false, helpEnabled, GUILayout.Height(16f));
                    pBSGroundTint = m_editorUtils.ColorField("PBSGroundTint", pBSGroundTint, helpEnabled);
                    pBSGroundEmissionTexture = (Cubemap)m_editorUtils.ObjectField("PBSGroundEmissionTexture", pBSGroundEmissionTexture, typeof(Cubemap), false, helpEnabled, GUILayout.Height(16f));
                    pBSGroundEmissionMultiplier = m_editorUtils.FloatField("PBSGroundEmissionMultiplier", pBSGroundEmissionMultiplier, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Heading("Space Settings");
                    pBSSpaceRotation = m_editorUtils.Vector3Field("PBSSpaceRotation", pBSSpaceRotation, helpEnabled);
                    pBSSpaceEmission = (Cubemap)m_editorUtils.ObjectField("PBSSpaceEmissionTexture", pBSSpaceEmission, typeof(Cubemap), false, helpEnabled, GUILayout.Height(16f));
                    pBSSpaceEmissionMultiplier = m_editorUtils.FloatField("PBSSpaceEmissionMultiplier", pBSSpaceEmissionMultiplier, helpEnabled);
                    EditorGUILayout.Space();
                    if (!pBSEarthPreset)
                    {
                        m_editorUtils.Heading("Air Settings");
                        pBSAirMaximumAltitude = m_editorUtils.FloatField("PBSAirMaximumAltitude", pBSAirMaximumAltitude, helpEnabled);
                        pBSAirDensityR = m_editorUtils.Slider("PBSAirDensityRed", pBSAirDensityR, 0f, 1f, helpEnabled);
                        pBSAirDensityG = m_editorUtils.Slider("PBSAirDensityGreen", pBSAirDensityG, 0f, 1f, helpEnabled);
                        pBSAirDensityB = m_editorUtils.Slider("PBSAirDensityBlue", pBSAirDensityB, 0f, 1f, helpEnabled);
                        pBSAirTint = m_editorUtils.ColorField("PBSAirTint", pBSAirTint, helpEnabled);
                        EditorGUILayout.Space();
                    }
                    m_editorUtils.Heading("Aerosols Settings");
                    pBSAerosolMaximumAltitude = m_editorUtils.FloatField("PBSAerosolMaximumAltitude", pBSAerosolMaximumAltitude, helpEnabled);
                    pBSAerosolDensity = m_editorUtils.Slider("PBSAerosolDensity", pBSAerosolDensity, 0f, 1f, helpEnabled);
                    pBSAerosolTint = m_editorUtils.ColorField("PBSAerosolTint", pBSAerosolTint, helpEnabled);
                    pBSAerosolAnisotropy = m_editorUtils.Slider("PBSAerosolAnisotropy", pBSAerosolAnisotropy, - 1f, 1f, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Heading("Artistic Overrides");
                    pBSColorSaturation = m_editorUtils.Slider("PBSColorSaturation", pBSColorSaturation, 0f, 1f, helpEnabled);
                    pBSAlphaSaturation = m_editorUtils.Slider("PBSAlphaSaturation", pBSAlphaSaturation, 0f, 1f, helpEnabled);
                    pBSAlphaMultiplier = m_editorUtils.Slider("PBSAlphaMultiplier", pBSAlphaMultiplier, 0f, 1f, helpEnabled);
                    pBSHorizonTint = m_editorUtils.ColorField("PBSHorizonTint", pBSHorizonTint, helpEnabled);
                    pBSHorizonZenithShift = m_editorUtils.Slider("PBSHorizonZenithShift", pBSHorizonZenithShift, -1f, 1f, helpEnabled);
                    pBSZenithTint = m_editorUtils.ColorField("PBSZenithTint", pBSZenithTint, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Heading("Miscellaneous Settings");
                    pBSNumberOfBounces = m_editorUtils.IntSlider("PBSNumberOfBounces", pBSNumberOfBounces, 1, 10, helpEnabled);
                    pBSIntensityMode = (AmbientSkiesConsts.HDRPIntensityMode)m_editorUtils.EnumPopup("PBSIntensityMode", pBSIntensityMode, helpEnabled);
                    if (pBSIntensityMode == AmbientSkiesConsts.HDRPIntensityMode.Exposure)
                    {
                        pBSExposure = m_editorUtils.FloatField("PBSExposure", pBSExposure, helpEnabled);
                    }
                    else
                    {
                        pBSMultiplier = m_editorUtils.FloatField("PBSMultiplier", pBSMultiplier, helpEnabled);
                    }
                    pBSUpdateMode = (AmbientSkiesConsts.EnvironementSkyUpdateMode)m_editorUtils.EnumPopup("PBSUpdateMode", pBSUpdateMode, helpEnabled);
                    pBSIncludeSunInBaking = m_editorUtils.Toggle("PBSIncludeSunInBaking", pBSIncludeSunInBaking, helpEnabled);
                }
                else
                {
                    topSkyColor = m_editorUtils.ColorField("TopSkyColor", topSkyColor, helpEnabled);
                    middleSkyColor = m_editorUtils.ColorField("MiddleSkyColor", middleSkyColor, helpEnabled);
                    bottomSkyColor = m_editorUtils.ColorField("BottomSkyColor", bottomSkyColor, helpEnabled);
                    gradientDiffusion = m_editorUtils.Slider("GradientDiffusion", gradientDiffusion, 0f, 5f, helpEnabled);
                    skyboxExposure = m_editorUtils.Slider("SkyExposure", skyboxExposure, 0f, 5f, helpEnabled);
                    skyMultiplier = m_editorUtils.Slider("SkyMultiplier", skyMultiplier, 0f, 5f, helpEnabled);
                    skyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", skyboxRotation, 0f, 360f, helpEnabled);
                    skyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", skyboxPitch, 0f, 360f, helpEnabled);
                }
#endif    
            }
            else
            {
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    if (!m_selectedSkyboxProfile.isPWProfile)
                    {
                        profileName = m_editorUtils.TextField("ProfileNaming", m_selectedSkyboxProfile.name, helpEnabled);
                        isProceduralSkybox = m_editorUtils.Toggle("IsProceduralSkybox", isProceduralSkybox, helpEnabled);
                        if (!isProceduralSkybox)
                        {
                            customSkybox = (Cubemap)m_editorUtils.ObjectField("CustomSkybox", customSkybox, typeof(Cubemap), false, helpEnabled, GUILayout.Height(16f));
                            skyboxTint = m_editorUtils.ColorField("SkyboxTintColorSelector", skyboxTint, helpEnabled);
                            skyboxExposure = m_editorUtils.Slider("SkyboxExposureSlider", skyboxExposure, 0f, 5f, helpEnabled);
                            skyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", skyboxRotation, 0f, 360f, helpEnabled);
                            skyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", skyboxPitch, 0f, 360f, helpEnabled);
                        }
                        else
                        {
                            enableSunDisk = m_editorUtils.Toggle("EnableSunDisk", enableSunDisk, helpEnabled);
                            proceduralAtmosphereThickness = m_editorUtils.Slider("AtmosphereThickness", proceduralAtmosphereThickness, 0f, 5f, helpEnabled);
                            skyboxTint = m_editorUtils.ColorField("SkyboxTintColorSelector", skyboxTint, helpEnabled);
                            proceduralGroundColor = m_editorUtils.ColorField("GroundColor", proceduralGroundColor, helpEnabled);
                            skyboxExposure = m_editorUtils.Slider("SkyboxExposureSlider", skyboxExposure, 0f, 5f, helpEnabled);
                            skyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", skyboxRotation, 0, 360f, helpEnabled);
                            skyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", skyboxPitch, 0f, 360f, helpEnabled);
                        }
                    }
                    else
                    {
                        skyboxTint = m_editorUtils.ColorField("SkyboxTintColorSelector", skyboxTint, helpEnabled);
                        skyboxExposure = m_editorUtils.Slider("SkyboxExposureSlider", skyboxExposure, 0f, 5f, helpEnabled);
                        skyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", skyboxRotation, 0f, 360f, helpEnabled);
                        skyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", skyboxPitch, 0f, 360f, helpEnabled);
                    }
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    proceduralAtmosphereThickness = m_editorUtils.Slider("AtmosphereThickness", proceduralAtmosphereThickness, 0f, 5f, helpEnabled);
                    skyboxTint = m_editorUtils.ColorField("SkyboxTintColorSelector", skyboxTint, helpEnabled);
                    proceduralGroundColor = m_editorUtils.ColorField("GroundColor", proceduralGroundColor, helpEnabled);
                    skyboxExposure = m_editorUtils.Slider("SkyboxExposureSlider", skyboxExposure, 0f, 5f, helpEnabled);
                    skyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", skyboxRotation, 0f, 360f, helpEnabled);
                    skyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", skyboxPitch, 0f, 360f, helpEnabled);
                }
                else
                {
                    EditorGUILayout.HelpBox("Gradient Sky is only avaliable in High Definition", MessageType.Warning);
                }
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateVisualEnvironment = true;

                //Settings
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    //Skybox
                    if (!m_selectedSkyboxProfile.isPWProfile)
                    {
                        m_selectedSkyboxProfile.name = profileName;
                        m_selectedSkyboxProfile.customSkyboxTint = skyboxTint;
                        m_selectedSkyboxProfile.customSkyboxExposure = skyboxExposure;
                        m_selectedSkyboxProfile.customSkyboxRotation = skyboxRotation;
                        m_selectedSkyboxProfile.customSkyboxPitch = skyboxPitch;
                        m_selectedSkyboxProfile.customProceduralSunSize = proceduralSunSize;
                        m_selectedSkyboxProfile.customProceduralSunSizeConvergence = proceduralSunSizeConvergence;
                        m_selectedSkyboxProfile.customProceduralAtmosphereThickness = proceduralAtmosphereThickness;
                        m_selectedSkyboxProfile.customProceduralGroundColor = proceduralGroundColor;
                        m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;
                    }

                    m_selectedSkyboxProfile.skyboxTint = skyboxTint;
                    m_selectedSkyboxProfile.skyboxExposure = skyboxExposure;
                    m_selectedSkyboxProfile.skyboxRotation = skyboxRotation;
                    m_selectedSkyboxProfile.skyMultiplier = skyMultiplier;
                    m_selectedSkyboxProfile.skyboxPitch = skyboxPitch;
                    m_selectedSkyboxProfile.customSkybox = customSkybox;
                    m_selectedSkyboxProfile.isProceduralSkybox = isProceduralSkybox;

                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //Skybox
                    m_selectedProceduralSkyboxProfile.proceduralSkyTint = skyboxTint;
                    m_selectedProceduralSkyboxProfile.proceduralSkyExposure = skyboxExposure;
                    m_selectedProceduralSkyboxProfile.proceduralSkyboxRotation = skyboxRotation;
                    m_selectedProceduralSkyboxProfile.proceduralSkyMultiplier = skyMultiplier;
                    m_selectedProceduralSkyboxProfile.proceduralSkyboxPitch = skyboxPitch;
                    m_selectedProceduralSkyboxProfile.proceduralSunSize = proceduralSunSize;
                    m_selectedProceduralSkyboxProfile.proceduralSunSizeConvergence = proceduralSunSizeConvergence;
                    m_selectedProceduralSkyboxProfile.proceduralAtmosphereThickness = proceduralAtmosphereThickness;
                    m_selectedProceduralSkyboxProfile.proceduralGroundColor = proceduralGroundColor;
                    m_selectedProceduralSkyboxProfile.includeSunInBaking = includeSunInBaking;
                    m_selectedProceduralSkyboxProfile.enableSunDisk = enableSunDisk;

                    //Physically Based Sky
                    //Planet
                    m_selectedProceduralSkyboxProfile.pBSEarthPreset = pBSEarthPreset;
                    m_selectedProceduralSkyboxProfile.pBSSphericalMode = pBSSphericalMode;
                    m_selectedProceduralSkyboxProfile.pBSPlanetCenterPosition = pBSPlanetCenterPosition;
                    m_selectedProceduralSkyboxProfile.pBSPlanetRotation = pBSPlanetRotation;
                    m_selectedProceduralSkyboxProfile.pBSGroundColorTexture = pBSGroundColorTexture;
                    m_selectedProceduralSkyboxProfile.pBSGroundTint = pBSGroundTint;
                    m_selectedProceduralSkyboxProfile.pBSGroundEmissionTexture = pBSGroundEmissionTexture;
                    m_selectedProceduralSkyboxProfile.pBSGroundEmissionMultiplier = pBSGroundEmissionMultiplier;
                    //Space
                    m_selectedProceduralSkyboxProfile.pBSSpaceRotation = pBSSpaceRotation;
                    m_selectedProceduralSkyboxProfile.pBSSpaceEmission = pBSSpaceEmission;
                    m_selectedProceduralSkyboxProfile.pBSSpaceEmissionMultiplier = pBSSpaceEmissionMultiplier;
                    //Air
                    m_selectedProceduralSkyboxProfile.pBSAirMaximumAltitude = pBSAirMaximumAltitude;
                    m_selectedProceduralSkyboxProfile.pBSAirDensityR = pBSAirDensityR;
                    m_selectedProceduralSkyboxProfile.pBSAirDensityG = pBSAirDensityG;
                    m_selectedProceduralSkyboxProfile.pBSAirDensityB = pBSAirDensityB;
                    m_selectedProceduralSkyboxProfile.pBSAirTint = pBSAirTint;
                    //Aerosols
                    m_selectedProceduralSkyboxProfile.pBSAerosolMaximumAltitude = pBSAerosolMaximumAltitude;
                    m_selectedProceduralSkyboxProfile.pBSAerosolDensity = pBSAerosolDensity;
                    m_selectedProceduralSkyboxProfile.pBSAerosolTint = pBSAerosolTint;
                    m_selectedProceduralSkyboxProfile.pBSAerosolAnisotropy = pBSAerosolAnisotropy;
                    //Artistic Overrides
                    m_selectedProceduralSkyboxProfile.pBSColorSaturation = pBSColorSaturation;
                    m_selectedProceduralSkyboxProfile.pBSAlphaSaturation = pBSAlphaSaturation;
                    m_selectedProceduralSkyboxProfile.pBSAlphaMultiplier = pBSAlphaMultiplier;
                    m_selectedProceduralSkyboxProfile.pBSHorizonTint = pBSHorizonTint;
                    m_selectedProceduralSkyboxProfile.pBSHorizonZenithShift = pBSHorizonZenithShift;
                    m_selectedProceduralSkyboxProfile.pBSZenithTint = pBSZenithTint;
                    //Miscellaneous
                    m_selectedProceduralSkyboxProfile.pBSNumberOfBounces = pBSNumberOfBounces;
                    m_selectedProceduralSkyboxProfile.pBSIntensityMode = pBSIntensityMode;
                    m_selectedProceduralSkyboxProfile.pBSMultiplier = pBSMultiplier;
                    m_selectedProceduralSkyboxProfile.pBSExposure = pBSExposure;
                    m_selectedProceduralSkyboxProfile.pBSUpdateMode = pBSUpdateMode;
                    m_selectedProceduralSkyboxProfile.pBSIncludeSunInBaking = pBSIncludeSunInBaking;
                }
                else
                {
                    //Skybox
                    m_selectedGradientSkyboxProfile.skyboxRotation = skyboxRotation;
                    m_selectedGradientSkyboxProfile.skyboxPitch = skyboxPitch;
#if HDPipeline
                    m_selectedGradientSkyboxProfile.topColor = topSkyColor;
                    m_selectedGradientSkyboxProfile.middleColor = middleSkyColor;
                    m_selectedGradientSkyboxProfile.bottomColor = bottomSkyColor;
                    m_selectedGradientSkyboxProfile.gradientDiffusion = gradientDiffusion;
                    m_selectedGradientSkyboxProfile.skyMultiplier = skyMultiplier;
                    m_selectedGradientSkyboxProfile.hDRIExposure = skyboxExposure;
#endif
                }

                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Fog settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void FogSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Time of day
            useTimeOfDay = m_profiles.m_useTimeOfDay;

            //Configuration type
            configurationType = m_profiles.m_configurationType;

            //Load Settings
            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                //Fog mode
                fogType = m_selectedSkyboxProfile.fogType;

                //Fog settings
                fogColor = m_selectedSkyboxProfile.fogColor;
                fogDistance = m_selectedSkyboxProfile.fogDistance;
                hDRPFogDistance = m_selectedSkyboxProfile.hDRPFogDistance;
                nearFogDistance = m_selectedSkyboxProfile.nearFogDistance;
                fogDensity = m_selectedSkyboxProfile.fogDensity;

                //Volumetric Fog
                useDistanceFog = m_selectedSkyboxProfile.volumetricEnableDistanceFog;
                baseFogDistance = m_selectedSkyboxProfile.volumetricBaseFogDistance;
                baseFogHeight = m_selectedSkyboxProfile.volumetricBaseFogHeight;
                meanFogHeight = m_selectedSkyboxProfile.volumetricMeanHeight;
                globalAnisotropy = m_selectedSkyboxProfile.volumetricGlobalAnisotropy;
                globalLightProbeDimmer = m_selectedSkyboxProfile.volumetricGlobalLightProbeDimmer;

                //Exponential Fog
                exponentialFogDensity = m_selectedSkyboxProfile.exponentialFogDensity;
                exponentialBaseFogHeight = m_selectedSkyboxProfile.exponentialBaseHeight;
                exponentialHeightAttenuation = m_selectedSkyboxProfile.exponentialHeightAttenuation;
                exponentialMaxFogDistance = m_selectedSkyboxProfile.exponentialMaxFogDistance;
                exponentialMipFogNear = m_selectedSkyboxProfile.exponentialMipFogNear;
                exponentialMipFogFar = m_selectedSkyboxProfile.exponentialMipFogFar;
                exponentialMipFogMax = m_selectedSkyboxProfile.exponentialMipFogMaxMip;

                //Linear Fog
                linearFogDensity = m_selectedSkyboxProfile.linearFogDensity;
                linearFogHeightStart = m_selectedSkyboxProfile.linearHeightStart;
                linearFogHeightEnd = m_selectedSkyboxProfile.linearHeightEnd;
                linearFogMaxDistance = m_selectedSkyboxProfile.linearMaxFogDistance;
                linearMipFogNear = m_selectedSkyboxProfile.linearMipFogNear;
                linearMipFogFar = m_selectedSkyboxProfile.linearMipFogFar;
                linearMipFogMax = m_selectedSkyboxProfile.linearMipFogMaxMip;

                //Volumetric Light Controller
                depthExtent = m_selectedSkyboxProfile.volumetricDistanceRange;
                sliceDistribution = m_selectedSkyboxProfile.volumetricSliceDistributionUniformity;

                //Density Fog Volume
                useDensityFogVolume = m_selectedSkyboxProfile.useFogDensityVolume;
                singleScatteringAlbedo = m_selectedSkyboxProfile.singleScatteringAlbedo;
                densityVolumeFogDistance = m_selectedSkyboxProfile.densityVolumeFogDistance;
                fogDensityMaskTexture = m_selectedSkyboxProfile.fogDensityMaskTexture;
                densityMaskTiling = m_selectedSkyboxProfile.densityMaskTiling;
                densityScale = m_selectedSkyboxProfile.densityScale;

                //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                //Fog Mode
                fogColorMode = m_selectedSkyboxProfile.fogColorMode;

#endif
                //Sun Settings
                shadowStrength = m_selectedSkyboxProfile.shadowStrength;
                indirectLightMultiplier = m_selectedSkyboxProfile.indirectLightMultiplier;
                sunColor = m_selectedSkyboxProfile.sunColor;
                sunIntensity = m_selectedSkyboxProfile.sunIntensity;
            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                //Fog mode
                fogType = m_selectedProceduralSkyboxProfile.fogType;

                //Fog settings
                fogColor = m_selectedProceduralSkyboxProfile.proceduralFogColor;
                fogDistance = m_selectedProceduralSkyboxProfile.proceduralFogDistance;
                hDRPFogDistance = m_selectedProceduralSkyboxProfile.hDRPFogDistance;
                nearFogDistance = m_selectedProceduralSkyboxProfile.proceduralNearFogDistance;
                fogDensity = m_selectedProceduralSkyboxProfile.proceduralFogDensity;

                //HD Pipeline Settings
                //Volumetric Fog
                useDistanceFog = m_selectedProceduralSkyboxProfile.volumetricEnableDistanceFog;
                baseFogDistance = m_selectedProceduralSkyboxProfile.volumetricBaseFogDistance;
                baseFogHeight = m_selectedProceduralSkyboxProfile.volumetricBaseFogHeight;
                meanFogHeight = m_selectedProceduralSkyboxProfile.volumetricMeanHeight;
                globalAnisotropy = m_selectedProceduralSkyboxProfile.volumetricGlobalAnisotropy;
                globalLightProbeDimmer = m_selectedProceduralSkyboxProfile.volumetricGlobalLightProbeDimmer;

                //Exponential Fog
                exponentialFogDensity = m_selectedProceduralSkyboxProfile.exponentialFogDensity;
                exponentialBaseFogHeight = m_selectedProceduralSkyboxProfile.exponentialBaseHeight;
                exponentialHeightAttenuation = m_selectedProceduralSkyboxProfile.exponentialHeightAttenuation;
                exponentialMaxFogDistance = m_selectedProceduralSkyboxProfile.exponentialMaxFogDistance;
                exponentialMipFogNear = m_selectedProceduralSkyboxProfile.exponentialMipFogNear;
                exponentialMipFogFar = m_selectedProceduralSkyboxProfile.exponentialMipFogFar;
                exponentialMipFogMax = m_selectedProceduralSkyboxProfile.exponentialMipFogMaxMip;

                //Linear Fog
                linearFogDensity = m_selectedProceduralSkyboxProfile.linearFogDensity;
                linearFogHeightStart = m_selectedProceduralSkyboxProfile.linearHeightStart;
                linearFogHeightEnd = m_selectedProceduralSkyboxProfile.linearHeightEnd;
                linearFogMaxDistance = m_selectedProceduralSkyboxProfile.linearMaxFogDistance;
                linearMipFogNear = m_selectedProceduralSkyboxProfile.linearMipFogNear;
                linearMipFogFar = m_selectedProceduralSkyboxProfile.linearMipFogFar;
                linearMipFogMax = m_selectedProceduralSkyboxProfile.linearMipFogMaxMip;

                //Volumetric Light Controller
                depthExtent = m_selectedProceduralSkyboxProfile.volumetricDistanceRange;
                sliceDistribution = m_selectedProceduralSkyboxProfile.volumetricSliceDistributionUniformity;

                //Density Fog Volume
                useDensityFogVolume = m_selectedProceduralSkyboxProfile.useFogDensityVolume;
                singleScatteringAlbedo = m_selectedProceduralSkyboxProfile.singleScatteringAlbedo;
                densityVolumeFogDistance = m_selectedProceduralSkyboxProfile.densityVolumeFogDistance;
                fogDensityMaskTexture = m_selectedProceduralSkyboxProfile.fogDensityMaskTexture;
                densityMaskTiling = m_selectedProceduralSkyboxProfile.densityMaskTiling;
                densityScale = m_selectedProceduralSkyboxProfile.densityScale;
#if HDPipeline && UNITY_2018_3_OR_NEWER
                //Fog Mode
                fogColorMode = m_selectedProceduralSkyboxProfile.fogColorMode;
#endif
                //Sun Settings
                shadowStrength = m_selectedProceduralSkyboxProfile.shadowStrength;
                indirectLightMultiplier = m_selectedProceduralSkyboxProfile.indirectLightMultiplier;
                sunColor = m_selectedProceduralSkyboxProfile.proceduralSunColor;
                sunIntensity = m_selectedProceduralSkyboxProfile.proceduralSunIntensity;
            }
            else
            {
                //Fog mode
                fogType = m_selectedGradientSkyboxProfile.fogType;

                //Fog settings
                fogColor = m_selectedGradientSkyboxProfile.fogColor;
                fogDistance = m_selectedGradientSkyboxProfile.fogDistance;
                hDRPFogDistance = m_selectedGradientSkyboxProfile.hDRPFogDistance;
                nearFogDistance = m_selectedGradientSkyboxProfile.nearFogDistance;
                fogDensity = m_selectedGradientSkyboxProfile.fogDensity;

                //HD Pipeline Settings
                //Volumetric Fog
                useDistanceFog = m_selectedGradientSkyboxProfile.volumetricEnableDistanceFog;
                baseFogDistance = m_selectedGradientSkyboxProfile.volumetricBaseFogDistance;
                baseFogHeight = m_selectedGradientSkyboxProfile.volumetricBaseFogHeight;
                meanFogHeight = m_selectedGradientSkyboxProfile.volumetricMeanHeight;
                globalAnisotropy = m_selectedGradientSkyboxProfile.volumetricGlobalAnisotropy;
                globalLightProbeDimmer = m_selectedGradientSkyboxProfile.volumetricGlobalLightProbeDimmer;

                //Exponential Fog
                exponentialFogDensity = m_selectedGradientSkyboxProfile.exponentialFogDensity;
                exponentialBaseFogHeight = m_selectedGradientSkyboxProfile.exponentialBaseHeight;
                exponentialHeightAttenuation = m_selectedGradientSkyboxProfile.exponentialHeightAttenuation;
                exponentialMaxFogDistance = m_selectedGradientSkyboxProfile.exponentialMaxFogDistance;
                exponentialMipFogNear = m_selectedGradientSkyboxProfile.exponentialMipFogNear;
                exponentialMipFogFar = m_selectedGradientSkyboxProfile.exponentialMipFogFar;
                exponentialMipFogMax = m_selectedGradientSkyboxProfile.exponentialMipFogMaxMip;

                //Linear Fog
                linearFogDensity = m_selectedGradientSkyboxProfile.linearFogDensity;
                linearFogHeightStart = m_selectedGradientSkyboxProfile.linearHeightStart;
                linearFogHeightEnd = m_selectedGradientSkyboxProfile.linearHeightEnd;
                linearFogMaxDistance = m_selectedGradientSkyboxProfile.linearMaxFogDistance;
                linearMipFogNear = m_selectedGradientSkyboxProfile.linearMipFogNear;
                linearMipFogFar = m_selectedGradientSkyboxProfile.linearMipFogFar;
                linearMipFogMax = m_selectedGradientSkyboxProfile.linearMipFogMaxMip;

                //Volumetric Light Controller
                depthExtent = m_selectedGradientSkyboxProfile.volumetricDistanceRange;
                sliceDistribution = m_selectedGradientSkyboxProfile.volumetricSliceDistributionUniformity;

                //Density Fog Volume
                useDensityFogVolume = m_selectedGradientSkyboxProfile.useFogDensityVolume;
                singleScatteringAlbedo = m_selectedGradientSkyboxProfile.singleScatteringAlbedo;
                densityVolumeFogDistance = m_selectedGradientSkyboxProfile.densityVolumeFogDistance;
                fogDensityMaskTexture = m_selectedGradientSkyboxProfile.fogDensityMaskTexture;
                densityMaskTiling = m_selectedGradientSkyboxProfile.densityMaskTiling;
                densityScale = m_selectedGradientSkyboxProfile.densityScale;
#if HDPipeline && UNITY_2018_3_OR_NEWER
                //Fog Mode
                fogColorMode = m_selectedGradientSkyboxProfile.fogColorMode;
#endif
                //Sun Settings
                shadowStrength = m_selectedGradientSkyboxProfile.shadowStrength;
                indirectLightMultiplier = m_selectedGradientSkyboxProfile.indirectLightMultiplier;
                sunColor = m_selectedGradientSkyboxProfile.sunColor;
                sunIntensity = m_selectedGradientSkyboxProfile.sunIntensity;
            }

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

#if HDPipeline
            fogType = (AmbientSkiesConsts.VolumeFogType)m_editorUtils.EnumPopup("FogType", fogType, helpEnabled);
#else
            fogType = (AmbientSkiesConsts.VolumeFogType)m_editorUtils.EnumPopup("FogType", fogType, helpEnabled);               
            if (fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
            {
                EditorGUILayout.HelpBox("Volumetric Fog only works in High Definition Render Pipeline. Please select a different Fog Mode", MessageType.Warning);
            }
#endif

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline
                if (nearFogDistance < 0)
                {
                    nearFogDistance = 0f;
                }

                if (fogType == AmbientSkiesConsts.VolumeFogType.Exponential)
                {
#if !UNITY_2019_3_OR_NEWER
                    EditorGUILayout.Space();
                    m_editorUtils.Label("FogSettings");
                    EditorGUI.indentLevel++;
                    fogColorMode = (FogColorMode)m_editorUtils.EnumPopup("FogColorMode", fogColorMode, helpEnabled);
                    if (fogColorMode == FogColorMode.ConstantColor)
                    {
                        fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                    }
                    exponentialFogDensity = m_editorUtils.Slider("FogDensitySlider", exponentialFogDensity, 0f, 1f, helpEnabled);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    hDRPFogDistance = m_editorUtils.Slider("FogDistanceSlider", hDRPFogDistance, 100f, 10000f, helpEnabled, GUILayout.MaxWidth(2000f));
                    EditorGUILayout.EndVertical();
                    if (m_editorUtils.ButtonRight("UpdateToCameraDistance"))
                    {
                        hDRPFogDistance = AmbientSkiesEditorFunctions.UpdateFogToCamera(1.6f, mainCam);
                    }
                    EditorGUILayout.EndHorizontal();
                    exponentialBaseFogHeight = m_editorUtils.Slider("ExponentialBaseFogHeight", exponentialBaseFogHeight, 0f, 10000f, helpEnabled);
                    exponentialHeightAttenuation = m_editorUtils.Slider("ExponentialFogHeightAttenuation", exponentialHeightAttenuation, 0f, 1f, helpEnabled);
                    exponentialMaxFogDistance = m_editorUtils.Slider("ExponentialMaxFogDistance", exponentialMaxFogDistance, 0f, 20000f, helpEnabled);
                    if (fogColorMode == FogColorMode.SkyColor)
                    {
                        exponentialMipFogNear = m_editorUtils.Slider("ExponentialMipFogNear", exponentialMipFogNear, 0f, 1000f, helpEnabled);
                        exponentialMipFogFar = m_editorUtils.Slider("ExponentialMipFogFar", exponentialMipFogFar, 0f, 2500f, helpEnabled);
                        exponentialMipFogMax = m_editorUtils.Slider("ExponentialMipFogMax", exponentialMipFogMax, 0f, 1f, helpEnabled);
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    exponentialBaseFogHeight = m_editorUtils.Slider("ExponentialBaseFogHeight", exponentialBaseFogHeight, 0f, 10000f, helpEnabled, GUILayout.MaxWidth(2000f));
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    if (m_editorUtils.ButtonRight("UpdateFogHeightToSceneV2"))
                    {
                        ReconfigureHDRPFog(m_profiles, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile);
                    }
                    EditorGUILayout.EndHorizontal();
#else
                    EditorGUILayout.HelpBox("Exponential is no longer supported in HDRP please use Volumetric mode instead.", MessageType.Info);
#endif
                }
                else if (fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                {
#if !UNITY_2019_3_OR_NEWER
                    fogType = AmbientSkiesConsts.VolumeFogType.Exponential;
                    m_selectedProceduralSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.Exponential;
                    m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.Exponential;
                    if (m_profiles.m_showDebug)
                    {
                        Debug.Log("Exponetial Squared fog is not supported in HDRP. Switching Fog Mode to Exponential");
                    }
#else
                    EditorGUILayout.HelpBox("Exponential Squared is no longer supported in HDRP please use Volumetric mode instead.", MessageType.Info);
#endif
                }
                else if (fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                {
#if !UNITY_2019_3_OR_NEWER
                    EditorGUILayout.Space();
                    m_editorUtils.Label("FogSettings");
                    EditorGUI.indentLevel++;
                    fogColorMode = (FogColorMode)m_editorUtils.EnumPopup("FogColorMode", fogColorMode, helpEnabled);
                    if (fogColorMode == FogColorMode.ConstantColor)
                    {
                        fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                    }
                    linearFogDensity = m_editorUtils.Slider("FogDensitySlider", linearFogDensity, 0f, 1f, helpEnabled);
                    nearFogDistance = m_editorUtils.Slider("NearFogDistanceSlider", nearFogDistance, 0f, 1000f, helpEnabled);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    hDRPFogDistance = m_editorUtils.Slider("FogDistanceSlider", hDRPFogDistance, 100f, 10000f, helpEnabled, GUILayout.MaxWidth(2000f));
                    EditorGUILayout.EndVertical();
                    if (m_editorUtils.ButtonRight("UpdateToCameraDistance"))
                    {
                        hDRPFogDistance = AmbientSkiesEditorFunctions.UpdateFogToCamera(1.6f, mainCam);
                    }
                    EditorGUILayout.EndHorizontal();
                    linearFogHeightStart = m_editorUtils.Slider("LinearFogHeightStart", linearFogHeightStart, 0f, 1250f, helpEnabled);
                    linearFogHeightEnd = m_editorUtils.Slider("LinearFogHeightEnd", linearFogHeightEnd, 0f, 10000f, helpEnabled);
                    linearFogMaxDistance = m_editorUtils.Slider("LinearFogMaxDistance", linearFogMaxDistance, 0f, 20000f, helpEnabled);
                    if (fogColorMode == FogColorMode.SkyColor)
                    {
                        linearMipFogNear = m_editorUtils.Slider("LinearMipFogNear", linearMipFogNear, 0f, 1000f, helpEnabled);
                        linearMipFogFar = m_editorUtils.Slider("LinearMipFogFar", linearMipFogFar, 0f, 2000f, helpEnabled);
                        linearMipFogMax = m_editorUtils.Slider("LinearMipFogMax", linearMipFogMax, 0f, 1f, helpEnabled);
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    linearFogHeightStart = m_editorUtils.Slider("LinearFogHeightStart", linearFogHeightStart, 0f, 1250f, helpEnabled, GUILayout.MaxWidth(2000f));
                    linearFogHeightEnd = m_editorUtils.Slider("LinearFogHeightEnd", linearFogHeightEnd, 0f, 10000f, helpEnabled, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    if (m_editorUtils.ButtonRight("UpdateFogHeightToScene"))
                    {
                        ReconfigureHDRPFog(m_profiles, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile);
                    }
                    EditorGUILayout.EndHorizontal();
#else
                    EditorGUILayout.HelpBox("Linear is no longer supported in HDRP please use Volumetric mode instead.", MessageType.Info);
#endif
                }
                else if (fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                {
                    EditorGUILayout.Space();
                    m_editorUtils.Label("FogSettings");
                    EditorGUI.indentLevel++;
                    if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
                    {                     
                        sliceDistribution = m_editorUtils.Slider("VolumetricSliceDistribution", sliceDistribution, 0f, 1f, helpEnabled);
                    }
                    else
                    {
                        fogColorMode = (FogColorMode)m_editorUtils.EnumPopup("FogColorMode", fogColorMode, helpEnabled);
                        useDistanceFog = m_editorUtils.Toggle("UseDistanceFog", useDistanceFog, helpEnabled);
                        if (useDistanceFog)
                        {
                            baseFogDistance = m_editorUtils.Slider("VolumetricBaseFogDistance", baseFogDistance, 0f, 10000f, helpEnabled);
                        }
                        if (fogColorMode == FogColorMode.ConstantColor)
                        {
                            fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                        }

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        hDRPFogDistance = m_editorUtils.Slider("FogDistanceSlider", hDRPFogDistance, 100f, 10000f, helpEnabled, GUILayout.MaxWidth(2000f));
                        EditorGUILayout.EndVertical();
                        if (m_editorUtils.ButtonRight("UpdateToCameraDistance"))
                        {
                            hDRPFogDistance = AmbientSkiesEditorFunctions.UpdateFogToCamera(1.6f, mainCam);
                        }
                        EditorGUILayout.EndHorizontal();
                        globalAnisotropy = m_editorUtils.Slider("VolumetricGlobalAnisotropy", globalAnisotropy, -1f, 1f, helpEnabled);
                        globalLightProbeDimmer = m_editorUtils.Slider("VolumetricGlobalLightProbeDimmer", globalLightProbeDimmer, 0f, 1f, helpEnabled);
                        depthExtent = m_editorUtils.Slider("VolumetricLightDepthDistance", depthExtent, 0.1f, 5000f, helpEnabled);
                        sliceDistribution = m_editorUtils.Slider("VolumetricSliceDistribution", sliceDistribution, 0f, 1f, helpEnabled);
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    baseFogHeight = m_editorUtils.Slider("VolumetricBaseFogHeight", baseFogHeight, -250f, 2500f, helpEnabled, GUILayout.MaxWidth(2000f));
                    meanFogHeight = m_editorUtils.Slider("VolumetricMeanHeight", meanFogHeight, 1f, 6000f, helpEnabled, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    if (m_editorUtils.ButtonRight("UpdateFogHeightToScene"))
                    {
                        ReconfigureHDRPFog(m_profiles, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();

                    if (meanFogHeight >= 400f && meanFogHeight < 700f)
                    {
                        EditorGUILayout.HelpBox("Your 'Mean Height' is set quite high. This will block a lot of sun light intensity in your scene. Increase your sun intensity by about 0.4 - 2.0 to add more light to your scene.", MessageType.Warning);
                    }
                    else if (meanFogHeight >= 700f && meanFogHeight < 1100f)
                    {
                        EditorGUILayout.HelpBox("Your 'Mean Height' is set quite high. This will block a lot of sun light intensity in your scene. Increase your sun intensity by about 2.0 - 4.4 to add more light to your scene.", MessageType.Warning);
                    }
                    else if (meanFogHeight >= 1100f)
                    {
                        EditorGUILayout.HelpBox("Your 'Mean Height' is set quite high. This will block a lot of sun light intensity in your scene. Increase your sun intensity by about 4.5 - 8.0 to add more light to your scene.", MessageType.Warning);
                    }

                    m_editorUtils.Label("DensityFogSettings");
                    EditorGUI.indentLevel++;
                    useDensityFogVolume = m_editorUtils.ToggleLeft("UseDensityFogVolume", useDensityFogVolume, helpEnabled);
                    if (useDensityFogVolume)
                    {
                        singleScatteringAlbedo = m_editorUtils.ColorField("DensityFogScatteringAlbedo", singleScatteringAlbedo, helpEnabled);
                        densityVolumeFogDistance = m_editorUtils.Slider("DensityVolumeFogDistance", densityVolumeFogDistance, 0.1f, 1000f, helpEnabled);
                        fogDensityMaskTexture = (Texture3D)m_editorUtils.ObjectField("FogDensityMaskTexture", fogDensityMaskTexture, typeof(Texture3D), false, helpEnabled, GUILayout.Height(16f));
                        densityMaskTiling = m_editorUtils.Vector3Field("DensityFogMaskTiling", densityMaskTiling, helpEnabled);
                        densityScale = m_editorUtils.Vector3Field("DesnityFogScale", densityScale, helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    m_editorUtils.Text("No fog mode selected. To enable fog select a fog mode in the Main Settings.");
                }
#endif
            }
            else
            {
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    if (fogType == AmbientSkiesConsts.VolumeFogType.Exponential || fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                    {
                        fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                        fogDensity = m_editorUtils.Slider("FogDensitySlider", fogDensity, 0f, 0.1f, helpEnabled);
                    }
                    else if (fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                        nearFogDistance = m_editorUtils.Slider("NearFogDistanceSlider", nearFogDistance, 0f - fogDistance, fogDistance - 1f, helpEnabled);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        fogDistance = m_editorUtils.Slider("FogDistanceSlider", fogDistance, 100f, 10000f, helpEnabled, GUILayout.MaxWidth(2000f));
                        if (nearFogDistance > fogDistance)
                        {
                            nearFogDistance = fogDistance - 1f;
                        }
                        EditorGUILayout.EndVertical();
                        if (m_editorUtils.ButtonRight("UpdateToCameraDistance"))
                        {
                            fogDistance = AmbientSkiesEditorFunctions.UpdateFogToCamera(1.6f, mainCam);
                        }
                        EditorGUILayout.EndHorizontal();

                        m_editorUtils.InlineHelp("FogDistanceHelp", helpEnabled);
                    }
                    else if (fogType == AmbientSkiesConsts.VolumeFogType.None)
                    {
                        m_editorUtils.Text("No fog mode selected. To enable fog select a fog mode in the Main Settings.");
                    }
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    if (fogType == AmbientSkiesConsts.VolumeFogType.Exponential)
                    {
                        fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                        fogDensity = m_editorUtils.Slider("FogDensitySlider", fogDensity, 0f, 0.1f, helpEnabled);
                    }
                    else if (fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                    {
                        fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                        fogDensity = m_editorUtils.Slider("FogDensitySlider", fogDensity, 0f, 0.1f, helpEnabled);
                    }
                    else if (fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                        nearFogDistance = m_editorUtils.Slider("NearFogDistanceSlider", nearFogDistance, 0f - fogDistance, fogDistance - 1f, helpEnabled);
                        if (nearFogDistance > fogDistance)
                        {
                            nearFogDistance = fogDistance - 1f;
                        }
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        fogDistance = m_editorUtils.Slider("FogDistanceSlider", fogDistance, 100f, 10000f, helpEnabled, GUILayout.MaxWidth(2000f));
                        EditorGUILayout.EndVertical();
                        if (m_editorUtils.ButtonRight("UpdateToCameraDistance"))
                        {
                            fogDistance = AmbientSkiesEditorFunctions.UpdateFogToCamera(1.6f, mainCam);
                        }
                        EditorGUILayout.EndHorizontal();

                        m_editorUtils.InlineHelp("FogDistanceHelp", helpEnabled);
                    }
                    else if (fogType == AmbientSkiesConsts.VolumeFogType.None)
                    {
                        m_editorUtils.Text("No fog mode selected. To enable fog select a fog mode in the Main Settings.");
                    }
                }
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateFog = true;
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    m_profiles.m_updateSun = true;
                }

                //Time of day
                m_profiles.m_useTimeOfDay = useTimeOfDay;

                //Configuration type
                m_profiles.m_configurationType = configurationType;

                //Settings
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    //Fog mode
                    if (m_selectedSkyboxProfile.fogType != fogType && renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
                        m_profiles.m_updateVisualEnvironment = true;
                    }
                    m_selectedSkyboxProfile.fogType = fogType;

                    //Fog settings
                    m_selectedSkyboxProfile.fogColor = fogColor;
                    m_selectedSkyboxProfile.fogDistance = fogDistance;
                    m_selectedSkyboxProfile.hDRPFogDistance = hDRPFogDistance;
                    m_selectedSkyboxProfile.nearFogDistance = nearFogDistance;
                    m_selectedSkyboxProfile.fogDensity = fogDensity;

                    //HD Pipeline Settings
                    //Volumetric Fog
                    m_selectedSkyboxProfile.volumetricEnableDistanceFog = useDistanceFog;
                    m_selectedSkyboxProfile.volumetricBaseFogDistance = baseFogDistance;
                    m_selectedSkyboxProfile.volumetricBaseFogHeight = baseFogHeight;
                    m_selectedSkyboxProfile.volumetricMeanHeight = meanFogHeight;
                    m_selectedSkyboxProfile.volumetricGlobalAnisotropy = globalAnisotropy;
                    m_selectedSkyboxProfile.volumetricGlobalLightProbeDimmer = globalLightProbeDimmer;

                    //Exponential Fog
                    m_selectedSkyboxProfile.exponentialFogDensity = exponentialFogDensity;
                    m_selectedSkyboxProfile.exponentialBaseHeight = exponentialBaseFogHeight;
                    m_selectedSkyboxProfile.exponentialHeightAttenuation = exponentialHeightAttenuation;
                    m_selectedSkyboxProfile.exponentialMaxFogDistance = exponentialMaxFogDistance;
                    m_selectedSkyboxProfile.exponentialMipFogNear = exponentialMipFogNear;
                    m_selectedSkyboxProfile.exponentialMipFogFar = exponentialMipFogFar;
                    m_selectedSkyboxProfile.exponentialMipFogMaxMip = exponentialMipFogMax;

                    //Linear Fog
                    m_selectedSkyboxProfile.linearFogDensity = linearFogDensity;
                    m_selectedSkyboxProfile.linearHeightStart = linearFogHeightStart;
                    m_selectedSkyboxProfile.linearHeightEnd = linearFogHeightEnd;
                    m_selectedSkyboxProfile.linearMaxFogDistance = linearFogMaxDistance;
                    m_selectedSkyboxProfile.linearMipFogNear = linearMipFogNear;
                    m_selectedSkyboxProfile.linearMipFogFar = linearMipFogFar;
                    m_selectedSkyboxProfile.linearMipFogMaxMip = linearMipFogMax;

                    //Volumetric Light Controller
                    m_selectedSkyboxProfile.volumetricDistanceRange = depthExtent;
                    m_selectedSkyboxProfile.volumetricSliceDistributionUniformity = sliceDistribution;

                    //Density Fog Volume
                    m_selectedSkyboxProfile.useFogDensityVolume = useDensityFogVolume;
                    m_selectedSkyboxProfile.singleScatteringAlbedo = singleScatteringAlbedo;
                    m_selectedSkyboxProfile.densityVolumeFogDistance = densityVolumeFogDistance;
                    m_selectedSkyboxProfile.fogDensityMaskTexture = fogDensityMaskTexture;
                    m_selectedSkyboxProfile.densityMaskTiling = densityMaskTiling;
                    m_selectedSkyboxProfile.densityScale = densityScale;
#if HDPipeline && UNITY_2018_3_OR_NEWER
                    //Fog Mode
                    m_selectedSkyboxProfile.fogColorMode = fogColorMode;

#endif
                    //Sun Settings
                    m_selectedSkyboxProfile.shadowStrength = shadowStrength;
                    m_selectedSkyboxProfile.indirectLightMultiplier = indirectLightMultiplier;
                    m_selectedSkyboxProfile.sunColor = sunColor;
                    m_selectedSkyboxProfile.sunIntensity = sunIntensity;
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //Fog mode
                    if (m_selectedProceduralSkyboxProfile.fogType != fogType && renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
                        m_profiles.m_updateVisualEnvironment = true;
                    }

                    m_selectedProceduralSkyboxProfile.fogType = fogType;

                    //Fog settings
                    m_selectedProceduralSkyboxProfile.proceduralFogColor = fogColor;
                    m_selectedProceduralSkyboxProfile.proceduralFogDistance = fogDistance;
                    m_selectedProceduralSkyboxProfile.hDRPFogDistance = hDRPFogDistance;
                    m_selectedProceduralSkyboxProfile.proceduralNearFogDistance = nearFogDistance;
                    m_selectedProceduralSkyboxProfile.proceduralFogDensity = fogDensity;

                    //HD Pipeline Settings
                    //Volumetric Fog
                    m_selectedProceduralSkyboxProfile.volumetricEnableDistanceFog = useDistanceFog;
                    m_selectedProceduralSkyboxProfile.volumetricBaseFogDistance = baseFogDistance;
                    m_selectedProceduralSkyboxProfile.volumetricBaseFogHeight = baseFogHeight;
                    m_selectedProceduralSkyboxProfile.volumetricMeanHeight = meanFogHeight;
                    m_selectedProceduralSkyboxProfile.volumetricGlobalAnisotropy = globalAnisotropy;
                    m_selectedProceduralSkyboxProfile.volumetricGlobalLightProbeDimmer = globalLightProbeDimmer;

                    //Exponential Fog
                    m_selectedProceduralSkyboxProfile.exponentialFogDensity = exponentialFogDensity;
                    m_selectedProceduralSkyboxProfile.exponentialBaseHeight = exponentialBaseFogHeight;
                    m_selectedProceduralSkyboxProfile.exponentialHeightAttenuation = exponentialHeightAttenuation;
                    m_selectedProceduralSkyboxProfile.exponentialMaxFogDistance = exponentialMaxFogDistance;
                    m_selectedProceduralSkyboxProfile.exponentialMipFogNear = exponentialMipFogNear;
                    m_selectedProceduralSkyboxProfile.exponentialMipFogFar = exponentialMipFogFar;
                    m_selectedProceduralSkyboxProfile.exponentialMipFogMaxMip = exponentialMipFogMax;

                    //Linear Fog
                    m_selectedProceduralSkyboxProfile.linearFogDensity = linearFogDensity;
                    m_selectedProceduralSkyboxProfile.linearHeightStart = linearFogHeightStart;
                    m_selectedProceduralSkyboxProfile.linearHeightEnd = linearFogHeightEnd;
                    m_selectedProceduralSkyboxProfile.linearMaxFogDistance = linearFogMaxDistance;
                    m_selectedProceduralSkyboxProfile.linearMipFogNear = linearMipFogNear;
                    m_selectedProceduralSkyboxProfile.linearMipFogFar = linearMipFogFar;
                    m_selectedProceduralSkyboxProfile.linearMipFogMaxMip = linearMipFogMax;

                    //Volumetric Light Controller
                    m_selectedProceduralSkyboxProfile.volumetricDistanceRange = depthExtent;
                    m_selectedProceduralSkyboxProfile.volumetricSliceDistributionUniformity = sliceDistribution;

                    //Density Fog Volume
                    m_selectedProceduralSkyboxProfile.useFogDensityVolume = useDensityFogVolume;
                    m_selectedProceduralSkyboxProfile.singleScatteringAlbedo = singleScatteringAlbedo;
                    m_selectedProceduralSkyboxProfile.densityVolumeFogDistance = densityVolumeFogDistance;
                    m_selectedProceduralSkyboxProfile.fogDensityMaskTexture = fogDensityMaskTexture;
                    m_selectedProceduralSkyboxProfile.densityMaskTiling = densityMaskTiling;
                    m_selectedProceduralSkyboxProfile.densityScale = densityScale;
#if HDPipeline && UNITY_2018_3_OR_NEWER
                    //Fog Mode
                    m_selectedProceduralSkyboxProfile.fogColorMode = fogColorMode;
#endif
                    //Sun Settings
                    m_selectedProceduralSkyboxProfile.enableSunDisk = enableSunDisk;
                    m_selectedProceduralSkyboxProfile.includeSunInBaking = includeSunInBaking;
                    m_selectedProceduralSkyboxProfile.shadowStrength = shadowStrength;
                    m_selectedProceduralSkyboxProfile.indirectLightMultiplier = indirectLightMultiplier;
                    m_selectedProceduralSkyboxProfile.proceduralSunColor = sunColor;
                    m_selectedProceduralSkyboxProfile.proceduralSunIntensity = sunIntensity;
                }
                else
                {
                    //Fog mode
                    if (m_selectedGradientSkyboxProfile.fogType != fogType && renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
                        m_profiles.m_updateVisualEnvironment = true;
                    }

                    m_selectedGradientSkyboxProfile.fogType = fogType;

                    //Fog settings
                    m_selectedGradientSkyboxProfile.fogColor = fogColor;
                    m_selectedGradientSkyboxProfile.fogDistance = fogDistance;
                    m_selectedGradientSkyboxProfile.hDRPFogDistance = hDRPFogDistance;
                    m_selectedGradientSkyboxProfile.nearFogDistance = nearFogDistance;
                    m_selectedGradientSkyboxProfile.fogDensity = fogDensity;

                    //HD Pipeline Settings
                    //Volumetric Fog
                    m_selectedGradientSkyboxProfile.volumetricEnableDistanceFog = useDistanceFog;
                    m_selectedGradientSkyboxProfile.volumetricBaseFogDistance = baseFogDistance;
                    m_selectedGradientSkyboxProfile.volumetricBaseFogHeight = baseFogHeight;
                    m_selectedGradientSkyboxProfile.volumetricMeanHeight = meanFogHeight;
                    m_selectedGradientSkyboxProfile.volumetricGlobalAnisotropy = globalAnisotropy;
                    m_selectedGradientSkyboxProfile.volumetricGlobalLightProbeDimmer = globalLightProbeDimmer;

                    //Exponential Fog
                    m_selectedGradientSkyboxProfile.exponentialFogDensity = exponentialFogDensity;
                    m_selectedGradientSkyboxProfile.exponentialBaseHeight = exponentialBaseFogHeight;
                    m_selectedGradientSkyboxProfile.exponentialHeightAttenuation = exponentialHeightAttenuation;
                    m_selectedGradientSkyboxProfile.exponentialMaxFogDistance = exponentialMaxFogDistance;
                    m_selectedGradientSkyboxProfile.exponentialMipFogNear = exponentialMipFogNear;
                    m_selectedGradientSkyboxProfile.exponentialMipFogFar = exponentialMipFogFar;
                    m_selectedGradientSkyboxProfile.exponentialMipFogMaxMip = exponentialMipFogMax;

                    //Linear Fog
                    m_selectedGradientSkyboxProfile.linearFogDensity = linearFogDensity;
                    m_selectedGradientSkyboxProfile.linearHeightStart = linearFogHeightStart;
                    m_selectedGradientSkyboxProfile.linearHeightEnd = linearFogHeightEnd;
                    m_selectedGradientSkyboxProfile.linearMaxFogDistance = linearFogMaxDistance;
                    m_selectedGradientSkyboxProfile.linearMipFogNear = linearMipFogNear;
                    m_selectedGradientSkyboxProfile.linearMipFogFar = linearMipFogFar;
                    m_selectedGradientSkyboxProfile.linearMipFogMaxMip = linearMipFogMax;

                    //Volumetric Light Controller
                    m_selectedGradientSkyboxProfile.volumetricDistanceRange = depthExtent;
                    m_selectedGradientSkyboxProfile.volumetricSliceDistributionUniformity = sliceDistribution;

                    //Density Fog Volume
                    m_selectedGradientSkyboxProfile.useFogDensityVolume = useDensityFogVolume;
                    m_selectedGradientSkyboxProfile.singleScatteringAlbedo = singleScatteringAlbedo;
                    m_selectedGradientSkyboxProfile.densityVolumeFogDistance = densityVolumeFogDistance;
                    m_selectedGradientSkyboxProfile.fogDensityMaskTexture = fogDensityMaskTexture;
                    m_selectedGradientSkyboxProfile.densityMaskTiling = densityMaskTiling;
                    m_selectedGradientSkyboxProfile.densityScale = densityScale;
#if HDPipeline && UNITY_2018_3_OR_NEWER
                    //Fog Mode
                    m_selectedGradientSkyboxProfile.fogColorMode = fogColorMode;
#endif
                    //Sun Settings
                    m_selectedGradientSkyboxProfile.shadowStrength = shadowStrength;
                    m_selectedGradientSkyboxProfile.indirectLightMultiplier = indirectLightMultiplier;
                    m_selectedGradientSkyboxProfile.sunColor = sunColor;
                    m_selectedGradientSkyboxProfile.sunIntensity = sunIntensity;
                }

                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Ambient settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void AmbientSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Time of day
            useTimeOfDay = m_profiles.m_useTimeOfDay;

            //Settings
            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                //Ambient mode
                ambientMode = m_selectedSkyboxProfile.ambientMode;

                //Ambient Settings
                skyColor = m_selectedSkyboxProfile.skyColor;
                equatorColor = m_selectedSkyboxProfile.equatorColor;
                groundColor = m_selectedSkyboxProfile.groundColor;
                lwrpSkyColor = m_selectedSkyboxProfile.lwrpSkyColor;
                lwrpEquatorColor = m_selectedSkyboxProfile.lwrpEquatorColor;
                lwrpGroundColor = m_selectedSkyboxProfile.lwrpGroundColor;
                skyboxGroundIntensity = m_selectedSkyboxProfile.skyboxGroundIntensity;

                //HD Pipeline Settings
                hdrpAmbientMode = m_selectedSkyboxProfile.hdrpAmbientMode;
#if HDPipeline
                //Ambient Lighting
                useStaticLighting = m_selectedSkyboxProfile.useBakingSky;
                diffuseAmbientIntensity = m_selectedSkyboxProfile.indirectDiffuseIntensity;
                specularAmbientIntensity = m_selectedSkyboxProfile.indirectSpecularIntensity;
#endif
            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                //Ambient mode
                ambientMode = m_selectedProceduralSkyboxProfile.ambientMode;

                //Ambient Settings
                skyColor = m_selectedProceduralSkyboxProfile.skyColor;
                equatorColor = m_selectedProceduralSkyboxProfile.equatorColor;
                groundColor = m_selectedProceduralSkyboxProfile.groundColor;
                lwrpSkyColor = m_selectedProceduralSkyboxProfile.lwrpSkyColor;
                lwrpEquatorColor = m_selectedProceduralSkyboxProfile.lwrpEquatorColor;
                lwrpGroundColor = m_selectedProceduralSkyboxProfile.lwrpGroundColor;
                skyboxGroundIntensity = m_selectedProceduralSkyboxProfile.skyboxGroundIntensity;

                //HD Pipeline Settings
                hdrpAmbientMode = m_selectedProceduralSkyboxProfile.hdrpAmbientMode;
#if HDPipeline
                //Ambient Lighting
                useStaticLighting = m_selectedProceduralSkyboxProfile.useBakingSky;
                diffuseAmbientIntensity = m_selectedProceduralSkyboxProfile.indirectDiffuseIntensity;
                specularAmbientIntensity = m_selectedProceduralSkyboxProfile.indirectSpecularIntensity;
#endif
            }
            else
            {
                //Ambient mode
                ambientMode = m_selectedGradientSkyboxProfile.ambientMode;

                //Ambient Settings
                skyColor = m_selectedGradientSkyboxProfile.skyColor;
                equatorColor = m_selectedGradientSkyboxProfile.equatorColor;
                groundColor = m_selectedGradientSkyboxProfile.groundColor;
                skyboxGroundIntensity = m_selectedGradientSkyboxProfile.skyboxGroundIntensity;

                //HD Pipeline Settings
                hdrpAmbientMode = m_selectedGradientSkyboxProfile.hdrpAmbientMode;
#if HDPipeline
                //Ambient Lighting
                useStaticLighting = m_selectedGradientSkyboxProfile.useBakingSky;
                diffuseAmbientIntensity = m_selectedGradientSkyboxProfile.indirectDiffuseIntensity;
                specularAmbientIntensity = m_selectedGradientSkyboxProfile.indirectSpecularIntensity;
#endif
            }

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                hdrpAmbientMode = (AmbientSkiesConsts.HDRPAmbientMode)m_editorUtils.EnumPopup("HDRPAmbientMode", hdrpAmbientMode, helpEnabled);
                //diffuseAmbientIntensity = m_editorUtils.Slider("DiffuseSkyboxGroundIntensitySlider", diffuseAmbientIntensity, 0f, 10f, helpEnabled);
                //specularAmbientIntensity = m_editorUtils.Slider("SpecularSkyboxGroundIntensitySlider", specularAmbientIntensity, 0f, 10f, helpEnabled);
            }
            else
            {
                ambientMode = (AmbientSkiesConsts.AmbientMode)m_editorUtils.EnumPopup("AmbientMode", ambientMode, helpEnabled);
                if (ambientMode == AmbientSkiesConsts.AmbientMode.Color)
                {
                    if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                    {
                        skyColor = EditorGUILayout.ColorField(new GUIContent(m_editorUtils.GetTextValue("SkyColor")), skyColor, false, false, true);
                        m_editorUtils.InlineHelp("SkyColor", helpEnabled);
                    }
                    else
                    {
                        lwrpSkyColor = EditorGUILayout.ColorField(new GUIContent(m_editorUtils.GetTextValue("SkyColor")), lwrpSkyColor, false, false, true);
                        m_editorUtils.InlineHelp("SkyColor", helpEnabled);
                    }

                }
                else if (ambientMode == AmbientSkiesConsts.AmbientMode.Gradient)
                {
                    if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                    {
                        skyColor = EditorGUILayout.ColorField(new GUIContent(m_editorUtils.GetTextValue("SkyColor")), skyColor, false, false, true);
                        m_editorUtils.InlineHelp("SkyColor", helpEnabled);
                        equatorColor = EditorGUILayout.ColorField(new GUIContent(m_editorUtils.GetTextValue("EquatorColor")), equatorColor, false, false, true);
                        m_editorUtils.InlineHelp("EquatorColor", helpEnabled);
                        groundColor = EditorGUILayout.ColorField(new GUIContent(m_editorUtils.GetTextValue("GroundColor")), groundColor, false, false, true);
                        m_editorUtils.InlineHelp("GroundColor", helpEnabled);
                    }
                    else
                    {
                        lwrpSkyColor = EditorGUILayout.ColorField(new GUIContent(m_editorUtils.GetTextValue("SkyColor")), lwrpSkyColor, false, false, true);
                        m_editorUtils.InlineHelp("SkyColor", helpEnabled);
                        lwrpEquatorColor = EditorGUILayout.ColorField(new GUIContent(m_editorUtils.GetTextValue("EquatorColor")), lwrpEquatorColor, false, false, true);
                        m_editorUtils.InlineHelp("EquatorColor", helpEnabled);
                        lwrpGroundColor = EditorGUILayout.ColorField(new GUIContent(m_editorUtils.GetTextValue("GroundColor")), lwrpGroundColor, false, false, true);
                        m_editorUtils.InlineHelp("GroundColor", helpEnabled);
                    }

                }
                else
                {
                    if (Lightmapping.lightingDataAsset == null)
                    {
                        EditorGUILayout.HelpBox("Light data is missing. To view ambient in Skybox Mode please bake your lighting. You can bake your lighting in the Lighting tab under main settings 'Bake Global Lighting (Slow)'.", MessageType.Info);
                    }

                    skyboxGroundIntensity = m_editorUtils.Slider("SkyboxGroundIntensitySlider", skyboxGroundIntensity, 0.01f, 8f, helpEnabled);
                }
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateAmbientLight = true;

                //Time of day
                m_profiles.m_useTimeOfDay = useTimeOfDay;

                //Settings
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    //Ambient mode
                    m_selectedSkyboxProfile.ambientMode = ambientMode;

                    //Ambient Settings
                    m_selectedSkyboxProfile.skyColor = skyColor;
                    m_selectedSkyboxProfile.equatorColor = equatorColor;
                    m_selectedSkyboxProfile.groundColor = groundColor;
                    m_selectedSkyboxProfile.lwrpSkyColor = lwrpSkyColor;
                    m_selectedSkyboxProfile.lwrpEquatorColor = lwrpEquatorColor;
                    m_selectedSkyboxProfile.lwrpGroundColor = lwrpGroundColor;
                    m_selectedSkyboxProfile.skyboxGroundIntensity = skyboxGroundIntensity;

                    //HD Pipeline Settings
                    m_selectedSkyboxProfile.hdrpAmbientMode = hdrpAmbientMode;
#if HDPipeline
                    //Ambient Lighting
                    m_selectedSkyboxProfile.useBakingSky = useStaticLighting;
                    m_selectedSkyboxProfile.indirectDiffuseIntensity = diffuseAmbientIntensity;
                    m_selectedSkyboxProfile.indirectSpecularIntensity = specularAmbientIntensity;
#endif

                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //Ambient mode
                    m_selectedProceduralSkyboxProfile.ambientMode = ambientMode;                 

                    //Ambient Settings
                    m_selectedProceduralSkyboxProfile.skyColor = skyColor;
                    m_selectedProceduralSkyboxProfile.equatorColor = equatorColor;
                    m_selectedProceduralSkyboxProfile.groundColor = groundColor;
                    m_selectedProceduralSkyboxProfile.lwrpSkyColor = lwrpSkyColor;
                    m_selectedProceduralSkyboxProfile.lwrpEquatorColor = lwrpEquatorColor;
                    m_selectedProceduralSkyboxProfile.lwrpGroundColor = lwrpGroundColor;
                    m_selectedProceduralSkyboxProfile.skyboxGroundIntensity = skyboxGroundIntensity;

                    //HD Pipeline Settings
                    m_selectedProceduralSkyboxProfile.hdrpAmbientMode = hdrpAmbientMode;
#if HDPipeline
                    //Ambient Lighting
                    m_selectedProceduralSkyboxProfile.useBakingSky = useStaticLighting;
                    m_selectedProceduralSkyboxProfile.indirectDiffuseIntensity = diffuseAmbientIntensity;
                    m_selectedProceduralSkyboxProfile.indirectSpecularIntensity = specularAmbientIntensity;
#endif

                }
                else
                {
                    //Ambient mode
                    m_selectedGradientSkyboxProfile.ambientMode = ambientMode;

                    //Ambient Settings
                    m_selectedGradientSkyboxProfile.skyColor = skyColor;
                    m_selectedGradientSkyboxProfile.equatorColor = equatorColor;
                    m_selectedGradientSkyboxProfile.groundColor = groundColor;
                    m_selectedGradientSkyboxProfile.skyboxGroundIntensity = skyboxGroundIntensity;

                    //HD Pipeline Settings
                    m_selectedGradientSkyboxProfile.hdrpAmbientMode = hdrpAmbientMode;
#if HDPipeline
                    //Ambient Lighting
                    m_selectedGradientSkyboxProfile.useBakingSky = useStaticLighting;
                    m_selectedGradientSkyboxProfile.indirectDiffuseIntensity = diffuseAmbientIntensity;
                    m_selectedGradientSkyboxProfile.indirectSpecularIntensity = specularAmbientIntensity;
#endif
                }

                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Sun settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void SunSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Time of day
            useTimeOfDay = m_profiles.m_useTimeOfDay;

            if (serializedObject == null)
            {
                serializedObject = new SerializedObject(this);
            }

            if (temperature == null)
            {
                temperature = serializedObject.FindProperty("currentTemperature");
            }

            //Settings
            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                //Sun Settings
                shadowStrength = m_selectedSkyboxProfile.shadowStrength;
                indirectLightMultiplier = m_selectedSkyboxProfile.indirectLightMultiplier;
                sunColor = m_selectedSkyboxProfile.sunColor;
                sunIntensity = m_selectedSkyboxProfile.sunIntensity;
                sunLWRPIntensity = m_selectedSkyboxProfile.LWRPSunIntensity;
                sunHDRPIntensity = m_selectedSkyboxProfile.HDRPSunIntensity;
                useTemperature = m_selectedSkyboxProfile.useTempature;
                currentTemperature = m_selectedSkyboxProfile.temperature;
            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                //Sun Settings
                enableSunDisk = m_selectedProceduralSkyboxProfile.enableSunDisk;
                includeSunInBaking = m_selectedProceduralSkyboxProfile.includeSunInBaking;
                shadowStrength = m_selectedProceduralSkyboxProfile.shadowStrength;
                indirectLightMultiplier = m_selectedProceduralSkyboxProfile.indirectLightMultiplier;
                sunColor = m_selectedProceduralSkyboxProfile.proceduralSunColor;
                sunIntensity = m_selectedProceduralSkyboxProfile.proceduralSunIntensity;
                sunLWRPIntensity = m_selectedProceduralSkyboxProfile.proceduralLWRPSunIntensity;
                sunHDRPIntensity = m_selectedProceduralSkyboxProfile.proceduralHDRPSunIntensity;
                proceduralSunSize = m_selectedProceduralSkyboxProfile.proceduralSunSize;
                hdrpProceduralSunSize = m_selectedProceduralSkyboxProfile.hdrpProceduralSunSize;
                proceduralSunSizeConvergence = m_selectedProceduralSkyboxProfile.proceduralSunSizeConvergence;
                hdrpProceduralSunSizeConvergence = m_selectedProceduralSkyboxProfile.hdrpProceduralSunSizeConvergence;
            }
            else
            {
                //Sun Settings
                shadowStrength = m_selectedGradientSkyboxProfile.shadowStrength;
                indirectLightMultiplier = m_selectedGradientSkyboxProfile.indirectLightMultiplier;
                sunColor = m_selectedGradientSkyboxProfile.sunColor;
                sunIntensity = m_selectedGradientSkyboxProfile.sunIntensity;
                sunLWRPIntensity = m_selectedGradientSkyboxProfile.LWRPSunIntensity;
                sunHDRPIntensity = m_selectedGradientSkyboxProfile.HDRPSunIntensity;
            }

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
            {
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
#if !UNITY_2019_3_OR_NEWER
                        enableSunDisk = m_editorUtils.Toggle("EnableSunDisk", enableSunDisk, helpEnabled);
                        if (enableSunDisk)
                        {
                            includeSunInBaking = m_editorUtils.Toggle("IncludeSunInBaking", includeSunInBaking, helpEnabled);
                            hdrpProceduralSunSize = m_editorUtils.Slider("SunSize", hdrpProceduralSunSize, 0f, 1f, helpEnabled);
                            hdrpProceduralSunSizeConvergence = m_editorUtils.Slider("SunConvergence", hdrpProceduralSunSizeConvergence, 0.1f, 10f, helpEnabled);
                        }
#endif
                        sunColor = m_editorUtils.ColorField("SunTintColorSelector", sunColor, helpEnabled);
                        sunHDRPIntensity = m_editorUtils.Slider("SunIntensitySlider", sunHDRPIntensity, 0f, 10f, helpEnabled);
                        indirectLightMultiplier = m_editorUtils.Slider("IndirectSunLightMultiplier", indirectLightMultiplier, 0f, 10f, helpEnabled);
                    }                    
                    else
                    {
                        sunColor = m_editorUtils.ColorField("SunTintColorSelector", sunColor, helpEnabled);
                        sunHDRPIntensity = m_editorUtils.Slider("SunIntensitySlider", sunHDRPIntensity, 0f, 10f, helpEnabled);                        
                        indirectLightMultiplier = m_editorUtils.Slider("IndirectSunLightMultiplier", indirectLightMultiplier, 0f, 10f, helpEnabled);
                    }
                }
                else
                {
                    if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        enableSunDisk = m_editorUtils.Toggle("EnableSunDisk", enableSunDisk, helpEnabled);
                        if (enableSunDisk)
                        {
                            proceduralSunSize = m_editorUtils.Slider("SunSize", proceduralSunSize, 0f, 1f, helpEnabled);
                            proceduralSunSizeConvergence = m_editorUtils.Slider("SunConvergence", proceduralSunSizeConvergence, 0.1f, 10f, helpEnabled);
                        }
                        sunColor = m_editorUtils.ColorField("SunTintColorSelector", sunColor, helpEnabled);
                        if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                        {
                            sunIntensity = m_editorUtils.Slider("SunIntensitySlider", sunIntensity, 0f, 10f, helpEnabled);
                        }
                        else
                        {
                            sunLWRPIntensity = m_editorUtils.Slider("SunIntensitySlider", sunLWRPIntensity, 0f, 10f, helpEnabled);
                        }
                        indirectLightMultiplier = m_editorUtils.Slider("IndirectSunLightMultiplier", indirectLightMultiplier, 0f, 10f, helpEnabled);
                        shadowStrength = m_editorUtils.Slider("SunShadowStrength", shadowStrength, 0f, 1f, helpEnabled);
                    }
                    else
                    {
                        /*
                        useTemperature = m_editorUtils.Toggle("UseTemperature", helpEnabled);
                        if (useTemperature)
                        {
                            EditorGUI.showMixedValue = true;
                            EditorGUILayout.Space();
                            serializedObject.Update();
                            TemperatureSlider("TempatureObject", temperature);
                            serializedObject.ApplyModifiedProperties();
                            EditorGUI.showMixedValue = false;
                        }
                        else
                        {
                            sunColor = m_editorUtils.ColorField("SunTintColorSelector", sunColor, helpEnabled);
                            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                            {
                                sunIntensity = m_editorUtils.Slider("SunIntensitySlider", sunIntensity, 0f, 10f, helpEnabled);
                            }
                            else
                            {
                                sunLWRPIntensity = m_editorUtils.Slider("SunIntensitySlider", sunLWRPIntensity, 0f, 10f, helpEnabled);
                            }
                        }
                        */

                        sunColor = m_editorUtils.ColorField("SunTintColorSelector", sunColor, helpEnabled);
                        if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
                        {
                            sunIntensity = m_editorUtils.Slider("SunIntensitySlider", sunIntensity, 0f, 10f, helpEnabled);
                        }
                        else
                        {
                            sunLWRPIntensity = m_editorUtils.Slider("SunIntensitySlider", sunLWRPIntensity, 0f, 10f, helpEnabled);
                        }
                        indirectLightMultiplier = m_editorUtils.Slider("IndirectSunLightMultiplier", indirectLightMultiplier, 0f, 10f, helpEnabled);
                        shadowStrength = m_editorUtils.Slider("SunShadowStrength", shadowStrength, 0f, 1f, helpEnabled);
                    }
                }
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateSun = true;

                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    m_profiles.m_updateFog = true;
                }

                //Time of day
                m_profiles.m_useTimeOfDay = useTimeOfDay;

                //Settings
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    //Sun Settings
                    m_selectedSkyboxProfile.shadowStrength = shadowStrength;
                    m_selectedSkyboxProfile.indirectLightMultiplier = indirectLightMultiplier;
                    m_selectedSkyboxProfile.sunColor = sunColor;
                    m_selectedSkyboxProfile.sunIntensity = sunIntensity;
                    m_selectedSkyboxProfile.LWRPSunIntensity = sunLWRPIntensity;
                    m_selectedSkyboxProfile.HDRPSunIntensity = sunHDRPIntensity;
                    m_selectedSkyboxProfile.useTempature = useTemperature;
                    m_selectedSkyboxProfile.temperature = currentTemperature;
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //Sun Settings
                    m_selectedProceduralSkyboxProfile.enableSunDisk = enableSunDisk;
                    m_selectedProceduralSkyboxProfile.includeSunInBaking = includeSunInBaking;
                    m_selectedProceduralSkyboxProfile.shadowStrength = shadowStrength;
                    m_selectedProceduralSkyboxProfile.indirectLightMultiplier = indirectLightMultiplier;
                    m_selectedProceduralSkyboxProfile.proceduralSunColor = sunColor;
                    m_selectedProceduralSkyboxProfile.proceduralSunIntensity = sunIntensity;
                    m_selectedProceduralSkyboxProfile.proceduralLWRPSunIntensity = sunLWRPIntensity;
                    m_selectedProceduralSkyboxProfile.proceduralHDRPSunIntensity = sunHDRPIntensity;
                    m_selectedProceduralSkyboxProfile.proceduralSunSize = proceduralSunSize;
                    m_selectedProceduralSkyboxProfile.hdrpProceduralSunSize = hdrpProceduralSunSize;
                    m_selectedProceduralSkyboxProfile.proceduralSunSizeConvergence = proceduralSunSizeConvergence;
                    m_selectedProceduralSkyboxProfile.hdrpProceduralSunSizeConvergence = hdrpProceduralSunSizeConvergence;
                }
                else
                {
                    //Sun Settings
                    m_selectedGradientSkyboxProfile.shadowStrength = shadowStrength;
                    m_selectedGradientSkyboxProfile.indirectLightMultiplier = indirectLightMultiplier;
                    m_selectedGradientSkyboxProfile.sunColor = sunColor;
                    m_selectedGradientSkyboxProfile.sunIntensity = sunIntensity;
                    m_selectedGradientSkyboxProfile.LWRPSunIntensity = sunLWRPIntensity;
                    m_selectedGradientSkyboxProfile.HDRPSunIntensity = sunHDRPIntensity;
                }

                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Horizon settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HorizonSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Time of day
            useTimeOfDay = m_profiles.m_useTimeOfDay;

            //Settings
            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                //Horizon Settings
                scaleHorizonObjectWithFog = m_selectedSkyboxProfile.scaleHorizonObjectWithFog;
                horizonEnabled = m_selectedSkyboxProfile.horizonSkyEnabled;
                horizonScattering = m_selectedSkyboxProfile.horizonScattering;
                horizonFogDensity = m_selectedSkyboxProfile.horizonFogDensity;
                horizonFalloff = m_selectedSkyboxProfile.horizonFalloff;
                horizonBlend = m_selectedSkyboxProfile.horizonBlend;
                horizonScale = m_selectedSkyboxProfile.horizonSize;
                followPlayer = m_selectedSkyboxProfile.followPlayer;
                horizonUpdateTime = m_selectedSkyboxProfile.horizonUpdateTime;
                horizonPosition = m_selectedSkyboxProfile.horizonPosition;
                enableSunDisk = m_selectedSkyboxProfile.enableSunDisk;
            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                //Horizon Settings
                scaleHorizonObjectWithFog = m_selectedProceduralSkyboxProfile.scaleHorizonObjectWithFog;
                horizonEnabled = m_selectedProceduralSkyboxProfile.horizonSkyEnabled;
                horizonScattering = m_selectedProceduralSkyboxProfile.horizonScattering;
                horizonFogDensity = m_selectedProceduralSkyboxProfile.horizonFogDensity;
                horizonFalloff = m_selectedProceduralSkyboxProfile.horizonFalloff;
                horizonBlend = m_selectedProceduralSkyboxProfile.horizonBlend;
                horizonScale = m_selectedProceduralSkyboxProfile.horizonSize;
                followPlayer = m_selectedProceduralSkyboxProfile.followPlayer;
                horizonUpdateTime = m_selectedProceduralSkyboxProfile.horizonUpdateTime;
                horizonPosition = m_selectedProceduralSkyboxProfile.horizonPosition;
                enableSunDisk = m_selectedProceduralSkyboxProfile.enableSunDisk;
            }
            else
            {
                //Horizon Settings
                scaleHorizonObjectWithFog = m_selectedGradientSkyboxProfile.scaleHorizonObjectWithFog;
                horizonEnabled = m_selectedGradientSkyboxProfile.horizonSkyEnabled;
                horizonScattering = m_selectedGradientSkyboxProfile.horizonScattering;
                horizonFogDensity = m_selectedGradientSkyboxProfile.horizonFogDensity;
                horizonFalloff = m_selectedGradientSkyboxProfile.horizonFalloff;
                horizonBlend = m_selectedGradientSkyboxProfile.horizonBlend;
                horizonScale = m_selectedGradientSkyboxProfile.horizonSize;
                followPlayer = m_selectedGradientSkyboxProfile.followPlayer;
                horizonUpdateTime = m_selectedGradientSkyboxProfile.horizonUpdateTime;
                horizonPosition = m_selectedGradientSkyboxProfile.horizonPosition;
                enableSunDisk = m_selectedGradientSkyboxProfile.enableSunDisk;
            }

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                horizonEnabled = m_editorUtils.ToggleLeft("EnableHorizonSky", horizonEnabled, helpEnabled);
                if (horizonEnabled)
                {
                    EditorGUI.indentLevel++;
                    followPlayer = m_editorUtils.Toggle("FollowPlayer", followPlayer, helpEnabled);
                    if (!followPlayer)
                    {
                        horizonPosition = m_editorUtils.Vector3Field("HorizonPosition", horizonPosition, helpEnabled);
                    }
                    if (followPlayer)
                    {
                        horizonUpdateTime = m_editorUtils.FloatField("HorizonUpdateTime", horizonUpdateTime, helpEnabled);
                    }
                    scaleHorizonObjectWithFog = m_editorUtils.Toggle("ScaleHorizonObjectWithFog", scaleHorizonObjectWithFog, helpEnabled);
                    if (!scaleHorizonObjectWithFog)
                    {
                        horizonScale = m_editorUtils.Vector3Field("HorizonScale", horizonScale, helpEnabled);
                    }
                    horizonScattering = m_editorUtils.Slider("HorizonSkyScattering", horizonScattering, 0f, 1f, helpEnabled);
                    horizonFogDensity = m_editorUtils.Slider("HorizonSkyFogDensity", horizonFogDensity, 0f, 1f, helpEnabled);
                    horizonFalloff = m_editorUtils.Slider("HorizonSkyFalloff", horizonFalloff, 0f, 1f, helpEnabled);
                    horizonBlend = m_editorUtils.Slider("HorizonSkyBlend", horizonBlend, 0f, 1f, helpEnabled);
                    EditorGUI.indentLevel--;
                }
            }
            else if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Universal)
            {
                m_editorUtils.Text("NotAvailableLW", GUILayout.MinHeight(15f), GUILayout.MaxHeight(30f), GUILayout.MinWidth(100f), GUILayout.MaxWidth(250f));
            }
            else
            {
                m_editorUtils.Text("NotAvailableHD", GUILayout.MinHeight(15f), GUILayout.MaxHeight(30f), GUILayout.MinWidth(100f), GUILayout.MaxWidth(250f));
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                //Time of day
                m_profiles.m_useTimeOfDay = useTimeOfDay;

                //Settings
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    //Horizon Settings
                    m_selectedSkyboxProfile.scaleHorizonObjectWithFog = scaleHorizonObjectWithFog;
                    m_selectedSkyboxProfile.horizonSkyEnabled = horizonEnabled;
                    m_selectedSkyboxProfile.horizonScattering = horizonScattering;
                    m_selectedSkyboxProfile.horizonFogDensity = horizonFogDensity;
                    m_selectedSkyboxProfile.horizonFalloff = horizonFalloff;
                    m_selectedSkyboxProfile.horizonBlend = horizonBlend;
                    m_selectedSkyboxProfile.horizonSize = horizonScale;
                    m_selectedSkyboxProfile.followPlayer = followPlayer;
                    m_selectedSkyboxProfile.horizonUpdateTime = horizonUpdateTime;
                    m_selectedSkyboxProfile.horizonPosition = horizonPosition;
                    m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //Horizon Settings
                    m_selectedProceduralSkyboxProfile.scaleHorizonObjectWithFog = scaleHorizonObjectWithFog;
                    m_selectedProceduralSkyboxProfile.horizonSkyEnabled = horizonEnabled;
                    m_selectedProceduralSkyboxProfile.horizonScattering = horizonScattering;
                    m_selectedProceduralSkyboxProfile.horizonFogDensity = horizonFogDensity;
                    m_selectedProceduralSkyboxProfile.horizonFalloff = horizonFalloff;
                    m_selectedProceduralSkyboxProfile.horizonBlend = horizonBlend;
                    m_selectedProceduralSkyboxProfile.horizonSize = horizonScale;
                    m_selectedProceduralSkyboxProfile.followPlayer = followPlayer;
                    m_selectedProceduralSkyboxProfile.horizonUpdateTime = horizonUpdateTime;
                    m_selectedProceduralSkyboxProfile.horizonPosition = horizonPosition;
                    m_selectedProceduralSkyboxProfile.enableSunDisk = enableSunDisk;
                }
                else
                {
                    //Horizon Settings
                    m_selectedGradientSkyboxProfile.scaleHorizonObjectWithFog = scaleHorizonObjectWithFog;
                    m_selectedGradientSkyboxProfile.horizonSkyEnabled = horizonEnabled;
                    m_selectedGradientSkyboxProfile.horizonScattering = horizonScattering;
                    m_selectedGradientSkyboxProfile.horizonFogDensity = horizonFogDensity;
                    m_selectedGradientSkyboxProfile.horizonFalloff = horizonFalloff;
                    m_selectedGradientSkyboxProfile.horizonBlend = horizonBlend;
                    m_selectedGradientSkyboxProfile.horizonSize = horizonScale;
                    m_selectedGradientSkyboxProfile.followPlayer = followPlayer;
                    m_selectedGradientSkyboxProfile.horizonUpdateTime = horizonUpdateTime;
                    m_selectedGradientSkyboxProfile.horizonPosition = horizonPosition;
                    m_selectedGradientSkyboxProfile.enableSunDisk = enableSunDisk;
                }

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Normal shadow settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ShadowSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Time of day
            useTimeOfDay = m_profiles.m_useTimeOfDay;

            //Settings
            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                //Shadow Settings
                shadowDistance = m_selectedSkyboxProfile.shadowDistance;
                shadowmaskMode = m_selectedSkyboxProfile.shadowmaskMode;
                shadowType = m_selectedSkyboxProfile.shadowType;
                shadowResolution = m_selectedSkyboxProfile.shadowResolution;
                shadowProjection = m_selectedSkyboxProfile.shadowProjection;
                shadowCascade = m_selectedSkyboxProfile.cascadeCount;
            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                //Shadow Settings
                shadowDistance = m_selectedProceduralSkyboxProfile.shadowDistance;
                shadowmaskMode = m_selectedProceduralSkyboxProfile.shadowmaskMode;
                shadowType = m_selectedProceduralSkyboxProfile.shadowType;
                shadowResolution = m_selectedProceduralSkyboxProfile.shadowResolution;
                shadowProjection = m_selectedProceduralSkyboxProfile.shadowProjection;
                shadowCascade = m_selectedProceduralSkyboxProfile.cascadeCount;
            }
            else
            {
                //Shadow Settings
                shadowDistance = m_selectedGradientSkyboxProfile.shadowDistance;
                shadowmaskMode = m_selectedGradientSkyboxProfile.shadowmaskMode;
                shadowType = m_selectedGradientSkyboxProfile.shadowType;
                shadowResolution = m_selectedGradientSkyboxProfile.shadowResolution;
                shadowProjection = m_selectedGradientSkyboxProfile.shadowProjection;
                shadowCascade = m_selectedGradientSkyboxProfile.cascadeCount;
            }

            m_profiles.m_updateFog = true;
            m_profiles.m_updateSun = true;

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            if (mainCam != null)
            {
                if (shadowDistance > mainCam.farClipPlane)
                {
                    shadowDistance = mainCam.farClipPlane;
                }

                shadowDistance = m_editorUtils.Slider("ShadowDistanceSlider", shadowDistance, 0f, mainCam.farClipPlane, helpEnabled);
            }
            else
            {
                shadowDistance = m_editorUtils.Slider("ShadowDistanceSlider", shadowDistance, 0f, 30000f, helpEnabled);
            }

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                shadowCascade = (AmbientSkiesConsts.ShadowCascade)m_editorUtils.EnumPopup("ShadowCascade", shadowCascade, helpEnabled);
                shadowmaskMode = (ShadowmaskMode)m_editorUtils.EnumPopup("ShadowmaskMode", shadowmaskMode, helpEnabled);
                shadowType = (LightShadows)m_editorUtils.EnumPopup("ShadowType", shadowType, helpEnabled);
                shadowResolution = (ShadowResolution)m_editorUtils.EnumPopup("ShadowResolution", shadowResolution, helpEnabled);
                shadowProjection = (ShadowProjection)m_editorUtils.EnumPopup("ShadowProjection", shadowProjection, helpEnabled);
            }
            else if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Universal)
            {
                shadowType = (LightShadows)m_editorUtils.EnumPopup("ShadowType", shadowType, helpEnabled);
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateShadows = true;

                //Time of day
                m_profiles.m_useTimeOfDay = useTimeOfDay;

                //Settings
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    //Shadow Settings
                    m_selectedSkyboxProfile.shadowDistance = shadowDistance;
                    m_selectedSkyboxProfile.shadowmaskMode = shadowmaskMode;
                    m_selectedSkyboxProfile.shadowType = shadowType;
                    m_selectedSkyboxProfile.shadowResolution = shadowResolution;
                    m_selectedSkyboxProfile.shadowProjection = shadowProjection;
                    m_selectedSkyboxProfile.cascadeCount = shadowCascade;
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //Shadow Settings
                    m_selectedProceduralSkyboxProfile.shadowDistance = shadowDistance;
                    m_selectedProceduralSkyboxProfile.shadowmaskMode = shadowmaskMode;
                    m_selectedProceduralSkyboxProfile.shadowType = shadowType;
                    m_selectedProceduralSkyboxProfile.shadowResolution = shadowResolution;
                    m_selectedProceduralSkyboxProfile.shadowProjection = shadowProjection;
                    m_selectedProceduralSkyboxProfile.cascadeCount = shadowCascade;
                }
                else
                {
                    //Shadow Settings
                    m_selectedGradientSkyboxProfile.shadowDistance = shadowDistance;
                    m_selectedGradientSkyboxProfile.shadowmaskMode = shadowmaskMode;
                    m_selectedGradientSkyboxProfile.shadowType = shadowType;
                    m_selectedGradientSkyboxProfile.shadowResolution = shadowResolution;
                    m_selectedGradientSkyboxProfile.shadowProjection = shadowProjection;
                    m_selectedGradientSkyboxProfile.cascadeCount = shadowCascade;
                }

                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// HD shadow settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDShadowSettingsEnabled(bool helpEnabled)
        {
#if HDPipeline
            EditorGUI.BeginChangeCheck();

            //Time of day
            useTimeOfDay = m_profiles.m_useTimeOfDay;

            //Settings
            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                //Shadow Settings
                shadowDistance = m_selectedSkyboxProfile.shadowDistance;
                shadowmaskMode = m_selectedSkyboxProfile.shadowmaskMode;
                shadowType = m_selectedSkyboxProfile.shadowType;
                shadowResolution = m_selectedSkyboxProfile.shadowResolution;
                shadowProjection = m_selectedSkyboxProfile.shadowProjection;
                shadowCascade = m_selectedSkyboxProfile.cascadeCount;

                //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                //HD Shadows
                hDShadowQuality = m_selectedSkyboxProfile.shadowQuality;
                split1 = m_selectedSkyboxProfile.cascadeSplit1;
                split2 = m_selectedSkyboxProfile.cascadeSplit2;
                split3 = m_selectedSkyboxProfile.cascadeSplit3;

                //Contact Shadows
                enableContactShadows = m_selectedSkyboxProfile.useContactShadows;
                contactLength = m_selectedSkyboxProfile.contactShadowsLength;
                contactScaleFactor = m_selectedSkyboxProfile.contactShadowsDistanceScaleFactor;
                contactMaxDistance = m_selectedSkyboxProfile.contactShadowsMaxDistance;
                contactFadeDistance = m_selectedSkyboxProfile.contactShadowsFadeDistance;
                contactSampleCount = m_selectedSkyboxProfile.contactShadowsSampleCount;
                contactOpacity = m_selectedSkyboxProfile.contactShadowsOpacity;

                //Micro Shadows
                enableMicroShadows = m_selectedSkyboxProfile.useMicroShadowing;
                microShadowOpacity = m_selectedSkyboxProfile.microShadowOpacity;
#endif
            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                //Shadow Settings
                shadowDistance = m_selectedProceduralSkyboxProfile.shadowDistance;
                shadowmaskMode = m_selectedProceduralSkyboxProfile.shadowmaskMode;
                shadowType = m_selectedProceduralSkyboxProfile.shadowType;
                shadowResolution = m_selectedProceduralSkyboxProfile.shadowResolution;
                shadowProjection = m_selectedProceduralSkyboxProfile.shadowProjection;
                shadowCascade = m_selectedProceduralSkyboxProfile.cascadeCount;

                //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                //HD Shadows
                hDShadowQuality = m_selectedProceduralSkyboxProfile.shadowQuality;
                split1 = m_selectedProceduralSkyboxProfile.cascadeSplit1;
                split2 = m_selectedProceduralSkyboxProfile.cascadeSplit2;
                split3 = m_selectedProceduralSkyboxProfile.cascadeSplit3;

                //Contact Shadows
                enableContactShadows = m_selectedProceduralSkyboxProfile.useContactShadows;
                contactLength = m_selectedProceduralSkyboxProfile.contactShadowsLength;
                contactScaleFactor = m_selectedProceduralSkyboxProfile.contactShadowsDistanceScaleFactor;
                contactMaxDistance = m_selectedProceduralSkyboxProfile.contactShadowsMaxDistance;
                contactFadeDistance = m_selectedProceduralSkyboxProfile.contactShadowsFadeDistance;
                contactSampleCount = m_selectedProceduralSkyboxProfile.contactShadowsSampleCount;
                contactOpacity = m_selectedProceduralSkyboxProfile.contactShadowsOpacity;

                //Micro Shadows
                enableMicroShadows = m_selectedProceduralSkyboxProfile.useMicroShadowing;
                microShadowOpacity = m_selectedProceduralSkyboxProfile.microShadowOpacity;
#endif
            }
            else
            {
                //Shadow Settings
                shadowDistance = m_selectedGradientSkyboxProfile.shadowDistance;
                shadowmaskMode = m_selectedGradientSkyboxProfile.shadowmaskMode;
                shadowType = m_selectedGradientSkyboxProfile.shadowType;
                shadowResolution = m_selectedGradientSkyboxProfile.shadowResolution;
                shadowProjection = m_selectedGradientSkyboxProfile.shadowProjection;
                shadowCascade = m_selectedGradientSkyboxProfile.cascadeCount;

                //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                //HD Shadows
                hDShadowQuality = m_selectedGradientSkyboxProfile.shadowQuality;
                split1 = m_selectedGradientSkyboxProfile.cascadeSplit1;
                split2 = m_selectedGradientSkyboxProfile.cascadeSplit2;
                split3 = m_selectedGradientSkyboxProfile.cascadeSplit3;

                //Contact Shadows
                enableContactShadows = m_selectedGradientSkyboxProfile.useContactShadows;
                contactLength = m_selectedGradientSkyboxProfile.contactShadowsLength;
                contactScaleFactor = m_selectedGradientSkyboxProfile.contactShadowsDistanceScaleFactor;
                contactMaxDistance = m_selectedGradientSkyboxProfile.contactShadowsMaxDistance;
                contactFadeDistance = m_selectedGradientSkyboxProfile.contactShadowsFadeDistance;
                contactSampleCount = m_selectedGradientSkyboxProfile.contactShadowsSampleCount;
                contactOpacity = m_selectedGradientSkyboxProfile.contactShadowsOpacity;

                //Micro Shadows
                enableMicroShadows = m_selectedGradientSkyboxProfile.useMicroShadowing;
                microShadowOpacity = m_selectedGradientSkyboxProfile.microShadowOpacity;
#endif
            }

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            hDShadowQuality = (AmbientSkiesConsts.HDShadowQuality)m_editorUtils.EnumPopup("ShadowQuality", hDShadowQuality, helpEnabled);
            if (mainCam != null)
            {
                shadowDistance = m_editorUtils.Slider("ShadowDistanceSlider", shadowDistance, 0f, mainCam.farClipPlane, helpEnabled);
            }
            else
            {
                shadowDistance = m_editorUtils.Slider("ShadowDistanceSlider", shadowDistance, 0, 3000, helpEnabled);
            }
            shadowmaskMode = (ShadowmaskMode)m_editorUtils.EnumPopup("ShadowmaskMode", shadowmaskMode, helpEnabled);

            shadowCascade = (AmbientSkiesConsts.ShadowCascade)m_editorUtils.EnumPopup("ShadowCascade", shadowCascade, helpEnabled);
            if (shadowCascade == AmbientSkiesConsts.ShadowCascade.CascadeCount1)
            {
                m_editorUtils.Text("Cascade count is set to 1 this will result in no shadow distanced base quality. If you want better shadow quality/resolution set a higher cascade count.");
            }
            else if (shadowCascade == AmbientSkiesConsts.ShadowCascade.CascadeCount2)
            {
                EditorGUI.indentLevel++;
                split1 = m_editorUtils.Slider("CascadeSplit1", split1, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }
            else if (shadowCascade == AmbientSkiesConsts.ShadowCascade.CascadeCount3)
            {
                EditorGUI.indentLevel++;
                split1 = m_editorUtils.Slider("CascadeSplit1", split1, 0f, 1f, helpEnabled);
                split2 = m_editorUtils.Slider("CascadeSplit2", split2, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.indentLevel++;
                split1 = m_editorUtils.Slider("CascadeSplit1", split1, 0f, 1f, helpEnabled);
                split2 = m_editorUtils.Slider("CascadeSplit2", split2, 0f, 1f, helpEnabled);
                split3 = m_editorUtils.Slider("CascadeSplit3", split3, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            enableContactShadows = m_editorUtils.Toggle("EnableContactShadows", enableContactShadows, helpEnabled);
            if (enableContactShadows)
            {
                EditorGUI.indentLevel++;
                contactLength = m_editorUtils.Slider("ContactLength", contactLength, 0f, 1f, helpEnabled);
                contactScaleFactor = m_editorUtils.Slider("ContactScaleFactor", contactScaleFactor, 0f, 1f, helpEnabled);
                contactMaxDistance = m_editorUtils.Slider("ContactMaxDistance", contactMaxDistance, 0f, 400f, helpEnabled);
                contactFadeDistance = m_editorUtils.Slider("ContactFadeDistance", contactFadeDistance, 0f, 200f, helpEnabled);
                contactSampleCount = m_editorUtils.IntSlider("ContactSampleCount", contactSampleCount, 0, 64, helpEnabled);
                contactOpacity = m_editorUtils.Slider("ContactOpacity", contactOpacity, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
            enableMicroShadows = m_editorUtils.Toggle("EnableMicroShadows", enableMicroShadows, helpEnabled);
            if (enableMicroShadows)
            {
                EditorGUI.indentLevel++;
                microShadowOpacity = m_editorUtils.Slider("MicroShadowOpacity", microShadowOpacity, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateShadows = true;
                m_profiles.m_updateSun = true;

                //Time of day
                m_profiles.m_useTimeOfDay = useTimeOfDay;

                //Settings
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    //Shadow Settings
                    m_selectedSkyboxProfile.shadowDistance = shadowDistance;
                    m_selectedSkyboxProfile.shadowmaskMode = shadowmaskMode;
                    m_selectedSkyboxProfile.shadowType = shadowType;
                    m_selectedSkyboxProfile.shadowResolution = shadowResolution;
                    m_selectedSkyboxProfile.shadowProjection = shadowProjection;
                    m_selectedSkyboxProfile.cascadeCount = shadowCascade;

                    //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                    //HD Shadows
                    m_selectedSkyboxProfile.shadowQuality = hDShadowQuality;
                    m_selectedSkyboxProfile.cascadeSplit1 = split1;
                    m_selectedSkyboxProfile.cascadeSplit2 = split2;
                    m_selectedSkyboxProfile.cascadeSplit3 = split3;

                    //Contact Shadows
                    m_selectedSkyboxProfile.useContactShadows = enableContactShadows;
                    m_selectedSkyboxProfile.contactShadowsLength = contactLength;
                    m_selectedSkyboxProfile.contactShadowsDistanceScaleFactor = contactScaleFactor;
                    m_selectedSkyboxProfile.contactShadowsMaxDistance = contactMaxDistance;
                    m_selectedSkyboxProfile.contactShadowsFadeDistance = contactFadeDistance;
                    m_selectedSkyboxProfile.contactShadowsSampleCount = contactSampleCount;
                    m_selectedSkyboxProfile.contactShadowsOpacity = contactOpacity;

                    //Micro Shadows
                    m_selectedSkyboxProfile.useMicroShadowing = enableMicroShadows;
                    m_selectedSkyboxProfile.microShadowOpacity = microShadowOpacity;
#endif
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //Shadow Settings
                    m_selectedProceduralSkyboxProfile.shadowDistance = shadowDistance;
                    m_selectedProceduralSkyboxProfile.shadowmaskMode = shadowmaskMode;
                    m_selectedProceduralSkyboxProfile.shadowType = shadowType;
                    m_selectedProceduralSkyboxProfile.shadowResolution = shadowResolution;
                    m_selectedProceduralSkyboxProfile.shadowProjection = shadowProjection;
                    m_selectedProceduralSkyboxProfile.cascadeCount = shadowCascade;

                    //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                    //HD Shadows
                    m_selectedProceduralSkyboxProfile.shadowQuality = hDShadowQuality;
                    m_selectedProceduralSkyboxProfile.cascadeSplit1 = split1;
                    m_selectedProceduralSkyboxProfile.cascadeSplit2 = split2;
                    m_selectedProceduralSkyboxProfile.cascadeSplit3 = split3;

                    //Contact Shadows
                    m_selectedProceduralSkyboxProfile.useContactShadows = enableContactShadows;
                    m_selectedProceduralSkyboxProfile.contactShadowsLength = contactLength;
                    m_selectedProceduralSkyboxProfile.contactShadowsDistanceScaleFactor = contactScaleFactor;
                    m_selectedProceduralSkyboxProfile.contactShadowsMaxDistance = contactMaxDistance;
                    m_selectedProceduralSkyboxProfile.contactShadowsFadeDistance = contactFadeDistance;
                    m_selectedProceduralSkyboxProfile.contactShadowsSampleCount = contactSampleCount;
                    m_selectedProceduralSkyboxProfile.contactShadowsOpacity = contactOpacity;

                    //Micro Shadows
                    m_selectedProceduralSkyboxProfile.useMicroShadowing = enableMicroShadows;
                    m_selectedProceduralSkyboxProfile.microShadowOpacity = microShadowOpacity;
#endif
                }
                else
                {
                    //Shadow Settings
                    m_selectedGradientSkyboxProfile.shadowDistance = shadowDistance;
                    m_selectedGradientSkyboxProfile.shadowmaskMode = shadowmaskMode;
                    m_selectedGradientSkyboxProfile.shadowType = shadowType;
                    m_selectedGradientSkyboxProfile.shadowResolution = shadowResolution;
                    m_selectedGradientSkyboxProfile.shadowProjection = shadowProjection;
                    m_selectedGradientSkyboxProfile.cascadeCount = shadowCascade;

                    //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                    //HD Shadows
                    m_selectedGradientSkyboxProfile.shadowQuality = hDShadowQuality;
                    m_selectedGradientSkyboxProfile.cascadeSplit1 = split1;
                    m_selectedGradientSkyboxProfile.cascadeSplit2 = split2;
                    m_selectedGradientSkyboxProfile.cascadeSplit3 = split3;

                    //Contact Shadows
                    m_selectedGradientSkyboxProfile.useContactShadows = enableContactShadows;
                    m_selectedGradientSkyboxProfile.contactShadowsLength = contactLength;
                    m_selectedGradientSkyboxProfile.contactShadowsDistanceScaleFactor = contactScaleFactor;
                    m_selectedGradientSkyboxProfile.contactShadowsMaxDistance = contactMaxDistance;
                    m_selectedGradientSkyboxProfile.contactShadowsFadeDistance = contactFadeDistance;
                    m_selectedGradientSkyboxProfile.contactShadowsSampleCount = contactSampleCount;
                    m_selectedGradientSkyboxProfile.contactShadowsOpacity = contactOpacity;

                    //Micro Shadows
                    m_selectedGradientSkyboxProfile.useMicroShadowing = enableMicroShadows;
                    m_selectedGradientSkyboxProfile.microShadowOpacity = microShadowOpacity;
#endif
                }

                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                m_hasChanged = true;
            }
#endif
        }

        /// <summary>
        /// Screen space reflection settings
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ScreenSpaceReflectionSettingsEnabled(bool helpEnabled)
        {
#if HDPipeline
            EditorGUI.BeginChangeCheck();

            //Time of day
            useTimeOfDay = m_profiles.m_useTimeOfDay;

            //Settings
            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                //HD Pipeline Settings
#if HDPipeline
                //SS Reflection
                enableSSReflection = m_selectedSkyboxProfile.enableScreenSpaceReflections;
                ssrEdgeFade = m_selectedSkyboxProfile.screenEdgeFadeDistance;
                ssrNumberOfRays = m_selectedSkyboxProfile.maxNumberOfRaySteps;
                ssrObjectThickness = m_selectedSkyboxProfile.objectThickness;
                ssrMinSmoothness = m_selectedSkyboxProfile.minSmoothness;
                ssrSmoothnessFade = m_selectedSkyboxProfile.smoothnessFadeStart;
                ssrReflectSky = m_selectedSkyboxProfile.reflectSky;
#endif
            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                //HD Pipeline Settings
#if HDPipeline
                //SS Reflection
                enableSSReflection = m_selectedProceduralSkyboxProfile.enableScreenSpaceReflections;
                ssrEdgeFade = m_selectedProceduralSkyboxProfile.screenEdgeFadeDistance;
                ssrNumberOfRays = m_selectedProceduralSkyboxProfile.maxNumberOfRaySteps;
                ssrObjectThickness = m_selectedProceduralSkyboxProfile.objectThickness;
                ssrMinSmoothness = m_selectedProceduralSkyboxProfile.minSmoothness;
                ssrSmoothnessFade = m_selectedProceduralSkyboxProfile.smoothnessFadeStart;
                ssrReflectSky = m_selectedProceduralSkyboxProfile.reflectSky;
#endif

            }
            else
            {
                //HD Pipeline Settings
#if HDPipeline
                //SS Reflection
                enableSSReflection = m_selectedGradientSkyboxProfile.enableScreenSpaceReflections;
                ssrEdgeFade = m_selectedGradientSkyboxProfile.screenEdgeFadeDistance;
                ssrNumberOfRays = m_selectedGradientSkyboxProfile.maxNumberOfRaySteps;
                ssrObjectThickness = m_selectedGradientSkyboxProfile.objectThickness;
                ssrMinSmoothness = m_selectedGradientSkyboxProfile.minSmoothness;
                ssrSmoothnessFade = m_selectedGradientSkyboxProfile.smoothnessFadeStart;
                ssrReflectSky = m_selectedGradientSkyboxProfile.reflectSky;
#endif
            }

            m_profiles.m_updateFog = true;
            m_profiles.m_updateSun = true;

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            enableSSReflection = m_editorUtils.Toggle("EnableScreenSpaceReflections", enableSSReflection, helpEnabled);
            if (enableSSReflection)
            {
                EditorGUI.indentLevel++;
                ssrEdgeFade = m_editorUtils.Slider("ScreenSpaceReflectionEdgeFade", ssrEdgeFade, 0f, 1f, helpEnabled);
                ssrNumberOfRays = m_editorUtils.IntSlider("ScreenSpaceReflectionRayNumber", ssrNumberOfRays, 0, 256, helpEnabled);
                ssrObjectThickness = m_editorUtils.Slider("ScreenSpaceReflectionObjectThickness", ssrObjectThickness, 0f, 1f, helpEnabled);
                ssrMinSmoothness = m_editorUtils.Slider("ScreenSpaceReflectionMinSmoothness", ssrMinSmoothness, 0f, 1f, helpEnabled);
                ssrSmoothnessFade = m_editorUtils.Slider("ScreenSpaceReflectionSmoothnessFade", ssrSmoothnessFade, 0f, 1f, helpEnabled);
                ssrReflectSky = m_editorUtils.Toggle("ScreenSpaceReflectionReflectSky", ssrReflectSky, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateScreenSpaceReflections = true;

                //Time of day
                m_profiles.m_useTimeOfDay = useTimeOfDay;

                //Settings
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    //HD Pipeline Settings
#if HDPipeline
                    //SS Reflection
                    m_selectedSkyboxProfile.enableScreenSpaceReflections = enableSSReflection;
                    m_selectedSkyboxProfile.screenEdgeFadeDistance = ssrEdgeFade;
                    m_selectedSkyboxProfile.maxNumberOfRaySteps = ssrNumberOfRays;
                    m_selectedSkyboxProfile.objectThickness = ssrObjectThickness;
                    m_selectedSkyboxProfile.minSmoothness = ssrMinSmoothness;
                    m_selectedSkyboxProfile.smoothnessFadeStart = ssrSmoothnessFade;
                    m_selectedSkyboxProfile.reflectSky = ssrReflectSky;
#endif
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //HD Pipeline Settings
#if HDPipeline
                    //SS Reflection
                    m_selectedProceduralSkyboxProfile.enableScreenSpaceReflections = enableSSReflection;
                    m_selectedProceduralSkyboxProfile.screenEdgeFadeDistance = ssrEdgeFade;
                    m_selectedProceduralSkyboxProfile.maxNumberOfRaySteps = ssrNumberOfRays;
                    m_selectedProceduralSkyboxProfile.objectThickness = ssrObjectThickness;
                    m_selectedProceduralSkyboxProfile.minSmoothness = ssrMinSmoothness;
                    m_selectedProceduralSkyboxProfile.smoothnessFadeStart = ssrSmoothnessFade;
                    m_selectedProceduralSkyboxProfile.reflectSky = ssrReflectSky;
#endif

                }
                else
                {
                    //HD Pipeline Settings
#if HDPipeline
                    //SS Reflection
                    m_selectedGradientSkyboxProfile.enableScreenSpaceReflections = enableSSReflection;
                    m_selectedGradientSkyboxProfile.screenEdgeFadeDistance = ssrEdgeFade;
                    m_selectedGradientSkyboxProfile.maxNumberOfRaySteps = ssrNumberOfRays;
                    m_selectedGradientSkyboxProfile.objectThickness = ssrObjectThickness;
                    m_selectedGradientSkyboxProfile.minSmoothness = ssrMinSmoothness;
                    m_selectedGradientSkyboxProfile.smoothnessFadeStart = ssrSmoothnessFade;
                    m_selectedGradientSkyboxProfile.reflectSky = ssrReflectSky;
#endif
                }

                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                m_hasChanged = true;
            }
#endif
        }

        /// <summary>
        /// Screen space refraction settings
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ScreenSpaceRefractionSettingsEnabled(bool helpEnabled)
        {
#if HDPipeline
            EditorGUI.BeginChangeCheck();

            //Time of day
            useTimeOfDay = m_profiles.m_useTimeOfDay;

            //Settings
            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                //HD Pipeline Settings
#if HDPipeline
                //SS Refract
                enableSSRefraction = m_selectedSkyboxProfile.enableScreenSpaceRefractions;
                ssrWeightDistance = m_selectedSkyboxProfile.screenWeightDistance;
#endif

            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                //HD Pipeline Settings
#if HDPipeline
                //SS Refract
                enableSSRefraction = m_selectedProceduralSkyboxProfile.enableScreenSpaceRefractions;
                ssrWeightDistance = m_selectedProceduralSkyboxProfile.screenWeightDistance;
#endif
            }
            else
            {
                //HD Pipeline Settings
#if HDPipeline
                //SS Refract
                enableSSRefraction = m_selectedGradientSkyboxProfile.enableScreenSpaceRefractions;
                ssrWeightDistance = m_selectedGradientSkyboxProfile.screenWeightDistance;
#endif
            }

            m_profiles.m_updateFog = true;
            m_profiles.m_updateSun = true;

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            enableSSRefraction = m_editorUtils.Toggle("EnableScreenSpaceRefraction", enableSSRefraction, helpEnabled);
            if (enableSSRefraction)
            {
                EditorGUI.indentLevel++;
                ssrWeightDistance = m_editorUtils.Slider("ScreenSpaceRefractionWeightDistance", ssrWeightDistance, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateScreenSpaceRefractions = true;

                //Time of day
                m_profiles.m_useTimeOfDay = useTimeOfDay;

                //Settings
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    //HD Pipeline Settings
#if HDPipeline
                    //SS Refract
                    m_selectedSkyboxProfile.enableScreenSpaceRefractions = enableSSRefraction;
                    m_selectedSkyboxProfile.screenWeightDistance = ssrWeightDistance;
#endif

                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //HD Pipeline Settings
#if HDPipeline
                    //SS Refract
                    m_selectedProceduralSkyboxProfile.enableScreenSpaceRefractions = enableSSRefraction;
                    m_selectedProceduralSkyboxProfile.screenWeightDistance = ssrWeightDistance;
#endif
                }
                else
                {
                    //HD Pipeline Settings
#if HDPipeline
                    //SS Refract
                    m_selectedGradientSkyboxProfile.enableScreenSpaceRefractions = enableSSRefraction;
                    m_selectedGradientSkyboxProfile.screenWeightDistance = ssrWeightDistance;
#endif
                }

                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                m_hasChanged = true;
            }
#endif
        }

        #endregion

        #region Extra Lights Functions

        /// <summary>
        /// Interior lighting settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ShowInteriorLightingMode(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            #region Get Variables

            GameObject activeObject = Selection.activeGameObject;

            lightingEditMode = m_profiles.m_lightingEditMode;
            lightName = m_profiles.m_lightName;
            spawnAtCamera = m_profiles.m_spawnAtCamera;
            parentToActiveObject = m_profiles.m_parentToActiveObject;

            #endregion            

            if (GetComponentOnObject(activeObject))
            {
                if (activeObject.GetComponent<Light>() == null)
                {
                    lightName = m_editorUtils.TextField("LightName", lightName, helpEnabled);
                    spawnAtCamera = m_editorUtils.Toggle("SpawnAtCamera", spawnAtCamera, helpEnabled);
                    parentToActiveObject = m_editorUtils.Toggle("ParentToActiveObject", parentToActiveObject, helpEnabled);

                    EditorGUILayout.Space();

                    if (m_editorUtils.Button("Create Point Light"))
                    {
                        CreatePointLight(lightName, spawnAtCamera, parentToActiveObject);
                    }

                    if (m_editorUtils.Button("Create Spot Light"))
                    {
                        CreateSpotLight(lightName, spawnAtCamera, parentToActiveObject);
                    }

                    if (m_editorUtils.Button("Create Area Light"))
                    {
                        CreateAreaLight(lightName, spawnAtCamera, parentToActiveObject);
                    }
                }
                else
                {
                    if (Selection.objects.Length > 1)
                    {
                        EditorGUILayout.HelpBox("Multiple editing mode not yet supported. Please select one light at a time.", MessageType.Warning);
                    }
                    else if (activeObject.GetComponent<Light>().type == LightType.Directional)
                    {
                        EditorGUILayout.HelpBox("To modify the Directional Light. Go to the Skies tab and Sun Settings", MessageType.Info);

                        lightName = m_editorUtils.TextField("LightName", lightName, helpEnabled);
                        spawnAtCamera = m_editorUtils.Toggle("SpawnAtCamera", spawnAtCamera, helpEnabled);
                        parentToActiveObject = m_editorUtils.Toggle("ParentToActiveObject", parentToActiveObject, helpEnabled);

                        EditorGUILayout.Space();

                        if (m_editorUtils.Button("Create Point Light"))
                        {
                            CreatePointLight(lightName, spawnAtCamera, parentToActiveObject);
                        }

                        if (m_editorUtils.Button("Create Spot Light"))
                        {
                            CreateSpotLight(lightName, spawnAtCamera, parentToActiveObject);
                        }

                        if (m_editorUtils.Button("Create Area Light"))
                        {
                            CreateAreaLight(lightName, spawnAtCamera, parentToActiveObject);
                        }
                    }
                    else
                    {
                        m_editorUtils.Panel("ShowAdditionalLightSettings", ShowLightSettings, true);
                    }
                }
            }
            else if (activeObject == null)
            {
                lightName = m_editorUtils.TextField("LightName", lightName, helpEnabled);
                spawnAtCamera = m_editorUtils.Toggle("SpawnAtCamera", spawnAtCamera, helpEnabled);
                parentToActiveObject = m_editorUtils.Toggle("ParentToActiveObject", parentToActiveObject, helpEnabled);

                EditorGUILayout.Space();

                if (m_editorUtils.Button("Create Point Light"))
                {
                    CreatePointLight(lightName, spawnAtCamera, parentToActiveObject);
                }

                if (m_editorUtils.Button("Create Spot Light"))
                {
                    CreateSpotLight(lightName, spawnAtCamera, parentToActiveObject);
                }

                if (m_editorUtils.Button("Create Area Light"))
                {
                    CreateAreaLight(lightName, spawnAtCamera, parentToActiveObject);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_lightName = lightName;
                m_profiles.m_spawnAtCamera = spawnAtCamera;
                m_profiles.m_parentToActiveObject = parentToActiveObject;
            }
        }

        /// <summary>
        /// Interior lighting settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ShowExteriorLightingMode(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            #region Get Variables

            GameObject activeObject = Selection.activeGameObject;

            lightingEditMode = m_profiles.m_lightingEditMode;
            lightName = m_profiles.m_lightName;
            spawnAtCamera = m_profiles.m_spawnAtCamera;
            parentToActiveObject = m_profiles.m_parentToActiveObject;

            #endregion            

            EditorGUILayout.HelpBox("Exterior Mode currently has no extra features to Interior Mode. ffeatures will be comign soon", MessageType.Info);

            if (GetComponentOnObject(activeObject))
            {
                if (activeObject.GetComponent<Light>() == null)
                {
                    lightName = m_editorUtils.TextField("LightName", lightName, helpEnabled);
                    spawnAtCamera = m_editorUtils.Toggle("SpawnAtCamera", spawnAtCamera, helpEnabled);
                    parentToActiveObject = m_editorUtils.Toggle("ParentToActiveObject", parentToActiveObject, helpEnabled);

                    EditorGUILayout.Space();

                    if (m_editorUtils.Button("Create Point Light"))
                    {
                        CreatePointLight(lightName, spawnAtCamera, parentToActiveObject);
                    }

                    if (m_editorUtils.Button("Create Spot Light"))
                    {
                        CreateSpotLight(lightName, spawnAtCamera, parentToActiveObject);
                    }

                    if (m_editorUtils.Button("Create Area Light"))
                    {
                        CreateAreaLight(lightName, spawnAtCamera, parentToActiveObject);
                    }
                }
                else
                {
                    if (Selection.objects.Length > 1)
                    {
                        EditorGUILayout.HelpBox("Multiple editing mode not yet supported", MessageType.Warning);
                    }
                    else if (activeObject.GetComponent<Light>().type == LightType.Directional)
                    {
                        EditorGUILayout.HelpBox("To modify the Directional Light. Go to the Skies tab and Sun Settings", MessageType.Info);

                        lightName = m_editorUtils.TextField("LightName", lightName, helpEnabled);
                        spawnAtCamera = m_editorUtils.Toggle("SpawnAtCamera", spawnAtCamera, helpEnabled);
                        parentToActiveObject = m_editorUtils.Toggle("ParentToActiveObject", parentToActiveObject, helpEnabled);

                        EditorGUILayout.Space();

                        if (m_editorUtils.Button("Create Point Light"))
                        {
                            CreatePointLight(lightName, spawnAtCamera, parentToActiveObject);
                        }

                        if (m_editorUtils.Button("Create Spot Light"))
                        {
                            CreateSpotLight(lightName, spawnAtCamera, parentToActiveObject);
                        }

                        if (m_editorUtils.Button("Create Area Light"))
                        {
                            CreateAreaLight(lightName, spawnAtCamera, parentToActiveObject);
                        }
                    }
                    else
                    {
                        m_editorUtils.Panel("ShowAdditionalLightSettings", ShowLightSettings, true);
                    }
                }
            }
            else if (activeObject == null)
            {
                lightName = m_editorUtils.TextField("LightName", lightName, helpEnabled);
                spawnAtCamera = m_editorUtils.Toggle("SpawnAtCamera", spawnAtCamera, helpEnabled);
                parentToActiveObject = m_editorUtils.Toggle("ParentToActiveObject", parentToActiveObject, helpEnabled);

                EditorGUILayout.Space();

                if (m_editorUtils.Button("Create Point Light"))
                {
                    CreatePointLight(lightName, spawnAtCamera, parentToActiveObject);
                }

                if (m_editorUtils.Button("Create Spot Light"))
                {
                    CreateSpotLight(lightName, spawnAtCamera, parentToActiveObject);
                }

                if (m_editorUtils.Button("Create Area Light"))
                {
                    CreateAreaLight(lightName, spawnAtCamera, parentToActiveObject);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_lightName = lightName;
                m_profiles.m_spawnAtCamera = spawnAtCamera;
                m_profiles.m_parentToActiveObject = parentToActiveObject;
            }
        }

        /// <summary>
        /// Light settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ShowLightSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            #region Local Variables

            //Active Object
            GameObject activeObject = Selection.activeGameObject;

            //Point Light
            Light mainLight = null;
            string currentLightName = "Light";
            float lightRange = 10f;
            float lightIntensity = 1f;
            float lightIndirectMultiplier = 0f;
            float lightSpotAngle = 30f;
            Color lightColor = Color.white;
            Vector2 lightAreaSize = new Vector2(5f, 5f);
            LightType lightType = LightType.Point;
            LightmapBakeType lightBakeMode = LightmapBakeType.Realtime;
            LightShadows lightShadowType = LightShadows.Hard;
            LightRenderMode lightRenderMode = LightRenderMode.Auto;

            //Light system script
            float lightMinIntensity = 0f;
            int lightFlickerSmoothing = 5;

            #endregion

            //Get main light
            mainLight = Selection.activeGameObject.GetComponent<Light>();

            AmbientSkiesAdditionalLightSystem lightSystem = activeObject.GetComponent<AmbientSkiesAdditionalLightSystem>();
            if (mainLight.type == LightType.Area)
            {
                EditorGUILayout.HelpBox("Area lights are bake only light sources. 'Ambient Skies Additional Light System' will not be added to this light as it has to remain static.", MessageType.Info);
            }
            else
            {
                if (lightSystem == null)
                {
                    lightSystem = activeObject.AddComponent<AmbientSkiesAdditionalLightSystem>();
                }
            }

            if (mainLight.type == LightType.Point)
            {
                lightTypeMode = lightSystem.m_lightTypeMode;
                currentLightName = mainLight.gameObject.name;
                lightType = mainLight.type;
                lightRange = mainLight.range;
                lightIntensity = mainLight.intensity;
                lightColor = mainLight.color;
                lightBakeMode = mainLight.lightmapBakeType;
                lightShadowType = mainLight.shadows;
                lightRenderMode = mainLight.renderMode;

                currentLightName = m_editorUtils.TextField("LightName", currentLightName, helpEnabled);
                lightType = (LightType)m_editorUtils.EnumPopup("InteriorLightType", lightType, helpEnabled);
                lightTypeMode = (AmbientSkiesConsts.LightTypeMode)m_editorUtils.EnumPopup("SelectedLightMode", lightTypeMode, helpEnabled);
                EditorGUILayout.Space();

                m_editorUtils.Label("InteriorLightSettings");
                EditorGUI.indentLevel++;
                lightRange = m_editorUtils.FloatField("InteriorLightRange", lightRange, helpEnabled);
                lightIntensity = m_editorUtils.FloatField("InteriorLightIntensity", lightIntensity, helpEnabled);
                lightColor = m_editorUtils.ColorField("InteriorLightColor", lightColor, helpEnabled);
                lightBakeMode = (LightmapBakeType)m_editorUtils.EnumPopup("InteriorLightBakeMode", lightBakeMode, helpEnabled);
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Universal)
                {
                    if (lightBakeMode != LightmapBakeType.Baked)
                    {
                        EditorGUILayout.HelpBox(("'Realtime Shadows' are not yet supported on 'Point Lights' please use 'Baked' for the 'Bake Mode' instead to remove this warning"), MessageType.Warning);
                    }
                }
                lightShadowType = (LightShadows)m_editorUtils.EnumPopup("InteriorLightShadowMode", lightShadowType, helpEnabled);
                lightRenderMode = (LightRenderMode)m_editorUtils.EnumPopup("InteriorLightRenderMode", lightRenderMode, helpEnabled);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                if (lightTypeMode == AmbientSkiesConsts.LightTypeMode.FlickeringLight)
                {
                    lightMinIntensity = lightSystem.m_minIntensity;
                    lightFlickerSmoothing = lightSystem.m_smoothing;

                    m_editorUtils.Label("LightRenderModeSettings");

                    lightMinIntensity = m_editorUtils.Slider("MinIntensity", lightMinIntensity, 0f, lightIntensity - 0.1f, helpEnabled);
                    lightFlickerSmoothing = m_editorUtils.IntSlider("FlickerSmoothing", lightFlickerSmoothing, 0, 50, helpEnabled);
                }
                EditorGUILayout.Space();
                if (m_editorUtils.Button("DeleteLight"))
                {
                    DeleteSelectedObject(activeObject);
                }
                if (m_editorUtils.Button("Create Point Light"))
                {
                    CreatePointLight(lightName, spawnAtCamera, parentToActiveObject);
                }

                if (m_editorUtils.Button("Create Spot Light"))
                {
                    CreateSpotLight(lightName, spawnAtCamera, parentToActiveObject);
                }

                if (m_editorUtils.Button("Create Area Light"))
                {
                    CreateAreaLight(lightName, spawnAtCamera, parentToActiveObject);
                }
            }
            else if (mainLight.type == LightType.Spot)
            {
                lightTypeMode = lightSystem.m_lightTypeMode;
                currentLightName = mainLight.gameObject.name;
                lightRange = mainLight.range;
                lightSpotAngle = mainLight.spotAngle;
                lightType = mainLight.type;
                lightIntensity = mainLight.intensity;
                lightColor = mainLight.color;
                lightBakeMode = mainLight.lightmapBakeType;
                lightShadowType = mainLight.shadows;
                lightRenderMode = mainLight.renderMode;

                currentLightName = m_editorUtils.TextField("LightName", currentLightName, helpEnabled);
                lightType = (LightType)m_editorUtils.EnumPopup("InteriorLightType", lightType, helpEnabled);
                lightTypeMode = (AmbientSkiesConsts.LightTypeMode)m_editorUtils.EnumPopup("SelectedLightMode", lightTypeMode, helpEnabled);
                EditorGUILayout.Space();

                m_editorUtils.Label("InteriorLightSettings");
                EditorGUI.indentLevel++;
                lightRange = m_editorUtils.FloatField("InteriorLightRange", lightRange, helpEnabled);
                lightSpotAngle = m_editorUtils.Slider("InteriorLightSpotAngle", lightSpotAngle, 1f, 179f, helpEnabled);
                lightIntensity = m_editorUtils.FloatField("InteriorLightIntensity", lightIntensity, helpEnabled);
                lightColor = m_editorUtils.ColorField("InteriorLightColor", lightColor, helpEnabled);
                lightBakeMode = (LightmapBakeType)m_editorUtils.EnumPopup("InteriorLightBakeMode", lightBakeMode, helpEnabled);
                lightShadowType = (LightShadows)m_editorUtils.EnumPopup("InteriorLightShadowMode", lightShadowType, helpEnabled);
                lightRenderMode = (LightRenderMode)m_editorUtils.EnumPopup("InteriorLightRenderMode", lightRenderMode, helpEnabled);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                if (lightTypeMode == AmbientSkiesConsts.LightTypeMode.FlickeringLight)
                {
                    lightMinIntensity = lightSystem.m_minIntensity;
                    lightFlickerSmoothing = lightSystem.m_smoothing;

                    m_editorUtils.Label("LightRenderModeSettings");

                    lightMinIntensity = m_editorUtils.Slider("MinIntensity", lightMinIntensity, 0f, lightIntensity - 0.1f, helpEnabled);
                    lightFlickerSmoothing = m_editorUtils.IntSlider("FlickerSmoothing", lightFlickerSmoothing, 0, 50, helpEnabled);
                }
                EditorGUILayout.Space();
                if (m_editorUtils.Button("DeleteLight"))
                {
                    DeleteSelectedObject(activeObject);
                }
                if (m_editorUtils.Button("Create Point Light"))
                {
                    CreatePointLight(lightName, spawnAtCamera, parentToActiveObject);
                }

                if (m_editorUtils.Button("Create Spot Light"))
                {
                    CreateSpotLight(lightName, spawnAtCamera, parentToActiveObject);
                }

                if (m_editorUtils.Button("Create Area Light"))
                {
                    CreateAreaLight(lightName, spawnAtCamera, parentToActiveObject);
                }
            }
            else 
            {
                currentLightName = mainLight.gameObject.name;
                lightRange = mainLight.range;
                lightType = mainLight.type;
                lightIntensity = mainLight.intensity;
                lightColor = mainLight.color;
                lightIndirectMultiplier = mainLight.bounceIntensity;
                lightRenderMode = mainLight.renderMode;
                lightAreaSize = mainLight.areaSize;

                currentLightName = m_editorUtils.TextField("LightName", currentLightName, helpEnabled);
                lightType = (LightType)m_editorUtils.EnumPopup("InteriorLightType", lightType, helpEnabled);
                EditorGUILayout.Space();

                m_editorUtils.Label("InteriorLightSettings");
                EditorGUI.indentLevel++;
                m_editorUtils.LabelField("InteriorLightRange", new GUIContent(lightRange.ToString()));
                lightAreaSize = m_editorUtils.Vector2Field("InteriorLightAreaSize", lightAreaSize, helpEnabled);
                lightIntensity = m_editorUtils.FloatField("InteriorLightIntensity", lightIntensity, helpEnabled);
                lightIndirectMultiplier = m_editorUtils.Slider("InteriorIndirectLightIntensity", lightIndirectMultiplier, 0f, 5f, helpEnabled);
                lightColor = m_editorUtils.ColorField("InteriorLightColor", lightColor, helpEnabled);
                lightRenderMode = (LightRenderMode)m_editorUtils.EnumPopup("InteriorLightRenderMode", lightRenderMode, helpEnabled);
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                if (m_editorUtils.Button("DeleteLight"))
                {
                    DeleteSelectedObject(activeObject);
                }
                if (m_editorUtils.Button("Create Point Light"))
                {
                    CreatePointLight(lightName, spawnAtCamera, parentToActiveObject);
                }

                if (m_editorUtils.Button("Create Spot Light"))
                {
                    CreateSpotLight(lightName, spawnAtCamera, parentToActiveObject);
                }

                if (m_editorUtils.Button("Create Area Light"))
                {
                    CreateAreaLight(lightName, spawnAtCamera, parentToActiveObject);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (mainLight != null)
                {
                    if (mainLight.type == LightType.Point || mainLight.type == LightType.Spot)
                    {
                        if (lightSystem == null)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }

                mainLight.gameObject.name = currentLightName;
                if (lightType == LightType.Directional)
                {
                    Debug.LogWarning("Light Type 'Directional' is not supported in this tab. Use Skies tab if you want to use/modify a Directional. Switching to point light.");
                    lightType = LightType.Point;
                    mainLight.type = lightType;
                }
                else
                {
                    mainLight.type = lightType;
                }
                mainLight.range = lightRange;
                mainLight.spotAngle = lightSpotAngle;
                mainLight.intensity = lightIntensity;
                mainLight.bounceIntensity = lightIndirectMultiplier;
                mainLight.color = lightColor;
                mainLight.lightmapBakeType = lightBakeMode;
                mainLight.shadows = lightShadowType;
                mainLight.renderMode = lightRenderMode;
                mainLight.areaSize = lightAreaSize;

                if (lightSystem != null)
                {
                    lightSystem.m_lightTypeMode = lightTypeMode;
                    lightSystem.m_maxIntensity = lightIntensity;
                    lightSystem.m_minIntensity = lightMinIntensity;
                    lightSystem.m_smoothing = lightFlickerSmoothing;
                    lightSystem.m_mainLight = mainLight;

                    if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                    {
#if HDPipeline
                        HDAdditionalLightData lightData = mainLight.GetComponent<HDAdditionalLightData>();
                        if (lightData == null)
                        {
                            lightData = mainLight.gameObject.AddComponent<HDAdditionalLightData>();
                        }

                        lightData.lightUnit = LightUnit.Lux;
                        lightData.color = lightColor;
                        lightData.intensity = lightIntensity;
#endif
                    }

                    if (lightTypeMode == AmbientSkiesConsts.LightTypeMode.ToggleLight)
                    {
                        lightSystem.m_isToggleLight = true;
                    }
                    else
                    {
                        lightSystem.m_isToggleLight = false;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new point light in the scene
        /// </summary>
        /// <param name="lightName"></param>
        /// <param name="spawnAtCameraLocation"></param>
        private void CreatePointLight(string lightName, bool spawnAtCameraLocation, bool parentToActiveObject)
        {
            GameObject newPointLight = new GameObject(lightName + " (Point)");
            if (spawnAtCameraLocation)
            {
                newPointLight.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
            }

            GameObject parentObject = InteriorParanting(true);
            if (parentToActiveObject)
            {
                GameObject activeObject = Selection.activeGameObject;
                if (activeObject != null)
                {
                    newPointLight.transform.SetParent(activeObject.transform);
                }
                else
                {
                    newPointLight.transform.SetParent(parentObject.transform);
                }
            }
            else
            {
                newPointLight.transform.SetParent(parentObject.transform);
            }

            Light light = newPointLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 10f;
            light.color = Color.white;
            light.lightmapBakeType = LightmapBakeType.Realtime;
            light.intensity = 1f;
            light.bounceIntensity = 0f;
            light.shadows = LightShadows.Hard;
            light.renderMode = LightRenderMode.Auto;

            Selection.activeObject = newPointLight;
        }

        /// <summary>
        /// Creates a new point light in the scene
        /// </summary>
        /// <param name="lightName"></param>
        /// <param name="spawnAtCameraLocation"></param>
        private void CreateSpotLight(string lightName, bool spawnAtCameraLocation, bool parentToActiveObject)
        {
            GameObject newPointLight = new GameObject(lightName + " (Spot)");
            if (spawnAtCameraLocation)
            {
                var transform = SceneView.lastActiveSceneView.camera.transform;
                newPointLight.transform.position = transform.position;
                newPointLight.transform.rotation = transform.rotation;
            }

            GameObject parentObject = InteriorParanting(true);
            if (parentToActiveObject)
            {
                GameObject activeObject = Selection.activeGameObject;
                if (activeObject != null)
                {
                    newPointLight.transform.SetParent(activeObject.transform);
                }
                else
                {
                    newPointLight.transform.SetParent(parentObject.transform);
                }
            }
            else
            {
                newPointLight.transform.SetParent(parentObject.transform);
            }

            Light light = newPointLight.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 10f;
            light.spotAngle = 30f;
            light.color = Color.white;
            light.lightmapBakeType = LightmapBakeType.Realtime;
            light.intensity = 1f;
            light.bounceIntensity = 0f;
            light.shadows = LightShadows.Hard;
            light.renderMode = LightRenderMode.Auto;

            Selection.activeObject = newPointLight;
        }

        /// <summary>
        /// Creates a new point light in the scene
        /// </summary>
        /// <param name="lightName"></param>
        /// <param name="spawnAtCameraLocation"></param>
        private void CreateAreaLight(string lightName, bool spawnAtCameraLocation, bool parentToActiveObject)
        {
            GameObject newPointLight = new GameObject(lightName + " (Area)");
            if (spawnAtCameraLocation)
            {
                newPointLight.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
            }

            GameObject parentObject = InteriorParanting(true);
            if (parentToActiveObject)
            {
                GameObject activeObject = Selection.activeGameObject;
                if (activeObject != null)
                {
                    newPointLight.transform.SetParent(activeObject.transform);
                }
                else
                {
                    newPointLight.transform.SetParent(parentObject.transform);
                }
            }
            else
            {
                newPointLight.transform.SetParent(parentObject.transform);
            }

            Light light = newPointLight.AddComponent<Light>();
            light.type = LightType.Rectangle;
            light.areaSize = new Vector2(10f, 10f);
            light.color = Color.white;
            light.lightmapBakeType = LightmapBakeType.Baked;
            light.intensity = 1f;
            light.bounceIntensity = 1f;
            light.renderMode = LightRenderMode.Auto;

            Selection.activeObject = newPointLight;
        }

        /// <summary>
        /// Deletes the active object selected
        /// </summary>
        /// <param name="activeObject"></param>
        private void DeleteSelectedObject(GameObject activeObject)
        {
            if (activeObject != null)
            {
                Selection.activeObject = null;
                DestroyImmediate(activeObject);

                DestroyInteriorParanting();
            }
        }

        /// <summary>
        /// Creates the interior light parent
        /// </summary>
        /// <param name="parentToAmbientSkies"></param>
        private GameObject InteriorParanting(bool parentToAmbientSkies)
        {
            GameObject mainParent = GameObject.Find("Interior Lighting");
            if (mainParent == null)
            {
                mainParent = new GameObject("Interior Lighting");
            }

            if (parentToAmbientSkies)
            {
                GameObject ambientSkiesParent = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Global", false);

                mainParent.transform.SetParent(ambientSkiesParent.transform);
            }

            return mainParent;
        }

        /// <summary>
        /// Find parent object and destroys it if it's empty
        /// </summary>
        /// <param name="parentGameObject"></param>
        private void DestroyInteriorParanting()
        {
            //Find parent object
            GameObject parentObject = GameObject.Find("Interior Lighting");
            if (parentObject != null)
            {
                //Find parents in parent object
                Transform[] parentChilds = parentObject.GetComponentsInChildren<Transform>();
                if (parentChilds.Length == 1)
                {
                    //Destroy object if object is empty
                    Object.DestroyImmediate(parentObject);
                }
            }
        }

        /// <summary>
        /// Get components and return if true
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <returns></returns>
        private bool GetComponentOnObject(GameObject objectToCheck)
        {
            bool isActive = false;

            if (objectToCheck == null)
            {
                return isActive;
            }

            if (objectToCheck.GetComponent<Component>() != null)
            {
                isActive = true;
            }

            return isActive;
        }

        #endregion

        #region Post FX Tab Functions

        /// <summary>
        /// Main settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void MainPostProcessSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Selection popup
            hideGizmos = m_editorUtils.Toggle("HidePostProcessingGizmos", hideGizmos, helpEnabled);
            antiAliasingMode = (AmbientSkiesConsts.AntiAliasingMode)m_editorUtils.EnumPopup("AntiAliasingMode", antiAliasingMode, helpEnabled);
            if (antiAliasingMode == AmbientSkiesConsts.AntiAliasingMode.TAA && renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Universal)
            {
                EditorGUILayout.HelpBox("TAA (Temporal Anti-Aliasing) may cause artifacts in Lightweight Render Pipeline. Recommend using SMAA (Subpixel Morphological Anti-Aliasing) or MSAA (Multisample Anti-Aliasing) instead.", MessageType.Info);
            }
            hDRMode = (AmbientSkiesConsts.HDRMode)m_editorUtils.EnumPopup("HDRMode", hDRMode, helpEnabled);

            if (!autoMatchProfile)
            {
                //Match profile to sky profile
                if (m_editorUtils.Button("MatchProfileToSkyboxProfile"))
                {
                    if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
                        m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingHDRIName(m_profiles, m_selectedSkyboxProfile);
                        newPPSelection = m_selectedPostProcessingProfileIndex;
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                        EditorUtility.SetDirty(m_creationToolSettings);
                        m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                    }
                    else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingProceduralName(m_profiles, m_selectedProceduralSkyboxProfile);
                        newPPSelection = m_selectedPostProcessingProfileIndex;
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                        EditorUtility.SetDirty(m_creationToolSettings);
                        m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                    }
                    else
                    {
                        m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingGradientName(m_profiles, m_selectedGradientSkyboxProfile);
                        newPPSelection = m_selectedPostProcessingProfileIndex;
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                        EditorUtility.SetDirty(m_creationToolSettings);
                        m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                    }
                }
            }

            if (m_editorUtils.Button("FocusProfile"))
            {
                PostProcessingUtils.FocusPostProcessProfile(m_profiles, m_selectedPostProcessingProfile);
            }

            m_editorUtils.InlineHelp("FocusProfileHelp", helpEnabled);

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.hideGizmos = hideGizmos;
                m_selectedPostProcessingProfile.autoMatchProfile = autoMatchProfile;
                m_selectedPostProcessingProfile.antiAliasingMode = antiAliasingMode;
                m_selectedPostProcessingProfile.hDRMode = hDRMode;
#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.customPostProcessingProfile = customPostProcessingProfile;
#endif

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Profile settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void PostProcessingProfileSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            EditorGUILayout.BeginHorizontal();

            m_editorUtils.Text("SelectPostProcesssingDropdown", GUILayout.Width(146f));
            newPPSelection = EditorGUILayout.Popup(m_selectedPostProcessingProfileIndex, ppChoices.ToArray(), GUILayout.MaxWidth(10000f), GUILayout.Height(16f));

            m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[newPPSelection];

            //Prev / Next
            //EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (newPPSelection == 0)
            {
                GUI.enabled = false;
            }
            if (m_editorUtils.Button("PrevPostProcessingButton"))
            {
                newPPSelection--;
                AmbientSkiesEditorFunctions.SetAllPostFxUpdateToTrue(m_profiles);
            }
            GUI.enabled = true;
            if (newPPSelection == ppChoices.Count - 1)
            {
                GUI.enabled = false;
            }
            if (m_editorUtils.Button("NextPostProcessingButton"))
            {
                newPPSelection++;
                AmbientSkiesEditorFunctions.SetAllPostFxUpdateToTrue(m_profiles);
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            #region Creation Mode

            //Creation
            if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
            {
                if (m_profiles.m_ppProfiles.Count == 1)
                {
                    GUI.enabled = false;
                }

                if (m_editorUtils.Button("RemoveNewProfile"))
                {
                    m_profiles.m_ppProfiles.Remove(m_selectedPostProcessingProfile);

                    m_newPostProcessingIndex--;

                    //Add post processing profile names
                    ppChoices.Clear();
                    foreach (var profile in m_profiles.m_ppProfiles)
                    {
                        ppChoices.Add(profile.name);
                    }

                    newPPSelection--;
                    m_selectedPostProcessingProfileIndex--;

                    Repaint();

                    return;
                }

                GUI.enabled = true;

                if (m_editorUtils.Button("CreateNewProfile"))
                {
                    AmbientPostProcessingProfile newProfile = new AmbientPostProcessingProfile();
                    m_profiles.m_ppProfiles.Add(newProfile);
                    newProfile.name = "New Post Processing Profile" + m_newPostProcessingIndex;

                    m_newPostProcessingIndex++;

                    //Add post processing profile names
                    ppChoices.Clear();
                    foreach (var profile in m_profiles.m_ppProfiles)
                    {
                        ppChoices.Add(profile.name);
                    }

                    newPPSelection++;
                    m_selectedPostProcessingProfileIndex++;

                    Repaint();

                    return;
                }
            }

            #endregion

            //if (m_editorUtils.Button("CreateVolume"))
            //{
            //    PostProcessProfile processProfile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(SkyboxUtils.GetAssetPath("Ambient Skies " + ppChoices[newPPSelection] + " Post Processing"));
            //    Debug.Log(ppChoices[newPPSelection]);
            //    GameObject newObject = AmbientSkiesEditorFunctions.CreateVolumeFromPostProcessingV2Profile(processProfile);
            //    Selection.activeObject = newObject;
            //}

            if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
            {
                m_foldoutCreationModeSettings = m_editorUtils.Panel("ShowCreationModeSettings", CreationPostProcessMode, m_foldoutCreationModeSettings);
            }

#if UNITY_2019_1_OR_NEWER && HDPipeline
            if (usePostProcess)
            {
                m_foldoutAmbientOcclusion = m_editorUtils.Panel("Show Ambient Occlusion Settings", HDRPAmbientOcclusionSettingsEnabled, m_foldoutAmbientOcclusion);
                m_foldoutAutoExposure = m_editorUtils.Panel("Show Auto Exposure Settings", HDRPAutoExposureSettingsEnabled, m_foldoutAutoExposure);
                m_foldoutBloom = m_editorUtils.Panel("Show Bloom Settings", HDRPBloomSettingsEnabled, m_foldoutBloom);
                m_foldoutChromaticAberration = m_editorUtils.Panel("Show Chromatic Aberration Settings", HDRPChromaticAberrationSettingsEnabled, m_foldoutChromaticAberration);
                m_foldoutColorGrading = m_editorUtils.Panel("Show Color Grading Settings", HDRPColorGradingSettingsEnabled, m_foldoutColorGrading);
                m_foldoutDepthOfField = m_editorUtils.Panel("Show Depth Of Field Settings", HDRPDepthOfFieldSettingsEnabled, m_foldoutDepthOfField);
                m_foldoutGrain = m_editorUtils.Panel("Show Grain Settings", HDRPGrainSettingsEnabled, m_foldoutGrain);
                m_foldoutLensDistortion = m_editorUtils.Panel("Show Lens Distortion Settings", HDRPLensDistortionSettingsEnabled, m_foldoutLensDistortion);
                m_foldoutMotionBlur = m_editorUtils.Panel("Show Motion Blur Settings", HDRPMotionBlurSettingsEnabled, m_foldoutMotionBlur);
                m_foldoutPaniniProjection = m_editorUtils.Panel("Show Panini Projection Settings", HDRPPaniniProjectionSettingsEnabled, m_foldoutPaniniProjection);
                m_foldoutVignette = m_editorUtils.Panel("Show Vignette Settings", HDRPVignetteSettingsEnabled, m_foldoutVignette);
            }
#elif UNITY_2019_3_OR_NEWER && UPPipeline
            if (usePostProcess)
            {
                if (m_selectedPostProcessingProfile.name != "User")
                {
                    //m_foldoutAmbientOcclusion = m_editorUtils.Panel("Show Ambient Occlusion Settings", URPAmbientOcclusionSettingsEnabled, m_foldoutAmbientOcclusion);
                    //m_foldoutAutoExposure = m_editorUtils.Panel("Show Auto Exposure Settings", URPAutoExposureSettingsEnabled, m_foldoutAutoExposure);
                    m_foldoutBloom = m_editorUtils.Panel("Show Bloom Settings", URPBloomSettingsEnabled, m_foldoutBloom);
                    m_foldoutChromaticAberration = m_editorUtils.Panel("Show Chromatic Aberration Settings", URPChromaticAberrationSettingsEnabled, m_foldoutChromaticAberration);
                    m_foldoutColorGrading = m_editorUtils.Panel("Show Color Grading Settings", URPColorGradingSettingsEnabled, m_foldoutColorGrading);
                    m_foldoutDepthOfField = m_editorUtils.Panel("Show Depth Of Field Settings", URPDepthOfFieldSettingsEnabled, m_foldoutDepthOfField);
                    m_foldoutGrain = m_editorUtils.Panel("Show Grain Settings", URPGrainSettingsEnabled, m_foldoutGrain);
                    m_foldoutLensDistortion = m_editorUtils.Panel("Show Lens Distortion Settings", URPLensDistortionSettingsEnabled, m_foldoutLensDistortion);
                    m_foldoutMotionBlur = m_editorUtils.Panel("Show Motion Blur Settings", URPMotionBlurSettingsEnabled, m_foldoutMotionBlur);
                    m_foldoutPaniniProjection = m_editorUtils.Panel("Show Panini Projection Settings", URPPaniniProjectionSettingsEnabled, m_foldoutPaniniProjection);
                    m_foldoutVignette = m_editorUtils.Panel("Show Vignette Settings", URPVignetteSettingsEnabled, m_foldoutVignette);
                }
                else
                {
                    m_foldoutCustomPPProfile = m_editorUtils.Panel("Show Custom Settings", URPCustomProfileSettings, m_foldoutCustomPPProfile);
                }
            }
#else
            //If using post processing
            if (usePostProcess)
            {
                if (m_selectedPostProcessingProfile.name != "User")
                {
                    m_foldoutAmbientOcclusion = m_editorUtils.Panel("Show Ambient Occlusion Settings", AmbientOcclusionSettingsEnabled, m_foldoutAmbientOcclusion);
                    m_foldoutAutoExposure = m_editorUtils.Panel("Show Auto Exposure Settings", AutoExposureSettingsEnabled, m_foldoutAutoExposure);
                    m_foldoutBloom = m_editorUtils.Panel("Show Bloom Settings", BloomSettingsEnabled, m_foldoutBloom);
                    m_foldoutChromaticAberration = m_editorUtils.Panel("Show Chromatic Aberration Settings", ChromaticAberrationSettingsEnabled, m_foldoutChromaticAberration);
                    m_foldoutColorGrading = m_editorUtils.Panel("Show Color Grading Settings", ColorGradingSettingsEnabled, m_foldoutColorGrading);
                    m_foldoutDepthOfField = m_editorUtils.Panel("Show Depth Of Field Settings", DepthOfFieldSettingsEnabled, m_foldoutDepthOfField);
                    m_foldoutGrain = m_editorUtils.Panel("Show Grain Settings", GrainSettingsEnabled, m_foldoutGrain);
                    m_foldoutLensDistortion = m_editorUtils.Panel("Show Lens Distortion Settings", LensDistortionSettingsEnabled, m_foldoutLensDistortion);
                    if (m_massiveCloudsPath)
                    {
                        m_foldoutMassiveClouds = m_editorUtils.Panel("Show Massive Clouds Settings", MassiveCloudsSettingsEnabled, m_foldoutMassiveClouds);
                    }
                    m_foldoutMotionBlur = m_editorUtils.Panel("Show Motion Blur Settings", MotionBlurSettingsEnabled, m_foldoutMotionBlur);
                    m_foldoutScreenSpaceReflections = m_editorUtils.Panel("Show Screen Space Reflection Settings", ScreenSpaceReflectionsSettingsEnabled, m_foldoutScreenSpaceReflections);
                    m_foldoutVignette = m_editorUtils.Panel("Show Vignette Settings", VignetteSettingsEnabled, m_foldoutVignette);
                }
                else
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    customPostProcessingProfile = (PostProcessProfile)m_editorUtils.ObjectField("CustomPostProcessingProfile", customPostProcessingProfile, typeof(PostProcessProfile), false, helpEnabled, GUILayout.Height(16f));
#endif
                }
            }
#endif
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                //Selection changing things - exit immediately to not polute settings
                if (newPPSelection != m_selectedPostProcessingProfileIndex)
                {
                    m_selectedPostProcessingProfileIndex = newPPSelection;
                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                    AmbientSkiesEditorFunctions.SetAllPostFxUpdateToTrue(m_profiles);
                }
#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.customPostProcessingProfile = customPostProcessingProfile;
#endif

#if HDPipeline && UNITY_2019_1_OR_NEWER
                m_selectedPostProcessingProfile.customHDRPPostProcessingprofile = customHDRPPostProcessingprofile;
#endif
                if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition || renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.Universal)
                {
                    #region Post Processing Values
                    //Global systems
                    systemtype = m_profiles.m_systemTypes;

                    //Enable Post Fx
                    usePostProcess = m_profiles.m_usePostFX;

                    //Selection
                    newPPSelection = m_selectedPostProcessingProfile.profileIndex;

                    //Hide Gizmo
                    hideGizmos = m_selectedPostProcessingProfile.hideGizmos;

                    //Custom profile
#if UNITY_POST_PROCESSING_STACK_V2
                    customPostProcessingProfile = m_selectedPostProcessingProfile.customPostProcessingProfile;
#endif
                    //HDR Mode
                    hDRMode = m_selectedPostProcessingProfile.hDRMode;

                    //Anti Aliasing Mode
                    antiAliasingMode = m_selectedPostProcessingProfile.antiAliasingMode;

                    //Target Platform
                    targetPlatform = m_profiles.m_targetPlatform;

                    if (m_selectedPostProcessingProfile.name != "User")
                    {
                        //AO settings
                        aoEnabled = m_selectedPostProcessingProfile.aoEnabled;
                        aoAmount = m_selectedPostProcessingProfile.aoAmount;
                        aoColor = m_selectedPostProcessingProfile.aoColor;
#if UNITY_POST_PROCESSING_STACK_V2
                        ambientOcclusionMode = m_selectedPostProcessingProfile.ambientOcclusionMode;
#endif
                        //Exposure settings
                        autoExposureEnabled = m_selectedPostProcessingProfile.autoExposureEnabled;
                        exposureAmount = m_selectedPostProcessingProfile.exposureAmount;
                        exposureMin = m_selectedPostProcessingProfile.exposureMin;
                        exposureMax = m_selectedPostProcessingProfile.exposureMax;

                        //Bloom settings
                        bloomEnabled = m_selectedPostProcessingProfile.bloomEnabled;
                        bloomIntensity = m_selectedPostProcessingProfile.bloomAmount;
                        bloomThreshold = m_selectedPostProcessingProfile.bloomThreshold;
                        lensIntensity = m_selectedPostProcessingProfile.lensIntensity;
                        lensTexture = m_selectedPostProcessingProfile.lensTexture;

                        //Chromatic Aberration
                        chromaticAberrationEnabled = m_selectedPostProcessingProfile.chromaticAberrationEnabled;
                        chromaticAberrationIntensity = m_selectedPostProcessingProfile.chromaticAberrationIntensity;

                        //Color Grading settings
                        colorGradingEnabled = m_selectedPostProcessingProfile.colorGradingEnabled;
#if UNITY_POST_PROCESSING_STACK_V2
                        colorGradingMode = m_selectedPostProcessingProfile.colorGradingMode;
#endif
                        colorGradingLut = m_selectedPostProcessingProfile.colorGradingLut;
                        colorGradingPostExposure = m_selectedPostProcessingProfile.colorGradingPostExposure;
                        colorGradingColorFilter = m_selectedPostProcessingProfile.colorGradingColorFilter;
                        colorGradingTempature = m_selectedPostProcessingProfile.colorGradingTempature;
                        colorGradingTint = m_selectedPostProcessingProfile.colorGradingTint;
                        colorGradingSaturation = m_selectedPostProcessingProfile.colorGradingSaturation;
                        colorGradingContrast = m_selectedPostProcessingProfile.colorGradingContrast;

                        //DOF settings
                        depthOfFieldMode = m_selectedPostProcessingProfile.depthOfFieldMode;
                        depthOfFieldEnabled = m_selectedPostProcessingProfile.depthOfFieldEnabled;
                        depthOfFieldFocusDistance = m_selectedPostProcessingProfile.depthOfFieldFocusDistance;
                        depthOfFieldAperture = m_selectedPostProcessingProfile.depthOfFieldAperture;
                        depthOfFieldFocalLength = m_selectedPostProcessingProfile.depthOfFieldFocalLength;
                        depthOfFieldTrackingType = m_selectedPostProcessingProfile.depthOfFieldTrackingType;
                        focusOffset = m_selectedPostProcessingProfile.focusOffset;
                        targetLayer = m_selectedPostProcessingProfile.targetLayer;
                        maxFocusDistance = m_selectedPostProcessingProfile.maxFocusDistance;
#if UNITY_POST_PROCESSING_STACK_V2
                        maxBlurSize = m_selectedPostProcessingProfile.maxBlurSize;
#endif
                        //Distortion settings
                        distortionEnabled = m_selectedPostProcessingProfile.distortionEnabled;
                        distortionIntensity = m_selectedPostProcessingProfile.distortionIntensity;
                        distortionScale = m_selectedPostProcessingProfile.distortionScale;

                        //Grain settings
                        grainEnabled = m_selectedPostProcessingProfile.grainEnabled;
                        grainIntensity = m_selectedPostProcessingProfile.grainIntensity;
                        grainSize = m_selectedPostProcessingProfile.grainSize;

                        //SSR settings
                        screenSpaceReflectionsEnabled = m_selectedPostProcessingProfile.screenSpaceReflectionsEnabled;
                        maximumIterationCount = m_selectedPostProcessingProfile.maximumIterationCount;
                        thickness = m_selectedPostProcessingProfile.thickness;
#if UNITY_POST_PROCESSING_STACK_V2
                        screenSpaceReflectionResolution = m_selectedPostProcessingProfile.spaceReflectionResolution;
                        screenSpaceReflectionPreset = m_selectedPostProcessingProfile.screenSpaceReflectionPreset;
#endif
                        maximumMarchDistance = m_selectedPostProcessingProfile.maximumMarchDistance;
                        distanceFade = m_selectedPostProcessingProfile.distanceFade;
                        screenSpaceVignette = m_selectedPostProcessingProfile.screenSpaceVignette;

                        //Vignette settings
                        vignetteEnabled = m_selectedPostProcessingProfile.vignetteEnabled;
                        vignetteIntensity = m_selectedPostProcessingProfile.vignetteIntensity;
                        vignetteSmoothness = m_selectedPostProcessingProfile.vignetteSmoothness;

                        //Motion Blur settings
                        motionBlurEnabled = m_selectedPostProcessingProfile.motionBlurEnabled;
                        motionShutterAngle = m_selectedPostProcessingProfile.shutterAngle;
                        motionSampleCount = m_selectedPostProcessingProfile.sampleCount;
#if Mewlist_Clouds
                        //Massive Cloud Settings
                        massiveCloudsEnabled = m_selectedPostProcessingProfile.massiveCloudsEnabled;
                        cloudProfile = m_selectedPostProcessingProfile.cloudProfile;
                        syncGlobalFogColor = m_selectedPostProcessingProfile.syncGlobalFogColor;
                        syncBaseFogColor = m_selectedPostProcessingProfile.syncBaseFogColor;
                        cloudsFogColor = m_selectedPostProcessingProfile.cloudsFogColor;
                        cloudsBaseFogColor = m_selectedPostProcessingProfile.cloudsBaseFogColor;
                        cloudIsHDRP = m_selectedPostProcessingProfile.cloudIsHDRP;
#endif
                    }

                    #endregion
                }
                else if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Universal)
                {
                    #region URP Post Processing Values

                    //Global systems
                    systemtype = m_profiles.m_systemTypes;

                    //Enable Post Fx
                    usePostProcess = m_profiles.m_usePostFX;

                    //Selection
                    newPPSelection = m_selectedPostProcessingProfile.profileIndex;
                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                    autoMatchProfile = m_profiles.m_autoMatchProfile;

                    //Creation mode
#if UNITY_POST_PROCESSING_STACK_V2
                    m_postProfileName = m_selectedPostProcessingProfile.name;
                    m_postProcessAssetName = m_selectedPostProcessingProfile.creationPostProcessProfile;
#endif

#if UPPipeline && UNITY_2019_3_OR_NEWER

                    hideGizmos = m_selectedPostProcessingProfile.hideGizmos;
                    antiAliasingMode = m_selectedPostProcessingProfile.antiAliasingMode;
                    dithering = m_selectedPostProcessingProfile.dithering;
                    hDRMode = m_selectedPostProcessingProfile.hDRMode;
                    customURPPostProcessingprofile = m_selectedPostProcessingProfile.customURPPostProcessingprofile;

                    bloomEnabled = m_selectedPostProcessingProfile.bloomEnabled;
                    uRPBloomIntensity = m_selectedPostProcessingProfile.uRPBloomIntensity;
                    uRPBloomScatter = m_selectedPostProcessingProfile.uRPBloomScatter;
                    uRPBloomTint = m_selectedPostProcessingProfile.uRPBloomTint;
                    uRPBloomDirtLensTexture = m_selectedPostProcessingProfile.uRPBloomDirtLensTexture;
                    uRPBloomDirtLensIntensity = m_selectedPostProcessingProfile.uRPBloomDirtLensIntensity;
                    uRPBloomHighQualityFiltering = m_selectedPostProcessingProfile.uRPBloomHighQualityFiltering;
                    uRPBloomPrefiler = m_selectedPostProcessingProfile.uRPBloomPrefiler;
                    uRPBloomAnamorphic = m_selectedPostProcessingProfile.uRPBloomAnamorphic;

                    chromaticAberrationEnabled = m_selectedPostProcessingProfile.chromaticAberrationEnabled;
                    uRPChromaticAberrationIntensity = m_selectedPostProcessingProfile.uRPChromaticAberrationIntensity;

                    colorGradingEnabled = m_selectedPostProcessingProfile.colorGradingEnabled;
                    uRPColorLookupTexture = m_selectedPostProcessingProfile.uRPColorLookupTexture;
                    uRPColorAdjustmentColorFilter = m_selectedPostProcessingProfile.uRPColorAdjustmentColorFilter;
                    uRPColorAdjustmentPostExposure = m_selectedPostProcessingProfile.uRPColorAdjustmentPostExposure;
                    uRPWhiteBalanceTempature = m_selectedPostProcessingProfile.uRPWhiteBalanceTempature;
                    uRPColorLookupContribution = m_selectedPostProcessingProfile.uRPColorLookupContribution;
                    uRPWhiteBalanceTint = m_selectedPostProcessingProfile.uRPWhiteBalanceTint;
                    uRPColorAdjustmentSaturation = m_selectedPostProcessingProfile.uRPColorAdjustmentSaturation;
                    uRPColorAdjustmentContrast = m_selectedPostProcessingProfile.uRPColorAdjustmentContrast;
                    uRPChannelMixerRed = m_selectedPostProcessingProfile.uRPChannelMixerRed;
                    uRPChannelMixerGreen = m_selectedPostProcessingProfile.uRPChannelMixerGreen;
                    uRPChannelMixerBlue = m_selectedPostProcessingProfile.uRPChannelMixerBlue;
                    uRPTonemappingMode = m_selectedPostProcessingProfile.uRPTonemappingMode;
                    uRPTonemappingToeStrength = m_selectedPostProcessingProfile.uRPTonemappingToeStrength;
                    uRPTonemappingToeLength = m_selectedPostProcessingProfile.uRPTonemappingToeLength;
                    uRPTonemappingShoulderStrength = m_selectedPostProcessingProfile.uRPTonemappingShoulderStrength;
                    uRPTonemappingShoulderLength = m_selectedPostProcessingProfile.uRPTonemappingShoulderLength;
                    uRPTonemappingShoulderAngle = m_selectedPostProcessingProfile.uRPTonemappingShoulderAngle;
                    uRPTonemappingGamma = m_selectedPostProcessingProfile.uRPTonemappingGamma;
                    uRPSplitToningShadows = m_selectedPostProcessingProfile.uRPSplitToningShadows;
                    uRPSplitToningHighlights = m_selectedPostProcessingProfile.uRPSplitToningHighlights;
                    uRPSplitToningBalance = m_selectedPostProcessingProfile.uRPSplitToningBalance;

                    depthOfFieldEnabled = m_selectedPostProcessingProfile.depthOfFieldEnabled;
                    depthOfFieldMode = m_selectedPostProcessingProfile.depthOfFieldMode;
                    depthOfFieldFocusDistance = m_selectedPostProcessingProfile.depthOfFieldFocusDistance;
                    depthOfFieldTrackingType = m_selectedPostProcessingProfile.depthOfFieldTrackingType;
                    focusOffset = m_selectedPostProcessingProfile.focusOffset;
                    targetLayer = m_selectedPostProcessingProfile.targetLayer;
                    maxFocusDistance = m_selectedPostProcessingProfile.maxFocusDistance;
                    uRPDepthOfFieldMode = m_selectedPostProcessingProfile.uRPDepthOfFieldMode;
                    //Gaussian
                    uRPDepthOfFieldStart = m_selectedPostProcessingProfile.uRPDepthOfFieldStart;
                    uRPDepthOfFieldEnd = m_selectedPostProcessingProfile.uRPDepthOfFieldEnd;
                    uRPDepthOfFieldMaxRadius = m_selectedPostProcessingProfile.uRPDepthOfFieldMaxRadius;
                    uRPDepthOfFieldHighQualityFiltering = m_selectedPostProcessingProfile.uRPDepthOfFieldHighQualityFiltering;
                    //Bokeh
                    uRPDepthOfFieldFocusDitance = m_selectedPostProcessingProfile.uRPDepthOfFieldFocusDitance;
                    uRPDepthOfFieldFocalLength = m_selectedPostProcessingProfile.uRPDepthOfFieldFocalLength;
                    uRPDepthOfFieldAperture = m_selectedPostProcessingProfile.uRPDepthOfFieldAperture;
                    uRPDepthOfFieldBladeCount = m_selectedPostProcessingProfile.uRPDepthOfFieldBladeCount;
                    uRPDepthOfFieldBladeCurvature = m_selectedPostProcessingProfile.uRPDepthOfFieldBladeCurvature;
                    uRPDepthOfFieldBladeRotation = m_selectedPostProcessingProfile.uRPDepthOfFieldBladeRotation;

                    grainEnabled = m_selectedPostProcessingProfile.grainEnabled;
                    uRPFilmGrainType = m_selectedPostProcessingProfile.uRPFilmGrainType;
                    uRPFilmGrainIntensity = m_selectedPostProcessingProfile.uRPFilmGrainIntensity;
                    uRPFilmGrainResponse = m_selectedPostProcessingProfile.uRPFilmGrainResponse;

                    distortionEnabled = m_selectedPostProcessingProfile.distortionEnabled;
                    uRPLensDistortionIntensity = m_selectedPostProcessingProfile.uRPLensDistortionIntensity;
                    uRPLensDistortionXMultiplier = m_selectedPostProcessingProfile.uRPLensDistortionXMultiplier;
                    uRPLensDistortionYMultiplier = m_selectedPostProcessingProfile.uRPLensDistortionYMultiplier;
                    uRPLensDistortionCenter = m_selectedPostProcessingProfile.uRPLensDistortionCenter;
                    uRPLensDistortionScale = m_selectedPostProcessingProfile.uRPLensDistortionScale;

                    vignetteEnabled = m_selectedPostProcessingProfile.vignetteEnabled;
                    uRPVignetteColor = m_selectedPostProcessingProfile.uRPVignetteColor;
                    uRPVignetteCenter = m_selectedPostProcessingProfile.uRPVignetteCenter;
                    uRPVignetteIntensity = m_selectedPostProcessingProfile.uRPVignetteIntensity;
                    uRPVignetteSmoothness = m_selectedPostProcessingProfile.uRPVignetteSmoothness;
                    uRPVignetteRoundness = m_selectedPostProcessingProfile.uRPVignetteRoundness;
                    uRPVignetteRounded = m_selectedPostProcessingProfile.uRPVignetteRounded;
                    uRPVignetteMask = m_selectedPostProcessingProfile.uRPVignetteMask;
                    uRPVignetteMaskOpacity = m_selectedPostProcessingProfile.uRPVignetteMaskOpacity;

                    motionBlurEnabled = m_selectedPostProcessingProfile.motionBlurEnabled;
                    uRPMotionBlurQuality = m_selectedPostProcessingProfile.uRPMotionBlurQuality;
                    uRPMotionBlurIntensity = m_selectedPostProcessingProfile.uRPMotionBlurIntensity;
                    uRPMotionBlurCameraRotationVelocityClamp = m_selectedPostProcessingProfile.uRPMotionBlurCameraRotationVelocityClamp;

                    uRPPaniniProjectionEnabled = m_selectedPostProcessingProfile.uRPPaniniProjectionEnabled;
                    uRPPaniniProjectionDistance = m_selectedPostProcessingProfile.uRPPaniniProjectionDistance;
                    uRPPaniniProjectionCropToFit = m_selectedPostProcessingProfile.uRPPaniniProjectionCropToFit;

#endif

                    #endregion
                }
                else
                {
                    #region HDRP Post Processing Values

#if HDPipeline && UNITY_2019_1_OR_NEWER

                    hideGizmos = m_selectedPostProcessingProfile.hideGizmos;
                    antiAliasingMode = m_selectedPostProcessingProfile.antiAliasingMode;
                    dithering = m_selectedPostProcessingProfile.dithering;
                    hDRMode = m_selectedPostProcessingProfile.hDRMode;
                    customHDRPPostProcessingprofile = m_selectedPostProcessingProfile.customHDRPPostProcessingprofile;

                    aoEnabled = m_selectedPostProcessingProfile.aoEnabled;
                    hDRPAOIntensity = m_selectedPostProcessingProfile.hDRPAOIntensity;
                    hDRPAOThicknessModifier = m_selectedPostProcessingProfile.hDRPAOThicknessModifier;
                    hDRPAODirectLightingStrength = m_selectedPostProcessingProfile.hDRPAODirectLightingStrength;

                    autoExposureEnabled = m_selectedPostProcessingProfile.autoExposureEnabled;
                    hDRPExposureMode = m_selectedPostProcessingProfile.hDRPExposureMode;
                    hDRPExposureMeteringMode = m_selectedPostProcessingProfile.hDRPExposureMeteringMode;
                    hDRPExposureLuminationSource = m_selectedPostProcessingProfile.hDRPExposureLuminationSource;
                    hDRPExposureFixedExposure = m_selectedPostProcessingProfile.hDRPExposureFixedExposure;
                    hDRPExposureCurveMap = m_selectedPostProcessingProfile.hDRPExposureCurveMap;
                    hDRPExposureCompensation = m_selectedPostProcessingProfile.hDRPExposureCompensation;
                    hDRPExposureLimitMin = m_selectedPostProcessingProfile.hDRPExposureLimitMin;
                    hDRPExposureLimitMax = m_selectedPostProcessingProfile.hDRPExposureLimitMax;
                    hDRPExposureAdaptionMode = m_selectedPostProcessingProfile.hDRPExposureAdaptionMode;
                    hDRPExposureAdaptionSpeedDarkToLight = m_selectedPostProcessingProfile.hDRPExposureAdaptionSpeedDarkToLight;
                    hDRPExposureAdaptionSpeedLightToDark = m_selectedPostProcessingProfile.hDRPExposureAdaptionSpeedLightToDark;

                    bloomEnabled = m_selectedPostProcessingProfile.bloomEnabled;
                    hDRPBloomIntensity = m_selectedPostProcessingProfile.hDRPBloomIntensity;
                    hDRPBloomScatter = m_selectedPostProcessingProfile.hDRPBloomScatter;
                    hDRPBloomTint = m_selectedPostProcessingProfile.hDRPBloomTint;
                    hDRPBloomDirtLensTexture = m_selectedPostProcessingProfile.hDRPBloomDirtLensTexture;
                    hDRPBloomDirtLensIntensity = m_selectedPostProcessingProfile.hDRPBloomDirtLensIntensity;
                    hDRPBloomResolution = m_selectedPostProcessingProfile.hDRPBloomResolution;
                    hDRPBloomHighQualityFiltering = m_selectedPostProcessingProfile.hDRPBloomHighQualityFiltering;
                    hDRPBloomPrefiler = m_selectedPostProcessingProfile.hDRPBloomPrefiler;
                    hDRPBloomAnamorphic = m_selectedPostProcessingProfile.hDRPBloomAnamorphic;

                    chromaticAberrationEnabled = m_selectedPostProcessingProfile.chromaticAberrationEnabled;
                    hDRPChromaticAberrationSpectralLut = m_selectedPostProcessingProfile.hDRPChromaticAberrationSpectralLut;
                    hDRPChromaticAberrationIntensity = m_selectedPostProcessingProfile.hDRPChromaticAberrationIntensity;
                    hDRPChromaticAberrationMaxSamples = m_selectedPostProcessingProfile.hDRPChromaticAberrationMaxSamples;

                    hDRPColorLookupTexture = m_selectedPostProcessingProfile.hDRPColorLookupTexture;
                    hDRPColorAdjustmentColorFilter = m_selectedPostProcessingProfile.hDRPColorAdjustmentColorFilter;
                    hDRPColorAdjustmentPostExposure = m_selectedPostProcessingProfile.hDRPColorAdjustmentPostExposure;
                    colorGradingEnabled = m_selectedPostProcessingProfile.colorGradingEnabled;
                    hDRPWhiteBalanceTempature = m_selectedPostProcessingProfile.hDRPWhiteBalanceTempature;
                    hDRPColorLookupContribution = m_selectedPostProcessingProfile.hDRPColorLookupContribution;
                    hDRPWhiteBalanceTint = m_selectedPostProcessingProfile.hDRPWhiteBalanceTint;
                    hDRPColorAdjustmentSaturation = m_selectedPostProcessingProfile.hDRPColorAdjustmentSaturation;
                    hDRPColorAdjustmentContrast = m_selectedPostProcessingProfile.hDRPColorAdjustmentContrast;
                    hDRPChannelMixerRed = m_selectedPostProcessingProfile.hDRPChannelMixerRed;
                    hDRPChannelMixerGreen = m_selectedPostProcessingProfile.hDRPChannelMixerGreen;
                    hDRPChannelMixerBlue = m_selectedPostProcessingProfile.hDRPChannelMixerBlue;
                    hDRPTonemappingMode = m_selectedPostProcessingProfile.hDRPTonemappingMode;
                    hDRPTonemappingToeStrength = m_selectedPostProcessingProfile.hDRPTonemappingToeStrength;
                    hDRPTonemappingToeLength = m_selectedPostProcessingProfile.hDRPTonemappingToeLength;
                    hDRPTonemappingShoulderStrength = m_selectedPostProcessingProfile.hDRPTonemappingShoulderStrength;
                    hDRPTonemappingShoulderLength = m_selectedPostProcessingProfile.hDRPTonemappingShoulderLength;
                    hDRPTonemappingShoulderAngle = m_selectedPostProcessingProfile.hDRPTonemappingShoulderAngle;
                    hDRPTonemappingGamma = m_selectedPostProcessingProfile.hDRPTonemappingGamma;
                    hDRPSplitToningShadows = m_selectedPostProcessingProfile.hDRPSplitToningShadows;
                    hDRPSplitToningHighlights = m_selectedPostProcessingProfile.hDRPSplitToningHighlights;
                    hDRPSplitToningBalance = m_selectedPostProcessingProfile.hDRPSplitToningBalance;

                    depthOfFieldEnabled = m_selectedPostProcessingProfile.depthOfFieldEnabled;
                    hDRPDepthOfFieldFocusMode = m_selectedPostProcessingProfile.hDRPDepthOfFieldFocusMode;
                    hDRPDepthOfFieldNearBlurStart = m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurStart;
                    hDRPDepthOfFieldNearBlurEnd = m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurEnd;
                    hDRPDepthOfFieldNearBlurSampleCount = m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurSampleCount;
                    hDRPDepthOfFieldNearBlurMaxRadius = m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurMaxRadius;
                    hDRPDepthOfFieldFarBlurStart = m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurStart;
                    hDRPDepthOfFieldFarBlurEnd = m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurEnd;
                    hDRPDepthOfFieldFarBlurSampleCount = m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurSampleCount;
                    hDRPDepthOfFieldFarBlurMaxRadius = m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurMaxRadius;
                    hDRPDepthOfFieldResolution = m_selectedPostProcessingProfile.hDRPDepthOfFieldResolution;
                    hDRPDepthOfFieldHighQualityFiltering = m_selectedPostProcessingProfile.hDRPDepthOfFieldHighQualityFiltering;

                    grainEnabled = m_selectedPostProcessingProfile.grainEnabled;
                    hDRPFilmGrainType = m_selectedPostProcessingProfile.hDRPFilmGrainType;
                    hDRPFilmGrainIntensity = m_selectedPostProcessingProfile.hDRPFilmGrainIntensity;
                    hDRPFilmGrainResponse = m_selectedPostProcessingProfile.hDRPFilmGrainResponse;

                    distortionEnabled = m_selectedPostProcessingProfile.distortionEnabled;
                    hDRPLensDistortionIntensity = m_selectedPostProcessingProfile.hDRPLensDistortionIntensity;
                    hDRPLensDistortionXMultiplier = m_selectedPostProcessingProfile.hDRPLensDistortionXMultiplier;
                    hDRPLensDistortionYMultiplier = m_selectedPostProcessingProfile.hDRPLensDistortionYMultiplier;
                    hDRPLensDistortionCenter = m_selectedPostProcessingProfile.hDRPLensDistortionCenter;
                    hDRPLensDistortionScale = m_selectedPostProcessingProfile.hDRPLensDistortionScale;

                    vignetteEnabled = m_selectedPostProcessingProfile.vignetteEnabled;
                    hDRPVignetteMode = m_selectedPostProcessingProfile.hDRPVignetteMode;
                    hDRPVignetteColor = m_selectedPostProcessingProfile.hDRPVignetteColor;
                    hDRPVignetteCenter = m_selectedPostProcessingProfile.hDRPVignetteCenter;
                    hDRPVignetteIntensity = m_selectedPostProcessingProfile.hDRPVignetteIntensity;
                    hDRPVignetteSmoothness = m_selectedPostProcessingProfile.hDRPVignetteSmoothness;
                    hDRPVignetteRoundness = m_selectedPostProcessingProfile.hDRPVignetteRoundness;
                    hDRPVignetteRounded = m_selectedPostProcessingProfile.hDRPVignetteRounded;
                    hDRPVignetteMask = m_selectedPostProcessingProfile.hDRPVignetteMask;
                    hDRPVignetteMaskOpacity = m_selectedPostProcessingProfile.hDRPVignetteMaskOpacity;

                    motionBlurEnabled = m_selectedPostProcessingProfile.motionBlurEnabled;
                    hDRPMotionBlurIntensity = m_selectedPostProcessingProfile.hDRPMotionBlurIntensity;
                    hDRPMotionBlurSampleCount = m_selectedPostProcessingProfile.hDRPMotionBlurSampleCount;
                    hDRPMotionBlurMaxVelocity = m_selectedPostProcessingProfile.hDRPMotionBlurMaxVelocity;
                    hDRPMotionBlurMinVelocity = m_selectedPostProcessingProfile.hDRPMotionBlurMinVelocity;
                    hDRPMotionBlurCameraRotationVelocityClamp = m_selectedPostProcessingProfile.hDRPMotionBlurCameraRotationVelocityClamp;

                    m_selectedPostProcessingProfile.hDRPPaniniProjectionEnabled = hDRPPaniniProjectionEnabled;
                    m_selectedPostProcessingProfile.hDRPPaniniProjectionDistance = hDRPPaniniProjectionDistance;
                    m_selectedPostProcessingProfile.hDRPPaniniProjectionCropToFit = hDRPPaniniProjectionCropToFit;

#endif

                    #endregion
                }

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Ambient Occlusion settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void AmbientOcclusionSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //AO
            aoEnabled = m_editorUtils.Toggle("AOEnabledToggle", aoEnabled, helpEnabled);
            if (aoEnabled)
            {
                EditorGUI.indentLevel++;
#if UNITY_POST_PROCESSING_STACK_V2
                ambientOcclusionMode = (AmbientOcclusionMode)m_editorUtils.EnumPopup("AmbientOcclusionMode", ambientOcclusionMode, helpEnabled);
#endif
                aoAmount = m_editorUtils.Slider("AOAmount", aoAmount, 0f, 4f, helpEnabled);
                aoColor = m_editorUtils.ColorField("AOColor", aoColor, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateAO = true;

                m_selectedPostProcessingProfile.aoEnabled = aoEnabled;
                m_selectedPostProcessingProfile.aoAmount = aoAmount;
                m_selectedPostProcessingProfile.aoColor = aoColor;
#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.ambientOcclusionMode = ambientOcclusionMode;
#endif

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Auto Exposre settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void AutoExposureSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Auto Exposure
            autoExposureEnabled = m_editorUtils.Toggle("AutoExposureEnabledToggle", autoExposureEnabled, helpEnabled);
            if (autoExposureEnabled)
            {
                EditorGUI.indentLevel++;
                exposureAmount = m_editorUtils.Slider("ExposureAmount", exposureAmount, 0f, 5f, helpEnabled);
                exposureMin = m_editorUtils.Slider("ExposureMin", exposureMin, -9f, 9f, helpEnabled);
                exposureMax = m_editorUtils.Slider("ExposureMax", exposureMax, -9f, 9f, helpEnabled);

                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateAutoExposure = true;

                m_selectedPostProcessingProfile.autoExposureEnabled = autoExposureEnabled;
                m_selectedPostProcessingProfile.exposureAmount = exposureAmount;
                m_selectedPostProcessingProfile.exposureMin = exposureMin;
                m_selectedPostProcessingProfile.exposureMax = exposureMax;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Bloom settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void BloomSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Bloom
            bloomEnabled = m_editorUtils.Toggle("BloomEnabledToggle", bloomEnabled, helpEnabled);
            if (bloomEnabled)
            {
                EditorGUI.indentLevel++;
                bloomIntensity = m_editorUtils.Slider("BloomIntensity", bloomIntensity, 0f, 5f, helpEnabled);
                bloomThreshold = m_editorUtils.Slider("BloomThreshold", bloomThreshold, 0f, 20f, helpEnabled);
                lensIntensity = m_editorUtils.Slider("BloomLensIntensity", lensIntensity, 0f, 20f, helpEnabled);
                lensTexture = (Texture2D)m_editorUtils.ObjectField("LensTexture", lensTexture, typeof(Texture2D), false, helpEnabled, GUILayout.Height(16f));
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateBloom = true;

                m_selectedPostProcessingProfile.bloomEnabled = bloomEnabled;
                m_selectedPostProcessingProfile.bloomAmount = bloomIntensity;
                m_selectedPostProcessingProfile.bloomThreshold = bloomThreshold;
                m_selectedPostProcessingProfile.lensTexture = lensTexture;
                m_selectedPostProcessingProfile.lensIntensity = lensIntensity;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Chromatic Aberration settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ChromaticAberrationSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            chromaticAberrationEnabled = m_editorUtils.Toggle("ChromaticAberrationEnabled", chromaticAberrationEnabled, helpEnabled);
            if (chromaticAberrationEnabled)
            {
                EditorGUI.indentLevel++;
                chromaticAberrationIntensity = m_editorUtils.Slider("ChromaticAberrationIntensity", chromaticAberrationIntensity, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateChromatic = true;

                m_selectedPostProcessingProfile.chromaticAberrationEnabled = chromaticAberrationEnabled;
                m_selectedPostProcessingProfile.chromaticAberrationIntensity = chromaticAberrationIntensity;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Color Grading settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ColorGradingSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Color grading
            colorGradingEnabled = m_editorUtils.Toggle("ColorGradingEnabledToggle", colorGradingEnabled, helpEnabled);
            if (colorGradingEnabled)
            {
                EditorGUI.indentLevel++;
#if UNITY_POST_PROCESSING_STACK_V2
                colorGradingMode = (GradingMode)m_editorUtils.EnumPopup("ColorGradingMode", colorGradingMode, helpEnabled);
                if (colorGradingMode  == GradingMode.HighDefinitionRange)
                {
                    colorGradingPostExposure = m_editorUtils.Slider("ColorGradingPostExposure", colorGradingPostExposure, -5f, 5f, helpEnabled);
                }
                else
                {
                    colorGradingLut = (Texture2D)m_editorUtils.ObjectField("ColorGradingLut", colorGradingLut, typeof(Texture2D), false, helpEnabled, GUILayout.Height(16f));
                }
#endif

                colorGradingColorFilter = m_editorUtils.ColorField("ColorGradingColorFilter", colorGradingColorFilter, helpEnabled);
                if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
                {
                    colorGradingTempature = m_editorUtils.IntSlider("ColorGradingTempatureIntSlider", colorGradingTempature, -100, 100, helpEnabled);
                }
                else if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable && !syncPostProcessing)
                {
                    colorGradingTempature = m_editorUtils.IntSlider("ColorGradingTempatureIntSlider", colorGradingTempature, -100, 100, helpEnabled);
                }
                colorGradingTint = m_editorUtils.IntSlider("ColorGradingTintIntSlider", colorGradingTint, -100, 100, helpEnabled);
                colorGradingSaturation = m_editorUtils.Slider("ColorGradingSaturationSlider", colorGradingSaturation, -100f, 100f, helpEnabled);
                colorGradingContrast = m_editorUtils.Slider("ColorGradingContrastSlider", colorGradingContrast, -100f, 100f, helpEnabled);

                m_editorUtils.Text("ChannelMixer");
                EditorGUI.indentLevel++;
                channelMixerRed = m_editorUtils.IntSlider("HDRPChannelMixerRed", channelMixerRed, -200, 200, helpEnabled);
                channelMixerGreen = m_editorUtils.IntSlider("HDRPChannelMixerGreen", channelMixerGreen, -200, 200, helpEnabled);
                channelMixerBlue = m_editorUtils.IntSlider("HDRPChannelMixerBlue", channelMixerBlue, -200, 200, helpEnabled);
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateColorGrading = true;

#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.colorGradingMode = colorGradingMode;
#endif

                m_selectedPostProcessingProfile.colorGradingLut = colorGradingLut;
                m_selectedPostProcessingProfile.colorGradingColorFilter = colorGradingColorFilter;
                m_selectedPostProcessingProfile.colorGradingPostExposure = colorGradingPostExposure;
                m_selectedPostProcessingProfile.colorGradingEnabled = colorGradingEnabled;
                m_selectedPostProcessingProfile.colorGradingTempature = colorGradingTempature;
                m_selectedPostProcessingProfile.colorGradingTint = colorGradingTint;
                m_selectedPostProcessingProfile.colorGradingSaturation = colorGradingSaturation;
                m_selectedPostProcessingProfile.colorGradingContrast = colorGradingContrast;
                m_selectedPostProcessingProfile.channelMixerRed = channelMixerRed;
                m_selectedPostProcessingProfile.channelMixerBlue = channelMixerBlue;
                m_selectedPostProcessingProfile.channelMixerGreen = channelMixerGreen;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Depth Of Field settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void DepthOfFieldSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Depth Of Field
            depthOfFieldEnabled = m_editorUtils.Toggle("DOFEnabledToggle", depthOfFieldEnabled, helpEnabled);
            if (depthOfFieldEnabled)
            {
                EditorGUI.indentLevel++;
                depthOfFieldMode = (AmbientSkiesConsts.DepthOfFieldMode)m_editorUtils.EnumPopup("DepthOfFieldMode", depthOfFieldMode, helpEnabled);
                if (depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                {
                    depthOfFieldTrackingType = (AmbientSkiesConsts.DOFTrackingType)m_editorUtils.EnumPopup("DepthOfFieldTrackingType", depthOfFieldTrackingType, helpEnabled);
#if UNITY_POST_PROCESSING_STACK_V2
                    maxBlurSize = (KernelSize)m_editorUtils.EnumPopup("DepthOfFieldMaxBlurSize", maxBlurSize, helpEnabled);
#endif
                    EditorGUILayout.BeginHorizontal();
                    m_editorUtils.Text("DOFFocusDistanceIs", GUILayout.Width(144f));
                    m_editorUtils.TextNonLocalized("" + depthOfFieldDistanceString, GUILayout.Width(80f), GUILayout.Height(16f));
                    EditorGUILayout.EndHorizontal();
                    depthOfFieldAperture = m_editorUtils.Slider("DOFApertureSlider", depthOfFieldAperture, 0.1f, 32f, helpEnabled);
                    depthOfFieldFocalLength = m_editorUtils.Slider("DOFFocalLengthSlider", depthOfFieldFocalLength, 1f, 300f, helpEnabled);
                    focusOffset = m_editorUtils.Slider("DOFFocusOffset", focusOffset, -100f, 100f, helpEnabled);
                    targetLayer = AmbientSkiesEditorFunctions.LayerMaskField(m_editorUtils, "DOFTargetLayer", targetLayer.value, helpEnabled);
                    maxFocusDistance = m_editorUtils.Slider("DOFMaxFocusDistance", maxFocusDistance, 0f, 5000f, helpEnabled);
                }
                else
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    maxBlurSize = (KernelSize)m_editorUtils.EnumPopup("DepthOfFieldMaxBlurSize", maxBlurSize, helpEnabled);
#endif
                    depthOfFieldFocusDistance = m_editorUtils.Slider("DOFFocusDistanceSlider", depthOfFieldFocusDistance, 1f, 10000f, helpEnabled);
                    depthOfFieldAperture = m_editorUtils.Slider("DOFApertureSlider", depthOfFieldAperture, 0.1f, 32f, helpEnabled);
                    depthOfFieldFocalLength = m_editorUtils.Slider("DOFFocalLengthSlider", depthOfFieldFocalLength, 1f, 300f, helpEnabled);
                }
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateDOF = true;

                m_selectedPostProcessingProfile.depthOfFieldEnabled = depthOfFieldEnabled;
                m_selectedPostProcessingProfile.depthOfFieldMode = depthOfFieldMode;
                m_selectedPostProcessingProfile.depthOfFieldFocusDistance = depthOfFieldFocusDistance;
                if (depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                {
                    m_selectedPostProcessingProfile.depthOfFieldTrackingType = depthOfFieldTrackingType;
                    m_selectedPostProcessingProfile.focusOffset = focusOffset;
                    m_selectedPostProcessingProfile.targetLayer = targetLayer;
                    m_selectedPostProcessingProfile.maxFocusDistance = maxFocusDistance;
                }

#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.maxBlurSize = maxBlurSize;
#endif
                m_selectedPostProcessingProfile.depthOfFieldAperture = depthOfFieldAperture;
                m_selectedPostProcessingProfile.depthOfFieldFocalLength = depthOfFieldFocalLength;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Grain settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void GrainSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Grain
            grainEnabled = m_editorUtils.Toggle("GrainEnabledToggle", grainEnabled, helpEnabled);
            if (grainEnabled)
            {
                EditorGUI.indentLevel++;
                grainIntensity = m_editorUtils.Slider("GrainIntensity", grainIntensity, 0f, 1f, helpEnabled);
                grainSize = m_editorUtils.Slider("GrainSize", grainSize, 0f, 3f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateGrain = true;

                m_selectedPostProcessingProfile.grainEnabled = grainEnabled;
                m_selectedPostProcessingProfile.grainIntensity = grainIntensity;
                m_selectedPostProcessingProfile.grainSize = grainSize;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Lens Distortion settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void LensDistortionSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Lens Distortion
            distortionEnabled = m_editorUtils.Toggle("DistortionEnabledToggle", distortionEnabled, helpEnabled);
            if (distortionEnabled)
            {
                EditorGUI.indentLevel++;
                distortionIntensity = m_editorUtils.Slider("DistortionIntensitySlider", distortionIntensity, -100f, 100f, helpEnabled);
                distortionScale = m_editorUtils.Slider("DistortionScaleSlider", distortionScale, 0.01f, 5f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.distortionEnabled = distortionEnabled;
                m_selectedPostProcessingProfile.distortionIntensity = distortionIntensity;
                m_selectedPostProcessingProfile.distortionScale = distortionScale;

                m_profiles.m_updateLensDistortion = true;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Screen Space Reflections settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ScreenSpaceReflectionsSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Get graphic Tiers
            var tier1 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1);
            var tier2 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2);
            var tier3 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3);
            if (mainCam != null)
            {
                if (mainCam.actualRenderingPath != RenderingPath.DeferredShading)
                {
                    EditorGUILayout.HelpBox("Screen Space Reflections is only avaliable in Deferred Rendering", MessageType.Warning);
                }
            }
            else
            {
                if (tier1.renderingPath != RenderingPath.DeferredShading)
                {
                    EditorGUILayout.HelpBox("Screen Space Reflections is only avaliable in Deferred Rendering", MessageType.Warning);
                }
                else if (tier2.renderingPath != RenderingPath.DeferredShading)
                {
                    EditorGUILayout.HelpBox("Screen Space Reflections is only avaliable in Deferred Rendering", MessageType.Warning);
                }
                else if (tier3.renderingPath != RenderingPath.DeferredShading)
                {
                    EditorGUILayout.HelpBox("Screen Space Reflections is only avaliable in Deferred Rendering", MessageType.Warning);
                }
            }

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Screen Space Reflection
            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                m_editorUtils.Text("Screen Space Reflections is handled in the Skies Tab in High Definition.");
            }
            else
            {
                screenSpaceReflectionsEnabled = m_editorUtils.Toggle("ScreenSpaceReflectionsEnabled", screenSpaceReflectionsEnabled, helpEnabled);
                if (screenSpaceReflectionsEnabled)
                {
                    EditorGUI.indentLevel++;
#if UNITY_POST_PROCESSING_STACK_V2
                    screenSpaceReflectionPreset = (ScreenSpaceReflectionPreset)m_editorUtils.EnumPopup("ScreenSpaceReflectionPreset", screenSpaceReflectionPreset, helpEnabled);
                    if (screenSpaceReflectionPreset == ScreenSpaceReflectionPreset.Custom)
                    {
                        maximumIterationCount = m_editorUtils.IntSlider("MaximumIterationCount", maximumIterationCount, 0, 256, helpEnabled);
                        thickness = m_editorUtils.Slider("Thickness", thickness, 0f, 64f, helpEnabled);
                        screenSpaceReflectionResolution = (ScreenSpaceReflectionResolution)m_editorUtils.EnumPopup("ScreenSpaceReflectionResolution", screenSpaceReflectionResolution, helpEnabled);
                    }
#endif
                    maximumMarchDistance = m_editorUtils.Slider("MaximumMarchDistance", maximumMarchDistance, 0f, 4000f, helpEnabled);
                    distanceFade = m_editorUtils.Slider("DistanceFade", distanceFade, 0f, 1f, helpEnabled);
                    screenSpaceVignette = m_editorUtils.Slider("ScreenSpaceVignette", screenSpaceVignette, 0f, 1f, helpEnabled);
                    EditorGUI.indentLevel--;
                }
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateSSR = true;

                m_selectedPostProcessingProfile.screenSpaceReflectionsEnabled = screenSpaceReflectionsEnabled;
                m_selectedPostProcessingProfile.maximumIterationCount = maximumIterationCount;
                m_selectedPostProcessingProfile.thickness = thickness;
#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.spaceReflectionResolution = screenSpaceReflectionResolution;
                m_selectedPostProcessingProfile.screenSpaceReflectionPreset = screenSpaceReflectionPreset;
#endif
                m_selectedPostProcessingProfile.maximumMarchDistance = maximumMarchDistance;
                m_selectedPostProcessingProfile.distanceFade = distanceFade;
                m_selectedPostProcessingProfile.screenSpaceVignette = screenSpaceVignette;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Vignette settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void VignetteSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Vignette
            vignetteEnabled = m_editorUtils.Toggle("VignetteEnabledToggle", vignetteEnabled, helpEnabled);
            if (vignetteEnabled)
            {
                EditorGUI.indentLevel++;
                vignetteIntensity = m_editorUtils.Slider("VignetteIntensitySlider", vignetteIntensity, 0f, 1f, helpEnabled);
                vignetteSmoothness = m_editorUtils.Slider("VignetteSmoothnessSlider", vignetteSmoothness, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateVignette = true;

                m_selectedPostProcessingProfile.vignetteEnabled = vignetteEnabled;
                m_selectedPostProcessingProfile.vignetteIntensity = vignetteIntensity;
                m_selectedPostProcessingProfile.vignetteSmoothness = vignetteSmoothness;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Motion Blur settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void MotionBlurSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Motion Blur
            motionBlurEnabled = m_editorUtils.Toggle("MotionBlurEnabled", motionBlurEnabled, helpEnabled);
            if (motionBlurEnabled)
            {
                EditorGUI.indentLevel++;
                motionShutterAngle = m_editorUtils.IntSlider("MotionShutterAngle", motionShutterAngle, 0, 360, helpEnabled);
                motionSampleCount = m_editorUtils.IntSlider("MotionSampleCount", motionSampleCount, 0, 32, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateMotionBlur = true;

                m_selectedPostProcessingProfile.motionBlurEnabled = motionBlurEnabled;
                m_selectedPostProcessingProfile.shutterAngle = motionShutterAngle;
                m_selectedPostProcessingProfile.sampleCount = motionSampleCount;

                m_hasChanged = true;
            }
        }

        #endregion

        #region HDRP Post FX Tab Functions

#if HDPipeline && UNITY_2019_1_OR_NEWER
        /// <summary>
        /// Main settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPMainPostProcessSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            antiAliasingMode = (AmbientSkiesConsts.AntiAliasingMode)m_editorUtils.EnumPopup("AntiAliasingMode", antiAliasingMode, helpEnabled);
            dithering = m_editorUtils.Toggle("Dithering", dithering, helpEnabled);

            if (!autoMatchProfile)
            {
                //Match profile to sky profile
                if (m_editorUtils.Button("MatchProfileToSkyboxProfile"))
                {
                    if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
                        m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingHDRIName(m_profiles, m_selectedSkyboxProfile);
                        newPPSelection = m_selectedPostProcessingProfileIndex;
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                        EditorUtility.SetDirty(m_creationToolSettings);
                        m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                    }
                    else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingProceduralName(m_profiles, m_selectedProceduralSkyboxProfile);
                        newPPSelection = m_selectedPostProcessingProfileIndex;
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                        EditorUtility.SetDirty(m_creationToolSettings);
                        m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                    }
                    else
                    {
                        m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingGradientName(m_profiles, m_selectedGradientSkyboxProfile);
                        newPPSelection = m_selectedPostProcessingProfileIndex;
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                        EditorUtility.SetDirty(m_creationToolSettings);
                        m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                    }
                }
            }

            if (m_editorUtils.Button("FocusProfile"))
            {
                PostProcessingUtils.FocusPostProcessProfile(m_profiles, m_selectedPostProcessingProfile);
            }

            m_editorUtils.InlineHelp("FocusProfileHelp", helpEnabled);

            EditorGUILayout.Space();

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.hideGizmos = hideGizmos;
                m_selectedPostProcessingProfile.autoMatchProfile = autoMatchProfile;
                m_selectedPostProcessingProfile.antiAliasingMode = antiAliasingMode;
                m_selectedPostProcessingProfile.dithering = dithering;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Ambient Occlusion settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPAmbientOcclusionSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //AO
            aoEnabled = m_editorUtils.Toggle("AOEnabledToggle", aoEnabled, helpEnabled);
            if (aoEnabled)
            {
                EditorGUI.indentLevel++;
                hDRPAOIntensity = m_editorUtils.Slider("AOAmount", hDRPAOIntensity, 0f, 4f, helpEnabled);
                hDRPAOThicknessModifier = m_editorUtils.Slider("AOThicknessModifier", hDRPAOThicknessModifier, 0f, 4f, helpEnabled);
                hDRPAODirectLightingStrength = m_editorUtils.Slider("AODirectLightingStrength", hDRPAODirectLightingStrength, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateAO = true;

                m_selectedPostProcessingProfile.aoEnabled = aoEnabled;
                m_selectedPostProcessingProfile.hDRPAOIntensity = hDRPAOIntensity;
                m_selectedPostProcessingProfile.hDRPAOThicknessModifier = hDRPAOThicknessModifier;
                m_selectedPostProcessingProfile.hDRPAODirectLightingStrength = hDRPAODirectLightingStrength;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Auto Exposre settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPAutoExposureSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Auto Exposure
            autoExposureEnabled = m_editorUtils.Toggle("AutoExposureEnabledToggle", autoExposureEnabled, helpEnabled);
            if (autoExposureEnabled)
            {
#if !UNITY_2019_3_OR_NEWER
                EditorGUI.indentLevel++;
                hDRPExposureMode = (UnityEngine.Experimental.Rendering.HDPipeline.ExposureMode)m_editorUtils.EnumPopup("HDRPExposureMode", hDRPExposureMode, helpEnabled);
                if (hDRPExposureMode == ExposureMode.Fixed)
                {
                    hDRPExposureFixedExposure = m_editorUtils.Slider("HDRPExposureFixedExposure", hDRPExposureFixedExposure, -50f, 50f, helpEnabled);
                }
                else if (hDRPExposureMode == ExposureMode.CurveMapping)
                {
                    hDRPExposureMeteringMode = (UnityEngine.Experimental.Rendering.HDPipeline.MeteringMode)m_editorUtils.EnumPopup("HDRPExposureMeteringMode", hDRPExposureMeteringMode, helpEnabled);
                    hDRPExposureLuminationSource = (UnityEngine.Experimental.Rendering.HDPipeline.LuminanceSource)m_editorUtils.EnumPopup("HDRPExposureLuminationSource", hDRPExposureLuminationSource, helpEnabled);
                    if (hDRPExposureLuminationSource == LuminanceSource.LightingBuffer)
                    {
                        EditorGUILayout.HelpBox("Warning Lighting Buffer lumination source is not yet supported in High Definition. Please use Color Buffer instead.", MessageType.Warning);
                    }
                    hDRPExposureCurveMap = m_editorUtils.CurveField("HDRPExposureCurveMap", hDRPExposureCurveMap, helpEnabled);
                    hDRPExposureCompensation = m_editorUtils.Slider("hDRPExposureCompensation", hDRPExposureCompensation, -100f, 250f, helpEnabled);
                    hDRPExposureLimitMin = m_editorUtils.Slider("HDRPExposureLimitMin", hDRPExposureLimitMin, -100f, 250f, helpEnabled);
                    hDRPExposureLimitMax = m_editorUtils.Slider("HDRPExposureLimitMax", hDRPExposureLimitMax, -100f, 500f, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Text("Adaption");
                    hDRPExposureAdaptionMode = (UnityEngine.Experimental.Rendering.HDPipeline.AdaptationMode)m_editorUtils.EnumPopup("HDRPExposureAdaptionMode", hDRPExposureAdaptionMode, helpEnabled);
                    if (hDRPExposureAdaptionMode == AdaptationMode.Progressive)
                    {
                        hDRPExposureAdaptionSpeedDarkToLight = m_editorUtils.Slider("HDRPExposureAdaptionSpeedDarkToLight", hDRPExposureAdaptionSpeedDarkToLight, 0f, 50f, helpEnabled);
                        hDRPExposureAdaptionSpeedLightToDark = m_editorUtils.Slider("HDRPExposureAdaptionSpeedLightToDark", hDRPExposureAdaptionSpeedLightToDark, 0f, 50f, helpEnabled);
                    }
                }
                else if (hDRPExposureMode == ExposureMode.UsePhysicalCamera)
                {
                    hDRPExposureCompensation = m_editorUtils.Slider("hDRPExposureCompensation", hDRPExposureCompensation, -100f, 250f, helpEnabled);
                }
                else
                {
                    hDRPExposureMeteringMode = (UnityEngine.Experimental.Rendering.HDPipeline.MeteringMode)m_editorUtils.EnumPopup("HDRPExposureMeteringMode", hDRPExposureMeteringMode, helpEnabled);
                    hDRPExposureLuminationSource = (UnityEngine.Experimental.Rendering.HDPipeline.LuminanceSource)m_editorUtils.EnumPopup("HDRPExposureLuminationSource", hDRPExposureLuminationSource, helpEnabled);
                    if (hDRPExposureLuminationSource == LuminanceSource.LightingBuffer)
                    {
                        EditorGUILayout.HelpBox("Warning Lighting Buffer lumination source is not yet supported in High Definition. Please use Color Buffer instead.", MessageType.Warning);
                    }
                    hDRPExposureCompensation = m_editorUtils.Slider("hDRPExposureCompensation", hDRPExposureCompensation, -100f, 250f, helpEnabled);
                    hDRPExposureLimitMin = m_editorUtils.Slider("HDRPExposureLimitMin", hDRPExposureLimitMin, -100f, 250f, helpEnabled);
                    hDRPExposureLimitMax = m_editorUtils.Slider("HDRPExposureLimitMax", hDRPExposureLimitMax, -100f, 500f, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Text("Adaption");
                    hDRPExposureAdaptionMode = (UnityEngine.Experimental.Rendering.HDPipeline.AdaptationMode)m_editorUtils.EnumPopup("HDRPExposureAdaptionMode", hDRPExposureAdaptionMode, helpEnabled);
                    if (hDRPExposureAdaptionMode == AdaptationMode.Progressive)
                    {
                        hDRPExposureAdaptionSpeedDarkToLight = m_editorUtils.Slider("HDRPExposureAdaptionSpeedDarkToLight", hDRPExposureAdaptionSpeedDarkToLight, 0f, 50f, helpEnabled);
                        hDRPExposureAdaptionSpeedLightToDark = m_editorUtils.Slider("HDRPExposureAdaptionSpeedLightToDark", hDRPExposureAdaptionSpeedLightToDark, 0f, 50f, helpEnabled);
                    }
                }
#else
                hDRPExposureMode = (UnityEngine.Rendering.HighDefinition.ExposureMode)m_editorUtils.EnumPopup("HDRPExposureMode", hDRPExposureMode, helpEnabled);
                if (hDRPExposureMode == ExposureMode.Fixed)
                {
                    hDRPExposureFixedExposure = m_editorUtils.Slider("HDRPExposureFixedExposure", hDRPExposureFixedExposure, -50f, 50f, helpEnabled);
                }
                else if (hDRPExposureMode == ExposureMode.CurveMapping)
                {
                    hDRPExposureMeteringMode = (UnityEngine.Rendering.HighDefinition.MeteringMode)m_editorUtils.EnumPopup("HDRPExposureMeteringMode", hDRPExposureMeteringMode, helpEnabled);
                    hDRPExposureLuminationSource = (UnityEngine.Rendering.HighDefinition.LuminanceSource)m_editorUtils.EnumPopup("HDRPExposureLuminationSource", hDRPExposureLuminationSource, helpEnabled);
                    if (hDRPExposureLuminationSource == LuminanceSource.LightingBuffer)
                    {
                        EditorGUILayout.HelpBox("Warning Lighting Buffer lumination source is not yet supported in High Definition. Please use Color Buffer instead.", MessageType.Warning);
                    }
                    hDRPExposureCurveMap = m_editorUtils.CurveField("HDRPExposureCurveMap", hDRPExposureCurveMap, helpEnabled);
                    hDRPExposureCompensation = m_editorUtils.Slider("hDRPExposureCompensation", hDRPExposureCompensation, -100f, 250f, helpEnabled);
                    hDRPExposureLimitMin = m_editorUtils.Slider("HDRPExposureLimitMin", hDRPExposureLimitMin, -100f, 250f, helpEnabled);
                    hDRPExposureLimitMax = m_editorUtils.Slider("HDRPExposureLimitMax", hDRPExposureLimitMax, -100f, 500f, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Text("Adaption");
                    hDRPExposureAdaptionMode = (UnityEngine.Rendering.HighDefinition.AdaptationMode)m_editorUtils.EnumPopup("HDRPExposureAdaptionMode", hDRPExposureAdaptionMode, helpEnabled);
                    if (hDRPExposureAdaptionMode == AdaptationMode.Progressive)
                    {
                        hDRPExposureAdaptionSpeedDarkToLight = m_editorUtils.Slider("HDRPExposureAdaptionSpeedDarkToLight", hDRPExposureAdaptionSpeedDarkToLight, 0f, 50f, helpEnabled);
                        hDRPExposureAdaptionSpeedLightToDark = m_editorUtils.Slider("HDRPExposureAdaptionSpeedLightToDark", hDRPExposureAdaptionSpeedLightToDark, 0f, 50f, helpEnabled);
                    }
                }
                else if (hDRPExposureMode == ExposureMode.UsePhysicalCamera)
                {
                    hDRPExposureCompensation = m_editorUtils.Slider("hDRPExposureCompensation", hDRPExposureCompensation, -100f, 250f, helpEnabled);
                }
                else
                {
                    hDRPExposureMeteringMode = (UnityEngine.Rendering.HighDefinition.MeteringMode)m_editorUtils.EnumPopup("HDRPExposureMeteringMode", hDRPExposureMeteringMode, helpEnabled);
                    hDRPExposureLuminationSource = (UnityEngine.Rendering.HighDefinition.LuminanceSource)m_editorUtils.EnumPopup("HDRPExposureLuminationSource", hDRPExposureLuminationSource, helpEnabled);
                    if (hDRPExposureLuminationSource == LuminanceSource.LightingBuffer)
                    {
                        EditorGUILayout.HelpBox("Warning Lighting Buffer lumination source is not yet supported in High Definition. Please use Color Buffer instead.", MessageType.Warning);
                    }
                    hDRPExposureCompensation = m_editorUtils.Slider("hDRPExposureCompensation", hDRPExposureCompensation, -100f, 250f, helpEnabled);
                    hDRPExposureLimitMin = m_editorUtils.Slider("HDRPExposureLimitMin", hDRPExposureLimitMin, -100f, 250f, helpEnabled);
                    hDRPExposureLimitMax = m_editorUtils.Slider("HDRPExposureLimitMax", hDRPExposureLimitMax, -100f, 500f, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Text("Adaption");
                    hDRPExposureAdaptionMode = (UnityEngine.Rendering.HighDefinition.AdaptationMode)m_editorUtils.EnumPopup("HDRPExposureAdaptionMode", hDRPExposureAdaptionMode, helpEnabled);
                    if (hDRPExposureAdaptionMode == AdaptationMode.Progressive)
                    {
                        hDRPExposureAdaptionSpeedDarkToLight = m_editorUtils.Slider("HDRPExposureAdaptionSpeedDarkToLight", hDRPExposureAdaptionSpeedDarkToLight, 0f, 50f, helpEnabled);
                        hDRPExposureAdaptionSpeedLightToDark = m_editorUtils.Slider("HDRPExposureAdaptionSpeedLightToDark", hDRPExposureAdaptionSpeedLightToDark, 0f, 50f, helpEnabled);
                    }
                }
#endif
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateAutoExposure = true;

                m_selectedPostProcessingProfile.autoExposureEnabled = autoExposureEnabled;
                m_selectedPostProcessingProfile.hDRPExposureMode = hDRPExposureMode;
                m_selectedPostProcessingProfile.hDRPExposureMeteringMode = hDRPExposureMeteringMode;
                m_selectedPostProcessingProfile.hDRPExposureLuminationSource = hDRPExposureLuminationSource;
                m_selectedPostProcessingProfile.hDRPExposureFixedExposure = hDRPExposureFixedExposure;
                m_selectedPostProcessingProfile.hDRPExposureCurveMap = hDRPExposureCurveMap;
                m_selectedPostProcessingProfile.hDRPExposureCompensation = hDRPExposureCompensation;
                m_selectedPostProcessingProfile.hDRPExposureLimitMin = hDRPExposureLimitMin;
                m_selectedPostProcessingProfile.hDRPExposureLimitMax = hDRPExposureLimitMax;
                m_selectedPostProcessingProfile.hDRPExposureAdaptionMode = hDRPExposureAdaptionMode;
                m_selectedPostProcessingProfile.hDRPExposureAdaptionSpeedDarkToLight = hDRPExposureAdaptionSpeedDarkToLight;
                m_selectedPostProcessingProfile.hDRPExposureAdaptionSpeedLightToDark = hDRPExposureAdaptionSpeedLightToDark;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Bloom settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPBloomSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Bloom
            bloomEnabled = m_editorUtils.Toggle("BloomEnabledToggle", bloomEnabled, helpEnabled);
            if (bloomEnabled)
            {
                EditorGUI.indentLevel++;
                hDRPBloomIntensity = m_editorUtils.Slider("BloomIntensity", hDRPBloomIntensity, 0f, 1f, helpEnabled);
                hDRPBloomScatter = m_editorUtils.Slider("HDRPBloomScatter", hDRPBloomScatter, 0f, 1f, helpEnabled);
                hDRPBloomTint = m_editorUtils.ColorField("HDRPBloomTint", hDRPBloomTint, helpEnabled);
                hDRPBloomDirtLensTexture = (Texture2D)m_editorUtils.ObjectField("LensTexture", hDRPBloomDirtLensTexture, typeof(Texture2D), false, helpEnabled, GUILayout.Height(16f));
                hDRPBloomDirtLensIntensity = m_editorUtils.Slider("BloomLensIntensity", hDRPBloomDirtLensIntensity, 0f, 20f, helpEnabled);
                EditorGUILayout.Space();

                m_editorUtils.Text("Quality");
#if !UNITY_2019_3_OR_NEWER
                hDRPBloomResolution = (UnityEngine.Experimental.Rendering.HDPipeline.BloomResolution)m_editorUtils.EnumPopup("hDRPBloomResolution", hDRPBloomResolution, helpEnabled);
#else
                hDRPBloomResolution = (UnityEngine.Rendering.HighDefinition.BloomResolution)m_editorUtils.EnumPopup("hDRPBloomResolution", hDRPBloomResolution, helpEnabled);
#endif
                hDRPBloomHighQualityFiltering = m_editorUtils.Toggle("hDRPBloomHighQualityFiltering", hDRPBloomHighQualityFiltering, helpEnabled);
                hDRPBloomPrefiler = m_editorUtils.Toggle("hDRPBloomPrefiler", hDRPBloomPrefiler, helpEnabled);
                hDRPBloomAnamorphic = m_editorUtils.Toggle("hDRPBloomAnamorphic", hDRPBloomAnamorphic, helpEnabled);

                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateBloom = true;

                m_selectedPostProcessingProfile.bloomEnabled = bloomEnabled;
                m_selectedPostProcessingProfile.hDRPBloomIntensity = hDRPBloomIntensity;
                m_selectedPostProcessingProfile.hDRPBloomScatter = hDRPBloomScatter;
                m_selectedPostProcessingProfile.hDRPBloomTint = hDRPBloomTint;
                m_selectedPostProcessingProfile.hDRPBloomDirtLensTexture = hDRPBloomDirtLensTexture;
                m_selectedPostProcessingProfile.hDRPBloomDirtLensIntensity = hDRPBloomDirtLensIntensity;
                m_selectedPostProcessingProfile.hDRPBloomResolution = hDRPBloomResolution;
                m_selectedPostProcessingProfile.hDRPBloomHighQualityFiltering = hDRPBloomHighQualityFiltering;
                m_selectedPostProcessingProfile.hDRPBloomPrefiler = hDRPBloomPrefiler;
                m_selectedPostProcessingProfile.hDRPBloomAnamorphic = hDRPBloomAnamorphic;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Chromatic Aberration settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPChromaticAberrationSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            chromaticAberrationEnabled = m_editorUtils.Toggle("ChromaticAberrationEnabled", chromaticAberrationEnabled, helpEnabled);
            if (chromaticAberrationEnabled)
            {
                EditorGUI.indentLevel++;
                hDRPChromaticAberrationSpectralLut = (Texture2D)m_editorUtils.ObjectField("ChromaticAberrationSpectralLut", hDRPChromaticAberrationSpectralLut, typeof(Texture2D), false, helpEnabled, GUILayout.Height(16f));
                hDRPChromaticAberrationIntensity = m_editorUtils.Slider("ChromaticAberrationIntensity", hDRPChromaticAberrationIntensity, 0f, 1f, helpEnabled);
                hDRPChromaticAberrationMaxSamples = m_editorUtils.IntSlider("HDRPChromaticAberrationMaxSamples", hDRPChromaticAberrationMaxSamples, 0, 24, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateChromatic = true;

                m_selectedPostProcessingProfile.chromaticAberrationEnabled = chromaticAberrationEnabled;
                m_selectedPostProcessingProfile.hDRPChromaticAberrationSpectralLut = hDRPChromaticAberrationSpectralLut;
                m_selectedPostProcessingProfile.hDRPChromaticAberrationIntensity = hDRPChromaticAberrationIntensity;
                m_selectedPostProcessingProfile.hDRPChromaticAberrationMaxSamples = hDRPChromaticAberrationMaxSamples;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Color Grading settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPColorGradingSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Color grading
            colorGradingEnabled = m_editorUtils.Toggle("ColorGradingEnabledToggle", colorGradingEnabled, helpEnabled);
            if (colorGradingEnabled)
            {
                EditorGUI.indentLevel++;
                m_editorUtils.Text("ColorAdjustments");
                hDRPColorAdjustmentPostExposure = m_editorUtils.Slider("ColorGradingPostExposure", hDRPColorAdjustmentPostExposure, -5f, 5f, helpEnabled);
                hDRPColorAdjustmentColorFilter = m_editorUtils.ColorField("ColorGradingColorFilter", hDRPColorAdjustmentColorFilter, helpEnabled);
                hDRPColorAdjustmentHueShift = m_editorUtils.IntSlider("hDRPColorAdjustmentHueShift", hDRPColorAdjustmentHueShift, -100, 100, helpEnabled);
                hDRPColorAdjustmentSaturation = m_editorUtils.Slider("ColorGradingSaturationSlider", hDRPColorAdjustmentSaturation, -100f, 100f, helpEnabled);
                hDRPColorAdjustmentContrast = m_editorUtils.Slider("ColorGradingContrastSlider", hDRPColorAdjustmentContrast, -100f, 100f, helpEnabled);
                EditorGUILayout.Space();

                m_editorUtils.Text("ChannelMixer");
                hDRPChannelMixerRed = m_editorUtils.IntSlider("HDRPChannelMixerRed", hDRPChannelMixerRed, -200, 200, helpEnabled);
                hDRPChannelMixerGreen = m_editorUtils.IntSlider("HDRPChannelMixerGreen", hDRPChannelMixerGreen, -200, 200, helpEnabled);
                hDRPChannelMixerBlue = m_editorUtils.IntSlider("HDRPChannelMixerBlue", hDRPChannelMixerBlue, -200, 200, helpEnabled);
                EditorGUILayout.Space();

                //m_editorUtils.Title("ColorCurves");
                //hDRPColorCurves = (UnityEngine.Experimental.Rendering.HDPipeline.ColorCurves)m_editorUtils.ObjectField("HDRPColorCurves", colorGradingLut, typeof(UnityEngine.Experimental.Rendering.HDPipeline.ColorCurves), false, helpEnabled);

                m_editorUtils.Text("ColorLookup");
                hDRPColorLookupTexture = (Texture)m_editorUtils.ObjectField("ColorGradingLut", hDRPColorLookupTexture, typeof(Texture), false, helpEnabled, GUILayout.Height(16f));
                hDRPColorLookupContribution = m_editorUtils.Slider("HDRPColorLookupContribution", hDRPColorLookupContribution, 0f, 1f, helpEnabled);
                if (hDRPColorLookupTexture != null)
                {
                    if (hDRPColorLookupTexture.GetType() != typeof(Texture3D))
                    {
                        EditorGUILayout.HelpBox("Warning look up texture in High Definition must be a 3D texture", MessageType.Warning);
                    }
                }
                EditorGUILayout.Space();


                //m_editorUtils.Title("LiftGammaGain");
                //hDRPLiftGammaGain = (UnityEngine.Experimental.Rendering.HDPipeline.LiftGammaGain)m_editorUtils.ObjectField("HDRPColorLiftGammaGain", colorGradingLut, typeof(UnityEngine.Experimental.Rendering.HDPipeline.LiftGammaGain), false, helpEnabled);

                m_editorUtils.Text("Tonemapping");
#if !UNITY_2019_3_OR_NEWER
                hDRPTonemappingMode = (UnityEngine.Experimental.Rendering.HDPipeline.TonemappingMode)m_editorUtils.EnumPopup("ColorGradingMode", hDRPTonemappingMode, helpEnabled);
#else
                hDRPTonemappingMode = (UnityEngine.Rendering.HighDefinition.TonemappingMode)m_editorUtils.EnumPopup("ColorGradingMode", hDRPTonemappingMode, helpEnabled);
#endif
                if (hDRPTonemappingMode == TonemappingMode.Custom)
                {
                    hDRPTonemappingToeStrength = m_editorUtils.Slider("hDRPTonemappingToeStrength", hDRPTonemappingToeStrength, 0f, 1f, helpEnabled);
                    hDRPTonemappingToeLength = m_editorUtils.Slider("hDRPTonemappingToeLength", hDRPTonemappingToeLength, 0f, 1f, helpEnabled);
                    hDRPTonemappingShoulderStrength = m_editorUtils.Slider("hDRPTonemappingShoulderStrength", hDRPTonemappingShoulderStrength, 0f, 1f, helpEnabled);
                    hDRPTonemappingShoulderLength = m_editorUtils.Slider("hDRPTonemappingShoulderLength", hDRPTonemappingShoulderLength, 0f, 5f, helpEnabled);
                    hDRPTonemappingShoulderAngle = m_editorUtils.Slider("hDRPTonemappingShoulderAngle", hDRPTonemappingShoulderAngle, 0f, 1f, helpEnabled);
                    hDRPTonemappingGamma = m_editorUtils.Slider("hDRPTonemappingGamma", hDRPTonemappingGamma, 0f, 1f, helpEnabled);
                }
                EditorGUILayout.Space();

                m_editorUtils.Text("SplitToning");
                hDRPSplitToningShadows = m_editorUtils.ColorField("HDRPSplitToningShadows", hDRPSplitToningShadows, helpEnabled);
                hDRPSplitToningHighlights = m_editorUtils.ColorField("HDRPSplitToningHighlights", hDRPSplitToningHighlights, helpEnabled);
                hDRPSplitToningBalance = m_editorUtils.IntSlider("HDRPSplitToningBalance", hDRPSplitToningBalance, -100, 100, helpEnabled);
                EditorGUILayout.Space();

                m_editorUtils.Text("White Balance");
                if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
                {
                    hDRPWhiteBalanceTempature = m_editorUtils.IntSlider("ColorGradingTempatureIntSlider", hDRPWhiteBalanceTempature, -100, 100, helpEnabled);
                }
                else if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable && !syncPostProcessing)
                {
                    hDRPWhiteBalanceTempature = m_editorUtils.IntSlider("ColorGradingTempatureIntSlider", hDRPWhiteBalanceTempature, -100, 100, helpEnabled);
                }
                hDRPWhiteBalanceTint = m_editorUtils.IntSlider("ColorGradingTintIntSlider", hDRPWhiteBalanceTint, -100, 100, helpEnabled);

                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateColorGrading = true;

                m_selectedPostProcessingProfile.hDRPColorLookupTexture = hDRPColorLookupTexture;
                m_selectedPostProcessingProfile.hDRPColorAdjustmentColorFilter = hDRPColorAdjustmentColorFilter;
                m_selectedPostProcessingProfile.hDRPColorAdjustmentPostExposure = hDRPColorAdjustmentPostExposure;
                m_selectedPostProcessingProfile.hDRPColorAdjustmentHueShift = hDRPColorAdjustmentHueShift;
                m_selectedPostProcessingProfile.colorGradingEnabled = colorGradingEnabled;
                m_selectedPostProcessingProfile.hDRPWhiteBalanceTempature = hDRPWhiteBalanceTempature;
                m_selectedPostProcessingProfile.hDRPColorLookupContribution = hDRPColorLookupContribution;
                m_selectedPostProcessingProfile.hDRPWhiteBalanceTint = hDRPWhiteBalanceTint;
                m_selectedPostProcessingProfile.hDRPColorAdjustmentSaturation = hDRPColorAdjustmentSaturation;
                m_selectedPostProcessingProfile.hDRPColorAdjustmentContrast = hDRPColorAdjustmentContrast;
                m_selectedPostProcessingProfile.hDRPChannelMixerRed = hDRPChannelMixerRed;
                m_selectedPostProcessingProfile.hDRPChannelMixerGreen = hDRPChannelMixerGreen;
                m_selectedPostProcessingProfile.hDRPChannelMixerBlue = hDRPChannelMixerBlue;
                m_selectedPostProcessingProfile.hDRPTonemappingMode = hDRPTonemappingMode;
                m_selectedPostProcessingProfile.hDRPTonemappingToeStrength = hDRPTonemappingToeStrength;
                m_selectedPostProcessingProfile.hDRPTonemappingToeLength = hDRPTonemappingToeLength;
                m_selectedPostProcessingProfile.hDRPTonemappingShoulderStrength = hDRPTonemappingShoulderStrength;
                m_selectedPostProcessingProfile.hDRPTonemappingShoulderLength = hDRPTonemappingShoulderLength;
                m_selectedPostProcessingProfile.hDRPTonemappingShoulderAngle = hDRPTonemappingShoulderAngle;
                m_selectedPostProcessingProfile.hDRPTonemappingGamma = hDRPTonemappingGamma;
                m_selectedPostProcessingProfile.hDRPSplitToningShadows = hDRPSplitToningShadows;
                m_selectedPostProcessingProfile.hDRPSplitToningHighlights = hDRPSplitToningHighlights;
                m_selectedPostProcessingProfile.hDRPSplitToningBalance = hDRPSplitToningBalance;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Depth Of Field settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPDepthOfFieldSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Depth Of Field
            depthOfFieldEnabled = m_editorUtils.Toggle("DOFEnabledToggle", depthOfFieldEnabled, helpEnabled);
            if (depthOfFieldEnabled)
            {
                EditorGUI.indentLevel++;
                depthOfFieldMode = (AmbientSkiesConsts.DepthOfFieldMode)m_editorUtils.EnumPopup("DepthOfFieldMode", depthOfFieldMode, helpEnabled);
                if (depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                {
                    depthOfFieldTrackingType = (AmbientSkiesConsts.DOFTrackingType)m_editorUtils.EnumPopup("DepthOfFieldTrackingType", depthOfFieldTrackingType, helpEnabled);
                    EditorGUILayout.BeginHorizontal();
                    m_editorUtils.Text("DOFFocusDistanceIs", GUILayout.Width(144f));
                    m_editorUtils.TextNonLocalized("" + depthOfFieldDistanceString, GUILayout.Width(80f), GUILayout.Height(16f));
                    EditorGUILayout.EndHorizontal();
                    focusOffset = m_editorUtils.Slider("DOFFocusOffset", focusOffset, -100f, 100f, helpEnabled);
                    targetLayer = AmbientSkiesEditorFunctions.LayerMaskField(m_editorUtils, "DOFTargetLayer", targetLayer.value, helpEnabled);
                    maxFocusDistance = m_editorUtils.Slider("DOFMaxFocusDistance", maxFocusDistance, 0f, 5000f, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Text("NearBlur");
                    hDRPDepthOfFieldNearBlurSampleCount = m_editorUtils.IntSlider("HDRPBlurSampleCount", hDRPDepthOfFieldNearBlurSampleCount, 0, 8, helpEnabled);
                    hDRPDepthOfFieldNearBlurMaxRadius = m_editorUtils.Slider("HDRPBlurMaxRadius", hDRPDepthOfFieldNearBlurMaxRadius, 0f, 8f, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Text("FarBlur");
                    hDRPDepthOfFieldFarBlurSampleCount = m_editorUtils.IntSlider("HDRPBlurSampleCount", hDRPDepthOfFieldFarBlurSampleCount, 0, 8, helpEnabled);
                    hDRPDepthOfFieldFarBlurMaxRadius = m_editorUtils.Slider("HDRPBlurMaxRadius", hDRPDepthOfFieldFarBlurMaxRadius, 0f, 8f, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Text("Quality");
#if !UNITY_2019_3_OR_NEWER
                    hDRPDepthOfFieldResolution = (UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldResolution)m_editorUtils.EnumPopup("HDRPDepthOfFieldResolution", hDRPDepthOfFieldResolution, helpEnabled);
#else
                    hDRPDepthOfFieldResolution = (UnityEngine.Rendering.HighDefinition.DepthOfFieldResolution)m_editorUtils.EnumPopup("HDRPDepthOfFieldResolution", hDRPDepthOfFieldResolution, helpEnabled);
#endif
                    hDRPDepthOfFieldHighQualityFiltering = m_editorUtils.Toggle("HDRPDepthOfFieldHighQualityFiltering", hDRPDepthOfFieldHighQualityFiltering, helpEnabled);
                }
                else
                {
                    EditorGUILayout.Space();
                    m_editorUtils.Text("NearBlur");
                    hDRPDepthOfFieldNearBlurStart = m_editorUtils.Slider("HDRPBlurStart", hDRPDepthOfFieldNearBlurStart, 0f, 100f, helpEnabled);
                    hDRPDepthOfFieldNearBlurEnd = m_editorUtils.Slider("HDRPBlurEnd", hDRPDepthOfFieldNearBlurEnd, 0f, 1000f, helpEnabled);
                    hDRPDepthOfFieldNearBlurSampleCount = m_editorUtils.IntSlider("HDRPBlurSampleCount", hDRPDepthOfFieldNearBlurSampleCount, 0, 8, helpEnabled);
                    hDRPDepthOfFieldNearBlurMaxRadius = m_editorUtils.Slider("HDRPBlurMaxRadius", hDRPDepthOfFieldNearBlurMaxRadius, 0f, 8f, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Text("FarBlur");
                    hDRPDepthOfFieldFarBlurStart = m_editorUtils.Slider("HDRPBlurStart", hDRPDepthOfFieldFarBlurStart, 0f, 1000f, helpEnabled);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    hDRPDepthOfFieldFarBlurEnd = m_editorUtils.Slider("HDRPBlurEnd", hDRPDepthOfFieldFarBlurEnd, 0f, 10000f, helpEnabled, GUILayout.MaxWidth(2000f));
                    EditorGUILayout.EndVertical();
                    if (m_editorUtils.ButtonRight("UpdateToTerrainDistance"))
                    {
                        hDRPDepthOfFieldFarBlurEnd = AmbientSkiesEditorFunctions.UpdateDepthOfFieldEndDistanceToTerrain(1.2f, mainCam);
                    }
                    EditorGUILayout.EndHorizontal();
                    hDRPDepthOfFieldFarBlurSampleCount = m_editorUtils.IntSlider("HDRPBlurSampleCount", hDRPDepthOfFieldFarBlurSampleCount, 0, 8, helpEnabled);
                    hDRPDepthOfFieldFarBlurMaxRadius = m_editorUtils.Slider("HDRPBlurMaxRadius", hDRPDepthOfFieldFarBlurMaxRadius, 0f, 8f, helpEnabled);
                    EditorGUILayout.Space();

                    m_editorUtils.Text("Quality");
#if !UNITY_2019_3_OR_NEWER
                    hDRPDepthOfFieldResolution = (UnityEngine.Experimental.Rendering.HDPipeline.DepthOfFieldResolution)m_editorUtils.EnumPopup("HDRPDepthOfFieldResolution", hDRPDepthOfFieldResolution, helpEnabled);
#else
                    hDRPDepthOfFieldResolution = (UnityEngine.Rendering.HighDefinition.DepthOfFieldResolution)m_editorUtils.EnumPopup("HDRPDepthOfFieldResolution", hDRPDepthOfFieldResolution, helpEnabled);
#endif
                    hDRPDepthOfFieldHighQualityFiltering = m_editorUtils.Toggle("HDRPDepthOfFieldHighQualityFiltering", hDRPDepthOfFieldHighQualityFiltering, helpEnabled);
                }

                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateDOF = true;

                m_selectedPostProcessingProfile.depthOfFieldEnabled = depthOfFieldEnabled;
                m_selectedPostProcessingProfile.depthOfFieldMode = depthOfFieldMode;
                m_selectedPostProcessingProfile.depthOfFieldFocusDistance = depthOfFieldFocusDistance;
                if (depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                {
                    m_selectedPostProcessingProfile.depthOfFieldTrackingType = depthOfFieldTrackingType;
                    m_selectedPostProcessingProfile.focusOffset = focusOffset;
                    m_selectedPostProcessingProfile.targetLayer = targetLayer;
                    m_selectedPostProcessingProfile.maxFocusDistance = maxFocusDistance;
                }

                m_selectedPostProcessingProfile.hDRPDepthOfFieldFocusMode = hDRPDepthOfFieldFocusMode;
                m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurStart = hDRPDepthOfFieldNearBlurStart;
                m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurEnd = hDRPDepthOfFieldNearBlurEnd;
                m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurSampleCount = hDRPDepthOfFieldNearBlurSampleCount;
                m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurMaxRadius = hDRPDepthOfFieldNearBlurMaxRadius;
                m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurStart = hDRPDepthOfFieldFarBlurStart;
                m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurEnd = hDRPDepthOfFieldFarBlurEnd;
                m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurSampleCount = hDRPDepthOfFieldFarBlurSampleCount;
                m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurMaxRadius = hDRPDepthOfFieldFarBlurMaxRadius;
                m_selectedPostProcessingProfile.hDRPDepthOfFieldResolution = hDRPDepthOfFieldResolution;
                m_selectedPostProcessingProfile.hDRPDepthOfFieldHighQualityFiltering = hDRPDepthOfFieldHighQualityFiltering;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Grain settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPGrainSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Grain
            grainEnabled = m_editorUtils.Toggle("GrainEnabledToggle", grainEnabled, helpEnabled);
            if (grainEnabled)
            {
                EditorGUI.indentLevel++;
#if !UNITY_2019_3_OR_NEWER
                hDRPFilmGrainType = (UnityEngine.Experimental.Rendering.HDPipeline.FilmGrainLookup)m_editorUtils.EnumPopup("HDRPFilmGrainType", hDRPFilmGrainType, helpEnabled);
#else
                hDRPFilmGrainType = (UnityEngine.Rendering.HighDefinition.FilmGrainLookup)m_editorUtils.EnumPopup("HDRPFilmGrainType", hDRPFilmGrainType, helpEnabled);
#endif
                hDRPFilmGrainIntensity = m_editorUtils.Slider("GrainIntensity", hDRPFilmGrainIntensity, 0f, 1f, helpEnabled);
                hDRPFilmGrainResponse = m_editorUtils.Slider("HDRPGrainResponse", hDRPFilmGrainResponse, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateGrain = true;

                m_selectedPostProcessingProfile.grainEnabled = grainEnabled;
                m_selectedPostProcessingProfile.hDRPFilmGrainType = hDRPFilmGrainType;
                m_selectedPostProcessingProfile.hDRPFilmGrainIntensity = hDRPFilmGrainIntensity;
                m_selectedPostProcessingProfile.hDRPFilmGrainResponse = hDRPFilmGrainResponse;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Lens Distortion settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPLensDistortionSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Lens Distortion
            distortionEnabled = m_editorUtils.Toggle("DistortionEnabledToggle", distortionEnabled, helpEnabled);
            if (distortionEnabled)
            {
                EditorGUI.indentLevel++;
                hDRPLensDistortionIntensity = m_editorUtils.Slider("DistortionIntensitySlider", hDRPLensDistortionIntensity, -100f, 100f, helpEnabled);
                hDRPLensDistortionXMultiplier = m_editorUtils.Slider("HDRPLensDistortionXMultiplier", hDRPLensDistortionXMultiplier, 0f, 1f, helpEnabled);
                hDRPLensDistortionYMultiplier = m_editorUtils.Slider("HDRPLensDistortionYMultiplier", hDRPLensDistortionYMultiplier, 0f, 1f, helpEnabled);
                hDRPLensDistortionCenter = m_editorUtils.Vector2Field("HDRPLensDistortionCenter", hDRPLensDistortionCenter, helpEnabled);
                hDRPLensDistortionScale = m_editorUtils.Slider("HDRPLensDistortionScale", hDRPLensDistortionScale, 0f, 5f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateLensDistortion = true;

                m_selectedPostProcessingProfile.distortionEnabled = distortionEnabled;
                m_selectedPostProcessingProfile.hDRPLensDistortionIntensity = hDRPLensDistortionIntensity;
                m_selectedPostProcessingProfile.hDRPLensDistortionXMultiplier = hDRPLensDistortionXMultiplier;
                m_selectedPostProcessingProfile.hDRPLensDistortionYMultiplier = hDRPLensDistortionYMultiplier;
                m_selectedPostProcessingProfile.hDRPLensDistortionCenter = hDRPLensDistortionCenter;
                m_selectedPostProcessingProfile.hDRPLensDistortionScale = hDRPLensDistortionScale;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Vignette settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPVignetteSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Vignette
            vignetteEnabled = m_editorUtils.Toggle("VignetteEnabledToggle", vignetteEnabled, helpEnabled);
            if (vignetteEnabled)
            {
                EditorGUI.indentLevel++;
#if !UNITY_2019_3_OR_NEWER
                hDRPVignetteMode = (UnityEngine.Experimental.Rendering.HDPipeline.VignetteMode)m_editorUtils.EnumPopup("HDRPVigmetteMode", hDRPVignetteMode, helpEnabled);
                if (hDRPVignetteMode == UnityEngine.Experimental.Rendering.HDPipeline.VignetteMode.Procedural)
                {
                    hDRPVignetteColor = m_editorUtils.ColorField("HDRPVignetteColor", hDRPVignetteColor, helpEnabled);
                    hDRPVignetteCenter = m_editorUtils.Vector2Field("HDRPVignetteCenter", hDRPVignetteCenter, helpEnabled);
                    hDRPVignetteIntensity = m_editorUtils.Slider("VignetteIntensitySlider", hDRPVignetteIntensity, 0f, 1f, helpEnabled);
                    hDRPVignetteSmoothness = m_editorUtils.Slider("VignetteSmoothnessSlider", hDRPVignetteSmoothness, 0f, 1f, helpEnabled);
                    hDRPVignetteRoundness = m_editorUtils.Slider("HDRPVignetteRoundness", hDRPVignetteRoundness, 0f, 1f, helpEnabled);
                    hDRPVignetteRounded = m_editorUtils.Toggle("HDRPVignetteRounded", hDRPVignetteRounded, helpEnabled);
                }
#else
                hDRPVignetteMode = (UnityEngine.Rendering.HighDefinition.VignetteMode)m_editorUtils.EnumPopup("HDRPVigmetteMode", hDRPVignetteMode, helpEnabled);
                if (hDRPVignetteMode == UnityEngine.Rendering.HighDefinition.VignetteMode.Procedural)
                {
                    hDRPVignetteColor = m_editorUtils.ColorField("HDRPVignetteColor", hDRPVignetteColor, helpEnabled);
                    hDRPVignetteCenter = m_editorUtils.Vector2Field("HDRPVignetteCenter", hDRPVignetteCenter, helpEnabled);
                    hDRPVignetteIntensity = m_editorUtils.Slider("VignetteIntensitySlider", hDRPVignetteIntensity, 0f, 1f, helpEnabled);
                    hDRPVignetteSmoothness = m_editorUtils.Slider("VignetteSmoothnessSlider", hDRPVignetteSmoothness, 0f, 1f, helpEnabled);
                    hDRPVignetteRoundness = m_editorUtils.Slider("HDRPVignetteRoundness", hDRPVignetteRoundness, 0f, 1f, helpEnabled);
                    hDRPVignetteRounded = m_editorUtils.Toggle("HDRPVignetteRounded", hDRPVignetteRounded, helpEnabled);
                }
#endif
                else
                {
                    hDRPVignetteColor = m_editorUtils.ColorField("HDRPVignetteColor", hDRPVignetteColor, helpEnabled);
                    hDRPVignetteMask = (Texture2D)m_editorUtils.ObjectField("HDRPVignetteMask", hDRPVignetteMask, typeof(Texture2D), false, helpEnabled, GUILayout.Height(16f));
                    hDRPVignetteMaskOpacity = m_editorUtils.Slider("HDRPVignetteMaskOpacity", hDRPVignetteMaskOpacity, 0f, 1f, helpEnabled);
                }
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateVignette = true;

                m_selectedPostProcessingProfile.vignetteEnabled = vignetteEnabled;
                m_selectedPostProcessingProfile.hDRPVignetteMode = hDRPVignetteMode;
                m_selectedPostProcessingProfile.hDRPVignetteColor = hDRPVignetteColor;
                m_selectedPostProcessingProfile.hDRPVignetteCenter = hDRPVignetteCenter;
                m_selectedPostProcessingProfile.hDRPVignetteIntensity = hDRPVignetteIntensity;
                m_selectedPostProcessingProfile.hDRPVignetteSmoothness = hDRPVignetteSmoothness;
                m_selectedPostProcessingProfile.hDRPVignetteRoundness = hDRPVignetteRoundness;
                m_selectedPostProcessingProfile.hDRPVignetteRounded = hDRPVignetteRounded;
                m_selectedPostProcessingProfile.hDRPVignetteMask = hDRPVignetteMask;
                m_selectedPostProcessingProfile.hDRPVignetteMaskOpacity = hDRPVignetteMaskOpacity;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Motion Blur settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPMotionBlurSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Motion Blur
            motionBlurEnabled = m_editorUtils.Toggle("MotionBlurEnabled", motionBlurEnabled, helpEnabled);
            if (motionBlurEnabled)
            {
                EditorGUI.indentLevel++;
                hDRPMotionBlurIntensity = m_editorUtils.Slider("HDRPMotionBlurIntensity", hDRPMotionBlurIntensity, 0f, 1000f, helpEnabled);
                hDRPMotionBlurSampleCount = m_editorUtils.IntSlider("MotionSampleCount", hDRPMotionBlurSampleCount, 0, 128, helpEnabled);
                hDRPMotionBlurMaxVelocity = m_editorUtils.IntSlider("HDRPMotionBlurMaxVelocity", hDRPMotionBlurMaxVelocity, 0, 1500, helpEnabled);
                hDRPMotionBlurMinVelocity = m_editorUtils.Slider("HDRPMotionBlurMinVelocity", hDRPMotionBlurMinVelocity, 0f, 64F, helpEnabled);
                hDRPMotionBlurCameraRotationVelocityClamp = m_editorUtils.Slider("HDRPMotionBlurCameraRotationVelocityClamp", hDRPMotionBlurCameraRotationVelocityClamp, 0f, 0.2f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateMotionBlur = true;

                m_selectedPostProcessingProfile.motionBlurEnabled = motionBlurEnabled;
                m_selectedPostProcessingProfile.hDRPMotionBlurIntensity = hDRPMotionBlurIntensity;
                m_selectedPostProcessingProfile.hDRPMotionBlurSampleCount = hDRPMotionBlurSampleCount;
                m_selectedPostProcessingProfile.hDRPMotionBlurMaxVelocity = hDRPMotionBlurMaxVelocity;
                m_selectedPostProcessingProfile.hDRPMotionBlurMinVelocity = hDRPMotionBlurMinVelocity;
                m_selectedPostProcessingProfile.hDRPMotionBlurCameraRotationVelocityClamp = hDRPMotionBlurCameraRotationVelocityClamp;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Motion Blur settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDRPPaniniProjectionSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Motion Blur
            hDRPPaniniProjectionEnabled = m_editorUtils.Toggle("HDRPPaniniProjectionEnabled", hDRPPaniniProjectionEnabled, helpEnabled);
            if (hDRPPaniniProjectionEnabled)
            {
                EditorGUI.indentLevel++;
                hDRPPaniniProjectionDistance = m_editorUtils.Slider("HDRPPaniniProjectionDistance", hDRPPaniniProjectionDistance, 0f, 1f, helpEnabled);
                hDRPPaniniProjectionCropToFit = m_editorUtils.Slider("HDRPPaniniProjectionCropToFit", hDRPPaniniProjectionCropToFit, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updatePanini = true;

                m_selectedPostProcessingProfile.hDRPPaniniProjectionEnabled = hDRPPaniniProjectionEnabled;
                m_selectedPostProcessingProfile.hDRPPaniniProjectionDistance = hDRPPaniniProjectionDistance;
                m_selectedPostProcessingProfile.hDRPPaniniProjectionCropToFit = hDRPPaniniProjectionCropToFit;

                m_hasChanged = true;
            }
        }

#endif

#endregion

        #region URP Post FX Tab Functions

        /// <summary>
        /// Main settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPMainPostProcessSettingsEnabled(bool helpEnabled)
        {
#if UPPipeline && UNITY_2019_3_OR_NEWER
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Selection popup
            antiAliasingMode = (AmbientSkiesConsts.AntiAliasingMode)m_editorUtils.EnumPopup("AntiAliasingMode", antiAliasingMode, helpEnabled);
            if (antiAliasingMode == AmbientSkiesConsts.AntiAliasingMode.TAA && renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Universal)
            {
                EditorGUILayout.HelpBox("TAA (Temporal Anti-Aliasing) is not supported in Universal Render Pipeline. Recommend using SMAA (Subpixel Morphological Anti-Aliasing) or MSAA (Multisample Anti-Aliasing) instead.", MessageType.Info);
            }
            hDRMode = (AmbientSkiesConsts.HDRMode)m_editorUtils.EnumPopup("HDRMode", hDRMode, helpEnabled);
            //hideGizmos = m_editorUtils.Toggle("HidePostProcessingGizmos", hideGizmos, helpEnabled);
            dithering = m_editorUtils.Toggle("Dithering", dithering, helpEnabled);

            if (!autoMatchProfile)
            {
                //Match profile to sky profile
                if (m_editorUtils.Button("MatchProfileToSkyboxProfile"))
                {
                    if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
                        m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingHDRIName(m_profiles, m_selectedSkyboxProfile);
                        newPPSelection = m_selectedPostProcessingProfileIndex;
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                        EditorUtility.SetDirty(m_creationToolSettings);
                        m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                    }
                    else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingProceduralName(m_profiles, m_selectedProceduralSkyboxProfile);
                        newPPSelection = m_selectedPostProcessingProfileIndex;
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                        EditorUtility.SetDirty(m_creationToolSettings);
                        m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                    }
                    else
                    {
                        m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.GetPostProcessingGradientName(m_profiles, m_selectedGradientSkyboxProfile);
                        newPPSelection = m_selectedPostProcessingProfileIndex;
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                        EditorUtility.SetDirty(m_creationToolSettings);
                        m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                    }
                }
            }

            if (m_editorUtils.Button("FocusProfile"))
            {
                PostProcessingUtils.FocusPostProcessProfile(m_profiles, m_selectedPostProcessingProfile);
            }

            m_editorUtils.InlineHelp("FocusProfileHelp", helpEnabled);

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.hideGizmos = hideGizmos;
                m_selectedPostProcessingProfile.autoMatchProfile = autoMatchProfile;
                m_selectedPostProcessingProfile.antiAliasingMode = antiAliasingMode;
                m_selectedPostProcessingProfile.hDRMode = hDRMode;
                m_selectedPostProcessingProfile.dithering = dithering;
#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.customPostProcessingProfile = customPostProcessingProfile;
#endif

                m_hasChanged = true;
            }
#endif
        }

#if UNITY_2019_3_OR_NEWER && UPPipeline

        /// <summary>
        /// Ambient Occlusion settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPAmbientOcclusionSettingsEnabled(bool helpEnabled)
        {
            //Add once unity adds support for Ambient Occlusion
        }

        /// <summary>
        /// Auto Exposre settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPAutoExposureSettingsEnabled(bool helpEnabled)
        {
            //Add once unity adds support for Auto Exposure
        }

        /// <summary>
        /// Bloom settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPBloomSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Bloom
            bloomEnabled = m_editorUtils.Toggle("BloomEnabledToggle", bloomEnabled, helpEnabled);
            if (bloomEnabled)
            {
                EditorGUI.indentLevel++;
                uRPBloomIntensity = m_editorUtils.Slider("BloomIntensity", uRPBloomIntensity, 0f, 1f, helpEnabled);
                uRPBloomScatter = m_editorUtils.Slider("HDRPBloomScatter", uRPBloomScatter, 0f, 1f, helpEnabled);
                uRPBloomTint = m_editorUtils.ColorField("HDRPBloomTint", uRPBloomTint, helpEnabled);
                uRPBloomDirtLensTexture = (Texture2D)m_editorUtils.ObjectField("LensTexture", uRPBloomDirtLensTexture, typeof(Texture2D), false, helpEnabled, GUILayout.Height(16f));
                uRPBloomDirtLensIntensity = m_editorUtils.Slider("BloomLensIntensity", uRPBloomDirtLensIntensity, 0f, 20f, helpEnabled);
                EditorGUILayout.Space();

                m_editorUtils.Text("Quality");
                uRPBloomHighQualityFiltering = m_editorUtils.Toggle("hDRPBloomHighQualityFiltering", uRPBloomHighQualityFiltering, helpEnabled);

                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateBloom = true;

                m_selectedPostProcessingProfile.bloomEnabled = bloomEnabled;
                m_selectedPostProcessingProfile.uRPBloomIntensity = uRPBloomIntensity;
                m_selectedPostProcessingProfile.uRPBloomScatter = uRPBloomScatter;
                m_selectedPostProcessingProfile.uRPBloomTint = uRPBloomTint;
                m_selectedPostProcessingProfile.uRPBloomDirtLensTexture = uRPBloomDirtLensTexture;
                m_selectedPostProcessingProfile.uRPBloomDirtLensIntensity = uRPBloomDirtLensIntensity;
                m_selectedPostProcessingProfile.uRPBloomHighQualityFiltering = uRPBloomHighQualityFiltering;
                m_selectedPostProcessingProfile.uRPBloomPrefiler = uRPBloomPrefiler;
                m_selectedPostProcessingProfile.uRPBloomAnamorphic = uRPBloomAnamorphic;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Chromatic Aberration settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPChromaticAberrationSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            chromaticAberrationEnabled = m_editorUtils.Toggle("ChromaticAberrationEnabled", chromaticAberrationEnabled, helpEnabled);
            if (chromaticAberrationEnabled)
            {
                EditorGUI.indentLevel++;
                uRPChromaticAberrationIntensity = m_editorUtils.Slider("ChromaticAberrationIntensity", uRPChromaticAberrationIntensity, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateChromatic = true;

                m_selectedPostProcessingProfile.chromaticAberrationEnabled = chromaticAberrationEnabled;
                m_selectedPostProcessingProfile.uRPChromaticAberrationIntensity = uRPChromaticAberrationIntensity;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Color Grading settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPColorGradingSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Color grading
            colorGradingEnabled = m_editorUtils.Toggle("ColorGradingEnabledToggle", colorGradingEnabled, helpEnabled);
            if (colorGradingEnabled)
            {
                EditorGUI.indentLevel++;
                m_editorUtils.Text("ColorAdjustments");
                uRPColorAdjustmentPostExposure = m_editorUtils.Slider("ColorGradingPostExposure", uRPColorAdjustmentPostExposure, -5f, 5f, helpEnabled);
                uRPColorAdjustmentColorFilter = m_editorUtils.ColorField("ColorGradingColorFilter", uRPColorAdjustmentColorFilter, helpEnabled);
                uRPColorAdjustmentHueShift = m_editorUtils.IntSlider("hDRPColorAdjustmentHueShift", uRPColorAdjustmentHueShift, -100, 100, helpEnabled);
                uRPColorAdjustmentSaturation = m_editorUtils.Slider("ColorGradingSaturationSlider", uRPColorAdjustmentSaturation, -100f, 100f, helpEnabled);
                uRPColorAdjustmentContrast = m_editorUtils.Slider("ColorGradingContrastSlider", uRPColorAdjustmentContrast, -100f, 100f, helpEnabled);
                EditorGUILayout.Space();

                m_editorUtils.Text("ChannelMixer");
                uRPChannelMixerRed = m_editorUtils.IntSlider("HDRPChannelMixerRed", uRPChannelMixerRed, -200, 200, helpEnabled);
                uRPChannelMixerGreen = m_editorUtils.IntSlider("HDRPChannelMixerGreen", uRPChannelMixerGreen, -200, 200, helpEnabled);
                uRPChannelMixerBlue = m_editorUtils.IntSlider("HDRPChannelMixerBlue", uRPChannelMixerBlue, -200, 200, helpEnabled);
                EditorGUILayout.Space();

                //m_editorUtils.Title("ColorCurves");
                //hDRPColorCurves = (UnityEngine.Experimental.Rendering.HDPipeline.ColorCurves)m_editorUtils.ObjectField("HDRPColorCurves", colorGradingLut, typeof(UnityEngine.Experimental.Rendering.HDPipeline.ColorCurves), false, helpEnabled);

                m_editorUtils.Text("ColorLookup");
                uRPColorLookupTexture = (Texture)m_editorUtils.ObjectField("ColorGradingLut", uRPColorLookupTexture, typeof(Texture), false, helpEnabled, GUILayout.Height(16f));
                uRPColorLookupContribution = m_editorUtils.Slider("HDRPColorLookupContribution", uRPColorLookupContribution, 0f, 1f, helpEnabled);
                if (uRPColorLookupTexture != null)
                {
                    if (uRPColorLookupTexture.GetType() != typeof(Texture2D))
                    {
                        EditorGUILayout.HelpBox("Warning look up texture in Universal must be a 2D texture", MessageType.Warning);
                    }
                }
                EditorGUILayout.Space();


                //m_editorUtils.Title("LiftGammaGain");
                //hDRPLiftGammaGain = (UnityEngine.Experimental.Rendering.HDPipeline.LiftGammaGain)m_editorUtils.ObjectField("HDRPColorLiftGammaGain", colorGradingLut, typeof(UnityEngine.Experimental.Rendering.HDPipeline.LiftGammaGain), false, helpEnabled);

                m_editorUtils.Text("Tonemapping");
                uRPTonemappingMode = (AmbientSkiesConsts.URPTonemappingMode)m_editorUtils.EnumPopup("TonemappingMode", uRPTonemappingMode, helpEnabled);
                EditorGUILayout.Space();

                m_editorUtils.Text("SplitToning");
                uRPSplitToningShadows = m_editorUtils.ColorField("HDRPSplitToningShadows", uRPSplitToningShadows, helpEnabled);
                uRPSplitToningHighlights = m_editorUtils.ColorField("HDRPSplitToningHighlights", uRPSplitToningHighlights, helpEnabled);
                uRPSplitToningBalance = m_editorUtils.IntSlider("HDRPSplitToningBalance", uRPSplitToningBalance, -100, 100, helpEnabled);
                EditorGUILayout.Space();

                m_editorUtils.Text("White Balance");
                if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
                {
                    uRPWhiteBalanceTempature = m_editorUtils.IntSlider("ColorGradingTempatureIntSlider", uRPWhiteBalanceTempature, -100, 100, helpEnabled);
                }
                else if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable && !syncPostProcessing)
                {
                    uRPWhiteBalanceTempature = m_editorUtils.IntSlider("ColorGradingTempatureIntSlider", uRPWhiteBalanceTempature, -100, 100, helpEnabled);
                }
                uRPWhiteBalanceTint = m_editorUtils.IntSlider("ColorGradingTintIntSlider", uRPWhiteBalanceTint, -100, 100, helpEnabled);

                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateColorGrading = true;

                m_selectedPostProcessingProfile.uRPColorLookupTexture = uRPColorLookupTexture;
                m_selectedPostProcessingProfile.uRPColorAdjustmentColorFilter = uRPColorAdjustmentColorFilter;
                m_selectedPostProcessingProfile.uRPColorAdjustmentPostExposure = uRPColorAdjustmentPostExposure;
                m_selectedPostProcessingProfile.uRPColorAdjustmentHueShift = uRPColorAdjustmentHueShift;
                m_selectedPostProcessingProfile.colorGradingEnabled = colorGradingEnabled;
                m_selectedPostProcessingProfile.uRPWhiteBalanceTempature = uRPWhiteBalanceTempature;
                m_selectedPostProcessingProfile.uRPColorLookupContribution = uRPColorLookupContribution;
                m_selectedPostProcessingProfile.uRPWhiteBalanceTint = uRPWhiteBalanceTint;
                m_selectedPostProcessingProfile.uRPColorAdjustmentSaturation = uRPColorAdjustmentSaturation;
                m_selectedPostProcessingProfile.uRPColorAdjustmentContrast = uRPColorAdjustmentContrast;
                m_selectedPostProcessingProfile.uRPChannelMixerRed = uRPChannelMixerRed;
                m_selectedPostProcessingProfile.uRPChannelMixerGreen = uRPChannelMixerGreen;
                m_selectedPostProcessingProfile.uRPChannelMixerBlue = uRPChannelMixerBlue;
                m_selectedPostProcessingProfile.uRPTonemappingMode = uRPTonemappingMode;
                m_selectedPostProcessingProfile.uRPTonemappingToeStrength = uRPTonemappingToeStrength;
                m_selectedPostProcessingProfile.uRPTonemappingToeLength = uRPTonemappingToeLength;
                m_selectedPostProcessingProfile.uRPTonemappingShoulderStrength = uRPTonemappingShoulderStrength;
                m_selectedPostProcessingProfile.uRPTonemappingShoulderLength = uRPTonemappingShoulderLength;
                m_selectedPostProcessingProfile.uRPTonemappingShoulderAngle = uRPTonemappingShoulderAngle;
                m_selectedPostProcessingProfile.uRPTonemappingGamma = uRPTonemappingGamma;
                m_selectedPostProcessingProfile.uRPSplitToningShadows = uRPSplitToningShadows;
                m_selectedPostProcessingProfile.uRPSplitToningHighlights = uRPSplitToningHighlights;
                m_selectedPostProcessingProfile.uRPSplitToningBalance = uRPSplitToningBalance;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Depth Of Field settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPDepthOfFieldSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Depth Of Field
            depthOfFieldEnabled = m_editorUtils.Toggle("DOFEnabledToggle", depthOfFieldEnabled, helpEnabled);
            if (depthOfFieldEnabled)
            {
                EditorGUI.indentLevel++;
                uRPDepthOfFieldMode = (AmbientSkiesConsts.URPDepthOfFieldMode)m_editorUtils.EnumPopup("DepthOfFieldMode", uRPDepthOfFieldMode, helpEnabled);
                if (uRPDepthOfFieldMode == AmbientSkiesConsts.URPDepthOfFieldMode.Gaussian)
                {
                    uRPDepthOfFieldStart = m_editorUtils.FloatField("URPDepthOfFieldStart", uRPDepthOfFieldStart, helpEnabled);
                    uRPDepthOfFieldEnd = m_editorUtils.FloatField("URPDepthOfFieldEnd", uRPDepthOfFieldEnd, helpEnabled);
                    uRPDepthOfFieldMaxRadius = m_editorUtils.FloatField("URPDepthOfFieldMaxRadius", uRPDepthOfFieldMaxRadius, helpEnabled);

                    m_editorUtils.Text("Quality");
                    uRPDepthOfFieldHighQualityFiltering = m_editorUtils.Toggle("HDRPDepthOfFieldHighQualityFiltering", uRPDepthOfFieldHighQualityFiltering, helpEnabled);
                }
                else
                {
                    uRPDepthOfFieldFocusDitance = m_editorUtils.FloatField("URPDepthOfFieldFocusDistance", uRPDepthOfFieldFocusDitance, helpEnabled);
                    uRPDepthOfFieldFocalLength = m_editorUtils.IntSlider("URPDepthOfFieldFocalLength", uRPDepthOfFieldFocalLength, 1, 300, helpEnabled);
                    uRPDepthOfFieldAperture = m_editorUtils.Slider("URPDepthOfFieldFocalLength", uRPDepthOfFieldAperture, 1f, 32f, helpEnabled);
                    uRPDepthOfFieldBladeCount = m_editorUtils.IntSlider("URPDepthOfFieldBladeCount", uRPDepthOfFieldBladeCount, 3, 9, helpEnabled);
                    uRPDepthOfFieldBladeCurvature = m_editorUtils.Slider("URPDepthOfFieldBladeCurvature", uRPDepthOfFieldBladeCurvature, 0f, 1f, helpEnabled);
                    uRPDepthOfFieldBladeRotation = m_editorUtils.IntSlider("URPDepthOfFieldBladeRotation", uRPDepthOfFieldBladeRotation, -180, 180, helpEnabled);
                }

                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateDOF = true;

                m_selectedPostProcessingProfile.depthOfFieldEnabled = depthOfFieldEnabled;
                m_selectedPostProcessingProfile.uRPDepthOfFieldMode = uRPDepthOfFieldMode;

                m_selectedPostProcessingProfile.uRPDepthOfFieldStart = uRPDepthOfFieldStart;
                m_selectedPostProcessingProfile.uRPDepthOfFieldEnd = uRPDepthOfFieldEnd;
                m_selectedPostProcessingProfile.uRPDepthOfFieldMaxRadius = uRPDepthOfFieldMaxRadius;
                m_selectedPostProcessingProfile.uRPDepthOfFieldHighQualityFiltering = uRPDepthOfFieldHighQualityFiltering;

                m_selectedPostProcessingProfile.uRPDepthOfFieldFocusDitance = uRPDepthOfFieldFocusDitance;
                m_selectedPostProcessingProfile.uRPDepthOfFieldFocalLength = uRPDepthOfFieldFocalLength;
                m_selectedPostProcessingProfile.uRPDepthOfFieldAperture = uRPDepthOfFieldAperture;
                m_selectedPostProcessingProfile.uRPDepthOfFieldBladeCount = uRPDepthOfFieldBladeCount;
                m_selectedPostProcessingProfile.uRPDepthOfFieldBladeCurvature = uRPDepthOfFieldBladeCurvature;
                m_selectedPostProcessingProfile.uRPDepthOfFieldBladeRotation = uRPDepthOfFieldBladeRotation;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Grain settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPGrainSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Grain
            grainEnabled = m_editorUtils.Toggle("GrainEnabledToggle", grainEnabled, helpEnabled);
            if (grainEnabled)
            {
                EditorGUI.indentLevel++;
                uRPFilmGrainType = (AmbientSkiesConsts.URPFilmGrainType)m_editorUtils.EnumPopup("HDRPFilmGrainType", uRPFilmGrainType, helpEnabled);
                uRPFilmGrainIntensity = m_editorUtils.Slider("GrainIntensity", uRPFilmGrainIntensity, 0f, 1f, helpEnabled);
                uRPFilmGrainResponse = m_editorUtils.Slider("HDRPGrainResponse", uRPFilmGrainResponse, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateGrain = true;

                m_selectedPostProcessingProfile.grainEnabled = grainEnabled;
                m_selectedPostProcessingProfile.uRPFilmGrainType = uRPFilmGrainType;
                m_selectedPostProcessingProfile.uRPFilmGrainIntensity = uRPFilmGrainIntensity;
                m_selectedPostProcessingProfile.uRPFilmGrainResponse = uRPFilmGrainResponse;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Lens Distortion settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPLensDistortionSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Lens Distortion
            distortionEnabled = m_editorUtils.Toggle("DistortionEnabledToggle", distortionEnabled, helpEnabled);
            if (distortionEnabled)
            {
                EditorGUI.indentLevel++;
                uRPLensDistortionIntensity = m_editorUtils.Slider("DistortionIntensitySlider", uRPLensDistortionIntensity, -100f, 100f, helpEnabled);
                uRPLensDistortionXMultiplier = m_editorUtils.Slider("HDRPLensDistortionXMultiplier", uRPLensDistortionXMultiplier, 0f, 1f, helpEnabled);
                uRPLensDistortionYMultiplier = m_editorUtils.Slider("HDRPLensDistortionYMultiplier", uRPLensDistortionYMultiplier, 0f, 1f, helpEnabled);
                uRPLensDistortionCenter = m_editorUtils.Vector2Field("HDRPLensDistortionCenter", uRPLensDistortionCenter, helpEnabled);
                uRPLensDistortionScale = m_editorUtils.Slider("HDRPLensDistortionScale", uRPLensDistortionScale, 0f, 5f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateLensDistortion = true;

                m_selectedPostProcessingProfile.distortionEnabled = distortionEnabled;
                m_selectedPostProcessingProfile.uRPLensDistortionIntensity = uRPLensDistortionIntensity;
                m_selectedPostProcessingProfile.uRPLensDistortionXMultiplier = uRPLensDistortionXMultiplier;
                m_selectedPostProcessingProfile.uRPLensDistortionYMultiplier = uRPLensDistortionYMultiplier;
                m_selectedPostProcessingProfile.uRPLensDistortionCenter = uRPLensDistortionCenter;
                m_selectedPostProcessingProfile.uRPLensDistortionScale = uRPLensDistortionScale;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Vignette settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPVignetteSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Vignette
            vignetteEnabled = m_editorUtils.Toggle("VignetteEnabledToggle", vignetteEnabled, helpEnabled);
            if (vignetteEnabled)
            {
                EditorGUI.indentLevel++;
                uRPVignetteColor = m_editorUtils.ColorField("HDRPVignetteColor", uRPVignetteColor, helpEnabled);
                uRPVignetteCenter = m_editorUtils.Vector2Field("HDRPVignetteCenter", uRPVignetteCenter, helpEnabled);
                uRPVignetteIntensity = m_editorUtils.Slider("VignetteIntensitySlider", uRPVignetteIntensity, 0f, 1f, helpEnabled);
                uRPVignetteSmoothness = m_editorUtils.Slider("VignetteSmoothnessSlider", uRPVignetteSmoothness, 0f, 1f, helpEnabled);
                uRPVignetteRounded = m_editorUtils.Toggle("HDRPVignetteRounded", uRPVignetteRounded, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateVignette = true;

                m_selectedPostProcessingProfile.vignetteEnabled = vignetteEnabled;
                m_selectedPostProcessingProfile.uRPVignetteColor = uRPVignetteColor;
                m_selectedPostProcessingProfile.uRPVignetteCenter = uRPVignetteCenter;
                m_selectedPostProcessingProfile.uRPVignetteIntensity = uRPVignetteIntensity;
                m_selectedPostProcessingProfile.uRPVignetteSmoothness = uRPVignetteSmoothness;
                m_selectedPostProcessingProfile.uRPVignetteRoundness = uRPVignetteRoundness;
                m_selectedPostProcessingProfile.uRPVignetteRounded = uRPVignetteRounded;
                m_selectedPostProcessingProfile.uRPVignetteMask = uRPVignetteMask;
                m_selectedPostProcessingProfile.uRPVignetteMaskOpacity = uRPVignetteMaskOpacity;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Motion Blur settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPMotionBlurSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Motion Blur
            motionBlurEnabled = m_editorUtils.Toggle("MotionBlurEnabled", motionBlurEnabled, helpEnabled);
            if (motionBlurEnabled)
            {
                EditorGUI.indentLevel++;
                uRPMotionBlurQuality = (AmbientSkiesConsts.URPMotionBlurQuality)m_editorUtils.EnumPopup("URPMotionBlurQuality", uRPMotionBlurQuality, helpEnabled);
                uRPMotionBlurIntensity = m_editorUtils.Slider("HDRPMotionBlurIntensity", uRPMotionBlurIntensity, 0f, 1000f, helpEnabled);
                uRPMotionBlurCameraRotationVelocityClamp = m_editorUtils.Slider("HDRPMotionBlurCameraRotationVelocityClamp", uRPMotionBlurCameraRotationVelocityClamp, 0f, 0.2f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateMotionBlur = true;

                m_selectedPostProcessingProfile.motionBlurEnabled = motionBlurEnabled;
                m_selectedPostProcessingProfile.uRPMotionBlurQuality = uRPMotionBlurQuality;
                m_selectedPostProcessingProfile.uRPMotionBlurIntensity = uRPMotionBlurIntensity;
                m_selectedPostProcessingProfile.uRPMotionBlurCameraRotationVelocityClamp = uRPMotionBlurCameraRotationVelocityClamp;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Motion Blur settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPPaniniProjectionSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //Motion Blur
            uRPPaniniProjectionEnabled = m_editorUtils.Toggle("HDRPPaniniProjectionEnabled", uRPPaniniProjectionEnabled, helpEnabled);
            if (uRPPaniniProjectionEnabled)
            {
                EditorGUI.indentLevel++;
                uRPPaniniProjectionDistance = m_editorUtils.Slider("HDRPPaniniProjectionDistance", uRPPaniniProjectionDistance, 0f, 1f, helpEnabled);
                uRPPaniniProjectionCropToFit = m_editorUtils.Slider("HDRPPaniniProjectionCropToFit", uRPPaniniProjectionCropToFit, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updatePanini = true;

                m_selectedPostProcessingProfile.uRPPaniniProjectionEnabled = uRPPaniniProjectionEnabled;
                m_selectedPostProcessingProfile.uRPPaniniProjectionDistance = uRPPaniniProjectionDistance;
                m_selectedPostProcessingProfile.uRPPaniniProjectionCropToFit = uRPPaniniProjectionCropToFit;

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Custom Profile settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void URPCustomProfileSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            customURPPostProcessingprofile = (VolumeProfile)m_editorUtils.ObjectField("CustomPostProcessingProfile", customURPPostProcessingprofile, typeof(VolumeProfile), false, helpEnabled, GUILayout.Height(16f));

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                AmbientSkiesEditorFunctions.SetAllPostFxUpdateToTrue(m_profiles);

                m_selectedPostProcessingProfile.customURPPostProcessingprofile = customURPPostProcessingprofile;

                m_hasChanged = true;
            }
        }

        #endif

        #endregion

        #region Lighting Tab Functions

        /// <summary>
        /// Main Lighting settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void MainLightingSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            autoLightmapGeneration = m_selectedLightingProfile.autoLightmapGeneration;

            //Select profile
            autoLightmapGeneration = m_editorUtils.Toggle("AutoLightmapGeneration", autoLightmapGeneration, helpEnabled);        
            //Get graphic Tiers
            var tier1 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1);
            var tier2 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2);
            var tier3 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3);

            //Set linear deferred lighting
            if (PlayerSettings.colorSpace != ColorSpace.Linear ||
                tier1.renderingPath != RenderingPath.DeferredShading ||
                tier2.renderingPath != RenderingPath.DeferredShading ||
                tier3.renderingPath != RenderingPath.DeferredShading)
            {
                if (m_editorUtils.Button("SetLinearDefferedButton"))
                {
                    if (EditorUtility.DisplayDialog("Alert!!", "Warning you're about to set Linear Color Space and Deferred Rendering Path. If you're Color Space is not already Linear this will require a reimport, this will also close the Ambient Skies window. Are you sure you want to proceed?", "Yes", "No"))
                    {
                        LightingUtils.SetLinearDeferredLighting(this);
                    }
                }
            }

            if (Lightmapping.isRunning)
            {
                //Cancel light bake
                if (m_editorUtils.Button("CancelLightmapBaking"))
                {
                    LightingUtils.CancelLighting();
                }
            }

            if (Lightmapping.isRunning)
            {
                GUI.enabled = false;
            }

            if (Application.isPlaying)
            {
                GUI.enabled = false;
            }
            //Bake lightmaps
            if (m_editorUtils.Button("BakeGlobalLightingButton"))
            {
                LightingUtils.BakeGlobalLighting();
            }
            //Bake lightmaps
            if (m_editorUtils.Button("ClearBakedLightmaps"))
            {
                LightingUtils.ClearLightmapData();
            }

            GUI.enabled = true;

            EditorGUILayout.Space();
            m_editorUtils.Heading("LightingSettingsHeader");
            m_editorUtils.Text("LightingButtonsInfo", GUILayout.MinHeight(100f), GUILayout.MaxHeight(250f), GUILayout.MinWidth(200f), GUILayout.MaxWidth(2000f));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_selectedLightingProfile.autoLightmapGeneration = autoLightmapGeneration;

                LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Lightmap Configuration settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void LightmapConfigurationEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            EditorGUILayout.BeginHorizontal();
            m_editorUtils.Text("LightmappingSelectionDropDown", GUILayout.Width(146f));
            newLightmappingSettings = EditorGUILayout.Popup(m_selectedLightingProfileIndex, lightmappingChoices.ToArray(), GUILayout.MaxWidth(10000f), GUILayout.Height(16f));
            m_selectedLightingProfile = m_profiles.m_lightingProfiles[newLightmappingSettings];

            //Prev / Next
            GUILayout.FlexibleSpace();
            if (newLightmappingSettings == 0)
            {
                GUI.enabled = false;
            }
            if (m_editorUtils.Button("PrevPostProcessingButton"))
            {
                newLightmappingSettings--;
            }
            GUI.enabled = true;
            if (newLightmappingSettings == lightmappingChoices.Count - 1)
            {
                GUI.enabled = false;
            }
            if (m_editorUtils.Button("NextPostProcessingButton"))
            {
                newLightmappingSettings++;
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            #region Creation Mode

            //Creation
            if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
            {
                if (m_profiles.m_lightingProfiles.Count == 1)
                {
                    GUI.enabled = false;
                }

                if (m_editorUtils.Button("RemoveNewProfile"))
                {
                    m_profiles.m_lightingProfiles.Remove(m_selectedLightingProfile);

                    m_newLightingProfileIndex--;

                    //Add lightmaps profile names
                    lightmappingChoices.Clear();
                    foreach (var profile in m_profiles.m_lightingProfiles)
                    {
                        lightmappingChoices.Add(profile.name);
                    }

                    newLightmappingSettings--;
                    m_selectedLightingProfileIndex--;

                    Repaint();

                    return;
                }

                GUI.enabled = true;

                if (m_editorUtils.Button("CreateNewProfile"))
                {
                    AmbientLightingProfile newProfile = new AmbientLightingProfile();
                    m_profiles.m_lightingProfiles.Add(newProfile);
                    newProfile.name = "New Lightmap Profile" + m_newLightingProfileIndex;

                    m_newLightingProfileIndex++;

                    //Add lightmaps profile names
                    lightmappingChoices.Clear();
                    foreach (var profile in m_profiles.m_lightingProfiles)
                    {
                        lightmappingChoices.Add(profile.name);
                    }

                    newLightmappingSettings++;
                    m_selectedLightingProfileIndex++;

                    Repaint();

                    return;
                }
            }

            #endregion

            EditorGUILayout.HelpBox("Changes to the lighting tab may not show when making changes. Close and reopen the window to view changes.", MessageType.Info);

            if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
            {
                m_foldoutCreationModeSettings = m_editorUtils.Panel("ShowCreationModeSettings", CreationLightingMode, m_foldoutCreationModeSettings);
            }

            m_foldoutRealtimeGI = m_editorUtils.Panel("Show Realtime GI Settings", RealtimeGISettingsEnabled, m_foldoutRealtimeGI);
            m_foldoutBakedGI = m_editorUtils.Panel("Show Baked GI Settings", BakedGISettingsEnabled, m_foldoutBakedGI);
            m_foldoutReflectionProbes = m_editorUtils.Panel("Show Reflection Probe Settings", ReflectionProbeSettingsEnabled, m_foldoutReflectionProbes);
            m_foldoutLightProbes = m_editorUtils.Panel("Show Light Probe Settings", LightProbeSettingsEnabled, m_foldoutLightProbes);

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                //Selection changing things - exit immediately to not polute settings
                if (newLightmappingSettings != m_selectedLightingProfileIndex)
                {
                    AmbientSkiesEditorFunctions.SetAllLightingUpdateToTrue(m_profiles);

                    m_selectedLightingProfile = m_profiles.m_lightingProfiles[m_selectedLightingProfileIndex];
                    m_selectedLightingProfileIndex = newLightmappingSettings;

                    LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);

                    AmbientSkiesEditorFunctions.SetAllLightingUpdateToFalse(m_profiles);
                }

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Realtime GI settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void RealtimeGISettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            #region Lightmaps Variables

            realtimeGlobalIllumination = m_selectedLightingProfile.realtimeGlobalIllumination;
            indirectRelolution = m_selectedLightingProfile.indirectRelolution;
            useDirectionalMode = m_selectedLightingProfile.useDirectionalMode;
            lightIndirectIntensity = m_selectedLightingProfile.lightIndirectIntensity;
            lightBoostIntensity = m_selectedLightingProfile.lightBoostIntensity;

            #endregion

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            realtimeGlobalIllumination = m_editorUtils.Toggle("RealtimeGI", realtimeGlobalIllumination, helpEnabled);
            //Realtime GI
            if (realtimeGlobalIllumination)
            {
                EditorGUI.indentLevel++;
                indirectRelolution = m_editorUtils.Slider("IndirectResolution", indirectRelolution, 0f, 40f, helpEnabled);
                useDirectionalMode = m_editorUtils.Toggle("UseDirectionalLightmapsMode", useDirectionalMode, helpEnabled);
                lightIndirectIntensity = m_editorUtils.Slider("LightIndirectIntensity", lightIndirectIntensity, 0f, 5f, helpEnabled);
                lightBoostIntensity = m_editorUtils.Slider("LightBoostIntensity", lightBoostIntensity, 0f, 10f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateRealtime = true;

                m_selectedLightingProfile.realtimeGlobalIllumination = realtimeGlobalIllumination;
                m_selectedLightingProfile.indirectRelolution = indirectRelolution;
                m_selectedLightingProfile.useDirectionalMode = useDirectionalMode;
                m_selectedLightingProfile.lightIndirectIntensity = lightIndirectIntensity;
                m_selectedLightingProfile.lightBoostIntensity = lightBoostIntensity;

                LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Baked GI settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void BakedGISettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            bakedGlobalIllumination = m_selectedLightingProfile.bakedGlobalIllumination;
            lightmappingMode = m_selectedLightingProfile.lightmappingMode;
            lightmapResolution = m_selectedLightingProfile.lightmapResolution;
#if UNITY_2018_3_OR_NEWER
            m_selectedLightingProfile.filterMode = filterMode;
#endif
            lightmapPadding = m_selectedLightingProfile.lightmapPadding;
            useHighResolutionLightmapSize = m_selectedLightingProfile.useHighResolutionLightmapSize;
            compressLightmaps = m_selectedLightingProfile.compressLightmaps;
            ambientOcclusion = m_selectedLightingProfile.ambientOcclusion;
            maxDistance = m_selectedLightingProfile.maxDistance;
            indirectContribution = m_selectedLightingProfile.indirectContribution;
            directContribution = m_selectedLightingProfile.directContribution;
            finalGather = m_selectedLightingProfile.finalGather;
            finalGatherRayCount = m_selectedLightingProfile.finalGatherRayCount;
            finalGatherDenoising = m_selectedLightingProfile.finalGatherDenoising;
            lightIndirectIntensity = m_selectedLightingProfile.lightIndirectIntensity;
            lightBoostIntensity = m_selectedLightingProfile.lightBoostIntensity;

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            bakedGlobalIllumination = m_editorUtils.Toggle("BakedGI", bakedGlobalIllumination, helpEnabled);
            //Baked GI
            if (bakedGlobalIllumination)
            {
                EditorGUI.indentLevel++;
                lightmappingMode = (AmbientSkiesConsts.LightmapperMode)m_editorUtils.EnumPopup("LightmappingMode", lightmappingMode, helpEnabled);
#if !UNITY_2018_3_OR_NEWER
                if (lightmappingMode == AmbientSkiesConsts.LightmapperMode.ProgressiveGPU)
                {
                    EditorUtility.DisplayDialog("Warning Not Supported!", "ProgressiveGPU is not supported in this version of Unity switching back to ProgressiveCPU. Please install Unity 2018.3 or newer to use this feature", "Ok");
                    lightmappingMode = AmbientSkiesConsts.LightmapperMode.ProgressiveCPU;
                }
#endif
#if !UNITY_2020_1_OR_NEWER
                filterMode = (LightmapEditorSettings.FilterMode)m_editorUtils.EnumPopup("LightmapFilterMode", filterMode, helpEnabled);
#else
                filterMode = (LightingSettings.FilterMode)m_editorUtils.EnumPopup("LightmapFilterMode", filterMode, helpEnabled);
#endif
                lightmapResolution = m_editorUtils.Slider("LightmapResolution", lightmapResolution, 0f, 250f, helpEnabled);
                lightmapPadding = m_editorUtils.IntSlider("LightmapPadding", lightmapPadding, 0, 100, helpEnabled);
                useHighResolutionLightmapSize = m_editorUtils.Toggle("UseHighQualityLightMapResolution", useHighResolutionLightmapSize, helpEnabled);
                compressLightmaps = m_editorUtils.Toggle("CompressLightmaps", compressLightmaps, helpEnabled);
                ambientOcclusion = m_editorUtils.Toggle("AmbientOcclusion", ambientOcclusion, helpEnabled);
                if (ambientOcclusion)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.indentLevel++;
                    maxDistance = m_editorUtils.Slider("AOMaxDistance", maxDistance, 0f, 25f, helpEnabled);
                    indirectContribution = m_editorUtils.Slider("AOIndirectContribution", indirectContribution, 0f, 10f, helpEnabled);
                    directContribution = m_editorUtils.Slider("AODirectContribution", directContribution, 0f, 10f, helpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
                finalGather = m_editorUtils.Toggle("FinalGather", finalGather, helpEnabled);
                if (finalGather)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.indentLevel++;
                    finalGatherRayCount = m_editorUtils.IntSlider("FinalGatherRayCount", finalGatherRayCount, 0, 4096, helpEnabled);
                    finalGatherDenoising = m_editorUtils.Toggle("FinalGatherDenoising", finalGatherDenoising, helpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
                lightIndirectIntensity = m_editorUtils.Slider("LightIndirectIntensity", lightIndirectIntensity, 0f, 5f, helpEnabled);
                lightBoostIntensity = m_editorUtils.Slider("LightBoostIntensity", lightBoostIntensity, 0f, 10f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_profiles.m_updateBaked = true;

                m_selectedLightingProfile.bakedGlobalIllumination = bakedGlobalIllumination;
                m_selectedLightingProfile.lightmappingMode = lightmappingMode;
                m_selectedLightingProfile.lightmapResolution = lightmapResolution;
                m_selectedLightingProfile.lightmapPadding = lightmapPadding;
                m_selectedLightingProfile.useHighResolutionLightmapSize = useHighResolutionLightmapSize;
                m_selectedLightingProfile.compressLightmaps = compressLightmaps;
                m_selectedLightingProfile.ambientOcclusion = ambientOcclusion;
                m_selectedLightingProfile.maxDistance = maxDistance;
                m_selectedLightingProfile.indirectContribution = indirectContribution;
                m_selectedLightingProfile.directContribution = directContribution;
                m_selectedLightingProfile.lightIndirectIntensity = lightIndirectIntensity;
                m_selectedLightingProfile.lightBoostIntensity = lightBoostIntensity;
                m_selectedLightingProfile.finalGather = finalGather;
                m_selectedLightingProfile.finalGatherRayCount = finalGatherRayCount;
                m_selectedLightingProfile.finalGatherDenoising = finalGatherDenoising;
#if UNITY_2018_3_OR_NEWER
                m_selectedLightingProfile.filterMode = filterMode;
#endif

                LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);

                m_hasChanged = true;
            }
        }

        /// <summary>
        /// Refleaction Probe settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ReflectionProbeSettingsEnabled(bool helpEnabled)
        {
            #if !GAIA_2_PRESENT
            EditorGUI.BeginChangeCheck();

            reflectionProbeName = m_selectedLightingProfile.reflectionProbeName;
            reflectionProbeSpawnType = m_selectedLightingProfile.reflectionProbeSpawnType;
            reflectionProbeMode = m_selectedLightingProfile.reflectionProbeMode;
            reflectionProbeRefresh = m_selectedLightingProfile.reflectionProbeRefresh;
            reflectionCubemapCompression = m_selectedLightingProfile.reflectionCubemapCompression;
            reflectionProbeTimeSlicingMode = m_selectedLightingProfile.reflectionProbeTimeSlicingMode;
            reflectionProbeScale = m_selectedLightingProfile.reflectionProbeScale;
            reflectionProbeOffset = m_selectedLightingProfile.reflectionProbeOffset;
            reflectionProbesPerRow = m_selectedLightingProfile.reflectionProbesPerRow;
            reflectionProbeClipPlaneDistance = m_selectedLightingProfile.reflectionProbeClipPlaneDistance;
            //reflectionProbeBlendDistance = m_selectedLightingProfile.reflectionProbeBlendDistance;
            reflectionprobeCullingMask = m_selectedLightingProfile.reflectionprobeCullingMask;
            reflectionProbeShadowDistance = m_selectedLightingProfile.reflectionProbeShadowDistance;
            reflectionProbeResolution = m_selectedLightingProfile.reflectionProbeResolution;
            seaLevel = m_selectedLightingProfile.seaLevel;

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            m_editorUtils.Link("LearnMoreAboutReflectionProbes");
            EditorGUILayout.Space();

            reflectionProbeSpawnType = (AmbientSkiesConsts.ReflectionProbeSpawnType)m_editorUtils.EnumPopup("ReflectionProbeSpawnType", reflectionProbeSpawnType, helpEnabled);
            EditorGUILayout.Space();

            if (reflectionProbeSpawnType == AmbientSkiesConsts.ReflectionProbeSpawnType.ManualPlacement)
            {
                m_editorUtils.Label("Object Naming");
                EditorGUI.indentLevel++;
                reflectionProbeName = m_editorUtils.TextField("ReflectionProbeName", reflectionProbeName, helpEnabled);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            m_editorUtils.Label("Probe Rendering Settings");
            EditorGUI.indentLevel++;
            reflectionProbeMode = (ReflectionProbeMode)m_editorUtils.EnumPopup("ReflectionProbeMode", reflectionProbeMode, helpEnabled);
            reflectionProbeRefresh = (ReflectionProbeRefreshMode)m_editorUtils.EnumPopup("ReflectionProbeRefresh", reflectionProbeRefresh, helpEnabled);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            if (reflectionProbeSpawnType == AmbientSkiesConsts.ReflectionProbeSpawnType.AutomaticallyGenerated)
            {
                m_editorUtils.Label("Probe Size Settings");
                EditorGUI.indentLevel++;
#if GAIA_2_PRESENT
                GaiaSessionManager sessionManager = FindObjectOfType<GaiaSessionManager>();
                if (sessionManager != null)
                {
                    seaLevel = m_editorUtils.FloatField("CurrentSeaLevel", seaLevel, helpEnabled);
                    if (seaLevel < 0)
                    {
                        seaLevel = 0f;
                        m_selectedLightingProfile.seaLevel = 0f;
                    }

                    float gaiaSeaLevel = sessionManager.GetSeaLevel();
                    if (seaLevel != gaiaSeaLevel)
                    {
                        if (m_editorUtils.Button("MatchSeaLevelToGaia"))
                        {
                            seaLevel = sessionManager.GetSeaLevel();
                            m_selectedLightingProfile.seaLevel = seaLevel;
                        }

                        if (m_editorUtils.Button("SetSeaLevelInGaia"))
                        {
                            bool sessionLocked = sessionManager.IsLocked();
                            if (sessionLocked)
                            {
                                sessionManager.UnLockSession();
                                sessionManager.SetSeaLevel(seaLevel);
                                sessionManager.LockSession();
                            }
                            else
                            {
                                sessionManager.SetSeaLevel(seaLevel);
                            }
                        }
                    }
                }
#endif
                reflectionProbesPerRow = m_editorUtils.IntField("ReflectionProbesPerRow", reflectionProbesPerRow, helpEnabled);
                reflectionProbeOffset = m_editorUtils.FloatField("ReflectionProbeOffset", reflectionProbeOffset, helpEnabled);
                Terrain t = Terrain.activeTerrain;
                if (t != null)
                {
                    int probeDist = 100;
                    probeDist = (int)(t.terrainData.size.x / reflectionProbesPerRow);
                    m_editorUtils.LabelField("ProbeDistanceLbl", new GUIContent(probeDist.ToString()));
                }
                m_editorUtils.LabelField("ProbesPerTerrainLbl", new GUIContent(probeSpawnCount));
                if (reflectionProbesPerRow < 2)
                {
                    EditorGUILayout.HelpBox("Please set a value of 2 or higher in the Probes Per Row", MessageType.Warning);
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                m_editorUtils.Label("Probe Size Settings");
                EditorGUI.indentLevel++;
                reflectionProbeScale = m_editorUtils.Vector3Field("ReflectionProbeScale", reflectionProbeScale, helpEnabled);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();

            m_editorUtils.Label("Probe Optimization Settings");
            EditorGUI.indentLevel++;
            if (reflectionProbeRefresh == ReflectionProbeRefreshMode.ViaScripting)
            {
                reflectionProbeTimeSlicingMode = (ReflectionProbeTimeSlicingMode)m_editorUtils.EnumPopup("ReflectionProbeTimeSlicing", reflectionProbeTimeSlicingMode, helpEnabled);
            }
            if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                reflectionProbeResolution = (AmbientSkiesConsts.ReflectionProbeResolution)m_editorUtils.EnumPopup("ReflectionProbeResolution", reflectionProbeResolution, helpEnabled);
                reflectionCubemapCompression = (ReflectionCubemapCompression)m_editorUtils.EnumPopup("ReflectionProbeCompression", reflectionCubemapCompression, helpEnabled);
            }
            reflectionProbeClipPlaneDistance = m_editorUtils.Slider("ReflectionProbeRenderDistance", reflectionProbeClipPlaneDistance, 0.1f, 10000f, helpEnabled);
            reflectionProbeShadowDistance = m_editorUtils.Slider("ReflectionProbeShadowDistance", reflectionProbeShadowDistance, 0.1f, 3000f, helpEnabled);
            reflectionprobeCullingMask = AmbientSkiesEditorFunctions.LayerMaskField(m_editorUtils, "ReflectionProbeCullingMask", reflectionprobeCullingMask.value, helpEnabled);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            if (reflectionProbeSpawnType == AmbientSkiesConsts.ReflectionProbeSpawnType.AutomaticallyGenerated)
            {
                if (m_editorUtils.Button("Generate Global Scene Reflection Probes"))
                {
                    if (EditorUtility.DisplayDialog("Warning!", "You're about to generate reflection probes to cover your whole terrain. Depending on your terrain size and Probe Per Row count this could take some time. Would you like to proceed?", "Yes", "No"))
                    {
                        ReflectionProbeUtils.CreateAutomaticProbes(m_selectedLightingProfile, renderPipelineSettings);
                    }
                }
            }
            else
            {
                if (m_editorUtils.Button("Create Probe At Scene View Location"))
                {
                    ReflectionProbeUtils.CreateProbeAtLocation(m_selectedLightingProfile, renderPipelineSettings);
                }
            }

            if (m_editorUtils.Button("Clear Created Reflection Probes"))
            {
                if (EditorUtility.DisplayDialog("Warning!", "You are about to clear all your reflection probes you created. Are you sure you want to proceed?", "Yes", "No"))
                {
                    ReflectionProbeUtils.ClearCreatedReflectionProbes();
                }
            }

            if (ReflectionProbeUtils.m_currentProbeCount > 0 && renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                EditorGUILayout.HelpBox("To allow the sky to be affected in the reflection probe bake be sure to set the Volume Layer Mask to Transparent FX layer.", MessageType.Info);
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_profiles, "Made changes");
                EditorUtility.SetDirty(m_profiles);

                m_selectedLightingProfile.reflectionProbeName = reflectionProbeName;
                m_selectedLightingProfile.reflectionProbeSpawnType = reflectionProbeSpawnType;
                m_selectedLightingProfile.reflectionProbeMode = reflectionProbeMode;
                m_selectedLightingProfile.reflectionProbeRefresh = reflectionProbeRefresh;
                m_selectedLightingProfile.reflectionCubemapCompression = reflectionCubemapCompression;
                m_selectedLightingProfile.reflectionProbeTimeSlicingMode = reflectionProbeTimeSlicingMode;
                m_selectedLightingProfile.reflectionProbeScale = reflectionProbeScale;
                m_selectedLightingProfile.reflectionProbeOffset = reflectionProbeOffset;
                m_selectedLightingProfile.reflectionProbesPerRow = reflectionProbesPerRow;
                m_selectedLightingProfile.reflectionProbeClipPlaneDistance = reflectionProbeClipPlaneDistance;
                //m_selectedLightingProfile.reflectionProbeBlendDistance = reflectionProbeBlendDistance;
                m_selectedLightingProfile.reflectionprobeCullingMask = reflectionprobeCullingMask;
                m_selectedLightingProfile.reflectionProbeShadowDistance = reflectionProbeShadowDistance;
                m_selectedLightingProfile.reflectionProbeResolution = reflectionProbeResolution;
                m_selectedLightingProfile.seaLevel = seaLevel;

                LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);

                m_hasChanged = true;
            }
#else
            EditorGUILayout.HelpBox("To use light and reflection probe generation go to the Gaia Manager under the lighting tab.", MessageType.Info);
#endif
        }

        /// <summary>
        /// Light Probe settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void LightProbeSettingsEnabled(bool helpEnabled)
        {
#if !GAIA_2_PRESENT
            EditorGUI.BeginChangeCheck();

            lightProbeSpawnType = m_selectedLightingProfile.lightProbeSpawnType;
            lightProbesPerRow = m_selectedLightingProfile.lightProbesPerRow;
            //lightProbeSpawnRadius = m_selectedLightingProfile.lightProbeSpawnRadius;
            seaLevel = m_selectedLightingProfile.seaLevel;

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            m_editorUtils.Link("LearnMoreAboutLightProbes");
            EditorGUILayout.Space();

            lightProbeSpawnType = (AmbientSkiesConsts.LightProbeSpawnType)m_editorUtils.EnumPopup("LightProbeSpawnType", lightProbeSpawnType, helpEnabled);
            if (lightProbeSpawnType == AmbientSkiesConsts.LightProbeSpawnType.AutomaticallyGenerated || lightProbeSpawnType == AmbientSkiesConsts.LightProbeSpawnType.MinDefaultHeight)
            {
                seaLevel = m_editorUtils.FloatField("CurrentSeaLevel", seaLevel, helpEnabled);
                lightProbesPerRow = m_editorUtils.IntField("LightProbesPerRow", lightProbesPerRow, helpEnabled);
                if (lightProbesPerRow < 2)
                {
                    EditorGUILayout.HelpBox("Please set a value of 2 or higher in the Light Probes Per Row", MessageType.Warning);
                }

                Terrain t = Terrain.activeTerrain;
                if (t != null)
                {
                    int probeDist = 100;
                    probeDist = (int)(t.terrainData.size.x / lightProbesPerRow);
                    m_editorUtils.LabelField("ProbeDistanceLbl", new GUIContent(probeDist.ToString()));
                }

                m_editorUtils.LabelField("ProbesPerTerrainLbl", new GUIContent(lightProbeSpawnCount));

                if (m_editorUtils.Button("Generate Global Scene Light Probes"))
                {
                    if (EditorUtility.DisplayDialog("Warning!", "You're about to generate light probes to cover your whole terrain. Depending on your terrain size and Probe Per Row count this could take some time. Would you like to proceed?", "Yes", "No"))
                    {
                        AmbientSkies.LightProbeUtils.CreateAutomaticProbes(m_selectedLightingProfile);
                    }
                }

                if (m_editorUtils.Button("Clear Created Light Probes"))
                {
                    if (EditorUtility.DisplayDialog("Warning!", "You are about to clear all your light probes you created. Are you sure you want to proceed?", "Yes", "No"))
                    {
                        AmbientSkies.LightProbeUtils.ClearCreatedLightProbes();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("This spawn type is not yet supported.", MessageType.Info);
            }
            if (AmbientSkies.LightProbeUtils.m_currentProbeCount > 0)
            {
                EditorGUILayout.HelpBox("Light Probes need to be baked. Go to Main Settings in the lighting tab and press Bake Global Lighting (slow) to make your lighting and Light Probes", MessageType.Info);
            }

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                m_selectedLightingProfile.lightProbeSpawnType = lightProbeSpawnType;
                m_selectedLightingProfile.lightProbesPerRow = lightProbesPerRow;
                m_selectedLightingProfile.seaLevel = seaLevel;

                LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);

                m_hasChanged = true;
            }

#else
            EditorGUILayout.HelpBox("To use light and reflection probe generation go to the Gaia Manager under the lighting tab.", MessageType.Info);
#endif

        }

        #endregion

        #region Massive Clouds Tab Function

        /// <summary>
        /// Massive Clouds system settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void MassiveCloudsSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

#if Mewlist_Clouds
            massiveCloudsEnabled = m_editorUtils.ToggleLeft("MassiveCloudsEnabled", massiveCloudsEnabled, helpEnabled);
            if (massiveCloudsEnabled)
            {
                EditorGUI.indentLevel++;
                cloudProfile = (MassiveCloudsProfile)m_editorUtils.ObjectField("CloudProfile", cloudProfile, typeof(MassiveCloudsProfile), false, helpEnabled, GUILayout.Height(16f));
                syncGlobalFogColor = m_editorUtils.Toggle("SyncCloudGlobalFogColor", syncGlobalFogColor, helpEnabled);
                if (!syncGlobalFogColor)
                {
                    EditorGUI.indentLevel++;
                    cloudsFogColor = m_editorUtils.ColorField("CloudFogColor", cloudsFogColor, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                syncBaseFogColor = m_editorUtils.Toggle("SyncCloudBaseFogColor", syncBaseFogColor, helpEnabled);
                if (!syncBaseFogColor)
                {
                    EditorGUI.indentLevel++;
                    cloudsBaseFogColor = m_editorUtils.ColorField("CloudBaseFogColor", cloudsBaseFogColor, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

             GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                m_selectedPostProcessingProfile.massiveCloudsEnabled = massiveCloudsEnabled;
                m_selectedPostProcessingProfile.cloudProfile = cloudProfile;
                m_selectedPostProcessingProfile.syncGlobalFogColor = syncGlobalFogColor;
                m_selectedPostProcessingProfile.cloudsFogColor = cloudsFogColor;
                m_selectedPostProcessingProfile.cloudsBaseFogColor = cloudsBaseFogColor;
                m_selectedPostProcessingProfile.syncBaseFogColor = syncBaseFogColor;
                m_selectedPostProcessingProfile.cloudIsHDRP = cloudIsHDRP;
            }
#endif
        }

        #endregion

        #region Info Function

        /// <summary>
        /// Built-in foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void BuiltInInfoSettings(bool helpEnabled)
        {
            m_editorUtils.Link("TutorialQuickStart");
            m_editorUtils.Link("TutorialIntergration");
            EditorGUILayout.Space();
            m_editorUtils.Title("AboutBuiltInTitle");
            m_editorUtils.Text("AboutBuiltIn");
            m_editorUtils.Link("LearnMoreAboutBuiltIn");
            EditorGUILayout.Space();
            m_editorUtils.Title("InformationAndTipsTitle");
            m_editorUtils.Text("TipsAndInfoBuiltIn");
        }

        /// <summary>
        /// Lightweight foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void LightweightInfoSettings(bool helpEnabled)
        {
            m_editorUtils.Link("TutorialQuickStart");
            m_editorUtils.Link("TutorialIntergration");
            EditorGUILayout.Space();
            m_editorUtils.Title("AboutLWRPTitle");
            m_editorUtils.Text("AboutLWRP");
            m_editorUtils.Link("LearnMoreAboutLWRP");
            EditorGUILayout.Space();
            m_editorUtils.Title("InformationAndTipsTitle");
            m_editorUtils.Text("TipsAndInfoLWRP");
        }

        /// <summary>
        /// High Definition foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HighDefinitionInfoSettings(bool helpEnabled)
        {
            m_editorUtils.Link("TutorialQuickStart");
            m_editorUtils.Link("TutorialIntergration");
            EditorGUILayout.Space();
            m_editorUtils.Title("AboutHDRPTitle");
            m_editorUtils.Text("AboutHDRP");
            m_editorUtils.Link("LearnMoreAboutHDRP");
            EditorGUILayout.Space();
            m_editorUtils.Title("InformationAndTipsTitle");
            m_editorUtils.Text("TipsAndInfoHDRP");
        }

        /// <summary>
        /// Creation Mode foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void CreateModeSettings(bool helpEnabled)
        {
            EditorGUILayout.BeginHorizontal();

            m_newSystemName = m_editorUtils.TextField("NewSystemName", m_newSystemName, helpEnabled, GUILayout.Height(18f), GUILayout.MaxWidth(2000f));
            if (m_editorUtils.ButtonRight("NewSystemProfile", GUILayout.ExpandWidth(true), GUILayout.Height(18f)))
            {
                CreateNewProfile();
            }

            EditorGUILayout.EndHorizontal();

            m_editorUtils.InlineHelp("NewSystemHelp", helpEnabled);

            if (m_profiles == null)
            {
                return;
            }

            if (!m_profiles.m_isProceduralCreatedProfile)
            {
                m_enableEditMode = m_editorUtils.Toggle("EnableEditMode", m_enableEditMode, helpEnabled);

                if (m_editorUtils.Button("RepairProfile"))
                {
                    RepairProfileIndex();
                }

                if (m_editorUtils.Button("DeleteProfile"))
                {
                    DeleteCreatedProfile();
                }
            }

            if(!m_profiles.m_isProceduralCreatedProfile)
            {
                EditorGUILayout.Space();
                m_editorUtils.Text("CreationHelpInformation");
            }

            m_profiles.m_editSettings = m_enableEditMode;
        }

        /// <summary>
        /// Pipeline Settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void PipelineSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            #region Material Conversion

            if (m_editorUtils.Button("Covert Materials"))
            {
                List<Material> materials = AmbientSkiesEditorFunctions.GetAllSkyMaterialsProjectSearch("t:Material");
                Terrain terrain = Terrain.activeTerrain;
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    if (materials != null)
                    {
                        foreach (Material mat in materials)
                        {
                            if (mat.shader == Shader.Find("Lightweight Render Pipeline/Lit"))
                            {
                                mat.shader = Shader.Find("HDRP/Lit");
                            }

                            if (mat.shader == Shader.Find("Standard"))
                            {
                                mat.shader = Shader.Find("HDRP/Lit");
                            }
                        }

                        if (terrain != null)
                        {
                            Material terrainMat = terrain.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Pipeline Terrain Material"));
                            terrainMat.shader = Shader.Find("HDRP/TerrainLit");
                        }
                    }
                }
                else if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Universal)
                {
                    if (materials != null)
                    {
                        foreach (Material mat in materials)
                        {
                            if (mat.shader == Shader.Find("HDRP/Lit"))
                            {
                                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                            }
                        }

                        foreach (Material mat in materials)
                        {
                            if (mat.shader == Shader.Find("Standard"))
                            {
                                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                            }
                        }

                        if (terrain != null)
                        {
                            Material terrainMat = terrain.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Pipeline Terrain Material"));
                            terrainMat.shader = Shader.Find("Lightweight Render Pipeline/Terrain/Lit");
                        }
                    }
                }
                else
                {
                    if (materials != null)
                    {
                        foreach (Material mat in materials)
                        {
                            if (mat.shader == Shader.Find("HDRP/Lit"))
                            {
                                mat.shader = Shader.Find("Standard");
                            }

                            if (mat.shader == Shader.Find("Lightweight Render Pipeline/Lit"))
                            {
                                mat.shader = Shader.Find("Standard");
                            }
                        }

                        if (terrain != null)
                        {
                            terrain.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Default-Terrain-Standard"));
                        }
                    }
                }

                AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(m_profiles);
                AmbientSkiesEditorFunctions.SetAllPostFxUpdateToTrue(m_profiles);

                //Update skybox
                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                //Update post processing
                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);

                AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToFalse(m_profiles);
                AmbientSkiesEditorFunctions.SetAllPostFxUpdateToFalse(m_profiles);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            #endregion

            if (EditorGUI.EndChangeCheck())
            {

            }
        }

        #endregion

        #endregion

        #region HelperMethods

        #region OnEnable

        /// <summary>
        /// Loads the settings based ont he bool input
        /// </summary>
        /// <param name="loadingFromNewScene"></param>
        public void LoadAndApplySettings(bool loadingFromNewScene)
        {
            //Checks and reverts index if exceeds limit count
            AmbientSkiesEditorFunctions.CheckProfilesLength(m_profiles, m_creationToolSettings, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, m_selectedPostProcessingProfileIndex, m_selectedLightingProfileIndex);

            //Get Current Saved
            if (m_selectedSkyboxProfileIndex >= 0)
            {
                m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_creationToolSettings.m_selectedHDRI];
                newSkyboxSelection = m_creationToolSettings.m_selectedHDRI;
            }
            else
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("HDRI Skybox Profile Empty");
                }

                m_selectedSkyboxProfile = null;
            }

            if (m_selectedProceduralSkyboxProfileIndex >= 0)
            {
                m_selectedProceduralSkyboxProfile = m_profiles.m_proceduralSkyProfiles[m_creationToolSettings.m_selectedProcedural];
                newProceduralSkyboxSelection = m_creationToolSettings.m_selectedProcedural;
            }
            else
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("Procedural Skybox Profile Empty");
                }

                m_selectedProceduralSkyboxProfile = null;
            }

            if (m_selectedGradientSkyboxProfileIndex >= 0)
            {
                m_selectedGradientSkyboxProfile = m_profiles.m_gradientSkyProfiles[m_creationToolSettings.m_selectedGradient];
                newGradientSkyboxSelection = m_creationToolSettings.m_selectedGradient;
            }
            else
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("Gradient Skybox Profile Empty");
                }

                m_selectedGradientSkyboxProfile = null;
            }

            if (m_selectedPostProcessingProfileIndex >= 0)
            {
                m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_creationToolSettings.m_selectedPostProcessing];
                newPPSelection = m_creationToolSettings.m_selectedPostProcessing;
            }
            else
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("Post Processing Profile Empty");
                }

                m_selectedPostProcessingProfile = null;
            }

            //Get profile index from scene
            if (!loadingFromNewScene)
            {
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    //m_selectedSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromActiveSkybox(m_profiles, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, true);
                    m_selectedSkyboxProfileIndex = m_creationToolSettings.m_selectedHDRI;
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    //m_selectedProceduralSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromActiveSkybox(m_profiles, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, true);
                    m_selectedProceduralSkyboxProfileIndex = m_creationToolSettings.m_selectedProcedural;
                }
                else
                {
                    //m_selectedGradientSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromActiveSkybox(m_profiles, m_selectedSkyboxProfile, m_selectedProceduralSkyboxProfile, m_selectedGradientSkyboxProfile, true);
                    m_selectedGradientSkyboxProfileIndex = m_creationToolSettings.m_selectedGradient;
                }

                //Assigns post processing profile
                m_selectedPostProcessingProfileIndex = m_creationToolSettings.m_selectedPostProcessing;

                /*
                string postProcessingName = "AmbientSkiesPostProcessing_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode();
                if (EditorPrefs.HasKey(postProcessingName))
                {
                    string profileName = EditorPrefs.GetString(postProcessingName);
                    int idx = PostProcessingUtils.GetProfileIndexFromProfileName(m_profiles, profileName);
                    if (idx >= 0 && idx < m_profiles.m_ppProfiles.Count)
                    {
                        m_selectedPostProcessingProfileIndex = idx;
                    }
                    else
                    {
                        m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromPostProcessing(m_profiles);
                    }
                }
                else
                {
                    m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromPostProcessing(m_profiles);
                }
                */

                if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                {
                    SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
                }

                if (m_selectedPostProcessingProfileIndex >= 0)
                {
                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                    if (m_profiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && GameObject.Find("Global Post Processing") == null)
                    {
                        m_profiles.m_usePostFX = false;
                    }
                    else if (m_profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && GameObject.Find("Post Processing HDRP Volume") == null)
                    {
                        m_profiles.m_usePostFX = false;
                    }
                    else
                    {
                        m_profiles.m_usePostFX = true;
                    }
                }
                else
                {
                    if (m_profiles.m_showDebug)
                    {
                        Debug.Log("Post Processing Profile Empty");
                    }

                    m_selectedPostProcessingProfile = null;
                }

                //Assigns lighting profile
                string lightingProfileName = EditorPrefs.GetString("AmbientSkiesLighting_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode());
                if (!string.IsNullOrEmpty(lightingProfileName))
                {
                    int idx = LightingUtils.GetProfileIndexFromProfileName(m_profiles, lightingProfileName);
                    if (idx >= 0 && idx < m_profiles.m_lightingProfiles.Count)
                    {
                        m_selectedLightingProfileIndex = idx;
                    }
                    else
                    {
                        m_selectedLightingProfileIndex = 0;
                    }
                }
                else
                {
                    m_selectedLightingProfileIndex = 0;
                }

                if (m_selectedLightingProfileIndex >= 0)
                {
                    m_selectedLightingProfile = m_profiles.m_lightingProfiles[m_selectedLightingProfileIndex];
                    if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        //Update lightmap settings                   
                        LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);
                    }
                }
                else
                {
                    m_selectedLightingProfile = null;
                }
            }
            else
            {
                m_selectedSkyboxProfileIndex = SkyboxUtils.GetFromIsPWProfile(m_profiles, false);
                if (m_selectedSkyboxProfileIndex >= 0 && m_selectedProceduralSkyboxProfileIndex >= 0 && m_selectedGradientSkyboxProfileIndex >= 0)
                {
                    m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_creationToolSettings.m_selectedHDRI];
                    m_selectedProceduralSkyboxProfile = m_profiles.m_proceduralSkyProfiles[m_creationToolSettings.m_selectedProcedural];
                    m_selectedGradientSkyboxProfile = m_profiles.m_gradientSkyProfiles[m_creationToolSettings.m_selectedGradient];

                    if (m_profiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && RenderSettings.skybox.name != "Ambient Skies Skybox")
                    {
                        m_profiles.m_systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;
                    }
                    else if (m_profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && GameObject.Find("High Definition Environment Volume") == null)
                    {
                        m_profiles.m_systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;
                    }

                    if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
                    }

                    if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
                        EditorPrefs.SetString("AmbientSkiesProfile_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex].name);
                    }
                    else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        EditorPrefs.SetString("AmbientSkiesProfile_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_proceduralSkyProfiles[m_selectedProceduralSkyboxProfileIndex].name);
                    }
                    else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                    {
                        EditorPrefs.SetString("AmbientSkiesProfile_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_gradientSkyProfiles[m_selectedGradientSkyboxProfileIndex].name);
                    }
                }
                else
                {
                    if (m_profiles.m_showDebug)
                    {
                        Debug.Log("Skybox Profile Empty");
                    }

                    m_selectedSkyboxProfile = null;
                }

#if UNITY_POST_PROCESSING_STACK_V2
                PostProcessVolume processVol = FindObjectOfType<PostProcessVolume>();
                if (processVol != null)
                {
                    m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromProfileName(m_profiles, "User");
                    if (m_selectedPostProcessingProfileIndex >= 0)
                    {
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                        if (m_profiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && GameObject.Find("Global Post Processing") == null)
                        {
                            m_profiles.m_usePostFX = false;
                        }
#if !UNITY_2019_1_OR_NEWER
                        if (m_profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && GameObject.Find("Global Post Processing") == null)
                        {
                            m_profiles.m_usePostFX = false;
                        }
#else
                        if (m_profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && GameObject.Find("Post Processing HDRP Volume") == null)
                        {
                                m_profiles.m_usePostFX = false;
                        }
#endif
                        if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);
                        }
                        EditorPrefs.SetString("AmbientSkiesPostProcessing_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex].name);
                    }
                    else
                    {
                        if (m_profiles.m_showDebug)
                        {
                            Debug.Log("Post Processing Profile Empty");
                        }

                        m_selectedPostProcessingProfile = null;
                    }
                }
                else
                {
                    m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromProfileName(m_profiles, "Alpine");
                    if (m_selectedPostProcessingProfileIndex >= 0)
                    {
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                        if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);
                        }
                        EditorPrefs.SetString("AmbientSkiesPostProcessing_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_ppProfiles[newPPSelection].name);
                    }
                }
#endif

                m_selectedLightingProfileIndex = LightingUtils.GetProfileIndexFromProfileName(m_profiles, "Default Quality Lighting");
                if (m_selectedLightingProfileIndex >= 0)
                {
                    m_selectedLightingProfile = m_profiles.m_lightingProfiles[m_selectedLightingProfileIndex];
                    if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        //Udpate lightmap settings                   
                        LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);
                    }
                    EditorPrefs.SetString("AmbientSkiesLighting_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_lightingProfiles[newLightmappingSettings].name);
                }
                else
                {
                    m_selectedLightingProfile = null;
                }
            }
        }

        public void LoadProfiles()
        {
            //Get Current Saved
            m_selectedSkyboxProfileIndex = AmbientSkiesEditorFunctions.CheckSkybox(m_selectedSkyboxProfileIndex, m_profiles);
            if (m_selectedSkyboxProfileIndex >= 0)
            {
                m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_creationToolSettings.m_selectedHDRI];
                newSkyboxSelection = m_creationToolSettings.m_selectedHDRI;
            }
            else
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("HDRI Skybox Profile Empty");
                }

                m_selectedSkyboxProfile = null;
            }

            if (m_selectedProceduralSkyboxProfileIndex >= 0)
            {
                m_selectedProceduralSkyboxProfile = m_profiles.m_proceduralSkyProfiles[m_creationToolSettings.m_selectedProcedural];
                newProceduralSkyboxSelection = m_creationToolSettings.m_selectedProcedural;
            }
            else
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("Procedural Skybox Profile Empty");
                }

                m_selectedProceduralSkyboxProfile = null;
            }

            if (m_selectedGradientSkyboxProfileIndex >= 0)
            {
                m_selectedGradientSkyboxProfile = m_profiles.m_gradientSkyProfiles[m_creationToolSettings.m_selectedGradient];
                newGradientSkyboxSelection = m_creationToolSettings.m_selectedGradient;
            }
            else
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("Gradient Skybox Profile Empty");
                }

                m_selectedGradientSkyboxProfile = null;
            }

            if (m_creationToolSettings.m_selectedPostProcessing < m_profiles.m_ppProfiles.Count)
            {
                m_creationToolSettings.m_selectedPostProcessing = 0;
            }

            if (m_creationToolSettings.m_selectedPostProcessing >= 0)
            {
                m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_creationToolSettings.m_selectedPostProcessing];
                newPPSelection = m_creationToolSettings.m_selectedPostProcessing;
            }
            else
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("Post Processing Profile Empty");
                }

                m_selectedPostProcessingProfile = null;
            }

            if (m_selectedLightingProfileIndex >= 0)
            {
                m_selectedLightingProfile = m_profiles.m_lightingProfiles[m_selectedLightingProfileIndex];
                newLightmappingSettings = m_creationToolSettings.m_selectedLighting;
            }
            else
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("Lightmap Profile Profile Empty");
                }

                m_selectedLightingProfile = null;
            }
        }

        #endregion

        #region System Method

        /// <summary>
        /// Updates the multi scene loading
        /// </summary>
        private void UpdateMultiScene(AmbientSkyProfiles profile, bool ApplySettingsAlso = true)
        {
            if (profile == null)
            {
                return;
            }
            if (profile.m_showDebug)
            {
                Debug.Log("New Scene Has Been Loaded. Getting settings");
            }

            m_activeSceneName = EditorSceneManager.GetActiveScene().name;
            m_multiScenePresent = AmbientSkiesEditorFunctions.MultiSceneSupport(profile.m_multiSceneSupport, "Ambient Skies Global", false, profile, m_creationToolSettings);
            m_selectedSkyboxProfileIndex = m_creationToolSettings.m_selectedHDRI;
            m_selectedProceduralSkyboxProfileIndex = m_creationToolSettings.m_selectedProcedural;
            m_selectedGradientSkyboxProfileIndex = m_creationToolSettings.m_selectedGradient;
            m_selectedPostProcessingProfileIndex = m_creationToolSettings.m_selectedPostProcessing;
            if (ApplySettingsAlso)
            {
                if (profile.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty || profile.m_systemTypes != AmbientSkiesConsts.SystemTypes.DefaultProcedural)
                {
                    AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(profile);
                    //Update skybox
                    SkyboxUtils.SetFromProfileIndex(profile, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, profile.m_updateVisualEnvironment, profile.m_updateFog, profile.m_updateShadows, profile.m_updateAmbientLight, profile.m_updateScreenSpaceReflections, profile.m_updateScreenSpaceRefractions, profile.m_updateSun);
                    //Update post processing
                    PostProcessingUtils.SetFromProfileIndex(profile, m_selectedPostProcessingProfileIndex, false, profile.m_updateAO, profile.m_updateAutoExposure, profile.m_updateBloom, profile.m_updateChromatic, profile.m_updateColorGrading, profile.m_updateDOF, profile.m_updateGrain, profile.m_updateLensDistortion, profile.m_updateMotionBlur, profile.m_updateSSR, profile.m_updateVignette, profile.m_updatePanini, profile.m_updateTargetPlaform);
                    //Update lightmaps
                    LightingUtils.SetFromProfileIndex(profile, m_selectedLightingProfileIndex, false, profile.m_updateRealtime, profile.m_updateBaked);
                }
            }
        }

        /// <summary>
        /// Updates the int values in the creation tools profile
        /// </summary>
        /// <param name="hasChanged"></param>
        public void ApplyCreationToolSettings(bool hasChanged)
        {
            if (hasChanged)
            {
                //Apply index to profile in creation settings
                if (m_creationToolSettings != null)
                {
                    EditorUtility.SetDirty(m_creationToolSettings);
                    m_creationToolSettings.m_selectedHDRI = m_selectedSkyboxProfileIndex;
                    m_creationToolSettings.m_selectedProcedural = m_selectedProceduralSkyboxProfileIndex;
                    m_creationToolSettings.m_selectedGradient = m_selectedGradientSkyboxProfileIndex;
                    m_creationToolSettings.m_selectedPostProcessing = m_selectedPostProcessingProfileIndex;
                    m_creationToolSettings.m_selectedLighting = m_selectedLightingProfileIndex;
                }
            }
        }

        /// <summary>
        /// Enables and applies post processing
        /// </summary>
        public void FixPostProcessingGlobalPanel()
        {
            usePostProcess = true;
            m_profiles.m_usePostFX = true;

            AmbientSkiesEditorFunctions.SetAllPostFxUpdateToTrue(m_profiles);

            //Update post processing
            PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);

            autoPostProcessingApplyTimer = 2f;
            m_inspectorUpdate = true;

            AmbientSkiesEditorFunctions.SetAllPostFxUpdateToFalse(m_profiles);
        }

        /// <summary>
        /// Loads all variables in ambient skies
        /// </summary>
        public void LoadAllVariables()
        {
            #region Sky tab variables

            //Global Settings
            systemtype = m_profiles.m_systemTypes;

            //Target Platform
            targetPlatform = m_profiles.m_targetPlatform;

            //Fog config
            configurationType = m_profiles.m_configurationType;

            //Check post processing
            usePostProcess = m_profiles.m_usePostFX;

            autoMatchProfile = m_profiles.m_autoMatchProfile;

            //Skybox Settings
            newSkyboxSelection = m_selectedSkyboxProfileIndex;
            newProceduralSkyboxSelection = m_selectedProceduralSkyboxProfileIndex;
            newGradientSkyboxSelection = m_selectedGradientSkyboxProfileIndex;

            useSkies = m_profiles.m_useSkies;

            m_enableEditMode = m_profiles.m_editSettings;

            configurationType = m_profiles.m_configurationType;

            //Main Settings
            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                //Set Profile
                m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];

                if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                {
                    m_skiesProfileName = m_selectedSkyboxProfile.name;
                    m_hdriAssetName = m_selectedSkyboxProfile.creationHDRIAsset;
#if UNITY_POST_PROCESSING_STACK_V2
                    m_creationMatchPostProcessing = m_selectedSkyboxProfile.creationMatchPostProcessing;
                    if (m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile != null)
                    {
                        postProcessingAssetName = m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile.name;
                    }
#endif
                }

                //Fog mode
                fogType = m_selectedSkyboxProfile.fogType;

                //Ambient mode
                ambientMode = m_selectedSkyboxProfile.ambientMode;

                //VSync Settings
                vSyncMode = m_profiles.m_vSyncMode;

                //Skybox
                skyboxTint = m_selectedSkyboxProfile.skyboxTint;
                skyboxExposure = m_selectedSkyboxProfile.skyboxExposure;
                skyboxRotation = m_selectedSkyboxProfile.skyboxRotation;
                skyMultiplier = m_selectedSkyboxProfile.skyMultiplier;
                skyboxPitch = m_selectedSkyboxProfile.skyboxPitch;
                customSkybox = m_selectedSkyboxProfile.customSkybox;
                isProceduralSkybox = m_selectedSkyboxProfile.isProceduralSkybox;

                if (!m_selectedSkyboxProfile.isPWProfile)
                {
                    skyboxTint = m_selectedSkyboxProfile.customSkyboxTint;
                    skyboxExposure = m_selectedSkyboxProfile.customSkyboxExposure;
                    skyboxRotation = m_selectedSkyboxProfile.customSkyboxRotation;
                    skyboxPitch = m_selectedSkyboxProfile.customSkyboxPitch;
                    proceduralSunSize = m_selectedSkyboxProfile.customProceduralSunSize;
                    proceduralSunSizeConvergence = m_selectedSkyboxProfile.customProceduralSunSizeConvergence;
                    proceduralAtmosphereThickness = m_selectedSkyboxProfile.customProceduralAtmosphereThickness;
                    proceduralGroundColor = m_selectedSkyboxProfile.customProceduralGroundColor;
                    enableSunDisk = m_selectedSkyboxProfile.enableSunDisk;
                }

                //Fog Settings
                fogColor = m_selectedSkyboxProfile.fogColor;
                fogDistance = m_selectedSkyboxProfile.fogDistance;
                nearFogDistance = m_selectedSkyboxProfile.nearFogDistance;
                fogDensity = m_selectedSkyboxProfile.fogDensity;

                //Ambient Settings
                skyColor = m_selectedSkyboxProfile.skyColor;
                equatorColor = m_selectedSkyboxProfile.equatorColor;
                groundColor = m_selectedSkyboxProfile.groundColor;
                skyboxGroundIntensity = m_selectedSkyboxProfile.skyboxGroundIntensity;

                //Sun Settings
                shadowStrength = m_selectedSkyboxProfile.shadowStrength;
                indirectLightMultiplier = m_selectedSkyboxProfile.indirectLightMultiplier;
                sunColor = m_selectedSkyboxProfile.sunColor;
                sunIntensity = m_selectedSkyboxProfile.sunIntensity;
                sunLWRPIntensity = m_selectedSkyboxProfile.LWRPSunIntensity;
                sunHDRPIntensity = m_selectedSkyboxProfile.HDRPSunIntensity;
                useTemperature = m_selectedSkyboxProfile.useTempature;
                currentTemperature = m_selectedSkyboxProfile.temperature;

                //Shadow Settings
                shadowDistance = m_selectedSkyboxProfile.shadowDistance;
                shadowmaskMode = m_selectedSkyboxProfile.shadowmaskMode;
                shadowType = m_selectedSkyboxProfile.shadowType;
                shadowResolution = m_selectedSkyboxProfile.shadowResolution;
                shadowProjection = m_selectedSkyboxProfile.shadowProjection;
                shadowCascade = m_selectedSkyboxProfile.cascadeCount;

                //Horizon Settings
                scaleHorizonObjectWithFog = m_selectedSkyboxProfile.scaleHorizonObjectWithFog;
                horizonEnabled = m_selectedSkyboxProfile.horizonSkyEnabled;
                horizonScattering = m_selectedSkyboxProfile.horizonScattering;
                horizonFogDensity = m_selectedSkyboxProfile.horizonFogDensity;
                horizonFalloff = m_selectedSkyboxProfile.horizonFalloff;
                horizonBlend = m_selectedSkyboxProfile.horizonBlend;
                horizonScale = m_selectedSkyboxProfile.horizonSize;
                followPlayer = m_selectedSkyboxProfile.followPlayer;
                horizonUpdateTime = m_selectedSkyboxProfile.horizonUpdateTime;
                horizonPosition = m_selectedSkyboxProfile.horizonPosition;

                //HD Pipeline Settings
#if HDPipeline
                //Fog Mode
#if UNITY_2018_3_OR_NEWER
                fogColorMode = m_selectedSkyboxProfile.fogColorMode;
#endif

                //Volumetric Fog
                useDistanceFog = m_selectedSkyboxProfile.volumetricEnableDistanceFog;
                baseFogDistance = m_selectedSkyboxProfile.volumetricBaseFogDistance;
                baseFogHeight = m_selectedSkyboxProfile.volumetricBaseFogHeight;
                meanFogHeight = m_selectedSkyboxProfile.volumetricMeanHeight;
                globalAnisotropy = m_selectedSkyboxProfile.volumetricGlobalAnisotropy;
                globalLightProbeDimmer = m_selectedSkyboxProfile.volumetricGlobalLightProbeDimmer;

                //Exponential Fog
                exponentialFogDensity = m_selectedSkyboxProfile.exponentialFogDensity;
                exponentialBaseFogHeight = m_selectedSkyboxProfile.exponentialBaseHeight;
                exponentialHeightAttenuation = m_selectedSkyboxProfile.exponentialHeightAttenuation;
                exponentialMaxFogDistance = m_selectedSkyboxProfile.exponentialMaxFogDistance;
                exponentialMipFogNear = m_selectedSkyboxProfile.exponentialMipFogNear;
                exponentialMipFogFar = m_selectedSkyboxProfile.exponentialMipFogFar;
                exponentialMipFogMax = m_selectedSkyboxProfile.exponentialMipFogMaxMip;

                //Linear Fog
                linearFogDensity = m_selectedSkyboxProfile.linearFogDensity;
                linearFogHeightStart = m_selectedSkyboxProfile.linearHeightStart;
                linearFogHeightEnd = m_selectedSkyboxProfile.linearHeightEnd;
                linearFogMaxDistance = m_selectedSkyboxProfile.linearMaxFogDistance;
                linearMipFogNear = m_selectedSkyboxProfile.linearMipFogNear;
                linearMipFogFar = m_selectedSkyboxProfile.linearMipFogFar;
                linearMipFogMax = m_selectedSkyboxProfile.linearMipFogMaxMip;

                //Volumetric Light Controller
                depthExtent = m_selectedSkyboxProfile.volumetricDistanceRange;
                sliceDistribution = m_selectedSkyboxProfile.volumetricSliceDistributionUniformity;

                //Density Fog Volume
                useDensityFogVolume = m_selectedSkyboxProfile.useFogDensityVolume;
                singleScatteringAlbedo = m_selectedSkyboxProfile.singleScatteringAlbedo;
                densityVolumeFogDistance = m_selectedSkyboxProfile.densityVolumeFogDistance;
                fogDensityMaskTexture = m_selectedSkyboxProfile.fogDensityMaskTexture;
                densityMaskTiling = m_selectedSkyboxProfile.densityMaskTiling;
                densityScale = m_selectedSkyboxProfile.densityScale;

                //HD Shadows
                hDShadowQuality = m_selectedSkyboxProfile.shadowQuality;
                split1 = m_selectedSkyboxProfile.cascadeSplit1;
                split2 = m_selectedSkyboxProfile.cascadeSplit2;
                split3 = m_selectedSkyboxProfile.cascadeSplit3;

                //Contact Shadows
                enableContactShadows = m_selectedSkyboxProfile.useContactShadows;
                contactLength = m_selectedSkyboxProfile.contactShadowsLength;
                contactScaleFactor = m_selectedSkyboxProfile.contactShadowsDistanceScaleFactor;
                contactMaxDistance = m_selectedSkyboxProfile.contactShadowsMaxDistance;
                contactFadeDistance = m_selectedSkyboxProfile.contactShadowsFadeDistance;
                contactSampleCount = m_selectedSkyboxProfile.contactShadowsSampleCount;
                contactOpacity = m_selectedSkyboxProfile.contactShadowsOpacity;

                //Micro Shadows
#if UNITY_2018_3_OR_NEWER
                enableMicroShadows = m_selectedSkyboxProfile.useMicroShadowing;
                microShadowOpacity = m_selectedSkyboxProfile.microShadowOpacity;
#endif

                //SS Reflection
                enableSSReflection = m_selectedSkyboxProfile.enableScreenSpaceReflections;
                ssrEdgeFade = m_selectedSkyboxProfile.screenEdgeFadeDistance;
                ssrNumberOfRays = m_selectedSkyboxProfile.maxNumberOfRaySteps;
                ssrObjectThickness = m_selectedSkyboxProfile.objectThickness;
                ssrMinSmoothness = m_selectedSkyboxProfile.minSmoothness;
                ssrSmoothnessFade = m_selectedSkyboxProfile.smoothnessFadeStart;
                ssrReflectSky = m_selectedSkyboxProfile.reflectSky;

                //SS Refract
                enableSSRefraction = m_selectedSkyboxProfile.enableScreenSpaceRefractions;
                ssrWeightDistance = m_selectedSkyboxProfile.screenWeightDistance;

                //Ambient Lighting
                useStaticLighting = m_selectedSkyboxProfile.useBakingSky;
                diffuseAmbientIntensity = m_selectedSkyboxProfile.indirectDiffuseIntensity;
                specularAmbientIntensity = m_selectedSkyboxProfile.indirectSpecularIntensity;
#endif
            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                //Set Profile
                m_selectedProceduralSkyboxProfile = m_profiles.m_proceduralSkyProfiles[m_selectedProceduralSkyboxProfileIndex];

                if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                {
                    m_skiesProfileName = m_selectedProceduralSkyboxProfile.name;
#if UNITY_POST_PROCESSING_STACK_V2
                    m_creationMatchPostProcessing = m_selectedProceduralSkyboxProfile.creationMatchPostProcessing;
                    if (m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile != null)
                    {
                        postProcessingAssetName = m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile.name;
                    }
#endif
                }

                //Fog mode
                fogType = m_selectedProceduralSkyboxProfile.fogType;

                //Ambient mode
                ambientMode = m_selectedProceduralSkyboxProfile.ambientMode;

                //Time of day
                #region Time Of Day Checks

                //Time Of Day Settings

                useTimeOfDay = m_profiles.m_useTimeOfDay;

                bool checkAllTODSettings = true;
                if (m_profiles.m_timeOfDayProfile != timeOfDayProfile)
                {
                    timeOfDayProfile = m_profiles.m_timeOfDayProfile;
                    m_profiles.m_timeOfDayProfile = timeOfDayProfile;

                    checkAllTODSettings = false;

                    /*
                    //Get all settings from new profile
                    currentSeason = m_profiles.timeOfDayProfile.m_environmentSeason;
                    hemisphereOrigin = m_profiles.timeOfDayProfile.m_hemisphereOrigin;
                    pauseTimeKey = m_profiles.timeOfDayProfile.m_pauseTimeKey;
                    incrementUpKey = m_profiles.timeOfDayProfile.m_incrementUpKey;
                    incrementDownKey = m_profiles.timeOfDayProfile.m_incrementDownKey;
                    timeToAddOrRemove = m_profiles.timeOfDayProfile.m_timeToAddOrRemove;
                    rotateSunLeftKey = m_profiles.timeOfDayProfile.m_rotateSunLeftKey;
                    rotateSunRightKey = m_profiles.timeOfDayProfile.m_rotateSunRightKey;
                    sunRotationAmount = m_profiles.timeOfDayProfile.m_sunRotationAmount;
                    pauseTime = m_profiles.timeOfDayProfile.m_pauseTime;
                    currentTimeOfDay = m_profiles.timeOfDayProfile.m_currentTime;
                    timeOfDaySkyboxRotation = m_profiles.timeOfDayProfile.m_sunRotation;
                    daySunIntensity = m_profiles.timeOfDayProfile.m_daySunIntensity;
                    daySunGradientColor = m_profiles.timeOfDayProfile.m_daySunGradientColor;
                    nightSunIntensity = m_profiles.timeOfDayProfile.m_nightSunIntensity;
                    nightSunGradientColor = m_profiles.timeOfDayProfile.m_nightSunGradientColor;
                    lightAnisotropy = m_profiles.timeOfDayProfile.m_lightAnisotropy;
                    lightProbeDimmer = m_profiles.timeOfDayProfile.m_lightProbeDimmer;
                    lightDepthExtent = m_profiles.timeOfDayProfile.m_lightDepthExtent;
                    sunSizeAmount = m_profiles.timeOfDayProfile.m_sunSize;
                    skyExposureAmount = m_profiles.timeOfDayProfile.m_skyExposure;
                    startFogDistance = m_profiles.timeOfDayProfile.m_startFogDistance;
                    dayFogDensity = m_profiles.timeOfDayProfile.m_dayFogDensity;
                    nightFogDensity = m_profiles.timeOfDayProfile.m_nightFogDensity;
                    dayFogDistance = m_profiles.timeOfDayProfile.m_dayFogDistance;
                    dayFogColor = m_profiles.timeOfDayProfile.m_dayFogColor;
                    nightFogDistance = m_profiles.timeOfDayProfile.m_nightFogDistance;
                    nightFogColor = m_profiles.timeOfDayProfile.m_nightFogColor;
                    dayTempature = m_profiles.timeOfDayProfile.m_dayTempature;
                    dayPostFXColor = m_profiles.timeOfDayProfile.m_dayColor;
                    nightTempature = m_profiles.timeOfDayProfile.m_nightTempature;
                    nightPostFXColor = m_profiles.timeOfDayProfile.m_nightColor;
                    syncPostProcessing = m_profiles.timeOfDayProfile.m_syncPostFXToTimeOfDay;
                    realtimeGIUpdate = m_profiles.timeOfDayProfile.m_realtimeGIUpdate;
                    gIUpdateInterval = m_profiles.timeOfDayProfile.m_gIUpdateIntervalInSeconds;
                    dayLengthInSeconds = m_profiles.timeOfDayProfile.m_dayLengthInSeconds;
                    nightLengthInSeconds = m_profiles.timeOfDayProfile.m_nightLengthInSeconds;
                    dayDate = m_profiles.timeOfDayProfile.m_day;
                    monthDate = m_profiles.timeOfDayProfile.m_month;
                    yearDate = m_profiles.timeOfDayProfile.m_year;
                    */
                }
                else
                {
                    timeOfDayProfile = m_profiles.m_timeOfDayProfile;
                }

                if (checkAllTODSettings)
                {
                    realtimeEmission = m_profiles.m_realtimeEmission;
                    syncRealtimeEmissionToTimeOfDay = m_profiles.m_syncRealtimeEmissionToTimeOfDay;

                    if (m_profiles.m_timeOfDayProfile != null)
                    {
                        if (currentSeason != m_profiles.m_timeOfDayProfile.m_environmentSeason)
                        {
                            currentSeason = m_profiles.m_timeOfDayProfile.m_environmentSeason;
                            m_profiles.m_currentSeason = m_profiles.m_timeOfDayProfile.m_environmentSeason;
                        }
                        else
                        {
                            currentSeason = m_profiles.m_currentSeason;
                        }
                        if (hemisphereOrigin != m_profiles.m_timeOfDayProfile.m_hemisphereOrigin)
                        {
                            hemisphereOrigin = m_profiles.m_timeOfDayProfile.m_hemisphereOrigin;
                            m_profiles.m_hemisphereOrigin = m_profiles.m_timeOfDayProfile.m_hemisphereOrigin;
                        }
                        else
                        {
                            hemisphereOrigin = m_profiles.m_hemisphereOrigin;
                        }
                        if (pauseTimeKey != m_profiles.m_timeOfDayProfile.m_pauseTimeKey)
                        {
                            pauseTimeKey = m_profiles.m_timeOfDayProfile.m_pauseTimeKey;
                            m_profiles.m_pauseTimeKey = m_profiles.m_timeOfDayProfile.m_pauseTimeKey;
                        }
                        else
                        {
                            pauseTimeKey = m_profiles.m_pauseTimeKey;
                        }
                        if (incrementUpKey != m_profiles.m_timeOfDayProfile.m_incrementUpKey)
                        {
                            incrementUpKey = m_profiles.m_timeOfDayProfile.m_incrementUpKey;
                            m_profiles.m_incrementUpKey = m_profiles.m_timeOfDayProfile.m_incrementUpKey;
                        }
                        else
                        {
                            incrementUpKey = m_profiles.m_incrementUpKey;
                        }
                        if (incrementDownKey != m_profiles.m_timeOfDayProfile.m_incrementDownKey)
                        {
                            incrementDownKey = m_profiles.m_timeOfDayProfile.m_incrementDownKey;
                            m_profiles.m_incrementDownKey = m_profiles.m_timeOfDayProfile.m_incrementDownKey;
                        }
                        else
                        {
                            incrementDownKey = m_profiles.m_incrementDownKey;
                        }
                        if (timeToAddOrRemove != m_profiles.m_timeOfDayProfile.m_timeToAddOrRemove)
                        {
                            timeToAddOrRemove = m_profiles.m_timeOfDayProfile.m_timeToAddOrRemove;
                            m_profiles.m_timeToAddOrRemove = m_profiles.m_timeOfDayProfile.m_timeToAddOrRemove;
                        }
                        else
                        {
                            timeToAddOrRemove = m_profiles.m_timeToAddOrRemove;
                        }
                        if (rotateSunLeftKey != m_profiles.m_timeOfDayProfile.m_rotateSunLeftKey)
                        {
                            rotateSunLeftKey = m_profiles.m_timeOfDayProfile.m_rotateSunLeftKey;
                            m_profiles.m_rotateSunLeftKey = m_profiles.m_timeOfDayProfile.m_rotateSunLeftKey;
                        }
                        else
                        {
                            rotateSunLeftKey = m_profiles.m_rotateSunLeftKey;
                        }
                        if (rotateSunRightKey != m_profiles.m_timeOfDayProfile.m_rotateSunRightKey)
                        {
                            rotateSunRightKey = m_profiles.m_timeOfDayProfile.m_rotateSunRightKey;
                            m_profiles.m_rotateSunRightKey = m_profiles.m_timeOfDayProfile.m_rotateSunRightKey;
                        }
                        else
                        {
                            rotateSunRightKey = m_profiles.m_rotateSunRightKey;
                        }
                        if (sunRotationAmount != m_profiles.m_timeOfDayProfile.m_sunRotationAmount)
                        {
                            sunRotationAmount = m_profiles.m_timeOfDayProfile.m_sunRotationAmount;
                            m_profiles.m_sunRotationAmount = m_profiles.m_timeOfDayProfile.m_sunRotationAmount;
                        }
                        else
                        {
                            sunRotationAmount = m_profiles.m_sunRotationAmount;
                        }
                        if (pauseTime != m_profiles.m_timeOfDayProfile.m_pauseTime)
                        {
                            pauseTime = m_profiles.m_timeOfDayProfile.m_pauseTime;
                            m_profiles.m_pauseTime = m_profiles.m_timeOfDayProfile.m_pauseTime;
                        }
                        else
                        {
                            pauseTime = m_profiles.m_pauseTime;
                        }
                        if (currentTimeOfDay != m_profiles.m_timeOfDayProfile.m_currentTime)
                        {
                            currentTimeOfDay = m_profiles.m_timeOfDayProfile.m_currentTime;
                            m_profiles.m_currentTimeOfDay = m_profiles.m_timeOfDayProfile.m_currentTime;
                        }
                        else
                        {
                            currentTimeOfDay = m_profiles.m_currentTimeOfDay;
                        }
                        if (timeOfDaySkyboxRotation != m_profiles.m_timeOfDayProfile.m_sunRotation)
                        {
                            timeOfDaySkyboxRotation = m_profiles.m_timeOfDayProfile.m_sunRotation;
                            m_profiles.m_sunRotationAmount = m_profiles.m_timeOfDayProfile.m_sunRotation;
                        }
                        else
                        {
                            timeOfDaySkyboxRotation = m_profiles.m_skyboxRotation;
                        }
                        if (daySunIntensity != m_profiles.m_timeOfDayProfile.m_daySunIntensity)
                        {
                            daySunIntensity = m_profiles.m_timeOfDayProfile.m_daySunIntensity;
                            m_profiles.m_daySunIntensity = m_profiles.m_timeOfDayProfile.m_daySunIntensity;
                        }
                        else
                        {
                            daySunIntensity = m_profiles.m_daySunIntensity;
                        }
                        if (daySunGradientColor != m_profiles.m_timeOfDayProfile.m_daySunGradientColor)
                        {
                            daySunGradientColor = m_profiles.m_timeOfDayProfile.m_daySunGradientColor;
                            m_profiles.m_daySunGradientColor = m_profiles.m_timeOfDayProfile.m_daySunGradientColor;
                        }
                        else
                        {
                            daySunGradientColor = m_profiles.m_daySunGradientColor;
                        }
                        if (nightSunIntensity != m_profiles.m_timeOfDayProfile.m_nightSunIntensity)
                        {
                            nightSunIntensity = m_profiles.m_timeOfDayProfile.m_nightSunIntensity;
                            m_profiles.m_nightSunIntensity = m_profiles.m_timeOfDayProfile.m_nightSunIntensity;
                        }
                        else
                        {
                            nightSunIntensity = m_profiles.m_nightSunIntensity;
                        }
                        if (nightSunGradientColor != m_profiles.m_timeOfDayProfile.m_nightSunGradientColor)
                        {
                            nightSunGradientColor = m_profiles.m_timeOfDayProfile.m_nightSunGradientColor;
                            m_profiles.m_nightSunGradientColor = m_profiles.m_timeOfDayProfile.m_nightSunGradientColor;
                        }
                        else
                        {
                            nightSunGradientColor = m_profiles.m_nightSunGradientColor;
                        }
                        if (lightAnisotropy != m_profiles.m_timeOfDayProfile.m_lightAnisotropy)
                        {
                            lightAnisotropy = m_profiles.m_timeOfDayProfile.m_lightAnisotropy;
                            m_profiles.m_lightAnisotropy = m_profiles.m_timeOfDayProfile.m_lightAnisotropy;
                        }
                        else
                        {
                            lightAnisotropy = m_profiles.m_lightAnisotropy;
                        }
                        if (lightProbeDimmer != m_profiles.m_timeOfDayProfile.m_lightProbeDimmer)
                        {
                            lightProbeDimmer = m_profiles.m_timeOfDayProfile.m_lightProbeDimmer;
                            m_profiles.m_lightProbeDimmer = m_profiles.m_timeOfDayProfile.m_lightProbeDimmer;
                        }
                        else
                        {
                            lightProbeDimmer = m_profiles.m_lightProbeDimmer;
                        }
                        if (lightDepthExtent != m_profiles.m_timeOfDayProfile.m_lightDepthExtent)
                        {
                            lightDepthExtent = m_profiles.m_timeOfDayProfile.m_lightDepthExtent;
                            m_profiles.m_lightDepthExtent = m_profiles.m_timeOfDayProfile.m_lightDepthExtent;
                        }
                        else
                        {
                            lightDepthExtent = m_profiles.m_lightDepthExtent;
                        }
                        if (sunSizeAmount != m_profiles.m_timeOfDayProfile.m_sunSize)
                        {
                            sunSizeAmount = m_profiles.m_timeOfDayProfile.m_sunSize;
                            m_profiles.m_sunSize = m_profiles.m_timeOfDayProfile.m_sunSize;
                        }
                        else
                        {
                            sunSizeAmount = m_profiles.m_sunSize;
                        }
                        if (skyExposureAmount != m_profiles.m_timeOfDayProfile.m_skyExposure)
                        {
                            skyExposureAmount = m_profiles.m_timeOfDayProfile.m_skyExposure;
                            m_profiles.m_skyExposure = m_profiles.m_timeOfDayProfile.m_skyExposure;
                        }
                        else
                        {
                            skyExposureAmount = m_profiles.m_skyExposure;
                        }
                        if (startFogDistance != m_profiles.m_timeOfDayProfile.m_startFogDistance)
                        {
                            startFogDistance = m_profiles.m_timeOfDayProfile.m_startFogDistance;
                            m_profiles.m_startFogDistance = m_profiles.m_timeOfDayProfile.m_startFogDistance;
                        }
                        else
                        {
                            startFogDistance = m_profiles.m_startFogDistance;
                        }
                        if (dayFogDensity != m_profiles.m_timeOfDayProfile.m_dayFogDensity)
                        {
                            dayFogDensity = m_profiles.m_timeOfDayProfile.m_dayFogDensity;
                            m_profiles.m_dayFogDensity = m_profiles.m_timeOfDayProfile.m_dayFogDensity;
                        }
                        else
                        {
                            dayFogDensity = m_profiles.m_dayFogDensity;
                        }
                        if (nightFogDensity != m_profiles.m_timeOfDayProfile.m_nightFogDensity)
                        {
                            nightFogDensity = m_profiles.m_timeOfDayProfile.m_nightFogDensity;
                            m_profiles.m_nightFogDensity = m_profiles.m_timeOfDayProfile.m_nightFogDensity;
                        }
                        else
                        {
                            nightFogDensity = m_profiles.m_nightFogDensity;
                        }
                        if (dayFogDistance != m_profiles.m_timeOfDayProfile.m_dayFogDistance)
                        {
                            dayFogDistance = m_profiles.m_timeOfDayProfile.m_dayFogDistance;
                            m_profiles.m_dayFogDistance = m_profiles.m_timeOfDayProfile.m_dayFogDistance;
                        }
                        else
                        {
                            dayFogDistance = m_profiles.m_dayFogDistance;
                        }
                        if (dayFogColor != m_profiles.m_timeOfDayProfile.m_dayFogColor)
                        {
                            dayFogColor = m_profiles.m_timeOfDayProfile.m_dayFogColor;
                            m_profiles.m_dayFogColor = m_profiles.m_timeOfDayProfile.m_dayFogColor;
                        }
                        else
                        {
                            dayFogColor = m_profiles.m_dayFogColor;
                        }
                        if (nightFogDistance != m_profiles.m_timeOfDayProfile.m_nightFogDistance)
                        {
                            nightFogDistance = m_profiles.m_timeOfDayProfile.m_nightFogDistance;
                            m_profiles.m_nightFogDistance = m_profiles.m_timeOfDayProfile.m_nightFogDistance;
                        }
                        else
                        {
                            nightFogDistance = m_profiles.m_nightFogDistance;
                        }
                        if (nightFogColor != m_profiles.m_timeOfDayProfile.m_nightFogColor)
                        {
                            nightFogColor = m_profiles.m_timeOfDayProfile.m_nightFogColor;
                            m_profiles.m_nightFogColor = m_profiles.m_timeOfDayProfile.m_nightFogColor;
                        }
                        else
                        {
                            nightFogColor = m_profiles.m_nightFogColor;
                        }
                        if (dayTempature != m_profiles.m_timeOfDayProfile.m_dayTempature)
                        {
                            dayTempature = m_profiles.m_timeOfDayProfile.m_dayTempature;
                            m_profiles.m_dayTempature = m_profiles.m_timeOfDayProfile.m_dayTempature;
                        }
                        else
                        {
                            dayTempature = m_profiles.m_dayTempature;
                        }
                        if (dayPostFXColor != m_profiles.m_timeOfDayProfile.m_dayColor)
                        {
                            dayPostFXColor = m_profiles.m_timeOfDayProfile.m_dayColor;
                            m_profiles.m_dayPostFXColor = m_profiles.m_timeOfDayProfile.m_dayColor;
                        }
                        else
                        {
                            dayPostFXColor = m_profiles.m_dayPostFXColor;
                        }
                        if (nightTempature != m_profiles.m_timeOfDayProfile.m_nightTempature)
                        {
                            nightTempature = m_profiles.m_timeOfDayProfile.m_nightTempature;
                            m_profiles.m_nightTempature = m_profiles.m_timeOfDayProfile.m_nightTempature;
                        }
                        else
                        {
                            nightTempature = m_profiles.m_nightTempature;
                        }
                        if (nightPostFXColor != m_profiles.m_timeOfDayProfile.m_nightColor)
                        {
                            nightPostFXColor = m_profiles.m_timeOfDayProfile.m_nightColor;
                            m_profiles.m_nightPostFXColor = m_profiles.m_timeOfDayProfile.m_nightColor;
                        }
                        else
                        {
                            nightPostFXColor = m_profiles.m_nightPostFXColor;
                        }
                        if (syncPostProcessing != m_profiles.m_timeOfDayProfile.m_syncPostFXToTimeOfDay)
                        {
                            syncPostProcessing = m_profiles.m_timeOfDayProfile.m_syncPostFXToTimeOfDay;
                            m_profiles.m_syncPostProcessing = m_profiles.m_timeOfDayProfile.m_syncPostFXToTimeOfDay;
                        }
                        else
                        {
                            syncPostProcessing = m_profiles.m_syncPostProcessing;
                        }
                        if (realtimeGIUpdate != m_profiles.m_timeOfDayProfile.m_realtimeGIUpdate)
                        {
                            realtimeGIUpdate = m_profiles.m_timeOfDayProfile.m_realtimeGIUpdate;
                            m_profiles.m_realtimeGIUpdate = m_profiles.m_timeOfDayProfile.m_realtimeGIUpdate;
                        }
                        else
                        {
                            realtimeGIUpdate = m_profiles.m_realtimeGIUpdate;
                        }
                        if (gIUpdateInterval != m_profiles.m_timeOfDayProfile.m_gIUpdateIntervalInSeconds)
                        {
                            gIUpdateInterval = m_profiles.m_timeOfDayProfile.m_gIUpdateIntervalInSeconds;
                            m_profiles.m_gIUpdateInterval = m_profiles.m_timeOfDayProfile.m_gIUpdateIntervalInSeconds;
                        }
                        else
                        {
                            gIUpdateInterval = m_profiles.m_gIUpdateInterval;
                        }
                        if (dayLengthInSeconds != m_profiles.m_timeOfDayProfile.m_dayLengthInSeconds)
                        {
                            dayLengthInSeconds = m_profiles.m_timeOfDayProfile.m_dayLengthInSeconds;
                            m_profiles.m_dayLengthInSeconds = m_profiles.m_timeOfDayProfile.m_dayLengthInSeconds;
                        }
                        else
                        {
                            dayLengthInSeconds = m_profiles.m_dayLengthInSeconds;
                        }
                        if (nightLengthInSeconds != m_profiles.m_timeOfDayProfile.m_nightLengthInSeconds)
                        {
                            nightLengthInSeconds = m_profiles.m_timeOfDayProfile.m_nightLengthInSeconds;
                            m_profiles.m_nightLengthInSeconds = m_profiles.m_timeOfDayProfile.m_nightLengthInSeconds;
                        }
                        else
                        {
                            nightLengthInSeconds = m_profiles.m_nightLengthInSeconds;
                        }
                        if (dayDate != m_profiles.m_timeOfDayProfile.m_day)
                        {
                            dayDate = m_profiles.m_timeOfDayProfile.m_day;
                            m_profiles.m_dayDate = m_profiles.m_timeOfDayProfile.m_day;
                        }
                        else
                        {
                            dayDate = m_profiles.m_dayDate;
                        }
                        if (monthDate != m_profiles.m_timeOfDayProfile.m_month)
                        {
                            monthDate = m_profiles.m_timeOfDayProfile.m_month;
                            m_profiles.m_monthDate = m_profiles.m_timeOfDayProfile.m_month;
                        }
                        else
                        {
                            monthDate = m_profiles.m_monthDate;
                        }
                        if (yearDate != m_profiles.m_timeOfDayProfile.m_year)
                        {
                            yearDate = m_profiles.m_timeOfDayProfile.m_year;
                            m_profiles.m_yearDate = m_profiles.m_timeOfDayProfile.m_year;
                        }
                        else
                        {
                            yearDate = m_profiles.m_yearDate;
                        }
                        if (pBSSpaceEmissionTexture != m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionTexture)
                        {
                            pBSSpaceEmissionTexture = m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionTexture;
                            m_profiles.m_pBSSpaceEmissionTexture = m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionTexture;
                        }
                        else
                        {
                            pBSSpaceEmissionTexture = m_profiles.m_pBSSpaceEmissionTexture;
                        }
                        if (pBSTODSpaceEmissionMultiplier != m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionMultiplier)
                        {
                            pBSTODSpaceEmissionMultiplier = m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionMultiplier;
                            m_profiles.m_pBSSpaceEmissionMultiplier = m_profiles.m_timeOfDayProfile.m_pBSSpaceEmissionMultiplier;
                        }
                        else
                        {
                            pBSTODSpaceEmissionMultiplier = m_profiles.m_pBSSpaceEmissionMultiplier;
                        }
                    }
                }

                #endregion

                //Skybox
                proceduralSunSize = m_selectedProceduralSkyboxProfile.proceduralSunSize;
                proceduralSunSizeConvergence = m_selectedProceduralSkyboxProfile.proceduralSunSizeConvergence;
                proceduralAtmosphereThickness = m_selectedProceduralSkyboxProfile.proceduralAtmosphereThickness;
                proceduralGroundColor = m_selectedProceduralSkyboxProfile.proceduralGroundColor;
                skyboxTint = m_selectedProceduralSkyboxProfile.proceduralSkyTint;
                skyboxExposure = m_selectedProceduralSkyboxProfile.proceduralSkyExposure;
                skyboxRotation = m_selectedProceduralSkyboxProfile.proceduralSkyboxRotation;
                skyboxPitch = m_selectedProceduralSkyboxProfile.proceduralSkyboxPitch;
                skyMultiplier = m_selectedProceduralSkyboxProfile.proceduralSkyMultiplier;
                includeSunInBaking = m_selectedProceduralSkyboxProfile.includeSunInBaking;
                enableSunDisk = m_selectedProceduralSkyboxProfile.enableSunDisk;

                //Fog Settings
                fogColor = m_selectedProceduralSkyboxProfile.proceduralFogColor;
                fogDistance = m_selectedProceduralSkyboxProfile.proceduralFogDistance;
                nearFogDistance = m_selectedProceduralSkyboxProfile.proceduralNearFogDistance;
                fogDensity = m_selectedProceduralSkyboxProfile.proceduralFogDensity;

                //Ambient Settings
                skyColor = m_selectedProceduralSkyboxProfile.skyColor;
                equatorColor = m_selectedProceduralSkyboxProfile.equatorColor;
                groundColor = m_selectedProceduralSkyboxProfile.groundColor;
                skyboxGroundIntensity = m_selectedProceduralSkyboxProfile.skyboxGroundIntensity;

                //Sun Settings
                shadowStrength = m_selectedProceduralSkyboxProfile.shadowStrength;
                indirectLightMultiplier = m_selectedProceduralSkyboxProfile.indirectLightMultiplier;
                sunColor = m_selectedProceduralSkyboxProfile.proceduralSunColor;
                sunIntensity = m_selectedProceduralSkyboxProfile.proceduralSunIntensity;
                sunLWRPIntensity = m_selectedProceduralSkyboxProfile.proceduralLWRPSunIntensity;
                sunHDRPIntensity = m_selectedProceduralSkyboxProfile.proceduralHDRPSunIntensity;

                //Shadow Settings
                shadowDistance = m_selectedProceduralSkyboxProfile.shadowDistance;
                shadowmaskMode = m_selectedProceduralSkyboxProfile.shadowmaskMode;
                shadowType = m_selectedProceduralSkyboxProfile.shadowType;
                shadowResolution = m_selectedProceduralSkyboxProfile.shadowResolution;
                shadowProjection = m_selectedProceduralSkyboxProfile.shadowProjection;
                shadowCascade = m_selectedProceduralSkyboxProfile.cascadeCount;

                //Horizon Settings
                scaleHorizonObjectWithFog = m_selectedProceduralSkyboxProfile.scaleHorizonObjectWithFog;
                horizonEnabled = m_selectedProceduralSkyboxProfile.horizonSkyEnabled;
                horizonScattering = m_selectedProceduralSkyboxProfile.horizonScattering;
                horizonFogDensity = m_selectedProceduralSkyboxProfile.horizonFogDensity;
                horizonFalloff = m_selectedProceduralSkyboxProfile.horizonFalloff;
                horizonBlend = m_selectedProceduralSkyboxProfile.horizonBlend;
                horizonScale = m_selectedProceduralSkyboxProfile.horizonSize;
                followPlayer = m_selectedProceduralSkyboxProfile.followPlayer;
                horizonUpdateTime = m_selectedProceduralSkyboxProfile.horizonUpdateTime;
                horizonPosition = m_selectedProceduralSkyboxProfile.horizonPosition;

                //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                //Fog Mode
                fogColorMode = m_selectedProceduralSkyboxProfile.fogColorMode;

                //Volumetric Fog
                useDistanceFog = m_selectedProceduralSkyboxProfile.volumetricEnableDistanceFog;
                baseFogDistance = m_selectedProceduralSkyboxProfile.volumetricBaseFogDistance;
                baseFogHeight = m_selectedProceduralSkyboxProfile.volumetricBaseFogHeight;
                meanFogHeight = m_selectedProceduralSkyboxProfile.volumetricMeanHeight;
                globalAnisotropy = m_selectedProceduralSkyboxProfile.volumetricGlobalAnisotropy;
                globalLightProbeDimmer = m_selectedProceduralSkyboxProfile.volumetricGlobalLightProbeDimmer;

                //Exponential Fog
                exponentialFogDensity = m_selectedProceduralSkyboxProfile.exponentialFogDensity;
                exponentialBaseFogHeight = m_selectedProceduralSkyboxProfile.exponentialBaseHeight;
                exponentialHeightAttenuation = m_selectedProceduralSkyboxProfile.exponentialHeightAttenuation;
                exponentialMaxFogDistance = m_selectedProceduralSkyboxProfile.exponentialMaxFogDistance;
                exponentialMipFogNear = m_selectedProceduralSkyboxProfile.exponentialMipFogNear;
                exponentialMipFogFar = m_selectedProceduralSkyboxProfile.exponentialMipFogFar;
                exponentialMipFogMax = m_selectedProceduralSkyboxProfile.exponentialMipFogMaxMip;

                //Linear Fog
                linearFogDensity = m_selectedProceduralSkyboxProfile.linearFogDensity;
                linearFogHeightStart = m_selectedProceduralSkyboxProfile.linearHeightStart;
                linearFogHeightEnd = m_selectedProceduralSkyboxProfile.linearHeightEnd;
                linearFogMaxDistance = m_selectedProceduralSkyboxProfile.linearMaxFogDistance;
                linearMipFogNear = m_selectedProceduralSkyboxProfile.linearMipFogNear;
                linearMipFogFar = m_selectedProceduralSkyboxProfile.linearMipFogFar;
                linearMipFogMax = m_selectedProceduralSkyboxProfile.linearMipFogMaxMip;

                //Volumetric Light Controller
                depthExtent = m_selectedProceduralSkyboxProfile.volumetricDistanceRange;
                sliceDistribution = m_selectedProceduralSkyboxProfile.volumetricSliceDistributionUniformity;

                //Density Fog Volume
                useDensityFogVolume = m_selectedProceduralSkyboxProfile.useFogDensityVolume;
                singleScatteringAlbedo = m_selectedProceduralSkyboxProfile.singleScatteringAlbedo;
                densityVolumeFogDistance = m_selectedProceduralSkyboxProfile.densityVolumeFogDistance;
                fogDensityMaskTexture = m_selectedProceduralSkyboxProfile.fogDensityMaskTexture;
                densityMaskTiling = m_selectedProceduralSkyboxProfile.densityMaskTiling;
                densityScale = m_selectedProceduralSkyboxProfile.densityScale;

                //HD Shadows
                hDShadowQuality = m_selectedProceduralSkyboxProfile.shadowQuality;
                split1 = m_selectedProceduralSkyboxProfile.cascadeSplit1;
                split2 = m_selectedProceduralSkyboxProfile.cascadeSplit2;
                split3 = m_selectedProceduralSkyboxProfile.cascadeSplit3;

                //Contact Shadows
                enableContactShadows = m_selectedProceduralSkyboxProfile.useContactShadows;
                contactLength = m_selectedProceduralSkyboxProfile.contactShadowsLength;
                contactScaleFactor = m_selectedProceduralSkyboxProfile.contactShadowsDistanceScaleFactor;
                contactMaxDistance = m_selectedProceduralSkyboxProfile.contactShadowsMaxDistance;
                contactFadeDistance = m_selectedProceduralSkyboxProfile.contactShadowsFadeDistance;
                contactSampleCount = m_selectedProceduralSkyboxProfile.contactShadowsSampleCount;
                contactOpacity = m_selectedProceduralSkyboxProfile.contactShadowsOpacity;

                //Micro Shadows
                enableMicroShadows = m_selectedProceduralSkyboxProfile.useMicroShadowing;
                microShadowOpacity = m_selectedProceduralSkyboxProfile.microShadowOpacity;

                //SS Reflection
                enableSSReflection = m_selectedProceduralSkyboxProfile.enableScreenSpaceReflections;
                ssrEdgeFade = m_selectedProceduralSkyboxProfile.screenEdgeFadeDistance;
                ssrNumberOfRays = m_selectedProceduralSkyboxProfile.maxNumberOfRaySteps;
                ssrObjectThickness = m_selectedProceduralSkyboxProfile.objectThickness;
                ssrMinSmoothness = m_selectedProceduralSkyboxProfile.minSmoothness;
                ssrSmoothnessFade = m_selectedProceduralSkyboxProfile.smoothnessFadeStart;
                ssrReflectSky = m_selectedProceduralSkyboxProfile.reflectSky;

                //SS Refract
                enableSSRefraction = m_selectedProceduralSkyboxProfile.enableScreenSpaceRefractions;
                ssrWeightDistance = m_selectedProceduralSkyboxProfile.screenWeightDistance;

                //Ambient Lighting
                useStaticLighting = m_selectedProceduralSkyboxProfile.useBakingSky;
                diffuseAmbientIntensity = m_selectedProceduralSkyboxProfile.indirectDiffuseIntensity;
                specularAmbientIntensity = m_selectedProceduralSkyboxProfile.indirectSpecularIntensity;
#endif
            }
            else
            {
                //Set profile
                m_selectedGradientSkyboxProfile = m_profiles.m_gradientSkyProfiles[m_selectedGradientSkyboxProfileIndex];

                if (m_enableEditMode && !m_profiles.m_isProceduralCreatedProfile)
                {
                    m_skiesProfileName = m_selectedGradientSkyboxProfile.name;
#if UNITY_POST_PROCESSING_STACK_V2
                    m_creationMatchPostProcessing = m_selectedGradientSkyboxProfile.creationMatchPostProcessing;
                    if (m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile != null)
                    {
                        postProcessingAssetName = m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile.name;
                    }
#endif
                }

                //Fog mode
                fogType = m_selectedGradientSkyboxProfile.fogType;

                //Ambient mode
                ambientMode = m_selectedGradientSkyboxProfile.ambientMode;

                //Skybox
                skyboxRotation = m_selectedGradientSkyboxProfile.skyboxRotation;
                skyboxPitch = m_selectedGradientSkyboxProfile.skyboxPitch;
#if HDPipeline
                topSkyColor = m_selectedGradientSkyboxProfile.topColor;
                middleSkyColor = m_selectedGradientSkyboxProfile.middleColor;
                bottomSkyColor = m_selectedGradientSkyboxProfile.bottomColor;
                gradientDiffusion = m_selectedGradientSkyboxProfile.gradientDiffusion;
                skyboxExposure = m_selectedGradientSkyboxProfile.hDRIExposure;
                skyMultiplier = m_selectedGradientSkyboxProfile.skyMultiplier;
#endif

                //Fog Settings
                fogColor = m_selectedGradientSkyboxProfile.fogColor;
                fogDistance = m_selectedGradientSkyboxProfile.fogDistance;
                nearFogDistance = m_selectedGradientSkyboxProfile.nearFogDistance;
                fogDensity = m_selectedGradientSkyboxProfile.fogDensity;

                //Ambient Settings
                skyColor = m_selectedGradientSkyboxProfile.skyColor;
                equatorColor = m_selectedGradientSkyboxProfile.equatorColor;
                groundColor = m_selectedGradientSkyboxProfile.groundColor;
                skyboxGroundIntensity = m_selectedGradientSkyboxProfile.skyboxGroundIntensity;

                //Sun Settings
                shadowStrength = m_selectedGradientSkyboxProfile.shadowStrength;
                indirectLightMultiplier = m_selectedGradientSkyboxProfile.indirectLightMultiplier;
                sunColor = m_selectedGradientSkyboxProfile.sunColor;
                sunIntensity = m_selectedGradientSkyboxProfile.sunIntensity;
                sunLWRPIntensity = m_selectedGradientSkyboxProfile.sunIntensity;
                sunHDRPIntensity = m_selectedGradientSkyboxProfile.HDRPSunIntensity;

                //Shadow Settings
                shadowDistance = m_selectedGradientSkyboxProfile.shadowDistance;
                shadowmaskMode = m_selectedGradientSkyboxProfile.shadowmaskMode;
                shadowType = m_selectedGradientSkyboxProfile.shadowType;
                shadowResolution = m_selectedGradientSkyboxProfile.shadowResolution;
                shadowProjection = m_selectedGradientSkyboxProfile.shadowProjection;
                shadowCascade = m_selectedGradientSkyboxProfile.cascadeCount;

                //Horizon Settings
                scaleHorizonObjectWithFog = m_selectedGradientSkyboxProfile.scaleHorizonObjectWithFog;
                horizonEnabled = m_selectedGradientSkyboxProfile.horizonSkyEnabled;
                horizonScattering = m_selectedGradientSkyboxProfile.horizonScattering;
                horizonFogDensity = m_selectedGradientSkyboxProfile.horizonFogDensity;
                horizonFalloff = m_selectedGradientSkyboxProfile.horizonFalloff;
                horizonBlend = m_selectedGradientSkyboxProfile.horizonBlend;
                horizonScale = m_selectedGradientSkyboxProfile.horizonSize;
                followPlayer = m_selectedGradientSkyboxProfile.followPlayer;
                horizonUpdateTime = m_selectedGradientSkyboxProfile.horizonUpdateTime;
                horizonPosition = m_selectedGradientSkyboxProfile.horizonPosition;

                //HD Pipeline Settings
#if HDPipeline && UNITY_2018_3_OR_NEWER
                //Fog Mode
                fogColorMode = m_selectedGradientSkyboxProfile.fogColorMode;

                //Volumetric Fog
                useDistanceFog = m_selectedGradientSkyboxProfile.volumetricEnableDistanceFog;
                baseFogDistance = m_selectedGradientSkyboxProfile.volumetricBaseFogDistance;
                baseFogHeight = m_selectedGradientSkyboxProfile.volumetricBaseFogHeight;
                meanFogHeight = m_selectedGradientSkyboxProfile.volumetricMeanHeight;
                globalAnisotropy = m_selectedGradientSkyboxProfile.volumetricGlobalAnisotropy;
                globalLightProbeDimmer = m_selectedGradientSkyboxProfile.volumetricGlobalLightProbeDimmer;

                //Exponential Fog
                exponentialFogDensity = m_selectedGradientSkyboxProfile.exponentialFogDensity;
                exponentialBaseFogHeight = m_selectedGradientSkyboxProfile.exponentialBaseHeight;
                exponentialHeightAttenuation = m_selectedGradientSkyboxProfile.exponentialHeightAttenuation;
                exponentialMaxFogDistance = m_selectedGradientSkyboxProfile.exponentialMaxFogDistance;
                exponentialMipFogNear = m_selectedGradientSkyboxProfile.exponentialMipFogNear;
                exponentialMipFogFar = m_selectedGradientSkyboxProfile.exponentialMipFogFar;
                exponentialMipFogMax = m_selectedGradientSkyboxProfile.exponentialMipFogMaxMip;

                //Linear Fog
                linearFogDensity = m_selectedGradientSkyboxProfile.linearFogDensity;
                linearFogHeightStart = m_selectedGradientSkyboxProfile.linearHeightStart;
                linearFogHeightEnd = m_selectedGradientSkyboxProfile.linearHeightEnd;
                linearFogMaxDistance = m_selectedGradientSkyboxProfile.linearMaxFogDistance;
                linearMipFogNear = m_selectedGradientSkyboxProfile.linearMipFogNear;
                linearMipFogFar = m_selectedGradientSkyboxProfile.linearMipFogFar;
                linearMipFogMax = m_selectedGradientSkyboxProfile.linearMipFogMaxMip;

                //Volumetric Light Controller
                depthExtent = m_selectedGradientSkyboxProfile.volumetricDistanceRange;
                sliceDistribution = m_selectedGradientSkyboxProfile.volumetricSliceDistributionUniformity;

                //Density Fog Volume
                useDensityFogVolume = m_selectedGradientSkyboxProfile.useFogDensityVolume;
                singleScatteringAlbedo = m_selectedGradientSkyboxProfile.singleScatteringAlbedo;
                densityVolumeFogDistance = m_selectedGradientSkyboxProfile.densityVolumeFogDistance;
                fogDensityMaskTexture = m_selectedGradientSkyboxProfile.fogDensityMaskTexture;
                densityMaskTiling = m_selectedGradientSkyboxProfile.densityMaskTiling;
                densityScale = m_selectedGradientSkyboxProfile.densityScale;

                //HD Shadows
                hDShadowQuality = m_selectedGradientSkyboxProfile.shadowQuality;
                split1 = m_selectedGradientSkyboxProfile.cascadeSplit1;
                split2 = m_selectedGradientSkyboxProfile.cascadeSplit2;
                split3 = m_selectedGradientSkyboxProfile.cascadeSplit3;

                //Contact Shadows
                enableContactShadows = m_selectedGradientSkyboxProfile.useContactShadows;
                contactLength = m_selectedGradientSkyboxProfile.contactShadowsLength;
                contactScaleFactor = m_selectedGradientSkyboxProfile.contactShadowsDistanceScaleFactor;
                contactMaxDistance = m_selectedGradientSkyboxProfile.contactShadowsMaxDistance;
                contactFadeDistance = m_selectedGradientSkyboxProfile.contactShadowsFadeDistance;
                contactSampleCount = m_selectedGradientSkyboxProfile.contactShadowsSampleCount;
                contactOpacity = m_selectedGradientSkyboxProfile.contactShadowsOpacity;

                //Micro Shadows
                enableMicroShadows = m_selectedGradientSkyboxProfile.useMicroShadowing;
                microShadowOpacity = m_selectedGradientSkyboxProfile.microShadowOpacity;

                //SS Reflection
                enableSSReflection = m_selectedGradientSkyboxProfile.enableScreenSpaceReflections;
                ssrEdgeFade = m_selectedGradientSkyboxProfile.screenEdgeFadeDistance;
                ssrNumberOfRays = m_selectedGradientSkyboxProfile.maxNumberOfRaySteps;
                ssrObjectThickness = m_selectedGradientSkyboxProfile.objectThickness;
                ssrMinSmoothness = m_selectedGradientSkyboxProfile.minSmoothness;
                ssrSmoothnessFade = m_selectedGradientSkyboxProfile.smoothnessFadeStart;
                ssrReflectSky = m_selectedGradientSkyboxProfile.reflectSky;

                //SS Refract
                enableSSRefraction = m_selectedGradientSkyboxProfile.enableScreenSpaceRefractions;
                ssrWeightDistance = m_selectedGradientSkyboxProfile.screenWeightDistance;

                //Ambient Lighting
                useStaticLighting = m_selectedGradientSkyboxProfile.useBakingSky;
                diffuseAmbientIntensity = m_selectedGradientSkyboxProfile.indirectDiffuseIntensity;
                specularAmbientIntensity = m_selectedGradientSkyboxProfile.indirectSpecularIntensity;
#endif
            }

            #endregion
        }

        /// <summary>
        /// Loads all the post processing settings based on render pipeline and profile selected
        /// </summary>
        private void LoadPostProcessingSettings()
        {
            //Global systems
            systemtype = m_profiles.m_systemTypes;

            //Enable Post Fx
            usePostProcess = m_profiles.m_usePostFX;

            //Selection
            newPPSelection = m_selectedPostProcessingProfile.profileIndex;
            if (m_selectedPostProcessingProfileIndex < m_profiles.m_ppProfiles.Count && m_selectedPostProcessingProfileIndex == -1)
            {
                newPPSelection = 0;
                m_selectedPostProcessingProfileIndex = 0;
            }
            m_selectedPostProcessingProfileIndex = AmbientSkiesEditorFunctions.CheckPostProcessing(m_selectedPostProcessingProfileIndex, m_profiles);
            m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
            autoMatchProfile = m_profiles.m_autoMatchProfile;

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                #region Post Processing Values

                //Creation mode
#if UNITY_POST_PROCESSING_STACK_V2
                m_postProfileName = m_selectedPostProcessingProfile.name;
                m_postProcessAssetName = m_selectedPostProcessingProfile.creationPostProcessProfile;
#endif

                //Hide Gizmo
                hideGizmos = m_selectedPostProcessingProfile.hideGizmos;

                autoMatchProfile = m_profiles.m_autoMatchProfile;

                //Custom profile
#if UNITY_POST_PROCESSING_STACK_V2
                customPostProcessingProfile = m_selectedPostProcessingProfile.customPostProcessingProfile;
#endif
                //HDR Mode
                hDRMode = m_selectedPostProcessingProfile.hDRMode;

                //Anti Aliasing Mode
                antiAliasingMode = m_selectedPostProcessingProfile.antiAliasingMode;

                //Target Platform
                targetPlatform = m_profiles.m_targetPlatform;

                if (m_selectedPostProcessingProfile.name != "User")
                {
                    //AO settings
                    aoEnabled = m_selectedPostProcessingProfile.aoEnabled;
                    aoAmount = m_selectedPostProcessingProfile.aoAmount;
                    aoColor = m_selectedPostProcessingProfile.aoColor;
#if UNITY_POST_PROCESSING_STACK_V2
                    ambientOcclusionMode = m_selectedPostProcessingProfile.ambientOcclusionMode;
#endif
                    //Exposure settings
                    autoExposureEnabled = m_selectedPostProcessingProfile.autoExposureEnabled;
                    exposureAmount = m_selectedPostProcessingProfile.exposureAmount;
                    exposureMin = m_selectedPostProcessingProfile.exposureMin;
                    exposureMax = m_selectedPostProcessingProfile.exposureMax;

                    //Bloom settings
                    bloomEnabled = m_selectedPostProcessingProfile.bloomEnabled;
                    bloomIntensity = m_selectedPostProcessingProfile.bloomAmount;
                    bloomThreshold = m_selectedPostProcessingProfile.bloomThreshold;
                    lensIntensity = m_selectedPostProcessingProfile.lensIntensity;
                    lensTexture = m_selectedPostProcessingProfile.lensTexture;

                    //Chromatic Aberration
                    chromaticAberrationEnabled = m_selectedPostProcessingProfile.chromaticAberrationEnabled;
                    chromaticAberrationIntensity = m_selectedPostProcessingProfile.chromaticAberrationIntensity;

                    //Color Grading settings
                    colorGradingEnabled = m_selectedPostProcessingProfile.colorGradingEnabled;
#if UNITY_POST_PROCESSING_STACK_V2
                    colorGradingMode = m_selectedPostProcessingProfile.colorGradingMode;
#endif
                    colorGradingLut = m_selectedPostProcessingProfile.colorGradingLut;
                    colorGradingPostExposure = m_selectedPostProcessingProfile.colorGradingPostExposure;
                    colorGradingColorFilter = m_selectedPostProcessingProfile.colorGradingColorFilter;
                    colorGradingTempature = m_selectedPostProcessingProfile.colorGradingTempature;
                    colorGradingTint = m_selectedPostProcessingProfile.colorGradingTint;
                    colorGradingSaturation = m_selectedPostProcessingProfile.colorGradingSaturation;
                    colorGradingContrast = m_selectedPostProcessingProfile.colorGradingContrast;
                    channelMixerRed = m_selectedPostProcessingProfile.channelMixerRed;
                    channelMixerBlue = m_selectedPostProcessingProfile.channelMixerBlue;
                    channelMixerGreen = m_selectedPostProcessingProfile.channelMixerGreen;

                    //DOF settings
                    depthOfFieldMode = m_selectedPostProcessingProfile.depthOfFieldMode;
                    depthOfFieldEnabled = m_selectedPostProcessingProfile.depthOfFieldEnabled;
                    depthOfFieldFocusDistance = m_selectedPostProcessingProfile.depthOfFieldFocusDistance;
                    depthOfFieldAperture = m_selectedPostProcessingProfile.depthOfFieldAperture;
                    depthOfFieldFocalLength = m_selectedPostProcessingProfile.depthOfFieldFocalLength;
                    depthOfFieldTrackingType = m_selectedPostProcessingProfile.depthOfFieldTrackingType;
                    focusOffset = m_selectedPostProcessingProfile.focusOffset;
                    targetLayer = m_selectedPostProcessingProfile.targetLayer;
                    maxFocusDistance = m_selectedPostProcessingProfile.maxFocusDistance;
#if UNITY_POST_PROCESSING_STACK_V2
                    maxBlurSize = m_selectedPostProcessingProfile.maxBlurSize;
#endif
                    //Distortion settings
                    distortionEnabled = m_selectedPostProcessingProfile.distortionEnabled;
                    distortionIntensity = m_selectedPostProcessingProfile.distortionIntensity;
                    distortionScale = m_selectedPostProcessingProfile.distortionScale;

                    //Grain settings
                    grainEnabled = m_selectedPostProcessingProfile.grainEnabled;
                    grainIntensity = m_selectedPostProcessingProfile.grainIntensity;
                    grainSize = m_selectedPostProcessingProfile.grainSize;

                    //SSR settings
                    screenSpaceReflectionsEnabled = m_selectedPostProcessingProfile.screenSpaceReflectionsEnabled;
                    maximumIterationCount = m_selectedPostProcessingProfile.maximumIterationCount;
                    thickness = m_selectedPostProcessingProfile.thickness;
#if UNITY_POST_PROCESSING_STACK_V2
                    screenSpaceReflectionResolution = m_selectedPostProcessingProfile.spaceReflectionResolution;
                    screenSpaceReflectionPreset = m_selectedPostProcessingProfile.screenSpaceReflectionPreset;
#endif
                    maximumMarchDistance = m_selectedPostProcessingProfile.maximumMarchDistance;
                    distanceFade = m_selectedPostProcessingProfile.distanceFade;
                    screenSpaceVignette = m_selectedPostProcessingProfile.screenSpaceVignette;

                    //Vignette settings
                    vignetteEnabled = m_selectedPostProcessingProfile.vignetteEnabled;
                    vignetteIntensity = m_selectedPostProcessingProfile.vignetteIntensity;
                    vignetteSmoothness = m_selectedPostProcessingProfile.vignetteSmoothness;

                    //Motion Blur settings
                    motionBlurEnabled = m_selectedPostProcessingProfile.motionBlurEnabled;
                    motionShutterAngle = m_selectedPostProcessingProfile.shutterAngle;
                    motionSampleCount = m_selectedPostProcessingProfile.sampleCount;
#if Mewlist_Clouds
                    //Massive Cloud Settings
                    massiveCloudsEnabled = m_selectedPostProcessingProfile.massiveCloudsEnabled;
                    cloudProfile = m_selectedPostProcessingProfile.cloudProfile;
                    syncGlobalFogColor = m_selectedPostProcessingProfile.syncGlobalFogColor;
                    syncBaseFogColor = m_selectedPostProcessingProfile.syncBaseFogColor;
                    cloudsFogColor = m_selectedPostProcessingProfile.cloudsFogColor;
                    cloudsBaseFogColor = m_selectedPostProcessingProfile.cloudsBaseFogColor;
                    cloudIsHDRP = m_selectedPostProcessingProfile.cloudIsHDRP;
#endif
                }

                #endregion
            }
#if !UNITY_2019_1_OR_NEWER
            else
            {
                #region Post Processing Values

                //Creation mode
#if UNITY_POST_PROCESSING_STACK_V2
                m_postProfileName = m_selectedPostProcessingProfile.name;
                m_postProcessAssetName = m_selectedPostProcessingProfile.creationPostProcessProfile;
#endif

                //Hide Gizmo
                hideGizmos = m_selectedPostProcessingProfile.hideGizmos;

                autoMatchProfile = m_profiles.m_autoMatchProfile;

                //Custom profile
#if UNITY_POST_PROCESSING_STACK_V2
                customPostProcessingProfile = m_selectedPostProcessingProfile.customPostProcessingProfile;
#endif
                //HDR Mode
                hDRMode = m_selectedPostProcessingProfile.hDRMode;

                //Anti Aliasing Mode
                antiAliasingMode = m_selectedPostProcessingProfile.antiAliasingMode;

                //Target Platform
                targetPlatform = m_profiles.m_targetPlatform;

                //AO settings
                aoEnabled = m_selectedPostProcessingProfile.aoEnabled;
                aoAmount = m_selectedPostProcessingProfile.aoAmount;
                aoColor = m_selectedPostProcessingProfile.aoColor;
#if UNITY_POST_PROCESSING_STACK_V2
                ambientOcclusionMode = m_selectedPostProcessingProfile.ambientOcclusionMode;
#endif
                //Exposure settings
                autoExposureEnabled = m_selectedPostProcessingProfile.autoExposureEnabled;
                exposureAmount = m_selectedPostProcessingProfile.exposureAmount;
                exposureMin = m_selectedPostProcessingProfile.exposureMin;
                exposureMax = m_selectedPostProcessingProfile.exposureMax;

                //Bloom settings
                bloomEnabled = m_selectedPostProcessingProfile.bloomEnabled;
                bloomIntensity = m_selectedPostProcessingProfile.bloomAmount;
                bloomThreshold = m_selectedPostProcessingProfile.bloomThreshold;
                lensIntensity = m_selectedPostProcessingProfile.lensIntensity;
                lensTexture = m_selectedPostProcessingProfile.lensTexture;

                //Chromatic Aberration
                chromaticAberrationEnabled = m_selectedPostProcessingProfile.chromaticAberrationEnabled;
                chromaticAberrationIntensity = m_selectedPostProcessingProfile.chromaticAberrationIntensity;

                //Color Grading settings
                colorGradingEnabled = m_selectedPostProcessingProfile.colorGradingEnabled;
#if UNITY_POST_PROCESSING_STACK_V2
                colorGradingMode = m_selectedPostProcessingProfile.colorGradingMode;
#endif
                colorGradingLut = m_selectedPostProcessingProfile.colorGradingLut;
                colorGradingPostExposure = m_selectedPostProcessingProfile.colorGradingPostExposure;
                colorGradingColorFilter = m_selectedPostProcessingProfile.colorGradingColorFilter;
                colorGradingTempature = m_selectedPostProcessingProfile.colorGradingTempature;
                colorGradingTint = m_selectedPostProcessingProfile.colorGradingTint;
                colorGradingSaturation = m_selectedPostProcessingProfile.colorGradingSaturation;
                colorGradingContrast = m_selectedPostProcessingProfile.colorGradingContrast;
                channelMixerRed = m_selectedPostProcessingProfile.channelMixerRed;
                channelMixerBlue = m_selectedPostProcessingProfile.channelMixerBlue;
                channelMixerGreen = m_selectedPostProcessingProfile.channelMixerGreen;

                //DOF settings
                depthOfFieldMode = m_selectedPostProcessingProfile.depthOfFieldMode;
                depthOfFieldEnabled = m_selectedPostProcessingProfile.depthOfFieldEnabled;
                depthOfFieldFocusDistance = m_selectedPostProcessingProfile.depthOfFieldFocusDistance;
                depthOfFieldAperture = m_selectedPostProcessingProfile.depthOfFieldAperture;
                depthOfFieldFocalLength = m_selectedPostProcessingProfile.depthOfFieldFocalLength;
                depthOfFieldTrackingType = m_selectedPostProcessingProfile.depthOfFieldTrackingType;
                focusOffset = m_selectedPostProcessingProfile.focusOffset;
                targetLayer = m_selectedPostProcessingProfile.targetLayer;
                maxFocusDistance = m_selectedPostProcessingProfile.maxFocusDistance;
#if UNITY_POST_PROCESSING_STACK_V2
                maxBlurSize = m_selectedPostProcessingProfile.maxBlurSize;
#endif
                //Distortion settings
                distortionEnabled = m_selectedPostProcessingProfile.distortionEnabled;
                distortionIntensity = m_selectedPostProcessingProfile.distortionIntensity;
                distortionScale = m_selectedPostProcessingProfile.distortionScale;

                //Grain settings
                grainEnabled = m_selectedPostProcessingProfile.grainEnabled;
                grainIntensity = m_selectedPostProcessingProfile.grainIntensity;
                grainSize = m_selectedPostProcessingProfile.grainSize;

                //SSR settings
                screenSpaceReflectionsEnabled = m_selectedPostProcessingProfile.screenSpaceReflectionsEnabled;
                maximumIterationCount = m_selectedPostProcessingProfile.maximumIterationCount;
                thickness = m_selectedPostProcessingProfile.thickness;
#if UNITY_POST_PROCESSING_STACK_V2
                screenSpaceReflectionResolution = m_selectedPostProcessingProfile.spaceReflectionResolution;
                screenSpaceReflectionPreset = m_selectedPostProcessingProfile.screenSpaceReflectionPreset;
#endif
                maximumMarchDistance = m_selectedPostProcessingProfile.maximumMarchDistance;
                distanceFade = m_selectedPostProcessingProfile.distanceFade;
                screenSpaceVignette = m_selectedPostProcessingProfile.screenSpaceVignette;

                //Vignette settings
                vignetteEnabled = m_selectedPostProcessingProfile.vignetteEnabled;
                vignetteIntensity = m_selectedPostProcessingProfile.vignetteIntensity;
                vignetteSmoothness = m_selectedPostProcessingProfile.vignetteSmoothness;

                //Motion Blur settings
                motionBlurEnabled = m_selectedPostProcessingProfile.motionBlurEnabled;
                motionShutterAngle = m_selectedPostProcessingProfile.shutterAngle;
                motionSampleCount = m_selectedPostProcessingProfile.sampleCount;
#if Mewlist_Clouds
                //Massive Cloud Settings
                massiveCloudsEnabled = m_selectedPostProcessingProfile.massiveCloudsEnabled;
                cloudProfile = m_selectedPostProcessingProfile.cloudProfile;
                syncGlobalFogColor = m_selectedPostProcessingProfile.syncGlobalFogColor;
                syncBaseFogColor = m_selectedPostProcessingProfile.syncBaseFogColor;
                cloudsFogColor = m_selectedPostProcessingProfile.cloudsFogColor;
                cloudsBaseFogColor = m_selectedPostProcessingProfile.cloudsBaseFogColor;
                cloudIsHDRP = m_selectedPostProcessingProfile.cloudIsHDRP;
#endif

        #endregion
            }
#endif

#if UNITY_2019_1_OR_NEWER
            else if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Universal)
            {
                #region URP Post Processing Values

                //Creation mode
#if UNITY_POST_PROCESSING_STACK_V2
                m_postProfileName = m_selectedPostProcessingProfile.name;
                m_postProcessAssetName = m_selectedPostProcessingProfile.creationPostProcessProfile;
#endif

#if UPPipeline && UNITY_2019_3_OR_NEWER

                hideGizmos = m_selectedPostProcessingProfile.hideGizmos;
                antiAliasingMode = m_selectedPostProcessingProfile.antiAliasingMode;
                dithering = m_selectedPostProcessingProfile.dithering;
                hDRMode = m_selectedPostProcessingProfile.hDRMode;
                customURPPostProcessingprofile = m_selectedPostProcessingProfile.customURPPostProcessingprofile;

                bloomEnabled = m_selectedPostProcessingProfile.bloomEnabled;
                uRPBloomIntensity = m_selectedPostProcessingProfile.uRPBloomIntensity;
                uRPBloomScatter = m_selectedPostProcessingProfile.uRPBloomScatter;
                uRPBloomTint = m_selectedPostProcessingProfile.uRPBloomTint;
                uRPBloomDirtLensTexture = m_selectedPostProcessingProfile.uRPBloomDirtLensTexture;
                uRPBloomDirtLensIntensity = m_selectedPostProcessingProfile.uRPBloomDirtLensIntensity;
                uRPBloomHighQualityFiltering = m_selectedPostProcessingProfile.uRPBloomHighQualityFiltering;
                uRPBloomPrefiler = m_selectedPostProcessingProfile.uRPBloomPrefiler;
                uRPBloomAnamorphic = m_selectedPostProcessingProfile.uRPBloomAnamorphic;

                chromaticAberrationEnabled = m_selectedPostProcessingProfile.chromaticAberrationEnabled;
                uRPChromaticAberrationIntensity = m_selectedPostProcessingProfile.uRPChromaticAberrationIntensity;

                colorGradingEnabled = m_selectedPostProcessingProfile.colorGradingEnabled;
                uRPColorLookupTexture = m_selectedPostProcessingProfile.uRPColorLookupTexture;
                uRPColorAdjustmentColorFilter = m_selectedPostProcessingProfile.uRPColorAdjustmentColorFilter;
                uRPColorAdjustmentPostExposure = m_selectedPostProcessingProfile.uRPColorAdjustmentPostExposure;
                uRPWhiteBalanceTempature = m_selectedPostProcessingProfile.uRPWhiteBalanceTempature;
                uRPColorLookupContribution = m_selectedPostProcessingProfile.uRPColorLookupContribution;
                uRPWhiteBalanceTint = m_selectedPostProcessingProfile.uRPWhiteBalanceTint;
                uRPColorAdjustmentSaturation = m_selectedPostProcessingProfile.uRPColorAdjustmentSaturation;
                uRPColorAdjustmentContrast = m_selectedPostProcessingProfile.uRPColorAdjustmentContrast;
                uRPChannelMixerRed = m_selectedPostProcessingProfile.uRPChannelMixerRed;
                uRPChannelMixerGreen = m_selectedPostProcessingProfile.uRPChannelMixerGreen;
                uRPChannelMixerBlue = m_selectedPostProcessingProfile.uRPChannelMixerBlue;
                uRPTonemappingMode = m_selectedPostProcessingProfile.uRPTonemappingMode;
                uRPTonemappingToeStrength = m_selectedPostProcessingProfile.uRPTonemappingToeStrength;
                uRPTonemappingToeLength = m_selectedPostProcessingProfile.uRPTonemappingToeLength;
                uRPTonemappingShoulderStrength = m_selectedPostProcessingProfile.uRPTonemappingShoulderStrength;
                uRPTonemappingShoulderLength = m_selectedPostProcessingProfile.uRPTonemappingShoulderLength;
                uRPTonemappingShoulderAngle = m_selectedPostProcessingProfile.uRPTonemappingShoulderAngle;
                uRPTonemappingGamma = m_selectedPostProcessingProfile.uRPTonemappingGamma;
                uRPSplitToningShadows = m_selectedPostProcessingProfile.uRPSplitToningShadows;
                uRPSplitToningHighlights = m_selectedPostProcessingProfile.uRPSplitToningHighlights;
                uRPSplitToningBalance = m_selectedPostProcessingProfile.uRPSplitToningBalance;

                depthOfFieldEnabled = m_selectedPostProcessingProfile.depthOfFieldEnabled;
                depthOfFieldMode = m_selectedPostProcessingProfile.depthOfFieldMode;
                depthOfFieldFocusDistance = m_selectedPostProcessingProfile.depthOfFieldFocusDistance;
                depthOfFieldTrackingType = m_selectedPostProcessingProfile.depthOfFieldTrackingType;
                focusOffset = m_selectedPostProcessingProfile.focusOffset;
                targetLayer = m_selectedPostProcessingProfile.targetLayer;
                maxFocusDistance = m_selectedPostProcessingProfile.maxFocusDistance;
                uRPDepthOfFieldMode = m_selectedPostProcessingProfile.uRPDepthOfFieldMode;
                //Gaussian
                uRPDepthOfFieldStart = m_selectedPostProcessingProfile.uRPDepthOfFieldStart;
                uRPDepthOfFieldEnd = m_selectedPostProcessingProfile.uRPDepthOfFieldEnd;
                uRPDepthOfFieldMaxRadius = m_selectedPostProcessingProfile.uRPDepthOfFieldMaxRadius;
                uRPDepthOfFieldHighQualityFiltering = m_selectedPostProcessingProfile.uRPDepthOfFieldHighQualityFiltering;
                //Bokeh
                uRPDepthOfFieldFocusDitance = m_selectedPostProcessingProfile.uRPDepthOfFieldFocusDitance;
                uRPDepthOfFieldFocalLength = m_selectedPostProcessingProfile.uRPDepthOfFieldFocalLength;
                uRPDepthOfFieldAperture = m_selectedPostProcessingProfile.uRPDepthOfFieldAperture;
                uRPDepthOfFieldBladeCount = m_selectedPostProcessingProfile.uRPDepthOfFieldBladeCount;
                uRPDepthOfFieldBladeCurvature = m_selectedPostProcessingProfile.uRPDepthOfFieldBladeCurvature;
                uRPDepthOfFieldBladeRotation = m_selectedPostProcessingProfile.uRPDepthOfFieldBladeRotation;

                grainEnabled = m_selectedPostProcessingProfile.grainEnabled;
                uRPFilmGrainType = m_selectedPostProcessingProfile.uRPFilmGrainType;
                uRPFilmGrainIntensity = m_selectedPostProcessingProfile.uRPFilmGrainIntensity;
                uRPFilmGrainResponse = m_selectedPostProcessingProfile.uRPFilmGrainResponse;

                distortionEnabled = m_selectedPostProcessingProfile.distortionEnabled;
                uRPLensDistortionIntensity = m_selectedPostProcessingProfile.uRPLensDistortionIntensity;
                uRPLensDistortionXMultiplier = m_selectedPostProcessingProfile.uRPLensDistortionXMultiplier;
                uRPLensDistortionYMultiplier = m_selectedPostProcessingProfile.uRPLensDistortionYMultiplier;
                uRPLensDistortionCenter = m_selectedPostProcessingProfile.uRPLensDistortionCenter;
                uRPLensDistortionScale = m_selectedPostProcessingProfile.uRPLensDistortionScale;

                vignetteEnabled = m_selectedPostProcessingProfile.vignetteEnabled;
                uRPVignetteColor = m_selectedPostProcessingProfile.uRPVignetteColor;
                uRPVignetteCenter = m_selectedPostProcessingProfile.uRPVignetteCenter;
                uRPVignetteIntensity = m_selectedPostProcessingProfile.uRPVignetteIntensity;
                uRPVignetteSmoothness = m_selectedPostProcessingProfile.uRPVignetteSmoothness;
                uRPVignetteRoundness = m_selectedPostProcessingProfile.uRPVignetteRoundness;
                uRPVignetteRounded = m_selectedPostProcessingProfile.uRPVignetteRounded;
                uRPVignetteMask = m_selectedPostProcessingProfile.uRPVignetteMask;
                uRPVignetteMaskOpacity = m_selectedPostProcessingProfile.uRPVignetteMaskOpacity;

                motionBlurEnabled = m_selectedPostProcessingProfile.motionBlurEnabled;
                uRPMotionBlurQuality = m_selectedPostProcessingProfile.uRPMotionBlurQuality;
                uRPMotionBlurIntensity = m_selectedPostProcessingProfile.uRPMotionBlurIntensity;
                uRPMotionBlurCameraRotationVelocityClamp = m_selectedPostProcessingProfile.uRPMotionBlurCameraRotationVelocityClamp;

                uRPPaniniProjectionEnabled = m_selectedPostProcessingProfile.uRPPaniniProjectionEnabled;
                uRPPaniniProjectionDistance = m_selectedPostProcessingProfile.uRPPaniniProjectionDistance;
                uRPPaniniProjectionCropToFit = m_selectedPostProcessingProfile.uRPPaniniProjectionCropToFit;

#endif

                #endregion
            }
            else
            {
                #region HDRP Post Processing Values

                //Creation mode
#if UNITY_POST_PROCESSING_STACK_V2
                m_postProfileName = m_selectedPostProcessingProfile.name;
                m_postProcessAssetName = m_selectedPostProcessingProfile.creationPostProcessProfile;
#endif

#if HDPipeline && UNITY_2019_1_OR_NEWER

                    hideGizmos = m_selectedPostProcessingProfile.hideGizmos;
                    antiAliasingMode = m_selectedPostProcessingProfile.antiAliasingMode;
                    dithering = m_selectedPostProcessingProfile.dithering;
                    hDRMode = m_selectedPostProcessingProfile.hDRMode;
                    customHDRPPostProcessingprofile = m_selectedPostProcessingProfile.customHDRPPostProcessingprofile;

                    aoEnabled = m_selectedPostProcessingProfile.aoEnabled;
                    hDRPAOIntensity = m_selectedPostProcessingProfile.hDRPAOIntensity;
                    hDRPAOThicknessModifier = m_selectedPostProcessingProfile.hDRPAOThicknessModifier;
                    hDRPAODirectLightingStrength = m_selectedPostProcessingProfile.hDRPAODirectLightingStrength;

                    autoExposureEnabled = m_selectedPostProcessingProfile.autoExposureEnabled;
                    hDRPExposureMode = m_selectedPostProcessingProfile.hDRPExposureMode;
                    hDRPExposureMeteringMode = m_selectedPostProcessingProfile.hDRPExposureMeteringMode;
                    hDRPExposureLuminationSource = m_selectedPostProcessingProfile.hDRPExposureLuminationSource;
                    hDRPExposureFixedExposure = m_selectedPostProcessingProfile.hDRPExposureFixedExposure;
                    hDRPExposureCurveMap = m_selectedPostProcessingProfile.hDRPExposureCurveMap;
                    hDRPExposureCompensation = m_selectedPostProcessingProfile.hDRPExposureCompensation;
                    hDRPExposureLimitMin = m_selectedPostProcessingProfile.hDRPExposureLimitMin;
                    hDRPExposureLimitMax = m_selectedPostProcessingProfile.hDRPExposureLimitMax;
                    hDRPExposureAdaptionMode = m_selectedPostProcessingProfile.hDRPExposureAdaptionMode;
                    hDRPExposureAdaptionSpeedDarkToLight = m_selectedPostProcessingProfile.hDRPExposureAdaptionSpeedDarkToLight;
                    hDRPExposureAdaptionSpeedLightToDark = m_selectedPostProcessingProfile.hDRPExposureAdaptionSpeedLightToDark;

                    bloomEnabled = m_selectedPostProcessingProfile.bloomEnabled;
                    hDRPBloomIntensity = m_selectedPostProcessingProfile.hDRPBloomIntensity;
                    hDRPBloomScatter = m_selectedPostProcessingProfile.hDRPBloomScatter;
                    hDRPBloomTint = m_selectedPostProcessingProfile.hDRPBloomTint;
                    hDRPBloomDirtLensTexture = m_selectedPostProcessingProfile.hDRPBloomDirtLensTexture;
                    hDRPBloomDirtLensIntensity = m_selectedPostProcessingProfile.hDRPBloomDirtLensIntensity;
                    hDRPBloomResolution = m_selectedPostProcessingProfile.hDRPBloomResolution;
                    hDRPBloomHighQualityFiltering = m_selectedPostProcessingProfile.hDRPBloomHighQualityFiltering;
                    hDRPBloomPrefiler = m_selectedPostProcessingProfile.hDRPBloomPrefiler;
                    hDRPBloomAnamorphic = m_selectedPostProcessingProfile.hDRPBloomAnamorphic;

                    chromaticAberrationEnabled = m_selectedPostProcessingProfile.chromaticAberrationEnabled;
                    hDRPChromaticAberrationSpectralLut = m_selectedPostProcessingProfile.hDRPChromaticAberrationSpectralLut;
                    hDRPChromaticAberrationIntensity = m_selectedPostProcessingProfile.hDRPChromaticAberrationIntensity;
                    hDRPChromaticAberrationMaxSamples = m_selectedPostProcessingProfile.hDRPChromaticAberrationMaxSamples;

                    hDRPColorLookupTexture = m_selectedPostProcessingProfile.hDRPColorLookupTexture;
                    hDRPColorAdjustmentColorFilter = m_selectedPostProcessingProfile.hDRPColorAdjustmentColorFilter;
                    hDRPColorAdjustmentPostExposure = m_selectedPostProcessingProfile.hDRPColorAdjustmentPostExposure;
                    colorGradingEnabled = m_selectedPostProcessingProfile.colorGradingEnabled;
                    hDRPWhiteBalanceTempature = m_selectedPostProcessingProfile.hDRPWhiteBalanceTempature;
                    hDRPColorLookupContribution = m_selectedPostProcessingProfile.hDRPColorLookupContribution;
                    hDRPWhiteBalanceTint = m_selectedPostProcessingProfile.hDRPWhiteBalanceTint;
                    hDRPColorAdjustmentSaturation = m_selectedPostProcessingProfile.hDRPColorAdjustmentSaturation;
                    hDRPColorAdjustmentContrast = m_selectedPostProcessingProfile.hDRPColorAdjustmentContrast;
                    hDRPChannelMixerRed = m_selectedPostProcessingProfile.hDRPChannelMixerRed;
                    hDRPChannelMixerGreen = m_selectedPostProcessingProfile.hDRPChannelMixerGreen;
                    hDRPChannelMixerBlue = m_selectedPostProcessingProfile.hDRPChannelMixerBlue;
                    hDRPTonemappingMode = m_selectedPostProcessingProfile.hDRPTonemappingMode;
                    hDRPTonemappingToeStrength = m_selectedPostProcessingProfile.hDRPTonemappingToeStrength;
                    hDRPTonemappingToeLength = m_selectedPostProcessingProfile.hDRPTonemappingToeLength;
                    hDRPTonemappingShoulderStrength = m_selectedPostProcessingProfile.hDRPTonemappingShoulderStrength;
                    hDRPTonemappingShoulderLength = m_selectedPostProcessingProfile.hDRPTonemappingShoulderLength;
                    hDRPTonemappingShoulderAngle = m_selectedPostProcessingProfile.hDRPTonemappingShoulderAngle;
                    hDRPTonemappingGamma = m_selectedPostProcessingProfile.hDRPTonemappingGamma;
                    hDRPSplitToningShadows = m_selectedPostProcessingProfile.hDRPSplitToningShadows;
                    hDRPSplitToningHighlights = m_selectedPostProcessingProfile.hDRPSplitToningHighlights;
                    hDRPSplitToningBalance = m_selectedPostProcessingProfile.hDRPSplitToningBalance;

                    depthOfFieldEnabled = m_selectedPostProcessingProfile.depthOfFieldEnabled;
                    depthOfFieldMode = m_selectedPostProcessingProfile.depthOfFieldMode;
                    depthOfFieldFocusDistance = m_selectedPostProcessingProfile.depthOfFieldFocusDistance;
                    depthOfFieldTrackingType = m_selectedPostProcessingProfile.depthOfFieldTrackingType;
                    focusOffset = m_selectedPostProcessingProfile.focusOffset;
                    targetLayer = m_selectedPostProcessingProfile.targetLayer;
                    maxFocusDistance = m_selectedPostProcessingProfile.maxFocusDistance;
                    hDRPDepthOfFieldFocusMode = m_selectedPostProcessingProfile.hDRPDepthOfFieldFocusMode;
                    hDRPDepthOfFieldNearBlurStart = m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurStart;
                    hDRPDepthOfFieldNearBlurEnd = m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurEnd;
                    hDRPDepthOfFieldNearBlurSampleCount = m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurSampleCount;
                    hDRPDepthOfFieldNearBlurMaxRadius = m_selectedPostProcessingProfile.hDRPDepthOfFieldNearBlurMaxRadius;
                    hDRPDepthOfFieldFarBlurStart = m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurStart;
                    hDRPDepthOfFieldFarBlurEnd = m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurEnd;
                    hDRPDepthOfFieldFarBlurSampleCount = m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurSampleCount;
                    hDRPDepthOfFieldFarBlurMaxRadius = m_selectedPostProcessingProfile.hDRPDepthOfFieldFarBlurMaxRadius;
                    hDRPDepthOfFieldResolution = m_selectedPostProcessingProfile.hDRPDepthOfFieldResolution;
                    hDRPDepthOfFieldHighQualityFiltering = m_selectedPostProcessingProfile.hDRPDepthOfFieldHighQualityFiltering;

                    grainEnabled = m_selectedPostProcessingProfile.grainEnabled;
                    hDRPFilmGrainType = m_selectedPostProcessingProfile.hDRPFilmGrainType;
                    hDRPFilmGrainIntensity = m_selectedPostProcessingProfile.hDRPFilmGrainIntensity;
                    hDRPFilmGrainResponse = m_selectedPostProcessingProfile.hDRPFilmGrainResponse;

                    distortionEnabled = m_selectedPostProcessingProfile.distortionEnabled;
                    hDRPLensDistortionIntensity = m_selectedPostProcessingProfile.hDRPLensDistortionIntensity;
                    hDRPLensDistortionXMultiplier = m_selectedPostProcessingProfile.hDRPLensDistortionXMultiplier;
                    hDRPLensDistortionYMultiplier = m_selectedPostProcessingProfile.hDRPLensDistortionYMultiplier;
                    hDRPLensDistortionCenter = m_selectedPostProcessingProfile.hDRPLensDistortionCenter;
                    hDRPLensDistortionScale = m_selectedPostProcessingProfile.hDRPLensDistortionScale;

                    vignetteEnabled = m_selectedPostProcessingProfile.vignetteEnabled;
                    hDRPVignetteMode = m_selectedPostProcessingProfile.hDRPVignetteMode;
                    hDRPVignetteColor = m_selectedPostProcessingProfile.hDRPVignetteColor;
                    hDRPVignetteCenter = m_selectedPostProcessingProfile.hDRPVignetteCenter;
                    hDRPVignetteIntensity = m_selectedPostProcessingProfile.hDRPVignetteIntensity;
                    hDRPVignetteSmoothness = m_selectedPostProcessingProfile.hDRPVignetteSmoothness;
                    hDRPVignetteRoundness = m_selectedPostProcessingProfile.hDRPVignetteRoundness;
                    hDRPVignetteRounded = m_selectedPostProcessingProfile.hDRPVignetteRounded;
                    hDRPVignetteMask = m_selectedPostProcessingProfile.hDRPVignetteMask;
                    hDRPVignetteMaskOpacity = m_selectedPostProcessingProfile.hDRPVignetteMaskOpacity;

                    motionBlurEnabled = m_selectedPostProcessingProfile.motionBlurEnabled;
                    hDRPMotionBlurIntensity = m_selectedPostProcessingProfile.hDRPMotionBlurIntensity;
                    hDRPMotionBlurSampleCount = m_selectedPostProcessingProfile.hDRPMotionBlurSampleCount;
                    hDRPMotionBlurMaxVelocity = m_selectedPostProcessingProfile.hDRPMotionBlurMaxVelocity;
                    hDRPMotionBlurMinVelocity = m_selectedPostProcessingProfile.hDRPMotionBlurMinVelocity;
                    hDRPMotionBlurCameraRotationVelocityClamp = m_selectedPostProcessingProfile.hDRPMotionBlurCameraRotationVelocityClamp;

                    hDRPPaniniProjectionEnabled = m_selectedPostProcessingProfile.hDRPPaniniProjectionEnabled;
                    hDRPPaniniProjectionDistance = m_selectedPostProcessingProfile.hDRPPaniniProjectionDistance;
                    hDRPPaniniProjectionCropToFit = m_selectedPostProcessingProfile.hDRPPaniniProjectionCropToFit;

#endif

                #endregion
            }
#endif
        }

        /// <summary>
        /// Sets the fog base height, blend height and distance
        /// </summary>
        /// <param name="skyProfile"></param>
        /// <param name="hdriSky"></param>
        /// <param name="proceduralSky"></param>
        /// <param name="gradientSky"></param>
        public void ReconfigureHDRPFog(AmbientSkyProfiles skyProfile, AmbientSkyboxProfile hdriSky, AmbientProceduralSkyboxProfile proceduralSky, AmbientGradientSkyboxProfile gradientSky)
        {
            //Main values
            float lowestFogHeight = 0f;
            float highestFogHeight = 50f;
            //Reconfiure the main values
            SkyboxUtils.SampleTerrainHeights(skyProfile, 5, out lowestFogHeight, out highestFogHeight);

            if (skyProfile == null)
            {
                //Exit if skyProfile is null
                Debug.LogError("Main system profile is missing, try reopening Ambient Skies and trying again");
            }
            else
            {
                switch (skyProfile.m_systemTypes)
                {
                    //Active sky mode equals HDRI
                    case AmbientSkiesConsts.SystemTypes.AmbientHDRISkies when hdriSky == null:
                        //If skybox profile is misisng, exit
                        Debug.LogError("Missing the HDRI Profile, exiting!");
                        return;
                    //If fog mode equals exponential
                    case AmbientSkiesConsts.SystemTypes.AmbientHDRISkies when hdriSky.fogType == AmbientSkiesConsts.VolumeFogType.Exponential:
                        //Set low fog height
                        highestFogHeight += 25f;

                        exponentialBaseFogHeight = highestFogHeight + 50f;
                        hdriSky.exponentialBaseHeight = highestFogHeight + 50f;
                        break;
                    case AmbientSkiesConsts.SystemTypes.AmbientHDRISkies when hdriSky.fogType == AmbientSkiesConsts.VolumeFogType.Linear:
                        //Set low fog height
                        linearFogHeightStart = lowestFogHeight;
                        hdriSky.linearHeightStart = lowestFogHeight;

                        //Set high fog height
                        highestFogHeight += 150f;

                        linearFogHeightEnd = highestFogHeight;
                        hdriSky.linearHeightEnd = highestFogHeight;
                        break;
                    //Active sky mode equals Procedural
                    case AmbientSkiesConsts.SystemTypes.AmbientHDRISkies:
                        //Set low fog height
                        baseFogHeight = lowestFogHeight;
                        hdriSky.volumetricBaseFogHeight = lowestFogHeight;

                        //Set high fog height
                        meanFogHeight = highestFogHeight;
                        hdriSky.volumetricMeanHeight = highestFogHeight;
                        break;
                    case AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies when proceduralSky == null:
                        //If skybox profile is misisng, exit
                        Debug.LogError("Missing the HDRI Profile, exiting!");
                        return;
                    //If fog mode equals exponential
                    case AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies when proceduralSky.fogType == AmbientSkiesConsts.VolumeFogType.Exponential:
                        //Set low fog height
                        highestFogHeight += 25f;

                        exponentialBaseFogHeight = highestFogHeight;
                        proceduralSky.exponentialBaseHeight = highestFogHeight;
                        break;
                    case AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies when proceduralSky.fogType == AmbientSkiesConsts.VolumeFogType.Linear:
                        //Set low fog height
                        linearFogHeightStart = lowestFogHeight;
                        proceduralSky.linearHeightStart = lowestFogHeight;

                        //Set high fog height
                        highestFogHeight += 150f;

                        linearFogHeightEnd = highestFogHeight;
                        proceduralSky.linearHeightEnd = highestFogHeight;
                        break;
                    //Active sky mode equals Gradient
                    case AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies:
                        //Set low fog height
                        baseFogHeight = lowestFogHeight;
                        proceduralSky.volumetricBaseFogHeight = lowestFogHeight;

                        //Set high fog height
                        meanFogHeight = highestFogHeight;
                        proceduralSky.volumetricMeanHeight = highestFogHeight;
                        break;
                    default:
                    {
                        if (gradientSky == null)
                        {
                            //If skybox profile is misisng, exit
                            Debug.LogError("Missing the HDRI Profile, exiting!");
                        }
                        else
                        {
                            switch (gradientSky.fogType)
                            {
                                //If fog mode equals exponential
                                //If fog mode equals linear
                                case AmbientSkiesConsts.VolumeFogType.Exponential:
                                    //Set low fog height
                                    highestFogHeight += 25f;

                                    exponentialBaseFogHeight = highestFogHeight;
                                    gradientSky.exponentialBaseHeight = highestFogHeight;
                                    break;
                                //If fog mode equals volumetric
                                case AmbientSkiesConsts.VolumeFogType.Linear:
                                    //Set low fog height
                                    linearFogHeightStart = lowestFogHeight;
                                    gradientSky.linearHeightStart = lowestFogHeight;

                                    //Set high fog height
                                    highestFogHeight += 150f;

                                    linearFogHeightEnd = highestFogHeight;
                                    gradientSky.linearHeightEnd = highestFogHeight;
                                    break;
                                case AmbientSkiesConsts.VolumeFogType.None:
                                    break;
                                case AmbientSkiesConsts.VolumeFogType.ExponentialSquared:
                                    break;
                                case AmbientSkiesConsts.VolumeFogType.Volumetric:
                                    break;
                                default:
                                    //Set low fog height
                                    baseFogHeight = lowestFogHeight;
                                    gradientSky.volumetricBaseFogHeight = lowestFogHeight;

                                    //Set high fog height
                                    meanFogHeight = highestFogHeight;
                                    gradientSky.volumetricMeanHeight = highestFogHeight;
                                    break;
                            }
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all variables from Ambient Skies and applies them
        /// </summary>
        /// <param name="getAllSettings"></param>
        private void GetAndApplyAllValuesFromAmbientSkies(bool getAllSettings)
        {
            if (m_profiles == null)
            {
                m_profiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>("Ambient Skies Volume 1");
                newProfileListIndex = 0;

                Repaint();
            }

            //If true proceed
            if (getAllSettings)
            {
                //Apply Settings
                if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                {
                    SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
                }

                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);

                //Udpate lightmap settings                   
                LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);
            }
            else
            {
                //Apply Settings
                if (systemtype != AmbientSkiesConsts.SystemTypes.ThirdParty)
                {
                    SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);
                }

                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);

                //Udpate lightmap settings                   
                LightingUtils.SetFromProfileIndex(m_profiles, m_selectedLightingProfileIndex, false, m_profiles.m_updateRealtime, m_profiles.m_updateBaked);
            }
        }

        #endregion

        #region Creation Mode

        /// <summary>
        /// Create new system profile
        /// </summary>
        private void CreateNewProfile()
        {
            //Create the profile
            AmbientSkyProfiles asset = ScriptableObject.CreateInstance<AmbientSkyProfiles>();
            AssetDatabase.CreateAsset(asset, "Assets/" + m_newSystemName + ".asset");

            //Update settings
            asset.m_timeOfDayProfile = m_profiles.m_timeOfDayProfile;
            asset.m_selectedRenderPipeline = renderPipelineSettings;
            AmbientSkyboxProfile newHDRI = new AmbientSkyboxProfile();
            asset.m_skyProfiles.Add(newHDRI);
            asset.m_skyProfiles[0].name = "New HDRI Skybox";
            AmbientProceduralSkyboxProfile newProcedural = new AmbientProceduralSkyboxProfile();
            asset.m_proceduralSkyProfiles.Add(newProcedural);
            asset.m_proceduralSkyProfiles[0].name = "New Procedural Skybox";
            AmbientGradientSkyboxProfile newGradient = new AmbientGradientSkyboxProfile();
            asset.m_gradientSkyProfiles.Add(newGradient);
            asset.m_gradientSkyProfiles[0].name = "New Gradient Skybox";
            AmbientPostProcessingProfile newPostFx = new AmbientPostProcessingProfile();
            asset.m_ppProfiles.Add(newPostFx);
            asset.m_ppProfiles[0].name = "New Post Processing Profile";
            AmbientLightingProfile newLighting = new AmbientLightingProfile();
            asset.m_lightingProfiles.Add(newLighting);
            asset.m_lightingProfiles[0].name = "New Lighting Profile";
            asset.m_CreationToolSettings = m_creationToolSettings;
            asset.m_editSettings = true;
            asset.m_isProceduralCreatedProfile = false;

            //m_selectedSkyboxProfile = asset.m_skyProfiles[0];
            //m_selectedProceduralSkyboxProfile = asset.m_proceduralSkyProfiles[0];
            //m_selectedGradientSkyboxProfile = asset.m_gradientSkyProfiles[0];
            //m_selectedPostProcessingProfile = asset.m_ppProfiles[0];
            //m_selectedLightingProfile = asset.m_lightingProfiles[0];

            m_profileList = AmbientSkiesEditorFunctions.GetAllSkyProfilesProjectSearch("t:AmbientSkyProfiles");

            //Add global profile names
            profileChoices.Clear();
            foreach (var profile in m_profileList)
            {
                profileChoices.Add(profile.name);
            }

            newPPSelection = 0;
            newSkyboxSelection = 0;
            newProceduralSkyboxSelection = 0;
            newGradientSkyboxSelection = 0;
            newLightmappingSettings = 0;

            m_selectedPostProcessingProfileIndex = newPPSelection;
            m_selectedSkyboxProfileIndex = newSkyboxSelection;
            m_selectedProceduralSkyboxProfileIndex = newProceduralSkyboxSelection;
            m_selectedGradientSkyboxProfileIndex = newGradientSkyboxSelection;
            m_selectedLightingProfileIndex = newLightmappingSettings;

            m_creationToolSettings.m_selectedHDRI = m_selectedSkyboxProfileIndex;
            m_creationToolSettings.m_selectedProcedural = m_selectedSkyboxProfileIndex;
            m_creationToolSettings.m_selectedGradient = m_selectedSkyboxProfileIndex;
            m_creationToolSettings.m_selectedPostProcessing = m_selectedSkyboxProfileIndex;
            m_creationToolSettings.m_selectedLighting = m_selectedSkyboxProfileIndex;

            EditorPrefs.SetString("AmbientSkiesActiveProfile_", asset.name);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            if (m_multiScenePresent)
            {
                if (m_savedSettings == null)
                {
                    m_savedSettings = AmbientSkiesEditorFunctions.GetSavedSettingsComponent("Ambient Skies Global", false);
                }

                if (m_savedSettings != null)
                {
                    ApplyCreationToolSettings(m_hasChanged);
                    m_savedSettings.SetSettings(asset, m_creationToolSettings);
                }
            }

            AmbientSkyProfiles skyProfiles = null;
            for (int i = 0; i < m_profileList.Count; i++)
            {
                if (m_profileList[i].name == asset.name)
                {
                    m_creationToolSettings.m_selectedSystem = i;
                    m_profileListIndex = m_creationToolSettings.m_selectedSystem;
                    newProfileListIndex = m_profileListIndex;
                    skyProfiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath(m_profileList[i].name));
                    break;
                }
            }

            if (skyProfiles != null)
            {
                m_profiles = skyProfiles;
            }

            EditorUtility.SetDirty(m_creationToolSettings);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            //EditorUtility.DisplayDialog("Successful!", "Profile was created succesfully and the active profiles have been used to create your first profile index. Ambient Skies will now close, you'll need to reopen the Ambient Skies Window", "Ok");
            //Close();
        }

        /// <summary>
        /// Repairs the profile if index errors
        /// </summary>
        private void RepairProfileIndex()
        {
            m_selectedSkyboxProfileIndex = 0;
            m_selectedProceduralSkyboxProfileIndex = 0;
            m_selectedGradientSkyboxProfileIndex = 0;
            m_selectedPostProcessingProfileIndex = 0;
            m_selectedLightingProfileIndex = 0;

            newSkyboxSelection = 0;
            newProceduralSkyboxSelection = 0;
            newGradientSkyboxSelection = 0;
            newPPSelection = 0;
            newLightmappingSettings = 0;

            m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];
            m_selectedProceduralSkyboxProfile = m_profiles.m_proceduralSkyProfiles[m_selectedProceduralSkyboxProfileIndex];
            m_selectedGradientSkyboxProfile = m_profiles.m_gradientSkyProfiles[m_selectedGradientSkyboxProfileIndex];
            m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
            m_selectedLightingProfile = m_profiles.m_lightingProfiles[m_selectedLightingProfileIndex];

            EditorUtility.DisplayDialog("Completed!", "Profile Repaired, All index's are reset to 0. So each tab profile will be set back to the first profile. Ambient Skies will now close. Please reopen ambient skies Window", "Ok");

            Close();
        }

        /// <summary>
        /// Removes the profile
        /// </summary>
        private void RemoveCreatedProfile()
        {
            AmbientSkyProfiles newProfile = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath(m_newSystemName));
            if (newProfile == null) return;
            profileChoices.Remove(newProfile.name);
            DestroyImmediate(newProfile);

            EditorPrefs.SetString("AmbientSkiesActiveProfile_", "Ambient Skies Volume 1");
        }

        /// <summary>
        /// Deletes the active profile
        /// </summary>
        private void DeleteCreatedProfile()
        {
            if (!m_profiles.m_isProceduralCreatedProfile)
            {
                m_profileListIndex = 0;
                newProfileListIndex = 0;

                AssetDatabase.DeleteAsset(SkyboxUtils.GetAssetPath(m_profiles.name));
                AssetDatabase.SaveAssets();

                m_profileList = AmbientSkiesEditorFunctions.GetAllSkyProfilesProjectSearch("t:AmbientSkyProfiles");
                //Add global profile names
                profileChoices.Clear();
                foreach (var profile in m_profileList)
                {
                    profileChoices.Add(profile.name);
                }

                newPPSelection = 0;
                newSkyboxSelection = 0;
                newProceduralSkyboxSelection = 0;
                newGradientSkyboxSelection = 0;
                newLightmappingSettings = 0;

                m_selectedPostProcessingProfileIndex = newPPSelection;
                m_selectedSkyboxProfileIndex = newSkyboxSelection;
                m_selectedProceduralSkyboxProfileIndex = newProceduralSkyboxSelection;
                m_selectedGradientSkyboxProfileIndex = newGradientSkyboxSelection;
                m_selectedLightingProfileIndex = newLightmappingSettings;

                m_creationToolSettings.m_selectedHDRI = m_selectedSkyboxProfileIndex;
                m_creationToolSettings.m_selectedProcedural = m_selectedSkyboxProfileIndex;
                m_creationToolSettings.m_selectedGradient = m_selectedSkyboxProfileIndex;
                m_creationToolSettings.m_selectedPostProcessing = m_selectedSkyboxProfileIndex;
                m_creationToolSettings.m_selectedLighting = m_selectedSkyboxProfileIndex;

                AmbientSkyProfiles skyProfiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath(m_profileList[newProfileListIndex].name));
                if (skyProfiles != null)
                {
                    m_profiles = skyProfiles;
                }
                if (m_multiScenePresent)
                {
                    if (m_savedSettings == null)
                    {
                        m_savedSettings = AmbientSkiesEditorFunctions.GetSavedSettingsComponent("Ambient Skies Global", false);
                    }

                    if (m_savedSettings != null)
                    {
                        m_hasChanged = true;
                        ApplyCreationToolSettings(m_hasChanged);
                        m_savedSettings.SetSettings(m_profiles, m_creationToolSettings);
                    }
                }

                EditorUtility.DisplayDialog("Completed!", "Profile successfully deleted.", "Ok");
            }
        }

        /// <summary>
        /// Creation mode foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        /// <param name="profileMode"></param>
        private void CreationSkyboxMode(bool helpEnabled)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            EditorGUI.BeginChangeCheck();

            m_editorUtils.Label("Creation Mode Settings");

            m_skiesProfileName = m_editorUtils.TextField("CreationProfileName", m_skiesProfileName, helpEnabled);
            if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
            {
                m_hdriAssetName = (Cubemap)m_editorUtils.ObjectField("CreationHDRISkyboxAssetName", m_hdriAssetName, typeof(Cubemap), false, helpEnabled, GUILayout.Height(16f));

                EditorGUILayout.BeginHorizontal();

                m_creationMatchPostProcessing = m_selectedSkyboxProfile.creationMatchPostProcessing;
                m_editorUtils.Text("CreationPostProcessingAssetName", GUILayout.Width(146f));
                m_creationMatchPostProcessing = EditorGUILayout.Popup(m_selectedSkyboxProfile.creationMatchPostProcessing, ppChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));

                EditorGUILayout.EndHorizontal();
            }
            else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
            {
                EditorGUILayout.BeginHorizontal();

                m_editorUtils.Text("CreationPostProcessingAssetName", GUILayout.Width(146f));
                m_creationMatchPostProcessing = EditorGUILayout.Popup(m_selectedProceduralSkyboxProfile.creationMatchPostProcessing, ppChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                m_editorUtils.Text("CreationPostProcessingAssetName", GUILayout.Width(146f));
                m_creationMatchPostProcessing = EditorGUILayout.Popup(m_selectedGradientSkyboxProfile.creationMatchPostProcessing, ppChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));

                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    m_selectedSkyboxProfile.name = m_skiesProfileName;
                    m_selectedSkyboxProfile.creationHDRIAsset = m_hdriAssetName;
                    if (m_hdriAssetName != null)
                    {
                        m_selectedSkyboxProfile.assetName = m_hdriAssetName.name;
                    }

                    m_selectedSkyboxProfile.creationMatchPostProcessing = m_creationMatchPostProcessing;
                    if (m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile != null)
                    {
                        m_selectedSkyboxProfile.postProcessingAssetName = m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile.name;
                    }

                    //Add skies profile names
                    skyboxChoices.Clear();
                    foreach (var profile in m_profiles.m_skyProfiles)
                    {
                        skyboxChoices.Add(profile.name);
                    }
                }
                else if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    m_selectedProceduralSkyboxProfile.name = m_skiesProfileName;
                    m_selectedProceduralSkyboxProfile.creationMatchPostProcessing = m_creationMatchPostProcessing;
                    m_selectedProceduralSkyboxProfile.postProcessingAssetName = m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile.name;

                    //Add procedural skies profile names
                    proceduralSkyboxChoices.Clear();
                    foreach (var profile in m_profiles.m_proceduralSkyProfiles)
                    {
                        proceduralSkyboxChoices.Add(profile.name);
                    }
                }
                else
                {
                    m_selectedGradientSkyboxProfile.name = m_skiesProfileName;
                    m_selectedGradientSkyboxProfile.creationMatchPostProcessing = m_creationMatchPostProcessing;
                    m_selectedGradientSkyboxProfile.postProcessingAssetName = m_profiles.m_ppProfiles[m_creationMatchPostProcessing].creationPostProcessProfile.name;

#if UNITY_2018_3_OR_NEWER
                    //Add gradient skies profile names
                    gradientSkyboxChoices.Clear();
                    foreach (var profile in m_profiles.m_gradientSkyProfiles)
                    {
                        gradientSkyboxChoices.Add(profile.name);
                    }
#endif
                }

                AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToTrue(m_profiles);

                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, m_selectedProceduralSkyboxProfileIndex, m_selectedGradientSkyboxProfileIndex, false, m_profiles.m_updateVisualEnvironment, m_profiles.m_updateFog, m_profiles.m_updateShadows, m_profiles.m_updateAmbientLight, m_profiles.m_updateScreenSpaceReflections, m_profiles.m_updateScreenSpaceRefractions, m_profiles.m_updateSun);

                AmbientSkiesEditorFunctions.SetAllEnvironmentUpdateToFalse(m_profiles);
            }
            #endif
        }

        /// <summary>
        /// Creation mode foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        /// <param name="profileMode"></param>
        private void CreationPostProcessMode(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
#if UNITY_POST_PROCESSING_STACK_V2
            m_editorUtils.Label("Creation Mode Settings");

            m_postProfileName = m_editorUtils.TextField("CreationProfileName", m_postProfileName, helpEnabled);
            m_postProcessAssetName = (PostProcessProfile)m_editorUtils.ObjectField("CreationPostProcessAssetName", m_postProcessAssetName, typeof(PostProcessProfile), false, helpEnabled, GUILayout.Height(16f));

            if (EditorGUI.EndChangeCheck())
            {
                m_selectedPostProcessingProfile.name = m_postProfileName;
                m_selectedPostProcessingProfile.creationPostProcessProfile = m_postProcessAssetName;
                if (m_selectedPostProcessingProfile != null)
                {
                    m_selectedPostProcessingProfile.assetName = m_postProcessAssetName.name;
                }

                //Add post processing profile names
                ppChoices.Clear();
                foreach (var profile in m_profiles.m_ppProfiles)
                {
                    ppChoices.Add(profile.name);
                }
            }
#endif
            if (EditorGUI.EndChangeCheck())
            {
                //Update post processing
                AmbientSkiesEditorFunctions.SetAllPostFxUpdateToTrue(m_profiles);

                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedPostProcessingProfileIndex, false, m_profiles.m_updateAO, m_profiles.m_updateAutoExposure, m_profiles.m_updateBloom, m_profiles.m_updateChromatic, m_profiles.m_updateColorGrading, m_profiles.m_updateDOF, m_profiles.m_updateGrain, m_profiles.m_updateLensDistortion, m_profiles.m_updateMotionBlur, m_profiles.m_updateSSR, m_profiles.m_updateVignette, m_profiles.m_updatePanini, m_profiles.m_updateTargetPlaform);

                EditorPrefs.SetString("AmbientSkiesPostProcessing_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_ppProfiles[newPPSelection].name);

                AmbientSkiesEditorFunctions.SetAllPostFxUpdateToFalse(m_profiles);
            }
        }

        /// <summary>
        /// Creation mode foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        /// <param name="profileMode"></param>
        private void CreationLightingMode(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            m_editorUtils.Label("Creation Mode Settings");

            m_lightProfileName = m_editorUtils.TextField("CreationProfileName", m_lightProfileName, helpEnabled);

            if (EditorGUI.EndChangeCheck())
            {
                m_selectedLightingProfile.name = m_lightProfileName;

                //Add lightmaps profile names
                lightmappingChoices.Clear();
                foreach (var profile in m_profiles.m_lightingProfiles)
                {
                    lightmappingChoices.Add(profile.name);
                }
            }
        }

        #endregion

        #endregion
    }
}