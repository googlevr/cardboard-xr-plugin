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
    using UnityEngine.Rendering;
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

        private static Texture2D _closeTexture;

        private static Texture2D _gearTexture;

        /// <summary>
        /// Pairs the native enum to set the graphics API being used.
        /// </summary>
        private enum CardboardGraphicsApi
        {
            kOpenGlEs2 = 1,
            kOpenGlEs3 = 2,
            kNone = -1,
        }

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

        /// <summary>
        /// Sets the screen parameters and the widgets in the VR scene.
        /// </summary>
        ///
        /// <param name="renderingArea">
        /// The rectangle where the VR scene will be rendered.
        /// </param>
        internal static void RecalculateRectangles(Rect renderingArea)
        {
            // TODO(b/171702321): Remove this method once the safe area size could be properly
            // fetched by the XRLoader.
            CardboardUnity_setScreenParams(
                    (int)renderingArea.x, (int)renderingArea.y, (int)renderingArea.width,
                    (int)renderingArea.height);

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
        /// Sets which Graphics API is being used by Unity to the native implementation.
        /// </summary>
        private static void SetGraphicsApi()
        {
            switch (SystemInfo.graphicsDeviceType)
            {
                case GraphicsDeviceType.OpenGLES2:
                    CardboardUnity_setGraphicsApi(CardboardGraphicsApi.kOpenGlEs2);
                    break;
                case GraphicsDeviceType.OpenGLES3:
                    CardboardUnity_setGraphicsApi(CardboardGraphicsApi.kOpenGlEs3);
                    break;
            }
        }

        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardUnity_setScreenParams(
            int x, int y, int width, int height);

        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardUnity_setWidgetCount(int count);

        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardUnity_setWidgetParams(
            int i, IntPtr texture, int x, int y, int width, int height);

        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardUnity_setGraphicsApi(CardboardGraphicsApi graphics_api);

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

            _closeTexture = Resources.Load<Texture2D>("Cardboard/quantum_ic_close_white_24");
            DontDestroyOnLoad(_closeTexture);
            _gearTexture = Resources.Load<Texture2D>("Cardboard/quantum_ic_settings_white_24");
            DontDestroyOnLoad(_gearTexture);

            SetGraphicsApi();

            // Safe area is required to avoid rendering behind the notch. If the device does not
            // have any notch, it will be equivalent to the full screen area.
            RecalculateRectangles(Screen.safeArea);
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
