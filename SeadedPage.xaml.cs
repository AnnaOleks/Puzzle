using Microsoft.Maui.Controls.Shapes;

namespace Puzzle;

public partial class SeadedPage : ContentPage
{
    VerticalStackLayout vsl;
    VerticalStackLayout menuvsl;
    ScrollView sv;
    Label laudLbl;
    Picker lauaSuurus;
    Border menuuKaart;

    public SeadedPage()
	{
        Title = "Vali laua suurus";

        BackgroundImageSource = "taust.jpg";

        menuvsl = new VerticalStackLayout              // Контейнер для карточек и кнопки
        {
            VerticalOptions = LayoutOptions.CenterAndExpand // По центру и растягиваться при необходимости
        };

        laudLbl = new Label
        {
            Text = "Laua suurus",                
            FontSize = 20,                             
            TextColor = Colors.White,                  
            FontFamily = "Monoton-Regular",    
            VerticalTextAlignment = TextAlignment.Center
        };

        menuvsl.Add(laudLbl);

        lauaSuurus = new Picker
        {
            Title = "Laua suurus",
            TitleColor = Colors.White,
            TextColor = Colors.White,
            FontFamily = "Monoton-Regular",
            FontSize = 20,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = 200
        };

        lauaSuurus.Items.Add("6×4");
        lauaSuurus.Items.Add("9×6");
        lauaSuurus.Items.Add("12×8");
        lauaSuurus.Items.Add("14×9");
        lauaSuurus.Items.Add("15×10");
        lauaSuurus.Items.Add("18×12");

        lauaSuurus.SelectedIndex = GameSettings.SelectedSizeIndex;

        lauaSuurus.SelectedIndexChanged += (s, e) =>
        {
            if (lauaSuurus.SelectedIndex < 0) return;

            if (lauaSuurus.SelectedIndex == 0)
            {
                int row = 6;
                int col = 4;
                GameSettings.SeaLauaSuurus(row, col, 0);
            }
            else if (lauaSuurus.SelectedIndex == 1)
            {
                int row = 9;
                int col = 6;
                GameSettings.SeaLauaSuurus(row, col, 1);
            }
            else if (lauaSuurus.SelectedIndex == 2)
            {
                int row = 12;
                int col = 8;
                GameSettings.SeaLauaSuurus(row, col, 2);
            }
            else if (lauaSuurus.SelectedIndex == 3)
            {
                int row = 14;
                int col = 9;
                GameSettings.SeaLauaSuurus(row, col, 3);
            }
            else if (lauaSuurus.SelectedIndex == 4)
            {
                int row = 15;
                int col = 10;
                GameSettings.SeaLauaSuurus(row, col, 4);
            }
            else if (lauaSuurus.SelectedIndex == 5)
            {
                int row = 18;
                int col = 12;
                GameSettings.SeaLauaSuurus(row, col, 5);
            }
        };

        menuvsl.Add(lauaSuurus);

        menuuKaart = MakeCard(menuvsl);

        vsl = new VerticalStackLayout();

        vsl.Add(menuuKaart);

        sv = new ScrollView
        {
            Content = vsl
        };

        Content = sv;
    }
    Border MakeCard(View content) => new Border
    {
        Stroke = Colors.White,                          // Цвет рамки
        StrokeThickness = 1,                            // Толщина рамки
        StrokeShape = new RoundRectangle { CornerRadius = 20 }, // Скруглённые углы
        Padding = 16,
        Margin = 15,                                    // Внешние отступы
        HorizontalOptions = LayoutOptions.Fill,         // Заполнение доступной ширины
        BackgroundColor = Color.FromArgb("#55222222"),  // Лёгкая прозрачная подложка
        Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Colors.White),  // Белая "тень" = свечение
            Offset = new Point(0, 0),                   // Без смещения — равномерное свечение
            Radius = 10,                                // Размытие — чем выше, тем мягче свечение
            Opacity = 0.3F                              // Полупрозрачное (не ослепляет)
        },
        Content = content                               // Вставляем переданный контент внутрь
    };
}