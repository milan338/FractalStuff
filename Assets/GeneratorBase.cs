using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorBase : MonoBehaviour
{
    public Mesh mesh;
    public Material material;

    protected Vector3[] vertices;
    protected int[] triangles;

    protected Vector3? start_xyz = null;
    protected float? base_length = null;
    protected int? max_iterations = null;
    protected int? current_iteration = null;

    // Needs to be assigned to draw fractal method
    protected delegate void DrawFractal(Vector3 xyz, float a, int n, int i);

    // Run when object is created
    protected void BeginFractal(DrawFractal DrawFractalCb)
    {
        // Get start coordinates
        Vector3 xyz = start_xyz.HasValue ? start_xyz.Value : new Vector3(0, 0, 0);
        // Get original tetrahedron length
        float a = base_length.HasValue ? base_length.Value : 50;
        // Get total number of iterations to run
        int n = max_iterations.HasValue ? max_iterations.Value : 3;
        // Get the current iteration of the object
        int i = current_iteration.HasValue ? current_iteration.Value : 1;
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
        material = new Material(Shader.Find("Diffuse"));
        gameObject.AddComponent<MeshRenderer>().material = material;
    }

    // Draw a mesh from calculated points
    protected void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }

    // Update private variables
    protected void SetData(Vector3 xyz, float a, int n, int i)
    {
        start_xyz = xyz;
        base_length = a;
        max_iterations = n;
        current_iteration = i;
    }

    // Create new fractal on startup
    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeMethodLoad()
    {
        new GameObject("TetrahedronGenerator")
        .AddComponent<TetrahedronGenerator>();
    }
}
