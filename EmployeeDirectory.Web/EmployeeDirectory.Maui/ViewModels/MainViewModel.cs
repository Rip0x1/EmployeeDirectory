using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmployeeDirectory.Maui.Services;
using EmployeeDirectory.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EmployeeDirectory.Maui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly EmployeeApiService _apiService;
    private CancellationTokenSource _searchCts;

    public ObservableCollection<Employee> Employees { get; } = new();

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string searchText;

    [ObservableProperty]
    private bool isBusy;

    public MainViewModel(EmployeeApiService apiService)
    {
        _apiService = apiService;
        Task.Run(async () => await LoadEmployeesInternal(false));
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(400, token);
                if (token.IsCancellationRequested) return;

                List<Employee> results;
                if (string.IsNullOrWhiteSpace(value))
                    results = await _apiService.GetEmployeesAsync(1, 10);
                else
                    results = await _apiService.SearchEmployeesAsync(value, "");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Employees.Clear();
                    if (results != null)
                    {
                        foreach (var emp in results)
                            Employees.Add(emp);
                    }
                });
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }, token);
    }

    [RelayCommand]
    private async Task LoadEmployeesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var employees = await _apiService.GetEmployeesAsync(1, 20);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Employees.Clear();
                if (employees != null)
                {
                    foreach (var emp in employees)
                        Employees.Add(emp);
                }
            });

            await Task.Delay(100);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Refresh Error]: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false; 
        }
    }

    private async Task LoadEmployeesInternal(bool showRefresh)
    {
        if (IsRefreshing) return;

        if (showRefresh) IsRefreshing = true;

        try
        {
            var employees = await _apiService.GetEmployeesAsync(1, 10);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Employees.Clear();
                if (employees != null)
                {
                    foreach (var emp in employees)
                        Employees.Add(emp);
                }
            });
        }
        catch (Exception ex) { Debug.WriteLine(ex.Message); }
        finally
        {
            if (showRefresh) IsRefreshing = false;
        }
    }
}