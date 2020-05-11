//-----------------------------------------------------------------------
// <copyright file="BuildPostProcessor.cs" company="Google LLC">
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

#if UNITY_EDITOR

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
            // If we are building for iOS, we need to disable EmbedBitcode support.
            if (buildTarget == BuildTarget.iOS)
            {
                string projectPath = PBXProject.GetPBXProjectPath(path);
                string projectConfig = File.ReadAllText(projectPath);
                projectConfig = projectConfig.Replace("ENABLE_BITCODE = YES",
                                                      "ENABLE_BITCODE = NO");
                File.WriteAllText(projectPath, projectConfig);
            }
        }
    }
}

#endif
