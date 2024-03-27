using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWS
{

    internal partial class KWS_UpdateManager
    {
        private KWS_WaterPassHandler _passHandler;

        void OnEnablePlatformSpecific()
        {
            //Debug.Log("Initialized update manager");

            RenderPipelineManager.beginCameraRendering  += OnBeforeCameraRendering;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += EditorUpdate;
#endif

            if (_passHandler == null) _passHandler = new KWS_WaterPassHandler();
        }

       
        void OnDisablePlatformSpecific()
        {
            //Debug.Log("Removed update manager");

            RenderPipelineManager.beginCameraRendering  -= OnBeforeCameraRendering;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= EditorUpdate;
#endif

            _passHandler?.Release();

            KWS_CoreUtils.ReleaseRTHandles();
        }

#if UNITY_EDITOR
        private void EditorUpdate()
        {
            if (Application.isPlaying) return;
            ExecutePerFrame();
        }
#endif

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;
            ExecutePerFrame();
        }



        private void OnBeforeCameraRendering(ScriptableRenderContext context, Camera cam)
        {
          
            ExecutePerCamera(cam, context);
        }
    }
}