using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Analects.SettingsService;
using NGitHub;
using NGitHub.Models;
using VSTOContrib.Core.Wpf;
using Repository = GithubForOutlook.Logic.Models.Repository;

namespace GithubForOutlook.Logic.Views
{
    public class AddNewIssueViewModel : OfficeViewModelBase
    {
        // TODO: @JakeGinnivan how should we handle potential cross-thread issues?

        private readonly IGitHubClient client;
        private readonly ISettingsService settingsService;

        public AddNewIssueViewModel(IGitHubClient client, ISettingsService settingsService)
        {
            this.client = client;
            this.settingsService = settingsService;
        }

        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private ObservableCollection<Repository> repositories;

        public ObservableCollection<Repository> Repositories
        {
            get { return repositories; }
            set
            {
                repositories = value;
                RaisePropertyChanged(() => Repositories);
            }
        }

        public void Initialize()
        {
            // TODO: how can we find the # of repos a user has?
            client.Repositories.GetRepositoriesAsync(settingsService.Get<string>("UserName"), 1, RepositoryTypes.Public, OnResult, OnError);
        }

        private void OnError(GitHubException obj)
        {
            // TODO: logging
        }

        private void OnResult(IEnumerable<NGitHub.Models.Repository> results)
        {
            Repositories = new ObservableCollection<Repository>();

            foreach (var result in results)
            {
                var repo = new Repository { Name = result.FullName };
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    new Action(() => Repositories.Add(repo)), DispatcherPriority.Background);
            }
        }
    }
}
