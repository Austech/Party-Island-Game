using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace PartyIsland
{
    public class GlobalVariables
    {
        static public GameClient Client;

        public static void Initialize()
        {
            if (Client == null)
                Client = new GameClient();
        }
    }
}
