//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.Rendering;
#if HDPipeline && !UNITY_2019_3_OR_NEWER
using UnityEngine.Experimental.Rendering.HDPipeline;
#elif HDPipeline && UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.HighDefinition;
#endif
#if LWPipeline && UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering.LWRP;
#elif LWPipeline && UNITY_2018_3_OR_NEWER
using UnityEngine.Experimental.Rendering.LightweightPipeline;
#endif

#if CTS_PRESENT
using CTS;
#endif
#if GAIA_2_PRESENT
using Gaia;
#endif

namespace AmbientSkies
{
    public static class AmbientSkiesPipelineUtils
    {
        #region Utils

        #region Variables

        //Get parent object
        public static GameObject parentObject;
        //Get volume object
        public static GameObject volumeObject;
        //Active terrain
        public static Terrain terrain;
        //Density volume object
        public static GameObject densityFogVolume;
        //The main light object
        public static GameObject directionalLight;
        //The sun light
        public static Light sunLight;
        //Camera object
        public static GameObject mainCamera;

#if HDPipeline && UNITY_2018_3_OR_NEWER
        //The volume object
        public static Volume volumeSettings;
        //The volume profile that contains the settings
        public static VolumeProfile volumeProfile;
        //The density volume settings
        public static DensityVolume density;  
        //HDRP Light data
        public static HDAdditionalLightData hDAdditionalLightData;
#if !UNITY_2019_3_OR_NEWER
        //HDRP Shadow data
        public static AdditionalShadowData shadowData;  
#endif
        //HDRP Camera data
        public static HDAdditionalCameraData hdCamData;
#endif

#if HDPipeline && UNITY_2019_1 || HDPipeline && UNITY_2019_2
        //Ambient lighting settings
        public static StaticLightingSky bakingSkySettings;
#elif HDPipeline && !UNITY_2019_1_OR_NEWER
        //Ambient lighting settings
        public static BakingSky bakingSkySettings;
#endif

        #endregion

        #region Volume and Profile Updates

