using System.Windows;

namespace HenshouseChatDesktop.ViewModels;

public class ServerConnectionInfoViewModel
{
    public string Domain { get; set; } = "192.168.1.248";

    public string Port { get; set; } = "25017";

    public override string ToString() {
        return $"{Domain}:{Port}";
    }
}