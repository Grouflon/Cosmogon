using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemGen : MonoBehaviour {

    public Planet planetPrefab;
    public int nbPlanet;

    // Use this for initialization
    void Start()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        List<Planet> planets = new List<Planet>();

        string vowels = "aeiou";
        string consonants = "bcdfghjklmnpqrstvwxz";

        //planetPrefabs instantiations
        for (int i = 0; i < nbPlanet; i++)
        {
            //position entre -2 et 2 et -4 et 4 par pas de 0.5
            float x = 0.5f * Random.Range(0, 9) - 2;
            float y = 0.5f * Random.Range(0, 17) - 4;

            // Name generation
            int nameLength = Random.Range(1, 6) * 2;
            string planetName = "";
            //int vowelConsonantSelector = Random.Range(0, 2); // outer bound is exclusive, so 2 and not 1
            int vowelConsonantSelector = 1; // 0 is vowel, 1 is consonant
            for (int l = 0; l < nameLength; ++l)
            {
                char nextChar;
                if (vowelConsonantSelector == 0)
                {
                    nextChar = vowels[Random.Range(0, vowels.Length)];
                }
                else
                {
                    nextChar = consonants[Random.Range(0, consonants.Length)];
                }
                planetName += nextChar;
                vowelConsonantSelector = (vowelConsonantSelector + 1) % 2;
            }


            // Add planet
            Planet p = Instantiate(planetPrefab, new Vector3(x, y, 0.0f), Quaternion.identity);
            //One chance out of three to be spiced
            int isSpice = Random.Range(0, 4);
            if (isSpice == 0) {
                p.isSpiceProvider = true;
            } else {
                p.isSpiceProvider = false;
            }
            p.name = planetName;
            p.SetMaxLinkCount(Random.Range(2, 4));
            p.armyCount = Random.Range(0, 4);
            planets.Add(p);
        }


        //Links Creation
        /*for (int j = 0; j < nbPlanet; j++) {
            for (int i = 0; i < nbPlanet; i++)
            {
                if (i != j) {
                    CreateLink(planets[j], planets[i]);
                }
            }
        }*/

        //Assign Players
        for (int i = 0; i < gm.GetPlayers().Count; ++i)
        {
            if (i >= nbPlanet)
                break;

            planets[i].owner = gm.GetPlayers()[i];
            planets[i].armyCount = 3;
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
