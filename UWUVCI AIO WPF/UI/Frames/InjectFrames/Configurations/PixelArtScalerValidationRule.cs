using System.Globalization;
using System.Windows.Controls;

namespace UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations
{
    public class PixelArtScalerValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int scaler;
            if (int.TryParse(value as string, out scaler))
            {
                if (scaler >= 1) // Allow any value 1 or above
                    return ValidationResult.ValidResult;
                else
                    return new ValidationResult(false, "Pixel Art Scaler must be 1 or higher.");
            }
            return new ValidationResult(false, "Invalid input. Must be an integer.");
        }
    }
}
