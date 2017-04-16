using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [Header("Scrolling")]
    public float scrollingEdgeSize = 50.0f;
    public float scrollingSpeed = 10.0f;
    public float playingZoneExtraBorder = 5.0f;
    public bool scrollingInertia = true;
    public float scrollingInertiaDeceleration = 1.0f;

    [Header("Zooming")]
    public float zoomingSpeed = 0.5f;
    public float minZoom = 0.5f;
    public float maxZoom = 10.0f;

    void Start ()
    {
        m_camera = GetComponent<Camera>();
        m_gm = FindObjectOfType<GameManager>();
        m_previousTouches = new List<WorldTouch>();
        m_invalidTouches = new List<int>();
	}
	
	void Update ()
    {
        if (!Input.touchSupported)
        {
            UpdateKeyboardMouseInput();
        }
        else
        {
            UpdateMultiTouchInput();
        }

        ClampCameraToPlayingZone();
    }

    void UpdateMultiTouchInput()
    {
        int validTouchCount = 0;
        foreach(Touch t in Input.touches)
        {
            // BYPASS INVALID TOUCHES
            if (m_invalidTouches.FindIndex(id => (id == t.fingerId)) >= 0)
                continue;

            // CHECK FOR INVALID TOUCHES
            if (t.phase == TouchPhase.Began)
            {
                Ray r = m_camera.ScreenPointToRay(new Vector3(t.position.x, t.position.y));
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(r, out hit, 100.0f, LayerMask.GetMask("Planets")))
                {
                    Player currentPlayer = m_gm.GetCurrentPlayer();
                    Planet p = hit.collider.gameObject.GetComponent<Planet>();
                    if (currentPlayer.controlType == PlayerControlType.Human && p.owner == currentPlayer)
                    {
                        m_invalidTouches.Add(t.fingerId);
                        continue;
                    }
                }
            }
            ++validTouchCount;
        }

        // STATE CHANGE EVENTS
        if (Input.touchCount == 0)
        {
            if (m_touchState != TouchState.Idle)
            {
                m_touchState = TouchState.Idle;
                // IDLE START STATE
            }
        }
        if (m_touchState == TouchState.Idle && Input.touchCount == 1) // TODO: raycast to see if we don't touch any planet 
        {
            if (m_touchState != TouchState.Scrolling)
            {
                m_touchState = TouchState.Scrolling;
                // SCROLLING START STATE
            }
        }
        if (Input.touchCount >= 2)
        {
            if (m_touchState != TouchState.Zooming)
            {
                m_touchState = TouchState.Zooming;
                // ZOOMING START STATE
            }
        }

        // STATES
        switch (m_touchState)
        {
            case TouchState.Idle:
                {
                    if (scrollingInertia && scrollInertiaVelocity.magnitude > 0.001f)
                    {
                        // APPLY INERTIA VELOCITY
                        transform.position = transform.position + (new Vector3(scrollInertiaVelocity.x, scrollInertiaVelocity.y) * Time.deltaTime * m_camera.orthographicSize);

                        // DECAY INERTIA VELOCITY
                        float v = scrollInertiaVelocity.magnitude;
                        v -= scrollingInertiaDeceleration * Time.deltaTime;
                        scrollInertiaVelocity = scrollInertiaVelocity.normalized * v;
                    }
                    else
                    {
                        scrollInertiaVelocity = Vector2.zero;
                    }
                }
                break;

            case TouchState.Scrolling:
                {
                    Touch currentTouch = Input.touches[0];
                    Vector3 currentTouchWorldPosition = m_camera.ScreenToWorldPoint(new Vector3(currentTouch.position.x, currentTouch.position.y));
                    WorldTouch previousTouch;
                    int previousTouchIndex = m_previousTouches.FindIndex(wt => (wt.fingerId == currentTouch.fingerId));
                    if (previousTouchIndex < 0)
                    {
                        previousTouch = new WorldTouch();
                        previousTouch.fingerId = currentTouch.fingerId;
                        previousTouch.worldPosition = currentTouchWorldPosition;
                    }
                    else
                    {
                        previousTouch = m_previousTouches[previousTouchIndex];
                    }

                    Vector3 delta = currentTouchWorldPosition - previousTouch.worldPosition;
                    transform.position = transform.position - delta;

                    scrollInertiaVelocity = -delta / Time.deltaTime;
                }
                break;

            case TouchState.Zooming:
                {
                    Touch currentTouchA = Input.touches[0];
                    Touch currentTouchB = Input.touches[1];
                    Vector3 currentTouchAWorldPosition = m_camera.ScreenToWorldPoint(new Vector3(currentTouchA.position.x, currentTouchA.position.y));
                    Vector3 currentTouchBWorldPosition = m_camera.ScreenToWorldPoint(new Vector3(currentTouchB.position.x, currentTouchB.position.y));
                    WorldTouch previousTouchA;
                    int previousTouchAIndex = m_previousTouches.FindIndex(wt => (wt.fingerId == currentTouchA.fingerId));
                    if (previousTouchAIndex < 0)
                    {
                        previousTouchA = new WorldTouch();
                        previousTouchA.fingerId = currentTouchA.fingerId;
                        previousTouchA.worldPosition = currentTouchAWorldPosition;
                    }
                    else
                    {
                        previousTouchA = m_previousTouches[previousTouchAIndex];
                    }
                    WorldTouch previousTouchB;
                    int previousTouchBIndex = m_previousTouches.FindIndex(wt => (wt.fingerId == currentTouchB.fingerId));
                    if (previousTouchBIndex < 0)
                    {
                        previousTouchB = new WorldTouch();
                        previousTouchB.fingerId = currentTouchB.fingerId;
                        previousTouchB.worldPosition = currentTouchBWorldPosition;
                    }
                    else
                    {
                        previousTouchB = m_previousTouches[previousTouchBIndex];
                    }

                    Vector3 previousAToB = previousTouchB.worldPosition - previousTouchA.worldPosition;
                    Vector3 previousCenter = previousTouchA.worldPosition + (previousAToB * 0.5f);
                    Vector3 currentAToB = currentTouchBWorldPosition - currentTouchAWorldPosition;
                    Vector3 currentCenter = currentTouchAWorldPosition + (currentAToB * 0.5f);

                    Vector3 delta = currentCenter - previousCenter;
                    transform.position = transform.position - delta;

                    float scaleFactor = previousAToB.magnitude / currentAToB.magnitude;
                    m_camera.orthographicSize *= scaleFactor;

                    scrollInertiaVelocity = Vector2.zero;
                }
                break;
        }

        // STORE TOUCHES FOR NEXT FRAME
        m_previousTouches.Clear();
        foreach (Touch t in Input.touches)
        {
            // CHECK FOR INVALID TOUCHES
            int invalidTouchIndex = m_invalidTouches.FindIndex(id => (id == t.fingerId));
            if (invalidTouchIndex >= 0)
            {
                if (t.phase == TouchPhase.Ended)
                {
                    m_invalidTouches.RemoveAt(invalidTouchIndex);
                }
                continue;
            }

            WorldTouch wt = new WorldTouch();
            wt.fingerId = t.fingerId;
            wt.worldPosition = m_camera.ScreenToWorldPoint(new Vector3(t.position.x, t.position.y));
            m_previousTouches.Add(wt);
        }
    }

    void UpdateKeyboardMouseInput()
    {
        Vector2 screenSize = new Vector2(m_camera.pixelWidth, m_camera.pixelHeight);

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

    void ClampCameraToPlayingZone()
    {
        Rect playingZone = ComputePlayingZone();
        Rect cameraZone = ComputeCameraZone();
        // ZOOM CLAMPING
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
        cameraZone = ComputeCameraZone(); // Camera may have changed, recompute zone
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

    Rect ComputePlayingZone()
    {
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

        return playingZone;
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

    enum TouchState
    {
        Idle,
        Scrolling,
        Zooming
    }
    TouchState m_touchState = TouchState.Idle;
    Vector2 scrollInertiaVelocity;
    struct WorldTouch
    {
        public int fingerId;
        public Vector3 worldPosition;
    }
    List<WorldTouch> m_previousTouches;
    List<int> m_invalidTouches;
}
