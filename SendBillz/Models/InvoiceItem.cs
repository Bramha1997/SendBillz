using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace SendBillz.Models
{
    public class InvoiceItem : ObservableObject
    {
        private string description = string.Empty;
        private int quantity = 1;
        private double unitPrice;
        private double discount;
        private double gstRate = 0;

        public string Description { get => description; set => SetProperty(ref description, value); }
        public int Quantity { get => quantity; set => SetProperty(ref quantity, value); }
        public double UnitPrice { get => unitPrice; set => SetProperty(ref unitPrice, value); }
        public double Discount { get => discount; set => SetProperty(ref discount, value); }
        public double GstRate { get => gstRate; set => SetProperty(ref gstRate, value); }

        public ICommand RemoveCommand { get; set; }
    }
}
