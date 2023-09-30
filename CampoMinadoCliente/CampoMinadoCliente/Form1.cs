using System;
using System.Drawing;
using System.Windows.Forms;
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
                        BackColor = Color.LightGray
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
                Console.WriteLine("OnCellButtonClicked chamado.");
                var button = (Button)sender;
                var position = (Tuple<int, int>)button.Tag;
                var row = position.Item1;
                var column = position.Item2;

                var moveType = "Reveal";
                if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    moveType = "Flag";
                }

                var moveMessage = new MoveMessage
                {
                    Action = "Move",
                    Player = 1,  // Fixed player number to 1 as it's a single player game
                    Row = row,
                    Column = column,
                    MoveType = moveType
                };

                var messageJson = JsonConvert.SerializeObject(moveMessage);
                Console.WriteLine($"Enviando mensagem: {messageJson}");
                await _clientConnection.SendMessageAsync(messageJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleServerMessage(MoveResultMessage message)
        {
            foreach (var cell in message.UpdatedCells)
            {
                var button = _buttons[cell.Row, cell.Column];
                if (cell.IsRevealed)
                {
                    button.Text = cell.IsMine ? "F" : "X";
                    button.Enabled = false;
                    button.BackColor = cell.IsMine ? Color.Black : Color.Red;
                }
                if (cell.IsFlagged)
                {
                    button.Text = "T";
                    button.BackColor = Color.Yellow;
                }
            }

            if (message.GameOver)
            {
                string result = message.IsVictory ? "Victory!" : "Defeat!";
                MessageBox.Show(result, "Game Over", MessageBoxButtons.OK);
                foreach (Button btn in _buttons)
                {
                    btn.Enabled = false;
                }
            }
        }
    }
}
