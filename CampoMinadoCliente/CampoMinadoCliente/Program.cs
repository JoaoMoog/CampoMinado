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

        Application.Run(new PortForm(clientConnection));
    }


}
