using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct BtnData
{
    public GameObject btn;
    public GameObject del_btn;
    public Vector3 xyz;
    public string text;
}

public struct FractalData
{
    public string name;
    public BtnData btn_data;
    public GameObject fractal;
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

    // Draw buttons on screen
    public static void DrawButtons(List<FractalData> fractal_data)
    {
        // Add all buttons from list
        int i = 0;
        if (fractal_data != null)
        {
            foreach (FractalData f_data in fractal_data)
            {
                // Fractal selection button
                f_data.btn_data.btn.transform.position = new Vector3(
                    80f,
                    (Screen.height - 15f) - ((f_data.btn_data.btn.GetComponent<RectTransform>().sizeDelta.y + 5f) * i),
                    0);
                // Fractal delete button
                f_data.btn_data.del_btn.GetComponent<Button>().onClick.RemoveAllListeners();
                f_data.btn_data.del_btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    // Delete 'delete' button
                    Destroy(f_data.btn_data.del_btn);
                    // Delete fractal button
                    Destroy(f_data.btn_data.btn);
                    // Delete fractal
                    Destroy(f_data.fractal);
                    // Remove fractal data
                    fractal_data.Remove(f_data);
                    // Redraw buttons
                    DrawButtons(fractal_data);
                    Debug.Log("clicked");
                });
                f_data.btn_data.del_btn.GetComponent<Button>().transform.position = new Vector3(
                    80f + f_data.btn_data.btn.GetComponent<RectTransform>().sizeDelta.x + 5f,
                    (Screen.height - 15f) - ((f_data.btn_data.btn.GetComponent<RectTransform>().sizeDelta.y + 5f) * i),
                    0);
                i++;
            }
        }
        // Add dropdown
        _dropdown.transform.position = new Vector3(
            80f,
            (Screen.height - 15f) - ((_dropdown.GetComponent<RectTransform>().sizeDelta.y + 5f) * i),
            0);
    }
}
