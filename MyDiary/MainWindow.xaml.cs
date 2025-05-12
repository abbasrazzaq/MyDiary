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
using MyDiary.Data;

/*
 *  TODO:

       - Private/Public
 *          - Password (what's the best encryption/hashing to use to store the password)
 *              - Inc secret question & answer
 *      - Remove code duplicaiton for add and edit tabs
 *      - Paging (for previous diary entries)

        - Clean up and refactoring
            - Fix warnings
- Don't allow setting of date that already has an entry.

 *      
 *      
 */

namespace MyDiary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _textChangedDuringEdit = false;

        public event PropertyChangedEventHandler PropertyChanged;
        
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

        public ICollectionView EntriesView { get; private set; }

        private void SetupCollectionView()
        {
            EntriesView = CollectionViewSource.GetDefaultView(PreviousEntries);
            EntriesView.Filter = filterDiaryEntries;

            OnPropertyChanged(nameof(EntriesView));
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                EntriesView.Refresh();
            }
        }

        public MainWindow(DiaryRepository diaryRepository)
        {
            InitializeComponent();

            _diaryRepository = diaryRepository;
            DataContext = this;

            resetDiaryEntryUI();
            loadDiaryEntries();

        }

        private bool filterDiaryEntries(object item)
        {
            if(item is DiaryEntryListItem entry)
            {
                return string.IsNullOrEmpty(SearchText)
                || entry.PlainDiaryText?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
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
            var entries = await _diaryRepository.GetAllEntriesAsync();
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            PreviousEntries = new ObservableCollection<DiaryEntryListItem>(entries);
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string diaryEntryXaml = getXamlFromRichTextBox(txtDiaryEntry);
            DateTime diaryDate = DateTime.Now;
            
            // TODO: Check if not selected and asks user to enter a date
            if(dateDiaryEntry.SelectedDate is not null)
            {
                diaryDate = dateDiaryEntry.SelectedDate.Value;
            }

            // Add diary entry
            var newEntry = new Diary
            {
                DiaryText = diaryEntryXaml,
                DiaryDate = diaryDate,
            };
            await _diaryRepository.AddEntryAsync(newEntry);

            PreviousEntries.Insert(0, new DiaryEntryListItem { DiaryId = newEntry.DiaryId, DiaryDate = newEntry.DiaryDate });

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

                    var itemToRemove = PreviousEntries.FirstOrDefault(i => i.DiaryId == diaryId);
                    if (itemToRemove != null)
                    {
                        PreviousEntries.Remove(itemToRemove);
                    }
                }
            }
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
                switchToDiaryEditing(diaryId);
            }
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.Tag is int diaryId)
            {
                string updatedXamlText = getXamlFromRichTextBox(updateTxtDiaryEntry);
                await _diaryRepository.UpdateEntryAsync(diaryId, updatedXamlText);

                // Update the entry in the UI collection
                var item = PreviousEntries.FirstOrDefault(x => x.DiaryId == diaryId);
                if(item  != null)
                {
                    item.DiaryText = updatedXamlText;
                }
                
            }

            DiaryTabs.SelectedItem = previousEntriesTabItem;
        }

        private void btnCancelUpdate_Click(object sender, RoutedEventArgs e)
        {
            var confirmationMessage = _textChangedDuringEdit
                ? "Are you sure you want to discard your changes?"
                : "Are you sure you want to cancel?";

            var confirmationResult = MessageBox.Show(confirmationMessage, "Confirm cancellation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if(confirmationResult == MessageBoxResult.Yes)
            {
                DiaryTabs.SelectedItem = previousEntriesTabItem;
            }
        }

        private void PreviousEntriesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(previousEntriesListView.SelectedItem is DiaryEntryListItem selectedEntry)
            {
                switchToDiaryEditing(selectedEntry.DiaryId);
            }
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
    }
}