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
                // Conex�o bem-sucedida, prosseguir para abrir o tabuleiro
                await clientConnection.StartReceivingMessages();
                Application.Run(new Form(clientConnection));
            }
            else
            {
                // A conex�o falhou ou o formul�rio foi fechado sem conectar
            }
        }
    }

}
