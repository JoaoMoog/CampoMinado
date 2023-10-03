using GameMessages;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace CampoMinadoServidor
{
    class Server
    {
        // Ouvinte para aceitar conexões TCP de clientes.
        private TcpListener _listener;

        // Representa o jogador 1 conectado ao servidor.
        private TcpClient _player1;

        // Representa o tabuleiro do jogo.
        private Board _board;

        // Flag para determinar se o jogo está em execução.
        private bool _running;

        // Semáforo para sincronizar as jogadas.
        private SemaphoreSlim _moveSemaphore = new SemaphoreSlim(1, 1);

        // Construtor do servidor, define em qual porta o servidor irá ouvir.
        public Server(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        // Método para iniciar o servidor e aceitar um jogador.
        public async Task Iniciar()
        {
            _listener.Start();
            Console.WriteLine("Aguardando jogador...");

            _player1 = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("Jogador conectado.");

            _board = new Board(10, 10);

            _running = true;
            await RunGameLoop();
        }

        // Processa a mensagem de movimento do jogador e executa o movimento no tabuleiro.
        private MoveResultMessage ProcessarMensagem(MoveResultMessage message)
        {
            Board.MoveType movetype;
            Enum.TryParse(message.MoveType, out movetype);
            return _board.FazerMovimento(message.Row, message.Column, movetype);
        }

        // Serializa a resposta do movimento para ser enviada de volta ao jogador.
        private string SerializarMensagem(MoveResultMessage result)
        {
            var resultMessage = new MoveResultMessage
            {
                IsMine = result.IsMine,
                UpdatedCells = result.UpdatedCells
            };
            return JsonConvert.SerializeObject(resultMessage);
        }

        // Lê uma mensagem do cliente.
        private async Task<MoveResultMessage> MensagemRecebida(TcpClient client)
        {
            if (client.Connected && client.GetStream() != null)
            {
                var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);
                var messageJson = await reader.ReadLineAsync();
                Console.WriteLine($"Mensagem recebida do cliente: {messageJson}");
                var message = JsonConvert.DeserializeObject<MoveResultMessage>(messageJson);
                return message;
            }
            throw new InvalidOperationException("Cliente não conectado");
        }

        // Envia uma mensagem para o cliente.
        private async Task EnviarMensagem(TcpClient client, string message)
        {
            if (client.Connected && client.GetStream() != null)
            {
                var stream = client.GetStream();
                var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                await writer.WriteLineAsync(message);
                Console.WriteLine($"Mensagem enviada para o cliente: {message}");
            }
            else
            {
                throw new InvalidOperationException("O cliente não está conectado");
            }
        }

        // Loop principal do jogo, onde as mensagens são processadas e as respostas são enviadas aos jogadores.
        private async Task RunGameLoop()
        {
            try
            {
                while (_running && _player1.Connected)
                {
                    await _moveSemaphore.WaitAsync();

                    var messageFromPlayer1 = await MensagemRecebida(_player1);
                    var resultFromPlayer1 = ProcessarMensagem(messageFromPlayer1);
                    var responseToPlayer1 = SerializarMensagem(resultFromPlayer1);
                    await EnviarMensagem(_player1, responseToPlayer1);

                    if (_board.GameOver(out bool isVictory))
                    {
                        var gameOverMessage = new GameOverMessage
                        {
                            IsVictory = isVictory,
                            GameOver = true,
                            Winner = _board.CurrentPlayer
                        };
                        var encodedGameOverMessage = MessageEncoder.SerializarGameOverMensagem(gameOverMessage);

                        await EnviarMensagem(_player1, encodedGameOverMessage);

                        _running = false;
                    }
                    _moveSemaphore.Release();
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Erro: {ex.Message}\n{ex.StackTrace}");
                _running = false;
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket Error: {ex.Message}\n{ex.StackTrace}");
                _running = false;
            }

            _player1?.Close();
            _listener?.Stop();
            Console.WriteLine("Servidor encerrado.");
        }
    }
}
