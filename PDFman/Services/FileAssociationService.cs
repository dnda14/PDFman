using PDFman.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PDFman.Services
{
    public class FileAssociationService
    {
        public async Task<bool> OpenPdfWithReaderAsync(string pdfPath, PdfReader reader)
        {
            try
            {
                if (!File.Exists(pdfPath))
                {
                    MessageBox.Show($"El archivo no existe: {pdfPath}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (!File.Exists(reader.ExecutablePath))
                {
                    MessageBox.Show($"El lector no está disponible: {reader.Name}\nRuta: {reader.ExecutablePath}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                await Task.Run(() =>
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = reader.ExecutablePath,
                        Arguments = $"\"{pdfPath}\"",
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(reader.ExecutablePath)
                    };

                    Process.Start(startInfo);
                });

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el archivo:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool OpenFileInExplorer(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el explorador:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        public bool OpenFolderInExplorer(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    Process.Start("explorer.exe", $"\"{folderPath}\"");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la carpeta:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }
    }
}
