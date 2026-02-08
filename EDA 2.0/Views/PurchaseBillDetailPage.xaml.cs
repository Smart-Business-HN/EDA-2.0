using EDA.APPLICATION.Features.PaymentTypeFeature.Queries;
using EDA.APPLICATION.Features.PurchaseBillFeature.Commands.AddPaymentToPurchaseBillCommand;
using EDA.APPLICATION.Features.PurchaseBillFeature.Commands.DeletePurchaseBillCommand;
using EDA.APPLICATION.Features.PurchaseBillFeature.Queries;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace EDA_2._0.Views
{
    public sealed partial class PurchaseBillDetailPage : Page
    {
        private readonly IMediator _mediator;
        private int _purchaseBillId;
        private PurchaseBill? _purchaseBill;
        private List<PaymentType> _paymentTypes = new();

        public PurchaseBillDetailPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        public void SetPurchaseBillId(int purchaseBillId)
        {
            _purchaseBillId = purchaseBillId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPaymentTypes();
            await LoadPurchaseBill();
        }

        private async Task LoadPaymentTypes()
        {
            try
            {
                var result = await _mediator.Send(new GetAllPaymentTypesQuery { GetAll = true });
                if (result.Succeeded && result.Data != null)
                {
                    _paymentTypes = result.Data.Items.ToList();
                }
            }
            catch
            {
                // Silently ignore payment types loading errors
            }
        }

        private async Task LoadPurchaseBill()
        {
            SetLoading(true);

            try
            {
                var result = await _mediator.Send(new GetPurchaseBillByIdQuery { Id = _purchaseBillId });

                if (result.Succeeded && result.Data != null)
                {
                    _purchaseBill = result.Data;
                    PopulateBillData();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar la factura de compra");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void PopulateBillData()
        {
            if (_purchaseBill == null) return;

            // Header
            PurchaseBillCodeText.Text = $"Factura #{_purchaseBill.PurchaseBillCode}";
            InvoiceDateText.Text = $"Fecha: {_purchaseBill.InvoiceDate:dd/MM/yyyy}";

            // Provider Info
            ProviderNameText.Text = _purchaseBill.Provider?.Name ?? "-";
            InvoiceNumberText.Text = _purchaseBill.InvoiceNumber ?? "-";

            // Additional Info
            ExpenseAccountText.Text = _purchaseBill.ExpenseAccount?.Name ?? "-";
            CaiText.Text = _purchaseBill.Cai ?? "-";
            DueDateText.Text = _purchaseBill.DueDate?.ToString("dd/MM/yyyy") ?? "-";

            // Tax Breakdown
            ExemptText.Text = $"L {_purchaseBill.Exempt:N2}";
            ExoneratedText.Text = $"L {_purchaseBill.Exonerated:N2}";
            Taxed15Text.Text = $"L {_purchaseBill.TaxedAt15Percent:N2}";
            Taxes15Text.Text = $"L {_purchaseBill.Taxes15Percent:N2}";
            Taxed18Text.Text = $"L {_purchaseBill.TaxedAt18Percent:N2}";
            Taxes18Text.Text = $"L {_purchaseBill.Taxes18Percent:N2}";

            // Payments
            PaymentsListView.ItemsSource = _purchaseBill.PurchaseBillPayments;

            // Totals
            TotalText.Text = $"L {_purchaseBill.Total:N2}";
            OutstandingText.Text = $"L {_purchaseBill.OutstandingAmount:N2}";

            UpdateStatusBadge();
        }

        private void UpdateStatusBadge()
        {
            if (_purchaseBill == null) return;

            string statusName;
            Color badgeColor;

            switch (_purchaseBill.StatusId)
            {
                case (int)PurchaseBillStatusEnum.Created:
                    statusName = "Creada";
                    badgeColor = Color.FromArgb(255, 52, 152, 219); // Azul
                    AddPaymentButton.Visibility = Visibility.Visible;
                    EditButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    break;
                case (int)PurchaseBillStatusEnum.Paid:
                    statusName = "Pagada";
                    badgeColor = Color.FromArgb(255, 39, 174, 96); // Verde
                    AddPaymentButton.Visibility = Visibility.Collapsed;
                    EditButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Visible;
                    break;
                case (int)PurchaseBillStatusEnum.Cancelled:
                    statusName = "Anulada";
                    badgeColor = Color.FromArgb(255, 231, 76, 60); // Rojo
                    AddPaymentButton.Visibility = Visibility.Collapsed;
                    EditButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Collapsed;
                    break;
                default:
                    statusName = "Desconocido";
                    badgeColor = Color.FromArgb(255, 128, 128, 128); // Gris
                    AddPaymentButton.Visibility = Visibility.Collapsed;
                    EditButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Collapsed;
                    break;
            }

            StatusText.Text = statusName;
            StatusBadge.Background = new SolidColorBrush(badgeColor);
            StatusText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.MainWindow as MainWindow;
            mainWindow?.NavigateToPurchaseBills();
        }

        private async void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_purchaseBill == null) return;

            var paymentTypeCombo = new ComboBox
            {
                PlaceholderText = "Tipo de pago",
                DisplayMemberPath = "Name",
                ItemsSource = _paymentTypes,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var amountBox = new NumberBox
            {
                Header = $"Monto (Pendiente: L {_purchaseBill.OutstandingAmount:N2})",
                PlaceholderText = "0.00",
                Value = (double)_purchaseBill.OutstandingAmount,
                Minimum = 0.01,
                Maximum = (double)_purchaseBill.OutstandingAmount,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var content = new StackPanel { Width = 300, Spacing = 12, Children = { paymentTypeCombo, amountBox } };

            var dialog = new ContentDialog
            {
                Title = "Agregar Pago",
                Content = content,
                PrimaryButtonText = "Agregar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary &&
                paymentTypeCombo.SelectedItem is PaymentType paymentType &&
                !double.IsNaN(amountBox.Value) && amountBox.Value > 0)
            {
                SetLoading(true);

                try
                {
                    var command = new AddPaymentToPurchaseBillCommand
                    {
                        PurchaseBillId = _purchaseBillId,
                        PaymentTypeId = paymentType.Id,
                        Amount = (decimal)amountBox.Value,
                        PaymentDate = DateTime.Now,
                        CreatedBy = App.CurrentUser?.Name ?? "Sistema"
                    };

                    var addResult = await _mediator.Send(command);
                    if (addResult.Succeeded)
                    {
                        await LoadPurchaseBill();
                        await ShowSuccess("Pago agregado exitosamente.");
                    }
                    else
                    {
                        await ShowError(addResult.Message ?? "Error al agregar pago.");
                    }
                }
                catch (Exception ex)
                {
                    await ShowError($"Error: {ex.Message}");
                }
                finally
                {
                    SetLoading(false);
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_purchaseBill == null) return;

            // Navigate back to PurchaseBillsPage with edit mode
            var mainWindow = App.MainWindow as MainWindow;
            mainWindow?.NavigateToPurchaseBillEdit(_purchaseBillId);
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_purchaseBill == null) return;

            var confirmDialog = new ContentDialog
            {
                Title = "Confirmar Anulacion",
                Content = "¿Esta seguro que desea anular esta factura de compra? Esta accion no se puede deshacer.",
                PrimaryButtonText = "Anular",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SetLoading(true);

                try
                {
                    var command = new DeletePurchaseBillCommand { Id = _purchaseBillId };
                    var deleteResult = await _mediator.Send(command);

                    if (deleteResult.Succeeded)
                    {
                        await LoadPurchaseBill();
                        await ShowSuccess("Factura de compra anulada exitosamente.");
                    }
                    else
                    {
                        await ShowError(deleteResult.Message ?? "Error al anular factura.");
                    }
                }
                catch (Exception ex)
                {
                    await ShowError($"Error: {ex.Message}");
                }
                finally
                {
                    SetLoading(false);
                }
            }
        }

        private void SetLoading(bool isLoading)
        {
            LoadingRing.IsActive = isLoading;
            AddPaymentButton.IsEnabled = !isLoading;
            EditButton.IsEnabled = !isLoading;
            CancelButton.IsEnabled = !isLoading;
        }

        private async Task ShowError(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uEA39",
                            FontSize = 48,
                            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                },
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task ShowSuccess(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Exito",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uE73E",
                            FontSize = 48,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                },
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
