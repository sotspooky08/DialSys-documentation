using CommunityToolkit.Mvvm.ComponentModel;
using DevTool01.Data;
using DevTool01.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace DevTool01
{
    public partial class MainPage : ContentPage
    {
        private readonly DataContext _dataContext;
        
        //Observable collections
        public ObservableCollection<Convo> ObscConvos { get; } = [];

        //mainpage constructor
        public MainPage(DataContext dataContext)
        {
            InitializeComponent();
            _dataContext = dataContext;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadConvos();
        }

        //load Convos onto page
        private async Task LoadConvos()
        {
            try {
                ObscConvos.Clear(); //For refreshing

                var convos = await _dataContext.Convos.ToListAsync();

                foreach (var convo in convos) {
                    ObscConvos.Add(convo);
                }
            } catch (Exception) {
                await DisplayAlert(
                    "Error",
                    "Failed to load conversations.",
                    "OK");
            }
        }
        private void NewConvoPage(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new AddConvoPage(_dataContext));
        }
    }
}
