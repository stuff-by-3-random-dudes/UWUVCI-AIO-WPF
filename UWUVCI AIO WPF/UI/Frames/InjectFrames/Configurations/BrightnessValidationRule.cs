using System.Globalization;
using System.Windows.Controls;

namespace UWUVCI_AIO_WPF.UI.Frames.InjectFrames.Configurations
{
    public class BrightnessValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int brightness;
            if (int.TryParse(value as string, out brightness))
            {
                if (brightness >= 0 && brightness <= 100)
                    return ValidationResult.ValidResult;
                else
                    return new ValidationResult(false, "Brightness must be between 0 and 100.");
            }
            return new ValidationResult(false, "Invalid input. Brightness must be an integer.");
        }
    }
}
