namespace CampoMinadoCliente
{

    public partial class PortForm : Form
    {
        private TextBox _portTextBox;
        private Button _connectButton;

        public int Port { get; private set; }

        public PortForm()
        {
            _portTextBox = new TextBox
            {
                Location = new Point(15, 15),
                Size = new Size(100, 20)
            };
            Controls.Add(_portTextBox);

            _connectButton = new Button
            {
                Location = new Point(130, 15),
                Size = new Size(75, 23),
                Text = "Connect"
            };
            _connectButton.Click += OnConnectButtonClicked;
            Controls.Add(_connectButton);
        }

        private async void OnConnectButtonClicked(object sender, EventArgs e)
        {
            if (int.TryParse(_portTextBox.Text, out int port) && port > 0 && port < 65536)
            {
                var isConnected = await _clientConnection.ConnectAsync("localhost", port);
                if (isConnected)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Failed to connect to the server. Please try again.");
                }
            }
            else
            {
                MessageBox.Show("Invalid port number. Please enter a number between 1 and 65535.");
            }
        }

    }
}