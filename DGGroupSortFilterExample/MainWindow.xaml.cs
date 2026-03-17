using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Linq;
using System.Globalization;

namespace DGGroupSortFilterExample
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            var tasksCollection = (Tasks)this.Resources["tasks"];
            tasksCollection.Clear();

            string[] projects = { "Разработка", "Маркетинг", "Документация", "Тестирование", "Аналитика" };
            string[] priorities = { "Высокий", "Средний", "Низкий" };
            string[] employees = { "Иванов", "Петров", "Сидоров", "Козлов", "Морозов", "Волков" };
            Random random = new Random();

            for (int i = 1; i <= 30; i++)
            {
                var dueDate = DateTime.Now.AddDays(random.Next(-10, 40));
                
                tasksCollection.Add(new Task
                {
                    ProjectName = projects[random.Next(projects.Length)],
                    TaskName = $"Задача {i}",
                    Description = $"Описание задачи {i}",
                    DueDate = dueDate,
                    Complete = random.Next(3) == 0, // 33% выполненных задач
                    Priority = priorities[random.Next(priorities.Length)],
                    AssignedTo = employees[random.Next(employees.Length)],
                    EstimatedHours = random.Next(2, 40)
                });
            }

            UpdateStatus($"Загружено задач: {tasksCollection.Count}");
            ApplyGrouping();
        }

        private void Grouping_Changed(object sender, RoutedEventArgs e)
        {
            ApplyGrouping();
        }

        private void ApplyGrouping()
        {
            try
            {
                var cvTasks = CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource);
                if (cvTasks == null || !cvTasks.CanGroup) return;

                cvTasks.GroupDescriptions.Clear();

                // Добавляем группировки в зависимости от выбора пользователя
                AddGroupDescription(cvTasks, cmbGroupBy1, chkSortGroups1.IsChecked == true);
                AddGroupDescription(cvTasks, cmbGroupBy2, chkSortGroups2.IsChecked == true);
                AddGroupDescription(cvTasks, cmbGroupBy3, chkSortGroups3.IsChecked == true);

                string groupingInfo = cvTasks.GroupDescriptions.Count > 0 
                    ? $"Группировка по {cvTasks.GroupDescriptions.Count} уровням" 
                    : "Группировка отключена";
                
                UpdateStatus(groupingInfo);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка группировки: {ex.Message}");
            }
        }

        private void AddGroupDescription(ICollectionView cvTasks, ComboBox comboBox, bool sortGroups)
        {
            if (comboBox.SelectedItem is ComboBoxItem item && 
                item.Content.ToString() != "Без группировки")
            {
                string content = item.Content.ToString();
                string propertyName = ExtractPropertyName(content);
                
                PropertyGroupDescription groupDesc;
                
                if (propertyName == "DueDate")
                {
                    // Специальная группировка по датам (по месяцам)
                    groupDesc = new PropertyGroupDescription(propertyName, new MonthGroupConverter());
                }
                else
                {
                    groupDesc = new PropertyGroupDescription(propertyName);
                }
                
                cvTasks.GroupDescriptions.Add(groupDesc);
                
                // Если нужно сортировать группы
                if (sortGroups)
                {
                    // Для сортировки групп нужно добавить соответствующую сортировку
                    // Это можно реализовать через SortDescriptions
                }
            }
        }

        private string ExtractPropertyName(string comboBoxContent)
        {
            if (comboBoxContent.Contains("ProjectName")) return "ProjectName";
            if (comboBoxContent.Contains("Complete")) return "Complete";
            if (comboBoxContent.Contains("Priority")) return "Priority";
            if (comboBoxContent.Contains("AssignedTo")) return "AssignedTo";
            if (comboBoxContent.Contains("DueDate")) return "DueDate";
            return comboBoxContent.Split(' ')[0];
        }

        private void ClearGrouping_Click(object sender, RoutedEventArgs e)
        {
            cmbGroupBy1.SelectedIndex = 0;
            cmbGroupBy2.SelectedIndex = 0;
            cmbGroupBy3.SelectedIndex = 0;
            
            var cvTasks = CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource);
            if (cvTasks != null && cvTasks.CanGroup)
            {
                cvTasks.GroupDescriptions.Clear();
            }
            
            UpdateStatus("Группировка сброшена");
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            LoadSampleData();
        }

        private void CompleteFilter_Changed(object sender, RoutedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource)?.Refresh();
            UpdateStatus(cbCompleteFilter.IsChecked == true 
                ? "Фильтр: скрыты выполненные задачи" 
                : "Фильтр отключен");
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (cbCompleteFilter.IsChecked == true && e.Item is Task task)
            {
                e.Accepted = !task.Complete;
            }
        }

        private void UpdateStatus(string message)
        {
            txtStatus?.Text = message;
        }
    }

    /// <summary>
    /// Конвертер для группировки по месяцам
    /// </summary>
    public class MonthGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для отображения статуса задачи
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class CompleteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool complete)
            {
                return complete ? "✅ Выполнено" : "⏳ В работе";
            }
            return value?.ToString() ?? "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strComplete)
            {
                return strComplete.Contains("Выполнено");
            }
            return false;
        }
    }

    /// <summary>
    /// Конвертер для отображения имени группы
    /// </summary>
    public class GroupNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Без названия";
            
            string str = value.ToString();
            
            // Преобразуем значения булевых полей
            if (bool.TryParse(str, out bool boolValue))
            {
                return boolValue ? "✅ Выполнено" : "⏳ В работе";
            }
            
            // Преобразуем даты
            if (DateTime.TryParse(str, out DateTime dateValue))
            {
                return dateValue.ToString("dd.MM.yyyy");
            }
            
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Класс задачи
    /// </summary>
    public class Task : INotifyPropertyChanged, IEditableObject
    {
        private string _projectName = string.Empty;
        private string _taskName = string.Empty;
        private string _description = string.Empty;
        private DateTime _dueDate = DateTime.Now;
        private bool _complete = false;
        private string _priority = "Средний";
        private string _assignedTo = string.Empty;
        private int _estimatedHours = 8;
        private Task _tempTask = null;
        private bool _isEditing = false;

        public string ProjectName
        {
            get { return _projectName; }
            set { if (value != _projectName) { _projectName = value; OnPropertyChanged(nameof(ProjectName)); } }
        }

        public string TaskName
        {
            get { return _taskName; }
            set { if (value != _taskName) { _taskName = value; OnPropertyChanged(nameof(TaskName)); } }
        }

        public string Description
        {
            get { return _description; }
            set { if (value != _description) { _description = value; OnPropertyChanged(nameof(Description)); } }
        }

        public DateTime DueDate
        {
            get { return _dueDate; }
            set { if (value != _dueDate) { _dueDate = value; OnPropertyChanged(nameof(DueDate)); } }
        }

        public bool Complete
        {
            get { return _complete; }
            set { if (value != _complete) { _complete = value; OnPropertyChanged(nameof(Complete)); } }
        }

        public string Priority
        {
            get { return _priority; }
            set { if (value != _priority) { _priority = value; OnPropertyChanged(nameof(Priority)); } }
        }

        public string AssignedTo
        {
            get { return _assignedTo; }
            set { if (value != _assignedTo) { _assignedTo = value; OnPropertyChanged(nameof(AssignedTo)); } }
        }

        public int EstimatedHours
        {
            get { return _estimatedHours; }
            set { if (value != _estimatedHours) { _estimatedHours = value; OnPropertyChanged(nameof(EstimatedHours)); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void BeginEdit()
        {
            if (!_isEditing)
            {
                _tempTask = (Task)this.MemberwiseClone();
                _isEditing = true;
            }
        }

        public void CancelEdit()
        {
            if (_isEditing && _tempTask != null)
            {
                ProjectName = _tempTask.ProjectName;
                TaskName = _tempTask.TaskName;
                Description = _tempTask.Description;
                DueDate = _tempTask.DueDate;
                Complete = _tempTask.Complete;
                Priority = _tempTask.Priority;
                AssignedTo = _tempTask.AssignedTo;
                EstimatedHours = _tempTask.EstimatedHours;
                _isEditing = false;
            }
        }

        public void EndEdit()
        {
            if (_isEditing)
            {
                _tempTask = null;
                _isEditing = false;
            }
        }
    }

    /// <summary>
    /// Коллекция задач
    /// </summary>
    public class Tasks : ObservableCollection<Task>
    {
    }
}