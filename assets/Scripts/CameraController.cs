using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float scrollingSpeed = 10.0f;

    public float zoomingSpeed = 0.5f;
    public float minZoom = 0.5f;
    public float maxZoom = 10.0f;

    // Use this for initialization
    void Start () {
        m_camera = GetComponent<Camera>();
        m_gm = FindObjectOfType<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!Input.touchSupported)
        {
            if (Input.mouseScrollDelta.y != 0.0f)
            {
                m_camera.orthographicSize += Input.mouseScrollDelta.y* zoomingSpeed  * Time.deltaTime;
                m_camera.orthographicSize = Mathf.Clamp(m_camera.orthographicSize, minZoom, maxZoom);
            }
        }
        else
        {

        }
	}

    Camera m_camera;
    GameManager m_gm;
}
