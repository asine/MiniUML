using MiniUML.Framework;

namespace MiniUML.Model.ViewModels
{
    public class ExportDocumentWindowViewModel : ViewModel
    {
        public double prop_Resolution
        {
            get { return _resolution; }
            set
            {
                _resolution = value;
                SendPropertyChanged("prop_Resolution");
            }
        }

        public bool prop_TransparentBackground
        {
            get { return _transparentBackground; }
            set
            {
                _transparentBackground = value;
                SendPropertyChanged("prop_TransparentBackground");
            }
        }

        public bool prop_EnableTransparentBackground
        {
            get { return _enableTransparentBackground; }
            set
            {
                _enableTransparentBackground = value;
                SendPropertyChanged("prop_EnableTransparentBackground");
            }
        }

        private double _resolution;
        private bool _transparentBackground;
        private bool _enableTransparentBackground;
    }
}
