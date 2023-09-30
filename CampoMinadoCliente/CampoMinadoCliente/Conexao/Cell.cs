// Namespace comum para ambas as classes
using System.Collections.Generic;

namespace CampoMinado
{
    // Definição da classe Cell
    public class Cell
    {
        public int Row { get; }
        public int Column { get; }
        public bool IsMine { get; }
        public bool IsRevealed { get; set; }
        public bool IsFlagged { get; set; }

        // Construtor para inicializar as propriedades
        public Cell(int row, int column, bool isMine = false)
        {
            Row = row;
            Column = column;
            IsMine = isMine;
        }
    }

    // Definição da classe MoveMessage
    public class MoveMessage
    {
        public string Action { get; set; }
        public int Player { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public string MoveType { get; set; }
    }

    // Definição da classe MoveResultMessage
    public class MoveResultMessage
    {
        public bool IsMine { get; set; }
        public bool GameOver { get; set; }
        public bool IsVictory { get; set; }
        public List<Cell> UpdatedCells { get; set; }  // Agora Cell é reconhecido aqui
    }
}
