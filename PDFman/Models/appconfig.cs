using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace PDFman.Models
{
    public class AppConfig
    {
        public int MaxRecentFiles { get; set; } = 15;
        public string DefaultReaderId { get; set; }
        public bool AutoOpenWithAssignedReader { get; set; } = true;
        public bool ShowNotifications { get; set; } = true;
        public string Theme { get; set; } = "Light";
        public List<string> RecentFolders { get; set; } = new List<string>();

        public WindowSettings WindowSettings { get; set; } = new WindowSettings();
    }

    public class WindowSettings
    {
        public double Width { get; set; } = 1000;
        public double Height { get; set; } = 700;
        public bool CenterOnScreen { get; set; } = true;
        public bool RememberPosition { get; set; } = false;
        public double Left { get; set; } = 0;
        public double Top { get; set; } = 0;
    }
}
