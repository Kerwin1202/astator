using AndroidX.AppCompat.App;
using astator.Core.Script;
using astator.Modules;
using astator.Views;
using NLog;
using System.IO.Compression;

namespace astator.Pages;

public partial class LogPage : ContentPage
{
    public LogPage()
    {
        InitializeComponent();

        ScriptLogger.AddCallback("logPage", AddLogText);

        InitLogList();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Android.App.Application.Context.PackageName != Globals.AstatorPackageName)
        {
            RunProject();
        }
        else
        {
            var navBar = new NavBarView
            {
                ActiveTab = "log"
            };
            navBar.SetValue(Grid.RowProperty, 2);
            this.RootGrid.Add(navBar);
        }
    }

    private static async void RunProject()
    {
        try
        {
            var outputDir = Android.App.Application.Context.GetExternalFilesDir("project").ToString();
            if (Directory.Exists(outputDir))
            {
                var PackageUndateTime = Android.App.Application.Context.PackageManager.GetPackageInfo(Android.App.Application.Context.PackageName, 0).LastUpdateTime;

                long lastUpdateTime = 0;

                if (File.Exists(Path.Combine(outputDir, "lastUpdateTime.txt")))
                {
                    lastUpdateTime = long.Parse(File.ReadAllText(Path.Combine(outputDir, "lastUpdateTime.txt")));
                }

                if (PackageUndateTime > lastUpdateTime)
                {
                    ReleaseProject();
                }
            }
            else
            {
                ReleaseProject();
            }

            var runtime = await ScriptManager.Instance.RunProjectFromDll(outputDir);
            runtime.IsExitAppOnStoped = runtime.IsUiMode;
        }
        catch (Exception ex)
        {
            ScriptLogger.Error(ex);
        }
    }

    private static void ReleaseProject()
    {
        var outputDir = Android.App.Application.Context.GetExternalFilesDir("project").ToString();
        var apkPath = Android.App.Application.Context.PackageManager.GetApplicationInfo(Android.App.Application.Context.PackageName, 0).SourceDir;

        if (File.Exists(apkPath))
        {
            using var fs = new FileStream(apkPath, FileMode.Open, FileAccess.Read);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries)
            {
                if (entry.FullName.StartsWith("assets/Resources/"))
                {
                    var path = Path.Combine(outputDir, entry.FullName.Remove(0, 17));
                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    entry.ExtractToFile(path, true);
                }
            }
        }
    }

    private void InitLogList()
    {
        var path = Path.Combine(MauiApplication.Current.GetExternalFilesDir("log").ToString(), "log.txt");
        var logList = new List<string>();
        if (File.Exists(path))
        {
            var lines = File.ReadAllText(path).Split("logger*/");
            var maxLen = lines.Length;
            for (var i = maxLen > 100 ? maxLen - 100 : 0; i < maxLen; i++)
            {
                try
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    logList.Add($"logger*/{line}");
                    var message = line.Split("*/");
                    var Level = LogLevel.FromString(message[0]) ?? LogLevel.Debug;

                    var label = new Label
                    {
                        Text = $"{message[1]} {message[2].Trim(':')}"
                    };

                    if (Level == LogLevel.Warn)
                    {
                        label.TextColor = Color.FromRgb(0xf0, 0xdc, 0x0c);
                    }
                    else if (Level == LogLevel.Error || Level == LogLevel.Fatal)
                    {
                        label.TextColor = Colors.Red;
                    }
                    this.LogLayout.Add(label);
                }
                catch (Exception ex)
                {
                    ScriptLogger.Error(ex);
                }
            }
            File.WriteAllLines(path, logList);
            this.LogScrollView.ScrollToAsync(0, this.LogLayout.Height, false);
        }
    }

    public void AddLogText(LogLevel logLevel, DateTime time, string msg)
    {
        Globals.InvokeOnMainThreadAsync(() =>
        {
            var label = new Label
            {
                Text = $"{time:MM-dd HH:mm:ss.fff}  {msg}"
            };
            if (logLevel == LogLevel.Warn)
            {
                label.TextColor = Color.FromRgb(0xf0, 0xdc, 0x0c);
            }
            else if (logLevel == LogLevel.Error || logLevel == LogLevel.Fatal)
            {
                label.TextColor = Colors.Red;
            }
            this.LogLayout.Add(label);
            this.LogScrollView.ScrollToAsync(0, this.LogLayout.Height, false);
        });
    }

    public void Delete_Clicked(object sender, EventArgs e)
    {
        var alert = new AlertDialog
            .Builder(Globals.AppContext)
            .SetTitle("清空日志")
            .SetMessage("确认清空吗?")
            .SetPositiveButton("确认", (s, e) =>
            {
                this.LogLayout.Clear();
                var path = Path.Combine(MauiApplication.Current.GetExternalFilesDir("Log").ToString(), "log.txt");
                File.WriteAllText(path, string.Empty);
            })
            .SetNegativeButton("取消", (s, e) => { });

        alert.Show();

        //var result = await DisplayAlert("清空日志", "确认清空吗?", "确认", "取消");
        //if (result)
        //{
        //    this.LogLayout.Clear();
        //    var path = Path.Combine(MauiApplication.Current.GetExternalFilesDir("Log").ToString(), "log.txt");
        //    File.WriteAllText(path, string.Empty);
        //}

    }

    private async void LogLayout_ChildAdded(object sender, EventArgs e)
    {
        await this.LogScrollView.ScrollToAsync(this.LogScrollView, ScrollToPosition.End, true);
    }
}