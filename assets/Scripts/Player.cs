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
}
