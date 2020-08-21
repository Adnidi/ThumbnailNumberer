using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ThumbnailNumberer
{
    public class MainImageViewModel : INotifyPropertyChanged
    {
        private int _xPosition = 5;
        private int _yPosition = 450;

        public MainImageViewModel()
        {
            OpenFileDialog = new RelayCommand(e => true, e => OnOpenFileDialog());

            Save = new RelayCommand(e => true, e => OnSave());

            SetupImage(FromNumber);
        }

        public BitmapImage Image { get; set; }

        //TODO configurable
        public string FilePath { get; set; } = $@"PathToImage";

        public int XPosition
        {
            get => _xPosition;
            set
            {
                _xPosition = value;
                SetupImage(FromNumber);
                OnPropertyChanged(nameof(XPosition));
                OnPropertyChanged(nameof(Image));
            }
        }

        public int YPosition
        {
            get => _yPosition;
            set
            {
                _yPosition = value;
                SetupImage(FromNumber);
                OnPropertyChanged(nameof(YPosition));
                OnPropertyChanged(nameof(Image));
            }
        }

        public int FromNumber { get; set; } = 1;
        public int ToNumber { get; set; } = 99;

        public ICommand OpenFileDialog { get; set; }
        public ICommand Save { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnOpenFileDialog()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.DefaultExt = ".png";

            var result = fileDialog.ShowDialog();

            if (result == true)
            {
                FilePath = fileDialog.FileName;

                SetupImage(FromNumber);

                OnPropertyChanged(nameof(FilePath));
                OnPropertyChanged(nameof(Image));
            }
        }

        public void OnSave()
        {
            using (var fileDialog = new FolderBrowserDialog())
            {
                // TODO path configurable
                fileDialog.SelectedPath = $@"D:\LP";
                var result = fileDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fileDialog.SelectedPath))
                {
                    var basePath = fileDialog.SelectedPath;

                    for (int i = FromNumber; i <= ToNumber; ++i)
                    {
                        SetupImage(i);

                        var filePath = Path.Combine(basePath, i.ToString("D3")+".png");

                        Image.Save(filePath);
                    }
                }

                OnPropertyChanged(nameof(Image));
            }
        }

        public void SetupImage(int number)
        {
            var numberAsPaddedString = number.ToString("D3");

            using (Bitmap bmp = new Bitmap(FilePath))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.DrawString(numberAsPaddedString, new Font("Arial", 90, FontStyle.Bold), new SolidBrush(Color.Black), XPosition, YPosition);

                using (MemoryStream memory = new MemoryStream())
                {
                    bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    Image = bitmapimage;
                }
            }
        }

        private void OnPropertyChanged(string prop)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(prop);
                handler(this, e);
            }
        }
    }

    public static class Extensions
    {
        public static void Save(this BitmapImage image, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private Predicate<object> _canExecute;
        private Action<object> _execute;

        public RelayCommand(Predicate<object> canExecute, Action<object> execute)
        {
            this._canExecute = canExecute;
            this._execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}