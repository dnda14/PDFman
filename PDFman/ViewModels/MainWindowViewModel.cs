using Microsoft.Win32;
using PDFman.Models;
using PDFman.Services;
using PDFman.ViewModels;
using PDFman.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PDFman.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly ConfigurationService _configService;
        private readonly PdfDetectionService _detectionService;
        private readonly FileAssociationService _fileService;

        private bool _isLoading;
        private PdfReader _selectedReader;
        private string _searchText = string.Empty;
        private string _statusMessage = "Listo";

        public ObservableCollection<PdfReader> Readers { get; } = new ObservableCollection<PdfReader>();
        public ObservableCollection<PdfAssignment> FilteredAssignments { get; } = new ObservableCollection<PdfAssignment>();
        public ObservableCollection<PdfAssignment> AllAssignments { get; } = new ObservableCollection<PdfAssignment>();
        public AppConfig Config { get; private set; } = new AppConfig();

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public PdfReader SelectedReader
        {
            get => _selectedReader;
            set
            {
                if (SetProperty(ref _selectedReader, value))
                {
                    FilterAssignmentsByReader();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterAssignmentsByReader();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand OpenPdfCommand { get; }
        public ICommand AddPdfCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand RemoveAssignmentCommand { get; }
        public ICommand ChangeReaderCommand { get; }
        public ICommand OpenInExplorerCommand { get; }
        public ICommand AddReaderCommand { get; }
        public ICommand RemoveReaderCommand { get; }
        public ICommand SetDefaultReaderCommand { get; }

        public MainWindowViewModel()
        {
            _configService = new ConfigurationService();
            _detectionService = new PdfDetectionService();
            _fileService = new FileAssociationService();

            OpenPdfCommand = new RelayCommand(async (param) => await OpenPdfAsync(param as PdfAssignment));
            AddPdfCommand = new RelayCommand(async () => await AddPdfManuallyAsync());
            RefreshCommand = new RelayCommand(async () => await RefreshDataAsync());
            RemoveAssignmentCommand = new RelayCommand(async (param) => await RemoveAssignmentAsync(param as PdfAssignment));
            ChangeReaderCommand = new RelayCommand(async (param) => await ChangeReaderAsync(param as PdfAssignment));
            OpenInExplorerCommand = new RelayCommand(param => OpenInExplorer(param as PdfAssignment));
            AddReaderCommand = new RelayCommand(async () => await AddReaderAsync());
            RemoveReaderCommand = new RelayCommand(async (param) => await RemoveReaderAsync(param as PdfReader));
            SetDefaultReaderCommand = new RelayCommand(async (param) => await SetDefaultReaderAsync(param as PdfReader));

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            IsLoading = true;
            StatusMessage = "Cargando configuración...";

            try
            {
                Config = await _configService.LoadAppConfigAsync();
                await LoadReadersAsync();
                await LoadAssignmentsAsync();
                await ScanForNewPdfsAsync();

                StatusMessage = $"Listo - {AllAssignments.Count} PDFs encontrados";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadReadersAsync()
        {
            var readers = await _configService.LoadReadersAsync();
            Readers.Clear();

            foreach (var reader in readers)
            {
                Readers.Add(reader);
            }

            if (Readers.Any())
            {
                SelectedReader = Readers.FirstOrDefault(r => r.IsDefault) ?? Readers.First();
            }
        }

        private async Task LoadAssignmentsAsync()
        {
            var assignments = await _configService.LoadAssignmentsAsync();
            AllAssignments.Clear();

            foreach (var assignment in assignments.OrderByDescending(a => a.LastOpened))
            {
                AllAssignments.Add(assignment);
            }

            FilterAssignmentsByReader();
        }

        private async Task ScanForNewPdfsAsync()
        {
            StatusMessage = "Buscando PDFs recientes...";

            var newAssignments = await _detectionService.ScanForNewPdfsAsync(
                AllAssignments.ToList(),
                Config.MaxRecentFiles * Readers.Count);

            foreach (var assignment in newAssignments)
            {
                AllAssignments.Insert(0, assignment);
            }

            if (newAssignments.Any())
            {
                await _configService.SaveAssignmentsAsync(AllAssignments.ToList());
                FilterAssignmentsByReader();
                StatusMessage = $"Se encontraron {newAssignments.Count} PDFs nuevos";
            }
        }

        private void FilterAssignmentsByReader()
        {
            FilteredAssignments.Clear();

            var query = AllAssignments.AsEnumerable();

            // Filtrar por lector seleccionado
            if (SelectedReader != null)
            {
                query = query.Where(a => a.ReaderId == SelectedReader.Id ||
                                       (string.IsNullOrEmpty(a.ReaderId) && SelectedReader.IsDefault));
            }

            // Filtrar por búsqueda
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(a => a.FileName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Limitar cantidad y ordenar
            foreach (var assignment in query.Take(Config.MaxRecentFiles).OrderByDescending(a => a.LastOpened))
            {
                FilteredAssignments.Add(assignment);
            }
        }

        private async Task OpenPdfAsync(PdfAssignment assignment)
        {
            if (assignment == null) return;

            try
            {
                PdfReader readerToUse = null;

                // Si ya tiene lector asignado, usarlo
                if (!string.IsNullOrEmpty(assignment.ReaderId))
                {
                    readerToUse = Readers.FirstOrDefault(r => r.Id == assignment.ReaderId);
                }

                // Si no tiene lector o el lector no existe, mostrar selector
                if (readerToUse == null)
                {
                    var selectionViewModel = new ReaderSelectionViewModel(Readers.ToList(), assignment);
                    var selectionWindow = new ReaderSelectionWindow { DataContext = selectionViewModel };

                    if (selectionWindow.ShowDialog() == true && selectionViewModel.SelectedReader != null)
                    {
                        readerToUse = selectionViewModel.SelectedReader;
                        assignment.ReaderId = readerToUse.Id;
                    }
                    else
                    {
                        // Usar lector por defecto si no se seleccionó ninguno
                        readerToUse = Readers.FirstOrDefault(r => r.IsDefault) ?? Readers.FirstOrDefault();
                        if (readerToUse != null)
                        {
                            assignment.ReaderId = readerToUse.Id;
                        }
                    }
                }

                if (readerToUse != null)
                {
                    assignment.LastOpened = DateTime.Now;
                    await _configService.SaveAssignmentsAsync(AllAssignments.ToList());

                    var success = await _fileService.OpenPdfWithReaderAsync(assignment.FilePath, readerToUse);
                    if (success)
                    {
                        StatusMessage = $"Abriendo {assignment.FileName} con {readerToUse.Name}";

                        // Reordenar la lista por último abierto
                        FilterAssignmentsByReader();
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al abrir PDF: {ex.Message}";
            }
        }

        private async Task AddPdfManuallyAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar archivos PDF",
                Filter = "Archivos PDF (*.pdf)|*.pdf",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    if (!AllAssignments.Any(a => a.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                    {
                        var assignment = new PdfAssignment
                        {
                            FilePath = filePath,
                            LastOpened = DateTime.Now
                        };

                        AllAssignments.Insert(0, assignment);
                    }
                }

                await _configService.SaveAssignmentsAsync(AllAssignments.ToList());
                FilterAssignmentsByReader();
                StatusMessage = $"Se agregaron {openFileDialog.FileNames.Length} archivos PDF";
            }
        }

        private async Task RefreshDataAsync()
        {
            await ScanForNewPdfsAsync();
        }

        private async Task RemoveAssignmentAsync(PdfAssignment assignment)
        {
            if (assignment == null) return;

            var result = MessageBox.Show(
                $"¿Desea quitar '{assignment.FileName}' de la lista?\n\nNota: El archivo no será eliminado del disco.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AllAssignments.Remove(assignment);
                FilteredAssignments.Remove(assignment);
                await _configService.SaveAssignmentsAsync(AllAssignments.ToList());
                StatusMessage = $"Se quitó {assignment.FileName} de la lista";
            }
        }

        private async Task ChangeReaderAsync(PdfAssignment assignment)
        {
            if (assignment == null) return;

            var selectionViewModel = new ReaderSelectionViewModel(Readers.ToList(), assignment);
            var selectionWindow = new ReaderSelectionWindow { DataContext = selectionViewModel };

            if (selectionWindow.ShowDialog() == true && selectionViewModel.SelectedReader != null)
            {
                assignment.ReaderId = selectionViewModel.SelectedReader.Id;
                await _configService.SaveAssignmentsAsync(AllAssignments.ToList());
                StatusMessage = $"Se asignó {selectionViewModel.SelectedReader.Name} a {assignment.FileName}";
            }
        }

        private void OpenInExplorer(PdfAssignment assignment)
        {
            if (assignment == null) return;
            _fileService.OpenFileInExplorer(assignment.FilePath);
        }

        private async Task AddReaderAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar ejecutable del lector PDF",
                Filter = "Archivos ejecutables (*.exe)|*.exe",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);

                var reader = new PdfReader
                {
                    Name = fileName,
                    ExecutablePath = openFileDialog.FileName,
                    IsDefault = !Readers.Any()
                };

                Readers.Add(reader);
                await _configService.SaveReadersAsync(Readers.ToList());
                StatusMessage = $"Se agregó el lector {reader.Name}";
            }
        }

        private async Task RemoveReaderAsync(PdfReader reader)
        {
            if (reader == null || Readers.Count <= 1) return;

            var result = MessageBox.Show(
                $"¿Desea eliminar el lector '{reader.Name}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Si era el lector por defecto, asignar otro
                if (reader.IsDefault && Readers.Count > 1)
                {
                    var newDefault = Readers.FirstOrDefault(r => r != reader);
                    if (newDefault != null)
                        newDefault.IsDefault = true;
                }

                Readers.Remove(reader);
                await _configService.SaveReadersAsync(Readers.ToList());
                StatusMessage = $"Se eliminó el lector {reader.Name}";
            }
        }

        private async Task SetDefaultReaderAsync(PdfReader reader)
        {
            if (reader == null || reader.IsDefault) return;

            foreach (var r in Readers)
            {
                r.IsDefault = r == reader;
            }

            await _configService.SaveReadersAsync(Readers.ToList());
            StatusMessage = $"{reader.Name} establecido como lector predeterminado";
        }
    }
}
