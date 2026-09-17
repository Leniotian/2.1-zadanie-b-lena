using System.Windows;
using SNAKE.Tests;

namespace SNAKE;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Contains("--test") || e.Args.Contains("-t") || e.Args.Contains("--verify"))
        {
            int result = GameEngineTests.RunAllTests();
            Environment.Exit(result);
            return;
        }

        base.OnStartup(e);
    }
}
