using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HSVPicker;

public class GeneratorBase : MonoBehaviour
{
    // Mesh to display fractal
    public Mesh mesh;
    public Material material = null;
    public Color? material_color = null;
    // Store vertices and order to draw triangles between
    protected Vector3[] vertices;
    protected int[] triangles;
    // Fractal data
    public Vector3? start_xyz = null;
    public float? base_length = null;
    public int? max_iterations = null;
    public int? current_iteration = null;
    // Track completion of iterations
    protected int? max_objects = null;
    // Parent object that will display the final mesh
    protected GameObject parent_obj = null;
    // List of mesh combines to later combine into single mesh in parent
    protected List<CombineInstance> mesh_combine = null;

    // Needs to be assigned to draw fractal method
    protected delegate void DrawFractal(Vector3 xyz, float a, int n, int i);

    // Store data for each fractal
    static public List<FractalData> fractal_data = new List<FractalData>();
    protected FractalData f_data;

    // Setup the parent object
    protected void SetupParent(bool create_mesh)
    {
        // Don't create parent object if it already exists
        if (parent_obj != null)
            return;
        // Set busy flag for updating other fractals
        UI.busy = true;
        // Create new parent object
        parent_obj = gameObject;
        // Add parent object mesh
        if (create_mesh)
            CreateMesh();
        // Add button for object
        GameObject btn = UI.NewButton(gameObject.name, "ButtonGeneric", null);
        GameObject del_btn = UI.NewButton("-", "ButtonGeneric", null);
        GameObject col_btn = UI.NewButton("C", "ButtonGeneric", null);
        ColorPicker picker = UI.NewPicker("ColorPicker", null);
        // Store button data
        BtnData btn_data;
        btn_data.btn = btn;
        btn_data.del_btn = del_btn;
        btn_data.col_btn = col_btn;
        // Store fractal data
        f_data.name = gameObject.name;
        f_data.btn_data = btn_data;
        f_data.fractal = gameObject;
        f_data.type = this.GetType();
        f_data.picker = picker;
        f_data.ColorSetter = SetColor;
        f_data.MaterialGetter = GetMaterial;
        f_data.TransformGetter = GetTransform;
        f_data.TransformSetter = SetTransform;
        f_data.IterationsGetter = GetIterations;
        f_data.LengthGetter = GetLength;
    }

    // Run when object is created
    protected void BeginFractal(DrawFractal DrawFractalCb)
    {
        Vector3 xyz = start_xyz.HasValue ? start_xyz.Value : new Vector3(0, 0, 0);
        start_xyz = xyz;
        float a = base_length.HasValue ? base_length.Value : UI.length;
        base_length = a;
        // TODO max supported vertices 4,294,967,295 prevent anything higher - should be 15 actual iterations for tetrahedron
        int n = max_iterations.HasValue ? max_iterations.Value : UI.iterations;
        max_iterations = n;
        int i = current_iteration.HasValue ? current_iteration.Value : 0;
        current_iteration = i;
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
    protected void UpdateMesh<T>(ref int obj_count, int n, bool destroy) where T : GeneratorBase
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
        if (obj_count == max_objects.Value & n != 0)
            // Get parent object to combine meshes and set as its own
            parent_obj.GetComponent<T>().CombineMeshes();
        // Cleanup and redraw UI
        if (obj_count == max_objects.Value | n == 0)
        {
            // Cleanup
            Cleanup();
            // Uncheck busy flag
            UI.busy = false;
            // Update UI
            UI.DrawUI(fractal_data);
        }
        // Remove game object, skip for n = 0
        if (n != 0 & destroy)
            Destroy(gameObject);
    }

    // Update state variables
    public void SetData(Material mat, int? max_obj, GameObject parent, List<CombineInstance> combine, Vector3 xyz, float a, int n, int i)
    {
        material = mat;
        max_objects = max_obj;
        parent_obj = parent;
        mesh_combine = combine;
        start_xyz = xyz;
        base_length = a;
        max_iterations = n;
        current_iteration = i;
    }

    // Combine child meshes into parent mesh
    protected void CombineMeshes()
    {
        Mesh combine_mesh = new Mesh();
        // For any iterations with more than 65535 vertices, the standard UInt16 format does not suffice
        combine_mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        // Combine all meshes into new combined mesh
        combine_mesh.CombineMeshes(mesh_combine.ToArray());
        // Clear existing mesh
        MeshFilter mesh_filter = gameObject.GetComponent<MeshFilter>();
        mesh_filter.mesh.Clear();
        // Set parent mesh to new combined mesh
        mesh_filter.mesh = combine_mesh;
        // Optimize mesh
        mesh_filter.mesh.RecalculateNormals();
        mesh_filter.mesh.RecalculateBounds();
        mesh_filter.mesh.Optimize();
    }

    // Continue to next iteration
    protected void NextIteration<T>(Vector3 xyz, Vector3?[,] start_offsets, float a, int n, int i, int new_objects) where T : GeneratorBase
    {
        // Points to start drawing objects from
        Vector3[] points = AddOffsets(xyz, start_offsets, i, new_objects);
        // Create smaller objects that line up
        for (int j = 0; j < new_objects; j++)
        {
            // Create new game object and set data
            new GameObject(gameObject.name + "Child")
            .AddComponent<T>()
            .SetData(
                material,
                max_objects,
                parent_obj,
                mesh_combine,
                new Vector3(points[j].x, points[j].y, points[j].z),
                a, n, i + 1);
        }
    }

    // Add offsets to all elements of Vector3 array
    protected Vector3[] AddOffsets(Vector3 xyz, Vector3?[,] offset_array, int i, int num_points)
    {
        Vector3[] points = new Vector3[num_points];
        for (int j = 0; j < num_points; j++)
        {
            points[j] = new Vector3(
                xyz.x + offset_array[i, j].Value.x,
                xyz.y + offset_array[i, j].Value.y,
                xyz.z + offset_array[i, j].Value.z);
        }
        return points;
    }

    // Calculate and add fractal midpoint to fractal data
    protected void AddMidpoint(Vector3 xyz, int i, int n_points, Action<int, Vector3?[,]> CalculateOffsets)
    {
        // Only run on 0th iteration
        if (i != 0)
            return;
        // Points for midpoint calculation
        Vector3?[,] offsets = new Vector3?[1, n_points];
        CalculateOffsets(0, offsets);
        Vector3[] points = AddOffsets(xyz, offsets, 0, n_points);
        // Calculate midpoint
        Vector3 midpoint = new Vector3(0, 0, 0);
        for (int j = 0; j < points.Length; j++)
            midpoint += points[j];
        midpoint /= n_points;
        f_data.midpoint = midpoint;
        // Add data to shared list
        fractal_data.Add(f_data);
    }

    // Externally modify fractal material color
    public void SetColor(Color color)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = color;
    }

    // Externally get fractal material
    public Material GetMaterial()
    {
        return gameObject.GetComponent<MeshRenderer>().material;
    }

    // Externally get fractal transform
    public Vector3 GetTransform()
    {
        return gameObject.transform.position;
    }

    // Externally modify fractal transform
    public void SetTransform(Vector3 xyz)
    {
        gameObject.transform.position = new Vector3(xyz.x, xyz.y, xyz.z);
    }

    // Externally get fractal iterations
    public int GetIterations()
    {
        return max_iterations.Value;
    }

    // Externally get fractal length
    public float GetLength()
    {
        return base_length.Value;
    }

    // Should be overwritten for any specific cleanup procedure needed after mesh combining complete
    protected virtual void Cleanup() { }
}
