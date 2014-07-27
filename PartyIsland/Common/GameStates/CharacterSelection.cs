using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.GameStates
{
    /// <summary>
    /// State for handling players selecting the character they want to choose
    /// </summary>
    public class CharacterSelection : GameState
    {
        public class CharacterSelectionData
        {
            public int SelectionId;
            public bool IsReady;

            public CharacterSelectionData()
            {
                SelectionId = 0;
                IsReady = false;
            }
        }

        public enum CharacterSelectionStates
        {

        }

        public List<CharacterSelectionData> PlayerSelections;
        public static int MaxSelectionOptions = 5;  //Max Available Characters to select

        public CharacterSelection()
        {
            PlayerSelections = new List<CharacterSelectionData>();
            for (var i = 0; i < PlayerCount; i++)
            {
                PlayerSelections.Add(new CharacterSelectionData());
            }
        }

        public override void Update(float dt)
        {
        }

        private bool IsSelected(int id)
        {
            for (var i = 0; i < PlayerSelections.Count; i++)
            {
                if (PlayerSelections[i].SelectionId == id && PlayerSelections[i].IsReady)
                {
                    return true;
                }
            }
            return false;
        }

        private void SetPlayerSelection(CharacterSelectionData player, int id)
        {
            player.SelectionId = id;
            if (player.SelectionId < 0)
            {
                player.SelectionId = MaxSelectionOptions - 1;
            }
            if (player.SelectionId >= MaxSelectionOptions)
            {
                player.SelectionId = 0;
            }
            EDispatcher.Dispatch(new Event(Event.EventTypes.CHARACTERSELECT_PLAYER_CHANGED_SELECTION, new byte[1]{(byte)player.SelectionId}));
        }

        public override void HandleEvent(Event ev)
        {
            base.HandleEvent(ev);
            switch (ev.Type)
            {
                case Event.EventTypes.INPUT:
                    var inputEvent = (Events.GameInput)ev;
                    var playerSelection = PlayerSelections[inputEvent.PlayerId];

                    if (inputEvent.GetInput(Events.GameInput.InputTypes.PRIMARY))
                    {
                        if (!IsSelected(playerSelection.SelectionId))
                        {
                            playerSelection.IsReady = true;
                            EDispatcher.Dispatch(new Event(Event.EventTypes.CHARACTERSELECT_PLAYER_CONFIRMED_SELECTION, new byte[1] { (byte)playerSelection.SelectionId }));

                            
                            for (var i = 0; i < PlayerSelections.Count; i++)
                            {

                            }
                        }
                    }
                    if (inputEvent.GetInput(Events.GameInput.InputTypes.LEFT) && playerSelection.IsReady == false)
                    {
                        SetPlayerSelection(playerSelection, playerSelection.SelectionId - 1);
                    }
                    if (inputEvent.GetInput(Events.GameInput.InputTypes.RIGHT) && playerSelection.IsReady == false)
                    {
                        SetPlayerSelection(playerSelection, playerSelection.SelectionId + 1);
                    }
                    break;
                case Event.EventTypes.PLAYER_JOINED:
                    PlayerSelections.Add(new CharacterSelectionData());
                    break;
            }
        }
    }
}
