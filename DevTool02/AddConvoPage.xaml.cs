using DevTool01.Data;
using System.Collections.ObjectModel;
using DevTool01.Models;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DevTool01;

public partial class AddConvoPage : ContentPage
{
	private readonly DataContext _dataContext;
	public ObservableCollection<obsCharacter> ObscCharacters { get; } = [];
	public ObservableCollection<obsLocation> ObscLocations { get; } = [];
	public string _namesDisplay = string.Empty;
	public string _locDisplay = string.Empty;
	public string? _remarkDisplay;
	public string? RemarkDisplay {
		get => _remarkDisplay;
		set {
			_remarkDisplay = value;
			OnPropertyChanged();
		}
    }
    public string? _conditionDisplay;
	public string? ConditionDisplay {
		get => _conditionDisplay;
		set {
			_conditionDisplay = value;
			OnPropertyChanged();
			}
    }
    public string NamesDisplay {
		get => _namesDisplay;
		set {
			_namesDisplay = value;
			OnPropertyChanged();
		}
	}
	public string LocDisplay {
		get => _locDisplay;
		set {
			_locDisplay = value;
			OnPropertyChanged();
		}
    }
    public AddConvoPage(DataContext dataContext)
	{
		InitializeComponent();
		NamesDisplay = string.Empty;
		_dataContext = dataContext;
		BindingContext = this;
        OpenDrop();
	}
	public async void OpenDrop()
	{
		await OpenDropDown(_dataContext.Characters, ObscCharacters);
		await OpenDropDown(_dataContext.Locations, ObscLocations);
	}
	private async Task OpenDropDown<T>(DbSet<T> Dcont, ObservableCollection<T> target) where T : class
	{
		try {
			target.Clear();
			var items = await Dcont.ToListAsync();
			foreach (var item in items) {
				/*
                if (item is obsCharacter charac) {
                    DisplayAlert("info", $"Loaded {charac.CharSelected} items. {charac.CharName.ToString()}", "OK");
                } else if (item is obsLocation locat) {
                    DisplayAlert("info", $"Loaded {locat.LocSelected} items.", "OK");
                }*/
                target.Add(item);
			}
		} catch {
			await DisplayAlert(
					"Error",
					"Failed to load characternames.",
					"OK");
		}
	}
    private void AddConvo(object sender, EventArgs e)
    {
        List<string> selectedCharacters = [];
        foreach (var character in ObscCharacters) {
            if (character.CharSelected) {
				//DisplayAlert("Info", $"Selected character: {character.CharName}", "OK");
                selectedCharacters.Add(character.CharName);
            }
        }
        string? selectedLocation = null;
        foreach (var location in ObscLocations) {
            if (location.LocSelected) {
                selectedLocation = location.LocName;
                break; 
            }
        }
        if (selectedCharacters.Count == 0) {
            DisplayAlert("Error", "Select at least one character", "OK");
            return;
        }
        if (selectedLocation == null) {
            DisplayAlert("Error", "Select a location", "OK");
            return;
        }
		DisplayAlert("Remark:", _remarkDisplay, "OK");
		DisplayAlert("Condition", _conditionDisplay, "OK");
        Navigation.PushModalAsync(
            new EditConvoPage(_dataContext, selectedCharacters, selectedLocation, _remarkDisplay, _conditionDisplay)
        );
    }
    private void GoBack(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new MainPage(_dataContext));
    }
}