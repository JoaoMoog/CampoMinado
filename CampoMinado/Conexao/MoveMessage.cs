using Newtonsoft.Json;
using static GameBoard;
using System.Collections.Generic;
using System.Dynamic;
using CampoMinado;

public class MoveMessage
{
    [JsonProperty("action")]
    public string Action { get; set; }

    [JsonProperty("player")]
    public int Player { get; set; }

    [JsonProperty("row")]
    public int Row { get; set; }

    [JsonProperty("column")]
    public int Column { get; set; }

    [JsonProperty("moveType")]
    public string MoveType { get; set; }
}

public class MoveResultMessage
{
    [JsonProperty("isMine")]
    public bool IsMine { get; set; }

    [JsonProperty("gameOver")]
    public bool GameOver { get; set; }

    [JsonProperty("isVictory")]
    public bool IsVictory { get; set; }

    [JsonProperty("updatedCells")]
    public List<Cell> UpdatedCells { get; set; }

}



