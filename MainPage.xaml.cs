using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;


namespace Puzzle
{
    public partial class MainPage : ContentPage
    {
        public List<ContentPage> lehed = new List<ContentPage>()   // Создаём список страниц (экраны приложения)
        {
            new GaleryPage(),     // 0 — страница игры
            new SeadedPage(),   // 1 — страница настроек
            new ReeglidPage()   // 2 — страница правил
        };

        public List<string> tekstid = new List<string>()   // Список строк для подписей на кнопках
        {
            "Mängi",   // Кнопка для перехода на игру
            "Seaded",  // Кнопка для перехода в настройки
            "Reeglid"  // Кнопка для перехода к правилам
        };

        VerticalStackLayout vsl;  // Вертикальный контейнер — кладём элементы друг под другом
        ScrollView sv;
        Label title;
        Label labelButton;
        Grid gridButton;
        Image bgImage;

        public MainPage()
        {
            Title = "Avaleht";

            BackgroundImageSource = "taust.jpg";

            vsl = new VerticalStackLayout();

            title = new Label
            {
                Text = "PUZZLE",
                FontFamily = "Monoton-Regular",
                FontSize = 45,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.White,
                Margin = new Thickness(0, 40, 0, 10)
            };

            vsl.Add(title);

            for (int i = 0; i < lehed.Count; i++)
            {
                int index = i;

                // === создаём контейнер-кнопку ===
                gridButton = new Grid
                {
                    WidthRequest = 220,
                    HeightRequest = 220,
                    HorizontalOptions = LayoutOptions.Center
                };

                // фоновое изображение
                bgImage = new Image
                {
                    Source = "silverpuzzle.jpg",
                    Aspect = Aspect.AspectFill
                };

                // текст поверх картинки
                labelButton = new Label
                {
                    Text = tekstid[i],
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Black,
                    FontSize = 20,
                    FontFamily = "Monoton-Regular"
                };

                // Добавляем элементы в Grid (картинка под текстом)
                gridButton.Children.Add(bgImage);
                gridButton.Children.Add(labelButton);

                // Добавляем обработчик нажатия
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) =>
                {
                    Navigation.PushAsync(lehed[index]);
                };
                gridButton.GestureRecognizers.Add(tapGesture);

                vsl.Add(gridButton);
            }

            sv = new ScrollView { Content = vsl };

            Content = sv;
        }
    }
}
