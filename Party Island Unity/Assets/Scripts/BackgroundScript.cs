using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundScript : MonoBehaviour {

    public GameObject BackgroundSprite1;
    public GameObject BackgroundSprite2;

    private GameObject[] sprites;

    public float ScrollSpeed = .05f;

    public float lastUpdate;

	// Use this for initialization
	void Start () 
    {
        lastUpdate = Time.time;
        sprites = new GameObject[2] { BackgroundSprite1, BackgroundSprite2 };
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Time.time > lastUpdate)
        {
            lastUpdate = Time.time + 1;
        }

        for (var i = 0; i < 2; i++)
        {
            sprites[i].transform.Translate(0, -ScrollSpeed, 0);
        }

        checkBounds(0, 1);
        checkBounds(1, 0);
	}

    public void checkBounds(int spr1, int spr2)
    {
        if (sprites[spr1].transform.position.y <= -8)
            sprites[spr1].transform.position = new Vector3(sprites[spr2].transform.position.x, sprites[spr2].transform.position.y + sprites[spr2].transform.collider2D.bounds.size.y, sprites[spr2].transform.position.z);
    }
}
