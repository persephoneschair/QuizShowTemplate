//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using UnityEditor;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
#if HDPipeline && !UNITY_2019_3_OR_NEWER
using UnityEngine.Experimental.Rendering.HDPipeline;
#elif HDPipeline && UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.HighDefinition;
#endif
#if UPPipeline
using UnityEngine.Rendering.Universal;
#endif

namespace AmbientSkies
{
    /// <summary>
    /// A class manipulate Unity Post Processing v2.
    /// The system has a global overide capability as well;
    /// </summary>
    public static class PostProcessingUtils
    {
        #region Variables

#if UNITY_POST_PROCESSING_STACK_V2
        //Global post processing profile
        public static PostProcessProfile postProcessProfile;
        //Post processing layer component
        public static PostProcessLayer processLayer;
        //Post processing voluem component
        public static PostProcessVolume postProcessVolume;
#endif
        //The parent object
        public static GameObject theParent;
        //Camera object
        public static GameObject mainCameraObj;
        //Camera component
        public static Camera camera;
        //Post processing volume object
        public static GameObject postProcessVolumeObj;
        //HDRP Post processing volume object
        public static GameObject postVolumeObject;

#if UPPipeline || HDPipeline
        //Volume settings
        public static Volume volume;
        //Volume profile settings
        public static VolumeProfile volumeProfile;
#endif
#if UPPipeline
        public static UniversalAdditionalCameraData urpCameraData;
#endif

#if HDPipeline
        //HDRP camera data
        public static HDAdditionalCameraData cameraData;
#endif

        #endregion

        #region Utils

        /// <summary>
        /// Returns true if post processing v2 is installed
        /// </summary>
        /// <returns>True if installed</returns>
        public static bool PostProcessingInstalled()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            return true;
#else
            return false;
#endif
        }

        #region Get/Set From Profile

        /// <summary>
        /// Get current profile index that has the profile name
        /// </summary>
        /// <param name="profiles">Profile list to search</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProfileIndexFromProfileName(AmbientSkyProfiles profiles, string name)
        {
            for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
            {
                if (profiles.m_ppProfiles[idx].name == name)
                {
                    return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get current profile index of currently active post processing profile
        /// </summary>
        /// <param name="profile">Profile list to search</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProfileIndexFromPostProcessing(AmbientSkyProfiles profiles)
        {
            #if UNITY_POST_PROCESSING_STACK_V2
            PostProcessProfile profile = GetGlobalPostProcessingProfile();
            if (profile == null)
            {
                return 0;
            }
            else
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].assetName == profile.name)
                    {
                        return idx;
                    }
                }
            }
            #endif
            return -1;
        }

        /// <summary>
        /// Get the currently active global post processing profile
        /// </summary>
        /// <returns>Currently active global post processing profile or null if there is none / its not set up properly</returns>
#if UNITY_POST_PROCESSING_STACK_V2
        public static PostProcessProfile GetGlobalPostProcessingProfile()
        {
            //Get global post processing object
            GameObject postProcessVolumeObj = GameObject.Find("Global Post Processing");
            if (postProcessVolumeObj == null)
            {
                return null;
            }

            //Get global post processing volume
            PostProcessVolume postProcessVolume = postProcessVolumeObj.GetComponent<PostProcessVolume>();
            if (postProcessVolume == null)
            {
                return null;
            }

            //Return its profile
            return postProcessVolume.sharedProfile;
    }
#endif

        /// <summary>
        /// Get a profile from the asset name
        /// </summary>
        /// <param name="profiles">List of profiles</param>
        /// <param name="profileAssetName">Asset name we are looking for</param>
        /// <returns>Profile or null if not found</returns>
        public static AmbientPostProcessingProfile GetProfileFromAssetName(AmbientSkyProfiles profiles, string profileAssetName)
        {
            if (profiles == null)
            {
                return null;
            }
            return profiles.m_ppProfiles.Find(x => x.assetName == profileAssetName);
        }

        /// <summary>
        /// Load the selected profile and apply it
        /// </summary>
        /// <param name="profile">The profiles object</param>
        /// <param name="profileName">The name of the profile to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileName(AmbientSkyProfiles profile, AmbientSkyboxProfile skyProfile, string profileName, bool useDefaults, bool updateAO, bool updateAutoExposure, bool updateBloom, bool updateChromatic, bool updateColorGrading, bool updateDOF, bool updateGrain, bool updateLensDistortion, bool updateMotionBlur, bool updateSSR, bool updateVignette, bool updateTargetPlatform)
        {
            AmbientPostProcessingProfile p = profile.m_ppProfiles.Find(x => x.name == profileName);
            if (p == null)
            {
                Debug.LogWarning("Invalid profile name supplied, can not apply post processing profile!");
                return;
            }
            SetPostProcessingProfile(p, profile, useDefaults, updateAO, updateAutoExposure, updateBloom, updateChromatic, updateColorGrading, updateDOF, updateGrain, updateLensDistortion, updateMotionBlur, updateSSR, updateVignette, updateTargetPlatform);
        }

        /// <summary>
        /// Load the selected profile 
        /// </summary>
        /// <param name="profile">The profiles object</param>
        /// <param name="assetName">The name of the profile to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromAssetName(AmbientSkyProfiles profile, AmbientSkyboxProfile skyProfile, string assetName, bool useDefaults, bool updateAO, bool updateAutoExposure, bool updateBloom, bool updateChromatic, bool updateColorGrading, bool updateDOF, bool updateGrain, bool updateLensDistortion, bool updateMotionBlur, bool updateSSR, bool updateVignette, bool updateTargetPlatform)
        {
            AmbientPostProcessingProfile p = profile.m_ppProfiles.Find(x => x.assetName == assetName);
            if (p == null)
            {
                Debug.LogWarning("Invalid asset name supplied, can not apply post processing settings!");
                return;
            }
            SetPostProcessingProfile(p, profile, useDefaults, updateAO, updateAutoExposure, updateBloom, updateChromatic, updateColorGrading, updateDOF, updateGrain, updateLensDistortion, updateMotionBlur, updateSSR, updateVignette, updateTargetPlatform);
        }

        /// <summary>
        /// Load the selected profile and apply
        /// </summary>
        /// <param name="profile">The profiles object</param>
        /// <param name="profileIndex">The zero based index to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileIndex(AmbientSkyProfiles skyProfile, int profileIndex, bool useDefaults, bool updateAO, bool updateAutoExposure, bool updateBloom, bool updateChromatic, bool updateColorGrading, bool updateDOF, bool updateGrain, bool updateLensDistortion, bool updateMotionBlur, bool updateSSR, bool updateVignette, bool updatePanini, bool updateTargetPlatform)
        {
            if (skyProfile == null)
            {
                Debug.LogError("Missing the sky profiles (m_profiles from Ambient Skies) try reopening ambient skies and try again. Exiting");
                return;
            }

            if (profileIndex > skyProfile.m_ppProfiles.Count || profileIndex == -1)
            {
                profileIndex = 0;
            }

            AmbientPostProcessingProfile profile = skyProfile.m_ppProfiles[profileIndex];
            if (profile == null)
            {
                Debug.LogError("Missing the sky profiles (m_selectedPostProcessingProfile from Ambient Skies) try reopening ambient skies and try again. Exiting");
                return;
            }

            if (profileIndex < 0 || profileIndex >= skyProfile.m_ppProfiles.Count)
            {
                Debug.LogWarning("Invalid profile index selected, can not apply post processing settings!");
                return;
            }
            switch (skyProfile.m_selectedRenderPipeline)
            {
                case AmbientSkiesConsts.RenderPipelineSettings.HighDefinition:
                    SetHDRPPostProcessingProfile(skyProfile.m_ppProfiles[profileIndex], skyProfile, useDefaults, updateAO, updateAutoExposure, updateBloom, updateChromatic, updateColorGrading, updateDOF, updateGrain, updateLensDistortion, updateMotionBlur, updatePanini, updateVignette, updateTargetPlatform, updateSSR);
                    break;
                case AmbientSkiesConsts.RenderPipelineSettings.Universal:
                    SetURPPostProcessingProfile(skyProfile.m_ppProfiles[profileIndex], skyProfile, useDefaults, updateAO, updateAutoExposure, updateBloom, updateChromatic, updateColorGrading, updateDOF, updateGrain, updateLensDistortion, updateMotionBlur, updatePanini, updateVignette, updateTargetPlatform, updateSSR);
                    break;
                default:
                    SetPostProcessingProfile(skyProfile.m_ppProfiles[profileIndex], skyProfile, useDefaults, updateAO, updateAutoExposure, updateBloom, updateChromatic, updateColorGrading, updateDOF, updateGrain, updateLensDistortion, updateMotionBlur, updateSSR, updateVignette, updateTargetPlatform);
                    break;
            }

            AmbientSkiesEditorFunctions.MarkActiveSceneAsDirty(skyProfile);
        }

        #endregion

        #region Post Processing Setup

        /// <summary>
        /// Set the specified post processing profile up.
        /// </summary>
        /// <param name="profile">Profile to set up</param>
        /// <param name="useDefaults">Use defaults or current</param>
        public static void SetPostProcessingProfile(AmbientPostProcessingProfile profile, AmbientSkyProfiles skyProfile, bool useDefaults, bool updateAO, bool updateAutoExposure, bool updateBloom, bool updateChromatic, bool updateColorGrading, bool updateDOF, bool updateGrain, bool updateLensDistortion, bool updateMotionBlur, bool updateSSR, bool updateVignette, bool updateTargetPlatform)
        {
            //Clear console if required
            if (skyProfile.m_smartConsoleClean)
            {
                SkyboxUtils.ClearLog();
                Debug.Log("Console cleared successfully");
            }
#if UNITY_POST_PROCESSING_STACK_V2
            if (skyProfile.m_usePostFX)
            {
#if GAIA_2_PRESENT
                SkyboxUtils.SetGaiaParenting(true);
#endif
                RemoveHDRPPostProcessing(skyProfile);

                //Get the FX parent
                if (theParent == null)
                {
                    theParent = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Global", false);
                }

                if (mainCameraObj == null)
                {
                    mainCameraObj = SkyboxUtils.GetOrCreateMainCamera();
                }

                if (camera == null)
                {
                    camera = mainCameraObj.GetComponent<Camera>();
                }

                if (processLayer == null)
                {
                    processLayer = mainCameraObj.GetComponent<PostProcessLayer>();
                    if (processLayer == null)
                    {
                        processLayer = mainCameraObj.AddComponent<PostProcessLayer>();
                        processLayer.volumeLayer = 2;
                    }
                }
                else
                {
                    processLayer.volumeLayer = 2;
                }

                //Find or create global post processing volume object
                if (postProcessVolumeObj == null)
                {
                    postProcessVolumeObj = GameObject.Find("Global Post Processing");
                    if (postProcessVolumeObj == null)
                    {
                        postProcessVolumeObj = new GameObject("Global Post Processing");
                        postProcessVolumeObj.transform.parent = theParent.transform;
                        postProcessVolumeObj.layer = LayerMask.NameToLayer("TransparentFX");
                        postProcessVolumeObj.AddComponent<PostProcessVolume>();
                    }
                }

                //Setup the global post processing volume
                if (postProcessVolume == null)
                {
                    postProcessVolume = postProcessVolumeObj.GetComponent<PostProcessVolume>();
                    postProcessVolume.isGlobal = true;
                    postProcessVolume.priority = 0f;
                    postProcessVolume.weight = 1f;
                    postProcessVolume.blendDistance = 0f;
                }
                else
                {
                    postProcessVolume.isGlobal = true;
                    postProcessVolume.priority = 0f;
                    postProcessVolume.weight = 1f;
                    postProcessVolume.blendDistance = 0f;
                }

                if (postProcessVolume != null)
                {
                    postProcessProfile = postProcessVolume.sharedProfile;

                    bool loadProfile = false;
                    if (postProcessVolume.sharedProfile == null)
                    {
                        loadProfile = true;
                    }
                    else
                    {
                        if (postProcessVolume.sharedProfile.name != profile.assetName)
                        {
                            loadProfile = true;
                        }
                    }
                    if (loadProfile)
                    {
                        if (profile.name == "User")
                        {
                            if (profile.customPostProcessingProfile == null)
                            {
                                if (skyProfile.m_showDebug)
                                {
                                    Debug.LogError("Missing profile please add one in the post fx tab. Exiting!");
                                    return;
                                }
                            }
                            else
                            {
                                if (postProcessVolume.sharedProfile == null)
                                {
                                    postProcessVolume.sharedProfile = profile.customPostProcessingProfile;
                                    postProcessProfile = postProcessVolume.sharedProfile;
                                }
                                else if (postProcessVolume.sharedProfile.name != profile.customPostProcessingProfile.name)
                                {
                                    postProcessVolume.sharedProfile = profile.customPostProcessingProfile;
                                    postProcessProfile = postProcessVolume.sharedProfile;
                                }
                            }
                        }
                        else
                        {
                            //Get the profile path
                            string postProcessPath = SkyboxUtils.GetAssetPath(profile.assetName);
                            if (string.IsNullOrEmpty(postProcessPath))
                            {
                                Debug.LogErrorFormat("AmbientSkies:SetPostProcessingProfile() : Unable to load '{0}' profile - Aborting!", profile.assetName);
                                return;
                            }

                            if (postProcessVolume.sharedProfile == null)
                            {
                                postProcessVolume.sharedProfile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(postProcessPath);
                                postProcessProfile = postProcessVolume.sharedProfile;
                            }
                            else
                            {
                                if (postProcessVolume.sharedProfile.name != postProcessPath)
                                {
                                    postProcessVolume.sharedProfile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(postProcessPath);
                                    postProcessProfile = postProcessVolume.sharedProfile;
                                }
                            }
                        }
                    }

                    if (postProcessProfile == null) return;
                    EditorUtility.SetDirty(postProcessProfile);

                    //Get the configurable values
                    SetAntiAliasing(skyProfile, profile, mainCameraObj, camera);
#if Mewlist_Clouds
                        MassiveCloudsUtils.SetupMassiveCloudsSystem(profile);
#endif
                    if (profile.name != "User")
                    {
                        if (updateAO)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetAmbientOcclusion()");
                            }

                            SetAmbientOcclusion(profile, postProcessProfile);
                        }

                        if (updateAutoExposure)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetAutoExposure()");
                            }

                            SetAutoExposure(profile, postProcessProfile);
                        }

                        if (updateBloom)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetBloom()");
                            }

                            SetBloom(profile, postProcessProfile);
                        }

                        if (updateChromatic)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetChromaticAberration()");
                            }

                            SetChromaticAberration(profile, postProcessProfile);
                        }

                        if (updateColorGrading)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetColorGrading()");
                            }

                            SetColorGrading(profile, postProcessProfile);
                        }

                        if (updateDOF)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetDepthOfField()");
                            }

                            SetDepthOfField(profile, camera, postProcessProfile);
                        }

                        if (updateGrain)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetGrain()");
                            }

                            SetGrain(profile, postProcessProfile);
                        }

                        if (updateLensDistortion)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetLensDistortion()");
                            }

                            SetLensDistortion(profile, postProcessProfile);
                        }

                        if (updateSSR)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetScreenSpaceReflections()");
                            }

                            SetScreenSpaceReflections(profile, postProcessProfile);
                        }

                        if (updateVignette)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetVignette()");
                            }

                            SetVignette(profile, postProcessProfile);
                        }

                        if (updateMotionBlur)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetMotionBlur()");
                            }

                            SetMotionBlur(profile, postProcessProfile);
                        }
                    }

                    if (updateTargetPlatform)
                    {
                        if (skyProfile.m_showFunctionDebugsOnly)
                        {
                            Debug.Log("Updating SetTargetPlatform()");
                        }

                        SetTargetPlatform(skyProfile, postProcessProfile);
                    }

                    HidePostProcessingGizmo(profile);
                }
            }
            else
            {
                RemovePostProcessing();
                SkyboxUtils.DestroyParent("Ambient Skies Global", skyProfile.m_multiSceneSupport);
            }
