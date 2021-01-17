using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using ImageMagick;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using PinkPill.ImageObfuscation;
using PinkPill.Utils;
using Path = System.Windows.Shapes.Path;

namespace PinkPill.ChatClient
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
        }

        public static MainWindow Instance { get; private set; }


        [DllImport("kernel32")]
        static extern bool AllocConsole();

        Process miraiProcess;
        
        void ConnectMirai(object sender, RoutedEventArgs e)
        {
            var process = Process.Start(new ProcessStartInfo("jvm/bin/java.exe", "-Dfile.encoding=UTF8 -jar mcl.jar -u")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = "mirai",
                StandardOutputEncoding = Encoding.UTF8
            });
            miraiProcess = process;
            AllocConsole();
            
            process.OutputDataReceived += (o, args) =>
            {
                var argsData = args.Data;
                Console.WriteLine(argsData);
            };
            process.BeginOutputReadLine();
            
            process.StandardInput.WriteLine($"/login {SelfIDTextBox.Text} {PasswordTextBox.Password}");

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var data = Console.ReadLine();
                    miraiProcess.StandardInput.WriteLine(data);
                }
            }, TaskCreationOptions.LongRunning);
        }


        void AddMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ChatView.StackPanel.Children.Add(new ChatContentControl(message));
            });
        }

        MiraiHttpSession session;
        SslStream stream;
        long TargetQQ;

        public void UserSendMessage(string s)
        {
            stream.Write(Encoding.UTF8.GetBytes(s));
            AddMessage($"你发送了: {s}");
        }

        public void SendMessage(MagickImage a)
        {
            Directory.CreateDirectory("caches");
            var fileName = System.IO.Path.GetFullPath(System.IO.Path.Combine("caches", $"{Guid.NewGuid():D}.png"));

            a.Write(fileName, MagickFormat.Png24);
            session.UploadPictureAsync(UploadTarget.Group, fileName).Wait();
            var imageMessage = session.UploadPictureAsync(UploadTarget.Friend, fileName).Result;
            session.SendFriendMessageAsync(TargetQQ, imageMessage).Wait();

            a.Dispose();
        }

        async void HandShakeServerStart(object sender, RoutedEventArgs e)
        {
            var port = int.Parse(PortTextBox.Text);
            var options = new MiraiHttpSessionOptions("localhost", port, "PinkPill");
            var selfQQ = long.Parse(SelfIDTextBox.Text); // 自己
            var otherQQ = long.Parse(TargetIDTextBox.Text); // 对方
            TargetQQ = otherQQ;
            session = new MiraiHttpSession();
            await session.ConnectAsync(options, selfQQ);
            var superStream = new DelayedBufferStream((_, e) => { SendMessage(new ImageObfuscator().Obfuscate(e)); });
            var serverStream = new SslStream(superStream, false,
                (sender, certificate, chain, errors) =>
                {
                    var subject = certificate.Subject;
                    AddMessage($"握手完成: 证书名 {subject}");
                    return true;
                }, null);
            stream = serverStream;
            
            var cert = X509Certificate2.CreateFromPemFile(@"cert.pem");
            cert = new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
            session.FriendMessageEvt += (sender, args) =>
            {
                if (args.Sender.Id == otherQQ)
                {
                    var obfs = new ImageObfuscator();
                    var msg = args.Chain;
                    if (msg.FirstOrDefault(c => c is ImageMessage) is not ImageMessage imageMessage) 
                        return Task.FromResult(true);
                    var imageBytes = new HttpClient().GetByteArrayAsync(imageMessage.Url).Result;

                    superStream.SendMessage(obfs.Deobfuscate(new MagickImage(imageBytes, MagickFormat.Png)));
                }

                return Task.FromResult(true);
            };
            await serverStream.AuthenticateAsServerAsync(cert, true, SslProtocols.Tls12, false);
            _ = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var buffer = new Span<byte>(new byte[1024]);
                    var i = serverStream.Read(buffer);
                    if (i > 0)
                    {
                        AddMessage($"接收到了: {Encoding.UTF8.GetString(buffer.Slice(0, i))}");
                    }
                }
            }, TaskCreationOptions.LongRunning);
            
        }

        async void HandShakeClientStart(object sender, RoutedEventArgs e)
        {
            var port = int.Parse(PortTextBox.Text);
            var options = new MiraiHttpSessionOptions("localhost", port, "PinkPill");
            var selfQQ = long.Parse(SelfIDTextBox.Text); // 自己
            var otherQQ = long.Parse(TargetIDTextBox.Text); // 对方
            TargetQQ = otherQQ;
            session = new MiraiHttpSession();

            await session.ConnectAsync(options, selfQQ);
            var superStream = new DelayedBufferStream((_, e) => { SendMessage(new ImageObfuscator().Obfuscate(e)); });
            var serverStream = new SslStream(superStream, false,
                (sender, certificate, chain, errors) =>
                {
                    var subject = certificate.Subject;
                    AddMessage($"握手完成: 证书名 {subject}");
                    return true;
                }, null);
            stream = serverStream;
            var cert = X509Certificate2.CreateFromPemFile(@"cert.pem");
            cert = new X509Certificate2(cert.Export(X509ContentType.Pkcs12));


            session.FriendMessageEvt += (_, args) =>
            {
                if (args.Sender.Id == otherQQ)
                {
                    var obfs = new ImageObfuscator();
                    var msg = args.Chain;
                    if (msg.FirstOrDefault(c => c is ImageMessage) is not ImageMessage imageMessage) return Task.FromResult(true);
                    var imageBytes = new HttpClient().GetByteArrayAsync(imageMessage.Url).Result;

                    superStream.SendMessage(obfs.Deobfuscate(new MagickImage(imageBytes, MagickFormat.Png)));
                }

                return Task.FromResult(true);
            };
            await serverStream.AuthenticateAsClientAsync("", new X509Certificate2Collection(new[] { cert }), SslProtocols.Tls12, false);

            _ = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var buffer = new Span<byte>(new byte[1024]);
                    var i = serverStream.Read(buffer);
                    if (i > 0)
                    {
                        AddMessage($"接收到了: {Encoding.UTF8.GetString(buffer.Slice(0, i))}");
                    }
                }
            }, TaskCreationOptions.LongRunning);
            

        }
    }
}