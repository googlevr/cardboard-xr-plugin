//-----------------------------------------------------------------------
// <copyright file="BuildPostProcessor.cs" company="Google LLC">
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

#if UNITY_EDITOR && UNITY_IOS

namespace Google.XR.Cardboard.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEditor.iOS.Xcode;
    using UnityEngine;

    /// <summary>Processes the project files after the build is performed.</summary>
    public static class BuildPostProcessor
    {
        /// <summary>Unity callback to process after build.</summary>
        /// <param name="buildTarget">Target built.</param>
        /// <param name="path">Path to built project.</param>
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                // Note: The meta files removal is a workaround for
                // <a https://issuetracker.unity3d.com/issues/possibly-ios-unity-meta-files-are-generated-in-the-plugin-directory-and-then-copied-to-plugins-directory-in-the-xcode-build>Issue #1184957</a>
                // in Unity.
                FileUtil.DeleteFileOrDirectory(
                    path + "/Frameworks/com.google.xr.cardboard/Runtime/iOS/sdk.bundle/qrSample.png.meta");
                FileUtil.DeleteFileOrDirectory(
                    path + "/Frameworks/com.google.xr.cardboard/Runtime/iOS/sdk.bundle/tickmarks.png.meta");
            }
        }
    }
}

#endif
