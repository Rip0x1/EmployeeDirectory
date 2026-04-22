using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using EmployeeDirectory.Models;
using EmployeeDirectory.Maui.Services;
using System.Diagnostics;

namespace EmployeeDirectory.Maui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly EmployeeApiService _apiService;
    private int _currentPage = 1;
    private const int PageSize = 10;
    private bool _isLoadingMore;
    private bool _allDataLoaded;
    private bool _isAutoRefreshing;

    public ObservableCollection<Employee> Employees { get; } = new();

    [ObservableProperty]
    private bool isRefreshing;

    public MainViewModel(EmployeeApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadEmployeesAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;
        _currentPage = 1;
        _allDataLoaded = false;

        try
        {
            var employees = await _apiService.GetEmployeesAsync(_currentPage, PageSize);
            Employees.Clear();
            if (employees != null)
            {
                foreach (var emp in employees)
                    Employees.Add(emp);

                if (employees.Count < PageSize) _allDataLoaded = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task LoadMoreEmployeesAsync()
    {
        if (_isLoadingMore || IsRefreshing || _allDataLoaded)
            return;

        _isLoadingMore = true;
        try
        {
            _currentPage++;
            var nextEmployees = await _apiService.GetEmployeesAsync(_currentPage, PageSize);

            if (nextEmployees != null && nextEmployees.Any())
            {
                foreach (var emp in nextEmployees)
                    Employees.Add(emp);

                if (nextEmployees.Count < PageSize)
                    _allDataLoaded = true;
            }
            else
            {
                _allDataLoaded = true;
            }
        }
        catch (Exception ex)
        {
            _currentPage--;
            Debug.WriteLine($"Ошибка подгрузки: {ex.Message}");
        }
        finally
        {
            _isLoadingMore = false;
        }
    }

    public void StartAutoRefresh()
    {
        _isAutoRefreshing = true;
        Application.Current?.Dispatcher.StartTimer(TimeSpan.FromSeconds(10), () =>
        {
            MainThread.BeginInvokeOnMainThread(() => {
                _ = LoadEmployeesAsync();
            });
            return _isAutoRefreshing;
        });
    }

    public void StopAutoRefresh()
    {
        _isAutoRefreshing = false;
    }
}