using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum WinningCondition
{
    LastManStanding,
    FullConquest
}

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
    public float basicLinkRange = 2.0f;
    public float AIPlayTime = 2.0f;

    public delegate void PlanetAction(Planet _planet);
    public delegate void GameAction();
    public delegate void PlayerAction(Player _player);
    public event PlanetAction planetAdded;
    public event GameAction phaseEnded;
    public event PlayerAction gameOver;

    [HideInInspector] public Player[] players;

    public WinningCondition winningCondition = WinningCondition.LastManStanding;

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

        m_AITimer = 0.0f;

        if (phaseEnded != null) phaseEnded();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

    public bool IsGameOver()
    {
        return m_gameOver;
    }

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

    public ReadOnlyCollection<Planet> GetPlanets()
    {
        return m_planets.AsReadOnly();
    }

    public bool CanLinkPlanets(Player _p, Planet _a, Planet _b)
    {
        if (_a.owner != _p && _b.owner != _p)
            return false;

        if (_a.GetFreeLinksCount() == 0 || _b.GetFreeLinksCount() == 0)
            return false;

        return (_a.transform.position - _b.transform.position).magnitude <= basicLinkRange;
    }

    #endregion

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

    //Give the current player one spice for each spicy planet he owns
    void harvestSpice()
    {
        foreach(Planet pl in m_planets)
        {
            if (pl.owner == GetCurrentPlayer()&&pl.isSpiceProvider)
            {
                GetCurrentPlayer().spiceCount++;
            }
        }
    }

    void StartPhase(Phase _phase)
    {
        m_phase = _phase;

        switch (_phase)
        {
            case Phase.Conquest:
                {
                    harvestSpice();
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

	void Update ()
    {
        //Reset Scene on f6
        if (Input.GetKeyUp(KeyCode.F6))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        //Advance game phase on Space
        if (Input.GetKeyUp(KeyCode.Space))
        {
            EndPhase();
        }


        if (!m_gameOver)
        {
            // MANAGE AI
            if (GetCurrentPlayer().controlType == PlayerControlType.AI)
            {
                m_AITimer += Time.deltaTime;


                // NOTE: do not factorize call to GetCurrentPlayer(), result may change during the loop.
                while (GetCurrentPlayer().controlType == PlayerControlType.AI && m_AITimer > AIPlayTime)
                {
                    GetCurrentPlayer().GetAI().PlayAction();
                    m_AITimer -= AIPlayTime;
                }
            }

            // GATHER INFORMATION
            List<Player> livingPlayers = new List<Player>();
            int nonOwnedPlanets = 0;
            foreach (Planet p in m_planets)
            {
                if (p.owner == null)
                {
                    ++nonOwnedPlanets;
                }
                else if (livingPlayers.FindIndex(player => p.owner == player) == -1)
                {
                    livingPlayers.Add(p.owner);
                }
            }

            // TEST GAMEOVER CONDITIONS
            switch(winningCondition)
            {
                case WinningCondition.LastManStanding:
                    {
                        if (livingPlayers.Count == 1)
                        {
                            m_gameOver = true;
                            gameOver(livingPlayers[0]);
                        }
                    }
                    break;

                case WinningCondition.FullConquest:
                    {
                        if (livingPlayers.Count == 1 && nonOwnedPlanets == 0)
                        {
                            m_gameOver = true;
                            gameOver(livingPlayers[0]);
                        }
                    }
                    break;
            }
        }
    }

    List<Planet> m_planets;

    Phase m_phase = Phase.Conquest;
    int m_turn = 0;
    int m_currentPlayer = 0;
    int m_remainingActions = 0;
    bool m_gameOver = false;
    float m_AITimer = 0.0f;
}
