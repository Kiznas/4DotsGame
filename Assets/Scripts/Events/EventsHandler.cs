using ConstantValues;
using System.Collections.Generic;

namespace Events
{
    public class CellAdded { public CellLogic.Cell Cell; }
    public class NextTurn { public Enums.Team CellTeam { get; set; } }
    public class AddBots { public List<Enums.Team> Teams; }
    public class AddToNearbyCells { public CellLogic.Cell Cell; }
    public class PlayerLost { public string PlayerName; }
    public class GetTurn { public Enums.Team Team; }
    public class PrepareForNextTurn { public Enums.Team CellTeam; }
}