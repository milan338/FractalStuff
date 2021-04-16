using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    // Store all available fractal types
    private List<Type> fractals = new List<Type>();

    // Start is called before the first frame update
    void Start()
    {
        // Fractal types
        fractals.Add(typeof(TetrahedronGenerator));
        fractals.Add(typeof(InverseTetrahedronGenerator));
        fractals.Add(typeof(PyramidGenerator));
        fractals.Add(typeof(InversePyramidGenerator));
        // Init UI
        UI.InitUI();
        // Create new dropdown menu
        GameObject dd = UI.NewDropdown(
            "Select Fractal",
            "DropdownGeneric",
            (int i) =>
            {
                // Ignore first text option
                if (i == 0)
                    return;
                i -= 1;
                // Reset UI defaults
                UI.ResetDefaults();
                // Add new fractal game object through reflection
                typeof(GameObject)
                .GetMethod(nameof(GameObject.AddComponent), new Type[0])
                .MakeGenericMethod(fractals[i])
                .Invoke(new GameObject(fractals[i].Name), new object[0]);
                // Set selection to first text option
                UI.dropdown.GetComponent<Dropdown>().value = 0;
            });
        // Add dropdown items
        foreach (Type fractal in fractals)
            dd.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(fractal.Name));
        // Set global dropdown
        UI.dropdown = dd;
        UI.DrawUI(null);
    }

    // Update is called once per frame
    void Update() { }

    // Load main class on program load
    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeMethodLoad()
    {
        new GameObject("MainHandler")
        .AddComponent<Main>();
    }
}
