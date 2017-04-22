using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour
{
    public Player owner = null;
    public bool isSpiceProvider = false;
    public int armyCount;
    public GameObject spiceIcon;

    #if UNITY_EDITOR
    [ReadOnly] public Planet[] links;
    #else
    public Planet[] links;
    #endif

    [HideInInspector] public GameObject[] linkAnchors;

    public Link linkPrefab;
    public GameObject linkAnchorPrefab;
    public float anchorIdleRadius = 1.0f;
    public float anchorActiveRadius = 1.5f;
    public float anchorLinkedRadius = 2.0f;
    public float anchorAcceleration = 0.5f;
    public float anchorRotationSpeed = 20.0f;

    public delegate void PlanetAction(Planet _planet);
    public event PlanetAction linkAdded;
    public event PlanetAction linkRemoved;

    // Low level link manipulation function, won't check the game rules (action count, current player etc...)
    public bool AddLink(Planet _planet)
    {
        if (_planet == this)
            return false;

        // First check if link is already there
        for (int i = 0; i < links.Length; ++i)
        {
            if (links[i] == _planet)
                return false;
        }

        // If not, try to add it
        int selfLink = -1;
        int otherLink = -1;
        for (int i = 0; i < links.Length; ++i)
        {
            if (links[i] == null)
            {
                selfLink = i;
            }
        }
        for (int i = 0; i < _planet.links.Length; ++i)
        {
            if (_planet.links[i] == null)
            {
                otherLink = i;
            }
        }
        if (selfLink == -1 || otherLink == -1)
            return false;


        links[selfLink] = _planet;
        _planet.links[otherLink] = this;

        Link link = Instantiate(linkPrefab);
        link.Init(this, _planet);

        if (linkAdded != null) linkAdded(_planet);
        if (_planet.linkAdded != null) _planet.linkAdded(this);

        return true;
    }

    // Low level link manipulation function, won't check the game rules (action count, current player etc...)
    public bool RemoveLink(Planet _planet)
    {
        bool found = false;
        for (int i = 0; i < links.Length; ++i)
        {
            if (links[i] == _planet)
            {
                links[i] = null;
                found = true;
                break;
            }
        }

        if (!found)
            return false;

        for (int i = 0; i < _planet.links.Length; ++i)
        {
            if (_planet.links[i] == this)
            {
                _planet.links[i] = null;
                break;
            }
        }

        if (linkRemoved != null) linkRemoved(_planet);
        if (_planet.linkRemoved != null) _planet.linkRemoved(this);

        return true;
    }

    public void SetMaxLinkCount(int _value)
    {
        if (_value < links.Length)
        {
            int toRemoveCount = links.Length - _value;
            for (int i = 0; i < toRemoveCount; ++i)
            {
                RemoveLink(links[_value + i]);
                links[_value + i] = null;
            }
        }

        Planet[] newLinks = new Planet[_value];
        for (int i = 0; i < Mathf.Min(_value, links.Length); ++i)
        {
            newLinks[i] = links[i];
        }
        links = newLinks;

        InitializeLinkAnchors();
    }

    public bool HasLinkWith(Planet _planet)
    {
        foreach (Planet p in links)
        {
            if (p == _planet)
                return true;
        }
        return false;
    }

    public int GetFreeLinksCount()
    {
        int result = 0;
        foreach(Planet p in links)
        {
            if (p == null) ++result;
        }
        return result;
    }

    // Use this for initialization
    void Start ()
    {
        m_gm = FindObjectOfType<GameManager>();
        m_ui = FindObjectOfType<UIManager>();

        //Spawn de l'icone de spice
        if (isSpiceProvider)
        {
            Instantiate(spiceIcon, new Vector3(transform.position.x + 0.10f, transform.position.y - 0.25f, 0.0f), Quaternion.identity);
        }

        InitializeLinkAnchors();

        // HANDLE INSTANTIATION FROM UNITY EDITOR
        m_gm.RegisterPlanet(this);
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateAnchors(anchorAcceleration);

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (owner != null)
        {
            renderer.material.color = owner.color;
        }
        else
        {
            renderer.material.color = Color.white;
        }
    }

    void InitializeLinkAnchors()
    {
        if (linkAnchors != null)
        {
            for (int i = 0; i < linkAnchors.Length; ++i)
            {
                Destroy(linkAnchors[i]);
            }
        }

        linkAnchors = new GameObject[links.Length];
        float angleStep = Mathf.PI * 2.0f / links.Length;
        for (int i = 0; i < links.Length; ++i)
        {
            linkAnchors[i] = Instantiate(linkAnchorPrefab);
        }

        UpdateAnchors(1.0f);
    }

    struct AnchorCoordinates
    {
        public int index;
        public float distance;
        public float angle;
    }

    void UpdateAnchors(float _t)
    {
        int unlinkedCount = 0;
        for (int i = 0; i < links.Length; ++i)
        {
            if (links[i] == null)
            {
                ++unlinkedCount;
            }
        }

        int unlinkedIndex = 0;
        for (int i = 0; i < links.Length; ++i)
        {
            Vector3 anchorRelativePosition = linkAnchors[i].transform.position - transform.position;
            anchorRelativePosition.z = 0.0f;
            float currentDistance = anchorRelativePosition.magnitude;
            float currentAngle = Mathf.Sign(Vector3.Cross(Vector3.right, anchorRelativePosition).z) * Vector2.Angle(Vector2.right, new Vector2(anchorRelativePosition.x, anchorRelativePosition.y));

            float wantedDistance;
            float wantedAngle;

            if (links[i] != null)
            {
                Vector3 selfToOther = links[i].transform.position - transform.position;
                wantedAngle = Mathf.Sign(Vector3.Cross(Vector3.right, selfToOther).z) * Vector2.Angle(Vector2.right, new Vector2(selfToOther.x, selfToOther.y));
                wantedDistance = anchorLinkedRadius;
            }
            else
            {
                wantedAngle = 360.0f / unlinkedCount * unlinkedIndex;
                wantedDistance = anchorIdleRadius;
                if (owner != null && m_gm.GetCurrentPlayer() == owner)
                {
                    wantedAngle += (Time.timeSinceLevelLoad % (360.0f / anchorRotationSpeed)) * anchorRotationSpeed;
                    wantedDistance = anchorActiveRadius;
                }
                ++unlinkedIndex;
            }


            float actualDistance = Mathf.Lerp(currentDistance, wantedDistance, _t);
            float actualRadAngle = Mathf.Deg2Rad * Mathf.LerpAngle((currentAngle + 360.0f) % 360.0f, (wantedAngle + 360.0f) % 360.0f, _t);

            linkAnchors[i].transform.position = transform.position + new Vector3(Mathf.Cos(actualRadAngle) * actualDistance, Mathf.Sin(actualRadAngle) * actualDistance, linkAnchors[i].transform.position.z - transform.position.z);
        }
    }

    void OnMouseEnter()
    {
        m_ui.OnMouseEnterPlanet(this);
    }

    void OnMouseExit()
    {
        m_ui.OnMouseExitPlanet(this);
    }

    void OnMouseUpAsButton()
    {
        m_ui.OnPlanetClicked(this);
    }

    private void OnMouseDrag()
    {
        m_ui.OnPlanetDragged(this);
    }

    GameManager m_gm;
    UIManager m_ui;
}
