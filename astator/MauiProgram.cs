﻿
using astator.Pages;
using astator.Views;
using Microsoft.Maui.Controls.Compatibility;

namespace astator;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("CascadiaCode-SemiLight.ttf", "CascadiaCode");
            })
            //.UseMauiCompatibility()
            .ConfigureMauiHandlers(handler =>
            {
                handler.AddCompatibilityRenderer(typeof(CustomLabelButton), typeof(CustomLabelButtonRenderer));
                handler.AddHandler(typeof(CustomSpinner), typeof(CustomSpinnerHandler));
                handler.AddHandler(typeof(CodeEditorView), typeof(CodeEditorViewHandler));
                handler.AddCompatibilityRenderer(typeof(TabbedPage), typeof(CustomTabbedPageRenderer));
                handler.AddHandler(typeof(CustomSwitch), typeof(CustomSwitchHandler));
                handler.AddHandler(typeof(WebView), typeof(CustomWebViewHandler));
            });
        return builder.Build();
    }
}
