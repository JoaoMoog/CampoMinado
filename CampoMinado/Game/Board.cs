
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
        // Escolha um número aleatório de minas entre 5 e 8
        var random = new Random();
        int mineCount = random.Next(5, 9);
        var placedMines = 0;

        // Inicialize células minadas
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
                    _cells[row, col] = new Cell(row, col, false);
                }
            }
        }

        // Calcule o número de minas adjacentes para cada célula
        for (var row = 0; row < Rows; row++)
        {
            for (var col = 0; col < Columns; col++)
            {
                _cells[row, col].AdjacentMines = GetAdjacentMineCount(row, col);
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

    public MoveResultMessage MakeMove(int row, int column, MoveType moveType)
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

        return new MoveResultMessage
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
}