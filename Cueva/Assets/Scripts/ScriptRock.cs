using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptRock : MonoBehaviour {
    private Rigidbody rockRigidbody;
    public GameObject brigde;
	// Use this for initialization
	void Start () {
        rockRigidbody = GetComponent<Rigidbody>();
        //brigde = GameObject.FindGameObjectWithTag("Bridge");
    }
	
	// Update is called once per frame
	void Update () {
        if (transform.position.y < -0.9)
        {
            rockRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            brigde.SetActive(true);
            Debug.Log(brigde.activeSelf);
        }
	}
}
