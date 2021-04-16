using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using HSVPicker;

public struct BtnData
{
    public GameObject btn;
    public GameObject del_btn;
    public GameObject col_btn;
}

public struct FractalData
{
    public string name;
    public BtnData btn_data;
    public GameObject fractal;
    public Type type;
    public ColorPicker picker;
    public Color color;
    public Action<Color> ColorSetter;
    public Func<Material> MaterialGetter;
    public Func<Vector3> TransformGetter;
    public Action<Vector3> TransformSetter;
    public Func<int> IterationsGetter;
    public Func<float> LengthGetter;
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
    // Store currently selected fractal
    public static FractalData current_fractal;
    // Store global fractal data
    public static int iterations;
    public static float length;
    // Prevent more than one color picker on screen at once
    private static bool picker_visible = false;
    private static ColorPicker current_picker = null;
    // Sliders
    public static GameObject slider_a;
    public static GameObject slider_n;
    public static GameObject slider_x;
    public static GameObject slider_y;
    public static GameObject slider_z;
    // Pause updates while other fractal is updating
    public static bool busy;

    // Init UI
    public static void InitUI()
    {
        // Update an existing fractal
        Action<float, int> UpdateCb = (float a, int n) =>
        {
            try
            {
                // Wait for any updating fractals to prevent altering the wrong fractal
                if (busy)
                    return;
                // Get current fractal position
                Vector3 transform = current_fractal.TransformGetter();
                // Update static vars
                iterations = n;
                length = a;
                // Create new fractal game object
                GameObject fractal = new GameObject(current_fractal.name);
                fractal.SetActive(false);
                // Add fractal component to object through reflection
                dynamic obj = typeof(GameObject)
                .GetMethod(nameof(GameObject.AddComponent), new Type[0])
                .MakeGenericMethod(current_fractal.type)
                .Invoke(fractal, new object[0]);
                // Set position of fractal
                // TODO inverse generators have their first iteration moved away for some reason
                obj.SetTransform(transform);
                fractal.SetActive(true);
                // Remove existing fractal
                current_fractal.btn_data.del_btn.GetComponent<Button>().onClick.Invoke();
                // Update current fractal
                current_fractal = GeneratorBase.fractal_data[GeneratorBase.fractal_data.Count];
            }
            catch { }
        };
        // Default values
        iterations = 2;
        length = 50f;
        // Draw sliders
        slider_a = UI.NewSlider("Length: ", "SliderGeneric", null);
        slider_n = UI.NewSlider("Iterations: ", "SliderGeneric", null);
        slider_x = UI.NewSlider("X: ", "SliderGeneric", null);
        slider_y = UI.NewSlider("Y: ", "SliderGeneric", null);
        slider_z = UI.NewSlider("Z: ", "SliderGeneric", null);
        slider_a.GetComponent<Slider>().transform.position = new Vector3(
            80f, 140f, 0);
        slider_a.GetComponent<Slider>().wholeNumbers = true;
        slider_a.GetComponent<Slider>().maxValue = 100f;
        slider_n.GetComponent<Slider>().transform.position = new Vector3(
            80f, 110f, 0);
        slider_n.GetComponent<Slider>().wholeNumbers = true;
        slider_n.GetComponent<Slider>().maxValue = 15f;
        slider_x.GetComponent<Slider>().transform.position = new Vector3(
            80f, 80f, 0);
        slider_x.GetComponent<Slider>().wholeNumbers = true;
        slider_x.GetComponent<Slider>().maxValue = 100f;
        slider_y.GetComponent<Slider>().transform.position = new Vector3(
            80f, 50f, 0);
        slider_y.GetComponent<Slider>().wholeNumbers = true;
        slider_y.GetComponent<Slider>().maxValue = 100f;
        slider_z.GetComponent<Slider>().transform.position = new Vector3(
            80f, 20f, 0);
        slider_z.GetComponent<Slider>().wholeNumbers = true;
        slider_z.GetComponent<Slider>().maxValue = 100f;
        slider_a.GetComponent<Slider>().onValueChanged.AddListener((float a) =>
            {
                UpdateCb(a, current_fractal.IterationsGetter());
                slider_a.GetComponentInChildren<Text>().text = "Length: " + length;
            });
        slider_n.GetComponent<Slider>().onValueChanged.AddListener((float n) =>
            {
                UpdateCb(current_fractal.LengthGetter(), (int)n);
                slider_n.GetComponentInChildren<Text>().text = "Iterations: " + iterations;
            });
        slider_x.GetComponent<Slider>().onValueChanged.AddListener((float x) =>
        {
            try
            {
                Vector3 xyz = current_fractal.TransformGetter();
                xyz.x = x;
                current_fractal.TransformSetter(xyz);
                slider_x.GetComponentInChildren<Text>().text = "x: " + x;
            }
            catch { }
        });
        slider_y.GetComponent<Slider>().onValueChanged.AddListener((float y) =>
        {
            try
            {
                Vector3 xyz = current_fractal.TransformGetter();
                xyz.y = y;
                current_fractal.TransformSetter(xyz);
                slider_y.GetComponentInChildren<Text>().text = "y: " + y;
            }
            catch { }
        });
        slider_z.GetComponent<Slider>().onValueChanged.AddListener((float z) =>
        {
            try
            {
                Vector3 xyz = current_fractal.TransformGetter();
                xyz.z = z;
                current_fractal.TransformSetter(xyz);
                slider_z.GetComponentInChildren<Text>().text = "z: " + z;
            }
            catch { }
        });
    }

