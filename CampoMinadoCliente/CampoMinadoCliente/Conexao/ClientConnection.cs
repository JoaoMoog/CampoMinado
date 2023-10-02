using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GameMessages;
using Newtonsoft.Json;

public class ClientConnection
{
    private TcpClient _client = new();  // Inicialização direta
    private StreamReader _reader;
    private StreamWriter _writer;
    public event Action<MoveResultMessage> OnServerMessageReceived;  // Indicando que o evento pode ser nulo

    public async Task<bool> ConnectAsync(string host, int port)
    {
        try
        {
            await _client.ConnectAsync(host, port);
            var stream = _client.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            return true;  // Conexão bem-sucedida
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar: {ex.Message}");
            return false;  // Falha na conexão
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (_writer != null && _client.Connected)
        {
            await _writer.WriteLineAsync(message);
        }
        else
        {
            throw new InvalidOperationException("Connection is not established.");
        }
    }

    public async Task<string?> ReceiveMessageAsync()  // Indicando que o método pode retornar nulo
    {
        if (_reader != null && _client.Connected)
        {
            return await _reader.ReadLineAsync();
        }
        else
        {
            throw new InvalidOperationException("Connection is not established.");
        }
    }

    public async Task ProcessServerMessageAsync(string message)
    {
        var moveResultMessage = JsonConvert.DeserializeObject<MoveResultMessage>(message);
        OnServerMessageReceived.Invoke(moveResultMessage);
    }

    public async Task StartReceivingMessages()
    {
        try
        {
            if (_client != null)
            {
                while (_client.Connected)
                {
                    Console.WriteLine("Waiting for server message...");
                    var message = await ReceiveMessageAsync();
                    if (message != null)
                    {
                        await ProcessServerMessageAsync(message);
                    }
                    else
                    {
                        // O servidor fechou a conexão
                        break;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Connection is not established.");
            }
        }
        catch (Exception ex)
        {
            // Trate quaisquer exceções que ocorram
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
