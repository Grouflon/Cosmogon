using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// All the player and ai input must manipulate the game through the functions of this class

public class GameManager : MonoBehaviour {

    public enum Phase
    {
        Conquest,
        Recruitment
    }

    [Header("Balance")]
    public int linksPerTurn = 3;
    public int maxRecruitsPerPlanet = 10;
    public float basicLinkRange = 4.0f;

    public delegate void PlanetAction(Planet _planet);
    public delegate void GameAction();
    public event PlanetAction planetAdded;
    public event GameAction phaseEnded;

    [HideInInspector] public Player[] players;

    #region GameActions

    public void EndPhase()
    {
        if (m_phase == Phase.Conquest)
        {
            StartPhase(Phase.Recruitment);
        }
        else if (m_phase == Phase.Recruitment)
        {
            m_currentPlayer = (m_currentPlayer + 1) % players.Length;
            if (m_currentPlayer == 0)
                ++m_turn;

            StartPhase(Phase.Conquest);
        }

        if (phaseEnded != null) phaseEnded();
    }

    public void Attack(Planet _from, Planet _to)
    {
        if (!_from.HasLinkWith(_to))
            return;

        if (_from.armyCount < 2)
            return;

        if (_from.owner == _to.owner)
            return;

        int diff = _to.armyCount - _from.armyCount;

        _from.armyCount = 1;
        _to.armyCount = Mathf.Abs(diff);
        if (diff < 0)
        {
            _to.owner = _from.owner;
        }
        else if (diff == 0)
        {
            _to.owner = null;
        }
    }

    public void LinkPlanets(Planet _a, Planet _b)
    {
        if (m_remainingActions == 0)
            return;

        Vector3 AtoB = _b.transform.position - _a.transform.position;
        if (AtoB.magnitude > basicLinkRange)
            return;

        if (_a.AddLink(_b))
            --m_remainingActions;
    }

    public void UnlinkPlanets(Planet _a, Planet _b)
    {
        if (m_remainingActions == 0)
            return;

        if (_a.RemoveLink(_b))
            --m_remainingActions;
    }

    public void Recruit(Planet _planet)
    {
        if (m_remainingActions == 0)
            return;

        if (_planet.armyCount < maxRecruitsPerPlanet)
        {
            ++_planet.armyCount;
            --m_remainingActions;
        }
    }

    #endregion

    public Player GetCurrentPlayer()
    {
        return players[m_currentPlayer];
    }

    public Phase GetCurrentPhase()
    {
        return m_phase;
    }

    public int GetRemainingActions()
    {
        return m_remainingActions;
    }

    public void RegisterPlanet(Planet _p)
    {
        foreach (Planet p in m_planets)
        {
            if (p == _p)
                return;
        }

        m_planets.Add(_p);
        if (planetAdded != null) planetAdded(_p);
    }

    void StartPhase(Phase _phase)
    {
        m_phase = _phase;

        switch (_phase)
        {
            case Phase.Conquest:
                {
                    m_remainingActions = linksPerTurn;
                }
                break;

            case Phase.Recruitment:
                {
                    int recruitsCount = 0;
                    foreach (Planet p in m_planets)
                    {
                        if (p.owner == GetCurrentPlayer())
                            ++recruitsCount;
                    }

                    m_remainingActions = recruitsCount;
                }
                break;

            default: break;
        }
    }


	// Use this for initialization
	void Awake ()
    {
        m_planets = new List<Planet>();
        players = FindObjectsOfType<Player>();
	}

    void Start()
    {
        StartPhase(Phase.Conquest);
    }

	// Update is called once per frame
	void Update ()
    {
		
	}

    List<Planet> m_planets;

    Phase m_phase = Phase.Conquest;
    int m_turn = 0;
    int m_currentPlayer = 0;
    int m_remainingActions = 0;
}
