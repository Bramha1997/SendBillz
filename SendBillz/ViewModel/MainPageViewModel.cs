using CommunityToolkit.Mvvm.Input;
using SendBillz.Helpers;
using SendBillz.Interfaces;
using SendBillz.Models;
using SendBillz.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SendBillz.ViewModel
{
    public class MainPageViewModel : NotifyProperty
    {
        #region Private variables

        private readonly IDialogService _dialogService;

        private bool _contentVisibility;
        private bool _pdfGeneratorLoaderVisibility;
        private string _storeName = string.Empty;
        private string _storeStreetAddress = string.Empty;
        private string _storeLandmark = string.Empty;
        private string _storePinCode = string.Empty;
        private string _sellerName = string.Empty;
        private string _sellerGstin = string.Empty;
        private string _buyerName = string.Empty;
        private string _buyerGstin = string.Empty;
        private string _invoiceNumber = string.Empty;
        private string _totalAmount = string.Empty;
        private DateTime _invoiceDate;
        private ObservableCollection<InvoiceItem> _items = [];
        private byte[] _logoBytes = [];
        private byte[] _signBytes = [];
        private ImageSource _logoImageSource;
        private ImageSource _signImageSource; 

        private IAsyncRelayCommand _generatePdfCommand;
        private IAsyncRelayCommand _uploadLogoCommand;
        private IAsyncRelayCommand _uploadSignCommand;
        private ICommand _addItemCommand;

        #endregion

        #region Constant variables

        private const long MaxImageSizeInBytes = 1 * 1024 * 1024; // 1 MB

        #endregion

        #region Properties

        public bool ContentVisibility
        {
            get { return _contentVisibility; }
            set
            {
                _contentVisibility = value;
                OnPropertyChanged(nameof(ContentVisibility));
            }
        }

        public bool PdfGeneratorLoaderVisibility
        {
            get { return _pdfGeneratorLoaderVisibility; }
            set
            {
                _pdfGeneratorLoaderVisibility = value;
                OnPropertyChanged(nameof(PdfGeneratorLoaderVisibility));
            }
        }

        public required string StoreName
        {
            get { return _storeName; }
            set
            {
                _storeName = value;
                OnPropertyChanged(nameof(StoreName));
            }
        }
        
        public required string StoreStreetAddress
        {
            get { return _storeStreetAddress; }
            set
            {
                _storeStreetAddress = value;
                OnPropertyChanged(nameof(StoreStreetAddress));
            }
        }
        
        public required string StoreLandmark
        {
            get { return _storeLandmark; }
            set
            {
                _storeLandmark = value;
                OnPropertyChanged(nameof(StoreLandmark));
            }
        }
        
        public required string StorePinCode
        {
            get { return _storePinCode; }
            set
            {
                _storePinCode = value;
                OnPropertyChanged(nameof(StorePinCode));
            }
        }

        public required string SellerName
        {
            get { return _sellerName; }
            set
            {
                _sellerName = value;
                OnPropertyChanged(nameof(SellerName));
            }
        }

        public string SellerGstin
        {
            get { return _sellerGstin; }
            set
            {
                _sellerGstin = value;
                OnPropertyChanged(nameof(SellerGstin));
            }
        }

        public required string BuyerName
        {
            get { return _buyerName; }
            set
            {
                _buyerName = value;
                OnPropertyChanged(nameof(BuyerName));
            }
        }

        public string BuyerGstin
        {
            get { return _buyerGstin; }
            set
            {
                _buyerGstin = value;
                OnPropertyChanged(nameof(BuyerGstin));
            }
        }

        public required string InvoiceNumber
        {
            get { return _invoiceNumber; }
            set
            {
                _invoiceNumber = value;
                OnPropertyChanged(nameof(InvoiceNumber));
            }
        }

        public string TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public DateTime InvoiceDate
        {
            get { return _invoiceDate; }
            set
            {
                _invoiceDate = value;
                OnPropertyChanged(nameof(InvoiceDate));
            }
        }

        public ImageSource LogoImageSource
        {
            get { return _logoImageSource; }
            set
            {
                _logoImageSource = value;
                OnPropertyChanged(nameof(LogoImageSource));
            }
        }

        public ImageSource SignImageSource
        {
            get { return _signImageSource; }
            set
            {
                _signImageSource = value;
                OnPropertyChanged(nameof(SignImageSource));
            }
        }

        public ObservableCollection<InvoiceItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        #endregion

        #region Commands

        public IAsyncRelayCommand GeneratePdfCommand =>
            _generatePdfCommand ?? (_generatePdfCommand = new AsyncRelayCommand(ExecuteGeneratePdfCommandAsync));

        public IAsyncRelayCommand UploadLogoCommand =>
            _uploadLogoCommand ?? (_uploadLogoCommand = new AsyncRelayCommand(ExecuteUploadLogoCommandAsync));

        public IAsyncRelayCommand UploadSignCommand =>
            _uploadSignCommand ?? (_uploadSignCommand = new AsyncRelayCommand(ExecuteUploadSignCommandAsync));

        public ICommand AddItemCommand => _addItemCommand ?? (_addItemCommand = new Command(ExecuteAddItemCommand));

        #endregion

        #region Constructors

        public MainPageViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            ContentVisibility = true;
            PdfGeneratorLoaderVisibility = false;
            TotalAmount = "Total: ₹0.00"; 
            InvoiceDate = DateTime.Now;

            Items.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (InvoiceItem item in e.NewItems)
                        item.PropertyChanged += (_, _) => UpdateTotal();
                UpdateTotal();
            };

            // add one
            Items.Add(CreateItem());
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Method to create items.
        /// </summary>
        /// <returns>Returns invoice item.</returns>
        private InvoiceItem CreateItem()
        {
            var item = new InvoiceItem();
            item.RemoveCommand = new RelayCommand(() =>
            {
                Items.Remove(item);
                UpdateTotal();
            });
            item.PropertyChanged += (_, _) => UpdateTotal();
            return item;
        }

        /// <summary>
        /// Method to update total amount.
        /// </summary>
        private void UpdateTotal()
        {
            double total = 0;
            foreach (var item in Items)
            {
                var qty = item.Quantity;
                var price = item.UnitPrice;
                var disc = qty * item.Discount;
                var discAmmount = price * (disc / 100);
                var gstRate = item.GstRate;
                var amount = qty * price - discAmmount;
                var gstAmt = amount * (gstRate / 100);
                total += amount + gstAmt;
            }
            TotalAmount = $"Total: ₹{total:F2}";
        }

        #endregion

        #region Command event methods

        /// <summary>
        /// Event method to execute GeneratePdfCommand. 
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteGeneratePdfCommandAsync()
        {
            // validation
            if (string.IsNullOrWhiteSpace(StoreName))
            {
                await _dialogService.ShowAlertAsync("Validation", "Please fill the Store name.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(SellerName) ||
                string.IsNullOrWhiteSpace(BuyerName))
            {
                await _dialogService.ShowAlertAsync("Validation", "Please fill seller and buyer details.", "OK");
                return;
            }

            SellerInfo seller = new SellerInfo { Name = SellerName, Gstin = SellerName };
            BuyerInfo buyer = new BuyerInfo { Name = BuyerName, Gstin = BuyerName };
            string invNum = InvoiceNumber;
            string invDate = InvoiceDate.Date.ToString("dd/MM/yyyy");
            double total = 0;

            foreach (var item in Items)
            {
                int qty = item.Quantity;
                double price = item.UnitPrice;
                double disc = qty * item.Discount;
                double discAmmount = price * (disc / 100);
                double gstRate = item.GstRate;
                double amount = qty * price - discAmmount;
                double gstAmt = amount * (gstRate / 100);
                total += amount + gstAmt;
            }

            string? storeName = StoreName ?? null;
            string? storeAddress = string.IsNullOrWhiteSpace(StoreLandmark) ? 
                                    $"{StoreStreetAddress}\n{StorePinCode}" : 
                                    $"{StoreStreetAddress}\n{StoreLandmark}\n{StorePinCode}";

            string filePath = Path.Combine(FileSystem.AppDataDirectory, $"invoice_{DateTime.Now.Ticks}.pdf");

            ContentVisibility = false;
            PdfGeneratorLoaderVisibility = true;

            bool isPdfGenerated = await PdfGeneratorService.GenerateIndianInvoicePdfAsync(
                filePath, seller, buyer, invNum, invDate, Items, total, storeName, storeAddress, _logoBytes, _signBytes);

            ContentVisibility = true;
            PdfGeneratorLoaderVisibility = false;

            if (isPdfGenerated)
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Invoice PDF",
                    File = new ShareFile(filePath)
                });
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", "PDF generation failed", "OK");
            }
        }

        /// <summary>
        /// Event method to execute UploadLogoCommand.
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteUploadLogoCommandAsync()
        {
            try
            {
                FileResult? result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Please select a logo image"
                });

                if (result != null)
                {
                    FileResult copyOfResult = result;
                    Stream stream = await result.OpenReadAsync();
                    Stream resultStreamCopy = await copyOfResult.OpenReadAsync();

                    LogoImageSource = ImageSource.FromStream(() => stream);

                    if (resultStreamCopy != null)
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            await resultStreamCopy.CopyToAsync(memoryStream);

                            if (memoryStream.Length > MaxImageSizeInBytes)
                            {
                                await _dialogService.ShowAlertAsync(
                                    "File Too Large",
                                    "The selected logo exceeds the maximum size of 1 MB.",
                                    "OK");
                                return;
                            }

                            _logoBytes = memoryStream.ToArray();
                        }
                    }
                }
            }
            catch (FeatureNotSupportedException)
            {
                await _dialogService.ShowAlertAsync("Unsupported", "This feature is not supported on your device.", "OK");
            }
            catch (PermissionException)
            {
                await _dialogService.ShowAlertAsync("Permission Denied", "Permission to access photos was denied.", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Unable to upload logo: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Event method to execute UploadSignCommand.
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteUploadSignCommandAsync()
        {
            try
            {
                FileResult? result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Please select a sign image"
                });

                if (result != null)
                {
                    FileResult copyOfResult = result;
                    Stream stream = await result.OpenReadAsync();
                    Stream resultStreamCopy = await copyOfResult.OpenReadAsync();
                    SignImageSource = ImageSource.FromStream(() => stream);

                    if (resultStreamCopy != null)
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            await resultStreamCopy.CopyToAsync(memoryStream);

                            if (memoryStream.Length > MaxImageSizeInBytes)
                            {
                                await _dialogService.ShowAlertAsync(
                                    "File Too Large",
                                    "The selected sign image exceeds the maximum size of 1 MB.",
                                    "OK");
                                return;
                            }

                            _signBytes = memoryStream.ToArray();
                        }
                    }
                }
            }
            catch (FeatureNotSupportedException)
            {
                await _dialogService.ShowAlertAsync("Unsupported", "This feature is not supported on your device.", "OK");
            }
            catch (PermissionException)
            {
                await _dialogService.ShowAlertAsync("Permission Denied", "Permission to access photos was denied.", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Unable to upload sign: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Event method to execute AddItemCommand.
        /// </summary>
        private void ExecuteAddItemCommand()
        {
            InvoiceItem newItem = CreateItem();
            Items.Add(newItem);
            UpdateTotal();
        }

        #endregion
    }
}
