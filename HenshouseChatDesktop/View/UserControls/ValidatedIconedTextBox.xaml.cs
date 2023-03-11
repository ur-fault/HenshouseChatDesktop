using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HenshouseChatDesktop.View.UserControls;

public partial class ValidatedIconedTextBox
{
    public string? Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(ValidatedIconedTextBox),
            new PropertyMetadata("",
                (d, e) => {
                    ((ValidatedIconedTextBox)d).Revalidate();

                    d.SetValue(IsEmptyProperty,
                        string.IsNullOrEmpty(
                            (string)d.GetValue(TextProperty)));
                }));


    public Func<ValidatedIconedTextBox, string, bool>? ValidationFunction {
        get =>
            (Func<ValidatedIconedTextBox, string, bool>?)GetValue(
                ValidationFunctionProperty);
        set => SetValue(ValidationFunctionProperty, value);
    }

    public static readonly DependencyProperty ValidationFunctionProperty =
        DependencyProperty.Register(
            nameof(ValidationFunction),
            typeof(Func<ValidatedIconedTextBox, string, bool>),
            typeof(ValidatedIconedTextBox),
            new PropertyMetadata(null));


    public string WatermarkText {
        get => (string)GetValue(WatermarkTextProperty);
        set => SetValue(WatermarkTextProperty, value);
    }

    public static readonly DependencyProperty WatermarkTextProperty =
        DependencyProperty.Register(nameof(WatermarkText), typeof(string),
            typeof(ValidatedIconedTextBox), new PropertyMetadata(""));


    public bool IsEmpty =>
        (bool)GetValue(IsEmptyProperty.DependencyProperty);

    internal static readonly DependencyPropertyKey IsEmptyProperty =
        DependencyProperty.RegisterReadOnly(nameof(IsEmpty), typeof(bool),
            typeof(ValidatedIconedTextBox),
            new PropertyMetadata(true,
                (d, e) => {
                    d.SetValue(WatermarkVisibilityProperty,
                        (bool)e.NewValue
                            ? Visibility.Visible
                            : Visibility.Hidden);
                }));


    public Visibility WatermarkVisibility =>
        (Visibility)GetValue(WatermarkVisibilityProperty
            .DependencyProperty);

    internal static readonly DependencyPropertyKey
        WatermarkVisibilityProperty =
            DependencyProperty.RegisterReadOnly(nameof(WatermarkVisibility),
                typeof(Visibility), typeof(ValidatedIconedTextBox),
                new PropertyMetadata(
                    Visibility.Visible,
                    (d, e) => { }));


    public Action OnEnterPressedAction {
        get => (Action)GetValue(OnEnterPressedActionProperty);
        set => SetValue(OnEnterPressedActionProperty, value);
    }

    public static readonly DependencyProperty OnEnterPressedActionProperty =
        DependencyProperty.Register(nameof(OnEnterPressedAction), typeof(Action), typeof(ValidatedIconedTextBox),
            new PropertyMetadata(() => { }));


    public bool IsValid => Text is not null && (ValidationFunction?.Invoke(this, Text) ?? true);


    public ValidatedIconedTextBox() {
        InitializeComponent();
        DataContext = this;
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

    private void InnerTextBox_OnKeyDown(object sender, KeyEventArgs e) {
        if (e.Key is Key.Enter)
            OnEnterPressedAction?.Invoke();
    }

    public void FocusInput() {
        InnerTextBox.Focus();
    }
}