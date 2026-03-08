using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace LiveGuide;

public partial class MainWindow : Window
{
    public static ObservableCollection<GuideModel> GuideList = new();
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ImportSegments_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var segments = LiveGuidePlugin.Host?.Splits;
        if (segments?.Count > 0)
        {
            GuideList.Clear();
            Dispatcher.UIThread.Invoke(() =>
            {
                foreach (var segment in segments)
                {
                    GuideList.Add(new GuideModel { Name = segment.Name });
                }
            });
        }
    }

    private async void ImportGuide_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Open Run File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                new FilePickerFileType("ReSplit Guide")
                {
                    Patterns = new[] { "*.rsg" }
                }
                }
            });

        if (files.Count > 0)
        {
            GuideList.Clear();
            var path = files[0].Path.LocalPath;
            LoadFile(path);
        }
    }

    public void LoadFile(string path)
    {
        var guide = JsonSerializer.Deserialize<List<GuideModel>>(File.ReadAllText(path));
        if (guide == null) return;
        Dispatcher.UIThread.Invoke(() =>
        {
            GuideList.Clear();
            foreach (var item in guide)
            {
                GuideList.Add(item);
            }
        });
    }
    public void SaveFile(string path)
    {
        var json = JsonSerializer.Serialize(GuideList);
        File.WriteAllText(path, json);
    }

    private void List_Segments_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (List_Segments.SelectedItem is GuideModel selected)
        {
            Lbl_Guide.Text = selected.Text;
        }
    }

    private async void SaveGuide_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save Guide File",
                SuggestedFileName = "guide.rsg",
                FileTypeChoices = new[]
                {
            new FilePickerFileType("ReSplit Guide")
            {
                Patterns = new[] { "*.rsg" }
            }
                }
            });

        if (file != null)
        {
            var path = file.Path.LocalPath;
            SaveFile(path);
        }
    }

    private void Lbl_Guide_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        var selected = List_Segments.SelectedItem as GuideModel;
        if (selected != null)
        {
            selected.Text = Lbl_Guide.Text ?? "";
        }
    }
}