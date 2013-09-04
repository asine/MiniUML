using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using MiniUML.Framework;

namespace MiniUML.View.Controls
{
    /// <summary>
    /// Interaction logic for CommandButton.xaml
    /// </summary>
    public partial class CommandButton : Button
    {
        static CommandButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandButton), new FrameworkPropertyMetadata("GenericCommandButtonStyle"));
        }

        #region CommandModel property

        public static readonly DependencyProperty CommandModelProperty
            = DependencyProperty.Register("CommandModel", typeof(CommandModel), typeof(CommandButton),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCommandModelPropertyChanged)));

        public CommandModel CommandModel
        {
            get { return (CommandModel)GetValue(CommandModelProperty); }
            set { SetValue(CommandModelProperty, value); }
        }

        private static void OnCommandModelPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            CommandButton commandButton = (CommandButton)obj;

            if (args.NewValue != null)
            {
                CommandModel commandModel = (CommandModel)args.NewValue;

                commandButton.Command = commandModel.Command;
                commandButton.Content = commandModel.Name;
                commandButton.ToolTip = commandModel.Description;
                commandButton.Image = commandModel.Image;
            }
            else
            {
                commandButton.Content = null;
                commandButton.Image = null;
            }
        }

        #endregion

        #region Image property

        public BitmapImage Image
        {
            get { return (BitmapImage)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty
            = DependencyProperty.Register("Image", typeof(BitmapImage), typeof(CommandButton),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region ButtonStyle property

        public ButtonStyles ButtonStyle
        {
            get { return (ButtonStyles)GetValue(ButtonStyleProperty); }
            set { SetValue(ButtonStyleProperty, value); }
        }

        public static readonly DependencyProperty ButtonStyleProperty
            = DependencyProperty.Register("ButtonStyle", typeof(ButtonStyles), typeof(CommandButton),
            new FrameworkPropertyMetadata(ButtonStyles.Text, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public enum ButtonStyles
        {
            Image,
            ImageAboveText,
            ImageBesideText,
            Text
        }

        #endregion

        #region ImageWidth property

        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        public static readonly DependencyProperty ImageWidthProperty
            = DependencyProperty.Register("ImageWidth", typeof(double), typeof(CommandButton),
            new FrameworkPropertyMetadata(32.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        #endregion

        #region ImageHeight property

        public double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public static readonly DependencyProperty ImageHeightProperty
            = DependencyProperty.Register("ImageHeight", typeof(double), typeof(CommandButton),
            new FrameworkPropertyMetadata(32.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        #endregion
    }
}