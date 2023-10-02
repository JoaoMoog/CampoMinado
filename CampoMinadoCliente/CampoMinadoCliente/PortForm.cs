namespace CampoMinadoCliente
{
    public partial class PortForm : Form
    {
        private ClientConnection _clientConnection;

        public int Port { get; private set; }

        public PortForm(ClientConnection clientConnection)
        {
            InitializeComponent();
            _clientConnection = clientConnection;
            button1.Click += OnConnectButtonClicked;  // Atualize o evento Click para o novo botão
        }

        private async void OnConnectButtonClicked(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out int port) && port > 0 && port < 65536)  // Use textBox1 aqui
            {
                var isConnected = await _clientConnection.ConnectAsync("localhost", port);
                if (isConnected)
                {
                    DialogResult = DialogResult.OK;
                    this.Hide();

                    var form1 = new Form1(_clientConnection);
                    form1.Show();
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

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void PortForm_Load(object sender, EventArgs e)
        {

        }
    }
}