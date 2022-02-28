using Android.Content;
using Microsoft.Maui.Platform;

namespace astator.TipsView;

public partial class TipsViewImpl : Grid
{
    public static readonly BindableProperty RadiusBindableProperty = BindableProperty.Create(nameof(Radius), typeof(int), typeof(TipsViewImpl), 30);
    public int Radius
    {
        get => (int)GetValue(RadiusBindableProperty);
        set => SetValue(RadiusBindableProperty, value);
    }

    public static Context AppContext { get; set; }

    private static TipsViewImpl instance;
    public static TipsViewImpl Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new TipsViewImpl();
            }
            return instance;
        }
    }

    public static void ChangeTipsText(string text)
    {
        Device.BeginInvokeOnMainThread(() => Instance.Tips.Text = text);
    }
    public static void Hide()
    {
        Device.BeginInvokeOnMainThread(() => Instance.IsVisible = false);
    }

    public static void Show()
    {
        Device.BeginInvokeOnMainThread(() => Instance.IsVisible = true);
    }


    private readonly Android.Views.View nativeView;
    private readonly AppFloatyWindow floaty;

    public TipsViewImpl()
    {
        InitializeComponent();

        this.nativeView = this.ToNative(Application.Current.MainPage.Handler.MauiContext);
        this.floaty = new AppFloatyWindow(AppContext, this.nativeView, gravity: Android.Views.GravityFlags.Center);
    }


    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        var view = this.Handler.NativeView as LayoutViewGroup;
        view.ClipToOutline = true;
        view.OutlineProvider = new RadiusOutlineProvider(this.Radius);
    }
}