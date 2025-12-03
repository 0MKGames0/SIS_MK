using System.Windows;
using System.Windows.Input;
using SIS_MK.ViewModels;

namespace SIS_MK
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.E && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (DataContext is not MainViewModel vm)
                    return;

                // создаём VM редактора на том же DataService, что и основное окно
                var editorVm = new SIS_MK.ViewModels.DatabaseEditorViewModel(vm.DataService);

                // ПЕРЕДАЁМ editorVm в конструктор окна
                var editor = new DatabaseEditorWindow(editorVm)
                {
                    Owner = this
                };

                editor.ShowDialog();

                // после закрытия перечитываем данные для текущей игры
                vm.ReloadDataFromDisk();
            }
        }

        private void ChooseGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm)
                return;

            var selector = new GameSelectionWindow(vm.Games, vm.SelectedGame)
            {
                Owner = this
            };

            var result = selector.ShowDialog();
            if (result == true && selector.SelectedGame != null)
            {
                // Это дернет сеттер SelectedGame в MainViewModel
                vm.SelectedGame = selector.SelectedGame;
            }
        }
    }
}
