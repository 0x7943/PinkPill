using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Mirai_CSharp;
using Mirai_CSharp.Extensions;
using Mirai_CSharp.Models;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using PinkPill.ImageObfuscation;
using PinkPill.Obfuscation;
using PinkPill.Utils;

namespace PinkPill.ConsoleTest
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            // this project is for internal use
            /*
            var obfuscate = new CustomBase64Obfuscator().Obfuscate(Encoding.UTF8.GetBytes(
                "一见短袖子，立刻想到白臂膊，立刻想到全裸体，立刻想到生殖器，立刻想到性交，立刻想到杂交，立刻想到私生子。" +
                "中国人的想像惟在这一层能够如此跃进。"));
            File.WriteAllText("test.txt", PostObfuscator.Obfuscate(obfuscate));
            */

            /*
            var source = Encoding.UTF8.GetBytes(new string('a', 500000));
            var image = new ImageObfuscator().Obfuscate(source);
            image.Write("t.png");
            var deobfuscate = new ImageObfuscator().Deobfuscate(image);
            if (deobfuscate.SequenceEqual(source))
            {
                Console.WriteLine("success!");
            }
            */

            var image = new ImageObfuscator().Obfuscate(new ReadOnlySpan<byte>(new byte[] { 1, 2, 3, 3, 3, 3, 3 }));
            image.Write("awffa.png");


            session = new MiraiHttpSession();

            if (Console.ReadKey().Key == ConsoleKey.A)
            {
                await ModeServer();
            }
            else
            {
                await ModeClient();

            }



            //var sourceImage = new MagickImage("a.png");

            /*
            var image = new MagickImage(new MagickColor("#000000"), 110, 20);
            image.Settings.Density = new Density(72);
            image.Settings.FontPointsize = 162/10.0;
            image.Settings.Font = "FiraCode-Regular.ttf";
            image.Settings.TextAntiAlias = false;
            image.Settings.StrokeColor = new MagickColor(255, 255, 255);
            image.Settings.FillColor = new MagickColor(255, 127, 195);
            image.Annotate("[Pink Pill]", new MagickGeometry(0, 10), Gravity.Northwest);


            */

            //image.Draw(new DrawableText(0, 100, "[Pink Pill]"));
            /*
            new Drawables()
                // Draw text on the image
                .Density(72)
                .FontPointSize(162)
                .Font("FiraCode-Regular.ttf")
                .StrokeColor(new MagickColor(255, 255, 255))
                .FillColor(new MagickColor(255,127,195))
                .TextAlignment(TextAlignment.Left)
                .TextAntialias(false)
                .TextInterwordSpacing(0)
                .Text(0, 100, "[Pink Pill]")
                .Draw(image);
            */
            //image.Write("a.png");
            //var metric = image.FontTypeMetrics("[Pink Pill]");
        }

        static long TargetQQ = 0;
        static async Task ModeServer()
        {
            var options = new MiraiHttpSessionOptions("localhost", 8080, "INITKEYc15Hx0xs");
            var selfQQ = 123;
            var otherQQ = 456;
            TargetQQ = otherQQ;

            Console.WriteLine("Act as A");

            await session.ConnectAsync(options, selfQQ);
            //await session.SendFriendMessageAsync(otherQQ, new PlainMessage(new string('1', 750)));
            var superStream = new DelayedBufferStream((_, e) => { SendMessage(new ImageObfuscator().Obfuscate(e)); });
            var serverStream = new SslStream(superStream, false,
                (sender, certificate, chain, errors) =>
                {
                    var subject = certificate.Subject;
                    Console.WriteLine($"Cert subject: {subject}");
                    return true;
                }, null);

            var cert = X509Certificate2.CreateFromEncryptedPemFile(@"C:\test\cert1.pem", "1234", @"C:\test\key1.pem");
            cert = new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
            session.FriendMessageEvt += (sender, args) =>
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
            serverStream.AuthenticateAsServer(cert, true, SslProtocols.Tls12, false);
            _ = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var buffer = new Span<byte>(new byte[1024]);
                    var i = serverStream.Read(buffer);
                    if (i > 0)
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(buffer.Slice(0, i)));
                    }
                }
            }, TaskCreationOptions.LongRunning);

            while (true)
            {
                var line = Console.ReadLine();
                serverStream.Write(Encoding.UTF8.GetBytes(line));
            }
        }

        static async Task ModeClient()
        {
            var options = new MiraiHttpSessionOptions("localhost", 8080, "INITKEYc15Hx0xs");
            var selfQQ = 456;
            var otherQQ = 123;
            TargetQQ = otherQQ;
            Console.WriteLine("Act as B");
            await session.ConnectAsync(options, selfQQ);
            /*
            var ramdom = new Random();
            var bytes = new byte[1000000];
            ramdom.NextBytes(bytes);
            SendMessage(new ImageObfuscator().Obfuscate(bytes));
            */
            var superStream = new DelayedBufferStream((_, e) => { SendMessage(new ImageObfuscator().Obfuscate(e)); });
            var serverStream = new SslStream(superStream, false,
                (sender, certificate, chain, errors) =>
                {
                    var subject = certificate.Subject;
                    Console.WriteLine($"Cert subject: {subject}");
                    return true;
                }, null);
            var cert = X509Certificate2.CreateFromEncryptedPemFile(@"C:\test\cert2.pem", "1234", @"C:\test\key2.pem");
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
            /*
            session.FriendMessageEvt += (sender, args) =>
            {
                if (args.Sender.Id == otherQQ)
                {
                    var obfs = new Base64Obfuscator();
                    
                    superStream.pipeServer.Write(obfs.Deobfuscate(args.Chain.GetPlain()));
                }

                return Task.FromResult(true);
            };*/
            serverStream.AuthenticateAsClient("", new X509Certificate2Collection(new[] { cert }), SslProtocols.Tls12, false);

            _ = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var buffer = new Span<byte>(new byte[1024]);
                    var i = serverStream.Read(buffer);
                    if (i > 0)
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(buffer.Slice(0, i)));
                    }
                }
            }, TaskCreationOptions.LongRunning);

            while (true)
            {
                var line = Console.ReadLine();
                serverStream.Write(Encoding.UTF8.GetBytes(line));
            }
        }


        public static void SendMessage(MagickImage a)
        {
            Directory.CreateDirectory("caches");
            var fileName = Path.GetFullPath(Path.Combine("caches", $"{Guid.NewGuid():D}.png"));

            a.Write(fileName, new LosslessPNGDefines());
            session.UploadPictureAsync(UploadTarget.Group, fileName).Wait();
            var imageMessage = session.UploadPictureAsync(UploadTarget.Friend, fileName).Result;
            session.SendFriendMessageAsync(TargetQQ, imageMessage).Wait();
            File.Delete(fileName);
            a.Dispose();
        }

        public class LosslessPNGDefines : IWriteDefines
        {
            public IEnumerable<IDefine> Defines { get; } = new[] { new MagickDefine("compression-level", "9") };
            public MagickFormat Format { get; } = MagickFormat.Png24;
        }

        /*
        public static void SendMessage(string a)
        {
            foreach (var s in ChunksUpto(a, 500))
            {
                session.SendFriendMessageAsync(TargetQQ, new PlainMessage(s)).Wait();
                Thread.Sleep(200);
            }

        }*/

        static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
        {
            for (var i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        public static MiraiHttpSession session { get; set; }

    }

}
