using System;
using System.Drawing;
using System.Net.Http.Headers;
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
            _clientConnection.OnServerMessageReceived += ManipularMensagemDoServidor;
            InitializeComponent();
            IniciarTabuleiro();
            _clientConnection.IniciarRecebimentoDeMensagem();
        }

        private void IniciarTabuleiro()
        {
            _buttons = new Button[_rows, _columns];

            // Calcular a largura e altura total do tabuleiro
            int totalWidth = _columns * 30;
            int totalHeight = _rows * 30;

            // Determinar a posição inicial
            int startX = (this.ClientSize.Width - totalWidth) / 2;
            int startY = (this.ClientSize.Height - totalHeight) / 2;

            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _columns; col++)
                {
                    var button = new Button
                    {
                        // Use a posição inicial ao definir a localização de cada botão
                        Location = new Point(startX + col * 30, startY + row * 30),
                        Size = new Size(30, 30),
                        Tag = new Tuple<int, int>(row, col),
                        Font = new Font("Arial", 10, FontStyle.Bold),
                        BackColor = Color.Gray
                    };
                    button.Click += AoClicarNoBotao;
                    _buttons[row, col] = button;
                    Controls.Add(button);
                }
            }
        }


        private async void AoClicarNoBotao(object sender, EventArgs e)
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
                await _clientConnection.EnviarMensagem(messageJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ManipularMensagemDoServidor(MoveResultMessage message)
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
                        button.Image = Resources.minesweeper_resized_30x30;
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
