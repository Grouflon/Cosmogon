using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour {

    public Text text;

    public int nbArmies=0;

    public List<GameObject> neighbors;

    // Use this for initialization
    void Start () {
        Vector3 tpos = GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(transform.position);
        Text armiesNb = Instantiate(text, tpos, Quaternion.identity) as Text;
        armiesNb.text = nbArmies.ToString();
        armiesNb.name = this.name;
        armiesNb.transform.SetParent(GameObject.Find("Canvas").transform, false);
    }
	
	// Update is called once per frame
	void Update () {
        /*if (Input.GetMouseButtonDown(0))
        {
            GetComponent<MeshRenderer>().material.color = Color.black;
        }*/
    }

    void OnMouseDown()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}
