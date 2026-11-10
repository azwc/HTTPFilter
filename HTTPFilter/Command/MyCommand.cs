using System;
using System.Windows.Input;

namespace HTTPFilter.Command;

public class MyCommand: ICommand
{
    private Action executeAction;

    public MyCommand(Action action)
    {
        executeAction = action;
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        executeAction();
    }

    public event EventHandler? CanExecuteChanged;
}