using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
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
 *      - Move db stuff to a data layer (out of the code behind)
 *  
 *      - Search 
 *      - Don't allow a future date?
 *      - Paging (for previous diary entries)
 *      - Private/Public
 *          - Password
 *      
 *     - Edit Diary Entry
 *      - Date (readonly)
 *      - Text box
 *      - Update & Cancel buttons (with "Are you sure?")
 *      
 *      
 *      
 *      - Use async for db save
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
        //private DiaryContext db = new DiaryContext();
        

        public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged(string propertyName)
        //    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                OnPropertyChanged(nameof(PreviousEntries));
            }
        }

        private readonly DiaryRepository _diaryRepository = new DiaryRepository();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            resetDiaryEntryUI();
            loadDiaryEntries();

        }

        private void resetDiaryEntryUI()
        {
            dateDiaryEntry.SelectedDate = DateTime.Now;

            txtDiaryEntry.Document.Blocks.Clear();
            txtDiaryEntry.AppendText("Dear Diary,\n");
            txtDiaryEntry.Focus();
            txtDiaryEntry.CaretPosition = txtDiaryEntry.Document.ContentEnd;
        }

        private async void loadDiaryEntries()
        {
            //PreviousEntries = new ObservableCollection<DiaryEntryListItem>
            //{
            //    new DiaryEntryListItem { DiaryId = 1, DiaryDate = DateTime.Today },
            //    new DiaryEntryListItem { DiaryId = 2, DiaryDate = DateTime.Today.AddDays(-1) }
            //};

            var entries = await _diaryRepository.GetAllEntriesAsync();
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            PreviousEntries = new ObservableCollection<DiaryEntryListItem>(entries);

            //PreviousEntries = new ObservableCollection<DiaryEntryListItem>(
            //    await _diaryRepository.GetAllEntriesAsync()
            //    );

            //PreviousEntries = new ObservableCollection<DiaryEntryListItem>(
            //    db.DiaryEntries
            //    .OrderByDescending(b => b.DiaryDate)
            //    .Select(x => new DiaryEntryListItem { DiaryId = x.DiaryId, DiaryDate = x.DiaryDate })
            //    .ToList()
            //    );
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



                //var entryToDelete = db.DiaryEntries.FirstOrDefault(d => d.DiaryId == diaryId);
                //if(entryToDelete != null)
                //{
                //    db.DiaryEntries.Remove(entryToDelete);
                //    db.SaveChanges();


                //}
            }
        }


        // TODO: Fix this hack
        private string updateDiaryTextInitial = null;

        private async void switchToDiaryEditing(int diaryId)
        {
            var entryToEdit = await _diaryRepository.GetEntryByIdAsync(diaryId);// db.DiaryEntries.FirstOrDefault(d => d.DiaryId == diaryId);
            if (entryToEdit != null)
            {
                // Setup data in the edit tab
                dudDateDiaryEntry.SelectedDate = entryToEdit.DiaryDate;

                updateDiaryTextInitial = entryToEdit.DiaryText;
                //updateTxtDiaryEntry.Document.Blocks.Clear();
                // TODO: 
                //updateTxtDiaryEntry.AppendText(updateDiaryTextInitial);
                loadXamlIntoRichTextBox(updateTxtDiaryEntry, updateDiaryTextInitial);
                updateTxtDiaryEntry.Focus();
                updateTxtDiaryEntry.CaretPosition = updateTxtDiaryEntry.Document.ContentEnd;

                // TODO: Store id somewhere for the tab to use on Update click
                btnUpdate.Tag = entryToEdit.DiaryId;

                // Switch tab
                DiaryTabs.SelectedItem = editDiaryTabItem;
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
                //Diary updatedDiary = new Diary
                //{ 
                //    DiaryId = diaryId,
                //    DiaryText = updateDiaryTextInitial,
                //    DiaryDate = 
                //};
                //await _diaryRepository.UpdateEntryAsync(updatedDiary);

                await _diaryRepository.UpdateEntryAsync(diaryId, getXamlFromRichTextBox(updateTxtDiaryEntry));

                //var entryToUpdate = db.DiaryEntries.FirstOrDefault(d => d.DiaryId == diaryId);
                //if(entryToUpdate != null)
                //{
                //    string updatedDiaryText = getXamlFromRichTextBox(updateTxtDiaryEntry);
                //    entryToUpdate.DiaryText = updatedDiaryText;

                //    db.SaveChanges();
                //}
            }

            DiaryTabs.SelectedItem = previousEntriesTabItem;
        }

        private void btnCancelUpdate_Click(object sender, RoutedEventArgs e)
        {
            var updatedDiaryText = getXamlFromRichTextBox(updateTxtDiaryEntry);
            var continueCancellation = true;

            // If any changes were made, confirm if they want to cancel
            if (!string.Equals(updateDiaryTextInitial, updatedDiaryText, StringComparison.Ordinal))
            {
                var confirmationResult = MessageBox.Show("Are you sure you want to cancel?", "Confirm cancellation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

                if(confirmationResult == MessageBoxResult.No)
                {
                    continueCancellation = false;
                }
            }

            if(continueCancellation)
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
    }
}