using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using synapse.Models;
using synapse.Models.Events;
using synapse.Services;
using synapse.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace synapse.ViewModels
{
    public partial class ClipboardHistoryViewModel : ObservableObject, IDisposable
    {
        private readonly IDataService _dataService;
        private readonly IClipboardService _clipboardService;
        private readonly IApplicationService _applicationService;
        private readonly IWindowManager _windowManager;
        private readonly IEventBus _eventBus;
        private readonly ISearchService _searchService;
        private readonly IProgressiveSearchService _progressiveSearchService;
        private readonly Dispatcher _dispatcher;
        private readonly IDisposable _eventSubscription;
        private List<ClipboardItem> _masterHistoryList = new();
        private readonly ObservableCollection<ClipboardItem> _filteredItems = new();
        private readonly CollectionViewSource _groupedViewSource;
        private readonly DispatcherTimer _searchDebounceTimer;
        private CancellationTokenSource? _searchCancellationTokenSource;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSelectedImageVisible))]
        [NotifyPropertyChangedFor(nameof(IsSelectedTextVisible))]
        [NotifyPropertyChangedFor(nameof(IsSelectedCodeVisible))]
        [NotifyPropertyChangedFor(nameof(IsSelectedUrlVisible))]
        private ClipboardItem? _selectedItem;


        private string? _searchTerm;
        private string? _actualSearchTerm;

        /// <summary>
        /// The search term entered by the user (immediate updates for UI binding)
        /// </summary>
        public string? SearchTerm
        {
            get => _searchTerm;
            set
            {
                SetProperty(ref _searchTerm, value);
                // Start/restart the debounce timer
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Start();
            }
        }

        /// <summary>
        /// The actual search term used for filtering (debounced)
        /// </summary>
        public string? ActualSearchTerm
        {
            get => _actualSearchTerm;
            private set
            {
                if (SetProperty(ref _actualSearchTerm, value))
                {
                    // Fire and forget the async operation - it will handle its own completion
                    FilterHistory();
                    // SetProperty already handles change notification
                }
            }
        }

        [ObservableProperty]
        private bool _isSearchEnhancing;

        [ObservableProperty]
        private string _searchStatusText = "";

        [ObservableProperty]
        private bool _shouldFocusList;

        private string _selectedContentTypeFilter = "All Types";
        public string SelectedContentTypeFilter
        {
            get => _selectedContentTypeFilter;
            set
            {
                SetProperty(ref _selectedContentTypeFilter, value);
                FilterHistory();
            }
        }

        public List<string> ContentTypeFilterOptions { get; } = new List<string>
        {
            "All Types",
            "Text",
            "Code",
            "URL", 
            "Image"
        };


        public bool IsSelectedImageVisible => SelectedItem?.ContentType == "Image";
        public bool IsSelectedTextVisible => SelectedItem?.ContentType == "Text";
        public bool IsSelectedCodeVisible => SelectedItem?.ContentType == "Code";
        public bool IsSelectedUrlVisible => SelectedItem?.ContentType == "URL";

        /// <summary>
        /// The grouped collection view for binding to the UI
        /// </summary>
        public ICollectionView GroupedClipboardItems => _groupedViewSource.View;

        public ClipboardHistoryViewModel(IDataService dataService, IClipboardService clipboardService, IApplicationService applicationService, IEventBus eventBus, IWindowManager windowManager, ISearchService searchService, IProgressiveSearchService progressiveSearchService)
        {
            _dataService = dataService;
            _clipboardService = clipboardService;
            _applicationService = applicationService;
            _eventBus = eventBus;
            _windowManager = windowManager;
            _searchService = searchService;
            _progressiveSearchService = progressiveSearchService;
            _dispatcher = Dispatcher.CurrentDispatcher;
            
            // Initialize search debounce timer
            _searchDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300) // 300ms debounce delay
            };
            _searchDebounceTimer.Tick += OnSearchDebounceTimerTick;
            
            // Initialize the CollectionViewSource for grouping
            _groupedViewSource = new CollectionViewSource
            {
                Source = _filteredItems
            };
            
            // Add custom date grouping
            _groupedViewSource.GroupDescriptions.Add(new DateGroupDescription());
            
            // Sort items within groups by timestamp (newest first)
            _groupedViewSource.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Descending));
            
            // Subscribe to clipboard item added events
            _eventSubscription = _eventBus.Subscribe<ClipboardItemAddedEvent>(OnClipboardItemAdded);
            
            _ = LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            _filteredItems.Clear();
            _masterHistoryList = await _dataService.GetClipboardHistoryAsync();

            FilterHistory(); // Initial population

            SelectedItem = _filteredItems.FirstOrDefault();
        }

        private async void FilterHistory()
        {
            // Cancel any ongoing search
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _searchCancellationTokenSource.Token;
            
            try
            {
                // First filter by content type (quick operation, keep on UI thread)
                var contentTypeFiltered = _masterHistoryList
                    .Where(item => PassesContentTypeFilter(item))
                    .ToList();

                List<ClipboardItem> finalList;

                // If searching, use progressive search
                if (!string.IsNullOrWhiteSpace(ActualSearchTerm))
                {
                    var searchTerm = ActualSearchTerm;
                    
                    // Reset search status
                    IsSearchEnhancing = false;
                    SearchStatusText = "";
                    
                    // Phase 1: Quick exact search
                    var phase1Result = await _progressiveSearchService.SearchPhase1Async(
                        contentTypeFiltered, 
                        searchTerm, 
                        cancellationToken);
                    
                    // Update UI with phase 1 results immediately
                    foreach (var result in phase1Result.Results)
                    {
                        result.Item.SearchScore = result.Score;
                    }
                    
                    finalList = phase1Result.Results.Select(r => r.Item).ToList();
                    UpdateUIWithResults(finalList);
                    
                    // Phase 2: Enhanced search if needed
                    if (phase1Result.NeedsEnhancement && !cancellationToken.IsCancellationRequested)
                    {
                        IsSearchEnhancing = true;
                        SearchStatusText = "Finding more...";
                        
                        // Small delay to show phase 1 results first
                        await Task.Delay(50, cancellationToken);
                        
                        var phase2Result = await _progressiveSearchService.SearchPhase2Async(
                            contentTypeFiltered,
                            searchTerm,
                            phase1Result.Results,
                            cancellationToken);
                        
                        // Update scores for all results
                        foreach (var result in phase2Result.Results)
                        {
                            result.Item.SearchScore = result.Score;
                        }
                        
                        finalList = phase2Result.Results.Select(r => r.Item).ToList();
                        
                        IsSearchEnhancing = false;
                        SearchStatusText = "";
                    }
                }
                else
                {
                    // No search term - reset scores and use timestamp ordering
                    finalList = contentTypeFiltered;
                    foreach (var item in finalList)
                    {
                        item.SearchScore = 0.0;
                    }
                }

                // Final UI update
                UpdateUIWithResults(finalList);
            }
            catch (OperationCanceledException)
            {
                // Search was cancelled, this is expected - do nothing
            }
        }

        private void UpdateUIWithResults(List<ClipboardItem> results)
        {
            // Update sort descriptions based on whether we're searching
            _groupedViewSource.SortDescriptions.Clear();
            
            if (!string.IsNullOrWhiteSpace(ActualSearchTerm))
            {
                // When searching, sort by score first (best matches at top), then by timestamp
                _groupedViewSource.SortDescriptions.Add(new SortDescription("SearchScore", ListSortDirection.Descending));
                _groupedViewSource.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Descending));
            }
            else
            {
                // When not searching, sort by timestamp only
                _groupedViewSource.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Descending));
            }
            
            // Batch update the collection to minimize UI updates
            _filteredItems.ReplaceAll(results);

            SelectedItem = _filteredItems.FirstOrDefault();
            
            // Notify that the grouped collection has changed
            OnPropertyChanged(nameof(GroupedClipboardItems));
        }

        private bool PassesContentTypeFilter(ClipboardItem item)
        {
            return SelectedContentTypeFilter == "All Types" || item.ContentType == SelectedContentTypeFilter;
        }

        private bool PassesSearchFilter(ClipboardItem item)
        {
            if (string.IsNullOrWhiteSpace(ActualSearchTerm))
                return true;

            return _searchService.MatchesSearch(item, ActualSearchTerm);
        }

        private void OnSearchDebounceTimerTick(object? sender, EventArgs e)
        {
            // Safety check - ensure timer hasn't been disposed
            if (_searchDebounceTimer == null) 
                return;
                
            _searchDebounceTimer.Stop();
            
            // Update the actual search term which triggers filtering and highlighting
            ActualSearchTerm = SearchTerm;
        }

        [RelayCommand]
        private void CopyToClipboard(ClipboardItem? item)
        {
            if (item == null) return;

            // Use the service method that doesn't add to history
            _clipboardService.SetContentWithoutHistory(item);
            
            // Hide the window after copying
            _windowManager.HideMainWindow();
        }

        [RelayCommand]
        private void HideWindow()
        {
            _windowManager.HideMainWindow();
        }

        [RelayCommand]
        private void FocusListFromSearch()
        {
            // Trigger focus change by toggling the property
            ShouldFocusList = false;
            ShouldFocusList = true;
        }

        [RelayCommand]
        private void InitializeWindow(nint windowHandle)
        {
            _windowManager.InitializeClipboardListener(windowHandle);
        }

        [RelayCommand]
        private void OpenUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            
            // Validate it's a proper URL before opening
            if (!Utils.UrlDetector.IsUrl(url)) return;
            
            try
            {
                // Open URL in default browser
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processInfo);
                
                // Hide the window after opening the URL
                _windowManager.HideMainWindow();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open URL: {ex.Message}");
            }
        }


        private void OnClipboardItemAdded(ClipboardItemAddedEvent eventData)
        {
            if (eventData?.Item == null) return;

            // Ensure UI updates happen on the UI thread
            _dispatcher.BeginInvoke(() =>
            {
                // Add to master list at the beginning (most recent first)
                _masterHistoryList.Insert(0, eventData.Item);
                
                // Check if the item should be visible based on current filters
                var shouldBeVisible = PassesContentTypeFilter(eventData.Item) && PassesSearchFilter(eventData.Item);
                
                if (shouldBeVisible)
                {
                    // Add to filtered items collection
                    // CollectionViewSource will handle grouping and sorting automatically
                    _filteredItems.Insert(0, eventData.Item);
                    
                    // Update selection to the new item if no item is currently selected
                    if (SelectedItem == null)
                    {
                        SelectedItem = eventData.Item;
                    }
                }
            });
        }

        public void Dispose()
        {
            _searchDebounceTimer?.Stop();
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            _eventSubscription?.Dispose();
        }
    }
} 