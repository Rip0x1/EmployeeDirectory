using EmployeeDirectory.Maui.ViewModels;

namespace EmployeeDirectory.Maui;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = (MainViewModel)BindingContext;
        vm.StartAutoRefresh();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        var vm = (MainViewModel)BindingContext;
        vm.StopAutoRefresh();
    }

}