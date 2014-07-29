using UnityEngine;
using System.Collections;
using Common;
using System.IO;

public class BoardSceneScript : MonoBehaviour, IGameObserver, IGameSubject
{

    public GameObject TileObjectPrefab;
    public GameObject DicePrefab;
    public GameObject BoardCharacterPrefab;

    public float CharacterYPositionOffset;

    private Board board;
    private GameObject[,] tileObjects;
    private GameObject[] characterObjects;
    private GameObject diceObject;

    private event NotificationDelegate observers;

    // Use this for initialization
    void Start()
    {
        board = new Board(new Tile[10, 10]);
        for (var i = 0; i < board.TileMap.GetLength(0); i++)
        {
            for (var j = 0; j < board.TileMap.GetLength(1); j++)
            {
                board.TileMap[i, j] = new Tile(i, j, Tile.TileTypes.BLUE);
                if (i % 3 == 0 && j % 3 == 0)
                {
                    board.TileMap[i, j].Type = Tile.TileTypes.SPECIAL;
                }
                else if (i % 3 == 0)
                {
                    board.TileMap[i, j].Type = Tile.TileTypes.RED;
                }
            }
        }
        board.Characters.Add(new BoardCharacter());
        board.Characters[0].X = 1;
        board.Characters[0].Y = 2;

        board.AddObserver(HandleEvent);
        board.HandleEvent(new GameEvent(GameEvent.EventTypes.BOARD_ENCODE_REQUEST, new byte[0]));
    }

    // Update is called once per frame
    void Update()
    {
        //Set board character positions
        if (characterObjects != null)
        {
            for (var i = 0; i < characterObjects.Length; i++)
            {
                characterObjects[i].transform.localPosition = new Vector3(board.Characters[i].X, CharacterYPositionOffset, board.TileMap.GetLength(1) - board.Characters[i].Y);
            }
        }

        switch (board.State)
        {
            case Board.BoardStates.WAITING_FOR_ROLL:
                if (diceObject == null)
                {
                    diceObject = (GameObject)Instantiate(DicePrefab, new Vector3(0, 0, 0), DicePrefab.transform.rotation);
                    diceObject.transform.parent = this.transform;
                }

                if(characterObjects != null)
                    diceObject.transform.localPosition = characterObjects[board.CurrentPlayer].transform.localPosition + new Vector3(0, 1, 0);

                break;
        }
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

    private void LoadBoardObjects()
    {
        tileObjects = new GameObject[board.TileMap.GetLength(0), board.TileMap.GetLength(1)];
        for (var i = 0; i < tileObjects.GetLength(0); i++)
        {
            for (var j = 0; j < tileObjects.GetLength(1); j++)
            {
                tileObjects[i, j] = (GameObject)Instantiate(TileObjectPrefab, new Vector3(i, 0, tileObjects.GetLength(1) - j), TileObjectPrefab.transform.rotation);
                tileObjects[i, j].SendMessage("SetTile", board.TileMap[i, j].Type);
                tileObjects[i, j].transform.parent = this.transform;
            }
        }

        characterObjects = new GameObject[board.Characters.Count];
        for (var i = 0; i < characterObjects.Length; i++)
        {
            characterObjects[i] = (GameObject)Instantiate(BoardCharacterPrefab, new Vector3(0, 0, 0), BoardCharacterPrefab.transform.rotation);
            characterObjects[i].transform.parent = this.transform;
        }
    }

    public void HandleEvent(GameEvent ev)
    {
        //Debug.Log(ev.Type);
        switch (ev.Type)
        {
            case GameEvent.EventTypes.BOARD_ENCODE_RESPONSE:
                board.Decode(new BinaryReader(new MemoryStream(ev.Data)));

                if (tileObjects == null || board.TileMap.Length > tileObjects.Length)  //Board tiles are different, reset map
                {
                    LoadBoardObjects();
                }
                break;
        }
    }
}
