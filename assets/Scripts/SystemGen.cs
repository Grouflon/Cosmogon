using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemGen : MonoBehaviour {

    public Planet planetPrefab;
    public int nbPlanet;

    public List<Planet> planets;


    // Use this for initialization
    void Start()
    {
        //planetPrefabs instantiations
        for (int i = 0; i < nbPlanet; i++)
        {
            //position entre -2 et 2 et -4 et 4 par pas de 0.5
            float x = 0.5f * Random.Range(0, 9) - 2;
            float y = 0.5f * Random.Range(0, 17) - 4;
            Vector3 pos = new Vector3(x, y, 0);

            //Instantiation de la planetPrefabe
            Planet pla = Instantiate(planetPrefab, pos, Quaternion.identity);
            pla.name = "planetPrefab" + i;
            pla.SetMaxLinkCount(nbPlanet);
            pla.armyCount = Random.Range(0, 4);
            planets.Add(pla);
        }


        //Links Creation
        for (int j = 0; j < nbPlanet; j++) {
            for (int i = 0; i < nbPlanet; i++)
            {
                if (i != j) {
                    CreateLink(planets[j], planets[i]);
                }
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
    }

    //Test distance, test neighbors and create a link
    void CreateLink (Planet a, Planet b)
    {
        float distance = Vector3.Distance(a.transform.position, b.transform.position);
        if (distance < 3.0f)
        {
            a.AddLink(b);
        }
    }
}
