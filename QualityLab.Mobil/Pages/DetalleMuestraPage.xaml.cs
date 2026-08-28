namespace QualityLab.Mobil.Pages
{
    [QueryProperty(nameof(MuestraId), "MuestraId")]
    [QueryProperty(nameof(Codigo), "Codigo")]
    [QueryProperty(nameof(TipoProducto), "TipoProducto")]
    [QueryProperty(nameof(Estado), "Estado")]
    public partial class DetalleMuestraPage : ContentPage
    {
        public int MuestraId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string TipoProducto { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        public DetalleMuestraPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LabelCodigo.Text = Codigo;
            LabelTipoProducto.Text = TipoProducto;
            LabelEstado.Text = $"Estado: {Estado}";
        }

        private async void BtnRegistrarAvance_Clicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegistrarAvancePage), new Dictionary<string, object>
            {
                { "MuestraId", MuestraId },
                { "Codigo", Codigo }
            });
        }

        private async void BtnRegistrarIncidencia_Clicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegistrarIncidenciaPage), new Dictionary<string, object>
            {
                { "MuestraId", MuestraId },
                { "Codigo", Codigo }
            });
        }
    }
}