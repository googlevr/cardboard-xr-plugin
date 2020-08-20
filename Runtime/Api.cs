//-----------------------------------------------------------------------
// <copyright file="Api.cs" company="Google LLC">
// Copyright 2020 Google LLC. All Rights Reserved.
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// Cardboard XR Plugin API.
    /// </summary>
    public static class Api
    {
        private static int _deviceParamsCount = -1;

        /// <summary>
        /// Gets a value indicating whether the close button is pressed this frame.
        /// </summary>
        public static bool IsCloseButtonPressed
        {
            get
            {
                if (Input.touchCount == 0)
                {
                    return false;
                }

                Touch touch = Input.GetTouch(0);
                Vector2Int touchPosition = Vector2Int.RoundToInt(touch.position);
                return touch.phase == TouchPhase.Began
                    && Widget.CloseButtonRect.Contains(touchPosition);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the gear button is pressed this frame.
        /// </summary>
        public static bool IsGearButtonPressed
        {
            get
            {
                if (Input.touchCount == 0)
                {
                    return false;
                }

                Touch touch = Input.GetTouch(0);
                Vector2Int touchPosition = Vector2Int.RoundToInt(touch.position);
                return touch.phase == TouchPhase.Began
                    && Widget.GearButtonRect.Contains(touchPosition);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Cardboard trigger button is pressed this frame.
        /// </summary>
        public static bool IsTriggerPressed
        {
            get
            {
                if (Input.touchCount == 0)
                {
                    return false;
                }

                Touch touch = Input.GetTouch(0);
                Vector2Int touchPosition = Vector2Int.RoundToInt(touch.position);
                return touch.phase == TouchPhase.Began
                    && !Widget.CloseButtonRect.Contains(touchPosition)
                    && !Widget.GearButtonRect.Contains(touchPosition);
            }
        }

        /// <summary>
        /// Evaluates whether or not device params are saved in the storage.
        /// </summary>
        ///
        /// <returns>Whether or not device parameters are found.</returns>
        public static bool HasDeviceParams()
        {
            IntPtr encodedDeviceParams;
            int size;
            CardboardQrCode_getSavedDeviceParams(out encodedDeviceParams, out size);
            if (size == 0)
            {
                Debug.Log("[CardboardApi] No device params found.");
                return false;
            }

            Debug.Log("[CardboardApi] Device params found.");
            CardboardQrCode_destroy(encodedDeviceParams);
            _deviceParamsCount = CardboardQrCode_getQrCodeScanCount();
            return true;
        }

        /// <summary>
        /// Starts QR Code scanning activity.
        /// </summary>
        public static void ScanDeviceParams()
        {
            _deviceParamsCount = CardboardQrCode_getQrCodeScanCount();
            Debug.Log("[CardboardApi] QR Code scanning activity launched.");
            CardboardQrCode_scanQrCodeAndSaveDeviceParams();
        }

        /// <summary>
        /// Evaluates if device parameters changed from last time they were reloaded.
        /// </summary>
        ///
        /// <returns>true when device parameters changed.</returns>
        public static bool HasNewDeviceParams()
        {
            // TODO(b/156501367):  Move this logic to the XR display provider.
            if (_deviceParamsCount == -1)
            {
              return false;
            }

            return _deviceParamsCount != CardboardQrCode_getQrCodeScanCount();
        }

        /// <summary>
        /// Enables device parameter reconfiguration on next frame update.
        /// </summary>
        public static void ReloadDeviceParams()
        {
            // TODO(b/156501367):  Move this logic to the XR display provider.
            Debug.Log("[CardboardApi] Reload device parameters.");
            _deviceParamsCount = CardboardQrCode_getQrCodeScanCount();
            CardboardUnity_setDeviceParametersChanged();
        }

        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardQrCode_scanQrCodeAndSaveDeviceParams();

        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardQrCode_getSavedDeviceParams(
          out IntPtr encodedDeviceParams, out int size);

        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardQrCode_destroy(IntPtr encodedDeviceParams);

        [DllImport(ApiConstants.CardboardApi)]
        private static extern int CardboardQrCode_getQrCodeScanCount();

        [DllImport(ApiConstants.CardboardApi)]
        private static extern void CardboardUnity_setDeviceParametersChanged();
    }
}
