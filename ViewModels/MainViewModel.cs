using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    var index = cycle.IndexOf(singleField);
                    singleField.CycleIndex = index < 0 ? (int?)null : index + 1;
                }
            }
        }

        private List<Field> FindCycle(Field startField)
        {
            var endField = startField;
            var cycle = new List<Field>();

            do
            {
                cycle.Add(endField);


                if (cycle.Count(f => f.X == endField.X) < 2)
                {
                    var sameColumnField = Fields.FirstOrDefault(f => !f.IsEmpty && f.X == endField.X && (!cycle.Contains(f) || (cycle.Count > 1 && f == startField)));

                    if (sameColumnField != null)
                    {
                        endField = sameColumnField;
                        continue;
                    }
                }

                if (cycle.Count(f => f.Y == endField.Y) < 2)
                {
                    var sameRowField = Fields.FirstOrDefault(f => !f.IsEmpty && f.Y == endField.Y && (!cycle.Contains(f) || (cycle.Count > 1 && f == startField)));

                    if (sameRowField != null)
                    {
                        endField = sameRowField;
                        continue;
                    }
                }

                if (endField.X == startField.X || endField.Y == startField.Y)
                {
                    break;
                }

                //trzeba zrobić rekurencję do przeszukiwania ścieżki

            } while (endField != startField);

            return cycle;
        }

        private Field GetField(int x, int y)
        {
            return Fields.FirstOrDefault(f => f.X == x && f.Y == y && !f.IsEmpty);
        }
    }
}
