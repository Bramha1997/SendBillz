using SendBillz.Interfaces;

namespace SendBillz.Services
{
    public class DialogService : IDialogService
    {
        public async Task ShowAlertAsync(string title, string message, string cancel)
        {
            // Ensure MainPage is not null
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
        }
    }
}