#endif
        }

        /// <summary>
        /// Sets HDRP post processing 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="skyProfile"></param>
        /// <param name="useDefaults"></param>
        public static void SetHDRPPostProcessingProfile(AmbientPostProcessingProfile profile, AmbientSkyProfiles skyProfile, bool useDefaults, bool updateAO, bool updateAutoExposure, bool updateBloom, bool updateChromatic, bool updateColorGrading, bool updateDOF, bool updateGrain, bool updateLensDistortion, bool updateMotionBlur, bool updatePanini, bool updateVignette, bool updateTargetPlatform, bool updateSSR)
        {
            //Clear console if required
            if (skyProfile.m_smartConsoleClean)
            {
                SkyboxUtils.ClearLog();
                Debug.Log("Console cleared successfully");
            }
#if HDPipeline
            if (skyProfile.m_usePostFX)
            {
#if !UNITY_2019_1_OR_NEWER
                RemovePostProcessing();
                SetPostProcessingProfile(profile, skyProfile, useDefaults, updateAO, updateAutoExposure, updateBloom, updateChromatic, updateColorGrading, updateDOF, updateGrain, updateLensDistortion, updateMotionBlur, updateSSR, updateVignette, updateTargetPlatform);
                return;
#else
                RemovePostProcessing();

                //Get the FX parent
                if (theParent == null)
                {
                    theParent = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Global", false);
                }

                if (mainCameraObj == null)
                {
                    mainCameraObj = SkyboxUtils.GetOrCreateMainCamera();
                }

                if (camera == null)
                {
                    camera = mainCameraObj.GetComponent<Camera>();
                }


                if (postVolumeObject == null)
                {
                    postVolumeObject = GameObject.Find("Post Processing HDRP Volume");
                    if (postVolumeObject == null)
                    {
                        postVolumeObject = new GameObject("Post Processing HDRP Volume");
                        postVolumeObject.layer = 1;
                        postVolumeObject.transform.SetParent(theParent.transform);
                    }
                }

                if (volume == null)
                {
                    volume = postVolumeObject.GetComponent<Volume>();
                    if (volume == null)
                    {
                        volume = postVolumeObject.AddComponent<Volume>();
                        volume.isGlobal = true;
                    }
                }

                bool loadProfile = false;
                if (volume.sharedProfile == null)
                {
                    loadProfile = true;
                }
                else
                {
                    if (volume.sharedProfile.name != profile.assetName + " HD")
                    {
                        loadProfile = true;
                    }
                }
                if (loadProfile)
                {
                    if (profile.name == "User")
                    {
                        if (profile.customHDRPPostProcessingprofile == null)
                        {
                            return;
                        }
                        if (volume.sharedProfile.name != profile.customHDRPPostProcessingprofile.name)
                        {
                            volume.sharedProfile = profile.customHDRPPostProcessingprofile;
                        }
                    }
                    else
                    {
                        //Get the profile path
                        string postProcessPath = profile.assetName + " HD";

                        if (string.IsNullOrEmpty(postProcessPath))
                        {
                            Debug.LogErrorFormat("AmbientSkies:SetPostProcessingProfile() : Unable to load '{0}' profile - Aborting!", profile.assetName);
                            return;
                        }

                        volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(postProcessPath));
                        volumeProfile = volume.sharedProfile;
                    }
                }

                if (volume.sharedProfile == null)
                {
                    return;
                }
                else
                {
                    //Volume shared profile
                    if (volumeProfile == null)
                    {
                        volumeProfile = volume.sharedProfile;
                    }

                    EditorUtility.SetDirty(volumeProfile);

#if HDPipeline && UNITY_2019_1_OR_NEWER
                    //Apply settings
                    SetAntiAliasing(skyProfile, profile, mainCameraObj, camera);

                    if (profile.name != "User")
                    {
                        if (updateAO)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPAmbientOcclusion()");
                            }

                            SetHDRPAmbientOcclusion(profile, volumeProfile);
                        }

                        if (updateAutoExposure)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPAutoExposure()");
                            }

                            SetHDRPAutoExposure(profile, volumeProfile);
                        }

                        if (updateBloom)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPBloom()");
                            }

                            SetHDRPBloom(profile, volumeProfile);
                        }

                        if (updateChromatic)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPChromaticAberration()");
                            }

                            SetHDRPChromaticAberration(profile, volumeProfile);
                        }

                        if (updateColorGrading)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPColorGrading()");
                            }

                            SetHDRPColorGrading(profile, volumeProfile);
                        }

                        if (updateDOF)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPDepthOfField()");
                            }

                            SetHDRPDepthOfField(profile, volumeProfile, camera);
                        }

                        if (updateGrain)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPGrain()");
                            }

                            SetHDRPGrain(profile, volumeProfile);
                        }

                        if (updateLensDistortion)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPLensDistortion()");
                            }

                            SetHDRPLensDistortion(profile, volumeProfile);
                        }



                        if (updateVignette)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPVignette()");
                            }

                            SetHDRPVignette(profile, volumeProfile);
                        }

                        if (updateMotionBlur)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPMotionBlur()");
                            }

                            SetHDRPMotionBlur(profile, volumeProfile);
                        }

                        if (updatePanini)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPPaniniProjection()");
                            }

                            SetHDRPPaniniProjection(profile, volumeProfile);
                        }
                    }
