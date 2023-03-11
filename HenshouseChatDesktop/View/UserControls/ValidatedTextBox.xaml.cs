using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HenshouseChatDesktop.View.UserControls;

public partial class ValidatedTextBox : UserControl
{
    public string? Text {
        get => (string)GetValue(TextProperty);
        set {
            SetValue(TextProperty, value);
            Revalidate();
        }
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(ValidatedTextBox),
            new PropertyMetadata("",
                (d, e) => {
                    d.SetValue(IsEmptyProperty,
                        string.IsNullOrEmpty(((ValidatedTextBox)d).Text));
                })
        );


    public Func<ValidatedTextBox, string, bool>? ValidationFunction {
        get => (Func<ValidatedTextBox, string, bool>?)GetValue(
            ValidationFunctionProperty);
        set {
            SetValue(ValidationFunctionProperty, value);
            Revalidate();
        }
    }

    public static readonly DependencyProperty ValidationFunctionProperty =
        DependencyProperty.Register(
            nameof(ValidationFunction),
            typeof(Func<ValidatedTextBox, string, bool>),
            typeof(ValidatedTextBox),
            new PropertyMetadata(null));


    public string WatermarkText {
        get => (string)GetValue(WatermarkTextProperty);
        set => SetValue(WatermarkTextProperty, value);
    }

    public static readonly DependencyProperty WatermarkTextProperty =
        DependencyProperty.Register(nameof(WatermarkText), typeof(string),
            typeof(ValidatedTextBox), new PropertyMetadata(""));


    public bool IsEmpty => (bool)GetValue(IsEmptyProperty.DependencyProperty);

    internal static readonly DependencyPropertyKey IsEmptyProperty =
        DependencyProperty.RegisterReadOnly(nameof(IsEmpty), typeof(bool),
            typeof(ValidatedTextBox),
            new PropertyMetadata(true,
                (d, e) => {
                    d.SetValue(WatermarkVisibilityProperty,
                        (bool)e.NewValue
                            ? Visibility.Visible
                            : Visibility.Hidden);
                }));


    public Visibility WatermarkVisibility =>
        (Visibility)GetValue(WatermarkVisibilityProperty.DependencyProperty);

    internal static readonly DependencyPropertyKey WatermarkVisibilityProperty =
        DependencyProperty.RegisterReadOnly(nameof(WatermarkVisibility),
            typeof(Visibility), typeof(ValidatedTextBox), new PropertyMetadata(
                Visibility.Visible,
                (d, e) => { }));


    public TextAlignment TextAlignment {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(ValidatedTextBox),
            new PropertyMetadata(System.Windows.TextAlignment.Left));


    public bool IsValid => Text is not null && (ValidationFunction?.Invoke(this, Text) ?? true);


    public ValidatedTextBox() {
        InitializeComponent();
        MainBorder.DataContext = this;
    }

    private void
        InnerTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        Revalidate();
    }

    public void Revalidate() {
        var text = InnerTextBox.Text;
        InnerTextBox.Foreground = !IsValid
            ? (SolidColorBrush)Application.Current.Resources["Red"]
            : (SolidColorBrush)Application.Current.Resources["Light"];
    }

    public void FocusInput() {
        InnerTextBox.Focus();
    }
}