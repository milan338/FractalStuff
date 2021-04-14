using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HSVPicker;

public struct BtnData
{
    public GameObject btn;
    public GameObject del_btn;
    public GameObject col_btn;
    public Vector3 xyz;
    public string text;
}

public struct FractalData
{
    public string name;
    public BtnData btn_data;
    public GameObject fractal;
    public ColorPicker picker;
    public Color color;
    public GeneratorBase.ColorSetter ColorSetter;
}

public class UI : ScriptableObject
{
    // Store main dropdown
    public static GameObject _dropdown;
    public static GameObject dropdown
    {
        get => _dropdown;
        set => _dropdown = value;
    }

    // Prevent more than one color picker on screen at once
    private static bool picker_visible = false;
    private static ColorPicker current_picker = null;

    // Create new button
    public static GameObject NewButton(string text, string style, UnityEngine.Events.UnityAction call)
    {
        // Get button prefab
        GameObject button_prefab = Resources.Load<GameObject>("prefabs/" + style);
        // Make new button using prefab
        GameObject btn = Instantiate(button_prefab) as GameObject;
        // Set button text
        btn.GetComponentInChildren<Text>().text = text;
        // Set button action
        if (call != null)
            btn.GetComponent<Button>().onClick.AddListener(call);
        // Add button to UI canvas
        btn.transform.SetParent(GameObject.Find("Canvas").transform);
        // Set button as active
        btn.SetActive(true);
        return btn;
    }

    // Create new dropdown
    public static GameObject NewDropdown(string text, string style, UnityEngine.Events.UnityAction<int> call)
    {
        // Get dropdown prefab
        GameObject dropdown_prefab = Resources.Load<GameObject>("prefabs/" + style);
        // Make new dropdown using prefab
        GameObject dd = Instantiate(dropdown_prefab) as GameObject;
        // Set dropdown text
        dd.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(text));
        // Set dropdown action
        if (call != null)
            dd.GetComponent<Dropdown>().onValueChanged.AddListener(call);
        // Add dropdown to UI canvas
        dd.transform.SetParent(GameObject.Find("Canvas").transform);
        // Set dropdown as active
        dd.SetActive(true);
        return dd;
    }

    // Create new color picker
    public static ColorPicker NewPicker(string style, UnityEngine.Events.UnityAction<Color> call)
    {
        // Get picker prefab
        ColorPicker picker_prefab = Resources.Load<ColorPicker>("prefabs/" + style);
        // Make new picker using prefab
        ColorPicker picker = Instantiate(picker_prefab) as ColorPicker;
        // Set picker action
        if (call != null)
            picker.onValueChanged.AddListener(call);
        // Add picker to UI canvas
        picker.transform.SetParent(GameObject.Find("Canvas").transform);
        // Set picker position
        picker.transform.position = new Vector3(0, 340f, 0);
        // Set picker as inactive
        // picker.enabled = false;
        picker.gameObject.SetActive(false);
        return picker;
    }

    // Draw buttons on screen
    public static void DrawButtons(List<FractalData> fractal_data)
    {
        // Add all buttons from list
        int i = 0;
        if (fractal_data != null)
        {
            foreach (FractalData f_data in fractal_data)
            {
                float btn_y = (Screen.height - 15f) - ((f_data.btn_data.btn.GetComponent<RectTransform>().sizeDelta.y + 5f) * i);
                // Fractal selection button
                f_data.btn_data.btn.transform.position = new Vector3(
                    80f, btn_y, 0);
                // Color picker button
                f_data.btn_data.col_btn.GetComponent<Button>().onClick.RemoveAllListeners();
                f_data.btn_data.col_btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    // Toggle existing pickers
                    if (picker_visible & current_picker != f_data.picker)
                        current_picker.gameObject.SetActive(false);
                    // Show new picker
                    f_data.picker.gameObject.SetActive(!f_data.picker.gameObject.activeSelf);
                    picker_visible = f_data.picker.gameObject.activeSelf;
                    current_picker = f_data.picker;
                });
                f_data.picker.onValueChanged.RemoveAllListeners();
                f_data.picker.onValueChanged.AddListener((Color color) => f_data.ColorSetter(color));
                f_data.picker.AssignColor(Color.white);
                // Set picker button translations
                f_data.btn_data.col_btn.GetComponent<RectTransform>().sizeDelta = new Vector3(
                    25f, f_data.btn_data.col_btn.GetComponent<RectTransform>().sizeDelta.y, 0);
                f_data.btn_data.col_btn.GetComponent<Button>().transform.position = new Vector3(
                    10f + f_data.btn_data.btn.GetComponent<RectTransform>().sizeDelta.x, btn_y, 0);
                // Fractal delete button
                f_data.btn_data.del_btn.GetComponent<Button>().onClick.RemoveAllListeners();
                f_data.btn_data.del_btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    // Delete 'delete' button
                    Destroy(f_data.btn_data.del_btn);
                    // Delete fractal button
                    Destroy(f_data.btn_data.btn);
                    // Delete color picker button
                    Destroy(f_data.btn_data.col_btn);
                    // Delete color picker
                    if (current_picker == f_data.picker)
                    {
                        current_picker = null;
                        picker_visible = false;
                    }
                    Destroy(f_data.picker.gameObject);
                    // Delete fractal
                    Destroy(f_data.fractal);
                    // Remove fractal data
                    fractal_data.Remove(f_data);
                    // Redraw buttons
                    DrawButtons(fractal_data);
                });
                // Set delete button translations
                f_data.btn_data.del_btn.GetComponent<RectTransform>().sizeDelta = new Vector3(
                    25f, f_data.btn_data.del_btn.GetComponent<RectTransform>().sizeDelta.y, 0);
                f_data.btn_data.del_btn.GetComponent<Button>().transform.position = new Vector3(
                    33f + f_data.btn_data.btn.GetComponent<RectTransform>().sizeDelta.x, btn_y, 0);
                i++;
            }
        }
        // Add dropdown
        _dropdown.transform.position = new Vector3(
            80f, (Screen.height - 15f) - ((_dropdown.GetComponent<RectTransform>().sizeDelta.y + 5f) * i), 0);
    }
}