#endif
                }
            }
            else
            {
                GameObject postVolumeObject = GameObject.Find("Post Processing HDRP Volume");
                if (postVolumeObject != null)
                {
                    Object.DestroyImmediate(postVolumeObject);
                }
#endif
                }
#if !UNITY_2019_1_OR_NEWER
            else
            {
                RemovePostProcessing();
            }
#endif

#endif
        }

        /// <summary>
        /// Sets URP post processing 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="skyProfile"></param>
        /// <param name="useDefaults"></param>
        public static void SetURPPostProcessingProfile(AmbientPostProcessingProfile profile, AmbientSkyProfiles skyProfile, bool useDefaults, bool updateAO, bool updateAutoExposure, bool updateBloom, bool updateChromatic, bool updateColorGrading, bool updateDOF, bool updateGrain, bool updateLensDistortion, bool updateMotionBlur, bool updatePanini, bool updateVignette, bool updateTargetPlatform, bool updateSSR)
        {
            //Clear console if required
            if (skyProfile.m_smartConsoleClean)
            {
                SkyboxUtils.ClearLog();
                Debug.Log("Console cleared successfully");
            }
#if UPPipeline
            if (mainCameraObj == null)
            {
                mainCameraObj = SkyboxUtils.GetOrCreateMainCamera();
            }

            if (camera == null)
            {
                camera = mainCameraObj.GetComponent<Camera>();
            }

            if (camera != null)
            {
                if (urpCameraData == null)
                {
                    urpCameraData = camera.gameObject.GetComponent<UniversalAdditionalCameraData>();
                }
            }

            if (skyProfile.m_usePostFX)
            {
                RemovePostProcessing();

                if (urpCameraData != null)
                {
                    urpCameraData.renderPostProcessing = true;
                    switch (profile.hDRMode)
                    {
                        case AmbientSkiesConsts.HDRMode.On:
                            camera.allowHDR = true;
                            break;
                        case AmbientSkiesConsts.HDRMode.Off:
                            camera.allowHDR = false;
                            break;
                    }

                    switch (profile.antiAliasingMode)
                    {
                        case AmbientSkiesConsts.AntiAliasingMode.None:
                            urpCameraData.antialiasing = AntialiasingMode.None;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.FXAA:
                            urpCameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.SMAA:
                            urpCameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.MSAA:
                            urpCameraData.antialiasing = AntialiasingMode.None;
                            camera.allowMSAA = true;
                            break;
                    }
                }

                //Get the FX parent
                if (theParent == null)
                {
                    theParent = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Global", false);
                }

                if (mainCameraObj == null)
                {
                    mainCameraObj = SkyboxUtils.GetOrCreateMainCamera();
                }

                if (postVolumeObject == null)
                {
                    postVolumeObject = GameObject.Find("Post Processing URP Volume");
                    if (postVolumeObject == null)
                    {
                        postVolumeObject = new GameObject("Post Processing URP Volume");
                        postVolumeObject.transform.SetParent(theParent.transform);
                    }
                }

                if (volume == null)
                {
                    volume = postVolumeObject.GetComponent<Volume>();
                    if (volume == null)
                    {
                        volume = postVolumeObject.AddComponent<Volume>();
                        volume.isGlobal = true;
                    }
                }

                bool loadProfile = false;
                if (volume.sharedProfile == null)
                {
                    loadProfile = true;
                }
                else
                {
                    if (volume.sharedProfile.name != profile.assetName + " URP")
                    {
                        loadProfile = true;
                    }
                }
                if (loadProfile)
                {
                    if (profile.name == "User")
                    {
                        if (profile.customURPPostProcessingprofile == null)
                        {
                            return;
                        }
                        if (volume.sharedProfile == null)
                        {
                            volume.sharedProfile = profile.customURPPostProcessingprofile;
                        }
                        else if (volume.sharedProfile.name != profile.customURPPostProcessingprofile.name)
                        {
                            volume.sharedProfile = profile.customURPPostProcessingprofile;
                        }
                    }
                    else
                    {
                        //Get the profile path
                        string postProcessPath = profile.assetName + " URP";

                        if (string.IsNullOrEmpty(postProcessPath))
                        {
                            Debug.LogErrorFormat("AmbientSkies:SetPostProcessingProfile() : Unable to load '{0}' profile - Aborting!", profile.assetName);
                            return;
                        }

                        volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(postProcessPath));
                        volumeProfile = volume.sharedProfile;
                    }
                }

                if (volume.sharedProfile == null)
                {
                    return;
                }
                else
                {
                    //Volume shared profile
                    if (volumeProfile == null)
                    {
                        volumeProfile = volume.sharedProfile;
                    }

                    EditorUtility.SetDirty(volumeProfile);

#if UPPipeline && UNITY_2019_3_OR_NEWER
                    //Apply settings
                    SetAntiAliasing(skyProfile, profile, mainCameraObj, camera);

                    if (profile.name != "User")
                    {
                        if (updateAO)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPAmbientOcclusion()");
                            }

                            SetURPAmbientOcclusion(profile, volumeProfile);
                        }

                        if (updateAutoExposure)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPAutoExposure()");
                            }

                            SetURPAutoExposure(profile, volumeProfile);
                        }

                        if (updateBloom)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPBloom()");
                            }

                            SetURPBloom(profile, volumeProfile);
                        }

                        if (updateChromatic)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPChromaticAberration()");
                            }

                            SetURPChromaticAberration(profile, volumeProfile);
                        }

                        if (updateColorGrading)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPColorGrading()");
                            }

                            SetURPColorGrading(profile, volumeProfile);
                        }

                        if (updateDOF)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPDepthOfField()");
                            }

                            SetURPDepthOfField(profile, volumeProfile, camera);
                        }

                        if (updateGrain)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPGrain()");
                            }

                            SetURPGrain(profile, volumeProfile);
                        }

                        if (updateLensDistortion)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPLensDistortion()");
                            }

                            SetURPLensDistortion(profile, volumeProfile);
                        }

                        if (updateVignette)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPVignette()");
                            }

                            SetURPVignette(profile, volumeProfile);
                        }

                        if (updateMotionBlur)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPMotionBlur()");
                            }

                            SetURPMotionBlur(profile, volumeProfile);
                        }

                        if (updatePanini)
                        {
                            if (skyProfile.m_showFunctionDebugsOnly)
                            {
                                Debug.Log("Updating SetHDRPPaniniProjection()");
                            }

                            SetURPPaniniProjection(profile, volumeProfile);
                        }
                    }
#endif
                }
            }
            else
            {
                if (urpCameraData != null)
                {
                    urpCameraData.renderPostProcessing = true;
                }

                GameObject postVolumeObject = GameObject.Find("Post Processing URP Volume");
                if (postVolumeObject != null)
                {
                    Object.DestroyImmediate(postVolumeObject);
                }
            }
#endif
#if !UNITY_2019_1_OR_NEWER
            else
            {
                RemovePostProcessing();
            }
#endif
        }

        /// <summary>
        /// Remove post processing from camera and scene
        /// </summary>
        public static void RemovePostProcessing()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            //Remove from camera
            GameObject mainCameraObj = SkyboxUtils.GetOrCreateMainCamera();
            PostProcessLayer cameraProcessLayer = mainCameraObj.GetComponent<PostProcessLayer>();
            GameObject postProcessVolumeObj = GameObject.Find("Global Post Processing");

            if (cameraProcessLayer == null && postProcessVolumeObj == null)
            {
                return;
            }

            if (cameraProcessLayer != null)
            {
                Object.DestroyImmediate(cameraProcessLayer);
            }

            //Remove from scene
            if (postProcessVolumeObj != null)
            {
                Object.DestroyImmediate(postProcessVolumeObj);
            }

#if !UNITY_2019_1_OR_NEWER
            AutoDepthOfField autoFocus = Object.FindObjectOfType<AutoDepthOfField>();
            if (autoFocus != null)
            {
                Object.DestroyImmediate(autoFocus);
            }
#endif
#endif
        }

        /// <summary>
        /// Removes HDRP post processing object
        /// </summary>
        public static void RemoveHDRPPostProcessing(AmbientSkyProfiles skyProfile)
        {
            if (skyProfile.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                GameObject postProcessingHDRPObject = GameObject.Find("Post Processing HDRP Volume");
                if (postProcessingHDRPObject != null)
                {
                    Object.DestroyImmediate(postProcessingHDRPObject);
                }
            }
        }

        /// <summary>
        /// Hides the gizmo of post processing
        /// </summary>
        /// <param name="profile"></param>
        public static void HidePostProcessingGizmo(AmbientPostProcessingProfile profile)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (postProcessVolume == null)
            {
                postProcessVolume = Object.FindObjectOfType<PostProcessVolume>();
            }
            else
            {
                if (profile.hideGizmos)
                {
                    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(postProcessVolume, false);
                }
                else
                {
                    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(postProcessVolume, true);
                }
                
            }
