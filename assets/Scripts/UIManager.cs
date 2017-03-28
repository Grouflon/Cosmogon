using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public Text armyCountTextPrefab;
    public Text planetNameTextPrefab;
    public GameObject selectionPrefab;
    public GameObject linkPrefab;
    public GameObject crossPrefab;
    public GameObject rangePrefab;

    public Text phaseInfoText;

    public void OnMouseEnterPlanet(Planet _p)
    {
        m_hoveredPlanet = _p;
    }


    public void OnMouseExitPlanet(Planet _p)
    {
        m_hoveredPlanet = null;
    }


    public void OnPlanetClicked(Planet _p)
    {
        if (m_gm.GetCurrentPhase() == GameManager.Phase.Conquest)
        {
            if (m_selectedPlanet == null)
            {
                if (_p.owner == m_gm.GetCurrentPlayer())
                    m_selectedPlanet = _p;
            }
            else
            {
                m_gm.Attack(m_selectedPlanet, _p);
                m_selectedPlanet = null;
            }
        }
        else if (m_gm.GetCurrentPhase() == GameManager.Phase.Recruitment)
        {
            if (_p.owner == m_gm.GetCurrentPlayer())
                m_gm.Recruit(_p);
        }
        
    }


    public void OnPlanetDragged(Planet _p)
    {
        if (m_gm.GetCurrentPhase() == GameManager.Phase.Conquest
            && _p.owner == m_gm.GetCurrentPlayer()
            && m_gm.GetRemainingActions() > 0)
        {
            m_linkSource = _p;
        }
    }


	void Awake ()
    {
        m_canvasTransform = (RectTransform)FindObjectOfType<Canvas>().transform;
        m_gm = FindObjectOfType<GameManager>();
        m_planetUIs = new Dictionary<Planet, PlanetUI>();

        m_selectionObject = Instantiate(selectionPrefab);
        m_selectionObject.SetActive(false);

        m_linkObject = Instantiate(linkPrefab);
        m_linkObject.SetActive(false);

        m_crossObject = Instantiate(crossPrefab);
        m_crossObject.SetActive(false);

        m_rangeObject = Instantiate(rangePrefab);
        m_rangeObject.SetActive(false);

        m_gm.planetAdded += OnPlanetAdded;
        m_gm.phaseEnded += OnPhaseEnded;
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
	

	void Update ()
    {
        Camera cam = Camera.main;
        m_selectionObject.SetActive(false);
        m_linkObject.SetActive(false);
        m_crossObject.SetActive(false);
        m_rangeObject.SetActive(false);

        // INPUTS
        if (Input.GetMouseButtonDown(0) && m_hoveredPlanet == null)
        {
            m_selectedPlanet = null;
        }

        if (m_linkSource != null && Input.GetMouseButtonUp(0))
        {
            if (m_hoveredPlanet != null)
            {
                if (m_linkSource.HasLinkWith(m_hoveredPlanet))
                {
                    m_gm.UnlinkPlanets(m_linkSource, m_hoveredPlanet);
                }
                else
                {
                    m_gm.LinkPlanets(m_linkSource, m_hoveredPlanet);
                }
            }

            m_linkSource = null;
        }

        // LINK CREATION FEEDBACK
        if (m_linkSource != null)
        {
            m_linkObject.SetActive(true);
            m_linkObject.transform.position = m_linkSource.transform.position;

            m_rangeObject.SetActive(true);
            m_rangeObject.transform.position = m_linkSource.transform.position + new Vector3(0.0f, 0.0f, 100.0f);
            m_rangeObject.transform.localScale = Vector3.one * m_gm.basicLinkRange * 2.0f; // Range object radius is 0.5

            Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0.0f;
            Vector3 linkVector = mouseWorldPosition - m_linkObject.transform.position;
            m_linkObject.transform.LookAt(mouseWorldPosition);
            m_linkObject.transform.localScale = new Vector3(1.0f, 1.0f, linkVector.magnitude);

            if (m_hoveredPlanet != null && m_linkSource.HasLinkWith(m_hoveredPlanet))
            {
                m_crossObject.SetActive(true);
                m_crossObject.transform.position = m_linkObject.transform.position + (linkVector * 0.5f);
            }
        }

        // PLANET TEXTS
		foreach(KeyValuePair<Planet, PlanetUI> pair in m_planetUIs)
        {
            Planet p = pair.Key;
            PlanetUI ui = pair.Value;
            Vector3 planetScreenPosition = cam.WorldToScreenPoint(pair.Key.transform.position);

            ui.armyCountText.text = p.armyCount.ToString();
            ui.nameText.text = p.name;

            ui.armyCountText.rectTransform.position = planetScreenPosition;
            ui.nameText.rectTransform.position = planetScreenPosition + new Vector3(15.0f, -1.0f, 0.0f);

            if (p.owner != null)
            {
                Color c = p.owner.color; ;
                c.a = 1.0f;
                ui.nameText.color = c;
            }
            else
            {
                ui.nameText.color = planetNameTextPrefab.color;
            }

            if (p == m_selectedPlanet)
            {
                m_selectionObject.SetActive(true);
                m_selectionObject.transform.position = p.transform.position + new Vector3(0.0f, 0.0f, 0.1f);
            }
        }

        // MAIN UI TEXTS
        Player currentPlayer = m_gm.GetCurrentPlayer();
        Color playerColor = currentPlayer.color;
        playerColor.a = 1.0f;
        phaseInfoText.color = playerColor;
        string actionQualifier = "actions";
        if (m_gm.GetCurrentPhase() == GameManager.Phase.Conquest) actionQualifier = "links";
        else if (m_gm.GetCurrentPhase() == GameManager.Phase.Recruitment) actionQualifier = "recruits";
        phaseInfoText.text = currentPlayer.name + " playing"
            + " | " + m_gm.GetCurrentPhase().ToString() + " phase"
            + " | " + m_gm.GetRemainingActions() + " " + actionQualifier + " remaining";
	}


    void OnPhaseEnded()
    {
        m_selectedPlanet = null;
        m_linkSource = null;
        m_hoveredPlanet = null;
    }


    RectTransform m_canvasTransform;
    GameManager m_gm;
    struct PlanetUI
    {
        public Text armyCountText;
        public Text nameText;
    }
    Dictionary<Planet, PlanetUI> m_planetUIs;
    Planet m_hoveredPlanet = null;
    Planet m_selectedPlanet = null;
    Planet m_linkSource = null;
    GameObject m_selectionObject;
    GameObject m_linkObject;
    GameObject m_crossObject;
    GameObject m_rangeObject;
}
