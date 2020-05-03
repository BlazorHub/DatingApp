﻿using DatingApp.Blazor.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DatingApp.Blazor.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public AuthService(HttpClient http,
                           IJSRuntime js,
                           IConfiguration configuration)
        {
            _http = http;
            _js = js;
            _configuration = configuration;
            _baseUrl = _configuration.GetSection("ApiUrl").Value + "auth/";
        }

        public async Task<string> Login(LoginForm loginForm)
        {
            var user = JsonSerializer.Serialize(loginForm);
            var response = await _http.PostAsync(_baseUrl + "login",
                                                 new StringContent(user,
                                                                   Encoding.UTF8,
                                                                   "application/json"));
            var content = response.Content.ReadAsStringAsync();

            await _js.InvokeVoidAsync("log", content.Result);

            if (content.Result.StartsWith("{\"token\""))
            {
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(content.Result,
                                                                            new JsonSerializerOptions
                                                                            {
                                                                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                                                            });

                return "OK " + authResponse.Token;
            }

            string errorMessage = ErrorInterceptor.InterceptError(content.Result);

            return errorMessage;
        }

        public async Task<string> LoggedIn()
        {
            var token = await _js.InvokeAsync<string>("getToken");
            return token;

            // TODO: return bool after token validation
        }

        public async Task<string> Register(LoginForm registerForm)
        {
            var user = JsonSerializer.Serialize(registerForm);
            var response = await _http.PostAsync(_baseUrl + "register",
                                                 new StringContent(user,
                                                                   Encoding.UTF8,
                                                                   "application/json"));
            var content = response.Content.ReadAsStringAsync();

            string errorMessage = ErrorInterceptor.InterceptError(content.Result);

            return errorMessage;
        }
    }
}