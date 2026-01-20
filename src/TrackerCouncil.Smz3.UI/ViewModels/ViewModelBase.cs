using AvaloniaControls.Extensions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class ViewModelBase : ReactiveObject
{
    public ViewModelBase()
    {
        this.LinkProperties();

        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != nameof(HasBeenModified))
            {
                HasBeenModified = true;
            }
        };
    }

    [Reactive] public partial bool HasBeenModified { get; set; }
}
