using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerControlType
{
    Human,
    AI
}

public class Player : MonoBehaviour
{
    public Color color;
    public PlayerControlType controlType;

    public int spiceCount=0;

    public AI GetAI()
    {
        return m_ai;
    }

    void Update()
    {
        // CREATE/DESTROY AI
        if (controlType == PlayerControlType.AI && m_ai == null)
        {
            GameObject go = new GameObject("AI");
            m_ai = go.AddComponent<AI>();
            m_ai.player = this;
            m_ai.transform.parent = transform;
        }

        if (controlType != PlayerControlType.AI && m_ai != null)
        {
            Destroy(m_ai.gameObject);
        }
    }

    AI m_ai;
}
