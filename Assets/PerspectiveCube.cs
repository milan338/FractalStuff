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
        transform.localRotation = Pivot.transform.localRotation;
    }
}
