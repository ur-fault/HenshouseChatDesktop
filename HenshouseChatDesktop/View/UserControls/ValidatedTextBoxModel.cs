using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HenshouseChatDesktop.View.UserControls;
public class ValidatedTextBoxModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _text = "";
    public string Text { get => _text; set => SetValue(ref _text, value); }

    private Func<string, bool>? _validationFunc;
    public Func<string, bool>? ValidationFunc { get => _validationFunc; set => SetValue(ref _validationFunc, value, new[] { "IsValid" }); }

    public bool IsValid => ValidationFunc?.Invoke(Text) ?? true;

    public void SetValue<T>(ref T field, T value, string[]? changed = null, [CallerMemberName] string? name = null) {
        if (name is null)
            return;

        if (Object.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        if (changed != null) {
            foreach (var item in changed) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"{item}"));
            }
        }
    }
}