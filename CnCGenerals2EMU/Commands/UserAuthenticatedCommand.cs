﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazeLibWV;
using System.Net.Sockets;

namespace CnCGenerals2EMU
{
    class UserAuthenticatedCommand
    {
        public static List<Blaze.Tdf> UserAuthenticated(PlayerInfo pi)
        {
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            Result.Add(Blaze.TdfInteger.Create("ALOC", 1403663841));
            Result.Add(Blaze.TdfInteger.Create("BUID", pi.userId));
            Result.Add(Blaze.TdfString.Create("DSNM", "Player1"));
            Result.Add(Blaze.TdfInteger.Create("FRSC", 0));
            Result.Add(Blaze.TdfInteger.Create("FRST", 0));
            Result.Add(Blaze.TdfString.Create("KEY", "SESSKY"));
            Result.Add(Blaze.TdfInteger.Create("LAST", 1403663841));
            Result.Add(Blaze.TdfInteger.Create("LLOG", 1403663841));
            Result.Add(Blaze.TdfString.Create("MAIL", "cnc.server.pc@ea.com"));
            Result.Add(Blaze.TdfInteger.Create("PID", pi.userId));
            Result.Add(Blaze.TdfInteger.Create("PLAT", 4));
            Result.Add(Blaze.TdfInteger.Create("UID", pi.userId));
            Result.Add(Blaze.TdfInteger.Create("USTP", 0));
            Result.Add(Blaze.TdfInteger.Create("XREF", 0));

            return Result;
        }
    }
}
