using UnityEngine;
using System.Collections;
using Common;
public class TileScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void SetTile(Tile.TileTypes type)
    {
        foreach (Transform child in transform)
        {
            switch (type)
            {
                default:
                case Tile.TileTypes.BLUE:
                    child.renderer.enabled = true;
                    child.renderer.material.color = new Color(.5f, .5f, 1f);
                    break;
                case Tile.TileTypes.RED:
                    child.renderer.enabled = true;
                    child.renderer.material.color = new Color(1f, .5f, .5f);
                    break;
                case Tile.TileTypes.SPECIAL:
                    child.renderer.enabled = true;
                    child.renderer.material.color = new Color(.5f, 1f, .5f);
                    break;
                case Tile.TileTypes.NONE:
                    //Don't render NONE tiles
                    child.renderer.enabled = false;
                    break;
            }
        }
        if (type == Tile.TileTypes.NONE)
        {
            renderer.enabled = false;
        }
        else
        {
            renderer.enabled = true;
        }
    }
}
