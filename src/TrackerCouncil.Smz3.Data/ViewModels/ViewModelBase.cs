using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TrackerCouncil.Smz3.Data.ViewModels;

public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshAll()
    {
        foreach (var property in GetType().GetProperties())
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property.Name));
        }
    }

    public void Copy(ViewModelBase other)
    {
        if (GetType() != other.GetType())
        {
            throw new InvalidOperationException("Cannot copy view models of different types");
        }

        foreach (var property in GetType().GetProperties().Where(x => x.GetSetMethod() != null))
        {
            property.SetValue(this, property.GetValue(other));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property.Name));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
