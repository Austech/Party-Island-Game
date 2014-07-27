using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.GameStates
{
    /// <summary>
    /// State for handling players selecting the character they want to choose
    /// </summary>
    public class CharacterSelection : GameState, IGameSubject, IGameObserver
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

        public List<CharacterSelectionData> PlayerSelections;
        public static int MaxSelectionOptions = 5;  //Max Available Characters to select

        public int PlayerCount
        {
            get;
            private set;
        }

        private Common.NotificationDelegate observers;

        public CharacterSelection()
        {
            PlayerCount = 0;

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
            Notify(new GameEvent(GameEvent.EventTypes.CHARACTERSELECT_PLAYER_CHANGED_SELECTION, new byte[1]{(byte)player.SelectionId}));
        }

        public void Subscribe(NotificationDelegate callback)
        {
            observers += callback;
        }

        public void Unsubscribe(NotificationDelegate callback)
        {
            observers -= callback;
        }

        public void Notify(GameEvent ge)
        {
            observers(ge);
        }

        public void HandleEvent(GameEvent ev)
        {
            switch (ev.Type)
            {
                case GameEvent.EventTypes.INPUT:
                    var inputEvent = (Events.GameInput)ev;
                    var playerSelection = PlayerSelections[inputEvent.PlayerId];

                    if (inputEvent.PlayerId >= PlayerSelections.Count)
                        break;
                    if (inputEvent.GetInput(Events.GameInput.InputTypes.PRIMARY))
                    {
                        if (!IsSelected(playerSelection.SelectionId))
                        {
                            playerSelection.IsReady = true;
                            Notify(new GameEvent(GameEvent.EventTypes.CHARACTERSELECT_PLAYER_CONFIRMED_SELECTION, new byte[1] { (byte)playerSelection.SelectionId }));
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
                case GameEvent.EventTypes.PLAYER_JOINED:
                    PlayerCount++;
                    PlayerSelections.Add(new CharacterSelectionData());
                    break;
            }
        }
    }
}
