//-----------------------------------------------------------------------
// <copyright file="CardboardMetadata.cs" company="Google LLC">
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

#if XR_MGMT_GTE_320

namespace Google.XR.Cardboard.Editor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.XR.Management.Metadata;
    using UnityEngine;

    /// <summary>
    /// XR Metadata for Cardboard XR Plugin.
    /// Required by XR Management package.
    /// </summary>
    public class CardboardPackage : IXRPackage
    {
        /// <summary>
        /// Package metadata instance.
        /// </summary>
        public IXRPackageMetadata metadata => new PackageMetadata();

        /// <summary>
        /// Populates package settings instance.
        /// </summary>
        ///
        /// <param name="obj">
        /// Settings object.
        /// </param>
        /// <returns>Settings analysis result.</returns>
        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            return false;
        }

        private class LoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName => "Cardboard XR Plugin";

            public string loaderType => typeof(Cardboard.XRLoader).FullName;

            public List<BuildTargetGroup> supportedBuildTargets => new List<BuildTargetGroup>()
            {
                BuildTargetGroup.Android,
                BuildTargetGroup.iOS
            };
        }

        private class PackageMetadata : IXRPackageMetadata
        {
            public string packageName => "Cardboard XR Plugin";

            public string packageId => "com.google.xr.cardboard";

            public string settingsType => typeof(Cardboard.XRSettings).FullName;

            public List<IXRLoaderMetadata> loaderMetadata => new List<IXRLoaderMetadata>()
            {
                new LoaderMetadata()
            };
        }
    }
}

#endif // XR_MGMT_GTE_320
