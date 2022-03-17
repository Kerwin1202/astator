﻿using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using astator.Core.Accessibility;
using astator.Core.Broadcast;
using astator.Core.Graphics;
using astator.Core.Script;
using astator.Core.UI.Base;
using astator.Core.UI.Floaty;
using astator.Modules;
using astator.Pages;
using Microsoft.Maui.Platform;

namespace astator
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : MauiAppCompatActivity, IActivity
    {
        public static MainActivity Instance { get; private set; }

        public LifecycleObserver LifecycleObserver { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Instance = this;
            Globals.AppContext = this;
            TipsView.TipsViewImpl.AppContext = this;

            this.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            this.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            this.Window.SetStatusBarColor(((Color)Microsoft.Maui.Controls.Application.Current.Resources["PrimaryColor"]).ToPlatform());
            this.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightStatusBar;

            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            this.LifecycleObserver = new LifecycleObserver(this);
            this.Lifecycle.AddObserver(this.LifecycleObserver);

            var permissionHelper = new PermissionHelper(this);
            PermissionHelperer.Instance = permissionHelper;

            var permissions = new string[]
            {
                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage
            };


            permissionHelper.ReqPermission(permissions[0], result =>
            {
                if (!result)
                {
                    NotPermissionExit();
                }

                permissionHelper.ReqPermission(permissions[1], result =>
                {
                    if (!result)
                    {
                        NotPermissionExit();
                    }

                    if (OperatingSystem.IsAndroidVersionAtLeast(30) && !Android.OS.Environment.IsExternalStorageManager)
                    {
                        permissionHelper.StartActivityForResult(new Intent(Android.Provider.Settings.ActionManageAllFilesAccessPermission), result =>
                        {
                            if (OperatingSystem.IsAndroidVersionAtLeast(30) && Android.OS.Environment.IsExternalStorageManager)
                            {
                                InitializePage();
                            }
                            else
                            {
                                NotPermissionExit();
                            }
                        });
                    }
                    else
                    {
                        InitializePage();
                    }
                });
            });

            var filter = new IntentFilter();
            filter.AddAction(Intent.ActionConfigurationChanged);
            filter.AddAction(Intent.CategoryHome);

            RegisterReceiver(ScriptBroadcastReceiver.Instance, filter);
        }

        private static void NotPermissionExit()
        {
            new AndroidX.AppCompat.App.AlertDialog
               .Builder(Globals.AppContext)
               .SetTitle("错误")
               .SetMessage("请求权限失败, 应用退出!")
               .SetPositiveButton("确认", (s, e) =>
               {
                   Process.KillProcess(Process.MyPid());
               })
               .Show();
        }

        private void InitializePage()
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current.MainPage as NavigationPage;
            var tabbedPage = mainPage.RootPage as TabbedPage;
            if (this.PackageName == Globals.AstatorPackageName)
            {
                tabbedPage.Children.Add(new HomePage());
                tabbedPage.Children.Add(new LogPage());
                tabbedPage.Children.Add(new DocPage());
                tabbedPage.Children.Add(new SettingsPage());
            }
            else
            {
                tabbedPage.Children.Add(new LogPage());
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            //UnregisterReceiver(ScriptBroadcastReceiver.Instance);
            //ScriptAccessibilityService.Instance?.DisableSelf();
            //ScreenCapturer.Instance?.Dispose();
            //FloatyService.Instance?.Dispose();
            //DebugService.Instance?.Dispose();
            //base.OnSaveInstanceState(outState);
        }



        public override void Finish()
        {
            base.Finish();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        }

        protected override void OnResume()
        {
            base.OnResume();
            var mainPage = Microsoft.Maui.Controls.Application.Current.MainPage as NavigationPage;
            var tabbedPage = mainPage.RootPage as TabbedPage;
            if (mainPage.Navigation.NavigationStack.Count == 1 && tabbedPage.CurrentPage is SettingsPage settingsPage)
            {
                settingsPage.OnResume();
            }
        }

        private DateTime latestTime;

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            var result = false;

            var mainPage = Microsoft.Maui.Controls.Application.Current.MainPage as NavigationPage;
            var tabbedPage = mainPage.RootPage as TabbedPage;
            if (mainPage.Navigation.NavigationStack.Count == 1)
            {
                if (tabbedPage.CurrentPage is HomePage homePage)
                {
                    result = homePage.OnKeyDown(keyCode, e);
                }
                else if (tabbedPage.CurrentPage is DocPage docPage)
                {
                    result = docPage.OnKeyDown(keyCode, e);
                }

            }

            if (!result && keyCode == Keycode.Back)
            {
                var pages = Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.NavigationStack;
                if (pages.Count > 1)
                {
                    Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    var time = DateTime.Now;
                    if (time.Subtract(this.latestTime).TotalMilliseconds < 1000)
                    {
                        Java.Lang.JavaSystem.Exit(0);
                    }
                    else
                    {
                        this.latestTime = time;
                        Globals.Toast("再按一次返回退出应用");
                    }
                }
                return true;
            }

            return result ? result : base.OnKeyDown(keyCode, e);
        }
    }
}