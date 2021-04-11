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
        int n = max_iterations.HasValue ? max_iterations.Value : 5;
        // Get the current iteration of the object
        int i = current_iteration.HasValue ? current_iteration.Value : 1;
        // Draw the fractal
        DrawFractalCb(xyz, a, n, i);
    }

    // Create new fractal on startup
    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeMethodLoad()
    {
        new GameObject("TetrahedronGeneratorChild")
        .AddComponent<TetrahedronGenerator>();
    }
}
