using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrahedronGenerator : GeneratorBase
{
    // Run when object is created
    private void Start()
    {
        DrawFractal DrawTetrahedronFractalCb = DrawTetrahedronFractal;
        BeginFractal(DrawTetrahedronFractalCb);
    }

    // Run on frame update
    private void Update() { }

    // Update object with new mesh and material
    private void CreateMesh()
    {
        // Add mesh to object
        mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        // Add diffuse material to object
        material = new Material(Shader.Find("Diffuse"));
        gameObject.AddComponent<MeshRenderer>().material = material;
    }

    // Draw tetrahedra or continue recursion
    private void DrawTetrahedronFractal(Vector3 xyz, float a, int n, int i)
    {
        // Side length at iteration i = a / 2^i
        float l = a / Mathf.Pow(2, i);
        // Only one tetrahedron to be drawn
        if (n == 1)
            DrawTetrahedron(xyz, a);
        // Draw tetrahedron at bottom iteration
        else if (n == i)
            DrawTetrahedron(xyz, l);
        // Draw mesh from calculated points
        if (n == 1 | n == i)
        {
            // Update mesh
            CreateMesh();
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();
        }
        // Stop recursion past defined iteration
        else if (i < n)
        {
            // Points to start drawing tetrahedra from
            Vector3[] points = new Vector3[]
            {
                new Vector3(xyz.x, xyz.y, xyz.z),
                new Vector3(xyz.x + (l / 2f), xyz.y, xyz.z),
                new Vector3(xyz.x + (l / 4f), xyz.y, xyz.z + (l * (Mathf.Sqrt(3f) / 4f))),
                new Vector3(xyz.x + (l / 4f), xyz.y + (l * (Mathf.Sqrt(6f) / 6f)), xyz.z + ((l / 4f) * Mathf.Tan(Mathf.PI / 6f)))
            };
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
    private void DrawTetrahedron(Vector3 xyz, float l)
    {
        // Vertices for individual tetrahedron
        vertices = new Vector3[]
        {
            // Triangle 0 - bottom vertex
            new Vector3(xyz.x, xyz.y, xyz.z),
            // Triangle 1 - bottom vertex
            new Vector3(xyz.x + l, xyz.y, xyz.z),
            // Triangle 2- bottom vertex
            new Vector3(xyz.x + (l / 2f), xyz.y, xyz.z + (l * (Mathf.Sqrt(3f) / 2f))),
            // Triangle 3 - top vertex
            new Vector3(xyz.x + (l / 2f), xyz.y + (l * (Mathf.Sqrt(6f) / 3f)), xyz.z + ((l / 2f) * Mathf.Tan(Mathf.PI / 6f)))
        };
        // Order to create triangles from vertices in
        triangles = new int[]
        {
            0,1,2,
            0,2,3,
            2,1,3,
            0,3,1
        };
    }

    // Update private variables
    public void SetData(Vector3 xyz, float a, int n, int i)
    {
        start_xyz = xyz;
        base_length = a;
        max_iterations = n;
        current_iteration = i;
    }
}
