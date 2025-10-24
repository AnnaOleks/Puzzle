namespace Puzzle;

public partial class GaleryPage : ContentPage
{
    Grid teemaGrid;
    Frame kaart;
    Image teemaPilt;
    Label teemaLbl;
    VerticalStackLayout kaartVsl;
    ScrollView sv;

    string[] teemad = { "Nature", "Animals", "Food", "City", "Art" };
    string[] pildid = { "nature1.jpg", "animal1.jpg", "food1.jpg", "city1.jpg", "art1.jpg" };

    public GaleryPage()
	{
        Title = "Vali teema";

        BackgroundImageSource = "taust.jpg";

        teemaGrid = new Grid
        {
            Padding = 15,
            RowSpacing = 15,
            ColumnSpacing = 15,
            RowDefinitions = { new RowDefinition(), new RowDefinition(), new RowDefinition() },
            ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() }
        };

        for (int i = 0; i < teemad.Length; i++)
        {
            int idx = i;

            kaart = new Frame
            {
                CornerRadius = 15,
                HasShadow = true,
                Padding = 0,
                BackgroundColor = Colors.White
            };

            teemaPilt = new Image
            {
                Source = pildid[i],
                Aspect = Aspect.AspectFit
            };

            teemaLbl = new Label
            {
                Text = teemad[i],
                TextColor = Colors.Black,
                FontFamily = "Monoton-Regular",
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.End
            };

            kaartVsl = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            kaartVsl.Add(teemaPilt);
            kaartVsl.Add(teemaLbl);
            kaart.Content = kaartVsl;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                Navigation.PushAsync(new TeemaPage(teemad[idx]));
            };

            kaart.GestureRecognizers.Add(tapGesture);

            teemaGrid.Add(kaart, i % 2, i / 2);

            sv = new ScrollView
            {
                Content = teemaGrid
            };

            Content = sv;
        }
    }

}