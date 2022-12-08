using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using TransportCyclesResolver.Models;

namespace TransportCyclesResolver.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        #region Commands
        public ICommand WindowInitializedCommand { get; set; }
        public ICommand OpenFileCommand { get; set; }
        public ICommand FieldMouseEnterCommand { get; set; }
        public ICommand FieldMouseLeaveCommand { get; set; }
        public ICommand FieldClickCommand { get; set; }
        #endregion

        #region Properties
        public IDialogCoordinator DialogCoordinator { get; set; }

        public List<Field> Fields { get; set; }
        public int FieldsWidth => Fields != null ? Fields.Last().X : 0;
        public int FieldsHeight => Fields != null ? Fields.Last().Y : 0;
        #endregion

        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            DialogCoordinator = dialogCoordinator;

            WindowInitializedCommand = new RelayCommand(WindowInitialized);
            OpenFileCommand = new RelayCommand(OpenFile);
            FieldMouseEnterCommand = new RelayCommand(FieldMouseEnter);
            FieldMouseLeaveCommand = new RelayCommand(FieldMouseLeave);
            FieldClickCommand = new RelayCommand(FieldClick);
        }

        private void WindowInitialized(object obj)
        {
            OpenFile("fields.txt");
        }

        private async void OpenFile(object obj = null)
        {
            string filePath;

            if (obj != null && File.Exists(obj.ToString()))
            {
                filePath = obj.ToString();
            }
            else
            {
                var openFileDialog = new OpenFileDialog()
                {
                    Title = "Choose file to open"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    filePath = openFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }

            string[] fileContent;

            try
            {
                fileContent = File.ReadAllLines(filePath);
            }
            catch
            {
                var dialogCoordinator = (Application.Current.MainWindow.DataContext as MainViewModel).DialogCoordinator;
                await dialogCoordinator.ShowMessageAsync(this, "Error", "Chosen file isn't text type");
                return;
            }

            Fields = new List<Field>();

            for (int i = 0; i < fileContent.Length; i++)
            {
                var row = fileContent[i].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < row.Length; j++)
                {
                    double? value = null;

                    if (double.TryParse(row[j], out double outValue))
                    {
                        value = outValue;
                    }

                    Fields.Add(new Field()
                    {
                        X = j + 1,
                        Y = i + 1,
                        Value = value
                    });
                }
            }

            OnPropertyChanged("Fields");
            OnPropertyChanged("FieldsWidth");
            OnPropertyChanged("FieldsHeight");
        }

        private void FieldMouseEnter(object obj)
        {
            var field = obj as Field;
            if (field != null && !field.IsEmpty)
            {
                field.IsHovered = true;
            }
        }

        private void FieldMouseLeave(object obj)
        {
            var field = obj as Field;
            if (field != null && !field.IsEmpty)
            {
                field.IsHovered = false;
            }
        }

        private void FieldClick(object obj)
        {
            var field = obj as Field;
            if (field != null && !field.IsEmpty)
            {
                field.IsHovered = false;
                var cycle = FindCycle(field);

                foreach(var singleField in Fields)
                {
                    var index = cycle?.IndexOf(singleField);
                    singleField.CycleIndex = index < 0 ? null : index + 1;
                }
            }
        }

        private List<Field> FindCycle(Field startField, List<Field> cycle = null, List<Field> bannedFields = null)
        {
            if (cycle == null)
            {
                cycle = new List<Field>();
                bannedFields = new List<Field>();
            }

            cycle.Add(startField);
            bannedFields.Add(startField);

            // warunek końcowy
            if (cycle.Count() > 2 && (startField.X == cycle[0].X || startField.Y == cycle[0].Y) &&
                !bannedFields.Intersect(GetRoute(startField, cycle[0])).Any())
            {
                return cycle;
            }

            var searchColumn = cycle.Count(f => f.X == startField.X) < 2;
            var searchRow = cycle.Count(f => f.Y == startField.Y) < 2;

            var potentialFields = Fields.FindAll(f => !f.IsEmpty &&
                   !bannedFields.Intersect(GetRoute(startField, f, true)).Any() &&
                   ((searchColumn && f.X == startField.X) || (searchRow && f.Y == startField.Y)))
                .OrderBy(f => f.X != startField.X)
                .OrderBy(f => f.Value == 0)
                .ThenBy(f => f == cycle[0]);

            var position = $"{startField.Value} ({startField.X}, {startField.Y})";
            var last = cycle.Count > 1 ? $"{cycle[cycle.Count() - 2].Value} ({cycle[cycle.Count() - 2].X}, {cycle[cycle.Count() - 2].Y})" : "NONE";
            var potential = string.Join(", ", potentialFields.Select(x => x.Value));
            var banned = string.Join(", ", bannedFields.Select(x => x.Value));

            foreach (var field in potentialFields)
            {
                List<Field> newBannedFields = bannedFields.ToList();

                newBannedFields.AddRange(GetRoute(startField, field));

                var newCycle = FindCycle(field, cycle.ToList(), newBannedFields);

                if (newCycle != null)
                {
                    return newCycle;
                }
            }

            return null;
        }

        private List<Field> GetRoute(Field startField, Field endField, bool includeEndField = false)
        {
            List<Field> route;

            if (startField.X == endField.X)
            {
                route = Fields.FindAll(f => f.X == startField.X && f.Y > Math.Min(startField.Y, endField.Y) && f.Y < Math.Max(startField.Y, endField.Y));
            }
            else // taki sam Y
            {
                route = Fields.FindAll(f => f.Y == startField.Y && f.X > Math.Min(startField.X, endField.X) && f.X < Math.Max(startField.X, endField.X));
            }

            if (includeEndField)
            {
                route.Add(endField);
            }

            return route;
        }

        private Field GetField(int x, int y)
        {
            return Fields.FirstOrDefault(f => f.X == x && f.Y == y && !f.IsEmpty);
        }
    }
}
