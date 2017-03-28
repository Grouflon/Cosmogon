using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Link : MonoBehaviour {

    public void Init(Planet _a, Planet _b)
    {
        m_planetA = _a;
        m_planetB = _b;

        m_planetA.linkRemoved += OnLinkRemoved;

        Update();
    }

    void OnLinkRemoved(Planet _planet)
    {
        if (_planet != m_planetB)
            return;

        m_planetA.linkRemoved -= OnLinkRemoved;
        Destroy(gameObject);
    }

    void Update()
    {
        if (m_planetA == null || m_planetB == null)
            return;

        int indexA = -1;
        int indexB = -1;
        for (int i = 0; i < m_planetA.links.Length; ++i) { if (m_planetA.links[i] == m_planetB) { indexA = i; break; } }
        for (int i = 0; i < m_planetB.links.Length; ++i) { if (m_planetB.links[i] == m_planetA) { indexB = i; break; } }

        Vector3 AtoB = m_planetB.linkAnchors[indexB].transform.position - m_planetA.linkAnchors[indexA].transform.position;
        transform.LookAt(m_planetB.linkAnchors[indexB].transform);
        transform.position = m_planetA.linkAnchors[indexA].transform.position;
        transform.localScale = new Vector3(1.0f, 1.0f, AtoB.magnitude);
    }

    Planet m_planetA;
    Planet m_planetB;
}
