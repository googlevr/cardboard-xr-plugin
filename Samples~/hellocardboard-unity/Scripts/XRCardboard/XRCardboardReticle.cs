using System.Collections;
using UnityEngine;

public class XRCardboardReticle : MonoBehaviour
{
    readonly int InnerRingHash = Shader.PropertyToID("_InnerRing");
    readonly int OuterRingHash = Shader.PropertyToID("_OuterRing");
    readonly int DistanceHash = Shader.PropertyToID("_Distance");

    [SerializeField]
    MeshRenderer childRenderer = default;
    [SerializeField, Range(.5f, 2)]
    float growthAngle = 1.5f;
    [SerializeField, Range(1, 16)]
    float growthSpeed = 8;
    [SerializeField, Range(0, .5f)]
    float minInnerAngle = 0;
    [SerializeField, Range(.5f, 1)]
    float minOuterAngle = .5f;
    [SerializeField, Range(1, 20)]
    float distance = 2;
    [SerializeField, Range(.5f, 3)]
    float clickFeedbackDuration = 1f;
    [SerializeField, Range(8, 32)]
    int reticleSegments = 20;
    [SerializeField]
    Color defaultColor = Color.white;
    [SerializeField]
    Gradient gazeGradient = default;
    [SerializeField]
    Gradient postGradient = default;
    [SerializeField]
    Color backgroundColor = Color.white;

    MeshRenderer meshRenderer;
    Coroutine hoverRoutine;
    Coroutine clickRoutine;
    Material reticleMaterial;
    Material childMaterial;
    Mesh childMesh;
    float innerDiameter;
    float outerDiameter;
    float innerAngle;
    float outerAngle;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = childRenderer.sortingOrder = 32767;
        reticleMaterial = meshRenderer.material;
        reticleMaterial.color = defaultColor;
        childMaterial = childRenderer.material;
        childRenderer.enabled = false;
        CreateReticleVertices();
        innerAngle = minInnerAngle;
        outerAngle = minOuterAngle;
        reticleMaterial.SetFloat(DistanceHash, distance);
        UpdateReticleDisplay();
    }

    void Update() => UpdateReticleDisplay();

    public void OnStartHover(float gazeTime)
    {
        if (clickRoutine != null)
            StopCoroutine(clickRoutine);
        hoverRoutine = Hover(gazeTime);
        innerAngle = minInnerAngle + growthAngle;
        outerAngle = minOuterAngle + growthAngle;
    }

    public void OnEndHover()
    {
        StopHover();
        reticleMaterial.color = defaultColor;
    }

    public void OnClick()
    {
        StopHover();
        clickRoutine = ClickFeedback();
    }

    void StopHover()
    {
        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);
        childRenderer.enabled = false;
        innerAngle = minInnerAngle;
        outerAngle = minOuterAngle;
    }

    void UpdateReticleDisplay()
    {
        var ratio = Time.unscaledDeltaTime * growthSpeed;
        innerDiameter = Mathf.Lerp(innerDiameter, 2f * Mathf.Tan(Mathf.Deg2Rad * innerAngle * .5f), ratio);
        outerDiameter = Mathf.Lerp(outerDiameter, 2f * Mathf.Tan(Mathf.Deg2Rad * outerAngle * .5f), ratio);

        reticleMaterial.SetFloat(InnerRingHash, innerDiameter * distance);
        reticleMaterial.SetFloat(OuterRingHash, outerDiameter * distance);
    }

    void CreateReticleVertices()
    {
        var mesh = new Mesh();
        var filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = mesh;

        int segments_count = reticleSegments;
        int vertex_count = (segments_count + 1) * 2;

        var vertices = new Vector3[vertex_count];

        const float kTwoPi = Mathf.PI * 2.0f;
        int vi = 0;
        for (int si = 0; si <= segments_count; ++si)
        {
            float angle = si / (float)segments_count * kTwoPi;

            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);

            vertices[vi++] = new Vector3(x, y, 0.0f);
            vertices[vi++] = new Vector3(x, y, 1.0f);
        }

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

        childMesh = new Mesh();
        filter = childRenderer.gameObject.AddComponent<MeshFilter>();
        filter.mesh = childMesh;
        childMesh.vertices = vertices;
        childMesh.triangles = indices;
        childMesh.RecalculateBounds();
    }

    void SetChildFillValue(float amount, Color targetColor)
    {
        childMaterial.SetFloat(InnerRingHash, innerDiameter * distance);
        childMaterial.SetFloat(OuterRingHash, outerDiameter * distance);
        childMaterial.SetFloat(DistanceHash, distance);
        var verticesLength = childMesh.vertices.Length;
        var colors = new Color[verticesLength];

        int i;
        int max = Mathf.FloorToInt(verticesLength / 2f * amount) * 2;
        var clearColor = Color.clear;
        var tempColor = targetColor;
        for (i = 0; i < max; i++)
            colors[i] = clearColor;
        if (max < verticesLength - 1)
        {
            tempColor.a = 1f - (amount * verticesLength - max) / 2;
            colors[max] = tempColor;
            colors[max + 1] = tempColor;
        }

        for (i = max + 2; i < verticesLength; i++)
            colors[i] = targetColor;

        childMesh.colors = colors;
    }

    Coroutine Hover(float gazeTime)
    {
        return StartCoroutine(hoverRoutine());

        IEnumerator hoverRoutine()
        {
            reticleMaterial.color = backgroundColor;
            childRenderer.enabled = true;
            float time = 0;
            float ratio;
            while (time < gazeTime)
            {
                time += Time.unscaledDeltaTime;
                ratio = time / gazeTime;
                SetChildFillValue(ratio, gazeGradient.Evaluate(ratio));
                yield return null;
            }
        }
    }

    Coroutine ClickFeedback()
    {
        return StartCoroutine(clickFeedbackRoutine());
        IEnumerator clickFeedbackRoutine()
        {
            float time = 0;
            while (time < clickFeedbackDuration)
            {
                time += Time.unscaledDeltaTime;
                reticleMaterial.color = postGradient.Evaluate(time / clickFeedbackDuration);
                yield return null;
            }
        }
    }
}