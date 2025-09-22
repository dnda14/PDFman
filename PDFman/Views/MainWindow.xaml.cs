using PDFman.Models;
using PDFman.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PDFman.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private async void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.DataContext is PdfAssignment assignment)
            {
                var viewModel = DataContext as MainWindowViewModel;
                if (viewModel?.OpenPdfCommand.CanExecute(assignment) == true)
                {
                    viewModel.OpenPdfCommand.Execute(assignment);
                }
            }
        }
    }
}