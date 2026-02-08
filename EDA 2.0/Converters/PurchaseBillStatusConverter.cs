using EDA.DOMAIN.Enums;
using Microsoft.UI.Xaml.Data;
using System;

namespace EDA_2._0.Converters
{
    public class PurchaseBillStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int statusId)
            {
                return statusId switch
                {
                    (int)PurchaseBillStatusEnum.Created => "Creada",
                    (int)PurchaseBillStatusEnum.Paid => "Pagada",
                    (int)PurchaseBillStatusEnum.Cancelled => "Anulada",
                    _ => "Desconocido"
                };
            }
            return "Desconocido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
