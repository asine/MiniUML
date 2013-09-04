using System.Windows;
using MiniUML.Framework;

namespace MiniUML.Model.ViewModels
{
    public class NewDocumentWindowViewModel : ViewModel
    {
        public Size prop_PageSize
        {
            get { return _pageSize; }
            set
            {
                _pageSize = value;
                SendPropertyChanged("prop_PageSize");
            }
        }

        public Thickness prop_PageMargins
        {
            get { return _pageMargins; }
            set
            {
                _pageMargins = value;
                SendPropertyChanged("prop_PageMargins");
            }
        }

        private Thickness _pageMargins;
        private Size _pageSize;
    }
}
