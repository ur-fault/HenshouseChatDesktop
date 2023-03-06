using System.Text.Json;
using HenshouseChat;

var client = await Client.ConnectTo("192.168.1.248");
Console.WriteLine("Connected");
var cancel = new CancellationTokenSource();

var task = Task.Run(() =>
        client.ListenAsync(msg => Console.WriteLine($"{msg.Author} >> {msg.Content}"), cancel.Token),
    cancel.Token);

Console.WriteLine("Press any key to stop");
Console.ReadKey();

cancel.Cancel();
try {
    await task;
}
catch (OperationCanceledException) { }
Console.WriteLine("Disconnected");