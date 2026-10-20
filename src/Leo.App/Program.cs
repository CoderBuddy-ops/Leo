using System;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Leo.App;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup.log");
        string firstChanceLog = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "first_chance.log");
        System.IO.File.Delete(firstChanceLog);
        AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
        {
            try
            {
                System.IO.File.AppendAllText(firstChanceLog, $"[{DateTime.Now:HH:mm:ss.fff}] {e.Exception.GetType().FullName}: {e.Exception.Message}\nStack:\n{e.Exception.StackTrace}\n\n");
            }
            catch { }
        };
        System.IO.File.WriteAllText(logPath, "1. Main entered\n");
        try
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            System.IO.File.AppendAllText(logPath, "2. ComWrappers initialized\n");

            Microsoft.UI.Xaml.Application.Start((p) =>
            {
                System.IO.File.AppendAllText(logPath, "3. Inside Application.Start callback\n");
                try
                {
                    var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                    var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
                    SynchronizationContext.SetSynchronizationContext(context);
                    System.IO.File.AppendAllText(logPath, "4. Creating new App()\n");
                    _ = new App();
                    System.IO.File.AppendAllText(logPath, "5. App created\n");
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(logPath, $"ERR in Start callback: {ex}\n");
                }
            });
            System.IO.File.AppendAllText(logPath, "6. Application.Start returned\n");
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(logPath, $"ERR in Main: {ex}\n");
        }
    }
}
