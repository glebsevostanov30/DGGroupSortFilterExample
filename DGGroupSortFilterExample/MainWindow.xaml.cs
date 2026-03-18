using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using DGGroupSortFilterExample.converter;
using DGGroupSortFilterExample.model;
using Task = DGGroupSortFilterExample.model.Task;

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

            string[] projects = ["Разработка", "Маркетинг", "Документация", "Тестирование", "Аналитика"];
            string[] priorities = ["Высокий", "Средний", "Низкий"];
            string[] employees = ["Иванов", "Петров", "Сидоров", "Козлов", "Морозов", "Волков"];
            var random = new Random();

            for (var i = 1; i <= 30; i++)
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

                var groupingInfo = cvTasks.GroupDescriptions.Count > 0 
                    ? $"Группировка по {cvTasks.GroupDescriptions.Count} уровням" 
                    : "Группировка отключена";
                
                UpdateStatus(groupingInfo);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка группировки: {ex.Message}");
            }
        }

        private static void AddGroupDescription(ICollectionView cvTasks, ComboBox comboBox, bool sortGroups)
        {
            if (comboBox.SelectedItem is not ComboBoxItem item ||
                item.Content.ToString() == "Без группировки") return;
            
            var content = item.Content.ToString();
            if (content != null)
            {
                var propertyName = ExtractPropertyName(content);

                var groupDesc =
                    // Специальная группировка по датам (по месяцам)
                    propertyName == "DueDate" ?
                        new PropertyGroupDescription(propertyName, new MonthGroupConverter()) 
                        : new PropertyGroupDescription(propertyName);
                
                cvTasks.GroupDescriptions.Add(groupDesc);
            }

            // Если нужно сортировать группы
            if (sortGroups)
            {
                // Для сортировки групп нужно добавить соответствующую сортировку
                // Это можно реализовать через SortDescriptions
            }
        }

        private static string ExtractPropertyName(string comboBoxContent)
        {
            if (comboBoxContent.Contains("ProjectName")) return "ProjectName";
            if (comboBoxContent.Contains("Complete")) return "Complete";
            if (comboBoxContent.Contains("Priority")) return "Priority";
            if (comboBoxContent.Contains("AssignedTo")) return "AssignedTo";
            return comboBoxContent.Contains("DueDate") ? "DueDate" : comboBoxContent.Split(' ')[0];
        }

        private void ClearGrouping_Click(object sender, RoutedEventArgs e)
        {
            cmbGroupBy1.SelectedIndex = 0;
            cmbGroupBy2.SelectedIndex = 0;
            cmbGroupBy3.SelectedIndex = 0;
            
            var cvTasks = CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource);
            if (cvTasks is { CanGroup: true })
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
            TxtStatus?.Text = message;
        }
    }
}