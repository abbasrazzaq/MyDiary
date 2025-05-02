using System.Collections.ObjectModel;
using System.ComponentModel;
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
 *      - Double click an entry to view it
 *          - Can update text
 *          - Can't change date
 *          
 *      - Search 
 *      - Paging
 *      - Private/Public
 *          - Password
 *      
 *     - Edit Diary Entry
 *      - Date (readonly)
 *      - Text box
 *      - Update & Cancel buttons (with "Are you sure?")
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
            TextRange textRange = new TextRange(txtDiaryEntry.Document.ContentStart, txtDiaryEntry.Document.ContentEnd);

            DateTime diaryDate = DateTime.Now;
            
            // TODO: Check if not select and asks user to enter a date
            if(dateDiaryEntry.SelectedDate is not null)
            {
                diaryDate = dateDiaryEntry.SelectedDate.Value;
            }

            // Add diary entry
            var newEntry = new Diary { DiaryText = textRange.Text, DiaryDate = diaryDate };
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

        private void viewEditEntryBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelUpdate_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}