#endif
        }

        /// <summary>
        /// Focuses the post processing profile
        /// </summary>
        /// <param name="profile"></param>
        public static void FocusPostProcessProfile(AmbientSkyProfiles skyProfiles, AmbientPostProcessingProfile profile)
        {
#if UNITY_2019_1_OR_NEWER
            //If HDRP
            if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline
                //Profile searched for
                VolumeProfile processProfile;

                //Get the profile path
                string postProcessPath = profile.assetName + " HD";

                string newPostProcessPath = SkyboxUtils.GetAssetPath(postProcessPath);

                if (string.IsNullOrEmpty(postProcessPath))
                {
                    Debug.LogErrorFormat("AmbientSkies:SetPostProcessingProfile() : Unable to load '{0}' profile - Aborting!", profile.assetName);
                    return;
                }
                else
                {
                    processProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(postProcessPath));
                    if (processProfile != null)
                    {
                        Selection.activeObject = processProfile;
                        ///
                        ///Caused a error using this
                        ///EditorUtility.FocusProjectWindow();
                        ///
                    }
                }
#endif
            }
            else
            {
                //Profile searched for
#if UNITY_POST_PROCESSING_STACK_V2
                PostProcessVolume volume = Object.FindObjectOfType<PostProcessVolume>();
                if (volume != null)
                {
                    PostProcessProfile processProfile = volume.sharedProfile;
                    if (processProfile != null)
                    {
                        Selection.activeObject = processProfile;
                        ///
                        ///Caused a error using this
                        ///EditorUtility.FocusProjectWindow();
                        ///
                    }
                    else
                    {
                        Debug.LogErrorFormat("Unable to focus profile, asset could not be found.");
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Unable to find post process volume, asset can not be found as volume does not exist.");
                }
#endif
            }

#else

            //Profile searched for

#if UNITY_POST_PROCESSING_STACK_V2
            PostProcessVolume volume = Object.FindObjectOfType<PostProcessVolume>();
            if (volume != null)
            {
                PostProcessProfile processProfile;

                processProfile = volume.sharedProfile;
                if (processProfile != null)
                {
                    Selection.activeObject = processProfile;
                    ///
                    ///Caused a error using this
                    ///EditorUtility.FocusProjectWindow();
                    ///
                }
                else
                {
                    Debug.LogErrorFormat("Unable to focus profile, asset could not be found.");
                    return;
                }
            }
            else
            {
                Debug.LogErrorFormat("Unable to find post process volume, asset can not be found as volume does not exist.");
                return;
            }
#endif

#endif
        }


        #region Apply Settings

        /// <summary>
        /// Sets camera and anti aliasing
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="mainCameraObj"></param>
        /// <param name="camera"></param>
        public static void SetAntiAliasing(AmbientSkyProfiles skyProfiles, AmbientPostProcessingProfile profile, GameObject mainCameraObj, Camera camera)
        {       
            if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline && UNITY_2019_1_OR_NEWER
                #if UNITY_POST_PROCESSING_STACK_V2
                PostProcessLayer cameraProcessLayer = Object.FindObjectOfType<PostProcessLayer>();
                if (cameraProcessLayer != null)
                {
                    Object.DestroyImmediate(cameraProcessLayer);
                }
                #endif

                if (cameraData == null)
                {
                    cameraData = mainCameraObj.GetComponent<HDAdditionalCameraData>();
                }
                else
                {
                    cameraData.dithering = profile.dithering;
                    switch (profile.antiAliasingMode)
                    {
                        case AmbientSkiesConsts.AntiAliasingMode.None:
                            cameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.FXAA:
                            cameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.TAA:
                            cameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.MSAA:
                            EditorUtility.DisplayDialog("Warning!", "MSAA is not supported in 2019 HDRP, switching to FXAA", "Ok");
                            profile.antiAliasingMode = AmbientSkiesConsts.AntiAliasingMode.FXAA;
                            break;
                    }
                }
#else
                #if UNITY_POST_PROCESSING_STACK_V2
                if (processLayer == null)
                {
                    processLayer = mainCameraObj.GetComponent<PostProcessLayer>();
                }
                else
                {
                    switch (profile.antiAliasingMode)
                    {
                        case AmbientSkiesConsts.AntiAliasingMode.None:
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.FXAA:
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.SMAA:
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.TAA:
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                            camera.allowMSAA = false;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.MSAA:
                            camera.allowMSAA = true;
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                            profile.antiAliasingMode = AmbientSkiesConsts.AntiAliasingMode.FXAA;
                            break;
                    }
                }
                #endif
#endif
            }
            else if (skyProfiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.Universal)
            {
#if UPPipeline
                if (camera != null)
                {
                    if (urpCameraData == null)
                    {
                        urpCameraData = camera.gameObject.GetComponent<UniversalAdditionalCameraData>();
                    }
                }

                if (urpCameraData != null)
                {
                    urpCameraData.dithering = profile.dithering;
                    switch (profile.antiAliasingMode)
                    {
                        case AmbientSkiesConsts.AntiAliasingMode.None:
                            urpCameraData.antialiasing = AntialiasingMode.None;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.FXAA:
                            urpCameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                            break;
                        case AmbientSkiesConsts.AntiAliasingMode.SMAA:
                            urpCameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                            break;
                    }
                }
#endif
            }
            else
            {
                //Checks to see if the camera is there
                if (camera != null)
                {
                    if (profile.hDRMode == AmbientSkiesConsts.HDRMode.On)
                    {
                        camera.allowHDR = true;
                    }
                    else
                    {
                        camera.allowHDR = false;
                    }
                }

                #if UNITY_POST_PROCESSING_STACK_V2

                //Setup the camera up to support post processing
                if (processLayer == null)
                {
                    processLayer = mainCameraObj.GetComponent<PostProcessLayer>();
                    if (processLayer == null)
                    {
                        processLayer = mainCameraObj.AddComponent<PostProcessLayer>();
                    }
                }

                processLayer.volumeTrigger = mainCameraObj.transform;
                processLayer.volumeLayer = 2;
                processLayer.fog.excludeSkybox = true;
                processLayer.fog.enabled = true;
                processLayer.stopNaNPropagation = true;

                switch (profile.antiAliasingMode)
                {
                    case AmbientSkiesConsts.AntiAliasingMode.None:
                        processLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.FXAA:
                        processLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.SMAA:
                        processLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.TAA:
                        processLayer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.MSAA:
                        if (camera.renderingPath == RenderingPath.DeferredShading)
                        {
                            if (EditorUtility.DisplayDialog("Warning!", "MSAA is not supported in deferred rendering path. Switching Anti Aliasing to none, please select another option that supported in deferred rendering path or set your rendering path to forward to use MSAA", "Ok"))
                            {
                                processLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                                profile.antiAliasingMode = AmbientSkiesConsts.AntiAliasingMode.None;
                            }
                        }
                        else
                        {
                            processLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                            camera.allowMSAA = true;
                        }
                        break;
                }
#endif
            }
        }

        #region Built-In and LWRP

        #if UNITY_POST_PROCESSING_STACK_V2

        /// <summary>
        /// Sets ambient occlusion settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetAmbientOcclusion(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.AmbientOcclusion ao;
            if (postProcessProfile.TryGetSettings(out ao))
            {
                ao.active = profile.aoEnabled;
                ao.intensity.value = profile.aoAmount;
                ao.color.value = profile.aoColor;
                if (!ao.color.overrideState)
                {
                    ao.color.overrideState = true;
                }
#if UNITY_POST_PROCESSING_STACK_V2
                ao.mode.value = profile.ambientOcclusionMode;
#endif
            }
        }

        /// <summary>
        /// Sets auto exposure settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetAutoExposure(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            AutoExposure autoExposure;
            if (postProcessProfile.TryGetSettings(out autoExposure))
            {
                autoExposure.active = profile.autoExposureEnabled;
                autoExposure.keyValue.value = profile.exposureAmount;
                autoExposure.minLuminance.value = profile.exposureMin;
                autoExposure.maxLuminance.value = profile.exposureMax;
            }
        }

        /// <summary>
        /// Sets bloom settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetBloom(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.Bloom bloom;
            if (postProcessProfile.TryGetSettings(out bloom))
            {
                bloom.active = profile.bloomEnabled;
                bloom.intensity.value = profile.bloomAmount;
                bloom.threshold.value = profile.bloomThreshold;
                bloom.dirtTexture.value = profile.lensTexture;
                bloom.dirtIntensity.value = profile.lensIntensity;
            }
        }

        /// <summary>
        /// Sets chromatic aberration settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetChromaticAberration(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.ChromaticAberration chromaticAberration;
            if (postProcessProfile.TryGetSettings(out chromaticAberration))
            {
                chromaticAberration.active = profile.chromaticAberrationEnabled;
                chromaticAberration.intensity.value = profile.chromaticAberrationIntensity;
            }
        }

        /// <summary>
        /// Sets color grading settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetColorGrading(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            ColorGrading colorGrading;
            if (postProcessProfile.TryGetSettings(out colorGrading))
            {
                colorGrading.active = profile.colorGradingEnabled;

                colorGrading.gradingMode.value = profile.colorGradingMode;
                colorGrading.gradingMode.overrideState = true;

                colorGrading.ldrLut.value = profile.colorGradingLut;
                colorGrading.ldrLut.overrideState = true;

                colorGrading.externalLut.value = profile.colorGradingLut;
                colorGrading.externalLut.overrideState = true;

                colorGrading.colorFilter.value = profile.colorGradingColorFilter;
                colorGrading.colorFilter.overrideState = true;
                colorGrading.postExposure.value = profile.colorGradingPostExposure;
                colorGrading.postExposure.overrideState = true;
                colorGrading.temperature.value = profile.colorGradingTempature;
                colorGrading.tint.value = profile.colorGradingTint;
                colorGrading.saturation.value = profile.colorGradingSaturation;
                colorGrading.contrast.value = profile.colorGradingContrast;

                colorGrading.mixerRedOutRedIn.overrideState = true;
                colorGrading.mixerBlueOutBlueIn.overrideState = true;
                colorGrading.mixerGreenOutGreenIn.overrideState = true;
                colorGrading.mixerRedOutRedIn.value = profile.channelMixerRed;
                colorGrading.mixerBlueOutBlueIn.value = profile.channelMixerBlue;
                colorGrading.mixerGreenOutGreenIn.value = profile.channelMixerGreen;
            }
        }

        /// <summary>
        /// Sets depth of field settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetDepthOfField(AmbientPostProcessingProfile profile, Camera camera, PostProcessProfile postProcessProfile)
        {
            if (profile.depthOfFieldEnabled)
            {
#if !GAIA_2_PRESENT
                if (profile.depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                {
                    AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                    if (autoFocus == null)
                    {
                        autoFocus = camera.gameObject.AddComponent<AutoDepthOfField>();
                        autoFocus.m_processingProfile = profile;
                        autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                        autoFocus.m_focusOffset = profile.focusOffset;
                        autoFocus.m_targetLayer = profile.targetLayer;
                        autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                        autoFocus.m_actualFocusDistance = profile.depthOfFieldFocusDistance;
                        profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                    }
                    else
                    {
                        autoFocus.m_processingProfile = profile;
                        autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                        autoFocus.m_focusOffset = profile.focusOffset;
                        autoFocus.m_targetLayer = profile.targetLayer;
                        autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                        profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                    }
                }
                else
                {
                    AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                    if (autoFocus != null)
                    {
                        Object.DestroyImmediate(autoFocus);
                    }
                }
#endif

                UnityEngine.Rendering.PostProcessing.DepthOfField dof;
                if (postProcessProfile.TryGetSettings(out dof))
                {
                    dof.active = profile.depthOfFieldEnabled;
                    if (profile.depthOfFieldMode != AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                    {
                        dof.focusDistance.value = profile.depthOfFieldFocusDistance;
                    }
                    else
                    {
                        profile.depthOfFieldFocusDistance = dof.focusDistance.value;
                    }

                    dof.aperture.value = profile.depthOfFieldAperture;
                    dof.focalLength.value = profile.depthOfFieldFocalLength;
                    dof.kernelSize.value = profile.maxBlurSize;
                }
            }
            else
            {
                UnityEngine.Rendering.PostProcessing.DepthOfField dof;
                if (postProcessProfile.TryGetSettings(out dof))
                {
                    dof.active = profile.depthOfFieldEnabled;
                }
#if !GAIA_2_PRESENT
                AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                if (autoFocus != null)
                {
                    Object.DestroyImmediate(autoFocus);
                }
#endif
            }
        }

        /// <summary>
        /// Sets grain settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetGrain(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            Grain grain;
            if (postProcessProfile.TryGetSettings(out grain))
            {
                grain.active = profile.grainEnabled;
                grain.intensity.value = profile.grainIntensity;
                grain.size.value = profile.grainSize;
            }
        }

        /// <summary>
        /// Sets lens distortion settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetLensDistortion(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.LensDistortion lensDistortion = null;
            if (postProcessProfile.TryGetSettings(out lensDistortion))
            {
                if (profile.distortionEnabled && profile.distortionIntensity > 0f)
                {
                    lensDistortion.active = profile.distortionEnabled;
                    lensDistortion.intensity.value = profile.distortionIntensity;
                    lensDistortion.scale.value = profile.distortionScale;
                }
                else
                {
                    lensDistortion.active = false;
                }
            }
        }

        /// <summary>
        /// Sets screen space reflections settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetScreenSpaceReflections(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            ScreenSpaceReflections screenSpaceReflections = null;
            if (postProcessProfile.TryGetSettings(out screenSpaceReflections))
            {
                if (profile.screenSpaceReflectionPreset == ScreenSpaceReflectionPreset.Custom)
                {
                    screenSpaceReflections.maximumIterationCount.overrideState = true;
                    screenSpaceReflections.thickness.overrideState = true;
                    screenSpaceReflections.resolution.overrideState = true;
                }
                else
                {
                    screenSpaceReflections.maximumIterationCount.overrideState = false;
                    screenSpaceReflections.thickness.overrideState = false;
                    screenSpaceReflections.resolution.overrideState = false;
                }

                screenSpaceReflections.active = profile.screenSpaceReflectionsEnabled;
                screenSpaceReflections.maximumIterationCount.value = profile.maximumIterationCount;
                screenSpaceReflections.thickness.value = profile.thickness;
                screenSpaceReflections.resolution.value = profile.spaceReflectionResolution;
                screenSpaceReflections.preset.value = profile.screenSpaceReflectionPreset;
                screenSpaceReflections.maximumMarchDistance.value = profile.maximumMarchDistance;
                screenSpaceReflections.distanceFade.value = profile.distanceFade;
                screenSpaceReflections.vignette.value = profile.screenSpaceVignette;
            }
        }

        /// <summary>
        /// Sets vignette settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetVignette(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.Vignette vignette = null;
            if (postProcessProfile.TryGetSettings(out vignette))
            {
                if (profile.vignetteEnabled && profile.vignetteIntensity > 0f)
                {
                    vignette.active = profile.vignetteEnabled;
                    vignette.intensity.value = profile.vignetteIntensity;
                    vignette.smoothness.value = profile.vignetteSmoothness;
                }
                else
                {
                    vignette.active = false;
                }
            }
        }

        /// <summary>
        /// Sets motion blur settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetMotionBlur(AmbientPostProcessingProfile profile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.MotionBlur motionBlur;
            if (postProcessProfile.TryGetSettings(out motionBlur))
            {
                motionBlur.active = profile.motionBlurEnabled;
                motionBlur.shutterAngle.value = profile.shutterAngle;
                motionBlur.sampleCount.value = profile.sampleCount;
            }

        }

        /// <summary>
        /// Sets target platform settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetTargetPlatform(AmbientSkyProfiles skyProfile, PostProcessProfile postProcessProfile)
        {
            UnityEngine.Rendering.PostProcessing.AmbientOcclusion ao;
            UnityEngine.Rendering.PostProcessing.Bloom bloom;
            UnityEngine.Rendering.PostProcessing.ChromaticAberration chromaticAberration;
            if (skyProfile.m_targetPlatform == AmbientSkiesConsts.PlatformTarget.DesktopAndConsole)
            {
                if (postProcessProfile.TryGetSettings(out bloom))
                {
                    bloom.fastMode.overrideState = true;
                    bloom.fastMode.value = false;
                }

                if (postProcessProfile.TryGetSettings(out chromaticAberration))
                {
                    chromaticAberration.fastMode.overrideState = true;
                    chromaticAberration.fastMode.value = false;
                }

                if (postProcessProfile.TryGetSettings(out ao))
                {
                    ao.ambientOnly.overrideState = true;
                    ao.ambientOnly.value = true;
                }
            }
            else
            {
                if (postProcessProfile.TryGetSettings(out bloom))
                {
                    bloom.fastMode.overrideState = true;
                    bloom.fastMode.value = true;
                }

                if (postProcessProfile.TryGetSettings(out chromaticAberration))
                {
                    chromaticAberration.fastMode.overrideState = true;
                    chromaticAberration.fastMode.value = true;
                }
                if (postProcessProfile.TryGetSettings(out ao))
                {
                    ao.ambientOnly.overrideState = true;
                    ao.ambientOnly.value = false;
                }
            }
        }
        #endif

        #endregion

        #region HDRP

