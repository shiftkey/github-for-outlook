using System;
using System.Windows;
using GithubForOutlook.Logic.Modules.Settings;
using GithubForOutlook.Logic.Views;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using VSTOContrib.Core.RibbonFactory;
using VSTOContrib.Core.RibbonFactory.Interfaces;
using VSTOContrib.Core.Wpf;
using VSTOContrib.Outlook.RibbonFactory;
using VSTOContrib.Core.Extensions;

namespace GithubForOutlook.Logic.Ribbons.MainExplorer
{
    [RibbonViewModel(OutlookRibbonType.OutlookExplorer)]
    public class GithubExplorerRibbon : OfficeViewModelBase, IRibbonViewModel
    {
        readonly Func<AddNewIssueViewModel> getNewIssueViewModel;
        readonly Func<SettingsViewModel> getSettingsViewModel;

        Explorer explorer;

        public GithubExplorerRibbon(
            Func<AddNewIssueViewModel> getNewIssueViewModel,
            Func<SettingsViewModel> getSettingsViewModel)
        {
            this.getNewIssueViewModel = getNewIssueViewModel;
            this.getSettingsViewModel = getSettingsViewModel;
        }

        public void Initialised(object context)
        {

        }

        private void CleanupFolder()
        {

        }

        public void CreateIssue(IRibbonControl ribbonControl)
        {
            var viewModel = getNewIssueViewModel();
            viewModel.Title = selectedMailItem.Subject;
            var view = new AddNewIssueView { DataContext = viewModel };
            var window = new Window { Content = view };
            window.Show();

            viewModel.Initialize();
        }

        public void ShowSettings(IRibbonControl ribbonControl)
        {
            var viewModel = getSettingsViewModel();
            
            var view = new SettingsView { DataContext = viewModel };
            var window = new Window { Content = view };
            window.Show();
        }

        public void CurrentViewChanged(object currentView)
        {
            explorer = (Explorer)currentView;
            explorer.SelectionChange += ExplorerOnSelectionChange;
        }

        private void ExplorerOnSelectionChange()
        {
            using (var selection = explorer.Selection.WithComCleanup())
            {
                if (selection.Resource.Count == 1)
                {
                    object item = null;
                    MailItem mailItem = null;
                    try
                    {
                        item = selection.Resource[1];
                        mailItem = item as MailItem;
                        if (mailItem != null)
                        {
                            if (selectedMailItem != null)
                                selectedMailItem.ReleaseComObject();
                            selectedMailItem = mailItem;
                            MailItemSelected = true;
                        }
                        else
                        {
                            MailItemSelected = false;
                        }
                    }
                    finally
                    {
                        if (mailItem == null)
                            item.ReleaseComObject();
                    }
                }
                else
                {
                    MailItemSelected = false;
                }
            }
        }

        private bool mailItemSelected;
        private MailItem selectedMailItem;

        public bool MailItemSelected
        {
            get { return mailItemSelected; }
            set
            {
                mailItemSelected = value;
                RaisePropertyChanged(() => MailItemSelected);
            }
        }

        public void Cleanup()
        {
            CleanupFolder();
            explorer = null;
        }

        public IRibbonUI RibbonUi { get; set; }
    }
}