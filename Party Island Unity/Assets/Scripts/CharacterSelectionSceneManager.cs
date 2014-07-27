using UnityEngine;
using System.Collections;
using Common;

public class CharacterSelectionSceneManager : MonoBehaviour, EventReceiver{

    public EventDispatcher EDispatcher;

    private Common.Events.GameInput inputEvent;

    public GameObject[] PlayerPortraits;

    //Value to darken player portraits for players not in lobby
    public float PlayerEmptyDarken;

    public Common.GameStates.CharacterSelection CharacterSelection;

	// Use this for initialization
	void Start () 
    {
        CharacterSelection = new Common.GameStates.CharacterSelection();

        inputEvent = new Common.Events.GameInput(0);

        EDispatcher = new EventDispatcher();
        EDispatcher.RegisterReceiver(this);
        EDispatcher.RegisterReceiver(CharacterSelection);
        CharacterSelection.EDispatcher = EDispatcher;

        PartyIsland.GlobalVariables.Initialize();
        PartyIsland.GlobalVariables.Client.SetDispatcher(EDispatcher);
        PartyIsland.GlobalVariables.Client.Connect("localhost", 1337);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetAxis("Vertical") > 0)
        {
            ChangeInput((int)BoardCharacter.FacingDirections.UP, true);
            ChangeInput((int)BoardCharacter.FacingDirections.DOWN, false);
        }
        else if (Input.GetAxis("Vertical") < 0)
        {
            ChangeInput((int)BoardCharacter.FacingDirections.DOWN, true);
            ChangeInput((int)BoardCharacter.FacingDirections.UP, false);
        }
        else
        {
            ChangeInput((int)BoardCharacter.FacingDirections.UP, false);
            ChangeInput((int)BoardCharacter.FacingDirections.DOWN, false);
        }

        if (Input.GetAxis("Horizontal") < 0)
        {
            ChangeInput((int)BoardCharacter.FacingDirections.LEFT, true);
            ChangeInput((int)BoardCharacter.FacingDirections.RIGHT, false);
        }
        else if (Input.GetAxis("Horizontal") > 0)
        {
            ChangeInput((int)BoardCharacter.FacingDirections.RIGHT, true);
            ChangeInput((int)BoardCharacter.FacingDirections.LEFT, false);
        }
        else
        {
            ChangeInput((int)BoardCharacter.FacingDirections.LEFT, false);
            ChangeInput((int)BoardCharacter.FacingDirections.RIGHT, false);
        }

        //Darken Player portraits for players that are not inside lobby
        for (var i = 0; i < PlayerPortraits.Length; i++)
        {
            if (i >= CharacterSelection.PlayerCount)
                PlayerPortraits[i].renderer.material.color = new Color(PlayerEmptyDarken, PlayerEmptyDarken, PlayerEmptyDarken);
            else
            {
                switch (CharacterSelection.PlayerSelections[i].SelectionId)
                {
                    case 0:
                    default:
                        PlayerPortraits[i].renderer.material.color = new Color(1, 1, 1);
                        break;
                    case 1:
                        PlayerPortraits[i].renderer.material.color = new Color(0, 0, 1);
                        break;
                    case 2:
                        PlayerPortraits[i].renderer.material.color = new Color(1, 0, 0);
                        break;
                    case 3:
                        PlayerPortraits[i].renderer.material.color = new Color(0, 1, 0);
                        break;
                }
            }
        }

        PartyIsland.GlobalVariables.Client.Update();
	}

    public void HandleEvent(Common.Event ev)
    {
        if(ev.Data.Length > 0)
        UnityEngine.Debug.Log(ev.Type + " " + ev.Data[0]);
    }

    void ChangeInput(int id, bool value)
    {
        if (inputEvent.SetInput((Common.Events.GameInput.InputTypes)id, value))
        {
            EDispatcher.Dispatch(inputEvent);
        }
    }
}