        /// <summary>
        /// Applies and creates the volume settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="renderPipelineSettings"></param>
        /// <param name="volumeName"></param>
        public static void SetupHDEnvironmentalVolume(AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, AmbientSkyProfiles skyProfiles, int profileIdx, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings, string volumeName, string hdVolumeProfile, bool updateVisualEnvironment, bool updateFog, bool updateShadows, bool updateAmbientLight, bool updateScreenReflections, bool updateScreenRefractions, bool updateSun)
        {
            //Get parent object
            if (parentObject == null)
            {
                parentObject = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Global", false);
            }

            //Get volume object
            if (volumeObject == null)
            {
                volumeObject = GameObject.Find(volumeName);
            }

            bool useTimeOfDay = false;
            if (skyProfiles.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
            {
                useTimeOfDay = true;
                if (useTimeOfDay)
                {
                    //removes warning
                }
            }

            //Apply only if system type is Ambient Skies
            if (skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.DefaultProcedural || skyProfiles.m_systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
            {
                //High Definition
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    RemoveOldHDRPObjects();

                    AddRequiredComponents();

                    if (skyProfiles.m_useSkies)
                    {
#if HDPipeline && UNITY_2018_3_OR_NEWER

                        SetHDCamera();

                        //Set Sun rotation
                        SkyboxUtils.SetSkyboxRotation(skyProfiles, profile, proceduralProfile, gradientProfile, renderPipelineSettings);

                        if (volumeObject == null)
                        {
                            volumeObject = SetupEnvironmentHDRPVolumeObject(volumeName, parentObject, volumeObject);
                        }

                        if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies || skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies || skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                        {
                            if (volumeSettings == null)
                            {
                                volumeSettings = SetVolumeSetup(volumeName);
                            }

                            if (skyProfiles.m_useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
                            {
                                if (!string.IsNullOrEmpty(hdVolumeProfile))
                                {
                                    if (volumeProfile == null)
                                    {
                                        volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                        volumeProfile = volumeSettings.sharedProfile;
                                    }
                                    else
                                    {
                                        if (volumeSettings.sharedProfile.name != hdVolumeProfile)
                                        {
                                            volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                            volumeProfile = volumeSettings.sharedProfile;
                                        }
                                    }

                                    if (volumeProfile != null)
                                    {
                                        EditorUtility.SetDirty(volumeProfile);

                                        //Visual Enviro
                                        if (updateVisualEnvironment)
                                        {
                                            ApplyVisualEnvironment(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile, parentObject, useTimeOfDay);
                                        }

                                        //Shadows
                                        if (updateShadows)
                                        {
                                            //HD Shadows
                                            ApplyHDShadowSettings(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

                                            //Contact Shadows
                                            ApplyContactShadows(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

                                            //Micro Shadows
                                            ApplyMicroShadowing(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }

                                        //Screen Space Reflection
                                        if (updateScreenReflections)
                                        {
                                            ApplyScreenSpaceReflection(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }

                                        //Screen Space Refraction
                                        if (updateScreenRefractions)
                                        {
                                            ApplyScreenSpaceRefraction(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(hdVolumeProfile))
                                {
                                    if (volumeProfile == null)
                                    {
                                        volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                        volumeProfile = volumeSettings.sharedProfile;
                                    }
                                    else
                                    {
                                        if (volumeSettings.sharedProfile != null)
                                        {
                                            if (volumeSettings.sharedProfile.name != hdVolumeProfile)
                                            {
                                                volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                                volumeProfile = volumeSettings.sharedProfile;
                                            }
                                        }
                                        else
                                        {
                                            volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                            volumeProfile = volumeSettings.sharedProfile;
                                        }
                                    }

                                    if (volumeProfile != null)
                                    {
                                        EditorUtility.SetDirty(volumeProfile);

                                        //Visual Enviro
                                        if (updateVisualEnvironment)
                                        {
                                            if (skyProfiles.m_showFunctionDebugsOnly)
                                            {
                                                Debug.Log("Updating ApplyVisualEnvironment()");
                                            }

                                            ApplyVisualEnvironment(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile, parentObject, useTimeOfDay);
                                        }

                                        //Shadows
                                        if (updateShadows)
                                        {
                                            if (skyProfiles.m_showFunctionDebugsOnly)
                                            {
                                                Debug.Log("Updating ApplyHDShadowSettings() + ApplyContactShadows() + ApplyMicroShadowing()");
                                            }

                                            //HD Shadows
                                            ApplyHDShadowSettings(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

                                            //Contact Shadows
                                            ApplyContactShadows(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

                                            //Micro Shadows
                                            ApplyMicroShadowing(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }

                                        //Fog
                                        if (updateFog)
                                        {
                                            if (skyProfiles.m_showFunctionDebugsOnly)
                                            {
                                                Debug.Log("Updating ApplyFogSettings()");
                                            }

                                            //Fog
                                            ApplyFogSettings(parentObject, volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                            //Volumetric Light
                                            updateSun = true;
                                        }

                                        //Screen Space Reflection
                                        if (updateScreenReflections)
                                        {
                                            if (skyProfiles.m_showFunctionDebugsOnly)
                                            {
                                                Debug.Log("Updating ApplyScreenSpaceReflection()");
                                            }

                                            ApplyScreenSpaceReflection(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }

                                        //Screen Space Refraction
                                        if (updateScreenRefractions)
                                        {
                                            if (skyProfiles.m_showFunctionDebugsOnly)
                                            {
                                                Debug.Log("Updating ApplyScreenSpaceRefraction()");
                                            }

                                            ApplyScreenSpaceRefraction(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                                        }
                                    }
                                }
                            }

                            if (updateSun)
                            {
                                if (skyProfiles.m_showFunctionDebugsOnly)
                                {
                                    Debug.Log("Updating ApplyDirectionalLightSettings()");
                                }

                                ApplyDirectionalLightSettings(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                            }
                        }
                        else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            Volume volumeSettings = volumeObject.GetComponent<Volume>();
                            if (volumeSettings != null)
                            {
                                Object.DestroyImmediate(volumeSettings);
                            }
                        }
                        else
                        {
                            if (volumeSettings == null)
                            {
                                volumeSettings = SetVolumeSetup(volumeName);
                            }

                            if (volumeSettings.sharedProfile.name != hdVolumeProfile)
                            {
                                volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                volumeProfile = volumeSettings.sharedProfile;
                            }

                            if (volumeProfile != null)
                            {
                                EditorUtility.SetDirty(volumeProfile);

                                DefaultProceduralSkySetup(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile, parentObject, volumeName, hdVolumeProfile);
                            }
                        }

                        if (updateAmbientLight)
                        {
                            if (skyProfiles.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating ApplyHDRPStaticLighting() + ApplyIndirectLightingController()");
                            }

                            ApplyIndirectLightingController(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
                        }
#endif
                    }
                }
                else
                {
                    GameObject environmentVolume = GameObject.Find(volumeName);
                    if (environmentVolume != null)
                    {
                        Object.DestroyImmediate(environmentVolume);
                    }

                    SkyboxUtils.DestroyParent("Ambient Skies Global", skyProfiles.m_multiSceneSupport);
                }
            }
            else
            {
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    //Hd Pipeline Volume Setup
                    volumeObject = GameObject.Find(volumeName);
                    if (volumeObject == null)
                    {
                        volumeObject = new GameObject(volumeName);
                        volumeObject.layer = LayerMask.NameToLayer("TransparentFX");
                        volumeObject.transform.SetParent(parentObject.transform);
                    }
                    else
                    {
                        volumeObject.layer = LayerMask.NameToLayer("TransparentFX");
                        volumeObject.transform.SetParent(parentObject.transform);
                    }
                }
                else
                {
                    //Removes the object
                    volumeObject = GameObject.Find(volumeName);
                    if (volumeObject != null)
                    {
                        Object.DestroyImmediate(volumeObject);
                    }

                    SkyboxUtils.DestroyParent("Ambient Skies Global", skyProfiles.m_multiSceneSupport);
                }
            }
        }

        #region Apply Settings

        /// <summary>
        /// Apply default sky system type
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        /// <param name="parentObject"></param>
#if HDPipeline
        public static void DefaultProceduralSkySetup(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, GameObject parentObject, string volumeName, string hdVolumeProfile)
        {
            //Visual Enviro
            ProceduralSkyVisualEnvironment(volumeProfile, proceduralProfile, parentObject);

            //HD Shadows
            ApplyHDShadowSettings(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Contact Shadows
            ApplyContactShadows(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Micro Shadows
            ApplyMicroShadowing(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Indirect Lighting
            ApplyIndirectLightingController(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Screen Space Reflection
            ApplyScreenSpaceReflection(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);

            //Screen Space Refraction
            ApplyScreenSpaceRefraction(volumeProfile, skyProfiles, profile, proceduralProfile, gradientProfile);
        }

        /// <summary>
        /// Sets up the procedural sky for default sky type
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        /// <param name="parentObject"></param>
        public static void ProceduralSkyVisualEnvironment(VolumeProfile volumeProfile, AmbientProceduralSkyboxProfile profile, GameObject parentObject)
        {
            //Visual Enviro
            VisualEnvironment visualEnvironmentSettings;
            if (volumeProfile.TryGet(out visualEnvironmentSettings))
            {
                PhysicallyBasedSky physicallyBasedSky;
                if (volumeProfile.TryGet(out physicallyBasedSky))
                {
                    physicallyBasedSky.active = true;
                    //Planet
                    physicallyBasedSky.earthPreset.value = true;
                    physicallyBasedSky.sphericalMode.value = true;
                    physicallyBasedSky.planetCenterPosition.value = new Vector3(0f, -6378100f, 0f);
                    physicallyBasedSky.planetRotation.value = new Vector3(0f, 0f, 0f);
                    physicallyBasedSky.groundColorTexture.value = null;
                    physicallyBasedSky.groundTint.value = SkyboxUtils.GetColorFromHTML("41596F");
                    physicallyBasedSky.groundEmissionTexture.value = null;
                    physicallyBasedSky.groundEmissionMultiplier.value = 1f;
                    //Space
                    physicallyBasedSky.spaceRotation.value = new Vector3(0f, 0f, 0f);
                    physicallyBasedSky.spaceEmissionTexture.value = null;
                    physicallyBasedSky.spaceEmissionMultiplier.value = 1f;
                    //Areosols
                    physicallyBasedSky.aerosolMaximumAltitude.value = 8289.296f;
                    physicallyBasedSky.aerosolDensity.value = 0.01192826f;
                    physicallyBasedSky.aerosolTint.value = SkyboxUtils.GetColorFromHTML("E6E6E6");
                    physicallyBasedSky.aerosolAnisotropy.value = 0f;
                    //Artistic Overrides
                    physicallyBasedSky.colorSaturation.value = 1f;
                    physicallyBasedSky.alphaSaturation.value = 1f;
                    physicallyBasedSky.alphaMultiplier.value = 1f;
                    physicallyBasedSky.horizonTint.value = SkyboxUtils.GetColorFromHTML("FFFFFF");
                    physicallyBasedSky.horizonZenithShift.value = 0f;
                    physicallyBasedSky.zenithTint.value = SkyboxUtils.GetColorFromHTML("FFFFFF");
                    //Miscellaneous
                    physicallyBasedSky.numberOfBounces.value = 8;
                    physicallyBasedSky.skyIntensityMode.value = SkyIntensityMode.Multiplier;
                    physicallyBasedSky.multiplier.value = 1.5f;
                    physicallyBasedSky.updateMode.value = EnvironmentUpdateMode.OnChanged;
                    physicallyBasedSky.includeSunInBaking.value = true;
                }

                Fog fog;
                if (volumeProfile.TryGet(out fog))
                {
                    fog.active = true;
                    fog.enableVolumetricFog.value = true;
                    fog.enabled.value = true;
                    fog.meanFreePath.value = 250f;
                    fog.baseHeight.value = 100f;
                    fog.maximumHeight.value = 250f;
                    fog.maxFogDistance.value = 5000f;
                    fog.colorMode.value = FogColorMode.SkyColor;
                    fog.enableVolumetricFog.value = true;
                    fog.albedo.value = SkyboxUtils.GetColorFromHTML("FFFFFF");
                    fog.anisotropy.value = 0.7f;
                    fog.globalLightProbeDimmer.value = 1f;
                    fog.depthExtent.value = 20f;
                }

                if (terrain == null)
                {
                    terrain = Terrain.activeTerrain;
                }

                densityFogVolume = GameObject.Find("Density Volume");
                if (densityFogVolume == null)
                {
                    densityFogVolume = new GameObject("Density Volume");
                    densityFogVolume.transform.SetParent(parentObject.transform);
                }

                density = densityFogVolume.AddComponent<DensityVolume>();
                density.parameters.albedo = profile.singleScatteringAlbedo;
                density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                density.parameters.volumeMask = profile.fogDensityMaskTexture;
                density.parameters.textureTiling = profile.densityMaskTiling;
                density.parameters.size = new Vector3(10000f, 10000f, 10000f);

                density = densityFogVolume.GetComponent<DensityVolume>();
                if (density == null)
                {
                    density = densityFogVolume.AddComponent<DensityVolume>();
                }

                density.parameters.albedo = profile.singleScatteringAlbedo;
                density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                density.parameters.volumeMask = profile.fogDensityMaskTexture;
                density.parameters.textureTiling = profile.densityMaskTiling;
                density.parameters.size = new Vector3(10000f, 10000f, 10000f);
            }
        }
#endif

        #region Apply Volume Settings

#if HDPipeline
        /// <summary>
        /// Applies the visual environent settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        /// <param name="parentObject"></param>
        public static void ApplyVisualEnvironment(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile, GameObject parentObject, bool timeOfDayEnabled)
        {
            if (skyProfiles.m_showDebug)
            {
                Debug.Log("Time Of Day enabel status is set to " + timeOfDayEnabled);
            }
            //Visual Enviro
            VisualEnvironment visualEnvironmentSettings;
            if (timeOfDayEnabled)
            {
                if (volumeProfile.TryGet(out visualEnvironmentSettings))
                {
                    visualEnvironmentSettings.skyType.value = 4;

                    PhysicallyBasedSky physicallyBasedSky;
                    if (volumeProfile.TryGet(out physicallyBasedSky))
                    {
                        physicallyBasedSky.active = true;
                        physicallyBasedSky.includeSunInBaking.value = true;
                    }

                    HDRISky hDRISky;
                    if (volumeProfile.TryGet(out hDRISky))
                    {
                        hDRISky.active = false;
                    }

                    GradientSky gradientSkyV;
                    if (volumeProfile.TryGet(out gradientSkyV))
                    {
                        gradientSkyV.active = false;
                    }

                    if (profile.fogType == AmbientSkiesConsts.VolumeFogType.None)
                    {
                        UnityEngine.Rendering.HighDefinition.Fog fog;
                        if (volumeProfile.TryGet(out fog))
                        {
                            fog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                    else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                    {
                        UnityEngine.Rendering.HighDefinition.Fog fog;
                        if (volumeProfile.TryGet(out fog))
                        {
                            fog.active = true;
                            fog.albedo.value = profile.fogColor;
                            fog.color.value = profile.fogColor;
                            fog.meanFreePath.value = profile.volumetricBaseFogDistance;
                            fog.baseHeight.value = profile.volumetricBaseFogHeight;
                            fog.maximumHeight.value = profile.volumetricMeanHeight;
                            fog.anisotropy.value = profile.volumetricGlobalAnisotropy;
                            fog.globalLightProbeDimmer.value = profile.volumetricGlobalLightProbeDimmer;
                            fog.maxFogDistance.value = profile.fogDistance;
                            fog.colorMode.value = profile.volumetricFogColorMode;
                            fog.mipFogNear.value = profile.nearFogDistance;
                            fog.mipFogFar.value = profile.fogDistance;
                            fog.mipFogMaxMip.value = profile.volumetricMipFogMaxMip;
                        }

                        if (profile.useFogDensityVolume)
                        {
                            Terrain terrain = Terrain.activeTerrain;
                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                            if (densityFogVolume == null)
                            {
                                densityFogVolume = new GameObject("Density Volume");
                                densityFogVolume.transform.SetParent(parentObject.transform);

                                DensityVolume density = densityFogVolume.AddComponent<DensityVolume>();
                                density.parameters.albedo = profile.singleScatteringAlbedo;
                                density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                density.parameters.textureTiling = profile.densityMaskTiling;

                                if (terrain != null)
                                {
                                    density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                }
                                else
                                {
                                    density.parameters.size = new Vector3(2000f, 400f, 2000f);
                                }
                            }
                            else
                            {
                                DensityVolume density = densityFogVolume.GetComponent<DensityVolume>();
                                if (density != null)
                                {
                                    density.parameters.albedo = profile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = profile.densityMaskTiling;
                                }

                                if (terrain != null)
                                {
                                    density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                }
                                else
                                {
                                    density.parameters.size = new Vector3(2000f, 400f, 2000f);
                                }
                            }
                        }
                        else
                        {
                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                            if (densityFogVolume != null)
                            {
                                Object.DestroyImmediate(densityFogVolume);
                            }
                        }
                    }
                }
            }
            else
            {
                if (volumeProfile.TryGet(out visualEnvironmentSettings))
                {
                    HDRISky hDRISky;
                    GradientSky gradientSky;
                    PhysicallyBasedSky physicallyBasedSky;
                    if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                    {
                        visualEnvironmentSettings.skyType.value = 1;


                        if (volumeProfile.TryGet(out hDRISky))
                        {
                            hDRISky.active = true;
                            if (!profile.isPWProfile)
                            {
                                hDRISky.hdriSky.value = profile.customSkybox;
                            }
                            else
                            {
                                hDRISky.hdriSky.value = profile.hDRISkybox;
                            }
                            hDRISky.skyIntensityMode.value = profile.hDRISkyIntensityMode;
                            hDRISky.exposure.value = profile.skyboxExposure;
                            hDRISky.multiplier.value = profile.skyMultiplier;
                            hDRISky.rotation.value = profile.skyboxRotation;

                            switch(profile.hDRIUpdateMode)
                            {
                                case AmbientSkiesConsts.EnvironementSkyUpdateMode.OnChanged:
                                    hDRISky.updateMode.value = EnvironmentUpdateMode.OnChanged;
                                    break;
                                case AmbientSkiesConsts.EnvironementSkyUpdateMode.OnDemand:
                                    hDRISky.updateMode.value = EnvironmentUpdateMode.OnDemand;
                                    break;
                                case AmbientSkiesConsts.EnvironementSkyUpdateMode.Realtime:
                                    hDRISky.updateMode.value = EnvironmentUpdateMode.Realtime;
                                    break;
                            }
                        }

                        if (volumeProfile.TryGet(out gradientSky))
                        {
                            gradientSky.active = false;
                        }

                        if (volumeProfile.TryGet(out physicallyBasedSky))
                        {
                            physicallyBasedSky.active = false;
                        }
                    }
                    else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                    {
                        visualEnvironmentSettings.skyType.value = 4;

                        if (volumeProfile.TryGet(out hDRISky))
                        {
                            hDRISky.active = false;
                        }

                        if (volumeProfile.TryGet(out gradientSky))
                        {
                            gradientSky.active = false;
                        }

                        if (volumeProfile.TryGet(out physicallyBasedSky))
                        {
                            physicallyBasedSky.active = true;
                            //Earth
                            physicallyBasedSky.earthPreset.value = proceduralProfile.pBSEarthPreset;
                            physicallyBasedSky.sphericalMode.value = proceduralProfile.pBSSphericalMode;
                            physicallyBasedSky.planetCenterPosition.value = proceduralProfile.pBSPlanetCenterPosition;
                            physicallyBasedSky.planetRotation.value = proceduralProfile.pBSPlanetRotation;
                            physicallyBasedSky.groundColorTexture.value = proceduralProfile.pBSGroundColorTexture;
                            physicallyBasedSky.groundTint.value = proceduralProfile.pBSGroundTint;
                            physicallyBasedSky.groundEmissionTexture.value = proceduralProfile.pBSGroundEmissionTexture;
                            physicallyBasedSky.groundEmissionMultiplier.value = proceduralProfile.pBSGroundEmissionMultiplier;
                            //Space
                            physicallyBasedSky.spaceRotation.value = proceduralProfile.pBSSpaceRotation;
                            physicallyBasedSky.spaceEmissionTexture.value = proceduralProfile.pBSSpaceEmission;
                            physicallyBasedSky.spaceEmissionMultiplier.value = proceduralProfile.pBSSpaceEmissionMultiplier;
                            //Air
                            physicallyBasedSky.airMaximumAltitude.value = proceduralProfile.pBSAirMaximumAltitude;
                            physicallyBasedSky.airDensityR.value = proceduralProfile.pBSAirDensityR;
                            physicallyBasedSky.airDensityG.value = proceduralProfile.pBSAirDensityG;
                            physicallyBasedSky.airDensityB.value = proceduralProfile.pBSAirDensityB;
                            physicallyBasedSky.airTint.value = proceduralProfile.pBSAirTint;
                            //Aerosols
                            physicallyBasedSky.aerosolMaximumAltitude.value = proceduralProfile.pBSAerosolMaximumAltitude;
                            physicallyBasedSky.aerosolDensity.value = proceduralProfile.pBSAerosolDensity;
                            physicallyBasedSky.aerosolTint.value = proceduralProfile.pBSAerosolTint;
                            physicallyBasedSky.aerosolAnisotropy.value = proceduralProfile.pBSAerosolAnisotropy;
                            //Artistic Overrides
                            physicallyBasedSky.colorSaturation.value = proceduralProfile.pBSColorSaturation;
                            physicallyBasedSky.alphaSaturation.value = proceduralProfile.pBSAlphaSaturation;
                            physicallyBasedSky.alphaMultiplier.value = proceduralProfile.pBSAlphaMultiplier;
                            physicallyBasedSky.horizonTint.value = proceduralProfile.pBSHorizonTint;
                            physicallyBasedSky.horizonZenithShift.value = proceduralProfile.pBSHorizonZenithShift;
                            physicallyBasedSky.zenithTint.value = proceduralProfile.pBSZenithTint;
                            //Miscellaneous
                            physicallyBasedSky.numberOfBounces.value = proceduralProfile.pBSNumberOfBounces;
                            switch(proceduralProfile.pBSIntensityMode)
                            {
                                case AmbientSkiesConsts.HDRPIntensityMode.Exposure:
                                    physicallyBasedSky.skyIntensityMode.value = SkyIntensityMode.Exposure;
                                    physicallyBasedSky.exposure.value = proceduralProfile.pBSExposure;
                                    break;
                                case AmbientSkiesConsts.HDRPIntensityMode.Multiplier:
                                    physicallyBasedSky.skyIntensityMode.value = SkyIntensityMode.Multiplier;
                                    physicallyBasedSky.multiplier.value = proceduralProfile.pBSMultiplier;
                                    break;
                            }
                            switch(proceduralProfile.pBSUpdateMode)
                            {
                                case AmbientSkiesConsts.EnvironementSkyUpdateMode.OnChanged:
                                    physicallyBasedSky.updateMode.value = EnvironmentUpdateMode.OnChanged;
                                    break;
                                case AmbientSkiesConsts.EnvironementSkyUpdateMode.OnDemand:
                                    physicallyBasedSky.updateMode.value = EnvironmentUpdateMode.OnDemand;
                                    break;
                                case AmbientSkiesConsts.EnvironementSkyUpdateMode.Realtime:
                                    physicallyBasedSky.updateMode.value = EnvironmentUpdateMode.Realtime;
                                    break;
                            }
                            physicallyBasedSky.includeSunInBaking.value = proceduralProfile.pBSIncludeSunInBaking;
                        }
                    }
                    else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                    {
                        visualEnvironmentSettings.skyType.value = 3;

                        if (volumeProfile.TryGet(out hDRISky))
                        {
                            hDRISky.active = false;
                        }

                        if (volumeProfile.TryGet(out gradientSky))
                        {
                            gradientSky.active = true;
                            gradientSky.top.value = gradientProfile.topColor;
                            gradientSky.middle.value = gradientProfile.middleColor;
                            gradientSky.bottom.value = gradientProfile.bottomColor;
                            gradientSky.gradientDiffusion.value = gradientProfile.gradientDiffusion;
                            gradientSky.exposure.value = gradientProfile.hDRIExposure;
                            gradientSky.multiplier.value = gradientProfile.skyMultiplier;
                        }

                        if (volumeProfile.TryGet(out physicallyBasedSky))
                        {
                            physicallyBasedSky.active = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies the fog settings
        /// </summary>
        /// <param name="parentObject"></param>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        /// <param name="proceduralProfile"></param>
        /// <param name="none"></param>
        /// <param name="hdri"></param>
        /// <param name="procedural"></param>
        /// <param name="gradient"></param>
        public static void ApplyFogSettings(GameObject parentObject, VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Visual Enviro
            VisualEnvironment visualEnvironmentSettings;
            if (volumeProfile.TryGet(out visualEnvironmentSettings))
            {
                //Fog
                Fog fog;
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                    {
                        if (volumeProfile.TryGet(out fog))
                        {
                            fog.active = true;
                            fog.enableVolumetricFog.value = true;
                            fog.meanFreePath.value = profile.volumetricBaseFogDistance;
                            fog.baseHeight.value = profile.volumetricBaseFogHeight;
                            fog.maximumHeight.value = profile.volumetricMeanHeight;
                            fog.maxFogDistance.value = profile.volumetricMaxFogDistance;
                            fog.colorMode.value = profile.volumetricFogColorMode;
                            fog.color.value = profile.volumetricConstFogColor;
                            fog.mipFogNear.value = profile.volumetricMipFogNear;
                            fog.mipFogFar.value = profile.volumetricMipFogFar;
                            fog.mipFogMaxMip.value = profile.volumetricMipFogMaxMip;
                            fog.enableVolumetricFog.value = true;
                            fog.albedo.value = profile.volumetricSingleScatteringAlbedo;
                            fog.anisotropy.value = profile.volumetricGlobalAnisotropy;
                            fog.globalLightProbeDimmer.value = profile.volumetricGlobalLightProbeDimmer;
                            fog.depthExtent.value = profile.volumetricDistanceRange;
                            fog.sliceDistributionUniformity.value = profile.volumetricSliceDistributionUniformity;
                            fog.filter.value = profile.volumetricFilter;
                        }

                        if (profile.useFogDensityVolume)
                        {
                            if (terrain == null)
                            {
                                terrain = Terrain.activeTerrain;
                            }

                            if (densityFogVolume == null)
                            {
                                densityFogVolume = GameObject.Find("Density Volume");
                                if (densityFogVolume == null)
                                {
                                    densityFogVolume = new GameObject("Density Volume");
                                    densityFogVolume.transform.SetParent(parentObject.transform);

                                    density = densityFogVolume.AddComponent<DensityVolume>();
                                    density.parameters.albedo = profile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = profile.densityMaskTiling;

                                    if (terrain != null)
                                    {
                                        density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                    }
                                    else
                                    {
                                        density.parameters.size = profile.densityScale;
                                    }
                                }
                            }


                            else
                            {
                                density = densityFogVolume.GetComponent<DensityVolume>();
                                if (density == null)
                                {
                                    density = densityFogVolume.AddComponent<DensityVolume>();
                                }
                                else
                                {
                                    density.parameters.albedo = profile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = profile.densityMaskTiling;
                                    density.parameters.size = profile.densityScale;
                                }
                            }
                        }
                        else
                        {
                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                            if (densityFogVolume != null)
                            {
                                Object.DestroyImmediate(densityFogVolume);
                            }
                        }
                    }
                    else
                    {
                        if (volumeProfile.TryGet(out fog))
                        {
                            fog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    if (proceduralProfile.fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                    {
                        if (volumeProfile.TryGet(out fog))
                        {
                            fog.active = true;
                            fog.enableVolumetricFog.value = true;
                            fog.meanFreePath.value = proceduralProfile.volumetricBaseFogDistance;
                            fog.baseHeight.value = proceduralProfile.volumetricBaseFogHeight;
                            fog.maximumHeight.value = proceduralProfile.volumetricMeanHeight;
                            fog.maxFogDistance.value = proceduralProfile.volumetricMaxFogDistance;
                            fog.colorMode.value = proceduralProfile.volumetricFogColorMode;
                            fog.color.value = proceduralProfile.volumetricConstFogColor;
                            fog.mipFogNear.value = proceduralProfile.volumetricMipFogNear;
                            fog.mipFogFar.value = proceduralProfile.volumetricMipFogFar;
                            fog.mipFogMaxMip.value = proceduralProfile.volumetricMipFogMaxMip;
                            fog.enableVolumetricFog.value = true;
                            fog.albedo.value = proceduralProfile.volumetricSingleScatteringAlbedo;
                            fog.anisotropy.value = proceduralProfile.volumetricGlobalAnisotropy;
                            fog.globalLightProbeDimmer.value = proceduralProfile.volumetricGlobalLightProbeDimmer;
                            fog.depthExtent.value = proceduralProfile.volumetricDistanceRange;
                            fog.sliceDistributionUniformity.value = proceduralProfile.volumetricSliceDistributionUniformity;
                            fog.filter.value = proceduralProfile.volumetricFilter;
                        }

                        if (proceduralProfile.useFogDensityVolume)
                        {
                            if (terrain != null)
                            {
                                terrain = Terrain.activeTerrain;
                            }

                            if (densityFogVolume == null)
                            {
                                densityFogVolume = GameObject.Find("Density Volume");
                                if (densityFogVolume == null)
                                {
                                    densityFogVolume = new GameObject("Density Volume");
                                    densityFogVolume.transform.SetParent(parentObject.transform);

                                    density = densityFogVolume.AddComponent<DensityVolume>();
                                    density.parameters.albedo = proceduralProfile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = proceduralProfile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = proceduralProfile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = proceduralProfile.densityMaskTiling;

                                    if (terrain != null)
                                    {
                                        density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                    }
                                    else
                                    {
                                        density.parameters.size = proceduralProfile.densityScale;
                                    }
                                }
                            }
                            else
                            {
                                if (density == null)
                                {
                                    density = densityFogVolume.GetComponent<DensityVolume>();
                                }
                                else
                                {
                                    density.parameters.albedo = proceduralProfile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = proceduralProfile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = proceduralProfile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = proceduralProfile.densityMaskTiling;
                                    density.parameters.size = proceduralProfile.densityScale;
                                }
                            }
                        }
                        else
                        {
                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                            if (densityFogVolume != null)
                            {
                                Object.DestroyImmediate(densityFogVolume);
                            }
                        }
                    }
                    else
                    {
                        if (volumeProfile.TryGet(out fog))
                        {
                            fog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    if (gradientProfile.fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                    {
                        if (volumeProfile.TryGet(out fog))
                        {
                            fog.active = true;
                            fog.enableVolumetricFog.value = true;
                            fog.meanFreePath.value = gradientProfile.volumetricBaseFogDistance;
                            fog.baseHeight.value = gradientProfile.volumetricBaseFogHeight;
                            fog.maximumHeight.value = gradientProfile.volumetricMeanHeight;
                            fog.maxFogDistance.value = proceduralProfile.volumetricMaxFogDistance;
                            fog.colorMode.value = gradientProfile.volumetricFogColorMode;
                            fog.color.value = gradientProfile.volumetricConstFogColor;
                            fog.mipFogNear.value = gradientProfile.volumetricMipFogNear;
                            fog.mipFogFar.value = gradientProfile.volumetricMipFogFar;
                            fog.mipFogMaxMip.value = gradientProfile.volumetricMipFogMaxMip;
                            fog.enableVolumetricFog.value = true;
                            fog.albedo.value = gradientProfile.volumetricSingleScatteringAlbedo;
                            fog.anisotropy.value = gradientProfile.volumetricGlobalAnisotropy;
                            fog.globalLightProbeDimmer.value = gradientProfile.volumetricGlobalLightProbeDimmer;
                            fog.depthExtent.value = gradientProfile.volumetricDistanceRange;
                            fog.sliceDistributionUniformity.value = gradientProfile.volumetricSliceDistributionUniformity;
                            fog.filter.value = gradientProfile.volumetricFilter;
                        }

                        if (gradientProfile.useFogDensityVolume)
                        {
                            if (terrain != null)
                            {
                                terrain = Terrain.activeTerrain;
                            }

                            if (densityFogVolume == null)
                            {
                                densityFogVolume = GameObject.Find("Density Volume");
                            }

                            if (densityFogVolume == null)
                            {
                                densityFogVolume = new GameObject("Density Volume");
                                densityFogVolume.transform.SetParent(parentObject.transform);

                                density = densityFogVolume.AddComponent<DensityVolume>();
                                density.parameters.albedo = gradientProfile.singleScatteringAlbedo;
                                density.parameters.meanFreePath = gradientProfile.densityVolumeFogDistance;
                                density.parameters.volumeMask = gradientProfile.fogDensityMaskTexture;
                                density.parameters.textureTiling = gradientProfile.densityMaskTiling;

                                if (terrain != null)
                                {
                                    density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                }
                                else
                                {
                                    density.parameters.size = gradientProfile.densityScale;
                                }
                            }
                            else
                            {
                                if (density == null)
                                {
                                    density = densityFogVolume.GetComponent<DensityVolume>();
                                }

                                else
                                {
                                    density.parameters.albedo = gradientProfile.singleScatteringAlbedo;
                                    density.parameters.meanFreePath = gradientProfile.densityVolumeFogDistance;
                                    density.parameters.volumeMask = gradientProfile.fogDensityMaskTexture;
                                    density.parameters.textureTiling = gradientProfile.densityMaskTiling;
                                    density.parameters.size = gradientProfile.densityScale;
                                }
                            }
                        }
                        else
                        {
                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                            if (densityFogVolume != null)
                            {
                                Object.DestroyImmediate(densityFogVolume);
                            }
                        }
                    }
                    else
                    {
                        if (volumeProfile.TryGet(out fog))
                        {
                            fog.active = false;
                        }

                        GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                        if (densityVolumeObject1 != null)
                        {
                            Object.DestroyImmediate(densityVolumeObject1);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies the hd shadow settings settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyHDShadowSettings(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //HD Shadows
            HDShadowSettings hDShadowSettings;
            if (volumeProfile.TryGet(out hDShadowSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    hDShadowSettings.maxShadowDistance.value = profile.shadowDistance;
                    if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount1)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 1;
                    }
                    else if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount2)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 2;
                    }
                    else if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount3)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 3;
                    }
                    else
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 4;
                    }

                    hDShadowSettings.cascadeShadowSplit0.value = profile.cascadeSplit1;
                    hDShadowSettings.cascadeShadowSplit1.value = profile.cascadeSplit2;
                    hDShadowSettings.cascadeShadowSplit2.value = profile.cascadeSplit3;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    hDShadowSettings.maxShadowDistance.value = proceduralProfile.shadowDistance;
                    if (proceduralProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount1)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 1;
                    }
                    else if (proceduralProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount2)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 2;
                    }
                    else if (proceduralProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount3)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 3;
                    }
                    else
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 4;
                    }

                    hDShadowSettings.cascadeShadowSplit0.value = proceduralProfile.cascadeSplit1;
                    hDShadowSettings.cascadeShadowSplit1.value = proceduralProfile.cascadeSplit2;
                    hDShadowSettings.cascadeShadowSplit2.value = proceduralProfile.cascadeSplit3;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    hDShadowSettings.maxShadowDistance.value = gradientProfile.shadowDistance;
                    if (gradientProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount1)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 1;
                    }
                    else if (gradientProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount2)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 2;
                    }
                    else if (gradientProfile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount3)
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 3;
                    }
                    else
                    {
                        hDShadowSettings.cascadeShadowSplitCount.value = 4;
                    }

                    hDShadowSettings.cascadeShadowSplit0.value = gradientProfile.cascadeSplit1;
                    hDShadowSettings.cascadeShadowSplit1.value = gradientProfile.cascadeSplit2;
                    hDShadowSettings.cascadeShadowSplit2.value = gradientProfile.cascadeSplit3;
                }
            }
        }

        /// <summary>
        /// Applies the contact shadows settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyContactShadows(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Contact Shadows
            ContactShadows contactShadowsSettings;
            if (volumeProfile.TryGet(out contactShadowsSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    contactShadowsSettings.active = profile.useContactShadows;
                    contactShadowsSettings.length.value = profile.contactShadowsLength;
                    contactShadowsSettings.distanceScaleFactor.value = profile.contactShadowsDistanceScaleFactor;
                    contactShadowsSettings.maxDistance.value = profile.contactShadowsMaxDistance;
                    contactShadowsSettings.fadeDistance.value = profile.contactShadowsFadeDistance;
                    switch(profile.contactShadowsQuality)
                    {
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Custom:
                            contactShadowsSettings.quality.value = 0;
                            contactShadowsSettings.sampleCount = profile.contactShadowsSampleCount;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Low:
                            contactShadowsSettings.quality.value = 1;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Medium:
                            contactShadowsSettings.quality.value = 2;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.High:
                            contactShadowsSettings.quality.value = 3;
                            break;
                    }
                    contactShadowsSettings.opacity.value = profile.contactShadowsOpacity;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    contactShadowsSettings.active = proceduralProfile.useContactShadows;
                    contactShadowsSettings.length.value = proceduralProfile.contactShadowsLength;
                    contactShadowsSettings.distanceScaleFactor.value = proceduralProfile.contactShadowsDistanceScaleFactor;
                    contactShadowsSettings.maxDistance.value = proceduralProfile.contactShadowsMaxDistance;
                    contactShadowsSettings.fadeDistance.value = proceduralProfile.contactShadowsFadeDistance;
                    switch (proceduralProfile.contactShadowsQuality)
                    {
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Custom:
                            contactShadowsSettings.quality.value = 0;
                            contactShadowsSettings.sampleCount = proceduralProfile.contactShadowsSampleCount;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Low:
                            contactShadowsSettings.quality.value = 1;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Medium:
                            contactShadowsSettings.quality.value = 2;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.High:
                            contactShadowsSettings.quality.value = 3;
                            break;
                    }
                    contactShadowsSettings.opacity.value = proceduralProfile.contactShadowsOpacity;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    contactShadowsSettings.active = gradientProfile.useContactShadows;
                    contactShadowsSettings.length.value = gradientProfile.contactShadowsLength;
                    contactShadowsSettings.distanceScaleFactor.value = gradientProfile.contactShadowsDistanceScaleFactor;
                    contactShadowsSettings.maxDistance.value = gradientProfile.contactShadowsMaxDistance;
                    contactShadowsSettings.fadeDistance.value = gradientProfile.contactShadowsFadeDistance;
                    switch (gradientProfile.contactShadowsQuality)
                    {
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Custom:
                            contactShadowsSettings.quality.value = 0;
                            contactShadowsSettings.sampleCount = gradientProfile.contactShadowsSampleCount;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Low:
                            contactShadowsSettings.quality.value = 1;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Medium:
                            contactShadowsSettings.quality.value = 2;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.High:
                            contactShadowsSettings.quality.value = 3;
                            break;
                    }
                    contactShadowsSettings.opacity.value = gradientProfile.contactShadowsOpacity;
                }
            }
        }

        /// <summary>
        /// Applies the micro shadows settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyMicroShadowing(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Micro Shadows
            MicroShadowing microShadowingSettings;
            if (volumeProfile.TryGet(out microShadowingSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    microShadowingSettings.active = profile.useMicroShadowing;
                    microShadowingSettings.opacity.value = profile.microShadowOpacity;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    microShadowingSettings.active = proceduralProfile.useMicroShadowing;
                    microShadowingSettings.opacity.value = proceduralProfile.microShadowOpacity;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    microShadowingSettings.active = gradientProfile.useMicroShadowing;
                    microShadowingSettings.opacity.value = gradientProfile.microShadowOpacity;
                }
            }
        }

        /// <summary>
        /// Applies the indirect light controller settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyIndirectLightingController(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Indirect Lighting
            VisualEnvironment visualEnvironment;
            if (volumeProfile.TryGet(out visualEnvironment))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    switch(profile.hdrpAmbientMode)
                    {
                        case AmbientSkiesConsts.HDRPAmbientMode.Dynamic:
                            visualEnvironment.skyAmbientMode.value = SkyAmbientMode.Dynamic;
                            break;
                        case AmbientSkiesConsts.HDRPAmbientMode.Static:
                            visualEnvironment.skyAmbientMode.value = SkyAmbientMode.Static;
                            break;
                    }
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    switch (proceduralProfile.hdrpAmbientMode)
                    {
                        case AmbientSkiesConsts.HDRPAmbientMode.Dynamic:
                            visualEnvironment.skyAmbientMode.value = SkyAmbientMode.Dynamic;
                            break;
                        case AmbientSkiesConsts.HDRPAmbientMode.Static:
                            visualEnvironment.skyAmbientMode.value = SkyAmbientMode.Static;
                            break;
                    }
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    switch (gradientProfile.hdrpAmbientMode)
                    {
                        case AmbientSkiesConsts.HDRPAmbientMode.Dynamic:
                            visualEnvironment.skyAmbientMode.value = SkyAmbientMode.Dynamic;
                            break;
                        case AmbientSkiesConsts.HDRPAmbientMode.Static:
                            visualEnvironment.skyAmbientMode.value = SkyAmbientMode.Static;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Applies the screen space reflections settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyScreenSpaceReflection(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Screen Space Reflection
            ScreenSpaceReflection screenSpaceReflectionSettings;
            if (volumeProfile.TryGet(out screenSpaceReflectionSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    screenSpaceReflectionSettings.active = profile.enableScreenSpaceReflections;
                    screenSpaceReflectionSettings.screenFadeDistance.value = profile.screenEdgeFadeDistance;
                    switch(profile.screenSpaceReflectionsQuality)
                    {
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Custom:
                            screenSpaceReflectionSettings.quality.value = 0;
                            screenSpaceReflectionSettings.rayMaxIterations = profile.maxNumberOfRaySteps;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Low:
                            screenSpaceReflectionSettings.quality.value = 1;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Medium:
                            screenSpaceReflectionSettings.quality.value = 2;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.High:
                            screenSpaceReflectionSettings.quality.value = 3;
                            break;
                    }

                    screenSpaceReflectionSettings.depthBufferThickness.value = profile.objectThickness;
                    screenSpaceReflectionSettings.minSmoothness.value = profile.minSmoothness;
                    screenSpaceReflectionSettings.smoothnessFadeStart.value = profile.smoothnessFadeStart;
                    screenSpaceReflectionSettings.reflectSky.value = profile.reflectSky;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    screenSpaceReflectionSettings.active = proceduralProfile.enableScreenSpaceReflections;
                    screenSpaceReflectionSettings.screenFadeDistance.value = proceduralProfile.screenEdgeFadeDistance;
                    switch (proceduralProfile.screenSpaceReflectionsQuality)
                    {
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Custom:
                            screenSpaceReflectionSettings.quality.value = 0;
                            screenSpaceReflectionSettings.rayMaxIterations = proceduralProfile.maxNumberOfRaySteps;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Low:
                            screenSpaceReflectionSettings.quality.value = 1;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Medium:
                            screenSpaceReflectionSettings.quality.value = 2;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.High:
                            screenSpaceReflectionSettings.quality.value = 3;
                            break;
                    }
                    screenSpaceReflectionSettings.depthBufferThickness.value = proceduralProfile.objectThickness;
                    screenSpaceReflectionSettings.minSmoothness.value = proceduralProfile.minSmoothness;
                    screenSpaceReflectionSettings.smoothnessFadeStart.value = proceduralProfile.smoothnessFadeStart;
                    screenSpaceReflectionSettings.reflectSky.value = proceduralProfile.reflectSky;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientGradientSkies)
                {
                    screenSpaceReflectionSettings.active = gradientProfile.enableScreenSpaceReflections;
                    screenSpaceReflectionSettings.screenFadeDistance.value = gradientProfile.screenEdgeFadeDistance;
                    switch (gradientProfile.screenSpaceReflectionsQuality)
                    {
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Custom:
                            screenSpaceReflectionSettings.quality.value = 0;
                            screenSpaceReflectionSettings.rayMaxIterations = gradientProfile.maxNumberOfRaySteps;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Low:
                            screenSpaceReflectionSettings.quality.value = 1;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.Medium:
                            screenSpaceReflectionSettings.quality.value = 2;
                            break;
                        case AmbientSkiesConsts.HDRPPostProcessingQuality.High:
                            screenSpaceReflectionSettings.quality.value = 3;
                            break;
                    }
                    screenSpaceReflectionSettings.depthBufferThickness.value = gradientProfile.objectThickness;
                    screenSpaceReflectionSettings.minSmoothness.value = gradientProfile.minSmoothness;
                    screenSpaceReflectionSettings.smoothnessFadeStart.value = gradientProfile.smoothnessFadeStart;
                    screenSpaceReflectionSettings.reflectSky.value = gradientProfile.reflectSky;
                }
            }
        }

        /// <summary>
        /// Applies the screen space refractions settings
        /// </summary>
        /// <param name="volumeProfile"></param>
        /// <param name="profile"></param>
        public static void ApplyScreenSpaceRefraction(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientProfile)
        {
            //Screen Space Refraction
            ScreenSpaceRefraction screenSpaceRefractionSettings;
            if (volumeProfile.TryGet(out screenSpaceRefractionSettings))
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    screenSpaceRefractionSettings.active = profile.enableScreenSpaceRefractions;
                    screenSpaceRefractionSettings.screenFadeDistance.value = profile.screenWeightDistance;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    screenSpaceRefractionSettings.active = proceduralProfile.enableScreenSpaceRefractions;
                    screenSpaceRefractionSettings.screenFadeDistance.value = proceduralProfile.screenWeightDistance;
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    screenSpaceRefractionSettings.active = gradientProfile.enableScreenSpaceRefractions;
                    screenSpaceRefractionSettings.screenFadeDistance.value = gradientProfile.screenWeightDistance;
                }
            }
        }
#endif
        #endregion

        #region Volume Setup

#if HDPipeline
        /// <summary>
        /// Sets up the volume to be disabled 
        /// </summary>
        /// <param name="volumeName"></param>
        /// <param name="parentObject"></param>
        public static void DisabledHDRPSky(string volumeName, GameObject parentObject)
        {
            if (volumeSettings == null)
            {
                volumeSettings = SetVolumeSetup(volumeName);
            }

            if (volumeObject == null)
            {
                volumeObject = GameObject.Find(volumeName);
                if (volumeObject == null)
                {
                    volumeObject = new GameObject(volumeName);
                    volumeObject.AddComponent<Volume>();
                    volumeObject.transform.SetParent(parentObject.transform);

                    volumeSettings.isGlobal = true;
                    volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath("Ambient Skies HD Volume Profile"));

                    volumeProfile = volumeSettings.sharedProfile;
                }
            }           
            else
            {
                if (volumeProfile == null)
                {
                    volumeProfile = volumeObject.GetComponent<Volume>().sharedProfile;
                }
            }

            if (volumeSettings != null)
            {
                EditorUtility.SetDirty(volumeSettings);

                Fog fog;
                if (volumeProfile.TryGet(out fog))
                {
                    fog.active = true;
                }

                VisualEnvironment visualEnvironment;
                if (volumeProfile.TryGet(out visualEnvironment))
                {
                    visualEnvironment.skyType.value = 2;
                }

                HDRISky hDRISky;
                if (volumeProfile.TryGet(out hDRISky))
                {
                    hDRISky.active = false;
                }

                PhysicallyBasedSky physicallyBasedSky;
                if (volumeProfile.TryGet(out physicallyBasedSky))
                {
                    physicallyBasedSky.active = true;
                }

                GradientSky gradientSky;
                if (volumeProfile.TryGet(out gradientSky))
                {
                    gradientSky.active = false;
                }   
            }

            if (directionalLight == null)
            {
                directionalLight = SkyboxUtils.GetMainDirectionalLight();
            }
            else
            {
                if (sunLight == null)
                {
                    sunLight = directionalLight.GetComponent<Light>();
                    if (sunLight != null)
                    {
                        sunLight.color = SkyboxUtils.GetColorFromHTML("FFDCC5");
                        sunLight.intensity = 3.14f;
                    }

                }
                else
                {
                    sunLight.color = SkyboxUtils.GetColorFromHTML("FFDCC5");
                    sunLight.intensity = 3.14f;
                }

                if (hDAdditionalLightData == null)
                {
                    hDAdditionalLightData = directionalLight.GetComponent<HDAdditionalLightData>();
                    if (hDAdditionalLightData == null)
                    {
                        hDAdditionalLightData = directionalLight.AddComponent<HDAdditionalLightData>();
                        hDAdditionalLightData.intensity = 3.14f;
                    }
                    else
                    {
                        hDAdditionalLightData.intensity = 3.14f;
                    }
                }
                else
                {
                    hDAdditionalLightData.intensity = 3.14f;
                }
            }
        }
#endif

        /// <summary>
        /// Sets the HDRP environment volume object
        /// </summary>
        /// <param name="volumeName"></param>
        /// <param name="parentObject"></param>
        /// <param name="volumeObject"></param>
        public static GameObject SetupEnvironmentHDRPVolumeObject(string volumeName, GameObject parentObject, GameObject volumeObject)
        {
            GameObject newGameobject = volumeObject;
            //Hd Pipeline Volume Setup           
            if (newGameobject == null)
            {
                newGameobject = new GameObject(volumeName);
                newGameobject.layer = LayerMask.NameToLayer("TransparentFX");
                newGameobject.transform.SetParent(parentObject.transform);
            }

            return newGameobject;
        }

        /// <summary>
        /// Sets the volume object setup
        /// </summary>
        /// <param name="volumeSettings"></param>
        /// <param name="volumeObject"></param>
        /// <param name="profile"></param>
#if HDPipeline
        public static Volume SetVolumeSetup(string volumeName)
        {
            //Get volume object
            GameObject volumeObject = GameObject.Find(volumeName);
            Volume volumeSettings = volumeObject.GetComponent<Volume>();
            if (volumeSettings == null)
            {
                volumeSettings = volumeObject.AddComponent<Volume>();
                volumeSettings.isGlobal = true;
                volumeSettings.blendDistance = 5f;
                volumeSettings.weight = 1f;
                volumeSettings.priority = 0f;
            }
            else
            {
                volumeSettings.isGlobal = true;
                volumeSettings.blendDistance = 5f;
                volumeSettings.weight = 1f;
                volumeSettings.priority = 0f;
            }

            return volumeSettings;
        }

        /// <summary>
        /// Applies directional light settings
        /// </summary>
        /// <param name="skyProfiles"></param>
        /// <param name="profile"></param>
        public static void ApplyDirectionalLightSettings(VolumeProfile volumeProfile, AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientProceduralSkyboxProfile proceduralProfile, AmbientGradientSkyboxProfile gradientPorfile)
        {
            //Get the main directional light
            if (directionalLight == null)
            {
                directionalLight = SkyboxUtils.GetMainDirectionalLight();
            }

            if (directionalLight != null)
            {
                if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientHDRISkies)
                {
                    if (hDAdditionalLightData == null)
                    {
                        hDAdditionalLightData = directionalLight.GetComponent<HDAdditionalLightData>();
                    }

                    if (hDAdditionalLightData != null)
                    {
                        if (sunLight == null)
                        {
                            sunLight = directionalLight.GetComponent<Light>();
                        }

                        hDAdditionalLightData.lightUnit = LightUnit.Lux;
                        hDAdditionalLightData.intensity = profile.HDRPSunIntensity * 3.14f;
                        hDAdditionalLightData.useContactShadow.level = -1;
                        hDAdditionalLightData.useContactShadow.useOverride = profile.useContactShadows;
                        sunLight.color = profile.sunColor;

                        switch (profile.shadowQuality)
                        {
                            case AmbientSkiesConsts.HDShadowQuality.Resolution64:
                                hDAdditionalLightData.SetShadowResolution(64);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution128:
                                hDAdditionalLightData.SetShadowResolution(128);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution256:
                                hDAdditionalLightData.SetShadowResolution(256);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution512:
                                hDAdditionalLightData.SetShadowResolution(512);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution1024:
                                hDAdditionalLightData.SetShadowResolution(1024);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution2048:
                                hDAdditionalLightData.SetShadowResolution(2048);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution4096:
                                hDAdditionalLightData.SetShadowResolution(4096);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution8192:
                                hDAdditionalLightData.SetShadowResolution(8192);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution16384:
                                hDAdditionalLightData.SetShadowResolution(16384);
                                break;
                        }
                    }
                }
                else if (skyProfiles.m_systemTypes == AmbientSkiesConsts.SystemTypes.AmbientProceduralSkies)
                {
                    if (hDAdditionalLightData == null)
                    {
                        hDAdditionalLightData = directionalLight.GetComponent<HDAdditionalLightData>();
                    }

                    if (hDAdditionalLightData != null)
                    {
                        if (sunLight == null)
                        {
                            sunLight = directionalLight.GetComponent<Light>();
                        }

                        hDAdditionalLightData.lightUnit = LightUnit.Lux;
                        hDAdditionalLightData.intensity = proceduralProfile.proceduralHDRPSunIntensity * 3.14f;
                        hDAdditionalLightData.useContactShadow.level = -1;
                        hDAdditionalLightData.useContactShadow.useOverride = proceduralProfile.useContactShadows;
                        sunLight.color = proceduralProfile.proceduralSunColor;

                        switch (proceduralProfile.shadowQuality)
                        {
                            case AmbientSkiesConsts.HDShadowQuality.Resolution64:
                                hDAdditionalLightData.SetShadowResolution(64);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution128:
                                hDAdditionalLightData.SetShadowResolution(128);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution256:
                                hDAdditionalLightData.SetShadowResolution(256);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution512:
                                hDAdditionalLightData.SetShadowResolution(512);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution1024:
                                hDAdditionalLightData.SetShadowResolution(1024);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution2048:
                                hDAdditionalLightData.SetShadowResolution(2048);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution4096:
                                hDAdditionalLightData.SetShadowResolution(4096);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution8192:
                                hDAdditionalLightData.SetShadowResolution(8192);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution16384:
                                hDAdditionalLightData.SetShadowResolution(16384);
                                break;
                        }
                    }
                }
                else
                {
                    if (hDAdditionalLightData == null)
                    {
                        hDAdditionalLightData = directionalLight.GetComponent<HDAdditionalLightData>();
                    }

                    if (hDAdditionalLightData != null)
                    {
                        if (sunLight == null)
                        {
                            sunLight = directionalLight.GetComponent<Light>();
                        }

                        hDAdditionalLightData.lightUnit = LightUnit.Lux;
                        hDAdditionalLightData.intensity = gradientPorfile.HDRPSunIntensity * 3.14f;
                        hDAdditionalLightData.useContactShadow.level = -1;
                        hDAdditionalLightData.useContactShadow.useOverride = gradientPorfile.useContactShadows;
                        sunLight.color = gradientPorfile.sunColor;

                        switch (gradientPorfile.shadowQuality)
                        {
                            case AmbientSkiesConsts.HDShadowQuality.Resolution64:
                                hDAdditionalLightData.SetShadowResolution(64);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution128:
                                hDAdditionalLightData.SetShadowResolution(128);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution256:
                                hDAdditionalLightData.SetShadowResolution(256);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution512:
                                hDAdditionalLightData.SetShadowResolution(512);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution1024:
                                hDAdditionalLightData.SetShadowResolution(1024);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution2048:
                                hDAdditionalLightData.SetShadowResolution(2048);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution4096:
                                hDAdditionalLightData.SetShadowResolution(4096);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution8192:
                                hDAdditionalLightData.SetShadowResolution(8192);
                                break;
                            case AmbientSkiesConsts.HDShadowQuality.Resolution16384:
                                hDAdditionalLightData.SetShadowResolution(16384);
                                break;
                        }
                    }
                }
            }
        }

#endif
        #endregion

        #region HDRP Utils

        /// <summary>
        /// Adds HDRP components to light and camera if missing
        /// </summary>
        public static void AddRequiredComponents()
        {
#if HDPipeline
            if (directionalLight == null)
            {
                directionalLight = SkyboxUtils.GetMainDirectionalLight();
            }

            if (mainCamera == null)
            {
                mainCamera = SkyboxUtils.GetOrCreateMainCamera();
            }

            if (directionalLight.GetComponent<HDAdditionalLightData>() == null)
            {
                hDAdditionalLightData = directionalLight.AddComponent<HDAdditionalLightData>();
            }

            if (mainCamera.GetComponent<HDAdditionalCameraData>() == null)
            {
                hdCamData = mainCamera.AddComponent<HDAdditionalCameraData>();
            }
#endif
        }

        /// <summary>
        /// Fixes realtime reflection probes in hd to set the main one to baked
        /// </summary>
        public static void FixHDReflectionProbes()
        {
            ReflectionProbe[] reflectionProbes = Object.FindObjectsOfType<ReflectionProbe>();
            if (reflectionProbes != null)
            {
                foreach (ReflectionProbe probe in reflectionProbes)
                {
                    if (probe.name == "Global Reflection Probe")
                    {
                        probe.mode = ReflectionProbeMode.Baked;
                    }
                }
#if HDPipeline
                HDAdditionalReflectionData[] reflectionData = Object.FindObjectsOfType<HDAdditionalReflectionData>();
                if (reflectionData != null)
                {
                    foreach (HDAdditionalReflectionData data in reflectionData)
                    {
                        if (data.gameObject.name == "Global Reflection Probe")
                        {
#if UNITY_2019_1_OR_NEWER
                            data.mode = ProbeSettings.Mode.Baked;
#elif UNITY_2018_3_OR_NEWER
                            data.mode = ReflectionProbeMode.Baked;

#endif
                        }
                    }
                }
#endif
            }
        }

#if HDPipeline
        /// <summary>
        /// Sets up the HD Camera Data
        /// </summary>
        public static void SetHDCamera()
        {
            if (hdCamData == null)
            {
                hdCamData = Object.FindObjectOfType<HDAdditionalCameraData>();
            }
            else
            {
#if UNITY_2019_1_OR_NEWER
                hdCamData.volumeLayerMask = 2;
#endif
            }
        }
#endif

        /// <summary>
        /// Removes the old HDRP objects from the scene
        /// </summary>
        public static void RemoveOldHDRPObjects()
        {
            //Locates the old volume profine that unity creates
            GameObject oldVolumeObject = GameObject.Find("Volume Settings");
            if (oldVolumeObject != null)
            {
                //Destroys the object
                Object.DestroyImmediate(oldVolumeObject);
            }

            //Find the old post processing object that unity creates
            GameObject oldPostProcessing = GameObject.Find("Post-process Volume");
            if (oldPostProcessing != null)
            {
                //Destoys the old post processing object
                Object.DestroyImmediate(oldPostProcessing);
            }

            //Locates the old volume profine that unity creates
            GameObject old2019VolumeObject = GameObject.Find("Render Settings");
            if (old2019VolumeObject != null)
            {
                //Destroys the object
                Object.DestroyImmediate(old2019VolumeObject);
            }

            //Locates the old volume profine that unity creates
            GameObject old2019VolumeObject2 = GameObject.Find("Rendering Settings");
            if (old2019VolumeObject2 != null)
            {
                //Destroys the object
                Object.DestroyImmediate(old2019VolumeObject2);
            }

            //Find the old post processing object that unity creates
            GameObject old2019PostProcessing = GameObject.Find("Post Processing Settings");
            if (old2019PostProcessing != null)
            {
                //Destroys the object
                Object.DestroyImmediate(old2019PostProcessing);
            }

            //Find the old post processing object that unity creates
            GameObject old2019PostProcessing2 = GameObject.Find("Default Post-process");
            if (old2019PostProcessing2 != null)
            {
                //Destroys the object
                Object.DestroyImmediate(old2019PostProcessing2);
            }

            //Find the old post processing object that unity creates
            GameObject old2019PostProcessing3 = GameObject.Find("Scene Post-process");
            if (old2019PostProcessing2 != null)
            {
                //Destroys the object
                Object.DestroyImmediate(old2019PostProcessing3);
            }
        }

        #endregion

        #endregion

        #endregion

        #endregion
    }
}