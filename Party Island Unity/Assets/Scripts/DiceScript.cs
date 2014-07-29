using UnityEngine;
using System.Collections;

public class DiceScript : MonoBehaviour {

    public float SpinSpeed;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.Rotate(0, SpinSpeed, 0);
	}
}
