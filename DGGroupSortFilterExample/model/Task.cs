using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DGGroupSortFilterExample.model;

public partial class Task : ObservableObject, IEditableObject
{
    [ObservableProperty]
    private string _projectName = string.Empty;
    [ObservableProperty]
    private string _taskName = string.Empty;
    [ObservableProperty]
    private string _description = string.Empty;
    [ObservableProperty]
    private DateTime _dueDate = DateTime.Now;
    [ObservableProperty]
    private bool _complete;
    [ObservableProperty]
    private string _priority = "Средний";
    [ObservableProperty]
    private string _assignedTo = string.Empty;
    [ObservableProperty]
    private int _estimatedHours = 8;
    [ObservableProperty]
    private Task? _tempTask;
    [ObservableProperty]
    private bool _isEditing;



    public void BeginEdit()
    {
        if (IsEditing) return;
        TempTask = (Task)MemberwiseClone();
        IsEditing = true;
    }

    public void CancelEdit()
    {
        if (!IsEditing) return;
        if(TempTask is  null) return;
            
        ProjectName = TempTask.ProjectName;
        TaskName = TempTask.TaskName;
        Description = TempTask.Description;
        DueDate = TempTask.DueDate;
        Complete = TempTask.Complete;
        Priority = TempTask.Priority;
        AssignedTo = TempTask.AssignedTo;
        EstimatedHours = TempTask.EstimatedHours;
        IsEditing = false;
    }

    public void EndEdit()
    {
        if (!IsEditing) return;
        TempTask = null;
        IsEditing = false;
    }
}