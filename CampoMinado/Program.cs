using System.Diagnostics;
using System.Threading.Tasks;
using System;

namespace CampoMinadoServidor {
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.Write("Digite o número da porta: ");
            var portInput = Console.ReadLine();
            if (!int.TryParse(portInput, out int port) || port <= 0 || port >= 65536)
            {
                Console.WriteLine("Número de porta inválido. Por favor, escolha um número entre 1 e 65535.");
                return;
            }

            var server = new Server(port);
            await server.StartAsync();

        }
    }
}
