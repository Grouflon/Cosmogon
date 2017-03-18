using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemGen : MonoBehaviour {

    public GameObject planet;
    public int nbPlanet;

    public GameObject link;
    public Vector3 scale;

    public Text text;
    public Canvas canvas;

    public List<GameObject> planets;


    // Use this for initialization
    void Start()
    {
        //Planets instantiations
        for (int i = 0; i < nbPlanet; i++)
        {
            //position entre -2 et 2 et -4 et 4 par pas de 0.5
            float x = 0.5f * Random.Range(0, 9) - 2;
            float y = 0.5f * Random.Range(0, 17) - 4;
            Vector3 pos = new Vector3(x, y, 0);

            //Instantiation de la planete
            GameObject pla = Instantiate(planet, pos, Quaternion.identity);
            pla.name = "Planet" + i;
            pla.GetComponent<Planet>().nbArmies = Random.Range(0, 4);
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
    void CreateLink (GameObject a, GameObject b)
    {
        float distance = Vector3.Distance(a.transform.position, b.transform.position);
        if (distance<3.0f && !a.GetComponent<Planet>().neighbors.Contains(b)) {
            GameObject link1;
            link1 = Instantiate(link, a.transform.position, Quaternion.identity);
            link1.transform.LookAt(b.transform);
            scale = new Vector3(1.0f, 1.0f, distance);
            link1.transform.localScale = scale;
            a.GetComponent<Planet>().neighbors.Add(b);
            b.GetComponent<Planet>().neighbors.Add(a);
        }
    }
}
