using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrahedronGenerator : GeneratorBase
{
    // Cache offsets to reduce calculations
    static protected Vector3?[,] start_offsets = null;
    static protected Vector3?[,] point_offsets = null;
    static protected float?[] lengths = null;
    // Track completion of iterations
    static protected int obj_count = 0;

    // Run when object is created
    private void Start()
    {
        // Setup parent object
        SetupParent(true);
        // Begin drawing fractal
        DrawFractal DrawTetrahedronFractalCb = DrawTetrahedronFractal;
        BeginFractal(DrawTetrahedronFractalCb);
    }

    // Run on frame update
    private void Update() { }

    // Draw tetrahedra or continue recursion
    private void DrawTetrahedronFractal(Vector3 xyz, float a, int n, int i)
    {
        // Calculate the total number of tetrahedra to draw
        if (!max_objects.HasValue)
            // Number of tetrahedra at iteration n = 4^n
            max_objects = (int)Mathf.Pow(4f, n);
        // Update cache arrays
        if (start_offsets == null | point_offsets == null | lengths == null | mesh_combine == null)
        {
            start_offsets = new Vector3?[n + 1, 4];
            point_offsets = new Vector3?[n + 1, 4];
            lengths = new float?[n + 1];
            mesh_combine = new List<CombineInstance>();
        }
        // Update cached length
        if (!lengths[i].HasValue)
            // Side length at iteration i = a / 2^i
            lengths[i] = a / Mathf.Pow(2f, i);
        float l = lengths[i].Value;
        // Calculate midpoint
        AddMidpoint(xyz, i, 4, CalculateOffsets);
        // Only one tetrahedron to be drawn
        if (n == 0)
            DrawTetrahedron(xyz, a, n);
        // Draw tetrahedron at bottom iteration
        else if (n == i)
            DrawTetrahedron(xyz, l, n);
        // Draw mesh from calculated points
        if (n == 0 | n == i)
        {
            // Update mesh
            if (n != 0)
                CreateMesh();
            UpdateMesh<TetrahedronGenerator>(ref obj_count, n, true);
        }
        // Stop recursion past defined iteration
        else if (i < n)
        {
            // Calculate offsets
            CalculateOffsets(i, start_offsets);
            // Continue to next iteration
            NextIteration<TetrahedronGenerator>(xyz, start_offsets, a, n, i, 4);
            // Don't destroy parent object
            if (i != 0)
                // Destroy current game object
                DestroyImmediate(gameObject);
        }
    }

    // Draw single tetrahedron
    private void DrawTetrahedron(Vector3 xyz, float l, int i)
    {
        // Calculate offsets
        CalculateOffsets(i, point_offsets);
        // Vertices for individual tetrahedron
        vertices = AddOffsets(xyz, point_offsets, i, 4);
        // Order to create triangles from vertices in
        triangles = new int[]
        {
            0,1,2,
            0,2,3,
            2,1,3,
            0,3,1
        };
    }

    // Calculate offsets for different start points / draw points for drawing tetrahedra
    private void CalculateOffsets(int i, Vector3?[,] offset_array, float s = 1f)
    {
        // Offset already exists - do nothing
        if (offset_array[i, 0] != null)
            return;
        // Some factors in start offsets halved compared to point offsets
        float f = offset_array == start_offsets ? 2f : 1f;
        float l = lengths[i].Value * s;
        // Set offsets
        offset_array[i, 0] = new Vector3(0, 0, 0);
        offset_array[i, 1] = new Vector3(l / (f * 1f), 0, 0);
        offset_array[i, 2] = new Vector3(l / (f * 2f), 0, l * (Mathf.Sqrt(3f) / (f * 2f)));
        offset_array[i, 3] = new Vector3(
            l / (f * 2f),
            l * (Mathf.Sqrt(6f) / (f * 3f)),
            (l / (f * 2f)) * Mathf.Tan(Mathf.PI / 6f));
    }

    // Reset static variables after drawing complete
    protected override void Cleanup()
    {
        base.Cleanup();
        start_offsets = null;
        point_offsets = null;
        lengths = null;
        obj_count = 0;
    }
}
