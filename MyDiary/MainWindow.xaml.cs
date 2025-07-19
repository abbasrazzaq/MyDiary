using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
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
using Microsoft.Extensions.Options;
using MyDiary.Data;

/*
 *  TODO:
 *     - Clean up and refactoring
            - Fix vs warnings
 *     - Remove code duplicaiton for add and edit tabs using UserControl

*     - Don't allow adding of diary entry to the same date.   
 */

namespace MyDiary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _textChangedDuringEdit = false;
        private int _previousEntriesPageIndex = 0;
        private int _previousEntriesPageCount = 0;

        public event PropertyChangedEventHandler? PropertyChanged;
        
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<DiaryEntryListItem> _previousEntries;
        public ObservableCollection<DiaryEntryListItem> PreviousEntries
        {
            get => _previousEntries;
            set
            {
                _previousEntries = value;
                OnPropertyChanged();

                SetupCollectionView();
            }
        }

        private readonly DiaryRepository _diaryRepository;
        private readonly Settings _settings;

        public ICollectionView EntriesView { get; private set; }

        private void SetupCollectionView()
        {
            EntriesView = CollectionViewSource.GetDefaultView(PreviousEntries);

            OnPropertyChanged(nameof(EntriesView));
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                
                _searchText = value;
                _previousEntriesPageIndex = 0;
                loadDiaryEntries();

                OnPropertyChanged();
                EntriesView.Refresh();
            }
        }

        public MainWindow(DiaryRepository diaryRepository, IOptions<Settings> appSettings)
        {
            InitializeComponent();

            _diaryRepository = diaryRepository;
            _settings = appSettings.Value;

            DataContext = this;

            resetDiaryEntryUI();

            _searchText = string.Empty;
        }

        private void resetDiaryEntryUI()
        {
            dateDiaryEntry.SelectedDate = DateTime.Now.Date;

            txtDiaryEntry.Document.Blocks.Clear();
            txtDiaryEntry.AppendText("Dear Diary,\n");
            txtDiaryEntry.Focus();
            txtDiaryEntry.CaretPosition = txtDiaryEntry.Document.ContentEnd;
        }

        private async void loadDiaryEntries()
        {
            var paged = await _diaryRepository.GetPagedEntriesAsync(_previousEntriesPageIndex, _settings.PreviousDiaryEntriesPageSize, _searchText);

            PreviousEntries = new ObservableCollection<DiaryEntryListItem>(paged.Items);
            _previousEntriesPageCount = paged.TotalPages;

            previousPageBtn.IsEnabled = (_previousEntriesPageIndex > 0);
            nextPageBtn.IsEnabled = (_previousEntriesPageIndex < (_previousEntriesPageCount - 1));
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string diaryEntryXaml = getXamlFromRichTextBox(txtDiaryEntry);
            string diaryEntryPlain = getSingleLineTextFromRichTextBox(txtDiaryEntry);
            DateTime diaryDate = DateTime.Now;
            
            if(dateDiaryEntry.SelectedDate is not null)
            {
                diaryDate = dateDiaryEntry.SelectedDate.Value;
            }

            // Add diary entry
            var newEntry = new Diary
            {
                DiaryText = diaryEntryXaml,
                DiaryTextPlain = diaryEntryPlain,
                DiaryDate = diaryDate,
            };
            await _diaryRepository.AddEntryAsync(newEntry);

            MessageBox.Show("Diary entry added!", "Notification");
            resetDiaryEntryUI();
        }

        private async void deleteEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.Tag is int diaryId)
            {
                var result = MessageBox.Show("Are you sure you want to delete this entry?", 
                    "Confirm Deletion", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);

                if(result == MessageBoxResult.Yes)
                {
                    await _diaryRepository.DeleteEntryAsync(diaryId);

                    _previousEntriesPageIndex = 0;
                    loadDiaryEntries();
                }
            }
        }

        private void switchToViewPreviousDiaries()
        {
            DiaryTabs.SelectedItem = previousEntriesTabItem;

            editDiaryTabItem.IsEnabled = false;

            newDiaryTabItem.IsEnabled = true;
            previousEntriesTabItem.IsEnabled = true;
        }

        private async void switchToDiaryEditing(int diaryId)
        {
            var entryToEdit = await _diaryRepository.GetEntryByIdAsync(diaryId);
            if (entryToEdit != null)
            {
                // Setup data in the edit tab
                dudDateDiaryEntry.SelectedDate = entryToEdit.DiaryDate;

                loadXamlIntoRichTextBox(updateTxtDiaryEntry, entryToEdit.DiaryText);
                updateTxtDiaryEntry.Focus();
                updateTxtDiaryEntry.CaretPosition = updateTxtDiaryEntry.Document.ContentEnd;
                // Store diary id on update button
                btnUpdate.Tag = entryToEdit.DiaryId;

                // Switch tab
                DiaryTabs.SelectedItem = editDiaryTabItem;
                _textChangedDuringEdit = false;
            }
            else
            {
                // TODO: What if we can't find it for some reason?
                // Error message box and remove it from the list?
            }
        }

        private void viewEditEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int diaryId)
            {
                editDiaryTabItem.IsEnabled = true;

                newDiaryTabItem.IsEnabled = false;
                previousEntriesTabItem.IsEnabled = false;

                switchToDiaryEditing(diaryId);
            }
        }

        private void previousPageBtn_Click(object sender, RoutedEventArgs e)
        {
            _previousEntriesPageIndex--;
            loadDiaryEntries();

        }

        private void nextPageBtn_Click(object sender, RoutedEventArgs e)
        {
            _previousEntriesPageIndex++;
            loadDiaryEntries();
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.Tag is int diaryId)
            {
                string updatedXamlText = getXamlFromRichTextBox(updateTxtDiaryEntry);
                string updatePlainText = getSingleLineTextFromRichTextBox(updateTxtDiaryEntry);

                await _diaryRepository.UpdateEntryAsync(diaryId, updatePlainText, updatedXamlText);

                // Update the entry in the UI collection
                var item = PreviousEntries.FirstOrDefault(x => x.DiaryId == diaryId);
                if(item  != null)
                {
                    item.DiaryText = updatedXamlText;
                    item.PlainDiaryText = updatePlainText;
                }
                
            }

            switchToViewPreviousDiaries();
        }

        private void btnCancelUpdate_Click(object sender, RoutedEventArgs e)
        {
            var confirmationMessage = _textChangedDuringEdit
                ? "Are you sure you want to discard your changes?"
                : "Are you sure you want to cancel?";

            var confirmationResult = MessageBox.Show(confirmationMessage, "Confirm cancellation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if(confirmationResult == MessageBoxResult.Yes)
            {
                switchToViewPreviousDiaries();
            }
        }

        private void PreviousEntriesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(previousEntriesListView.SelectedItem is DiaryEntryListItem selectedEntry)
            {
                switchToDiaryEditing(selectedEntry.DiaryId);
            }
        }

        private string getSingleLineTextFromRichTextBox(RichTextBox richTextBox)
        {
            TextRange textRange = new TextRange(txtDiaryEntry.Document.ContentStart, txtDiaryEntry.Document.ContentEnd);
            return textRange.Text.Replace('\r', ' ').Replace('\n', ' ');
        }

        private string getXamlFromRichTextBox(RichTextBox richTextBox)
        {
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            using (var ms = new MemoryStream())
            {
                textRange.Save(ms, DataFormats.Xaml);
                ms.Position = 0;
                using (var reader = new StreamReader(ms))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private void loadXamlIntoRichTextBox(RichTextBox richTextBox, string xamlText)
        {
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xamlText)))
            {
                textRange.Load(ms, DataFormats.Xaml);
            }
        }

        private void updateTxtDiaryEntry_Changed(object sender, TextChangedEventArgs e)
        {
            _textChangedDuringEdit = true;
        }

        private void dateDiaryEntry_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dateDiaryEntry.SelectedDate is null)
            {
                MessageBox.Show("Please select a date.", "Missing Date", MessageBoxButton.OK, MessageBoxImage.Warning);
                dateDiaryEntry.SelectedDate = DateTime.Now.Date;
                return;
            }

            if(dateDiaryEntry.SelectedDate > DateTime.Today)
            {
                MessageBox.Show("Future dates are not allowed.", "Invalid Date", MessageBoxButton.OK, MessageBoxImage.Warning);
                dateDiaryEntry.SelectedDate = DateTime.Now.Date;
            }
        }

        private void diaryTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.Source is TabControl)
            {
                var tabControl = (TabControl)e.Source;
                var newTab = tabControl.SelectedItem as TabItem;
                if(newTab != null && newTab == previousEntriesTabItem)
                {
                    _previousEntriesPageIndex = 0;
                    loadDiaryEntries();
                }
            }
        }
    }
}