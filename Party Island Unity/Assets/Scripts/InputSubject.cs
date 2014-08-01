using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using UnityEngine;

namespace Assets.Scripts
{
    public class InputSubject: IGameSubject
    {
        private DefaultGameSubject defaultGameSubject;

        private Common.Events.GameInput inputEvent;

        public InputSubject()
        {
            defaultGameSubject = new DefaultGameSubject();
            inputEvent = new Common.Events.GameInput(0);
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

        public void SetPlayerId(byte id)
        {
            inputEvent.PlayerId = id;
        }

        public void Update()
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

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeInput((int)Common.Events.GameInput.InputTypes.PRIMARY, true);
            }
            else
            {
                ChangeInput((int)Common.Events.GameInput.InputTypes.PRIMARY, false);
            }
        }

        void ChangeInput(int id, bool value)
        {
            if (inputEvent.SetInput((Common.Events.GameInput.InputTypes)id, value))
            {
                Notify(inputEvent);
            }
        }
    }
}
