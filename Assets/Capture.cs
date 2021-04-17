using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;

public class Capture : MonoBehaviour
{
    // Capture screenshot without UI
    private IEnumerator CaptureScreen(string path)
    {
        // UI components to hid
        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        Canvas canvas_3d = GameObject.Find("Canvas3D").GetComponent<Canvas>();
        GameObject perspective_cube = GameObject.Find("PerspectiveCube");
        // Wait before screen rendering to hide UI
        yield return null;
        // Hide UI
        canvas.enabled = false;
        canvas_3d.enabled = false;
        perspective_cube.SetActive(false);
        // Wait for screen rendering to finish
        yield return new WaitForEndOfFrame();
        // Take screenshot
        ScreenCapture.CaptureScreenshot(path);
        // Re-enable UI
        canvas.enabled = true;
        canvas_3d.enabled = true;
        perspective_cube.SetActive(true);
    }

    // Open native file explorer prompt to select export location
    public void OpenExplorer()
    {
        StandaloneFileBrowser.SaveFilePanelAsync("Save Image", "", "fractals_export", "png", SaveCapture);
    }

    // Callback to save file based on user-selection location
    private void SaveCapture(string path)
    {
        StartCoroutine(CaptureScreen(path));
    }
}
