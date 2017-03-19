using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour
{
    public Link linkPrefab;

    // public Player owner = null;
    public int armyCount;
    [ReadOnly] public Planet[] links;

    public delegate void PlanetAction(Planet _planet);
    public event PlanetAction linkAdded;
    public event PlanetAction linkRemoved;

    public void AddLink(Planet _planet)
    {
        // First check if link is already there
        for (int i = 0; i < links.Length; ++i)
        {
            if (links[i] == _planet)
                return;
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
        if (selfLink != -1 && otherLink != -1)
        {
            links[selfLink] = _planet;
            _planet.links[otherLink] = this;

            Link link = Instantiate(linkPrefab);
            link.Init(this, _planet);

            if (linkAdded != null) linkAdded(_planet);
            if (_planet.linkAdded != null) _planet.linkAdded(this);
        }
    }

    public void RemoveLink(Planet _planet)
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

        for (int i = 0; i < _planet.links.Length; ++i)
        {
            if (_planet.links[i] == this)
            {
                _planet.links[i] = null;
                break;
            }
        }

        if (found)
        {
            if (linkRemoved != null) linkRemoved(_planet);
            if (_planet.linkRemoved != null) _planet.linkRemoved(this);
        }
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
    }

    // Use this for initialization
    void Start ()
    {
    }
	
	// Update is called once per frame
	void Update ()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            GetComponent<MeshRenderer>().material.color = Color.black;
        }*/
    }
}
