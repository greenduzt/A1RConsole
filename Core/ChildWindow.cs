using A1RConsole.DB;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Dispatch;
using A1RConsole.Models.Invoices;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Orders.OnlineOrders;
using A1RConsole.Models.Production.Peeling;
using A1RConsole.Models.Production.Slitting;
using A1RConsole.Models.Products;
using A1RConsole.Models.Purchasing;
using A1RConsole.Models.Quoting;
using A1RConsole.Models.RawMaterials;
using A1RConsole.Models.Stock;
using A1RConsole.Models.Suppliers;
using A1RConsole.Models.Users;
using A1RConsole.ViewModels;
using A1RConsole.ViewModels.Customers;
using A1RConsole.ViewModels.Discounts;
using A1RConsole.ViewModels.Formulas;
using A1RConsole.ViewModels.Invoicing;
using A1RConsole.ViewModels.Loading;
using A1RConsole.ViewModels.Orders.NewOrderPDF;
using A1RConsole.ViewModels.Orders.SalesOrders;
using A1RConsole.ViewModels.Productions.Slitting;
using A1RConsole.ViewModels.Products;
using A1RConsole.ViewModels.Purchasing;
using A1RConsole.ViewModels.Shipping;
using A1RConsole.ViewModels.Stock;
using A1RConsole.Views.Customers;
using A1RConsole.Views.Discounts;
using A1RConsole.Views.Formulas;
using A1RConsole.Views.Invoicing;
using A1RConsole.Views.Loading;
using A1RConsole.Views.Orders.NewOrderPDF;
using A1RConsole.Views.Orders.SalesOrders;
using A1RConsole.Views.Products;
using A1RConsole.Views.Purchasing;
using A1RConsole.Views.Shipping;
using A1RConsole.Views.Stock;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class ChildWindow : BaseViewModel
    {
        public event Action openWaitingScreen_Closed;
        public event Action<Product> productCodeSearch_Closed;
        public event Action<int> updateSalesOrder_Closed;
        public event Action<int> newSalesOrder_Closed;
        public event Action<ObservableCollection<DiscountStructure>> showAddUpdateDiscountView_Closed;
        public event Action slitPeelConfirmation_Closed;
        public event Action peelingConfirmation_Closed;
        public event Action shiftOrder_Closed;
        public event Action conOrder_Closed;
        public event Action formulaScreen_Closed;
        public event Action changeInvoiceDate_Closed;
        public event Action<ProductStock> updateProductInventory_Closed;
        public event Action showUpdateSupplierView_Closed;
        public event Action showAddSupplierView_Closed;
        public event Action<ReasonCode> reasonCodesView_Closed;
        public event Action<PurchaseOrder> purchaseOrderView_Closed;
        public event Action<int> openConsolidatePurchasingOrderView_Closed;
        public event Action<int> dispatchOrderView_Closed;
        public event Action dispatchConfirmation_Closed;
        public event Action<Customer> customerCreditForm_Closed;
        public event Action<int> invoicingView_Closed;
        public event Action<int> showAddCustomerView_Closed;
        //public event Action<int> showNewOrderPDFView_Closed;
        public event Action<ContactPerson> addEditContactPerson_Closed;

        ///*****************Showing Loading Screen******************/
        public void ShowWaitingScreen(string msg)
        {
            LoadingScreenViewModel lsvm = new LoadingScreenViewModel(msg);
            lsvm.Closed += CloseWaitingScreen;
            ChildWindowManager.Instance.ShowChildWindow(new LoadingScreen(msg) { DataContext = lsvm });
        }

        public void CloseWaitingScreen()
        {
            if (openWaitingScreen_Closed != null)
                openWaitingScreen_Closed();
            ChildWindowManager.Instance.CloseChildWindow();
        }

        /*****************Product Search Code******************/
        public void ShowProductSearch(string un, string s, List<UserPrivilages> p, List<MetaData> md,  SalesOrder so, List<DiscountStructure> dsList, string openType)
        {
            SearchProductViewModel dovm = new SearchProductViewModel(un, s, p, md, so, dsList, openType);
            dovm.Closed += showProductSearch_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new SearchProductView(un, s, p, md, so, dsList, openType) { DataContext = dovm });
        }

        void showProductSearch_Closed(Product product)
        {
            if (productCodeSearch_Closed != null)
                productCodeSearch_Closed(product);
            ChildWindowManager.Instance.CloseChildWindow();
        }

        ///************************Update SalesOrder ***********************************/
        public void ShowUpdateSalesOrderView(string un, string state, List<UserPrivilages> p, List<MetaData> md, SalesOrder so, List<DiscountStructure> dsList, string s, ObservableCollection<Customer> CustomerList)
        {
            UpdateSalesOrderViewModel uvvm = new UpdateSalesOrderViewModel(un, state, p, md, so, dsList, s, CustomerList);
            uvvm.Closed += UpdateSalesOrderView_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new UpdateSalesOrderView(un, state, p, md, so, dsList, s, CustomerList) { DataContext = uvvm });
        }
        void UpdateSalesOrderView_Closed(int res)
        {
            if (updateSalesOrder_Closed != null)
                updateSalesOrder_Closed(res);
            ChildWindowManager.Instance.CloseChildWindow();
        }


        ///************************Discount View ***********************************/
        public void ShowAddUpdateDiscountView(int cusId, ObservableCollection<DiscountStructure> dsL)
        {
            DiscountViewModel uvvm = new DiscountViewModel(cusId, dsL);
            uvvm.Closed += ShowAddUpdateDiscountView_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new DiscountView(cusId, dsL) { DataContext = uvvm });
        }
        void ShowAddUpdateDiscountView_Closed(ObservableCollection<DiscountStructure> r)
        {
            if (showAddUpdateDiscountView_Closed != null)
                showAddUpdateDiscountView_Closed(r);
            ChildWindowManager.Instance.CloseChildWindow();
        }


        /*****************Slitting Confirmation******************/
        public void ShowSlittingConfirmationView(SlittingOrder slittingProductionDetails)
        {
            //SlittingConfirmationViewModel spcvm = new SlittingConfirmationViewModel(slittingProductionDetails);
            //spcvm.Closed += slitPeelConfirmationVM_Closed;
            //ChildWindowManager.Instance.ShowChildWindow(new SlittingConfirmationView(slittingProductionDetails) { DataContext = spcvm });
        }

        void slitPeelConfirmationVM_Closed()
        {
            if (slitPeelConfirmation_Closed != null)
                slitPeelConfirmation_Closed();
            ChildWindowManager.Instance.CloseChildWindow();
        }

        /*****************Peeling Confirmation******************/
        public void ShowPeelingConfirmationView(PeelingOrder peelingOrder)
        {
            //PeelingConfirmationViewModel pcvm = new PeelingConfirmationViewModel(peelingOrder);
            //pcvm.Closed += PeelConfirmationVM_Closed;
            //ChildWindowManager.Instance.ShowChildWindow(new PeelingConfirmationView(peelingOrder) { DataContext = pcvm });
        }

        void PeelConfirmationVM_Closed()
        {
            if (peelingConfirmation_Closed != null)
                peelingConfirmation_Closed();
            ChildWindowManager.Instance.CloseChildWindow();
        }


        /*****************Shifting Orders******************/

        public void ShowShiftOrderWindow(RawProductionDetails rawProductionDetails, string type)
        {
            //ShiftProductionViewModel shiftOrderVM = new ShiftProductionViewModel(rawProductionDetails, type);
            //shiftOrderVM.Closed += shiftOrderVM_Closed;
            //ChildWindowManager.Instance.ShowChildWindow(new ShiftProductionView(rawProductionDetails, type) { DataContext = shiftOrderVM });
        }

        void shiftOrderVM_Closed()
        {
            if (shiftOrder_Closed != null)
                shiftOrder_Closed();
            ChildWindowManager.Instance.CloseChildWindow();
        }


        /*****************Converting Order To Graded******************/
        public void ShowConvertOrderWindow(RawProductionDetails rawProductionDetails)
        {
            //ConvertOrderViewModel convertOrderVM = new ConvertOrderViewModel(rawProductionDetails);
            //convertOrderVM.Closed += convertOrderVM_Closed;
            //ChildWindowManager.Instance.ShowChildWindow(new ConvertOrderView(rawProductionDetails) { DataContext = convertOrderVM });
        }

        void convertOrderVM_Closed()
        {
            if (conOrder_Closed != null)
                conOrder_Closed();
            ChildWindowManager.Instance.CloseChildWindow();
        }

        ///*****************Show Formula******************/
        public void ShowFormula(string formula)
        {
            ShowFormulaViewModel sfvm = new ShowFormulaViewModel(formula);
            sfvm.Closed += CloseFormulaScreen;
            ChildWindowManager.Instance.ShowChildWindow(new ShowFormulaView(formula) { DataContext = sfvm });
        }

        public void CloseFormulaScreen()
        {
            if (formulaScreen_Closed != null)
                formulaScreen_Closed();
            ChildWindowManager.Instance.CloseChildWindow();
        }

        /*****************Change Invoice Date******************/
        public void ShowChangeInvoiceDate(SalesOrder so)
        {
            ChangeInvoiceDateViewModel pcvm = new ChangeInvoiceDateViewModel(so);
            pcvm.Closed += ShowChangeInvoiceDate_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new ChangeInvoiceDateView(so) { DataContext = pcvm });
        }

        void ShowChangeInvoiceDate_Closed()
        {
            if (changeInvoiceDate_Closed != null)
                changeInvoiceDate_Closed();
            ChildWindowManager.Instance.CloseChildWindow();
        }


        ///************************Update Product Inventory ***********************************/
        public void ShowUpdateProductInventory(ProductStockMaintenance smvm, string userName, bool b, Supplier s)
        {
            UpdateProductViewModel uvvm = new UpdateProductViewModel(smvm, userName, b, s);
            uvvm.Closed += UpdateProductInventory_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new UpdateProductView(smvm, userName, b, s) { DataContext = uvvm });
        }
        void UpdateProductInventory_Closed(ProductStock res)
        {
            if (updateProductInventory_Closed != null)
                updateProductInventory_Closed(res);
            ChildWindowManager.Instance.CloseChildWindow();
        }

        ///************************Update Supplier ***********************************/
        public void ShowUpdateSupplierView(ProductStockMaintenance psm, string userName, Supplier sp)
        {
            UpdateSupplierViewModel uvvm = new UpdateSupplierViewModel(psm, userName, sp);
            uvvm.Closed += UpdateSupplier_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new UpdateSupplierView(psm, userName, sp) { DataContext = uvvm });
        }
        void UpdateSupplier_Closed()
        {
            if (showUpdateSupplierView_Closed != null)
                showUpdateSupplierView_Closed();
            ChildWindowManager.Instance.CloseChildWindow();
        }

        ///************************Add Supplier ***********************************/
        public void ShowAddSupplierView(ProductStockMaintenance psm, string userName)
        {
            AddSupplierViewModel uvvm = new AddSupplierViewModel(psm, userName);
            uvvm.Closed += AddSupplier_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new AddSupplierView(psm, userName) { DataContext = uvvm });
        }
        void AddSupplier_Closed()
        {
            if (showAddSupplierView_Closed != null)
                showAddSupplierView_Closed();
            ChildWindowManager.Instance.CloseChildWindow();
        }


        ///************************Reason Code ***********************************/
        public void ShowReasonCodes(StockAdjustmentViewModel savm)
        {
            ReasonCodesViewModel uvvm = new ReasonCodesViewModel(savm);
            uvvm.Closed += ShowReasonCodes_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new ReasonCodesView(savm) { DataContext = uvvm });
        }
        void ShowReasonCodes_Closed(ReasonCode s)
        {
            if (reasonCodesView_Closed != null)
                reasonCodesView_Closed(s);
            ChildWindowManager.Instance.CloseChildWindow();
        }

        ///************************Add Purchase Order ***********************************/
        public void ShowPurchaseOrderView(Product p)
        {
            AddPurchasingOrderViewModel uvvm = new AddPurchasingOrderViewModel(p);
            uvvm.Closed += PurchaseOrder_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new AddPurchasingOrderView(p) { DataContext = uvvm });
        }
        void PurchaseOrder_Closed(PurchaseOrder po)
        {
            if (purchaseOrderView_Closed != null)
                purchaseOrderView_Closed(po);
            ChildWindowManager.Instance.CloseChildWindow();
        }



        /***************ConsolidatePurchasingOrderView***********/
        public void ShowConsolidatePurchasingOrderView(ObservableCollection<PurchaseOrder> exPurchasingOrders, PurchaseOrder newPurchasingOrders, string userName)
        {
            ConsolidatePurchasingOrderViewModel dovm = new ConsolidatePurchasingOrderViewModel(exPurchasingOrders, newPurchasingOrders, userName);
            dovm.Closed += CloseConsolidatePurchasingOrderView;
            ChildWindowManager.Instance.ShowChildWindow(new ConsolidatePurchasingOrderView(exPurchasingOrders, newPurchasingOrders, userName) { DataContext = dovm });
        }

        void CloseConsolidatePurchasingOrderView(int r)
        {
            if (openConsolidatePurchasingOrderView_Closed != null)
                openConsolidatePurchasingOrderView_Closed(r);
            ChildWindowManager.Instance.CloseChildWindow();
        }

        /*****************Shipping Order View******************/
        public void ShowDispatchOrderViewWindow(DispatchOrder dispatchOrder)
        {
            DispatchOrderViewModel dovm = new DispatchOrderViewModel(dispatchOrder);
            dovm.Closed += dispatchOrderVM_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new DispatchOrderView(dispatchOrder) { DataContext = dovm });
        }

        void dispatchOrderVM_Closed(int res)
        {
            if (dispatchOrderView_Closed != null)
                dispatchOrderView_Closed(res);
            ChildWindowManager.Instance.CloseChildWindow();
        }

        /*****************Dispatch Confirmation******************/
        public void ShowDispatchConfirmation(DispatchOrder dis, List<Tuple<string, Int16, string>> timeStamps)
        {
            DispatchConfirmationViewModel dovm = new DispatchConfirmationViewModel(dis, timeStamps);
            dovm.Closed += DispatchConfirmation_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new DispatchConfirmationView(dis, timeStamps) { DataContext = dovm });
        }

        void DispatchConfirmation_Closed()
        {
            if (dispatchConfirmation_Closed != null)
                dispatchConfirmation_Closed();
            ChildWindowManager.Instance.CloseChildWindow();
        }

        /*****************Customer Credit************/
        public void ShowAddCustomerCredit(Customer customer)
        {
            AddCustomerCreditViewModel dovm = new AddCustomerCreditViewModel(customer);
            dovm.Closed += ShowAddCustomerCredit_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new AddCustomerCreditView(customer) { DataContext = dovm });
        }

        void ShowAddCustomerCredit_Closed(Customer c)
        {
            if (customerCreditForm_Closed != null)
                customerCreditForm_Closed(c);
            ChildWindowManager.Instance.CloseChildWindow();
        }

        /*****************Invoicing******************/
        public void ShowInvoicingWindow(Invoice invoice)
        {
            InvoicingViewModel dovm = new InvoicingViewModel(invoice);
            dovm.Closed += invoicing_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new InvoicingView(invoice) { DataContext = dovm });
        }

        void invoicing_Closed(int res)
        {
            if (invoicingView_Closed != null)
                invoicingView_Closed(res);
            ChildWindowManager.Instance.CloseChildWindow();
        }

        ///************************View Customer***********************************/
        public void ShowAddCustomerView(CustomerPending cp)
        {
            CustomerViewModel uvvm = new CustomerViewModel(cp);
            uvvm.Closed += ShowAddCustomerView_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new CustomerView(cp) { DataContext = uvvm });
        }
        void ShowAddCustomerView_Closed(int x)
        {
            if (showAddCustomerView_Closed != null)
                showAddCustomerView_Closed(x);
            ChildWindowManager.Instance.CloseChildWindow();
        }

        /*****************Add / Edit Contact Person******************/
        public void ShowAddUpdateContactPerson(ContactPerson cp)
        {
            AddUpdateContactPersonViewModel dovm = new AddUpdateContactPersonViewModel(cp);
            dovm.Closed += AddEditContactPerson_Closed;
            ChildWindowManager.Instance.ShowChildWindow(new AddUpdateContactPersonView(cp) { DataContext = dovm });
        }

        void AddEditContactPerson_Closed(ContactPerson cp)
        {
            if (addEditContactPerson_Closed != null)
                addEditContactPerson_Closed(cp);
            ChildWindowManager.Instance.CloseChildWindow();
        }

        ///************************New SalesOrder ***********************************/
        public void ShowNewSalesOrderView(SalesOrder so, OrderOrigin<object,string> oo)
        {
            //NewSalesOrderViewModel uvvm = new NewSalesOrderViewModel(so, oo);
            //uvvm.Closed += NewSalesOrderView_Closed;
            //ChildWindowManager.Instance.ShowChildWindow(new NewSalesOrderView(so, oo) { DataContext = uvvm });
        }
        void NewSalesOrderView_Closed(int res)
        {
            if (newSalesOrder_Closed != null)
                newSalesOrder_Closed(res);
            ChildWindowManager.Instance.CloseChildWindow();
        }

        ///************************View NewOrderPDF***********************************/
        //public void ShowNewOrderPDFView(NewOrderPDFM q)
        //{
        //    NewOrderPDFViewModel uvvm = new NewOrderPDFViewModel();
        //    uvvm.Closed += ShowNewOrderPDFView_Closed;
        //    ChildWindowManager.Instance.ShowChildWindow(new NewOrderPDFView() { DataContext = uvvm });
        //}
        //void ShowNewOrderPDFView_Closed(int x)
        //{
        //    if (showNewOrderPDFView_Closed != null)
        //        showNewOrderPDFView_Closed(x);
        //    ChildWindowManager.Instance.CloseChildWindow();
        //}
    }
}
