using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrahedronGenerator : MonoBehaviour
{
    public Mesh mesh;
    public Material material;

    private Vector3[] vertices;
    private int[] triangles;

    private float? start_x = null;
    private float? start_y = null;
    private float? start_z = null;
    private float? base_length = null;
    private int? max_iterations = null;
    private int? current_iteration = null;

    // Run when object is created
    private void Start()
    {
        // Get start coordinates
        float x = start_x.HasValue ? start_x.Value : 0;
        float y = start_y.HasValue ? start_y.Value : 0;
        float z = start_z.HasValue ? start_z.Value : 0;
        // Get original tetrahedron length
        float a = base_length.HasValue ? base_length.Value : 50;
        // Get total number of iterations to run
        int n = max_iterations.HasValue ? max_iterations.Value : 5;
        // Get the current iteration of the object
        int i = current_iteration.HasValue ? current_iteration.Value : 1;
        // Draw remainder of fractal
        DrawTetrahedronFractal(x, y, z, a, n, i);
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
    private void DrawTetrahedronFractal(float x, float y, float z, float a, int n, int i)
    {
        // Side length at iteration i = a / 2^i
        float l = a / Mathf.Pow(2, i);
        // Only one tetrahedron to be drawn
        if (n == 1)
            DrawTetrahedron(x, y, z, a);
        // Draw tetrahedron at bottom iteration
        else if (n == i)
            DrawTetrahedron(x, y, z, l);
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
                new Vector3(x, y, z),
                new Vector3(x + (l / 2f), y, z),
                new Vector3(x + (l / 4f), y, z + (l * (Mathf.Sqrt(3f) / 4f))),
                new Vector3(x + (l / 4f), y + (l * (Mathf.Sqrt(6f) / 6f)), z + ((l / 4f) * Mathf.Tan(Mathf.PI / 6f)))
            };
            // Continue to next iteration
            for (int j = 0; j < 4; j++)
            {
                // Create 4 smaller tetrahedra that line up
                new GameObject("TetrahedronGeneratorChild")
                .AddComponent<TetrahedronGenerator>()
                .SetData(points[j].x, points[j].y, points[j].z, a, n, i + 1);
            }
            // Don't destroy root object
            if (i != 1)
                // Destroy current game object
                Destroy(gameObject);
        }
    }

    // Draw single tetrahedron
    private void DrawTetrahedron(float x, float y, float z, float l)
    {
        // Vertices for individual tetrahedron
        vertices = new Vector3[]
        {
            // Triangle 0 - bottom vertex
            new Vector3(x, y, z),
            // Triangle 1 - bottom vertex
            new Vector3(x + l, y, z),
            // Triangle 2- bottom vertex
            new Vector3(x + (l / 2f), y, z + (l * (Mathf.Sqrt(3f) / 2f))),
            // Triangle 3 - top vertex
            new Vector3(x + (l / 2f), y + (l * (Mathf.Sqrt(6f) / 3f)), z + ((l / 2f) * Mathf.Tan(Mathf.PI / 6f)))
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
    public void SetData(float x, float y, float z, float a, int n, int i)
    {
        start_x = x;
        start_y = y;
        start_z = z;
        base_length = a;
        max_iterations = n;
        current_iteration = i;
    }
}
