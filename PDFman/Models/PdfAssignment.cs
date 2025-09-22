using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace PDFman.Models
{
    public class PdfAssignment : INotifyPropertyChanged
    {
        private string _filePath;
        private string _readerId;
        private DateTime _lastOpened;
        private string _fileName;

        public string FilePath
        {
            get => _filePath;
            set
            {
                SetProperty(ref _filePath, value);
                FileName = Path.GetFileName(value);
            }
        }

        public string ReaderId
        {
            get => _readerId;
            set => SetProperty(ref _readerId, value);
        }

        public DateTime LastOpened
        {
            get => _lastOpened;
            set => SetProperty(ref _lastOpened, value);
        }

        public string FileName
        {
            get => _fileName;
            private set => SetProperty(ref _fileName, value);
        }

        public bool FileExists => File.Exists(FilePath);

        public long FileSize => FileExists ? new FileInfo(FilePath).Length : 0;

        public string FileSizeFormatted => FormatFileSize(FileSize);

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
