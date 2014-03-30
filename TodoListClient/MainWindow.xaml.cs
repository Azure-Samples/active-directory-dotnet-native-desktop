using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Globalization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Runtime.InteropServices;

namespace TodoListClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int INTERNET_OPTION_END_BROWSER_SESSION = 42;

        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The Tenant is the name of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Redirect URI is the URI where Azure AD will return OAuth responses.
        // The Authority is the sign-in URL of the tenant.
        //
        const string aadInstance = "https://login.windows.net/{0}";
        const string tenant = "skwantoso.com";
        const string clientId = "fb715b0e-3ca9-45b8-9928-2329a776b42d";
        Uri redirectUri = new Uri("http://TodoListClient");

        static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        //
        // To authenticate to the To Do list service, the client needs to know the service's App ID URI.
        // To contact the To Do list service we need it's URL as well.
        //
        const string todoListResourceId = "https://skwantoso.com/TodoListService";
        const string todoListBaseAddress = "https://localhost:44321";

        private HttpClient httpClient = new HttpClient();
        private AuthenticationContext authContext = null;

        public MainWindow()
        {
            InitializeComponent();

            //
            // As the application starts, try to get an access token without prompting the user.  If one exists, populate the To Do list.  If not, continue.
            //
            authContext = new AuthenticationContext(authority, new CredManCache());
            AuthenticationResult result = null;
            try
            {
                result = authContext.AcquireToken(todoListResourceId, clientId, redirectUri, PromptBehavior.Never);

                // A valid token is in the cache - get the To Do list.
                SignInButton.Content = "Clear Cache";
                GetTodoList();
            }
            catch (ActiveDirectoryAuthenticationException ex)
            {
                if (ex.ErrorCode == "user_interaction_required")
                {
                    // There are no tokens in the cache.  Proceed without calling the To Do list service.
                }
                else
                {
                    // An unexpected error occurred.
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Inner Exception : " + ex.InnerException.Message;
                    }
                    MessageBox.Show(message);
                }
                return;
            }

        }

        private async void GetTodoList()
        {
            //
            // Get an access token to call the To Do service.
            //
            AuthenticationResult result = null;
            try
            {
                result = authContext.AcquireToken(todoListResourceId, clientId, redirectUri, PromptBehavior.Never);
            }
            catch (ActiveDirectoryAuthenticationException ex)
            {
                // There is no access token in the cache, so prompt the user to sign-in.
                if (ex.ErrorCode == "user_interaction_required")
                {
                    MessageBox.Show("Please sign in first");
                    SignInButton.Content = "Sign In";
                }
                else
                {
                    // An unexpected error occurred.
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Inner Exception : " + ex.InnerException.Message;
                    }
                    MessageBox.Show(message);
                }

                return;
            }

            // Once the token has been returned by ADAL, add it to the http authorization header, before making the call to access the To Do list service.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Call the To Do list service.
            HttpResponseMessage response = await httpClient.GetAsync(todoListBaseAddress + "/api/todolist");

            if (response.IsSuccessStatusCode)
            {

                // Read the response and databind to the GridView to display To Do items.
                string s = await response.Content.ReadAsStringAsync();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<TodoItem> toDoArray = serializer.Deserialize<List<TodoItem>>(s);

                TodoList.ItemsSource = toDoArray.Select(t => new { t.Title });
            }
            else
            {
                MessageBox.Show("An error occurred : " + response.ReasonPhrase);
            }

            return;
        }

        private async void AddTodoItem(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TodoText.Text))
            {
                MessageBox.Show("Please enter a value for the To Do item name");
                return;
            }

            //
            // Get an access token to call the To Do service.
            //
            AuthenticationResult result = null;
            try
            {
                result = authContext.AcquireToken(todoListResourceId, clientId, redirectUri, PromptBehavior.Never);
            }
            catch (ActiveDirectoryAuthenticationException ex)
            {
                // There is no access token in the cache, so prompt the user to sign-in.
                if (ex.ErrorCode == "user_interaction_required")
                {
                    MessageBox.Show("Please sign in first");
                    SignInButton.Content = "Sign In";
                }
                else
                {
                    // An unexpected error occurred.
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Inner Exception : " + ex.InnerException.Message;
                    }

                    MessageBox.Show(message);
                }

                return;
            }

            //
            // Call the To Do service.
            //
            TodoItem item = new TodoItem();
            item.Title = TodoText.Text;

            // Once the token has been returned by ADAL, add it to the http authorization header, before making the call to access the To Do service.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Forms encode Todo item, to POST to the todo list web api.
            HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Title", TodoText.Text) });

            // Call the To Do list service.
            HttpResponseMessage response = await httpClient.PostAsync(todoListBaseAddress + "/api/todolist", content);

            if (response.IsSuccessStatusCode)
            {
                TodoText.Text = "";
                GetTodoList();
            }
            else
            {
                MessageBox.Show("An error occurred : " + response.ReasonPhrase);
            }
        }

        private void SignIn(object sender = null, RoutedEventArgs args = null)
        {
            // If there is already a token in the cache, clear the cache and update the label on the button.
            if (SignInButton.Content.ToString() == "Clear Cache")
            {
                TodoList.ItemsSource = string.Empty;
                authContext.TokenCacheStore.Clear();
                SignInButton.Content = "Sign In";
                return;
            }

            //
            // Get an access token to call the To Do list service.
            //
            AuthenticationResult result = null;
            try
            {
                result = authContext.AcquireToken(todoListResourceId, clientId, redirectUri, PromptBehavior.Always);
                SignInButton.Content = "Clear Cache";
                GetTodoList();
            }
            catch (ActiveDirectoryAuthenticationException ex)
            {
                if (ex.ErrorCode == "authentication_canceled")
                {
                    MessageBox.Show("Sign in was canceled by the user");
                }
                else
                {
                    // An unexpected error occurred.
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Inner Exception : " + ex.InnerException.Message;
                    }

                    MessageBox.Show(message);
                }

                return;
            }

            // Uncomment this next line if you want to clear all of the state from the browser control used by ADAL.
            // ClearCookies();
        }

        //
        // This function clears cookies from the browser control used by ADAL.
        // It is provided here for reference in case you want to reset the cookie state of the browser control.
        // It is not used by default in this sample.
        //
        private void ClearCookies()
        {
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

    }
}
