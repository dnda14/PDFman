using PDFman.Models;
using PDFman.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PDFman.ViewModels
{
    public class ReaderSelectionViewModel : BaseViewModel
    {
        private PdfReader _selectedReader;
        private bool _rememberChoice;

        public ObservableCollection<PdfReader> AvailableReaders { get; } = new ObservableCollection<PdfReader>();
        public PdfAssignment Assignment { get; }

        public PdfReader SelectedReader
        {
            get => _selectedReader;
            set => SetProperty(ref _selectedReader, value);
        }

        public bool RememberChoice
        {
            get => _rememberChoice;
            set => SetProperty(ref _rememberChoice, value);
        }

        public string PdfFileName => Assignment?.FileName ?? "Archivo PDF";

        public ICommand SelectReaderCommand { get; }
        public ICommand CancelCommand { get; }

        public ReaderSelectionViewModel(List<PdfReader> readers, PdfAssignment assignment)
        {
            Assignment = assignment;

            foreach (var reader in readers)
            {
                AvailableReaders.Add(reader);
            }

            // Seleccionar el lector por defecto inicialmente
            SelectedReader = AvailableReaders.FirstOrDefault(r => r.IsDefault) ?? AvailableReaders.FirstOrDefault();
            RememberChoice = true;

            SelectReaderCommand = new RelayCommand(param => OnReaderSelected());
            CancelCommand = new RelayCommand(param => OnCancel());
        }

        private void OnReaderSelected()
        {
            // La ventana se cerrará con DialogResult = true
        }

        private void OnCancel()
        {
            SelectedReader = null;
            // La ventana se cerrará con DialogResult = false
        }
    }
}
