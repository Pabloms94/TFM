using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ScriptRock : MonoBehaviour {
    private Rigidbody rockRigidbody;
    public GameObject bridge;
    private NavMeshObstacle obstacle;
    // Use this for initialization
    void Start () {
        rockRigidbody = GetComponent<Rigidbody>();
        obstacle = bridge.GetComponent<NavMeshObstacle>();
     
        //brigde = GameObject.FindGameObjectWithTag("Bridge");
    }
	
	// Update is called once per frame
	void Update () {
        if (transform.position.y < -0.9)
        {
            rockRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            bridge.SetActive(true);
            obstacle.enabled = false;
            //Debug.Log(brigde.activeSelf);
        }
	}
}
