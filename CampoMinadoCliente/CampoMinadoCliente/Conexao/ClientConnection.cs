using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GameMessages;
using Newtonsoft.Json;

public class ClientConnection
{
    // TcpClient usado para a comunicação com o servidor.
    private TcpClient _client = new();

    // Leitor para ler mensagens do servidor.
    private StreamReader _reader;

    // Escritor para enviar mensagens para o servidor.
    private StreamWriter _writer;

    // Evento disparado quando uma mensagem é recebida do servidor.
    public event Action<MoveResultMessage> OnServerMessageReceived;

    // Método para conectar ao servidor. Retorna 'true' se a conexão for bem-sucedida, 'false' caso contrário.
    public async Task<bool> ConnectAsync(string host, int port)
    {
        try
        {
            await _client.ConnectAsync(host, port);
            var stream = _client.GetStream();

            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar: {ex.Message}");
            return false;
        }
    }

    // Método para enviar uma mensagem para o servidor.
    public async Task EnviarMensagem(string message)
    {
        if (_writer != null && _client.Connected)
        {
            await _writer.WriteLineAsync(message);
        }
        else
        {
            throw new InvalidOperationException("Conexão não está estabelecida.");
        }
    }

    // Método para receber uma mensagem do servidor.
    public async Task<string?> ReceberMensagem()
    {
        if (_reader != null && _client.Connected)
        {
            return await _reader.ReadLineAsync();
        }
        else
        {
            throw new InvalidOperationException("Conexão não está estabelecida.");
        }
    }

    // Processa a mensagem recebida do servidor, deserializando-a e disparando o evento correspondente.
    public async Task ProcessServerMessageAsync(string message)
    {
        try
        {
            Console.WriteLine($"Mensagem recebida do servidor: {message}");

            message = message.Trim(new char[] { '\uFEFF', '\u200B' });

            var moveResultMessage = JsonConvert.DeserializeObject<MoveResultMessage>(message);
            OnServerMessageReceived.Invoke(moveResultMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    // Método que inicia o processo de escuta contínua de mensagens do servidor.
    public async Task IniciarRecebimentoDeMensagem()
    {
        try
        {
            if (_client != null)
            {
                while (_client.Connected)
                {
                    var message = await ReceberMensagem();
                    if (message != null)
                    {
                        await ProcessServerMessageAsync(message);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Conexão não está estabelecida.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
