using UnityEngine;
using System.Collections;
using Common;
using System.IO;
using Assets.Scripts;

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

    private InputSubject inputSubject;

    private DefaultGameSubject defaultGameObject;

    private Assets.Scripts.PlayMode playMode;

    // Use this for initialization
    void Start()
    {
        defaultGameObject = new DefaultGameSubject();

        inputSubject = new InputSubject();
        board = new Board(new Tile[0, 0]);

        playMode = new MultiplayerModeSubject("localhost", 1337);
        playMode.AddObserver(HandleEvent);

        inputSubject.AddObserver(playMode.HandleEvent);
    }

    // Update is called once per frame
    void Update()
    {
        inputSubject.Update();

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

        if (board.State != Board.BoardStates.WAITING_FOR_ROLL)
        {
            if (diceObject != null)
            {
                DestroyObject(diceObject);
                diceObject = null;
            }
        }

        board.Update();

        if (playMode != null)
        {
            playMode.Update();
        }
    }


    public void AddObserver(NotificationDelegate callback)
    {
        defaultGameObject.AddObserver(callback);
    }

    public void RemoveObserver(NotificationDelegate callback)
    {
        defaultGameObject.RemoveObserver(callback);
    }

    public void Notify(GameEvent ge)
    {
        defaultGameObject.Notify(ge);
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

    private void RollDice()
    {
        //Destroy the dice object
        if (diceObject != null)
        {
            DestroyObject(diceObject);
            diceObject = null;
        }

        //Make effects and play sounds here

    }

    public void HandleEvent(GameEvent ev)
    {
        switch (ev.Type)
        {
            case GameEvent.EventTypes.BOARD_ENCODE_RESPONSE:
                board.Decode(new BinaryReader(new MemoryStream(ev.Data)));

                if (tileObjects == null || board.TileMap.Length > tileObjects.Length)  //Board tiles are different, reset map
                {
                    LoadBoardObjects();
                }
                break;

            case GameEvent.EventTypes.BOARD_PLAYER_ROLLED:
                RollDice();
                break;

            case GameEvent.EventTypes.PLAYER_ID_RESPONSE:
                inputSubject.SetPlayerId(ev.Data[0]);
                break;
        }
    }
}
