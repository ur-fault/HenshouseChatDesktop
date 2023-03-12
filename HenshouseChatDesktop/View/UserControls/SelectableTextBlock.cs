using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace HenshouseChatDesktop.View.UserControls;

// Make this class work
public class SelectableTextBlock : TextBlock
{
    private TextPointer? _startSelectPosition;
    private TextPointer? _endSelectPosition;
    public string SelectedText = "";

    public event Action<string>? TextSelected;

    protected override void OnMouseDown(MouseButtonEventArgs e) {
        base.OnMouseDown(e);
        var mouseDownPoint = e.GetPosition(this);
        _startSelectPosition = GetPositionFromPoint(mouseDownPoint, true) ??
                               throw new InvalidOperationException("Cannot get text position");
    }

    //protected override [FIXME](DragEventArgs e) {
    //    base.OnDragOver(e);
    //    if (_startSelectPosition is null) return;

    //    var mouseUpPoint = e.GetPosition(this);
    //    _endSelectPosition = GetPositionFromPoint(mouseUpPoint, true) ??
    //                         throw new InvalidOperationException("Cannot get text position");

    //    var otr = new TextRange(ContentStart, ContentEnd);
    //    otr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));

    //    var ntr = new TextRange(_startSelectPosition, _endSelectPosition);
    //    ntr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.LimeGreen));
    //}

    protected override void OnMouseUp(MouseButtonEventArgs e) {
        base.OnMouseUp(e);
        var mouseUpPoint = e.GetPosition(this);
        _endSelectPosition = GetPositionFromPoint(mouseUpPoint, true) ??
                             throw new InvalidOperationException("Cannot get text position");

        var otr = new TextRange(ContentStart, ContentEnd);
        otr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));

        var ntr = new TextRange(_startSelectPosition, _endSelectPosition);
        ntr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.LimeGreen));

        SelectedText = ntr.Text;
        TextSelected?.Invoke(SelectedText);
    }
}