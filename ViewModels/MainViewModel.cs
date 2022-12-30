using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Caching;
using System.Windows;
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
        public ICommand ShowCycleCommand { get; set; }
        #endregion

        #region Properties
        public IDialogCoordinator DialogCoordinator { get; set; }

        private List<Field> _fields;
        public List<Field> Fields
        {
            get
            {
                return _fields;
            }
            set
            {
                _fields = value;
                OnPropertyChanged();
                OnPropertyChanged("FieldsWidth");
                OnPropertyChanged("FieldsHeight");
            }
        }

        public int FieldsWidth => Fields != null ? Fields.Last().X : 0;
        public int FieldsHeight => Fields != null ? Fields.Last().Y : 0;

        private List<Cycle> _cycles;
        public List<Cycle> Cycles
        {
            get
            {
                return _cycles;
            }
            set
            {
                _cycles = value;
                OnPropertyChanged();
            }
        }

        private Cycle _selectedCycle;
        public Cycle SelectedCycle
        {
            get
            {
                return _selectedCycle;
            }
            set
            {
                _selectedCycle = value;
                OnPropertyChanged();
            }
        }

        private string _listBoxTitle;
        public string ListBoxTitle
        {
            get
            {
                return _listBoxTitle;
            }
            set
            {
                _listBoxTitle = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            DialogCoordinator = dialogCoordinator;
            ListBoxTitle = "Select a field";

            WindowInitializedCommand = new RelayCommand(WindowInitialized);
            OpenFileCommand = new RelayCommand(OpenFile);
            FieldMouseEnterCommand = new RelayCommand(FieldMouseEnter);
            FieldMouseLeaveCommand = new RelayCommand(FieldMouseLeave);
            FieldClickCommand = new RelayCommand(FieldClick);
            ShowCycleCommand = new RelayCommand(ShowCycle);
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

            var fields = new List<Field>();

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

                    fields.Add(new Field()
                    {
                        X = j + 1,
                        Y = i + 1,
                        Value = value
                    });
                }
            }

            Fields = fields;
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
                ListBoxTitle = $"Selected field: ({field.X}, {field.Y})";
                field.IsHovered = false;

                var cycles = FindCycles(field);
                cycles = cycles.Where(c => c.Fields.Where(f => f.CycleIndex % 2 == 0).Min(f => f.Value) != 0 &&
                    !c.Fields.Where(f => f.CycleIndex % 2 == 0).Any(f => f.Value - c.Fields.Where(f2 => f2.CycleIndex % 2 == 0).Min(f2 => f2.Value) < 0)).ToList();
                cycles.ForEach(c => c.Name = $"Cycle {("#" + (cycles.IndexOf(c) + 1).ToString()).PadLeft(3)} ({c.Fields?.Count ?? 0} fields)");
                SelectedCycle = cycles.FirstOrDefault();
                Cycles = cycles;
            }
        }

        private void ShowCycle(object obj = null)
        {
            foreach (var field in Fields)
            {
                var index = SelectedCycle?.Fields?.IndexOf(field);
                field.CycleIndex = index < 0 ? null : index + 1;
                field.UpLineGoUpVisibility = Visibility.Collapsed;
                field.UpLineGoDownVisibility = Visibility.Collapsed;
                field.DownLineVisibility = Visibility.Collapsed;
                field.RightLineGoRightVisibility = Visibility.Collapsed;
                field.RightLineGoLeftVisibility = Visibility.Collapsed;
                field.LeftLineVisibility = Visibility.Collapsed;
            }

            if (SelectedCycle?.Fields != null)
            {
                for (int i = 0; i < SelectedCycle.Fields.Count; i++)
                {
                    // ustawiamy względem poprzedniego
                    if (i > 0)
                    {
                        if (SelectedCycle.Fields[i - 1].Y != SelectedCycle.Fields[i].Y)
                        {
                            if (SelectedCycle.Fields[i - 1].Y < SelectedCycle.Fields[i].Y)
                            {
                                SelectedCycle.Fields[i].UpLineGoDownVisibility = Visibility.Visible;
                            }
                            else
                            {
                                SelectedCycle.Fields[i].DownLineVisibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            if (SelectedCycle.Fields[i - 1].X < SelectedCycle.Fields[i].X)
                            {
                                SelectedCycle.Fields[i].LeftLineVisibility = Visibility.Visible;
                            }
                            else
                            {
                                SelectedCycle.Fields[i].RightLineGoLeftVisibility = Visibility.Visible;
                            }
                        }
                    }

                    var nextIndex = i + 1;

                    if (i + 1 >= SelectedCycle.Fields.Count)
                    {
                        nextIndex = 0;
                    }

                    // ustawiamy względem kolejnego
                    if (SelectedCycle.Fields[i].Y != SelectedCycle.Fields[nextIndex].Y)
                    {
                        var isGoingUp = false;

                        if (SelectedCycle.Fields[i].Y < SelectedCycle.Fields[nextIndex].Y)
                        {
                            SelectedCycle.Fields[i].DownLineVisibility = Visibility.Visible;
                        }
                        else
                        {
                            SelectedCycle.Fields[i].UpLineGoUpVisibility = Visibility.Visible;
                            isGoingUp = true;
                        }

                        var fieldsBetween = Fields.FindAll(f => f.X == SelectedCycle.Fields[i].X &&
                            f.Y > Math.Min(SelectedCycle.Fields[i].Y, SelectedCycle.Fields[nextIndex].Y) &&
                            f.Y < Math.Max(SelectedCycle.Fields[i].Y, SelectedCycle.Fields[nextIndex].Y));

                        foreach (var field in fieldsBetween)
                        {
                            if (isGoingUp)
                            {
                                field.UpLineGoUpVisibility = Visibility.Visible;
                            }
                            else
                            {
                                field.UpLineGoDownVisibility = Visibility.Visible;
                            }

                            field.DownLineVisibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        var isGoingRight = false;

                        if (SelectedCycle.Fields[i].X < SelectedCycle.Fields[nextIndex].X)
                        {
                            SelectedCycle.Fields[i].RightLineGoRightVisibility = Visibility.Visible;
                            isGoingRight = true;
                        }
                        else if (SelectedCycle.Fields[i].X > SelectedCycle.Fields[nextIndex].X)
                        {
                            SelectedCycle.Fields[i].LeftLineVisibility = Visibility.Visible;
                        }

                        var fieldsBetween = Fields.FindAll(f => f.Y == SelectedCycle.Fields[i].Y &&
                            f.X > Math.Min(SelectedCycle.Fields[i].X, SelectedCycle.Fields[nextIndex].X) &&
                            f.X < Math.Max(SelectedCycle.Fields[i].X, SelectedCycle.Fields[nextIndex].X));

                        foreach (var field in fieldsBetween)
                        {
                            field.LeftLineVisibility = Visibility.Visible;

                            if (isGoingRight)
                            {
                                field.RightLineGoRightVisibility = Visibility.Visible;
                            }
                            else
                            {
                                field.RightLineGoLeftVisibility = Visibility.Visible;
                            }
                        }
                    } 
                }
            }
        }

        private List<Cycle> FindCycles(Field startField, List<Field> cycle = null, List<Field> bannedFields = null)
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
                !bannedFields.Intersect(GetRoute(startField, cycle[0])).Any() &&
                !GetRoute(startField, cycle[0]).Any(x => x.IsEmpty))
            {
                return new List<Cycle>()
                {
                    new Cycle()
                    {
                        Fields = cycle
                    }
                };
            }

            var searchColumn = cycle.Count(f => f.X == startField.X) < 2;
            var searchRow = cycle.Count(f => f.Y == startField.Y) < 2;

            var potentialFields = Fields.FindAll(f => !f.IsEmpty &&
                   !bannedFields.Intersect(GetRoute(startField, f, true)).Any() &&
                   ((searchColumn && f.X == startField.X) || (searchRow && f.Y == startField.Y)) &&
                   !GetRoute(startField, f).Any(x => x.IsEmpty))
                .OrderBy(f => f.X != startField.X)
                .OrderBy(f => f.Value == 0)
                .ThenBy(f => f == cycle[0]);

            var position = $"{startField.Value} ({startField.X}, {startField.Y})";
            var last = cycle.Count > 1 ? $"{cycle[cycle.Count() - 2].Value} ({cycle[cycle.Count() - 2].X}, {cycle[cycle.Count() - 2].Y})" : "NONE";
            var potential = string.Join(", ", potentialFields.Select(x => x.Value));
            var banned = string.Join(", ", bannedFields.Select(x => x.Value));

            var cycles = new List<Cycle>();

            foreach (var field in potentialFields)
            {
                List<Field> newBannedFields = bannedFields.ToList();

                newBannedFields.AddRange(GetRoute(startField, field));

                var newCycle = FindCycles(field, cycle.ToList(), newBannedFields);
                cycles.AddRange(newCycle);
            }

            return cycles;
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
    }
}
