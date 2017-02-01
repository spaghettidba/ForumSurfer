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
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Reflection;

namespace ForumSurfer.ViewModel
{

    public class MainViewModel : ViewModelBase
    {

        #region ViewModelAttributes
        public TreeViewModel TreeModel
        {
            get
            {
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
                                Children = new SortableObservableCollection<TreeNodeViewModel>(Hosts),
                                IsSelected = true
                            }
                        }
                    };
            }
        }



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
                if (_selectedArticle != null && _selectedArticle.Unread)
                {
                    MarkArticleRead(_selectedArticle);
                }
                _selectedArticle = value;

                try
                {
                    var msg = new SendSetZoomMessage(_selectedArticle.ParentFeed.ParentHost.Zoom, _selectedArticle.ParentFeed.ParentHost.TextZoom);
                    Messenger.Default.Send<SendSetZoomMessage>(msg);
                }
                catch (Exception)
                {
                    Debug.Print("Unable to set zoom: host not found.");
                    //ignore
                }


                RaisePropertyChanged("SelectedArticle");
                RaisePropertyChanged("Articles");
            }
        }

        public Boolean IsBrowserVisible { get; set; }
        public Boolean IsOptionsVisible
        {
            get
            {
                return _optionsVisible;
            }
            set
            {
                _optionsVisible = value;
                IsBrowserVisible = !value;
                RaisePropertyChanged("IsBrowserVisible");
            }
        }


        public SortableObservableCollection<TreeNodeViewModel> Hosts
        {
            get
            {
                if (_allData == null)
                    return null;
                SortableObservableCollection<TreeNodeViewModel> _hosts = new SortableObservableCollection<TreeNodeViewModel>();
                foreach (Host h in _allData)
                {
                    _hosts.Add(new SimpleTreeNodeViewModel(h));
                }
                _hosts.Sort(el => el.SortKey, ListSortDirection.Ascending);
                return _hosts;
            }
        }

        private Data.Settings settings = new Data.Settings();

        public int AutoUpdateMinutes
        {
            get { return settings.AutoUpdateMinutes; }
            set
            {
                settings.AutoUpdateMinutes = value;
                settings.Save();
            }
        }
        public int RetentionDays
        {
            get { return settings.RetentionDays; }
            set
            {
                settings.RetentionDays = value;
                settings.Save();
            }

        }

        public String VersionInfo { get; private set; }

        public ObservableCollection<BoilerplateAnswer> BoilerplateAnswers { get; set; }
        public String StatusMessage { get; set; }
        #endregion

        #region privateVars
        private Thread _updaterThread;
        private Model.Article _selectedArticle;
        private Thread _uiThread = Thread.CurrentThread;
        private bool _updaterInPause = false;
        private SynchronizationContext _uiContext;
        private SortableObservableCollection<Model.Article> _articles;
        private TreeNodeViewModel _selectedNode;
        private List<Data.Host> _allData;
        private DialogCoordinator _dialogCoordinator;
        private bool _optionsVisible = false;
        private CollectionViewSource _sortedArticles = new CollectionViewSource();
        private View.BoilerplateDialog _boilerplate_dialog;
        private View.ZoomDialog _zoom_dialog;
        #endregion


        #region commands
        public ICommand SelectedItemChangedCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand MarkAllReadCommand { get; set; }
        public ICommand AddFeedCommand { get; set; }
        public ICommand AddBoilerplateCommand { get; set; }
        public ICommand MarkFeedReadCommand { get; set; }
        public ICommand DeleteFeedCommand { get; set; }
        public ICommand EditFeedCommand { get; set; }
        public ICommand ShowOptionsCommand { get; set; }
        public ICommand DeleteBoilerplateCommand { get; set; }
        public ICommand DoubleClickBoilerplateCommand { get; set; }
        public ICommand ImportOPMLCommand { get; set; }
        public ICommand ExportOPMLCommand { get; set; }
        #endregion

        public MainViewModel()
        {
            IsBrowserVisible = true;
            IsOptionsVisible = false;
            SelectedItemChangedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<object>>(SelectedItemChanged);
            LoadedCommand = new RelayCommand<RoutedEventArgs>(Loaded);
            MarkAllReadCommand = new RelayCommand<RoutedEventArgs>(MarkAllRead);
            AddFeedCommand = new RelayCommand<RoutedEventArgs>(AddFeed);
            AddBoilerplateCommand = new RelayCommand<RoutedEventArgs>(AddBoilerplate);
            DeleteBoilerplateCommand = new RelayCommand<BoilerplateAnswer>(param => DeleteBoilerplate(param));
            DoubleClickBoilerplateCommand = new RelayCommand<BoilerplateAnswer>(param => EditBoilerplate(param));
            MarkFeedReadCommand = new RelayCommand<RoutedEventArgs>(MarkFeedRead);
            DeleteFeedCommand = new RelayCommand<RoutedEventArgs>(DeleteFeed);
            EditFeedCommand = new RelayCommand<RoutedEventArgs>(EditFeed);
            ShowOptionsCommand = new RelayCommand<RoutedEventArgs>(ShowOptions);
            ImportOPMLCommand = new RelayCommand<RoutedEventArgs>(ImportOPML);
            ExportOPMLCommand = new RelayCommand<RoutedEventArgs>(ExportOPML);
            BoilerplateAnswers = new ObservableCollection<BoilerplateAnswer>();
            VersionInfo = "ForumSurfer Version " + Assembly.GetEntryAssembly().GetName().Version.ToString();
            _dialogCoordinator = DialogCoordinator.Instance;
        }



        //private void BoilerplateAnswers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == NotifyCollectionChangedAction.Remove)
        //    {
        //        foreach (object item in e.OldItems)
        //        {
        //            //BoilerplateAnswer ans = item as BoilerplateAnswer;
        //            //ans.Boilerplate.Delete();
        //        }
        //    }

        //    if (e.Action == NotifyCollectionChangedAction.Add)
        //    {
        //        foreach (object item in e.NewItems)
        //        {

        //        }
        //    }
        //}

        private void ShowOptions(RoutedEventArgs obj)
        {
            IsOptionsVisible = !IsOptionsVisible;
            RaisePropertyChanged("IsOptionsVisible");
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
            {
                if (tvm.Node is Model.Host)
                {
                    try
                    {
                        Model.Host selectedHost = (Model.Host)tvm.Node;
                        Model.Host prevHost = selectedHost;
                        IsBrowserVisible = false;
                        RaisePropertyChanged("IsBrowserVisible");
                        EditZoom(selectedHost);
                    }
                    catch (Exception ex)
                    {
                        await _dialogCoordinator.ShowMessageAsync(this, "Set Zoom Level", "Unable to set zoom: " + ex.Message);
                        IsBrowserVisible = true;
                        RaisePropertyChanged("IsBrowserVisible");
                    }
                }
                return;
            }


            // Hides browser otherwise dialog gets behind it
            IsBrowserVisible = false;
            RaisePropertyChanged("IsBrowserVisible");
            MetroDialogSettings dialogSettings = new MetroDialogSettings();
            dialogSettings.DefaultText = selectedFeed.Location.ToString();
            var FeedText = await _dialogCoordinator.ShowInputAsync(this, "Edit feed", "Enter the URL of the feed:", dialogSettings);
            if (FeedText != null)
            {
                string errMsg = null;
                try
                {
                    Uri feedUri = new Uri(FeedText);
                    Data.Feed f = new Data.Feed(selectedFeed);
                    f.Location = feedUri;
                    f.UpdateFromUri(true, RetentionDays);
                    f.Save(true);
                    InitializeData(true);
                }
                catch (Exception ex)
                {
                    errMsg = ex.Message;
                }
                if (errMsg != null)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Edit Feed", "Unable to edit feed with the supplied URL: " + errMsg);
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
                string errMsg = null;
                try
                {
                    Data.Feed feed = new Data.Feed(selectedFeed);
                    feed.Delete();
                }
                catch (Exception ex)
                {
                    errMsg = ex.Message;
                }
                if (errMsg != null)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Edit Feed", "Unable to edit feed with the supplied URL: " + errMsg);
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

            settings.Load();
            RaisePropertyChanged("AutoUpdateMinutes");
            RaisePropertyChanged("RetentionDays");

            Data.Article.PurgeOlderItems(RetentionDays);
            InitializeData(true);

            Thread.CurrentThread.Name = "UI";
            Debug.Print(Thread.CurrentThread.Name);
            _updaterThread = new Thread(() => UpdaterDelegate());
            _updaterThread.Start();

            //BoilerplateAnswers.CollectionChanged += BoilerplateAnswers_CollectionChanged;
            InitializeBoilerPlate();
        }

        private void InitializeBoilerPlate()
        {
            List<Data.Boilerplate> allItems = Data.Boilerplate.LoadAll();

            BoilerplateAnswers.Clear();

            foreach (Data.Boilerplate item in allItems)
            {
                BoilerplateAnswers.Add(new BoilerplateAnswer(item, () => { boilerPlateSelected(item); }));
            }

        }

        private void boilerPlateSelected(Data.Boilerplate item)
        {
            var msg = new SendBoilerplateMessage(item.Text);
            Messenger.Default.Send<SendBoilerplateMessage>(msg);
        }

        private async void MarkAllRead(RoutedEventArgs e)
        {
            // Hides browser otherwise dialog gets behind it
            IsBrowserVisible = false;
            RaisePropertyChanged("IsBrowserVisible");
            try
            {
                MessageDialogResult x = await _dialogCoordinator.ShowMessageAsync(this, "Mark all as read", "This will mark all feeds are read. Are you sure?", MessageDialogStyle.AffirmativeAndNegative);
                if (x.Equals(MessageDialogResult.Affirmative))
                {
                    Data.Article.MarkAllRead();
                    if (Articles != null)
                    {
                        foreach (Article a in Articles)
                        {
                            a.Unread = false;
                        }
                    }
                }
            }
            finally
            {
                IsBrowserVisible = true;
                RaisePropertyChanged("IsBrowserVisible");
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
                string errMsg = null;
                try
                {
                    Uri feedUri = new Uri(FeedText);
                    Data.Feed f = new Data.Feed();
                    f.Location = feedUri;
                    f.UpdateFromUri(true, RetentionDays);
                    f.Save(true);
                    InitializeData(true);
                }
                catch (Exception ex)
                {
                    errMsg = ex.Message;
                }
                if (errMsg != null)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Add Feed", "Unable to create a feed for the supplied URL: " + errMsg);
                }
            }
            IsBrowserVisible = true;
            RaisePropertyChanged("IsBrowserVisible");

        }

        private async void AddBoilerplate(RoutedEventArgs obj)
        {
            _boilerplate_dialog = new View.BoilerplateDialog();
            BoilerplateEditorViewModel bpdc = new BoilerplateEditorViewModel() { Text = "", Title = "", Context = this, Dialog = _boilerplate_dialog };
            _boilerplate_dialog.DataContext = bpdc;
            await _dialogCoordinator.ShowMetroDialogAsync(this, _boilerplate_dialog);
        }



        public async void UpdateBoilerplate(Data.Boilerplate bp)
        {
            BoilerplateEditorViewModel bpdc = (BoilerplateEditorViewModel)_boilerplate_dialog.DataContext;
            if (!bpdc.Cancel && bpdc.Exception == null)
            {
                BoilerplateAnswer bpa = BoilerplateAnswers.FirstOrDefault(item => item.Title.Equals(bp.Title));
                if (bpa == null)
                {
                    BoilerplateAnswers.Add(new BoilerplateAnswer(bp, () => { boilerPlateSelected(bp); }));
                }
                else
                {
                    // this won't update the text in the datagrid though...
                    bpa.Text = bp.Text;
                    bpa.Title = bp.Title;
                }
            }
            if (bpdc.Exception != null)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Add Boilerplate Answer", "Unable to save: " + bpdc.Exception.Message);
            }
        }


        private async void EditBoilerplate(BoilerplateAnswer param)
        {
            _boilerplate_dialog = new View.BoilerplateDialog();
            BoilerplateEditorViewModel bpdc = new BoilerplateEditorViewModel() { Text = param.Text, Title = param.Title, Context = this, Dialog = _boilerplate_dialog };
            _boilerplate_dialog.DataContext = bpdc;
            await _dialogCoordinator.ShowMetroDialogAsync(this, _boilerplate_dialog);
        }

        private async void DeleteBoilerplate(BoilerplateAnswer param)
        {
            MessageDialogResult result = await _dialogCoordinator.ShowMessageAsync(this, "Delete Boilerplate Answer", "Are you sure?", MessageDialogStyle.AffirmativeAndNegative);

            if (result.Equals(MessageDialogResult.Affirmative))
            {
                try
                {
                    var theItem = BoilerplateAnswers.FirstOrDefault(item => item.Title.Equals(param.Title));
                    BoilerplateAnswers.Remove(theItem);
                    param.Boilerplate.Delete();
                }
                catch (Exception e)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Delete Boilerplate Answer", "Unable to delete: " + e.Message);
                }
            }
        }


        public async void UpdateZoom(int PageZoom, int TextZoom, Host host)
        {
            try
            {
                ZoomEditorViewModel bpdc = (ZoomEditorViewModel)_zoom_dialog.DataContext;
                if (!bpdc.Cancel && bpdc.Exception == null)
                {
                    host.Zoom = PageZoom;
                    host.TextZoom = TextZoom;
                    Data.Host dh = new Data.Host(host);
                    dh.Save();

                    Data.Host selectedHost = Data.Host.Load(host.Location.Host.ToString());

                    IsBrowserVisible = true;
                    RaisePropertyChanged("IsBrowserVisible");

                    var m = new SendSetZoomMessage(selectedHost.Zoom, selectedHost.TextZoom);
                    m.SetImmediately = true;
                    Messenger.Default.Send<SendSetZoomMessage>(m);
                }
                if (bpdc.Exception != null)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Update Zoom", "Unable to save: " + bpdc.Exception.Message);
                }
            }
            finally
            {
                IsBrowserVisible = true;
                RaisePropertyChanged("IsBrowserVisible");
            }
        }


        private async void EditZoom(Host param)
        {
            _zoom_dialog = new View.ZoomDialog();
            ZoomEditorViewModel bpdc = new ZoomEditorViewModel() { PageZoom = param.Zoom, TextZoom = param.TextZoom, Host = param, Context = this, Dialog = _zoom_dialog };
            _zoom_dialog.DataContext = bpdc;
            await _dialogCoordinator.ShowMetroDialogAsync(this, _zoom_dialog);
        }


        private void SelectedItemChanged(RoutedPropertyChangedEventArgs<object> obj)
        {
            object selected = obj.NewValue;

            if (selected == null)
                return;

            if (selected is SimpleTreeNodeViewModel)
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
                    foreach (Model.Feed feed in ((Model.Host)tvm.Node).Feeds)
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
                    foreach (Model.Host h in _allData)
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
            SetStatusMessage("Loading");
            //
            // Read from Database
            //
            _allData = Data.Host.LoadAll();

            SetStatusMessage("Loaded");

            //
            // Update the treeview
            //

            if (refreshTreeView)
            {
                //Set selected item
                TreeModel.Items[0].IsSelected = true;
                RaisePropertyChanged("TreeModel");
                SelectedItemChanged(new RoutedPropertyChangedEventArgs<object>(null, TreeModel.Items[0]));
            }

            //
            // Update articles
            //
            if (_selectedNode != null)
            {
                SimpleTreeNodeViewModel tvm = (SimpleTreeNodeViewModel)_selectedNode;
                if (tvm.Node is Model.Feed)
                {
                    SetStatusMessage("Updating Articles (feed)... ");
                    Model.Feed selectedFeed = (Model.Feed)tvm.Node;
                    // Find the feed and add the Articles
                    foreach (Host h in _allData)
                    {
                        Feed f = h.Feeds.FirstOrDefault(el => el.Location.Equals(selectedFeed.Location));
                        if (f != null)
                        {
                            Articles.AddMissing(f.Articles, new ArticleEqualityComparer(), _uiContext);
                        }
                    }

                }
                else if (tvm.Node is Model.Host)
                {
                    SetStatusMessage("Updating Articles (host)... ");
                    Model.Host selectedHost = (Model.Host)tvm.Node;
                    // Find the host and add the Articels
                    Model.Host h = _allData.FirstOrDefault(el => el.Location.Equals(selectedHost.Location));
                    foreach (Model.Feed feed in h.Feeds)
                    {
                        Articles.AddMissing(feed.Articles, new ArticleEqualityComparer(), _uiContext);
                    }
                }
                else
                {
                    SetStatusMessage("Updating Articles (article)... ");
                    foreach (Model.Host h in _allData)
                    {
                        foreach (Model.Feed feed in h.Feeds)
                        {
                            Articles.AddMissing(feed.Articles, new ArticleEqualityComparer(), _uiContext);
                        }
                    }
                }
            }
            if (refreshTreeView)
            {
                SetStatusMessage("Refreshing Article list");
                SetSortOrder();
            }
            SetStatusMessage("Ready");
        }


        public void SetSortOrder()
        {
            SortedArticles.SortDescriptions.Clear(); // Clear all 
            SortedArticles.SortDescriptions.Add(new SortDescription("SortKey", ListSortDirection.Descending)); // Sort descending by "PropertyName"
        }




        private void UpdateData()
        {
            try
            {
                InitializeData(false);
            }
            catch (Exception e)
            {
                StatusMessage = e.StackTrace;
            }
        }


        private void UpdaterDelegate()
        {
            Task databaseTask = null;
            while (_uiThread.IsAlive)
            {
                Debug.Print(Thread.CurrentThread.Name);
                if ((databaseTask == null || databaseTask.IsCompleted) && !_updaterInPause)
                {
                    databaseTask = Task.Factory.StartNew(() =>
                    {
                        Data.Feed.UpdateAll(RetentionDays);
                        if (_uiContext != null)
                        {
                            UpdateData();
                            _uiContext.Send(DivideByZeroException => SetSortOrder(), null);
                        }
                    },
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskScheduler.Default);
                }
                for (int i = 0; i < AutoUpdateMinutes * 6000; i++)
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

        void SetStatusMessage(String s)
        {
            Debug.Print(DateTime.Now + " " + s);
            StatusMessage = s;
            RaisePropertyChanged("StatusMessage");
        }


        private async void ImportOPML(RoutedEventArgs e)
        {
            try
            {
                OPML opml = new OPML();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.DefaultExt = ".opml";
                openFileDialog.Filter = "OPML Files (*.opml)|*.opml";

                Exception exc = null;

                if (openFileDialog.ShowDialog() == true)
                {
                    List<Feed> t = opml.Import(openFileDialog.FileName);
                    foreach (Feed f in t)
                    {
                        try
                        {
                            Data.Feed feed = new Data.Feed(f);
                            feed.Location = f.Location;
                            feed.Save(false);
                        }
                        catch (Exception ee)
                        {
                            exc = ee;
                        }
                    }

                    // Reload tree
                    InitializeData(true);


                    await _dialogCoordinator.ShowMessageAsync(this, "Import OPML", "OPML imported successfully. Allow some minutes to let feed titles update from the RSS feed.");

                    // Update details from the uri
                    Task updaterTask = Task.Factory.StartNew(async () =>
                    {
                        _updaterInPause = true;
                        foreach (Feed f in t)
                        {
                            try
                            {
                                Data.Feed feed = new Data.Feed(f);
                                feed.Location = f.Location;
                                feed.UpdateFromUri(true, RetentionDays);
                                feed.Save(true);
                            }
                            catch (Exception ee)
                            {
                                exc = ee;
                            }
                        }
                        _updaterInPause = false;

                        InitializeData(true);

                        if (exc != null)
                            await _dialogCoordinator.ShowMessageAsync(this, "Import OPML", "Unable to update feeds: " + exc.Message);

                    },
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskScheduler.Default);


                    if (exc != null)
                        throw exc;
                }
            }
            catch (Exception ex)
            {
                IsBrowserVisible = false;
                RaisePropertyChanged("IsBrowserVisible");
                await _dialogCoordinator.ShowMessageAsync(this, "Import OPML", "Unable to import: " + ex.Message);
            }
        }

        private async void ExportOPML(RoutedEventArgs e)
        {
            OPML opml = new OPML();

            try
            {
                XDocument doc = opml.Export(_allData);
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = ".opml";
                sfd.Filter = "OPML Files (*.opml)|*.opml";

                if (sfd.ShowDialog() == true)
                {
                    doc.Save(sfd.FileName);
                }
            }
            catch (Exception ex)
            {
                IsBrowserVisible = false;
                RaisePropertyChanged("IsBrowserVisible");
                await _dialogCoordinator.ShowMessageAsync(this, "Export OPML", "Unable to export: " + ex.Message);
            }
        }
    }
}