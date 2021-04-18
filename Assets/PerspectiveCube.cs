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

public class PerspectiveCube : MonoBehaviour
{
    private static GameObject Pivot;

    // Start is called before the first frame update
    void Start()
    {
        Pivot = GameObject.Find("CameraPivot");
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Inverse(Pivot.transform.localRotation);
    }
}
