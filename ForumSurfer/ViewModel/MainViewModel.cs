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

namespace ForumSurfer.ViewModel
{

    public class MainViewModel : ViewModelBase
    {

        #region ViewModelAttributes
        public TreeViewModel TreeModel {
            get {
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


        public SortableObservableCollection<TreeNodeViewModel> Hosts
        {
            get
            {
                return _hosts;
            }
        }

        public int AutoUpdateSeconds = 60;
        #endregion

        #region privateVars
        private Thread _updaterThread;
        private SortableObservableCollection<TreeNodeViewModel> _hosts = new SortableObservableCollection<TreeNodeViewModel>();
        private Model.Article _selectedArticle;
        private Thread _uiThread = Thread.CurrentThread;
        private SynchronizationContext _uiContext;
        private SortableObservableCollection<Model.Article> _articles;
        #endregion


        #region commands
        public ICommand SelectedItemChangedCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        #endregion

        public MainViewModel()
        {
            SelectedItemChangedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<object>>(SelectedItemChanged);
            LoadedCommand = new RelayCommand<RoutedEventArgs>(Loaded);
            InitializeData();
            _updaterThread = new Thread(() => UpdaterDelegate());
            _updaterThread.Start();
        }


        private void Loaded(RoutedEventArgs e)
        {
            _uiContext = SynchronizationContext.Current;
        }

        private void SelectedItemChanged(RoutedPropertyChangedEventArgs<object> obj)
        {
            object selected = obj.NewValue;

            if (selected == null)
                return;

            if(selected is SimpleTreeNodeViewModel)
            {
                SimpleTreeNodeViewModel tvm = (SimpleTreeNodeViewModel)selected;
                if(tvm.Node is Model.Feed)
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
                    foreach(SimpleTreeNodeViewModel item in tvm.Children)
                    {
                        Model.Host h = (Model.Host)(item.Node);
                        List<Model.Article> intermediate = new List<Model.Article>();
                        foreach (Model.Feed feed in h.Feeds)
                        {
                            foreach (Model.Article a in feed.Articles)
                            {
                                intermediate.Add(a);
                            }
                        }
                        Articles = new SortableObservableCollection<Model.Article>(intermediate);
                    }
                }
                Articles.OrderByDescending(el => el.SortKey);
            }
            RaisePropertyChanged("Articles");
        }



        private void InitializeData()
        {
            List<Data.Host> allHosts = Data.Host.LoadAll();
            foreach(Data.Host host in allHosts)
            {
                var theHost = _hosts.FirstOrDefault(el => el.Id.Equals(host.Location));
                if (theHost == null)
                {
                    _hosts.Add(new SimpleTreeNodeViewModel(host));
                }
                else
                {
                    SortableObservableCollection<TreeNodeViewModel> newChildren = new SortableObservableCollection<TreeNodeViewModel>();
                    foreach(Model.Feed f in host.Feeds)
                    {
                        newChildren.Add(new SimpleTreeNodeViewModel(f));
                    }
                    theHost.Children.AddMissing(newChildren, new TreeNodeEqualityComparer());
                    theHost.Children.OrderByDescending(el => el.SortKey);
                }
            }
            RaisePropertyChanged("TreeModel");
        }


        private void UpdateData()
        {
            InitializeData();
        }


        private void UpdaterDelegate()
        {
            while(_uiThread.IsAlive)
            {
                if(_uiContext != null)
                {
                    _uiContext.Send(DivideByZeroException => UpdateData(), null);
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