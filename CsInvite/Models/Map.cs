using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Models
{
    public enum Map
    {
        None,
        Cache,
        Cobblestone,
        Dust2,
        Inferno,
        Mirage,
        Nuke,
        Overpass,
        Train,

        Canals,
        Office,
        Vertigo,
        Dust1,
        Assault,
        Militia,
        Aztec,
        Italy,

        Season,
        Santorini,
        Subzero
    }

    public static class Maps
    {
        public static Map[] SeriousMaps
        { get; private set; } = { Map.Cache, Map.Cobblestone, Map.Dust2, Map.Inferno, Map.Mirage, Map.Nuke, Map.Overpass, Map.Train };

        public static Map[] ActiveMaps
        { get; private set; } = { Map.Cache, Map.Cobblestone, Map.Inferno, Map.Mirage, Map.Nuke, Map.Overpass, Map.Train };

        public static Map[] Hostage
        { get; private set; } = { Map.Office, Map.Italy, Map.Assault, Map.Militia };
    }
}
