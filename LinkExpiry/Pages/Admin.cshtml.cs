using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class AdminPageModel : PageModel
{
    private readonly HttpClient _httpClient;

    public AdminPageModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [BindProperty]
    [Required, MaxLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Username contains invalid characters.")]
    public string Username { get; set; }

    [BindProperty]
    public int? ExpirationMinutes { get; set; }
    [BindProperty]
    public int MaxClicks { get; set; } = 1; // Default to 1 click

    public string GeneratedLink { get; set; }
    public string ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // Prepare the request URL with optional expiration
        var url = $"https://localhost:7138/api/Link/generate?expirationMinutes={ExpirationMinutes}&maxClicks={MaxClicks}"; 

        // Create the request body
        var requestBody = new { Username };

        // Make the HTTP POST request to the API
        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(result);

                // Get the generated link from the JSON response
                GeneratedLink = jsonDocument.RootElement.GetProperty("linkUrl").GetString();
            }
            else
            {
                ErrorMessage = "Error generating the link.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }

        return Page();
    }
}
