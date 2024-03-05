//-----------------------------------------------------------------------
// <copyright file="GraphicsAPITextController.cs" company="Google LLC">
// Copyright 2022 Google LLC
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Initializes Graphics API Text Mesh according to the used graphics API.
/// </summary>
public class GraphicsAPITextController : MonoBehaviour
{
    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start()
    {
        TextMesh tm = gameObject.GetComponent(typeof(TextMesh)) as TextMesh;
        switch (SystemInfo.graphicsDeviceType)
        {
#if !UNITY_2023_1_OR_NEWER
            case GraphicsDeviceType.OpenGLES2:
                tm.text = "OpenGL ES 2";
                break;
#endif
            case GraphicsDeviceType.OpenGLES3:
                tm.text = "OpenGL ES 3";
                break;
#if UNITY_IOS
            case GraphicsDeviceType.Metal:
                tm.text = "Metal";
                break;
#endif
#if UNITY_ANDROID
            case GraphicsDeviceType.Vulkan:
                tm.text = "Vulkan";
                break;
#endif
            default:
                tm.text = "Unrecognized Graphics API";
                break;
        }
    }
}
