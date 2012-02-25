using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using GithubForOutlook.Logic.Models;
using NGitHub;
using NGitHub.Authentication;
using Newtonsoft.Json;
using RestSharp;
using VSTOContrib.Core.Wpf;

namespace GithubForOutlook.Logic.Modules.Settings
{
    // TODO: is there a "Loaded" event in VSTOContrib to mimic the Activate/Deactivate hooks inside CM?

    public class SettingsViewModel : OfficeViewModelBase
    {
        private readonly IGitHubOAuthAuthorizer authorizer;
        private readonly IGitHubClient client;

        public SettingsViewModel(IGitHubOAuthAuthorizer authorizer, IGitHubClient client)
        {
            this.authorizer = authorizer;
            this.client = client;
        }

        private bool trackIssues;
        public bool TrackIssues
        {
            get { return trackIssues; }
            set
            {
                trackIssues = value;
                RaisePropertyChanged(() => TrackIssues);
            }
        }

        private bool trackPullRequests;
        public bool TrackPullRequests
        {
            get { return trackPullRequests; }
            set
            {
                trackPullRequests = value;
                RaisePropertyChanged(() => TrackPullRequests);
            }
        }

        private User user;
        private bool showAuthenticateButton;
        private string authenticationSecret;

        public User User
        {
            get { return user; }
            set
            {
                user = value;
                RaisePropertyChanged(() => User);
            }
        }

        public ICommand SignInCommand { get { return new DelegateCommand(SignIn); } }

        public void SignIn()
        {
            // TODO: settings provider
            // TODO: landing page at Code52 to get user to paste auth credentials in
            var url = authorizer.BuildAuthenticationUrl("9e96382c3109d9f35371", "http://code52.org");
            Process.Start(url);

            ShowAuthenticateButton = true;
        }

        public bool ShowAuthenticateButton
        {
            get { return showAuthenticateButton; }
            set
            {
                showAuthenticateButton = value;
                RaisePropertyChanged(() => ShowAuthenticateButton);
                AuthenticateCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand AuthenticateCommand { get { return new DelegateCommand(Authenticate, CanAuthenticate); } }

        private bool CanAuthenticate()
        {
            return !string.IsNullOrWhiteSpace(AuthenticationSecret);
        }

        public string AuthenticationSecret
        {
            get { return authenticationSecret; }
            set
            {
                authenticationSecret = value;
                RaisePropertyChanged(() => AuthenticationSecret);
                AuthenticateCommand.RaiseCanExecuteChanged();
            }
        }

        private void Authenticate()
        {
            var request = new RestRequest("https://github.com/login/oauth/access_token", Method.POST);

            request.AddHeader("Content-Type", "text/html");
            request.AddParameter("client_id", "9e96382c3109d9f35371");
            request.AddParameter("redirect_uri", "http://code52.org");
            request.AddParameter("client_secret", "60d6c49b946ba4ddc52a34aa0dc1cf43e6077ba6");
            request.AddParameter("code", AuthenticationSecret);

            var restClient = new RestClient();
            restClient.ExecuteAsync(request, OnAuthenticateCompleted);
        }

        private void OnAuthenticateCompleted(RestResponse arg1, RestRequestAsyncHandle arg2)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<dynamic>(arg1.Content);
                string token = result.access_token;
                client.Authenticator = new OAuth2UriQueryParameterAuthenticator(token);
                client.Users.GetAuthenticatedUserAsync(MapUser, LogError);
            }
            catch (Exception ex)
            {
                // TODO: notify that the code was not successful
            }
        }

        private void LogError(GitHubException obj)
        {

        }

        private void MapUser(NGitHub.Models.User obj)
        {
            User = new User
            {
                Name = obj.Login,
                Icon = obj.AvatarUrl
            };
        }

        public ICommand ClearCommand { get { return new DelegateCommand(Clear); } }

        public void Clear()
        {
            User = null;
        }
    }
}
