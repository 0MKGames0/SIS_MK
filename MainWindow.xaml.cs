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
            if (e.Key == Key.N && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                var help = new HelpWindow
                {
                    Owner = this
                };
                help.Show();
            }

            if (e.Key == Key.E && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                // Ctrl+E — редактор базы
                var editor = new DatabaseEditorWindow
                {
                    Owner = this
                };

                editor.ShowDialog(); // ждём, пока закроется

                // После закрытия перечитываем базу в основной VM
                var vm = DataContext as MainViewModel;
                if (vm != null)
                {
                    vm.ReloadDataFromDisk();
                }
            }
        }
    }
}
