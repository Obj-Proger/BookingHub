using System.Runtime.CompilerServices;
using FluentValidation;

namespace BookingHub.Application.Tests;

internal static class TestAssemblySetup
{
    [ModuleInitializer]
    public static void Initialize()
    {
        ValidatorOptions.Global.LanguageManager.Enabled = false;
    }
}