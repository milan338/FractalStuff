using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PyramidGenerator : GeneratorBase
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
        DrawFractal DrawPyramidFractalCb = DrawPyramidFractal;
        BeginFractal(DrawPyramidFractalCb);
    }

    // Run on frame update
    private void Update() { }

    // Draw pyramids or continue recursion
    private void DrawPyramidFractal(Vector3 xyz, float a, int n, int i)
    {
        // Calculate the total number of pyramids to drw
        if (!max_objects.HasValue)
            // Number of pyramids at iteration n = 5^n
            max_objects = (int)Mathf.Pow(5f, n);
        // Update cache arrays
        if (start_offsets == null | point_offsets == null | lengths == null | mesh_combine == null)
        {
            start_offsets = new Vector3?[n + 1, 5];
            point_offsets = new Vector3?[n + 1, 5];
            lengths = new float?[n + 1];
            mesh_combine = new List<CombineInstance>();
        }
        // Update cached length
        if (!lengths[i].HasValue)
            // Side lengths at iteration i = a / 2^i
            lengths[i] = a / Mathf.Pow(2f, i);
        float l = lengths[i].Value;
        // Calculate midpoint
        AddMidpoint(xyz, i, 5, CalculateOffsets);
        // Only one pyramid to be drawn
        if (n == 0)
            DrawPyramid(xyz, a, n);
        // Draw pyramid at bottom iteration
        else if (n == i)
            DrawPyramid(xyz, l, n);
        // Draw mesh from calculated points
        if (n == 0 | n == i)
        {
            // Update mesh
            if (n != 0)
                CreateMesh();
            UpdateMesh<PyramidGenerator>(ref obj_count, n, true);
        }
        // Stop recursion past defined iteration
        else if (i < n)
        {
            // Calculate offsets
            CalculateOffsets(i, start_offsets);
            // Continue to next iteration
            NextIteration<PyramidGenerator>(xyz, start_offsets, a, n, i, 5);
            // Don't destroy parent object
            if (i != 0)
                // Destroy current game object
                DestroyImmediate(gameObject);
        }
    }

    // Draw single pyramid
    private void DrawPyramid(Vector3 xyz, float l, int i)
    {
        // Calculate offsets
        CalculateOffsets(i, point_offsets);
        // Vertices for individual octahedron
        vertices = AddOffsets(xyz, point_offsets, i, 5);
        // Order to create triangles from vertices in
        triangles = new int[]
        {
            3,1,0,
            3,2,1,
            0,1,4,
            1,2,4,
            2,3,4,
            3,0,4
        };
    }

    // Calculate offsets for different start points / draw points for drawing pyramids
    private void CalculateOffsets(int i, Vector3?[,] offset_array, float s = 1f)
    {
        // Offset already exists - do nothing
        if (offset_array[i, 0] != null)
            return;
        // Some factors in start offets halved compared to point offsets
        float f = offset_array == start_offsets ? 2f : 1f;
        float l = lengths[i].Value * s;
        // Set offsets
        offset_array[i, 0] = new Vector3(0, 0, 0);
        offset_array[i, 1] = new Vector3(0, 0, l / (f * 1f));
        offset_array[i, 2] = new Vector3(l / (f * 1f), 0, l / (f * 1f));
        offset_array[i, 3] = new Vector3(l / (f * 1f), 0, 0);
        offset_array[i, 4] = new Vector3(l / (f * 2f), l * (Mathf.Sqrt(2f) / (f * 2f)), l / (f * 2f));
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
