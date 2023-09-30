using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using static GameBoard;

class Server
{
    private TcpListener _listener;
    private TcpClient _player1;
    private GameBoard _board;
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

        _board = new GameBoard(10, 10, 20);

        _running = true;
        await RunGameLoop();
    }

    private async Task<MoveMessage> ReceiveMessageAsync(TcpClient client)
    {
        var stream = client.GetStream();
        var reader = new StreamReader(stream, Encoding.UTF8);
        var messageJson = await reader.ReadLineAsync();
        var message = JsonConvert.DeserializeObject<MoveMessage>(messageJson);
        return message;
    }

    private MoveResult ProcessMessage(MoveMessage message)
    {
        return _board.MakeMove(message.Row, message.Column, (GameBoard.MoveType)Enum.Parse(typeof(GameBoard.MoveType), message.MoveType));
    }

    private string EncodeMoveResult(MoveResult result)
    {
        var resultMessage = new MoveResultMessage
        {
            IsMine = result.IsMine,
            UpdatedCells = result.UpdatedCells
        };
        return JsonConvert.SerializeObject(resultMessage);
    }

    private async Task SendMessageAsync(TcpClient client, string message)
    {
        var stream = client.GetStream();
        var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        await writer.WriteLineAsync(message);
    }

    private async Task RunGameLoop()
    {
        while (_running)
        {
            try
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
            catch (IOException ex)
            {
                Console.WriteLine("Erro de IO: " + ex.Message);
                _running = false;
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Erro de Socket: " + ex.Message);
                _running = false;
            }
            finally
            {
                _moveSemaphore.Release();
            }
        }

        _player1?.Close();
        _listener?.Stop();
        Console.WriteLine("Servidor encerrado.");
    }

    public void Stop()
    {
        _running = false;
    }
}
