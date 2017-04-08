using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    public Player player;

    public void PlayAction()
    {
        // GATHER PLANETS
        List<Planet> myPlanets = new List<Planet>();
        List<Planet> notMyPlanets = new List<Planet>();
        foreach(Planet p in m_gm.GetPlanets())
        {
            if (p.owner == player) myPlanets.Add(p);
            else notMyPlanets.Add(p);
        }

        // CONQUEST PHASE
        if (m_gm.GetCurrentPhase() == GameManager.Phase.Conquest)
        {
            // SORT MY PLANETS FROM STRONGEST TO WEAKEST
            myPlanets.Sort((x, y) => -x.armyCount.CompareTo(y.armyCount));
            // SORT OTHER PLANETS FROM WEAKEST TO STRONGEST
            notMyPlanets.Sort((x, y) => x.armyCount.CompareTo(y.armyCount));

            foreach (Planet myPlanet in myPlanets)
            {
                // IF WE REACH PLANET WITH 1 ARMY, WE CAN'T DO ANYTHING
                if (myPlanet.armyCount == 1)
                    break;

                foreach (Planet otherPlanet in notMyPlanets)
                {
                    // IF WE CAN ATTACK, ATTACK
                    if (myPlanet.HasLinkWith(otherPlanet))
                    {
                        m_gm.Attack(myPlanet, otherPlanet);
                        return;
                    }

                    // IF NO ACTIONS LEFT, DON'T GO ANY FURTHER
                    if (m_gm.GetRemainingActions() == 0)
                        continue;

                    // TRY TO LINK TO THE PLANET
                    if (m_gm.CanLinkPlanets(player, myPlanet, otherPlanet))
                    {
                        m_gm.LinkPlanets(myPlanet, otherPlanet);
                        return;
                    }
                }
            }
        }
        // RECRUITMENT PHASE
        else if (m_gm.GetCurrentPhase() == GameManager.Phase.Recruitment)
        {
            if (m_gm.GetRemainingActions() > 0)
            {
                // MAKE A LIST OF PLANETS THAT ARE LINKED TO ENEMIES
                List<Planet> linkedToEnemyPlanets = new List<Planet>();
                linkedToEnemyPlanets.InsertRange(0, myPlanets);
                linkedToEnemyPlanets.RemoveAll(p => (CountEnemyLinks(p) == 0));

                if (linkedToEnemyPlanets.Count > 0)
                {
                    // SORT LINKED PLANETS FROM THE MOST TO THE LEAST LINKED TO ENEMIES 
                    linkedToEnemyPlanets.Sort((x, y) => CountEnemyLinks(x).CompareTo(CountEnemyLinks(y)));

                    foreach (Planet p in linkedToEnemyPlanets)
                    {
                        int enemyArmy = GetHighestLinkedEnemyArmy(p);
                        if (enemyArmy >= p.armyCount && p.armyCount < m_gm.maxRecruitsPerPlanet)
                        {
                            m_gm.Recruit(p);
                            return;
                        }
                    }

                    // IF ALL LINKED PLANETS ARE STRONGER THAN ENEMY PLANETS, RECRUIT ON THE WEAKER ONE
                    // SORT FROM WEAKEST TO STRONGEST
                    linkedToEnemyPlanets.Sort((x, y) => x.armyCount.CompareTo(y.armyCount));

                    foreach (Planet p in linkedToEnemyPlanets)
                    {
                        if (p.armyCount < m_gm.maxRecruitsPerPlanet)
                        {
                            m_gm.Recruit(p);
                            return;
                        }
                    }
                }
                else
                {
                    // IF NOT LINKED TO ANYONE, WE'LL JUST RECRUIT ON THE LEAST ARMED PLANET

                    // SORT MY PLANETS FROM THE LEAST TO THE MOST ARMED
                    myPlanets.Sort((x, y) => x.armyCount.CompareTo(y.armyCount));

                    foreach (Planet p in myPlanets)
                    {
                        if (p.armyCount < m_gm.maxRecruitsPerPlanet)
                        {
                            m_gm.Recruit(p);
                            return;
                        }
                    }
                }
            }
        }

        m_gm.EndPhase();
    }

    void Start()
    {
        m_gm = FindObjectOfType<GameManager>();
    }

    static int CountEnemyLinks(Planet _p)
    {
        int result = 0;
        foreach (Planet p in _p.links)
        {
            if (p != null && p.owner != null && p.owner != _p.owner)
            {
                ++result;
            }
        }
        return result;
    }

    static int GetHighestLinkedEnemyArmy(Planet _p)
    {
        int result = 0;
        foreach (Planet p in _p.links)
        {
            if (p != null && p.owner != null && p.owner != _p.owner)
            {
                result = Mathf.Max(result, p.armyCount);
            }
        }
        return result;
    }

    GameManager m_gm;
}
