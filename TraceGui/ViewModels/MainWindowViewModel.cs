﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using BitmapToVector;
using ReactiveUI;
using SkiaSharp;

namespace TraceGui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IEnumerable<IGeometryImpl>? _paths;
        private int _width;
        private int _height;

        public MainWindowViewModel()
        {
            OpenCommand = ReactiveCommand.CreateFromTask(async () => await OnOpen());
            SaveCommand = ReactiveCommand.CreateFromTask(async () => await OnSave());
        }

        private async Task OnOpen()
        {
            var dlg = new OpenFileDialog();

            dlg.Filters.Add(new FileDialogFilter()
            {
                Name = "Supported Files (*.png;*.jpg;*.jpeg;*.bmp)", 
                Extensions = new List<string> {"png", "jpg", "jpeg", "bmp"}
            });

            dlg.Filters.Add(new FileDialogFilter()
            {
                Name = "All Files", 
                Extensions = new List<string> {"*"}
            });

            var result = await dlg.ShowAsync(GetMainWindow());
            if (result is not null && result.Length == 1)
            {
                Load(result[0]);
            }
        }

        private async Task OnSave()
        {
            var dlg = new SaveFileDialog();

            dlg.Filters.Add(new FileDialogFilter()
            {
                Name = "Svg Files (*.svg)", 
                Extensions = new List<string> {"svg"}
            });

            var result = await dlg.ShowAsync(GetMainWindow());
            if (result is not null)
            {
                Save(result);
            }
        }

        private void Save(string filename)
        {
            if (_paths is not null)
            {
                SvgWriter.Save(Width, _height, _paths, filename);
            }
        }

        private void Load(string filename)
        {
            using var source = SKBitmap.Decode(filename);
            var param = new PotraceParam();
            var paths = PotraceAvalonia.Trace(param, source).ToList();

            Width = source.Width;
            Height = source.Height;
            Paths = paths;
        }

        private Window? GetMainWindow()
        {
            return (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }

        public ICommand OpenCommand { get; }

        public ICommand SaveCommand { get; }

        public IEnumerable<IGeometryImpl>? Paths
        {
            get => _paths;
            set => this.RaiseAndSetIfChanged(ref _paths, value);
        }

        public int Width
        {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value);
        }

        public int Height
        {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value);
        }
    }
}