#if HDPipeline && UNITY_2019_1_OR_NEWER

        /// <summary>
        /// Sets ambient occlusion settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetHDRPAmbientOcclusion(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.AmbientOcclusion ao;
            if (volumeProfile.TryGet(out ao))
            {
                ao.active = profile.aoEnabled;
                ao.intensity.value = profile.hDRPAOIntensity;
#if !UNITY_2019_2_OR_NEWER
                ao.thicknessModifier.value = profile.hDRPAOThicknessModifier;
#else
                ao.radius.value = profile.hDRPAOThicknessModifier;
#endif
                ao.directLightingStrength.value = profile.hDRPAODirectLightingStrength;
            }
#else
            UnityEngine.Rendering.HighDefinition.AmbientOcclusion ao;
            if (volumeProfile.TryGet(out ao))
            {
                ao.active = profile.aoEnabled;
                ao.intensity.value = profile.hDRPAOIntensity;
                ao.directLightingStrength.value = profile.hDRPAODirectLightingStrength;
                ao.radius.value = profile.hDRPAORadius;
                switch(profile.hDRPAOQuality)
                {
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Custom:
                        ao.quality.value = 0;
                        ao.maximumRadiusInPixels = profile.hDRPAOMaximumRadiusInPixels;
                        ao.fullResolution = profile.hDRPAOFullResolution;
                        ao.stepCount = profile.hDRPAOStepCount;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Low:
                        ao.quality.value = 1;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Medium:
                        ao.quality.value = 2;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.High:
                        ao.quality.value = 3;
                        break;
                }
                ao.temporalAccumulation.value = profile.hDRPAOTemporalAccumulation;
                ao.ghostingReduction.value = profile.hDRPAOGhostingReduction;
            }
#endif
        }

        /// <summary>
        /// Sets auto exposure settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetHDRPAutoExposure(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.Exposure exposure;
#else
            UnityEngine.Rendering.HighDefinition.Exposure exposure;
