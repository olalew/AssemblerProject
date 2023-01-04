using ImageResizing.Infrastructure;
using ImageResizing.Models;
using ImageResizing.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageResizing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap workingImage;
        private Bitmap resultBitmap;

        public MainWindow()
        {
            InitializeComponent();

            ThreadSlider.Value = 8;

            AddButton.Click += delegate
            {
                if (ThreadSlider.Value < 64)
                {
                    ThreadSlider.Value++;
                }
            };

            SubtractButton.Click += delegate
            {
                if (ThreadSlider.Value > 1)
                {
                    ThreadSlider.Value--;
                }
            };

            ThreadSlider.ValueChanged += delegate
            {
                ThreadCountLabel.Text = $"{ThreadSlider.Value}";
            };

            WidthTextBox.Text = $"1024";
            HeightTextBox.Text = $"1024";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[0-9]");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            TextBox textBox = (TextBox)sender;

            if (!int.TryParse($"{textBox.Text}{e.Text}", out int value))
            {
                e.Handled = true;
                return;
            }

            if (value > 32768 || value == 0)
            {
                e.Handled = true;
                return;
            }

            e.Handled = false;
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            ExcecutionConfiguration configuration = new ExcecutionConfiguration
            {
                InitialBitmap = workingImage,
                IsAssembly = (bool)IsAssemblyCheckBox.IsChecked,
                ThreadCount = (int)ThreadSlider.Value,
                Height = int.Parse(HeightTextBox.Text),
                Width = int.Parse(WidthTextBox.Text)
            };

            if (workingImage == null)
                return;

            ImageSizeModifierService sizeModifierService = new ImageSizeModifierService(configuration);

            long executionTime = 0;
            (resultBitmap, executionTime) = sizeModifierService.ExecuteAlgorithm();
            BitmapSource resultImage = BitmapParser.BitmapToBitmapSource(resultBitmap);

            TimeElapsedLabel.Text = @$"{executionTime}";

            resultBitmap.Save(@"C:\Users\olale\Downloads\result.png");
            Process.Start(new ProcessStartInfo(@"C:\Users\olale\Downloads\result.png") { UseShellExecute = true });
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            resultBitmap.Save(@"C:\Users\olale\Downloads\result.png");
        }

        #region FILE OPENER

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            GetFileWithDialog();
        }

        private void GetFileWithDialog()
        {
            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = @"C:\Users\olale\Downloads",
                Filter = "Image Files (*.bmp;*jpg;*.png)|*.png;*.bmp;*jpg;",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadImageWithPath(openFileDialog.FileName);
            }
        }

        private void LoadImageWithPath(string url)
        {
            BitmapImage bitmapImage = new(new Uri(url));
            workingImage = BitmapParser.BitmapImageToBitmap(bitmapImage);
            InputImage.ImageSource = bitmapImage;
        }

        #endregion
    }
}
