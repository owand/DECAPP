using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DECAPP.Services
{
    public class AuthenticationHelper
    {
        public static GraphServiceClient GraphClient { get; set; }
        public static IPublicClientApplication PCA { get; set; }
        public static string TokenForUser;

        public static GraphServiceClient GetAuthenticatedClient()
        {
            // Create Microsoft Graph client.
            try
            {
                GraphClient = new GraphServiceClient(
                    //"https://graph.microsoft.com/v1.0",
                    new DelegateAuthenticationProvider(
                        async (requestMessage) =>
                        {
                            TokenForUser = await GetAccessToken();
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", TokenForUser);
                        }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not create a graph client: " + ex.Message);
                GraphClient = null;
            }
            return GraphClient;
        }

        // Get Token for User.
        public static async Task<string> GetAccessToken()
        {
            PCA = PublicClientApplicationBuilder.Create(App.ClientID).WithRedirectUri($"msal{App.ClientID}://auth").Build();

            IEnumerable<IAccount> accounts = await PCA.GetAccountsAsync();
            // let's see if we have a user in our belly already
            if (TokenForUser == null)/* || expiration <= DateTimeOffset.UtcNow.AddMinutes(5)*/
            {
                AuthenticationResult authResult;
                try
                {
                    IAccount firstAccount = accounts.FirstOrDefault();
                    authResult = await PCA.AcquireTokenSilent(App.Scopes, firstAccount).ExecuteAsync();
                    await RefreshUserDataAsync(authResult.AccessToken).ConfigureAwait(false);
                    TokenForUser = authResult.AccessToken;
                }
                catch (MsalUiRequiredException)
                {
                    try
                    {
                        authResult = await PCA.AcquireTokenInteractive(App.Scopes).WithParentActivityOrWindow(App.ParentWindow).ExecuteAsync();
                        TokenForUser = authResult.AccessToken;
                        await RefreshUserDataAsync(authResult.AccessToken);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return TokenForUser;
        }

        public static async Task RefreshUserDataAsync(string token)
        {
            //get data from API
            HttpClient client = new HttpClient();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
            message.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
            await client.SendAsync(message);
        }
    }
}