    // Create new button
    public static GameObject NewButton(string text, string style, UnityAction call)
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
    public static GameObject NewDropdown(string text, string style, UnityAction<int> call)
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

    // Create new slider
    public static GameObject NewSlider(string text, string style, UnityAction<float> call)
    {
        // Get slider prefab
        GameObject slider_prefab = Resources.Load<GameObject>("prefabs/" + style);
        // Make new slider using prefab
        GameObject slider = Instantiate(slider_prefab) as GameObject;
        // Set slider text
        slider.GetComponentInChildren<Text>().text = text;
        // Set slider action
        if (call != null)
            slider.GetComponent<Slider>().onValueChanged.AddListener(call);
        // Add slider to UI canvas
        slider.transform.SetParent(GameObject.Find("Canvas").transform);
        // Set slider as active
        slider.SetActive(true);
        return slider;
    }

    // Create new color picker
    public static ColorPicker NewPicker(string style, UnityAction<Color> call)
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
        picker.gameObject.SetActive(false);
        return picker;
    }

    // Draw UI on screen
    public static void DrawUI(List<FractalData> fractal_data)
    {
        // Add all buttons from list
        int i = 0;
        if (fractal_data != null)
        {
            foreach (FractalData f_data in fractal_data)
            {
                current_fractal = f_data;
                // Update sliders
                UnityAction update_sliders = () =>
                {
                    Vector3 xyz = f_data.TransformGetter();
                    float a = f_data.LengthGetter();
                    int n = f_data.IterationsGetter();
                    slider_a.GetComponent<Slider>().SetValueWithoutNotify(a);
                    slider_n.GetComponent<Slider>().SetValueWithoutNotify(n);
                    slider_x.GetComponent<Slider>().SetValueWithoutNotify(xyz.x);
                    slider_y.GetComponent<Slider>().SetValueWithoutNotify(xyz.y);
                    slider_z.GetComponent<Slider>().SetValueWithoutNotify(xyz.z);
                    slider_a.GetComponentInChildren<Text>().text = "Length: " + a;
                    slider_n.GetComponentInChildren<Text>().text = "Iterations: " + n;
                    slider_x.GetComponentInChildren<Text>().text = "x: " + xyz.x;
                    slider_y.GetComponentInChildren<Text>().text = "y: " + xyz.y;
                    slider_z.GetComponentInChildren<Text>().text = "z: " + xyz.z;
                };
                update_sliders();
                float btn_y = (Screen.height - 15f) - ((f_data.btn_data.btn.GetComponent<RectTransform>().sizeDelta.y + 5f) * i);
                // Fractal selection button
                f_data.btn_data.btn.GetComponent<Button>().onClick.RemoveAllListeners();
                f_data.btn_data.btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    current_fractal = f_data;
                    update_sliders();
                });
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
                // Color picker
                f_data.picker.onValueChanged.RemoveAllListeners();
                f_data.picker.onValueChanged.AddListener((Color color) => f_data.ColorSetter(color));
                f_data.picker.AssignColor(Color.white);
                f_data.btn_data.col_btn.GetComponent<RectTransform>().sizeDelta = new Vector3(
                    25f, f_data.btn_data.col_btn.GetComponent<RectTransform>().sizeDelta.y, 0);
                f_data.btn_data.col_btn.GetComponent<Button>().transform.position = new Vector3(
                    10f + f_data.btn_data.btn.GetComponent<RectTransform>().sizeDelta.x, btn_y, 0);
                // Fractal delete button
                f_data.btn_data.del_btn.GetComponent<Button>().onClick.RemoveAllListeners();
                f_data.btn_data.del_btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Destroy(f_data.btn_data.del_btn);
                    Destroy(f_data.btn_data.btn);
                    Destroy(f_data.btn_data.col_btn);
                    if (current_picker == f_data.picker)
                    {
                        current_picker = null;
                        picker_visible = false;
                    }
                    Destroy(f_data.picker.gameObject);
                    Destroy(f_data.fractal);
                    fractal_data.Remove(f_data);
                    // Redraw buttons
                    DrawUI(fractal_data);
                });
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
