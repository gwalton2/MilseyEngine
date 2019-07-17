using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MilseyEngine
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class TitleWindow : Page
    {
        public TitleWindow()
        {
            InitializeComponent();
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (manualCheck.IsChecked == true)
            {
                GameWindow gw = new GameWindow(false, false);
                this.NavigationService.Navigate(gw);
            }
            else if (aiCheck.IsChecked == true)
            {
                if (whiteCheck.IsChecked == true)
                {
                    GameWindow gw = new GameWindow(true, false);
                    this.NavigationService.Navigate(gw);
                }
                else if (blackCheck.IsChecked == true)
                {
                    GameWindow gw = new GameWindow(true, true);
                    this.NavigationService.Navigate(gw);
                }
            }
        }

        private void aiCheck_Checked(object sender, RoutedEventArgs e)
        {
            whiteCheck.IsEnabled = true;
            blackCheck.IsEnabled = true;

            manualCheck.IsChecked = false;
        }

        private void blackCheck_Checked(object sender, RoutedEventArgs e)
        {
            whiteCheck.IsChecked = false;
        }

        private void whiteCheck_Checked(object sender, RoutedEventArgs e)
        {
            blackCheck.IsChecked = false;
        }

        private void manualCheck_Checked(object sender, RoutedEventArgs e)
        {
            aiCheck.IsChecked = false;

            whiteCheck.IsEnabled = false;
            blackCheck.IsEnabled = false;
        }
    }
}
