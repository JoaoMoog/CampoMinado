// Program.cs
using CampoMinadoCliente;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

static class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var clientConnection = new ClientConnection();

        using (var portForm = new PortForm(clientConnection))
        {
            var dialogResult = portForm.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                // Conexão bem-sucedida, prosseguir para abrir o tabuleiro
                await clientConnection.StartReceivingMessages();
                Application.Run(new Form(clientConnection));
            }
            else
            {
                // A conexão falhou ou o formulário foi fechado sem conectar
            }
        }
    }

}
