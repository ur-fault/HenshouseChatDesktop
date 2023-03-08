using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HenshouseChat;

namespace HenshouseChatDesktop
{
    public partial class MainWindow : Window
    {
        Thread _listenThread;

        public MainWindow() {
            InitializeComponent();
            //_listenThread = new Thread(async () => {
            //    try {
            //        var client = await Client.ConnectTo("192.168.1.248");
            //        await client.ListenAsync(msg => { MessageBox.Show(msg.Content, msg.Author); });
            //    }
            //    catch (Exception e) {
            //        MessageBox.Show(e.Message);
            //    }
            //});
            //_listenThread.Start();
        }
    }
}