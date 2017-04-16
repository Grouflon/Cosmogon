using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float scrollingEdgeSize = 50.0f;
    public float scrollingSpeed = 10.0f;
    public float playingZoneExtraBorder = 5.0f;

    public float zoomingSpeed = 0.5f;
    public float minZoom = 0.5f;
    public float maxZoom = 10.0f;

    void Start ()
    {
        m_camera = GetComponent<Camera>();
        m_gm = FindObjectOfType<GameManager>();
	}
	
	void Update ()
    {
        Vector2 screenSize = new Vector2(m_camera.pixelWidth, m_camera.pixelHeight);

        // COMPUTE PLAYING ZONE
        Rect playingZone;
        if (m_gm.GetPlanets().Count == 0)
            playingZone = new Rect(m_camera.transform.position.x, m_camera.transform.position.y, 0.0f, 0.0f);
        else
            playingZone = new Rect(m_gm.GetPlanets()[0].transform.position.x, m_gm.GetPlanets()[0].transform.position.y, 0.0f, 0.0f);

        foreach (Planet p in m_gm.GetPlanets())
        {
            playingZone.xMin = Mathf.Min(playingZone.xMin, p.transform.position.x);
            playingZone.xMax = Mathf.Max(playingZone.xMax, p.transform.position.x);
            playingZone.yMin = Mathf.Min(playingZone.yMin, p.transform.position.y);
            playingZone.yMax = Mathf.Max(playingZone.yMax, p.transform.position.y);
        }

        playingZone.xMin -= playingZoneExtraBorder;
        playingZone.xMax += playingZoneExtraBorder;
        playingZone.yMin -= playingZoneExtraBorder;
        playingZone.yMax += playingZoneExtraBorder;


        if (!Input.touchSupported)
        {
            // SCROLL
            Vector2 scrollDirection = Vector2.zero;
            if (Input.GetKey(KeyCode.LeftArrow) ||
               (Input.mousePosition.x <= scrollingEdgeSize && Input.mousePosition.x >= 0.0f))
                scrollDirection.x -= 1.0f;
            if (Input.GetKey(KeyCode.RightArrow) ||
               (screenSize.x - Input.mousePosition.x < scrollingEdgeSize && Input.mousePosition.x < screenSize.x))
                scrollDirection.x += 1.0f;
            if (Input.GetKey(KeyCode.DownArrow) ||
               (Input.mousePosition.y <= scrollingEdgeSize && Input.mousePosition.y >= 0.0f))
                scrollDirection.y -= 1.0f;
            if (Input.GetKey(KeyCode.UpArrow) ||
               (screenSize.y - Input.mousePosition.y < scrollingEdgeSize && Input.mousePosition.y < screenSize.y))
                scrollDirection.y += 1.0f;

            if (scrollDirection.magnitude > 0.0f)
            {
                scrollDirection.Normalize();
                scrollDirection *= scrollingSpeed * Time.deltaTime * m_camera.orthographicSize;
                transform.position += new Vector3(scrollDirection.x, scrollDirection.y, 0.0f);
            }

            // ZOOM 
            if (Input.mouseScrollDelta.y != 0.0f)
            {
                m_camera.orthographicSize += Input.mouseScrollDelta.y * zoomingSpeed * Time.deltaTime;
                m_camera.orthographicSize = Mathf.Clamp(m_camera.orthographicSize, minZoom, maxZoom);
            }
        }
        else
        {

        }

        // CLAMP CAMERA TO PLAYING ZONE
        Rect cameraZone;
        // ZOOM CLAMPING
        cameraZone = ComputeCameraZone();
        if (cameraZone.width > playingZone.width && cameraZone.height > playingZone.height)
        {
            float scaleFactor = 1.0f;
            if (playingZone.width > playingZone.height)
            {
                scaleFactor = playingZone.width / cameraZone.width;
            }
            else
            {
                scaleFactor = playingZone.height / cameraZone.height;
            }
            m_camera.orthographicSize *= scaleFactor;
        }


        // SCROLL CLAMPING
        cameraZone = ComputeCameraZone();
        Vector3 cameraPosition = transform.position;
        if ((cameraZone.width) > playingZone.width)
        {
            cameraPosition.x = playingZone.center.x;
        }
        else
        {
            if (cameraZone.xMin < playingZone.xMin)
            {
                cameraZone.x = playingZone.xMin;
            }
            else if (cameraZone.xMax > playingZone.xMax)
            {
                cameraZone.x = playingZone.xMax - cameraZone.width;
            }
            cameraPosition.x = cameraZone.center.x;
        }

        if (cameraZone.height > playingZone.height)
        {
            cameraPosition.y = playingZone.center.y;
        }
        else
        {
            if (cameraZone.yMin < playingZone.yMin)
            {
                cameraZone.y = playingZone.yMin;
            }
            else if (cameraZone.yMax > playingZone.yMax)
            {
                cameraZone.y = playingZone.yMax - cameraZone.height;
            }
            cameraPosition.y = cameraZone.center.y;
        }
        transform.position = cameraPosition;
    }

    Rect ComputeCameraZone()
    {
        Vector3 cameraWorldMin = m_camera.ViewportToWorldPoint(Vector3.zero);
        Vector3 cameraWorldMax = m_camera.ViewportToWorldPoint(Vector3.one);
        Rect cameraZone = new Rect();
        cameraZone.xMin = cameraWorldMin.x;
        cameraZone.xMax = cameraWorldMax.x;
        cameraZone.yMin = cameraWorldMin.y;
        cameraZone.yMax = cameraWorldMax.y;
        return cameraZone;
    }

    Camera m_camera;
    GameManager m_gm;
}
