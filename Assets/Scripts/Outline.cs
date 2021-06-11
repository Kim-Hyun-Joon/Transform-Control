using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Outline : MonoBehaviour {

    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();
    //테두리 색
    public Color OutlineColor {
        get { return outlineColor; }
        set {
            outlineColor = value;
            needsUpdate = true;
        }
    }

    //테두리 두께
    public float OutlineWidth {
        get { return outlineWidth; }
        set {
            outlineWidth = value;
            needsUpdate = true;
        }
    }

    [Serializable]
    private class ListVector3 {
        public List<Vector3> data;
    }

    //에디터 기본 색상
    [SerializeField]
    private Color outlineColor = Color.red;

    //에디터 기본 두께
    [SerializeField, Range(0f, 20f)]
    private float outlineWidth = 10f;

    [SerializeField, HideInInspector]
    private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector]
    private List<ListVector3> bakeValues = new List<ListVector3>();


    [SerializeField]
    private Renderer[] renderers;
    private Material outlineFillMaterial;   //전체를 채우고
    private Material outlineMaskMaterial;   //오브젝트만큼 Mask처리

    private bool needsUpdate;

    private void Awake() {
        // 게임 오브젝트에 붙어 있으며 생성과 동시에 렌더러 캐치
        renderers = GetComponentsInChildren<Renderer>();

        // 테두리 머터리얼 인스턴스화
        outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));
        outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));

        // Retrieve or generate smooth normals
        // 부드러운 법선 검색 또는 생성
        LoadSmoothNormals();

        // Apply material properties immediately
        needsUpdate = true;

    }


    void OnEnable() {
        foreach (var renderer in renderers) {

            // Append outline shaders
            var materials = renderer.sharedMaterials.ToList();

            materials.Add(outlineFillMaterial);
            materials.Add(outlineMaskMaterial);

            renderer.materials = materials.ToArray();
        }
    }


    void Update() {
        if (needsUpdate) {
            needsUpdate = false;

            UpdateMaterialProperties();
        }
    }

    void OnDisable() {
        foreach (var renderer in renderers) {

            // Remove outline shaders
            var materials = renderer.sharedMaterials.ToList();

            materials.Remove(outlineFillMaterial);
            materials.Remove(outlineMaskMaterial);

            renderer.materials = materials.ToArray();
        }
    }

    void OnDestroy() {

        // Destroy material instances
        Destroy(outlineMaskMaterial);
        Destroy(outlineFillMaterial);
    }



    void LoadSmoothNormals() {

        // Retrieve or generate smooth normals
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>()) {

            // Skip if smooth normals have already been adopted
            if (!registeredMeshes.Add(meshFilter.sharedMesh)) {
                continue;
            }

            // Retrieve or generate smooth normals
            var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
            var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

            // Store smooth normals in UV3
            meshFilter.sharedMesh.SetUVs(3, smoothNormals);
        }

        // Clear UV3 on skinned mesh renderers
        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>()) {
            if (registeredMeshes.Add(skinnedMeshRenderer.sharedMesh)) {
                skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];
            }
        }
    }

    void UpdateMaterialProperties() {

        // Apply properties according to mode
        outlineFillMaterial.SetColor("_OutlineColor", outlineColor);


        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
    }
    List<Vector3> SmoothNormals(Mesh mesh) {

        // Group vertices by location
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

        // Copy normals to a new list
        var smoothNormals = new List<Vector3>(mesh.normals);

        // Average normals for grouped vertices
        foreach (var group in groups) {

            // Skip single vertices
            if (group.Count() == 1) {
                continue;
            }

            // Calculate the average normal
            var smoothNormal = Vector3.zero;

            foreach (var pair in group) {
                smoothNormal += mesh.normals[pair.Value];
            }

            smoothNormal.Normalize();

            // Assign smooth normal to each vertex
            foreach (var pair in group) {
                smoothNormals[pair.Value] = smoothNormal;
            }
        }

        return smoothNormals;
    }

}
