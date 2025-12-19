using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JUTPS.FX
{
    [AddComponentMenu("JU TPS/FX/Blood Screen")]
    [RequireComponent(typeof(Image))]
    public class BloodScreen : MonoBehaviour
    {
        public static BloodScreen instance;
        JUTPS.CharacterBrain.JUCharacterBrain pl;
        Image img;
        float healthvalue;
        Color currentColor;
        
        [Header("Pulse Settings")]
        [Tooltip("Enable pulsing effect")]
        public bool enablePulse = true;
        
        private float pulseTimer = 0f;
        private bool isPulsing = false;
        private float pulseDuration = 0.3f;
        
        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            pl = (player != null) ? player.GetComponent<JUTPS.CharacterBrain.JUCharacterBrain>() : null;
            img = GetComponent<Image>();
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (pl == null) return;
            if (pl.CharacterHealth != null)
            {
                healthvalue = Mathf.Lerp(healthvalue, pl.CharacterHealth.Health / pl.CharacterHealth.MaxHealth, 15 * Time.deltaTime);
                
                // Only show blood screen when health is below 50%
                if (healthvalue < 0.5f)
                {
                    // Calculate pulse rate based on health (50% to 5%)
                    float pulseRate = CalculatePulseRate(healthvalue);
                    
                    // Update pulse timer
                    pulseTimer += Time.deltaTime;
                    
                    if (enablePulse && pulseTimer >= pulseRate)
                    {
                        // Trigger pulse
                        isPulsing = true;
                        pulseTimer = 0f;
                    }
                    
                    // Handle pulse effect
                    if (isPulsing)
                    {
                        float pulseProgress = pulseTimer / pulseDuration;
                        
                        if (pulseProgress >= 1f)
                        {
                            isPulsing = false;
                            currentColor = CalculateBaseColor(healthvalue);
                        }
                        else
                        {
                            // Pulse from white to base color
                            Color baseColor = CalculateBaseColor(healthvalue);
                            currentColor = Color.Lerp(Color.white, baseColor, pulseProgress);
                        }
                    }
                    else
                    {
                        currentColor = CalculateBaseColor(healthvalue);
                    }
                }
                else
                {
                    currentColor = Color.clear;
                    pulseTimer = 0f;
                    isPulsing = false;
                }
                
                img.color = Color.Lerp(img.color, currentColor, 5 * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Calculate pulse rate based on health percentage
        /// 50% health = 2 seconds per pulse
        /// 5% health = 1 second per pulse
        /// Linear scaling between these values
        /// </summary>
        private float CalculatePulseRate(float healthPercent)
        {
            if (healthPercent <= 0.05f)
            {
                return 1f; // 1 second at 5% or below
            }
            
            // Linear interpolation between 50% (2 seconds) and 5% (1 second)
            // Remap health 0.05-0.5 to pulse rate 1-2 seconds
            float t = (healthPercent - 0.05f) / (0.5f - 0.05f); // Normalize to 0-1
            return Mathf.Lerp(1f, 2f, t);
        }
        
        /// <summary>
        /// Calculate base overlay intensity based on health
        /// </summary>
        private Color CalculateBaseColor(float healthPercent)
        {
            // Intensity increases as health decreases
            // At 50% health: minimal intensity
            // At 0% health: full intensity
            float intensity = Mathf.Lerp(1f, 0.2f, healthPercent / 0.5f);
            return new Color(1f, 1f, 1f, intensity);
        }
        
        private void PlayerHasHited()
        {
            // Flash on damage only if health is below 50%
            if (healthvalue < 0.5f)
            {
                img.color = Color.white;
                isPulsing = true;
                pulseTimer = 0f;
            }
        }
        
        public static void PlayerTakingDamaged()
        {
            if (instance == null) { return; }

            instance.PlayerHasHited();
        }
    }
}