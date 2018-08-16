using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FaceRoll.Common
{
    public static class NavigationHelper
    {
        public static Frame NavigationFrame { get; set; }

        public static bool Navigate(Type page, object parameter = null)
        {
            return NavigationFrame.Navigate(page, parameter);
        }

    }
}
