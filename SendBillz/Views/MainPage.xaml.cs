using CommunityToolkit.Mvvm.Input;
using SendBillz.Models;
using SendBillz.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SendBillz
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<InvoiceItem> Items { get; } = new();
        public ICommand GeneratePdfCommand { get; }

        private byte[] _logoBytes;
        private byte[] _signBytes;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

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
            TotalLabel.Text = $"Total: ₹{total:F2}";
        }

        private async void GenerateAndSharePdfAsync_Clicked(object sender, EventArgs e)
        {
            // validation
            if (string.IsNullOrWhiteSpace(SellerNameEntry.Text) ||
                string.IsNullOrWhiteSpace(BuyerNameEntry.Text))
            {
                await DisplayAlert("Validation", "Please fill seller and buyer details.", "OK");
                return;
            }

            var seller = new SellerInfo { Name = SellerNameEntry.Text, Gstin = SellerGstinEntry.Text };
            var buyer = new BuyerInfo { Name = BuyerNameEntry.Text, Gstin = BuyerGstinEntry.Text };
            var invNum = InvoiceNumberEntry.Text;
            var invDate = InvoiceDatePicker.Date.ToString("dd/MM/yyyy");
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
            var storeName = StoreNameEntry.Text ?? null;

            var filePath = Path.Combine(FileSystem.AppDataDirectory, $"invoice_{DateTime.Now.Ticks}.pdf");
            var ok = PdfGenerator.GenerateIndianInvoicePdfAsync(
                filePath, seller, buyer, invNum, invDate, Items, total, storeName, _logoBytes, _signBytes);

            if (ok)
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Invoice PDF",
                    File = new ShareFile(filePath)
                });
            }
            else
            {
                await DisplayAlert("Error", "PDF generation failed", "OK");
            }
        }

        private void Add_Item_Button_Clicked(object sender, EventArgs e)
        {
            var newItem = CreateItem();
            Items.Add(newItem);
            UpdateTotal();
        }

        private async void OnUploadLogoClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Please select a logo image"
                });
                if (result != null)
                {
                    var copyOfResult = result;
                    var stream = await result.OpenReadAsync();
                    var stream1 = await copyOfResult.OpenReadAsync();
                    LogoImage.Source = ImageSource.FromStream(() => stream);

                    if (stream1 != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream1.CopyToAsync(memoryStream);
                            _logoBytes = memoryStream.ToArray();
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, such as permissions not granted
                await DisplayAlert("Error", $"Unable to upload logo: {ex.Message}", "OK");
            }
        }

        private async void OnUploadSignClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Please select a sign image"
                });
                if (result != null)
                {
                    var copyOfResult = result;
                    var stream = await result.OpenReadAsync();
                    var stream1 = await copyOfResult.OpenReadAsync();
                    SignImage.Source = ImageSource.FromStream(() => stream);

                    if (stream1 != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream1.CopyToAsync(memoryStream);
                            _signBytes = memoryStream.ToArray();
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, such as permissions not granted
                await DisplayAlert("Error", $"Unable to upload sign: {ex.Message}", "OK");
            }
        }
    }
}
