using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PullCanvasData2.Structures;
using static PullCanvasData2.Util;

namespace PullCanvasData2;

internal class AuthMonitor {
    private readonly HttpClient client;
    private AuthenticationResponse authData;

    internal AuthMonitor(string baseUrl, string clientId, string clientSecret) {
        client = new HttpClient();
        client.BaseAddress = new Uri(baseUrl);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
             "Basic",
             Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(clientId + ":" + clientSecret))
        );
    }

    internal async Task Authenticate() {
        authData = null;

        var args = new[] {
            ("grant_type", "client_credentials")
        };

        var response = await client.PostAsync("ids/auth/login", BuildHttpArguments(args));
        AssertSuccess(response, message => new CanvasDataAuthenticationException(message));

        try {
            authData = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync());
        } catch (Exception e) {
            throw new CanvasDataAuthenticationException("Couldn't deserialize the auth response.", e);
        }
    }

    internal async Task<string> GetAccessToken() {
        if (authData == null) {
            await Authenticate();
            Debug.Assert(authData != null);
        }

        if (authData.ExpiresAt.AddMinutes(-10) > DateTime.Now) {
            return authData.AccessToken;
        }
        
        try {
            await Authenticate();
            Debug.Assert(authData != null);
        } catch (Exception e) {
            throw new CanvasDataAuthenticationException("Couldn't authenticate. We needed to reauthenticate due to an expired token, but it failed.", e);
        }

        return authData.AccessToken;
    }
}
