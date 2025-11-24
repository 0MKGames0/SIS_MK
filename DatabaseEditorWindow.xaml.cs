using System.Windows;
using System.Windows.Controls;
using SIS_MK.Services;
using SIS_MK.ViewModels;

namespace SIS_MK
{
    public partial class DatabaseEditorWindow : Window
    {
        private readonly DatabaseEditorViewModel _viewModel;

        public DatabaseEditorWindow()
        {
            InitializeComponent();

            _viewModel = new DatabaseEditorViewModel(new DataService());
            DataContext = _viewModel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // фиксируем редактирование в гридax
            if (CharactersGrid != null)
            {
                CharactersGrid.CommitEdit(DataGridEditingUnit.Row, true);
                CharactersGrid.CommitEdit();
            }

            if (ItemsGrid != null)
            {
                ItemsGrid.CommitEdit(DataGridEditingUnit.Row, true);
                ItemsGrid.CommitEdit();
            }

            _viewModel.Save();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
