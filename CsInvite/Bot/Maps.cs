using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Bot
{
    public enum Map
    {
        Cache,
        Cobblestone,
        Dust2,
        Inferno,
        Mirage,
        Nuke,
        Overpass,
        Train,
    }

    public static class MapExtensions
    {
        public static Map[] GetMaps(this Map map)
        {
            return new Map[] { Map.Cache, Map.Cobblestone, Map.Dust2, Map.Inferno, Map.Mirage, Map.Nuke, Map.Overpass, Map.Train };
        }
    }
}
