using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/*
 *  TODO:
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
 *      - Move db stuff to a data layer (out of the code behind)
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
    public partial class MainWindow : Window
    {
        private DiaryContext db = new DiaryContext();

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

        private void loadDiaryEntries()
        {
            PreviousEntries = new ObservableCollection<DiaryEntryListItem>(
                db.DiaryEntries
                .OrderByDescending(b => b.DiaryDate)
                .Select(x => new DiaryEntryListItem { DiaryId = x.DiaryId, DiaryDate = x.DiaryDate })
                .ToList()
                );
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string diaryEntryXaml = getXamlFromRichTextBox(txtDiaryEntry);
            DateTime diaryDate = DateTime.Now;
            
            // TODO: Check if not selected and asks user to enter a date
            if(dateDiaryEntry.SelectedDate is not null)
            {
                diaryDate = dateDiaryEntry.SelectedDate.Value;
            }

            // Add diary entry
            var newEntry = new Diary { DiaryText = diaryEntryXaml, DiaryDate = diaryDate };
            db.Add(newEntry);
            db.SaveChangesAsync();

            PreviousEntries.Insert(0, new DiaryEntryListItem { DiaryId = newEntry.DiaryId, DiaryDate = newEntry.DiaryDate });

            MessageBox.Show("Diary entry added!", "Notification");
            resetDiaryEntryUI();
        }

        private void deleteEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.Tag is int diaryId)
            {
                var entryToDelete = db.DiaryEntries.FirstOrDefault(d => d.DiaryId == diaryId);
                if(entryToDelete != null)
                {
                    db.DiaryEntries.Remove(entryToDelete);
                    db.SaveChanges();

                    var itemToRemove = PreviousEntries.FirstOrDefault(i => i.DiaryId == diaryId);
                    if(itemToRemove != null)
                    {
                        PreviousEntries.Remove(itemToRemove);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // TODO: Fix this hack
        private string updateDiaryTextInitial = null;

        private void switchToDiaryEditing(int diaryId)
        {
            var entryToEdit = db.DiaryEntries.FirstOrDefault(d => d.DiaryId == diaryId);
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

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.Tag is int diaryId)
            {
                var entryToUpdate = db.DiaryEntries.FirstOrDefault(d => d.DiaryId == diaryId);
                if(entryToUpdate != null)
                {
                    string updatedDiaryText = getXamlFromRichTextBox(updateTxtDiaryEntry);
                    entryToUpdate.DiaryText = updatedDiaryText;

                    db.SaveChanges();
                }
            }

            DiaryTabs.SelectedItem = previousEntriesTabItem;
        }

        private void btnCancelUpdate_Click(object sender, RoutedEventArgs e)
        {
            string updatedDiaryText = getXamlFromRichTextBox(updateTxtDiaryEntry);
            // If any changes were made, confirm if they want to cancel
            if (!string.Equals(updateDiaryTextInitial, updatedDiaryText, StringComparison.Ordinal))
            {
                var confirmationResult = MessageBox.Show("Are you sure you want to cancel?", "Confirm cancellation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

                if(confirmationResult == MessageBoxResult.Yes)
                {
                    DiaryTabs.SelectedItem = previousEntriesTabItem;
                }
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