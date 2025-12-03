using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SIS_MK.Models;

namespace SIS_MK
{
    public partial class GameSelectionWindow : Window
    {
        // Коллекция игр для отображения
        public ObservableCollection<GameDefinition> Games { get; }

        // Выбранная игра (то, что заберём после закрытия окна)
        public GameDefinition SelectedGame { get; set; }

        public GameSelectionWindow(
            ObservableCollection<GameDefinition> games,
            GameDefinition currentGame)
        {
            InitializeComponent();

            Games = games;
            SelectedGame = currentGame;

            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Просто закрываемся с DialogResult = true
            DialogResult = true;
        }

        private void GamesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GamesListBox.SelectedItem is GameDefinition def)
            {
                SelectedGame = def;
                DialogResult = true;
            }
        }
    }
}