#endif
            if (volumeProfile.TryGet(out exposure))
            {
                //exposure.SetAllOverridesTo(true);

                exposure.active = profile.autoExposureEnabled;
                exposure.mode.value = profile.hDRPExposureMode;
                exposure.fixedExposure.value = profile.hDRPExposureFixedExposure;
                exposure.compensation.value = profile.hDRPExposureCompensation;
                exposure.meteringMode.value = profile.hDRPExposureMeteringMode;
                exposure.luminanceSource.value = profile.hDRPExposureLuminationSource;
                exposure.curveMap.value = profile.hDRPExposureCurveMap;
                exposure.limitMin.value = profile.hDRPExposureLimitMin;
                exposure.limitMax.value = profile.hDRPExposureLimitMax;
                exposure.adaptationMode.value = profile.hDRPExposureAdaptionMode;
                exposure.adaptationSpeedDarkToLight.value = profile.hDRPExposureAdaptionSpeedDarkToLight;
                exposure.adaptationSpeedLightToDark.value = profile.hDRPExposureAdaptionSpeedLightToDark;
            }
        }

        /// <summary>
        /// Sets bloom settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetHDRPBloom(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.Bloom bloom;
            if (volumeProfile.TryGet(out bloom))
            {
                bloom.active = profile.bloomEnabled;
                bloom.intensity.value = profile.hDRPBloomIntensity;
                bloom.scatter.value = profile.hDRPBloomScatter;
                bloom.tint.value = profile.hDRPBloomTint;
                bloom.dirtTexture.value = profile.hDRPBloomDirtLensTexture;
                bloom.dirtIntensity.value = profile.hDRPBloomDirtLensIntensity;

                bloom.resolution.value = profile.hDRPBloomResolution;
                bloom.highQualityFiltering.value = profile.hDRPBloomHighQualityFiltering;
                bloom.prefilter.value = profile.hDRPBloomPrefiler;
                bloom.anamorphic.value = profile.hDRPBloomAnamorphic;
            }
#else
            UnityEngine.Rendering.HighDefinition.Bloom bloom;
            if (volumeProfile.TryGet(out bloom))
            {
                bloom.active = profile.bloomEnabled;
                switch(profile.hDRPBloomQuality)
                {
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Custom:
                        bloom.quality.value = 0;
                        bloom.resolution = profile.hDRPBloomResolution;
                        bloom.highQualityFiltering = profile.hDRPBloomHighQualityFiltering;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Low:
                        bloom.quality.value = 1;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Medium:
                        bloom.quality.value = 2;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.High:
                        bloom.quality.value = 3;
                        break;
                }

                bloom.threshold.value = profile.hDRPBloomThreshold;
                bloom.intensity.value = profile.hDRPBloomIntensity;
                bloom.scatter.value = profile.hDRPBloomScatter;
                bloom.tint.value = profile.hDRPBloomTint;
                bloom.dirtTexture.value = profile.hDRPBloomDirtLensTexture;
                bloom.dirtIntensity.value = profile.hDRPBloomDirtLensIntensity;
                bloom.anamorphic.value = profile.hDRPBloomAnamorphic;
            }
#endif
        }

        /// <summary>
        /// Sets chromatic aberration settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetHDRPChromaticAberration(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.ChromaticAberration chromaticAberration;
            if (volumeProfile.TryGet(out chromaticAberration))
            {
                chromaticAberration.active = profile.chromaticAberrationEnabled;
                chromaticAberration.spectralLut.value = profile.hDRPChromaticAberrationSpectralLut;
                chromaticAberration.intensity.value = profile.hDRPChromaticAberrationIntensity;
                chromaticAberration.maxSamples.value = profile.hDRPChromaticAberrationMaxSamples;
            }
#else
            UnityEngine.Rendering.HighDefinition.ChromaticAberration chromaticAberration;
            if (volumeProfile.TryGet(out chromaticAberration))
            {
                chromaticAberration.active = profile.chromaticAberrationEnabled;
                switch (profile.hDRPChromaticAberrationQuality)
                {
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Custom:
                        chromaticAberration.quality.value = 0;
                        chromaticAberration.maxSamples = profile.hDRPChromaticAberrationMaxSamples;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Low:
                        chromaticAberration.quality.value = 1;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Medium:
                        chromaticAberration.quality.value = 2;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.High:
                        chromaticAberration.quality.value = 3;
                        break;
                }
                chromaticAberration.spectralLut.value = profile.hDRPChromaticAberrationSpectralLut;
                chromaticAberration.intensity.value = profile.hDRPChromaticAberrationIntensity;
            }
#endif
        }

        /// <summary>
        /// Sets color grading settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPColorGrading(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.ColorAdjustments colorAdjustments;
#else
            UnityEngine.Rendering.HighDefinition.ColorAdjustments colorAdjustments;
#endif
            if (volumeProfile.TryGet(out colorAdjustments))
            {
                colorAdjustments.active = profile.colorGradingEnabled;
                colorAdjustments.postExposure.value = profile.hDRPColorAdjustmentPostExposure;
                colorAdjustments.contrast.value = profile.hDRPColorAdjustmentContrast;
                colorAdjustments.colorFilter.value = profile.hDRPColorAdjustmentColorFilter;
                colorAdjustments.hueShift.value = profile.hDRPColorAdjustmentHueShift;
                colorAdjustments.saturation.value = profile.hDRPColorAdjustmentSaturation;
            }

#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.ChannelMixer channelMixer;
#else
            UnityEngine.Rendering.HighDefinition.ChannelMixer channelMixer;
#endif
            if (volumeProfile.TryGet(out channelMixer))
            {
                channelMixer.active = profile.colorGradingEnabled;
                channelMixer.redOutRedIn.value = profile.hDRPChannelMixerRed;
                channelMixer.greenOutGreenIn.value = profile.hDRPChannelMixerGreen;
                channelMixer.blueOutBlueIn.value = profile.hDRPChannelMixerBlue;
            }

#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.ColorLookup colorLookup;
            if (volumeProfile.TryGet(out colorLookup))
            {
                colorLookup.active = profile.colorGradingEnabled;
                colorLookup.texture.value = profile.hDRPColorLookupTexture;
                colorLookup.contribution.value = profile.hDRPColorLookupContribution;
            }
#endif

#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.Tonemapping tonemapping;
#else
            UnityEngine.Rendering.HighDefinition.Tonemapping tonemapping;
#endif
            if (volumeProfile.TryGet(out tonemapping))
            {
                tonemapping.active = profile.colorGradingEnabled;
                tonemapping.mode.value = profile.hDRPTonemappingMode;
                tonemapping.toeStrength.value = profile.hDRPTonemappingToeStrength;
                tonemapping.toeLength.value = profile.hDRPTonemappingToeLength;
                tonemapping.shoulderStrength.value = profile.hDRPTonemappingShoulderStrength;
                tonemapping.shoulderLength.value = profile.hDRPTonemappingShoulderLength;
                tonemapping.shoulderAngle.value = profile.hDRPTonemappingShoulderAngle;
                tonemapping.gamma.value = profile.hDRPTonemappingGamma;
            }

#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.SplitToning splitToning;
#else
            UnityEngine.Rendering.HighDefinition.SplitToning splitToning;
#endif
            if (volumeProfile.TryGet(out splitToning))
            {
                splitToning.active = profile.colorGradingEnabled;
                splitToning.shadows.value = profile.hDRPSplitToningShadows;
                splitToning.highlights.value = profile.hDRPSplitToningHighlights;
                splitToning.balance.value = profile.hDRPSplitToningBalance;
            }

#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.WhiteBalance whiteBalance;
#else
            UnityEngine.Rendering.HighDefinition.WhiteBalance whiteBalance;
