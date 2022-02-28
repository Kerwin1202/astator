﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Graphics.Drawable;
using astator.Modules.Base;
using astator.Core.Graphics;
using astator.Core.Script;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using OperationCanceledException = System.OperationCanceledException;

namespace astator.Modules;

[Service(Label = ".debug", ForegroundServiceType = ForegroundService.TypeNone, Enabled = true)]
internal class DebugService : Service, IDisposable
{
    private static DebugService instance;
    public static DebugService Instance
    {
        get => instance;
        private set => instance = value;
    }

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        Instance = this;
        StartNotification();

        var mode = intent.GetIntExtra("mode", 0);
        var ip = intent.GetStringExtra("ip");

        if (mode == 0)
            StartServer(ip);
        else
            ConnectServer(ip);

        return base.OnStartCommand(intent, flags, startId);
    }

    public override IBinder OnBind(Intent intent)
    {
        throw new NotImplementedException();
    }

    private TcpListener tcpListener;
    private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

    public async void StartServer(string ip)
    {
        try
        {
            this.tcpListener = new TcpListener(IPAddress.Parse(ip), 1024);
            this.tcpListener.Start();
            ScriptLogger.Log($"开启调试服务: {ip}");
            while (true)
            {
                var client = await this.tcpListener.AcceptTcpClientAsync(this.cancelTokenSource.Token);
                ConnectAsync(client, this.cancelTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            ScriptLogger.Log("停止调试服务");
        }
        catch (Exception ex)
        {
            ScriptLogger.Error(ex.Message);
            Dispose();
        }
    }

    public void ConnectServer(string ip)
    {
        try
        {
            var client = new TcpClient(ip, 1025);
            ScriptLogger.Log($"连接到电脑: {ip}");
            ConnectAsync(client, this.cancelTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            ScriptLogger.Log("停止调试服务");
        }
        catch (Exception ex)
        {
            ScriptLogger.Error(ex.Message);
            Dispose();
        }
    }

    private static void ConnectAsync(TcpClient client, CancellationToken cancelToken)
    {
        _ = Task.Run(async () =>
        {
            var stream = client.GetStream();
            var info = $"{Globals.Devices.Brand} {Globals.Devices.Model}";
            await stream.WriteAsync(Stick.MakePackData("init", info));

            var key = ScriptLogger.AddCallback("info", async (args) =>
            {
                var pack = Stick.MakePackData("showMessage", Encoding.UTF8.GetBytes($"{args.Time:HH:mm:ss.fff}: {args.Message}"));
                await stream.WriteAsync(pack);
            });

            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    Thread.Sleep(50);
                    var data = await Stick.ReadPackAsync(stream, cancelToken);

                    switch (data.Key)
                    {
                        case "runProject":
                        {
                            using var zipStream = new MemoryStream(data.Buffer);
                            var directory = Path.Combine(MauiApplication.Current.ExternalCacheDir.ToString(), data.Description);
                            ClearProject(directory);

                            using (var archive = new ZipArchive(zipStream))
                            {
                                archive.ExtractToDirectory(directory, true);
                            }

                            _ = ScriptManager.Instance.RunProject(directory);

                            break;
                        }
                        case "runScript":
                        {
                            using var zipStream = new MemoryStream(data.Buffer);
                            var description = data.Description.Split("|");
                            var directory = Path.Combine(MauiApplication.Current.ExternalCacheDir.ToString(), description[0]);
                            ClearProject(directory);

                            using (var archive = new ZipArchive(zipStream))
                            {
                                archive.ExtractToDirectory(directory, true);
                            }

                            _ = ScriptManager.Instance.RunScript(Path.Combine(directory, description[1]));

                            break;
                        }
                        case "saveProject":
                        {
                            var astatorDir = Android.OS.Environment.GetExternalStoragePublicDirectory("astator").ToString();
                            var directory = Path.Combine(astatorDir, "脚本");
                            if (Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            using var zipStream = new MemoryStream(data.Buffer);
                            var saveDirectory = Path.Combine(directory, data.Description);
                            ClearProject(saveDirectory);

                            using var archive = new ZipArchive(zipStream);
                            archive.ExtractToDirectory(saveDirectory, true);
                            Globals.Toast($"项目已保存至{saveDirectory}");
                            break;
                        }
                        case "screenShot":
                        {
                            byte[] pack;

                            if (ScreenCapturer.Instance is not null)
                            {
                                try
                                {
                                    var img = ScreenCapturer.Instance.AcquireLatestBitmap();

                                    var ms = new MemoryStream();
                                    img.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, ms);
                                    var bytes = ms.ToArray();

                                    pack = Stick.MakePackData("screenShot_success", bytes);
                                }
                                catch (Exception)
                                {
                                    pack = Stick.MakePackData("screenShot_fail", "获取截图失败!");
                                }
                            }
                            else
                            {
                                pack = Stick.MakePackData("screenShot_fail", "截图服务未开启!");
                            }


                            await stream.WriteAsync(pack);
                            break;
                        }
                        case "heartBeat":
                        {
                            var pack = Stick.MakePackData("heartBeat");
                            await stream.WriteAsync(pack);
                            break;
                        }
                        default:
                            break;
                    }
                }
            }
            catch { }
            finally
            {
                client.Close();
                ScriptLogger.RemoveCallback(key);
            }
        }, cancelToken);
    }


    private static void ClearProject(string dir)
    {
        if (!Directory.Exists(dir))
        {
            return;
        }

        var searchPatterns = new string[] { "*.cs", "*.csproj" };

        foreach (var searchPattern in searchPatterns)
        {
            var files = Directory.GetFiles(dir, searchPattern, SearchOption.AllDirectories);
            if (files.Any())
            {
                foreach (var f in files)
                {
                    File.Delete(f);
                }
            }
        }
    }

    private void StartNotification()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            NotificationChannel channel = new("1002", "调试服务", NotificationImportance.Default);
            notificationManager.CreateNotificationChannel(channel);
        }

        var notification = new NotificationCompat.Builder(this, "1002")
          .SetContentTitle("调试服务正在运行中")
          .SetSmallIcon(IconCompat.CreateWithResource(this, Android.Resource.Drawable.SymDefAppIcon))
          .Build();

        StartForeground(1002, notification);
    }


    private bool disposedValue;

    protected new virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.cancelTokenSource?.Cancel();
                this.tcpListener?.Stop();
                this.tcpListener = null;
                this.disposedValue = true;
                StopSelf();
                Instance = null;
            }
        }
    }

    ~DebugService()
    {
        Dispose(disposing: false);
    }

    public new void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
