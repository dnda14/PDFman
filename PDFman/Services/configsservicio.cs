using PDFman.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PDFman.Services
{
    internal class configsservicio
    {
    }
}


namespace PDFManager.Services
{
    public class ConfigurationService
    {
        private static readonly string ConfigFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PDFManager");

        private static readonly string ReadersConfigPath = Path.Combine(ConfigFolder, "lectores.json");
        private static readonly string AssignmentsConfigPath = Path.Combine(ConfigFolder, "asignaciones.json");
        private static readonly string AppConfigPath = Path.Combine(ConfigFolder, "config.json");

        public ConfigurationService()
        {
            EnsureConfigFolderExists();
            InitializeDefaultConfigurations();
        }

        private void EnsureConfigFolderExists()
        {
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);
        }

        private void InitializeDefaultConfigurations()
        {
            if (!File.Exists(ReadersConfigPath))
            {
                var defaultReaders = GetDefaultReaders();
                SaveReadersAsync(defaultReaders).Wait();
            }
        }

        private List<PdfReader> GetDefaultReaders()
        {
            var readers = new List<PdfReader>();

            // Adobe Acrobat Reader
            var adobePath = FindExecutable("AcroRd32.exe", @"Adobe\Acrobat Reader DC\Reader");
            if (adobePath != null)
            {
                readers.Add(new PdfReader
                {
                    Name = "Adobe Acrobat Reader",
                    ExecutablePath = adobePath,
                    IsDefault = true
                });
            }

            // Microsoft Edge
            var edgePath = FindExecutable("msedge.exe", @"Microsoft\Edge\Application");
            if (edgePath != null)
            {
                readers.Add(new PdfReader
                {
                    Name = "Microsoft Edge",
                    ExecutablePath = edgePath,
                    IsDefault = readers.Count == 0
                });
            }

            // Firefox
            var firefoxPath = FindExecutable("firefox.exe", @"Mozilla Firefox");
            if (firefoxPath != null)
            {
                readers.Add(new PdfReader
                {
                    Name = "Mozilla Firefox",
                    ExecutablePath = firefoxPath,
                    IsDefault = readers.Count == 0
                });
            }

            // Chrome
            var chromePath = FindExecutable("chrome.exe", @"Google\Chrome\Application");
            if (chromePath != null)
            {
                readers.Add(new PdfReader
                {
                    Name = "Google Chrome",
                    ExecutablePath = chromePath,
                    IsDefault = readers.Count == 0
                });
            }

            return readers;
        }

        private string FindExecutable(string exeName, string relativePath)
        {
            var programFiles = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            foreach (var pf in programFiles)
            {
                var fullPath = Path.Combine(pf, relativePath, exeName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

        public async Task<List<PdfReader>> LoadReadersAsync()
        {
            try
            {
                if (!File.Exists(ReadersConfigPath))
                    return new List<PdfReader>();

                var json = await File.ReadAllTextAsync(ReadersConfigPath);
                return JsonConvert.DeserializeObject<List<PdfReader>>(json) ?? new List<PdfReader>();
            }
            catch
            {
                return new List<PdfReader>();
            }
        }

        public async Task SaveReadersAsync(List<PdfReader> readers)
        {
            try
            {
                var json = JsonConvert.SerializeObject(readers, Formatting.Indented);
                await File.WriteAllTextAsync(ReadersConfigPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar lectores: {ex.Message}");
            }
        }

        public async Task<List<PdfAssignment>> LoadAssignmentsAsync()
        {
            try
            {
                if (!File.Exists(AssignmentsConfigPath))
                    return new List<PdfAssignment>();

                var json = await File.ReadAllTextAsync(AssignmentsConfigPath);
                var assignments = JsonConvert.DeserializeObject<List<PdfAssignment>>(json) ?? new List<PdfAssignment>();

                // Filtrar archivos que ya no existen
                return assignments.Where(a => File.Exists(a.FilePath)).ToList();
            }
            catch
            {
                return new List<PdfAssignment>();
            }
        }

        public async Task SaveAssignmentsAsync(List<PdfAssignment> assignments)
        {
            try
            {
                var json = JsonConvert.SerializeObject(assignments, Formatting.Indented);
                await File.WriteAllTextAsync(AssignmentsConfigPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar asignaciones: {ex.Message}");
            }
        }

        public async Task<AppConfig> LoadAppConfigAsync()
        {
            try
            {
                if (!File.Exists(AppConfigPath))
                    return new AppConfig();

                var json = await File.ReadAllTextAsync(AppConfigPath);
                return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }

        public async Task SaveAppConfigAsync(AppConfig config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                await File.WriteAllTextAsync(AppConfigPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar configuración: {ex.Message}");
            }
        }
    }
}