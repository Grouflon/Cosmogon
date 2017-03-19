using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Link : MonoBehaviour {

    public void Init(Planet _a, Planet _b)
    {
        m_planetA = _a;
        m_planetB = _b;

        _a.linkRemoved += OnLinkRemoved;
    }

    void OnLinkRemoved(Planet _planet)
    {
        Destroy(gameObject);
    }

    void Update()
    {
        Vector3 AtoB = m_planetB.transform.position - m_planetA.transform.position;
        transform.LookAt(m_planetB.transform);
        transform.position = m_planetA.transform.position;
        transform.localScale = new Vector3(1.0f, 1.0f, AtoB.magnitude); ;
    }

    Planet m_planetA;
    Planet m_planetB;
}
