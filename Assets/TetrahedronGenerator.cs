using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrahedronGenerator : GeneratorBase
{
    // Cache offsets to reduce calculations
    static Vector3?[,] start_offsets = null;
    static Vector3?[,] point_offsets = null;
    static float?[] lengths = null;

    // Run when object is created
    private void Start()
    {
        if (parent_obj == null)
            parent_obj = gameObject;
        DrawFractal DrawTetrahedronFractalCb = DrawTetrahedronFractal;
        BeginFractal(DrawTetrahedronFractalCb);
    }

    // Run on frame update
    private void Update() { }

    // Draw tetrahedra or continue recursion
    private void DrawTetrahedronFractal(Vector3 xyz, float a, int n, int i)
    {
        // Calculate the total number of tetrahedra to draw
        if (!max_obj.HasValue)
            max_obj = (int)Mathf.Pow(4f, n - 1);
        // Update cache arrays
        if (start_offsets == null | point_offsets == null | lengths == null)
        {
            start_offsets = new Vector3?[n, 4];
            point_offsets = new Vector3?[n, 4];
            lengths = new float?[n];
        }
        // Update cached length
        if (!lengths[i - 1].HasValue)
            // Side length at iteration i = a / 2^i
            lengths[i - 1] = a / Mathf.Pow(2, i);
        float l = lengths[i - 1].Value;
        // Only one tetrahedron to be drawn
        if (n == 1)
            DrawTetrahedron(xyz, a, n);
        // Draw tetrahedron at bottom iteration
        else if (n == i)
            DrawTetrahedron(xyz, l, n);
        // Draw mesh from calculated points
        if (n == 1 | n == i)
        {
            // Update mesh
            CreateMesh();
            UpdateMesh(n);
        }
        // Stop recursion past defined iteration
        else if (i < n)
        {
            // Calculate offsets
            CalculateOffsets(i, start_offsets);
            // Points to start drawing tetrahedra from
            Vector3[] points = new Vector3[4];
            for (int j = 0; j < 4; j++)
            {
                points[j] = new Vector3(
                    xyz.x + start_offsets[i - 1, j].Value.x,
                    xyz.y + start_offsets[i - 1, j].Value.y,
                    xyz.z + start_offsets[i - 1, j].Value.z);
            }
            // Continue to next iteration
            for (int j = 0; j < 4; j++)
            {
                // Create 4 smaller tetrahedra that line up
                new GameObject("TetrahedronGeneratorChild")
                .AddComponent<TetrahedronGenerator>()
                .SetData(new Vector3(points[j].x, points[j].y, points[j].z), a, n, i + 1);
            }
            // Don't destroy root object
            if (i != 1)
                // Destroy current game object
                Destroy(gameObject);
        }
    }

    // Draw single tetrahedron
    private void DrawTetrahedron(Vector3 xyz, float l, int n)
    {
        // Calculate offsets
        CalculateOffsets(n, point_offsets);
        // Vertices for individual tetrahedron
        vertices = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            vertices[i] = new Vector3(
                xyz.x + point_offsets[n - 1, i].Value.x,
                xyz.y + point_offsets[n - 1, i].Value.y,
                xyz.z + point_offsets[n - 1, i].Value.z);
        }
        // Order to create triangles from vertices in
        triangles = new int[]
        {
            0,1,2,
            0,2,3,
            2,1,3,
            0,3,1
        };
    }

    private void CalculateOffsets(int i, Vector3?[,] offset_array)
    {
        // Offset already exists - do nothing
        if (offset_array[i - 1, 0] != null)
            return;
        // Some factors in start offsets halved compared to point offsets
        float f = offset_array == start_offsets ? 2f : 1f;
        float l = lengths[i - 1].Value;
        // Set offsets
        offset_array[i - 1, 0] = new Vector3(0, 0, 0);
        offset_array[i - 1, 1] = new Vector3(l / (f * 1f), 0, 0);
        offset_array[i - 1, 2] = new Vector3(l / (f * 2f), 0, l * (Mathf.Sqrt(3f) / (f * 2f)));
        offset_array[i - 1, 3] = new Vector3(l / (f * 2f), l * (Mathf.Sqrt(6f) / (f * 3f)), (l / (f * 2f)) * Mathf.Tan(Mathf.PI / 6f));
    }
}
