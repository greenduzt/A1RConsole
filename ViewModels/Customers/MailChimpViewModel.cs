using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Categories;
using A1RConsole.Models.Customers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Customers
{
    public class MailChimpViewModel : ViewModelBase, IContent
    {
        private string _noOfRecords;
        private ObservableCollection<Customer> _customerList;
        private ObservableCollection<CategoryCollection> _categoryList;
        private string _selectedState;
        private bool canExecute;
        private ObservableCollection<Category> _categories;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;

        private ICommand _removeCommand;
        private ICommand _searchCommand;
        private ICommand _clearSearchCommand;
        private ICommand _addCategoryCommand;
        private ICommand _exportToExcelCommand;

        public MailChimpViewModel()
        {
            CustomerList = new ObservableCollection<Customer>();
            List<Category> mainCategories = DBAccess.GetCategories();
            if (mainCategories != null && mainCategories.Count > 0)
            {
                Categories = new ObservableCollection<Category>();
                mainCategories.Add(new Category() { CategoryID = 0, CategoryName = "Select" });
                Categories = new ObservableCollection<Category>(mainCategories);
            }
            canExecute = true;
            SelectedState = "Select";
        }


        private ObservableCollection<Customer> SearchCustomer()
        {     
            NoOfRecords =string.Empty;
            if (CategoryList != null && CategoryList.Count > 0)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    for (int i = 0; i < CategoryList.Count; i++)
                    {
                        if (CategoryList[i].SelectedCategory.CategoryName.Equals("Select"))
                        {
                            CategoryList.RemoveAt(i);
                        }
                    }
                }));
            }
            
            CustomerList = DBAccess.GetCustomersByStateByCategories(SelectedState, CategoryList);           
            if (CustomerList.Count > 0)
            {
                NoOfRecords = CustomerList.Count + " records found";                        
            }
            else
            {
                MessageBox.Show("No information found", "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }                    
                   
            return CustomerList;
        }

        private void AddNewCategory()
        {
            if (CategoryList == null)
            {
                CategoryList = new ObservableCollection<CategoryCollection>();
            }
            CategoryList.Add(new CategoryCollection(Categories));
        }

        private void CloseWindow(object p)
        {
            if (this.CanClose)
            {
                var handler = this.Closing;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        private void RemoveCategory(object parameter)
        {
            int index = CategoryList.IndexOf(parameter as CategoryCollection);
            if (index > -1 && index < CategoryList.Count)
            {
                CategoryList.RemoveAt(index);

            }
        }

        private void ClearSearchField()
        {
            if (CategoryList != null && CategoryList.Count > 0)
            {
                CategoryList.Clear();
            }

            SelectedState = "Select";
            CustomerList.Clear();
            NoOfRecords = string.Empty;
        }
        private void ExportToExcel()
        {
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();

            LoadingScreen.ShowWaitingScreen("Exporting.");

            worker.DoWork += (_, __) =>
            {

                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                excel.Visible = false;
                excel.DisplayAlerts = false;

                Microsoft.Office.Interop.Excel.Workbook worKbooK = excel.Workbooks.Add(Type.Missing);
                Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;

                worksheet.Name = "Customers";
                int row = 1;
                worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 8]].Merge();

                try
                {
                    excel = new Microsoft.Office.Interop.Excel.Application();
                    excel.Visible = false;
                    excel.DisplayAlerts = false;
                    worKbooK = excel.Workbooks.Add(Type.Missing);

                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
                    worksheet.Name = "Customers";

                    row++;
                    worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 11]].Merge();
                    worksheet.Cells[row, 1] = "Printed Date Time : " + DateTime.Now;
                    worksheet.Cells[row, 1].Font.Bold = true;
                    worksheet.Cells.Font.Size = 12;
                    row += 2;

                    if (SelectedState != null && !SelectedState.Equals("Select"))
                    {
                        row++;
                        worksheet.Cells[row, 1] = "Filtered by customer state : " + SelectedState;
                        worksheet.Cells[row, 1].Font.Bold = true;
                        worksheet.Cells.Font.Size = 12;
                        worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 11]].Merge();
                    }

                    string categoryName = string.Empty;
                    if (CategoryList != null && CategoryList.Count > 0)
                    {
                        bool run = false;
                        foreach (var item in CategoryList)
                        {
                            if (item.SelectedCategory != null && !item.SelectedCategory.CategoryName.Equals("Select"))
                            {
                                if (!run)
                                {
                                    categoryName += item.SelectedCategory.CategoryName;
                                    run = true;
                                }
                                else
                                {
                                    categoryName += " | " + item.SelectedCategory.CategoryName;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        row++;
                        worksheet.Cells[row, 1] = "Filtered by category : " + categoryName;
                        worksheet.Cells[row, 1].Font.Bold = true;
                        worksheet.Cells[row, 1].Font.Size = 12;
                        worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 11]].Merge();
                    }

                    row++;
                    worksheet.Rows[row].Font.Bold = true;
                    worksheet.Rows[row].Font.Size = 14;
                    worksheet.Cells[row, 1] = "Company Name";
                    worksheet.Cells[row, 2] = "Category Name";
                    worksheet.Cells[row, 3] = "Discount";
                    worksheet.Cells[row, 4] = "Company Email";
                    worksheet.Cells[row, 5] = "Company State";

                    worksheet.Range["A" + row].RowHeight = 30;
                    worksheet.Range["A" + row].ColumnWidth = 100;

                    worksheet.Range["B" + row].RowHeight = 30;
                    worksheet.Range["B" + row].ColumnWidth = 100;

                    worksheet.Range["C" + row].RowHeight = 30;
                    worksheet.Range["C" + row].ColumnWidth = 100;

                    worksheet.Range["D" + row].RowHeight = 30;
                    worksheet.Range["D" + row].ColumnWidth = 100;

                    worksheet.Range["G" + row].RowHeight = 30;
                    worksheet.Range["G" + row].ColumnWidth = 120;

                    worksheet.Range["A" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;
                    worksheet.Range["B" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;
                    worksheet.Range["C" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;
                    worksheet.Range["D" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;

                    row++;

                    foreach (var item in CustomerList)
                    {
                        worksheet.Cells[row, 1] = item.CompanyName;
                        worksheet.Cells[row, 2] = item.PrimaryBusiness.CategoryName;
                        worksheet.Cells[row, 3] = item.DiscountStructure[0].Discount + "%";
                        worksheet.Cells[row, 4] = item.CompanyEmail;
                        worksheet.Cells[row, 5] = item.CompanyState;

                        row++;
                    }

                    worksheet.Columns.AutoFit();

                    string fileName = "Customers" + DateTime.Now.ToString("dd-MM-yyyy-HH.mm.ss");

                    excel.Visible = true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                }
                finally
                {
                    worksheet = null;
                    worKbooK = null;
                }

        };
        worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();

            };
    worker.RunWorkerAsync();
        }

        private void SearchCustomerCalled()
        {
            ObservableCollection<Customer> res = null;
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                res = SearchCustomer();
            };

            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();                
            };

            worker.RunWorkerAsync();
        }
                
        public void Export()
        {
            ObservableCollection<Customer> res = null;
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                res =   SearchCustomer();                        
            };

            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
                if (res.Count > 0)
                {
                    ExportToExcel();
                }
            };

            worker.RunWorkerAsync();            
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        public bool CanClose
        {
            get
            {
                return this.IsDirty == false || MessageBox.Show("Changes were made. Do you want to close this window?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }
        }

        #region PUBLIC_PROPERTIES

        public string NoOfRecords
        {
            get
            {
                return _noOfRecords;
            }
            set
            {
                _noOfRecords = value;
                RaisePropertyChanged("NoOfRecords");
            }
        }
        public ObservableCollection<Customer> CustomerList
        {
            get
            {
                return _customerList;
            }
            set
            {
                _customerList = value;
                RaisePropertyChanged("CustomerList");
            }
        }

        public ObservableCollection<Category> Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                _categories = value;
                RaisePropertyChanged("Categories");
            }
        }

        public string Title
        {
            get
            {
                return "MailChimp Report";
            }
        }

        public string SelectedState
        {
            get
            {
                return _selectedState;
            }
            set
            {
                _selectedState = value;
                RaisePropertyChanged("SelectedState");
            }
        }

        public ObservableCollection<CategoryCollection> CategoryList
        {
            get
            {
                return _categoryList;
            }
            set
            {
                _categoryList = value;
                RaisePropertyChanged("CategoryList");
            }
        }

        #endregion

        #region COMMANDS
        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new CommandHandler(() => SearchCustomerCalled(), canExecute));
            }
        }

        public ICommand ClearSearchCommand
        {
            get
            {
                return _clearSearchCommand ?? (_clearSearchCommand = new CommandHandler(() => ClearSearchField(), canExecute));
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new DelegateCommand(CanExecute, RemoveCategory);
                }
                return _removeCommand;
            }
        }

        public ICommand AddCategoryCommand
        {
            get
            {
                return _addCategoryCommand ?? (_addCategoryCommand = new CommandHandler(() => AddNewCategory(), true));
            }
        }

        public ICommand ExportToExcelCommand
        {
            get
            {
                return _exportToExcelCommand ?? (_exportToExcelCommand = new CommandHandler(() => Export(), true));
            }
        }

        #endregion
    }
}
