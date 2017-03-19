using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public Planet planetPrefab;

    public delegate void PlanetAction(Planet _planet);
    public event PlanetAction planetAdded;

    public Planet AddPlanet(string _name, Vector2 _position)
    {
        Planet p = Instantiate(planetPrefab, new Vector3(_position.x, _position.y, 0.0f), Quaternion.identity);
        p.name = _name;

        m_planets.Add(p);

        if (planetAdded != null) planetAdded(p);

        return p;
    }

	// Use this for initialization
	void Start () {
        m_planets = new List<Planet>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    List<Planet> m_planets;
}
