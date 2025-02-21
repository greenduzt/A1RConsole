using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Products;
using A1RConsole.Models.Quoting;
using A1RConsole.Models.Users;
using A1RConsole.PdfGeneration;
using A1RConsole.ViewModels.Orders.NewOrderPDF;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace A1RConsole.ViewModels.Quoting
{
    public class ViewUpdateQuoteViewModel : ViewModelBase, IContent
    {
        //private string _quoteNoSearch;
        private string _projectNameSearch;
        //private string _customerNameSearch;
        private string _prePaidCustomerName;
        private string _searchString;
        //private ICollectionView _itemsView;
        private ObservableCollection<Quote> _quoteList;
        private ObservableCollection<NewOrderPDFM> _convertedQuotes;
        private Quote _quote;
        //public ObservableCollection<Product> Product { get; set; }
        private List<string> _quoteNoList;
        private List<string> _projectNames;
        private List<DiscountStructure> _displayDiscountStructure;
        private List<DiscountStructure> _discountStructure;
        private ObservableCollection<FreightCode> _orgFreightCodeDetails;
        private int _tabSelectedIndex;
        private string _selectedProjectName;
        private string _selectedQuoteNo;
        private List<string> _cusProjectNames;
        private string _selectedSearchedCustomerName;
        private bool IsDirty { get; set; }
        private string _discountAppliedTextVisibility;
        private string _quoteDateStr;
        private Customer _selectedCustomer;
        private int _itemCount;
        private bool _addShippingAddress;
        private bool _projectNameEnabled;
        private bool _soldToEnabled;
        private bool _shipToEnabled;
        private bool _quoteDetailsEnabled;
        private bool _freightDetailsEnabled;
        private bool _instructionsEnabled;
        private bool _internalCommentsEnabled;
        private bool _shippingEnabled;
        private bool _addShippingAddressEnabled;
        private string _isUpdateEnabled;
        private string _refreshButtonVisibility;
        private string _otherContactName;
        private string _otherContactNamePhone;
        private string _otherContactPersonVisibility;
        private string _contactPersonPhoneVisibility;
        private string _outDatedQuoteTextVisibility;
        //private string _viewPDFBackGroundCol;
        private string _shippingAddressVisibility;
        private string _shipToText;
        private string _quoteButtonVisiblity;
        private string _projectNameSearchOrder;
        private string searchOrderType;
        private FreightDetails _selectedFreightDetails;
        private bool _viewPDFEnabled;
        private bool _allChecked;
        private bool _convertedQuotesChecked;
        private bool _directOrdersChecked;
        public static ViewUpdateQuoteViewModel instance;

        public event EventHandler Closing;
        public RelayCommand CloseCommand { get; private set; }
        private ICommand _viewUpdateCommand;
        private ICommand _searchCommand;
        //private ICommand _searchProductCommand;
        private ICommand _removeCommand;
        //private ICommand _selectionChangedCommand;
        private ICommand _removeFreightCodeCommand;
        private ICommand _freightPriceKeyUpCommand;
        private ICommand _clearCommand;
        private ICommand _lostFocusCommand;
        //private ICommand _freightLostFocusCommand;
        private ICommand _updateCommand;
        private ICommand _discountLostFocusCommand;
        private ICommand _deleteQuoteCommand;
        private ICommand _refreshGridCommand;
        private ICommand _sendToSalesCommand;
        private ICommand _searchQuoteListCommand;
        private ICommand _clearQuoteListCommand;
        private ICommand _quoteToOrderCommand;
        private ICommand _refreshConvQuotesGridCommand;
        private ICommand _searchOrderListCommand;
        private ICommand _clearOrderListCommand;
        private ICommand _viewUpdateOrderCommand;
        private ICommand _viewPDFCommand;
        private ICommand _viewCompletedPDFCommand;
        MainWindowViewModel mainWindowViewModelRefernce;
        private int _start = 0;
        private int _startCQ = 0;

        private int itemCount = 20;
        private int itemCountCQ = 20;           

        private int _totalItems = 0;
        private int _totalItemsCQ = 0;

        private ICommand _firstCommand;
        private ICommand _firstCQCommand;

        private ICommand _previousCommand;
        private ICommand _previousCQCommand;

        private ICommand _nextCommand;
        private ICommand _nextCommandCQ;

        private ICommand _lastCommand;
        private ICommand _lastCommandCQ;

        public ViewUpdateQuoteViewModel()
        {
            QuoteList = new ObservableCollection<Quote>();
            QuoteNoList = new List<string>();
            ProjectNames = new List<string>();
            CusProjectNames = new List<string>();
            DiscountStructure = new List<DiscountStructure>();
            Quote = new Quote();
            Quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
            Quote.FreightDetails = new BindingList<FreightDetails>();           
            OrgFreightCodeDetails = new ObservableCollection<FreightCode>();
            QuoteDateStr = string.Empty;
            DiscountAppliedTextVisibility = "Collapsed";
            OtherContactPersonVisibility = "Collapsed";
            ContactPersonPhoneVisibility = "Collapsed";
            OutDatedQuoteTextVisibility = "Collapsed";
            ShippingAddressVisibility = "Visible";
            //ViewPDFBackGroundCol = "#a6a6a6";
            AddShippingAddress = false;
            ProjectNameEnabled = false;
            SoldToEnabled = false;
            ShipToEnabled = false;
            QuoteDetailsEnabled = false;
            FreightDetailsEnabled = false;
            InstructionsEnabled = false;
            InternalCommentsEnabled = false;
            ShippingEnabled = false;
            AddShippingAddressEnabled = false;
            AllChecked = true;
            ConvertedQuotesChecked = false;
            DirectOrdersChecked = false;
            searchOrderType = "All";
            TabSelectedIndex = 0;
            SelectedFreightDetails = new FreightDetails();
            //NewOrderPDFViewLoadingLocation.IsLoadFromMainMenu = false;

            foreach (var item in UserData.UserPrivilages)
            {
                if (item.Area.Trim() == "Quoting->NewQuote")
                {
                    IsUpdateEnabled = item.Visibility;
                    break;
                }
            }

            ProjectNameSearch =string.Empty;
            ProjectNameSearchOrder = string.Empty;
            Quote.QuoteDetails.CollectionChanged += productChanged;
            Quote.FreightDetails.ListChanged += freightChanged;
                        
            this.CloseCommand = new RelayCommand(CloseWindow);

            instance = this;
        }

        private void LoadQuotes()
        {
            //BackgroundWorker worker = new BackgroundWorker();
            //ChildWindow LoadingScreen = new ChildWindow();
            //LoadingScreen.ShowWaitingScreen("Loading");

            //worker.DoWork += (_, __) =>
            //{
                //ObservableCollection<Quote> quoteList= DBAccess.GetAllQuotes(_start, itemCount, out _totalItems, ProjectNameSearch, TabSelectedIndex);

                //var clonedFreightDetails = quoteList.Select(objEntity => (Quote)objEntity.Clone()).ToList();

                //QuoteList = new ObservableCollection<Quote>(clonedFreightDetails);

                QuoteList = DBAccess.GetAllQuotes(_start, itemCount, out _totalItems, ProjectNameSearch, TabSelectedIndex);
       
                RaisePropertyChanged("Start");
                RaisePropertyChanged("End");
                RaisePropertyChanged("TotalItems");

            //};
            //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            //{
            //    LoadingScreen.CloseWaitingScreen();
            //};
            //worker.RunWorkerAsync();
        }

        private void LoadConvertedQuotes()
        {
            ConvertedQuotes = DBAccess.GetAllConvertedQuotes(_startCQ, itemCountCQ, out _totalItemsCQ, ProjectNameSearchOrder, TabSelectedIndex,searchOrderType);

            RaisePropertyChanged("StartCQ");
            RaisePropertyChanged("EndCQ");
            RaisePropertyChanged("TotalItemsCQ");
        }

        public void RefreshGrid()
        {
            _start = 0;
            itemCount = 20;
            _totalItems = 0;
            ProjectNameSearch = string.Empty;
            QuoteList.Clear();

            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                LoadQuotes();
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
        }

        public void RefreshConvQuotesGrid()
        {
            _startCQ = 0;
            itemCountCQ = 20;
            _totalItemsCQ = 0;
            ProjectNameSearchOrder = string.Empty;
            AllChecked = true;
            ConvertedQuotesChecked = false;
            DirectOrdersChecked = false;
            searchOrderType = "All";
            if (ConvertedQuotes != null)
            {
                ConvertedQuotes.Clear();
            }
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                LoadConvertedQuotes();
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
        }

        private void FetchQuoteAndProjectNames()
        {
            if (QuoteList == null || QuoteList.Count == 0)
            {
                //MessageBox.Show("Cannot load Quote List", "Loading Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                //BackgroundWorker worker = new BackgroundWorker();
                //ChildWindow LoadingScreen = new ChildWindow();
                //LoadingScreen.ShowWaitingScreen("Loading");

                //worker.DoWork += (_, __) =>
                //{
                    QuoteNoList = new List<string>();
                    ProjectNames = new List<string>();
                    foreach (var item in QuoteList)
                    {
                        QuoteNoList.Add(item.QuoteNo.ToString());
                        ProjectNames.Add(item.ProjectName);
                    }

                    QuoteNoList = QuoteNoList.OrderByDescending(x => x).ToList();
                    ProjectNames = ProjectNames.OrderByDescending(x => x).ToList();
                //};

                //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                //{
                //    LoadingScreen.CloseWaitingScreen();
                //};
                //worker.RunWorkerAsync();
            }
        }

        void productChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ItemCount = this.Quote.QuoteDetails.Count;

            // Resequence list
            // SequencingService.SetCollectionSequence(this.Quote.QuoteDetails);
            CalculateFinalTotal();
        }

        void freightChanged(object sender, ListChangedEventArgs e)
        {
            CalculateFinalTotal();
        }

        private void FetchCustomerAndProjectNames()
        {
            if (QuoteList == null || QuoteList.Count == 0)
            {
               // MessageBox.Show("Cannot load Quote List", "Loading Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {

                //BackgroundWorker worker = new BackgroundWorker();
                //ChildWindow LoadingScreen = new ChildWindow();
                //LoadingScreen.ShowWaitingScreen("Loading");

                //worker.DoWork += (_, __) =>
                //{
                    CusProjectNames = new List<string>();
                    foreach (var item in QuoteList)
                    {
                        CusProjectNames.Add(item.Customer.CompanyName + " [" + item.ProjectName + "]");
                    }

                    CusProjectNames = CusProjectNames.OrderByDescending(x => x).ToList();

                //};

                //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                //{
                //    LoadingScreen.CloseWaitingScreen();
                //};
                //worker.RunWorkerAsync();
            }
        }

        public string Title
        {
            get
            {
                return "View/Update Quote";
            }
        }

        private void CloseWindow(object p)
        {
            if (this.CanClose)
            {
                var hander = this.Closing;
                if (hander != null)
                {
                    hander(this, EventArgs.Empty);
                }
            }
        }

        private void ViewUpdateQuote(Object parameter)
        {

            int index = QuoteList.IndexOf(parameter as Quote);
            if (index >= 0)
            {
                bool exist = DBAccess.CheckRecordExist(QuoteList[index].QuoteNo, "Quote");
                if (exist)
                {
                    //TabSelectedIndex = 1;
                    //SelectedQuoteNo = QuoteList[index].QuoteNo.ToString();
                    SearchQuote(QuoteList[index].QuoteNo);
                    //UpdateQuote();
                }
                else
                {
                    MessageBox.Show("This quote does not exist", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshGrid();
                }
            }
        }
        private void QuoteToOrder(Object parameter)
        {
            int index = QuoteList.IndexOf(parameter as Quote);
            if (index >= 0)
            {               
                
                //Check if this quote available
                bool exist = DBAccess.CheckRecordExist(QuoteList[index].QuoteNo, "QuoteSentToAdmin");
                if (exist ==false)
                {
                    mainWindowViewModelRefernce = MainWindowViewModel.instance;
                    if (mainWindowViewModelRefernce != null)
                    {
                        mainWindowViewModelRefernce.ShowNewOrderPDF(QuoteList[index].QuoteNo, 0);
                    }
                }
                else
                {
                    MessageBox.Show("This quote has already been sent to order", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshGrid();
                }
            }
        }

        private void ViewUpdateOrder(Object parameter)
        {
            int index = ConvertedQuotes.IndexOf(parameter as NewOrderPDFM);
            if (index >= 0)
            {
                //NewOrderPDFViewLoadingLocation.NewOrderPDFM = ConvertedQuotes[index];
                //TabSelectedIndex = 3;

                mainWindowViewModelRefernce = MainWindowViewModel.instance;
                if(mainWindowViewModelRefernce != null)
                {
                    mainWindowViewModelRefernce.ShowNewOrderPDF(0, ConvertedQuotes[index].ID);
                }                  
            }
        }

        private void DeleteQuote(Object parameter)
        {
            int index = QuoteList.IndexOf(parameter as Quote);
            if (index >= 0)
            {
                bool exist = DBAccess.CheckRecordExist(QuoteList[index].QuoteNo, "Quote");
                if (exist)
                {
                    //Deleting quote
                    if (MessageBox.Show("Are you sure you want to delete quote no - " + QuoteList[index].QuoteNo, "Deleting Quote", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        int res = DBAccess.DeleteQuote(QuoteList[index].QuoteNo);
                        if (res == 1)
                        {
                            MessageBox.Show("Quote no - " + QuoteList[index].QuoteNo + " deleted", "Quote Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Could not delete Quote no - " + QuoteList[index].QuoteNo + System.Environment.NewLine + "Please try again later", "Quote Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        RefreshGrid();
                    }
                }
                else
                {
                    MessageBox.Show("This quote does not exist", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshGrid();
                }
            }           
        }

        private void ConvertQuoteToSales(Object parameter)
        {
            int index = QuoteList.IndexOf(parameter as Quote);
            if (index >= 0)
            {
                bool exist = DBAccess.CheckRecordExist(QuoteList[index].QuoteNo, "Quote");
                if (exist)
                {
                    if (MessageBox.Show("Are you sure you want to convert quote no - " + QuoteList[index].QuoteNo + " to a sales order?", "Convert To Sales Order", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        PendingQuote pq = new PendingQuote();
                        pq.QuoteNo = QuoteList[index].QuoteNo;
                        pq.SentToPendingSale = true;
                        pq.SentToPendingSaleBy = new User() { FullName = UserData.FirstName + " " + UserData.LastName };
                        pq.SentToPendingSaleDate = DateTime.Now;

                        pq.PendingSaleToSale = false;
                    
                        int res = DBAccess.SendtoPendingSale(pq);
                        if(res > 0 )
                        {
                            MessageBox.Show("Quote no - " + QuoteList[index].QuoteNo + " was sent to Pending Sales", "Quote Sent", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Could not send Quote no - " + QuoteList[index].QuoteNo + " to Pending Sales" + System.Environment.NewLine + "Please try again later", "Quote Sending Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        BackgroundWorker worker = new BackgroundWorker();
                        ChildWindow LoadingScreen = new ChildWindow();
                        LoadingScreen.ShowWaitingScreen("Loading");

                        worker.DoWork += (_, __) =>
                        {
                            LoadQuotes();
                        };
                        worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                        {
                            LoadingScreen.CloseWaitingScreen();
                        };
                        worker.RunWorkerAsync();
                    }
                }
                else
                {
                    MessageBox.Show("This quote does not exist", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    BackgroundWorker worker = new BackgroundWorker();
                    ChildWindow LoadingScreen = new ChildWindow();
                    LoadingScreen.ShowWaitingScreen("Loading");

                    worker.DoWork += (_, __) =>
                    {
                        LoadQuotes();
                    };
                    worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    {
                        LoadingScreen.CloseWaitingScreen();
                    };
                    worker.RunWorkerAsync();
                }
            }           
        }
                

        private bool CheckQuoteExpired(ObservableCollection<QuoteDetails> qdList, BindingList<FreightDetails> fdList)
        {
            bool skipPrice = false;
            bool outDated = false;
            Tuple<List<Product>,FreightDetails> tuple = DBAccess.GetProductByProductNo(qdList, fdList);

            if (tuple.Item1 != null && tuple.Item1.Count > 0)
            {
                string[] prods = null;
                prods = MetaDataManager.GetPriceEditingProducts();
                
                foreach (var item in Quote.QuoteDetails)
                {
                    var p = prods.SingleOrDefault(x => Convert.ToInt16(x) == item.Product.ProductID);

                    if (p == null)
                    {
                        var data = tuple.Item1.SingleOrDefault(x => x.ProductID == item.Product.ProductID);
                        if (data != null)
                        {
                            if (p != null)
                            {
                                skipPrice = true;
                            }

                            if (skipPrice == false && item.Product.UnitPrice != data.UnitPrice)
                            {
                                outDated = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (tuple.Item2 != null && outDated==false)
            {
                foreach (var item in Quote.FreightDetails)
                {
                    if (tuple.Item2.FreightCodeDetails.ID != 50)
                    {
                        if (tuple.Item2.FreightCodeDetails.ID == item.FreightCodeDetails.ID && tuple.Item2.FreightCodeDetails.Price != item.FreightCodeDetails.Price)
                        {
                            outDated = true;
                            break;
                        }
                    }
                }
            }

            return outDated;
        }

        private void SearchQuote(int quoteNo)
        {           
            Quote = DBAccess.GetQuote(quoteNo);                       

            if (Quote.FreightDetails.Count > 0)
            {
                SelectedFreightDetails.FreightCodeDetails = new FreightCode();
                SelectedFreightDetails.FreightCodeDetails.Code = Quote.FreightDetails[0].FreightCodeDetails.Code;
                SelectedFreightDetails.FreightCodeDetails.Description = Quote.FreightDetails[0].FreightCodeDetails.Description;
                SelectedFreightDetails.FreightCodeDetails.Unit = Quote.FreightDetails[0].FreightCodeDetails.Unit;
                SelectedFreightDetails.FreightCodeDetails.Price = Quote.FreightDetails[0].FreightCodeDetails.Price;
                SelectedFreightDetails.PalletsStr = Quote.FreightDetails[0].PalletsStr;
                SelectedFreightDetails.Discount = Quote.FreightDetails[0].Discount;
            }

            if (Quote.Customer.CompanyName != null && Quote.Customer.CompanyName != "Select")
            {
                DiscountStructure = DBAccess.GetDiscount(Quote.Customer.CustomerId);
                DisplayDiscountStructure = new List<DiscountStructure>();
                            
                for (int i = 0; i < DiscountStructure.Count; i++)
                {
                    if (DiscountStructure[i].Discount > 0)
                    {
                        DisplayDiscountStructure.Add(DiscountStructure[i]);
                    }
                }
            }                        

            UpdateQuote();                                
        }
                       

        private void CalculateQtyToMake()
        {
            if (DiscountStructure != null || DiscountStructure.Count > 0)
            {
                bool disApplied = false;
                if (Quote.QuoteDetails != null)
                {
                    //Apply non discount products
                    var noDiscountProducts = UserData.MetaData.SingleOrDefault(x => x.KeyName == "no_discount_products");
                    string[] noDiscountProductsArr = noDiscountProducts.Description.Split('|');

                    foreach (var item in Quote.QuoteDetails)
                    {
                        if (item.Product != null)
                        {
                            bool isNoDisExists = noDiscountProductsArr.Any(x => Convert.ToInt16(x) == item.Product.ProductID);
                            if (isNoDisExists == false)
                            {
                                foreach (var items in DiscountStructure)
                                {
                                    if (item.Product != null && items.Category.CategoryID == item.Product.Category.CategoryID && item.Discount == items.Discount)
                                    {
                                        disApplied = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                DiscountAppliedTextVisibility = disApplied ? "Visible" : "Collapsed";
            }

            CalculateFinalTotal();
        }


        private void CalculateFinalTotal()
        {
            if (Quote != null)
            {
                for (int i = 0; i < Quote.QuoteDetails.Count; i++)
                {
                    CalculateRowTotal(Quote.QuoteDetails[i]);
                }

                Quote.ListPriceTotal = Quote.QuoteDetails.Sum(x => x.ProductTotalBeforeDis);
                Quote.DiscountedTotal = Quote.QuoteDetails.Sum(x => x.DiscountedTotal);

                decimal noOfPallets = Convert.ToDecimal(string.IsNullOrWhiteSpace(SelectedFreightDetails.PalletsStr) == true ? 0 : Convert.ToDecimal(SelectedFreightDetails.PalletsStr));
                decimal freightPrice = SelectedFreightDetails.FreightCodeDetails == null ? 0 : SelectedFreightDetails.FreightCodeDetails.Price;

                Quote.FreightTotal = freightPrice * noOfPallets - (((freightPrice * noOfPallets) * SelectedFreightDetails.Discount) / 100);
                if (Quote.GSTActive)
                {
                    Quote.Gst = Quote.GSTActive ? Math.Round((((Quote.ListPriceTotal - Quote.DiscountedTotal) + Quote.FreightTotal) * 10) / 100, 2) : 0;
                }
                else
                {
                    Quote.Gst = 0;
                }

                Quote.TotalBeforeTax = Quote.ListPriceTotal - Quote.DiscountedTotal;
                Quote.TotalAmount = Math.Round(Quote.TotalBeforeTax, 2) + Math.Round(Quote.FreightTotal, 2) + Math.Round(Quote.Gst, 2);                               
            }
        }

        private void CalculateFinalTotalNoForm()
        {
            if (Quote != null)
            {
                if (Quote.QuoteDetails != null && Quote.QuoteDetails.Count > 0)
                {
                    for (int i = 0; i < Quote.QuoteDetails.Count; i++)
                    {
                        CalculateRowTotal(Quote.QuoteDetails[i]);
                    }

                    Quote.ListPriceTotal = Quote.QuoteDetails.Sum(x => x.ProductTotalBeforeDis);
                    Quote.DiscountedTotal = Quote.QuoteDetails.Sum(x => x.DiscountedTotal);
                    Quote.FreightTotal = Quote.FreightDetails[0].DummyPrice * Quote.FreightDetails[0].Pallets - (((Quote.FreightDetails[0].DummyPrice * Quote.FreightDetails[0].Pallets) * Quote.FreightDetails[0].Discount) / 100);

                    if (Quote.GSTActive)
                    {
                        Quote.Gst = Quote.GSTActive ? Math.Round((((Quote.ListPriceTotal - Quote.DiscountedTotal) + Quote.FreightTotal) * 10) / 100, 2) : 0;
                    }
                    else
                    {
                        Quote.Gst = 0;
                    }

                    Quote.TotalBeforeTax = Quote.ListPriceTotal - Quote.DiscountedTotal;
                    Quote.TotalAmount = Math.Round(Quote.TotalBeforeTax, 2) + Math.Round(Quote.FreightTotal, 2) + Math.Round(Quote.Gst, 2);
                }
                else
                {
                    MessageBox.Show("System was unable to find the item details", "", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                }
            }
            else
            {
                MessageBox.Show("System was unable to find the quote", "", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
            }
        }

        private NewOrderPDFM CalculateFinalTotalNoFormOrders(NewOrderPDFM newOrderPDF)
        {
            if (newOrderPDF != null)
            {
                if (newOrderPDF.QuoteDetails != null && newOrderPDF.QuoteDetails.Count > 0)
                {

                    for (int i = 0; i < newOrderPDF.QuoteDetails.Count; i++)
                    {
                        CalculateRowTotal(newOrderPDF.QuoteDetails[i]);
                    }

                    newOrderPDF.ListPriceTotal = newOrderPDF.QuoteDetails.Sum(x => x.ProductTotalBeforeDis);
                    newOrderPDF.DiscountedTotal = newOrderPDF.QuoteDetails.Sum(x => x.DiscountedTotal);
                    newOrderPDF.FreightTotal = newOrderPDF.FreightDetails[0].DummyPrice * newOrderPDF.FreightDetails[0].Pallets - (((newOrderPDF.FreightDetails[0].DummyPrice * newOrderPDF.FreightDetails[0].Pallets) * newOrderPDF.FreightDetails[0].Discount) / 100);

                    if (newOrderPDF.GSTActive)
                    {
                        newOrderPDF.Gst = newOrderPDF.GSTActive ? Math.Round((((newOrderPDF.ListPriceTotal - newOrderPDF.DiscountedTotal) + newOrderPDF.FreightTotal) * 10) / 100, 2) : 0;
                    }
                    else
                    {
                        newOrderPDF.Gst = 0;
                    }

                    newOrderPDF.TotalBeforeTax = newOrderPDF.ListPriceTotal - newOrderPDF.DiscountedTotal;
                    newOrderPDF.TotalAmount = Math.Round(newOrderPDF.TotalBeforeTax, 2) + Math.Round(newOrderPDF.FreightTotal, 2) + Math.Round(newOrderPDF.Gst, 2);
                }
                else
                {
                    MessageBox.Show("System was unable to find the item details", "", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                }
            }
            else
            {
                MessageBox.Show("System was unable to find the order", "", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
            }

            return newOrderPDF;
        }


        private void CalculateRowTotal(QuoteDetails qd)
        {
            decimal subTotal = qd.QuoteUnitPrice * qd.Quantity;
            decimal discountedTotal = (subTotal * qd.Discount) / 100;
            qd.Total = subTotal - discountedTotal;

            qd.ProductTotalBeforeDis = subTotal;
            qd.DiscountedTotal = discountedTotal;


        }

        private void ReAddOriginalFreightPrice()
        {

            //for (int i = 0; i < FreightCodeDetails.Count; i++)
            //{
            //    var d = OrgFreightCodeDetails.SingleOrDefault(x=> x.ID == FreightCodeDetails[i].ID);
            //    if(d !=null)
            //    {
            //        FreightCodeDetails[i].Price = d.Price;

            //        Console.WriteLine(FreightCodeDetails[i].ID + " " + FreightCodeDetails[i].Price + " " + d.Price);
            //    }
            //}



            CalculateFinalTotal();
        }

        private void RemoveProduct(object parameter)
        {
            int index = Quote.QuoteDetails.IndexOf(parameter as QuoteDetails);
            if (index > -1 && index < Quote.QuoteDetails.Count)
            {
                Quote.QuoteDetails.RemoveAt(index);
                ObservableCollection<QuoteDetails> tempColl = new ObservableCollection<QuoteDetails>();
                tempColl = Quote.QuoteDetails;
                Quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
                Quote.QuoteDetails = tempColl;
            }
        }

        private void DeleteFreightCode(object parameter)
        {
            int index = Quote.FreightDetails.IndexOf(parameter as FreightDetails);
            if (index > -1 && index < Quote.FreightDetails.Count)
            {
                Quote.FreightDetails.RemoveAt(index);
            }
            if (Quote.FreightDetails.Count == 0)
            {
                Quote.FreightDetails = new BindingList<FreightDetails>();
                Quote.FreightDetails.ListChanged += freightChanged;
            }
        }

        private void CalculateFreightTotal()
        {
            foreach (var item in Quote.FreightDetails)
            {
                item.Total = item.FreightCodeDetails.Price * item.Pallets;
            }

            CalculateFinalTotal();
        }

        private void ClearFields()
        {
            SelectedCustomer = null;
            SelectedCustomer = new Customer();
            SelectedCustomer.CompanyName = "Select";
            Quote = null;
            Quote = new Quote();
            Quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
            Quote.QuoteDetails.CollectionChanged += productChanged;
            Quote.FreightDetails = new BindingList<FreightDetails>();
            Quote.FreightDetails.ListChanged += freightChanged;
            Quote.Customer = new Customer();
            Quote.FreightCarrier = new FreightCarrier();
            Quote.QuoteDate = DateTime.Now;
            AddShippingAddress = false;
            QuoteDateStr = string.Empty;
            AddShippingAddressEnabled = false;

            DiscountStructure.Clear();
            DiscountStructure = new List<DiscountStructure>();
            if (DisplayDiscountStructure != null)
            {
                DisplayDiscountStructure.Clear();
                DisplayDiscountStructure = new List<DiscountStructure>();
            }


            ProjectNameEnabled = false;
            SoldToEnabled = false;
            ShipToEnabled = false;
            QuoteDetailsEnabled = false;
            FreightDetailsEnabled = false;
            InstructionsEnabled = false;
            InternalCommentsEnabled = false;
            ShippingEnabled = false;
            DiscountAppliedTextVisibility = "Collapsed";

            SelectedFreightDetails = new FreightDetails();

            //Quote.User = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
            //Quote.LastUpdatedBy = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
            //Quote.LastUpdatedDate = DateTime.Now;
        }

        private void ClearSearchFields()
        {
            SelectedQuoteNo = string.Empty;
            SelectedProjectName = string.Empty;
            SelectedSearchedCustomerName = string.Empty;
            OutDatedQuoteTextVisibility = "Collapsed";

            ClearFields();
        }

        private void UpdateQuote()
        {
            if (Quote.QuoteNo == 0)
            {
                MessageBox.Show("Enter quote no/ Project name/ Customer name and search at the top to retrieve quote", "Quote Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                //Create a new Quote

                //List<YourType> newList = new List<YourType>(oldList);

                // Quote q = (Quote)Quote.Shallowcopy();

                //Copying without reference
                Quote q = new Quote();
                q.QuoteNo = Quote.QuoteNo;                
                //q.Customer = Quote.Customer;
                q.QuoteDetails = new ObservableCollection<QuoteDetails>();
                q.ContactPerson = new ContactPerson();
                q.ContactPerson.Active = Quote.ContactPerson.Active;
                q.ContactPerson.ContactPersonID = Quote.ContactPerson.ContactPersonID;
                q.ContactPerson.ContactPersonName =  Quote.ContactPerson.ContactPersonName;
                q.ContactPerson.CustomerID = Quote.ContactPerson.CustomerID;
                q.ContactPerson.Email = Quote.ContactPerson.Email;
                q.ContactPerson.PhoneNumber1 = Quote.ContactPerson.PhoneNumber1;
                q.ContactPerson.PhoneNumber2 = Quote.ContactPerson.PhoneNumber2;
                q.ContactPerson.TimeStamp = Quote.ContactPerson.TimeStamp;

                q.FreightDetails = new BindingList<FreightDetails>();
                q.InternalComments = Quote.InternalComments;
                q.Instructions = Quote.Instructions;
                q.GSTActive = Quote.GSTActive;

                q.Customer = new Customer();
                q.Customer.Active = Quote.Customer.Active;
                q.Customer.CompanyAddress = Quote.Customer.CompanyAddress;
                q.Customer.CompanyCity = Quote.Customer.CompanyCity;
                q.Customer.CompanyCountry = Quote.Customer.CompanyCountry;
                q.Customer.CompanyEmail = Quote.Customer.CompanyEmail;
                q.Customer.CompanyFax = Quote.Customer.CompanyFax;                
                q.Customer.CompanyName = Quote.Customer.CompanyName;
                q.Customer.CompanyPostCode = Quote.Customer.CompanyPostCode;
                q.Customer.CompanyState = Quote.Customer.CompanyState;
                q.Customer.CompanyTelephone = Quote.Customer.CompanyTelephone;
                q.Customer.ContactPerson = Quote.Customer.ContactPerson;
                q.Customer.CreditLimit = Quote.Customer.CreditLimit;
                q.Customer.CreditOwed = Quote.Customer.CreditOwed;
                q.Customer.CreditRemaining = Quote.Customer.CreditRemaining;
                q.Customer.CustomerId = Quote.Customer.CustomerId;
                q.Customer.CustomerType = Quote.Customer.CustomerType;
                q.Customer.PrimaryBusiness = Quote.Customer.PrimaryBusiness;
                q.Customer.ShipAddress = Quote.Customer.ShipAddress;
                q.Customer.ShipCity = Quote.Customer.ShipCity;
                q.Customer.ShipCountry = Quote.Customer.ShipCountry;
                q.Customer.ShipPostCode = Quote.Customer.ShipPostCode;
                q.Customer.ShipState = Quote.Customer.ShipState;
                q.Customer.TimeStamp = Quote.Customer.TimeStamp;
                q.ProjectName = Quote.ProjectName;
                q.QuoteCourierName = Quote.QuoteCourierName;               

                var clonedQuoteDetails = Quote.QuoteDetails.Select(objEntity => (QuoteDetails)objEntity.Clone()).ToList();
                var clonedFreightDetails = Quote.FreightDetails.Select(objEntity => (FreightDetails)objEntity.Clone()).ToList();

                q.QuoteDetails = new ObservableCollection<QuoteDetails>(clonedQuoteDetails);
                q.FreightDetails = new BindingList<FreightDetails>(clonedFreightDetails);

                //TabSelectedIndex = 0;

                mainWindowViewModelRefernce = MainWindowViewModel.instance;
                if (mainWindowViewModelRefernce != null)
                {
                    List<ContactPerson> cpList = DBAccess.GetContactPersonByCustomerID(q.Customer.CustomerId);
                    if (cpList != null)
                    {
                        q.Customer.ContactPerson = cpList;
                    }
                    mainWindowViewModelRefernce.NewQuote(q);

                }              
            }
        }


        private bool CheckProjectNameAvailable()
        {
            bool isAvailable = true;

            isAvailable = DBAccess.CheckQuoteProjectNameAndQuoteNo(Quote.ProjectName, Quote.QuoteNo);

            return isAvailable;
        }


        //private void RemoveZeroQuoteDetails()
        //{
        //    var itemToRemove = Quote.QuoteDetails.Where(x => x.Product == null).ToList();
        //    foreach (var item in itemToRemove)
        //    {
        //        Quote.QuoteDetails.Remove(item);
        //    }
        //}

        //private void RemoveZeroFreightDetails()
        //{
        //    var itemToRemove = Quote.FreightDetails.Where(x => x.FreightCodeDetails == null).ToList();
        //    foreach (var item in itemToRemove)
        //    {
        //        Quote.FreightDetails.Remove(item);
        //    }
        //}

        private void ViewAsPDF(object para)
        {
            int index = QuoteList.IndexOf(para as Quote);
            if (index >= 0)
            {
                bool exist = DBAccess.CheckRecordExist(QuoteList[index].QuoteNo, "Quote");
                if (exist)
                {
                    Regex rgx = new Regex("[^a-zA-Z0-9 _]");
                    string filePath = string.Empty;
                    bool createFile = false;
                    bool canDirectoryAccessed = true;
                    Exception exception = null;
                    Quote.FileName = QuoteList[index].FileName;

                    BackgroundWorker worker = new BackgroundWorker();
                    ChildWindow LoadingScreen = new ChildWindow();
                    LoadingScreen.ShowWaitingScreen("Loading");

                    worker.DoWork += (_, __) =>
                    {
                        filePath = FilePathManager.GetQuoteSavingPath();

                        try
                        {
                            Directory.CreateDirectory(filePath);
                        }
                        catch (DirectoryNotFoundException dirEx)
                        {
                            canDirectoryAccessed = false;
                        }

                        if (canDirectoryAccessed)
                        {
                            if (!String.IsNullOrWhiteSpace(Quote.FileName))
                            {
                                //check if file exists
                                if (File.Exists(@"" + filePath + "/" + Quote.FileName))
                                {
                                    createFile = false;
                                }
                                else
                                {
                                    createFile = true;
                                }
                            }
                            else
                            {
                                //File name is not existing but file might be available
                                string dbCusName = QuoteList[index].Customer.CompanyName.Replace("/", "").Replace(@"\", "");
                                Quote.FileName = rgx.Replace(dbCusName.Replace(" ", "") + "_" + QuoteList[index].ProjectName.Replace("/", "_").Replace(@"\", "_") + "_" + QuoteList[index].QuoteNo, "");
                                Quote.FileName = Quote.FileName + ".pdf";
                                //Check if file exists
                                if (File.Exists(@"" + filePath + "/" + Quote.FileName))
                                {
                                    //If available delete file
                                    File.Delete(@"" + filePath + "/" + Quote.FileName);
                                }
                                //Create a new file
                                createFile = true;
                            }

                            if (createFile)
                            {
                            newFile:

                                //Get quote details
                                Quote = DBAccess.GetQuote(QuoteList[index].QuoteNo);
                                CalculateFinalTotalNoForm();

                                PrePaidCustomerName = Quote.Customer.CustomerId == 0 ? Quote.Customer.CompanyName : "";
                                string dbCusName = Quote.Customer.CompanyName.Replace("/", "").Replace(@"\", "");

                                Quote.FileName = rgx.Replace(dbCusName.Replace(" ", "") + "_" + Quote.ProjectName.Replace("/", "").Replace(@"\", "_") + "_" + Quote.QuoteNo, "");
                                Quote.FileName = Quote.FileName + ".pdf";
                                UpdateQuotePDF uqpdf = new UpdateQuotePDF(Quote);
                                exception = uqpdf.CreateQuote();
                                //Upate new file location to database;
                                if (exception == null)
                                {
                                    int res = DBAccess.UpdateQuoteFileLocation(Quote);

                                }
                                else
                                {
                                    File.Delete(@"" + filePath + "/" + Quote.FileName);
                                    goto newFile;
                                }
                            }
                        }
                    };
                    worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
                    {
                        LoadingScreen.CloseWaitingScreen();

                        if (canDirectoryAccessed)
                        {
                            if (exception != null)
                            {
                                MessageBox.Show("Cannot open this quote. Please try again later." + System.Environment.NewLine + exception, "Cannot Open Quote", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                            }
                            else
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var childWindow = new ChildWindow();
                                    childWindow.ShowFormula(filePath + "/" + Quote.FileName);
                                });
                            }
                        }
                        else
                        {
                            MessageBox.Show("System was unable to access path (" + filePath + ")" + System.Environment.NewLine + "Please check the file path or try enabling VPN", "Cannot Access Path", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                        }
                    };
                    worker.RunWorkerAsync();
                }
                else
                {
                    MessageBox.Show("This quote does not exist", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshGrid();
                }
            }
            else
            {
                MessageBox.Show("This quote does not exist", "", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshGrid();
            }
        }

        private void ViewCompletedOrdersAsPDF(object para)
        {
            int index = ConvertedQuotes.IndexOf(para as NewOrderPDFM);
            if (index >= 0)
            {
                bool exist = DBAccess.CheckRecordExist(ConvertedQuotes[index].ID, "NewOrderPDF");
                if (exist)
                {
                    Regex rgx = new Regex("[^a-zA-Z0-9 _]");
                    string filePath = string.Empty;
                    bool createFile = false;
                    bool canDirectoryAccessed = true;
                    Exception exception = null;
                    NewOrderPDFM temp = new NewOrderPDFM();
                    BackgroundWorker worker = new BackgroundWorker();
                    ChildWindow LoadingScreen = new ChildWindow();
                    LoadingScreen.ShowWaitingScreen("Loading");

                    worker.DoWork += (_, __) =>
                    {
                        filePath = FilePathManager.GetNewOrderSavingPath();

                        try
                        {
                            Directory.CreateDirectory(filePath);
                        }
                        catch (DirectoryNotFoundException dirEx)
                        {
                            canDirectoryAccessed = false;
                        }

                        if (canDirectoryAccessed)
                        {
                            if (!String.IsNullOrWhiteSpace(ConvertedQuotes[index].FileName))
                            {
                                //check if file exists
                                if (File.Exists(@"" + filePath + "/" + ConvertedQuotes[index].FileName))
                                {
                                    createFile = false;
                                    temp = ConvertedQuotes[index];
                                }
                                else
                                {
                                    createFile = true;
                                }
                            }
                            else
                            {
                                //File name is not existing but file might be available
                                string dbCusName = ConvertedQuotes[index].Customer.CompanyName.Replace("/", "").Replace(@"\", "");
                                ConvertedQuotes[index].FileName = rgx.Replace(dbCusName.Replace(" ", "") + "_" + ConvertedQuotes[index].ProjectName.Replace("/", "_").Replace(@"\", "_") + "_" + ConvertedQuotes[index].ID,"");
                                ConvertedQuotes[index].FileName = ConvertedQuotes[index].FileName + ".pdf";
                                //Check if file exists
                                if (File.Exists(@"" + filePath + "/" + ConvertedQuotes[index].FileName))
                                {
                                    //If available delete file
                                    File.Delete(@"" + filePath + "/" + ConvertedQuotes[index].FileName);
                                }
                                //Create a new file
                                createFile = true;
                            }

                            if (createFile)
                            {
                            newFile:                               
                                
                                temp = DBAccess.GetNewOrderPDF(ConvertedQuotes[index].ID);
                                if (temp != null)
                                {
                                    //Get quote details
                                    temp = CalculateFinalTotalNoFormOrders(temp);

                                    PrePaidCustomerName = temp.Customer.CustomerId == 0 ? temp.Customer.CompanyName : "";
                                    string dbCusName = temp.Customer.CompanyName.Replace("/", "").Replace(@"\", "");

                                    temp.FileName = rgx.Replace(dbCusName.Replace(" ", "") + "_" + temp.ProjectName.Replace("/", "").Replace(@"\", "_") + "_" + ConvertedQuotes[index].ID,"");
                                    temp.FileName = temp.FileName + ".pdf";

                                    NewSalesOrderPDF uqpdf = new NewSalesOrderPDF(ConvertedQuotes[index].ID, temp.FileName);

                                    exception = uqpdf.CreateQuote();
                                    //Upate new file location to database;
                                    if (exception == null)
                                    {
                                        int res = DBAccess.UpdateOrderFileLocation(temp);
                                    }
                                    else
                                    {
                                        File.Delete(@"" + filePath + "/" + temp.FileName);
                                        goto newFile;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Unable to find the order information", "", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                                }
                            }
                        }
                    };
                    worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    {
                        LoadingScreen.CloseWaitingScreen();

                        if (canDirectoryAccessed)
                        {
                            if (exception != null)
                            {
                                MessageBox.Show("Cannot open this order. Please try again later." + System.Environment.NewLine + exception, "Cannot Open Order", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                            }
                            else
                            {
                                if (temp != null && !string.IsNullOrWhiteSpace(temp.FileName))                                    
                                {
                                    string f = filePath + "/" + temp.FileName;

                                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        var childWindow = new ChildWindow();
                                        childWindow.ShowFormula(filePath + "/" + temp.FileName);
                                    });
                                }
                                else
                                {
                                    if(temp == null)
                                    {
                                        MessageBox.Show("System was unable to find the order", "", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                                    }
                                    if (string.IsNullOrWhiteSpace(temp.FileName))
                                    {
                                        MessageBox.Show("System was unable to find the file name", "", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("System was unable to access path (" + filePath + ")" + System.Environment.NewLine + "Please check the file path or try enabling VPN", "Cannot Access Path", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                        }
                    };
                    worker.RunWorkerAsync();
                }
                else
                {
                    MessageBox.Show("This order does not exist", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshGrid();
                }
            }
            else
            {
                MessageBox.Show("This order does not exist", "", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshGrid();
            }
        }

        public bool CanClose
        {
            get
            {
                return this.IsDirty == false || MessageBox.Show("Changes were made. Do you want to close this window?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        private void ClearQuoteListFields()
        {
            _start = 0;
            itemCount = 20;
            _totalItems = 0;

            ProjectNameSearch = string.Empty;

            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                LoadQuotes();
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
        }

        private void ClearOrderListFields()
        {
            _startCQ = 0;
            itemCountCQ = 20;
            _totalItemsCQ = 0;

            ProjectNameSearchOrder = string.Empty;
            AllChecked = true;
            ConvertedQuotesChecked = false;
            DirectOrdersChecked = false;
            searchOrderType = "All";
            //RefreshConvQuotesGrid();

            //BackgroundWorker worker = new BackgroundWorker();
            //ChildWindow LoadingScreen = new ChildWindow();
            //LoadingScreen.ShowWaitingScreen("Loading");

            //worker.DoWork += (_, __) =>
            //{
                LoadConvertedQuotes();
            //};
            //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            //{
            //    LoadingScreen.CloseWaitingScreen();
            //};
            //worker.RunWorkerAsync();
        }

        private void SearchQuoteList()
        {
            _start = 0;
            itemCount = 20;
            _totalItems = 0;

            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                LoadQuotes();
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
        }

       
        private void SearchOrderList()
        {
            _startCQ = 0;
            itemCountCQ = 20;
            _totalItemsCQ = 0;

            //BackgroundWorker worker = new BackgroundWorker();
            //ChildWindow LoadingScreen = new ChildWindow();
            //LoadingScreen.ShowWaitingScreen("Loading");

            //worker.DoWork += (_, __) =>
            //{
                LoadConvertedQuotes();
            //};
            //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            //{
            //    LoadingScreen.CloseWaitingScreen();
            //};
            //worker.RunWorkerAsync();
        }
        //public NewOrderPDFViewModel NewOrderPDFViewModel { get; set; }
        public int TabSelectedIndex
        {
            get { return _tabSelectedIndex; }

            set
            {
                _tabSelectedIndex = value;
                base.RaisePropertyChanged("TabSelectedIndex");

                //NewOrderPDFViewLoadingLocation.IsLoadFromMainMenu = false;

                if (TabSelectedIndex == 0)
                {
                    RefreshButtonVisibility = "Visible";
                    ProjectNameSearch = string.Empty;
                    LoadQuotes();
                    //QuoteButtonVisiblity = "Collapsed";                  
                }
                //else if (TabSelectedIndex == 1)
                //{
                //    OutDatedQuoteTextVisibility = "Collapsed";
                //    RefreshButtonVisibility = "Collapsed";
                //    ClearSearchFields();
                //    LoadQuotes();
                //    FetchQuoteAndProjectNames();
                //    FetchCustomerAndProjectNames();
                //}
                else if (TabSelectedIndex == 1)
                {                   
                    ProjectNameSearchOrder = string.Empty;
                    AllChecked = true;
                    searchOrderType = "All";
                    ConvertedQuotesChecked = false;
                    DirectOrdersChecked = false;
                    LoadConvertedQuotes();
                    _startCQ = 0;
                  
                }
                else if (TabSelectedIndex == 3)
                {
                    //QuoteButtonVisiblity = "Collapsed";  
                   
                }
            }
        }
        
         public string IsUpdateEnabled
        {
            get { return _isUpdateEnabled; }

            set
            {
                _isUpdateEnabled = value;
                base.RaisePropertyChanged("IsUpdateEnabled");
            }
        }

        public bool AllChecked
        {
            get { return _allChecked; }

            set
            {
                _allChecked = value;
                base.RaisePropertyChanged("AllChecked");                              
            }
        }

        public bool ConvertedQuotesChecked
        {
            get { return _convertedQuotesChecked; }

            set
            {
                _convertedQuotesChecked = value;
                base.RaisePropertyChanged("ConvertedQuotesChecked");                              
            }
        }

        public bool DirectOrdersChecked
        {
            get { return _directOrdersChecked; }

            set
            {
                _directOrdersChecked = value;
                base.RaisePropertyChanged("DirectOrdersChecked");                              
            }
        }

        public Customer SelectedCustomer
        {
            get { return _selectedCustomer; }

            set
            {
                _selectedCustomer = value;
                base.RaisePropertyChanged("SelectedCustomer");

                DiscountAppliedTextVisibility = "Collapsed";
                ContactPersonPhoneVisibility = "Collapsed";
            }
        }


        public ObservableCollection<Quote> QuoteList
        {
            get { return _quoteList; }

            set
            {
                _quoteList = value;
                base.RaisePropertyChanged("QuoteList");
            }
        }

        public List<string> QuoteNoList
        {
            get { return _quoteNoList; }

            set
            {
                _quoteNoList = value;
                base.RaisePropertyChanged("QuoteNoList");
            }
        }

        public List<string> ProjectNames
        {
            get { return _projectNames; }

            set
            {
                _projectNames = value;
                base.RaisePropertyChanged("ProjectNames");
            }
        }

        public string SelectedProjectName
        {
            get { return _selectedProjectName; }

            set
            {
                _selectedProjectName = value;
                base.RaisePropertyChanged("SelectedProjectName");
            }
        }

        public string SelectedQuoteNo
        {
            get { return _selectedQuoteNo; }

            set
            {
                _selectedQuoteNo = value;
                base.RaisePropertyChanged("SelectedQuoteNo");
            }
        }

        public List<string> CusProjectNames
        {
            get { return _cusProjectNames; }

            set
            {
                _cusProjectNames = value;
                base.RaisePropertyChanged("CusProjectNames");
            }
        }

        public string SelectedSearchedCustomerName
        {
            get { return _selectedSearchedCustomerName; }

            set
            {
                _selectedSearchedCustomerName = value;
                base.RaisePropertyChanged("SelectedSearchedCustomerName");
            }
        }

        public Quote Quote
        {
            get
            {
                if (_quote != null)
                {
                    if (_quote.ContactPerson != null)
                    {

                        if (_quote.ContactPerson.ContactPersonID == -1)
                        {
                            OtherContactPersonVisibility = "Visible";
                            ContactPersonPhoneVisibility = "Collapsed";
                        }
                        else
                        {
                            OtherContactPersonVisibility = "Collapsed";
                            ContactPersonPhoneVisibility = "Visible";
                        }
                    }
                    else
                    {
                        OtherContactPersonVisibility = "Collapsed";
                        ContactPersonPhoneVisibility = "Collapsed";
                    }
                }
                return _quote;
            }
            set
            {
                _quote = value;
                base.RaisePropertyChanged("Quote");
            }
        }

        public List<DiscountStructure> DisplayDiscountStructure
        {
            get
            {
                return _displayDiscountStructure;
            }
            set
            {
                _displayDiscountStructure = value;
                RaisePropertyChanged("DisplayDiscountStructure");
            }
        }

        public List<DiscountStructure> DiscountStructure
        {
            get
            {
                return _discountStructure;
            }
            set
            {
                _discountStructure = value;
                RaisePropertyChanged("DiscountStructure");
            }
        }


        public int ItemCount
        {
            get { return _itemCount; }

            set
            {
                _itemCount = value;
                base.RaisePropertyChanged("ItemCount");
            }
        }

        public bool AddShippingAddress
        {
            get { return _addShippingAddress; }
            set
            {
                _addShippingAddress = value;
                RaisePropertyChanged("AddShippingAddress");
            }
        }

        public bool ProjectNameEnabled
        {
            get { return _projectNameEnabled; }
            set
            {
                _projectNameEnabled = value;
                RaisePropertyChanged("ProjectNameEnabled");
            }
        }


        public bool SoldToEnabled
        {
            get { return _soldToEnabled; }
            set
            {
                _soldToEnabled = value;
                RaisePropertyChanged("SoldToEnabled");
            }
        }

        public bool ShipToEnabled
        {
            get { return _shipToEnabled; }
            set
            {
                _shipToEnabled = value;
                RaisePropertyChanged("ShipToEnabled");
            }
        }

        public bool QuoteDetailsEnabled
        {
            get { return _quoteDetailsEnabled; }
            set
            {
                _quoteDetailsEnabled = value;
                RaisePropertyChanged("QuoteDetailsEnabled");
            }
        }

        public bool FreightDetailsEnabled
        {
            get { return _freightDetailsEnabled; }
            set
            {
                _freightDetailsEnabled = value;
                RaisePropertyChanged("FreightDetailsEnabled");
            }
        }

        public bool InstructionsEnabled
        {
            get { return _instructionsEnabled; }
            set
            {
                _instructionsEnabled = value;
                RaisePropertyChanged("InstructionsEnabled");
            }
        }

        public bool InternalCommentsEnabled
        {
            get { return _internalCommentsEnabled; }
            set
            {
                _internalCommentsEnabled = value;
                RaisePropertyChanged("InternalCommentsEnabled");
            }
        }

        public string DiscountAppliedTextVisibility
        {
            get { return _discountAppliedTextVisibility; }
            set
            {
                _discountAppliedTextVisibility = value;
                RaisePropertyChanged("DiscountAppliedTextVisibility");
            }
        }

        public string OutDatedQuoteTextVisibility
        {
            get { return _outDatedQuoteTextVisibility; }
            set
            {
                _outDatedQuoteTextVisibility = value;
                RaisePropertyChanged("OutDatedQuoteTextVisibility");
            }
        }



        //public ObservableCollection<FreightCode> FreightCodeDetails
        //{
        //    get
        //    {
        //        return _freightCodeDetails;
        //    }
        //    set
        //    {
        //        _freightCodeDetails = value;
        //        RaisePropertyChanged("FreightCodeDetails");
        //    }
        //}

        public ObservableCollection<FreightCode> OrgFreightCodeDetails
        {
            get
            {
                return _orgFreightCodeDetails;
            }
            set
            {
                _orgFreightCodeDetails = value;
                RaisePropertyChanged("OrgFreightCodeDetails");
            }
        }

        public bool ShippingEnabled
        {
            get
            {
                return _shippingEnabled;
            }
            set
            {
                _shippingEnabled = value;
                RaisePropertyChanged("ShippingEnabled");
            }
        }

        public bool AddShippingAddressEnabled
        {
            get
            {
                return _addShippingAddressEnabled;
            }
            set
            {
                _addShippingAddressEnabled = value;
                RaisePropertyChanged("AddShippingAddressEnabled");
            }
        }

        public string QuoteDateStr
        {
            get
            {
                return _quoteDateStr;
            }
            set
            {
                _quoteDateStr = value;
                RaisePropertyChanged("QuoteDateStr");
            }
        }

        public string RefreshButtonVisibility
        {
            get
            {
                return _refreshButtonVisibility;
            }
            set
            {
                _refreshButtonVisibility = value;
                RaisePropertyChanged("RefreshButtonVisibility");
            }
        }


        public string OtherContactName
        {
            get
            {
                return _otherContactName;
            }
            set
            {
                _otherContactName = value;
                RaisePropertyChanged("OtherContactName");
            }
        }

        public string OtherContactNamePhone
        {
            get
            {
                return _otherContactNamePhone;
            }
            set
            {
                _otherContactNamePhone = value;
                RaisePropertyChanged("OtherContactNamePhone");
            }
        }

        public string OtherContactPersonVisibility
        {
            get
            {
                return _otherContactPersonVisibility;
            }
            set
            {
                _otherContactPersonVisibility = value;
                RaisePropertyChanged("OtherContactPersonVisibility");
            }
        }

        public string ContactPersonPhoneVisibility
        {
            get
            {
                return _contactPersonPhoneVisibility;
            }
            set
            {
                _contactPersonPhoneVisibility = value;
                RaisePropertyChanged("ContactPersonPhoneVisibility");
            }
        }

        //public string SearchString
        //{
        //    get { return _searchString; }
        //    set
        //    {
        //        _searchString = value;
        //        RaisePropertyChanged("SearchString");


        //        ItemsView.Refresh();
        //    }
        //}

        //public bool ViewPDFEnabled
        //{
        //    get { return _viewPDFEnabled; }
        //    set
        //    {
        //        _viewPDFEnabled = value;
        //        RaisePropertyChanged("ViewPDFEnabled");
        //    }
        //}

        //public string ViewPDFBackGroundCol
        //{
        //    get { return _viewPDFBackGroundCol; }
        //    set
        //    {
        //        _viewPDFBackGroundCol = value;
        //        RaisePropertyChanged("ViewPDFBackGroundCol");
        //    }
        //}

        public string ShippingAddressVisibility
        {
            get { return _shippingAddressVisibility; }
            set
            {
                _shippingAddressVisibility = value;
                RaisePropertyChanged("ShippingAddressVisibility");
            }
        }

        public string ShipToText
        {
            get { return _shipToText; }
            set
            {
                _shipToText = value;
                RaisePropertyChanged("ShipToText");
            }
        }

        public FreightDetails SelectedFreightDetails
        {
            get { return _selectedFreightDetails; }
            set
            {
                _selectedFreightDetails = value;
                RaisePropertyChanged("SelectedFreightDetails");
            }
        }

        public string PrePaidCustomerName
        {
            get { return _prePaidCustomerName; }
            set
            {
                _prePaidCustomerName = value;
                RaisePropertyChanged("PrePaidCustomerName");
            }
        }

        
        public string ProjectNameSearch
        {
            get { return _projectNameSearch; }
            set
            {
                _projectNameSearch = value;
                RaisePropertyChanged("ProjectNameSearch");               
            }
        }

        public string ProjectNameSearchOrder
        {
            get { return _projectNameSearchOrder; }
            set
            {
                _projectNameSearchOrder = value;
                RaisePropertyChanged("ProjectNameSearchOrder");
                //if(!string.IsNullOrWhiteSpace(ProjectNameSearchOrder))
                //{
                //    if(AllChecked==false)
                //    {
                //        AllChecked = true;
                //    }
                //}
            }
        }

        //public string QuoteButtonVisiblity
        //{
        //    get { return _quoteButtonVisiblity; }
        //    set
        //    {
        //        _quoteButtonVisiblity = value;
        //        RaisePropertyChanged("QuoteButtonVisiblity");               
        //    }
        //}

        public ObservableCollection<NewOrderPDFM> ConvertedQuotes
        {
            get { return _convertedQuotes; }
            set
            {
                _convertedQuotes = value;
                RaisePropertyChanged("ConvertedQuotes");
            }
        }
                

        /// <summary>
        /// Gets the index of the first item in the products list.
        /// </summary>
        public int Start { get { return _start + 1; } }
        public int StartCQ { get { return _startCQ + 1; } }

        /// <summary>
        /// Gets the index of the last item in the products list.
        /// </summary>
        public int End { get { return _start + itemCount < _totalItems ? _start + itemCount : _totalItems; } }
        public int EndCQ { get { return _startCQ + itemCountCQ < _totalItemsCQ ? _startCQ + itemCountCQ : _totalItemsCQ; } }

        /// <summary>
        /// The number of total items in the data store.
        /// </summary>
        public int TotalItems { get { return _totalItems; } }
        public int TotalItemsCQ { get { return _totalItemsCQ; } }

        /// <summary>
        /// Gets the command for moving to the first page of products.
        /// </summary>
        

        //public ICollectionView ItemsView
        //{
        //    get { return _itemsView; }
        //}

        #region COMMANDS

        public ICommand ViewUpdateCommand
        {
            get
            {
                if (_viewUpdateCommand == null)
                {
                    _viewUpdateCommand = new DelegateCommand(CanExecute, ViewUpdateQuote);
                }
                return _viewUpdateCommand;
            }
        }

        public ICommand ViewPDFCommand
        {
            get
            {
                if (_viewPDFCommand == null)
                {
                    _viewPDFCommand = new DelegateCommand(CanExecute, ViewAsPDF);
                }
                return _viewPDFCommand;
            }
        }

        public ICommand ViewCompletedPDFCommand
        {
            get
            {
                if (_viewCompletedPDFCommand == null)
                {
                    _viewCompletedPDFCommand = new DelegateCommand(CanExecute, ViewCompletedOrdersAsPDF);
                }
                return _viewCompletedPDFCommand;
            }
        }

        public ICommand ViewUpdateOrderCommand
        {
            get
            {
                if (_viewUpdateOrderCommand == null)
                {
                    _viewUpdateOrderCommand = new DelegateCommand(CanExecute, ViewUpdateOrder);
                }
                return _viewUpdateOrderCommand;
            }
        }

        

        public ICommand QuoteToOrderCommand
        {
            get
            {
                if (_quoteToOrderCommand == null)
                {
                    _quoteToOrderCommand = new DelegateCommand(CanExecute, QuoteToOrder);
                }
                return _quoteToOrderCommand;
            }
        }

        

        public ICommand DeleteQuoteCommand
        {
            get
            {
                if (_deleteQuoteCommand == null)
                {
                    _deleteQuoteCommand = new DelegateCommand(CanExecute, DeleteQuote);
                }
                return _deleteQuoteCommand;
            }
        }

        public ICommand SendToSalesCommand
        {
            get
            {
                if (_sendToSalesCommand == null)
                {
                    _sendToSalesCommand = new DelegateCommand(CanExecute, ConvertQuoteToSales);
                }
                return _sendToSalesCommand;
            }
        }
        

        //public ICommand SearchCommand
        //{
        //    get
        //    {
        //        return _searchCommand ?? (_searchCommand = new CommandHandler(() => SearchQuote(10), true));
        //    }
        //}

        //public ICommand SearchProductCommand
        //{
        //    get
        //    {
        //        if (_searchProductCommand == null)
        //        {
        //            _searchProductCommand = new DelegateCommand(CanExecute, SearchProduct);
        //        }
        //        return _searchProductCommand;
        //    }
        //}

        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new DelegateCommand(CanExecute, RemoveProduct);
                }
                return _removeCommand;
            }
        }

        //public ICommand SelectionChangedCommand
        //{
        //    get
        //    {
        //        return _selectionChangedCommand ?? (_selectionChangedCommand = new CommandHandler(() => CalculateQtyToMake(), true));
        //    }
        //}

        public ICommand RemoveFreightCodeCommand
        {
            get
            {
                if (_removeFreightCodeCommand == null)
                {
                    _removeFreightCodeCommand = new DelegateCommand(CanExecute, DeleteFreightCode);
                }
                return _removeFreightCodeCommand;
            }
        }

        public ICommand FreightPriceKeyUpCommand
        {
            get
            {
                return _freightPriceKeyUpCommand ?? (_freightPriceKeyUpCommand = new CommandHandler(() => CalculateFreightTotal(), true));
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand ?? (_clearCommand = new CommandHandler(() => ClearSearchFields(), true));
            }
        }

        //public ICommand UpdateCommand
        //{
        //    get
        //    {
        //        return _updateCommand ?? (_updateCommand = new CommandHandler(() => UpdateQuote(), true));
        //    }
        //}


        public ICommand LostFocusCommand
        {
            get
            {
                return _lostFocusCommand ?? (_lostFocusCommand = new CommandHandler(() => CalculateQtyToMake(), true));
            }
        }

        //public ICommand FreightLostFocusCommand
        //{
        //    get
        //    {
        //        return _freightLostFocusCommand ?? (_freightLostFocusCommand = new CommandHandler(() => ReAddOriginalFreightPrice(), true));
        //    }
        //}

        public ICommand DiscountLostFocusCommand
        {
            get
            {
                return _discountLostFocusCommand ?? (_discountLostFocusCommand = new CommandHandler(() => CalculateFinalTotal(), true));
            }
        }

        public ICommand RefreshGridCommand
        {
            get
            {
                return _refreshGridCommand ?? (_refreshGridCommand = new CommandHandler(() => RefreshGrid(), true));
            }
        }
        public ICommand RefreshConvQuotesGridCommand
        {
            get
            {
                return _refreshConvQuotesGridCommand ?? (_refreshConvQuotesGridCommand = new CommandHandler(() => RefreshConvQuotesGrid(), true));
            }
        }

        //public ICommand ViewPDFCommand
        //{
        //    get
        //    {
        //        return _viewPDFCommand ?? (_viewPDFCommand = new CommandHandler(() => ViewAsPDF(), true));
        //    }
        //}

        public ICommand SearchQuoteListCommand
        {
            get
            {
                return _searchQuoteListCommand ?? (_searchQuoteListCommand = new CommandHandler(() => SearchQuoteList(), true));
            }
        }

        public ICommand ClearOrderListCommand
        {
            get
            {
                return _clearOrderListCommand ?? (_clearOrderListCommand = new CommandHandler(() => ClearOrderListFields(), true));
            }
        }
        
        
        public ICommand SearchOrderListCommand
        {
            get
            {
                return _searchOrderListCommand ?? (_searchOrderListCommand = new CommandHandler(() => SearchOrderList(), true));
            }
        }

        public ICommand ClearQuoteListCommand
        {
            get
            {
                return _clearQuoteListCommand ?? (_clearQuoteListCommand = new CommandHandler(() => ClearQuoteListFields(), true));
            }
        }

        /// <summary>
        /// Gets the command for moving to the first page of products.
        /// </summary>
        public ICommand FirstCommand
        {
            get
            {
                if (_firstCommand == null)
                {
                    _firstCommand = new RelayCommand
                    (
                        param =>
                        {
                            _start = 0;
                            BackgroundWorker worker = new BackgroundWorker();
                            ChildWindow LoadingScreen = new ChildWindow();
                            LoadingScreen.ShowWaitingScreen("Loading");

                            worker.DoWork += (_, __) =>
                            {
                                LoadQuotes();
                            };
                            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                            {
                                LoadingScreen.CloseWaitingScreen();
                            };
                            worker.RunWorkerAsync();
                        },
                        param =>
                        {
                            return _start - itemCount >= 0 ? true : false;
                        }
                    );
                }

                return _firstCommand;
            }
        }

        public ICommand FirstCQCommand
        {
            get
            {
                if (_firstCQCommand == null)
                {
                    _firstCQCommand = new RelayCommand
                    (
                        param =>
                        {
                            _startCQ = 0;
                            BackgroundWorker worker = new BackgroundWorker();
                            ChildWindow LoadingScreen = new ChildWindow();
                            LoadingScreen.ShowWaitingScreen("Loading");

                            worker.DoWork += (_, __) =>
                            {
                                LoadConvertedQuotes();
                            };
                            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                            {
                                LoadingScreen.CloseWaitingScreen();
                            };
                            worker.RunWorkerAsync();
                        },
                        param =>
                        {
                            return _startCQ - itemCountCQ >= 0 ? true : false;
                        }
                    );
                }

                return _firstCQCommand;
            }
        }

        /// <summary>
        /// Gets the command for moving to the previous page of products.
        /// </summary>
        public ICommand PreviousCommand
        {
            get
            {
                if (_previousCommand == null)
                {
                    _previousCommand = new RelayCommand
                    (
                        param =>
                        {
                            _start -= itemCount;
                            BackgroundWorker worker = new BackgroundWorker();
                            ChildWindow LoadingScreen = new ChildWindow();
                            LoadingScreen.ShowWaitingScreen("Loading");

                            worker.DoWork += (_, __) =>
                            {
                                LoadQuotes();
                            };
                            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                            {
                                LoadingScreen.CloseWaitingScreen();
                            };
                            worker.RunWorkerAsync();
                        },
                        param =>
                        {
                            return _start - itemCount >= 0 ? true : false;
                        }
                    );
                }

                return _previousCommand;
            }
        }

        public ICommand PreviousCQCommand
        {
            get
            {
                if (_previousCQCommand == null)
                {
                    _previousCQCommand = new RelayCommand
                    (
                        param =>
                        {
                            _startCQ -= itemCountCQ;
                            BackgroundWorker worker = new BackgroundWorker();
                            ChildWindow LoadingScreen = new ChildWindow();
                            LoadingScreen.ShowWaitingScreen("Loading");

                            worker.DoWork += (_, __) =>
                            {
                                LoadConvertedQuotes();
                            };
                            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                            {
                                LoadingScreen.CloseWaitingScreen();
                            };
                            worker.RunWorkerAsync();
                        },
                        param =>
                        {
                            return _startCQ - itemCountCQ >= 0 ? true : false;
                        }
                    );
                }

                return _previousCQCommand;
            }
        }

        /// <summary>
        /// Gets the command for moving to the next page of products.
        /// </summary>
        public ICommand NextCommand
        {
            get
            {
                if (_nextCommand == null)
                {
                    _nextCommand = new RelayCommand
                    (
                        param =>
                        {
                            _start += itemCount;
                            BackgroundWorker worker = new BackgroundWorker();
                            ChildWindow LoadingScreen = new ChildWindow();
                            LoadingScreen.ShowWaitingScreen("Loading");

                            worker.DoWork += (_, __) =>
                            {
                                LoadQuotes();
                            };
                            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                            {
                                LoadingScreen.CloseWaitingScreen();
                            };
                            worker.RunWorkerAsync();
                        },
                        param =>
                        {
                            return _start + itemCount < _totalItems ? true : false;
                        }
                    );
                }

                return _nextCommand;
            }
        }

        public ICommand NextCommandCQ
        {
            get
            {
                if (_nextCommandCQ == null)
                {
                    _nextCommandCQ = new RelayCommand
                    (
                        param =>
                        {
                            _startCQ += itemCountCQ;
                            BackgroundWorker worker = new BackgroundWorker();
                            ChildWindow LoadingScreen = new ChildWindow();
                            LoadingScreen.ShowWaitingScreen("Loading");

                            worker.DoWork += (_, __) =>
                            {
                                LoadConvertedQuotes();
                            };
                            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                            {
                                LoadingScreen.CloseWaitingScreen();
                            };
                            worker.RunWorkerAsync();
                        },
                        param =>
                        {
                            return _startCQ + itemCountCQ < _totalItemsCQ ? true : false;
                        }
                    );
                }

                return _nextCommandCQ;
            }
        }

        /// <summary>
        /// Gets the command for moving to the last page of products.
        /// </summary>
        public ICommand LastCommand
        {
            get
            {
                if (_lastCommand == null)
                {
                    _lastCommand = new RelayCommand
                    (
                        param =>
                        {
                            _start = (_totalItems / itemCount - 1) * itemCount;
                            _start += _totalItems % itemCount == 0 ? 0 : itemCount;
                            BackgroundWorker worker = new BackgroundWorker();
                            ChildWindow LoadingScreen = new ChildWindow();
                            LoadingScreen.ShowWaitingScreen("Loading");

                            worker.DoWork += (_, __) =>
                            {
                                LoadQuotes();
                            };
                            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                            {
                                LoadingScreen.CloseWaitingScreen();
                            };
                            worker.RunWorkerAsync();
                        },
                        param =>
                        {
                            return _start + itemCount < _totalItems ? true : false;
                        }
                    );
                }

                return _lastCommand;
            }
        }

        public ICommand LastCommandCQ
        {
            get
            {
                if (_lastCommandCQ == null)
                {
                    _lastCommandCQ = new RelayCommand
                    (
                        param =>
                        {
                            _startCQ = (_totalItemsCQ / itemCountCQ - 1) * itemCountCQ;
                            _startCQ += _totalItemsCQ % itemCountCQ == 0 ? 0 : itemCountCQ;
                            BackgroundWorker worker = new BackgroundWorker();
                            ChildWindow LoadingScreen = new ChildWindow();
                            LoadingScreen.ShowWaitingScreen("Loading");

                            worker.DoWork += (_, __) =>
                            {
                                LoadConvertedQuotes();
                            };
                            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                            {
                                LoadingScreen.CloseWaitingScreen();
                            };
                            worker.RunWorkerAsync();
                        },
                        param =>
                        {
                            return _startCQ + itemCountCQ < _totalItemsCQ ? true : false;
                        }
                    );
                }

                return _lastCommandCQ;
            }
        }


        public RelayCommand SearchOrderTypeCommand
        {
            get
            {
                return new RelayCommand(parameter =>
                {
                    string str = (string)parameter;
                    searchOrderType = str;
                    //ProjectNameSearchOrder = string.Empty;
                    LoadConvertedQuotes();
                });
            }
        }
               
        #endregion
    }
}
