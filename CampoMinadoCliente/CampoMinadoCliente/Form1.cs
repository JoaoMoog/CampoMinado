using System;
using System.Drawing;
using System.Windows.Forms;
using GameMessages;
using Newtonsoft.Json;

namespace CampoMinadoCliente
{
    public partial class Form1 : Form
    {
        private ClientConnection _clientConnection;
        private Button[,] _buttons;
        private int _rows = 10;
        private int _columns = 10;

        public Form1(ClientConnection clientConnection)
        {
            _clientConnection = clientConnection;
            _clientConnection.OnServerMessageReceived += HandleServerMessage;
            InitializeComponent();
            InitializeBoard();
            _clientConnection.StartReceivingMessages();
        }

        private void InitializeBoard()
        {
            _buttons = new Button[_rows, _columns];
            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _columns; col++)
                {
                    var button = new Button
                    {
                        Location = new Point(col * 30, row * 30),
                        Size = new Size(30, 30),
                        Tag = new Tuple<int, int>(row, col),
                        Font = new Font("Arial", 10, FontStyle.Bold),
                        BackColor = Color.Gray
                    };
                    button.Click += OnCellButtonClicked;
                    _buttons[row, col] = button;
                    Controls.Add(button);
                }
            }
        }

        private async void OnCellButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var button = (Button)sender;
                var position = (Tuple<int, int>)button.Tag;
                var row = position.Item1;
                var column = position.Item2;

                var moveType = "Reveal";
                if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    moveType = "Flag";
                }

                var moveMessage = new MoveResultMessage
                {
                    Action = "Move",
                    Player = 1,
                    Row = row,
                    Column = column,
                    MoveType = moveType
                };

                var messageJson = JsonConvert.SerializeObject(moveMessage);
                await _clientConnection.SendMessageAsync(messageJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleServerMessage(MoveResultMessage message)
        {
            if (message.GameOver)
            {
                string result = message.IsVictory ? "Você ganhou!" : "Você perdeu!";
                MessageBox.Show(result, "Fim de Jogo", MessageBoxButtons.OK);
                foreach (Button btn in _buttons)
                {
                    btn.Enabled = false;
                }
                return;
            }

            foreach (var cell in message.UpdatedCells)
            {
                var button = _buttons[cell.Row, cell.Column];
                if (cell.IsRevealed)
                {
                    if (cell.IsMine)
                    {
                        button.Text = "";
                        button.Image = Resources.resized_bomb;
                        button.BackColor = Color.Red;
                    }
                    else
                    {
                        button.Text = cell.AdjacentMines > 0 ? cell.AdjacentMines.ToString() : "";
                        button.BackColor = Color.LightGreen;
                    }

                    button.Enabled = false;
                }
                else if (cell.IsFlagged)
                {
                    button.Text = "";
                    button.Image = Resources.resized_flag;
                    button.BackColor = Color.Yellow;
                }
                else
                {
                    button.Text = "";
                    button.Image = null;
                    button.BackColor = Color.LightGray;
                }
            }
            if (message.IsMine)
            {
                MessageBox.Show("Você perdeu!", "Fim de Jogo", MessageBoxButtons.OK);
                foreach (Button btn in _buttons)
                {
                    btn.Enabled = false;
                }
            }


        }

    }
}
