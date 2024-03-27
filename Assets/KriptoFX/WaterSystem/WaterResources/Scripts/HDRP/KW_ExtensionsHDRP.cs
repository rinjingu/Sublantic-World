using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KWS
{
    internal static class KW_ExtensionsHDRP
    {
        public static void SetCameraFrameSetting(this Camera cam, FrameSettingsField setting, bool enabled)
        {
            var cameraData = cam.GetComponent<HDAdditionalCameraData>();
            if (cameraData == null) cameraData = cam.gameObject.AddComponent<HDAdditionalCameraData>();
            SetCameraFrameSetting(cameraData, setting, enabled);
        }

        public static void SetCameraFrameSetting(this HDAdditionalCameraData cameraData, FrameSettingsField setting, bool enabled)
        {
            var frameSettings = cameraData.renderingPathCustomFrameSettings;
            var frameSettingsOverrideMask = cameraData.renderingPathCustomFrameSettingsOverrideMask;

            frameSettingsOverrideMask.mask[(uint)setting] = true;
            frameSettings.SetEnabled(setting, enabled);

            cameraData.renderingPathCustomFrameSettings = frameSettings;
            cameraData.renderingPathCustomFrameSettingsOverrideMask = frameSettingsOverrideMask;
        }

        public static void SetFrameSetting(this HDAdditionalReflectionData reflData, FrameSettingsField setting, bool enabled)
        {
            var frameSettings             = reflData.frameSettings;
            var frameSettingsOverrideMask = reflData.frameSettingsOverrideMask;

            frameSettingsOverrideMask.mask[(uint)setting] = true;
            frameSettings.SetEnabled(setting, enabled);

            reflData.frameSettings = frameSettings;
            reflData.frameSettingsOverrideMask = frameSettingsOverrideMask;
        }

        public static void SetFrameSetting(this PlanarReflectionProbe reflData, FrameSettingsField setting, bool enabled)
        {
            var frameSettings             = reflData.frameSettings;
            var frameSettingsOverrideMask = reflData.frameSettingsOverrideMask;

            frameSettingsOverrideMask.mask[(uint)setting] = true;
            frameSettings.SetEnabled(setting, enabled);

            reflData.frameSettings             = frameSettings;
            reflData.frameSettingsOverrideMask = frameSettingsOverrideMask;
        }


        public static void SetFrameSetting(this CameraSettings settings, FrameSettingsField setting, bool enabled)
        {
            var frameSettings             = settings.renderingPathCustomFrameSettings;
            var frameSettingsOverrideMask = settings.renderingPathCustomFrameSettingsOverrideMask;

            frameSettingsOverrideMask.mask[(uint)setting] = true;
            frameSettings.SetEnabled(setting, enabled);

            settings.renderingPathCustomFrameSettings = frameSettings;
            settings.renderingPathCustomFrameSettingsOverrideMask = frameSettingsOverrideMask;
        }

        public static void DisableAllCameraFrameSettings(this PlanarReflectionProbe reflData)
        {
            var frameSettings             = reflData.frameSettings;
            var frameSettingsOverrideMask = reflData.frameSettingsOverrideMask;


            frameSettingsOverrideMask.mask = new BitArray128(ulong.MaxValue, ulong.MaxValue);

            for (uint i = 0; i < frameSettingsOverrideMask.mask.capacity; i++)
            {
                frameSettings.SetEnabled((FrameSettingsField)i, false);
            }


            reflData.frameSettings = frameSettings;
            reflData.frameSettingsOverrideMask = frameSettingsOverrideMask;
        }

        public static void DisableAllCameraFrameSettings(this HDAdditionalCameraData cameraData)
        {
            var frameSettings             = cameraData.renderingPathCustomFrameSettings;
            var frameSettingsOverrideMask = cameraData.renderingPathCustomFrameSettingsOverrideMask;

           
            frameSettingsOverrideMask.mask = new BitArray128(ulong.MaxValue, ulong.MaxValue);

            for (uint i = 0; i < frameSettingsOverrideMask.mask.capacity; i++)
            {
                frameSettings.SetEnabled((FrameSettingsField) i, false);
            }


            cameraData.renderingPathCustomFrameSettings             = frameSettings;
            cameraData.renderingPathCustomFrameSettingsOverrideMask = frameSettingsOverrideMask;
        }
    }
}