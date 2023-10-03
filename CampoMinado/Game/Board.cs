
using GameMessages;
using System;
using System.Collections.Generic;

class Board
{
    public int Rows { get; }
    public int Columns { get; }
    private Cell[,] _cells;
    public int CurrentPlayer { get; private set; } = 1;


    public Board(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _cells = new Cell[rows, columns];
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        var random = new Random();
        int mineCount = random.Next(13, 18);
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

        for (var row = 0; row < Rows; row++)
        {
            for (var col = 0; col < Columns; col++)
            {
                if (_cells[row, col] == null)
                {
                    _cells[row, col] = new Cell(row, col, false);
                }
            }
        }

        for (var row = 0; row < Rows; row++)
        {
            for (var col = 0; col < Columns; col++)
            {
                _cells[row, col].AdjacentMines = BuscarMinasRestantes(row, col);
            }
        }
    }


    private int BuscarMinasRestantes(int row, int column)
    {
        int mineCount = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int newRow = row + i;
                int newCol = column + j;
                if (EstaDentroDoTabuleiro(newRow, newCol) && (_cells[newRow, newCol]?.IsMine ?? false))
                {
                    mineCount++;
                }
            }
        }
        return mineCount;
    }

    private bool EstaDentroDoTabuleiro(int row, int column)
    {
        return row >= 0 && row < Rows && column >= 0 && column < Columns;
    }

    private void RevelacaoEmCascata(int row, int column, List<Cell> updatedCells)
    {
        if (!EstaDentroDoTabuleiro(row, column) || _cells[row, column].IsRevealed || _cells[row, column].IsFlagged)
        {
            return;
        }

        _cells[row, column].IsRevealed = true;
        updatedCells.Add(_cells[row, column]);

        if (BuscarMinasRestantes(row, column) == 0)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    RevelacaoEmCascata(row + i, column + j, updatedCells);
                }
            }
        }
    }

    public MoveResultMessage FazerMovimento(int row, int column, MoveType moveType)
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
                    RevelacaoEmCascata(row, column, updatedCells);
                }
                break;

            case MoveType.Flag:
                _cells[row, column].IsFlagged = !_cells[row, column].IsFlagged;
                updatedCells.Add(_cells[row, column]);
                break;

            default:
                throw new InvalidOperationException("Tipo de movimento inválido.");
        }

        return new MoveResultMessage
        {
            IsMine = moveType == MoveType.Reveal && _cells[row, column].IsMine,
            UpdatedCells = updatedCells
        };
    }


    public bool GameOver(out bool isVictory)
    {
        isVictory = false;
        foreach (var cell in _cells)
        {
            if (cell.IsMine && cell.IsRevealed)
            {
                return true; 
            }

            if (!cell.IsRevealed)
            {
                return false; 
            }
        }

        isVictory = true;
        return true;
    }


    public enum MoveType
    {
        Reveal,
        Flag
    }
}