using UnityEngine;
using System.Collections;
using Common;
using System.IO;

public class BoardStateManager : MonoBehaviour , IGameObserver, IGameSubject
{

    public GameObject TileObjectPrefab;

    private Board board;
    private GameObject[,] tileObjects;

    private event NotificationDelegate observers;

	// Use this for initialization
	void Start () 
    {
        board = new Board(new Tile[0, 0]);

        PartyIsland.GlobalVariables.Initialize();
        PartyIsland.GlobalVariables.Client.Connect("localhost", 1337);
        PartyIsland.GlobalVariables.Client.AddObserver(this.HandleEvent);

	}

    // Update is called once per frame
    void Update()
    {
        if (tileObjects == null || tileObjects.Length == 0)
        {
            tileObjects = new GameObject[board.TileMap.GetLength(0), board.TileMap.GetLength(1)];
            for (var i = 0; i < board.TileMap.GetLength(0); i++)
            {
                for (var j = 0; j < board.TileMap.GetLength(1); j++)
                {
                    tileObjects[i, j] = (GameObject)Instantiate(TileObjectPrefab, new Vector3(i, -j * 1, 0), TileObjectPrefab.transform.rotation);
                    tileObjects[i, j].renderer.material.color = new Color(1, 0, 0);
                }
            }
        }
        for (var i = 0; i < board.TileMap.GetLength(0); i++)
        {
            for (var j = 0; j < board.TileMap.GetLength(1); j++)
            {
                switch (board.TileMap[i, j].Type)
                {
                    default:
                    case Tile.TileTypes.BLUE:
                        tileObjects[i, j].renderer.material.color = new Color(.5f, .5f, 1);
                        break;
                    case Tile.TileTypes.RED:
                        tileObjects[i, j].renderer.material.color = new Color(1, .5f, .5f);
                        break;
                    case Tile.TileTypes.SPECIAL:
                        tileObjects[i, j].renderer.material.color = new Color(.5f, 1, .5f);
                        break;
                }

                if (board.Characters.Count > 0)
                {
                    if (board.Characters[0].X == i && board.Characters[0].Y == j)
                    {
                        foreach (Transform child in tileObjects[i, j].transform)
                        {
                            if (child.tag == "Button")
                            {
                                child.renderer.material.color = new Color(1, 0, 1);
                            }
                        }
                    }
                    else
                    {
                        foreach (Transform child in tileObjects[i, j].transform)
                        {
                            if (child.tag == "Button")
                            {
                                child.renderer.material.color = new Color(1, 1, 1);
                            }
                        }
                    }
                }
            }
        }
        PartyIsland.GlobalVariables.Client.Update();
	}


    public void AddObserver(NotificationDelegate callback)
    {
        observers += callback;
    }

    public void RemoveObserver(NotificationDelegate callback)
    {
        observers -= callback;
    }

    public void Notify(GameEvent ge)
    {
        if (observers != null)
            observers(ge);
    }

    public void HandleEvent(GameEvent ev)
    {
        //Debug.Log(ev.Type);
        switch (ev.Type)
        {
            case GameEvent.EventTypes.BOARD_ENCODE_RESPONSE:
                board.Decode(new BinaryReader(new MemoryStream(ev.Data)));
                break;
        }
    }
}
