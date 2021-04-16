using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    private Transform camera_transform;
    private Transform parent_transform;

    private Vector3 local_rotation;
    private float camera_distance = 50f;
    private float start_x = 0f;
    private float start_y = 0f;

    public float mouse_sensitivity = 4;
    public float scroll_sensitivity = 2f;
    public float orbit_dampening = 10f;
    public float scroll_dampening = 5f;

    // Start is called before the first frame update
    protected void Start()
    {
        camera_transform = this.transform;
        parent_transform = this.transform.parent;
        // Set initial camera position
        camera_transform.position = new Vector3(start_x, start_y, -camera_distance);
    }

    // Update is called once per frame
    private void Update() { }

    // Called once per frame after update
    protected void LateUpdate()
    {
        // Don't move camera if not dragging mouse
        if (Input.GetMouseButton(1))
        {
            // Rotate camera based on mouse coordinates
            if (Input.GetAxis("Mouse X") != 0 | Input.GetAxis("Mouse Y") != 0)
            {
                local_rotation.x += Input.GetAxis("Mouse X") * mouse_sensitivity;
                local_rotation.y -= Input.GetAxis("Mouse Y") * mouse_sensitivity;
                // Clamp y oration to not flip at poles
                local_rotation.y = Mathf.Clamp(local_rotation.y, -90f, 90f);
            }
        }
        // Zoom camera on zoom
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            float scroll_amount = Input.GetAxis("Mouse ScrollWheel") * scroll_sensitivity;
            // Increase zoom speed further from object, decrease closer to object
            scroll_amount *= (camera_distance * 0.3f);
            camera_distance += scroll_amount * -1f;
            // Constrain camera zoom
            camera_distance = Mathf.Clamp(camera_distance, 1f, 200f);
        }
        // Set camera transform
        Quaternion qt = Quaternion.Euler(local_rotation.y, local_rotation.x, 0);
        transform.parent.rotation = Quaternion.Lerp(parent_transform.rotation, qt, Time.deltaTime * orbit_dampening);
        if (camera_transform.localPosition.z != camera_distance * -1f)
        {
            camera_transform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(
                camera_transform.localPosition.z, camera_distance * -1f, Time.deltaTime * scroll_dampening));
        }
    }

    // Move camera pivot position with interpolation
    public static void MovePivot(Vector3 xyz, bool interpolate)
    {
        GameObject.Find("CameraPivot").transform.position = xyz;
    }
}
