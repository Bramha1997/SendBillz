namespace SendBillz.Interfaces
{
    public interface IDialogService
    {
        /// <summary>
        /// Method to show alert dialog.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        Task ShowAlertAsync(string title, string message, string cancel);
    }
}