#endif
            if (volumeProfile.TryGet(out whiteBalance))
            {
                whiteBalance.active = profile.colorGradingEnabled;
                whiteBalance.temperature.value = profile.hDRPWhiteBalanceTempature;
                whiteBalance.tint.value = profile.hDRPWhiteBalanceTint;
            }
        }

        /// <summary>
        /// Sets depth of field settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPDepthOfField(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile, Camera camera)
        {
#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.DepthOfField dof;
            if (volumeProfile.TryGet(out dof))
            {
                if (profile.depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus && profile.depthOfFieldEnabled)
                {
                    AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                    if (autoFocus == null)
                    {
                        autoFocus = camera.gameObject.AddComponent<AutoDepthOfField>();
                        autoFocus.m_processingProfile = profile;
                        autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                        autoFocus.m_focusOffset = profile.focusOffset;
                        autoFocus.m_targetLayer = profile.targetLayer;
                        autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                        profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                        autoFocus.m_actualFocusDistance = profile.depthOfFieldFocusDistance;
                    }
                    else
                    {
                        autoFocus.m_processingProfile = profile;
                        autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                        autoFocus.m_focusOffset = profile.focusOffset;
                        autoFocus.m_targetLayer = profile.targetLayer;
                        profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                        autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                    }
                }
                else
                {
                    AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                    if (autoFocus != null)
                    {
                        Object.DestroyImmediate(autoFocus);
                    }
                }

                dof.active = profile.depthOfFieldEnabled;
                if (profile.depthOfFieldMode != AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                {
                    dof.focusDistance.value = profile.hDRPDepthOfFieldFocusDistance;
                    dof.focusMode.value = DepthOfFieldMode.Manual;
                }
                else
                {
                    dof.focusMode.value = DepthOfFieldMode.UsePhysicalCamera;
                }

                dof.nearFocusStart.value = profile.hDRPDepthOfFieldNearBlurStart;
                dof.nearFocusEnd.value = profile.hDRPDepthOfFieldNearBlurEnd;
                dof.nearSampleCount.value = profile.hDRPDepthOfFieldNearBlurSampleCount;
                dof.nearMaxBlur.value = profile.hDRPDepthOfFieldNearBlurMaxRadius;

                dof.farFocusStart.value = profile.hDRPDepthOfFieldFarBlurStart;
                dof.farFocusEnd.value = profile.hDRPDepthOfFieldFarBlurEnd;
                dof.farSampleCount.value = profile.hDRPDepthOfFieldFarBlurSampleCount;
                dof.farMaxBlur.value = profile.hDRPDepthOfFieldFarBlurMaxRadius;

                dof.resolution.value = profile.hDRPDepthOfFieldResolution;
                dof.highQualityFiltering.value = profile.hDRPDepthOfFieldHighQualityFiltering;
            }
#else
            UnityEngine.Rendering.HighDefinition.DepthOfField dof;
            if (volumeProfile.TryGet(out dof))
            {
#if UNITY_POST_PROCESSING_STACK_V2 && !GAIA_2_PRESENT
                if (profile.depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus && profile.depthOfFieldEnabled)
                {
                    AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                    if (autoFocus == null)
                    {
                        autoFocus = camera.gameObject.AddComponent<AutoDepthOfField>();
                        autoFocus.m_processingProfile = profile;
                        autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                        autoFocus.m_focusOffset = profile.focusOffset;
                        autoFocus.m_targetLayer = profile.targetLayer;
                        autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                        profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                        autoFocus.m_actualFocusDistance = profile.depthOfFieldFocusDistance;
                    }
                    else
                    {
                        autoFocus.m_processingProfile = profile;
                        autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                        autoFocus.m_focusOffset = profile.focusOffset;
                        autoFocus.m_targetLayer = profile.targetLayer;
                        profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                        autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                    }
                }
                else
                {
                    AutoDepthOfField autoFocus = camera.gameObject.GetComponent<AutoDepthOfField>();
                    if (autoFocus != null)
                    {
                        Object.DestroyImmediate(autoFocus);
                    }
                }
#endif

                dof.active = profile.depthOfFieldEnabled;
                if (profile.depthOfFieldMode != AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                {
                    dof.focusDistance.value = profile.hDRPDepthOfFieldFocusDistance;
                    dof.focusMode.value = UnityEngine.Rendering.HighDefinition.DepthOfFieldMode.Manual;
                }
                else
                {
                    dof.focusMode.value = UnityEngine.Rendering.HighDefinition.DepthOfFieldMode.UsePhysicalCamera;
                }

                switch(profile.hDRPDepthOfFieldQuality)
                {
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Custom:
                        dof.quality.value = 0;
                        dof.nearSampleCount = profile.hDRPDepthOfFieldNearBlurSampleCount;
                        dof.nearMaxBlur = profile.hDRPDepthOfFieldNearBlurMaxRadius;
                        dof.farSampleCount = profile.hDRPDepthOfFieldFarBlurSampleCount;
                        dof.farMaxBlur = profile.hDRPDepthOfFieldFarBlurMaxRadius;
                        dof.resolution = profile.hDRPDepthOfFieldResolution;
                        dof.highQualityFiltering = profile.hDRPDepthOfFieldHighQualityFiltering;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Low:
                        dof.quality.value = 1;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Medium:
                        dof.quality.value = 2;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.High:
                        dof.quality.value = 3;
                        break;
                }

                dof.nearFocusStart.value = profile.hDRPDepthOfFieldNearBlurStart;
                dof.nearFocusEnd.value = profile.hDRPDepthOfFieldNearBlurEnd;
                dof.farFocusStart.value = profile.hDRPDepthOfFieldFarBlurStart;
                dof.farFocusEnd.value = profile.hDRPDepthOfFieldFarBlurEnd;
            }
#endif
        }

        /// <summary>
        /// Sets grain settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPGrain(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.FilmGrain grain;
#else
            UnityEngine.Rendering.HighDefinition.FilmGrain grain;
#endif
            if (volumeProfile.TryGet(out grain))
            {
                grain.active = profile.grainEnabled;
                grain.type.value = profile.hDRPFilmGrainType;
                grain.intensity.value = profile.hDRPFilmGrainIntensity;
                grain.response.value = profile.hDRPFilmGrainResponse;
            }
        }

        /// <summary>
        /// Sets lens distortion settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPLensDistortion(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.LensDistortion lensDistortion;
#else
            UnityEngine.Rendering.HighDefinition.LensDistortion lensDistortion;
#endif
            if (volumeProfile.TryGet(out lensDistortion))
            {
                lensDistortion.active = profile.distortionEnabled;
                lensDistortion.intensity.value = profile.hDRPLensDistortionIntensity;
                lensDistortion.xMultiplier.value = profile.hDRPLensDistortionXMultiplier;
                lensDistortion.yMultiplier.value = profile.hDRPLensDistortionYMultiplier;
                lensDistortion.center.value = profile.hDRPLensDistortionCenter;
                lensDistortion.scale.value = profile.hDRPLensDistortionScale;
            }
        }

        /// <summary>
        /// Sets motion blur settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPMotionBlur(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.MotionBlur motionBlur;
            if (volumeProfile.TryGet(out motionBlur))
            {
                motionBlur.active = profile.motionBlurEnabled;
                motionBlur.intensity.value = profile.hDRPMotionBlurIntensity;
                motionBlur.sampleCount.value = profile.hDRPMotionBlurSampleCount;
#if !UNITY_2019_2_OR_NEWER
                motionBlur.maxVelocity.value = profile.hDRPMotionBlurMaxVelocity;
                motionBlur.minVel.value = profile.hDRPMotionBlurMinVelocity;
#else
                motionBlur.maximumVelocity.value = profile.hDRPMotionBlurMaxVelocity;
                motionBlur.minimumVelocity.value = profile.hDRPMotionBlurMinVelocity;
#endif
                motionBlur.cameraRotationVelocityClamp.value = profile.hDRPMotionBlurCameraRotationVelocityClamp;
            }
#else
            UnityEngine.Rendering.HighDefinition.MotionBlur motionBlur;
            if (volumeProfile.TryGet(out motionBlur))
            {
                motionBlur.active = profile.motionBlurEnabled;
                motionBlur.intensity.value = profile.hDRPMotionBlurIntensity;
                switch(profile.hDRPMotionBlurQuality)
                {
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Custom:
                        motionBlur.quality.value = 0;
                        motionBlur.sampleCount = profile.hDRPMotionBlurSampleCount;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Low:
                        motionBlur.quality.value = 1;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.Medium:
                        motionBlur.quality.value = 2;
                        break;
                    case AmbientSkiesConsts.HDRPPostProcessingQuality.High:
                        motionBlur.quality.value = 3;
                        break;
                }

                motionBlur.maximumVelocity.value = profile.hDRPMotionBlurMaxVelocity;
                motionBlur.minimumVelocity.value = profile.hDRPMotionBlurMinVelocity;
                motionBlur.cameraRotationVelocityClamp.value = profile.hDRPMotionBlurCameraRotationVelocityClamp;
            }
#endif
        }

        /// <summary>
        /// Sets panini projection settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetHDRPPaniniProjection(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.PaniniProjection paniniProjection;
#else
            UnityEngine.Rendering.HighDefinition.PaniniProjection paniniProjection;
#endif
            if (volumeProfile.TryGet(out paniniProjection))
            {
                paniniProjection.active = profile.hDRPPaniniProjectionEnabled;
                paniniProjection.distance.value = profile.hDRPPaniniProjectionDistance;
                paniniProjection.cropToFit.value = profile.hDRPPaniniProjectionCropToFit;
            }
        }

        /// <summary>
        /// Sets vignette settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetHDRPVignette(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
#if !UNITY_2019_3_OR_NEWER
            UnityEngine.Experimental.Rendering.HDPipeline.Vignette vignette;
#else
            UnityEngine.Rendering.HighDefinition.Vignette vignette;
#endif
            if (volumeProfile.TryGet(out vignette))
            {
                vignette.active = profile.vignetteEnabled;
                vignette.mode.value = profile.hDRPVignetteMode;
                vignette.color.value = profile.hDRPVignetteColor;
                vignette.center.value = profile.hDRPVignetteCenter;
                vignette.intensity.value = profile.hDRPVignetteIntensity;
                vignette.smoothness.value = profile.hDRPVignetteSmoothness;
                vignette.roundness.value = profile.hDRPVignetteRoundness;
                vignette.rounded.value = profile.hDRPVignetteRounded;
                vignette.mask.value = profile.hDRPVignetteMask;
                vignette.opacity.value = profile.hDRPVignetteMaskOpacity;
            }
        }

#endif

        #endregion

        #region URP

