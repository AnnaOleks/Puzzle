// ===== БИБЛИОТЕКИ =====
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MauiApp_TARgv24_;

public partial class MangPage : ContentPage
{
    string teema;
    string pilt;

    int rows;
    int cols;


    // Арена и центральное поле
    AbsoluteLayout arena;
    Grid targetGrid;
    Border boardBorder;

    // Справочники: id -> Image и правильная позиция
    private readonly Dictionary<string, Image> pieceImages = new();
    private readonly Dictionary<(int row, int col), string> correctPositions = new();

    // Размеры тайла (динамически пересчитываются)
    double tileW = 96, tileH = 96;

    // Превью и кнопки
    Image previewImage;
    Button newGameBtn, pickImageBtn;

    // Для рандома
    readonly Random rng = new();

    public MangPage()
    {
        Title = teema;
        BackgroundImageSource = "taust.jpg";
    }
    public MangPage(string teemaNimi, string pildiFail)
    {
        teema = teemaNimi;
        pilt = pildiFail;

        

        // Верхняя панель
        newGameBtn = new Button
        {
            Text = "Alusta uut mängu",
            FontSize = 20,
            BackgroundColor = Colors.LightBlue,
            TextColor = Color.FromRgb(4, 48, 61),
            CornerRadius = 6
        };
        newGameBtn.Clicked += OnNewGameClicked;

        pickImageBtn = new Button
        {
            Text = "Vali pilt",
            FontSize = 20,
            BackgroundColor = Colors.LightGreen,
            TextColor = Color.FromRgb(4, 48, 61),
            CornerRadius = 6
        };
        pickImageBtn.Clicked += async (_, __) => await PickImageAsync();

        previewImage = new Image { Source = "dotnet_bot.png", WidthRequest = 140, HeightRequest = 140 };

        var topBar = new HorizontalStackLayout
        {
            Spacing = 16,
            Padding = new Thickness(10, 8),
            Children = { newGameBtn, pickImageBtn, previewImage }
        };

        // Центральная сетка — стол для сборки
        targetGrid = new Grid
        {
            RowSpacing = 3,
            ColumnSpacing = 3,
            Padding = 6
        };
        for (int r = 0; r < rows; r++) targetGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        for (int c = 0; c < cols; c++) targetGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        boardBorder = new Border
        {
            StrokeThickness = 2,
            Stroke = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromRgb(35, 35, 35).WithAlpha(0.25f)), // слегка серый
            Shadow = new Shadow { Radius = 18, Opacity = 0.6F, Offset = new Point(0, 10) },
            Padding = new Thickness(4),
            Content = targetGrid
        };

        // Арена: центр — поле, вокруг — россыпь
        arena = new AbsoluteLayout { Padding = new Thickness(10) };
        arena.Children.Add(boardBorder);
        AbsoluteLayout.SetLayoutFlags(boardBorder, AbsoluteLayoutFlags.SizeProportional);
        AbsoluteLayout.SetLayoutBounds(boardBorder, new Rect(0.15, 0.15, 0.70, 0.70)); // 70% площади в центре

