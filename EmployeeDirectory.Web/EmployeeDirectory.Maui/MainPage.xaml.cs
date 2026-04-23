using EmployeeDirectory.Maui.ViewModels;

namespace EmployeeDirectory.Maui;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}