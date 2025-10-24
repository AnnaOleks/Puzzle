namespace Puzzle;

public partial class TeemaPage : ContentPage
{
    Grid pildiGrid;
    Dictionary<string, string[]> teemadePildid;
    Image pilt;
    Frame kaart;
    ScrollView sv;
    public TeemaPage(string teemaNimi)
	{
        Title = teemaNimi;

        BackgroundImageSource = "taust.jpg";

        pildiGrid = new Grid
        {
            Padding = 15,
            RowSpacing = 15,
            ColumnSpacing = 15,
            RowDefinitions = { new RowDefinition(), new RowDefinition(), new RowDefinition() },
            ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() }
        };

        teemadePildid = new Dictionary<string, string[]>
        {
            { "Nature", new[] { "nature1.jpg", "nature2.jpg", "nature3.jpg", "nature4.jpg", "nature5.jpg", "nature6.jpg" } },
            { "Animals", new[] { "animal1.jpg", "animal2.jpg", "animal3.jpg", "animal4.jpg", "animal5.jpg", "animal6.jpg" } },
            { "Food", new[] { "food1.jpg", "food2.jpg", "food3.jpg", "food4.jpg", "food5.jpg", "food6.jpg" } },
            { "City", new[] { "city1.jpg", "city2.jpg", "city3.jpg", "city4.jpg", "city5.jpg", "city6.jpg" } },
            { "Art", new[] { "art1.jpg", "art2.jpg", "art3.jpg", "art4.jpg", "art5.jpg", "art6.jpg" } }
        };

        string[] pildid;  // Объявляем переменную, но пока не присваиваем значение

        // Проверяем, есть ли выбранная тема в словаре
        if (teemadePildid.ContainsKey(teemaNimi))
        {
            // Если есть — получаем массив картинок этой темы
            pildid = teemadePildid[teemaNimi];
        }
        else
        {
            // Если нет — создаём пустой массив (чтобы не было ошибки)
            pildid = new string[0];
        };

        for (int i = 0; i < pildid.Length; i++)
        {
            int index = i;

            string file = pildid[i];

            pilt = new Image
            {
                Source = file,
                Aspect = Aspect.AspectFit
            };
            kaart = new Frame
            {
                CornerRadius = 15,
                HasShadow = true,
                Padding = 0,
                BackgroundColor = Colors.White,
                Content = pilt
            };
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                Navigation.PushAsync(new MangPage(teemaNimi, pildid[index]));
            };
            kaart.GestureRecognizers.Add(tapGesture);
            int row = i / 2;
            int col = i % 2;
            pildiGrid.Add(kaart, col, row);
        }
        sv = new ScrollView { Content = pildiGrid };

        Content = sv;
    }
}