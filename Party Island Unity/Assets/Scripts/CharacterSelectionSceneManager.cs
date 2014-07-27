using UnityEngine;
using System.Collections;
using Common;

public class CharacterSelectionSceneManager : MonoBehaviour, EventReceiver{

    public EventDispatcher EDispatcher;

    private Common.Events.GameInput inputEvent;

	// Use this for initialization
	void Start () 
    {
        inputEvent = new Common.Events.GameInput(0);

        EDispatcher = new EventDispatcher();

        EDispatcher.RegisterReceiver(this);
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

        PartyIsland.GlobalVariables.Client.Update();
	}

    public void HandleEvent(Common.Event ev)
    {
    }

    void ChangeInput(int id, bool value)
    {
        if (inputEvent.SetInput((Common.Events.GameInput.InputTypes)id, value))
        {
            EDispatcher.Dispatch(inputEvent);
        }
    }
}
