using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public Text armyCountTextPrefab;
    public Text planetNameTextPrefab;

	// Use this for initialization
	void Start ()
    {
        m_canvasTransform = (RectTransform)FindObjectOfType<Canvas>().transform;
        m_gameManager = FindObjectOfType<GameManager>();

        m_planetUIs = new Dictionary<Planet, PlanetUI>();

        m_gameManager.planetAdded += OnPlanetAdded;
	}

    void OnPlanetAdded(Planet _planet)
    {
        PlanetUI ui = new PlanetUI();
        ui.armyCountText = Instantiate(armyCountTextPrefab);
        ui.nameText = Instantiate(planetNameTextPrefab);

        ui.armyCountText.transform.SetParent(m_canvasTransform);
        ui.nameText.transform.SetParent(m_canvasTransform);

        m_planetUIs.Add(_planet, ui);
    }
	
	// Update is called once per frame
	void Update ()
    {
        Camera cam = Camera.main;
		foreach(KeyValuePair<Planet, PlanetUI> pair in m_planetUIs)
        {
            Planet p = pair.Key;
            PlanetUI ui = pair.Value;
            Vector3 planetScreenPosition = cam.WorldToScreenPoint(pair.Key.transform.position);

            ui.armyCountText.text = p.armyCount.ToString();
            ui.nameText.text = p.name;

            ui.armyCountText.rectTransform.position = planetScreenPosition;
            ui.nameText.rectTransform.position = planetScreenPosition + new Vector3(15.0f, -1.0f, 0.0f);
        }
	}

    RectTransform m_canvasTransform;
    GameManager m_gameManager;

    struct PlanetUI
    {
        public Text armyCountText;
        public Text nameText;
    }
    Dictionary<Planet, PlanetUI> m_planetUIs;
}
