namespace Speedy.Samples.Xamarin.Services;

public interface IFileService
{
    #region Methods

    void WriteFile(string fileName, string data);

    #endregion
}