using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common.GameStates
{
    /// <summary>
    /// State for handling players selecting the character they want to choose
    /// </summary>
    public class CharacterSelection : IGameSystem, IGameSubject, IGameObserver, IStateEncodable
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

        public byte PlayerCount
        {
            get;
            private set;
        }

        private DefaultGameSubject defaultGameSubject;

        public CharacterSelection()
        {
            defaultGameSubject = new DefaultGameSubject();

            PlayerCount = 0;

            PlayerSelections = new List<CharacterSelectionData>();
            for (var i = 0; i < PlayerCount; i++)
            {
                PlayerSelections.Add(new CharacterSelectionData());
            }
        }

        public void Update(int dt) { }

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

        public void AddObserver(NotificationDelegate callback)
        {
            defaultGameSubject.AddObserver(callback);
        }

        public void RemoveObserver(NotificationDelegate callback)
        {
            defaultGameSubject.RemoveObserver(callback);
        }

        public void Notify(GameEvent ge)
        {
            defaultGameSubject.Notify(ge);
        }

        public void HandleEvent(GameEvent ev)
        {
            switch (ev.Type)
            {
                case GameEvent.EventTypes.INPUT:
                    var inputEvent = (Events.GameInput)ev;

                    if (inputEvent.PlayerId >= PlayerSelections.Count)
                        break;

                    var playerSelection = PlayerSelections[inputEvent.PlayerId];

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
                case GameEvent.EventTypes.CHARACTERSELECT_ENCODE_REQUEST:
                    Notify(new GameEvent(GameEvent.EventTypes.CHARACTERSELECT_ENCODE_RESPONSE, Encode()));
                    break;
            }
        }

        public byte[] Encode()
        {
            var memory = new MemoryStream();
            var writer = new BinaryWriter(memory);

            writer.Write((byte)PlayerSelections.Count);
            for (var i = 0; i < PlayerSelections.Count; i++)
            {
                writer.Write((byte)PlayerSelections[i].SelectionId);
                writer.Write(PlayerSelections[i].IsReady);
            }

            writer.Close();
            memory.Close();
            return memory.ToArray();
        }

        public void Decode(BinaryReader reader)
        {
            PlayerSelections.Clear();

            PlayerCount = reader.ReadByte();

            for (var i = 0; i < PlayerCount; i++)
            {
                var newSelectionData = new CharacterSelectionData();
                newSelectionData.SelectionId = reader.ReadByte();
                newSelectionData.IsReady = reader.ReadBoolean();
                PlayerSelections.Add(newSelectionData);
            }
        }
    }
}
