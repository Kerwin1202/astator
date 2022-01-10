﻿
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
            }).ConfigureMauiHandlers(handler =>
            {
                handler.AddCompatibilityRenderer(typeof(CustomLabelButton), typeof(CustomLabelButtonRenderer));
            });
        return builder.Build();
    }
}