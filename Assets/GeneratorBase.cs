using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HSVPicker;

public class GeneratorBase : MonoBehaviour
{
    // Mesh to display fractal
    public Mesh mesh;
    public Material material = null;
    // Store vertices and order to draw triangles between
    protected Vector3[] vertices;
    protected int[] triangles;
    // Fractal data
    protected Vector3? start_xyz = null;
    protected float? base_length = null;
    protected int? max_iterations = null;
    protected int? current_iteration = null;
    // Track completion of iterations
    protected int? max_objects = null;
    // Parent object that will display the final mesh
    protected GameObject parent_obj = null;
    // List of mesh combines to later combine into single mesh in parent
    protected List<CombineInstance> mesh_combine = null;

    // Needs to be assigned to draw fractal method
    protected delegate void DrawFractal(Vector3 xyz, float a, int n, int i);
    // Used to externally update mesh colors
    public delegate void ColorSetter(Color color);

    // Store data for each fractal
    static protected List<FractalData> fractal_data = new List<FractalData>();

    // Setup the parent object
    protected void SetupParent(bool create_mesh)
    {
        // Don't create parent object if it already exists
        if (parent_obj != null)
            return;
        // Create new parent object
        parent_obj = gameObject;
        // Add parent object mesh
        if (create_mesh)
            CreateMesh();
        // Add button for object
        GameObject btn = UI.NewButton(
            gameObject.name,
            "ButtonGeneric",
            () => Debug.Log(gameObject.name));
        GameObject del_btn = UI.NewButton(
            "-",
            "ButtonGeneric",
            null);
        GameObject col_btn = UI.NewButton(
            "C",
            "ButtonGeneric",
            null);
        ColorPicker picker = UI.NewPicker(
            "ColorPicker",
            null);
        // Store button data
        BtnData btn_data;
        btn_data.btn = btn;
        btn_data.del_btn = del_btn;
        btn_data.col_btn = col_btn;
        btn_data.xyz = new Vector3(0, 0, 0);
        btn_data.text = gameObject.name;
        // Store fractal data
        FractalData f_data;
        f_data.name = gameObject.name;
        f_data.btn_data = btn_data;
        f_data.fractal = gameObject;
        f_data.picker = picker;
        f_data.color = Color.white;
        f_data.ColorSetter = SetColor;
        // Add data to shared list
        fractal_data.Add(f_data);
        // Update UI with buttons
        UI.DrawButtons(fractal_data);
    }

    // Run when object is created
    protected void BeginFractal(DrawFractal DrawFractalCb)
    {
        // Get start coordinates
        Vector3 xyz = start_xyz.HasValue ? start_xyz.Value : new Vector3(0, 0, 0);
        // Get original tetrahedron length
        float a = base_length.HasValue ? base_length.Value : 50;
        // Get total number of iterations to run (starting at 0)
        // TODO max supported vertices 4,294,967,295 prevent anything higher - should be 15 actual iterations for tetrahedron
        int n = max_iterations.HasValue ? max_iterations.Value : 5;
        // Get the current iteration of the object
        int i = current_iteration.HasValue ? current_iteration.Value : 0;
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
        // Remove game object, skip for n = 0
        if (n != 0 & destroy)
            Destroy(gameObject);
    }

    // Update state variables
    protected void SetData(Material mat, int? max_obj, GameObject parent, List<CombineInstance> combine, Vector3 xyz, float a, int n, int i)
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
        // Cleanup
        Cleanup();
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

    // Externally modify fractal material color
    public void SetColor(Color color)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = color;
    }

    // Should be overwritten for any specific cleanup procedure needed after mesh combining complete
    protected virtual void Cleanup() { }
}
