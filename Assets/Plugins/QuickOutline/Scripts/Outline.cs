//
//  Outline.cs
//  QuickOutline
//
//  Created by Chris Nolet on 3/30/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuickOutline.Scripts
{
    [DisallowMultipleComponent]
    public class Outline : MonoBehaviour
    {
        private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

        public enum Mode
        {
            OutlineAll,
            OutlineVisible,
            OutlineHidden,
            OutlineAndSilhouette,
            SilhouetteOnly
        }

        public Mode OutlineMode
        {
            get { return this.outlineMode; }
            set
            {
                this.outlineMode = value;
                this.needsUpdate = true;
            }
        }

        public Color OutlineColor
        {
            get { return this.outlineColor; }
            set
            {
                this.outlineColor = value;
                this.needsUpdate = true;
            }
        }

        public Color OutlineSubColor
        {
            get { return this.outlineSubColor; }
            set
            {
                this.outlineSubColor = value;
                this.needsUpdate = true;
            }
        }

        public float OutlineWidth
        {
            get { return this.outlineWidth; }
            set
            {
                this.outlineWidth = value;
                this.needsUpdate = true;
            }
        }

        [Serializable]
        private class ListVector3
        {
            public List<Vector3> data;
        }

        [SerializeField] private Mode outlineMode;

        [SerializeField] private Color outlineColor = Color.white;

        [SerializeField] private Color outlineSubColor = Color.clear;

        [SerializeField, Range(0f, 10f)] private float outlineWidth = 2f;

        [Header("Optional")]
        [SerializeField, Tooltip(
             "Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
             + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
        private bool precomputeOutline;

        [SerializeField, HideInInspector] private List<Mesh> bakeKeys = new List<Mesh>();

        [SerializeField, HideInInspector] private List<ListVector3> bakeValues = new List<ListVector3>();

        private Renderer[] renderers;
        private Material outlineMaskMaterial;
        private Material outlineFillMaterial;

        private bool needsUpdate;
        private static readonly int MainColor = Shader.PropertyToID("_OutlineColor");
        private static readonly int ZTest = Shader.PropertyToID("_ZTest");
        private static readonly int Width = Shader.PropertyToID("_OutlineWidth");
        private static readonly int SubColor = Shader.PropertyToID("_OutlineSubColor");

        void Awake()
        {
            // Cache renderers
            this.renderers = this.GetComponentsInChildren<Renderer>();

            // Instantiate outline materials
            this.outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
            this.outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

            this.outlineMaskMaterial.name = "OutlineMask (Instance)";
            this.outlineFillMaterial.name = "OutlineFill (Instance)";

            // Retrieve or generate smooth normals
            this.LoadSmoothNormals();

            // Apply material properties immediately
            this.needsUpdate = true;
        }

        void OnEnable()
        {
            foreach (var renderer in this.renderers)
            {
                // Append outline shaders
                var materials = renderer.sharedMaterials.ToList();

                materials.Add(this.outlineMaskMaterial);
                materials.Add(this.outlineFillMaterial);

                renderer.materials = materials.ToArray();
            }
        }

        void OnValidate()
        {
            // Update material properties
            this.needsUpdate = true;

            // Clear cache when baking is disabled or corrupted
            if (!this.precomputeOutline && this.bakeKeys.Count != 0 || this.bakeKeys.Count != this.bakeValues.Count)
            {
                this.bakeKeys.Clear();
                this.bakeValues.Clear();
            }

            // Generate smooth normals when baking is enabled
            if (this.precomputeOutline && this.bakeKeys.Count == 0)
            {
                this.Bake();
            }
        }

        void Update()
        {
            if (this.needsUpdate)
            {
                this.needsUpdate = false;

                this.UpdateMaterialProperties();
            }
        }

        void OnDisable()
        {
            foreach (var renderer in this.renderers)
            {
                // Remove outline shaders
                var materials = renderer.sharedMaterials.ToList();

                materials.Remove(this.outlineMaskMaterial);
                materials.Remove(this.outlineFillMaterial);

                renderer.materials = materials.ToArray();
            }
        }

        void OnDestroy()
        {
            // Destroy material instances
            Destroy(this.outlineMaskMaterial);
            Destroy(this.outlineFillMaterial);
        }

        void Bake()
        {
            // Generate smooth normals for each mesh
            var bakedMeshes = new HashSet<Mesh>();

            foreach (var meshFilter in this.GetComponentsInChildren<MeshFilter>())
            {
                // Skip duplicates
                if (!bakedMeshes.Add(meshFilter.sharedMesh))
                {
                    continue;
                }

                // Serialize smooth normals
                var smoothNormals = this.SmoothNormals(meshFilter.sharedMesh);

                this.bakeKeys.Add(meshFilter.sharedMesh);
                this.bakeValues.Add(new ListVector3() { data = smoothNormals });
            }
        }

        void LoadSmoothNormals()
        {
            // Retrieve or generate smooth normals
            foreach (var meshFilter in this.GetComponentsInChildren<MeshFilter>())
            {
                // Skip if smooth normals have already been adopted
                if (!registeredMeshes.Add(meshFilter.sharedMesh))
                {
                    continue;
                }

                // Retrieve or generate smooth normals
                var index = this.bakeKeys.IndexOf(meshFilter.sharedMesh);
                var smoothNormals =
                    (index >= 0) ? this.bakeValues[index].data : this.SmoothNormals(meshFilter.sharedMesh);

                // Store smooth normals in UV3
                meshFilter.sharedMesh.SetUVs(3, smoothNormals);

                // Combine submeshes
                var renderer = meshFilter.GetComponent<Renderer>();

                if (renderer != null)
                {
                    this.CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
                }
            }

            // Clear UV3 on skinned mesh renderers
            foreach (var skinnedMeshRenderer in this.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                // Skip if UV3 has already been reset
                if (!registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
                {
                    continue;
                }

                // Clear UV3
                skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];

                // Combine submeshes
                this.CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
            }
        }

        List<Vector3> SmoothNormals(Mesh mesh)
        {
            // Group vertices by location
            var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index))
                .GroupBy(pair => pair.Key);

            // Copy normals to a new list
            var smoothNormals = new List<Vector3>(mesh.normals);

            // Average normals for grouped vertices
            foreach (var group in groups)
            {
                // Skip single vertices
                if (group.Count() == 1)
                {
                    continue;
                }

                // Calculate the average normal
                var smoothNormal = Vector3.zero;

                foreach (var pair in group)
                {
                    smoothNormal += smoothNormals[pair.Value];
                }

                smoothNormal.Normalize();

                // Assign smooth normal to each vertex
                foreach (var pair in group)
                {
                    smoothNormals[pair.Value] = smoothNormal;
                }
            }

            return smoothNormals;
        }

        void CombineSubmeshes(Mesh mesh, Material[] materials)
        {
            // Skip meshes with a single submesh
            if (mesh.subMeshCount == 1)
            {
                return;
            }

            // Skip if submesh count exceeds material count
            if (mesh.subMeshCount > materials.Length)
            {
                return;
            }

            // Append combined submesh
            mesh.subMeshCount++;
            mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
        }

        void UpdateMaterialProperties()
        {
            // Apply properties according to mode
            this.outlineFillMaterial.SetColor(MainColor, this.outlineColor);
            this.outlineFillMaterial.SetColor(SubColor,
                this.outlineSubColor == Color.clear ? this.outlineColor : this.outlineSubColor);

            switch (this.outlineMode)
            {
                case Mode.OutlineAll:
                    this.outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                    this.outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                    this.outlineFillMaterial.SetFloat(Width, this.outlineWidth);
                    break;

                case Mode.OutlineVisible:
                    this.outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                    this.outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                    this.outlineFillMaterial.SetFloat(Width, this.outlineWidth);
                    break;

                case Mode.OutlineHidden:
                    this.outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                    this.outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Greater);
                    this.outlineFillMaterial.SetFloat(Width, this.outlineWidth);
                    break;

                case Mode.OutlineAndSilhouette:
                    this.outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                    this.outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                    this.outlineFillMaterial.SetFloat(Width, this.outlineWidth);
                    break;

                case Mode.SilhouetteOnly:
                    this.outlineMaskMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                    this.outlineFillMaterial.SetFloat(ZTest, (float)UnityEngine.Rendering.CompareFunction.Greater);
                    this.outlineFillMaterial.SetFloat(Width, 0f);
                    break;
            }
        }
    }
}