using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace WPFGeniusSudoku.Services
{
    public static class ModalService
    {
        public static Grid? ModalHostContent;
        public static Grid ModalHost;

        public static void ShowModal(UIElement content)
        {
            ModalHostContent?.Children.Clear();
            ModalHostContent?.Children.Add(content);
            ModalHost.Visibility = Visibility.Visible;
        }
    
        public static void CloseModal()
        {
            ModalHost.Visibility = Visibility.Collapsed;
        }
    }
}
