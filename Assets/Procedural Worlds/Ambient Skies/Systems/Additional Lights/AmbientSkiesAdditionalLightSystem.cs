//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System.Collections.Generic;
using UnityEngine;

namespace AmbientSkies
{
    public class AmbientSkiesAdditionalLightSystem : MonoBehaviour
    {
        public AmbientSkiesConsts.LightTypeMode m_lightTypeMode = AmbientSkiesConsts.LightTypeMode.Static;

        #region Light FLicker Values

        [Tooltip("External light to flicker; you can leave this null if you attach script to a light")]
        public Light m_mainLight;
        [Tooltip("Minimum random light intensity")]
        public float m_minIntensity = 0f;
        [Tooltip("Maximum random light intensity")]
        public float m_maxIntensity = 1f;
        [Tooltip("How much to smooth out the randomness; lower values = sparks, higher = lantern")]
        [Range(1, 50)]
        public int m_smoothing = 5;
        public bool m_isToggleLight = false;

        #endregion

        // Continuous average calculation via FIFO queue
        // Saves us iterating every time we update, we just change by the delta
        private Queue<float> smoothQueue;
        private float lastSum = 0;

        private void OnEnable()
        {
            // External or internal light?
            if (m_mainLight == null)
            {
                m_mainLight = GetComponent<Light>();
            }
        }

        private void Start()
        {
            // External or internal light?
            if (m_mainLight == null)
            {
                m_mainLight = GetComponent<Light>();
            }

            smoothQueue = new Queue<float>(m_smoothing);

            smoothQueue.Clear();
            lastSum = 0;

            if (m_lightTypeMode == AmbientSkiesConsts.LightTypeMode.ToggleLight)
            {
                m_isToggleLight = true;
            }
        }

        private void Update()
        {
            if (m_mainLight == null)
            {
                return;
            }

            if (m_lightTypeMode == AmbientSkiesConsts.LightTypeMode.FlickeringLight)
            {
                // pop off an item if too big
                while (smoothQueue.Count >= m_smoothing)
                {
                    lastSum -= smoothQueue.Dequeue();
                }

                // Generate random new item, calculate new average
                float newVal = Random.Range(m_minIntensity, m_maxIntensity);
                smoothQueue.Enqueue(newVal);
                lastSum += newVal;

                // Calculate new smoothed average
                m_mainLight.intensity = lastSum / (float)smoothQueue.Count;
            }
            else
            {

            }
        }
    }
}