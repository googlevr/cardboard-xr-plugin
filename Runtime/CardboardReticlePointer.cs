//-----------------------------------------------------------------------
// <copyright file="CardboardReticlePointer.cs" company="Google LLC">
// Copyright 2023 Google LLC
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

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Draws a circular reticle in front of any object that the user points at.
/// </summary>
/// <remarks>
/// Sends messages to gazed GameObject. The reticle dilates if the object has an interactive layer.
/// </remarks>
public class CardboardReticlePointer : MonoBehaviour
{
    /// <summary>
    /// Sorting order to use for the reticle's renderer.
    /// </summary>
    /// <remarks><para>
    /// Range values come from https://docs.unity3d.com/ScriptReference/Renderer-sortingOrder.html.
    /// </para><para>
    /// Default value 32767 ensures gaze reticle is always rendered on top.
    /// </para></remarks>
    [Range(-32767, 32767)]
    public int ReticleSortingOrder = 32767;

    /// <summary>
    /// Mask used to indicate interactive objects.
    /// </summary>
    public LayerMask ReticleInteractionLayerMask = 1 << _RETICLE_INTERACTION_DEFAULT_LAYER;

    /// <summary>
    /// Default layer for interactive game objects.
    /// </summary>
    private const int _RETICLE_INTERACTION_DEFAULT_LAYER = 8;

    /// <summary>
    /// The angle in degrees defined between the 2 vectors that depart from the camera and point to
    /// the extremes of the minimum inner diameter of the reticle.
    ///
    /// Being `z` the distance from the camera to the object and `d_i` the inner diameter of the
    /// reticle, this is 2*arctg(d_i/(2*z)).
    /// </summary>
    private const float _RETICLE_MIN_INNER_ANGLE = 0.0f;

    /// <summary>
    /// The angle in degrees defined between the 2 vectors that depart from the camera and point to
    /// the extremes of the minimum outer diameter of the reticle.
    ///
    /// Being `z` the distance from the camera to the object and `d_o` the outer diameter of the
    /// reticle, this is 2*arctg(d_o/(2*z)).
    /// </summary>
    private const float _RETICLE_MIN_OUTER_ANGLE = 0.5f;

    /// <summary>
    /// Angle at which to expand the reticle when intersecting with an object (in degrees).
    /// </summary>
    private const float _RETICLE_GROWTH_ANGLE = 1.5f;

    /// <summary>
    /// Minimum distance between the camera and the reticle (in meters).
    /// </summary>
    private const float _RETICLE_MIN_DISTANCE = 0.45f;

    /// <summary>
    /// Maximum distance between the camera and the reticle (in meters).
    /// </summary>
    private const float _RETICLE_MAX_DISTANCE = 20.0f;

    /// <summary>
    /// Number of segments making the reticle circle.
    /// </summary>
    private const int _RETICLE_SEGMENTS = 20;

    /// <summary>
    /// Growth speed multiplier for the reticle.
    /// </summary>
    private const float _RETICLE_GROWTH_SPEED = 8.0f;

    /// <summary>
    /// The game object the reticle is pointing at.
    /// </summary>
    private GameObject _gazedAtObject = null;

    /// <summary>
    /// The material used to render the reticle.
    /// </summary>
    private Material _reticleMaterial;

    /// <summary>
    /// The current inner angle of the reticle (in degrees).
    /// </summary>
    private float _reticleInnerAngle;

    /// <summary>
    /// The current outer angle of the reticle (in degrees).
    /// </summary>
    private float _reticleOuterAngle;

    /// <summary>
    /// The current distance of the reticle (in meters).
    /// </summary>
    private float _reticleDistanceInMeters;

    /// <summary>
    /// The current inner diameter of the reticle, before distance multiplication (in meters).
    /// </summary>
    private float _reticleInnerDiameter;

    /// <summary>
    /// The current outer diameter of the reticle, before distance multiplication (in meters).
    /// </summary>
    private float _reticleOuterDiameter;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    private void Start()
    {
        Renderer rendererComponent = GetComponent<Renderer>();
        rendererComponent.sortingOrder = ReticleSortingOrder;

        _reticleMaterial = rendererComponent.material;

        CreateMesh();
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    private void Update()
    {
        // Casts ray towards camera's forward direction, to detect if a GameObject is being gazed
        // at.
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, _RETICLE_MAX_DISTANCE))
        {
            // GameObject detected in front of the camera.
            if (_gazedAtObject != hit.transform.gameObject)
            {
                // New GameObject.
                if (IsInteractive(_gazedAtObject))
                {
                    _gazedAtObject?.SendMessage("OnPointerExit");
                }

                _gazedAtObject = hit.transform.gameObject;

                if (IsInteractive(_gazedAtObject))
                {
                    _gazedAtObject.SendMessage("OnPointerEnter");
                }
            }

            SetParams(hit.distance, IsInteractive(_gazedAtObject));
        }
        else
        {
            // No GameObject detected in front of the camera.
            if (IsInteractive(_gazedAtObject))
            {
                _gazedAtObject?.SendMessage("OnPointerExit");
            }

            _gazedAtObject = null;
            ResetParams();
        }