        // Корневая сетка
        var root = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }
        };
        root.Add(topBar, 0, 0);
        root.Add(arena, 0, 1);

        Content = root;

        // Пересчёт размеров/раскладка по мере измерения экрана
        arena.SizeChanged += (_, __) => { RecomputeTileSize(); ScatterTilesAround(); };

        // Инициализация по умолчанию
        InitializeWithDefaultImage();
    }

    private async void InitializeWithDefaultImage() => await BuildPuzzleFromFileAsync("dotnet_bot.png");

    // ——— Пересчёт размеров тайла от фактического размера центра
    private void RecomputeTileSize()
    {
        // Геометрия центра: совпадает с 15–85% арены
        double W = arena.Width, H = arena.Height;
        if (W <= 0 || H <= 0) return;

        Rect center = new Rect(W * 0.15, H * 0.15, W * 0.70, H * 0.70);

        // Учитываем отступы/spacing, чтобы поместились ровно Rows×Columns
        double gridPad = 6 * 2;          // Padding targetGrid = 6
        double hSpacing = (cols - 1) * 3;
        double vSpacing = (rows - 1) * 3;

        double usableW = Math.Max(1, center.Width - gridPad - hSpacing);
        double usableH = Math.Max(1, center.Height - gridPad - vSpacing);

        tileW = Math.Floor(usableW / cols);
        tileH = Math.Floor(usableH / rows);

        // Обновим размеры для всех кусков, чтобы и «вокруг» были того же размера
        foreach (var img in pieceImages.Values)
        {
            img.WidthRequest = tileW;
            img.HeightRequest = tileH;
        }
    }

    // ——— Построение целевых ячеек с Drop
    private void BuildTargets()
    {
        targetGrid.Children.Clear();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var slot = new Grid
                {
                    BackgroundColor = Color.FromRgba(255, 255, 255, 0.06),
                    Margin = new Thickness(1)
                };

                // Подложим невидимый "placeholder", чтобы содержимое растягивалось по ячейке
                slot.Children.Add(new BoxView { Opacity = 0 });

                var drop = new DropGestureRecognizer { AllowDrop = true };
                int rr = r, cc = c; // фиксируем в замыкание

                drop.Drop += (_, e) =>
                {
                    if (!e.Data.Properties.TryGetValue("id", out var raw) || raw is not string id) return;
                    if (!pieceImages.TryGetValue(id, out var img)) return;

                    // Если в ячейке уже что-то лежит — вернём на обочину
                    if (slot.Children.OfType<Image>().FirstOrDefault() is Image existing)
                    {
                        ReturnPieceToPerimeter(existing);
                        slot.Children.Remove(existing);
                    }

                    // Удалим из прежнего родителя
                    if (img.Parent is AbsoluteLayout abs) abs.Children.Remove(img);
                    if (img.Parent is Layout lay) lay.Children.Remove(img);

                    // Положим внутрь ячейки: размер задаст Grid (img растянется на tileW×tileH)
                    img.HorizontalOptions = LayoutOptions.Fill;
                    img.VerticalOptions = LayoutOptions.Fill;
                    img.Aspect = Aspect.Fill;

                    slot.Children.Add(img);

                    // Подсветка правильности
                    var should = correctPositions[(rr, cc)];
                    slot.BackgroundColor = id == should
                        ? Color.FromRgba(144, 238, 144, 0.6)
                        : Color.FromRgba(255, 228, 225, 0.6);

                    // Тут можно вызвать CheckSolved();
                };

                slot.GestureRecognizers.Add(drop);
                targetGrid.Add(slot, c, r);
            }
        }
    }

    // ——— Разложить кусочки вокруг центрального поля, не выходя за экран
    private void ScatterTilesAround()
    {
        if (arena?.Children is null || pieceImages.Count == 0) return;

        double W = arena.Width, H = arena.Height;
        if (W <= 0 || H <= 0) return;

        Rect center = new Rect(W * 0.15, H * 0.15, W * 0.70, H * 0.70);

        // 4 безопасные зоны вокруг
        var zones = new List<Rect>
        {
            new Rect(0, 0, W, Math.Max(0, center.Y)),                          // верх
            new Rect(0, center.Bottom, W, Math.Max(0, H - center.Bottom)),     // низ
            new Rect(0, center.Y, Math.Max(0, center.X), center.Height),       // лево
            new Rect(center.Right, center.Y, Math.Max(0, W - center.Right), center.Height) // право
        };

        // Небольший отступ внутри зоны
        const double pad = 8;

        foreach (var kv in pieceImages)
        {
            var img = kv.Value;

            if (!arena.Children.Contains(img))
                arena.Children.Add(img);

            double tw = tileW > 0 ? tileW : (img.Width > 0 ? img.Width : 96);
            double th = tileH > 0 ? tileH : (img.Height > 0 ? img.Height : 96);

            // выбираем случайную зону
            var z = zones[rng.Next(zones.Count)];
            double maxX = Math.Max(0, z.Width - tw - pad * 2);
            double maxY = Math.Max(0, z.Height - th - pad * 2);

            // если зона совсем узкая (вдруг маленький экран), прижмём к началу
            double dx = maxX <= 0 ? 0 : rng.NextDouble() * maxX;
            double dy = maxY <= 0 ? 0 : rng.NextDouble() * maxY;

            double x = z.X + pad + dx;
            double y = z.Y + pad + dy;

            AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(img, new Rect(x, y, tw, th));
        }
    }

    // ——— Привязка жестов к куску
    private void AttachPieceGestures(Image piece, string id)
    {
        var drag = new DragGestureRecognizer { CanDrag = true };
        drag.DragStarting += (_, e) =>
        {
            e.Data.Properties["id"] = id;
            piece.Opacity = 0.9;
            piece.ScaleTo(0.97, 80);
        };
        drag.DropCompleted += (_, __) =>
        {
            piece.Opacity = 1.0;
            piece.ScaleTo(1.0, 80);
        };
        piece.GestureRecognizers.Add(drag);
    }

    // ——— Вернуть кусок обратно на «обочину»
    private void ReturnPieceToPerimeter(Image img)
    {
        if (img.Parent is Layout lay) lay.Children.Remove(img);
        if (!arena.Children.Contains(img)) arena.Children.Add(img);

        RecomputeTileSize(); // вдруг менялось
        double tw = tileW, th = tileH;

        double W = arena.Width <= 0 ? 600 : arena.Width;
        double H = arena.Height <= 0 ? 800 : arena.Height;

        Rect center = new Rect(W * 0.15, H * 0.15, W * 0.70, H * 0.70);
        var zones = new[]
        {
            new Rect(0, 0, W, center.Y),
            new Rect(0, center.Bottom, W, H - center.Bottom),
            new Rect(0, center.Y, center.X, center.Height),
            new Rect(center.Right, center.Y, W - center.Right, center.Height)
        };

        var z = zones[rng.Next(zones.Length)];
        const double pad = 8;
        double maxX = Math.Max(0, z.Width - tw - pad * 2);
        double maxY = Math.Max(0, z.Height - th - pad * 2);

        double x = z.X + pad + (maxX <= 0 ? 0 : rng.NextDouble() * maxX);
        double y = z.Y + pad + (maxY <= 0 ? 0 : rng.NextDouble() * maxY);

        AbsoluteLayout.SetLayoutFlags(img, AbsoluteLayoutFlags.None);
        AbsoluteLayout.SetLayoutBounds(img, new Rect(x, y, tw, th));
    }

    // ——— Собрать пазл из файла
    private async Task BuildPuzzleFromFileAsync(string filePath)
    {
        previewImage.Source = ImageSource.FromFile(filePath);

        var sources = SplitImage(filePath, rows, cols);

        // Очистка и карта правильных позиций
        pieceImages.Clear();
        correctPositions.Clear();

        int idx = 0;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                correctPositions[(r, c)] = $"piece_{r}_{c}";

        // Пересобираем сетку целей
        BuildTargets();

        // Создаём кусочки
        foreach (var child in arena.Children.ToList())
        {
            if (child is Image im && im.ClassId?.StartsWith("piece_") == true)
                arena.Children.Remove(im);
        }

        idx = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                string id = $"piece_{r}_{c}";
                var img = new Image
                {
                    ClassId = id,
                    Source = sources[idx++],
                    Aspect = Aspect.Fill
                };
                pieceImages[id] = img;
                AttachPieceGestures(img, id);
                arena.Children.Add(img); // положим на арену; позицию задаст Scatter
            }
        }

        // Подождём, пока арена измерится, затем выставим размеры и разложим
        await Task.Delay(1);
        RecomputeTileSize();
        ScatterTilesAround();
    }

    // ——— Выбор картинки пользователем
    private async Task PickImageAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Vali pilt pusle jaoks",
            FileTypes = FilePickerFileType.Images
        });

        string filePath = result?.FullPath ?? "dotnet_bot.png";
        await BuildPuzzleFromFileAsync(filePath);
    }

    // ——— Новая игра
    private async void OnNewGameClicked(object sender, EventArgs e)
    {
        await BuildPuzzleFromFileAsync("dotnet_bot.png");
    }

    // ——— Нарезка изображения (оставил твою версию, с фиксированными расчётами)
    private static List<ImageSource> SplitImage(string filePath, int rows, int columns)
    {
        var result = new List<ImageSource>();

        using var input = File.OpenRead(filePath);
        using var bitmap = SKBitmap.Decode(input);

        int pieceWidth = bitmap.Width / columns;
        int pieceHeight = bitmap.Height / rows;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                var src = new SKRectI(
                    c * pieceWidth,
                    r * pieceHeight,
                    (c + 1) * pieceWidth,
                    (r + 1) * pieceHeight
                );

                using var piece = new SKBitmap(src.Width, src.Height);
                using (var canvas = new SKCanvas(piece))
                {
                    canvas.DrawBitmap(bitmap, src, new SKRect(0, 0, src.Width, src.Height));
                }

                using var skImage = SKImage.FromBitmap(piece);
                using var data = skImage.Encode(SKEncodedImageFormat.Png, 100);
                var bytes = data.ToArray();

                result.Add(ImageSource.FromStream(() => new MemoryStream(bytes)));
            }
        }

        return result;
    }
}
