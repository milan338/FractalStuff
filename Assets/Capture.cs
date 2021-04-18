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
