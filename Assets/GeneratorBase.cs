using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorBase : MonoBehaviour
{
    // Cache offsets to reduce calculations
    static protected Vector3?[,] start_offsets = null;
    static protected Vector3?[,] point_offsets = null;
    static protected float?[] lengths = null;
    // Mesh to display fractal
    public Mesh mesh;
    static public Material material = null;
    // Store vertices and order to draw triangles between
    protected Vector3[] vertices;
    protected int[] triangles;
    // Fractal data
    protected Vector3? start_xyz = null;
    protected float? base_length = null;
    protected int? max_iterations = null;
    protected int? current_iteration = null;
    // Track completion of iterations
    static protected int? max_obj = null;
    static protected int obj_count = 0;
    // Parent object that will display the final mesh
    static protected GameObject parent_obj = null;
    // List of mesh combines to later combine into single mesh in parent
    static protected List<CombineInstance> mesh_combine = new List<CombineInstance>();

    // Needs to be assigned to draw fractal method
    protected delegate void DrawFractal(Vector3 xyz, float a, int n, int i);

    // Run when object is created
    protected void BeginFractal(DrawFractal DrawFractalCb)
    {
        // Get start coordinates
        Vector3 xyz = start_xyz.HasValue ? start_xyz.Value : new Vector3(0, 0, 0);
        // Get original tetrahedron length
        float a = base_length.HasValue ? base_length.Value : 50;
        // Get total number of iterations to run (starting at 0)
        // TODO max supported vertices 4,294,967,295 prevent anything higher - should be 15 actual iterations
        int n = max_iterations.HasValue ? max_iterations.Value : 4;
        // Get the current iteration of the object
        int i = current_iteration.HasValue ? current_iteration.Value : 0;
        // Draw the fractal
        DrawFractalCb(xyz, a, n, i);
    }

    // Update object with new mesh and material
    protected void CreateMesh()
    {
        // Add mesh to object
        mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        // Add diffuse material to object
        if (material == null)
        {
            material = new Material(Shader.Find("Diffuse"));
            material.enableInstancing = true;
        }
        gameObject.AddComponent<MeshRenderer>().material = material;
    }

    // Draw a mesh from calculated points
    protected void UpdateMesh(int n)
    {
        // Update mesh data
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        // Add mesh data to combine list
        MeshFilter[] mesh_filters = GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < mesh_filters.Length; i++)
        {
            CombineInstance combine = new CombineInstance();
            combine.mesh = mesh_filters[i].mesh;
            combine.transform = mesh_filters[i].transform.localToWorldMatrix;
            mesh_combine.Add(combine);
        }
        // Increment object counter
        obj_count++;
        // Combine meshes once all meshes created, skip for n = 0
        if (obj_count == max_obj.Value & n != 0)
            // Get parent object to combine meshes and set as its own
            parent_obj.GetComponent<TetrahedronGenerator>().CombineMeshes();
        // Remove game object, skip for n = 0
        if (n != 0)
            Destroy(gameObject);
    }

    // Update private variables
    protected void SetData(Vector3 xyz, float a, int n, int i)
    {
        start_xyz = xyz;
        base_length = a;
        max_iterations = n;
        current_iteration = i;
    }

    // Combine child meshes into parent mesh
    protected void CombineMeshes()
    {
        mesh = new Mesh();
        // For any iterations with more than 65535 vertices, the standard UInt16 format does not suffice
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.CombineMeshes(mesh_combine.ToArray());
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().material = material;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }

    // Create new fractal on startup
    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeMethodLoad()
    {
        new GameObject("TetrahedronGenerator")
        .AddComponent<TetrahedronGenerator>();
    }
}
