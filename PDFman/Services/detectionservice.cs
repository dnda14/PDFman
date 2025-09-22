using PDFman.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace PDFman.Services
{
    public class PdfDetectionService
    {
        private readonly string _recentFolder;

        public PdfDetectionService()
        {
            _recentFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Recent));
        }

        public async Task<List<string>> GetRecentPdfFilesAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(_recentFolder))
                        return new List<string>();

                    var pdfFiles = new List<string>();
                    var recentFiles = Directory.GetFiles(_recentFolder, "*.lnk")
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(f => f.LastWriteTime)
                        .Take(50); // Tomar más para filtrar después

                    foreach (var file in recentFiles)
                    {
                        try
                        {
                            var targetPath = GetShortcutTarget(file.FullName);
                            if (!string.IsNullOrEmpty(targetPath) &&
                                targetPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) &&
                                File.Exists(targetPath))
                            {
                                pdfFiles.Add(targetPath);
                            }
                        }
                        catch
                        {
                            // Ignorar shortcuts corruptos
                        }
                    }

                    return pdfFiles.Distinct().ToList();
                }
                catch
                {
                    return new List<string>();
                }
            });
        }

        private string GetShortcutTarget(string shortcutPath)
        {
            try
            {
                var shell = new IWshRuntimeLibrary.WshShell();
                var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                return shortcut.TargetPath;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<PdfAssignment>> ScanForNewPdfsAsync(
            List<PdfAssignment> existingAssignments,
            int maxFiles)
        {
            var recentPdfs = await GetRecentPdfFilesAsync();
            var newAssignments = new List<PdfAssignment>();

            foreach (var pdfPath in recentPdfs.Take(maxFiles))
            {
                if (!existingAssignments.Any(a => a.FilePath.Equals(pdfPath, StringComparison.OrdinalIgnoreCase)))
                {
                    var fileInfo = new FileInfo(pdfPath);
                    newAssignments.Add(new PdfAssignment
                    {
                        FilePath = pdfPath,
                        LastOpened = fileInfo.LastAccessTime > fileInfo.LastWriteTime
                            ? fileInfo.LastAccessTime
                            : fileInfo.LastWriteTime
                    });
                }
            }

            return newAssignments;
        }
    }
}