#if UPPipeline
        /// <summary>
        /// Sets ambient occlusion settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetURPAmbientOcclusion(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            //To be added when URP releases an Ambient Occlusion solution
        }

        /// <summary>
        /// Sets auto exposure settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetURPAutoExposure(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            //To be added when URP releases an Auto Exposure solution
        }

        /// <summary>
        /// Sets bloom settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetURPBloom(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Rendering.Universal.Bloom bloom;
            if (volumeProfile.TryGet(out bloom))
            {
                bloom.active = profile.bloomEnabled;
                if (profile.bloomEnabled)
                {
                    bloom.threshold.value = profile.uRPBloomThreshold;

                    bloom.intensity.value = profile.uRPBloomIntensity;
                    bloom.scatter.value = profile.uRPBloomScatter;
                    bloom.tint.value = profile.uRPBloomTint;
                    bloom.highQualityFiltering.value = profile.uRPBloomHighQualityFiltering;
                    bloom.dirtTexture.value = profile.uRPBloomDirtLensTexture;
                    bloom.dirtIntensity.value = profile.uRPBloomDirtLensIntensity;
                }
            }
        }

        /// <summary>
        /// Sets chromatic aberration settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetURPChromaticAberration(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Rendering.Universal.ChromaticAberration chromaticAberration;
            if (volumeProfile.TryGet(out chromaticAberration))
            {
                chromaticAberration.active = profile.chromaticAberrationEnabled;
                if (profile.chromaticAberrationEnabled)
                {
                    chromaticAberration.intensity.value = profile.uRPChromaticAberrationIntensity;
                }
            }
        }

        /// <summary>
        /// Sets color grading settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetURPColorGrading(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;
            if (volumeProfile.TryGet(out colorAdjustments))
            {
                colorAdjustments.active = profile.colorGradingEnabled;
                if (profile.colorGradingEnabled)
                {
                    colorAdjustments.postExposure.value = profile.uRPColorAdjustmentPostExposure;
                    colorAdjustments.contrast.value = profile.uRPColorAdjustmentContrast;
                    colorAdjustments.colorFilter.value = profile.uRPColorAdjustmentColorFilter;
                    colorAdjustments.hueShift.value = profile.uRPColorAdjustmentHueShift;
                    colorAdjustments.saturation.value = profile.uRPColorAdjustmentSaturation;
                }
            }

            UnityEngine.Rendering.Universal.ChannelMixer channelMixer;
            if (volumeProfile.TryGet(out channelMixer))
            {
                channelMixer.active = profile.colorGradingEnabled;
                if (profile.colorGradingEnabled)
                {
                    channelMixer.redOutRedIn.value = profile.uRPChannelMixerRed;
                    channelMixer.greenOutGreenIn.value = profile.uRPChannelMixerGreen;
                    channelMixer.blueOutBlueIn.value = profile.uRPChannelMixerBlue;
                }
            }

            UnityEngine.Rendering.Universal.ColorLookup colorLookup;
            if (volumeProfile.TryGet(out colorLookup))
            {
                colorLookup.active = profile.colorGradingEnabled;
                if (profile.colorGradingEnabled)
                {
                    colorLookup.texture.value = profile.uRPColorLookupTexture;
                    colorLookup.contribution.value = profile.uRPColorLookupContribution;
                }
            }

            UnityEngine.Rendering.Universal.Tonemapping tonemapping;
            if (volumeProfile.TryGet(out tonemapping))
            {
                tonemapping.active = profile.colorGradingEnabled;
                if (profile.colorGradingEnabled)
                {
                    switch (profile.uRPTonemappingMode)
                    {
                        case AmbientSkiesConsts.URPTonemappingMode.None:
                            tonemapping.mode.value = UnityEngine.Rendering.Universal.TonemappingMode.None;
                            break;
                        case AmbientSkiesConsts.URPTonemappingMode.Neutral:
                            tonemapping.mode.value = UnityEngine.Rendering.Universal.TonemappingMode.Neutral;
                            break;
                        case AmbientSkiesConsts.URPTonemappingMode.ACES:
                            tonemapping.mode.value = UnityEngine.Rendering.Universal.TonemappingMode.ACES;
                            break;
                    }
                }
            }

            UnityEngine.Rendering.Universal.SplitToning splitToning;
            if (volumeProfile.TryGet(out splitToning))
            {
                splitToning.active = profile.colorGradingEnabled;
                if (profile.colorGradingEnabled)
                {
                    splitToning.shadows.value = profile.uRPSplitToningShadows;
                    splitToning.highlights.value = profile.uRPSplitToningHighlights;
                    splitToning.balance.value = profile.uRPSplitToningBalance;
                }
            }

            UnityEngine.Rendering.Universal.WhiteBalance whiteBalance;
            if (volumeProfile.TryGet(out whiteBalance))
            {
                whiteBalance.active = profile.colorGradingEnabled;
                if (profile.colorGradingEnabled)
                {
                    whiteBalance.temperature.value = profile.uRPWhiteBalanceTempature;
                    whiteBalance.tint.value = profile.uRPWhiteBalanceTint;
                }
            }
        }

        /// <summary>
        /// Sets depth of field settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetURPDepthOfField(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile, Camera camera)
        {
            UnityEngine.Rendering.Universal.DepthOfField dof;
            if (volumeProfile.TryGet(out dof))
            {
                //dof.SetAllOverridesTo(true);
                dof.active = profile.depthOfFieldEnabled;
                if (profile.depthOfFieldEnabled)
                {
                    switch (profile.uRPDepthOfFieldMode)
                    {
                        case AmbientSkiesConsts.URPDepthOfFieldMode.Off:
                            dof.mode.value = UnityEngine.Rendering.Universal.DepthOfFieldMode.Off;
                            break;
                        case AmbientSkiesConsts.URPDepthOfFieldMode.Gaussian:
                            dof.mode.value = UnityEngine.Rendering.Universal.DepthOfFieldMode.Gaussian;
                            dof.gaussianStart.value = profile.uRPDepthOfFieldStart;
                            dof.gaussianEnd.value = profile.uRPDepthOfFieldEnd;
                            dof.gaussianMaxRadius.value = profile.uRPDepthOfFieldMaxRadius;
                            dof.highQualitySampling.value = profile.uRPDepthOfFieldHighQualityFiltering;
                            break;
                        case AmbientSkiesConsts.URPDepthOfFieldMode.Bokeh:
                            dof.mode.value = UnityEngine.Rendering.Universal.DepthOfFieldMode.Bokeh;
                            dof.focusDistance.value = profile.uRPDepthOfFieldFocusDitance;
                            dof.focalLength.value = profile.uRPDepthOfFieldFocalLength;
                            dof.aperture.value = profile.uRPDepthOfFieldAperture;
                            dof.bladeCount.value = profile.uRPDepthOfFieldBladeCount;
                            dof.bladeCurvature.value = profile.uRPDepthOfFieldBladeCurvature;
                            dof.bladeRotation.value = profile.uRPDepthOfFieldBladeRotation;
                            break;
                    }
                }
                else
                {
                    dof.mode.value = UnityEngine.Rendering.Universal.DepthOfFieldMode.Off;
                }
            }
        }

        /// <summary>
        /// Sets grain settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetURPGrain(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Rendering.Universal.FilmGrain grain;
            if (volumeProfile.TryGet(out grain))
            {
                //grain.SetAllOverridesTo(true);

                grain.active = profile.grainEnabled;
                if (profile.grainEnabled)
                {
                    grain.intensity.value = profile.uRPFilmGrainIntensity;
                    grain.response.value = profile.uRPFilmGrainResponse;
                    switch (profile.uRPFilmGrainType)
                    {
                        case AmbientSkiesConsts.URPFilmGrainType.Thin1:
                            grain.type.value = UnityEngine.Rendering.Universal.FilmGrainLookup.Thin1;
                            break;
                        case AmbientSkiesConsts.URPFilmGrainType.Thin2:
                            grain.type.value = UnityEngine.Rendering.Universal.FilmGrainLookup.Thin2;
                            break;
                        case AmbientSkiesConsts.URPFilmGrainType.Medium1:
                            grain.type.value = UnityEngine.Rendering.Universal.FilmGrainLookup.Medium1;
                            break;
                        case AmbientSkiesConsts.URPFilmGrainType.Medium2:
                            grain.type.value = UnityEngine.Rendering.Universal.FilmGrainLookup.Medium2;
                            break;
                        case AmbientSkiesConsts.URPFilmGrainType.Medium3:
                            grain.type.value = UnityEngine.Rendering.Universal.FilmGrainLookup.Medium3;
                            break;
                        case AmbientSkiesConsts.URPFilmGrainType.Medium4:
                            grain.type.value = UnityEngine.Rendering.Universal.FilmGrainLookup.Medium4;
                            break;
                        case AmbientSkiesConsts.URPFilmGrainType.Medium5:
                            grain.type.value = UnityEngine.Rendering.Universal.FilmGrainLookup.Medium5;
                            break;
                        case AmbientSkiesConsts.URPFilmGrainType.Medium6:
                            grain.type.value = UnityEngine.Rendering.Universal.FilmGrainLookup.Medium6;
                            break;
                        case AmbientSkiesConsts.URPFilmGrainType.Large01:
                            grain.type.value = UnityEngine.Rendering.Universal.FilmGrainLookup.Large01;
                            break;
                        case AmbientSkiesConsts.URPFilmGrainType.Large02:
                            grain.type.value = UnityEngine.Rendering.Universal.FilmGrainLookup.Large02;
                            break;
                        case AmbientSkiesConsts.URPFilmGrainType.Custom:
                            grain.type.value = UnityEngine.Rendering.Universal.FilmGrainLookup.Custom;
                            grain.texture.value = profile.uRPFilmGrainCustomTexture;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Sets lens distortion settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetURPLensDistortion(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Rendering.Universal.LensDistortion lensDistortion;
            if (volumeProfile.TryGet(out lensDistortion))
            {
                lensDistortion.active = profile.distortionEnabled;
                if (profile.distortionEnabled)
                {
                    lensDistortion.intensity.value = profile.uRPLensDistortionIntensity;
                    lensDistortion.xMultiplier.value = profile.uRPLensDistortionXMultiplier;
                    lensDistortion.yMultiplier.value = profile.uRPLensDistortionYMultiplier;
                    lensDistortion.center.value = profile.uRPLensDistortionCenter;
                    lensDistortion.scale.value = profile.uRPLensDistortionScale;
                }
            }
        }

        /// <summary>
        /// Sets motion blur settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetURPMotionBlur(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Rendering.Universal.MotionBlur motionBlur;
            if (volumeProfile.TryGet(out motionBlur))
            {
                motionBlur.active = profile.motionBlurEnabled;
                if (profile.motionBlurEnabled)
                {
                    motionBlur.intensity.value = profile.uRPMotionBlurIntensity;
                    motionBlur.clamp.value = profile.uRPMotionBlurCameraRotationVelocityClamp;
                    switch (profile.uRPMotionBlurQuality)
                    {
                        case AmbientSkiesConsts.URPMotionBlurQuality.Low:
                            motionBlur.quality.value = MotionBlurQuality.Low;
                            break;
                        case AmbientSkiesConsts.URPMotionBlurQuality.Medium:
                            motionBlur.quality.value = MotionBlurQuality.Medium;
                            break;
                        case AmbientSkiesConsts.URPMotionBlurQuality.High:
                            motionBlur.quality.value = MotionBlurQuality.High;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Sets panini projection settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="volumeProfile"></param>
        public static void SetURPPaniniProjection(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Rendering.Universal.PaniniProjection paniniProjection;
            if (volumeProfile.TryGet(out paniniProjection))
            {
                paniniProjection.active = profile.uRPPaniniProjectionEnabled;
                if (profile.hDRPPaniniProjectionEnabled)
                {
                    paniniProjection.distance.value = profile.uRPPaniniProjectionDistance;
                    paniniProjection.cropToFit.value = profile.uRPPaniniProjectionCropToFit;
                }
            }
        }

        /// <summary>
        /// Sets vignette settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="postProcessProfile"></param>
        public static void SetURPVignette(AmbientPostProcessingProfile profile, VolumeProfile volumeProfile)
        {
            UnityEngine.Rendering.Universal.Vignette vignette;
            if (volumeProfile.TryGet(out vignette))
            {
                vignette.active = profile.vignetteEnabled;
                if (profile.vignetteEnabled)
                {
                    vignette.color.value = profile.uRPVignetteColor;
                    vignette.center.value = profile.uRPVignetteCenter;
                    vignette.intensity.value = profile.uRPVignetteIntensity;
                    vignette.smoothness.value = profile.uRPVignetteSmoothness;
                    vignette.rounded.value = profile.uRPVignetteRounded;
                }
            }
        }

#endif

            #endregion

    #endregion

    #endregion

        #endregion

    }
}