using PDFman.Models;
using PDFman.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PDFman.Views
{
    public partial class ReaderSelectionWindow : Window
    {
        public ReaderSelectionWindow()
        {
            InitializeComponent();
        }

        private void ReaderCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is PdfReader reader)
            {
                var viewModel = DataContext as ReaderSelectionViewModel;
                if (viewModel != null)
                {
                    viewModel.SelectedReader = reader;
                }
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ReaderSelectionViewModel;
            if (viewModel?.SelectedReader != null)
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Por favor seleccione un lector PDF.", "Selección requerida",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
