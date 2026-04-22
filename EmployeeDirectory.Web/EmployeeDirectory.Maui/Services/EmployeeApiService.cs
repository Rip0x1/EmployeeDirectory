using System.Diagnostics;
using System.Net.Http.Json;
using EmployeeDirectory.Models;
using System.Text.Json;

namespace EmployeeDirectory.Maui.Services;

using System.Text.Json;
using System.Net.Http.Json;

public class EmployeeApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    private readonly string _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                                       ? "http://10.0.2.2:7019"
                                       : "https://localhost:7019";

    public EmployeeApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<List<Employee>> GetEmployeesAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var url = $"{_baseUrl}/api/employees?page={page}&pageSize={pageSize}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Employee>>(content, _jsonOptions) ?? new();
            }
            return new();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[API Error]: {ex.Message}");
            return new();
        }
    }
}