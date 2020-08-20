//-----------------------------------------------------------------------
// <copyright file="Widget.cs" company="Google LLC">
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
    using UnityEngine;

    /// <summary>
    /// Misc utilities for Cardboard widgets.
    /// </summary>
    public static class Widget
    {
        /// <summary>
        /// Width and height of the gear and close buttons in device-independent pixels.
        /// </summary>
        private const int _buttonSizeDp = 42;

        /// <summary>
        /// Padding for the gear and close buttons in device-independent pixels.
        ///
        /// Padding behaves like CSS padding, it is clickable but with no content rendered.
        /// </summary>
        private const int _buttonPaddingDp = 9;

        /// <summary>
        /// Width of the alignment rectangle in device-independent pixels.
        /// </summary>
        private const int _alignmentWidthDp = 2;

        /// <summary>
        /// Height of the alignment rectangle in millimeters.
        /// </summary>
        private const int _alignmentHeightMm = 24;

        /// <summary>
        /// Gets the rectangle for the close button in pixels in full screen coordinates.
        /// </summary>
        public static RectInt CloseButtonRect
        {
            get
            {
                RectInt rect = new RectInt();
                rect.x = (int)Screen.safeArea.xMin;
                rect.width = DpToPixels(_buttonSizeDp);
                rect.y = (int)Screen.safeArea.yMax - DpToPixels(_buttonSizeDp);
                rect.height = DpToPixels(_buttonSizeDp);
                return rect;
            }
        }

        /// <summary>
        /// Gets the content rendering rectangle for the close button in pixels in safe area
        /// coordinates.
        /// TODO(b/156266086): Change Unity widgets rendering rectangles position to full screen
        /// coordinates.
        /// </summary>
        public static RectInt CloseButtonRenderRect
        {
            get
            {
                RectInt rect = TranslateToSafeAreaFrame(CloseButtonRect);
                int paddingPixels = DpToPixels(_buttonPaddingDp);
                rect.xMin += paddingPixels;
                rect.xMax -= paddingPixels;
                rect.yMin += paddingPixels;
                rect.yMax -= paddingPixels;
                return rect;
            }
        }

        /// <summary>
        /// Gets the rectangle for the gear button in pixels in full screen coordinates.
        /// </summary>
        public static RectInt GearButtonRect
        {
            get
            {
                RectInt rect = new RectInt();
                rect.x = (int)Screen.safeArea.xMax - DpToPixels(_buttonSizeDp);
                rect.width = DpToPixels(_buttonSizeDp);
                rect.y = (int)Screen.safeArea.yMax - DpToPixels(_buttonSizeDp);
                rect.height = DpToPixels(_buttonSizeDp);
                return rect;
            }
        }

        /// <summary>
        /// Gets the content rendering rectangle for the gear button in pixels in safe area
        /// coordinates.
        /// TODO(b/156266086): Change Unity widgets rendering rectangles position to full screen
        /// coordinates.
        /// </summary>
        public static RectInt GearButtonRenderRect
        {
            get
            {
                RectInt rect = TranslateToSafeAreaFrame(GearButtonRect);
                int paddingPixels = DpToPixels(_buttonPaddingDp);
                rect.xMin += paddingPixels;
                rect.xMax -= paddingPixels;
                rect.yMin += paddingPixels;
                rect.yMax -= paddingPixels;
                return rect;
            }
        }

        /// <summary>
        /// Gets the rectangle for the alignment divider in pixels in safe area coordinates.
        /// </summary>
        public static RectInt AlignmentRect
        {
            get
            {
                RectInt rect = new RectInt();
                int widthPixels = DpToPixels(_alignmentWidthDp);
                rect.xMin = ((int)Screen.safeArea.width - widthPixels) / 2;
                rect.xMax = ((int)Screen.safeArea.width + widthPixels) / 2;
                rect.y = 0;
                rect.height = MmToPixels(_alignmentHeightMm);
                return rect;
            }
        }

        private static int DpToPixels(int dp)
        {
            float scale = Screen.dpi / 160;
            return (int)(scale * dp);
        }

        private static int MmToPixels(int mm)
        {
            float scale = Screen.dpi / 25.4f;
            return (int)(scale * mm);
        }

        /// TODO(b/156266086): Change Unity widgets rendering rectangles position to full screen
        /// coordinates.
      
        /// <summary>
        /// Translates the origin of rect to the safe are frame.
        /// </summary>
        ///
        /// <param name="rect">
        /// The rectangle to change its origin frame.
        /// </param>
        ///
        /// <returns>The same rect but translated to the safe area frame.</returns>
        private static RectInt TranslateToSafeAreaFrame(RectInt rect)
        {
            RectInt result = rect;
            result.x -= (int)Screen.safeArea.xMin;
            result.y -= (int)Screen.safeArea.yMin;
            return result;
        }
    }
}
