// Crest Ocean System

// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Crest
{
    public interface IValidated
    {
        bool Validate(OceanRenderer ocean, ValidatedHelper.ShowMessage function);
    }

    // This is only used with help boxes since we want to group messages together.
    public struct ValidatedMessage
    {
        public string message;
        public MessageType type;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(OceanRenderer))]
    public class OceanRendererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Validate Setup"))
            {
                RunValidation(target as OceanRenderer);
            }
        }

        public static void RunValidation(OceanRenderer ocean)
        {
            // OceanRenderer
            if (ocean.transform.childCount > 0)
            {
                Debug.LogWarning("Validation: The ocean changes scale at runtime so may not be a good idea to store objects underneath it, especially if they are sensitive to scale.", ocean);
            }
            if (FindObjectsOfType<OceanRenderer>().Length > 1)
            {
                Debug.LogWarning("Validation: Multiple OceanRenderer scripts detected in open scenes, this is not typical - usually only one OceanRenderer is expected to be present.", ocean);
            }

            // ShapeGerstnerBatched
            var gerstners = FindObjectsOfType<ShapeGerstnerBatched>();
            if (gerstners.Length == 0)
            {
                Debug.Log("Validation: No ShapeGerstnerBatched script found, so ocean will appear flat (no waves).", ocean);
            }
            foreach (var gerstner in gerstners)
            {
                if (gerstner._componentsPerOctave == 0)
                {
                    Debug.LogWarning("Validation: Components Per Octave set to 0 meaning this Gerstner component won't generate any waves. Click this message to see the component in question.", gerstner);
                }
            }

            // UnderwaterEffect
            var underwaters = FindObjectsOfType<UnderwaterEffect>();
            foreach (var underwater in underwaters)
            {
                if (underwater.transform.parent.GetComponent<Camera>() == null)
                {
                    Debug.LogError("Validation: UnderwaterEffect script expected to be parented to a GameObject with a Camera. Click this message to see the script in question.", underwater);
                }
            }

            // OceanDepthCache
            var depthCaches = FindObjectsOfType<OceanDepthCache>();
            foreach (var depthCache in depthCaches)
            {
                depthCache.Validate(ocean);
            }

            var assignLayers = FindObjectsOfType<AssignLayer>();
            foreach (var assign in assignLayers)
            {
                assign.Validate(ocean, ValidatedHelper.DebugLog);
            }

            // FloatingObjectBase
            var floatingObjects = FindObjectsOfType<FloatingObjectBase>();
            foreach (var floatingObject in floatingObjects)
            {
                if (ocean._simSettingsAnimatedWaves != null && ocean._simSettingsAnimatedWaves.CollisionSource == SimSettingsAnimatedWaves.CollisionSources.None)
                {
                    Debug.LogWarning("Collision Source in Animated Waves Settings is set to None. The floating objects in the scene will use a flat horizontal plane.", ocean);
                }

                var rbs = floatingObject.GetComponentsInChildren<Rigidbody>();
                if (rbs.Length != 1)
                {
                    Debug.LogError("Expected to have one rigidbody on floating object, currently has " + rbs.Length + " object(s). Click this message to see the script in question.", floatingObject);
                }
            }

            // Inputs
            var inputs = FindObjectsOfType<RegisterLodDataInputBase>();
            foreach (var input in inputs)
            {
                input.Validate(ocean, ValidatedHelper.DebugLog);
            }

            Debug.Log("Validation complete!", ocean);
        }
    }

    public static class ValidatedHelper
    {
        // This won't work cos we want to combine strings for help box but not for debug log. So we would have to have a 
        // collector which is the same as the advanced proposal anyway.
        public delegate void ShowMessage(string message, MessageType type);

        public static void DebugLog(string message, MessageType type)
        {
            switch (type)
            {
                // NOTE: this is incomplete
                case MessageType.Warning: Debug.LogWarning(message); break;
                case MessageType.Error: Debug.LogError(message); break;
                default: Debug.Log(message); break;
            }
        }

        public static void HelpBox(string message, MessageType type)
        {
            messages.Add(new ValidatedMessage { message = message, type = type });
        }

        public static readonly List<ValidatedMessage> messages = new List<ValidatedMessage>();
    }
#endif
}
