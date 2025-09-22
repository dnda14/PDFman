using PDFman.Views;
using System;
using System.Linq;
using System.Windows;

namespace PDFman
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Verificar si se pasó un archivo PDF como argumento
            if (e.Args.Length > 0 && e.Args[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                // TODO: Implementar lógica para abrir PDF específico
                // Por ahora, simplemente abre la aplicación normal
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                // Apertura normal de la aplicación
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}