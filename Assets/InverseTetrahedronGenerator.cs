using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseTetrahedronGenerator : GeneratorBase
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
        if (parent_obj == null)
            parent_obj = gameObject;
        // Begin drawing fractal
        DrawFractal DrawInverseTetrahedronFractalCb = DrawInverseTetrahedronFractal;
        BeginFractal(DrawInverseTetrahedronFractalCb);
    }

    // Run on frame update
    private void Update() { }

    // Draw octahedra or continue recursion
    private void DrawInverseTetrahedronFractal(Vector3 xyz, float a, int n, int i)
    {
        // Calculate the total number of octahedra to draw
        if (!max_objects.HasValue)
            // Number of octahedra at iteration n = (4^(n+1) - 1) / 3
            max_objects = (int)((Mathf.Pow(4f, n + 1) - 1f) / 3f);
        // Update cache arrays
        if (start_offsets == null | point_offsets == null | lengths == null | mesh_combine == null)
        {
            start_offsets = new Vector3?[n + 1, 4];
            point_offsets = new Vector3?[n + 1, 6];
            lengths = new float?[n + 1];
            mesh_combine = new List<CombineInstance>();
        }
        // Update cached length
        if (!lengths[i].HasValue)
            // Side length at iteration i = a / 2^i
            lengths[i] = a / Mathf.Pow(2, i);
        float l = lengths[i].Value;
        // Only one octahedron to be drawn
        if (n == 0)
            DrawOctahedron(xyz, a, n);
        else
            DrawOctahedron(xyz, l, i);
        // Draw mesh from calculated points
        CreateMesh();
        bool destroy = i == 0 ? false : true;
        UpdateMesh<InverseTetrahedronGenerator>(ref obj_count, n, destroy);
        // Stop recursion past defined iteration
        if (i < n)
        {
            // Calculate offsets
            CalculateOffsets(i, start_offsets);
            // Points to start drawing octahedra from
            Vector3[] points = new Vector3[4];
            for (int j = 0; j < 4; j++)
            {
                points[j] = new Vector3(
                    xyz.x + start_offsets[i, j].Value.x,
                    xyz.y + start_offsets[i, j].Value.y,
                    xyz.z + start_offsets[i, j].Value.z);
            }
            // Continue to next iteration
            for (int j = 0; j < 4; j++)
            {
                // Create 4 smaller tetrahedra that line up
                new GameObject("InverseTetrahedronGeneratorChild")
                .AddComponent<InverseTetrahedronGenerator>()
                .SetData(
                    material,
                    max_objects,
                    parent_obj,
                    mesh_combine,
                    new Vector3(points[j].x, points[j].y, points[j].z),
                    a, n, i + 1);
            }
        }
    }

    // Draw single octahedron
    private void DrawOctahedron(Vector3 xyz, float l, int i)
    {
        // Calculate offsets
        CalculateOffsets(i, point_offsets);
        // Vertices for individual octahedron
        vertices = new Vector3[6];
        for (int j = 0; j < 6; j++)
        {
            vertices[j] = new Vector3(
                xyz.x + point_offsets[i, j].Value.x,
                xyz.y + point_offsets[i, j].Value.y,
                xyz.z + point_offsets[i, j].Value.z);
        }
        // Order to create triangles from vertices in
        triangles = new int[]
        {
            0,2,1,
            0,1,3,
            1,2,4,
            2,0,5,
            0,3,5,
            1,4,3,
            2,5,4,
            3,4,5
        };
    }

    // Calculate offsets for different start points / draw points for drawing octahedra
    private void CalculateOffsets(int i, Vector3?[,] offset_array)
    {
        // Offset already exists - do nothing
        if (offset_array[i, 0] != null)
            return;
        float l = lengths[i].Value;
        // Set offsets for drawing points
        if (offset_array == point_offsets)
        {
            offset_array[i, 0] = new Vector3(
                l,
                0,
                0);
            offset_array[i, 1] = new Vector3(
                l / 2f,
                0,
                l * (Mathf.Sqrt(3f) / 2f));
            offset_array[i, 2] = new Vector3(
                l * (3f / 2f),
                0,
                l * (Mathf.Sqrt(3f) / 2f));
            offset_array[i, 3] = new Vector3(
                l / 2f,
                l * (Mathf.Sqrt(6f) / 3f),
                (l / 2) * Mathf.Tan(Mathf.PI / 6f));
            offset_array[i, 4] = new Vector3(
                l,
                l * (Mathf.Sqrt(6f) / 3f),
                (l * (Mathf.Sqrt(3f) / 2f)) + ((l / 2) * Mathf.Tan(Mathf.PI / 6f)));
            offset_array[i, 5] = new Vector3(
                l * (3f / 2f),
                l * (Mathf.Sqrt(6f) / 3f),
                (l / 2) * Mathf.Tan(Mathf.PI / 6f));
        }
        // Set offsets for starting points - 4 new octahedra being drawn, so 4 offsets
        else
        {
            offset_array[i, 0] = new Vector3(
                0,
                0,
                0);
            offset_array[i, 1] = new Vector3(
                l / 2f,
                0,
                l * (Mathf.Sqrt(3f) / 2f));
            offset_array[i, 2] = new Vector3(
                l,
                0,
                0);
            offset_array[i, 3] = new Vector3(
                l / 2f,
                l * (Mathf.Sqrt(6f) / 3f),
                (l / 2f) * Mathf.Tan(Mathf.PI / 6f));
        }
    }
}
