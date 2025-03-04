using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
Only works with the BNG framework 
and Steering Wheel script attached in the same component
*/

namespace BNG {
    public class ScrewHelper : MonoBehaviour {

        public SteeringWheel steeringWheel;
        public AudioSource audioSource;
        public AudioClip screwSound;
        public float angleStep = 45f;  // Increased from 30f to make steps more significant
        public float looseAngle = 90f;
        public int angleStepsThreshold = 4; // Can be positive or negative based on desired tightening direction
        public bool invertTighteningDirection = false; // Set to true to reverse tightening direction

        public UnityEvent onAngleStepsReached;
        public UnityEvent onScrewOvertight;  // Event for when screw is tightened beyond threshold
        public UnityEvent onScrewLoosened;   // Event for when screw is loosened below threshold

        private float lastAngle;
        private float screwTightness = 0f;  // Can be positive or negative depending on direction
        private int stepsCompleted = 0;
        private bool pastLooseAngle = false;
        private bool isOvertight = false;
        private bool thresholdReached = false;

        void Start() {
            if (steeringWheel == null) {
                Debug.LogError("SteeringWheel reference is not set.");
                return;
            }

            lastAngle = steeringWheel.RawAngle;
        }
        
        void Update() {
            float currentAngle = steeringWheel.RawAngle;
            float angleDifference = currentAngle - lastAngle;
            
            // Account for angle wrapping (e.g., going from 359° to 0°)
            if (angleDifference > 180f) angleDifference -= 360f;
            else if (angleDifference < -180f) angleDifference += 360f;
            
            // Apply direction based on user preference
            if (invertTighteningDirection) {
                angleDifference = -angleDifference;
            }
            
            // Update screw tightness (can increase or decrease in either direction)
            screwTightness += angleDifference;
            
            // Calculate absolute tightness for loose angle check
            float absoluteTightness = Mathf.Abs(screwTightness);
            
            // Check if we've moved beyond the loose angle
            if (!pastLooseAngle && absoluteTightness >= looseAngle) {
                pastLooseAngle = true;
            }
            
            // If we're past the loose angle, track steps
            if (pastLooseAngle) {
                // Calculate effective rotation, preserving sign
                float effectiveRotation;
                if (screwTightness >= 0) {
                    effectiveRotation = absoluteTightness - looseAngle;
                } else {
                    effectiveRotation = -(absoluteTightness - looseAngle);
                }
                
                // Return to loose state if we drop below the loose angle
                if (absoluteTightness < looseAngle) {
                    effectiveRotation = 0f;
                    pastLooseAngle = false;
                    thresholdReached = false;  // Reset threshold tracking
                }
                
                // Calculate steps, preserving sign
                int currentSteps;
                if (effectiveRotation >= 0) {
                    currentSteps = Mathf.FloorToInt(effectiveRotation / angleStep);
                } else {
                    currentSteps = Mathf.CeilToInt(effectiveRotation / angleStep);
                }
                
                // If we've changed steps
                if (currentSteps != stepsCompleted) {
                    // Play sound on step change
                    Debug.Log("Steps: " + currentSteps);
                    PlayScrewSound();
                    
                    // Check if we've EXACTLY reached the threshold value
                    bool reachedThreshold = currentSteps == angleStepsThreshold;
                    
                    // Check direction of approach
                    bool approachingFromLooseEnd = false;
                    if (angleStepsThreshold >= 0) {
                        // For positive threshold, we're approaching from loose end if previous steps were lower
                        approachingFromLooseEnd = stepsCompleted < angleStepsThreshold;
                    } else {
                        // For negative threshold, we're approaching from loose end if previous steps were higher
                        approachingFromLooseEnd = stepsCompleted > angleStepsThreshold;
                    }
                    
                    // Check if we're tightening beyond threshold
                    bool tighteningBeyondThreshold = false;
                    if (angleStepsThreshold >= 0) {
                        // For positive threshold, overtightening means steps > threshold
                        tighteningBeyondThreshold = currentSteps > angleStepsThreshold && stepsCompleted <= angleStepsThreshold;
                    } else {
                        // For negative threshold, overtightening means steps < threshold
                        tighteningBeyondThreshold = currentSteps < angleStepsThreshold && stepsCompleted >= angleStepsThreshold;
                    }
                    
                    // Check if we're loosening below threshold
                    bool looseningBelowThreshold = false;
                    if (angleStepsThreshold >= 0) {
                        // For positive threshold, loosening means steps < threshold
                        looseningBelowThreshold = currentSteps < angleStepsThreshold && stepsCompleted >= angleStepsThreshold;
                    } else {
                        // For negative threshold, loosening means steps > threshold
                        looseningBelowThreshold = currentSteps > angleStepsThreshold && stepsCompleted <= angleStepsThreshold;
                    }
                    
                    // Reaching threshold (from loose end OR returning from overtightened)
                    if (reachedThreshold && (approachingFromLooseEnd || isOvertight) && !thresholdReached) {
                        onAngleStepsReached.Invoke();
                        isOvertight = false;
                        thresholdReached = true;
                    }
                    // Overtightening beyond threshold
                    else if (tighteningBeyondThreshold && !isOvertight) {
                        onScrewOvertight.Invoke();
                        isOvertight = true;
                        thresholdReached = false;
                    }
                    // Loosening from threshold or beyond
                    else if (looseningBelowThreshold) {
                        onScrewLoosened.Invoke();
                        isOvertight = false;
                        thresholdReached = false;
                    }
                    
                    // Update steps
                    stepsCompleted = currentSteps;
                }
            }
            
            lastAngle = currentAngle;
        }

        void PlayScrewSound() {
            if (audioSource != null && screwSound != null) {
                audioSource.PlayOneShot(screwSound);
            }
        }
        
        // Public method to reset tracking (can be called from other scripts or events)
        public void ResetTracking() {
            screwTightness = 0f;
            stepsCompleted = 0;
            pastLooseAngle = false;
            isOvertight = false;
            thresholdReached = false;
        }
    }
}