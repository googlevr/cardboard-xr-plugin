//-----------------------------------------------------------------------
// <copyright file="XRLoader.cs" company="Google LLC">
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.Cardboard
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.XR;
    using UnityEngine.XR.Management;

    /// <summary>
    /// XR Loader for Cardboard XR Plugin.
    /// Loads Display and Input Subsystems.
    /// </summary>
    public class XRLoader : XRLoaderHelper
    {
        private static List<XRDisplaySubsystemDescriptor> _displaySubsystemDescriptors =
            new List<XRDisplaySubsystemDescriptor>();

        private static List<XRInputSubsystemDescriptor> _inputSubsystemDescriptors =
            new List<XRInputSubsystemDescriptor>();

        private Texture2D _closeTexture;

        private Texture2D _gearTexture;

        /// <summary>
        /// Gets a value indicating whether the subsystems are initialized or not.
        /// </summary>
        ///
        /// <returns>
        /// True after a successful call to Initialize() without a posterior call to
        /// Deinitialize().
        /// </returns>
        internal static bool _isInitialized { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the subsystems are started or not.
        /// </summary>
        ///
        /// <returns>
        /// True after a successful call to Start() without a posterior call to Stop().
        /// </returns>
        internal static bool _isStarted { get; private set; }

        /// <summary>
        /// Initialize the loader. This should initialize all subsystems to support the desired
        /// runtime setup this loader represents.
        /// </summary>
        ///
        /// <returns>Whether or not initialization succeeded.</returns>
        public override bool Initialize()
        {
            CardboardSDKInitialize();
            CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(
                _displaySubsystemDescriptors, "Display");
            CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(
                _inputSubsystemDescriptors, "Input");
            _isInitialized = true;
            return true;
        }

        /// <summary>
        /// Ask loader to start all initialized subsystems.
        /// </summary>
        ///
        /// <returns>Whether or not all subsystems were successfully started.</returns>
        public override bool Start()
        {
            StartSubsystem<XRDisplaySubsystem>();
            StartSubsystem<XRInputSubsystem>();
            _isStarted = true;
            return true;
        }

        /// <summary>
        /// Ask loader to stop all initialized subsystems.
        /// </summary>
        ///
        /// <returns>Whether or not all subsystems were successfully stopped.</returns>
        public override bool Stop()
        {
            StopSubsystem<XRDisplaySubsystem>();
            StopSubsystem<XRInputSubsystem>();
            _isStarted = false;
            return true;
        }

        /// <summary>
        /// Ask loader to deinitialize all initialized subsystems.
        /// </summary>
        ///
        /// <returns>Whether or not deinitialization succeeded.</returns>
        public override bool Deinitialize()
        {
            DestroySubsystem<XRDisplaySubsystem>();
            DestroySubsystem<XRInputSubsystem>();
            CardboardSDKDeinitialize();
            _isInitialized = false;
            return true;
        }

        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardUnity_setScreenParams(
            int x, int y, int width, int height);

        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardUnity_setWidgetCount(int count);

        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardUnity_setWidgetParams(
            int i, IntPtr texture, int x, int y, int width, int height);

#if UNITY_ANDROID
        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardUnity_initializeAndroid(IntPtr context);
#endif

        /// <summary>
        /// For Android, initializes JavaVM and Android activity context.
        /// Then, for both Android and iOS, it sets the screen size in pixels.
        /// </summary>
        private void CardboardSDKInitialize()
        {
#if UNITY_ANDROID
            // TODO(b/169797155): Move this to UnityPluginLoad().
            // Gets Unity context (Main Activity).
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var context = activity.Call<AndroidJavaObject>("getApplicationContext");

            // Initializes Cardboard SDK.
            CardboardUnity_initializeAndroid(activity.GetRawObject());
#endif

            // Safe area is required because notch in the screen. If the device does not have any
            // notch, it will be equivalent to:
            // CardboardUnity_setScreenParams(0, 0, Screen.width, Screen.height);
            CardboardUnity_setScreenParams(
                    (int)Screen.safeArea.x, (int)Screen.safeArea.y, (int)Screen.safeArea.width,
                    (int)Screen.safeArea.height);

            _closeTexture = Resources.Load<Texture2D>("Cardboard/quantum_ic_close_white_24");
            DontDestroyOnLoad(_closeTexture);
            _gearTexture = Resources.Load<Texture2D>("Cardboard/quantum_ic_settings_white_24");
            DontDestroyOnLoad(_gearTexture);

            RectInt closeRect = Widget.CloseButtonRenderRect;
            RectInt gearRect = Widget.GearButtonRenderRect;
            RectInt alignmentRect = Widget.AlignmentRect;
            CardboardUnity_setWidgetCount(3);
            CardboardUnity_setWidgetParams(
                    0, _closeTexture.GetNativeTexturePtr(), closeRect.x, closeRect.y,
                    closeRect.width, closeRect.height);
            CardboardUnity_setWidgetParams(
                    1, _gearTexture.GetNativeTexturePtr(), gearRect.x, gearRect.y, gearRect.width,
                    gearRect.height);
            CardboardUnity_setWidgetParams(
                    2, Texture2D.whiteTexture.GetNativeTexturePtr(), alignmentRect.x,
                    alignmentRect.y, alignmentRect.width, alignmentRect.height);
        }

        /// <summary>
        /// Close and gear button textures are preserved until the XR provider is deinitialized.
        /// </summary>
        private void CardboardSDKDeinitialize()
        {
            Resources.UnloadAsset(_closeTexture);
            Resources.UnloadAsset(_gearTexture);
        }
    }
}
