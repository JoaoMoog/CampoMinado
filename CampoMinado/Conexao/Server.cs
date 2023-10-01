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
        private TcpListener _listener;
        private TcpClient _player1;
        private Board _board;
        private bool _running;
        private SemaphoreSlim _moveSemaphore = new SemaphoreSlim(1, 1);

        public Server(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("Aguardando jogador...");

            _player1 = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("Jogador conectado.");

            _board = new Board(10, 10, 20);

            _running = true;
            await RunGameLoop();
        }

        private MoveResultMessage ProcessMessage(MoveResultMessage message)
        {
            Board.MoveType movetype;
            Enum.TryParse(message.MoveType, out movetype);
            return _board.MakeMove(message.Row, message.Column, movetype);
        }

        private string EncodeMoveResult(MoveResultMessage result)
        {
            var resultMessage = new MoveResultMessage
            {
                IsMine = result.IsMine,
                UpdatedCells = result.UpdatedCells
            };
            return JsonConvert.SerializeObject(resultMessage);
        }

        private async Task<MoveResultMessage> ReceiveMessageAsync(TcpClient client)
        {
            if (client.Connected && client.GetStream() != null)
            {
                var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);
                var messageJson = await reader.ReadLineAsync();
                var message = JsonConvert.DeserializeObject<MoveResultMessage>(messageJson);
                return message;
            }
            throw new InvalidOperationException("Client is not connected.");
        }

        private async Task SendMessageAsync(TcpClient client, string message)
        {
            if (client.Connected && client.GetStream() != null)
            {
                var stream = client.GetStream();
                var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                await writer.WriteLineAsync(message);
                Console.WriteLine($"Message sent to client: {message}");  // Log the message
            }
            else
            {
                throw new InvalidOperationException("Client is not connected.");
            }
        }

        private async Task RunGameLoop()
        {
            try
            {
                while (_running)
                {
                    await _moveSemaphore.WaitAsync();

                    var messageFromPlayer1 = await ReceiveMessageAsync(_player1);
                    var resultFromPlayer1 = ProcessMessage(messageFromPlayer1);
                    var responseToPlayer1 = EncodeMoveResult(resultFromPlayer1);
                    await SendMessageAsync(_player1, responseToPlayer1);

                    if (_board.IsGameOver(out bool isVictory))
                    {
                        var gameOverMessage = new GameOverMessage
                        {
                            IsVictory = isVictory,
                            Winner = _board.CurrentPlayer
                        };
                        var encodedGameOverMessage = MessageEncoder.EncodeGameOverMessage(gameOverMessage);

                        await SendMessageAsync(_player1, encodedGameOverMessage);

                        _running = false;
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO Error: {ex.Message}\n{ex.StackTrace}");
                _running = false;
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket Error: {ex.Message}\n{ex.StackTrace}");
                _running = false;
            }
            finally
            {
                _moveSemaphore.Release();
            }

            // Move these lines out of the finally block
            _player1?.Close();
            _listener?.Stop();
            Console.WriteLine("Servidor encerrado.");
        }


        public void Stop()
        {
            _running = false;
        }
    }
}