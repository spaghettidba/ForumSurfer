using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System;
using ForumSurfer.Collections;
using System.Linq;
using ForumSurfer.Model;
using System.Diagnostics;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using MahApps.Metro.Controls.Dialogs;

namespace ForumSurfer.ViewModel
{

    public class MainViewModel : ViewModelBase
    {

        #region ViewModelAttributes
        public TreeViewModel TreeModel {
            get {
                if (Hosts == null)
                    return null;
                else
                    return new TreeViewModel
                    {
                        Items = new SortableObservableCollection<TreeNodeViewModel>
                        {
                            new SimpleTreeNodeViewModel()
                            {
                                Title = "Feeds",
                                Children = new SortableObservableCollection<TreeNodeViewModel>(Hosts)
                            }
                        }
                    };
            }
        }


        private CollectionViewSource _sortedArticles = new CollectionViewSource();
        public CollectionViewSource SortedArticles
        {
            get
            {
                _sortedArticles.Source = this._articles; // Set source to our original ObservableCollection
                return _sortedArticles;
            }
            set
            {
                if (value != _sortedArticles)
                {
                    _sortedArticles = value;
                    RaisePropertyChanged("SortedArticles"); // MVVMLight ObservableObject
                }
            }
        }

        public SortableObservableCollection<Model.Article> Articles
        {
            get
            {
                return _articles;
            }
            set
            {
                _articles = value;
            }
        }
        public Model.Article SelectedArticle
        {
            get
            {
                return _selectedArticle;
            }
            set
            {
                if(_selectedArticle != null && _selectedArticle.Unread)
                {
                    MarkArticleRead(_selectedArticle);
                }
                _selectedArticle = value;
                RaisePropertyChanged("SelectedArticle");
                RaisePropertyChanged("Articles");
            }
        }

        public Boolean IsBrowserVisible { get; set; } = true;


        public SortableObservableCollection<TreeNodeViewModel> Hosts
        {
            get
            {
                if (_allData == null)
                    return null;
                SortableObservableCollection<TreeNodeViewModel> _hosts = new SortableObservableCollection<TreeNodeViewModel>();
                foreach(Host h in _allData)
                {
                    _hosts.Add(new SimpleTreeNodeViewModel(h));
                }
                _hosts.Sort(el => el.SortKey, ListSortDirection.Ascending);
                return _hosts;
            }
        }

        public int AutoUpdateSeconds = 60;
        #endregion

        #region privateVars
        private Thread _updaterThread;
        private Model.Article _selectedArticle;
        private Thread _uiThread = Thread.CurrentThread;
        private SynchronizationContext _uiContext;
        private SortableObservableCollection<Model.Article> _articles;
        private TreeNodeViewModel _selectedNode;
        private List<Data.Host> _allData;
        private DialogCoordinator _dialogCoordinator;
        #endregion


        #region commands
        public ICommand SelectedItemChangedCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand MarkAllReadCommand { get; set; }
        public ICommand AddFeedCommand { get; set; }
        public ICommand MarkFeedReadCommand { get; set; }
        public ICommand DeleteFeedCommand { get; set; }
        public ICommand EditFeedCommand { get; set; }
        #endregion

        public MainViewModel()
        {
            SelectedItemChangedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<object>>(SelectedItemChanged);
            LoadedCommand = new RelayCommand<RoutedEventArgs>(Loaded);
            MarkAllReadCommand = new RelayCommand<RoutedEventArgs>(MarkAllRead);
            AddFeedCommand = new RelayCommand<RoutedEventArgs>(AddFeed);
            MarkFeedReadCommand = new RelayCommand<RoutedEventArgs>(MarkFeedRead);
            DeleteFeedCommand = new RelayCommand<RoutedEventArgs>(DeleteFeed);
            EditFeedCommand = new RelayCommand<RoutedEventArgs>(EditFeed);
            _dialogCoordinator = DialogCoordinator.Instance;
        }

