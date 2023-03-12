using System.Windows;

namespace HenshouseChatDesktop.ViewModels;

public class ServerConnectionInfoViewModel
{
    public string Address { get; set; } = "";

    public string Port { get; set; } = "25017";

    public override string ToString() {
        return $"{Address}:{Port}";
    }
}