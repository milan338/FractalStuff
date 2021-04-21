// This file is part of FractalStuff, an app to visualise fractals.
// Copyright (C) 2021 milan338.
//
// FractalStuff is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FractalStuff is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FractalStuff.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InversePyramidGenerator : GeneratorBase
{
    // Cache offsets to reduce calculations
    static protected Vector3?[,] start_offsets = null;
    static protected Vector3?[,] point_offsets = null;
    static protected float?[] lengths = null;
    // Track completion of iterations
    static protected int obj_count = 0;

    // Run when object is created
    protected void Start()
    {
        // Setup parent object
        SetupParent(false);
        // Begin drawing fractal
        DrawFractal DrawTridecahedronFractalCb = DrawTridecahedronFractal;
        BeginFractal(DrawTridecahedronFractalCb);
    }

    // Run on frame update
    private void Update() { }

    // Draw tridecahedra or continue recursion
    private void DrawTridecahedronFractal(Vector3 xyz, float a, int n, int i)
    {
        // Calculate the total number of tridecahedra to draw
        if (!max_objects.HasValue)
            // Number of tridecahedra at iteration n = (5^(n+1) - 1) / 4
            max_objects = (int)((Mathf.Pow(5f, n + 1) - 1f) / 4f);
        // Update cache arrays
        if (start_offsets == null || point_offsets == null || lengths == null || mesh_combine == null)
        {
            start_offsets = new Vector3?[n + 1, 5];
            point_offsets = new Vector3?[n + 1, 9];
            lengths = new float?[n + 1];
            mesh_combine = new List<CombineInstance>();
        }
        // Update cached length
        if (!lengths[i].HasValue)
            // Side length at iteration i = a / 2^i
            lengths[i] = a / Mathf.Pow(2f, i);
        float l = lengths[i].Value;
        // Calculate midpoint
        AddMidpoint(xyz, i, 5, CalculateOffsets, 2f);
        // Only one tridecahedron to be drawn
        if (n == 0)
            DrawTridecahedron(xyz, a, n);
        else
            DrawTridecahedron(xyz, l, i);
        // Draw mesh from calculated points
        CreateMesh();
        bool destroy = i == 0 ? false : true;
        UpdateMesh<InversePyramidGenerator>(ref obj_count, n, destroy);
        // Stop recursion past defined iteration
        if (i < n)
        {
            // Calculate offsets
            CalculateOffsets(i, start_offsets);
            // Continue to next iteration
            NextIteration<InversePyramidGenerator>(xyz, start_offsets, a, n, i, 5);
        }
    }

    // Draw single tridecahedron
    private void DrawTridecahedron(Vector3 xyz, float l, int i)
    {
        // Calculate offsets
        CalculateOffsets(i, point_offsets);
        // Vertices for individual tridecahedra
        vertices = AddOffsets(xyz, point_offsets, i, 9);
        // Order to create triangles from vertices in
        triangles = new int[]
        {
            0,1,5,
            1,2,5,
            2,1,6,
            1,3,6,
            3,1,7,
            1,4,7,
            4,1,8,
            1,0,8,

            0,5,8,
            2,6,5,
            3,7,6,
            4,8,7,

            5,6,8,
            6,7,8
        };
    }

    // Calculate offsets for different start points / draw points for drawing tridecahedra
    private void CalculateOffsets(int i, Vector3?[,] offset_array, float s = 1f)
    {
        // Offset already exists - do nothing
        if (offset_array[i, 0] != null)
            return;
        float l = lengths[i].Value * s;
        // Set offsets for drawing points
        if (offset_array == point_offsets)
        {
            offset_array[i, 0] = new Vector3(l / 2f, 0, 0);
            offset_array[i, 1] = new Vector3(l / 2f, 0, l / 2f);
            offset_array[i, 2] = new Vector3(0, 0, l / 2f);
            offset_array[i, 3] = new Vector3(l / 2f, 0, l);
            offset_array[i, 4] = new Vector3(l, 0, l / 2f);
            offset_array[i, 5] = new Vector3(l / 4f, l * (Mathf.Sqrt(2f) / 4f), l / 4f);
            offset_array[i, 6] = new Vector3(l / 4f, l * (Mathf.Sqrt(2f) / 4f), l * (3f / 4f));
            offset_array[i, 7] = new Vector3(l * (3f / 4f), l * (Mathf.Sqrt(2f) / 4f), l * (3f / 4f));
            offset_array[i, 8] = new Vector3(l * (3f / 4f), l * (Mathf.Sqrt(2f) / 4f), l / 4f);
        }
        // Set offsets for starting points - 5 new tridecahedra being drawn, so 5 offsets
        else
        {
            offset_array[i, 0] = new Vector3(0, 0, 0);
            offset_array[i, 1] = new Vector3(0, 0, l / 2f);
            offset_array[i, 2] = new Vector3(l / 2f, 0, l / 2f);
            offset_array[i, 3] = new Vector3(l / 2f, 0, 0);
            offset_array[i, 4] = new Vector3(l / 4f, l * (Mathf.Sqrt(2f) / 4f), l / 4f);
        }
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
