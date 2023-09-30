using CampoMinado;
using System;
using System.Collections.Generic;

class GameBoard
{
    public int Rows { get; }
    public int Columns { get; }
    private Cell[,] _cells;
    public int CurrentPlayer { get; private set; } = 1;


    public GameBoard(int rows, int columns, int mineCount)
    {
        Rows = rows;
        Columns = columns;
        _cells = new Cell[rows, columns];
        InitializeBoard(mineCount);
    }

    private void InitializeBoard(int mineCount)
    {
        var random = new Random();
        var placedMines = 0;

        while (placedMines < mineCount)
        {
            var row = random.Next(Rows);
            var col = random.Next(Columns);

            if (_cells[row, col] == null)
            {
                _cells[row, col] = new Cell(row, col, true);
                placedMines++;
            }
        }

        // Inicialize células não-minadas
        for (var row = 0; row < Rows; row++)
        {
            for (var col = 0; col < Columns; col++)
            {
                if (_cells[row, col] == null)
                {
                    _cells[row, col] = new Cell(row, col);
                }
            }
        }
    }


    private int GetAdjacentMineCount(int row, int column)
    {
        int mineCount = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int newRow = row + i;
                int newCol = column + j;
                if (IsInsideBoard(newRow, newCol) && (_cells[newRow, newCol]?.IsMine ?? false))
                {
                    mineCount++;
                }
            }
        }
        return mineCount;
    }

    private bool IsInsideBoard(int row, int column)
    {
        return row >= 0 && row < Rows && column >= 0 && column < Columns;
    }

    private void CascadeReveal(int row, int column, List<Cell> updatedCells)
    {
        if (!IsInsideBoard(row, column) || _cells[row, column].IsRevealed || _cells[row, column].IsFlagged)
        {
            return;
        }

        _cells[row, column].IsRevealed = true;
        updatedCells.Add(_cells[row, column]);

        if (GetAdjacentMineCount(row, column) == 0)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    CascadeReveal(row + i, column + j, updatedCells);
                }
            }
        }
    }

    public MoveResult MakeMove(int row, int column, MoveType moveType)
    {
        var updatedCells = new List<Cell>();

        switch (moveType)
        {
            case MoveType.Reveal:
                if (_cells[row, column].IsMine)
                {
                    _cells[row, column].IsRevealed = true;
                    updatedCells.Add(_cells[row, column]);
                }
                else
                {
                    CascadeReveal(row, column, updatedCells);
                }
                break;

            case MoveType.Flag:
                _cells[row, column].IsFlagged = !_cells[row, column].IsFlagged;
                updatedCells.Add(_cells[row, column]);
                break;

            default:
                throw new InvalidOperationException("Tipo de movimento inválido.");
        }

        return new MoveResult
        {
            IsMine = _cells[row, column].IsMine,
            UpdatedCells = updatedCells
        };
    }


    public bool IsGameOver(out bool isVictory)
    {
        isVictory = false;
        foreach (var cell in _cells)
        {
            if (cell.IsMine && cell.IsRevealed)
            {
                return true;  // Uma mina foi revelada, derrota
            }

            if (!cell.IsMine && !cell.IsRevealed)
            {
                return false;  // Algumas células não-minadas ainda não foram reveladas, o jogo continua
            }
        }

        isVictory = true;  // Todas as células não-minadas foram reveladas, vitória
        return true;
    }


    public enum MoveType
    {
        Reveal,
        Flag
    }

    public class MoveResult
    {
        public bool IsMine { get; set; }
        public List<Cell> UpdatedCells { get; set; }
    }
}
