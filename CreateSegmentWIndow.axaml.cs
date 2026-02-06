using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ReSplit;

public partial class CreateSegmentWindow : Window
{
    public CreateSegmentWindow()
    {
        InitializeComponent();
    }

    private async void OnBrowseScreenshot(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Screenshot",
            AllowMultiple = false,
            Filters =
        {
            new FileDialogFilter
            {
                Name = "Images",
                Extensions = { "png", "jpg", "jpeg", "webp" }
            }
        }
        };

        var result = await dialog.ShowAsync(this);
        if (result != null && result.Length > 0)
        {
            ScreenshotPathBox.Text = result[0];
        }
    }
    private record Segment(string Name, string Screenshot, string State);
    private void SaveSegment()
    {
        var segmentName = SegmentNameBox.Text;
        var screenshotPath = ScreenshotPathBox.Text;
        var state = MainWindow.Instance.Lbl_Status.Text;

        if (string.IsNullOrWhiteSpace(segmentName) ||
            string.IsNullOrWhiteSpace(screenshotPath))
            return;

        var savePath = Path.Combine(
            AppContext.BaseDirectory,
            "segments.json"
        );

        // Load existing list (or create new)
        List<Segment> segments;
        if (File.Exists(savePath))
        {
            var existingJson = File.ReadAllText(savePath);
            segments = JsonSerializer.Deserialize<List<Segment>>(existingJson)
                       ?? new List<Segment>();
        }
        else
        {
            segments = new List<Segment>();
        }

        // Append
        segments.Add(new Segment(segmentName, screenshotPath, state));

        // Save back
        var json = JsonSerializer.Serialize(segments, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(savePath, json);
    }

    private void OnCreate(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SaveSegment();
        this.Close();
    }
}