        // Checks for screen touches.
        if (Google.XR.Cardboard.Api.IsTriggerPressed)
        {
            if (IsInteractive(_gazedAtObject))
            {
                _gazedAtObject?.SendMessage("OnPointerClick");
            }
        }

        UpdateDiameters();
    }

    /// <summary>
    /// Updates the material based on the reticle properties.
    /// </summary>
    private void UpdateDiameters()
    {
        _reticleDistanceInMeters =
      Mathf.Clamp(_reticleDistanceInMeters, _RETICLE_MIN_DISTANCE, _RETICLE_MAX_DISTANCE);

        if (_reticleInnerAngle < _RETICLE_MIN_INNER_ANGLE)
        {
            _reticleInnerAngle = _RETICLE_MIN_INNER_ANGLE;
        }

        if (_reticleOuterAngle < _RETICLE_MIN_OUTER_ANGLE)
        {
            _reticleOuterAngle = _RETICLE_MIN_OUTER_ANGLE;
        }

        float inner_half_angle_radians = Mathf.Deg2Rad * _reticleInnerAngle * 0.5f;
        float outer_half_angle_radians = Mathf.Deg2Rad * _reticleOuterAngle * 0.5f;

        float inner_diameter = 2.0f * Mathf.Tan(inner_half_angle_radians);
        float outer_diameter = 2.0f * Mathf.Tan(outer_half_angle_radians);

        _reticleInnerDiameter = Mathf.Lerp(
            _reticleInnerDiameter, inner_diameter, Time.unscaledDeltaTime * _RETICLE_GROWTH_SPEED);
        _reticleOuterDiameter = Mathf.Lerp(
            _reticleOuterDiameter, outer_diameter, Time.unscaledDeltaTime * _RETICLE_GROWTH_SPEED);

        _reticleMaterial.SetFloat(
            "_InnerDiameter", _reticleInnerDiameter * _reticleDistanceInMeters);
        _reticleMaterial.SetFloat(
            "_OuterDiameter", _reticleOuterDiameter * _reticleDistanceInMeters);
        _reticleMaterial.SetFloat("_DistanceInMeters", _reticleDistanceInMeters);
    }

    /// <summary>
    /// Sets the reticle pointer's inner angle, outer angle and distance.
    /// </summary>
    /// <param name="distance">The distance to the target location.</param>
    /// <param name="interactive">Whether the pointer is pointing at an interactive object.</param>
    private void SetParams(float distance, bool interactive)
    {
        _reticleDistanceInMeters = Mathf.Clamp(distance,
                                              _RETICLE_MIN_DISTANCE,
                                              _RETICLE_MAX_DISTANCE);
        if (interactive)
        {
            _reticleInnerAngle = _RETICLE_MIN_INNER_ANGLE + _RETICLE_GROWTH_ANGLE;
            _reticleOuterAngle = _RETICLE_MIN_OUTER_ANGLE + _RETICLE_GROWTH_ANGLE;
        }
        else
        {
            _reticleInnerAngle = _RETICLE_MIN_INNER_ANGLE;
            _reticleOuterAngle = _RETICLE_MIN_OUTER_ANGLE;
        }
    }

    /// <summary>
    /// Exits the reticle pointer's target.
    /// </summary>
    private void ResetParams()
    {
        _reticleDistanceInMeters = _RETICLE_MAX_DISTANCE;
        _reticleInnerAngle = _RETICLE_MIN_INNER_ANGLE;
        _reticleOuterAngle = _RETICLE_MIN_OUTER_ANGLE;
    }

    /// <summary>
    /// Creates the mesh used to draw the reticle.
    /// </summary>
    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>();
        GetComponent<MeshFilter>().mesh = mesh;

        int segments_count = _RETICLE_SEGMENTS;
        int vertex_count = (segments_count + 1) * 2;

        // Vertices.
        Vector3[] vertices = new Vector3[vertex_count];

        const float kTwoPi = Mathf.PI * 2.0f;
        int vi = 0;
        for (int si = 0; si <= segments_count; ++si)
        {
            // Add two vertices for every circle segment: one at the beginning of the
            // prism, and one at the end of the prism.
            float angle = (float)si / (float)segments_count * kTwoPi;

            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);

            vertices[vi++] = new Vector3(x, y, 0.0f); // Outer vertex.
            vertices[vi++] = new Vector3(x, y, 1.0f); // Inner vertex.
        }

        // Triangles.
        int indices_count = (segments_count + 1) * 3 * 2;
        int[] indices = new int[indices_count];

        int vert = 0;
        int idx = 0;
        for (int si = 0; si < segments_count; ++si)
        {
            indices[idx++] = vert + 1;
            indices[idx++] = vert;
            indices[idx++] = vert + 2;

            indices[idx++] = vert + 1;
            indices[idx++] = vert + 2;
            indices[idx++] = vert + 3;

            vert += 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateBounds();
    }

    /// <summary>
    /// Evaluates if the provided GameObject is interactive based on its layer.
    /// </summary>
    ///
    /// <param name="gameObject">The game object on which to check if its layer is interactive.</param>
    ///
    /// <returns>Whether or not a GameObject's layer is interactive.</returns>
    private bool IsInteractive(GameObject gameObject)
    {
        return (1 << gameObject?.layer & ReticleInteractionLayerMask) != 0;
    }
}