        private async void EditFeed(RoutedEventArgs e)
        {
            if (_selectedNode == null)
                return;
            SimpleTreeNodeViewModel tvm = (SimpleTreeNodeViewModel)_selectedNode;
            Model.Feed selectedFeed = null;
            if (tvm.Node is Model.Feed)
            {
                selectedFeed = (Model.Feed)tvm.Node;
            }
            else
                return;

            // Hides browser otherwise dialog gets behind it
            IsBrowserVisible = false;
            RaisePropertyChanged("IsBrowserVisible");
            var FeedText = await _dialogCoordinator.ShowInputAsync(this, "Edit feed", "Enter the URL of the feed:");
            if (FeedText != null)
            {
                try
                {
                    Uri feedUri = new Uri(FeedText);
                    Data.Feed f = new Data.Feed(selectedFeed);
                    f.Location = feedUri;
                    f.UpdateFromUri(true);
                    f.Save(true);
                    InitializeData(true);
                }
                catch (Exception ex)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Edit Feed", "Unable to edit feed with the supplied URL: " + ex.Message);
                }
            }
            IsBrowserVisible = true;
            RaisePropertyChanged("IsBrowserVisible");
        }

        private async void DeleteFeed(RoutedEventArgs e)
        {
            if (_selectedNode == null)
                return;
            SimpleTreeNodeViewModel tvm = (SimpleTreeNodeViewModel)_selectedNode;
            Model.Feed selectedFeed = null;
            if (tvm.Node is Model.Feed)
            {
                selectedFeed = (Model.Feed)tvm.Node;
            }
            else
                return;


            // Hides browser otherwise dialog gets behind it
            IsBrowserVisible = false;
            RaisePropertyChanged("IsBrowserVisible");

            MessageDialogResult result = await _dialogCoordinator.ShowMessageAsync(this, "Delete Feed", "This action also deletes all downloaded articles and cannot be undone. Are you sure?", MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
                try
                {
                    Data.Feed feed = new Data.Feed(selectedFeed);
                    feed.Delete();
                }
                catch (Exception ex)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Edit Feed", "Unable to edit feed with the supplied URL: " + ex.Message);
                }
            }
            IsBrowserVisible = true;
            RaisePropertyChanged("IsBrowserVisible");
            InitializeData(true);
        }

        private void MarkFeedRead(RoutedEventArgs e)
        {
            if (_selectedNode == null)
                return;
            SimpleTreeNodeViewModel tvm = (SimpleTreeNodeViewModel)_selectedNode;
            if (tvm.Node is Model.Feed)
            {
                Model.Feed selectedFeed = (Model.Feed)tvm.Node;
                Data.Feed df = new Data.Feed(selectedFeed);
                df.MarkAllRead();
            }
            else if (tvm.Node is Model.Host)
            {
                Model.Host selectedHost = (Model.Host)tvm.Node;
                Data.Host dh = new Data.Host(selectedHost);
                dh.MarkAllRead();
            }
            else
                return;

            foreach (Article a in Articles)
            {
                a.Unread = false;
            }
        }

        private void Loaded(RoutedEventArgs e)
        {
            _uiContext = SynchronizationContext.Current;
            Data.Repository.CreateDatabase();
            InitializeData(true);
            _updaterThread = new Thread(() => UpdaterDelegate());
            _updaterThread.Start();
        }

        private void MarkAllRead(RoutedEventArgs e)
        {
            Data.Article.MarkAllRead();
            foreach(Article a in Articles)
            {
                a.Unread = false;
            }
        }

        private async void AddFeed(RoutedEventArgs e)
        {
            // Hides browser otherwise dialog gets behind it
            IsBrowserVisible = false;
            RaisePropertyChanged("IsBrowserVisible");
            var FeedText = await _dialogCoordinator.ShowInputAsync(this, "Add Feed", "Enter the URL of the feed:");
            if (FeedText != null)
            {
                try
                {
                    Uri feedUri = new Uri(FeedText);
                    Data.Feed f = new Data.Feed();
                    f.Location = feedUri;
                    f.UpdateFromUri(true);
                    f.Save(true);
                    InitializeData(true);
                }
                catch (Exception ex)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Add Feed", "Unable to create a feed for the supplied URL: " + ex.Message);
                }
            }
            IsBrowserVisible = true;
            RaisePropertyChanged("IsBrowserVisible");

        }

        private void SelectedItemChanged(RoutedPropertyChangedEventArgs<object> obj)
        {
            object selected = obj.NewValue;

            if (selected == null)
                return;

            if(selected is SimpleTreeNodeViewModel)
            {
                SimpleTreeNodeViewModel tvm = (SimpleTreeNodeViewModel)selected;
                _selectedNode = tvm;
                if (tvm.Node is Model.Feed)
                {
                    Articles = new SortableObservableCollection<Model.Article>(((Model.Feed)tvm.Node).Articles);
                }
                else if (tvm.Node is Model.Host)
                {
                    List<Model.Article> intermediate = new List<Model.Article>();
                    foreach(Model.Feed feed in ((Model.Host)tvm.Node).Feeds)
                    {
                        foreach (Model.Article a in feed.Articles)
                        {
                            intermediate.Add(a);
                        }
                    }
                    Articles = new SortableObservableCollection<Model.Article>(intermediate);
                }
                else
                {
                    if (_articles == null)
                        _articles = new SortableObservableCollection<Article>();
                    _articles.Clear();
                    foreach(Model.Host h in _allData)
                    {
                        foreach (Model.Feed feed in h.Feeds)
                        {
                            foreach (Model.Article a in feed.Articles)
                            {
                                _articles.Add(a);
                            }
                        }
                    }
                }
                SortedArticles.SortDescriptions.Clear(); // Clear all 
                SortedArticles.SortDescriptions.Add(new SortDescription("SortKey", ListSortDirection.Descending)); // Sort descending by "PropertyName"
                //Articles.OrderByDescending(el => el.SortKey);
            }
            RaisePropertyChanged("SortedArticles");
        }



        private void InitializeData(Boolean refreshTreeView)
        {
            //
            // Read from Database
            //
            _allData = Data.Host.LoadAll();

            //
            // Update the treeview
            //

            if (refreshTreeView)
                RaisePropertyChanged("TreeModel");

            //
            // Update articles
            //
            if(_selectedNode != null)
            {
                SimpleTreeNodeViewModel tvm = (SimpleTreeNodeViewModel)_selectedNode;
                if (tvm.Node is Model.Feed)
                {
                    Model.Feed selectedFeed = (Model.Feed)tvm.Node;
                    // Find the feed and add the Articles
                    foreach(Host h in _allData)
                    {
                        Feed f = h.Feeds.FirstOrDefault(el => el.Location.Equals(selectedFeed.Location));
                        if(f != null)
                        {
                            Articles.AddMissing(f.Articles, new ArticleEqualityComparer());
                        }
                    }

                }
                else if (tvm.Node is Model.Host)
                {
                    Model.Host selectedHost = (Model.Host)tvm.Node;
                    // Find the host and add the Articels
                    Model.Host h = _allData.FirstOrDefault(el => el.Location.Equals(selectedHost.Location));
                    foreach (Model.Feed feed in h.Feeds)
                    {
                        Articles.AddMissing(feed.Articles, new ArticleEqualityComparer());
                    }
                }
                else
                {
                    foreach(Model.Host h in _allData)
                    {
                        foreach (Model.Feed feed in h.Feeds)
                        {
                            Articles.AddMissing(feed.Articles, new ArticleEqualityComparer());
                        }
                    }
                }
                SortedArticles.SortDescriptions.Clear(); // Clear all 
                SortedArticles.SortDescriptions.Add(new SortDescription("SortKey", ListSortDirection.Descending)); // Sort descending by "PropertyName"
                //Articles.OrderByDescending(el => el.SortKey);
            }
            //RaisePropertyChanged("Articles");
        }


        private void UpdateData()
        {
            InitializeData(false);
        }


        private void UpdaterDelegate()
        {
            Task databaseTask = null;
            while(_uiThread.IsAlive)
            {

                if (databaseTask == null || databaseTask.IsCompleted)
                {
                    databaseTask = Task.Run(() =>
                    {
                        Data.Feed.UpdateAll();
                        if (_uiContext != null)
                        {
                            _uiContext.Send(DivideByZeroException => UpdateData(), null);
                        }
                    });
                }
                for(int i =0;i< AutoUpdateSeconds * 100; i++)
                {
                    Thread.Sleep(10);
                    if (!_uiThread.IsAlive)
                        break;
                }
            }
        }


        private void MarkArticleRead(Article art)
        {
            art.Unread = false;
            Data.Article dataArticle = new Data.Article(art);
            dataArticle.Save();
        }


        void Articles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Model.Article item in e.NewItems)
                    item.PropertyChanged += Article_PropertyChanged;

            if (e.OldItems != null)
                foreach (Model.Article item in e.OldItems)
                    item.PropertyChanged -= Article_PropertyChanged;
        }

        void Article_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Unread")
                Debug.Print("Unread has changed!");
        }
    }
}